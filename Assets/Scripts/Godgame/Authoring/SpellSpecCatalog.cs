using System;
using Godgame.Abilities;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace Godgame.Authoring
{
    /// <summary>
    /// ScriptableObject catalog for spell specifications.
    /// Baked into blob asset for runtime PureDOTS systems.
    /// </summary>
    [CreateAssetMenu(fileName = "SpellSpecCatalog", menuName = "Godgame/Spell Spec Catalog")]
    public sealed class SpellSpecCatalog : ScriptableObject
    {
        [Serializable]
        public struct SpellSpecDefinition
        {
            [Tooltip("Unique spell identifier")]
            public string Id;

            [Tooltip("Target shape")]
            public TargetShape Shape;

            [Tooltip("Range in meters")]
            public float Range;

            [Tooltip("Radius for area effects")]
            public float Radius;

            [Tooltip("Cast time in seconds")]
            public float CastTime;

            [Tooltip("Cooldown in seconds")]
            public float Cooldown;

            [Tooltip("Resource cost (mana, energy, etc.)")]
            public float Cost;

            [Tooltip("GCD group (0 = no GCD)")]
            public byte GcdGroup;

            [Tooltip("Is this a channeled spell?")]
            public bool Channeled;

            [Tooltip("Effect operations (executed in order)")]
            public EffectOpDefinition[] Effects;

            [Tooltip("School name (e.g., Fire, Nature)")]
            public string School;

            [Tooltip("Spell tags (flags)")]
            public SpellTags Tags;
        }

        [Serializable]
        public struct EffectOpDefinition
        {
            [Tooltip("Effect kind")]
            public EffectOpKind Kind;

            [Tooltip("Base magnitude")]
            public float Magnitude;

            [Tooltip("Scaling stat (e.g., Power, Level)")]
            public float ScaleWith;

            [Tooltip("Duration in seconds (0 = instant)")]
            public float Duration;

            [Tooltip("Period for HoT/DoT (0 = no periodic)")]
            public float Period;

            [Tooltip("Optional status effect ID")]
            public uint StatusId;
        }

        [Tooltip("Spell definitions")]
        public SpellSpecDefinition[] Spells = Array.Empty<SpellSpecDefinition>();

    }
}

