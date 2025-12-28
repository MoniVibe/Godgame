using Godgame.Registry;
using Godgame.Resources;
using PureDOTS.Runtime.AI;
using PureDOTS.Runtime.Components;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Godgame.Villagers
{
    /// <summary>
    /// Spawns job tickets for resource nodes and aggregate piles.
    /// </summary>
    [UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
    [UpdateBefore(typeof(GodgameJobTicketClaimSystem))]
    public partial struct GodgameJobTicketSpawnSystem : ISystem
    {
        private EntityQuery _ticketQuery;
        private EntityQuery _nodeQuery;
        private EntityQuery _pileQuery;
        private EntityArchetype _ticketArchetype;

        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<TimeState>();
            state.RequireForUpdate<GodgameJobTicketTuning>();
            _ticketArchetype = state.EntityManager.CreateArchetype(typeof(JobTicket));
            _ticketQuery = SystemAPI.QueryBuilder()
                .WithAll<JobTicket>()
                .Build();
            _nodeQuery = SystemAPI.QueryBuilder()
                .WithAll<GodgameResourceNodeMirror, LocalTransform>()
                .Build();
            _pileQuery = SystemAPI.QueryBuilder()
                .WithAll<AggregatePile, LocalTransform>()
                .Build();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            if (!SystemAPI.TryGetSingleton<TimeState>(out var timeState) || timeState.IsPaused)
            {
                return;
            }

            if (SystemAPI.TryGetSingleton<RewindState>(out var rewindState) && rewindState.Mode != RewindMode.Record)
            {
                return;
            }

            var tuning = SystemAPI.GetSingleton<GodgameJobTicketTuning>();
            var activeTicketCounts = new NativeParallelHashMap<Entity, int>(
                math.max(1, _ticketQuery.CalculateEntityCount()),
                Allocator.Temp);

            foreach (var ticket in SystemAPI.Query<RefRO<JobTicket>>())
            {
                if (ticket.ValueRO.State == JobTicketState.Done || ticket.ValueRO.State == JobTicketState.Cancelled)
                {
                    continue;
                }

                var target = ticket.ValueRO.TargetEntity;
                if (target == Entity.Null)
                {
                    continue;
                }

                if (activeTicketCounts.TryGetValue(target, out var count))
                {
                    activeTicketCounts[target] = count + 1;
                }
                else
                {
                    activeTicketCounts[target] = 1;
                }
            }

            var nodeEntities = _nodeQuery.ToEntityArray(state.WorldUpdateAllocator);
            var nodeMirrors = _nodeQuery.ToComponentDataArray<GodgameResourceNodeMirror>(state.WorldUpdateAllocator);
            for (int i = 0; i < nodeEntities.Length; i++)
            {
                var mirror = nodeMirrors[i];
                if (mirror.IsDepleted != 0 || mirror.RemainingAmount <= 0f || mirror.ResourceTypeIndex == ushort.MaxValue)
                {
                    continue;
                }

                var maxTickets = math.max(0, tuning.MaxConcurrentNodeTickets + tuning.OpenTicketBuffer);
                if (maxTickets <= 0)
                {
                    continue;
                }

                var activeCount = activeTicketCounts.TryGetValue(nodeEntities[i], out var currentCount)
                    ? currentCount
                    : 0;
                var chunk = ResolveChunkAmount(mirror.RemainingAmount, tuning);
                var maxByRemaining = ResolveTicketsByRemaining(mirror.RemainingAmount, chunk, tuning);
                var desiredTotal = math.min(maxTickets, maxByRemaining);
                var toSpawn = math.max(0, desiredTotal - activeCount);
                for (int spawnIndex = 0; spawnIndex < toSpawn; spawnIndex++)
                {
                    var workAmount = ResolveWorkAmount(mirror.RemainingAmount, chunk, tuning);
                    var requiredWorkers = ResolveRequiredWorkers(workAmount, tuning);
                    var minWorkers = ResolveMinWorkers(requiredWorkers, tuning);
                    var ticketEntity = state.EntityManager.CreateEntity(_ticketArchetype);
                    state.EntityManager.SetComponentData(ticketEntity, new JobTicket
                    {
                        Type = JobTicketType.Gather,
                        State = JobTicketState.Open,
                        SourceEntity = nodeEntities[i],
                        TargetEntity = nodeEntities[i],
                        DestinationEntity = Entity.Null,
                        Assignee = Entity.Null,
                        ResourceTypeIndex = mirror.ResourceTypeIndex,
                        WorkAmount = workAmount,
                        RequiredWorkers = requiredWorkers,
                        MinWorkers = minWorkers,
                        IsSingleItem = 0,
                        ItemMass = 0f,
                        ClaimExpiresTick = 0,
                        LastStateTick = timeState.Tick,
                        BatchKey = 0u,
                        JobKey = BuildJobKey(JobTicketType.Gather, nodeEntities[i], mirror.ResourceTypeIndex, (uint)spawnIndex, timeState.Tick)
                    });
                }
            }

            var pileEntities = _pileQuery.ToEntityArray(state.WorldUpdateAllocator);
            var pileData = _pileQuery.ToComponentDataArray<AggregatePile>(state.WorldUpdateAllocator);
            for (int i = 0; i < pileEntities.Length; i++)
            {
                var pile = pileData[i];
                if (pile.Amount <= 0f || pile.ResourceTypeIndex == ushort.MaxValue)
                {
                    continue;
                }

                var maxTickets = math.max(0, tuning.MaxConcurrentPileTickets + tuning.OpenTicketBuffer);
                if (maxTickets <= 0)
                {
                    continue;
                }

                var activeCount = activeTicketCounts.TryGetValue(pileEntities[i], out var currentCount)
                    ? currentCount
                    : 0;
                var chunk = ResolveChunkAmount(pile.Amount, tuning);
                var maxByRemaining = ResolveTicketsByRemaining(pile.Amount, chunk, tuning);
                var desiredTotal = math.min(maxTickets, maxByRemaining);
                var toSpawn = math.max(0, desiredTotal - activeCount);
                for (int spawnIndex = 0; spawnIndex < toSpawn; spawnIndex++)
                {
                    var workAmount = ResolveWorkAmount(pile.Amount, chunk, tuning);
                    var requiredWorkers = ResolveRequiredWorkers(workAmount, tuning);
                    var minWorkers = ResolveMinWorkers(requiredWorkers, tuning);
                    var ticketEntity = state.EntityManager.CreateEntity(_ticketArchetype);
                    state.EntityManager.SetComponentData(ticketEntity, new JobTicket
                    {
                        Type = JobTicketType.Gather,
                        State = JobTicketState.Open,
                        SourceEntity = pileEntities[i],
                        TargetEntity = pileEntities[i],
                        DestinationEntity = Entity.Null,
                        Assignee = Entity.Null,
                        ResourceTypeIndex = pile.ResourceTypeIndex,
                        WorkAmount = workAmount,
                        RequiredWorkers = requiredWorkers,
                        MinWorkers = minWorkers,
                        IsSingleItem = 0,
                        ItemMass = 0f,
                        ClaimExpiresTick = 0,
                        LastStateTick = timeState.Tick,
                        BatchKey = 0u,
                        JobKey = BuildJobKey(JobTicketType.Gather, pileEntities[i], pile.ResourceTypeIndex, (uint)spawnIndex, timeState.Tick)
                    });
                }
            }

            activeTicketCounts.Dispose();
        }

        private static ulong BuildJobKey(JobTicketType type, Entity target, ushort resourceTypeIndex, uint spawnIndex, uint tick)
        {
            return math.hash(new uint4((uint)target.Index, (uint)target.Version, resourceTypeIndex, tick + spawnIndex)) ^ (ulong)type;
        }

        private static float ResolveChunkAmount(float remaining, in GodgameJobTicketTuning tuning)
        {
            if (remaining <= 0f)
            {
                return 0f;
            }

            var min = math.max(0f, tuning.ChunkMinUnits);
            var max = math.max(min, tuning.ChunkMaxUnits);
            if (max <= 0f && tuning.ChunkDefaultUnits <= 0f)
            {
                return 0f;
            }
            var chunk = math.clamp(tuning.ChunkDefaultUnits, min > 0f ? min : 0f, max > 0f ? max : tuning.ChunkDefaultUnits);
            return math.max(0.01f, chunk);
        }

        private static int ResolveTicketsByRemaining(float remaining, float chunk, in GodgameJobTicketTuning tuning)
        {
            if (remaining <= 0f || chunk <= 0f)
            {
                return 0;
            }

            var count = (int)math.floor(remaining / chunk);
            var remainder = remaining - count * chunk;
            var remainderThreshold = math.max(0f, tuning.ChunkMinUnits);
            if (remainder >= remainderThreshold && remainder > 0f)
            {
                count += 1;
            }

            if (count == 0)
            {
                count = 1;
            }

            return count;
        }

        private static float ResolveWorkAmount(float remaining, float chunk, in GodgameJobTicketTuning tuning)
        {
            if (remaining <= 0f)
            {
                return 0f;
            }

            var min = math.max(0f, tuning.ChunkMinUnits);
            var max = math.max(min, tuning.ChunkMaxUnits);
            var clamped = math.clamp(chunk, min > 0f ? min : 0f, max > 0f ? max : chunk);
            var amount = math.min(remaining, clamped);
            if (amount <= 0f && remaining > 0f)
            {
                amount = remaining;
            }

            return amount;
        }

        private static byte ResolveRequiredWorkers(float workAmount, in GodgameJobTicketTuning tuning)
        {
            if (tuning.HeavyCarryThresholdUnits > 0f && workAmount >= tuning.HeavyCarryThresholdUnits)
            {
                return (byte)math.clamp(tuning.HeavyCarryRequiredWorkers, 2, 16);
            }

            return 1;
        }

        private static byte ResolveMinWorkers(byte requiredWorkers, in GodgameJobTicketTuning tuning)
        {
            if (requiredWorkers <= 1)
            {
                return 1;
            }

            return (byte)math.clamp(tuning.HeavyCarryMinWorkers, 1, requiredWorkers);
        }
    }
}
