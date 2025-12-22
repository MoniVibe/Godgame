using Unity.Entities;
using Unity.Collections;
using UnityEngine;

namespace Godgame.Scenario
{
    /// <summary>
    /// Authoring component that bakes GodgameScenarioConfig ScriptableObject to blob asset.
    /// Place on a GameObject in your scene and assign a GodgameScenarioConfig asset.
    /// </summary>
    public class GodgameScenarioConfigAuthoring : MonoBehaviour
    {
        [Header("Scenario Configuration")]
        [Tooltip("Scenario config asset (create via Assets > Create > Godgame > Scenario Config)")]
        public GodgameScenarioConfig Config;

        class Baker : Baker<GodgameScenarioConfigAuthoring>
        {
            public override void Bake(GodgameScenarioConfigAuthoring authoring)
            {
                if (authoring.Config == null)
                {
                    Debug.LogWarning("[GodgameScenarioConfigAuthoring] No GodgameScenarioConfig assigned, using defaults.");
                    return;
                }

                var builder = new BlobBuilder(Allocator.Temp);
                ref var configBlob = ref builder.ConstructRoot<GodgameScenarioConfigBlob>();

                configBlob.Mode = authoring.Config.Mode;
                configBlob.VillageCount = authoring.Config.VillageCount;
                configBlob.VillagersPerVillageMin = authoring.Config.VillagersPerVillageMin;
                configBlob.VillagersPerVillageMax = authoring.Config.VillagersPerVillageMax;
                configBlob.VillageSpacing = authoring.Config.VillageSpacing;
                configBlob.VillageInfluenceRadius = authoring.Config.VillageInfluenceRadius;
                configBlob.ResourceNodeCount = authoring.Config.ResourceNodeCount;
                configBlob.ResourceChunkCount = authoring.Config.ResourceChunkCount;
                configBlob.RandomSeed = authoring.Config.RandomSeed;

                var blobAsset = builder.CreateBlobAssetReference<GodgameScenarioConfigBlob>(Allocator.Persistent);
                builder.Dispose();

                var entity = GetEntity(TransformUsageFlags.None);
                AddComponent(entity, new GodgameScenarioConfigBlobReference { Config = blobAsset });
            }
        }
    }
}
