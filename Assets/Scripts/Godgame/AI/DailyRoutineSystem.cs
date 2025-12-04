using PureDOTS.Runtime.Components;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace Godgame.AI
{
    /// <summary>
    /// Manages daily phase transitions and villager routine states.
    /// Tracks Dawn/Noon/Dusk/Midnight milestones and sleep patterns.
    /// </summary>
    [BurstCompile]
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public partial struct DailyRoutineSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<TimeState>();

            // Create config singleton if it doesn't exist
            if (!SystemAPI.HasSingleton<RoutineConfig>())
            {
                var configEntity = state.EntityManager.CreateEntity();
                state.EntityManager.AddComponentData(configEntity, RoutineConfig.Default);
            }

            // Create day time state singleton if it doesn't exist
            if (!SystemAPI.HasSingleton<DayTimeState>())
            {
                var dayStateEntity = state.EntityManager.CreateEntity();
                state.EntityManager.AddComponentData(dayStateEntity, DayTimeState.Default);
            }
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var timeState = SystemAPI.GetSingleton<TimeState>();
            if (timeState.IsPaused) return;

            var config = SystemAPI.GetSingleton<RoutineConfig>();
            var dayTimeState = SystemAPI.GetSingleton<DayTimeState>();
            var currentTick = timeState.Tick;

            // Update global day time state
            UpdateDayTimeState(ref dayTimeState, config, currentTick);

            // Write back the updated day time state
            foreach (var dayState in SystemAPI.Query<RefRW<DayTimeState>>())
            {
                dayState.ValueRW = dayTimeState;
            }

            // Update individual villager routines
            foreach (var routine in SystemAPI.Query<RefRW<VillagerRoutine>>())
            {
                var routineValue = routine.ValueRW;
                UpdateVillagerRoutine(ref routineValue, dayTimeState, config);
                routine.ValueRW = routineValue;
            }
        }

        private void UpdateDayTimeState(ref DayTimeState dayTimeState, in RoutineConfig config, uint currentTick)
        {
            // Calculate which day we're on
            uint newDayNumber = currentTick / config.TicksPerDay;
            if (newDayNumber != dayTimeState.DayNumber)
            {
                dayTimeState.DayNumber = newDayNumber;
                dayTimeState.DayStartTick = newDayNumber * config.TicksPerDay;
            }

            // Calculate time within day
            uint tickInDay = currentTick - dayTimeState.DayStartTick;
            dayTimeState.NormalizedDayTime = (float)tickInDay / config.TicksPerDay;

            // Determine current phase
            DailyPhase previousPhase = dayTimeState.CurrentPhase;

            if (tickInDay < config.NoonStartTick)
            {
                dayTimeState.CurrentPhase = DailyPhase.Dawn;
                dayTimeState.DayNight = DayNightState.Daylight;
            }
            else if (tickInDay < config.DuskStartTick)
            {
                dayTimeState.CurrentPhase = DailyPhase.Noon;
                dayTimeState.DayNight = DayNightState.Daylight;
            }
            else if (tickInDay < config.MidnightStartTick)
            {
                dayTimeState.CurrentPhase = DailyPhase.Dusk;
                dayTimeState.DayNight = DayNightState.Nightfall;
            }
            else
            {
                dayTimeState.CurrentPhase = DailyPhase.Midnight;
                dayTimeState.DayNight = DayNightState.Nightfall;
            }
        }

        private void UpdateVillagerRoutine(
            ref VillagerRoutine routine,
            in DayTimeState dayTimeState,
            in RoutineConfig config)
        {
            // Update current phase
            routine.CurrentPhase = dayTimeState.CurrentPhase;

            // Check if villager should be sleeping
            bool shouldSleep = routine.ShouldBeSleeping(dayTimeState.CurrentPhase, dayTimeState.DayNight);

            if (shouldSleep && !routine.IsSleeping)
            {
                // Start sleeping
                routine.IsSleeping = true;
            }
            else if (!shouldSleep && routine.IsSleeping)
            {
                // Wake up
                routine.IsSleeping = false;
            }

            // Update sleep debt
            if (routine.IsSleeping)
            {
                // Reduce sleep debt while sleeping
                routine.SleepDebt = math.max(0f, routine.SleepDebt - 1f);
            }
            else if (!shouldSleep)
            {
                // Accumulate sleep debt while awake (when should be awake)
                // Only accumulate if actually should have been sleeping
            }
            else
            {
                // Accumulate debt when should be sleeping but isn't
                routine.SleepDebt += 0.1f;
            }

            // Reset daily counters at dawn
            if (dayTimeState.CurrentPhase == DailyPhase.Dawn && routine.MealsToday > 0)
            {
                // This is a new day
                routine.MealsToday = 0;
                routine.HasWorshippedToday = false;
            }
        }
    }

    /// <summary>
    /// Helper system to apply routine-based modifiers to work and other activities.
    /// </summary>
    [BurstCompile]
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(DailyRoutineSystem))]
    public partial struct RoutineModifierSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<RoutineConfig>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var config = SystemAPI.GetSingleton<RoutineConfig>();

            // Apply sleep debt penalties
            foreach (var (routine, initiative) in SystemAPI.Query<
                RefRO<VillagerRoutine>,
                RefRW<VillagerInitiative>>())
            {
                var routineValue = routine.ValueRO;

                // Apply sleep debt penalty to initiative
                if (routineValue.SleepDebt > 0f)
                {
                    float penalty = routineValue.SleepDebt * config.SleepDebtWorkPenalty;
                    // The penalty would be factored into work speed calculations
                    // This is handled by systems that consume VillagerRoutine
                }
            }
        }

        /// <summary>
        /// Gets the work speed modifier based on routine state.
        /// </summary>
        public static float GetWorkSpeedModifier(in VillagerRoutine routine, in RoutineConfig config)
        {
            float modifier = 1f;

            // Sleep debt penalty
            modifier -= routine.SleepDebt * config.SleepDebtWorkPenalty;

            // Currently sleeping = no work
            if (routine.IsSleeping)
            {
                modifier = 0f;
            }

            return math.max(0f, modifier);
        }
    }
}

