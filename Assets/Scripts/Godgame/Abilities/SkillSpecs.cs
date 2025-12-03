using Unity.Collections;
using Unity.Entities;

namespace Godgame.Abilities
{
    /// <summary>
    /// Skill tags for categorization and tree organization.
    /// </summary>
    [System.Flags]
    public enum SkillTags : uint
    {
        None = 0,
        Combat = 1 << 0,
        Magic = 1 << 1,
        Crafting = 1 << 2,
        Social = 1 << 3,
        Survival = 1 << 4
    }

    /// <summary>
    /// Skill specification blob data (runtime, Burst-compatible).
    /// </summary>
    public struct SkillSpecBlob
    {
        public FixedString64Bytes Id;
        public byte Passive; // 0 = active, 1 = passive
        public float StatDeltaPower;
        public float StatDeltaSpeed;
        public float StatDeltaDefense;
        public BlobArray<FixedString64Bytes> GrantsSpellIds; // new actives
        public BlobArray<FixedString64Bytes> Requires; // prereq skills
        public byte Tier;
        public uint Tags; // tree/category
    }

    /// <summary>
    /// In-memory catalog definition (ScriptableObject holds authoring data).
    /// </summary>
    public class SkillSpecCatalog : UnityEngine.ScriptableObject
    {
        public SkillSpecDefinition[] Skills;

        [System.Serializable]
        public struct SkillSpecDefinition
        {
            public string Id;
            public bool Passive;
            public float StatDeltaPower;
            public float StatDeltaSpeed;
            public float StatDeltaDefense;
            public string[] GrantsSpellIds;
            public string[] Requires;
            public byte Tier;
            public SkillTags Tags;
        }
    }

    /// <summary>
    /// Catalog blob containing all skill specs.
    /// </summary>
    public struct SkillSpecCatalogBlob
    {
        public BlobArray<SkillSpecBlob> Skills;
    }

    /// <summary>
    /// Component storing reference to skill spec catalog blob asset.
    /// </summary>
    public struct SkillSpecCatalogBlobRef : IComponentData
    {
        public BlobAssetReference<SkillSpecCatalogBlob> Catalog;
    }
}

