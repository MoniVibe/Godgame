using PureDOTS.Authoring;
using PureDOTS.Runtime.Spatial;
using Unity.Entities;
using UnityEngine;

namespace Godgame.Registry.Authoring
{
    /// <summary>
    /// Lightweight copy of the shared SpatialPartitionAuthoring that lets the Godgame project
    /// reference a SpatialPartitionProfile without depending on editor-only packages.
    /// Ensures the spatial grid singletons/buffers exist so registry systems can publish
    /// spatial tokens out of the box.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class GodgameSpatialBootstrapAuthoring : MonoBehaviour
    {
        [Tooltip("Spatial partition profile that drives the grid bounds/cell sizing for the demo scene.")]
        public SpatialPartitionProfile profile;

        [Header("Optional Environment Grid")]
        [Tooltip("Environment grid config that seeds moisture/temperature/sunlight defaults for the demo loop.")]
        public EnvironmentGridConfig environmentConfig;
    }

    public sealed class GodgameSpatialBootstrapBaker : Baker<GodgameSpatialBootstrapAuthoring>
    {
        public override void Bake(GodgameSpatialBootstrapAuthoring authoring)
        {
            if (authoring.profile == null)
            {
                Debug.LogWarning("GodgameSpatialBootstrapAuthoring has no profile asset assigned.", authoring);
                return;
            }

            var entity = GetEntity(TransformUsageFlags.None);
            BootstrapSpatialGrid(authoring, entity);
            BootstrapEnvironmentGrid(authoring, entity);
        }

        private void BootstrapSpatialGrid(GodgameSpatialBootstrapAuthoring authoring, Entity entity)
        {
            var config = authoring.profile.ToComponent();
            var state = CreateDefaultState();
            var thresholds = new SpatialRebuildThresholds
            {
                MaxDirtyOpsForPartialRebuild = authoring.profile.MaxDirtyOpsForPartialRebuild,
                MaxDirtyRatioForPartialRebuild = authoring.profile.MaxDirtyRatioForPartialRebuild,
                MinEntryCountForPartialRebuild = authoring.profile.MinEntryCountForPartialRebuild
            };

            if (TryPatchExistingSingleton(config, state, thresholds))
            {
                return;
            }

            AddComponent(entity, config);
            AddComponent(entity, state);
            AddComponent(entity, thresholds);
            AddBuffer<SpatialGridCellRange>(entity);
            AddBuffer<SpatialGridEntry>(entity);
            AddBuffer<SpatialGridStagingEntry>(entity);
            AddBuffer<SpatialGridStagingCellRange>(entity);
            AddBuffer<SpatialGridEntryLookup>(entity);
            AddBuffer<SpatialGridDirtyOp>(entity);
        }

        private void BootstrapEnvironmentGrid(GodgameSpatialBootstrapAuthoring authoring, Entity entity)
        {
            if (authoring.environmentConfig == null)
            {
                Debug.LogWarning("GodgameSpatialBootstrapAuthoring is missing an EnvironmentGridConfig asset. Environment systems will use defaults.", authoring);
                return;
            }

            AddComponent(entity, authoring.environmentConfig.ToComponent());
        }

        private static bool TryPatchExistingSingleton(SpatialGridConfig config, SpatialGridState state, SpatialRebuildThresholds thresholds)
        {
            var world = World.DefaultGameObjectInjectionWorld;
            if (world == null || !world.IsCreated)
            {
                return false;
            }

            var entityManager = world.EntityManager;
            using var query = entityManager.CreateEntityQuery(ComponentType.ReadOnly<SpatialGridConfig>());
            if (query.IsEmptyIgnoreFilter)
            {
                return false;
            }

            var gridEntity = query.GetSingletonEntity();
            entityManager.SetComponentData(gridEntity, config);

            if (entityManager.HasComponent<SpatialGridState>(gridEntity))
            {
                entityManager.SetComponentData(gridEntity, state);
            }
            else
            {
                entityManager.AddComponentData(gridEntity, state);
            }

            EnsureBuffer<SpatialGridCellRange>(entityManager, gridEntity);
            EnsureBuffer<SpatialGridEntry>(entityManager, gridEntity);
            EnsureBuffer<SpatialGridStagingEntry>(entityManager, gridEntity);
            EnsureBuffer<SpatialGridStagingCellRange>(entityManager, gridEntity);
            EnsureBuffer<SpatialGridEntryLookup>(entityManager, gridEntity);
            EnsureBuffer<SpatialGridDirtyOp>(entityManager, gridEntity);

            if (entityManager.HasComponent<SpatialRebuildThresholds>(gridEntity))
            {
                entityManager.SetComponentData(gridEntity, thresholds);
            }
            else
            {
                entityManager.AddComponentData(gridEntity, thresholds);
            }

            return true;
        }

        private static void EnsureBuffer<T>(EntityManager entityManager, Entity entity) where T : unmanaged, IBufferElementData
        {
            if (!entityManager.HasBuffer<T>(entity))
            {
                entityManager.AddBuffer<T>(entity);
            }
        }

        private static SpatialGridState CreateDefaultState()
        {
            return new SpatialGridState
            {
                ActiveBufferIndex = 0,
                TotalEntries = 0,
                Version = 0,
                LastUpdateTick = 0,
                LastDirtyTick = 0,
                DirtyVersion = 0,
                DirtyAddCount = 0,
                DirtyUpdateCount = 0,
                DirtyRemoveCount = 0,
                LastRebuildMilliseconds = 0f,
                LastStrategy = SpatialGridRebuildStrategy.None
            };
        }
    }
}
