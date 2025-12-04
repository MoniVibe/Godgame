using Unity.Entities;
using UnityEngine;

namespace Godgame.Villagers
{
    /// <summary>
    /// Authoring component for villager behavioral personality traits.
    /// Configures the Bold/Vengeful axes that determine how villagers respond emotionally.
    /// </summary>
    public class VillagerBehaviorAuthoring : MonoBehaviour
    {
        [Header("Behavioral Axes")]
        [Tooltip("Response to harm/betrayal. Negative = forgiving, Positive = vengeful")]
        [Range(-100f, 100f)]
        public float VengefulScore = 0f;

        [Tooltip("Response to danger/risk. Negative = craven, Positive = bold")]
        [Range(-100f, 100f)]
        public float BoldScore = 0f;

        [Header("Randomization")]
        [Tooltip("If true, randomize traits within the specified range on spawn")]
        public bool RandomizeOnSpawn = false;

        [Tooltip("Range for randomization (traits will be ±this value from 0)")]
        [Range(0f, 100f)]
        public float RandomizationRange = 60f;

        [Header("Combat Behavior")]
        [Tooltip("If true, also add VillagerCombatBehavior component")]
        public bool IncludeCombatBehavior = true;

        public class Baker : Baker<VillagerBehaviorAuthoring>
        {
            public override void Bake(VillagerBehaviorAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.None);

                var behavior = new VillagerBehavior
                {
                    VengefulScore = authoring.VengefulScore,
                    BoldScore = authoring.BoldScore,
                    InitiativeModifier = 0f,
                    ActiveGrudgeCount = 0,
                    LastMajorActionTick = 0
                };

                // Recalculate initiative based on traits
                behavior.RecalculateInitiative();

                AddComponent(entity, behavior);

                // Add combat behavior if requested
                if (authoring.IncludeCombatBehavior)
                {
                    AddComponent(entity, VillagerCombatBehavior.FromBehavior(in behavior));
                }

                // Mark for randomization if requested (system will randomize at runtime)
                if (authoring.RandomizeOnSpawn)
                {
                    AddComponent(entity, new RandomizeBehaviorOnSpawn
                    {
                        Range = authoring.RandomizationRange
                    });
                }
            }
        }
    }

    /// <summary>
    /// Tag component indicating behavior traits should be randomized on first spawn.
    /// Removed after randomization is applied.
    /// </summary>
    public struct RandomizeBehaviorOnSpawn : IComponentData
    {
        public float Range;
    }
}

