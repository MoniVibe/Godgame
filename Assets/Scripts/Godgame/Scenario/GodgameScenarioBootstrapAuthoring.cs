using Godgame.Scenario;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace Godgame.Scenario
{
    /// <summary>
    /// Authoring component for scenario bootstrap configuration.
    /// Add to a GameObject in the scenario scene to configure entity spawning at startup.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class GodgameScenarioBootstrapAuthoring : MonoBehaviour
    {
        [Header("Prefabs")]
        public GameObject villagerPrefab;
        public GameObject storehousePrefab;

        [Header("Entity Counts")]
        [Range(1, 100)] public int initialVillagerCount = 10;
        [Range(1, 20)] public int resourceNodeCount = 6;

        [Header("Layout")]
        [Range(5f, 50f)] public float villagerSpawnRadius = 15f;
        [Range(5f, 50f)] public float resourceNodeRadius = 20f;

        [Header("Seed")]
        [Tooltip("Deterministic seed. 0 = derive from instance ID.")]
        public uint seed;

        [Header("Villager Behavior")]
        [Tooltip("Range for randomizing villager personality traits (Bold/Vengeful). Traits will be ±this value.")]
        [Range(0f, 100f)] public float behaviorRandomizationRange = 60f;

        private sealed class GodgameScenarioBootstrapBaker : Baker<GodgameScenarioBootstrapAuthoring>
        {
            public override void Bake(GodgameScenarioBootstrapAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.None);

                AddComponent(entity, new GodgameScenarioBootstrapConfig
                {
                    VillagerPrefab = ResolvePrefab(authoring.villagerPrefab),
                    StorehousePrefab = ResolvePrefab(authoring.storehousePrefab),
                    InitialVillagerCount = math.max(1, authoring.initialVillagerCount),
                    ResourceNodeCount = math.max(1, authoring.resourceNodeCount),
                    VillagerSpawnRadius = math.max(5f, authoring.villagerSpawnRadius),
                    ResourceNodeRadius = math.max(5f, authoring.resourceNodeRadius),
                    Seed = ResolveSeed(authoring),
                    BehaviorRandomizationRange = math.max(0f, authoring.behaviorRandomizationRange)
                });

                AddComponent(entity, new GodgameScenarioBootstrapRuntime());
            }

            private Entity ResolvePrefab(GameObject prefab)
            {
                return prefab != null ? GetEntity(prefab, TransformUsageFlags.Dynamic) : Entity.Null;
            }

            private static uint ResolveSeed(GodgameScenarioBootstrapAuthoring authoring)
            {
                if (authoring.seed != 0)
                {
                    return authoring.seed;
                }

                var derived = math.abs(authoring.GetInstanceID());
                return derived == 0 ? 1u : (uint)derived;
            }
        }
    }
}
