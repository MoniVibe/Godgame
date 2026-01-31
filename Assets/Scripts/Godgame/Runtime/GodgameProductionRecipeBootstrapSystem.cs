using PureDOTS.Runtime.Economy.Production;
using Unity.Collections;
using Unity.Entities;

namespace Godgame.Runtime
{
    /// <summary>
    /// Godgame-specific production recipe catalog (game-specific fork).
    /// </summary>
    [UpdateInGroup(typeof(SimulationSystemGroup), OrderFirst = true)]
    [UpdateAfter(typeof(ProductionRecipeBootstrapSystem))]
    public partial struct GodgameProductionRecipeBootstrapSystem : ISystem
    {
        private static BlobAssetReference<ProductionRecipeCatalogBlob> s_CatalogBlob;

        public void OnCreate(ref SystemState state)
        {
            EnsureCatalog(ref state);
            state.Enabled = false;
        }

        public void OnUpdate(ref SystemState state) { }

        public void OnDestroy(ref SystemState state)
        {
            DisposeCatalog(ref state);
        }

        private static void EnsureCatalog(ref SystemState state)
        {
            if (s_CatalogBlob.IsCreated)
            {
                AssignCatalog(ref state);
                return;
            }

            using var builder = new BlobBuilder(Allocator.Temp);
            ref var root = ref builder.ConstructRoot<ProductionRecipeCatalogBlob>();

            var recipeData = new NativeList<(ProductionRecipeBlob recipe, NativeList<RecipeInputBlob> inputs, NativeList<RecipeOutputBlob> outputs)>(8, Allocator.Temp);

            // Wood -> Plank
            var woodInputs = new NativeList<RecipeInputBlob>(Allocator.Temp);
            woodInputs.Add(new RecipeInputBlob
            {
                ItemId = new FixedString64Bytes("gg_wood"),
                Quantity = 50f,
                MinPurity = 0f,
                MinQuality = 0f
            });
            var plankOutputs = new NativeList<RecipeOutputBlob>(Allocator.Temp);
            plankOutputs.Add(new RecipeOutputBlob
            {
                ItemId = new FixedString64Bytes("gg_plank"),
                Quantity = 35f
            });
            recipeData.Add((new ProductionRecipeBlob
            {
                RecipeId = new FixedString64Bytes("gg_wood_to_plank"),
                Stage = ProductionStage.Refining,
                RequiredBusinessType = BusinessType.Sawmill,
                MinTechTier = 1,
                MinArtisanExpertise = 5,
                BaseTimeCost = 5.0f,
                LaborCost = 1.0f
            }, woodInputs, plankOutputs));

            // Ore -> Ingot
            var oreInputs = new NativeList<RecipeInputBlob>(Allocator.Temp);
            oreInputs.Add(new RecipeInputBlob
            {
                ItemId = new FixedString64Bytes("gg_ore"),
                Quantity = 60f,
                MinPurity = 0f,
                MinQuality = 0f
            });
            var ingotOutputs = new NativeList<RecipeOutputBlob>(Allocator.Temp);
            ingotOutputs.Add(new RecipeOutputBlob
            {
                ItemId = new FixedString64Bytes("gg_ingot"),
                Quantity = 40f
            });
            recipeData.Add((new ProductionRecipeBlob
            {
                RecipeId = new FixedString64Bytes("gg_ore_to_ingot"),
                Stage = ProductionStage.Refining,
                RequiredBusinessType = BusinessType.Blacksmith,
                MinTechTier = 1,
                MinArtisanExpertise = 10,
                BaseTimeCost = 6.0f,
                LaborCost = 1.0f
            }, oreInputs, ingotOutputs));

            // Plank + Ingot -> Tools
            var toolInputs = new NativeList<RecipeInputBlob>(Allocator.Temp);
            toolInputs.Add(new RecipeInputBlob
            {
                ItemId = new FixedString64Bytes("gg_plank"),
                Quantity = 10f,
                MinPurity = 0f,
                MinQuality = 0f
            });
            toolInputs.Add(new RecipeInputBlob
            {
                ItemId = new FixedString64Bytes("gg_ingot"),
                Quantity = 5f,
                MinPurity = 0f,
                MinQuality = 0f
            });
            var toolOutputs = new NativeList<RecipeOutputBlob>(Allocator.Temp);
            toolOutputs.Add(new RecipeOutputBlob
            {
                ItemId = new FixedString64Bytes("gg_tools"),
                Quantity = 2f
            });
            recipeData.Add((new ProductionRecipeBlob
            {
                RecipeId = new FixedString64Bytes("gg_tools_craft"),
                Stage = ProductionStage.Crafting,
                RequiredBusinessType = BusinessType.Builder,
                MinTechTier = 1,
                MinArtisanExpertise = 12,
                BaseTimeCost = 8.0f,
                LaborCost = 1.0f
            }, toolInputs, toolOutputs));

            var recipesArray = builder.Allocate(ref root.Recipes, recipeData.Length);
            for (int i = 0; i < recipeData.Length; i++)
            {
                var (recipeTemplate, inputs, outputs) = recipeData[i];
                ref var recipe = ref recipesArray[i];
                recipe.RecipeId = recipeTemplate.RecipeId;
                recipe.Stage = recipeTemplate.Stage;
                recipe.RequiredBusinessType = recipeTemplate.RequiredBusinessType;
                recipe.MinTechTier = recipeTemplate.MinTechTier;
                recipe.MinArtisanExpertise = recipeTemplate.MinArtisanExpertise;
                recipe.BaseTimeCost = recipeTemplate.BaseTimeCost;
                recipe.LaborCost = recipeTemplate.LaborCost;

                var inputsArray = builder.Allocate(ref recipe.Inputs, inputs.Length);
                for (int j = 0; j < inputs.Length; j++)
                {
                    inputsArray[j] = inputs[j];
                }

                var outputsArray = builder.Allocate(ref recipe.Outputs, outputs.Length);
                for (int j = 0; j < outputs.Length; j++)
                {
                    outputsArray[j] = outputs[j];
                }
            }

            for (int i = 0; i < recipeData.Length; i++)
            {
                recipeData[i].inputs.Dispose();
                recipeData[i].outputs.Dispose();
            }
            recipeData.Dispose();

            s_CatalogBlob = builder.CreateBlobAssetReference<ProductionRecipeCatalogBlob>(Allocator.Persistent);
            AssignCatalog(ref state);
        }

