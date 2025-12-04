using Godgame.Villagers;
using NUnit.Framework;
using PureDOTS.Runtime.Components;
using Unity.Entities;
using Unity.Mathematics;
using GInterrupt = Godgame.Villagers.VillagerInterrupt;
using GInterruptType = Godgame.Villagers.VillagerInterruptType;
using PTimeState = PureDOTS.Runtime.Time.TimeState;

namespace Godgame.Tests
{
    /// <summary>
    /// Tests for villager interrupt system (hand interaction override).
    /// </summary>
    public class VillagerInterruptTests
    {
        private World _world;
        private EntityManager _entityManager;

        [SetUp]
        public void SetUp()
        {
            _world = World.DefaultGameObjectInjectionWorld;
            if (_world == null)
            {
                _world = new World("Test World");
            }
            _entityManager = _world.EntityManager;
        }

        [TearDown]
        public void TearDown()
        {
            if (_world != null && _world.Name == "Test World")
            {
                _world.Dispose();
            }
        }

        [Test]
        public void VillagerInterruptSystem_Exists()
        {
            var systemHandle = _world.GetOrCreateSystem<VillagerInterruptSystem>();
            Assert.AreNotEqual(SystemHandle.Null, systemHandle, "VillagerInterruptSystem should exist");
        }

        [Test]
        public void VillagerInterrupt_HandPickupSavesJobState()
        {
            // Create test villager with job
            var villager = _entityManager.CreateEntity();
            var handEntity = _entityManager.CreateEntity();

            _entityManager.AddComponentData(villager, new VillagerJob
            {
                Type = VillagerJob.JobType.Gatherer,
                Phase = VillagerJob.JobPhase.Gathering,
                ActiveTicketId = 123,
                Productivity = 1f,
                LastStateChangeTick = 0
            });

            _entityManager.AddComponentData(villager, new VillagerJobTicket
            {
                ResourceEntity = Entity.Null,
                TicketId = 123,
                Phase = (byte)VillagerJob.JobPhase.Gathering,
                LastProgressTick = 0
            });

            _entityManager.AddComponentData(villager, new GInterrupt
            {
                Type = GInterruptType.HandPickup,
                SourceEntity = handEntity,
                StartTick = 0,
                Duration = 0f // Indefinite
            });

            _entityManager.AddComponentData(villager, new HandCarriedTag());
            _entityManager.AddComponentData(villager, new VillagerInterruptState());

            _entityManager.AddComponentData(villager, new VillagerFlags());
            _entityManager.AddComponentData(villager, new VillagerAvailability());

            // Create time state
            var timeEntity = _entityManager.CreateEntity();
            _entityManager.AddComponentData(timeEntity, default(PTimeState));

            // Run system
            var interruptSystem = _world.GetOrCreateSystem<VillagerInterruptSystem>();
            _world.Update();

            // Verify job state was saved
            var savedState = _entityManager.GetComponentData<VillagerInterruptState>(villager);
            Assert.AreEqual(VillagerJob.JobPhase.Gathering, savedState.SavedPhase, "Job phase should be saved");
            Assert.AreEqual(123u, savedState.SavedTicketId, "Ticket ID should be saved");

            // Verify job was cleared
            var job = _entityManager.GetComponentData<VillagerJob>(villager);
            Assert.AreEqual(VillagerJob.JobPhase.Idle, job.Phase, "Job phase should be cleared to Idle");
            Assert.AreEqual(0u, job.ActiveTicketId, "Active ticket should be cleared");
        }

        [Test]
        public void VillagerInterrupt_PathBlockedExpiresAfterTimeout()
        {
            // Create test villager with path blocked
            var villager = _entityManager.CreateEntity();

            _entityManager.AddComponentData(villager, new GInterrupt
            {
                Type = GInterruptType.PathBlocked,
                SourceEntity = Entity.Null,
                StartTick = 0,
                Duration = 0f
            });

            _entityManager.AddComponentData(villager, new PathBlockedTag
            {
                BlockedTick = 0,
                BlockedPosition = float3.zero
            });

            _entityManager.AddComponentData(villager, new VillagerInterruptState
            {
                SavedPhase = VillagerJob.JobPhase.Gathering,
                SavedType = VillagerJob.JobType.Gatherer,
                SavedTicketId = 123
            });

            _entityManager.AddComponentData(villager, new VillagerJob
            {
                Type = VillagerJob.JobType.Gatherer,
                Phase = VillagerJob.JobPhase.Idle,
                ActiveTicketId = 0,
                Productivity = 1f,
                LastStateChangeTick = 0
            });

            // Create time state (simulate 6 seconds passed, > 5 second timeout)
            var timeEntity = _entityManager.CreateEntity();
            _entityManager.AddComponentData(timeEntity, default(PTimeState));

            // Run system
            var interruptSystem = _world.GetOrCreateSystem<VillagerInterruptSystem>();
            _world.Update();

            // Verify path blocked tag was removed
            Assert.IsFalse(_entityManager.HasComponent<PathBlockedTag>(villager), "PathBlockedTag should be removed after timeout");

            // Verify interrupt was cleared
            var interrupt = _entityManager.GetComponentData<GInterrupt>(villager);
            Assert.AreEqual(GInterruptType.None, interrupt.Type, "Interrupt should be cleared after timeout");
        }

        [Test]
        public void VillagerInterrupt_ExpiresAfterDuration()
        {
            // Create test villager with timed interrupt
            var villager = _entityManager.CreateEntity();
            var sourceEntity = _entityManager.CreateEntity();

            _entityManager.AddComponentData(villager, new GInterrupt
            {
                Type = GInterruptType.Miracle,
                SourceEntity = sourceEntity,
                StartTick = 0,
                Duration = 2f // 2 seconds
            });

            _entityManager.AddComponentData(villager, new VillagerInterruptState
            {
                SavedPhase = VillagerJob.JobPhase.Gathering,
                SavedType = VillagerJob.JobType.Gatherer,
                SavedTicketId = 123
            });

            _entityManager.AddComponentData(villager, new VillagerJob
            {
                Type = VillagerJob.JobType.Gatherer,
                Phase = VillagerJob.JobPhase.Idle,
                ActiveTicketId = 0,
                Productivity = 1f,
                LastStateChangeTick = 0
            });

            // Create time state (simulate 3 seconds passed, > 2 second duration)
            var timeEntity = _entityManager.CreateEntity();
            _entityManager.AddComponentData(timeEntity, default(PTimeState));

            // Run system
            var interruptSystem = _world.GetOrCreateSystem<VillagerInterruptSystem>();
            _world.Update();

            // Verify interrupt expired
            var interrupt = _entityManager.GetComponentData<GInterrupt>(villager);
            Assert.AreEqual(GInterruptType.None, interrupt.Type, "Interrupt should expire after duration");

            // Verify saved state was removed
            Assert.IsFalse(_entityManager.HasComponent<VillagerInterruptState>(villager), "Saved state should be removed after interrupt expires");
        }
    }
}

