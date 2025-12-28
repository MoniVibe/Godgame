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
    public struct AggregatePilePresentationTag : IComponentData { }
    public struct CarriedResourcePresentationTag : IComponentData { }
    public struct ResourceNodePresentationTag : IComponentData { }
    public struct VegetationPresentationTag : IComponentData { }
    public struct StorehousePresentationTag : IComponentData { }
    public struct HousingPresentationTag : IComponentData { }
    public struct WorshipPresentationTag : IComponentData { }
    public struct ConstructionGhostPresentationTag : IComponentData { }
    public struct BandPresentationTag : IComponentData { }

    // Presentation layers (distance buckets)
    public enum PresentationLayerId : byte
    {
        Colony = 0,
        Island = 1,
        Continent = 2,
        Planet = 3,
        Orbital = 4,
        System = 5,
        Galactic = 6
    }

    public struct PresentationLayer : IComponentData
    {
        public PresentationLayerId Value;
    }

    public struct CameraTag : IComponentData { }

    public struct CameraTransform : IComponentData
    {
        public float3 Position;
        public quaternion Rotation;
    }

    public struct CarriedResourceLink : IComponentData
    {
        public Entity VisualEntity;
    }

    public struct CarriedResourceParent : IComponentData
    {
        public Entity Value;
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

    public struct PresentationExpiryHint : IComponentData
    {
        public float SecondsRemaining;
        public float SecondsTotal;
        public byte IsActive;
    }

    public struct PresentationExpiryBaseTint : IComponentData
    {
        public float4 Value;
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

    public struct PresentationConfigRuntimeTag : IComponentData
    {
    }

    public struct PresentationLayerConfig : IComponentData
    {
        public float ColonyMultiplier;
        public float IslandMultiplier;
        public float ContinentMultiplier;
        public float PlanetMultiplier;
        public float OrbitalMultiplier;
        public float SystemMultiplier;
        public float GalacticMultiplier;

        public static PresentationLayerConfig Default => new PresentationLayerConfig
        {
            ColonyMultiplier = 0.15f,
            IslandMultiplier = 0.3f,
            ContinentMultiplier = 0.6f,
            PlanetMultiplier = 1f,
            OrbitalMultiplier = 2f,
            SystemMultiplier = 6f,
            GalacticMultiplier = 20f
        };
    }

    public struct PresentationScaleConfig : IComponentData
    {
        public float ColonyMultiplier;
        public float IslandMultiplier;
        public float ContinentMultiplier;
        public float PlanetMultiplier;
        public float OrbitalMultiplier;
        public float SystemMultiplier;
        public float GalacticMultiplier;

        public static PresentationScaleConfig Default => new PresentationScaleConfig
        {
            ColonyMultiplier = 1f,
            IslandMultiplier = 1.1f,
            ContinentMultiplier = 1.2f,
            PlanetMultiplier = 1.35f,
            OrbitalMultiplier = 1.6f,
            SystemMultiplier = 2f,
            GalacticMultiplier = 3f
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
