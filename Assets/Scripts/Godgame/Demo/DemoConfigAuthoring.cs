using Unity.Entities;
using Unity.Collections;
using UnityEngine;

namespace Godgame.Demo
{
    /// <summary>
    /// Authoring component that bakes DemoConfig ScriptableObject to blob asset.
    /// Place on a GameObject in your scene and assign a DemoConfig asset.
    /// </summary>
    public class DemoConfigAuthoring : MonoBehaviour
    {
        [Header("Demo Configuration")]
        [Tooltip("Demo config asset (create via Assets > Create > Godgame > Demo Config)")]
        public DemoConfig Config;

        class Baker : Baker<DemoConfigAuthoring>
        {
            public override void Bake(DemoConfigAuthoring authoring)
            {
                if (authoring.Config == null)
                {
                    Debug.LogWarning("[DemoConfigAuthoring] No DemoConfig assigned, using defaults.");
                    return;
                }

                var builder = new BlobBuilder(Allocator.Temp);
                ref var configBlob = ref builder.ConstructRoot<DemoConfigBlob>();

                configBlob.Mode = authoring.Config.Mode;
                configBlob.VillageCount = authoring.Config.VillageCount;
                configBlob.VillagersPerVillageMin = authoring.Config.VillagersPerVillageMin;
                configBlob.VillagersPerVillageMax = authoring.Config.VillagersPerVillageMax;
                configBlob.VillageSpacing = authoring.Config.VillageSpacing;
                configBlob.VillageInfluenceRadius = authoring.Config.VillageInfluenceRadius;
                configBlob.ResourceNodeCount = authoring.Config.ResourceNodeCount;
                configBlob.ResourceChunkCount = authoring.Config.ResourceChunkCount;
                configBlob.RandomSeed = authoring.Config.RandomSeed;

                var blobAsset = builder.CreateBlobAssetReference<DemoConfigBlob>(Allocator.Persistent);
                builder.Dispose();

                var entity = GetEntity(TransformUsageFlags.None);
                AddComponent(entity, new DemoConfigBlobReference { Config = blobAsset });
            }
        }
    }
}

