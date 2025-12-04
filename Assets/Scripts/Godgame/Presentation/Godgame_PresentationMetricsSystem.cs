using Godgame.Economy;
using Godgame.Villages;
using Godgame.Villagers;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

namespace Godgame.Presentation
{
    /// <summary>
    /// System that collects presentation metrics for profiling and debugging.
    /// Tracks LOD breakdown, render counts, and performance data.
    /// </summary>
    [BurstCompile]
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    [UpdateAfter(typeof(Godgame_VillagerPresentationSystem))]
    public partial struct Godgame_PresentationMetricsSystem : ISystem
    {
        private int _frameCounter;
        private const int MetricsUpdateInterval = 30; // Update every 30 frames

        public void OnCreate(ref SystemState state)
        {
            // Create metrics singleton if not exists
            var query = state.GetEntityQuery(typeof(PresentationMetrics));
            if (query.IsEmpty)
            {
                var metricsEntity = state.EntityManager.CreateEntity();
                state.EntityManager.AddComponentData(metricsEntity, new PresentationMetrics());
                state.EntityManager.SetName(metricsEntity, "PresentationMetrics");
            }
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            _frameCounter++;
            if (_frameCounter < MetricsUpdateInterval)
            {
                return;
            }
            _frameCounter = 0;

            // Count simulated villagers (all entities with VillagerBehavior)
            int villagersSimulated = 0;
            foreach (var _ in SystemAPI.Query<RefRO<VillagerBehavior>>())
            {
                villagersSimulated++;
            }

            // Count rendered villagers by LOD
            int villagersLOD0 = 0;
            int villagersLOD1 = 0;
            int villagersLOD2 = 0;
            int villagersTotal = 0; // Total with presentation tag
            int villagersRendered = 0;

            foreach (var (lodState, _) in SystemAPI.Query<RefRO<PresentationLODState>, RefRO<VillagerPresentationTag>>())
            {
                villagersTotal++;
                if (lodState.ValueRO.ShouldRender == 1)
                {
                    villagersRendered++;
                    switch (lodState.ValueRO.CurrentLOD)
                    {
                        case PresentationLOD.LOD0_Full:
                            villagersLOD0++;
                            break;
                        case PresentationLOD.LOD1_Mid:
                            villagersLOD1++;
                            break;
                        case PresentationLOD.LOD2_Far:
                            villagersLOD2++;
                            break;
                    }
                }
            }

            // Count simulated chunks (all entities with ExtractedResource)
            int chunksSimulated = 0;
            foreach (var _ in SystemAPI.Query<RefRO<ExtractedResource>>())
            {
                chunksSimulated++;
            }

            // Count rendered chunks
            int chunksTotal = 0; // Total with presentation tag
            int chunksRendered = 0;
            foreach (var (lodState, _) in SystemAPI.Query<RefRO<PresentationLODState>, RefRO<ResourceChunkPresentationTag>>())
            {
                chunksTotal++;
                if (lodState.ValueRO.ShouldRender == 1)
                {
                    chunksRendered++;
                }
            }

            // Count simulated villages (all entities with Village component)
            int villagesSimulated = 0;
            foreach (var _ in SystemAPI.Query<RefRO<Village>>())
            {
                villagesSimulated++;
            }

            // Count rendered villages
            int villagesTotal = 0; // Total with presentation tag
            int villagesRendered = 0;
            foreach (var (lodState, _) in SystemAPI.Query<RefRO<PresentationLODState>, RefRO<VillageCenterPresentationTag>>())
            {
                villagesTotal++;
                if (lodState.ValueRO.ShouldRender == 1)
                {
                    villagesRendered++;
                }
            }

            // Count resource nodes
            int resourceNodesTotal = 0;
            int resourceNodesRendered = 0;
            foreach (var (lodState, _) in SystemAPI.Query<RefRO<PresentationLODState>, RefRO<ResourceNodePresentationTag>>())
            {
                resourceNodesTotal++;
                if (lodState.ValueRO.ShouldRender == 1)
                {
                    resourceNodesRendered++;
                }
            }

            // Get density slider from config
            float densitySlider = 1f;
            if (SystemAPI.TryGetSingleton<PresentationConfig>(out var config))
            {
                densitySlider = config.DensitySlider;
            }

            // Update metrics singleton
            foreach (var metricsRef in SystemAPI.Query<RefRW<PresentationMetrics>>())
            {
                ref var metrics = ref metricsRef.ValueRW;

                // Simulated counts
                metrics.VillagersSimulated = villagersSimulated;
                metrics.ChunksSimulated = chunksSimulated;
                metrics.VillagesSimulated = villagesSimulated;

                // Rendered counts (with presentation tags)
                metrics.VillagersTotal = villagersTotal;
                metrics.VillagersRendered = villagersRendered;
                metrics.VillagersLOD0 = villagersLOD0;
                metrics.VillagersLOD1 = villagersLOD1;
                metrics.VillagersLOD2 = villagersLOD2;

                metrics.ChunksTotal = chunksTotal;
                metrics.ChunksRendered = chunksRendered;

                metrics.VillagesTotal = villagesTotal;
                metrics.VillagesRendered = villagesRendered;

                metrics.ResourceNodesTotal = resourceNodesTotal;
                metrics.ResourceNodesRendered = resourceNodesRendered;

                // Calculate render ratio (rendered vs simulated)
                metrics.RenderRatio = villagersSimulated > 0
                    ? (float)villagersRendered / villagersSimulated
                    : 1f;

                // Density sampling ratio (actual ratio used)
                metrics.DensitySamplingRatio = densitySlider;
            }
        }
    }

