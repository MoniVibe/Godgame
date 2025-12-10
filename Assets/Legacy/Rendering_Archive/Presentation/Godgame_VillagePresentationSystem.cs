using Godgame.Villages;
using Moni.Godgame.CameraSystems;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Godgame.Presentation
{
    /// <summary>
    /// Presentation system for village centers.
    /// Reads village sim data and updates visual state (phase color, influence ring, impostors).
    /// Runs in PresentationSystemGroup (frame-time, not tick-time).
    /// </summary>
    [BurstCompile]
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    public partial struct Godgame_VillagePresentationSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<VillageCenterPresentationTag>();
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

            // Update village presentation state
            var job = new UpdateVillagePresentationJob
            {
                CameraPosition = cameraPosition,
                HasCameraTransform = hasCameraTransform,
                Config = config
            };
            job.ScheduleParallel();
        }
    }

    /// <summary>
    /// Job to update village center presentation state based on distance to camera and sim data.
    /// </summary>
    [BurstCompile]
    public partial struct UpdateVillagePresentationJob : IJobEntity
    {
        public float3 CameraPosition;
        public bool HasCameraTransform;
        public PresentationConfig Config;

        public void Execute(
            ref PresentationLODState lodState,
            ref VillageCenterVisualState visualState,
            in LocalTransform transform,
            in VillageCenterPresentationTag tag)
        {
            // Calculate distance to camera
            float distance = HasCameraTransform
                ? math.distance(transform.Position, CameraPosition)
                : 0f;

            lodState.DistanceToCamera = distance;

            // Village centers always render (they serve as impostors at distance)
            // But we track LOD for potential detail reduction
            if (distance < Config.LOD0Distance)
            {
                lodState.CurrentLOD = PresentationLOD.LOD0_Full;
            }
            else if (distance < Config.LOD1Distance)
            {
                lodState.CurrentLOD = PresentationLOD.LOD1_Mid;
            }
            else
            {
                lodState.CurrentLOD = PresentationLOD.LOD2_Far;
            }

            // Village centers always render (no culling)
            lodState.ShouldRender = 1;
        }
    }

    /// <summary>
    /// Job to update village visual state from sim components (Village).
    /// </summary>
    [BurstCompile]
    public partial struct UpdateVillageVisualFromSimJob : IJobEntity
    {
        public void Execute(
            ref VillageCenterVisualState visualState,
            in Village village,
            in VillageCenterPresentationTag tag)
        {
            // Set color based on village phase
            visualState.PhaseTint = GetPhaseColor(village.Phase);

            // Set influence radius from village data
            visualState.InfluenceRadius = village.InfluenceRadius;

            // Intensity based on member count (more members = more vibrant)
            float memberFactor = math.saturate(village.MemberCount / 100f);
            visualState.Intensity = 0.5f + memberFactor * 0.5f;
        }

        private static float4 GetPhaseColor(VillagePhase phase)
        {
            return phase switch
            {
                VillagePhase.Forming => new float4(0.5f, 0.5f, 0.5f, 1f),    // Gray
                VillagePhase.Growing => new float4(0.2f, 0.8f, 0.2f, 1f),    // Green
                VillagePhase.Stable => new float4(0.2f, 0.4f, 0.8f, 1f),     // Blue
                VillagePhase.Expanding => new float4(0.2f, 0.9f, 0.9f, 1f),  // Cyan
                VillagePhase.Crisis => new float4(0.9f, 0.2f, 0.2f, 1f),     // Red
                VillagePhase.Declining => new float4(0.6f, 0.3f, 0.1f, 1f),  // Brown
                _ => new float4(1f, 1f, 1f, 1f)                               // White (default)
            };
        }
    }
}

