using Unity.Collections;
using Unity.Entities;

namespace Godgame.Abilities
{
    /// <summary>
    /// Target shape enumeration for spell targeting.
    /// </summary>
    public enum TargetShape : byte
    {
        Self = 0,
        Unit = 1,
        Area = 2,
        Cone = 3,
        Line = 4,
        Chain = 5
    }

    /// <summary>
    /// Spell tags (flags) for categorization and filtering.
    /// </summary>
    [System.Flags]
    public enum SpellTags : uint
    {
        None = 0,
        Damage = 1 << 0,
        Heal = 1 << 1,
        CrowdControl = 1 << 2,
        Buff = 1 << 3,
        Debuff = 1 << 4,
        Movement = 1 << 5,
        Summon = 1 << 6,
        ResourceMod = 1 << 7
    }

    /// <summary>
    /// Effect operation kind for spell effects.
    /// </summary>
    public enum EffectOpKind : byte
    {
        Damage = 0,
        Heal = 1,
        Buff = 2,
        Debuff = 3,
        Summon = 4,
        Move = 5,
        ResourceMod = 6
    }

    /// <summary>
    /// Spell specification blob data (runtime, Burst-compatible).
    /// </summary>
    public struct SpellSpecBlob
    {
        public FixedString64Bytes Id;
        public TargetShape Shape;
        public float Range;
        public float Radius;
        public float CastTime;
        public float Cooldown;
        public float Cost;
        public byte GcdGroup;
        public byte Channeled; // 0 = false, 1 = true
        public BlobArray<EffectOpBlob> Effects;
        public FixedString32Bytes School;
        public uint Tags;
    }

    /// <summary>
    /// Effect operation blob data.
    /// </summary>
    public struct EffectOpBlob
    {
        public EffectOpKind Kind;
        public float Magnitude;
        public float ScaleWith; // e.g., Power or Level
        public float Duration;
        public float Period; // for HoT/DoT
        public uint StatusId; // optional link to StatusSpec
    }

    /// <summary>
    /// In-memory catalog definition (ScriptableObject holds authoring data).
    /// </summary>
    public class SpellSpecCatalog : UnityEngine.ScriptableObject
    {
        public SpellSpecDefinition[] Spells;

        [System.Serializable]
        public struct SpellSpecDefinition
        {
            public string Id;
            public TargetShape Shape;
            public float Range;
            public float Radius;
            public float CastTime;
            public float Cooldown;
            public float Cost;
            public byte GcdGroup;
            public bool Channeled;
            public EffectOpDefinition[] Effects;
            public string School;
            public SpellTags Tags;
        }

        [System.Serializable]
        public struct EffectOpDefinition
        {
            public EffectOpKind Kind;
            public float Magnitude;
            public float ScaleWith;
            public float Duration;
            public float Period;
            public uint StatusId;
        }
    }

    /// <summary>
    /// Catalog blob containing all spell specs.
    /// </summary>
    public struct SpellSpecCatalogBlob
    {
        public BlobArray<SpellSpecBlob> Spells;
    }

    /// <summary>
    /// Component storing reference to spell spec catalog blob asset.
    /// </summary>
    public struct SpellSpecCatalogBlobRef : IComponentData
    {
        public BlobAssetReference<SpellSpecCatalogBlob> Catalog;
    }
}

