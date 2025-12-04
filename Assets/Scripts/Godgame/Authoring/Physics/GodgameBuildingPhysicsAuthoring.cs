using Godgame.Physics;
using PureDOTS.Runtime.Physics;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Godgame.Authoring
{
    /// <summary>
    /// Authoring component for Godgame buildings that need physics collision detection.
    /// Buildings are static colliders that units avoid.
    /// </summary>
    public class GodgameBuildingPhysicsAuthoring : MonoBehaviour
    {
        [Header("Collider Settings")]
        [Tooltip("Type of collider to use")]
        public ColliderType colliderType = ColliderType.Box;

        [Tooltip("Radius for sphere colliders")]
        public float radius = 1f;

        [Tooltip("Size for box colliders (x, y, z)")]
        public Vector3 size = new Vector3(4f, 3f, 4f);

        [Tooltip("Center offset from transform")]
        public Vector3 centerOffset = new Vector3(0f, 1.5f, 0f);

        [Header("Behavior Flags")]
        [Tooltip("Entity generates collision events")]
        public bool raisesCollisionEvents = true;

        [Header("Priority")]
        [Tooltip("Physics processing priority (0-255)")]
        [Range(0, 255)]
        public int priority = 100;

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(0.8f, 0.4f, 0.2f, 0.5f);
            var center = transform.position + centerOffset;

            switch (colliderType)
            {
                case ColliderType.Sphere:
                    Gizmos.DrawWireSphere(center, radius);
                    break;
                case ColliderType.Box:
                    Gizmos.DrawWireCube(center, size);
                    break;
            }
        }
    }

    /// <summary>
    /// Baker for Godgame building physics authoring.
    /// </summary>
    public class GodgameBuildingPhysicsBaker : Baker<GodgameBuildingPhysicsAuthoring>
    {
        public override void Bake(GodgameBuildingPhysicsAuthoring authoring)
        {
            var entity = GetEntity(authoring, TransformUsageFlags.Dynamic);

            // Build physics flags - buildings are static
            var flags = GodgamePhysicsFlags.IsActive | GodgamePhysicsFlags.IsStatic;
            if (authoring.raisesCollisionEvents)
                flags |= GodgamePhysicsFlags.RaisesCollisionEvents;

            // Add GodgamePhysicsBody marker
            AddComponent(entity, new GodgamePhysicsBody
            {
                Layer = GodgamePhysicsLayer.Building,
                Priority = (byte)authoring.priority,
                Flags = flags
            });

            // Add GodgameColliderData
            AddComponent(entity, new GodgameColliderData
            {
                Type = authoring.colliderType,
                Radius = authoring.radius,
                Size = new float3(authoring.size.x, authoring.size.y, authoring.size.z),
                Height = 0f,
                CenterOffset = new float3(authoring.centerOffset.x, authoring.centerOffset.y, authoring.centerOffset.z)
            });

            // Add RequiresPhysics from PureDOTS
            AddComponent(entity, new RequiresPhysics
            {
                Priority = (byte)authoring.priority,
                Flags = PhysicsInteractionFlags.Collidable
            });

            // Add PhysicsInteractionConfig
            AddComponent(entity, new PhysicsInteractionConfig
            {
                Mass = 0f, // Static
                CollisionRadius = math.max(authoring.size.x, authoring.size.z) * 0.5f,
                Restitution = 0f,
                Friction = 1f,
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

