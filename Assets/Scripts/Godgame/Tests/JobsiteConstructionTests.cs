using System.Collections;
using Godgame.Construction;
using NUnit.Framework;
using PureDOTS.Runtime.Components;
using PureDOTS.Runtime.Presentation;
using PureDOTS.Runtime.Telemetry;
using PureDOTS.Systems;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine.TestTools;

namespace Godgame.Tests
{
    public class JobsiteConstructionTests
    {
        private World _world;
        private EntityManager _entityManager;

        [SetUp]
        public void SetUp()
        {
            _world = new World("JobsiteConstructionTests");
            _entityManager = _world.EntityManager;
            CoreSingletonBootstrapSystem.EnsureSingletons(_entityManager);

            // Ensure time and rewind state mirror runtime expectations.
            var timeEntity = _entityManager.CreateEntityQuery(ComponentType.ReadOnly<TimeState>()).GetSingletonEntity();
            var timeState = _entityManager.GetComponentData<TimeState>(timeEntity);
            timeState.IsPaused = false;
            timeState.FixedDeltaTime = 0.5f;
            _entityManager.SetComponentData(timeEntity, timeState);

            var rewindEntity = _entityManager.CreateEntityQuery(ComponentType.ReadOnly<RewindState>()).GetSingletonEntity();
            var rewindState = _entityManager.GetComponentData<RewindState>(rewindEntity);
            rewindState.Mode = RewindMode.Record;
            _entityManager.SetComponentData(rewindEntity, rewindState);
        }

        [TearDown]
        public void TearDown()
        {
            if (_world.IsCreated)
            {
                _world.Dispose();
            }
        }

        [Test]
        public void JobsiteGhost_Completes_EmitsEffectAndTelemetry()
        {
            var bootstrap = _world.GetOrCreateSystem<JobsiteBootstrapSystem>();
            bootstrap.Update(_world.Unmanaged);

            var placementEntity = _entityManager.CreateEntityQuery(ComponentType.ReadWrite<JobsitePlacementConfig>()).GetSingletonEntity();
            var config = _entityManager.GetComponentData<JobsitePlacementConfig>(placementEntity);
            config.DefaultRequiredProgress = 2f;
            config.BuildRatePerSecond = 4f;
            config.CompletionEffectId = 7;
            config.CompletionEffectDuration = 0.5f;
            config.TelemetryKey = new FixedString64Bytes("telemetry.jobsite.completed");
            _entityManager.SetComponentData(placementEntity, config);

            var requests = _entityManager.GetBuffer<JobsitePlacementRequest>(placementEntity);
            var sitePosition = new float3(3f, 0f, 4f);
            requests.Add(new JobsitePlacementRequest { Position = sitePosition });

            var placement = _world.GetOrCreateSystem<JobsitePlacementSystem>();
            placement.Update(_world.Unmanaged);

            var jobsiteQuery = _entityManager.CreateEntityQuery(ComponentType.ReadOnly<ConstructionSiteId>());
            Assert.AreEqual(1, jobsiteQuery.CalculateEntityCount());
            var jobsiteEntity = jobsiteQuery.GetSingletonEntity();

            var build = _world.GetOrCreateSystem<JobsiteBuildSystem>();
            build.Update(_world.Unmanaged);

            var completion = _world.GetOrCreateSystem<JobsiteCompletionSystem>();
            completion.Update(_world.Unmanaged);

            var registrySystem = _world.GetOrCreateSystem<ConstructionRegistrySystem>();
            registrySystem.Update(_world.Unmanaged);

            var flags = _entityManager.GetComponentData<ConstructionSiteFlags>(jobsiteEntity);
            Assert.AreNotEqual(0, flags.Value & ConstructionSiteFlags.Completed);

            var progress = _entityManager.GetComponentData<ConstructionSiteProgress>(jobsiteEntity);
            Assert.GreaterOrEqual(progress.CurrentProgress, progress.RequiredProgress);

            var registryEntity = _entityManager.CreateEntityQuery(ComponentType.ReadOnly<ConstructionRegistry>())
                .GetSingletonEntity();
            var registry = _entityManager.GetComponentData<ConstructionRegistry>(registryEntity);
            Assert.AreEqual(1, registry.CompletedSiteCount);

            var entries = _entityManager.GetBuffer<ConstructionRegistryEntry>(registryEntity);
            Assert.AreEqual(1, entries.Length);
            Assert.AreEqual(jobsiteEntity, entries[0].SiteEntity);
            Assert.AreNotEqual(0, entries[0].Flags & ConstructionSiteFlags.Completed);
            Assert.AreEqual(sitePosition, entries[0].Position);

            var metrics = _entityManager.GetComponentData<JobsiteMetrics>(placementEntity);
            Assert.AreEqual(1, metrics.CompletedCount);

            var effectEntity = _entityManager.CreateEntityQuery(ComponentType.ReadOnly<PresentationCommandQueue>())
                .GetSingletonEntity();
            var effects = _entityManager.GetBuffer<PlayEffectRequest>(effectEntity);
            Assert.AreEqual(1, effects.Length);
            Assert.AreEqual(config.CompletionEffectId, effects[0].EffectId);
            Assert.AreEqual(jobsiteEntity, effects[0].Target);
            Assert.AreEqual(sitePosition, effects[0].Position);

            var telemetryEntity = _entityManager.CreateEntityQuery(ComponentType.ReadOnly<TelemetryStream>())
                .GetSingletonEntity();
            var telemetry = _entityManager.GetComponentData<TelemetryStream>(telemetryEntity);
            Assert.Greater(telemetry.Version, 0);
            var telemetryMetrics = _entityManager.GetBuffer<PureDOTS.Runtime.Telemetry.TelemetryMetric>(telemetryEntity);
            Assert.IsTrue(ContainsTelemetryMetric(telemetryMetrics, config.TelemetryKey, 1));
        }

