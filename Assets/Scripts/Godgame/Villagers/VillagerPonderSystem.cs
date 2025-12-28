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

            foreach (var (goal, assignment, ponder, nav, transform) in SystemAPI
                         .Query<RefRO<VillagerGoalState>, RefRO<JobAssignment>, RefRW<VillagerPonderState>, RefRW<Navigation>, RefRO<LocalTransform>>())
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
                    anchor = transform.ValueRO.Position;
                    ponder.ValueRW.AnchorPosition = anchor;
                }

                nav.ValueRW.Destination = anchor;
                nav.ValueRW.Speed = 0f;
            }
        }
    }
}
