using PureDOTS.Runtime.Components;
using Unity.Collections;
using Unity.Entities;

namespace Godgame.Resources
{
    /// <summary>
    /// Simplified storehouse component wrapper for API usage.
    /// Bridges to PureDOTS StorehouseInventory component.
    /// </summary>
    public struct Storehouse : IComponentData
    {
        public int Capacity;
    }

    /// <summary>
    /// Simplified inventory item using resource type indices.
    /// Bridges to PureDOTS StorehouseInventoryItem buffer.
    /// </summary>
    public struct InventoryItem : IBufferElementData
    {
        public ushort ResourceTypeIndex;
        public int Count;
    }
}

