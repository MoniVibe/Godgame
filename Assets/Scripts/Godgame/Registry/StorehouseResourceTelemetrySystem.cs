using PureDOTS.Runtime.Components;
using PureDOTS.Runtime.Resource;
using PureDOTS.Runtime.Telemetry;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

namespace Godgame.Registry
{
    /// <summary>
    /// Publishes per-resource storehouse capacity and stored totals into the telemetry stream.
    /// </summary>
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(StorehouseTelemetrySystem))]
    public partial struct StorehouseResourceTelemetrySystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<ResourceTypeIndex>();
            state.RequireForUpdate<TelemetryStream>();
            state.RequireForUpdate<StorehouseInventory>();
        }

        public void OnUpdate(ref SystemState state)
        {
            var catalog = SystemAPI.GetSingleton<ResourceTypeIndex>().Catalog;
            if (!catalog.IsCreated || catalog.Value.Ids.Length == 0)
            {
                return;
            }

            var telemetryEntity = SystemAPI.GetSingletonEntity<TelemetryStream>();
            if (!state.EntityManager.HasBuffer<TelemetryMetric>(telemetryEntity))
            {
                state.EntityManager.AddBuffer<TelemetryMetric>(telemetryEntity);
            }

            var capacityTotals = new NativeArray<float>(catalog.Value.Ids.Length, Allocator.Temp);
            var storedTotals = new NativeArray<float>(catalog.Value.Ids.Length, Allocator.Temp);

            foreach (var (inventory, capacities, items) in SystemAPI
                         .Query<RefRO<StorehouseInventory>, DynamicBuffer<StorehouseCapacityElement>, DynamicBuffer<StorehouseInventoryItem>>())
            {
                Accumulate(capacities, catalog, capacityTotals);
                Accumulate(items, catalog, storedTotals);
            }

            var metrics = state.EntityManager.GetBuffer<TelemetryMetric>(telemetryEntity);
            for (int i = 0; i < catalog.Value.Ids.Length; i++)
            {
                var id = catalog.Value.Ids[i];
                if (id.Length == 0)
                {
                    continue;
                }

                var storedKey = CreateKey("storehouse.resource.", id, ".stored");
                var capacityKey = CreateKey("storehouse.resource.", id, ".capacity");
                metrics.Add(new TelemetryMetric { Key = storedKey, Value = storedTotals[i], Unit = TelemetryMetricUnit.Count });
                metrics.Add(new TelemetryMetric { Key = capacityKey, Value = capacityTotals[i], Unit = TelemetryMetricUnit.Count });
            }

            capacityTotals.Dispose();
            storedTotals.Dispose();
        }

        public void OnDestroy(ref SystemState state)
        {
        }

        private static void Accumulate(DynamicBuffer<StorehouseCapacityElement> capacities, BlobAssetReference<ResourceTypeIndexBlob> catalog, NativeArray<float> totals)
        {
            for (int i = 0; i < capacities.Length; i++)
            {
                var cap = capacities[i];
                var index = ResolveIndex(catalog, cap.ResourceTypeId);
                if (index >= 0 && index < totals.Length)
                {
                    totals[index] += cap.MaxCapacity;
                }
            }
        }

        private static void Accumulate(DynamicBuffer<StorehouseInventoryItem> items, BlobAssetReference<ResourceTypeIndexBlob> catalog, NativeArray<float> totals)
        {
            for (int i = 0; i < items.Length; i++)
            {
                var item = items[i];
                var index = ResolveIndex(catalog, item.ResourceTypeId);
                if (index >= 0 && index < totals.Length)
                {
                    totals[index] += item.Amount;
                }
            }
        }

        private static int ResolveIndex(BlobAssetReference<ResourceTypeIndexBlob> catalog, FixedString64Bytes resourceId)
        {
            if (!catalog.IsCreated || resourceId.Length == 0)
            {
                return -1;
            }

            for (int i = 0; i < catalog.Value.Ids.Length; i++)
            {
                if (catalog.Value.Ids[i].Equals(resourceId))
                {
                    return i;
                }
            }

            return -1;
        }

        private static FixedString64Bytes CreateKey(string prefix, FixedString64Bytes id, string suffix)
        {
            FixedString64Bytes key = default;
            Append(ref key, prefix);
            Append(ref key, id);
            Append(ref key, suffix);
            return key;
        }

        private static void Append(ref FixedString64Bytes target, string raw)
        {
            for (int i = 0; i < raw.Length && target.Length < target.Capacity; i++)
            {
                target.Append(raw[i]);
            }
        }

        private static void Append(ref FixedString64Bytes target, in FixedString64Bytes raw)
        {
            for (int i = 0; i < raw.Length && target.Length < target.Capacity; i++)
            {
                target.Append(raw[i]);
            }
        }
    }
}
