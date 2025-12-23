using Godgame.Buildings;
using Godgame.Villages;
using PureDOTS.Runtime.AI;
using PureDOTS.Runtime.Components;
using PureDOTS.Runtime.Core;
using PureDOTS.Runtime.Perception;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;

namespace Godgame.Perception
{
    public struct FireSignalTag : IComponentData
    {
    }

    /// <summary>
    /// Seeds perception-related components for core Godgame entities.
    /// </summary>
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    [UpdateAfter(typeof(Godgame.Scenario.GodgameScenarioSpawnSystem))]
    [UpdateAfter(typeof(Godgame.Scenario.SettlementSpawnSystem))]
    [UpdateAfter(typeof(Godgame.Scenario.SmokeTestFallbackBootstrapSystem))]
    public partial struct GodgamePerceptionBootstrapSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<GameWorldTag>();
        }

        public void OnUpdate(ref SystemState state)
        {
            var em = state.EntityManager;
            var ecb = new EntityCommandBuffer(Allocator.Temp);

            var villagerDetectable = new Detectable
            {
                Visibility = 1f,
                Audibility = 1f,
                ThreatLevel = 0,
                Category = DetectableCategory.Neutral
            };
            var villagerSignature = new SensorSignature
            {
                VisualSignature = 1f,
                AuditorySignature = 1f,
                OlfactorySignature = 0.6f,
                EMSignature = 0f,
                GraviticSignature = 0f,
                ExoticSignature = 0f,
                ParanormalSignature = 0.2f
            };

            foreach (var (_, _, entity) in SystemAPI.Query<RefRO<VillagerId>, RefRO<LocalTransform>>()
                .WithEntityAccess())
            {
                EnsurePerception(ref ecb, em, entity, villagerDetectable, villagerSignature);
            }

            var resourceDetectable = new Detectable
            {
                Visibility = 0.8f,
                Audibility = 0.1f,
                ThreatLevel = 0,
                Category = DetectableCategory.Resource
            };
            var resourceSignature = new SensorSignature
            {
                VisualSignature = 0.8f,
                AuditorySignature = 0.1f,
                OlfactorySignature = 0.2f,
                EMSignature = 0f,
                GraviticSignature = 0f,
                ExoticSignature = 0f,
                ParanormalSignature = 0f
            };

            foreach (var (_, _, entity) in SystemAPI.Query<RefRO<ResourceSourceConfig>, RefRO<LocalTransform>>()
                .WithEntityAccess())
            {
                EnsurePerception(ref ecb, em, entity, resourceDetectable, resourceSignature);
            }

            foreach (var (_, _, entity) in SystemAPI.Query<RefRO<ResourceChunkState>, RefRO<LocalTransform>>()
                .WithEntityAccess())
            {
                EnsurePerception(ref ecb, em, entity, resourceDetectable, resourceSignature);
            }

            var structureDetectable = new Detectable
            {
                Visibility = 1f,
                Audibility = 0.2f,
                ThreatLevel = 0,
                Category = DetectableCategory.Structure
            };
            var structureSignature = new SensorSignature
            {
                VisualSignature = 1f,
                AuditorySignature = 0.2f,
                OlfactorySignature = 0.2f,
                EMSignature = 0f,
                GraviticSignature = 0f,
                ExoticSignature = 0f,
                ParanormalSignature = 0f
            };

            foreach (var (_, _, entity) in SystemAPI.Query<RefRO<StorehouseConfig>, RefRO<LocalTransform>>()
                .WithEntityAccess())
            {
                EnsurePerception(ref ecb, em, entity, structureDetectable, structureSignature);
            }

            foreach (var (_, _, entity) in SystemAPI.Query<RefRO<BuildingDurability>, RefRO<LocalTransform>>()
                .WithEntityAccess())
            {
                EnsurePerception(ref ecb, em, entity, structureDetectable, structureSignature);
            }

            foreach (var (_, _, entity) in SystemAPI.Query<RefRO<Village>, RefRO<LocalTransform>>()
                .WithEntityAccess())
            {
                EnsurePerception(ref ecb, em, entity, structureDetectable, structureSignature);
            }

            var fireEmitter = new SensorySignalEmitter
            {
                Channels = PerceptionChannel.Smell | PerceptionChannel.Hearing,
                SmellStrength = 0.7f,
                SoundStrength = 0.8f,
                EMStrength = 0f,
                IsActive = 1
            };

            foreach (var (_, _, entity) in SystemAPI.Query<RefRO<BuildingOnFire>, RefRO<LocalTransform>>()
                .WithEntityAccess())
            {
                if (!em.HasComponent<SensorySignalEmitter>(entity))
                {
                    ecb.AddComponent(entity, fireEmitter);
                    ecb.AddComponent<FireSignalTag>(entity);
                }
                else if (em.HasComponent<FireSignalTag>(entity))
                {
                    ecb.SetComponent(entity, fireEmitter);
                }
            }

            foreach (var (_, entity) in SystemAPI.Query<RefRO<FireSignalTag>>()
                .WithNone<BuildingOnFire>()
                .WithEntityAccess())
            {
                if (!em.HasComponent<SensorySignalEmitter>(entity))
                {
                    continue;
                }

                var emitter = em.GetComponentData<SensorySignalEmitter>(entity);
                if (emitter.IsActive != 0)
                {
                    emitter.IsActive = 0;
                    ecb.SetComponent(entity, emitter);
                }
            }

            ecb.Playback(em);
            ecb.Dispose();
        }

        private static void EnsurePerception(
            ref EntityCommandBuffer ecb,
            EntityManager entityManager,
            Entity entity,
            in Detectable detectable,
            in SensorSignature signature)
        {
            if (!entityManager.HasComponent<Detectable>(entity))
            {
                ecb.AddComponent(entity, detectable);
            }

            if (!entityManager.HasComponent<SensorSignature>(entity))
            {
                ecb.AddComponent(entity, signature);
            }
        }
    }
}
