using Godgame.Registry;
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
    /// Assigns lightweight cooperation intents so villagers cluster around shared work targets.
    /// </summary>
    [UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
    [UpdateBefore(typeof(VillagerJobSystem))]
    public partial struct VillagerCooperationIntentSystem : ISystem
    {
        private EntityQuery _storehouseQuery;

        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<TimeState>();
            state.RequireForUpdate<VillagerGoalState>();
            _storehouseQuery = SystemAPI.QueryBuilder()
                .WithAll<GodgameStorehouse, LocalTransform>()
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

            var storehouseEntities = _storehouseQuery.ToEntityArray(state.WorldUpdateAllocator);
            var storehouseTransforms = _storehouseQuery.ToComponentDataArray<LocalTransform>(state.WorldUpdateAllocator);
            if (storehouseEntities.Length == 0)
            {
                return;
            }

            var ecb = new EntityCommandBuffer(Allocator.Temp);

            foreach (var (goal, behavior, job, transform, entity) in SystemAPI
                         .Query<RefRO<VillagerGoalState>, RefRO<VillagerBehavior>, RefRO<VillagerJobState>, RefRO<LocalTransform>>()
                         .WithEntityAccess())
            {
                if (goal.ValueRO.CurrentGoal != VillagerGoal.Work || job.ValueRO.ResourceTypeIndex == ushort.MaxValue)
                {
                    if (SystemAPI.HasComponent<VillagerCooperationIntent>(entity))
                    {
                        ecb.SetComponent(entity, new VillagerCooperationIntent
                        {
                            ResourceTypeIndex = ushort.MaxValue,
                            Urgency = 0f,
                            SharedStorehouse = Entity.Null,
                            SharedNode = Entity.Null,
                            AssignedTick = timeState.Tick
                        });
                    }
                    continue;
                }

                var patience01 = math.saturate((behavior.ValueRO.PatienceScore + 100f) * 0.005f);
                var urgency = math.lerp(0.35f, 0.9f, patience01);

                var nearestStorehouse = Entity.Null;
                var minDistSq = float.MaxValue;
                for (int i = 0; i < storehouseEntities.Length; i++)
                {
                    var distSq = math.distancesq(transform.ValueRO.Position, storehouseTransforms[i].Position);
                    if (distSq < minDistSq)
                    {
                        minDistSq = distSq;
                        nearestStorehouse = storehouseEntities[i];
                    }
                }

                var intent = new VillagerCooperationIntent
                {
                    ResourceTypeIndex = job.ValueRO.ResourceTypeIndex,
                    Urgency = urgency,
                    SharedStorehouse = nearestStorehouse,
                    SharedNode = Entity.Null,
                    AssignedTick = timeState.Tick
                };

                if (SystemAPI.HasComponent<VillagerCooperationIntent>(entity))
                {
                    ecb.SetComponent(entity, intent);
                }
                else
                {
                    ecb.AddComponent(entity, intent);
                }
            }

            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }
    }
}
