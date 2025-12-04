using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Godgame.Buildings
{
    /// <summary>
    /// Authoring component for building durability.
    /// Add to building prefabs to enable durability tracking.
    /// </summary>
    public class BuildingDurabilityAuthoring : MonoBehaviour
    {
        [Header("Durability Settings")]
        [Tooltip("Maximum durability points")]
        public float maxDurability = 1000f;

        [Tooltip("Starting durability (percentage of max, 0-1)")]
        [Range(0f, 1f)]
        public float startingDurabilityPercent = 1.0f;

        [Header("Quality")]
        [Tooltip("Building quality tier")]
        public BuildingQuality quality = BuildingQuality.Common;

        [Tooltip("Include quality component")]
        public bool useQualityComponent = true;

        private sealed class BuildingDurabilityBaker : Baker<BuildingDurabilityAuthoring>
        {
            public override void Bake(BuildingDurabilityAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);

                float maxDura = math.max(100f, authoring.maxDurability);
                float startPct = math.saturate(authoring.startingDurabilityPercent);

                // Apply quality multiplier to max durability
                float qualityMultiplier = 1.0f;
                if (authoring.useQualityComponent)
                {
                    qualityMultiplier = authoring.quality switch
                    {
                        BuildingQuality.Poor => 0.7f,
                        BuildingQuality.Common => 1.0f,
                        BuildingQuality.Fine => 1.2f,
                        BuildingQuality.Superior => 1.5f,
                        BuildingQuality.Masterwork => 2.0f,
                        _ => 1.0f
                    };
                }

                float finalMax = maxDura * qualityMultiplier;
                float startingDura = finalMax * startPct;

                var durability = new BuildingDurability
                {
                    Current = startingDura,
                    Max = finalMax,
                    Status = DurabilityStatus.Stable,
                    LastDamageSource = DamageSource.None,
                    OutputMultiplier = 1.0f,
                    DefenseMultiplier = 1.0f,
                    LastDamageTick = 0
                };
                durability.UpdateStatus();

                AddComponent(entity, durability);

                if (authoring.useQualityComponent)
                {
                    AddComponent(entity, new BuildingQualityData
                    {
                        Quality = authoring.quality
                    });
                }
            }
        }
    }

    /// <summary>
    /// Authoring component for building durability system configuration.
    /// Place on a single GameObject in your scene.
    /// </summary>
    public class BuildingDurabilityConfigAuthoring : MonoBehaviour
    {
        [Header("Decay Settings")]
        [Tooltip("Natural decay rate per day (percentage of max, 0-1)")]
        [Range(0f, 0.01f)]
        public float naturalDecayRatePerDay = 0.001f;

        [Header("Fire Settings")]
        [Tooltip("Fire damage multiplier")]
        public float fireDamageMultiplier = 1.0f;

        [Header("Repair Settings")]
        [Tooltip("Durability threshold to flag for repair (0-1)")]
        [Range(0f, 1f)]
        public float repairThreshold = 0.75f;

        private sealed class BuildingDurabilityConfigBaker : Baker<BuildingDurabilityConfigAuthoring>
        {
            public override void Bake(BuildingDurabilityConfigAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.None);

                AddComponent(entity, new BuildingDurabilityConfig
                {
                    NaturalDecayRatePerDay = math.max(0f, authoring.naturalDecayRatePerDay),
                    FireDamageMultiplier = math.max(0f, authoring.fireDamageMultiplier),
                    RepairThreshold = math.saturate(authoring.repairThreshold)
                });
            }
        }
    }
}

