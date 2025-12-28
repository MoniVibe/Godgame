using PureDOTS.Runtime.Components;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Godgame.AI; // For VillagerRoutine (resting state)
using VillagerRoutine = Godgame.AI.VillagerRoutine;

namespace Godgame.Combat
{
    /// <summary>
    /// Regenerates focus over time.
    /// Regen rate is modified by:
    /// - Resting state (idle/sleeping = faster regen)
    /// - Combat state (slower regen)
    /// - Active abilities draining focus
    /// </summary>
    [BurstCompile]
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public partial struct FocusRegenSystem : ISystem
    {
        private ComponentLookup<VillagerRoutine> _routineLookup;

        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<TimeState>();

            // Create default config if not present
            if (!SystemAPI.HasSingleton<FocusConfig>())
            {
                var configEntity = state.EntityManager.CreateEntity();
                state.EntityManager.AddComponentData(configEntity, FocusConfig.Default);
            }

            _routineLookup = state.GetComponentLookup<VillagerRoutine>(true);
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var timeState = SystemAPI.GetSingleton<TimeState>();
            var deltaTime = timeState.DeltaTime;
            var config = SystemAPI.GetSingleton<FocusConfig>();
            var currentTick = timeState.Tick;

            _routineLookup.Update(ref state);

            new FocusRegenJob
            {
                DeltaTime = deltaTime,
                Config = config,
                CurrentTick = currentTick,
                RoutineLookup = _routineLookup
            }.ScheduleParallel();
        }

        [BurstCompile]
        public partial struct FocusRegenJob : IJobEntity
        {
            public float DeltaTime;
            public FocusConfig Config;
            public uint CurrentTick;

            [ReadOnly] public ComponentLookup<VillagerRoutine> RoutineLookup;

            public void Execute(Entity entity, ref EntityFocus focus, DynamicBuffer<ActiveFocusAbility> activeAbilities)
            {
                // Skip if in coma
                if (focus.IsInComa) return;

                // Calculate total drain from active abilities
                float totalDrain = 0f;
                for (int i = 0; i < activeAbilities.Length; i++)
                {
                    var ability = activeAbilities[i];
                    if (ability.CostType == FocusCostType.PerSecond)
                    {
                        totalDrain += ability.DrainRate;
                    }
                }
                focus.TotalDrainRate = totalDrain;

                // Determine regen multiplier based on state
                float regenMultiplier = 1f;

                // Check if entity is resting
                if (RoutineLookup.TryGetComponent(entity, out var routine))
                {
                    // Resting phases get idle bonus
                    bool isResting = routine.CurrentPhase == DailyPhase.Midnight ||
                                     routine.CurrentPhase == DailyPhase.Dusk;

                    if (isResting)
                    {
                        regenMultiplier = Config.IdleRegenMultiplier;
                    }
                }

                // If actively draining (in combat/using abilities), reduce regen
                if (totalDrain > 0f)
                {
                    regenMultiplier = math.min(regenMultiplier, Config.CombatRegenMultiplier);
                }

                // Apply regeneration
                focus.CurrentRegenRate = focus.BaseRegenRate * regenMultiplier;
                float netRegen = focus.CurrentRegenRate - totalDrain;

                focus.CurrentFocus += netRegen * DeltaTime;
                focus.CurrentFocus = math.clamp(focus.CurrentFocus, 0f, focus.MaxFocus);

                focus.LastUpdateTick = CurrentTick;
            }
        }
    }
}
