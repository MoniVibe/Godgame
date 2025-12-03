using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace Godgame.Construction
{
    /// <summary>
    /// Construction ghost component tracking resource costs and payment progress.
    /// Extends JobsiteGhost with cost tracking.
    /// </summary>
    public struct ConstructionGhost : IComponentData
    {
        public ushort ResourceTypeIndex;
        public int Cost;
        public int Paid;
    }

    /// <summary>
    /// Extended placement request with resource cost information.
    /// </summary>
    public struct PlaceConstructionRequest : IBufferElementData
    {
        public float3 Position;
        public ushort ResourceTypeIndex;
        public int Cost;
    }
}

