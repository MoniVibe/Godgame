using PureDOTS.Runtime.Components;
using PureDOTS.Runtime.Registry;
using PureDOTS.Runtime.Resource;
using StorehouseCapacityElement = PureDOTS.Runtime.Components.StorehouseCapacityElement;
using StorehouseInventoryItem = PureDOTS.Runtime.Components.StorehouseInventoryItem;
using PureDOTS.Runtime.Spatial;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Godgame.Registry
{
    /// <summary>
    /// Syncs Godgame storehouses into the shared storehouse registry so HUD/telemetry has live inventory data.
    /// </summary>
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(GodgameRegistryCoordinatorSystem))]
    public partial struct GodgameStorehouseSyncSystem : ISystem
    {
        private EntityQuery _storehouseQuery;
        private ComponentLookup<StorehouseJobReservation> _reservationLookup;
        private BufferLookup<StorehouseReservationItem> _reservationItemsLookup;
        private BufferLookup<StorehouseCapacityElement> _capacityLookup;
        private BufferLookup<StorehouseInventoryItem> _inventoryItemsLookup;
        private ComponentLookup<SpatialGridResidency> _residencyLookup;

        public void OnCreate(ref SystemState state)
        {
            _storehouseQuery = SystemAPI.QueryBuilder()
                .WithAll<StorehouseConfig, StorehouseInventory, LocalTransform>()
                .Build();

            _reservationLookup = state.GetComponentLookup<StorehouseJobReservation>(true);
            _reservationItemsLookup = state.GetBufferLookup<StorehouseReservationItem>(true);
            _capacityLookup = state.GetBufferLookup<StorehouseCapacityElement>(true);
            _inventoryItemsLookup = state.GetBufferLookup<StorehouseInventoryItem>(true);
            _residencyLookup = state.GetComponentLookup<SpatialGridResidency>(true);

            state.RequireForUpdate<StorehouseRegistry>();
            state.RequireForUpdate<ResourceTypeIndex>();
            state.RequireForUpdate<TimeState>();
            state.RequireForUpdate<RewindState>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var timeState = SystemAPI.GetSingleton<TimeState>();
            var rewindState = SystemAPI.GetSingleton<RewindState>();
            if (timeState.IsPaused || rewindState.Mode != RewindMode.Record)
            {
                return;
            }

            var registryEntity = SystemAPI.GetSingletonEntity<StorehouseRegistry>();
            var catalogRef = SystemAPI.GetSingleton<ResourceTypeIndex>().Catalog;
            if (!catalogRef.IsCreated || _storehouseQuery.IsEmptyIgnoreFilter)
            {
                ClearRegistry(ref state, registryEntity, timeState.Tick);
                return;
            }

            _reservationLookup.Update(ref state);
            _reservationItemsLookup.Update(ref state);
            _capacityLookup.Update(ref state);
            _inventoryItemsLookup.Update(ref state);
            _residencyLookup.Update(ref state);

            ref var catalog = ref catalogRef.Value;
            var registry = SystemAPI.GetComponentRW<StorehouseRegistry>(registryEntity);
            var entries = state.EntityManager.GetBuffer<StorehouseRegistryEntry>(registryEntity);
            ref var metadata = ref SystemAPI.GetComponentRW<RegistryMetadata>(registryEntity).ValueRW;

            var hasSpatialConfig = SystemAPI.TryGetSingleton(out SpatialGridConfig gridConfig);
            var hasSpatialState = SystemAPI.TryGetSingleton(out SpatialGridState gridState);
            var hasSpatial = hasSpatialConfig
                             && hasSpatialState
                             && gridConfig.CellCount > 0
                             && gridConfig.CellSize > 0f;
            var hasSyncState = SystemAPI.TryGetSingleton(out RegistrySpatialSyncState syncState);
            var requireSpatialSync = metadata.SupportsSpatialQueries && hasSyncState && syncState.HasSpatialData;
            var spatialVersionSource = hasSpatial
                ? gridState.Version
                : (requireSpatialSync ? syncState.SpatialVersion : 0u);

            var expectedCount = math.max(8, _storehouseQuery.CalculateEntityCount());
            using var builder = new DeterministicRegistryBuilder<StorehouseRegistryEntry>(expectedCount, Allocator.Temp);

            var totalStorehouses = 0;
            var totalCapacity = 0f;
            var totalStored = 0f;
            var resolvedCount = 0;
            var fallbackCount = 0;
            var unmappedCount = 0;

            foreach (var (config, inventory, transform, entity) in SystemAPI.Query<RefRO<StorehouseConfig>, RefRO<StorehouseInventory>, RefRO<LocalTransform>>()
                         .WithEntityAccess())
            {
                totalStorehouses++;
                totalCapacity += math.max(0f, inventory.ValueRO.TotalCapacity);
                totalStored += math.max(0f, inventory.ValueRO.TotalStored);

                var typeSummaries = new FixedList64Bytes<StorehouseRegistryCapacitySummary>();

                if (_capacityLookup.HasBuffer(entity))
                {
                    var capacities = _capacityLookup[entity];
                    for (var i = 0; i < capacities.Length; i++)
                    {
                        var capacityEntry = capacities[i];
                        if (capacityEntry.ResourceTypeId.Length == 0)
                        {
                            continue;
                        }

                        var typeIndex = catalog.LookupIndex(capacityEntry.ResourceTypeId);
                        if (typeIndex < 0)
                        {
                            continue;
                        }

                        EnsureCapacitySummary(ref typeSummaries, (ushort)typeIndex, capacityEntry.MaxCapacity);
                    }
                }

                if (_inventoryItemsLookup.HasBuffer(entity))
                {
                    var inventoryItems = _inventoryItemsLookup[entity];
                    for (var i = 0; i < inventoryItems.Length; i++)
                    {
                        var item = inventoryItems[i];
                        if (item.ResourceTypeId.Length == 0)
                        {
                            continue;
                        }

                        var typeIndex = catalog.LookupIndex(item.ResourceTypeId);
                        if (typeIndex < 0)
                        {
                            continue;
                        }

                        ApplyInventorySummary(ref typeSummaries, (ushort)typeIndex, item.Amount, item.Reserved, item.TierId, item.AverageQuality);
                    }
                }

                if (_reservationLookup.HasComponent(entity) && _reservationItemsLookup.HasBuffer(entity))
                {
                    var reservationItems = _reservationItemsLookup[entity];
                    for (var i = 0; i < reservationItems.Length; i++)
                    {
                        var reservation = reservationItems[i];
                        ApplyReservation(ref typeSummaries, reservation.ResourceTypeIndex, reservation.Reserved);
                    }
                }

                var cellId = -1;
                var entrySpatialVersion = spatialVersionSource;
                if (hasSpatial)
                {
                    var resolved = false;
                    var fallback = false;

                    if (_residencyLookup.HasComponent(entity))
                    {
                        var residency = _residencyLookup[entity];
                        if ((uint)residency.CellId < (uint)gridConfig.CellCount && residency.Version == gridState.Version)
                        {
                            cellId = residency.CellId;
                            entrySpatialVersion = residency.Version;
                            resolved = true;
                        }
                    }

                    if (!resolved)
                    {
                        SpatialHash.Quantize(transform.ValueRO.Position, gridConfig, out var coords);
                        var fallbackCell = SpatialHash.Flatten(in coords, in gridConfig);
                        if ((uint)fallbackCell < (uint)gridConfig.CellCount)
                        {
                            cellId = fallbackCell;
                            entrySpatialVersion = gridState.Version;
                            fallback = true;
                        }
                        else
                        {
                            cellId = -1;
                            entrySpatialVersion = 0;
                            unmappedCount++;
                        }
                    }

                    if (resolved)
                    {
                        resolvedCount++;
                    }
                    else if (fallback)
                    {
                        fallbackCount++;
                    }
                }

                var dominantTier = ResourceQualityTier.Unknown;
                ushort dominantQuality = 0;
                var bestStored = -1f;
                for (var i = 0; i < typeSummaries.Length; i++)
                {
                    var summary = typeSummaries[i];
                    if (summary.Stored > bestStored)
                    {
                        bestStored = summary.Stored;
                        var clampedTier = (byte)math.clamp((int)summary.TierId, (int)ResourceQualityTier.Unknown, (int)ResourceQualityTier.Relic);
                        dominantTier = (ResourceQualityTier)clampedTier;
                        dominantQuality = summary.AverageQuality;
                    }
                }

                var lastMutationTick = inventory.ValueRO.LastUpdateTick;
                if (_reservationLookup.HasComponent(entity))
                {
                    lastMutationTick = math.max(lastMutationTick, _reservationLookup[entity].LastMutationTick);
                }

                var entry = new StorehouseRegistryEntry
                {
                    StorehouseEntity = entity,
                    Position = transform.ValueRO.Position,
                    TotalCapacity = math.max(0f, inventory.ValueRO.TotalCapacity),
                    TotalStored = math.max(0f, inventory.ValueRO.TotalStored),
                    TypeSummaries = typeSummaries,
                    LastMutationTick = lastMutationTick,
                    CellId = cellId,
                    SpatialVersion = entrySpatialVersion,
                    DominantTier = dominantTier,
                    AverageQuality = dominantQuality
                };

                builder.Add(in entry);
            }

            var continuity = metadata.SupportsSpatialQueries && (resolvedCount + fallbackCount + unmappedCount > 0)
                ? RegistryContinuitySnapshot.WithSpatialData(spatialVersionSource, resolvedCount, fallbackCount, unmappedCount, requireSpatialSync)
                : metadata.SupportsSpatialQueries
                    ? RegistryContinuitySnapshot.WithoutSpatialData(requireSpatialSync)
                    : default;

            builder.ApplyTo(ref entries, ref metadata, timeState.Tick, continuity);

            var registryValue = registry.ValueRW;
            registryValue.TotalStorehouses = totalStorehouses;
            registryValue.TotalCapacity = totalCapacity;
            registryValue.TotalStored = totalStored;
            registryValue.LastUpdateTick = timeState.Tick;
            registryValue.LastSpatialVersion = spatialVersionSource;
            registryValue.SpatialResolvedCount = resolvedCount;
            registryValue.SpatialFallbackCount = fallbackCount;
            registryValue.SpatialUnmappedCount = unmappedCount;
            registry.ValueRW = registryValue;
        }

        private static void EnsureCapacitySummary(ref FixedList64Bytes<StorehouseRegistryCapacitySummary> summaries, ushort typeIndex, float maxCapacity)
        {
            for (var i = 0; i < summaries.Length; i++)
            {
                var summary = summaries[i];
                if (summary.ResourceTypeIndex == typeIndex)
                {
                    summary.Capacity = math.max(summary.Capacity, maxCapacity);
                    summaries[i] = summary;
                    return;
                }
            }

            TryAddSummary(ref summaries, new StorehouseRegistryCapacitySummary
            {
                ResourceTypeIndex = typeIndex,
                Capacity = math.max(0f, maxCapacity)
            });
        }

        private static void ApplyInventorySummary(ref FixedList64Bytes<StorehouseRegistryCapacitySummary> summaries, ushort typeIndex, float stored, float reserved, byte tierId, ushort averageQuality)
        {
            var clampedStored = math.max(0f, stored);
            var clampedReserved = math.max(0f, reserved);
            for (var i = 0; i < summaries.Length; i++)
            {
                var summary = summaries[i];
                if (summary.ResourceTypeIndex == typeIndex)
                {
                    summary.Stored = clampedStored;
                    summary.Reserved = clampedReserved;
                    summary.TierId = tierId;
                    summary.AverageQuality = averageQuality;
                    summaries[i] = summary;
                    return;
                }
            }

            TryAddSummary(ref summaries, new StorehouseRegistryCapacitySummary
            {
                ResourceTypeIndex = typeIndex,
                Stored = clampedStored,
                Reserved = clampedReserved,
                TierId = tierId,
                AverageQuality = averageQuality
            });
        }

        private static void ApplyReservation(ref FixedList64Bytes<StorehouseRegistryCapacitySummary> summaries, ushort resourceTypeIndex, float reserved)
        {
            if (resourceTypeIndex == ushort.MaxValue)
            {
                return;
            }

            for (var i = 0; i < summaries.Length; i++)
            {
                var summary = summaries[i];
                if (summary.ResourceTypeIndex == resourceTypeIndex)
                {
                    summary.Reserved += math.max(0f, reserved);
                    summaries[i] = summary;
                    return;
                }
            }

            TryAddSummary(ref summaries, new StorehouseRegistryCapacitySummary
            {
                ResourceTypeIndex = resourceTypeIndex,
                Reserved = math.max(0f, reserved)
            });
        }

        private static void TryAddSummary(ref FixedList64Bytes<StorehouseRegistryCapacitySummary> summaries, StorehouseRegistryCapacitySummary summary)
        {
            if (summaries.Length < summaries.Capacity)
            {
                summaries.Add(summary);
            }
        }

        private void ClearRegistry(ref SystemState state, Entity registryEntity, uint tick)
        {
            var entityManager = state.EntityManager;
            var entries = entityManager.GetBuffer<StorehouseRegistryEntry>(registryEntity);
            entries.Clear();

            var metadata = entityManager.GetComponentData<RegistryMetadata>(registryEntity);
            metadata.MarkUpdated(0, tick);
            entityManager.SetComponentData(registryEntity, metadata);

            var registryValue = entityManager.GetComponentData<StorehouseRegistry>(registryEntity);
            registryValue = default;
            registryValue.LastUpdateTick = tick;
            entityManager.SetComponentData(registryEntity, registryValue);
        }
    }
}

