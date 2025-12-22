#if UNITY_INCLUDE_TESTS
using Godgame.Villagers;
using NUnit.Framework;
using PureDOTS.Runtime.AI;
using PureDOTS.Runtime.Combat;
using PureDOTS.Runtime.Components;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Godgame.Tests
{
    public sealed class VillagerIntentBridgeTests
    {
        private World _world;
        private EntityManager _entityManager;

        [SetUp]
        public void SetUp()
        {
            _world = new World(nameof(VillagerIntentBridgeTests));
            _entityManager = _world.EntityManager;

            _entityManager.CreateEntity(typeof(GameWorldTag));
            _entityManager.SetComponentData(_entityManager.CreateEntity(typeof(TimeState)), new TimeState
            {
                Tick = 0,
                FixedDeltaTime = 0.1f,
                IsPaused = false
            });
            var rewindEntity = _entityManager.CreateEntity(typeof(RewindState), typeof(RewindLegacyState));
            _entityManager.SetComponentData(rewindEntity, new RewindState
            {
                Mode = RewindMode.Record,
                TargetTick = 0,
                TickDuration = 0.1f,
                MaxHistoryTicks = 600,
                PendingStepTicks = 0
            });
            _entityManager.SetComponentData(rewindEntity, new RewindLegacyState
            {
                PlaybackSpeed = 1f,
                CurrentTick = 0,
                StartTick = 0,
                PlaybackTick = 0,
                PlaybackTicksPerSecond = 10f,
                ScrubDirection = 0,
                ScrubSpeedMultiplier = 1f,
                RewindWindowTicks = 0,
                ActiveTrack = default
            });

            var cadence = MindCadenceSettings.CreateDefault();
            cadence.EvaluationCadenceTicks = 1;
            _entityManager.SetComponentData(_entityManager.CreateEntity(typeof(MindCadenceSettings)), cadence);
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
        public void FleeIntentOverridesNavigation()
        {
            var villager = CreateVillagerEntity();
            _entityManager.SetComponentData(villager, new VillagerGoalState
            {
                CurrentGoal = VillagerGoal.Flee
            });
            _entityManager.SetComponentData(villager, new VillagerFleeIntent
            {
                ExitDirection = new float3(1f, 0f, 0f),
                Urgency = 1f
            });

            var system = _world.GetOrCreateSystem<VillagerIntentBridgeSystem>();
            system.Update(_world.Unmanaged);

            var nav = _entityManager.GetComponentData<Navigation>(villager);
            Assert.AreNotEqual(float3.zero, nav.Destination);
            Assert.Greater(nav.Speed, 0f);
            var job = _entityManager.GetComponentData<VillagerJobState>(villager);
            Assert.AreEqual(JobPhase.Idle, job.Phase);
        }

        [Test]
        public void NonWorkGoalResetsJob()
        {
            var villager = CreateVillagerEntity();
            _entityManager.SetComponentData(villager, new VillagerGoalState
            {
                CurrentGoal = VillagerGoal.Sleep
            });

            var system = _world.GetOrCreateSystem<VillagerIntentBridgeSystem>();
            system.Update(_world.Unmanaged);

            var job = _entityManager.GetComponentData<VillagerJobState>(villager);
            Assert.AreEqual(JobPhase.Idle, job.Phase);
            Assert.AreEqual(Entity.Null, job.Target);
        }

        private Entity CreateVillagerEntity()
        {
            var entity = _entityManager.CreateEntity(
                typeof(VillagerJobState),
                typeof(Navigation),
                typeof(LocalTransform),
                typeof(VillagerGoalState),
                typeof(FocusBudget),
                typeof(VillagerFleeIntent),
                typeof(VillagerMindCadence),
                typeof(HazardAvoidanceState));

            _entityManager.SetComponentData(entity, LocalTransform.Identity);
            _entityManager.SetComponentData(entity, new VillagerJobState
            {
                Phase = JobPhase.Gather,
                Target = Entity.Null
            });
            _entityManager.SetComponentData(entity, new Navigation
            {
                Destination = float3.zero,
                Speed = 5f
            });
            _entityManager.SetComponentData(entity, new FocusBudget
            {
                Current = 1f,
                Max = 1f,
                RegenPerTick = 0.05f,
                Reserved = 0f,
                IsLocked = 0
            });
            _entityManager.SetComponentData(entity, new VillagerMindCadence
            {
                CadenceTicks = 1,
                LastRunTick = 0
            });
            _entityManager.SetComponentData(entity, new HazardAvoidanceState
            {
                CurrentAdjustment = new float3(0f, 0f, 1f),
                AvoidanceUrgency = 0.5f
            });

            return entity;
        }
    }
}
#endif
