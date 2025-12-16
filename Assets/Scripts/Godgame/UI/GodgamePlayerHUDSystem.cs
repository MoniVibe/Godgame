using PureDOTS.Runtime.AI;
using PureDOTS.Runtime.Components;
using PureDOTS.Runtime.Core;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace Godgame.Presentation
{
    /// <summary>
    /// Aggregates villager intent/focus data for the MonoBehaviour HUD.
    /// </summary>
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public partial struct GodgamePlayerHUDSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<GameWorldTag>();

            var query = SystemAPI.QueryBuilder()
                .WithAll<PlayerHUDSingleton>()
                .Build();
            if (query.IsEmptyIgnoreFilter)
            {
                var hudEntity = state.EntityManager.CreateEntity();
                state.EntityManager.AddComponent<PlayerHUDSingleton>(hudEntity);
                state.EntityManager.AddComponent<PlayerHUDData>(hudEntity);
                state.EntityManager.SetName(hudEntity, "PlayerHUDSingleton");
            }
        }

        public void OnUpdate(ref SystemState state)
        {
            var query = SystemAPI.QueryBuilder()
                .WithAll<VillagerGoalState, FocusBudget, VillagerNeedState>()
                .Build();

            var hudEntity = SystemAPI.GetSingletonEntity<PlayerHUDSingleton>();
            var hudData = SystemAPI.GetComponent<PlayerHUDData>(hudEntity);

            var villagerCount = query.CalculateEntityCount();
            if (villagerCount == 0)
            {
                hudData.VillagerCount = 0;
                hudData.AverageVillagerFocus = 0f;
                hudData.PeakNeedUrgency = 0f;
                hudData.DominantVillagerGoal = default;
                state.EntityManager.SetComponentData(hudEntity, hudData);
                return;
            }

            var goalCounts = new NativeArray<int>(8, Allocator.Temp);
            float focusAccumulator = 0f;
            float peakNeed = 0f;

            foreach (var (goal, focus, needs) in SystemAPI
                         .Query<RefRO<VillagerGoalState>, RefRO<FocusBudget>, RefRO<VillagerNeedState>>())
            {
                focusAccumulator += focus.ValueRO.Current;
                peakNeed = math.max(peakNeed, GetMaxNeedUrgency(in needs.ValueRO));
                goalCounts[(int)goal.ValueRO.CurrentGoal]++;
            }

            hudData.VillagerCount = villagerCount;
            hudData.AverageVillagerFocus = focusAccumulator / villagerCount;
            hudData.PeakNeedUrgency = peakNeed;
            hudData.DominantVillagerGoal = new FixedString64Bytes(GetDominantGoalLabel(goalCounts));

            state.EntityManager.SetComponentData(hudEntity, hudData);
            goalCounts.Dispose();
        }

        private static float GetMaxNeedUrgency(in VillagerNeedState state)
        {
            var maxA = math.max(state.HungerUrgency, state.RestUrgency);
            var maxB = math.max(state.FaithUrgency, state.SafetyUrgency);
            var maxC = math.max(state.SocialUrgency, state.WorkUrgency);
            return math.max(maxA, math.max(maxB, maxC));
        }

        private static string GetDominantGoalLabel(NativeArray<int> counts)
        {
            var dominantIndex = 0;
            var dominantCount = -1;
            for (int i = 0; i < counts.Length; i++)
            {
                if (counts[i] > dominantCount)
                {
                    dominantIndex = i;
                    dominantCount = counts[i];
                }
            }

            return ((VillagerGoal)dominantIndex) switch
            {
                VillagerGoal.Work => "Work",
                VillagerGoal.Eat => "Eat",
                VillagerGoal.Sleep => "Rest",
                VillagerGoal.SeekShelter => "Shelter",
                VillagerGoal.Pray => "Pray",
                VillagerGoal.Socialize => "Socialize",
                VillagerGoal.Flee => "Flee",
                _ => "Idle"
            };
        }

        public void OnDestroy(ref SystemState state) { }
    }
}
