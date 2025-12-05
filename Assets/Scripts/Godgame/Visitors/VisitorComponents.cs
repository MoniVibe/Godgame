using Unity.Entities;

namespace Godgame.Visitors
{
    /// <summary>
    /// Kind of visitor object (comet, asteroid, etc.).
    /// </summary>
    public enum VisitorKind : byte
    {
        Comet,
        Asteroid,
        IceChunk,
        Derelict,
        StrangeSatellite
    }

    /// <summary>
    /// Tag component marking an entity as a visitor object.
    /// </summary>
    public struct VisitorTag : IComponentData { }

    /// <summary>
    /// State for visitor objects (comets, asteroids, etc.).
    /// </summary>
    public struct VisitorState : IComponentData
    {
        public VisitorKind Kind;
        
        /// <summary>True when visitor is near player's world and can be picked up.</summary>
        public bool IsPickable;
        
        /// <summary>True when visitor is currently held by god hand.</summary>
        public bool IsHeld;
        
        /// <summary>True when visitor is on impact trajectory.</summary>
        public bool IsInbound;
    }
}

