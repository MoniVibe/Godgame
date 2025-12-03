using System;
using Godgame.Abilities;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace Godgame.Authoring
{
    /// <summary>
    /// ScriptableObject catalog for skill specifications.
    /// Baked into blob asset for runtime PureDOTS systems.
    /// </summary>
    [CreateAssetMenu(fileName = "SkillSpecCatalog", menuName = "Godgame/Skill Spec Catalog")]
    public sealed class SkillSpecCatalog : ScriptableObject
    {
        [Serializable]
        public struct SkillSpecDefinition
        {
            [Tooltip("Unique skill identifier")]
            public string Id;

            [Tooltip("Is this a passive skill?")]
            public bool Passive;

            [Tooltip("Power stat delta")]
            public float StatDeltaPower;

            [Tooltip("Speed stat delta")]
            public float StatDeltaSpeed;

            [Tooltip("Defense stat delta")]
            public float StatDeltaDefense;

            [Tooltip("Spell IDs granted by this skill")]
            public string[] GrantsSpellIds;

            [Tooltip("Prerequisite skill IDs")]
            public string[] Requires;

            [Tooltip("Skill tier")]
            public byte Tier;

            [Tooltip("Skill tags (flags)")]
            public SkillTags Tags;
        }

        [Tooltip("Skill definitions")]
        public SkillSpecDefinition[] Skills = Array.Empty<SkillSpecDefinition>();

    }
}

