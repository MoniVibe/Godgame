#if UNITY_EDITOR || UNITY_STANDALONE
using PureDOTS.Runtime.Aggregate;
using PureDOTS.Authoring.Aggregate;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace Godgame.Aggregate
{
    /// <summary>
    /// Bootstrap system that creates AggregateConfigState singleton with Godgame-specific config catalog.
    /// Defines aggregation rules for Village, Guild, Band types.
    /// </summary>
    public static class GodgameAggregateBootstrap
    {
        /// <summary>
        /// Initializes aggregate system by creating AggregateConfigState singleton with default config.
        /// Called from game-specific bootstrap code.
        /// </summary>
        public static void InitializeAggregateSystem(EntityManager entityManager, AggregateConfigCatalogAsset catalogAsset = null)
        {
            // Check if already initialized
            if (HasSingleton<AggregateConfigState>(entityManager))
            {
                return; // Already initialized
            }

            BlobAssetReference<AggregateConfigCatalog> catalogBlob;

            if (catalogAsset != null && catalogAsset.TypeConfigs != null && catalogAsset.TypeConfigs.Length > 0)
            {
                // Build blob from provided catalog asset
                catalogBlob = AggregateConfigCatalogAsset.BuildBlobAssetFromConfigs(catalogAsset.TypeConfigs);
            }
            else
            {
                // Create empty catalog as fallback
                catalogBlob = AggregateConfigCatalogAsset.BuildBlobAssetFromConfigs(new AggregateTypeConfigAsset[0]);
            }

            // Create AggregateConfigState singleton
            var configEntity = entityManager.CreateEntity();
            entityManager.AddComponentData(configEntity, new AggregateConfigState
            {
                Catalog = catalogBlob,
                AmbientUpdateFrequency = 100u // Update every 100 ticks (daily/weekly sim time)
            });
        }

        private static bool HasSingleton<T>(EntityManager entityManager) where T : unmanaged, IComponentData
        {
            using var query = entityManager.CreateEntityQuery(ComponentType.ReadOnly<T>());
            return !query.IsEmptyIgnoreFilter;
        }
    }
}
#endif





