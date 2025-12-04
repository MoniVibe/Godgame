using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace Godgame.Spawners
{
    /// <summary>
    /// Flags for spawner state.
    /// </summary>
    public enum SpawnerFlags : byte
    {
        None = 0,
        Active = 1 << 0,
        Paused = 1 << 1,
        CooldownActive = 1 << 2
    }

    /// <summary>
    /// Spawn pattern types for placement.
    /// </summary>
    public enum SpawnPattern : byte
    {
        Point = 0,
        RandomCircle = 1,
        Ring = 2,
        Grid = 3
    }

    /// <summary>
    /// Godgame-specific spawner component.
    /// Bridges to PureDOTS SpawnerId, SpawnerConfig, and SpawnerState components.
    /// </summary>
    public struct Spawner : IComponentData
    {
        public int Id;
        public FixedString64Bytes SpawnTypeId;
        public Entity OwnerFaction;
        public int Capacity;
        public float CooldownSeconds;
        public int ActiveSpawnCount;
        public float RemainingCooldown;
        public SpawnerFlags Flags;
        
        // Godgame-specific fields
        public Entity Prefab;          // Entity prefab to spawn
        public SpawnPattern Pattern;   // Spawn placement pattern
        public float SpawnRadius;     // Radius for circle/ring patterns
        public float SpawnRate;        // Spawns per second (alternative to cooldown)
        public uint Seed;              // Random seed for deterministic spawning
    }
}

