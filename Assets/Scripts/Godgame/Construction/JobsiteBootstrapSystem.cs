using Godgame.Construction;
using PureDOTS.Runtime.Components;
using PureDOTS.Runtime.Presentation;
using PureDOTS.Runtime.Registry;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace Godgame.Construction
{
    /// <summary>
    /// Seeds singleton state required for construction flows when no authoring data exists.
    /// </summary>
    [UpdateInGroup(typeof(InitializationSystemGroup), OrderLast = true)]
    public partial struct JobsiteBootstrapSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            EnsurePlacementConfig(ref state);
            EnsurePlacementRequests(ref state);
            EnsureHotkeyState(ref state);
            EnsureMetrics(ref state);
            EnsurePresentationQueue(ref state);
            EnsureResourceCatalog(ref state);
        }

        public void OnDestroy(ref SystemState state)
        {
            if (!SystemAPI.TryGetSingleton(out ResourceTypeIndex resourceIndex))
            {
                return;
            }

            if (!resourceIndex.Catalog.IsCreated)
            {
                return;
            }

            resourceIndex.Catalog.Dispose();
            resourceIndex.Catalog = default;
            SystemAPI.SetSingleton(resourceIndex);
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
        }

        private void EnsurePlacementConfig(ref SystemState state)
        {
            var entityManager = state.EntityManager;
            var placementEntity = ResolvePlacementEntity(ref state);

            var config = entityManager.HasComponent<JobsitePlacementConfig>(placementEntity)
                ? entityManager.GetComponentData<JobsitePlacementConfig>(placementEntity)
                : new JobsitePlacementConfig
                {
                    StartPosition = float3.zero,
                    PositionStep = new float3(2f, 0f, 0f),
                    DefaultRequiredProgress = 5f,
                    BuildRatePerSecond = 2f,
                    CompletionEffectId = 1001,
                    CompletionEffectDuration = 1.25f,
                    TelemetryKey = default
                };

            if (entityManager.HasComponent<RegistryMetadata>(placementEntity))
            {
                var metadata = entityManager.GetComponentData<RegistryMetadata>(placementEntity);
                if (metadata.TelemetryKey.IsValid && config.TelemetryKey.Length == 0)
                {
                    config.TelemetryKey = metadata.TelemetryKey.Key;
                }
            }

            if (config.TelemetryKey.Length == 0)
            {
                config.TelemetryKey = new FixedString64Bytes("registry.construction");
            }

            if (entityManager.HasComponent<JobsitePlacementConfig>(placementEntity))
            {
                entityManager.SetComponentData(placementEntity, config);
            }
            else
            {
                entityManager.AddComponentData(placementEntity, config);
            }

            var nextSiteId = 1;
            var nextPosition = config.StartPosition;
            if (entityManager.HasBuffer<ConstructionRegistryEntry>(placementEntity))
            {
                var entries = entityManager.GetBuffer<ConstructionRegistryEntry>(placementEntity);
                for (int i = 0; i < entries.Length; i++)
                {
                    nextSiteId = math.max(nextSiteId, entries[i].SiteId + 1);
                }

                if (entries.Length > 0)
                {
                    nextPosition = config.StartPosition + config.PositionStep * entries.Length;
                }
            }

            if (entityManager.HasComponent<JobsitePlacementState>(placementEntity))
            {
                entityManager.SetComponentData(placementEntity, new JobsitePlacementState
                {
                    NextSiteId = nextSiteId,
                    NextPosition = nextPosition
                });
            }
            else
            {
                entityManager.AddComponentData(placementEntity, new JobsitePlacementState
                {
                    NextSiteId = nextSiteId,
                    NextPosition = nextPosition
                });
            }
        }

        private void EnsurePlacementRequests(ref SystemState state)
        {
            var entityManager = state.EntityManager;
            var placementEntity = ResolvePlacementEntity(ref state);

            if (!entityManager.HasBuffer<JobsitePlacementRequest>(placementEntity))
            {
                entityManager.AddBuffer<JobsitePlacementRequest>(placementEntity);
            }
        }

        private void EnsureHotkeyState(ref SystemState state)
        {
            var entityManager = state.EntityManager;
            var placementEntity = ResolvePlacementEntity(ref state);
            if (!entityManager.HasComponent<JobsitePlacementHotkeyState>(placementEntity))
            {
                entityManager.AddComponentData(placementEntity, new JobsitePlacementHotkeyState
                {
                    PlaceRequested = 0
                });
            }
        }

        private void EnsureMetrics(ref SystemState state)
        {
            var entityManager = state.EntityManager;
            var placementEntity = ResolvePlacementEntity(ref state);
            if (!entityManager.HasComponent<JobsiteMetrics>(placementEntity))
            {
                entityManager.AddComponentData(placementEntity, new JobsiteMetrics { CompletedCount = 0 });
            }
        }

        private void EnsurePresentationQueue(ref SystemState state)
        {
            var entityManager = state.EntityManager;
            Entity queueEntity;

            if (SystemAPI.TryGetSingletonEntity<PresentationCommandQueue>(out var existing))
            {
                queueEntity = existing;
            }
            else
            {
                queueEntity = entityManager.CreateEntity(typeof(PresentationCommandQueue));
            }

            if (!entityManager.HasComponent<PresentationRequestHub>(queueEntity))
            {
                entityManager.AddComponentData(queueEntity, new PresentationRequestHub());
            }

            if (!entityManager.HasComponent<PresentationRequestFailures>(queueEntity))
            {
                entityManager.AddComponentData(queueEntity, new PresentationRequestFailures());
            }

            if (!entityManager.HasBuffer<PlayEffectRequest>(queueEntity))
            {
                entityManager.AddBuffer<PlayEffectRequest>(queueEntity);
            }

            if (!entityManager.HasBuffer<PresentationSpawnRequest>(queueEntity))
            {
                entityManager.AddBuffer<PresentationSpawnRequest>(queueEntity);
            }

            if (!entityManager.HasBuffer<PresentationRecycleRequest>(queueEntity))
            {
                entityManager.AddBuffer<PresentationRecycleRequest>(queueEntity);
            }

            if (!entityManager.HasBuffer<SpawnCompanionRequest>(queueEntity))
            {
                entityManager.AddBuffer<SpawnCompanionRequest>(queueEntity);
            }

            if (!entityManager.HasBuffer<DespawnCompanionRequest>(queueEntity))
            {
                entityManager.AddBuffer<DespawnCompanionRequest>(queueEntity);
            }
        }

        private Entity ResolvePlacementEntity(ref SystemState state)
        {
            var entityManager = state.EntityManager;

            if (RegistryDirectoryLookup.TryGetRegistryEntity(ref state, RegistryKind.Construction, out var registryEntity))
            {
                return registryEntity;
            }

            if (SystemAPI.TryGetSingletonEntity<ConstructionRegistry>(out var fallback))
            {
                return fallback;
            }

            return entityManager.CreateEntity(typeof(ConstructionRegistry));
        }

        private void EnsureResourceCatalog(ref SystemState state)
        {
            if (SystemAPI.HasSingleton<ResourceTypeIndex>())
            {
                var catalog = SystemAPI.GetSingleton<ResourceTypeIndex>().Catalog;
                if (catalog.IsCreated)
                {
                    return;
                }
            }

            var entityManager = state.EntityManager;
            Entity catalogEntity;

            if (SystemAPI.TryGetSingletonEntity<ResourceTypeIndex>(out var existing))
            {
                catalogEntity = existing;
            }
            else
            {
                catalogEntity = entityManager.CreateEntity(typeof(ResourceTypeIndex));
            }

            using var builder = new BlobBuilder(Allocator.Temp);
            ref var root = ref builder.ConstructRoot<ResourceTypeIndexBlob>();
            builder.Allocate(ref root.Ids, 0);
            builder.Allocate(ref root.DisplayNames, 0);
            builder.Allocate(ref root.Colors, 0);

            var blob = builder.CreateBlobAssetReference<ResourceTypeIndexBlob>(Allocator.Persistent);
            entityManager.SetComponentData(catalogEntity, new ResourceTypeIndex { Catalog = blob });
        }
    }
}
