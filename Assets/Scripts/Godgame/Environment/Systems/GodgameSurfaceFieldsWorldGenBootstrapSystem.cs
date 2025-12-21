using PureDOTS.Runtime.Core;
using PureDOTS.Runtime.WorldGen;
using Unity.Collections;
using Unity.Entities;

namespace Godgame.Environment.Systems
{
    /// <summary>
    /// Minimal bootstrap so Stage 1 SurfaceFields can run in Godgame without scene authoring.
    /// Creates default <see cref="WorldRecipeComponent"/> and <see cref="WorldGenDefinitionsComponent"/> singletons if missing.
    /// </summary>
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateBefore(typeof(PureDOTS.Systems.RecordSimulationSystemGroup))]
    public partial struct GodgameSurfaceFieldsWorldGenBootstrapSystem : ISystem
    {
        private BlobAssetReference<WorldRecipeBlob> _ownedRecipe;
        private BlobAssetReference<WorldGenDefinitionsBlob> _ownedDefinitions;

        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<GameWorldTag>();
            state.RequireForUpdate<SurfaceFieldsChunkRequestQueue>();
        }

        public void OnUpdate(ref SystemState state)
        {
            var entityManager = state.EntityManager;

            var hasRecipe = HasAny<WorldRecipeComponent>(entityManager);
            var hasDefinitions = HasAny<WorldGenDefinitionsComponent>(entityManager);

            if (hasRecipe && hasDefinitions)
            {
                state.Enabled = false;
                return;
            }

            var entity = entityManager.CreateEntity();
            if (!hasRecipe)
            {
                _ownedRecipe = BuildDefaultRecipeBlob();
                entityManager.AddComponentData(entity, new WorldRecipeComponent
                {
                    RecipeHash = default,
                    Recipe = _ownedRecipe
                });
            }

            if (!hasDefinitions)
            {
                _ownedDefinitions = BuildDefaultDefinitionsBlob();
                entityManager.AddComponentData(entity, new WorldGenDefinitionsComponent
                {
                    DefinitionsHash = default,
                    Definitions = _ownedDefinitions
                });
            }

            state.Enabled = false;
        }

        public void OnDestroy(ref SystemState state)
        {
            if (_ownedDefinitions.IsCreated)
            {
                _ownedDefinitions.Dispose();
            }

            if (_ownedRecipe.IsCreated)
            {
                _ownedRecipe.Dispose();
            }
        }

        private static bool HasAny<T>(EntityManager entityManager) where T : unmanaged, IComponentData
        {
            using var query = entityManager.CreateEntityQuery(ComponentType.ReadOnly<T>());
            return !query.IsEmptyIgnoreFilter;
        }

        private static BlobAssetReference<WorldRecipeBlob> BuildDefaultRecipeBlob()
        {
            using var builder = new BlobBuilder(Allocator.Temp);
            ref var root = ref builder.ConstructRoot<WorldRecipeBlob>();
            root.SchemaVersion = WorldGenSchema.WorldRecipeSchemaVersion;
            root.WorldSeed = 12345u;
            root.DefinitionsHash = default;

            var stages = builder.Allocate(ref root.Stages, 1);
            stages[0].Kind = WorldGenStageKind.SurfaceFields;
            stages[0].SeedSalt = 999u;

            var parameters = builder.Allocate(ref stages[0].Parameters, 1);
            parameters[0] = new WorldGenParamBlob
            {
                Key = new FixedString64Bytes("sea_level"),
                Type = WorldGenParamType.Float,
                FloatValue = 0.5f
            };

            return builder.CreateBlobAssetReference<WorldRecipeBlob>(Allocator.Persistent);
        }

        private static BlobAssetReference<WorldGenDefinitionsBlob> BuildDefaultDefinitionsBlob()
        {
            using var builder = new BlobBuilder(Allocator.Temp);
            ref var root = ref builder.ConstructRoot<WorldGenDefinitionsBlob>();
            root.SchemaVersion = WorldGenSchema.WorldGenDefinitionsSchemaVersion;

            var biomes = builder.Allocate(ref root.Biomes, 6);
            biomes[0] = new WorldGenBiomeDefinitionBlob
            {
                Id = new FixedString64Bytes("plains"),
                Weight = 0.1f,
                TemperatureMin = 0f,
                TemperatureMax = 1f,
                MoistureMin = 0f,
                MoistureMax = 1f
            };
            biomes[1] = new WorldGenBiomeDefinitionBlob
            {
                Id = new FixedString64Bytes("ocean"),
                Weight = 0.01f,
                TemperatureMin = 0f,
                TemperatureMax = 1f,
                MoistureMin = 0f,
                MoistureMax = 1f
            };
            biomes[2] = new WorldGenBiomeDefinitionBlob
            {
                Id = new FixedString64Bytes("desert"),
                Weight = 1f,
                TemperatureMin = 0.55f,
                TemperatureMax = 1f,
                MoistureMin = 0f,
                MoistureMax = 0.35f
            };
            biomes[3] = new WorldGenBiomeDefinitionBlob
            {
                Id = new FixedString64Bytes("forest"),
                Weight = 1f,
                TemperatureMin = 0.25f,
                TemperatureMax = 0.85f,
                MoistureMin = 0.35f,
                MoistureMax = 1f
            };
            biomes[4] = new WorldGenBiomeDefinitionBlob
            {
                Id = new FixedString64Bytes("tundra"),
                Weight = 1f,
                TemperatureMin = 0f,
                TemperatureMax = 0.35f,
                MoistureMin = 0f,
                MoistureMax = 0.7f
            };
            biomes[5] = new WorldGenBiomeDefinitionBlob
            {
                Id = new FixedString64Bytes("swamp"),
                Weight = 1f,
                TemperatureMin = 0.35f,
                TemperatureMax = 0.85f,
                MoistureMin = 0.75f,
                MoistureMax = 1f
            };

            builder.Allocate(ref root.Resources, 0);
            builder.Allocate(ref root.RuinSets, 0);

            return builder.CreateBlobAssetReference<WorldGenDefinitionsBlob>(Allocator.Persistent);
        }
    }
}

