using PureDOTS.Runtime.AI;
using PureDOTS.Runtime.Components;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Godgame.Villagers
{
    /// <summary>
    /// Holds villagers in place while they ponder work decisions.
    /// </summary>
    [UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
    [UpdateAfter(typeof(VillagerJobSystem))]
    [UpdateBefore(typeof(VillagerNeedMovementSystem))]
    public partial struct VillagerPonderSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<TimeState>();
            state.RequireForUpdate<VillagerPonderState>();
            state.RequireForUpdate<JobAssignment>();
            state.RequireForUpdate<VillagerGoalState>();
            state.RequireForUpdate<VillagerScheduleConfig>();
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

            var deltaTime = SystemAPI.Time.DeltaTime;
            var schedule = SystemAPI.GetSingleton<VillagerScheduleConfig>();

            foreach (var (goal, assignment, ponder, nav, transform, entity) in SystemAPI
                         .Query<RefRO<VillagerGoalState>, RefRO<JobAssignment>, RefRW<VillagerPonderState>, RefRW<Navigation>, RefRO<LocalTransform>>()
                         .WithEntityAccess())
            {
                if (goal.ValueRO.CurrentGoal != VillagerGoal.Work)
                {
                    ponder.ValueRW.RemainingSeconds = 0f;
                    continue;
                }

                if (assignment.ValueRO.Ticket != Entity.Null)
                {
                    ponder.ValueRW.RemainingSeconds = 0f;
                    continue;
                }

                var remaining = ponder.ValueRO.RemainingSeconds;
                if (remaining <= 0f)
                {
                    continue;
                }

                ponder.ValueRW.RemainingSeconds = math.max(0f, remaining - deltaTime);
                var anchor = ponder.ValueRO.AnchorPosition;
                if (math.lengthsq(anchor) <= 1e-4f)
                {
                    anchor = transform.ValueRO.Position + ResolvePonderOffset(entity, schedule.NeedWanderRadius);
                    ponder.ValueRW.AnchorPosition = anchor;
                }

                nav.ValueRW.Destination = anchor;
                var arrivalDistance = math.max(0.1f, schedule.NeedWanderRadius * 0.05f);
                if (math.lengthsq(anchor - transform.ValueRO.Position) <= arrivalDistance * arrivalDistance)
                {
                    nav.ValueRW.Speed = 0f;
                    continue;
                }

                var shuffleSpeed = math.max(0.01f, schedule.NeedMoveSpeed * 0.35f);
                nav.ValueRW.Speed = shuffleSpeed;
            }
        }

        private static float3 ResolvePonderOffset(Entity entity, float baseRadius)
        {
            var radius = math.max(0.1f, baseRadius * 0.25f);
            var seed = math.hash(new uint2((uint)entity.Index, (uint)entity.Version));
            var angle = (seed / (float)uint.MaxValue) * math.PI * 2f;
            return new float3(math.cos(angle), 0f, math.sin(angle)) * radius;
        }
    }
}
