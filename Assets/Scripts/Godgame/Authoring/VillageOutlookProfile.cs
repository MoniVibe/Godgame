using System;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace Godgame.Authoring
{
    /// <summary>
    /// ScriptableObject defining village outlook profiles (cultural/behavioral expressions).
    /// Baked into blob asset for runtime PureDOTS systems.
    /// </summary>
    [CreateAssetMenu(fileName = "VillageOutlookProfile", menuName = "Godgame/Village Outlook Profile")]
    public sealed class VillageOutlookProfile : ScriptableObject
    {
        [Serializable]
        public struct AxisBlend
        {
            [Tooltip("Alignment axis ID to blend")]
            public string AxisId;

            [Tooltip("Blend value (-1 to +1)")]
            [Range(-1f, 1f)]
            public float BlendValue;
        }

        [Serializable]
        public enum InitiativeBandType
        {
            Slow,
            Measured,
            Bold,
            Reckless
        }

        [Serializable]
        public enum GovernanceTemplate
        {
            Council,
            Meritocratic,
            Theocratic,
            Oligarchic,
            Authoritarian
        }

        [Tooltip("Profile name/identifier")]
        public string ProfileName;

        [Tooltip("Axis blend values mapping to alignment catalog")]
        public AxisBlend[] AxisBlends = Array.Empty<AxisBlend>();

        [Tooltip("Default initiative band for new settlements")]
        public InitiativeBandType DefaultInitiativeBand = InitiativeBandType.Measured;

        [Tooltip("Governance template")]
        public GovernanceTemplate Governance = GovernanceTemplate.Council;

        [Tooltip("Worship modifier curve (faith gain/decay multipliers)")]
        public AnimationCurve WorshipModifier = AnimationCurve.Linear(0f, 1f, 1f, 1f);

    }

    /// <summary>
    /// Runtime blob asset containing outlook profile data.
    /// </summary>
    public struct VillageOutlookProfileBlob
    {
        public FixedString64Bytes ProfileName;
        public BlobArray<AxisBlendBlob> AxisBlends;
        public byte DefaultInitiativeBand;
        public byte Governance;
    }

    public struct AxisBlendBlob
    {
        public FixedString64Bytes AxisId;
        public float BlendValue;
    }

    /// <summary>
    /// Component storing reference to outlook profile blob asset.
    /// </summary>
    public struct VillageOutlookProfileBlobComponent : IComponentData
    {
        public BlobAssetReference<VillageOutlookProfileBlob> Profile;
    }
}

