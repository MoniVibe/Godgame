using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace Godgame.Villages
{
    /// <summary>
    /// Presentation data for village visualization.
    /// Maps village state to visual representation.
    /// </summary>
    public struct VillagePresentation : IComponentData
    {
        public FixedString64Bytes EffectId;      // Effect ID for village visuals
        public float3 VisualPosition;            // Position for visual effects
        public float VisualRadius;               // Radius for influence ring visualization
        public byte VisualIntensity;             // 0-100, intensity of visual effects
        public uint LastVisualUpdateTick;        // When visuals were last updated
    }

    /// <summary>
    /// Presentation data for band visualization.
    /// </summary>
    public struct BandPresentation : IComponentData
    {
        public FixedString64Bytes EffectId;      // Effect ID for band visuals
        public float3 VisualPosition;            // Position for visual effects
        public float FormationRadius;             // Radius for formation visualization
        public byte VisualIntensity;             // 0-100, intensity of visual effects
        public uint LastVisualUpdateTick;        // When visuals were last updated
    }
}

