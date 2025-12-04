using Godgame.Environment;
using PureDOTS.Runtime.Components;
using PureDOTS.Systems;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace Godgame.Miracles
{
    /// <summary>
    /// Consumes miracle release events and triggers the corresponding miracle effects.
    /// </summary>
    [UpdateInGroup(typeof(HandSystemGroup))]
    [UpdateAfter(typeof(DivineHandSystem))]
    public partial struct GodgameMiracleReleaseSystem : ISystem
    {
        private EntityQuery _rainQueueQuery;

        public void OnCreate(ref SystemState state)
        {
            _rainQueueQuery = state.GetEntityQuery(ComponentType.ReadOnly<RainMiracleCommandQueue>());
            state.RequireForUpdate(_rainQueueQuery);
            state.RequireForUpdate<MiracleReleaseEvent>();
            state.RequireForUpdate<WeatherRequestQueueTag>();
        }

        public void OnUpdate(ref SystemState state)
        {
            if (_rainQueueQuery.IsEmptyIgnoreFilter)
            {
                return;
            }

            var rainQueueEntity = _rainQueueQuery.GetSingletonEntity();
            var rainCommands = state.EntityManager.GetBuffer<RainMiracleCommand>(rainQueueEntity);
            var weatherRequests = SystemAPI.GetSingletonBuffer<WeatherRequest>();

            foreach (var (events, casterState) in SystemAPI.Query<DynamicBuffer<MiracleReleaseEvent>, RefRO<MiracleCasterState>>())
            {
                if (events.Length == 0)
                {
                    continue;
                }

                for (int i = 0; i < events.Length; i++)
                {
                    var release = events[i];
                    switch (release.Type)
                    {
                        // TODO: Replace Rain with valid PureDOTS miracle type
                        // case MiracleType.Rain:
                        //     QueueRainMiracle(ref state, rainCommands, weatherRequests, in release);
                        //     break;
                        default:
                            // No miracle handler for this type yet
                            break;
                    }
                }

                events.Clear();
            }
        }

        private static void QueueRainMiracle(ref SystemState state, DynamicBuffer<RainMiracleCommand> commands, DynamicBuffer<WeatherRequest> weatherRequests, in MiracleReleaseEvent release)
        {
            if (release.ConfigEntity == Entity.Null)
            {
                return;
            }

            if (!state.EntityManager.HasComponent<RainMiracleConfig>(release.ConfigEntity))
            {
                return;
            }

            var config = state.EntityManager.GetComponentData<RainMiracleConfig>(release.ConfigEntity);
            commands.Add(new RainMiracleCommand
            {
                Center = release.Position,
                CloudCount = math.max(1, config.CloudCount),
                Radius = config.SpawnRadius,
                HeightOffset = config.SpawnHeightOffset,
                RainCloudPrefab = config.RainCloudPrefab,
                Seed = config.Seed
            });

            var requestedWeather = release.Impulse > 0.85f ? WeatherType.Storm : WeatherType.Rain;
            var payload = requestedWeather == WeatherType.Storm
                ? new FixedString64Bytes("miracle.storm")
                : new FixedString64Bytes("miracle.rain");

            weatherRequests.Add(new WeatherRequest
            {
                Source = WeatherRequestSource.Miracle,
                Weather = requestedWeather,
                Intensity = math.saturate(config.CloudCount / 4f + release.Impulse * 0.5f),
                DurationSeconds = math.max(6f, config.SpawnRadius * 0.35f),
                Position = release.Position,
                Payload = payload
            });
        }
    }
}
