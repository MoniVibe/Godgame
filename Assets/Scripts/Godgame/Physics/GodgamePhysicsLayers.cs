using Unity.Physics;

namespace Godgame.Physics
{
    /// <summary>
    /// Physics layer enumeration for Godgame entities.
    /// Used for collision filtering between different entity types.
    /// </summary>
    public enum GodgamePhysicsLayer : byte
    {
        /// <summary>
        /// Ground-based units (villagers, soldiers, animals).
        /// Collides with: Building, Terrain, GroundUnit (soft avoidance), Projectile
        /// </summary>
        GroundUnit = 0,

        /// <summary>
        /// Flying units (birds, flying creatures, divine messengers).
        /// Collides with: Building, Terrain (altitude limiting), Projectile
        /// </summary>
        FlyingUnit = 1,

        /// <summary>
        /// Buildings and structures.
        /// Collides with: GroundUnit, FlyingUnit, Projectile
        /// </summary>
        Building = 2,

        /// <summary>
        /// Terrain colliders.
        /// Collides with: GroundUnit, FlyingUnit
        /// </summary>
        Terrain = 3,

        /// <summary>
        /// Projectiles (arrows, magic bolts, thrown objects).
        /// Collides with: GroundUnit, FlyingUnit, Building, Terrain
        /// </summary>
        Projectile = 4,

        /// <summary>
        /// Decorative objects (trees, rocks, bushes).
        /// Collides with: GroundUnit (soft avoidance only)
        /// </summary>
        Decoration = 5,

        /// <summary>
        /// Resource piles and gatherable objects.
        /// Collides with: GroundUnit (interaction trigger)
        /// </summary>
        Resource = 6,

        /// <summary>
        /// Trigger zones (area effects, miracle zones).
        /// Collides with: GroundUnit, FlyingUnit (as triggers)
        /// </summary>
        TriggerZone = 7
    }

    /// <summary>
    /// Static helper class for Godgame collision layer configuration.
    /// </summary>
    public static class GodgamePhysicsLayers
    {
        // Layer indices (matching enum values)
        public const int GroundUnit = 0;
        public const int FlyingUnit = 1;
        public const int Building = 2;
        public const int Terrain = 3;
        public const int Projectile = 4;
        public const int Decoration = 5;
        public const int Resource = 6;
        public const int TriggerZone = 7;

        /// <summary>
        /// Creates a collision filter for the specified layer.
        /// </summary>
        public static CollisionFilter CreateFilter(GodgamePhysicsLayer layer)
        {
            return new CollisionFilter
            {
                BelongsTo = GetBelongsToMask(layer),
                CollidesWith = GetCollidesWithMask(layer),
                GroupIndex = 0
            };
        }

        /// <summary>
        /// Gets the "belongs to" bitmask for a layer.
        /// </summary>
        public static uint GetBelongsToMask(GodgamePhysicsLayer layer)
        {
            return 1u << (int)layer;
        }

        /// <summary>
        /// Gets the "collides with" bitmask for a layer.
        /// </summary>
        public static uint GetCollidesWithMask(GodgamePhysicsLayer layer)
        {
            return layer switch
            {
                // GroundUnit collides with: Building, Terrain, GroundUnit (soft), Projectile, Decoration (soft), Resource (trigger)
                GodgamePhysicsLayer.GroundUnit => (1u << Building) | (1u << Terrain) | (1u << GroundUnit) | (1u << Projectile) | (1u << Decoration) | (1u << Resource) | (1u << TriggerZone),

                // FlyingUnit collides with: Building, Terrain (altitude), Projectile, TriggerZone
                GodgamePhysicsLayer.FlyingUnit => (1u << Building) | (1u << Terrain) | (1u << Projectile) | (1u << TriggerZone),

                // Building collides with: GroundUnit, FlyingUnit, Projectile
                GodgamePhysicsLayer.Building => (1u << GroundUnit) | (1u << FlyingUnit) | (1u << Projectile),

                // Terrain collides with: GroundUnit, FlyingUnit
                GodgamePhysicsLayer.Terrain => (1u << GroundUnit) | (1u << FlyingUnit),

                // Projectile collides with: GroundUnit, FlyingUnit, Building, Terrain
                GodgamePhysicsLayer.Projectile => (1u << GroundUnit) | (1u << FlyingUnit) | (1u << Building) | (1u << Terrain),

                // Decoration collides with: GroundUnit (soft avoidance)
                GodgamePhysicsLayer.Decoration => (1u << GroundUnit),

                // Resource collides with: GroundUnit (interaction trigger), Terrain (physics), Resource (piling), Building (physics)
                GodgamePhysicsLayer.Resource => (1u << GroundUnit) | (1u << Terrain) | (1u << Resource) | (1u << Building),

                // TriggerZone collides with: GroundUnit, FlyingUnit (as triggers)
                GodgamePhysicsLayer.TriggerZone => (1u << GroundUnit) | (1u << FlyingUnit),

                _ => 0u
            };
        }

        /// <summary>
        /// Checks if two layers should collide.
        /// </summary>
        public static bool ShouldCollide(GodgamePhysicsLayer layerA, GodgamePhysicsLayer layerB)
        {
            var maskA = GetCollidesWithMask(layerA);
            var maskB = GetBelongsToMask(layerB);
            return (maskA & maskB) != 0;
        }

        /// <summary>
        /// Gets the default priority for a layer.
        /// </summary>
        public static byte GetDefaultPriority(GodgamePhysicsLayer layer)
        {
            return layer switch
            {
                GodgamePhysicsLayer.GroundUnit => 150,   // High priority (player-visible)
                GodgamePhysicsLayer.FlyingUnit => 150,   // High priority
                GodgamePhysicsLayer.Building => 100,     // Medium (static)
                GodgamePhysicsLayer.Terrain => 50,       // Low (static, always present)
                GodgamePhysicsLayer.Projectile => 255,   // Highest (fast-moving)
                GodgamePhysicsLayer.Decoration => 25,    // Very low (cosmetic)
                GodgamePhysicsLayer.Resource => 75,      // Low-medium
                GodgamePhysicsLayer.TriggerZone => 50,   // Low (triggers only)
                _ => 100
            };
        }

        /// <summary>
        /// Checks if a layer uses soft avoidance (pushback) rather than hard collision.
        /// </summary>
        public static bool UsesSoftAvoidance(GodgamePhysicsLayer layer)
        {
            return layer switch
            {
                GodgamePhysicsLayer.GroundUnit => true,  // Units avoid each other softly
                GodgamePhysicsLayer.Decoration => true,  // Decorations push units gently
                _ => false
            };
        }

        /// <summary>
        /// Checks if a layer is a trigger (no physical response).
        /// </summary>
        public static bool IsTriggerLayer(GodgamePhysicsLayer layer)
        {
            return layer switch
            {
                GodgamePhysicsLayer.Resource => true,
                GodgamePhysicsLayer.TriggerZone => true,
                _ => false
            };
        }

        /// <summary>
        /// Checks if a layer is static (doesn't move).
        /// </summary>
        public static bool IsStaticLayer(GodgamePhysicsLayer layer)
        {
            return layer switch
            {
                GodgamePhysicsLayer.Building => true,
                GodgamePhysicsLayer.Terrain => true,
                GodgamePhysicsLayer.Decoration => true,
                GodgamePhysicsLayer.Resource => true,
                GodgamePhysicsLayer.TriggerZone => true,
                _ => false
            };
        }
    }
}

