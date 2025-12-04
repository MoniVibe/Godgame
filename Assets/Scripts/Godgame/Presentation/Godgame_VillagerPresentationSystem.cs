using Godgame.Villagers;
using Moni.Godgame.CameraSystems;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Godgame.Presentation
{
    /// <summary>
    /// Presentation system for villagers.
    /// Reads villager sim data and updates visual state (LOD, color tint, animation state).
    /// Runs in PresentationSystemGroup (frame-time, not tick-time).
    /// </summary>
    [BurstCompile]
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    public partial struct Godgame_VillagerPresentationSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<VillagerPresentationTag>();
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

            // Update villager presentation state
            var job = new UpdateVillagerPresentationJob
            {
                CameraPosition = cameraPosition,
                HasCameraTransform = hasCameraTransform,
                Config = config
            };
            job.ScheduleParallel();
        }
    }

    /// <summary>
    /// Job to update villager presentation state based on distance to camera and sim data.
    /// </summary>
    [BurstCompile]
    public partial struct UpdateVillagerPresentationJob : IJobEntity
    {
        public float3 CameraPosition;
        public bool HasCameraTransform;
        public PresentationConfig Config;

        public void Execute(
            ref PresentationLODState lodState,
            ref VillagerVisualState visualState,
            in LocalTransform transform,
            in VillagerPresentationTag tag,
            [EntityIndexInQuery] int entityIndex)
        {
            // TEMPORARY: LOD bypass for testing - force all villagers visible
            // TODO: Remove this bypass after verifying rendering works
#if GODGAME_FORCE_VISIBLE_VILLAGERS
            lodState.CurrentLOD = PresentationLOD.LOD0_Full;
            lodState.ShouldRender = 1;
            lodState.DistanceToCamera = 0f;
            return;
#endif

            // Calculate distance to camera
            float distance = HasCameraTransform
                ? math.distance(transform.Position, CameraPosition)
                : 0f;

            lodState.DistanceToCamera = distance;

            // Determine LOD level
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

            // Density sampling (stable ID hashing)
            bool shouldRender = true;
            if (Config.DensitySlider < 1f && lodState.CurrentLOD != PresentationLOD.LOD0_Full)
            {
                // Only sample entities outside LOD0 (always render close entities)
                int densityDivisor = (int)(1f / math.max(Config.DensitySlider, 0.01f));
                shouldRender = (entityIndex % densityDivisor) == 0;
            }

            lodState.ShouldRender = shouldRender ? (byte)1 : (byte)0;
        }
    }

    /// <summary>
    /// Job to update villager visual state from sim components (VillagerBehavior, VillagerAlignment).
    /// </summary>
    [BurstCompile]
    public partial struct UpdateVillagerVisualFromSimJob : IJobEntity
    {
        public void Execute(
            ref VillagerVisualState visualState,
            in VillagerBehavior behavior,
            in VillagerPresentationTag tag)
        {
            // Update alignment tint based on VengefulScore and BoldScore
            // Vengeful: Red tint (VengefulScore > 0)
            // Forgiving: Blue tint (VengefulScore < 0)
            // Bold: Bright saturation (BoldScore > 0)
            // Craven: Desaturated (BoldScore < 0)

            float vengefulFactor = math.clamp(behavior.VengefulScore / 100f, -1f, 1f);
            float boldFactor = math.clamp(behavior.BoldScore / 100f, -1f, 1f);

            // Base color (neutral gray)
            float4 baseColor = new float4(0.7f, 0.7f, 0.7f, 1f);

            // Apply vengeful/forgiving tint
            if (vengefulFactor > 0)
            {
                // Red tint for vengeful
                baseColor.x = math.lerp(0.7f, 1f, vengefulFactor);
                baseColor.y = math.lerp(0.7f, 0.3f, vengefulFactor);
                baseColor.z = math.lerp(0.7f, 0.3f, vengefulFactor);
            }
            else
            {
                // Blue tint for forgiving
                float forgFactor = -vengefulFactor;
                baseColor.x = math.lerp(0.7f, 0.3f, forgFactor);
                baseColor.y = math.lerp(0.7f, 0.5f, forgFactor);
                baseColor.z = math.lerp(0.7f, 1f, forgFactor);
            }

            // Apply bold/craven saturation
            if (boldFactor < 0)
            {
                // Desaturate for craven
                float desatFactor = -boldFactor;
                float gray = (baseColor.x + baseColor.y + baseColor.z) / 3f;
                baseColor.x = math.lerp(baseColor.x, gray, desatFactor * 0.5f);
                baseColor.y = math.lerp(baseColor.y, gray, desatFactor * 0.5f);
                baseColor.z = math.lerp(baseColor.z, gray, desatFactor * 0.5f);
            }
            else
            {
                // Increase saturation for bold
                float satFactor = boldFactor * 0.3f;
                baseColor.x = math.clamp(baseColor.x * (1f + satFactor), 0f, 1f);
                baseColor.y = math.clamp(baseColor.y * (1f + satFactor), 0f, 1f);
                baseColor.z = math.clamp(baseColor.z * (1f + satFactor), 0f, 1f);
            }

            visualState.AlignmentTint = baseColor;
        }
    }
}

