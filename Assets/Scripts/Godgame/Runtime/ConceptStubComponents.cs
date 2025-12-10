using Unity.Entities;

namespace Godgame.Runtime
{
    // STUB: placeholder components to wire scenes ahead of full implementations.

    public struct MiracleRequest : IComponentData
    {
        public int RequestId;
        public Entity Target;
    }

    public struct VillagerNeed : IComponentData
    {
        public byte NeedType;
        public byte Urgency;
    }

    public struct NeedSatisfaction : IComponentData
    {
        public byte NeedType;
        public float Value;
    }

    public struct BandCohesionState : IComponentData
    {
        public byte Cohesion; // 0-100
    }

    public struct VillageBoundary : IComponentData
    {
        public float Radius;
    }

    public struct ConstructionPhase : IComponentData
    {
        public byte Phase; // planned/foundation/build/finish
    }

    public struct AlertState : IComponentData
    {
        public byte Level; // none/low/high
    }

    public struct GrudgeEntry : IBufferElementData
    {
        public Entity Target;
        public byte Severity;
    }

    public struct LanguageAffinity : IComponentData
    {
        public byte Affinity; // 0-100
    }

    public struct HandInputIntent : IComponentData
    {
        public byte Intent; // idle/drag/throw/miracle
    }
}
