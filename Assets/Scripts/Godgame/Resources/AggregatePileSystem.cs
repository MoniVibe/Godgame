using Godgame.Resources;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Godgame.Systems.Resources
{
    /// <summary>
    /// Manages aggregate resource piles: spawns from commands, merges nearby piles, and splits overflow.
    /// </summary>
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public partial struct AggregatePileSystem : ISystem
    {
        private EntityQuery _pileQuery;

        public void OnCreate(ref SystemState state)
        {
            _pileQuery = state.GetEntityQuery(ComponentType.ReadWrite<AggregatePile>(), ComponentType.ReadWrite<LocalTransform>());
            state.RequireForUpdate<AggregatePileConfig>();
            state.RequireForUpdate<AggregatePileRuntimeState>();
        }

        public void OnUpdate(ref SystemState state)
        {
            var configEntity = SystemAPI.GetSingletonEntity<AggregatePileConfig>();
            var config = SystemAPI.GetSingleton<AggregatePileConfig>();
            var runtime = SystemAPI.GetSingletonRW<AggregatePileRuntimeState>();
            var commands = EnsureSpawnBuffer(ref state, configEntity);

            for (int i = 0; i < commands.Length; i++)
            {
                var cmd = commands[i];
                if (cmd.Amount < config.MinSpawnAmount)
                {
                    continue;
                }

                SpawnOrAddPile(ref state, ref runtime.ValueRW, in config, cmd);
            }

            commands.Clear();

            if (SystemAPI.Time.ElapsedTime < runtime.ValueRW.NextMergeTime)
            {
                return;
            }

            runtime.ValueRW.NextMergeTime = (float)(SystemAPI.Time.ElapsedTime + config.MergeCheckSeconds);
            MergeNearbyPiles(ref state, ref runtime.ValueRW, in config);
        }

        private void SpawnOrAddPile(ref SystemState state, ref AggregatePileRuntimeState runtime, in AggregatePileConfig config, in AggregatePileSpawnCommand cmd)
        {
            var mergeRadiusSq = config.MergeRadius * config.MergeRadius;
            var piles = _pileQuery.ToEntityArray(state.WorldUpdateAllocator);
            var remaining = cmd.Amount;
            var targetCap = math.max(0.0001f, math.min(config.GlobalMaxCapacity, config.DefaultMaxCapacity));

            // Try to add to an existing pile of the same resource type.
            for (int i = 0; i < piles.Length && remaining > config.ConservationEpsilon; i++)
            {
                var entity = piles[i];
                var pile = state.EntityManager.GetComponentData<AggregatePile>(entity);
                if (pile.ResourceType != cmd.ResourceType)
                {
                    continue;
                }

                var transform = state.EntityManager.GetComponentData<LocalTransform>(entity);
                var distSq = math.lengthsq(transform.Position - cmd.Position);
                if (distSq > mergeRadiusSq)
                {
                    continue;
                }

                var available = math.max(0f, pile.Capacity - pile.Amount);
                if (available <= config.ConservationEpsilon)
                {
                    continue;
                }

                var delta = math.min(available, remaining);
                pile.Amount += delta;
                remaining -= delta;
                state.EntityManager.SetComponentData(entity, pile);
            }

            // Spawn new piles for any leftover.
            while (remaining > config.ConservationEpsilon && runtime.ActivePiles < config.MaxActivePiles)
            {
                var toAssign = math.min(targetCap, remaining);
                remaining -= toAssign;
                var pileEntity = state.EntityManager.CreateEntity(
                    typeof(AggregatePile),
                    typeof(LocalTransform));

                state.EntityManager.SetComponentData(pileEntity, new AggregatePile
                {
                    ResourceType = cmd.ResourceType,
                    Amount = toAssign,
                    Capacity = targetCap
                });

                state.EntityManager.SetComponentData(pileEntity, LocalTransform.FromPosition(cmd.Position));
                runtime.ActivePiles++;
            }
        }

        private DynamicBuffer<AggregatePileSpawnCommand> EnsureSpawnBuffer(ref SystemState state, Entity configEntity)
        {
            if (!state.EntityManager.HasBuffer<AggregatePileSpawnCommand>(configEntity))
            {
                state.EntityManager.AddBuffer<AggregatePileSpawnCommand>(configEntity);
            }

            return state.EntityManager.GetBuffer<AggregatePileSpawnCommand>(configEntity);
        }

        private void MergeNearbyPiles(ref SystemState state, ref AggregatePileRuntimeState runtime, in AggregatePileConfig config)
        {
            var mergeRadiusSq = config.MergeRadius * config.MergeRadius;
            var piles = _pileQuery.ToEntityArray(state.WorldUpdateAllocator);
            var ecb = new EntityCommandBuffer(state.WorldUpdateAllocator);

            for (int i = 0; i < piles.Length; i++)
            {
                var entityA = piles[i];
                if (!state.EntityManager.Exists(entityA))
                {
                    continue;
                }

                var pileA = state.EntityManager.GetComponentData<AggregatePile>(entityA);
                var transformA = state.EntityManager.GetComponentData<LocalTransform>(entityA);

                for (int j = i + 1; j < piles.Length; j++)
                {
                    var entityB = piles[j];
                    if (!state.EntityManager.Exists(entityB))
                    {
                        continue;
                    }

                    var pileB = state.EntityManager.GetComponentData<AggregatePile>(entityB);
                    if (pileA.ResourceType != pileB.ResourceType)
                    {
                        continue;
                    }

                    var transformB = state.EntityManager.GetComponentData<LocalTransform>(entityB);
                    var distSq = math.lengthsq(transformA.Position - transformB.Position);
                    if (distSq > mergeRadiusSq)
                    {
                        continue;
                    }

                    var available = math.max(0f, pileA.Capacity - pileA.Amount);
                    if (available <= config.ConservationEpsilon)
                    {
                        continue;
                    }

                    var transfer = math.min(available, pileB.Amount);
                    pileA.Amount += transfer;
                    pileB.Amount -= transfer;

                    if (pileB.Amount <= config.ConservationEpsilon)
                    {
                        ecb.DestroyEntity(entityB);
                        runtime.ActivePiles = math.max(0, runtime.ActivePiles - 1);
                    }
                    else
                    {
                        state.EntityManager.SetComponentData(entityB, pileB);
                    }
                }

                state.EntityManager.SetComponentData(entityA, pileA);

                // Split overflow if any pile exceeds capacity.
                if (pileA.Amount > pileA.Capacity + config.ConservationEpsilon && runtime.ActivePiles < config.MaxActivePiles)
                {
                    var overflow = pileA.Amount - pileA.Capacity;
                    pileA.Amount = pileA.Capacity;
                    state.EntityManager.SetComponentData(entityA, pileA);

                    while (overflow > config.ConservationEpsilon && runtime.ActivePiles < config.MaxActivePiles)
                    {
                        var toAssign = math.min(pileA.Capacity, overflow);
                        overflow -= toAssign;
                        var splitEntity = state.EntityManager.CreateEntity(typeof(AggregatePile), typeof(LocalTransform));
                        state.EntityManager.SetComponentData(splitEntity, new AggregatePile
                        {
                            ResourceType = pileA.ResourceType,
                            Amount = toAssign,
                            Capacity = pileA.Capacity
                        });

                        state.EntityManager.SetComponentData(splitEntity, LocalTransform.FromPosition(transformA.Position));
                        runtime.ActivePiles++;
                    }
                }
            }

            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }

        public void OnDestroy(ref SystemState state)
        {
        }
    }
}
