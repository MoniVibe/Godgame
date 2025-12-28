using Godgame.Relations;
using PureDOTS.Runtime.AI;
using PureDOTS.Runtime.Components;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Godgame.Villagers
{
    /// <summary>
    /// Chooses a nearby social target, biased by current goals and relations.
    /// </summary>
    [UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
    [UpdateBefore(typeof(VillagerNeedMovementSystem))]
    public partial struct VillagerSocialFocusSystem : ISystem
    {
        private EntityQuery _villagerQuery;
        private ComponentLookup<VillagerSocialFocus> _focusLookup;

        public void OnCreate(ref SystemState state)
        {
            _villagerQuery = SystemAPI.QueryBuilder()
                .WithAll<LocalTransform, VillagerGoalState, VillagerBehavior, VillagerSocialFocus>()
                .Build();
            _focusLookup = state.GetComponentLookup<VillagerSocialFocus>(false);
            state.RequireForUpdate<TimeState>();
            state.RequireForUpdate<VillagerScheduleConfig>();
        }

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
            var radius = math.max(0f, schedule.NeedSocialRadius);
            if (radius <= 0f)
            {
                return;
            }

            var entities = _villagerQuery.ToEntityArray(Allocator.Temp);
            var transforms = _villagerQuery.ToComponentDataArray<LocalTransform>(Allocator.Temp);
            var goals = _villagerQuery.ToComponentDataArray<VillagerGoalState>(Allocator.Temp);
            var behaviors = _villagerQuery.ToComponentDataArray<VillagerBehavior>(Allocator.Temp);

            _focusLookup.Update(ref state);
            var radiusSq = radius * radius;
            var relationWeight = schedule.SocialPreferRelationWeight;
            var socialBonus = schedule.SocialPreferGoalBonus;
            var pickMin = math.max(0f, schedule.SocialPickMinSeconds);
            var pickMax = math.max(pickMin, schedule.SocialPickMaxSeconds);

            for (int i = 0; i < entities.Length; i++)
            {
                var entity = entities[i];
                var goal = goals[i];
                if (goal.CurrentGoal != VillagerGoal.Socialize)
                {
                    if (_focusLookup.HasComponent(entity))
                    {
                        var focus = _focusLookup[entity];
                        focus.Target = Entity.Null;
                        focus.NextPickTick = 0;
                        _focusLookup[entity] = focus;
                    }
                    continue;
                }

                if (!_focusLookup.HasComponent(entity))
                {
                    continue;
                }

                var focusState = _focusLookup[entity];
                if (focusState.Target != Entity.Null && state.EntityManager.Exists(focusState.Target))
                {
                    if (timeState.Tick < focusState.NextPickTick)
                    {
                        continue;
                    }
                }

                var bestTarget = Entity.Null;
                var bestScore = float.MaxValue;

                for (int j = 0; j < entities.Length; j++)
                {
                    if (i == j)
                    {
                        continue;
                    }

                    var candidate = entities[j];
                    var distSq = math.distancesq(transforms[i].Position, transforms[j].Position);
                    if (distSq > radiusSq)
                    {
                        continue;
                    }

                    var score = distSq;
                    if (socialBonus > 0f && goals[j].CurrentGoal == VillagerGoal.Socialize)
                    {
                        score -= socialBonus;
                    }

                    if (relationWeight > 0f && TryGetRelationValue(ref state, entity, candidate, out var relationValue))
                    {
                        score -= relationValue * relationWeight;
                    }

                    if (score < bestScore)
                    {
                        bestScore = score;
                        bestTarget = candidate;
                    }
                }

                focusState.Target = bestTarget;
                focusState.NextPickTick = timeState.Tick + ResolvePickTicks(pickMin, pickMax, behaviors[i], timeState.FixedDeltaTime, entity);
                _focusLookup[entity] = focusState;
            }

            entities.Dispose();
            transforms.Dispose();
            goals.Dispose();
            behaviors.Dispose();
        }

        private static uint ResolvePickTicks(float minSeconds, float maxSeconds, in VillagerBehavior behavior, float fixedDeltaTime, Entity entity)
        {
            if (maxSeconds <= 0f)
            {
                return 1;
            }

            var min = math.max(0f, minSeconds);
            var max = math.max(min, maxSeconds);
            var patience01 = math.saturate((behavior.PatienceScore + 100f) * 0.005f);
            var seed = math.hash(new uint2((uint)(entity.Index + 13), 991u));
            var random = Unity.Mathematics.Random.CreateFromIndex(seed == 0u ? 1u : seed);
            var sample = random.NextFloat(min, max);
            var seconds = math.lerp(min, sample, math.lerp(0.6f, 1.1f, patience01));
            return (uint)math.max(1f, math.ceil(seconds / math.max(1e-4f, fixedDeltaTime)));
        }

        private static bool TryGetRelationValue(ref SystemState state, Entity owner, Entity target, out sbyte value)
        {
            value = 0;
            if (!state.EntityManager.HasBuffer<EntityRelation>(owner))
            {
                return false;
            }

            var relations = state.EntityManager.GetBuffer<EntityRelation>(owner);
            for (int i = 0; i < relations.Length; i++)
            {
                if (relations[i].OtherEntity.Equals(target))
                {
                    value = relations[i].RelationValue;
                    return true;
                }
            }

            return false;
        }
    }
}
