using System;
using Godgame.Demo;
using PureDOTS.Environment;
using PureDOTS.Runtime.Components;
using PureDOTS.Systems;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace Godgame.Environment
{
    /// <summary>
    /// Derives global weather state from climate, moisture grids, and queued requests.
    /// Emits weather events that presentation and audio stacks can consume.
    /// </summary>
    [BurstCompile]
    [UpdateInGroup(typeof(EnvironmentSystemGroup))]
    [UpdateAfter(typeof(PureDOTS.Systems.Environment.ClimateStateUpdateSystem))]
    public partial struct WeatherControllerSystem : ISystem
    {
        private EntityQuery _groundQuery;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<WeatherState>();
            state.RequireForUpdate<WeatherForecast>();
            state.RequireForUpdate<WeatherOverrideState>();
            state.RequireForUpdate<WeatherRequestQueueTag>();
            state.RequireForUpdate<ClimateState>();
            state.RequireForUpdate<TimeState>();

            using var builder = new EntityQueryBuilder(Allocator.Temp)
                .WithAll<GroundMoisture, GroundBiome>();
            _groundQuery = state.GetEntityQuery(builder);
        }

        public void OnUpdate(ref SystemState state)
        {
            var entityManager = state.EntityManager;
            var timeState = SystemAPI.GetSingleton<TimeState>();
            var climate = SystemAPI.GetSingleton<ClimateState>();
            var weatherEntity = SystemAPI.GetSingletonEntity<WeatherState>();

            var weatherState = entityManager.GetComponentData<WeatherState>(weatherEntity);
            var forecast = entityManager.GetComponentData<WeatherForecast>(weatherEntity);
            var overrideState = entityManager.GetComponentData<WeatherOverrideState>(weatherEntity);
            var events = entityManager.GetBuffer<WeatherEvent>(weatherEntity);
            var requests = SystemAPI.GetSingletonBuffer<WeatherRequest>();

            var stats = SampleGroundStats(ref state, _groundQuery);
            weatherState.MoisturePercent = stats.AverageMoisture;
            weatherState.DominantBiome = stats.DominantBiome;
            weatherState.DominantBiomeMoisture = stats.DominantBiomeMoisture;
            weatherState.TemperatureCelsius = climate.GlobalTemperature;

            var phase = WeatherControllerUtilities.ResolvePhase(climate.TimeOfDayHours);
            if (weatherState.ActivePhase != (byte)phase)
            {
                weatherState.ActivePhase = (byte)phase;
                WeatherControllerUtilities.AppendEvent(ref events, WeatherEventType.PhaseChanged, weatherState, overrideState, phase, timeState.Tick, default, WeatherRequestSource.TimeOfDay);
            }

            WeatherControllerUtilities.ProcessRequests(ref overrideState, requests, ref events, timeState.FixedDeltaTime, timeState.Tick);

            var target = WeatherControllerUtilities.ComputeTarget(in climate, stats, phase, ref weatherState);
            if (overrideState.IsActive != 0 && overrideState.SecondsRemaining > 0f)
            {
                target.Type = overrideState.Weather;
                target.Intensity = math.max(target.Intensity, overrideState.Intensity);
                target.TransitionSeconds = math.max(target.TransitionSeconds, math.max(overrideState.SecondsRemaining, 0.25f));
            }

            if (weatherState.Target != target.Type)
            {
                weatherState.Target = target.Type;
                forecast.Next = target.Type;
                forecast.TransitionSeconds = target.TransitionSeconds;
                forecast.TimeRemainingSeconds = target.TransitionSeconds;
            }

            if (weatherState.Current != weatherState.Target)
            {
                forecast.TimeRemainingSeconds = math.max(0f, forecast.TimeRemainingSeconds - timeState.FixedDeltaTime);
                if (forecast.TimeRemainingSeconds <= 0f)
                {
                    weatherState.Current = weatherState.Target;
                    weatherState.LastChangeTick = timeState.Tick;
                    WeatherControllerUtilities.AppendEvent(ref events, WeatherEventType.WeatherChanged, weatherState, overrideState, phase, timeState.Tick, default, WeatherRequestSource.System);
                }
            }

            var smoothing = math.saturate(timeState.FixedDeltaTime / math.max(0.0001f, target.TransitionSeconds));
            var blendedIntensity = math.lerp(weatherState.Intensity, target.Intensity, smoothing);
            if (math.abs(blendedIntensity - weatherState.Intensity) > 0.01f)
            {
                weatherState.Intensity = blendedIntensity;
                WeatherControllerUtilities.AppendEvent(ref events, WeatherEventType.WeatherIntensityChanged, weatherState, overrideState, phase, timeState.Tick, default, WeatherRequestSource.System);
            }

            entityManager.SetComponentData(weatherEntity, weatherState);
            entityManager.SetComponentData(weatherEntity, forecast);
            entityManager.SetComponentData(weatherEntity, overrideState);

            if (requests.Length > 0)
            {
                requests.Clear();
            }

            WeatherControllerUtilities.TrimEvents(ref events);
        }

        private GroundSamplingStats SampleGroundStats(ref SystemState state, EntityQuery query)
        {
            var stats = new GroundSamplingStats
            {
                AverageMoisture = 0f,
                DominantBiome = BiomeType.Unknown,
                DominantBiomeMoisture = 0f
            };

            var total = query.IsEmptyIgnoreFilter ? 0 : query.CalculateEntityCount();
            if (total == 0)
            {
                return stats;
            }

            var sampleBudget = math.min(total, 256);
            var stride = math.max(1, total / sampleBudget);

            var biomeCounts = new NativeArray<int>(9, Allocator.Temp);
            var biomeMoisture = new NativeArray<float>(9, Allocator.Temp);

            int index = 0;
            int sampled = 0;
            double sum = 0d;

            try
            {
                foreach (var (moisture, biome) in SystemAPI.Query<RefRO<GroundMoisture>, RefRO<GroundBiome>>())
                {
                    if ((index++ % stride) != 0)
                    {
                        continue;
                    }

                    sum += moisture.ValueRO.Value;
                    sampled++;

                    var biomeIndex = math.clamp((int)biome.ValueRO.Value, 0, 8);
                    biomeCounts[biomeIndex] = biomeCounts[biomeIndex] + 1;
                    biomeMoisture[biomeIndex] = biomeMoisture[biomeIndex] + moisture.ValueRO.Value;

                    if (sampled >= sampleBudget)
                    {
                        break;
                    }
                }

                if (sampled == 0)
                {
                    return stats;
                }

                stats.AverageMoisture = (float)(sum / sampled);

                int dominantIndex = 0;
                int dominantCount = 0;
                for (int i = 0; i < biomeCounts.Length; i++)
                {
                    var count = biomeCounts[i];
                    if (count > dominantCount)
                    {
                        dominantCount = count;
                        dominantIndex = i;
                    }
                }

                stats.DominantBiome = (BiomeType)dominantIndex;
                stats.DominantBiomeMoisture = dominantCount > 0 ? biomeMoisture[dominantIndex] / dominantCount : stats.AverageMoisture;
                return stats;
            }
            finally
            {
                biomeCounts.Dispose();
                biomeMoisture.Dispose();
            }
        }

    }

    internal struct GroundSamplingStats
    {
        public float AverageMoisture;
        public BiomeType DominantBiome;
        public float DominantBiomeMoisture;
    }

    internal static class WeatherControllerUtilities
    {
        private const int MaxWeatherEvents = 32;

        public static void ProcessRequests(ref WeatherOverrideState overrideState, DynamicBuffer<WeatherRequest> requests, ref DynamicBuffer<WeatherEvent> events, float deltaSeconds, uint currentTick)
        {
            if (overrideState.IsActive != 0 && overrideState.SecondsRemaining > 0f)
            {
                overrideState.SecondsRemaining = math.max(0f, overrideState.SecondsRemaining - deltaSeconds);
                if (overrideState.SecondsRemaining <= 0f)
                {
                    overrideState.IsActive = 0;
                    overrideState.Payload = default;
                }
            }

            if (requests.Length == 0)
            {
                return;
            }

            int bestPriority = -1;
            WeatherRequest best = default;
            bool hasBest = false;

            for (int i = 0; i < requests.Length; i++)
            {
                var request = requests[i];
                var priority = PriorityOf(request.Source);
                if (!hasBest || priority > bestPriority || (priority == bestPriority && request.Intensity > best.Intensity))
                {
                    bestPriority = priority;
                    best = request;
                    hasBest = true;
                }

                if (request.Source != WeatherRequestSource.System)
                {
                    AppendSpecialFxEvent(ref events, request, currentTick);
                }
            }

            if (hasBest)
            {
                overrideState.IsActive = 1;
                overrideState.Source = best.Source;
                overrideState.Weather = best.Weather;
                overrideState.Intensity = math.clamp(best.Intensity, 0f, 2f);
                overrideState.SecondsRemaining = math.max(best.DurationSeconds, 0.5f);
                overrideState.LastRequestPosition = best.Position;
                overrideState.Payload = best.Payload;
            }
        }

        public static void AppendEvent(ref DynamicBuffer<WeatherEvent> events, WeatherEventType type, in WeatherState weatherState, in WeatherOverrideState overrideState, TimeOfDayPhase phase, uint tick, float3 position, WeatherRequestSource source)
        {
            var eventSource = overrideState.IsActive != 0 ? overrideState.Source : source;
            var evt = new WeatherEvent
            {
                EventType = type,
                Weather = weatherState.Current,
                Phase = phase,
                Source = eventSource,
                Intensity = weatherState.Intensity,
                Tick = tick,
                Position = position,
                Payload = overrideState.Payload
            };

            events.Add(evt);
            TrimEvents(ref events);
        }

        public static void AppendSpecialFxEvent(ref DynamicBuffer<WeatherEvent> events, in WeatherRequest request, uint tick)
        {
            var fx = new WeatherEvent
            {
                EventType = WeatherEventType.SpecialFxTriggered,
                Weather = request.Weather,
                Phase = TimeOfDayPhase.Morning,
                Source = request.Source,
                Intensity = math.max(0.1f, request.Intensity),
                Tick = tick,
                Position = request.Position,
                Payload = request.Payload
            };

            events.Add(fx);
            TrimEvents(ref events);
        }

        public static void TrimEvents(ref DynamicBuffer<WeatherEvent> events)
        {
            while (events.Length > MaxWeatherEvents)
            {
                events.RemoveAt(0);
            }
        }

        public static TimeOfDayPhase ResolvePhase(float hours)
        {
            var wrapped = math.frac(hours / 24f) * 24f;
            if (wrapped >= 5f && wrapped < 8f)
            {
                return TimeOfDayPhase.Dawn;
            }

            if (wrapped >= 8f && wrapped < 12.5f)
            {
                return TimeOfDayPhase.Morning;
            }

            if (wrapped >= 12.5f && wrapped < 17.5f)
            {
                return TimeOfDayPhase.Afternoon;
            }

            if (wrapped >= 17.5f && wrapped < 21f)
            {
                return TimeOfDayPhase.Dusk;
            }

            return TimeOfDayPhase.Night;
        }

        public static WeatherTarget ComputeTarget(in ClimateState climate, in GroundSamplingStats stats, TimeOfDayPhase phase, ref WeatherState current)
        {
            WeatherTarget best = new()
            {
                Type = current.Current,
                Intensity = math.clamp(current.Intensity, 0f, 1.5f),
                TransitionSeconds = math.max(6f, current.LastChangeTick == 0 ? 6f : 8f)
            };

            float humidity = math.clamp(climate.AtmosphericMoisture, 0f, 100f);
            float cloudCover = math.clamp(climate.CloudCover, 0f, 100f);
            float wind = math.max(0f, climate.GlobalWindStrength);
            float avgMoisture = math.clamp(stats.AverageMoisture, 0f, 100f);
            float dryness = 1f - avgMoisture / 100f;

            float baseScore = 0.1f;
            TryPromote(ref best, WeatherType.Clear, baseScore + dryness, math.max(0.25f, 1f - dryness * 0.75f), 8f);

            var rainScore = humidity / 100f + (1f - dryness) * 0.35f + CloudCoverBias(cloudCover);
            rainScore += BiomeMoistureBias(stats.DominantBiome);
            var rainIntensity = math.saturate((humidity - 55f) / 45f);
            TryPromote(ref best, WeatherType.Rain, rainScore, rainIntensity, 10f);

            var fogScore = humidity / 120f + (phase is TimeOfDayPhase.Dawn or TimeOfDayPhase.Night ? 0.3f : 0f);
            fogScore -= dryness * 0.4f;
            var fogIntensity = math.saturate((humidity - 35f) / 30f);
            TryPromote(ref best, WeatherType.Fog, fogScore, fogIntensity, 6f);

            if (climate.GlobalTemperature <= 1.5f)
            {
                var snowScore = rainScore + (1.5f - climate.GlobalTemperature) * 0.1f;
                var snowIntensity = math.saturate(avgMoisture / 60f);
                TryPromote(ref best, WeatherType.Snow, snowScore, snowIntensity, 12f);
            }

            var stormScore = rainScore + (wind / 20f);
            var stormIntensity = math.saturate((wind - 8f) / 12f);
            TryPromote(ref best, WeatherType.Storm, stormScore, stormIntensity, 7f);

            best.Intensity = math.clamp(best.Intensity, 0.05f, 1.5f);
            return best;
        }

        private static float CloudCoverBias(float cover)
        {
            return math.clamp(cover / 100f, 0f, 1f) * 0.35f;
        }

        private static float BiomeMoistureBias(BiomeType biome)
        {
            return biome switch
            {
                BiomeType.Desert => -0.5f,
                BiomeType.Savanna => -0.2f,
                BiomeType.Rainforest => 0.5f,
                BiomeType.Forest => 0.2f,
                BiomeType.Swamp => 0.35f,
                _ => 0f
            };
        }

        private static void TryPromote(ref WeatherTarget best, WeatherType candidate, float score, float intensity, float transition)
        {
            if (score <= best.Score)
            {
                return;
            }

            best = new WeatherTarget
            {
                Type = candidate,
                Intensity = math.clamp(intensity, 0f, 1.5f),
                TransitionSeconds = math.max(transition, 4f),
                Score = score
            };
        }

        private static int PriorityOf(WeatherRequestSource source)
        {
            return source switch
            {
                WeatherRequestSource.Miracle => 4,
                WeatherRequestSource.Script => 3,
                WeatherRequestSource.Biome => 2,
                WeatherRequestSource.TimeOfDay => 1,
                _ => 0
            };
        }
    }

    internal struct WeatherTarget
    {
        public WeatherType Type;
        public float Intensity;
        public float TransitionSeconds;
        public float Score;
    }
}
