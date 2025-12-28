using PureDOTS.Runtime.Components;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace Godgame.Combat
{
    /// <summary>
    /// Tracks focus usage and awards XP/wisdom bonuses.
    /// Entities who push themselves grow faster than those who relax.
    /// </summary>
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(FocusAbilitySystem))]
    public partial struct FocusGrowthSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<TimeState>();

            // Create default growth config if not present
            if (!SystemAPI.HasSingleton<FocusGrowthConfig>())
            {
                var configEntity = state.EntityManager.CreateEntity();
                state.EntityManager.AddComponentData(configEntity, FocusGrowthConfig.Default);
            }
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var currentTick = SystemAPI.GetSingleton<TimeState>().Tick;
            var config = SystemAPI.GetSingleton<FocusGrowthConfig>();

            new TrackFocusUsageJob
            {
                CurrentTick = currentTick,
                Config = config
            }.ScheduleParallel();
        }

        /// <summary>
        /// Tracks focus spent and updates growth metrics.
        /// </summary>
        [BurstCompile]
        public partial struct TrackFocusUsageJob : IJobEntity
        {
            public uint CurrentTick;
            public FocusGrowthConfig Config;

            public void Execute(
                ref FocusGrowthTracking tracking,
                in EntityFocus focus,
                in DynamicBuffer<ActiveFocusAbility> activeAbilities)
            {
                // Skip if already updated this tick
                if (tracking.LastUpdateTick == CurrentTick)
                    return;

                // Calculate current intensity (ratio of drain to max focus)
                float intensity = focus.MaxFocus > 0
                    ? math.clamp(focus.TotalDrainRate / (focus.MaxFocus * 0.1f), 0f, 1f)
                    : 0f;

                // Track focus being spent this frame (TotalDrainRate is per-second)
                float focusSpentThisFrame = focus.TotalDrainRate * (1f / 30f); // Assume 30 TPS
                if (focusSpentThisFrame > 0f)
                {
                    FocusGrowthHelpers.RecordFocusSpent(ref tracking, focusSpentThisFrame, intensity);
                }

                tracking.LastUpdateTick = CurrentTick;
            }
        }
    }

    /// <summary>
    /// Determines entity motivation based on their situation.
    /// Runs less frequently than growth tracking.
    /// </summary>
    [UpdateInGroup(typeof(SimulationSystemGroup), OrderLast = true)]
    public partial struct FocusMotivationSystem : ISystem
    {
        private uint _lastUpdateTick;
        private const uint UPDATE_INTERVAL = 300; // Every 10 seconds at 30 TPS

        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<TimeState>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var currentTick = SystemAPI.GetSingleton<TimeState>().Tick;

            // Only update motivation periodically
            if (currentTick - _lastUpdateTick < UPDATE_INTERVAL)
                return;

            _lastUpdateTick = currentTick;

            new UpdateMotivationJob().ScheduleParallel();
        }

        /// <summary>
        /// Updates entity motivation based on current circumstances.
        /// </summary>
        [BurstCompile]
        public partial struct UpdateMotivationJob : IJobEntity
        {
            public void Execute(
                ref FocusGrowthTracking tracking,
                in EntityFocus focus)
            {
                // Determine motivation from circumstances
                // Priority order: Survival > Desperate > Passionate > Ambitious > Perfectionist > Dutiful > Casual > Leisurely

                FocusMotivation newMotivation;

                // Check for survival situation (very low health, in combat, etc.)
                // TODO: Add health check when integrated
                bool inDanger = false; // Placeholder

                // Check for desperate circumstances
                // TODO: Add debt/threat check when integrated
                bool isDesparate = false; // Placeholder

                if (inDanger)
                {
                    newMotivation = FocusMotivation.Survival;
                }
                else if (isDesparate)
                {
                    newMotivation = FocusMotivation.Desperate;
                }
                else
                {
                    // Base motivation from personality
                    if (tracking.DrivePersonality >= 0.9f)
                        newMotivation = FocusMotivation.Passionate;
                    else if (tracking.DrivePersonality >= 0.75f)
                        newMotivation = FocusMotivation.Ambitious;
                    else if (tracking.DrivePersonality >= 0.6f)
                        newMotivation = FocusMotivation.Perfectionist;
                    else if (tracking.DrivePersonality >= 0.4f)
                        newMotivation = FocusMotivation.Dutiful;
                    else if (tracking.DrivePersonality >= 0.2f)
                        newMotivation = FocusMotivation.Casual;
                    else
                        newMotivation = FocusMotivation.Leisurely;
                }

                tracking.CurrentMotivation = newMotivation;
            }
        }
    }

    /// <summary>
    /// Applies focus-based XP bonus when XP is awarded.
    /// Call this from XP allocation systems.
    /// </summary>
    public static class FocusXPIntegration
    {
        /// <summary>
        /// Calculates total XP with focus bonuses applied.
        /// </summary>
        public static uint ApplyFocusXPBonus(
            uint baseXP,
            float focusSpentOnAction,
            in FocusGrowthTracking tracking,
            in EntityFocus focus,
            in FocusGrowthConfig config)
        {
            // Base focus bonus from spending focus
            uint focusBonus = FocusGrowthHelpers.CalculateFocusXPBonus(focusSpentOnAction, baseXP, config);

            // Daily modifier based on effort level
            float dailyMod = FocusGrowthHelpers.GetDailyXPModifier(tracking, focus, config);

            // Calculate final XP
            float totalXP = (baseXP + focusBonus) * dailyMod;

            return (uint)math.max(1, totalXP);
        }

        /// <summary>
        /// Gets wisdom bonus from lifetime focus usage.
        /// Add this to base wisdom when querying stats.
        /// </summary>
        public static byte GetWisdomBonus(in FocusGrowthTracking tracking, in FocusGrowthConfig config)
        {
            return FocusGrowthHelpers.CalculateWisdomFromFocus(tracking.TotalFocusSpent, config);
        }
    }
}

