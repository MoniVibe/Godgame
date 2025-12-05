using PureDOTS.Environment;
using Unity.Entities;
using Unity.Mathematics;

namespace Godgame.Climate
{
    /// <summary>
    /// God climate brush tool that creates/updates climate control sources.
    /// </summary>
    public struct GodClimateBrush : IComponentData
    {
        public ClimateVector TargetClimate;
        public float BrushRadius;
        public float BrushStrength;
        public byte IsActive;  // 0 = false, 1 = true
    }

    /// <summary>
    /// Command to create or update a climate control source from brush input.
    /// </summary>
    public struct ClimateBrushCommand : IBufferElementData
    {
        public float3 Position;
        public ClimateVector TargetClimate;
        public float Radius;
        public float Strength;
    }
}

