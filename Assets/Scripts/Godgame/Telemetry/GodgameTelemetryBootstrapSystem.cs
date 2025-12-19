using PureDOTS.Runtime.Telemetry;
using Unity.Entities;

namespace Godgame.Telemetry
{
    /// <summary>
    /// Ensures the dedicated telemetry event stream entity exists before any telemetry systems run.
    /// </summary>
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    public partial struct GodgameTelemetryBootstrapSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<TelemetryStream>();
        }

        public void OnUpdate(ref SystemState state)
        {
            TelemetryStreamUtility.EnsureEventStream(state.EntityManager);
            state.Enabled = false;
        }
    }
}
