using PureDOTS.Runtime.Components;
using PureDOTS.Systems;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace Godgame.Time
{
    /// <summary>
    /// HUD event buffer element for time state changes.
    /// </summary>
    public struct HudEvent : IBufferElementData
    {
        public FixedString32Bytes Key;
        public float Value;
    }

    /// <summary>
    /// System that reads TimeControlInput and mutates TimeState/RewindState, emitting HUD events.
    /// </summary>
    [BurstCompile]
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    // Removed invalid UpdateAfter: TimeDemoBootstrapSystem runs OrderLast, so ordering must stay at group level.
    public partial struct TimeControlSystem : ISystem
    {
        private static readonly float[] SpeedTiers = { 0f, 0.25f, 0.5f, 1f, 2f, 4f, 8f };

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<TimeState>();
            state.RequireForUpdate<RewindState>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            if (!SystemAPI.TryGetSingleton<TimeControlInput>(out var input))
            {
                return;
            }

            var timeState = SystemAPI.GetSingletonRW<TimeState>();
            var rewindState = SystemAPI.GetSingletonRW<RewindState>();

            // Get HUD event buffer (assumes entity exists from bootstrap)
            if (!SystemAPI.TryGetSingletonBuffer<HudEvent>(out var hudBuffer))
            {
                // Skip HUD events if buffer doesn't exist (headless mode)
                hudBuffer = default;
            }

            // Handle pause toggle
            if (input.TogglePause != 0)
            {
                if (timeState.ValueRO.IsPaused)
                {
                    // Resume at last speed (or 1x if was 0)
                    var lastSpeed = math.max(1f, timeState.ValueRO.CurrentSpeedMultiplier);
                    timeState.ValueRW.IsPaused = false;
                    timeState.ValueRW.CurrentSpeedMultiplier = lastSpeed;
                    if (hudBuffer.IsCreated)
                        EmitHudEvent(ref hudBuffer, "time.paused", 0f);
                }
                else
                {
                    timeState.ValueRW.IsPaused = true;
                    if (hudBuffer.IsCreated)
                        EmitHudEvent(ref hudBuffer, "time.paused", 1f);
                }
            }

            // Handle step (single tick back)
            if (input.Step != 0 && timeState.ValueRO.IsPaused)
            {
                // Step back one tick
                if (rewindState.ValueRO.Mode == RewindMode.Record && timeState.ValueRO.Tick > 0)
                {
                    timeState.ValueRW.Tick = math.max(0u, timeState.ValueRO.Tick - 1);
                    if (hudBuffer.IsCreated)
                        EmitHudEvent(ref hudBuffer, "time.tick", timeState.ValueRO.Tick);
                }
            }

            // Handle speed delta
            if (math.abs(input.SpeedDelta) > 0.001f && !timeState.ValueRO.IsPaused)
            {
                var currentSpeed = timeState.ValueRO.CurrentSpeedMultiplier;
                var newSpeed = ChangeSpeedTier(currentSpeed, input.SpeedDelta > 0f);
                timeState.ValueRW.CurrentSpeedMultiplier = newSpeed;
                if (hudBuffer.IsCreated)
                    EmitHudEvent(ref hudBuffer, "time.speed", newSpeed);
            }

            // Handle rewind hold
            if (input.RewindHold != 0)
            {
                // Enter rewind mode
                if (rewindState.ValueRO.Mode == RewindMode.Record)
                {
                    rewindState.ValueRW.Mode = RewindMode.Playback;
                    // Simplified: set playback tick to current - some offset
                    rewindState.ValueRW.PlaybackTick = timeState.ValueRO.Tick > 60u
                        ? timeState.ValueRO.Tick - 60u
                        : 0u;
                    if (hudBuffer.IsCreated)
                        EmitHudEvent(ref hudBuffer, "time.rewind", 1f);
                }
            }
            else if (rewindState.ValueRO.Mode == RewindMode.Playback)
            {
                // Exit rewind mode
                rewindState.ValueRW.Mode = RewindMode.Record;
                if (hudBuffer.IsCreated)
                    EmitHudEvent(ref hudBuffer, "time.rewind", 0f);
            }

            // Clear input after processing
            if (SystemAPI.TryGetSingletonRW<TimeControlInput>(out var inputRW))
            {
                inputRW.ValueRW = default;
            }
        }

        private static float ChangeSpeedTier(float currentSpeed, bool increase)
        {
            int currentIndex = FindClosestTierIndex(currentSpeed);
            if (increase)
            {
                currentIndex = math.min(currentIndex + 1, SpeedTiers.Length - 1);
            }
            else
            {
                currentIndex = math.max(currentIndex - 1, 0);
            }

            return SpeedTiers[currentIndex];
        }

        private static int FindClosestTierIndex(float speed)
        {
            int closest = 0;
            float minDist = math.abs(SpeedTiers[0] - speed);
            for (int i = 1; i < SpeedTiers.Length; i++)
            {
                var dist = math.abs(SpeedTiers[i] - speed);
                if (dist < minDist)
                {
                    minDist = dist;
                    closest = i;
                }
            }

            return closest;
        }

        private static void EmitHudEvent(ref DynamicBuffer<HudEvent> buffer, in FixedString32Bytes key, float value)
        {
            // Upsert: update if exists, add if not
            for (int i = 0; i < buffer.Length; i++)
            {
                if (buffer[i].Key.Equals(key))
                {
                    buffer[i] = new HudEvent { Key = key, Value = value };
                    return;
                }
            }

            buffer.Add(new HudEvent { Key = key, Value = value });
        }
    }
}

