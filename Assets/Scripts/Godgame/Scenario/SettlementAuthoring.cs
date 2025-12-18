using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace Godgame.Scenario
{
    /// <summary>
    /// Describes which prefabs should be used when standing up the interactive scenario settlement.
    /// </summary>
    // Forced rebake
    [DisallowMultipleComponent]
    public sealed class SettlementAuthoring : MonoBehaviour
    {
        [Header("Prefabs")]
        public GameObject villageCenterPrefab;
        public GameObject storehousePrefab;
        public GameObject housingPrefab;
        public GameObject worshipPrefab;
        public GameObject villagerPrefab;

        [Header("Counts & Layout")]
        [Min(0)] public int initialVillagers = 6;
        [Min(0.5f)] public float villagerSpawnRadius = 8f;
        [Min(1f)] public float buildingRingRadius = 6f;
        [Min(1f)] public float resourceRingRadius = 12f;
        [Tooltip("Optional deterministic seed. If zero we derive a stable seed from the authoring object's instance id.")]
        public uint randomSeed;

        private sealed class SettlementBaker : Baker<SettlementAuthoring>
        {
            public override void Bake(SettlementAuthoring authoring)
            {
                Debug.Log($"[SettlementBaker] Baking {authoring.name} in scene {authoring.gameObject.scene.path}");
                // Use ConvertAndDestroy so the config lives only in the DOTS world.
                var entity = GetEntity(authoring, TransformUsageFlags.Dynamic);

                var config = new SettlementConfig
                {
                    VillageCenterPrefab = ResolvePrefab(authoring.villageCenterPrefab),
                    StorehousePrefab = ResolvePrefab(authoring.storehousePrefab),
                    HousingPrefab = ResolvePrefab(authoring.housingPrefab),
                    WorshipPrefab = ResolvePrefab(authoring.worshipPrefab),
                    VillagerPrefab = ResolvePrefab(authoring.villagerPrefab),
                    InitialVillagers = math.max(0, authoring.initialVillagers),
                    VillagerSpawnRadius = math.max(0.5f, authoring.villagerSpawnRadius),
                    BuildingRingRadius = math.max(1f, authoring.buildingRingRadius),
                    ResourceRingRadius = math.max(1f, authoring.resourceRingRadius),
                    Seed = ResolveSeed(authoring)
                };

                AddComponent(entity, config);
                AddComponent(entity, new SettlementRuntime());
                AddComponent(entity, LocalTransform.FromPositionRotationScale(authoring.transform.position, quaternion.identity, 1f));
                AddBuffer<SettlementResource>(entity);
            }

            private Entity ResolvePrefab(GameObject prefab)
            {
                if (prefab == null)
                {
                    Debug.LogWarning($"[SettlementBaker] Prefab is null");
                    return Entity.Null;
                }

                try
                {
                    var entity = GetEntity(prefab, TransformUsageFlags.Dynamic);
                    if (entity == Entity.Null)
                    {
                        Debug.LogWarning($"[SettlementBaker] GetEntity returned Null for prefab {prefab.name}");
                    }
                    else
                    {
                        Debug.Log($"[SettlementBaker] Resolved prefab {prefab.name} to entity {entity}");
                    }
                    return entity;
                }
                catch (MissingReferenceException)
                {
                    Debug.LogError($"[SettlementBaker] MissingReferenceException when resolving prefab. The reference exists but points to a missing object.");
                    return Entity.Null;
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"[SettlementBaker] Exception resolving prefab: {e}");
                    return Entity.Null;
                }
            }

            private static uint ResolveSeed(SettlementAuthoring authoring)
            {
                if (authoring.randomSeed != 0)
                {
                    return authoring.randomSeed;
                }

                var derived = math.abs(authoring.GetInstanceID());
                return derived == 0 ? 1u : (uint)derived;
            }
        }
    }
}
