using Unity.Entities;

namespace Godgame.Telemetry
{
    /// <summary>
    /// Ensures all Godgame telemetry exporters run together before the shared PureDOTS export writer.
    /// </summary>
    [UpdateInGroup(typeof(PureDOTS.Systems.PureDotsPresentationSystemGroup))]
    [UpdateBefore(typeof(PureDOTS.Systems.Telemetry.TelemetryExportSystem))]
    public partial class TelemetryExportSystemGroup : ComponentSystemGroup
    {
    }
}
