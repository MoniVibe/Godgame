using PureDOTS.Runtime;
using PureDOTS.Runtime.Components;
using PureDOTS.Runtime.Physics;
using Unity.Entities;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using Godgame.Physics;

namespace Godgame.Authoring
{
    /// <summary>
    /// Authoring component for Godgame rocks (throwable, mineable physics objects).
    /// Creates a rock entity with physics, throwability, and optional resource deposits.
    /// Similar to Space4XRockAuthoring but uses PureDOTS physics components.
    /// </summary>
    public class GodgameRockPhysicsAuthoring : MonoBehaviour
    {
        [Header("Collider Settings")]
        [Tooltip("Type of collider to use")]
        public ColliderType colliderType = ColliderType.Sphere;

        [Tooltip("Radius for sphere/capsule colliders")]
        public float radius = 1f;

        [Tooltip("Size for box colliders (x, y, z)")]
        public Vector3 size = Vector3.one;

        [Tooltip("Height for capsule colliders")]
        public float height = 2f;

        [Tooltip("Center offset from transform")]
        public Vector3 centerOffset = Vector3.zero;

        [Header("Destructible")]
        [Tooltip("Is this rock destructible?")]
        public bool isDestructible = true;

        [Tooltip("Hit points (if destructible)")]
        public float hitPoints = 100f;

        [Header("Impact Damage")]
        [Tooltip("Does this rock deal impact damage?")]
        public bool dealsImpactDamage = true;

        [Tooltip("Damage per unit of collision impulse")]
        public float damagePerImpulse = 10f;

        [Tooltip("Minimum impulse to count as a hit")]
        public float minImpulse = 1f;

        [Header("Resource Deposit")]
        [Tooltip("Is this rock a resource deposit?")]
        public bool isResourceDeposit = true;

        [Tooltip("Resource type ID (index into ResourceTypeIndex catalog)")]
        public int resourceTypeId = 0;

        [Tooltip("Current amount of resource")]
        public float resourceAmount = 50f;

        [Tooltip("Maximum amount (for UI/regen)")]
        public float maxResourceAmount = 50f;

        [Tooltip("Regeneration per second (0 for non-regenerating)")]
        public float regenPerSecond = 0f;

        [Header("Material Properties")]
        [Tooltip("Material hardness (resistance to deformation). Rock: 2.0, Ship: 1.5, Soft: 0.5")]
        public float hardness = 2f;

        [Tooltip("Material fragility (how easily it shatters). Brittle rock: 1.5, Durable: 0.5")]
        public float fragility = 0.5f;

        [Tooltip("Material density (for mass calculations). Rock: 3.0, Ship: 2.0, Soft: 0.8")]
        public float density = 3f;

        [Header("Behavior Flags")]
        [Tooltip("Entity generates collision events")]
        public bool raisesCollisionEvents = true;

        [Tooltip("Entity is a trigger (no physical response)")]
        public bool isTrigger = false;

        [Tooltip("Use continuous collision detection")]
        public bool continuousCollision = false;

        [Header("Priority")]
        [Tooltip("Physics processing priority (0-255)")]
        [Range(0, 255)]
        public int priority = 100;

        [Header("Divine Hand Interaction")]
        [Tooltip("Allow the divine hand to pick up and throw this rock")]
        public bool allowDivineHandPickup = true;

        [Tooltip("Hand pickable mass (used for future tuning)")]
        public float handPickableMass = 10f;

        [Tooltip("Maximum hold distance for hand pickup")]
        public float handMaxHoldDistance = 12f;

        [Tooltip("Throw impulse multiplier for hand throws")]
        public float handThrowImpulseMultiplier = 1f;

