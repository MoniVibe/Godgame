using Unity.Entities;
using Unity.Mathematics;
using PureDOTS.Runtime.Physics;

namespace Godgame.Physics
{
    /// <summary>
    /// Marker component for Godgame entities that participate in physics simulation.
    /// Entities with this component will have their ECS transforms synced to Havok bodies.
    /// </summary>
    /// <remarks>
    /// Philosophy:
    /// - ECS is authoritative for positions, velocities, and gameplay state
    /// - Havok is used for collision detection and avoidance only
    /// - All physics bodies are kinematic (driven by pathfinding/steering systems)
    /// - Ground units use Havok for building/obstacle avoidance
    /// - Flying units use Havok for terrain/building collision detection
    /// </remarks>
    public struct GodgamePhysicsBody : IComponentData
    {
        /// <summary>
        /// Physics layer for collision filtering.
        /// </summary>
        public GodgamePhysicsLayer Layer;

        /// <summary>
        /// Priority for physics processing (0-255, higher = more important).
        /// </summary>
        public byte Priority;

        /// <summary>
        /// Flags for physics behavior.
        /// </summary>
        public GodgamePhysicsFlags Flags;
    }

    /// <summary>
    /// Flags for Godgame physics behavior.
    /// </summary>
    [System.Flags]
    public enum GodgamePhysicsFlags : byte
    {
        None = 0,

        /// <summary>
        /// Entity generates collision events.
        /// </summary>
        RaisesCollisionEvents = 1 << 0,

        /// <summary>
        /// Entity is a trigger (no physical response, just events).
        /// </summary>
        IsTrigger = 1 << 1,

        /// <summary>
        /// Entity should be pushed away on collision (soft avoidance).
        /// </summary>
        SoftAvoidance = 1 << 2,

        /// <summary>
        /// Entity is currently active in physics simulation.
        /// </summary>
        IsActive = 1 << 3,

        /// <summary>
        /// Entity was recently involved in a collision.
        /// </summary>
        HasRecentCollision = 1 << 4,

        /// <summary>
        /// Entity is grounded (touching terrain).
        /// </summary>
        IsGrounded = 1 << 5,

        /// <summary>
        /// Entity is a static collider (buildings, terrain).
        /// </summary>
        IsStatic = 1 << 6
    }

    /// <summary>
    /// Collider configuration for Godgame entities.
    /// </summary>
    public struct GodgameColliderData : IComponentData
    {
        /// <summary>
        /// Radius for sphere/capsule colliders.
        /// </summary>
        public float Radius;

        /// <summary>
        /// Size for box colliders (x, y, z dimensions).
        /// </summary>
        public float3 Size;

        /// <summary>
        /// Height for capsule colliders.
        /// </summary>
        public float Height;

        /// <summary>
        /// Collider type.
        /// </summary>
        public ColliderType Type;

        /// <summary>
        /// Center offset from entity transform.
        /// </summary>
        public float3 CenterOffset;

        /// <summary>
        /// Creates a capsule collider for a villager/unit.
        /// </summary>
        public static GodgameColliderData CreateVillagerCapsule(float radius = 0.3f, float height = 1.8f)
        {
            return new GodgameColliderData
            {
                Type = ColliderType.Capsule,
                Radius = radius,
                Height = height,
                CenterOffset = new float3(0f, height * 0.5f, 0f)
            };
        }

        /// <summary>
        /// Creates a box collider for a building.
        /// </summary>
        public static GodgameColliderData CreateBuildingBox(float3 size)
        {
            return new GodgameColliderData
            {
                Type = ColliderType.Box,
                Size = size,
                CenterOffset = new float3(0f, size.y * 0.5f, 0f)
            };
        }

        /// <summary>
        /// Creates a sphere collider for a creature.
        /// </summary>
        public static GodgameColliderData CreateCreatureSphere(float radius)
        {
            return new GodgameColliderData
            {
                Type = ColliderType.Sphere,
                Radius = radius,
                CenterOffset = new float3(0f, radius, 0f)
            };
        }
    }

    /// <summary>
    /// Ground contact information for grounded entities.
    /// </summary>
    public struct GroundContact : IComponentData
    {
        /// <summary>
        /// Ground normal at contact point.
        /// </summary>
        public float3 GroundNormal;

        /// <summary>
        /// Ground height at entity position.
        /// </summary>
        public float GroundHeight;

        /// <summary>
        /// Distance from entity to ground.
        /// </summary>
        public float DistanceToGround;

        /// <summary>
        /// Tick when ground contact was last updated.
        /// </summary>
        public uint LastUpdateTick;

        /// <summary>
        /// Ground surface type (for different movement speeds, sounds, etc.).
        /// </summary>
        public GroundSurfaceType SurfaceType;
    }

    /// <summary>
    /// Ground surface types for movement/sound effects.
    /// </summary>
    public enum GroundSurfaceType : byte
    {
        Default = 0,
        Grass = 1,
        Dirt = 2,
        Stone = 3,
        Wood = 4,
        Water = 5,
        Sand = 6,
        Snow = 7
    }

    /// <summary>
    /// Collision event buffer element for Godgame entities.
    /// Populated by PhysicsEventSystem after physics step.
    /// </summary>
    public struct GodgameCollisionEvent : IBufferElementData
    {
        /// <summary>
        /// The other entity involved in the collision.
        /// </summary>
        public Entity OtherEntity;

        /// <summary>
        /// Contact point in world space.
        /// </summary>
        public float3 ContactPoint;

        /// <summary>
        /// Contact normal (pointing away from this entity).
        /// </summary>
        public float3 ContactNormal;

        /// <summary>
        /// Tick when collision occurred.
        /// </summary>
        public uint Tick;

        /// <summary>
        /// Type of collision.
        /// </summary>
        public GodgameCollisionType CollisionType;

        /// <summary>
        /// Layer of the other entity.
        /// </summary>
        public GodgamePhysicsLayer OtherLayer;
    }

    /// <summary>
    /// Type of Godgame collision.
    /// </summary>
    public enum GodgameCollisionType : byte
    {
        /// <summary>
        /// Unit-to-unit collision (avoidance).
        /// </summary>
        UnitToUnit = 0,

        /// <summary>
        /// Unit-to-building collision (obstacle).
        /// </summary>
        UnitToBuilding = 1,

        /// <summary>
        /// Unit-to-terrain collision (grounding/clipping).
        /// </summary>
        UnitToTerrain = 2,

        /// <summary>
        /// Projectile impact.
        /// </summary>
        ProjectileImpact = 3,

        /// <summary>
        /// Trigger zone entry.
        /// </summary>
        TriggerEnter = 4,

        /// <summary>
        /// Trigger zone exit.
        /// </summary>
        TriggerExit = 5
    }

    /// <summary>
    /// Tag component for entities that need physics initialization.
    /// </summary>
    public struct GodgameNeedsPhysicsSetup : IComponentData { }

    /// <summary>
    /// Tag component for entities with active physics colliders.
    /// </summary>
    public struct GodgameHasPhysicsCollider : IComponentData { }

    /// <summary>
    /// Avoidance push component for soft collision response.
    /// Accumulated by collision system and applied by movement system.
    /// </summary>
    public struct AvoidancePush : IComponentData
    {
        /// <summary>
        /// Accumulated push direction (normalized).
        /// </summary>
        public float3 Direction;

        /// <summary>
        /// Push strength (0-1).
        /// </summary>
        public float Strength;

        /// <summary>
        /// Tick when push was last updated.
        /// </summary>
        public uint LastUpdateTick;
    }
}

