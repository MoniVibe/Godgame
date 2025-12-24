#if UNITY_INCLUDE_TESTS
using Godgame.Systems;
using NUnit.Framework;
using PureDOTS.Runtime.Components;
using PureDOTS.Runtime.Intent;
using PureDOTS.Runtime.Interrupts;
using PureDOTS.Runtime.Villagers;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Godgame.Tests
{
    /// <summary>
    /// Tests for GodgameVillagerIntentBridgeSystem - validates intent-to-goal mapping and intent clearing.
    /// </summary>
    public sealed class GodgameVillagerIntentBridgeSystemTests
    {
        private World _world;
        private EntityManager _entityManager;

        [SetUp]
        public void SetUp()
        {
            _world = new World(nameof(GodgameVillagerIntentBridgeSystemTests), WorldFlags.Game);
            _entityManager = _world.EntityManager;

            // Setup required singletons
            _entityManager.CreateEntity(typeof(GameWorldTag));
            _entityManager.SetComponentData(_entityManager.CreateEntity(typeof(TimeState)), new TimeState
            {
                Tick = 1,
                FixedDeltaTime = 0.1f,
                IsPaused = false
            });
            var rewindEntity = _entityManager.CreateEntity(typeof(RewindState));
            _entityManager.SetComponentData(rewindEntity, new RewindState
            {
                Mode = RewindMode.Record,
                TargetTick = 0,
                TickDuration = 0.1f,
                MaxHistoryTicks = 600,
                PendingStepTicks = 0
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

        #region Suite 1: Destroyed Target Entity Clearing

        [Test]
        public void GodgameVillagerIntentBridgeSystem_DestroysTarget_ClearsIntent()
        {
            // Create villager with EntityIntent targeting another entity
            var villager = CreateVillagerWithIntent(IntentMode.Gather, Entity.Null, float3.zero);
            var targetEntity = _entityManager.CreateEntity(typeof(LocalTransform));
            _entityManager.SetComponentData(targetEntity, LocalTransform.Identity);

            // Set intent to target the entity
            var intent = _entityManager.GetComponentData<EntityIntent>(villager);
            intent.TargetEntity = targetEntity;
            intent.Mode = IntentMode.Gather;
            intent.IsValid = 1;
            intent.Priority = InterruptPriority.Normal;
            intent.IntentSetTick = 1;
            _entityManager.SetComponentData(villager, intent);

            // Set AI state based on intent
            var aiState = _entityManager.GetComponentData<VillagerAIState>(villager);
            aiState.CurrentGoal = VillagerAIState.Goal.Work;
            aiState.TargetEntity = targetEntity;
            _entityManager.SetComponentData(villager, aiState);

            // Destroy target entity
            _entityManager.DestroyEntity(targetEntity);
            _world.Update(); // Allow entity destruction to process

            // Run bridge system
            var bridgeSystem = _world.GetOrCreateSystemManaged<GodgameVillagerIntentBridgeSystem>();
            bridgeSystem.Update(_world.Unmanaged);

            // Verify intent is cleared
            var updatedIntent = _entityManager.GetComponentData<EntityIntent>(villager);
            Assert.AreEqual(0, updatedIntent.IsValid, "Intent should be invalid after target destroyed");
            Assert.AreEqual(IntentMode.Idle, updatedIntent.Mode, "Intent mode should be Idle after clearing");
        }

        [Test]
        public void IntentBridge_DestroysTarget_PositionBasedIntent_NotCleared()
        {
            // Create villager with position-based intent (no target entity)
            var villager = CreateVillagerWithIntent(IntentMode.MoveTo, Entity.Null, new float3(10f, 0f, 10f));
            var unrelatedEntity = _entityManager.CreateEntity(typeof(LocalTransform));
            _entityManager.SetComponentData(unrelatedEntity, LocalTransform.Identity);

            // Set intent with position but no entity
            var intent = _entityManager.GetComponentData<EntityIntent>(villager);
            intent.TargetEntity = Entity.Null;
            intent.TargetPosition = new float3(10f, 0f, 10f);
            intent.Mode = IntentMode.MoveTo;
            intent.IsValid = 1;
            intent.Priority = InterruptPriority.Normal;
            intent.IntentSetTick = 1;
            _entityManager.SetComponentData(villager, intent);

            // Destroy unrelated entity
            _entityManager.DestroyEntity(unrelatedEntity);
            _world.Update();

            // Run bridge system
            var bridgeSystem = _world.GetOrCreateSystemManaged<GodgameVillagerIntentBridgeSystem>();
            bridgeSystem.Update(_world.Unmanaged);

            // Verify intent remains valid (position-based intents don't depend on entities)
            var updatedIntent = _entityManager.GetComponentData<EntityIntent>(villager);
            Assert.AreEqual(1, updatedIntent.IsValid, "Position-based intent should remain valid");
            Assert.AreEqual(IntentMode.MoveTo, updatedIntent.Mode, "Intent mode should remain MoveTo");
        }

        [Test]
        public void IntentBridge_DestroysTarget_EntityStorageInfoLookup_Works()
        {
            // Create multiple villagers with intents targeting different entities
            var target1 = _entityManager.CreateEntity(typeof(LocalTransform));
            var target2 = _entityManager.CreateEntity(typeof(LocalTransform));
            var target3 = _entityManager.CreateEntity(typeof(LocalTransform));

            var villager1 = CreateVillagerWithIntent(IntentMode.Gather, target1, float3.zero);
            var villager2 = CreateVillagerWithIntent(IntentMode.Gather, target2, float3.zero);
            var villager3 = CreateVillagerWithIntent(IntentMode.Gather, target3, float3.zero);

            // Destroy subset of targets
            _entityManager.DestroyEntity(target1);
            _entityManager.DestroyEntity(target3);
            _world.Update();

            // Run bridge system
            var bridgeSystem = _world.GetOrCreateSystemManaged<GodgameVillagerIntentBridgeSystem>();
            bridgeSystem.Update(_world.Unmanaged);

            // Verify only intents with destroyed targets are cleared
            var intent1 = _entityManager.GetComponentData<EntityIntent>(villager1);
            var intent2 = _entityManager.GetComponentData<EntityIntent>(villager2);
            var intent3 = _entityManager.GetComponentData<EntityIntent>(villager3);

            Assert.AreEqual(0, intent1.IsValid, "Intent 1 should be cleared (target destroyed)");
            Assert.AreEqual(1, intent2.IsValid, "Intent 2 should remain valid (target exists)");
            Assert.AreEqual(0, intent3.IsValid, "Intent 3 should be cleared (target destroyed)");
        }

        #endregion

        #region Suite 2: Intent Mode Mapping Verification

        [Test]
        public void GodgameVillagerIntentBridgeSystem_IntentModeMappings()
        {
            // Test Attack → Fight
            TestIntentModeMapping(IntentMode.Attack, VillagerAIState.Goal.Fight, VillagerAIState.State.Fighting);

            // Test Flee → Flee
            TestIntentModeMapping(IntentMode.Flee, VillagerAIState.Goal.Flee, VillagerAIState.State.Fleeing);

            // Test Gather → Work
            TestIntentModeMapping(IntentMode.Gather, VillagerAIState.Goal.Work, VillagerAIState.State.Working);

            // Test Build → Work (not None)
            TestIntentModeMapping(IntentMode.Build, VillagerAIState.Goal.Work, VillagerAIState.State.Working);

            // Test UseAbility → Fight (assume combat)
            TestIntentModeMapping(IntentMode.UseAbility, VillagerAIState.Goal.Fight, VillagerAIState.State.Fighting);

            // Test Patrol → Work
            TestIntentModeMapping(IntentMode.Patrol, VillagerAIState.Goal.Work, VillagerAIState.State.Working);

            // Test Follow → Work
            TestIntentModeMapping(IntentMode.Follow, VillagerAIState.Goal.Work, VillagerAIState.State.Working);

            // Test Defend → Fight
            TestIntentModeMapping(IntentMode.Defend, VillagerAIState.Goal.Fight, VillagerAIState.State.Fighting);

            // Test Custom modes → None (intentional, game-specific)
            TestIntentModeMapping(IntentMode.Custom0, VillagerAIState.Goal.None, VillagerAIState.State.Idle);
            TestIntentModeMapping(IntentMode.Custom1, VillagerAIState.Goal.None, VillagerAIState.State.Idle);
        }

        [Test]
        public void IntentBridge_IntentMode_InterruptDriven_Flow()
        {
            // Create villager with interrupt buffer
            var villager = CreateVillagerWithIntent(IntentMode.Idle, Entity.Null, float3.zero);
            var interruptBuffer = _entityManager.GetBuffer<Interrupt>(villager);

            // Emit interrupt
            var targetEntity = _entityManager.CreateEntity(typeof(LocalTransform));
            _entityManager.SetComponentData(targetEntity, LocalTransform.Identity);

            InterruptUtils.Emit(
                ref interruptBuffer,
                InterruptType.NewThreatDetected,
                InterruptPriority.High,
                villager,
                1,
                targetEntity);

            // Run InterruptHandlerSystem
            var interruptSystem = _world.GetOrCreateSystemManaged<PureDOTS.Systems.Interrupts.InterruptHandlerSystem>();
            interruptSystem.Update(_world.Unmanaged);

            // Verify EntityIntent created
            var intent = _entityManager.GetComponentData<EntityIntent>(villager);
            Assert.AreEqual(IntentMode.Attack, intent.Mode, "Interrupt should create Attack intent");
            Assert.AreEqual(1, intent.IsValid, "Intent should be valid");

            // Run bridge system
            var bridgeSystem = _world.GetOrCreateSystemManaged<GodgameVillagerIntentBridgeSystem>();
            bridgeSystem.Update(_world.Unmanaged);

            // Verify goal updated
            var aiState = _entityManager.GetComponentData<VillagerAIState>(villager);
            Assert.AreEqual(VillagerAIState.Goal.Fight, aiState.CurrentGoal, "Goal should be Fight");
            Assert.AreEqual(VillagerAIState.State.Fighting, aiState.CurrentState, "State should be Fighting");
        }

        [Test]
        public void IntentBridge_IntentMode_PriorityOverride()
        {
            // Create villager with EntityIntent (Normal priority, Gather mode)
            var villager = CreateVillagerWithIntent(IntentMode.Gather, Entity.Null, float3.zero);
            var intent = _entityManager.GetComponentData<EntityIntent>(villager);
            intent.Mode = IntentMode.Gather;
            intent.Priority = InterruptPriority.Normal;
            intent.IsValid = 1;
            intent.IntentSetTick = 1;
            _entityManager.SetComponentData(villager, intent);

            var aiState = _entityManager.GetComponentData<VillagerAIState>(villager);
            aiState.CurrentGoal = VillagerAIState.Goal.Work;
            _entityManager.SetComponentData(villager, aiState);

            // Emit higher-priority interrupt
            var interruptBuffer = _entityManager.GetBuffer<Interrupt>(villager);
            var threatEntity = _entityManager.CreateEntity(typeof(LocalTransform));
            _entityManager.SetComponentData(threatEntity, LocalTransform.Identity);

            InterruptUtils.Emit(
                ref interruptBuffer,
                InterruptType.UnderAttack,
                InterruptPriority.High,
                villager,
                2,
                threatEntity);

            // Run EnhancedInterruptHandlerSystem
            var enhancedInterruptSystem = _world.GetOrCreateSystemManaged<PureDOTS.Systems.Intent.EnhancedInterruptHandlerSystem>();
            enhancedInterruptSystem.Update(_world.Unmanaged);

            // Verify intent overridden
            var updatedIntent = _entityManager.GetComponentData<EntityIntent>(villager);
            Assert.AreEqual(IntentMode.Attack, updatedIntent.Mode, "Intent should be overridden to Attack");
            Assert.AreEqual(InterruptPriority.High, updatedIntent.Priority, "Priority should be High");

            // Run bridge system
            var bridgeSystem = _world.GetOrCreateSystemManaged<GodgameVillagerIntentBridgeSystem>();
            bridgeSystem.Update(_world.Unmanaged);

            // Verify goal updated
            var updatedAiState = _entityManager.GetComponentData<VillagerAIState>(villager);
            Assert.AreEqual(VillagerAIState.Goal.Fight, updatedAiState.CurrentGoal, "Goal should be updated to Fight");
        }

        #endregion

        #region Suite 4: Integration & Edge Cases

        [Test]
        public void IntentBridge_GoalCompletion_Clearing()
        {
            // Create villager with EntityIntent and VillagerAIState.Goal = Work
            var villager = CreateVillagerWithIntent(IntentMode.Gather, Entity.Null, float3.zero);
            var intent = _entityManager.GetComponentData<EntityIntent>(villager);
            intent.Mode = IntentMode.Gather;
            intent.IsValid = 1;
            intent.IntentSetTick = 1;
            _entityManager.SetComponentData(villager, intent);

            var aiState = _entityManager.GetComponentData<VillagerAIState>(villager);
            aiState.CurrentGoal = VillagerAIState.Goal.Work;
            aiState.CurrentState = VillagerAIState.State.Working;
            _entityManager.SetComponentData(villager, aiState);

            // Simulate goal completion (set to None and Idle)
            aiState.CurrentGoal = VillagerAIState.Goal.None;
            aiState.CurrentState = VillagerAIState.State.Idle;
            _entityManager.SetComponentData(villager, aiState);

            // Run bridge system
            var bridgeSystem = _world.GetOrCreateSystemManaged<GodgameVillagerIntentBridgeSystem>();
            bridgeSystem.Update(_world.Unmanaged);

            // Verify EntityIntent is cleared
            var updatedIntent = _entityManager.GetComponentData<EntityIntent>(villager);
            Assert.AreEqual(0, updatedIntent.IsValid, "Intent should be cleared when goal completed");
        }

        #endregion

        #region Helper Methods

        private Entity CreateVillagerWithIntent(IntentMode mode, Entity targetEntity, float3 targetPosition)
        {
            var entity = _entityManager.CreateEntity(
                typeof(VillagerAIState),
                typeof(EntityIntent),
                typeof(LocalTransform));

            _entityManager.SetComponentData(entity, LocalTransform.Identity);

            _entityManager.SetComponentData(entity, new VillagerAIState
            {
                CurrentState = VillagerAIState.State.Idle,
                CurrentGoal = VillagerAIState.Goal.None,
                TargetEntity = Entity.Null,
                TargetPosition = float3.zero,
                StateTimer = 0f,
                StateStartTick = 0
            });

            _entityManager.SetComponentData(entity, new EntityIntent
            {
                Mode = mode,
                TargetEntity = targetEntity,
                TargetPosition = targetPosition,
                TriggeringInterrupt = InterruptType.None,
                IntentSetTick = 1,
                Priority = InterruptPriority.Normal,
                IsValid = 1
            });

            // Add interrupt buffer (required for interrupt-driven flow)
            _entityManager.AddBuffer<Interrupt>(entity);

            return entity;
        }

        private void TestIntentModeMapping(IntentMode intentMode, VillagerAIState.Goal expectedGoal, VillagerAIState.State expectedState)
        {
            var villager = CreateVillagerWithIntent(intentMode, Entity.Null, float3.zero);
            var intent = _entityManager.GetComponentData<EntityIntent>(villager);
            intent.Mode = intentMode;
            intent.IsValid = 1;
            intent.IntentSetTick = 1;
            _entityManager.SetComponentData(villager, intent);

            // Run bridge system
            var bridgeSystem = _world.GetOrCreateSystemManaged<GodgameVillagerIntentBridgeSystem>();
            bridgeSystem.Update(_world.Unmanaged);

            // Verify mapping
            var aiState = _entityManager.GetComponentData<VillagerAIState>(villager);
            Assert.AreEqual(expectedGoal, aiState.CurrentGoal, 
                $"IntentMode.{intentMode} should map to Goal.{expectedGoal}");
            Assert.AreEqual(expectedState, aiState.CurrentState,
                $"Goal.{expectedGoal} should map to State.{expectedState}");
        }

        #endregion
    }
}
#endif

