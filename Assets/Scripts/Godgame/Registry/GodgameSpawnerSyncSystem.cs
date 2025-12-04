using Godgame.Spawners;
using PureDOTS.Runtime.Components;
using PureDOTS.Runtime.Registry;
using PureDOTS.Systems;
using Unity.Burst;
using Unity.Entities;

namespace Godgame.Registry
{
    /// <summary>
    /// Bridges Godgame-specific spawner components to PureDOTS canonical SpawnerId, SpawnerConfig, and SpawnerState components.
    /// Follows projection pattern: if entity has PureDOTS SpawnerId/SpawnerConfig/SpawnerState, leave it alone.
    /// If entity has Godgame spawner components but not canonical, project/add canonical components.
    /// </summary>
    [BurstCompile]
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateBefore(typeof(EnvironmentSystemGroup))]
    public partial struct GodgameSpawnerSyncSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            // Require PureDOTS registry and time state
            state.RequireForUpdate<SpawnerRegistry>();
            state.RequireForUpdate<TimeState>();
            state.RequireForUpdate<RewindState>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var timeState = SystemAPI.GetSingleton<TimeState>();
            var rewindState = SystemAPI.GetSingleton<RewindState>();
            
            // Skip if paused or not recording
            if (timeState.IsPaused || rewindState.Mode != RewindMode.Record)
            {
                return;
            }

            var ecb = SystemAPI
                .GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>()
                .CreateCommandBuffer(state.WorldUnmanaged);

            // 1. PROJECT: Query entities with Godgame spawner components but no SpawnerId/SpawnerConfig/SpawnerState
            foreach (var (ggSpawner, entity) in SystemAPI
                     .Query<RefRO<Spawner>>()
                     .WithNone<SpawnerId, SpawnerConfig, SpawnerState>()
                     .WithEntityAccess())
            {
                ecb.AddComponent(entity, new SpawnerId
                {
                    Value = ggSpawner.ValueRO.Id
                });
                
                ecb.AddComponent(entity, new SpawnerConfig
                {
                    SpawnTypeId = ggSpawner.ValueRO.SpawnTypeId,
                    OwnerFaction = ggSpawner.ValueRO.OwnerFaction,
                    Capacity = ggSpawner.ValueRO.Capacity,
                    CooldownSeconds = ggSpawner.ValueRO.CooldownSeconds
                });
                
                ecb.AddComponent(entity, new SpawnerState
                {
                    ActiveSpawnCount = ggSpawner.ValueRO.ActiveSpawnCount,
                    RemainingCooldown = ggSpawner.ValueRO.RemainingCooldown,
                    Flags = MapFlags(ggSpawner.ValueRO.Flags)
                });
                
                ecb.AddComponent<SyncedFromGodgame>(entity);
            }

            // 2. UPDATE: Query entities with both Godgame and canonical components
            foreach (var (ggSpawner, id, config, spawnState) in SystemAPI
                     .Query<RefRO<Spawner>, RefRW<SpawnerId>, RefRW<SpawnerConfig>, RefRW<SpawnerState>>()
                     .WithChangeFilter<Spawner>())
            {
                id.ValueRW.Value = ggSpawner.ValueRO.Id;
                config.ValueRW.SpawnTypeId = ggSpawner.ValueRO.SpawnTypeId;
                config.ValueRW.OwnerFaction = ggSpawner.ValueRO.OwnerFaction;
                config.ValueRW.Capacity = ggSpawner.ValueRO.Capacity;
                config.ValueRW.CooldownSeconds = ggSpawner.ValueRO.CooldownSeconds;
                spawnState.ValueRW.ActiveSpawnCount = ggSpawner.ValueRO.ActiveSpawnCount;
                spawnState.ValueRW.RemainingCooldown = ggSpawner.ValueRO.RemainingCooldown;
                spawnState.ValueRW.Flags = MapFlags(ggSpawner.ValueRO.Flags);
            }

            // 3. CLEANUP: Query entities with SpawnerId/SpawnerConfig/SpawnerState + SyncedFromGodgame but no Godgame source
            foreach (var (id, config, spawnState, entity) in SystemAPI
                     .Query<RefRO<SpawnerId>, RefRO<SpawnerConfig>, RefRO<SpawnerState>>()
                     .WithAll<SyncedFromGodgame>()
                     .WithNone<Spawner>()
                     .WithEntityAccess())
            {
                ecb.RemoveComponent<SpawnerId>(entity);
                ecb.RemoveComponent<SpawnerConfig>(entity);
                ecb.RemoveComponent<SpawnerState>(entity);
                ecb.RemoveComponent<SyncedFromGodgame>(entity);
            }
        }

        private static byte MapFlags(SpawnerFlags flags)
        {
            byte result = 0;
            if ((flags & SpawnerFlags.Active) != 0) result |= 1;
            if ((flags & SpawnerFlags.Paused) != 0) result |= 2;
            if ((flags & SpawnerFlags.CooldownActive) != 0) result |= 4;
            return result;
        }
    }
}

