using Unity.Entities;
using UnityEngine;

namespace Godgame.Presentation.Authoring
{
    // Scenario-level knobs for presentation; baked into PresentationConfig.
    public class PresentationConfigAuthoring : MonoBehaviour
    {
        [Header("LOD Distances")]
        public float LOD0Distance = 40f;
        public float LOD1Distance = 200f;
        public float LOD2Distance = 500f;

        [Header("Density & Budgets")]
        [Range(0.01f, 1f)] public float DensitySlider = 1f;
        public int MaxLOD0Villagers = 1000;
        public int MaxRenderedChunks = 2000;

        [Header("Performance")]
        public float MaxFrameTimeMs = 16.7f;
    }

    public class PresentationConfigBaker : Baker<PresentationConfigAuthoring>
    {
        public override void Bake(PresentationConfigAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.None);

            AddComponent(entity, new Godgame.Presentation.PresentationConfig
            {
                LOD0Distance = authoring.LOD0Distance,
                LOD1Distance = authoring.LOD1Distance,
                LOD2Distance = authoring.LOD2Distance,
                DensitySlider = authoring.DensitySlider,
                MaxLOD0Villagers = authoring.MaxLOD0Villagers,
                MaxRenderedChunks = authoring.MaxRenderedChunks,
                MaxFrameTimeMs = authoring.MaxFrameTimeMs
            });
        }
    }

    public class PresentationLayerConfigAuthoring : MonoBehaviour
    {
        [Header("Layer Distance Multipliers")]
        public float ColonyMultiplier = PresentationLayerConfig.Default.ColonyMultiplier;
        public float IslandMultiplier = PresentationLayerConfig.Default.IslandMultiplier;
        public float ContinentMultiplier = PresentationLayerConfig.Default.ContinentMultiplier;
        public float PlanetMultiplier = PresentationLayerConfig.Default.PlanetMultiplier;
        public float OrbitalMultiplier = PresentationLayerConfig.Default.OrbitalMultiplier;
        public float SystemMultiplier = PresentationLayerConfig.Default.SystemMultiplier;
        public float GalacticMultiplier = PresentationLayerConfig.Default.GalacticMultiplier;
    }

    public class PresentationLayerConfigBaker : Baker<PresentationLayerConfigAuthoring>
    {
        public override void Bake(PresentationLayerConfigAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.None);

            AddComponent(entity, new Godgame.Presentation.PresentationLayerConfig
            {
                ColonyMultiplier = authoring.ColonyMultiplier,
                IslandMultiplier = authoring.IslandMultiplier,
                ContinentMultiplier = authoring.ContinentMultiplier,
                PlanetMultiplier = authoring.PlanetMultiplier,
                OrbitalMultiplier = authoring.OrbitalMultiplier,
                SystemMultiplier = authoring.SystemMultiplier,
                GalacticMultiplier = authoring.GalacticMultiplier
            });
        }
    }

    public class PresentationScaleConfigAuthoring : MonoBehaviour
    {
        [Header("Layer Scale Multipliers")]
        public float ColonyMultiplier = PresentationScaleConfig.Default.ColonyMultiplier;
        public float IslandMultiplier = PresentationScaleConfig.Default.IslandMultiplier;
        public float ContinentMultiplier = PresentationScaleConfig.Default.ContinentMultiplier;
        public float PlanetMultiplier = PresentationScaleConfig.Default.PlanetMultiplier;
        public float OrbitalMultiplier = PresentationScaleConfig.Default.OrbitalMultiplier;
        public float SystemMultiplier = PresentationScaleConfig.Default.SystemMultiplier;
        public float GalacticMultiplier = PresentationScaleConfig.Default.GalacticMultiplier;
    }

    public class PresentationScaleConfigBaker : Baker<PresentationScaleConfigAuthoring>
    {
        public override void Bake(PresentationScaleConfigAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.None);

            AddComponent(entity, new Godgame.Presentation.PresentationScaleConfig
            {
                ColonyMultiplier = authoring.ColonyMultiplier,
                IslandMultiplier = authoring.IslandMultiplier,
                ContinentMultiplier = authoring.ContinentMultiplier,
                PlanetMultiplier = authoring.PlanetMultiplier,
                OrbitalMultiplier = authoring.OrbitalMultiplier,
                SystemMultiplier = authoring.SystemMultiplier,
                GalacticMultiplier = authoring.GalacticMultiplier
            });
        }
    }

    // Base class for biome/terrain binding authoring; extend as needed.
    public class SwappablePresentationBindingAuthoring : MonoBehaviour
    {
        public int BiomeId;
    }
}
