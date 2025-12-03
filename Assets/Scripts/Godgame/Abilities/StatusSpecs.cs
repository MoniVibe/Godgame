using Unity.Collections;
using Unity.Entities;

namespace Godgame.Abilities
{
    /// <summary>
    /// Dispel tags for status effect removal rules.
    /// </summary>
    [System.Flags]
    public enum DispelTags : uint
    {
        None = 0,
        Magic = 1 << 0,
        Disease = 1 << 1,
        Poison = 1 << 2,
        Curse = 1 << 3,
        Blessing = 1 << 4
    }

    /// <summary>
    /// Status specification blob data (runtime, Burst-compatible).
    /// </summary>
    public struct StatusSpecBlob
    {
        public FixedString64Bytes Id;
        public byte MaxStacks;
        public byte Dispellable; // 0 = false, 1 = true
        public uint DispelTags;
        public float Duration;
        public float Period; // periodic tick interval (0 = no periodic)
    }

    /// <summary>
    /// In-memory catalog definition (ScriptableObject holds authoring data).
    /// </summary>
    public class StatusSpecCatalog : UnityEngine.ScriptableObject
    {
        public StatusSpecDefinition[] Statuses;

        [System.Serializable]
        public struct StatusSpecDefinition
        {
            public string Id;
            public byte MaxStacks;
            public bool Dispellable;
            public DispelTags DispelTags;
            public float Duration;
            public float Period;
        }
    }

    /// <summary>
    /// Catalog blob containing all status specs.
    /// </summary>
    public struct StatusSpecCatalogBlob
    {
        public BlobArray<StatusSpecBlob> Statuses;
    }

    /// <summary>
    /// Component storing reference to status spec catalog blob asset.
    /// </summary>
    public struct StatusSpecCatalogBlobRef : IComponentData
    {
        public BlobAssetReference<StatusSpecCatalogBlob> Catalog;
    }
}

