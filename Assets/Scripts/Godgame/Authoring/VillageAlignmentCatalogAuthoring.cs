using Godgame.Villages;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace Godgame.Authoring
{
    /// <summary>
    /// MonoBehaviour wrapper to bake VillageAlignmentCatalog ScriptableObjects.
    /// </summary>
    public sealed class VillageAlignmentCatalogAuthoring : MonoBehaviour
    {
        public VillageAlignmentCatalog Catalog;
    }

    public sealed class VillageAlignmentCatalogAuthoringBaker : Baker<VillageAlignmentCatalogAuthoring>
    {
        public override void Bake(VillageAlignmentCatalogAuthoring authoring)
        {
            var catalog = authoring.Catalog;
            if (catalog == null || catalog.Axes == null || catalog.Axes.Length == 0)
            {
                return;
            }

            var builder = new BlobBuilder(Allocator.Temp);
            ref var root = ref builder.ConstructRoot<VillageAlignmentDefinitionsBlob>();

            var axesArray = builder.Allocate(ref root.Axes, catalog.Axes.Length);

            for (int i = 0; i < catalog.Axes.Length; i++)
            {
                var axisDef = catalog.Axes[i];
                axesArray[i] = new VillageAlignmentAxisBlob
                {
                    AxisId = new FixedString64Bytes(axisDef.AxisId ?? $"Axis_{i}"),
                    InitiativeResponse = new InitiativeResponseBlob
                    {
                        MinBias = axisDef.InitiativeResponse.MinBias,
                        MaxBias = axisDef.InitiativeResponse.MaxBias,
                        EasingType = (byte)axisDef.InitiativeResponse.EasingType
                    },
                    SurplusWeights = new SurplusPriorityWeightsBlob
                    {
                        Build = axisDef.SurplusWeights.Build,
                        Defend = axisDef.SurplusWeights.Defend,
                        Proselytize = axisDef.SurplusWeights.Proselytize,
                        Research = axisDef.SurplusWeights.Research,
                        Migrate = axisDef.SurplusWeights.Migrate
                    }
                };
            }

            var blobAsset = builder.CreateBlobAssetReference<VillageAlignmentDefinitionsBlob>(Allocator.Persistent);
            builder.Dispose();

            var entity = GetEntity(TransformUsageFlags.None);
            AddComponent(entity, new VillageAlignmentCatalogBlob { Catalog = blobAsset });
        }
    }
}
