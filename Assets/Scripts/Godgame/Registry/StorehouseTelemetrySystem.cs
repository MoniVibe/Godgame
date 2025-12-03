using PureDOTS.Runtime.Components;
using PureDOTS.Runtime.Resource;
using PureDOTS.Runtime.Telemetry;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

namespace Godgame.Registry
{
    /// <summary>
    /// Publishes storehouse totals into the shared telemetry stream for HUD/debug dashboards.
    /// </summary>
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public partial struct StorehouseTelemetrySystem : ISystem
    {
        private FixedString64Bytes _capacityKey;
        private FixedString64Bytes _storedKey;
        private FixedString64Bytes _countKey;

        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<StorehouseRegistry>();
            state.RequireForUpdate<TelemetryStream>();
            _capacityKey = CreateKey("storehouse.capacity.total");
            _storedKey = CreateKey("storehouse.stored.total");
            _countKey = CreateKey("storehouse.count");
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var registry = SystemAPI.GetSingleton<StorehouseRegistry>();
            var telemetryEntity = SystemAPI.GetSingletonEntity<TelemetryStream>();
            if (!state.EntityManager.HasBuffer<TelemetryMetric>(telemetryEntity))
            {
                state.EntityManager.AddBuffer<TelemetryMetric>(telemetryEntity);
            }

            var metrics = state.EntityManager.GetBuffer<TelemetryMetric>(telemetryEntity);
            metrics.Add(new TelemetryMetric { Key = _capacityKey, Value = registry.TotalCapacity, Unit = TelemetryMetricUnit.Count });
            metrics.Add(new TelemetryMetric { Key = _storedKey, Value = registry.TotalStored, Unit = TelemetryMetricUnit.Count });
            metrics.Add(new TelemetryMetric { Key = _countKey, Value = registry.TotalStorehouses, Unit = TelemetryMetricUnit.Count });
        }

        public void OnDestroy(ref SystemState state)
        {
        }

        private static FixedString64Bytes CreateKey(string raw)
        {
            FixedString64Bytes key = default;
            for (int i = 0; i < raw.Length && i < key.Capacity; i++)
            {
                key.Append(raw[i]);
            }

            return key;
        }
    }
}
