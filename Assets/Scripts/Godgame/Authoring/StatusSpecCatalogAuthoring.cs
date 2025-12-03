using Godgame.Abilities;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace Godgame.Authoring
{
    /// <summary>
    /// MonoBehaviour wrapper to bake StatusSpecCatalog ScriptableObjects into blob assets.
    /// </summary>
    public sealed class StatusSpecCatalogAuthoring : MonoBehaviour
    {
        public StatusSpecCatalog Catalog;
    }

    public sealed class StatusSpecCatalogAuthoringBaker : Baker<StatusSpecCatalogAuthoring>
    {
        public override void Bake(StatusSpecCatalogAuthoring authoring)
        {
            var catalog = authoring.Catalog;
            if (catalog == null || catalog.Statuses == null || catalog.Statuses.Length == 0)
            {
                return;
            }

            var builder = new BlobBuilder(Allocator.Temp);
            ref var root = ref builder.ConstructRoot<StatusSpecCatalogBlob>();

            var statusesArray = builder.Allocate(ref root.Statuses, catalog.Statuses.Length);

            for (int i = 0; i < catalog.Statuses.Length; i++)
            {
                var statusDef = catalog.Statuses[i];
                statusesArray[i] = new StatusSpecBlob
                {
                    Id = new FixedString64Bytes(statusDef.Id ?? $"Status_{i}"),
                    MaxStacks = statusDef.MaxStacks,
                    Dispellable = statusDef.Dispellable ? (byte)1 : (byte)0,
                    DispelTags = (uint)statusDef.DispelTags,
                    Duration = statusDef.Duration,
                    Period = statusDef.Period
                };
            }

            var blobAsset = builder.CreateBlobAssetReference<StatusSpecCatalogBlob>(Allocator.Persistent);
            builder.Dispose();

            var entity = GetEntity(TransformUsageFlags.None);
            AddComponent(entity, new StatusSpecCatalogBlobRef { Catalog = blobAsset });
        }
    }
}
