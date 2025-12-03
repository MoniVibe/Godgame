using Godgame.Authoring;
using Godgame.Environment.Vegetation;
using NUnit.Framework;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace Godgame.Tests.Environment
{
    /// <summary>
    /// Tests that vegetation catalog authoring produces stable blob hashes across runs.
    /// </summary>
    [TestFixture]
    public class VegetationIdempotentTests
    {
        private World world;
        private EntityManager entityManager;

        [SetUp]
        public void SetUp()
        {
            world = new World("TestWorld");
            entityManager = world.EntityManager;
        }

        [TearDown]
        public void TearDown()
        {
            if (world != null && world.IsCreated)
            {
                world.Dispose();
            }
        }

        [Test]
        public void PlantSpecCatalog_ProducesStableBlobHash()
        {
            // Create a test plant catalog authoring
            var authoring = UnityEngine.ScriptableObject.CreateInstance<PlantSpecCatalogAuthoring>();
            authoring.Plants = new PlantSpecCatalogAuthoring.PlantSpecData[]
            {
                new PlantSpecCatalogAuthoring.PlantSpecData
                {
                    id = "Oak",
                    biomeMask = 1, // Temperate
                    tempMin = 10f,
                    tempMax = 25f,
                    moistMin = 0.4f,
                    moistMax = 0.7f,
                    sunMin = 0.5f,
                    sunMax = 1f,
                    elevMin = -100f,
                    elevMax = 500f,
                    stageSecSeedling = 10f,
                    stageSecSapling = 100f,
                    stageSecMature = 1000f,
                    reproSeedsPerDay = 0.1f,
                    reproRadius = 5f,
                    seasonMask = 15, // All seasons
                    yields = new PlantSpecCatalogAuthoring.PlantLootEntryData[]
                    {
                        new PlantSpecCatalogAuthoring.PlantLootEntryData
                        {
                            materialId = "Wood",
                            weight = 1f,
                            minAmount = 10f,
                            maxAmount = 20f
                        }
                    },
                    style = new PlantSpecCatalogAuthoring.PlantStyleTokensData
                    {
                        stylePalette = 1,
                        stylePattern = 1,
                        familyToken = 1
                    }
                }
            };

            // Bake first time
            var baker = new PlantSpecCatalogAuthoring.Baker();
            var entity1 = entityManager.CreateEntity();
            baker.Bake(authoring);
            var blob1 = entityManager.GetComponentData<PlantSpecSingleton>(entity1).Specs;

            // Clear and bake again
            entityManager.DestroyEntity(entity1);
            var entity2 = entityManager.CreateEntity();
            baker.Bake(authoring);
            var blob2 = entityManager.GetComponentData<PlantSpecSingleton>(entity2).Specs;

            // Compare blob contents (should be identical)
            Assert.IsTrue(blob1.IsCreated);
            Assert.IsTrue(blob2.IsCreated);

            ref var plants1 = ref blob1.Value.Plants;
            ref var plants2 = ref blob2.Value.Plants;

            Assert.AreEqual(plants1.Length, plants2.Length, "Plant count should match");

            for (int i = 0; i < plants1.Length; i++)
            {
                Assert.AreEqual(plants1[i].Id, plants2[i].Id, $"Plant {i} ID should match");
                Assert.AreEqual(plants1[i].BiomeMask, plants2[i].BiomeMask, $"Plant {i} BiomeMask should match");
                Assert.AreEqual(plants1[i].TempMin, plants2[i].TempMin, 0.001f, $"Plant {i} TempMin should match");
                Assert.AreEqual(plants1[i].Yields.Length, plants2[i].Yields.Length, $"Plant {i} yield count should match");
            }
        }

        [Test]
        public void StandSpecCatalog_ProducesStableBlobHash()
        {
            // Create a test stand catalog authoring
            var authoring = UnityEngine.ScriptableObject.CreateInstance<StandSpecCatalogAuthoring>();
            authoring.Stands = new StandSpecCatalogAuthoring.StandSpecData[]
            {
                new StandSpecCatalogAuthoring.StandSpecData
                {
                    id = "OakForest",
                    plantId = "Oak",
                    density = 0.5f,
                    clustering = 0.7f,
                    minDistance = 2f,
                    spawnRadius = 10f,
                    spawnWeightsPerBiome = new float[] { 1f, 0.5f, 0f } // Temperate=1, Grasslands=0.5, Mountains=0
                }
            };

            // Bake first time
            var baker = new StandSpecCatalogAuthoring.Baker();
            var entity1 = entityManager.CreateEntity();
            baker.Bake(authoring);
            var blob1 = entityManager.GetComponentData<StandSpecSingleton>(entity1).Specs;

            // Clear and bake again
            entityManager.DestroyEntity(entity1);
            var entity2 = entityManager.CreateEntity();
            baker.Bake(authoring);
            var blob2 = entityManager.GetComponentData<StandSpecSingleton>(entity2).Specs;

            // Compare blob contents
            Assert.IsTrue(blob1.IsCreated);
            Assert.IsTrue(blob2.IsCreated);

            ref var stands1 = ref blob1.Value.Stands;
            ref var stands2 = ref blob2.Value.Stands;

            Assert.AreEqual(stands1.Length, stands2.Length, "Stand count should match");

            for (int i = 0; i < stands1.Length; i++)
            {
                Assert.AreEqual(stands1[i].Id, stands2[i].Id, $"Stand {i} ID should match");
                Assert.AreEqual(stands1[i].PlantId, stands2[i].PlantId, $"Stand {i} PlantId should match");
                Assert.AreEqual(stands1[i].Density, stands2[i].Density, 0.001f, $"Stand {i} Density should match");
            }
        }
    }
}

