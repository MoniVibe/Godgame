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

        /// <summary>
        /// Baker that converts StandSpecCatalogAuthoring to StandSpecBlob.
        /// </summary>
        public sealed class Baker : Unity.Entities.Baker<StandSpecCatalogAuthoring>
        {
            public override void Bake(StandSpecCatalogAuthoring authoring)
            {
                if (authoring.stands == null || authoring.stands.Length == 0)
                {
                    Debug.LogWarning($"StandSpecCatalogAuthoring '{authoring.name}' has no stands defined.");
                    return;
                }

                // Create blob builder
                var builder = new BlobBuilder(Allocator.Temp);
                ref var blobRoot = ref builder.ConstructRoot<StandSpecBlob>();
                var blobStands = builder.Allocate(ref blobRoot.Stands, authoring.stands.Length);

                // Convert stands to blob data
                for (int i = 0; i < authoring.stands.Length; i++)
                {
                    var standData = authoring.stands[i];
                    
                    // Allocate spawn weights array
                    int weightCount = standData.spawnWeightsPerBiome != null ? standData.spawnWeightsPerBiome.Length : 0;
                    var weightsBuilder = builder.Allocate(ref blobStands[i].SpawnWeightsPerBiome, weightCount);
                    for (int j = 0; j < weightCount; j++)
                    {
                        weightsBuilder[j] = standData.spawnWeightsPerBiome[j];
                    }

                    blobStands[i] = new StandSpec
                    {
                        Id = standData.id,
                        PlantId = standData.plantId,
                        Density = standData.density,
                        Clustering = standData.clustering,
                        MinDistance = standData.minDistance,
                        SpawnRadius = standData.spawnRadius,
                        SpawnWeightsPerBiome = weightsBuilder
                    };
                }

                // Create blob asset reference
                var blobAsset = builder.CreateBlobAssetReference<StandSpecBlob>(Allocator.Persistent);
                builder.Dispose();

                // Add singleton component with blob reference
                var entity = GetEntity(TransformUsageFlags.None);
                AddComponent(entity, new StandSpecSingleton
                {
                    Specs = blobAsset
                });
            }
        }
    }
}