        [UnityTest]
        public IEnumerator JobsiteHotkey_PlacesGhost_CompletesAndUpdatesTelemetry()
        {
            var bootstrap = _world.GetOrCreateSystem<JobsiteBootstrapSystem>();
            bootstrap.Update(_world.Unmanaged);

            var placementEntity = _entityManager.CreateEntityQuery(ComponentType.ReadWrite<JobsitePlacementConfig>()).GetSingletonEntity();
            var config = _entityManager.GetComponentData<JobsitePlacementConfig>(placementEntity);
            config.DefaultRequiredProgress = 2f;
            config.BuildRatePerSecond = 4f;
            config.CompletionEffectId = 9;
            config.CompletionEffectDuration = 0.85f;
            _entityManager.SetComponentData(placementEntity, config);

            var hotkey = _entityManager.GetComponentData<JobsitePlacementHotkeyState>(placementEntity);
            hotkey.PlaceRequested = 1;
            _entityManager.SetComponentData(placementEntity, hotkey);

            _world.GetOrCreateSystem<JobsitePlacementHotkeySystem>().Update(_world.Unmanaged);
            _world.GetOrCreateSystem<JobsitePlacementSystem>().Update(_world.Unmanaged);

            var jobsiteQuery = _entityManager.CreateEntityQuery(ComponentType.ReadOnly<ConstructionSiteId>());
            Assert.AreEqual(1, jobsiteQuery.CalculateEntityCount());
            var jobsiteEntity = jobsiteQuery.GetSingletonEntity();

            _world.GetOrCreateSystem<JobsiteBuildSystem>().Update(_world.Unmanaged);
            _world.GetOrCreateSystem<JobsiteCompletionSystem>().Update(_world.Unmanaged);

            var registrySystem = _world.GetOrCreateSystem<ConstructionRegistrySystem>();
            registrySystem.Update(_world.Unmanaged);

            var registryEntity = _entityManager.CreateEntityQuery(ComponentType.ReadOnly<ConstructionRegistry>())
                .GetSingletonEntity();
            var registry = _entityManager.GetComponentData<ConstructionRegistry>(registryEntity);
            Assert.AreEqual(1, registry.CompletedSiteCount);

            var telemetryEntity = _entityManager.CreateEntityQuery(ComponentType.ReadOnly<TelemetryStream>())
                .GetSingletonEntity();
            var telemetry = _entityManager.GetComponentData<TelemetryStream>(telemetryEntity);
            Assert.Greater(telemetry.Version, 0);
            var telemetryMetrics = _entityManager.GetBuffer<TelemetryMetric>(telemetryEntity);
            Assert.IsTrue(ContainsTelemetryMetric(telemetryMetrics, config.TelemetryKey, 1));

            var effectsEntity = _entityManager.CreateEntityQuery(ComponentType.ReadOnly<PresentationCommandQueue>())
                .GetSingletonEntity();
            var effects = _entityManager.GetBuffer<PlayEffectRequest>(effectsEntity);
            Assert.AreEqual(1, effects.Length);
            Assert.AreEqual(config.CompletionEffectId, effects[0].EffectId);
            Assert.AreEqual(jobsiteEntity, effects[0].Target);

            yield return null;
        }

        private static bool ContainsTelemetryMetric(DynamicBuffer<TelemetryMetric> buffer, in FixedString64Bytes key, float expected)
        {
            for (int i = 0; i < buffer.Length; i++)
            {
                if (buffer[i].Key.Equals(key))
                {
                    return math.abs(buffer[i].Value - expected) < 0.001f;
                }
            }

            return false;
        }
    }
}
