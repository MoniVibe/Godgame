using Unity.Burst;
using Unity.Entities;

namespace Godgame.Adapters.Launch
{
    /// <summary>
    /// Tag marking Godgame slingshot entities.
    /// </summary>
    [BurstCompile]
    public struct GodgameSlingshotTag : IComponentData { }

    /// <summary>
    /// Godgame-specific slingshot configuration.
    /// </summary>
    [BurstCompile]
    public struct GodgameSlingshotConfig : IComponentData
    {
        public float MaxRange;
        public float ArcHeightMultiplier;
    }
}

