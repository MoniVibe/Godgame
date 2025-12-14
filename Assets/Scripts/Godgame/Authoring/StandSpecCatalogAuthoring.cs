using Godgame.Environment.Vegetation;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace Godgame.Authoring
{
    /// <summary>
    /// ScriptableObject authoring asset for stand/patch specifications.
    /// Bakes stand specs into a StandSpecBlob.
    /// </summary>
    [CreateAssetMenu(fileName = "StandSpecCatalog", menuName = "Godgame/Stand Spec Catalog", order = 102)]
    public sealed class StandSpecCatalogAuthoring : ScriptableObject
    {
        [System.Serializable]
        public struct StandSpecData
        {
            [Tooltip("Unique stand identifier")]
            public string id;

            [Tooltip("Primary plant species ID")]
            public string plantId;

            [Tooltip("Plants per square meter")]
            [Range(0f, 100f)]
            public float density;

            [Tooltip("Clustering factor (0=uniform, 1=clustered)")]
            [Range(0f, 1f)]
            public float clustering;

            [Tooltip("Minimum distance between plants (meters)")]
            public float minDistance;

            [Tooltip("Spawn radius for stand (meters)")]
            public float spawnRadius;

            [Tooltip("Spawn weights per biome (indexed by BiomeId32 - 1: Temperate=0, Grasslands=1, Mountains=2)")]
            public float[] spawnWeightsPerBiome;
        }

        [SerializeField]
        [Tooltip("List of stand specifications")]
        private StandSpecData[] stands = new StandSpecData[0];

        public StandSpecData[] Stands => stands;

        public bool TryBuildBlob(out BlobAssetReference<StandSpecBlob> blobAsset)
        {
            blobAsset = default;
            if (stands == null || stands.Length == 0)
            {
#if UNITY_EDITOR
                Debug.LogWarning($"StandSpecCatalogAuthoring '{name}' has no stands defined.");
#endif
                return false;
            }

            var builder = new BlobBuilder(Allocator.Temp);
            ref var blobRoot = ref builder.ConstructRoot<StandSpecBlob>();
            var blobStands = builder.Allocate(ref blobRoot.Stands, stands.Length);

            for (int i = 0; i < stands.Length; i++)
            {
                var standData = stands[i];
                ref var stand = ref blobStands[i];
                stand = new StandSpec
                {
                    Id = standData.id,
                    PlantId = standData.plantId,
                    Density = standData.density,
                    Clustering = standData.clustering,
                    MinDistance = standData.minDistance,
                    SpawnRadius = standData.spawnRadius
                };

                int weightCount = standData.spawnWeightsPerBiome != null ? standData.spawnWeightsPerBiome.Length : 0;
                var weights = builder.Allocate(ref stand.SpawnWeightsPerBiome, weightCount);
                for (int j = 0; j < weightCount; j++)
                {
                    weights[j] = standData.spawnWeightsPerBiome[j];
                }
            }

            blobAsset = builder.CreateBlobAssetReference<StandSpecBlob>(Allocator.Persistent);
            builder.Dispose();
            return true;
        }
    }

    [DisallowMultipleComponent]
    public sealed class StandSpecCatalogAuthoringComponent : MonoBehaviour
    {
        [SerializeField] private StandSpecCatalogAuthoring catalog;

        public sealed class Baker : Unity.Entities.Baker<StandSpecCatalogAuthoringComponent>
        {
            public override void Bake(StandSpecCatalogAuthoringComponent authoringComponent)
            {
                if (authoringComponent.catalog == null)
                {
#if UNITY_EDITOR
                    Debug.LogWarning("[StandSpecCatalog] No StandSpecCatalogAuthoring asset assigned.");
#endif
                    return;
                }

                if (!authoringComponent.catalog.TryBuildBlob(out var blobAsset))
                {
                    return;
                }

                var entity = GetEntity(TransformUsageFlags.None);
                AddComponent(entity, new StandSpecSingleton
                {
                    Specs = blobAsset
                });
            }
        }
    }
}
