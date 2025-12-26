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
    [BurstCompile]
    [UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
    [UpdateBefore(typeof(GodgameJobTicketClaimSystem))]
    public partial struct GodgameJobTicketSpawnSystem : ISystem
    {
        private EntityQuery _ticketQuery;
        private EntityQuery _nodeQuery;
        private EntityQuery _pileQuery;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<TimeState>();
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

            var tickets = _ticketQuery.ToComponentDataArray<JobTicket>(state.WorldUpdateAllocator);
            var existingKeys = new NativeParallelHashMap<ulong, byte>(
                math.max(1, tickets.Length),
                Allocator.Temp);

            for (int i = 0; i < tickets.Length; i++)
            {
                var ticket = tickets[i];
                if (ticket.JobKey == 0 || ticket.State == JobTicketState.Done || ticket.State == JobTicketState.Cancelled)
                {
                    continue;
                }

                existingKeys.TryAdd(ticket.JobKey, 0);
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

                var key = BuildJobKey(JobTicketType.Gather, nodeEntities[i], mirror.ResourceTypeIndex);
                if (existingKeys.ContainsKey(key))
                {
                    continue;
                }

                var ticketEntity = state.EntityManager.CreateEntity(typeof(JobTicket));
                state.EntityManager.SetComponentData(ticketEntity, new JobTicket
                {
                    Type = JobTicketType.Gather,
                    State = JobTicketState.Open,
                    TargetEntity = nodeEntities[i],
                    Assignee = Entity.Null,
                    ResourceTypeIndex = mirror.ResourceTypeIndex,
                    ClaimExpiresTick = 0,
                    LastStateTick = timeState.Tick,
                    JobKey = key
                });
                existingKeys.TryAdd(key, 0);
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

                var key = BuildJobKey(JobTicketType.Gather, pileEntities[i], pile.ResourceTypeIndex);
                if (existingKeys.ContainsKey(key))
                {
                    continue;
                }

                var ticketEntity = state.EntityManager.CreateEntity(typeof(JobTicket));
                state.EntityManager.SetComponentData(ticketEntity, new JobTicket
                {
                    Type = JobTicketType.Gather,
                    State = JobTicketState.Open,
                    TargetEntity = pileEntities[i],
                    Assignee = Entity.Null,
                    ResourceTypeIndex = pile.ResourceTypeIndex,
                    ClaimExpiresTick = 0,
                    LastStateTick = timeState.Tick,
                    JobKey = key
                });
                existingKeys.TryAdd(key, 0);
            }

            existingKeys.Dispose();
        }

        private static ulong BuildJobKey(JobTicketType type, Entity target, ushort resourceTypeIndex)
        {
            ulong key = ((ulong)(uint)target.Index << 32) | (uint)target.Version;
            key ^= ((ulong)resourceTypeIndex << 8);
            key ^= (ulong)type;
            return key;
        }
    }
}
