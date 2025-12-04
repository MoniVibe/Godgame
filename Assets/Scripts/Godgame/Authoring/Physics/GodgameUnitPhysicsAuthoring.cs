using Godgame.Physics;
using PureDOTS.Runtime.Physics;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Godgame.Authoring
{
    /// <summary>
    /// Authoring component for Godgame units that need physics collision detection.
    /// Adds GodgamePhysicsBody, GodgameColliderData, and related components.
    /// </summary>
    /// <remarks>
    /// Philosophy:
    /// - ECS is authoritative; physics bodies are kinematic
    /// - Havok is used for collision detection and avoidance only
    /// - Ground units use pathfinding/steering, not physics forces
    /// - Flying units use ECS altitude control, Havok for collision detection
    /// </remarks>
    public class GodgameUnitPhysicsAuthoring : MonoBehaviour
    {
        [Header("Collider Settings")]
        [Tooltip("Type of collider to use")]
        public ColliderType colliderType = ColliderType.Capsule;

        [Tooltip("Radius for sphere/capsule colliders")]
        public float radius = 0.3f;

        [Tooltip("Size for box colliders (x, y, z)")]
        public Vector3 size = Vector3.one;

        [Tooltip("Height for capsule colliders")]
        public float height = 1.8f;

        [Tooltip("Center offset from transform")]
        public Vector3 centerOffset = new Vector3(0f, 0.9f, 0f);

        [Header("Physics Layer")]
        [Tooltip("Physics layer for collision filtering")]
        public GodgamePhysicsLayer layer = GodgamePhysicsLayer.GroundUnit;

        [Header("Behavior Flags")]
        [Tooltip("Entity generates collision events")]
        public bool raisesCollisionEvents = true;

        [Tooltip("Entity is a trigger (no physical response)")]
        public bool isTrigger = false;

        [Tooltip("Entity uses soft avoidance (pushback) instead of hard collision")]
        public bool softAvoidance = true;

        [Tooltip("Entity is a static collider (doesn't move)")]
        public bool isStatic = false;

        [Header("Ground Detection")]
        [Tooltip("Enable ground contact detection")]
        public bool detectGroundContact = true;

        [Header("Priority")]
        [Tooltip("Physics processing priority (0-255, higher = more important)")]
        [Range(0, 255)]
        public int priority = 100;

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(0.2f, 0.6f, 0.9f, 0.5f);
            var center = transform.position + centerOffset;

            switch (colliderType)
            {
                case ColliderType.Sphere:
                    Gizmos.DrawWireSphere(center, radius);
                    break;
                case ColliderType.Box:
                    Gizmos.DrawWireCube(center, size);
                    break;
                case ColliderType.Capsule:
                    // Draw capsule approximation
                    var halfHeight = height * 0.5f - radius;
                    Gizmos.DrawWireSphere(center + Vector3.up * halfHeight, radius);
                    Gizmos.DrawWireSphere(center - Vector3.up * halfHeight, radius);
                    // Draw connecting lines
                    Gizmos.DrawLine(center + Vector3.up * halfHeight + Vector3.right * radius,
                                   center - Vector3.up * halfHeight + Vector3.right * radius);
                    Gizmos.DrawLine(center + Vector3.up * halfHeight - Vector3.right * radius,
                                   center - Vector3.up * halfHeight - Vector3.right * radius);
                    Gizmos.DrawLine(center + Vector3.up * halfHeight + Vector3.forward * radius,
                                   center - Vector3.up * halfHeight + Vector3.forward * radius);
                    Gizmos.DrawLine(center + Vector3.up * halfHeight - Vector3.forward * radius,
                                   center - Vector3.up * halfHeight - Vector3.forward * radius);
                    break;
            }
        }
    }

    /// <summary>
    /// Baker for Godgame unit physics authoring.
    /// </summary>
    public class GodgameUnitPhysicsBaker : Baker<GodgameUnitPhysicsAuthoring>
    {
        public override void Bake(GodgameUnitPhysicsAuthoring authoring)
        {
            var entity = GetEntity(authoring, TransformUsageFlags.Dynamic);

            // Build physics flags
            var flags = GodgamePhysicsFlags.IsActive;
            if (authoring.raisesCollisionEvents)
                flags |= GodgamePhysicsFlags.RaisesCollisionEvents;
            if (authoring.isTrigger)
                flags |= GodgamePhysicsFlags.IsTrigger;
            if (authoring.softAvoidance)
                flags |= GodgamePhysicsFlags.SoftAvoidance;
            if (authoring.isStatic)
                flags |= GodgamePhysicsFlags.IsStatic;

            // Add GodgamePhysicsBody marker
            AddComponent(entity, new GodgamePhysicsBody
            {
                Layer = authoring.layer,
                Priority = (byte)authoring.priority,
                Flags = flags
            });

            // Add GodgameColliderData
            AddComponent(entity, new GodgameColliderData
            {
                Type = authoring.colliderType,
                Radius = authoring.radius,
                Size = new float3(authoring.size.x, authoring.size.y, authoring.size.z),
                Height = authoring.height,
                CenterOffset = new float3(authoring.centerOffset.x, authoring.centerOffset.y, authoring.centerOffset.z)
            });

            // Add ground contact component if enabled
            if (authoring.detectGroundContact)
            {
                AddComponent(entity, new GroundContact
                {
                    GroundNormal = new float3(0f, 1f, 0f),
                    GroundHeight = 0f,
                    DistanceToGround = 0f,
                    LastUpdateTick = 0,
                    SurfaceType = GroundSurfaceType.Default
                });
            }

            // Add avoidance push component for soft avoidance
            if (authoring.softAvoidance)
            {
                AddComponent(entity, new AvoidancePush
                {
                    Direction = float3.zero,
                    Strength = 0f,
                    LastUpdateTick = 0
                });
            }

            // Add RequiresPhysics from PureDOTS
            AddComponent(entity, new RequiresPhysics
            {
                Priority = (byte)authoring.priority,
                Flags = authoring.raisesCollisionEvents 
                    ? PhysicsInteractionFlags.Collidable 
                    : PhysicsInteractionFlags.None
            });

            // Add PhysicsInteractionConfig
            AddComponent(entity, new PhysicsInteractionConfig
            {
                Mass = 1f, // Kinematic, mass doesn't matter
                CollisionRadius = authoring.radius,
                Restitution = 0f,
                Friction = 0f,
                LinearDamping = 0f,
                AngularDamping = 0f
            });

            // Add collision event buffer if events are enabled
            if (authoring.raisesCollisionEvents)
            {
                AddBuffer<GodgameCollisionEvent>(entity);
                AddBuffer<PureDOTS.Runtime.Physics.PhysicsCollisionEventElement>(entity);
            }

            // Add NeedsPhysicsSetup tag for bootstrap system
            AddComponent(entity, new GodgameNeedsPhysicsSetup());
        }
    }
}

