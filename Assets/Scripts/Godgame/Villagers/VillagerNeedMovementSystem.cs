using Godgame.Scenario;
using PureDOTS.Runtime.AI;
using PureDOTS.Runtime.Components;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Godgame.Villagers
{
    /// <summary>
    /// Moves villagers toward non-work goals (eat, rest, pray, socialize, shelter, flee).
    /// </summary>
    [BurstCompile]
    [UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
    [UpdateAfter(typeof(VillagerJobSystem))]
    public partial struct VillagerNeedMovementSystem : ISystem
    {
        private ComponentLookup<LocalTransform> _transformLookup;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<TimeState>();
            state.RequireForUpdate<VillagerGoalState>();
            state.RequireForUpdate<VillagerScheduleConfig>();
            _transformLookup = state.GetComponentLookup<LocalTransform>(true);
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

            _transformLookup.Update(ref state);
            var schedule = SystemAPI.GetSingleton<VillagerScheduleConfig>();
            var moveSpeedBase = math.max(0.1f, schedule.NeedMoveSpeed);

            var hasSettlement = TryGetSettlementRuntime(ref state, out var runtime);
            var deltaTime = SystemAPI.Time.DeltaTime;

            foreach (var (goal, behavior, nav, transform, entity) in SystemAPI
                         .Query<RefRO<VillagerGoalState>, RefRO<VillagerBehavior>, RefRW<Navigation>, RefRW<LocalTransform>>()
                         .WithEntityAccess())
            {
                var currentGoal = goal.ValueRO.CurrentGoal;
                if (currentGoal == VillagerGoal.Work)
                {
                    continue;
                }

                float3 targetPosition;
                if (currentGoal == VillagerGoal.Flee)
                {
                    targetPosition = nav.ValueRO.Destination;
                }
                else if (!TryResolveNeedTarget(ref state, currentGoal, hasSettlement, runtime, transform.ValueRO.Position, out targetPosition))
                {
                    continue;
                }

                nav.ValueRW.Destination = targetPosition;

                var patience01 = math.saturate((behavior.ValueRO.PatienceScore + 100f) * 0.005f);
                var speedScale = math.lerp(1.15f, 0.65f, patience01);
                var moveSpeed = math.max(0.05f, moveSpeedBase * speedScale);
                nav.ValueRW.Speed = moveSpeed;

                var toTarget = targetPosition - transform.ValueRO.Position;
                toTarget.y = 0f;
                var distance = math.length(toTarget);
                if (distance <= 0.01f)
                {
                    continue;
                }

                var move = math.min(distance, moveSpeed * deltaTime);
                var dir = toTarget / math.max(distance, 1e-4f);
                transform.ValueRW.Position += dir * move;
            }
        }

        private static bool TryResolveNeedTarget(ref SystemState state, VillagerGoal goal, bool hasSettlement, in SettlementRuntime runtime, float3 origin, out float3 targetPosition)
        {
            targetPosition = float3.zero;

            if (hasSettlement)
            {
                switch (goal)
                {
                    case VillagerGoal.Eat:
                        return TryGetTargetPosition(runtime.StorehouseInstance, ref state, out targetPosition);
                    case VillagerGoal.Sleep:
                    case VillagerGoal.SeekShelter:
                        return TryGetTargetPosition(runtime.HousingInstance, ref state, out targetPosition);
                    case VillagerGoal.Pray:
                        return TryGetTargetPosition(runtime.WorshipInstance, ref state, out targetPosition);
                    case VillagerGoal.Socialize:
                        return TryGetTargetPosition(runtime.VillageCenterInstance, ref state, out targetPosition);
                }
            }

            return false;
        }

        private static bool TryGetTargetPosition(Entity target, ref SystemState state, out float3 position)
        {
            position = float3.zero;
            if (target == Entity.Null || !state.EntityManager.Exists(target))
            {
                return false;
            }

            if (!state.EntityManager.HasComponent<LocalTransform>(target))
            {
                return false;
            }

            position = state.EntityManager.GetComponentData<LocalTransform>(target).Position;
            return true;
        }

        private bool TryGetSettlementRuntime(ref SystemState state, out SettlementRuntime runtime)
        {
            runtime = default;
            var bestScore = int.MinValue;
            var found = false;

            foreach (var runtimeRef in SystemAPI.Query<RefRO<SettlementRuntime>>())
            {
                var candidate = runtimeRef.ValueRO;
                var score = ScoreSettlementRuntime(ref state, candidate);
                if (score > bestScore)
                {
                    bestScore = score;
                    runtime = candidate;
                    found = true;
                }
            }

            return found && bestScore > int.MinValue;
        }

        private int ScoreSettlementRuntime(ref SystemState state, in SettlementRuntime runtime)
        {
            var score = 0;

            if (TryGetTargetPosition(runtime.StorehouseInstance, ref state, out _))
            {
                score += 200;
            }

            if (TryGetTargetPosition(runtime.VillageCenterInstance, ref state, out _))
            {
                score += 100;
            }

            if (TryGetTargetPosition(runtime.HousingInstance, ref state, out _))
            {
                score += 60;
            }

            if (TryGetTargetPosition(runtime.WorshipInstance, ref state, out _))
            {
                score += 40;
            }

            return score;
        }
    }
}
