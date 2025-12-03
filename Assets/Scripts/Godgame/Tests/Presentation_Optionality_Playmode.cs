using Godgame.Presentation;
using NUnit.Framework;
using PureDOTS.Runtime.Components;
using PureDOTS.Runtime.Presentation;
using PureDOTS.Systems;
using Unity.Entities;

namespace Godgame.Tests.Presentation
{
    /// <summary>
    /// Validates that presentation bridge is optional and sim runs with/without it.
    /// </summary>
    public class Presentation_Optionality_Playmode
    {
        private World _world;
        private EntityManager _entityManager;
        private SimulationSystemGroup _simGroup;

        [SetUp]
        public void SetUp()
        {
            _world = new World(nameof(Presentation_Optionality_Playmode));
            _entityManager = _world.EntityManager;
            CoreSingletonBootstrapSystem.EnsureSingletons(_entityManager);
            _world.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
            _simGroup = _world.GetOrCreateSystemManaged<SimulationSystemGroup>();
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
        public void Sim_Runs_WithoutPresentationBridge()
        {
            // Create some gameplay entities
            var entity1 = _entityManager.CreateEntity();
            var entity2 = _entityManager.CreateEntity();

            // Update systems (without presentation bridge)
            _simGroup.Update();

            // Verify entities still exist (sim didn't crash)
            Assert.IsTrue(_entityManager.Exists(entity1));
            Assert.IsTrue(_entityManager.Exists(entity2));
        }

        [Test]
        public void Sim_Runs_WithPresentationBridge()
        {
            // Create presentation bridge
            var bridgeEntity = _entityManager.CreateEntity(typeof(PresentationCommandQueue));
            _entityManager.AddBuffer<PlayEffectRequest>(bridgeEntity);

            // Create some gameplay entities
            var entity1 = _entityManager.CreateEntity();
            var entity2 = _entityManager.CreateEntity();

            // Update systems including presentation bootstrap
            var bootstrap = _world.GetOrCreateSystem<GodgamePresentationBindingBootstrapSystem>();
            _simGroup.RemoveSystemFromUpdateList(bootstrap);
            _simGroup.AddSystemToUpdateList(bootstrap);
            _simGroup.SortSystems();
            _simGroup.Update();

            // Verify entities still exist
            Assert.IsTrue(_entityManager.Exists(entity1));
            Assert.IsTrue(_entityManager.Exists(entity2));

            // Verify presentation binding was created
            var bindingQuery = _entityManager.CreateEntityQuery(typeof(PresentationBindingReference));
            Assert.IsFalse(bindingQuery.IsEmptyIgnoreFilter, "Presentation binding should exist");
        }

        [Test]
        public void PresentationRequests_QueueSafely_WithoutStructuralChanges()
        {
            // Create presentation command queue
            var queueEntity = _entityManager.CreateEntity(typeof(PresentationCommandQueue));
            var requestBuffer = _entityManager.AddBuffer<PlayEffectRequest>(queueEntity);

            // Enqueue effect requests
            requestBuffer.Add(new PlayEffectRequest
            {
                EffectId = GodgamePresentationIds.MiraclePingEffectId,
                Position = Unity.Mathematics.float3.zero,
                Rotation = Unity.Mathematics.quaternion.identity
            });

            requestBuffer.Add(new PlayEffectRequest
            {
                EffectId = GodgamePresentationIds.JobsiteGhostEffectId,
                Position = new Unity.Mathematics.float3(1f, 0f, 1f),
                Rotation = Unity.Mathematics.quaternion.identity
            });

            // Update systems
            _simGroup.Update();

            // Verify requests are still queued (no structural changes)
            requestBuffer = _entityManager.GetBuffer<PlayEffectRequest>(queueEntity);
            Assert.Greater(requestBuffer.Length, 0, "Requests should remain queued");
            Assert.AreEqual(GodgamePresentationIds.MiraclePingEffectId, requestBuffer[0].EffectId);
        }

        [Test]
        public void PresentationBinding_ContainsAllPlaceholderIds()
        {
            // Create presentation bridge and bootstrap
            var queueEntity = _entityManager.CreateEntity(typeof(PresentationCommandQueue));
            _entityManager.AddBuffer<PlayEffectRequest>(queueEntity);

            var bootstrap = _world.GetOrCreateSystem<GodgamePresentationBindingBootstrapSystem>();
            _simGroup.RemoveSystemFromUpdateList(bootstrap);
            _simGroup.AddSystemToUpdateList(bootstrap);
            _simGroup.SortSystems();
            _simGroup.Update();

            // Verify binding contains all placeholder IDs
            var bindingQuery = _entityManager.CreateEntityQuery(typeof(PresentationBindingReference));
            var bindingEntity = bindingQuery.GetSingletonEntity();
            var bindingRef = _entityManager.GetComponentData<PresentationBindingReference>(bindingEntity);

            Assert.IsTrue(bindingRef.Binding.IsCreated, "Binding blob should be created");

            ref var blob = ref bindingRef.Binding.Value;
            Assert.GreaterOrEqual(blob.Effects.Length, 4, "Should have at least 4 effects");

            bool hasMiraclePing = false;
            bool hasJobsiteGhost = false;
            bool hasModuleRefit = false;
            bool hasHandAffordance = false;

            for (int i = 0; i < blob.Effects.Length; i++)
            {
                var effectId = blob.Effects[i].EffectId;
                if (effectId == GodgamePresentationIds.MiraclePingEffectId) hasMiraclePing = true;
                if (effectId == GodgamePresentationIds.JobsiteGhostEffectId) hasJobsiteGhost = true;
                if (effectId == GodgamePresentationIds.ModuleRefitSparksEffectId) hasModuleRefit = true;
                if (effectId == GodgamePresentationIds.HandAffordanceEffectId) hasHandAffordance = true;
            }

            Assert.IsTrue(hasMiraclePing, "Should have MiraclePing effect");
            Assert.IsTrue(hasJobsiteGhost, "Should have JobsiteGhost effect");
            Assert.IsTrue(hasModuleRefit, "Should have ModuleRefitSparks effect");
            Assert.IsTrue(hasHandAffordance, "Should have HandAffordance effect");
        }
    }
}

