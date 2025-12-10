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

    // Base class for biome/terrain binding authoring; extend as needed.
    public class SwappablePresentationBindingAuthoring : MonoBehaviour
    {
        public int BiomeId;
    }
}