        private static void AssignCatalog(ref SystemState state)
        {
            if (!s_CatalogBlob.IsCreated)
            {
                return;
            }

            var catalogComponent = new ProductionRecipeCatalog { Catalog = s_CatalogBlob };
            using var query = state.EntityManager.CreateEntityQuery(ComponentType.ReadOnly<ProductionRecipeCatalog>());
            if (query.TryGetSingletonEntity<ProductionRecipeCatalog>(out var entity))
            {
                state.EntityManager.SetComponentData(entity, catalogComponent);
            }
            else
            {
                var newEntity = state.EntityManager.CreateEntity(typeof(ProductionRecipeCatalog));
                state.EntityManager.SetComponentData(newEntity, catalogComponent);
            }
        }

        private static void DisposeCatalog(ref SystemState state)
        {
            using var query = state.EntityManager.CreateEntityQuery(ComponentType.ReadOnly<ProductionRecipeCatalog>());
            if (query.TryGetSingleton(out ProductionRecipeCatalog catalog))
            {
                var entity = query.GetSingletonEntity();
                catalog.Catalog = default;
                if (state.EntityManager.Exists(entity))
                {
                    state.EntityManager.SetComponentData(entity, catalog);
                }
            }

            if (s_CatalogBlob.IsCreated)
            {
                s_CatalogBlob.Dispose();
                s_CatalogBlob = default;
            }
        }
    }
}
