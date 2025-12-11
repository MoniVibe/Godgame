using Godgame.Environment.Vegetation;
using Godgame.Presentation.Bindings;
using PureDOTS.Systems;
using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;

namespace Godgame.Presentation.Bindings
{
    /// <summary>
    /// Bridge system that reads plant state and pushes visual token requests to presentation.
    /// Updates vegetation visuals based on current plant state and growth stage.
    /// </summary>
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    [BurstCompile]
    public partial struct VegetationBridgeSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            // Optional: presentation can work without bindings
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            // Try to get vegetation bindings (optional)
            if (!SystemAPI.TryGetSingleton<VegetationBindingSingleton>(out var bindings))
            {
                return; // No bindings configured, skip presentation updates
            }

            // Use Minimal bindings by default
            if (!bindings.MinimalBindings.IsCreated)
            {
                return;
            }

            var ecbSingleton = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();
            var ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged);

            // Update visual requests for all plants
            foreach (var (plantState, transform, entity) in SystemAPI.Query<RefRO<PlantState>, RefRO<LocalTransform>>()
                .WithEntityAccess())
            {
                // Find binding entry for this plant and stage
                byte stylePalette = 0;
                byte stylePattern = 0;
                byte prefabToken = 0;

                ref var entries = ref bindings.MinimalBindings.Value.Entries;
                for (int i = 0; i < entries.Length; i++)
                {
                    if (entries[i].PlantId.Equals(plantState.ValueRO.PlantId) &&
                        entries[i].Stage == plantState.ValueRO.Stage)
                    {
                        stylePalette = entries[i].StylePalette;
                        stylePattern = entries[i].StylePattern;
                        prefabToken = entries[i].PrefabToken;
                        break;
                    }
                }

                // Update or create visual request component
                if (SystemAPI.HasComponent<VegetationVisualRequest>(entity))
                {
                    var request = SystemAPI.GetComponentRW<VegetationVisualRequest>(entity);
                    if (request.ValueRO.PlantId.Equals(plantState.ValueRO.PlantId) &&
                        request.ValueRO.Stage == plantState.ValueRO.Stage)
                    {
                        // Already up to date
                        continue;
                    }

                    request.ValueRW.PlantId = plantState.ValueRO.PlantId;
                    request.ValueRW.Stage = plantState.ValueRO.Stage;
                    request.ValueRW.StylePalette = stylePalette;
                    request.ValueRW.StylePattern = stylePattern;
                    request.ValueRW.PrefabToken = prefabToken;
                    request.ValueRW.IsDirty = 1;
                }
                else
                {
                    ecb.AddComponent(entity, new VegetationVisualRequest
                    {
                        PlantId = plantState.ValueRO.PlantId,
                        Stage = plantState.ValueRO.Stage,
                        StylePalette = stylePalette,
                        StylePattern = stylePattern,
                        PrefabToken = prefabToken,
                        IsDirty = 1
                    });
                }
            }

            // Note: Actual visual spawning/updating happens in presentation systems
            // that read VegetationVisualRequest and apply changes via ECB
            // Removing this bridge system should not break the simulation
        }
    }
}

