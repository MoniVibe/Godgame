using Godgame.Bands;
using NUnit.Framework;
using PureDOTS.Runtime.Components;
using PureDOTS.Systems;
using Unity.Entities;
using Unity.Mathematics;

namespace Godgame.Tests.Gameplay
{
    [TestFixture]
    public class BandSocioDynamicsSystemTests
    {
        private World _world;
        private EntityManager _entityManager;
        private InitializationSystemGroup _initGroup;
        private SimulationSystemGroup _simGroup;

        [SetUp]
        public void SetUp()
        {
            _world = new World("BandSocioDynamicsSystemTests");
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
        public void MoraleAboveTenPercent_RemainsObedient()
        {
            var band = CreateBand(0.35f);
            BootstrapAndRunSocioDynamics(band);

            var discipline = _entityManager.GetComponentData<BandDisciplineState>(band);
            Assert.AreEqual(BandComplianceState.Obedient, discipline.ComplianceState);
        }

        [Test]
        public void MoraleBelowTenPercent_WithDivergence_EntersItalianStrike()
        {
            var band = CreateBand(0.09f);
            BootstrapOnly(band);

            var climate = _entityManager.GetComponentData<BandOrderClimate>(band);
            climate.CurrentOrderDivergence = 0.82f;
            climate.AggregateGrievance = 0.75f;
            _entityManager.SetComponentData(band, climate);

            RunSocioDynamics();

            var discipline = _entityManager.GetComponentData<BandDisciplineState>(band);
            Assert.AreEqual(BandComplianceState.ItalianStrike, discipline.ComplianceState);
        }

        [Test]
        public void MoraleBelowFivePercent_EntersOpenMutiny()
        {
            var band = CreateBand(0.04f);
            BootstrapAndRunSocioDynamics(band);

            var discipline = _entityManager.GetComponentData<BandDisciplineState>(band);
            Assert.AreEqual(BandComplianceState.OpenMutiny, discipline.ComplianceState);
        }

        [Test]
        public void ExecutionHurtsSympathizersMore()
        {
            var sympathizerBand = CreateBand(0.6f);
            var loyalistBand = CreateBand(0.6f);
            BootstrapOnly(sympathizerBand);
            BootstrapOnly(loyalistBand);

            var doctrineSym = _entityManager.GetComponentData<BandDoctrineProfile>(sympathizerBand);
            doctrineSym.EgalitarianBias = 0.9f;
            doctrineSym.AuthoritarianBias = 0.1f;
            _entityManager.SetComponentData(sympathizerBand, doctrineSym);
            var socioSym = _entityManager.GetComponentData<BandSocioProfile>(sympathizerBand);
            socioSym.ChaosAxis = 0.9f;
            socioSym.MutinySympathy = 0.85f;
            socioSym.DueProcessPreference = 0.2f;
            _entityManager.SetComponentData(sympathizerBand, socioSym);

            var doctrineLoy = _entityManager.GetComponentData<BandDoctrineProfile>(loyalistBand);
            doctrineLoy.EgalitarianBias = 0.1f;
            doctrineLoy.AuthoritarianBias = 0.9f;
            _entityManager.SetComponentData(loyalistBand, doctrineLoy);
            var socioLoy = _entityManager.GetComponentData<BandSocioProfile>(loyalistBand);
            socioLoy.ChaosAxis = 0.1f;
            socioLoy.MutinySympathy = 0.1f;
            socioLoy.DueProcessPreference = 0.8f;
            _entityManager.SetComponentData(loyalistBand, socioLoy);

            AddExecutionEvent(sympathizerBand, 0.85f, true);
            AddExecutionEvent(loyalistBand, 0.85f, true);

            RunSocioDynamics();

            var moraleSym = _entityManager.GetComponentData<Band>(sympathizerBand).Morale;
            var moraleLoy = _entityManager.GetComponentData<Band>(loyalistBand).Morale;
            Assert.Less(moraleSym, moraleLoy);
        }

        [Test]
        public void RumorDrivenBand_AbsorbsMoreDeceptionBias()
        {
            var rumorBand = CreateBand(0.45f);
            var verifiedBand = CreateBand(0.45f);
            BootstrapOnly(rumorBand);
            BootstrapOnly(verifiedBand);

            var rumorProfile = _entityManager.GetComponentData<BandSocioProfile>(rumorBand);
            rumorProfile.RumorImpulse = 0.95f;
            rumorProfile.ChaosAxis = 0.9f;
            _entityManager.SetComponentData(rumorBand, rumorProfile);

            var verifiedProfile = _entityManager.GetComponentData<BandSocioProfile>(verifiedBand);
            verifiedProfile.RumorImpulse = 0.05f;
            verifiedProfile.ChaosAxis = 0.1f;
            _entityManager.SetComponentData(verifiedBand, verifiedProfile);

            AddRumorReport(rumorBand, 0.75f);
            AddRumorReport(verifiedBand, 0.75f);

            RunSocioDynamics();

            var rumorContext = _entityManager.GetComponentData<BandDoctrineContext>(rumorBand);
            var verifiedContext = _entityManager.GetComponentData<BandDoctrineContext>(verifiedBand);
            Assert.Greater(rumorContext.FrontThreat, verifiedContext.FrontThreat + 0.1f);
        }

        [Test]
        public void PermanentMemoriesPersist_WhileMinorGrievancesDecay()
        {
            var band = CreateBand(0.55f);
            BootstrapOnly(band);

            var memories = _entityManager.GetBuffer<BandMemoryEvent>(band);
            memories.Add(new BandMemoryEvent
            {
                Type = BandMemoryType.Heroism,
                Salience = 0.9f,
                DecayRate = 0.2f,
                Flags = BandMemoryFlags.None
            });
            memories.Add(new BandMemoryEvent
            {
                Type = BandMemoryType.Grievance,
                Salience = 0.02f,
                DecayRate = 0.05f,
                Flags = BandMemoryFlags.None
            });

            RunSocioDynamics();

            memories = _entityManager.GetBuffer<BandMemoryEvent>(band);
            var heroismCount = 0;
            var grievanceCount = 0;
            for (var i = 0; i < memories.Length; i++)
            {
                if (memories[i].Type == BandMemoryType.Heroism)
                {
                    heroismCount++;
                }
                if (memories[i].Type == BandMemoryType.Grievance)
                {
                    grievanceCount++;
                }
            }

            Assert.AreEqual(1, heroismCount);
            Assert.AreEqual(0, grievanceCount);
        }

        [Test]
        public void LowEvidenceCrewExecution_RaisesScapegoatBiasAndRadicalization()
        {
            var band = CreateBand(0.58f);
            BootstrapOnly(band);

            var doctrine = _entityManager.GetComponentData<BandDoctrineProfile>(band);
            doctrine.AuthoritarianBias = 0.82f;
            doctrine.EgalitarianBias = 0.18f;
            doctrine.CorruptionBias = 0.78f;
            _entityManager.SetComponentData(band, doctrine);

            var socio = _entityManager.GetComponentData<BandSocioProfile>(band);
            socio.ChaosAxis = 0.82f;
            socio.DueProcessPreference = 0.12f;
            socio.NepotismTolerance = 0.72f;
            _entityManager.SetComponentData(band, socio);

            AddJusticeEvent(
                band,
                BandJusticeOutcome.Execution,
                BandJusticeTargetClass.Crew,
                severity: 0.92f,
                evidence: 0.1f,
                affinity: 0.08f,
                isPublic: true);

            RunSocioDynamics();

            var governance = _entityManager.GetComponentData<BandGovernancePulse>(band);
            var discipline = _entityManager.GetComponentData<BandDisciplineState>(band);
            var moraleAfter = _entityManager.GetComponentData<Band>(band).Morale;

            Assert.Greater(governance.ScapegoatBias, 0.05f);
            Assert.Greater(discipline.Radicalization, 0.02f);
            Assert.Less(moraleAfter, 0.58f);
        }

        [Test]
        public void HighEvidenceDueProcess_ImprovesJusticeCredibilityAndTrust()
        {
            var band = CreateBand(0.5f);
            BootstrapOnly(band);

            var doctrine = _entityManager.GetComponentData<BandDoctrineProfile>(band);
            doctrine.AuthoritarianBias = 0.22f;
            doctrine.EgalitarianBias = 0.75f;
            doctrine.CorruptionBias = 0.08f;
            _entityManager.SetComponentData(band, doctrine);

            var socio = _entityManager.GetComponentData<BandSocioProfile>(band);
            socio.ChaosAxis = 0.08f;
            socio.DueProcessPreference = 0.95f;
            socio.NepotismTolerance = 0.08f;
            socio.InstitutionLoyalty = 0.85f;
            _entityManager.SetComponentData(band, socio);

            AddJusticeEvent(
                band,
                BandJusticeOutcome.Demotion,
                BandJusticeTargetClass.Officer,
                severity: 0.7f,
                evidence: 0.93f,
                affinity: 0.2f,
                isPublic: true);

            RunSocioDynamics();

            var governance = _entityManager.GetComponentData<BandGovernancePulse>(band);
            var social = _entityManager.GetComponentData<BandSocialState>(band);
            Assert.Greater(governance.JusticeCredibility, 0.5f);
            Assert.Greater(governance.ExternalLegitimacy, 0.5f);
            Assert.Greater(social.CommandTrust, 0.5f);
        }

        [Test]
        public void ConnectedLeniency_HurtsEgalitarianBandsMoreThanAuthoritarianBands()
        {
            var authoritarianBand = CreateBand(0.6f);
            var egalitarianBand = CreateBand(0.6f);
            BootstrapOnly(authoritarianBand);
            BootstrapOnly(egalitarianBand);

            var doctrineAuth = _entityManager.GetComponentData<BandDoctrineProfile>(authoritarianBand);
            doctrineAuth.AuthoritarianBias = 0.9f;
            doctrineAuth.EgalitarianBias = 0.1f;
            doctrineAuth.CorruptionBias = 0.62f;
            _entityManager.SetComponentData(authoritarianBand, doctrineAuth);

            var doctrineEgal = _entityManager.GetComponentData<BandDoctrineProfile>(egalitarianBand);
            doctrineEgal.AuthoritarianBias = 0.1f;
            doctrineEgal.EgalitarianBias = 0.9f;
            doctrineEgal.CorruptionBias = 0.62f;
            _entityManager.SetComponentData(egalitarianBand, doctrineEgal);

            var socioAuth = _entityManager.GetComponentData<BandSocioProfile>(authoritarianBand);
            socioAuth.NepotismTolerance = 0.85f;
            socioAuth.ChaosAxis = 0.25f;
            socioAuth.DueProcessPreference = 0.3f;
            _entityManager.SetComponentData(authoritarianBand, socioAuth);

            var socioEgal = _entityManager.GetComponentData<BandSocioProfile>(egalitarianBand);
            socioEgal.NepotismTolerance = 0.1f;
            socioEgal.ChaosAxis = 0.85f;
            socioEgal.DueProcessPreference = 0.65f;
            _entityManager.SetComponentData(egalitarianBand, socioEgal);

            AddJusticeEvent(
                authoritarianBand,
                BandJusticeOutcome.Fine,
                BandJusticeTargetClass.Elite,
                severity: 0.3f,
                evidence: 0.88f,
                affinity: 0.95f,
                isPublic: true);
            AddJusticeEvent(
                egalitarianBand,
                BandJusticeOutcome.Fine,
                BandJusticeTargetClass.Elite,
                severity: 0.3f,
                evidence: 0.88f,
                affinity: 0.95f,
                isPublic: true);

            RunSocioDynamics();

            var authGov = _entityManager.GetComponentData<BandGovernancePulse>(authoritarianBand);
            var egalGov = _entityManager.GetComponentData<BandGovernancePulse>(egalitarianBand);
            var authDiscipline = _entityManager.GetComponentData<BandDisciplineState>(authoritarianBand);
            var egalDiscipline = _entityManager.GetComponentData<BandDisciplineState>(egalitarianBand);
            Assert.Greater(authGov.InternalEliteSupport, 0.5f);
            Assert.Less(egalGov.ExternalLegitimacy, authGov.ExternalLegitimacy);
            Assert.Greater(egalDiscipline.Radicalization, authDiscipline.Radicalization);
        }

        [Test]
        public void OpenMutiny_WithStrongMeans_PrefersEscapeIntent()
        {
            var band = CreateBand(0.03f);
            BootstrapOnly(band);

            var means = _entityManager.GetComponentData<BandSplinterMeans>(band);
            means.OwnShipAccess = 0.9f;
            means.SeizureCapability = 0.75f;
            means.ProvisioningReadiness = 0.8f;
            means.TravelNetworkAccess = 0.7f;
            _entityManager.SetComponentData(band, means);

            var discipline = _entityManager.GetComponentData<BandDisciplineState>(band);
            discipline.SplinterPressure = 0.7f;
            discipline.SecretCoordination = 0.8f;
            _entityManager.SetComponentData(band, discipline);

            RunSocioDynamics();

            var intent = _entityManager.GetComponentData<BandSplinterIntentState>(band);
            Assert.AreEqual(BandSplinterIntentType.Escape, intent.ActiveIntent);
        }

        [Test]
        public void OpenMutiny_WithHighDriftAndSeizure_PrefersCaptureLeaderIntent()
        {
            var band = CreateBand(0.03f);
            BootstrapOnly(band);

            var means = _entityManager.GetComponentData<BandSplinterMeans>(band);
            means.OwnShipAccess = 0.1f;
            means.SeizureCapability = 0.95f;
            means.ProvisioningReadiness = 0.3f;
            means.TravelNetworkAccess = 0.2f;
            _entityManager.SetComponentData(band, means);

            var climate = _entityManager.GetComponentData<BandOrderClimate>(band);
            climate.CurrentOrderDivergence = 0.95f;
            _entityManager.SetComponentData(band, climate);

            var governance = _entityManager.GetComponentData<BandGovernancePulse>(band);
            governance.NepotismBias = 0.85f;
            governance.ScapegoatBias = 0.85f;
            _entityManager.SetComponentData(band, governance);

            var socio = _entityManager.GetComponentData<BandSocioProfile>(band);
            socio.InstitutionLoyalty = 0.1f;
            _entityManager.SetComponentData(band, socio);

            var discipline = _entityManager.GetComponentData<BandDisciplineState>(band);
            discipline.SecretCoordination = 0.9f;
            discipline.SplinterPressure = 0.5f;
            _entityManager.SetComponentData(band, discipline);

            RunSocioDynamics();

            var intent = _entityManager.GetComponentData<BandSplinterIntentState>(band);
            Assert.AreEqual(BandSplinterIntentType.CaptureLeader, intent.ActiveIntent);
        }

        private Entity CreateBand(float morale)
        {
            var entity = _entityManager.CreateEntity(typeof(Band));
            _entityManager.SetComponentData(entity, new Band
            {
                Status = BandStatus.Idle,
                Morale = morale,
                Cohesion = 0.7f,
                Fatigue = 0.2f
            });
            return entity;
        }

        private void AddExecutionEvent(Entity band, float severity, bool isPublic)
        {
            var orders = _entityManager.GetBuffer<BandOrderEvent>(band);
            orders.Add(new BandOrderEvent
            {
                Type = BandOrderEventType.Execution,
                Divergence = 0.7f,
                Severity = severity,
                IsPublic = isPublic ? (byte)1 : (byte)0
            });
        }

        private void AddRumorReport(Entity band, float threatBias)
        {
            var reports = _entityManager.GetBuffer<BandIntelReport>(band);
            reports.Add(new BandIntelReport
            {
                Quality = BandIntelQuality.Rumor,
                ThreatBias = threatBias,
                ObjectiveBias = 0.45f,
                DeceptionIntent = 0.9f,
                BeneficiaryBias = 0.8f,
                EvidenceStrength = 0.05f
            });
        }

        private void AddJusticeEvent(
            Entity band,
            BandJusticeOutcome outcome,
            BandJusticeTargetClass targetClass,
            float severity,
            float evidence,
            float affinity,
            bool isPublic)
        {
            var justiceEvents = _entityManager.GetBuffer<BandJusticeEvent>(band);
            justiceEvents.Add(new BandJusticeEvent
            {
                Outcome = outcome,
                TargetClass = targetClass,
                Severity = severity,
                EvidenceStrength = evidence,
                TargetAffinity = affinity,
                IsPublic = isPublic ? (byte)1 : (byte)0
            });
        }

        private void BootstrapOnly(Entity _)
        {
            var bootstrap = _world.GetOrCreateSystem<BandDoctrineBootstrapSystem>();
            UpdateInitSystem(bootstrap);
        }

        private void RunSocioDynamics()
        {
            var system = _world.GetOrCreateSystem<BandSocioDynamicsSystem>();
            UpdateSimSystem(system);
        }

        private void BootstrapAndRunSocioDynamics(Entity band)
        {
            BootstrapOnly(band);
            RunSocioDynamics();
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
