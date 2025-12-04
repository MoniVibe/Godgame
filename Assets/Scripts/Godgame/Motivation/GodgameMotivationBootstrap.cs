#if UNITY_EDITOR || UNITY_STANDALONE
using PureDOTS.Authoring.Motivation;
using PureDOTS.Runtime.Motivation;
using PureDOTS.Systems.Motivation;
using Unity.Entities;
using UnityEngine;

namespace Godgame.Motivation
{
    /// <summary>
    /// Bootstrap system for Godgame motivation system.
    /// Creates Godgame-specific MotivationCatalog with villager/village ambitions.
    /// </summary>
    public static class GodgameMotivationBootstrap
    {
        /// <summary>
        /// Initializes the motivation system with Godgame-specific catalogs.
        /// Call this from your game bootstrap code.
        /// </summary>
        public static void Initialize(EntityManager entityManager)
        {
            // Create example motivation specs for Godgame
            var specs = new[]
            {
                // Dream: Craft rare item
                new MotivationSpec
                {
                    SpecId = 1001,
                    Layer = MotivationLayer.Dream,
                    Scope = MotivationScope.Individual,
                    Tag = MotivationTag.GainWealth,
                    BaseImportance = 150,
                    BaseInitiativeCost = 50,
                    MaxConcurrentHolders = 0,
                    RequiredLoyalty = 0,
                    MinCorruptPure = -100,
                    MinLawChaos = -100,
                    MinGoodEvil = -100
                },
                // Aspiration: Become master craftsman
                new MotivationSpec
                {
                    SpecId = 1002,
                    Layer = MotivationLayer.Aspiration,
                    Scope = MotivationScope.Individual,
                    Tag = MotivationTag.BecomeLegendary,
                    BaseImportance = 200,
                    BaseInitiativeCost = 80,
                    MaxConcurrentHolders = 0,
                    RequiredLoyalty = 0,
                    MinCorruptPure = -100,
                    MinLawChaos = -100,
                    MinGoodEvil = -100
                },
                // Ambition: Raise dynasty
                new MotivationSpec
                {
                    SpecId = 1003,
                    Layer = MotivationLayer.Ambition,
                    Scope = MotivationScope.Individual,
                    Tag = MotivationTag.GrowLineage,
                    BaseImportance = 255,
                    BaseInitiativeCost = 100,
                    MaxConcurrentHolders = 0,
                    RequiredLoyalty = 0,
                    MinCorruptPure = -100,
                    MinLawChaos = -100,
                    MinGoodEvil = -100
                },
                // Ambition: Village survival
                new MotivationSpec
                {
                    SpecId = 2001,
                    Layer = MotivationLayer.Ambition,
                    Scope = MotivationScope.Aggregate,
                    Tag = MotivationTag.WinCombat,
                    BaseImportance = 255,
                    BaseInitiativeCost = 120,
                    MaxConcurrentHolders = 0,
                    RequiredLoyalty = 100,
                    MinCorruptPure = -100,
                    MinLawChaos = -100,
                    MinGoodEvil = -100
                },
                // Wish: Better house
                new MotivationSpec
                {
                    SpecId = 1004,
                    Layer = MotivationLayer.Wish,
                    Scope = MotivationScope.Individual,
                    Tag = MotivationTag.GainWealth,
                    BaseImportance = 100,
                    BaseInitiativeCost = 30,
                    MaxConcurrentHolders = 0,
                    RequiredLoyalty = 0,
                    MinCorruptPure = -100,
                    MinLawChaos = -100,
                    MinGoodEvil = -100
                }
            };

            // Build catalog blob
            var catalogBlob = MotivationCatalogAsset.BuildBlobAssetFromSpecs(specs);

            // Initialize motivation system with our Godgame-specific catalog
            PureDOTS.Systems.Motivation.MotivationBootstrapHelper.InitializeMotivationSystem(
                entityManager,
                catalogBlob);

            Debug.Log("[GodgameMotivationBootstrap] Motivation system initialized with Godgame-specific catalog.");
        }
    }
}
#endif

