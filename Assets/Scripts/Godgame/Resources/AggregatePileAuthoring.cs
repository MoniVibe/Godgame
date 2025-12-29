using PureDOTS.Runtime.Components;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Godgame.Resources
{
    /// <summary>
    /// Authoring component for aggregate pile system configuration.
    /// Place on a single GameObject in your scene.
    /// </summary>
    public class AggregatePileConfigAuthoring : MonoBehaviour
    {
        [Header("Merge Settings")]
        [Tooltip("Radius within which piles of same type should merge")]
        public float mergeRadius = 2.5f;

        [Tooltip("How often to check for merges (seconds)")]
        public float mergeCheckInterval = 5f;

        [Header("Capacity Settings")]
        [Tooltip("Maximum pile capacity before split")]
        public float maxPileCapacity = 2500f;

        [Tooltip("Maximum total piles in world")]
        public int maxTotalPiles = 200;

        private sealed class AggregatePileConfigBaker : Baker<AggregatePileConfigAuthoring>
        {
            public override void Bake(AggregatePileConfigAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.None);

                AddComponent(entity, new AggregatePileConfig
                {
                    MergeRadius = math.max(0.5f, authoring.mergeRadius),
                    GlobalMaxCapacity = math.max(100f, authoring.maxPileCapacity),
                    MaxActivePiles = math.max(10, authoring.maxTotalPiles),
                    MergeCheckSeconds = math.max(1f, authoring.mergeCheckInterval),
                    DefaultMaxCapacity = math.max(100f, authoring.maxPileCapacity),
                    SplitThreshold = 0.8f,
                    MinSpawnAmount = 10f,
                    ConservationEpsilon = 0.01f
                });

                AddComponent(entity, new AggregatePileStats
                {
                    TotalPiles = 0,
                    TotalResourceAmount = 0f,
                    LastMergeCheckTick = 0
                });
            }
        }
    }

    /// <summary>
    /// Authoring component for placing a pile directly in the scene.
    /// Use for pre-placed resource piles.
    /// </summary>
    public class AggregatePileAuthoring : MonoBehaviour
    {
        [Header("Resource")]
        [Tooltip("Resource type identifier")]
        public string resourceTypeId = "wood";

        [Tooltip("Resource type index (for fast lookup)")]
        public ushort resourceTypeIndex = 0;

        [Header("Amount")]
        [Tooltip("Initial amount in pile")]
        public float initialAmount = 100f;

        [Tooltip("Maximum capacity")]
        public float maxCapacity = 2500f;

        private sealed class AggregatePileBaker : Baker<AggregatePileAuthoring>
        {
            public override void Bake(AggregatePileAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);

                var pile = new AggregatePile
                {
                    ResourceTypeId = new Unity.Collections.FixedString64Bytes(authoring.resourceTypeId),
                    ResourceTypeIndex = authoring.resourceTypeIndex,
                    Amount = math.max(0f, authoring.initialAmount),
                    MaxCapacity = math.max(100f, authoring.maxCapacity),
                    State = PileState.Stable,
                    LastModifiedTick = 0
                };
                pile.UpdateVisualSize();

                AddComponent(entity, pile);
                AddComponent(entity, new AggregatePileTag());
                AddComponent(entity, new SiphonSource
                {
                    ResourceTypeIndex = authoring.resourceTypeIndex,
                    Amount = pile.Amount,
                    MinChunkSize = 1f,
                    SiphonResistance = 0f
                });
            }
        }
    }
}