        [Tooltip("Follow lerp while held by the divine hand")]
        [Range(0.01f, 1f)]
        public float handFollowLerp = 0.2f;

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(0.5f, 0.3f, 0.1f, 0.5f); // Brown for rocks
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
                    Gizmos.DrawWireSphere(center + Vector3.up * (height * 0.5f - radius), radius);
                    Gizmos.DrawWireSphere(center - Vector3.up * (height * 0.5f - radius), radius);
                    break;
            }
        }
    }

    /// <summary>
    /// Baker for Godgame rock authoring.
    /// </summary>
    public class GodgameRockPhysicsBaker : Baker<GodgameRockPhysicsAuthoring>
    {
        public override void Bake(GodgameRockPhysicsAuthoring authoring)
        {
            var entity = GetEntity(authoring, TransformUsageFlags.Dynamic);

            // Add rock tags
            AddComponent<RockTag>(entity);
            AddComponent<ThrowableTag>(entity);

            // Add resource node tag if it's a resource deposit
            if (authoring.isResourceDeposit)
            {
                AddComponent<ResourceNodeTag>(entity);
                AddComponent(entity, new ResourceDeposit
                {
                    ResourceTypeId = authoring.resourceTypeId,
                    CurrentAmount = authoring.resourceAmount,
                    MaxAmount = authoring.maxResourceAmount,
                    RegenPerSecond = authoring.regenPerSecond
                });

                // Add ResourceSourceConfig for ResourceRegistrySystem
                // Map index to string ID (simplified mapping for now)
                FixedString64Bytes resourceId = default;
                if (authoring.resourceTypeId == 0) resourceId = new FixedString64Bytes("stone");
                else if (authoring.resourceTypeId == 1) resourceId = new FixedString64Bytes("wood");
                else if (authoring.resourceTypeId == 2) resourceId = new FixedString64Bytes("ore");
                else resourceId = new FixedString64Bytes("unknown");

                AddComponent(entity, new Godgame.Resources.ResourceSourceConfig
                {
                    ResourceTypeId = resourceId,
                    Amount = authoring.resourceAmount,
                    MaxAmount = authoring.maxResourceAmount,
                    RegenRate = authoring.regenPerSecond
                });
            }

            // Add destructible if enabled
            if (authoring.isDestructible)
            {
                AddComponent(entity, new Destructible
                {
                    HitPoints = authoring.hitPoints,
                    MaxHitPoints = authoring.hitPoints
                });
            }

            // Add impact damage if enabled
            if (authoring.dealsImpactDamage)
            {
                AddComponent(entity, new ImpactDamage
                {
                    DamagePerImpulse = authoring.damagePerImpulse,
                    MinImpulse = authoring.minImpulse
                });
            }

            // Add material stats (always added for material-aware damage calculation)
            AddComponent(entity, new MaterialStats
            {
                Hardness = authoring.hardness,
                Fragility = authoring.fragility,
                Density = authoring.density
            });

            // Add GodgamePhysicsBody marker
            AddComponent(entity, new GodgamePhysicsBody
            {
                Layer = GodgamePhysicsLayer.Resource,
                Priority = (byte)authoring.priority,
                Flags = GodgamePhysicsFlags.RaisesCollisionEvents | GodgamePhysicsFlags.IsActive
            });

            // Add PureDOTS physics components
            var flags = PhysicsInteractionFlags.None;
            if (authoring.raisesCollisionEvents)
                flags |= PhysicsInteractionFlags.Collidable;

            AddComponent(entity, new RequiresPhysics
            {
                Priority = (byte)authoring.priority,
                Flags = flags
            });

            AddComponent(entity, new PhysicsInteractionConfig
            {
                Mass = 1f,
                CollisionRadius = authoring.radius,
                Restitution = 0f,
                Friction = 0f,
                LinearDamping = 0f,
                AngularDamping = 0f
            });

            // Add collision event buffer if events are enabled
            if (authoring.raisesCollisionEvents)
            {
                AddBuffer<PureDOTS.Runtime.Physics.PhysicsCollisionEventElement>(entity);
            }

            if (authoring.allowDivineHandPickup)
            {
                AddComponent<Pickable>(entity);
                AddComponent(entity, new HandPickable
                {
                    Mass = math.max(0.1f, authoring.handPickableMass),
                    MaxHoldDistance = math.max(0.1f, authoring.handMaxHoldDistance),
                    ThrowImpulseMultiplier = math.max(0.1f, authoring.handThrowImpulseMultiplier),
                    FollowLerp = math.clamp(authoring.handFollowLerp, 0.01f, 1f)
                });
            }
        }
    }
}

