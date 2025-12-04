using Godgame.Villagers;
using PureDOTS.Runtime.Components;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;

namespace Godgame.AI
{
    /// <summary>
    /// Updates villager initiative based on personality, mood, and grudges.
    /// Initiative controls how frequently villagers make autonomous decisions.
    /// </summary>
    [BurstCompile]
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public partial struct VillagerInitiativeSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<TimeState>();

            // Create config singleton if it doesn't exist
            if (!SystemAPI.HasSingleton<InitiativeConfig>())
            {
                var configEntity = state.EntityManager.CreateEntity();
                state.EntityManager.AddComponentData(configEntity, InitiativeConfig.Default);
            }
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var timeState = SystemAPI.GetSingleton<TimeState>();
            if (timeState.IsPaused) return;

            var config = SystemAPI.GetSingleton<InitiativeConfig>();
            var currentTick = timeState.Tick;

            // Update initiative for all villagers with initiative tracking
            foreach (var (initiative, behavior, mood) in SystemAPI.Query<
                RefRW<VillagerInitiative>,
                RefRO<VillagerBehavior>,
                RefRO<VillagerMood>>())
            {
                UpdateInitiative(ref initiative.ValueRW, behavior.ValueRO, mood.ValueRO, config, currentTick);
            }

            // Also handle villagers without mood component
            foreach (var (initiative, behavior) in SystemAPI.Query<
                RefRW<VillagerInitiative>,
                RefRO<VillagerBehavior>>()
                .WithNone<VillagerMood>())
            {
                UpdateInitiativeWithoutMood(ref initiative.ValueRW, behavior.ValueRO, config, currentTick);
            }
        }

        private void UpdateInitiative(
            ref VillagerInitiative initiative,
            in VillagerBehavior behavior,
            in VillagerMood mood,
            in InitiativeConfig config,
            uint currentTick)
        {
            // Calculate base initiative from personality
            // Bold villagers: higher base initiative
            // Craven villagers: lower base initiative
            float baseFromBold = 50f + (behavior.BoldScore * 0.3f); // 20-80 range
            initiative.BaseInitiative = math.clamp(baseFromBold, 20f, 80f);

            // Apply mood modifier
            float moodModifier = mood.InitiativeModifier * 100f; // Convert to percentage points

            // Apply grudge boost
            float grudgeBoost = behavior.ActiveGrudgeCount * 5f; // +5 per active grudge

            // Calculate current initiative
            initiative.CurrentInitiative = math.clamp(
                initiative.BaseInitiative + moodModifier + grudgeBoost + behavior.InitiativeModifier,
                0f, 100f);

            // Determine initiative band
            initiative.Band = DetermineInitiativeBand(initiative.CurrentInitiative, config);

            // Update stress state based on external factors
            // (Simplified: use mood band as proxy for stress)
            initiative.StressState = mood.Band switch
            {
                MoodBand.Despair => InitiativeStressState.Panic,
                MoodBand.Elated => InitiativeStressState.Frenzy,
                MoodBand.Cheerful => InitiativeStressState.Rally,
                _ => InitiativeStressState.Calm
            };

            // Tick down action cooldown
            if (initiative.TicksUntilAction > 0)
            {
                initiative.TicksUntilAction--;
            }

            initiative.LastUpdateTick = currentTick;
        }

        private void UpdateInitiativeWithoutMood(
            ref VillagerInitiative initiative,
            in VillagerBehavior behavior,
            in InitiativeConfig config,
            uint currentTick)
        {
            // Calculate base initiative from personality only
            float baseFromBold = 50f + (behavior.BoldScore * 0.3f);
            initiative.BaseInitiative = math.clamp(baseFromBold, 20f, 80f);

            float grudgeBoost = behavior.ActiveGrudgeCount * 5f;

            initiative.CurrentInitiative = math.clamp(
                initiative.BaseInitiative + grudgeBoost + behavior.InitiativeModifier,
                0f, 100f);

            initiative.Band = DetermineInitiativeBand(initiative.CurrentInitiative, config);
            initiative.StressState = InitiativeStressState.Calm;

            if (initiative.TicksUntilAction > 0)
            {
                initiative.TicksUntilAction--;
            }

            initiative.LastUpdateTick = currentTick;
        }

        private InitiativeBand DetermineInitiativeBand(float initiative, in InitiativeConfig config)
        {
            if (initiative >= config.RecklessThreshold)
                return InitiativeBand.Reckless;
            if (initiative >= config.BoldThreshold)
                return InitiativeBand.Bold;
            if (initiative < config.SlowThreshold)
                return InitiativeBand.Slow;
            return InitiativeBand.Measured;
        }
    }
}

