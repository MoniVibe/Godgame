#if UNITY_EDITOR || UNITY_STANDALONE
using PureDOTS.Authoring.Guild;
using PureDOTS.Runtime.Components;
using PureDOTS.Runtime.Guild;
using Unity.Entities;
using UnityEngine;

namespace Godgame.Guild
{
    /// <summary>
    /// Bootstrap system for Godgame-specific guild configuration.
    /// Creates GuildConfigState, GuildActionConfigState, and GuildGovernanceConfigState singletons
    /// with Godgame-specific catalogs.
    /// </summary>
    public class GodgameGuildBootstrap : MonoBehaviour
    {
        [Header("Guild Type Catalog")]
        [Tooltip("Guild type catalog asset defining Godgame guild types (Heroes, Merchants, Artisans, etc.).")]
        public GuildTypeCatalogAsset GuildTypeCatalog;

        [Header("Guild Action Catalog")]
        [Tooltip("Guild action catalog asset defining actions guilds can take.")]
        public GuildActionCatalogAsset GuildActionCatalog;

        [Header("Guild Governance Catalog")]
        [Tooltip("Guild governance catalog asset defining governance rules.")]
        public GuildGovernanceCatalogAsset GuildGovernanceCatalog;

        [Header("Formation Settings")]
        [Tooltip("Frequency of formation checks in ticks.")]
        public uint FormationCheckFrequency = 300;

        /// <summary>
        /// Initializes guild config singletons in the ECS world.
        /// Called from bootstrap system or world initialization.
        /// </summary>
        public void InitializeGuildConfigs(World world)
        {
            var em = world.EntityManager;

            // Create or get singleton entity for GuildConfigState
            var guildConfigEntity = em.CreateEntity();
            em.SetName(guildConfigEntity, "GuildConfigState");

            if (GuildTypeCatalog != null)
            {
                var catalogBlob = GuildTypeCatalog.BuildBlobAsset();
                em.AddComponentData(guildConfigEntity, new GuildConfigState
                {
                    Catalog = catalogBlob,
                    FormationCheckFrequency = FormationCheckFrequency
                });
            }
            else
            {
                // Create empty catalog if no asset provided
                var emptyCatalog = GuildTypeCatalogAsset.BuildBlobAssetFromSpecs(new GuildTypeSpecAsset[0]);
                em.AddComponentData(guildConfigEntity, new GuildConfigState
                {
                    Catalog = emptyCatalog,
                    FormationCheckFrequency = FormationCheckFrequency
                });
            }

            // Create or get singleton entity for GuildActionConfigState
            var actionConfigEntity = em.CreateEntity();
            em.SetName(actionConfigEntity, "GuildActionConfigState");

            if (GuildActionCatalog != null)
            {
                var actionCatalogBlob = GuildActionCatalog.BuildBlobAsset();
                em.AddComponentData(actionConfigEntity, new GuildActionConfigState
                {
                    Catalog = actionCatalogBlob
                });
            }
            else
            {
                // Create empty catalog if no asset provided
                var emptyCatalog = GuildActionCatalogAsset.BuildBlobAssetFromSpecs(new GuildActionSpecAsset[0]);
                em.AddComponentData(actionConfigEntity, new GuildActionConfigState
                {
                    Catalog = emptyCatalog
                });
            }

            // Create or get singleton entity for GuildGovernanceConfigState
            var governanceConfigEntity = em.CreateEntity();
            em.SetName(governanceConfigEntity, "GuildGovernanceConfigState");

            if (GuildGovernanceCatalog != null)
            {
                var governanceCatalogBlob = GuildGovernanceCatalog.BuildBlobAsset();
                em.AddComponentData(governanceConfigEntity, new GuildGovernanceConfigState
                {
                    Catalog = governanceCatalogBlob
                });
            }
            else
            {
                // Create empty catalog if no asset provided
                var emptyCatalog = GuildGovernanceCatalogAsset.BuildBlobAssetFromSpecs(new GuildGovernanceSpecAsset[0]);
                em.AddComponentData(governanceConfigEntity, new GuildGovernanceConfigState
                {
                    Catalog = emptyCatalog
                });
            }
        }
    }
}
#endif



















