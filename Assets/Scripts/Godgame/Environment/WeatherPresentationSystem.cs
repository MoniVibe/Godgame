using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Godgame.Environment
{
    /// <summary>
    /// Copies the DOTS weather singleton into the scene rig so COZY, VFXGraph, and ambient audio react instantly.
    /// </summary>
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    public partial class WeatherPresentationSystem : SystemBase
    {
        protected override void OnCreate()
        {
            RequireForUpdate<WeatherState>();
        }

        protected override void OnUpdate()
        {
            var rig = WeatherRigAuthoring.Instance;
            if (rig == null)
            {
                return;
            }

            var weatherEntity = SystemAPI.GetSingletonEntity<WeatherState>();
            var weatherState = EntityManager.GetComponentData<WeatherState>(weatherEntity);
            var forecast = EntityManager.GetComponentData<WeatherForecast>(weatherEntity);
            var overrideState = EntityManager.GetComponentData<WeatherOverrideState>(weatherEntity);
            var events = EntityManager.GetBuffer<WeatherEvent>(weatherEntity);

            var snapshot = new WeatherPresentationSnapshot(
                weatherState.Current,
                weatherState.Intensity,
                (TimeOfDayPhase)weatherState.ActivePhase,
                weatherState.TemperatureCelsius,
                weatherState.MoisturePercent,
                math.max(forecast.TransitionSeconds, overrideState.SecondsRemaining),
                overrideState.IsActive != 0 ? overrideState.Source : WeatherRequestSource.System,
                overrideState.Payload.ToString(),
                new Vector3(overrideState.LastRequestPosition.x, overrideState.LastRequestPosition.y, overrideState.LastRequestPosition.z));

            rig.ApplySnapshot(snapshot);

            if (events.Length > 0)
            {
                for (int i = 0; i < events.Length; i++)
                {
                    rig.HandleEvent(events[i]);
                }

                events.Clear();
            }
        }
    }
}
