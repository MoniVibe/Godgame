using PureDOTS.Runtime.Navigation;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Godgame.Authoring.Navigation
{
    /// <summary>
    /// Authoring component for building NavGraph from terrain.
    /// Phase 1: Simple grid-based graph from heightmap.
    /// Phase 2: Advanced terrain analysis, obstacle detection, etc.
    /// </summary>
    public class GodgameNavGraphAuthoring : MonoBehaviour
    {
        [Tooltip("Graph cell size")]
        public float CellSize = 5f;

        [Tooltip("Graph bounds min")]
        public Vector3 BoundsMin = new Vector3(-100f, 0f, -100f);

        [Tooltip("Graph bounds max")]
        public Vector3 BoundsMax = new Vector3(100f, 0f, 100f);

        [Tooltip("Maximum slope angle (degrees) for ground movement")]
        public float MaxSlopeAngle = 45f;

        [Tooltip("Heightmap texture (optional, for terrain-based graph)")]
        public Texture2D Heightmap;

        [Tooltip("Obstacle layer mask")]
        public LayerMask ObstacleLayers;
    }

    /// <summary>
    /// Baker for GodgameNavGraphAuthoring.
    /// Creates NavGraph singleton with nodes and edges.
    /// </summary>
    public class GodgameNavGraphBaker : Baker<GodgameNavGraphAuthoring>
    {
        public override void Bake(GodgameNavGraphAuthoring authoring)
        {
            // Create graph singleton entity
            var graphEntity = CreateAdditionalEntity(TransformUsageFlags.None);

            // Add NavGraph component
            AddComponent(graphEntity, new NavGraph
            {
                Version = 1,
                NodeCount = 0, // Will be populated by runtime system
                EdgeCount = 0,
                BoundsMin = authoring.BoundsMin,
                BoundsMax = authoring.BoundsMax
            });

            // Add NavNode and NavEdge buffers (empty, populated at runtime)
            AddBuffer<NavNode>(graphEntity);
            AddBuffer<NavEdge>(graphEntity);

            // TODO Phase 2: Pre-bake nodes/edges from heightmap and obstacles
            // For Phase 1, runtime system will build graph dynamically
        }
    }
}

