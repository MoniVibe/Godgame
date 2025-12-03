using PureDOTS.Runtime.Components;
using PureDOTS.Runtime.Resource;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace Godgame.Resources
{
    /// <summary>
    /// API wrapper for storehouse deposit/withdraw operations using resource type indices.
    /// Bridges to PureDOTS StorehouseInventoryItem buffers via ResourceTypeIndexBlob catalog.
    /// </summary>
    public static class StorehouseApi
    {
        /// <summary>
        /// Resolves resource type index to resource ID using the catalog.
        /// </summary>
        public static FixedString64Bytes ResolveResourceId(BlobAssetReference<ResourceTypeIndexBlob> catalog, ushort resourceTypeIndex)
        {
            if (!catalog.IsCreated || resourceTypeIndex >= catalog.Value.Ids.Length)
            {
                return default;
            }

            return catalog.Value.Ids[resourceTypeIndex];
        }

        /// <summary>
        /// Attempts to deposit resources into a storehouse inventory buffer.
        /// </summary>
        /// <param name="inv">Inventory buffer (StorehouseInventoryItem)</param>
        /// <param name="catalog">Resource type catalog</param>
        /// <param name="res">Resource type index</param>
        /// <param name="amount">Amount to deposit</param>
        /// <param name="capacity">Total capacity for this resource type</param>
        /// <returns>True if deposit succeeded (or partially succeeded), false if no space</returns>
        public static bool TryDeposit(ref DynamicBuffer<StorehouseInventoryItem> inv, BlobAssetReference<ResourceTypeIndexBlob> catalog, ushort res, float amount, float capacity)
        {
            if (amount <= 0f || capacity <= 0f)
            {
                return false;
            }

            var resourceId = ResolveResourceId(catalog, res);
            if (resourceId.Length == 0)
            {
                return false;
            }

            // Find existing item for this resource type
            int existingIndex = -1;
            float stored = 0f;
            for (int i = 0; i < inv.Length; i++)
            {
                if (inv[i].ResourceTypeId.Equals(resourceId))
                {
                    existingIndex = i;
                    stored = inv[i].Amount;
                    break;
                }
            }

            // Check capacity
            if (stored + amount > capacity)
            {
                // Partial deposit
                var available = math.max(0f, capacity - stored);
                if (available <= 0f)
                {
                    return false;
                }

                amount = available;
            }

            // Add or update inventory item
            if (existingIndex >= 0)
            {
                var item = inv[existingIndex];
                item.Amount += amount;
                inv[existingIndex] = item;
            }
            else
            {
                inv.Add(new StorehouseInventoryItem
                {
                    ResourceTypeId = resourceId,
                    Amount = amount,
                    Reserved = 0f
                });
            }

            return true;
        }

        /// <summary>
        /// Withdraws resources from a storehouse inventory buffer.
        /// </summary>
        /// <param name="inv">Inventory buffer</param>
        /// <param name="catalog">Resource type catalog</param>
        /// <param name="res">Resource type index</param>
        /// <param name="want">Desired amount to withdraw</param>
        /// <returns>Actual amount withdrawn</returns>
        public static float Withdraw(ref DynamicBuffer<StorehouseInventoryItem> inv, BlobAssetReference<ResourceTypeIndexBlob> catalog, ushort res, float want)
        {
            if (want <= 0f)
            {
                return 0f;
            }

            var resourceId = ResolveResourceId(catalog, res);
            if (resourceId.Length == 0)
            {
                return 0f;
            }

            // Find matching item by resource type
            for (int i = 0; i < inv.Length; i++)
            {
                var item = inv[i];
                if (item.ResourceTypeId.Equals(resourceId))
                {
                    var take = math.min(item.Amount, want);
                    item.Amount -= take;
                    if (item.Amount <= 0f)
                    {
                        inv.RemoveAt(i);
                    }
                    else
                    {
                        inv[i] = item;
                    }
                    return take;
                }
            }

            return 0f;
        }

        /// <summary>
        /// Gets available space for a resource type.
        /// </summary>
        /// <param name="inv">Inventory buffer</param>
        /// <param name="capacities">Capacity buffer</param>
        /// <param name="catalog">Resource type catalog</param>
        /// <param name="res">Resource type index</param>
        /// <returns>Available space (capacity - stored - reserved)</returns>
        public static float Space(ref DynamicBuffer<StorehouseInventoryItem> inv, ref DynamicBuffer<StorehouseCapacityElement> capacities, BlobAssetReference<ResourceTypeIndexBlob> catalog, ushort res)
        {
            var resourceId = ResolveResourceId(catalog, res);
            if (resourceId.Length == 0)
            {
                return 0f;
            }

            // Find capacity for this resource type
            float capacity = 0f;
            for (int i = 0; i < capacities.Length; i++)
            {
                if (capacities[i].ResourceTypeId.Equals(resourceId))
                {
                    capacity = capacities[i].MaxCapacity;
                    break;
                }
            }

            // Find stored amount
            float stored = 0f;
            float reserved = 0f;
            for (int i = 0; i < inv.Length; i++)
            {
                if (inv[i].ResourceTypeId.Equals(resourceId))
                {
                    stored = inv[i].Amount;
                    reserved = inv[i].Reserved;
                    break;
                }
            }

            return math.max(0f, capacity - stored - reserved);
        }
    }
}

