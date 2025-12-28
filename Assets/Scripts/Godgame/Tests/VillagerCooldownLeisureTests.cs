using Godgame.Villagers;
using NUnit.Framework;
using PureDOTS.Runtime.AI;
using PureDOTS.Runtime.Components;
using PureDOTS.Runtime.Core;
using PureDOTS.Systems;
using System.Text;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Godgame.Tests.Villagers
{
    /// <summary>
    /// Verifies cooldown-driven leisure and guardrails on cooldown scaling.
    /// </summary>
    public class VillagerCooldownLeisureTests
    {
        private World _world;
        private EntityManager _entityManager;
        private FixedStepSimulationSystemGroup _fixedStepGroup;

        [SetUp]
        public void SetUp()
        {
            _world = new World(nameof(VillagerCooldownLeisureTests));
            _entityManager = _world.EntityManager;
            CoreSingletonBootstrapSystem.EnsureSingletons(_entityManager);
            _world.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
            _fixedStepGroup = _world.GetOrCreateSystemManaged<FixedStepSimulationSystemGroup>();

            var cooldownEntity = _entityManager.CreateEntity(typeof(VillagerCooldownProfile));
            _entityManager.SetComponentData(cooldownEntity, VillagerCooldownProfile.Default);
            _entityManager.AddBuffer<VillagerCooldownOutlookRule>(cooldownEntity);
            _entityManager.AddBuffer<VillagerCooldownArchetypeModifier>(cooldownEntity);

            if (_entityManager.CreateEntityQuery(typeof(VillagerScheduleConfig)).IsEmpty)
            {
                var scheduleEntity = _entityManager.CreateEntity(typeof(VillagerScheduleConfig));
                _entityManager.SetComponentData(scheduleEntity, VillagerScheduleConfig.Default);
            }

            _entityManager.CreateEntity(typeof(GameWorldTag));
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
        public void BalancedCooldown_AlternatesWorkAndLeisure()
        {
            var villager = _entityManager.CreateEntity(
                typeof(VillagerGoalState),
                typeof(VillagerWorkCooldown),
                typeof(VillagerNeedMovementState),
                typeof(VillagerNeedState),
                typeof(FocusBudget),
                typeof(VillagerNeedBias),
                typeof(VillagerLeisureState),
                typeof(VillagerSocialFocus),
                typeof(VillagerCooldownPressureState),
                typeof(LocalTransform),
                typeof(Navigation));

            _entityManager.SetComponentData(villager, new VillagerGoalState
            {
                CurrentGoal = VillagerGoal.Work,
                PreviousGoal = VillagerGoal.Idle,
                LastGoalChangeTick = 0,
                CurrentGoalUrgency = 0f
            });
            _entityManager.SetComponentData(villager, new VillagerWorkCooldown
            {
                StartTick = 10,
                EndTick = 20,
                Mode = VillagerWorkCooldownMode.Socialize
            });
            _entityManager.SetComponentData(villager, new VillagerNeedState
            {
                WorkUrgency = 1f,
                HungerUrgency = 0f,
                RestUrgency = 0f,
                FaithUrgency = 0f,
                SafetyUrgency = 0f,
                SocialUrgency = 0f
            });
            _entityManager.SetComponentData(villager, new FocusBudget
            {
                Current = 1f,
                Max = 1f,
                Reserved = 0f,
                RegenPerTick = 0f,
                IsLocked = 0
            });
            _entityManager.SetComponentData(villager, new VillagerNeedBias
            {
                WorkWeight = 1f,
                RestWeight = 0.6f,
                SocialWeight = 0.6f,
                FaithWeight = 0.4f,
                HungerWeight = 1f,
                SafetyWeight = 1f
            });
            _entityManager.SetComponentData(villager, new VillagerLeisureState
            {
                CooldownStartTick = 0,
                CadenceTicks = 0,
                EpisodeIndex = 0,
                RerollCount = 0,
                Action = VillagerLeisureAction.None,
                ActionTarget = Entity.Null
            });
            _entityManager.SetComponentData(villager, new VillagerSocialFocus
            {
                Target = Entity.Null,
                NextPickTick = 0
            });
            _entityManager.SetComponentData(villager, new VillagerCooldownPressureState
            {
                ActiveMask = VillagerCooldownPressureMask.None,
                LastClearReason = VillagerCooldownClearReason.None,
                LastClearTick = 0
            });
            _entityManager.SetComponentData(villager, new LocalTransform
            {
                Position = float3.zero,
                Rotation = quaternion.identity,
                Scale = 1f
            });
            _entityManager.SetComponentData(villager, new Navigation
            {
                Destination = new float3(100f, 0f, 100f),
                Speed = 0f,
                Velocity = float3.zero
            });

            SetTick(10);
            UpdateFixed(_world.GetOrCreateSystem<VillagerLeisureSystem>());

            var duringCooldown = _entityManager.GetComponentData<VillagerGoalState>(villager);
            Assert.AreNotEqual(VillagerGoal.Work, duringCooldown.CurrentGoal, "Cooldown should force leisure.");
            Assert.IsTrue(duringCooldown.CurrentGoal == VillagerGoal.Idle || duringCooldown.CurrentGoal == VillagerGoal.Socialize,
                "Cooldown should pick a leisure goal.");

            SetTick(21);
            UpdateFixed(_world.GetOrCreateSystem<VillagerWorkCooldownSystem>());
            _world.GetOrCreateSystem<VillagerGoalSelectionSystem>().Update(_world.Unmanaged);

            var afterCooldown = _entityManager.GetComponentData<VillagerGoalState>(villager);
            Assert.AreEqual(VillagerGoal.Work, afterCooldown.CurrentGoal, "After cooldown ends, work goal should return.");
        }

        [Test]
        public void WorkaholicProfile_DoesNotZeroCooldownUnlessConfigured()
        {
            var workaholicProfile = new VillagerCooldownProfile
            {
                MinCooldownTicks = 5,
                MaxCooldownTicks = 20,
                WorkBiasToCooldownCurve = new float2(1f, 0f)
            };

            var ticks = VillagerCooldownProfile.ResolveCooldownTicks(workaholicProfile, 1f, 0f);
            Assert.AreEqual(5u, ticks, "Workaholic scaling should clamp to min cooldown.");

            var zeroProfile = new VillagerCooldownProfile
            {
                MinCooldownTicks = 0,
                MaxCooldownTicks = 0,
                WorkBiasToCooldownCurve = new float2(1f, 0f)
            };

            var zeroTicks = VillagerCooldownProfile.ResolveCooldownTicks(zeroProfile, 1f, 0f);
            Assert.AreEqual(0u, zeroTicks, "Explicit zero config should allow zero cooldown.");
        }

        [Test]
        public void DeterministicLeisureSequence_RemainsStable()
        {
            var first = RunLeisureSequence();
            var second = RunLeisureSequence();
            Assert.AreEqual(first, second, "Leisure sequence should be deterministic for a fixed seed.");
        }

        private void UpdateFixed(SystemHandle handle)
        {
            _fixedStepGroup.RemoveSystemFromUpdateList(handle);
            _fixedStepGroup.AddSystemToUpdateList(handle);
            _fixedStepGroup.SortSystems();
            _fixedStepGroup.Update();
        }

        private static void UpdateFixed(FixedStepSimulationSystemGroup group, SystemHandle handle)
        {
            group.RemoveSystemFromUpdateList(handle);
            group.AddSystemToUpdateList(handle);
            group.SortSystems();
            group.Update();
        }

        private void SetTick(uint tick)
        {
            var timeEntity = _entityManager.CreateEntityQuery(typeof(TimeState)).GetSingletonEntity();
            var timeState = _entityManager.GetComponentData<TimeState>(timeEntity);
            timeState.Tick = tick;
            timeState.WorldSeconds = timeState.FixedDeltaTime * tick;
            _entityManager.SetComponentData(timeEntity, timeState);

            if (_entityManager.HasComponent<TickTimeState>(timeEntity))
            {
                var tickState = _entityManager.GetComponentData<TickTimeState>(timeEntity);
                tickState.Tick = tick;
                tickState.TargetTick = tick;
                _entityManager.SetComponentData(timeEntity, tickState);
            }
        }

        private static void SetTick(EntityManager entityManager, uint tick)
        {
            var timeEntity = entityManager.CreateEntityQuery(typeof(TimeState)).GetSingletonEntity();
            var timeState = entityManager.GetComponentData<TimeState>(timeEntity);
            timeState.Tick = tick;
            timeState.WorldSeconds = timeState.FixedDeltaTime * tick;
            entityManager.SetComponentData(timeEntity, timeState);

            if (entityManager.HasComponent<TickTimeState>(timeEntity))
            {
                var tickState = entityManager.GetComponentData<TickTimeState>(timeEntity);
                tickState.Tick = tick;
                tickState.TargetTick = tick;
                entityManager.SetComponentData(timeEntity, tickState);
            }
        }

        private static string RunLeisureSequence()
        {
            using var world = new World("LeisureSequenceWorld");
            var entityManager = world.EntityManager;
            CoreSingletonBootstrapSystem.EnsureSingletons(entityManager);
            world.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
            var fixedStepGroup = world.GetOrCreateSystemManaged<FixedStepSimulationSystemGroup>();

            var cooldownEntity = entityManager.CreateEntity(typeof(VillagerCooldownProfile));
            var profile = VillagerCooldownProfile.Default;
            profile.LeisureCadenceMinTicks = 6;
            profile.LeisureCadenceMaxTicks = 6;
            profile.LeisureCrowdingPressureThreshold = 2f;
            entityManager.SetComponentData(cooldownEntity, profile);
            entityManager.AddBuffer<VillagerCooldownOutlookRule>(cooldownEntity);
            entityManager.AddBuffer<VillagerCooldownArchetypeModifier>(cooldownEntity);

            var scheduleEntity = entityManager.CreateEntity(typeof(VillagerScheduleConfig));
            var schedule = VillagerScheduleConfig.Default;
            schedule.NeedWanderRadius = 4f;
            schedule.NeedSocialRadius = 6f;
            entityManager.SetComponentData(scheduleEntity, schedule);

            entityManager.CreateEntity(typeof(GameWorldTag));

            var villager = entityManager.CreateEntity(
                typeof(VillagerGoalState),
                typeof(VillagerWorkCooldown),
                typeof(VillagerNeedMovementState),
                typeof(VillagerNeedState),
                typeof(FocusBudget),
                typeof(VillagerNeedBias),
                typeof(VillagerLeisureState),
                typeof(VillagerSocialFocus),
                typeof(VillagerCooldownPressureState),
                typeof(LocalTransform),
                typeof(Navigation));

            entityManager.SetComponentData(villager, new VillagerGoalState
            {
                CurrentGoal = VillagerGoal.Work,
                PreviousGoal = VillagerGoal.Idle,
                LastGoalChangeTick = 0,
                CurrentGoalUrgency = 0f
            });
            entityManager.SetComponentData(villager, new VillagerWorkCooldown
            {
                StartTick = 10,
                EndTick = 30,
                Mode = VillagerWorkCooldownMode.Wander
            });
            entityManager.SetComponentData(villager, new VillagerNeedState
            {
                WorkUrgency = 1f,
                HungerUrgency = 0f,
                RestUrgency = 0f,
                FaithUrgency = 0f,
                SafetyUrgency = 0f,
                SocialUrgency = 0f
            });
            entityManager.SetComponentData(villager, new FocusBudget
            {
                Current = 1f,
                Max = 1f,
                Reserved = 0f,
                RegenPerTick = 0f,
                IsLocked = 0
            });
            entityManager.SetComponentData(villager, new VillagerNeedBias
            {
                WorkWeight = 1f,
                RestWeight = 0.6f,
                SocialWeight = 0.6f,
                FaithWeight = 0.4f,
                HungerWeight = 1f,
                SafetyWeight = 1f
            });
            entityManager.SetComponentData(villager, new VillagerLeisureState
            {
                CooldownStartTick = 0,
                CadenceTicks = 0,
                EpisodeIndex = 0,
                RerollCount = 0,
                Action = VillagerLeisureAction.None,
                ActionTarget = Entity.Null
            });
            entityManager.SetComponentData(villager, new VillagerSocialFocus
            {
                Target = Entity.Null,
                NextPickTick = 0
            });
            entityManager.SetComponentData(villager, new VillagerCooldownPressureState
            {
                ActiveMask = VillagerCooldownPressureMask.None,
                LastClearReason = VillagerCooldownClearReason.None,
                LastClearTick = 0
            });
            entityManager.SetComponentData(villager, new LocalTransform
            {
                Position = float3.zero,
                Rotation = quaternion.identity,
                Scale = 1f
            });
            entityManager.SetComponentData(villager, new Navigation
            {
                Destination = new float3(100f, 0f, 100f),
                Speed = 0f,
                Velocity = float3.zero
            });

            var handle = world.GetOrCreateSystem<VillagerLeisureSystem>();
            var builder = new StringBuilder();
            var lastEpisode = uint.MaxValue;

            for (uint tick = 10; tick <= 30; tick++)
            {
                SetTick(entityManager, tick);
                UpdateFixed(fixedStepGroup, handle);

                var leisure = entityManager.GetComponentData<VillagerLeisureState>(villager);
                if (leisure.EpisodeIndex == lastEpisode)
                {
                    continue;
                }

                lastEpisode = leisure.EpisodeIndex;
                var goal = entityManager.GetComponentData<VillagerGoalState>(villager);
                var movement = entityManager.GetComponentData<VillagerNeedMovementState>(villager);
                var offsetX = (int)math.round(movement.AnchorOffset.x * 100f);
                var offsetZ = (int)math.round(movement.AnchorOffset.z * 100f);
                builder.Append($"{tick}:{(byte)goal.CurrentGoal}:{leisure.EpisodeIndex}:{offsetX},{offsetZ};");
            }

            return builder.ToString();
        }
    }
}
