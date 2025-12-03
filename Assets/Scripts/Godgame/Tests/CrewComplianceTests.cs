using NUnit.Framework;
using PureDOTS.Runtime.Alignment;
using PureDOTS.Runtime.Ships;
using PureDOTS.Systems;
using PureDOTS.Systems.Ships;
using Unity.Entities;

namespace Godgame.Tests.Alignment
{
    /// <summary>
    /// Validates crew compliance aggregation and default component provisioning.
    /// </summary>
    public class CrewComplianceTests
    {
        private World _world;
        private EntityManager _entityManager;
        private SimulationSystemGroup _simGroup;

        [SetUp]
        public void SetUp()
        {
            _world = new World(nameof(CrewComplianceTests));
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
        public void Aggregation_ComputesWarningAndWritesAlerts()
        {
            var entity = _entityManager.CreateEntity(typeof(CrewAggregate));
            var samples = _entityManager.AddBuffer<CrewAlignmentSample>(entity);
            samples.Add(new CrewAlignmentSample
            {
                Affiliation = new AffiliationId { Value = "crew.alpha" },
                Doctrine = DoctrineId.FromString("doctrine.alpha"),
                Loyalty = 0.5f,
                Suspicion = 0.2f,
                Obedience = 0.3f,
                Fanaticism = 0.1f,
                Outlook = Outlook.Opportunist
            });

            UpdateSystem(_world.GetOrCreateSystem<CrewAggregationSystem>());

            Assert.IsTrue(_entityManager.HasComponent<CrewCompliance>(entity));
            var compliance = _entityManager.GetComponentData<CrewCompliance>(entity);
            Assert.AreEqual(ComplianceStatus.Warning, compliance.Status);
            Assert.AreEqual(0.5f, compliance.AverageLoyalty, 0.0001f);
            Assert.AreEqual(0.2f, compliance.AverageSuspicion, 0.0001f);
            Assert.AreEqual(0.2f, compliance.SuspicionDelta, 0.0001f);
            Assert.AreEqual(0, compliance.MissingData);
            Assert.AreEqual("crew.alpha", compliance.Affiliation.Value.ToString());
            Assert.AreEqual("doctrine.alpha", compliance.Doctrine.Value.ToString());

            var alerts = _entityManager.GetBuffer<ComplianceAlert>(entity);
            Assert.AreEqual(1, alerts.Length);
            Assert.AreEqual(ComplianceStatus.Warning, alerts[0].Status);
        }

        [Test]
        public void Aggregation_MarksMissingDataWhenNoSamplesExist()
        {
            var entity = _entityManager.CreateEntity(typeof(CrewAggregate));

            UpdateSystem(_world.GetOrCreateSystem<CrewAggregationSystem>());

            Assert.IsTrue(_entityManager.HasComponent<CrewCompliance>(entity));
            var compliance = _entityManager.GetComponentData<CrewCompliance>(entity);
            Assert.AreEqual(ComplianceStatus.Warning, compliance.Status);
            Assert.AreEqual(1, compliance.MissingData);
            Assert.AreEqual(0, compliance.AverageLoyalty, 0.0001f);
            Assert.AreEqual(0, compliance.AverageSuspicion, 0.0001f);
        }

        private void UpdateSystem(SystemHandle handle)
        {
            _simGroup.RemoveSystemFromUpdateList(handle);
            _simGroup.AddSystemToUpdateList(handle);
            _simGroup.SortSystems();
            _simGroup.Update();
        }
    }
}
