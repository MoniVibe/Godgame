using StorehouseCapacityElement = PureDOTS.Runtime.Components.StorehouseCapacityElement;
using StorehouseInventoryItem = PureDOTS.Runtime.Components.StorehouseInventoryItem;
using ResourceQualityTier = PureDOTS.Runtime.Resource.ResourceQualityTier;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace Godgame.Economy
{
    /// <summary>
    /// Clean API wrapper for storehouse inventory operations.
    /// Provides Add/Remove/Space methods per the Storehouse_API.md spec.
    /// 
    /// Usage:
    ///   var accepted = StorehouseAPI.Add(ref inventoryBuffer, ref capacityBuffer, resourceTypeId, amount);
    ///   var removed = StorehouseAPI.Remove(ref inventoryBuffer, resourceTypeId, amount);
    ///   var space = StorehouseAPI.Space(in inventoryBuffer, in capacityBuffer, resourceTypeId);
    /// </summary>
    public static class StorehouseAPI
    {
        /// <summary>
        /// Add resources to storehouse inventory.
        /// </summary>
        /// <param name="inventory">Inventory buffer</param>
        /// <param name="capacity">Capacity buffer</param>
        /// <param name="resourceTypeId">Resource type identifier</param>
        /// <param name="amount">Amount to add</param>
        /// <returns>Amount actually accepted (may be less if capacity limited)</returns>
        public static float Add(
            ref DynamicBuffer<StorehouseInventoryItem> inventory,
            in DynamicBuffer<StorehouseCapacityElement> capacity,
            in FixedString64Bytes resourceTypeId,
            float amount)
        {
            if (amount <= 0)
                return 0f;

            // Find capacity for this resource type
            float maxCapacity = 0f;
            bool typeAllowed = false;

            for (int i = 0; i < capacity.Length; i++)
            {
                if (capacity[i].ResourceTypeId.Equals(resourceTypeId))
                {
                    maxCapacity = capacity[i].MaxCapacity;
                    typeAllowed = true;
                    break;
                }
            }

            // Type validation - reject unknown types
            if (!typeAllowed)
                return 0f;

            // Find existing inventory entry
            int existingIndex = -1;
            float currentStored = 0f;

            for (int i = 0; i < inventory.Length; i++)
            {
                if (inventory[i].ResourceTypeId.Equals(resourceTypeId))
                {
                    existingIndex = i;
                    currentStored = inventory[i].Amount;
                    break;
                }
            }

            // Calculate available space
            float availableSpace = math.max(0f, maxCapacity - currentStored);
            float acceptedAmount = math.min(amount, availableSpace);

            if (acceptedAmount <= 0)
                return 0f;

            // Update or create inventory entry
            if (existingIndex >= 0)
            {
                var item = inventory[existingIndex];
                item.Amount += acceptedAmount;
                inventory[existingIndex] = item;
            }
            else
            {
                inventory.Add(new StorehouseInventoryItem
                {
                    ResourceTypeId = resourceTypeId,
                    Amount = acceptedAmount,
                    Reserved = 0f,
                    TierId = (byte)ResourceQualityTier.Common,
                    AverageQuality = 50
                });
            }

            return acceptedAmount;
        }

        /// <summary>
        /// Remove resources from storehouse inventory.
        /// </summary>
        /// <param name="inventory">Inventory buffer</param>
        /// <param name="resourceTypeId">Resource type identifier</param>
        /// <param name="amount">Amount to remove</param>
        /// <returns>Amount actually removed (may be less if insufficient)</returns>
        public static float Remove(
            ref DynamicBuffer<StorehouseInventoryItem> inventory,
            in FixedString64Bytes resourceTypeId,
            float amount)
        {
            if (amount <= 0)
                return 0f;

            // Find inventory entry
            for (int i = 0; i < inventory.Length; i++)
            {
                if (inventory[i].ResourceTypeId.Equals(resourceTypeId))
                {
                    var item = inventory[i];
                    float available = math.max(0f, item.Amount - item.Reserved);
                    float removedAmount = math.min(amount, available);

                    if (removedAmount > 0)
                    {
                        item.Amount -= removedAmount;
                        inventory[i] = item;
                    }

                    return removedAmount;
                }
            }

            return 0f;
        }

        /// <summary>
        /// Query available space for a resource type.
        /// </summary>
        /// <param name="inventory">Inventory buffer</param>
        /// <param name="capacity">Capacity buffer</param>
        /// <param name="resourceTypeId">Resource type identifier</param>
        /// <returns>Available space (capacity - stored - reserved)</returns>
        public static float Space(
            in DynamicBuffer<StorehouseInventoryItem> inventory,
            in DynamicBuffer<StorehouseCapacityElement> capacity,
            in FixedString64Bytes resourceTypeId)
        {
            // Find capacity
            float maxCapacity = 0f;
            for (int i = 0; i < capacity.Length; i++)
            {
                if (capacity[i].ResourceTypeId.Equals(resourceTypeId))
                {
                    maxCapacity = capacity[i].MaxCapacity;
                    break;
                }
            }

            if (maxCapacity <= 0)
                return 0f;

            // Find current stored + reserved
            float stored = 0f;
            float reserved = 0f;

            for (int i = 0; i < inventory.Length; i++)
            {
                if (inventory[i].ResourceTypeId.Equals(resourceTypeId))
                {
                    stored = inventory[i].Amount;
                    reserved = inventory[i].Reserved;
                    break;
                }
            }

            return math.max(0f, maxCapacity - stored - reserved);
        }

        /// <summary>
        /// Get the current amount stored of a resource type.
        /// </summary>
        public static float GetStored(
            in DynamicBuffer<StorehouseInventoryItem> inventory,
            in FixedString64Bytes resourceTypeId)
        {
            for (int i = 0; i < inventory.Length; i++)
            {
                if (inventory[i].ResourceTypeId.Equals(resourceTypeId))
                {
                    return inventory[i].Amount;
                }
            }
            return 0f;
        }

        /// <summary>
        /// Get the capacity for a resource type.
        /// </summary>
        public static float GetCapacity(
            in DynamicBuffer<StorehouseCapacityElement> capacity,
            in FixedString64Bytes resourceTypeId)
        {
            for (int i = 0; i < capacity.Length; i++)
            {
                if (capacity[i].ResourceTypeId.Equals(resourceTypeId))
                {
                    return capacity[i].MaxCapacity;
                }
            }
            return 0f;
        }

        /// <summary>
        /// Reserve resources for pickup (prevents double-allocation).
        /// </summary>
        public static bool Reserve(
            ref DynamicBuffer<StorehouseInventoryItem> inventory,
            in FixedString64Bytes resourceTypeId,
            float amount)
        {
            if (amount <= 0)
                return false;

            for (int i = 0; i < inventory.Length; i++)
            {
                if (inventory[i].ResourceTypeId.Equals(resourceTypeId))
                {
                    var item = inventory[i];
                    float available = item.Amount - item.Reserved;

                    if (available >= amount)
                    {
                        item.Reserved += amount;
                        inventory[i] = item;
                        return true;
                    }
                    return false;
                }
            }
            return false;
        }

        /// <summary>
        /// Cancel a reservation (unreserve resources).
        /// </summary>
        public static void CancelReservation(
            ref DynamicBuffer<StorehouseInventoryItem> inventory,
            in FixedString64Bytes resourceTypeId,
            float amount)
        {
            if (amount <= 0)
                return;

            for (int i = 0; i < inventory.Length; i++)
            {
                if (inventory[i].ResourceTypeId.Equals(resourceTypeId))
                {
                    var item = inventory[i];
                    item.Reserved = math.max(0f, item.Reserved - amount);
                    inventory[i] = item;
                    return;
                }
            }
        }

        /// <summary>
        /// Check if storehouse has at least the specified amount available (unreserved).
        /// </summary>
        public static bool HasAvailable(
            in DynamicBuffer<StorehouseInventoryItem> inventory,
            in FixedString64Bytes resourceTypeId,
            float amount)
        {
            for (int i = 0; i < inventory.Length; i++)
            {
                if (inventory[i].ResourceTypeId.Equals(resourceTypeId))
                {
                    float available = inventory[i].Amount - inventory[i].Reserved;
                    return available >= amount;
                }
            }
            return false;
        }
    }
}

