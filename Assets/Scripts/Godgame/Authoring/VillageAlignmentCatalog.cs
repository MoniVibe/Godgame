using System;
using Godgame.Villages;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Godgame.Authoring
{
    /// <summary>
    /// ScriptableObject catalog defining alignment axes (Moral, Order, Purity) and their value curves.
    /// Baked into blob asset for runtime PureDOTS systems.
    /// </summary>
    [CreateAssetMenu(fileName = "VillageAlignmentCatalog", menuName = "Godgame/Village Alignment Catalog")]
    public sealed class VillageAlignmentCatalog : ScriptableObject
    {
        [Serializable]
        public struct AlignmentAxisDefinition
        {
            [Tooltip("Unique identifier for this axis (e.g., 'OrderChaos', 'IsolationExpansion')")]
            public string AxisId;

            [Tooltip("Value curve mapping raw alignment values to normalized behavior tiers")]
            public AnimationCurve ValueCurve;

            [Tooltip("Initiative response: min/max bias and easing function")]
            public InitiativeResponse InitiativeResponse;

            [Tooltip("Surplus priority weights: build, defend, proselytize, research, migrate")]
            public SurplusPriorityWeights SurplusWeights;
        }

        [Serializable]
        public struct InitiativeResponse
        {
            [Tooltip("Minimum initiative bias (-1 to +1)")]
            [Range(-1f, 1f)]
            public float MinBias;

            [Tooltip("Maximum initiative bias (-1 to +1)")]
            [Range(-1f, 1f)]
            public float MaxBias;

            [Tooltip("Easing function type")]
            public EasingType EasingType;
        }

        [Serializable]
        public struct SurplusPriorityWeights
        {
            [Range(0f, 1f)]
            public float Build;

            [Range(0f, 1f)]
            public float Defend;

            [Range(0f, 1f)]
            public float Proselytize;

            [Range(0f, 1f)]
            public float Research;

            [Range(0f, 1f)]
            public float Migrate;
        }

        public enum EasingType
        {
            Linear,
            EaseIn,
            EaseOut,
            EaseInOut
        }

        [Tooltip("Alignment axis definitions")]
        public AlignmentAxisDefinition[] Axes = Array.Empty<AlignmentAxisDefinition>();

    }

}
