using Unity.Entities;

namespace Godgame.Environment
{
    /// <summary>
    /// Ensures the weather singleton, request queue, and buffer entities exist before runtime systems run.
    /// </summary>
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    public partial struct WeatherBootstrapSystem : ISystem
    {
        private EntityQuery _stateQuery;
        private EntityQuery _queueQuery;

        public void OnCreate(ref SystemState state)
        {
            _stateQuery = state.GetEntityQuery(ComponentType.ReadOnly<WeatherState>());
            _queueQuery = state.GetEntityQuery(ComponentType.ReadOnly<WeatherRequestQueueTag>());

            EnsureWeatherSingleton(ref state);
            EnsureRequestQueue(ref state);
        }

        public void OnUpdate(ref SystemState state)
        {
            state.Enabled = false;
        }

        private void EnsureWeatherSingleton(ref SystemState state)
        {
            if (!_stateQuery.IsEmptyIgnoreFilter)
            {
                return;
            }

            var entity = state.EntityManager.CreateEntity();
            state.EntityManager.AddComponentData(entity, new WeatherState
            {
                Current = WeatherType.Clear,
                Target = WeatherType.Clear,
                Intensity = 0f,
                MoisturePercent = 0f,
                TemperatureCelsius = 20f,
                ActivePhase = (byte)TimeOfDayPhase.Morning,
                DominantBiome = PureDOTS.Environment.BiomeType.Unknown,
                DominantBiomeMoisture = 0f,
                LastChangeTick = 0u
            });

            state.EntityManager.AddComponentData(entity, new WeatherForecast
            {
                Next = WeatherType.Clear,
                TransitionSeconds = 8f,
                TimeRemainingSeconds = 0f
            });

            state.EntityManager.AddComponentData(entity, new WeatherOverrideState
            {
                IsActive = 0,
                Source = WeatherRequestSource.System,
                Weather = WeatherType.Clear,
                Intensity = 0f,
                SecondsRemaining = 0f,
                LastRequestPosition = default,
                Payload = default
            });

            state.EntityManager.AddBuffer<WeatherEvent>(entity);
        }

        private void EnsureRequestQueue(ref SystemState state)
        {
            if (!_queueQuery.IsEmptyIgnoreFilter)
            {
                return;
            }

            var queueEntity = state.EntityManager.CreateEntity();
            state.EntityManager.AddComponent<WeatherRequestQueueTag>(queueEntity);
            state.EntityManager.AddBuffer<WeatherRequest>(queueEntity);
        }
    }
}
