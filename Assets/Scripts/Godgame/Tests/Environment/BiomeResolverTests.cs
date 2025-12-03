using Godgame.Environment;
using Godgame.Environment.Systems;
using NUnit.Framework;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace Godgame.Tests.Environment
{
    /// <summary>
    /// Tests that biome resolver selects the correct biome for given climate conditions.
    /// </summary>
    [TestFixture]
    public class BiomeResolverTests
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
        public void BiomeResolver_SelectsTemperate_ForModerateConditions()
        {
            // Create biome definitions
            var builder = new BlobBuilder(Allocator.Temp);
            ref var root = ref builder.ConstructRoot<BiomeDefinitionBlob>();
            var profiles = builder.Allocate(ref root.Profiles, 3);

            // Temperate: 10-25°C, 0.4-0.7 moisture, -100-500m elevation
            profiles[0] = new BiomeProfile
            {
                Id = "Temperate",
                BiomeId32 = 1,
                TempMin = 10f,
                TempMax = 25f,
                MoistMin = 0.4f,
                MoistMax = 0.7f,
                ElevMin = -100f,
                ElevMax = 500f,
                ResourceBiasWood = 800,
                ResourceBiasOre = 500
            };

            // Grasslands: 15-30°C, 0.2-0.5 moisture, -50-200m elevation
            profiles[1] = new BiomeProfile
            {
                Id = "Grasslands",
                BiomeId32 = 2,
                TempMin = 15f,
                TempMax = 30f,
                MoistMin = 0.2f,
                MoistMax = 0.5f,
                ElevMin = -50f,
                ElevMax = 200f,
                ResourceBiasWood = 200,
                ResourceBiasOre = 300
            };

            // Mountains: -10-15°C, 0.2-0.4 moisture, 500-3000m elevation
            profiles[2] = new BiomeProfile
            {
                Id = "Mountains",
                BiomeId32 = 3,
                TempMin = -10f,
                TempMax = 15f,
                MoistMin = 0.2f,
                MoistMax = 0.4f,
                ElevMin = 500f,
                ElevMax = 3000f,
                ResourceBiasWood = 300,
                ResourceBiasOre = 900
            };

            var blobAsset = builder.CreateBlobAssetReference<BiomeDefinitionBlob>(Allocator.Persistent);
            builder.Dispose();

            // Test moderate conditions (should match Temperate best)
            float tempC = 17f;      // Middle of Temperate range
            float moisture = 0.6f;   // Middle of Temperate range
            float elevation = 0f;    // Sea level

            uint chosenId = BiomeResolveSystem.ChooseBiomeId(blobAsset, tempC, moisture, elevation);

            Assert.AreEqual(1u, chosenId, "Should select Temperate biome for moderate conditions");

            blobAsset.Dispose();
        }

        [Test]
        public void BiomeResolver_SelectsMountains_ForHighElevationColdConditions()
        {
            // Create same biome definitions as above
            var builder = new BlobBuilder(Allocator.Temp);
            ref var root = ref builder.ConstructRoot<BiomeDefinitionBlob>();
            var profiles = builder.Allocate(ref root.Profiles, 3);

            profiles[0] = new BiomeProfile { Id = "Temperate", BiomeId32 = 1, TempMin = 10f, TempMax = 25f, MoistMin = 0.4f, MoistMax = 0.7f, ElevMin = -100f, ElevMax = 500f };
            profiles[1] = new BiomeProfile { Id = "Grasslands", BiomeId32 = 2, TempMin = 15f, TempMax = 30f, MoistMin = 0.2f, MoistMax = 0.5f, ElevMin = -50f, ElevMax = 200f };
            profiles[2] = new BiomeProfile { Id = "Mountains", BiomeId32 = 3, TempMin = -10f, TempMax = 15f, MoistMin = 0.2f, MoistMax = 0.4f, ElevMin = 500f, ElevMax = 3000f };

            var blobAsset = builder.CreateBlobAssetReference<BiomeDefinitionBlob>(Allocator.Persistent);
            builder.Dispose();

            // Test mountain conditions
            float tempC = 5f;        // Cold, within Mountains range
            float moisture = 0.3f;   // Low moisture, within Mountains range
            float elevation = 1000f; // High elevation, within Mountains range

            uint chosenId = BiomeResolveSystem.ChooseBiomeId(blobAsset, tempC, moisture, elevation);

            Assert.AreEqual(3u, chosenId, "Should select Mountains biome for high elevation cold conditions");

            blobAsset.Dispose();
        }
    }
}

