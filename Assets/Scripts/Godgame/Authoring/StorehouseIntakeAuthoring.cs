using System;
using System.Collections.Generic;
using Unity.Collections;
using PureDOTS.Runtime.Components;
using PureDOTS.Runtime.Spatial;
using PureDOTS.Runtime.Resource;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace Godgame.Authoring
{
    /// <summary>
    /// Configures a storehouse with capacity and initial inventory so DOTS storehouse systems can run in scenes.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class StorehouseIntakeAuthoring : MonoBehaviour
    {
        [Serializable]
        public sealed class ResourceEntry
        {
            public string resourceId = "wood";
            public float capacity = 100f;
            public float stored = 0f;
            public float reserved = 0f;
        }

        [Header("Inventory")]
        public List<ResourceEntry> resources = new();

        [Header("Rates")]
        public float inputRate = 25f;
        public float outputRate = 25f;
        public float shredRate = 0f;
        public int maxShredQueueSize = 4;

        private sealed class Baker : Unity.Entities.Baker<StorehouseIntakeAuthoring>
        {
            public override void Bake(StorehouseIntakeAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent<SpatialIndexedTag>(entity);
                AddComponent(entity, LocalTransform.FromPositionRotationScale(authoring.transform.position, authoring.transform.rotation, 1f));

                AddComponent(entity, new StorehouseConfig
                {
                    ShredRate = math.max(0f, authoring.shredRate),
                    MaxShredQueueSize = math.max(0, authoring.maxShredQueueSize),
                    InputRate = math.max(0f, authoring.inputRate),
                    OutputRate = math.max(0f, authoring.outputRate)
                });

                AddComponent<DumpTargetStorehouse>(entity);

                var inventory = new StorehouseInventory
                {
                    TotalCapacity = 0f,
                    TotalStored = 0f,
                    ItemTypeCount = 0,
                    IsShredding = (byte)(authoring.shredRate > 0f ? 1 : 0),
                    LastUpdateTick = 0
                };

                var capacityBuffer = AddBuffer<StorehouseCapacityElement>(entity);
                var inventoryBuffer = AddBuffer<StorehouseInventoryItem>(entity);

                if (authoring.resources != null)
                {
                    foreach (var entry in authoring.resources)
                    {
                        if (entry == null)
                        {
                            continue;
                        }

                        var resourceId = string.IsNullOrWhiteSpace(entry.resourceId)
                            ? new FixedString64Bytes("unknown")
                            : new FixedString64Bytes(entry.resourceId.Trim().ToLowerInvariant());

                        if (entry.capacity > 0f)
                        {
                            capacityBuffer.Add(new StorehouseCapacityElement
                            {
                                ResourceTypeId = resourceId,
                                MaxCapacity = math.max(0f, entry.capacity)
                            });
                            inventory.TotalCapacity += math.max(0f, entry.capacity);
                        }

                        if (entry.stored > 0f)
                        {
                            inventoryBuffer.Add(new StorehouseInventoryItem
                            {
                                ResourceTypeId = resourceId,
                                Amount = math.max(0f, entry.stored),
                                Reserved = math.max(0f, entry.reserved)
                            });
                            inventory.TotalStored += math.max(0f, entry.stored);
                        }
                    }
                }

                inventory.ItemTypeCount = inventoryBuffer.Length;
                AddComponent(entity, inventory);
            }
        }
    }
}
