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
    /// Reduces need urgencies when villagers reach goal targets in non-headless gameplay.
    /// </summary>
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(VillagerNeedMovementSystem))]
    public partial struct VillagerNeedSatisfactionSystem : ISystem
    {
        private ComponentLookup<LocalTransform> _transformLookup;

        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<TimeState>();
            state.RequireForUpdate<VillagerNeedState>();
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

            var schedule = SystemAPI.GetSingleton<VillagerScheduleConfig>();
            _transformLookup.Update(ref state);

            var hasSettlement = TryGetSettlementRuntime(ref state, out var runtime);
            var deltaTime = SystemAPI.Time.DeltaTime;
            var fixedDt = math.max(timeState.FixedDeltaTime, 1e-4f);
            var arrivalDistanceSq = 2.5f * 2.5f;

            foreach (var (needs, goal, tuning, transform) in SystemAPI
                         .Query<RefRW<VillagerNeedState>, RefRO<VillagerGoalState>, RefRO<VillagerNeedTuning>, RefRO<LocalTransform>>())
            {
                var currentGoal = goal.ValueRO.CurrentGoal;
                if (!IsNeedGoal(currentGoal))
                {
                    continue;
                }

                if (!TryResolveNeedTarget(ref state, currentGoal, hasSettlement, runtime, out var targetPosition))
                {
                    continue;
                }

                var distSq = math.distancesq(transform.ValueRO.Position.xz, targetPosition.xz);
                if (distSq > arrivalDistanceSq)
                {
                    continue;
                }

                var delta = ComputeSatisfactionDelta(currentGoal, tuning.ValueRO, fixedDt, deltaTime, schedule.NeedSatisfyRate);
                if (delta <= 0f)
                {
                    continue;
                }

                var updated = needs.ValueRO;
                ApplySatisfaction(currentGoal, ref updated, delta);
                needs.ValueRW = updated;
            }
        }

        private static float ComputeSatisfactionDelta(VillagerGoal goal, in VillagerNeedTuning tuning, float fixedDt, float deltaTime, float baselineRate)
        {
            var perTickIncrease = goal switch
            {
                VillagerGoal.Eat => tuning.HungerDecayPerTick,
                VillagerGoal.Sleep => tuning.RestDecayPerTick,
                VillagerGoal.Pray => tuning.FaithDecayPerTick,
                VillagerGoal.SeekShelter => tuning.SafetyDecayPerTick,
                VillagerGoal.Socialize => tuning.SocialDecayPerTick,
                _ => 0f
            };

            perTickIncrease = math.max(0f, perTickIncrease);
            var requiredPerSecond = perTickIncrease / math.max(fixedDt, 1e-4f);
            var satisfyPerSecond = math.max(baselineRate, requiredPerSecond * 1.5f);
            return satisfyPerSecond * deltaTime;
        }

        private static bool IsNeedGoal(VillagerGoal goal)
        {
            return goal == VillagerGoal.Eat
                   || goal == VillagerGoal.Sleep
                   || goal == VillagerGoal.Pray
                   || goal == VillagerGoal.SeekShelter
                   || goal == VillagerGoal.Socialize;
        }

        private static bool TryResolveNeedTarget(ref SystemState state, VillagerGoal goal, bool hasSettlement, in SettlementRuntime runtime, out float3 targetPosition)
        {
            targetPosition = float3.zero;
            if (!hasSettlement)
            {
                return false;
            }

            var target = goal switch
            {
                VillagerGoal.Eat => runtime.StorehouseInstance != Entity.Null
                    ? runtime.StorehouseInstance
                    : runtime.VillageCenterInstance,
                VillagerGoal.Sleep => runtime.HousingInstance != Entity.Null
                    ? runtime.HousingInstance
                    : runtime.StorehouseInstance,
                VillagerGoal.Pray => runtime.WorshipInstance != Entity.Null
                    ? runtime.WorshipInstance
                    : runtime.VillageCenterInstance,
                VillagerGoal.SeekShelter => runtime.HousingInstance != Entity.Null
                    ? runtime.HousingInstance
                    : runtime.VillageCenterInstance,
                VillagerGoal.Socialize => runtime.VillageCenterInstance != Entity.Null
                    ? runtime.VillageCenterInstance
                    : runtime.StorehouseInstance,
                _ => Entity.Null
            };

            return TryGetTargetPosition(target, ref state, out targetPosition);
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

        private static void ApplySatisfaction(VillagerGoal goal, ref VillagerNeedState needs, float delta)
        {
            switch (goal)
            {
                case VillagerGoal.Eat:
                    needs.HungerUrgency = math.max(0f, needs.HungerUrgency - delta);
                    break;
                case VillagerGoal.Sleep:
                    needs.RestUrgency = math.max(0f, needs.RestUrgency - delta);
                    break;
                case VillagerGoal.Pray:
                    needs.FaithUrgency = math.max(0f, needs.FaithUrgency - delta);
                    break;
                case VillagerGoal.SeekShelter:
                    needs.SafetyUrgency = math.max(0f, needs.SafetyUrgency - delta);
                    break;
                case VillagerGoal.Socialize:
                    needs.SocialUrgency = math.max(0f, needs.SocialUrgency - delta);
                    break;
            }
        }
    }
}
