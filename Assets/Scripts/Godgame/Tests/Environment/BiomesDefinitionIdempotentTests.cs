using Godgame.Authoring;
using Godgame.Environment;
using NUnit.Framework;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace Godgame.Tests.Environment
{
    /// <summary>
    /// Tests that biome definition authoring produces stable blob hashes across runs.
    /// </summary>
    [TestFixture]
    public class BiomesDefinitionIdempotentTests
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
        public void BiomeDefinitionAuthoring_ProducesStableBlobHash()
        {
            // Create a test biome definition authoring
            var authoring = UnityEngine.ScriptableObject.CreateInstance<BiomeDefinitionAuthoring>();
            authoring.Profiles = new BiomeDefinitionAuthoring.BiomeProfileData[]
            {
                new BiomeDefinitionAuthoring.BiomeProfileData
                {
                    id = "Temperate",
                    tempMin = 10f,
                    tempMax = 25f,
                    moistMin = 0.4f,
                    moistMax = 0.7f,
                    elevMin = -100f,
                    elevMax = 500f,
                    slopeMaxDeg = 45f,
                    villagerStaminaDrainPct = 100,
                    diseaseRiskPct = 10,
                    resourceBiasWood = 800,
                    resourceBiasOre = 500,
                    minimapPalette = 1,
                    groundStyle = 1,
                    weatherProfileToken = 1
                },
                new BiomeDefinitionAuthoring.BiomeProfileData
                {
                    id = "Grasslands",
                    tempMin = 15f,
                    tempMax = 30f,
                    moistMin = 0.2f,
                    moistMax = 0.5f,
                    elevMin = -50f,
                    elevMax = 200f,
                    slopeMaxDeg = 30f,
                    villagerStaminaDrainPct = 90,
                    diseaseRiskPct = 5,
                    resourceBiasWood = 200,
                    resourceBiasOre = 300,
                    minimapPalette = 2,
                    groundStyle = 2,
                    weatherProfileToken = 2
                }
            };

            // Bake first time
            var baker = new BiomeDefinitionAuthoring.Baker();
            var entity1 = entityManager.CreateEntity();
            baker.Bake(authoring);
            var blob1 = entityManager.GetComponentData<BiomeDefinitionSingleton>(entity1).Definitions;

            // Clear and bake again
            entityManager.DestroyEntity(entity1);
            var entity2 = entityManager.CreateEntity();
            baker.Bake(authoring);
            var blob2 = entityManager.GetComponentData<BiomeDefinitionSingleton>(entity2).Definitions;

            // Compare blob contents (should be identical)
            Assert.IsTrue(blob1.IsCreated);
            Assert.IsTrue(blob2.IsCreated);

            ref var profiles1 = ref blob1.Value.Profiles;
            ref var profiles2 = ref blob2.Value.Profiles;

            Assert.AreEqual(profiles1.Length, profiles2.Length, "Profile count should match");

            for (int i = 0; i < profiles1.Length; i++)
            {
                Assert.AreEqual(profiles1[i].Id, profiles2[i].Id, $"Profile {i} ID should match");
                Assert.AreEqual(profiles1[i].BiomeId32, profiles2[i].BiomeId32, $"Profile {i} BiomeId32 should match");
                Assert.AreEqual(profiles1[i].TempMin, profiles2[i].TempMin, 0.001f, $"Profile {i} TempMin should match");
                Assert.AreEqual(profiles1[i].TempMax, profiles2[i].TempMax, 0.001f, $"Profile {i} TempMax should match");
                Assert.AreEqual(profiles1[i].ResourceBiasWood, profiles2[i].ResourceBiasWood, $"Profile {i} ResourceBiasWood should match");
            }
        }
    }
}