    /// <summary>
    /// Singleton component storing presentation metrics.
    /// </summary>
    public struct PresentationMetrics : IComponentData
    {
        // Simulated counts (all entities in simulation)
        public int VillagersSimulated;
        public int ChunksSimulated;
        public int VillagesSimulated;

        // Rendered counts (entities with presentation tags)
        public int VillagersTotal;
        public int VillagersRendered;
        public int VillagersLOD0;
        public int VillagersLOD1;
        public int VillagersLOD2;

        // Chunk metrics
        public int ChunksTotal;
        public int ChunksRendered;

        // Village metrics
        public int VillagesTotal;
        public int VillagesRendered;

        // Resource node metrics
        public int ResourceNodesTotal;
        public int ResourceNodesRendered;

        // Overall metrics
        public float RenderRatio; // Rendered / Simulated
        public float DensitySamplingRatio; // Actual density slider value used
    }

    /// <summary>
    /// MonoBehaviour to display presentation metrics in the editor or runtime.
    /// </summary>
    public class PresentationMetricsDisplay : UnityEngine.MonoBehaviour
    {
        [UnityEngine.Header("Display Settings")]
        [UnityEngine.SerializeField] private bool showMetrics = true;

        private PresentationMetrics _cachedMetrics;
        private bool _hasMetrics;
        private Unity.Entities.World _ecsWorld;

        private void Start()
        {
            _ecsWorld = Unity.Entities.World.DefaultGameObjectInjectionWorld;
        }

        private void Update()
        {
            // Read input from ECS
            if (_ecsWorld != null && _ecsWorld.IsCreated)
            {
                var em = _ecsWorld.EntityManager;
                var debugQuery = em.CreateEntityQuery(typeof(Godgame.Input.DebugInput));
                if (!debugQuery.IsEmpty)
                {
                    var debugInput = debugQuery.GetSingleton<Godgame.Input.DebugInput>();
                    if (debugInput.TogglePresentationMetrics == 1)
                    {
                        showMetrics = !showMetrics;
                    }
                }

                // Try to get metrics from ECS
                var metricsQuery = em.CreateEntityQuery(typeof(PresentationMetrics));
                if (!metricsQuery.IsEmpty)
                {
                    _cachedMetrics = metricsQuery.GetSingleton<PresentationMetrics>();
                    _hasMetrics = true;
                }
            }
        }

        private void OnGUI()
        {
            if (!showMetrics || !_hasMetrics)
            {
                return;
            }

            var boxStyle = new UnityEngine.GUIStyle(UnityEngine.GUI.skin.box);
            boxStyle.alignment = UnityEngine.TextAnchor.UpperLeft;
            boxStyle.padding = new UnityEngine.RectOffset(10, 10, 10, 10);

            UnityEngine.GUILayout.BeginArea(new UnityEngine.Rect(10, 10, 280, 220), boxStyle);

            UnityEngine.GUILayout.Label("<b>Presentation Metrics</b>", new UnityEngine.GUIStyle(UnityEngine.GUI.skin.label) { richText = true });
            UnityEngine.GUILayout.Space(5);

            UnityEngine.GUILayout.Label($"Villagers: {_cachedMetrics.VillagersRendered}/{_cachedMetrics.VillagersSimulated} rendered");
            UnityEngine.GUILayout.Label($"  LOD0: {_cachedMetrics.VillagersLOD0}  LOD1: {_cachedMetrics.VillagersLOD1}  LOD2: {_cachedMetrics.VillagersLOD2}");
            UnityEngine.GUILayout.Space(3);

            UnityEngine.GUILayout.Label($"Chunks: {_cachedMetrics.ChunksRendered}/{_cachedMetrics.ChunksSimulated} rendered");
            UnityEngine.GUILayout.Label($"Villages: {_cachedMetrics.VillagesRendered}/{_cachedMetrics.VillagesSimulated} rendered");
            UnityEngine.GUILayout.Label($"Resource Nodes: {_cachedMetrics.ResourceNodesRendered}/{_cachedMetrics.ResourceNodesTotal}");
            UnityEngine.GUILayout.Space(5);

            UnityEngine.GUILayout.Label($"Render Ratio: {_cachedMetrics.RenderRatio:P1}");
            UnityEngine.GUILayout.Label($"Density Sampling: {_cachedMetrics.DensitySamplingRatio:P1}");
            UnityEngine.GUILayout.Space(5);

            UnityEngine.GUILayout.Label("<i>Press O to toggle</i>", new UnityEngine.GUIStyle(UnityEngine.GUI.skin.label) { richText = true });

            UnityEngine.GUILayout.EndArea();
        }
    }
}

