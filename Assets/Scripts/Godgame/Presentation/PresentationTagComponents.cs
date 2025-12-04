using Unity.Entities;

namespace Godgame.Presentation
{
    /// <summary>
    /// Tag component for villager presentation entities.
    /// Entities with this tag will be processed by Godgame_VillagerPresentationSystem.
    /// </summary>
    public struct VillagerPresentationTag : IComponentData { }

    /// <summary>
    /// Tag component for resource chunk presentation entities.
    /// Entities with this tag will be processed by Godgame_ResourceChunkPresentationSystem.
    /// </summary>
    public struct ResourceChunkPresentationTag : IComponentData { }

    /// <summary>
    /// Tag component for village center presentation entities.
    /// Entities with this tag will be processed by Godgame_VillagePresentationSystem.
    /// </summary>
    public struct VillageCenterPresentationTag : IComponentData { }

    /// <summary>
    /// Tag component for resource node presentation entities.
    /// </summary>
    public struct ResourceNodePresentationTag : IComponentData { }

    /// <summary>
    /// LOD level for presentation entities.
    /// </summary>
    public enum PresentationLOD : byte
    {
        /// <summary>Full detail, close range (less than 50 units)</summary>
        LOD0_Full = 0,
        /// <summary>Simplified, mid range (50-200 units)</summary>
        LOD1_Mid = 1,
        /// <summary>Impostor/sampled, far range (greater than 200 units)</summary>
        LOD2_Far = 2,
        /// <summary>Not rendered (beyond render distance or culled)</summary>
        Culled = 3
    }

    /// <summary>
    /// Component storing current LOD state for presentation entities.
    /// </summary>
    public struct PresentationLODState : IComponentData
    {
        public float DistanceToCamera;
        public PresentationLOD CurrentLOD;
        public byte ShouldRender; // 0 = false, 1 = true
    }

    /// <summary>
    /// Visual state ID for villagers.
    /// </summary>
    public enum VillagerVisualStateId : byte
    {
        Idle,
        Walking,
        Gathering,
        Carrying,
        Throwing,
        MiracleAffected,
    }

    /// <summary>
    /// Component storing visual state for villagers.
    /// </summary>
    public struct VillagerVisualState : IComponentData
    {
        public Unity.Mathematics.float4 AlignmentTint;
        public int TaskIconIndex;
        public int AnimationState;
        public float EffectIntensity;
        public VillagerTaskState TaskState;
        public float TaskStateIntensity;
    }

    /// <summary>
    /// Visual state ID for resource chunks.
    /// </summary>
    public enum ResourceChunkVisualStateId : byte
    {
        Normal,
        Carried,
        Shredded,
    }

    /// <summary>
    /// Component storing visual state for resource chunks.
    /// </summary>
    public struct ResourceChunkVisualState : IComponentData
    {
        public Unity.Mathematics.float4 ResourceTypeTint;
        public float QuantityScale;
        public byte IsCarried; // 0 = false, 1 = true
    }

    /// <summary>
    /// Visual state ID for village centers.
    /// </summary>
    public enum VillageCenterVisualStateId : byte
    {
        Normal,
        Prosperous,
        Starving,
        UnderMiracle,
        Crisis,
    }

    /// <summary>
    /// Component storing visual state for village centers.
    /// </summary>
    public struct VillageCenterVisualState : IComponentData
    {
        public Unity.Mathematics.float4 PhaseTint;
        public float InfluenceRadius;
        public float Intensity;
        public VillageAggregateState AggregateState;
        public float AggregateStateIntensity;
    }

    /// <summary>
    /// Configuration singleton for presentation system parameters.
    /// </summary>
    public struct PresentationConfig : IComponentData
    {
        // Density / budgets
        public float DensitySlider;

        public float MaxFrameTimeMs;
        public int MaxLOD0Villagers;
        public int MaxRenderedChunks;

        // LOD distance thresholds
        public float LOD0Distance;
        public float LOD1Distance;
        public float LOD2Distance;

        public static PresentationConfig Default => new PresentationConfig
        {
            DensitySlider = 1f,

            MaxFrameTimeMs = 16.67f,
            MaxLOD0Villagers = 10_000,
            MaxRenderedChunks = 5_000,

            LOD0Distance = 20f,
            LOD1Distance = 50f,
            LOD2Distance = 100f,
        };
    }

}
