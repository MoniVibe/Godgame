using Godgame.Resources;
using PureDOTS.Runtime.Components;
using PureDOTS.Runtime.Registry;
using PureDOTS.Runtime.Resource;
using PureDOTS.Runtime.Telemetry;
using PureDOTS.Systems;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace Godgame.Construction
{
    /// <summary>
    /// Handles construction ghost resource payment and conversion to built entities.
    /// Villagers pay via tickets (withdraw from storehouse) until Paid >= Cost.
    /// </summary>
    [BurstCompile]
    [UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
    [UpdateAfter(typeof(JobsitePlacementSystem))]
    public partial struct ConstructionSystem : ISystem
    {
        private ComponentLookup<StorehouseInventory> _storehouseLookup;
        private BufferLookup<StorehouseInventoryItem> _inventoryLookup;
        private BufferLookup<StorehouseCapacityElement> _capacityLookup;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<ConstructionGhost>();
            state.RequireForUpdate<ResourceTypeIndex>();
            state.RequireForUpdate<TimeState>();
            state.RequireForUpdate<RewindState>();
            _storehouseLookup = state.GetComponentLookup<StorehouseInventory>(isReadOnly: true);
            _inventoryLookup = state.GetBufferLookup<StorehouseInventoryItem>(false);
            _capacityLookup = state.GetBufferLookup<StorehouseCapacityElement>(isReadOnly: true);
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

            _storehouseLookup.Update(ref state);
            _inventoryLookup.Update(ref state);
            _capacityLookup.Update(ref state);

            var catalog = SystemAPI.GetSingleton<ResourceTypeIndex>().Catalog;
            if (!catalog.IsCreated)
            {
                return;
            }

            var ecb = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>()
                .CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter();

            var completedCount = new NativeArray<int>(1, Allocator.TempJob);

            new ConstructionPaymentJob
            {
                Catalog = catalog,
                InventoryLookup = _inventoryLookup,
                CapacityLookup = _capacityLookup,
                Ecb = ecb,
                CompletedCount = completedCount
            }.ScheduleParallel();

            state.Dependency.Complete();

            // Sequential pass: process payments from storehouses
            // This must be sequential because we need to iterate all storehouses for each ghost
            foreach (var (ghost, entity) in SystemAPI.Query<RefRW<ConstructionGhost>>().WithEntityAccess())
            {
                if (ghost.ValueRO.Paid >= ghost.ValueRO.Cost)
                {
                    continue; // Already paid, will be handled by job
                }

                var remainingCost = ghost.ValueRO.Cost - ghost.ValueRO.Paid;
                if (remainingCost > 0)
                {
                    // Find first storehouse with resources
                    foreach (var (storehouseInventory, storehouseEntity) in SystemAPI
                                 .Query<RefRO<StorehouseInventory>>()
                                 .WithEntityAccess())
                    {
                        if (!_inventoryLookup.HasBuffer(storehouseEntity) || !_capacityLookup.HasBuffer(storehouseEntity))
                        {
                            continue;
                        }

                        var inventory = _inventoryLookup[storehouseEntity];
                        var withdrawn = Godgame.Resources.StorehouseApi.Withdraw(ref inventory, catalog, ghost.ValueRO.ResourceTypeIndex, remainingCost);
                        if (withdrawn > 0f)
                        {
                            ghost.ValueRW.Paid += (int)math.floor(withdrawn);
                            break; // Paid from this storehouse
                        }
                    }
                }

                // Check if fully paid after payment attempt
                if (ghost.ValueRO.Paid >= ghost.ValueRO.Cost)
                {
                    ecb.AddComponent<JobsiteCompletionTag>(0, entity);
                    ecb.RemoveComponent<ConstructionGhost>(0, entity);
                    completedCount[0]++;
                }
            }

            state.Dependency.Complete();

            // Emit telemetry
            var telemetryEntity = SystemAPI.TryGetSingletonEntity<TelemetryStream>(out var telemetryStreamEntity)
                ? telemetryStreamEntity
                : Entity.Null;
            if (telemetryEntity != Entity.Null && state.EntityManager.HasBuffer<TelemetryMetric>(telemetryEntity))
            {
                var telemetryBuffer = state.EntityManager.GetBuffer<TelemetryMetric>(telemetryEntity);
                var count = completedCount[0];
                if (count > 0)
                {
                    var key = MakeTelemetryKey();
                    UpsertTelemetry(ref telemetryBuffer, key, count);
                }
            }

            completedCount.Dispose();
        }

        [BurstCompile]
        public partial struct ConstructionPaymentJob : IJobEntity
        {
            [ReadOnly] public BlobAssetReference<ResourceTypeIndexBlob> Catalog;
            public BufferLookup<StorehouseInventoryItem> InventoryLookup;
            [ReadOnly] public BufferLookup<StorehouseCapacityElement> CapacityLookup;
            public EntityCommandBuffer.ParallelWriter Ecb;
            public NativeArray<int> CompletedCount;

            [BurstCompile]
            void Execute([ChunkIndexInQuery] int ciq, Entity e, ref ConstructionGhost ghost)
            {
                // Skip if already paid
                if (ghost.Paid >= ghost.Cost)
                {
                    Ecb.AddComponent<JobsiteCompletionTag>(ciq, e);
                    Ecb.RemoveComponent<ConstructionGhost>(ciq, e);
                    Increment(ref CompletedCount);
                    return;
                }

                // Payment logic will be handled in a sequential pass after this job
                // This job only marks completed ghosts for conversion
                if (ghost.Paid >= ghost.Cost)
                {
                    Ecb.AddComponent<JobsiteCompletionTag>(ciq, e);
                    Ecb.RemoveComponent<ConstructionGhost>(ciq, e);
                    Increment(ref CompletedCount);
                }
            }

            private static void Increment(ref NativeArray<int> counter)
            {
                var value = counter[0];
                counter[0] = value + 1;
            }
        }

        private static void UpsertTelemetry(ref DynamicBuffer<TelemetryMetric> buffer, in FixedString64Bytes key, float value)
        {
            for (int i = 0; i < buffer.Length; i++)
            {
                if (buffer[i].Key.Equals(key))
                {
                    buffer[i] = new TelemetryMetric
                    {
                        Key = key,
                        Value = buffer[i].Value + value,
                        Unit = TelemetryMetricUnit.Count
                    };
                    return;
                }
            }

            buffer.Add(new TelemetryMetric
            {
                Key = key,
                Value = value,
                Unit = TelemetryMetricUnit.Count
            });
        }

        private static FixedString64Bytes MakeTelemetryKey()
        {
            FixedString64Bytes key = default;
            key.Append('c');
            key.Append('o');
            key.Append('n');
            key.Append('s');
            key.Append('t');
            key.Append('r');
            key.Append('u');
            key.Append('c');
            key.Append('t');
            key.Append('i');
            key.Append('o');
            key.Append('n');
            key.Append('.');
            key.Append('c');
            key.Append('o');
            key.Append('m');
            key.Append('p');
            key.Append('l');
            key.Append('e');
            key.Append('t');
            key.Append('e');
            key.Append('d');
            return key;
        }
    }
}

