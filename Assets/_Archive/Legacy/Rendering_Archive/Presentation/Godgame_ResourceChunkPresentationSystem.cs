#if LEGACY_PRESENTATION_ARCHIVE_ENABLED
using Godgame.Economy;
using Moni.Godgame.CameraSystems;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Godgame.Presentation
{
    /// <summary>
    /// Presentation system for resource chunks.
    /// Reads resource chunk sim data and updates visual state (LOD, color tint, scale).
    /// Runs in PresentationSystemGroup (frame-time, not tick-time).
    /// </summary>
    [BurstCompile]
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    public partial struct Godgame_ResourceChunkPresentationSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<ResourceChunkPresentationTag>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            // Get camera position for LOD calculations
            float3 cameraPosition = float3.zero;
            bool hasCameraTransform = false;

            foreach (var (cameraTransform, _) in SystemAPI.Query<RefRO<CameraTransform>, RefRO<CameraTag>>())
            {
                cameraPosition = cameraTransform.ValueRO.Position;
                hasCameraTransform = true;
                break;
            }

            // Get presentation config (use defaults if not found)
            PresentationConfig config = PresentationConfig.Default;
            if (SystemAPI.TryGetSingleton<PresentationConfig>(out var configSingleton))
            {
                config = configSingleton;
            }

            // Update chunk presentation state
            var job = new UpdateResourceChunkPresentationJob
            {
                CameraPosition = cameraPosition,
                HasCameraTransform = hasCameraTransform,
                Config = config
            };
            job.ScheduleParallel();
        }
    }

    /// <summary>
    /// Job to update resource chunk presentation state based on distance to camera and sim data.
    /// </summary>
    [BurstCompile]
    public partial struct UpdateResourceChunkPresentationJob : IJobEntity
    {
        public float3 CameraPosition;
        public bool HasCameraTransform;
        public PresentationConfig Config;

        public void Execute(
            ref PresentationLODState lodState,
            ref ResourceChunkVisualState visualState,
            in LocalTransform transform,
            in ResourceChunkPresentationTag tag,
            [EntityIndexInQuery] int entityIndex)
        {
            // Calculate distance to camera
            float distance = HasCameraTransform
                ? math.distance(transform.Position, CameraPosition)
                : 0f;

            lodState.DistanceToCamera = distance;

            // Determine LOD level (chunks use same distances as villagers)
            if (distance < Config.LOD0Distance)
            {
                lodState.CurrentLOD = PresentationLOD.LOD0_Full;
            }
            else if (distance < Config.LOD1Distance)
            {
                lodState.CurrentLOD = PresentationLOD.LOD1_Mid;
            }
            else if (distance < Config.LOD2Distance)
            {
                lodState.CurrentLOD = PresentationLOD.LOD2_Far;
            }
            else
            {
                lodState.CurrentLOD = PresentationLOD.Culled;
            }

            // Density sampling for chunks (always render carried chunks)
            bool shouldRender = true;
            if (visualState.IsCarried == 0 && Config.DensitySlider < 1f && lodState.CurrentLOD != PresentationLOD.LOD0_Full)
            {
                // Only sample chunks on ground outside LOD0
                int densityDivisor = (int)(1f / math.max(Config.DensitySlider, 0.01f));
                shouldRender = (entityIndex % densityDivisor) == 0;
            }

            lodState.ShouldRender = shouldRender ? (byte)1 : (byte)0;
        }
    }

    /// <summary>
    /// Job to update resource chunk visual state from sim components (ExtractedResource).
    /// </summary>
    [BurstCompile]
    public partial struct UpdateResourceChunkVisualFromSimJob : IJobEntity
    {
        public void Execute(
            ref ResourceChunkVisualState visualState,
            in ExtractedResource resource,
            in ResourceChunkPresentationTag tag)
        {
            // Set color based on resource type
            visualState.ResourceTypeTint = GetResourceTypeColor(resource.Type);

            // Scale based on quantity (logarithmic to prevent huge chunks)
            float baseScale = 0.5f;
            float quantityFactor = math.log2(math.max(resource.Quantity, 1) + 1) * 0.1f;
            visualState.QuantityScale = baseScale + quantityFactor;
        }

        private static float4 GetResourceTypeColor(ResourceType type)
        {
            // Color coding by resource category
            byte typeValue = (byte)type;

            // Ores (1-7): Gray
            if (typeValue >= 1 && typeValue <= 7)
            {
                return new float4(0.5f, 0.5f, 0.5f, 1f);
            }

            // Wood (10-14): Brown
            if (typeValue >= 10 && typeValue <= 14)
            {
                return new float4(0.55f, 0.27f, 0.07f, 1f);
            }

            // Stone (20-23): Dark gray
            if (typeValue >= 20 && typeValue <= 23)
            {
                return new float4(0.4f, 0.4f, 0.4f, 1f);
            }

            // Herbs (30-33): Green
            if (typeValue >= 30 && typeValue <= 33)
            {
                return new float4(0.2f, 0.6f, 0.2f, 1f);
            }

            // Agricultural (40-44): Yellow/Gold
            if (typeValue >= 40 && typeValue <= 44)
            {
                return new float4(0.9f, 0.8f, 0.2f, 1f);
            }

            // Default: White
            return new float4(1f, 1f, 1f, 1f);
        }
    }
}
#endif
