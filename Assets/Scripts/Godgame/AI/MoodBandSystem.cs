using PureDOTS.Runtime.Components;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;

namespace Godgame.AI
{
    /// <summary>
    /// Classifies villager morale into mood bands and applies modifiers.
    /// Mood recalculates at Dawn and Midnight milestones, or when morale changes significantly.
    /// </summary>
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public partial struct VillagerMoodBandSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<TimeState>();

            // Create config singleton if it doesn't exist
            if (!SystemAPI.HasSingleton<MoodConfig>())
            {
                var configEntity = state.EntityManager.CreateEntity();
                state.EntityManager.AddComponentData(configEntity, MoodConfig.Default);
            }
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var timeState = SystemAPI.GetSingleton<TimeState>();
            if (timeState.IsPaused) return;

            var config = SystemAPI.GetSingleton<MoodConfig>();
            var currentTick = timeState.Tick;

            // Update mood bands for all villagers with needs
            foreach (var (mood, needs) in SystemAPI.Query<
                RefRW<VillagerMood>,
                RefRO<VillagerNeeds>>())
            {
                var moodValue = mood.ValueRW;
                UpdateMoodBand(ref moodValue, needs.ValueRO.Morale, config, currentTick);
                mood.ValueRW = moodValue;
            }

            // Process mood modifiers (tick down durations, apply decay)
            foreach (var (mood, modifiers) in SystemAPI.Query<
                RefRW<VillagerMood>,
                DynamicBuffer<MoodModifier>>())
            {
                var moodValue = mood.ValueRW;
                ProcessMoodModifiers(ref moodValue, modifiers);
                mood.ValueRW = moodValue;
            }

            // Process mood memories (decay over time)
            foreach (var memories in SystemAPI.Query<DynamicBuffer<MoodMemory>>())
            {
                var buffer = memories;
                ProcessMoodMemories(buffer, currentTick);
            }
        }

        private void UpdateMoodBand(
            ref VillagerMood mood,
            float morale,
            in MoodConfig config,
            uint currentTick)
        {
            // Classify morale into mood band
            MoodBand newBand;
            if (morale < config.DespairThreshold)
                newBand = MoodBand.Despair;
            else if (morale < config.UnhappyThreshold)
                newBand = MoodBand.Unhappy;
            else if (morale < config.CheerfulThreshold)
                newBand = MoodBand.Stable;
            else if (morale < config.ElatedThreshold)
                newBand = MoodBand.Cheerful;
            else
                newBand = MoodBand.Elated;

            // Only update if band changed
            if (newBand != mood.Band)
            {
                mood.Band = newBand;
                mood.RecalculateModifiers();
                mood.LastBandUpdateTick = currentTick;
            }
        }

        private void ProcessMoodModifiers(ref VillagerMood mood, DynamicBuffer<MoodModifier> modifiers)
        {
            // Process modifiers in reverse to safely remove expired ones
            for (int i = modifiers.Length - 1; i >= 0; i--)
            {
                var modifier = modifiers[i];

                // Tick down duration
                if (modifier.RemainingTicks > 0)
                {
                    modifier.RemainingTicks--;
                    modifiers[i] = modifier;

                    // Remove if expired
                    if (modifier.RemainingTicks == 0)
                    {
                        modifiers.RemoveAt(i);
                    }
                }
            }
        }

        private void ProcessMoodMemories(DynamicBuffer<MoodMemory> memories, uint currentTick)
        {
            for (int i = memories.Length - 1; i >= 0; i--)
            {
                var memory = memories[i];

                // Calculate decay based on half-life
                if (memory.DecayHalfLife > 0)
                {
                    uint ticksSinceFormed = currentTick - memory.FormedTick;
                    float decayFactor = math.pow(0.5f, (float)ticksSinceFormed / memory.DecayHalfLife);
                    memory.CurrentMagnitude = (sbyte)(memory.InitialMagnitude * decayFactor);
                    memories[i] = memory;

                    // Remove if magnitude is negligible
                    if (math.abs(memory.CurrentMagnitude) < 1)
                    {
                        memories.RemoveAt(i);
                    }
                }
            }
        }
    }
}

