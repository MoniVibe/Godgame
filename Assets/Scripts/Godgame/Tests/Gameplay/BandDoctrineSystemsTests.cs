using Godgame.Bands;
using NUnit.Framework;
using PureDOTS.Runtime.Components;
using PureDOTS.Systems;
using Unity.Entities;
using Unity.Mathematics;

namespace Godgame.Tests.Gameplay
{
    [TestFixture]
    public class BandDoctrineSystemsTests
    {
        private World _world;
        private EntityManager _entityManager;
        private InitializationSystemGroup _initGroup;
        private SimulationSystemGroup _simGroup;

        [SetUp]
        public void SetUp()
        {
            _world = new World("BandDoctrineSystemsTests");
            _entityManager = _world.EntityManager;
            CoreSingletonBootstrapSystem.EnsureSingletons(_entityManager);
            _initGroup = _world.GetOrCreateSystemManaged<InitializationSystemGroup>();
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
        public void DoctrineBootstrap_AddsDefaultsToBand()
        {
            var bandEntity = _entityManager.CreateEntity(typeof(Band));
            _entityManager.SetComponentData(bandEntity, new Band
            {
                Status = BandStatus.Idle,
                Cohesion = 0.6f,
                Morale = 0.55f,
                Fatigue = 0.1f
            });

            var bootstrap = _world.GetOrCreateSystem<BandDoctrineBootstrapSystem>();
            UpdateInitSystem(bootstrap);

            Assert.IsTrue(_entityManager.HasComponent<BandDoctrineProfile>(bandEntity));
            Assert.IsTrue(_entityManager.HasComponent<BandDoctrineContext>(bandEntity));
            Assert.IsTrue(_entityManager.HasComponent<BandDoctrineSelection>(bandEntity));
            Assert.IsTrue(_entityManager.HasComponent<BandDoctrineProjection>(bandEntity));
            Assert.IsTrue(_entityManager.HasBuffer<BandDoctrineWeight>(bandEntity));

            var weights = _entityManager.GetBuffer<BandDoctrineWeight>(bandEntity);
            Assert.GreaterOrEqual(weights.Length, 6);
        }

        [Test]
        public void DoctrineSelection_PrefersSplitShell_WhenRearThreatDominates()
        {
            var bandEntity = _entityManager.CreateEntity(typeof(Band));
            _entityManager.SetComponentData(bandEntity, new Band
            {
                Status = BandStatus.Engaged,
                Cohesion = 0.62f,
                Morale = 0.7f,
                Fatigue = 0.3f
            });

            var bootstrap = _world.GetOrCreateSystem<BandDoctrineBootstrapSystem>();
            UpdateInitSystem(bootstrap);

            var context = _entityManager.GetComponentData<BandDoctrineContext>(bandEntity);
            context.FrontThreat = 0.25f;
            context.RearThreat = 0.95f;
            context.AttritionPressure = 0.4f;
            context.AmmoPressure = 0.2f;
            context.ObjectivePressure = 0.3f;
            context.ThreatVolatility = 0.35f;
            context.CasualtyRisk = 0.35f;
            context.RotationDemand = 0.1f;
            _entityManager.SetComponentData(bandEntity, context);

            var profile = _entityManager.GetComponentData<BandDoctrineProfile>(bandEntity);
            profile.AuthoritarianBias = 0.45f;
            profile.EgalitarianBias = 0.55f;
            profile.CorruptionBias = 0.05f;
            profile.CrueltyBias = 0.05f;
            profile.GoalRigidity = 0.25f;
            _entityManager.SetComponentData(bandEntity, profile);

            var selectionSystem = _world.GetOrCreateSystem<BandDoctrineSelectionSystem>();
            UpdateSimSystem(selectionSystem);

            var selection = _entityManager.GetComponentData<BandDoctrineSelection>(bandEntity);
            Assert.AreEqual(BandDoctrineModuleType.SplitShell, selection.ActiveModule);
            Assert.Greater(selection.ActiveScore, 0f);
        }

        [Test]
        public void HighCohesion_EnablesAbstraction_AndReducesCommunicationLoad()
        {
            var bandEntity = _entityManager.CreateEntity(typeof(Band), typeof(BandFormation));
            _entityManager.SetComponentData(bandEntity, new Band
            {
                Status = BandStatus.Engaged,
                Cohesion = 0.93f,
                Morale = 0.8f,
                Fatigue = 0.2f
            });
            _entityManager.SetComponentData(bandEntity, new BandFormation
            {
                Formation = BandFormationType.Line,
                Spacing = 2f,
                Width = 8f,
                Depth = 2.5f,
                Facing = new float3(0f, 0f, 1f),
                Anchor = float3.zero,
                Stability = 0.9f
            });

            var bootstrap = _world.GetOrCreateSystem<BandDoctrineBootstrapSystem>();
            UpdateInitSystem(bootstrap);

            var context = _entityManager.GetComponentData<BandDoctrineContext>(bandEntity);
            context.FrontThreat = 0.35f;
            context.RearThreat = 0.2f;
            context.AttritionPressure = 0.25f;
            context.AmmoPressure = 0.15f;
            context.ObjectivePressure = 0.4f;
            context.ThreatVolatility = 0.05f;
            context.CasualtyRisk = 0.2f;
            context.RotationDemand = 0.1f;
            _entityManager.SetComponentData(bandEntity, context);

            var profile = _entityManager.GetComponentData<BandDoctrineProfile>(bandEntity);
            profile.CohesionAbstractionThreshold = 0.65f;
            profile.MaxThreatVolatilityForAbstraction = 0.3f;
            profile.CommunicationCompression = 0.8f;
            _entityManager.SetComponentData(bandEntity, profile);

            var selectionSystem = _world.GetOrCreateSystem<BandDoctrineSelectionSystem>();
            var projectionSystem = _world.GetOrCreateSystem<BandDoctrineProjectionSystem>();
            UpdateSimSystem(selectionSystem);
            UpdateSimSystem(projectionSystem);

            var selection = _entityManager.GetComponentData<BandDoctrineSelection>(bandEntity);
            var projection = _entityManager.GetComponentData<BandDoctrineProjection>(bandEntity);
            var formation = _entityManager.GetComponentData<BandFormation>(bandEntity);

            Assert.AreEqual(1, selection.IsAbstractedControl);
            Assert.Greater(selection.CommunicationIntentSuppression, 0.1f);
            Assert.Less(projection.CommunicationLoadMultiplier, 1f);
            Assert.That(formation.Spacing, Is.Not.EqualTo(2f).Within(0.0001f));
        }

        [Test]
        public void Governance_DeniedRotation_IncreasesResentment()
        {
            var bandEntity = _entityManager.CreateEntity(typeof(Band), typeof(BandFormation));
            _entityManager.SetComponentData(bandEntity, new Band
            {
                Leader = Entity.Null,
                Status = BandStatus.Idle,
                Cohesion = 0.7f,
                Morale = 0.52f,
                Fatigue = 0.3f
            });
            _entityManager.SetComponentData(bandEntity, new BandFormation
            {
                Formation = BandFormationType.Line,
                Spacing = 2f,
                Width = 7f,
                Depth = 2f,
                Facing = new float3(0f, 0f, 1f),
                Anchor = float3.zero,
                Stability = 0.8f
            });

            var bootstrap = _world.GetOrCreateSystem<BandDoctrineBootstrapSystem>();
            UpdateInitSystem(bootstrap);

            var profile = _entityManager.GetComponentData<BandDoctrineProfile>(bandEntity);
            profile.AuthoritarianBias = 0.95f;
            profile.EgalitarianBias = 0.1f;
            profile.CrueltyBias = 0.85f;
            profile.CorruptionBias = 0.65f;
            profile.GoalRigidity = 0.9f;
            _entityManager.SetComponentData(bandEntity, profile);

            var hierarchy = _entityManager.GetComponentData<BandCommandHierarchy>(bandEntity);
            hierarchy.ApprovalThresholdBase = 0.85f;
            hierarchy.ApprovalThresholdScale = 0.3f;
            _entityManager.SetComponentData(bandEntity, hierarchy);

            var context = _entityManager.GetComponentData<BandDoctrineContext>(bandEntity);
            context.ObjectivePressure = 0.9f;
            context.CasualtyRisk = 0.7f;
            context.AttritionPressure = 0.8f;
            context.RotationDemand = 0.75f;
            _entityManager.SetComponentData(bandEntity, context);

            var requests = _entityManager.GetBuffer<BandDoctrineRequest>(bandEntity);
            requests.Add(new BandDoctrineRequest
            {
                RequestedModule = BandDoctrineModuleType.Rotation,
                RequesterRole = BandCommandRole.Captain,
                RequesterEntity = Entity.Null,
                Urgency = 0.8f,
                CasualtyConcern = 0.85f,
                MoraleConcern = 0.65f
            });

            var governanceSystem = _world.GetOrCreateSystem<BandDoctrineGovernanceSystem>();
            UpdateSimSystem(governanceSystem);

            var social = _entityManager.GetComponentData<BandSocialState>(bandEntity);
            var events = _entityManager.GetBuffer<BandDoctrineDecisionEvent>(bandEntity);
            Assert.AreEqual(1, events.Length);
            Assert.AreEqual(BandRequestDisposition.Denied, events[0].Disposition);
            Assert.Greater(social.CrewResentment, 0f);
            Assert.Less(social.CommandTrust, 0.5f);
        }

        [Test]
        public void Governance_ApprovedRotation_UpdatesSelectionAndTrust()
        {
            var bandEntity = _entityManager.CreateEntity(typeof(Band), typeof(BandFormation), typeof(BandDoctrineSelection));
            _entityManager.SetComponentData(bandEntity, new Band
            {
                Leader = Entity.Null,
                Status = BandStatus.Idle,
                Cohesion = 0.82f,
                Morale = 0.48f,
                Fatigue = 0.2f
            });
            _entityManager.SetComponentData(bandEntity, new BandFormation
            {
                Formation = BandFormationType.Line,
                Spacing = 1.8f,
                Width = 8f,
                Depth = 2.2f,
                Facing = new float3(0f, 0f, 1f),
                Anchor = float3.zero,
                Stability = 0.85f
            });
            _entityManager.SetComponentData(bandEntity, new BandDoctrineSelection
            {
                ActiveModule = BandDoctrineModuleType.ElasticFront,
                PreviousModule = BandDoctrineModuleType.ElasticFront,
                ActiveScore = 0.45f
            });

            var bootstrap = _world.GetOrCreateSystem<BandDoctrineBootstrapSystem>();
            UpdateInitSystem(bootstrap);

            var profile = _entityManager.GetComponentData<BandDoctrineProfile>(bandEntity);
            profile.AuthoritarianBias = 0.2f;
            profile.EgalitarianBias = 0.85f;
            profile.CrueltyBias = 0.05f;
            profile.CorruptionBias = 0.05f;
            profile.GoalRigidity = 0.2f;
            _entityManager.SetComponentData(bandEntity, profile);

            var hierarchy = _entityManager.GetComponentData<BandCommandHierarchy>(bandEntity);
            hierarchy.StrikeGroupApprovalBias = 0.85f;
            hierarchy.FleetAdmiralApprovalBias = 0.8f;
            hierarchy.ApprovalThresholdBase = 0.35f;
            hierarchy.ApprovalThresholdScale = 0.15f;
            _entityManager.SetComponentData(bandEntity, hierarchy);

            var context = _entityManager.GetComponentData<BandDoctrineContext>(bandEntity);
            context.ObjectivePressure = 0.3f;
            context.CasualtyRisk = 0.75f;
            context.AttritionPressure = 0.7f;
            context.RotationDemand = 0.9f;
            _entityManager.SetComponentData(bandEntity, context);

            var requests = _entityManager.GetBuffer<BandDoctrineRequest>(bandEntity);
            requests.Add(new BandDoctrineRequest
            {
                RequestedModule = BandDoctrineModuleType.Rotation,
                RequesterRole = BandCommandRole.Captain,
                RequesterEntity = Entity.Null,
                Urgency = 0.9f,
                CasualtyConcern = 0.8f,
                MoraleConcern = 0.75f
            });

            var governanceSystem = _world.GetOrCreateSystem<BandDoctrineGovernanceSystem>();
            UpdateSimSystem(governanceSystem);

            var social = _entityManager.GetComponentData<BandSocialState>(bandEntity);
            var selection = _entityManager.GetComponentData<BandDoctrineSelection>(bandEntity);
            var events = _entityManager.GetBuffer<BandDoctrineDecisionEvent>(bandEntity);
            Assert.AreEqual(1, events.Length);
            Assert.AreEqual(BandRequestDisposition.Approved, events[0].Disposition);
            Assert.Greater(social.CommandTrust, 0.5f);
            Assert.AreEqual(BandDoctrineModuleType.Rotation, selection.ActiveModule);
        }

        [Test]
        public void Governance_NecessityUnderstood_DenialCanIncreaseTrust()
        {
            var bandEntity = _entityManager.CreateEntity(typeof(Band), typeof(BandFormation));
            _entityManager.SetComponentData(bandEntity, new Band
            {
                Leader = Entity.Null,
                Status = BandStatus.Idle,
                Cohesion = 0.9f,
                Morale = 0.66f,
                Fatigue = 0.35f
            });
            _entityManager.SetComponentData(bandEntity, new BandFormation
            {
                Formation = BandFormationType.Line,
                Spacing = 2f,
                Width = 7f,
                Depth = 2f,
                Facing = new float3(0f, 0f, 1f),
                Anchor = float3.zero,
                Stability = 0.82f
            });

            var bootstrap = _world.GetOrCreateSystem<BandDoctrineBootstrapSystem>();
            UpdateInitSystem(bootstrap);

            var profile = _entityManager.GetComponentData<BandDoctrineProfile>(bandEntity);
            profile.AuthoritarianBias = 0.3f;
            profile.EgalitarianBias = 0.2f;
            profile.CrueltyBias = 0.05f;
            profile.CorruptionBias = 0.05f;
            profile.GoalRigidity = 0.9f;
            _entityManager.SetComponentData(bandEntity, profile);

            var hierarchy = _entityManager.GetComponentData<BandCommandHierarchy>(bandEntity);
            hierarchy.ApprovalThresholdBase = 0.9f;
            hierarchy.ApprovalThresholdScale = 0.25f;
            _entityManager.SetComponentData(bandEntity, hierarchy);

            var context = _entityManager.GetComponentData<BandDoctrineContext>(bandEntity);
            context.ObjectivePressure = 0.95f;
            context.Criticality = 0.85f;
            context.CasualtyRisk = 0.35f;
            context.AttritionPressure = 0.5f;
            context.RotationDemand = 0.45f;
            _entityManager.SetComponentData(bandEntity, context);

            var requests = _entityManager.GetBuffer<BandDoctrineRequest>(bandEntity);
            requests.Add(new BandDoctrineRequest
            {
                RequestedModule = BandDoctrineModuleType.Rotation,
                RequesterRole = BandCommandRole.Captain,
                RequesterEntity = Entity.Null,
                Urgency = 0.5f,
                CasualtyConcern = 0.35f,
                MoraleConcern = 0.3f
            });

            var governanceSystem = _world.GetOrCreateSystem<BandDoctrineGovernanceSystem>();
            UpdateSimSystem(governanceSystem);

            var social = _entityManager.GetComponentData<BandSocialState>(bandEntity);
            var events = _entityManager.GetBuffer<BandDoctrineDecisionEvent>(bandEntity);
            Assert.AreEqual(1, events.Length);
            Assert.AreEqual(BandRequestDisposition.Denied, events[0].Disposition);
            Assert.LessOrEqual(social.CrewResentment, 0.05f);
            Assert.Greater(social.CommandTrust, 0.5f);
        }

        [Test]
        public void Governance_CorruptCommand_ProtectsKnownMembersMoreThanUnknown()
        {
            var unknownBand = _entityManager.CreateEntity(typeof(Band), typeof(BandFormation));
            var knownBand = _entityManager.CreateEntity(typeof(Band), typeof(BandFormation));
            _entityManager.SetComponentData(unknownBand, new Band
            {
                Status = BandStatus.Idle,
                Cohesion = 0.72f,
                Morale = 0.58f,
                Fatigue = 0.25f
            });
            _entityManager.SetComponentData(knownBand, new Band
            {
                Status = BandStatus.Idle,
                Cohesion = 0.72f,
                Morale = 0.58f,
                Fatigue = 0.25f
            });
            _entityManager.SetComponentData(unknownBand, new BandFormation
            {
                Formation = BandFormationType.Line,
                Spacing = 2f,
                Width = 8f,
                Depth = 2f
            });
            _entityManager.SetComponentData(knownBand, new BandFormation
            {
                Formation = BandFormationType.Line,
                Spacing = 2f,
                Width = 8f,
                Depth = 2f
            });

            var unknownMembers = _entityManager.AddBuffer<BandMember>(unknownBand);
            unknownMembers.Add(new BandMember { Villager = _entityManager.CreateEntity(), LoyaltyScore = 10, JoinedTick = 0 });
            unknownMembers.Add(new BandMember { Villager = _entityManager.CreateEntity(), LoyaltyScore = 12, JoinedTick = 0 });
            unknownMembers.Add(new BandMember { Villager = _entityManager.CreateEntity(), LoyaltyScore = 15, JoinedTick = 0 });

            var knownMembers = _entityManager.AddBuffer<BandMember>(knownBand);
            knownMembers.Add(new BandMember { Villager = _entityManager.CreateEntity(), LoyaltyScore = 80, JoinedTick = 0 });
            knownMembers.Add(new BandMember { Villager = _entityManager.CreateEntity(), LoyaltyScore = 82, JoinedTick = 0 });
            knownMembers.Add(new BandMember { Villager = _entityManager.CreateEntity(), LoyaltyScore = 85, JoinedTick = 0 });

            var bootstrap = _world.GetOrCreateSystem<BandDoctrineBootstrapSystem>();
            UpdateInitSystem(bootstrap);

            ConfigureCorruptProfileAndThreshold(unknownBand);
            ConfigureCorruptProfileAndThreshold(knownBand);

            AddManualRotationRequest(unknownBand, 0.7f, 0.8f, 0.5f);
            AddManualRotationRequest(knownBand, 0.7f, 0.8f, 0.5f);

            var governanceSystem = _world.GetOrCreateSystem<BandDoctrineGovernanceSystem>();
            UpdateSimSystem(governanceSystem);

            var unknownEvents = _entityManager.GetBuffer<BandDoctrineDecisionEvent>(unknownBand);
            var knownEvents = _entityManager.GetBuffer<BandDoctrineDecisionEvent>(knownBand);
            Assert.AreEqual(1, unknownEvents.Length);
            Assert.AreEqual(1, knownEvents.Length);
            Assert.AreEqual(BandRequestDisposition.Denied, unknownEvents[0].Disposition);
            Assert.AreEqual(BandRequestDisposition.Approved, knownEvents[0].Disposition);
        }

        [Test]
        public void Governance_Whistleblow_SucceedsWithCharismaAndEvidence()
        {
            var bandEntity = _entityManager.CreateEntity(typeof(Band), typeof(BandFormation));
            _entityManager.SetComponentData(bandEntity, new Band
            {
                Status = BandStatus.Idle,
                Cohesion = 0.66f,
                Morale = 0.52f,
                Fatigue = 0.4f
            });
            _entityManager.SetComponentData(bandEntity, new BandFormation
            {
                Formation = BandFormationType.Line,
                Spacing = 2f,
                Width = 7f,
                Depth = 2f
            });

            var bootstrap = _world.GetOrCreateSystem<BandDoctrineBootstrapSystem>();
            UpdateInitSystem(bootstrap);

            var hierarchy = _entityManager.GetComponentData<BandCommandHierarchy>(bandEntity);
            hierarchy.HighCommandCorruption = 0.35f;
            hierarchy.HighCommandSuppression = 0.2f;
            hierarchy.WhistleblowThresholdBase = 0.5f;
            _entityManager.SetComponentData(bandEntity, hierarchy);

            var autonomy = _entityManager.GetComponentData<BandCommandAutonomy>(bandEntity);
            autonomy.CaptainCharisma = 0.88f;
            autonomy.CaptainIntegrity = 0.85f;
            autonomy.WhistleblowRiskTolerance = 0.82f;
            _entityManager.SetComponentData(bandEntity, autonomy);

            var social = _entityManager.GetComponentData<BandSocialState>(bandEntity);
            social.ObservedCorruptionEvidence = 0.8f;
            social.CommandTrust = 0.52f;
            _entityManager.SetComponentData(bandEntity, social);

            var context = _entityManager.GetComponentData<BandDoctrineContext>(bandEntity);
            context.ScrutinyPressure = 0.92f;
            context.Criticality = 0.88f;
            context.CasualtyRisk = 0.75f;
            context.AttritionPressure = 0.7f;
            _entityManager.SetComponentData(bandEntity, context);

            var governanceSystem = _world.GetOrCreateSystem<BandDoctrineGovernanceSystem>();
            UpdateSimSystem(governanceSystem);

            var escalations = _entityManager.GetBuffer<BandCommandEscalationEvent>(bandEntity);
            var updatedSocial = _entityManager.GetComponentData<BandSocialState>(bandEntity);
            Assert.AreEqual(1, escalations.Length);
            Assert.AreEqual(BandEscalationDisposition.Succeeded, escalations[0].Disposition);
            Assert.Greater(updatedSocial.CommandTrust, 0.52f);
        }

        [Test]
        public void Governance_Whistleblow_FailsAgainstSuppression()
        {
            var bandEntity = _entityManager.CreateEntity(typeof(Band), typeof(BandFormation));
            _entityManager.SetComponentData(bandEntity, new Band
            {
                Status = BandStatus.Idle,
                Cohesion = 0.66f,
                Morale = 0.52f,
                Fatigue = 0.4f
            });
            _entityManager.SetComponentData(bandEntity, new BandFormation
            {
                Formation = BandFormationType.Line,
                Spacing = 2f,
                Width = 7f,
                Depth = 2f
            });

            var bootstrap = _world.GetOrCreateSystem<BandDoctrineBootstrapSystem>();
            UpdateInitSystem(bootstrap);

            var hierarchy = _entityManager.GetComponentData<BandCommandHierarchy>(bandEntity);
            hierarchy.HighCommandCorruption = 0.92f;
            hierarchy.HighCommandSuppression = 0.9f;
            hierarchy.WhistleblowThresholdBase = 0.72f;
            _entityManager.SetComponentData(bandEntity, hierarchy);

            var autonomy = _entityManager.GetComponentData<BandCommandAutonomy>(bandEntity);
            autonomy.CaptainCharisma = 0.3f;
            autonomy.CaptainIntegrity = 0.45f;
            autonomy.WhistleblowRiskTolerance = 0.5f;
            _entityManager.SetComponentData(bandEntity, autonomy);

            var social = _entityManager.GetComponentData<BandSocialState>(bandEntity);
            social.ObservedCorruptionEvidence = 0.85f;
            social.CommandTrust = 0.55f;
            _entityManager.SetComponentData(bandEntity, social);

            var context = _entityManager.GetComponentData<BandDoctrineContext>(bandEntity);
            context.ScrutinyPressure = 0.9f;
            context.Criticality = 0.9f;
            context.CasualtyRisk = 0.82f;
            context.AttritionPressure = 0.78f;
            _entityManager.SetComponentData(bandEntity, context);

            var governanceSystem = _world.GetOrCreateSystem<BandDoctrineGovernanceSystem>();
            UpdateSimSystem(governanceSystem);

            var escalations = _entityManager.GetBuffer<BandCommandEscalationEvent>(bandEntity);
            var updatedSocial = _entityManager.GetComponentData<BandSocialState>(bandEntity);
            Assert.AreEqual(1, escalations.Length);
            Assert.AreEqual(BandEscalationDisposition.Failed, escalations[0].Disposition);
            Assert.Less(updatedSocial.CommandTrust, 0.55f);
        }

        private void ConfigureCorruptProfileAndThreshold(Entity bandEntity)
        {
            var profile = _entityManager.GetComponentData<BandDoctrineProfile>(bandEntity);
            profile.AuthoritarianBias = 0.8f;
            profile.EgalitarianBias = 0.2f;
            profile.CrueltyBias = 0.7f;
            profile.CorruptionBias = 0.9f;
            profile.GoalRigidity = 0.8f;
            _entityManager.SetComponentData(bandEntity, profile);

            var hierarchy = _entityManager.GetComponentData<BandCommandHierarchy>(bandEntity);
            hierarchy.ApprovalThresholdBase = 0.2f;
            hierarchy.ApprovalThresholdScale = 0.05f;
            _entityManager.SetComponentData(bandEntity, hierarchy);

            var context = _entityManager.GetComponentData<BandDoctrineContext>(bandEntity);
            context.ObjectivePressure = 0.2f;
            context.CasualtyRisk = 0.7f;
            context.AttritionPressure = 0.6f;
            context.RotationDemand = 0.65f;
            _entityManager.SetComponentData(bandEntity, context);
        }

        private void AddManualRotationRequest(Entity bandEntity, float urgency, float casualtyConcern, float moraleConcern)
        {
            var requests = _entityManager.GetBuffer<BandDoctrineRequest>(bandEntity);
            requests.Add(new BandDoctrineRequest
            {
                RequestedModule = BandDoctrineModuleType.Rotation,
                RequesterRole = BandCommandRole.Captain,
                RequesterEntity = Entity.Null,
                Urgency = urgency,
                CasualtyConcern = casualtyConcern,
                MoraleConcern = moraleConcern
            });
        }

        private void UpdateInitSystem(SystemHandle handle)
        {
            _initGroup.RemoveSystemFromUpdateList(handle);
            _initGroup.AddSystemToUpdateList(handle);
            _initGroup.SortSystems();
            _initGroup.Update();
        }

        private void UpdateSimSystem(SystemHandle handle)
        {
            _simGroup.RemoveSystemFromUpdateList(handle);
            _simGroup.AddSystemToUpdateList(handle);
            _simGroup.SortSystems();
            _simGroup.Update();
        }
    }
}
