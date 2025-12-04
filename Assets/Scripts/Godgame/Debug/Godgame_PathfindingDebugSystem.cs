using Godgame.Input;
using Godgame.Presentation;
using PureDOTS.Runtime.Components;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Godgame.Debugging
{
    /// <summary>
    /// Component for pathfinding debug markers (arrow/line entities showing target positions).
    /// </summary>
    public struct PathfindingMarker : IComponentData
    {
        public Entity SourceEntity;
        public float3 TargetPosition;
    }

    /// <summary>
    /// System that shows pathfinding debug visualization (lines/arrows from villagers to targets).
    /// Toggle with P key (DebugInput.TogglePathfinding).
    /// </summary>
    [BurstCompile]
    [UpdateInGroup(typeof(Unity.Entities.PresentationSystemGroup))]
    [UpdateAfter(typeof(Godgame_VillagerPresentationSystem))]
    public partial struct Godgame_PathfindingDebugSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<VillagerPresentationTag>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            // Check if pathfinding debug is enabled
            bool enabled = false;
            if (SystemAPI.TryGetSingleton<DebugInput>(out var debugInput))
            {
                enabled = debugInput.TogglePathfinding == 1;
            }

            var ecb = new Unity.Entities.EntityCommandBuffer(Unity.Collections.Allocator.Temp);

            if (enabled)
            {
                // Show pathfinding markers for villagers with jobs
                foreach (var (job, transform, villagerEntity) in SystemAPI.Query<RefRO<VillagerJob>, RefRO<LocalTransform>>()
                    .WithAll<VillagerPresentationTag>()
                    .WithNone<PathfindingMarker>()
                    .WithEntityAccess())
                {
                    // Check if villager has a target (assigned or gathering)
                    // Note: Navigating phase was removed, using Assigned as equivalent
                    if (job.ValueRO.Phase == VillagerJob.JobPhase.Assigned || 
                        job.ValueRO.Phase == VillagerJob.JobPhase.Gathering)
                    {
                        // For now, use a placeholder target position (would need actual target from job ticket)
                        // This is a simplified version - full implementation would read from VillagerJobTicket
                        float3 targetPos = transform.ValueRO.Position + new float3(10f, 0f, 10f); // Placeholder

                        var markerEntity = ecb.CreateEntity();
                        ecb.AddComponent(markerEntity, LocalTransform.FromPosition(transform.ValueRO.Position));
                        ecb.AddComponent(markerEntity, new PathfindingMarker
                        {
                            SourceEntity = villagerEntity,
                            TargetPosition = targetPos
                        });
                        state.EntityManager.SetName(markerEntity, $"PathfindingMarker_{villagerEntity.Index}");
                    }
                }

                // Update existing markers
                foreach (var (marker, transform, markerEntity) in SystemAPI.Query<RefRW<PathfindingMarker>, RefRW<LocalTransform>>().WithEntityAccess())
                {
                    if (state.EntityManager.Exists(marker.ValueRO.SourceEntity))
                    {
                        var sourceTransform = state.EntityManager.GetComponentData<LocalTransform>(marker.ValueRO.SourceEntity);
                        transform.ValueRW.Position = sourceTransform.Position;
                    }
                    else
                    {
                        ecb.DestroyEntity(markerEntity);
                    }
                }
            }
            else
            {
                // Remove all pathfinding markers
                foreach (var (_, entity) in SystemAPI.Query<RefRO<PathfindingMarker>>().WithEntityAccess())
                {
                    ecb.DestroyEntity(entity);
                }
            }

            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }
    }
}

