using PureDOTS.Runtime.Spatial;
using Unity.Entities;
using UnityEngine;

namespace Godgame.Registry.Authoring
{
    /// <summary>
    /// Base authoring component for entities that should be registered with PureDOTS registries.
    /// Ensures entities are tagged for spatial grid participation and registry sync.
    /// </summary>
    [DisallowMultipleComponent]
    public class RegistryAuthoring : MonoBehaviour
    {
        [Header("Registry Configuration")]
        [Tooltip("If true, this entity will be indexed in the spatial grid")]
        public bool participateInSpatialGrid = true;

        private void OnValidate()
        {
            // Ensure spatial grid participation is configured correctly
        }
    }

    /// <summary>
    /// Baker for RegistryAuthoring - adds SpatialIndexedTag for spatial grid participation.
    /// </summary>
    public class RegistryAuthoringBaker : Baker<RegistryAuthoring>
    {
        public override void Bake(RegistryAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            
            if (authoring.participateInSpatialGrid)
            {
                AddComponent<SpatialIndexedTag>(entity);
            }
            
            // TODO: Add registry-specific tags/components based on entity type
            // This will be extended by specific authoring components (VillagerAuthoring, etc.)
        }
    }
}

