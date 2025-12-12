#if LEGACY_PRESENTATION_ARCHIVE_ENABLED
using Godgame.Demo;
using PureDOTS.Environment;
using PureDOTS.Runtime.Camera;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Godgame.Presentation
{
    /// <summary>
    /// System that updates vegetation visual state from PlantStand components.
    /// Applies LOD and density sampling (aggressive: only dense near camera).
    /// Reacts to biome changes.
    /// </summary>
    [BurstCompile]
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    [UpdateAfter(typeof(Godgame_BiomeDataHookSystem))]
    public partial struct Godgame_VegetationPresentationSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<PresentationConfig>();
            // CameraRigState is not IComponentData, cannot use RequireForUpdate
            // Will get camera position from MonoBehaviour or alternative source
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
            // This will fail at runtime since CameraRigState is not IComponentData
            // var cameraState = SystemAPI.GetSingleton<CameraRigState>();
            // var cameraPosition = cameraState.Position;
#endif

            var job = new UpdateVegetationVisualStateJob
            {
                CameraPosition = cameraPosition,
                LOD0Distance = config.LOD0Distance,
                LOD1Distance = config.LOD1Distance,
                LOD2Distance = config.LOD2Distance,
                DensitySlider = config.DensitySlider
            };
            job.ScheduleParallel();
        }
    }

    /// <summary>
    /// Job that updates vegetation visual state.
    /// Note: This is a placeholder - actual implementation requires PlantStand component from PureDOTS.
    /// </summary>
    [BurstCompile]
    public partial struct UpdateVegetationVisualStateJob : IJobEntity
    {
        public float3 CameraPosition;
        public float LOD0Distance;
        public float LOD1Distance;
        public float LOD2Distance;
        public float DensitySlider;

        public void Execute(
            ref VegetationVisualState visualState,
            ref PresentationLODState lodState,
            in VegetationPresentationTag tag,
            in LocalTransform transform)
        {
            // Calculate distance to camera
            lodState.DistanceToCamera = math.distance(transform.Position, CameraPosition);

            // Determine LOD
            if (lodState.DistanceToCamera < LOD0Distance)
            {
                lodState.CurrentLOD = PresentationLOD.LOD0_Full;
            }
            else if (lodState.DistanceToCamera < LOD1Distance)
            {
                lodState.CurrentLOD = PresentationLOD.LOD1_Mid;
            }
            else if (lodState.DistanceToCamera < LOD2Distance)
            {
                lodState.CurrentLOD = PresentationLOD.LOD2_Far;
            }
            else
            {
                lodState.CurrentLOD = PresentationLOD.Culled;
            }

            // Apply density sampling for mid/far LODs
            bool shouldRender = true;
            if (lodState.CurrentLOD > PresentationLOD.LOD0_Full && DensitySlider < 1.0f)
            {
                // Stable ID hashing for sampling - use Burst-safe math.hash instead of GetHashCode()
                uint hash = math.hash(transform.Position);
                shouldRender = (hash % (uint)(1.0f / DensitySlider)) == 0;
            }

            lodState.ShouldRender = shouldRender ? (byte)1 : (byte)0;

            // Update visual state based on LOD
            if (lodState.CurrentLOD == PresentationLOD.LOD0_Full)
            {
                visualState.IsClumped = 0; // Individual entities
            }
            else if (lodState.CurrentLOD == PresentationLOD.LOD1_Mid)
            {
                visualState.IsClumped = 1; // Clumped/instanced
            }
            else
            {
                visualState.IsClumped = 1; // Aggregated or hidden
            }

            // TODO: Read PlantStand component when available
            // For now, use placeholder values
            visualState.GrowthStage = 2; // Mature
            visualState.Health = 1f;
            visualState.BiomeTint = new float4(1f, 1f, 1f, 1f); // Default white
        }
    }

    /// <summary>
    /// Job that updates vegetation biome reactivity.
    /// Reads BiomePresentationData from nearby ground tiles and adjusts vegetation appearance.
    /// </summary>
    [BurstCompile]
    public partial struct UpdateVegetationBiomeReactivityJob : IJobEntity
    {
        [ReadOnly] public ComponentLookup<BiomePresentationData> BiomeDataLookup;
        [ReadOnly] public ComponentLookup<Godgame.Demo.GroundTile> GroundTileLookup;

        public void Execute(
            ref VegetationVisualState visualState,
            in VegetationPresentationTag tag,
            in LocalTransform transform)
        {
            // TODO: Implement spatial query to find nearest ground tile with biome data
            // IJobEntity cannot use SystemAPI.Query - need to use ComponentLookup or spatial queries
            // For now, use placeholder values
            // The actual implementation would:
            // 1. Use spatial partitioning (e.g., PureDOTS spatial grid) to find nearby tiles
            // 2. Or use ComponentLookup to query ground tiles by position
            // 3. Read BiomePresentationData from the nearest tile
            // 4. Adjust visual state based on biome compatibility
            
            // Placeholder: Use default values
            visualState.BiomeTint = new float4(1f, 1f, 1f, 1f);
            visualState.Health = 1f;
        }
    }
}
#endif
