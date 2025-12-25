using Godgame.Resources;
using Godgame.Runtime;
using Godgame.Systems.Resources;
using NUnit.Framework;
using PureDOTS.Runtime.Components;
using PureDOTS.Runtime.Hand;
using Unity.Entities;
using Unity.Mathematics;
using GodgameDivineHandState = Godgame.Runtime.DivineHandState;

namespace Godgame.Tests.Resources
{
    public class HandToAggregatePileSystemTests
    {
        private World _world;
        private EntityManager _entityManager;
        private SimulationSystemGroup _simGroup;
        private Entity _configEntity;

        [SetUp]
        public void SetUp()
        {
            _world = new World(nameof(HandToAggregatePileSystemTests));
            _entityManager = _world.EntityManager;
            _world.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
            _simGroup = _world.GetOrCreateSystemManaged<SimulationSystemGroup>();

            _configEntity = _entityManager.CreateEntity(
                typeof(AggregatePileConfig),
                typeof(AggregatePileRuntimeState));

            _entityManager.SetComponentData(_configEntity, new AggregatePileConfig
            {
                DefaultMaxCapacity = 100f,
                GlobalMaxCapacity = 100f,
                MergeRadius = 5f,
                SplitThreshold = 0f,
                MergeCheckSeconds = 1f,
                MinSpawnAmount = 0.01f,
                ConservationEpsilon = 0.001f,
                MaxActivePiles = 8
            });
            _entityManager.SetComponentData(_configEntity, new AggregatePileRuntimeState
            {
                ActivePiles = 0,
                NextMergeTime = 0f
            });

            var timeEntity = _entityManager.CreateEntity(typeof(TimeState));
            _entityManager.SetComponentData(timeEntity, new TimeState
            {
                Tick = 0,
                DeltaTime = 1f / 60f,
                DeltaSeconds = 1f / 60f,
                ElapsedTime = 0f,
                WorldSeconds = 0f,
                IsPaused = false,
                FixedDeltaTime = 1f / 60f,
                CurrentSpeedMultiplier = 1f
            });
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
        public void GroundDrip_ConvertsHeldResourcesToPileCommand()
        {
            var handEntity = _entityManager.CreateEntity(
                typeof(DivineHandTag),
                typeof(GodgameDivineHandState));
            _entityManager.AddBuffer<HandCommand>(handEntity);

            _entityManager.SetComponentData(handEntity, new GodgameDivineHandState
            {
                HeldAmount = 25,
                HeldResourceTypeIndex = 2,
                CursorPosition = new float3(3f, 0f, 4f)
            });
            var commandBuffer = _entityManager.GetBuffer<HandCommand>(handEntity);
            commandBuffer.Add(new HandCommand
            {
                Tick = 0,
                Type = HandCommandType.Dump,
                TargetEntity = Entity.Null,
                TargetPosition = new float3(3f, 0f, 4f)
            });

            UpdateSystem(_world.GetOrCreateSystem<HandToAggregatePileSystem>());

            var commands = _entityManager.GetBuffer<AggregatePileSpawnCommand>(_configEntity);
            Assert.AreEqual(1, commands.Length);
            Assert.AreEqual(2, commands[0].ResourceType);
            Assert.AreEqual(25f, commands[0].Amount, 0.0001f);
            Assert.AreEqual(new float3(3f, 0f, 4f), commands[0].Position);

            var handState = _entityManager.GetComponentData<GodgameDivineHandState>(handEntity);
            Assert.AreEqual(0, handState.HeldAmount);
            Assert.AreEqual(DivineHandConstants.NoResourceType, handState.HeldResourceTypeIndex);

            var command = _entityManager.GetBuffer<HandCommand>(handEntity);
            Assert.AreEqual(0, command.Length);
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
