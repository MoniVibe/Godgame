using Godgame.Environment;
using Godgame.Environment.Vegetation;
using Godgame.Environment.Vegetation.Systems;
using NUnit.Framework;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.TestTools;

namespace Godgame.Tests.Environment
{
    /// <summary>
    /// PlayMode tests for vegetation growth determinism and stage transitions.
    /// </summary>
    public class VegetationGrowthDeterminism_Playmode
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

        [UnityTest]
        public System.Collections.IEnumerator VegetationGrowth_StageTransitionsAtExpectedAges()
        {
            // Create plant spec blob
            var builder = new BlobBuilder(Allocator.Temp);
            ref var root = ref builder.ConstructRoot<PlantSpecBlob>();
            var plants = builder.Allocate(ref root.Plants, 1);
            plants[0] = new PlantSpec
            {
                Id = "TestPlant",
                StageSecSeedling = 5f,  // 5 seconds to sapling
                StageSecSapling = 10f,  // 10 more seconds to mature
                StageSecMature = 100f,
                TempMin = 10f,
                TempMax = 30f,
                MoistMin = 0f,
                MoistMax = 1f,
                BiomeMask = 1
            };
            builder.Allocate(ref plants[0].Yields, 0);
            var plantBlob = builder.CreateBlobAssetReference<PlantSpecBlob>(Allocator.Persistent);
            builder.Dispose();

            // Create singletons
            var plantSpecEntity = entityManager.CreateEntity();
            entityManager.AddComponentData(plantSpecEntity, new PlantSpecSingleton { Specs = plantBlob });

            var climateEntity = entityManager.CreateEntity();
            entityManager.AddComponentData(climateEntity, new ClimateState
            {
                TempC = 20f,
                Moisture01 = 0.6f,
                ElevationMean = 0f
            });

            var moistureEntity = entityManager.CreateEntity();
            var moistureBuilder = new BlobBuilder(Allocator.Temp);
            ref var moistureRoot = ref moistureBuilder.ConstructRoot<BlobArray<float>>();
            var moistureArray = moistureBuilder.Allocate(ref moistureRoot, 1);
            moistureArray[0] = 0.6f;
            var moistureBlob = moistureBuilder.CreateBlobAssetReference<BlobArray<float>>(Allocator.Persistent);
            moistureBuilder.Dispose();
            entityManager.AddComponentData(moistureEntity, new MoistureGrid
            {
                Width = 1,
                Height = 1,
                Values = moistureBlob
            });

            var biomeEntity = entityManager.CreateEntity();
            var biomeBuilder = new BlobBuilder(Allocator.Temp);
            ref var biomeRoot = ref biomeBuilder.ConstructRoot<BlobArray<uint>>();
            var biomeArray = biomeBuilder.Allocate(ref biomeRoot, 1);
            biomeArray[0] = 1;
            var biomeBlob = biomeBuilder.CreateBlobAssetReference<BlobArray<uint>>(Allocator.Persistent);
            biomeBuilder.Dispose();
            entityManager.AddComponentData(biomeEntity, new BiomeGrid
            {
                Width = 1,
                Height = 1,
                BiomeIds = biomeBlob
            });

            var weatherEntity = entityManager.CreateEntity();
            entityManager.AddComponentData(weatherEntity, new WeatherState { WeatherToken = 0, Intensity01 = 0f });

            // Create test plant
            var plantEntity = entityManager.CreateEntity();
            entityManager.AddComponentData(plantEntity, new PlantState
            {
                PlantId = "TestPlant",
                Stage = GrowthStage.Seedling,
                AgeSeconds = 0f,
                Health01 = 1f,
                StressLevel01 = 0f
            });
            entityManager.AddComponentData(plantEntity, new PlantStand
            {
                StandId = "TestStand",
                StandCenter = Entity.Null,
                DistanceFromCenter = 0f
            });
            entityManager.AddComponentData(plantEntity, LocalTransform.FromPosition(float3.zero));
            entityManager.AddBuffer<ReproductionCommand>(plantEntity);

            // Get growth system
            var growthSystem = world.GetOrCreateSystemManaged<VegetationGrowthSystem>();

            // Simulate for 4 seconds (should still be Seedling)
            for (int i = 0; i < 40; i++) // 40 frames at 0.1s each = 4 seconds
            {
                growthSystem.Update(world.Unmanaged);
                yield return null;
            }

            var plantState = entityManager.GetComponentData<PlantState>(plantEntity);
            Assert.AreEqual(GrowthStage.Seedling, plantState.Stage, "Should still be Seedling after 4 seconds");

            // Simulate for 2 more seconds (should transition to Sapling at 5s)
            for (int i = 0; i < 20; i++)
            {
                growthSystem.Update(world.Unmanaged);
                yield return null;
            }

            plantState = entityManager.GetComponentData<PlantState>(plantEntity);
            Assert.AreEqual(GrowthStage.Sapling, plantState.Stage, "Should transition to Sapling at 5 seconds");

            // Simulate for 10 more seconds (should transition to Mature at 15s total)
            for (int i = 0; i < 100; i++)
            {
                growthSystem.Update(world.Unmanaged);
                yield return null;
            }

            plantState = entityManager.GetComponentData<PlantState>(plantEntity);
            Assert.AreEqual(GrowthStage.Mature, plantState.Stage, "Should transition to Mature at 15 seconds");

            plantBlob.Dispose();
            moistureBlob.Dispose();
            biomeBlob.Dispose();
        }
    }
}

