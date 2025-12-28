using Godgame.Input;
using Godgame.Presentation;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Godgame.Debugging
{
    /// <summary>
    /// Component for density visualization markers (billboard entities above rendered entities).
    /// </summary>
    public struct DensityMarker : IComponentData
    {
        public Entity SourceEntity;
    }

    /// <summary>
    /// System that shows markers above rendered entities for density sampling visualization.
    /// Toggle with D key (DebugInput.ToggleDensity).
    /// </summary>
    [UpdateInGroup(typeof(Unity.Entities.PresentationSystemGroup))]
    [UpdateAfter(typeof(Godgame_VillagerPresentationSystem))]
    [UpdateAfter(typeof(Godgame_ResourceChunkPresentationSystem))]
    public partial struct Godgame_DensityVisualizationSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<PresentationLODState>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            // Check if density visualization is enabled
            bool enabled = false;
            if (SystemAPI.TryGetSingleton<DebugInput>(out var debugInput))
            {
                enabled = debugInput.ToggleDensity == 1;
            }

            var ecb = new Unity.Entities.EntityCommandBuffer(Unity.Collections.Allocator.Temp);

            if (enabled)
            {
                // Spawn markers above rendered entities
                foreach (var (lodState, transform, entity) in SystemAPI.Query<RefRO<PresentationLODState>, RefRO<LocalTransform>>()
                    .WithAll<VillagerPresentationTag>()
                    .WithNone<DensityMarker>()
                    .WithEntityAccess())
                {
                    if (lodState.ValueRO.ShouldRender == 1)
                    {
                        // Spawn marker entity above villager
                        var markerEntity = ecb.CreateEntity();
                        ecb.AddComponent(markerEntity, LocalTransform.FromPosition(transform.ValueRO.Position + new float3(0, 2f, 0)));
                        ecb.AddComponent(markerEntity, new DensityMarker { SourceEntity = entity });
                        state.EntityManager.SetName(markerEntity, $"DensityMarker_{entity.Index}");
                    }
                }

                foreach (var (lodState, transform, entity) in SystemAPI.Query<RefRO<PresentationLODState>, RefRO<LocalTransform>>()
                    .WithAll<ResourceChunkPresentationTag>()
                    .WithNone<DensityMarker>()
                    .WithEntityAccess())
                {
                    if (lodState.ValueRO.ShouldRender == 1)
                    {
                        // Spawn marker entity above chunk
                        var markerEntity = ecb.CreateEntity();
                        ecb.AddComponent(markerEntity, LocalTransform.FromPosition(transform.ValueRO.Position + new float3(0, 1f, 0)));
                        ecb.AddComponent(markerEntity, new DensityMarker { SourceEntity = entity });
                        state.EntityManager.SetName(markerEntity, $"DensityMarker_{entity.Index}");
                    }
                }
            }
            else
            {
                // Remove all density markers
                foreach (var (_, entity) in SystemAPI.Query<RefRO<DensityMarker>>().WithEntityAccess())
                {
                    ecb.DestroyEntity(entity);
                }
            }

            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }
    }
}

