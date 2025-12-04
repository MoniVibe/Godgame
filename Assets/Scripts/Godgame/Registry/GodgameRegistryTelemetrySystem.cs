using PureDOTS.Runtime.Components;
using PureDOTS.Runtime.Telemetry;
using PureDOTS.Runtime.Transport;
using PureDOTS.Runtime.Registry;
using Unity.Collections;
using Unity.Entities;

namespace Godgame.Registry
{
    /// <summary>
    /// Publishes Godgame registry summaries (villagers, storehouses, logistics, miracles) to the shared telemetry stream.
    /// Keeps the neutral HUD/dashboard in sync with Godgame data just like the Space4X bridge.
    /// </summary>
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(GodgameRegistryBridgeSystem))]
    public partial struct GodgameRegistryTelemetrySystem : ISystem
    {
        private static readonly FixedString64Bytes s_VillagersTotal = new FixedString64Bytes("godgame.registry.villagers.total");
        private static readonly FixedString64Bytes s_VillagersAvailable = new FixedString64Bytes("godgame.registry.villagers.available");
        private static readonly FixedString64Bytes s_VillagersIdle = new FixedString64Bytes("godgame.registry.villagers.idle");
        private static readonly FixedString64Bytes s_VillagersCombatReady = new FixedString64Bytes("godgame.registry.villagers.combatReady");
        private static readonly FixedString64Bytes s_VillagersAvgHealth = new FixedString64Bytes("godgame.registry.villagers.avgHealth");
        private static readonly FixedString64Bytes s_VillagersAvgMorale = new FixedString64Bytes("godgame.registry.villagers.avgMorale");
        private static readonly FixedString64Bytes s_VillagersAvgEnergy = new FixedString64Bytes("godgame.registry.villagers.avgEnergy");

        private static readonly FixedString64Bytes s_StorehousesTotal = new FixedString64Bytes("godgame.registry.storehouses.total");
        private static readonly FixedString64Bytes s_StorehousesCapacity = new FixedString64Bytes("godgame.registry.storehouses.capacity");
        private static readonly FixedString64Bytes s_StorehousesStored = new FixedString64Bytes("godgame.registry.storehouses.stored");

        private static readonly FixedString64Bytes s_LogisticsTotal = new FixedString64Bytes("godgame.registry.logistics.total");
        private static readonly FixedString64Bytes s_LogisticsPending = new FixedString64Bytes("godgame.registry.logistics.pending");
        private static readonly FixedString64Bytes s_LogisticsInProgress = new FixedString64Bytes("godgame.registry.logistics.inProgress");
        private static readonly FixedString64Bytes s_LogisticsCritical = new FixedString64Bytes("godgame.registry.logistics.critical");
        private static readonly FixedString64Bytes s_LogisticsRequested = new FixedString64Bytes("godgame.registry.logistics.requestedUnits");
        private static readonly FixedString64Bytes s_LogisticsRemaining = new FixedString64Bytes("godgame.registry.logistics.remainingUnits");

        private static readonly FixedString64Bytes s_MiraclesTotal = new FixedString64Bytes("godgame.registry.miracles.total");
        private static readonly FixedString64Bytes s_MiraclesActive = new FixedString64Bytes("godgame.registry.miracles.active");
        private static readonly FixedString64Bytes s_MiraclesSustained = new FixedString64Bytes("godgame.registry.miracles.sustained");
        private static readonly FixedString64Bytes s_MiraclesCooling = new FixedString64Bytes("godgame.registry.miracles.cooling");
        private static readonly FixedString64Bytes s_MiraclesEnergyCost = new FixedString64Bytes("godgame.registry.miracles.energyCost");
        private static readonly FixedString64Bytes s_MiraclesCooldownSeconds = new FixedString64Bytes("godgame.registry.miracles.cooldownSeconds");

        private EntityQuery _telemetryQuery;
        private EntityQuery _villagerRegistryQuery;
        private EntityQuery _storehouseRegistryQuery;
        private EntityQuery _logisticsRegistryQuery;
        private EntityQuery _miracleRegistryQuery;

        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<TelemetryStream>();
            _telemetryQuery = state.GetEntityQuery(ComponentType.ReadOnly<TelemetryStream>(), ComponentType.ReadWrite<TelemetryMetric>());
            _villagerRegistryQuery = state.GetEntityQuery(ComponentType.ReadOnly<VillagerRegistry>());
            _storehouseRegistryQuery = state.GetEntityQuery(ComponentType.ReadOnly<StorehouseRegistry>());
            _logisticsRegistryQuery = state.GetEntityQuery(ComponentType.ReadOnly<LogisticsRequestRegistry>());
            _miracleRegistryQuery = state.GetEntityQuery(ComponentType.ReadOnly<MiracleRegistry>());
        }

        public void OnUpdate(ref SystemState state)
        {
            if (_telemetryQuery.IsEmptyIgnoreFilter)
            {
                return;
            }

            var telemetryEntity = _telemetryQuery.GetSingletonEntity();
            var buffer = state.EntityManager.GetBuffer<TelemetryMetric>(telemetryEntity);

            if (_villagerRegistryQuery.TryGetSingleton(out VillagerRegistry villagerRegistry))
            {
                buffer.AddMetric(s_VillagersTotal, villagerRegistry.TotalVillagers);
                buffer.AddMetric(s_VillagersAvailable, villagerRegistry.AvailableVillagers);
                buffer.AddMetric(s_VillagersIdle, villagerRegistry.IdleVillagers);
                buffer.AddMetric(s_VillagersCombatReady, villagerRegistry.CombatReadyVillagers);
                buffer.AddMetric(s_VillagersAvgHealth, villagerRegistry.AverageHealthPercent, TelemetryMetricUnit.Ratio);
                buffer.AddMetric(s_VillagersAvgMorale, villagerRegistry.AverageMoralePercent, TelemetryMetricUnit.Ratio);
                buffer.AddMetric(s_VillagersAvgEnergy, villagerRegistry.AverageEnergyPercent, TelemetryMetricUnit.Ratio);
            }

            if (_storehouseRegistryQuery.TryGetSingleton(out StorehouseRegistry storehouseRegistry))
            {
                buffer.AddMetric(s_StorehousesTotal, storehouseRegistry.TotalStorehouses);
                buffer.AddMetric(s_StorehousesCapacity, storehouseRegistry.TotalCapacity);
                buffer.AddMetric(s_StorehousesStored, storehouseRegistry.TotalStored);
            }

            if (_logisticsRegistryQuery.TryGetSingleton(out LogisticsRequestRegistry logisticsRegistry))
            {
                buffer.AddMetric(s_LogisticsTotal, logisticsRegistry.TotalRequests);
                buffer.AddMetric(s_LogisticsPending, logisticsRegistry.PendingRequests);
                buffer.AddMetric(s_LogisticsInProgress, logisticsRegistry.InProgressRequests);
                buffer.AddMetric(s_LogisticsCritical, logisticsRegistry.CriticalRequests);
                buffer.AddMetric(s_LogisticsRequested, logisticsRegistry.TotalRequestedUnits);
                buffer.AddMetric(s_LogisticsRemaining, logisticsRegistry.TotalRemainingUnits);
            }

            if (_miracleRegistryQuery.TryGetSingleton(out MiracleRegistry miracleRegistry))
            {
                buffer.AddMetric(s_MiraclesTotal, miracleRegistry.TotalMiracles);
                buffer.AddMetric(s_MiraclesActive, miracleRegistry.ActiveMiracles);
                buffer.AddMetric(s_MiraclesSustained, miracleRegistry.SustainedMiracles);
                buffer.AddMetric(s_MiraclesCooling, miracleRegistry.CoolingMiracles);
                buffer.AddMetric(s_MiraclesEnergyCost, miracleRegistry.TotalEnergyCost);
                buffer.AddMetric(s_MiraclesCooldownSeconds, miracleRegistry.TotalCooldownSeconds);
            }
        }
    }
}
