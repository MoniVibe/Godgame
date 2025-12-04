using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Godgame.Prayer
{
    /// <summary>
    /// Authoring component to configure the prayer power pool singleton.
    /// Place on a single GameObject in your scene to initialize prayer system.
    /// </summary>
    public class PrayerPoolAuthoring : MonoBehaviour
    {
        [Header("Pool Settings")]
        [Tooltip("Starting prayer power")]
        public float initialPrayer = 1000f;

        [Tooltip("Maximum prayer power cap (0 = no cap)")]
        public float maxPrayer = 50000f;

        [Header("Config Settings")]
        [Tooltip("Global multiplier for all prayer generation")]
        public float globalMultiplier = 1.0f;

        [Tooltip("Minimum prayer per villager per second")]
        public float minPrayerPerVillager = 0.1f;

        [Tooltip("Maximum prayer per villager per second")]
        public float maxPrayerPerVillager = 10.0f;

        [Tooltip("Enforce the max prayer cap")]
        public bool enforceCap = true;

        private sealed class PrayerPoolBaker : Baker<PrayerPoolAuthoring>
        {
            public override void Bake(PrayerPoolAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.None);

                AddComponent(entity, new PrayerPowerPool
                {
                    CurrentPrayer = math.max(0f, authoring.initialPrayer),
                    MaxPrayer = math.max(1000f, authoring.maxPrayer),
                    GenerationRate = 0f,
                    TotalGenerated = 0f,
                    TotalConsumed = 0f
                });

                AddComponent(entity, new PrayerPoolTag());

                AddComponent(entity, new PrayerSystemConfig
                {
                    GlobalMultiplier = math.max(0.01f, authoring.globalMultiplier),
                    MinPrayerPerVillager = math.max(0f, authoring.minPrayerPerVillager),
                    MaxPrayerPerVillager = math.max(authoring.minPrayerPerVillager, authoring.maxPrayerPerVillager),
                    EnforceCap = authoring.enforceCap
                });
            }
        }
    }

    /// <summary>
    /// Authoring component to mark an entity as a prayer generator.
    /// Add to villager prefabs or buildings that should generate prayer.
    /// </summary>
    public class PrayerGeneratorAuthoring : MonoBehaviour
    {
        [Header("Generation Settings")]
        [Tooltip("Base prayer generation rate (per second)")]
        public float baseRate = 1.0f;

        [Tooltip("Initial multiplier (mood, temple effects, etc.)")]
        public float multiplier = 1.0f;

        [Tooltip("Whether generator is active on spawn")]
        public bool activeOnSpawn = true;

        private sealed class PrayerGeneratorBaker : Baker<PrayerGeneratorAuthoring>
        {
            public override void Bake(PrayerGeneratorAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);

                AddComponent(entity, new PrayerGenerator
                {
                    BaseRate = math.max(0f, authoring.baseRate),
                    Multiplier = math.max(0.01f, authoring.multiplier),
                    IsActive = authoring.activeOnSpawn
                });
            }
        }
    }
}

