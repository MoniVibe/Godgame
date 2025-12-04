using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using PureDOTS.Runtime.Components;

namespace Godgame.TimeDebug
{
    /// <summary>
    /// Debug visualization for rewind - draws debug lines showing past positions.
    /// Attach to a GameObject in the scene to enable visualization.
    /// </summary>
    public class GodgameRewindVisualization : MonoBehaviour
    {
        [Header("Visualization Settings")]
        [Tooltip("Enable visualization")]
        public bool enableVisualization = true;
        
        [Tooltip("Maximum number of history samples to visualize")]
        public int maxSamplesToShow = 20;
        
        [Tooltip("Color for rewind trail")]
        public Color trailColor = new Color(1f, 0f, 0f, 0.5f);
        
        [Tooltip("Only visualize entities with this tag (leave empty for all)")]
        public string filterTag = "";

        private World _world;

        private void OnEnable()
        {
            _world = World.DefaultGameObjectInjectionWorld;
        }

        private void OnDrawGizmos()
        {
            if (!enableVisualization || _world == null || !_world.IsCreated)
            {
                return;
            }

            var entityManager = _world.EntityManager;
            
            // Check if we're in rewind mode
            if (!entityManager.CreateEntityQuery(typeof(RewindState)).IsEmptyIgnoreFilter)
            {
                var rewindState = entityManager.CreateEntityQuery(typeof(RewindState)).GetSingleton<RewindState>();
                if (rewindState.Mode != RewindMode.Playback)
                {
                    return; // Only visualize during playback
                }
            }

            // Draw history trails for entities with ComponentHistory<LocalTransform>
            var query = entityManager.CreateEntityQuery(
                ComponentType.ReadOnly<HistoryProfile>(),
                ComponentType.ReadOnly<LocalTransform>(),
                ComponentType.ReadOnly<ComponentHistory<LocalTransform>>(),
                ComponentType.ReadOnly<RewindableTag>());

            if (query.IsEmptyIgnoreFilter)
            {
                return;
            }

            Gizmos.color = trailColor;
            
            var entities = query.ToEntityArray(Unity.Collections.Allocator.Temp);
            foreach (var entity in entities)
            {
                var transform = entityManager.GetComponentData<LocalTransform>(entity);
                var historyBuffer = entityManager.GetBuffer<ComponentHistory<LocalTransform>>(entity);
                
                if (historyBuffer.Length == 0)
                {
                    continue;
                }

                // Draw trail from current position backwards through history
                float3 previousPos = transform.Position;
                int samplesDrawn = 0;
                
                for (int i = historyBuffer.Length - 1; i >= 0 && samplesDrawn < maxSamplesToShow; i--)
                {
                    var sample = historyBuffer[i];
                    float3 samplePos = sample.Value.Position;
                    
                    // Draw line from previous position to this sample
                    Gizmos.DrawLine(previousPos, samplePos);
                    
                    previousPos = samplePos;
                    samplesDrawn++;
                }
            }
            
            entities.Dispose();
        }
    }
}


