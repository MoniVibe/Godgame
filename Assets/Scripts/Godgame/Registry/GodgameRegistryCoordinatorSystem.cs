using PureDOTS.Runtime.Bands;
using PureDOTS.Runtime.Components;
using PureDOTS.Runtime.Registry;
using PureDOTS.Runtime.Transport;
using Unity.Burst;
using Unity.Entities;

namespace Godgame.Registry
{
    /// <summary>
    /// Main orchestrator for Godgame registry bridge systems.
    /// Coordinates all sync systems and ensures registries appear inside the shared directory so
    /// telemetry/UI layers can discover them.
    /// </summary>
    [BurstCompile]
    [UpdateInGroup(typeof(SimulationSystemGroup), OrderFirst = true)]
    public partial struct GodgameRegistryCoordinatorSystem : ISystem
    {
        private uint _lastWarningTick;

        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<RegistryDirectory>();
            state.RequireForUpdate<VillagerRegistry>();
            state.RequireForUpdate<StorehouseRegistry>();
            state.RequireForUpdate<LogisticsRequestRegistry>();
            state.RequireForUpdate<BandRegistry>();
            state.RequireForUpdate<SpawnerRegistry>();
            state.RequireForUpdate<MiracleRegistry>();
            state.RequireForUpdate<TimeState>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var timeState = SystemAPI.GetSingleton<TimeState>();
            var directoryEntity = SystemAPI.GetSingletonEntity<RegistryDirectory>();
            var entries = state.EntityManager.GetBuffer<RegistryDirectoryEntry>(directoryEntity);

            var villagerRegistered = entries.TryGetHandle(RegistryKind.Villager, out _);
            var storehouseRegistered = entries.TryGetHandle(RegistryKind.Storehouse, out _);
            var logisticsRegistered = entries.TryGetHandle(RegistryKind.LogisticsRequest, out _);
            var bandRegistered = entries.TryGetHandle(RegistryKind.Band, out _);
            var spawnerRegistered = entries.TryGetHandle(RegistryKind.Spawner, out _);
            var miracleRegistered = entries.TryGetHandle(RegistryKind.Miracle, out _);

            if (villagerRegistered && storehouseRegistered && logisticsRegistered && 
                bandRegistered && spawnerRegistered && miracleRegistered)
            {
                _lastWarningTick = timeState.Tick;
                return;
            }

#if UNITY_EDITOR
            // Emit lightweight warnings during scenario bring-up so missing registries are obvious.
            const uint warningIntervalTicks = 120; // ~2 seconds @60fps
            if (timeState.Tick - _lastWarningTick >= warningIntervalTicks)
            {
                _lastWarningTick = timeState.Tick;
                if (!villagerRegistered)
                {
                    UnityEngine.Debug.LogWarning("[GodgameRegistryCoordinatorSystem] Villager registry handle is missing from the directory. Ensure GodgameVillagerSyncSystem ran this frame.");
                }

                if (!storehouseRegistered)
                {
                    UnityEngine.Debug.LogWarning("[GodgameRegistryCoordinatorSystem] Storehouse registry handle is missing from the directory. Ensure GodgameStorehouseSyncSystem ran this frame.");
                }

                if (!logisticsRegistered)
                {
                    UnityEngine.Debug.LogWarning("[GodgameRegistryCoordinatorSystem] LogisticsRequest registry handle is missing from the directory. Ensure GodgameLogisticsSyncSystem ran this frame.");
                }

                if (!bandRegistered)
                {
                    UnityEngine.Debug.LogWarning("[GodgameRegistryCoordinatorSystem] Band registry handle is missing from the directory. Ensure GodgameBandSyncSystem ran this frame.");
                }

                if (!spawnerRegistered)
                {
                    UnityEngine.Debug.LogWarning("[GodgameRegistryCoordinatorSystem] Spawner registry handle is missing from the directory. Ensure GodgameSpawnerSyncSystem ran this frame.");
                }

                if (!miracleRegistered)
                {
                    UnityEngine.Debug.LogWarning("[GodgameRegistryCoordinatorSystem] Miracle registry handle is missing from the directory. Ensure GodgameMiracleSyncSystem ran this frame.");
                }
            }
#endif
        }
    }
}
