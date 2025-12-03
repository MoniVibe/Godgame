using Unity.Entities;
using Unity.Mathematics;

namespace Godgame.MoveAct
{
    /// <summary>
    /// Configuration for spawning minimal Band entities without authoring data.
    /// </summary>
    public struct BandSpawnConfig : IComponentData
    {
        public float3 StartPosition;
        public float3 PositionStep;
        public int DefaultFactionId;
        public int DefaultMemberCount;
        public float DefaultMorale;
        public float DefaultCohesion;
        public float DefaultDiscipline;
        public float DefaultSpacing;
    }

    /// <summary>
    /// Tracks deterministic placement offsets and band identifiers.
    /// </summary>
    public struct BandSpawnState : IComponentData
    {
        public int NextBandId;
        public float3 NextPosition;
    }

    /// <summary>
    /// Buffered requests for spawning bands.
    /// </summary>
    public struct BandSpawnRequest : IBufferElementData
    {
        public float3 Position;
    }

    /// <summary>
    /// Current Move & Act selection.
    /// </summary>
    public struct BandSelection : IComponentData
    {
        public Entity Selected;
        public float3 Position;
        public uint SelectionTick;
    }

    /// <summary>
    /// Hotkey snapshot for triggering actions (e.g., ping effect).
    /// </summary>
    public struct BandActionHotkeyState : IComponentData
    {
        public byte PlayPingRequested;
    }
}
