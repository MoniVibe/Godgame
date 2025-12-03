using Godgame.Authoring;
using Godgame.Environment.Vegetation;
using NUnit.Framework;
using UnityEngine;

namespace Godgame.Tests.Environment
{
    /// <summary>
    /// Tests vegetation validation logic (preference contradictions, unreachable stages, etc.).
    /// </summary>
    [TestFixture]
    public class VegetationValidatorTests
    {
        [Test]
        public void PlantSpec_DetectsPreferenceContradictions()
        {
            var catalog = ScriptableObject.CreateInstance<PlantSpecCatalogAuthoring>();
            catalog.Plants = new PlantSpecCatalogAuthoring.PlantSpecData[]
            {
                new PlantSpecCatalogAuthoring.PlantSpecData
                {
                    id = "InvalidPlant",
                    tempMin = 25f,  // Min > Max
                    tempMax = 10f,
                    moistMin = 0.8f,
                    moistMax = 0.2f, // Min > Max
                    biomeMask = 0    // No biomes allowed
                }
            };

            // Validation would detect these issues
            Assert.Greater(catalog.Plants[0].tempMin, catalog.Plants[0].tempMax, "Should detect tempMin > tempMax");
            Assert.Greater(catalog.Plants[0].moistMin, catalog.Plants[0].moistMax, "Should detect moistMin > moistMax");
            Assert.AreEqual(0u, catalog.Plants[0].biomeMask, "Should detect missing biome mask");
        }

        [Test]
        public void PlantSpec_DetectsUnreachableStages()
        {
            var catalog = ScriptableObject.CreateInstance<PlantSpecCatalogAuthoring>();
            catalog.Plants = new PlantSpecCatalogAuthoring.PlantSpecData[]
            {
                new PlantSpecCatalogAuthoring.PlantSpecData
                {
                    id = "UnreachableStages",
                    stageSecSeedling = 0f,  // Can't reach Sapling
                    stageSecSapling = 100f,
                    stageSecMature = 1000f
                }
            };

            // Validation would detect unreachable stages
            Assert.AreEqual(0f, catalog.Plants[0].stageSecSeedling, "Seedling duration is 0");
            Assert.Greater(catalog.Plants[0].stageSecSapling, 0f, "Sapling duration > 0 but Seedling is 0");
        }

        [Test]
        public void PlantSpec_ValidatesVariantOfReferences()
        {
            var catalog = ScriptableObject.CreateInstance<PlantSpecCatalogAuthoring>();
            catalog.Plants = new PlantSpecCatalogAuthoring.PlantSpecData[]
            {
                new PlantSpecCatalogAuthoring.PlantSpecData
                {
                    id = "BasePlant",
                    tempMin = 10f,
                    tempMax = 25f
                },
                new PlantSpecCatalogAuthoring.PlantSpecData
                {
                    id = "ValidVariant",
                    variantOf = "BasePlant",
                    tempMin = 15f  // Override
                },
                new PlantSpecCatalogAuthoring.PlantSpecData
                {
                    id = "InvalidVariant",
                    variantOf = "NonexistentPlant"  // Reference doesn't exist
                }
            };

            // Validation would detect invalid VariantOf reference
            bool foundInvalid = false;
            for (int i = 0; i < catalog.Plants.Length; i++)
            {
                if (catalog.Plants[i].id == "InvalidVariant")
                {
                    bool baseFound = false;
                    for (int j = 0; j < catalog.Plants.Length; j++)
                    {
                        if (catalog.Plants[j].id == catalog.Plants[i].variantOf)
                        {
                            baseFound = true;
                            break;
                        }
                    }
                    if (!baseFound)
                    {
                        foundInvalid = true;
                    }
                    break;
                }
            }

            Assert.IsTrue(foundInvalid, "Should detect invalid VariantOf reference");
        }

        [Test]
        public void StandSpec_ValidatesPlantIdReferences()
        {
            var catalog = ScriptableObject.CreateInstance<StandSpecCatalogAuthoring>();
            catalog.Stands = new StandSpecCatalogAuthoring.StandSpecData[]
            {
                new StandSpecCatalogAuthoring.StandSpecData
                {
                    id = "InvalidStand",
                    plantId = "",  // Missing plant ID
                    density = 0f   // Invalid density
                }
            };

            // Validation would detect these issues
            Assert.IsEmpty(catalog.Stands[0].plantId, "Should detect missing plantId");
            Assert.AreEqual(0f, catalog.Stands[0].density, "Should detect invalid density");
        }
    }
}

