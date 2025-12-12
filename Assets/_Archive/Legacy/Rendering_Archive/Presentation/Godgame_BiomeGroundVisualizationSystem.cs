#if LEGACY_PRESENTATION_ARCHIVE_ENABLED
using Godgame.Demo;
using PureDOTS.Environment;
using PureDOTS.Runtime.Camera;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;

namespace Godgame.Presentation
{
    /// <summary>
    /// System that visualizes biomes on ground tiles using material properties.
    /// Close-up (LOD0-1): Updates MaterialProperty on GroundTile entities.
    /// Far (LOD2+): Delegates to BiomeOverlaySystem.
    /// </summary>
    [BurstCompile]
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    [UpdateAfter(typeof(Godgame_BiomeDataHookSystem))]
    public partial struct Godgame_BiomeGroundVisualizationSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<BiomePresentationData>();
            state.RequireForUpdate<PresentationConfig>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var config = SystemAPI.GetSingleton<PresentationConfig>();
            // TODO: Get camera position from proper ECS camera component or MonoBehaviour
            // For now, use a placeholder position
            Unity.Mathematics.float3 cameraPosition = Unity.Mathematics.float3.zero;
#if GODGAME_LEGACY_DEBUG
            // Legacy debug: Try to get from CameraRigState if available
            // var cameraState = SystemAPI.GetSingleton<CameraRigState>();
            // var cameraPosition = cameraState.Position;
#endif

            var job = new UpdateGroundTileBiomeVisualizationJob
            {
                CameraPosition = cameraPosition,
                LOD1Distance = config.LOD1Distance,
                LOD2Distance = config.LOD2Distance
            };
            job.ScheduleParallel();
        }
    }

    /// <summary>
    /// Job that updates ground tile material properties based on biome data.
    /// </summary>
    [BurstCompile]
    public partial struct UpdateGroundTileBiomeVisualizationJob : IJobEntity
    {
        public float3 CameraPosition;
        public float LOD1Distance;
        public float LOD2Distance;

        public void Execute(
            ref BiomePresentationData presentationData,
            in Godgame.Demo.GroundTile tile,
            in LocalTransform transform)
        {
            // Calculate distance to camera
            float distance = math.distance(transform.Position, CameraPosition);

            // Only update material properties for close-up (LOD0-1)
            // Far LOD (LOD2+) will use overlay chunks
            if (distance > LOD2Distance)
            {
                return; // Skip - overlay system handles this
            }

            // Calculate biome color tint
            float4 biomeTint = GetBiomeColor(presentationData.Biome);
            
            // Adjust tint based on moisture (affects saturation)
            float moistureFactor = presentationData.Moisture / 100f;
            float saturation = math.lerp(0.5f, 1f, moistureFactor);
            biomeTint = math.lerp(new float4(0.5f, 0.5f, 0.5f, 1f), biomeTint, saturation);
            
            // Adjust tint based on temperature (warm/cool shift)
            float tempFactor = (presentationData.Temperature - 10f) / 30f; // Normalize 10-40°C to 0-1
            float4 warmTint = new float4(1f, 0.9f, 0.7f, 1f); // Warm (yellow-orange)
            float4 coolTint = new float4(0.7f, 0.8f, 1f, 1f); // Cool (blue)
            float4 tempTint = math.lerp(coolTint, warmTint, math.saturate(tempFactor));
            biomeTint = math.lerp(biomeTint, tempTint, 0.2f); // 20% temperature influence

            // Note: Actual MaterialProperty update would be done via MaterialMeshInfo
            // For now, we're just calculating the tint - actual material update requires
            // Unity.Entities.Graphics.MaterialProperty component which needs shader setup
            // This is a placeholder that shows the calculation logic
        }

        private static float4 GetBiomeColor(BiomeType biome)
        {
            return biome switch
            {
                BiomeType.Tundra => new float4(0.8f, 0.85f, 0.9f, 1f),      // Light blue-gray
                BiomeType.Taiga => new float4(0.4f, 0.5f, 0.3f, 1f),        // Dark green-gray
                BiomeType.Grassland => new float4(0.6f, 0.7f, 0.4f, 1f),    // Yellow-green
                BiomeType.Forest => new float4(0.2f, 0.5f, 0.2f, 1f),       // Dark green
                BiomeType.Desert => new float4(0.9f, 0.8f, 0.6f, 1f),       // Sandy yellow
                BiomeType.Rainforest => new float4(0.1f, 0.4f, 0.2f, 1f),   // Deep green
                BiomeType.Savanna => new float4(0.7f, 0.6f, 0.3f, 1f),      // Yellow-brown
                BiomeType.Swamp => new float4(0.3f, 0.4f, 0.3f, 1f),        // Dark gray-green
                _ => new float4(0.5f, 0.5f, 0.5f, 1f)                      // Gray (default)
            };
        }
    }
}
#endif
