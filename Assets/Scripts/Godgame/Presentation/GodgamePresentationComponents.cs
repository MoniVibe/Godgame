using PureDOTS.Runtime.Components;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Godgame.Presentation
{
    // ECS TAGS
    public struct VillagerPresentationTag : IComponentData { }
    public struct VillageCenterPresentationTag : IComponentData { }
    public struct ResourceChunkPresentationTag : IComponentData { }
    public struct ResourceNodePresentationTag : IComponentData { }
    public struct VegetationPresentationTag : IComponentData { }

    public struct CameraTag : IComponentData { }

    public struct CameraTransform : IComponentData
    {
        public float3 Position;
        public quaternion Rotation;
    }

    // VISUAL STATE / LOD
    public enum PresentationLOD : byte
    {
        LOD0_Full = 0,
        LOD1_Mid = 1,
        LOD2_Far = 2,
        LOD3_Culled = 3
    }

    public struct PresentationLODState : IComponentData
    {
        public PresentationLOD CurrentLOD;
        public float DistanceToCamera;
        public byte ShouldRender;
    }

    public struct VillagerVisualState : IComponentData
    {
        public float4 AlignmentTint;
        public int TaskIconIndex;
        public int AnimationState;
        public int TaskState;
        public float EffectIntensity;
    }

    public enum VillagerTaskState : byte
    {
        None = 0,
        Idle = 1,
        Working = 2,
        Traveling = 3
    }

    public struct ResourceChunkVisualState : IComponentData
    {
        public float4 ResourceTypeTint;
        public float QuantityScale;
        public byte IsCarried;
    }

    public struct VegetationVisualState : IComponentData
    {
        public byte GrowthStage;
        public float Health;
        public byte IsClumped;
        public float4 BiomeTint;
    }

    public struct StaticVisualPrefab : IComponentData
    {
        public float3 LocalOffset;
        public quaternion LocalRotationOffset;
        public float ScaleMultiplier;
        public byte InheritParentScale;
    }

    public struct StaticVisualPrefabReference : IComponentData
    {
        public UnityObjectRef<UnityEngine.GameObject> Prefab;
    }

    public struct VillageCenterVisualState : IComponentData
    {
        public float4 PhaseTint;
        public float InfluenceRadius;
        public float Intensity;
    }

    // BIOME / CONFIG DATA
    public struct BiomePresentationData : IComponentData
    {
        public int BiomeId;
        public float Wetness;
        public float Fertility;
        public float Moisture;
    }

    public struct PresentationConfig : IComponentData
    {
        public float LOD0Distance;
        public float LOD1Distance;
        public float LOD2Distance;
        public float DensitySlider;
        public int MaxLOD0Villagers;
        public int MaxRenderedChunks;
        public float MaxFrameTimeMs;
        public bool ForceVillagersVisible; // debug-only toggle

        public static PresentationConfig Default => new PresentationConfig
        {
            LOD0Distance = 40f,
            LOD1Distance = 200f,
            LOD2Distance = 500f,
            DensitySlider = 1f,
            MaxLOD0Villagers = 1000,
            MaxRenderedChunks = 2000,
            MaxFrameTimeMs = 16.7f,
            ForceVillagersVisible = false
        };
    }

    public enum AggregateTrend : byte
    {
        Improving = 0,
        Stable = 1,
        Declining = 2
    }

    public struct AggregateTrendData : IComponentData
    {
        public AggregateTrend PopulationTrend;
        public AggregateTrend WealthTrend;
        public AggregateTrend FoodTrend;
        public AggregateTrend FertilityTrend;
        public float TrendIntensity;
    }

    public struct AggregateHistory : IBufferElementData
    {
        public uint Tick;
        public int Population;
        public int Wealth;
        public int Food;
        public float VegetationHealth;
        public float Fertility;
    }

}
