using Godgame.Modules;
using NUnit.Framework;
using PureDOTS.Runtime.Components;
using PureDOTS.Runtime.Skills;
using PureDOTS.Runtime.Telemetry;
using PureDOTS.Systems;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace Godgame.Tests.Modules
{
    /// <summary>
    /// Validates module degradation, refit progression, and telemetry emission.
    /// </summary>
    public class ModuleMaintenanceTests
    {
        private World _world;
        private EntityManager _entityManager;
        private SimulationSystemGroup _simGroup;

        [SetUp]
        public void SetUp()
        {
            _world = new World(nameof(ModuleMaintenanceTests));
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
        public void DegradationQueuesRefitWhenBelowThreshold()
        {
            var configEntity = _entityManager.CreateEntity(typeof(ModuleMaintenanceConfig));
            _entityManager.SetComponentData(configEntity, new ModuleMaintenanceConfig
            {
                BaseWorkRate = 4f,
                SkillRateBonus = 0.2f,
                WorkRequiredPerCondition = 0.05f,
                AutoRepairThreshold = 0.8f,
                CriticalThreshold = 0.2f
            });

            var module = _entityManager.CreateEntity(typeof(ModuleData));
            _entityManager.SetComponentData(module, new ModuleData
            {
                ModuleId = new FixedString64Bytes("engine.core"),
                SlotType = new FixedString64Bytes("engine"),
                Status = ModuleStatus.Operational,
                MaxCondition = 100f,
                Condition = 50f,
                DegradationPerSecond = 25f,
                LastServiceTick = 0
            });

            UpdateSystem(_world.GetOrCreateSystem<ModuleDegradationSystem>());

            Assert.IsTrue(_entityManager.HasComponent<ModuleRefitRequest>(module));
            var refit = _entityManager.GetComponentData<ModuleRefitRequest>(module);
            Assert.Greater(refit.WorkRemaining, 0f);
            var moduleData = _entityManager.GetComponentData<ModuleData>(module);
            Assert.AreEqual(ModuleStatus.Refit, moduleData.Status);
        }

        [Test]
        public void DamageRequestQueuesRefitAndMarksOffline()
        {
            var configEntity = _entityManager.CreateEntity(typeof(ModuleMaintenanceConfig));
            _entityManager.SetComponentData(configEntity, new ModuleMaintenanceConfig
            {
                BaseWorkRate = 4f,
                SkillRateBonus = 0.25f,
                WorkRequiredPerCondition = 0.1f,
                AutoRepairThreshold = 0.8f,
                CriticalThreshold = 0.2f
            });

            var host = _entityManager.CreateEntity();
            var slots = _entityManager.AddBuffer<ModuleSlot>(host);
            var module = _entityManager.CreateEntity(typeof(ModuleData));
            slots.Add(new ModuleSlot
            {
                SlotIndex = 0,
                SlotType = new FixedString64Bytes("engine"),
                InstalledModule = module
            });

            _entityManager.SetComponentData(module, new ModuleData
            {
                ModuleId = new FixedString64Bytes("engine.core"),
                SlotType = new FixedString64Bytes("engine"),
                Status = ModuleStatus.Operational,
                MaxCondition = 100f,
                Condition = 30f,
                DegradationPerSecond = 0f,
                LastServiceTick = 0
            });

            var damageBuffer = _entityManager.AddBuffer<ModuleDamageRequest>(host);
            damageBuffer.Add(new ModuleDamageRequest
            {
                SlotIndex = 0,
                Damage = 20f
            });

            UpdateSystem(_world.GetOrCreateSystem<ModuleDamageSystem>());

            var updatedModule = _entityManager.GetComponentData<ModuleData>(module);
            Assert.AreEqual(10f, updatedModule.Condition, 0.0001f);
            Assert.AreEqual(ModuleStatus.Offline, updatedModule.Status);
            Assert.IsTrue(_entityManager.HasComponent<ModuleRefitRequest>(module));
            Assert.Greater(_entityManager.GetComponentData<ModuleRefitRequest>(module).WorkRemaining, 0f);
        }

        [Test]
        public void MaintainerSkillAcceleratesRefitWork()
        {
            var configEntity = _entityManager.CreateEntity(typeof(ModuleMaintenanceConfig));
            _entityManager.SetComponentData(configEntity, new ModuleMaintenanceConfig
            {
                BaseWorkRate = 2f,
                SkillRateBonus = 0.5f,
                WorkRequiredPerCondition = 1f,
                AutoRepairThreshold = 0.6f,
                CriticalThreshold = 0.2f
            });

            var worker = _entityManager.CreateEntity(typeof(SkillSet));
            var skillSet = new SkillSet();
            skillSet.SetLevel(SkillId.Processing, 3);
            _entityManager.SetComponentData(worker, skillSet);

            var skilled = _entityManager.CreateEntity(typeof(ModuleData), typeof(ModuleRefitRequest), typeof(ModuleMaintainerAssignment));
            _entityManager.SetComponentData(skilled, new ModuleData
            {
                ModuleId = new FixedString64Bytes("mainhand.alpha"),
                SlotType = ModuleSlotIds.MainHand,
                Status = ModuleStatus.Refit,
                MaxCondition = 100f,
                Condition = 10f,
                DegradationPerSecond = 0f,
                LastServiceTick = 0
            });
            _entityManager.SetComponentData(skilled, new ModuleRefitRequest
            {
                WorkRemaining = 1f,
                TargetCondition = 100f,
                SkillId = SkillId.Processing,
                AutoRequested = 0
            });
            _entityManager.SetComponentData(skilled, new ModuleMaintainerAssignment
            {
                WorkerEntity = worker
            });

            var unskilled = _entityManager.CreateEntity(typeof(ModuleData), typeof(ModuleRefitRequest));
            _entityManager.SetComponentData(unskilled, new ModuleData
            {
                ModuleId = new FixedString64Bytes("mainhand.beta"),
                SlotType = ModuleSlotIds.MainHand,
                Status = ModuleStatus.Refit,
                MaxCondition = 100f,
                Condition = 10f,
                DegradationPerSecond = 0f,
                LastServiceTick = 0
            });
            _entityManager.SetComponentData(unskilled, new ModuleRefitRequest
            {
                WorkRemaining = 1f,
                TargetCondition = 100f,
                SkillId = SkillId.Processing,
                AutoRequested = 0
            });

            UpdateSystem(_world.GetOrCreateSystem<ModuleRefitSystem>());

            var skilledRemaining = _entityManager.GetComponentData<ModuleRefitRequest>(skilled).WorkRemaining;
            var unskilledRemaining = _entityManager.GetComponentData<ModuleRefitRequest>(unskilled).WorkRemaining;
            Assert.Less(skilledRemaining, unskilledRemaining);
        }

        [Test]
        public void HostSkillsAppliedWhenNoDirectMaintainerAssignment()
        {
            var configEntity = _entityManager.CreateEntity(typeof(ModuleMaintenanceConfig));
            _entityManager.SetComponentData(configEntity, new ModuleMaintenanceConfig
            {
                BaseWorkRate = 2f,
                SkillRateBonus = 0.25f,
                WorkRequiredPerCondition = 1f,
                AutoRepairThreshold = 0.6f,
                CriticalThreshold = 0.2f,
                ResourceCostPerWork = 0f
            });

            var timeEntity = GetSingletonEntity<TimeState>();
            _entityManager.SetComponentData(timeEntity, new TimeState
            {
                Tick = 1,
                FixedDeltaTime = 1f,
                CurrentSpeedMultiplier = 1f,
                IsPaused = false
            });
            var rewindEntity = GetSingletonEntity<RewindState>();
            _entityManager.SetComponentData(rewindEntity, new RewindState
            {
                Mode = RewindMode.Record,
                TargetTick = 1,
                TickDuration = 1f,
                MaxHistoryTicks = 600,
                PendingStepTicks = 0
            });
            var legacy = new RewindLegacyState
            {
                PlaybackSpeed = 1f,
                CurrentTick = 1,
                StartTick = 0,
                PlaybackTick = 1,
                PlaybackTicksPerSecond = 1f,
                ScrubDirection = 0,
                ScrubSpeedMultiplier = 1f,
                RewindWindowTicks = 0,
                ActiveTrack = default
            };
            if (_entityManager.HasComponent<RewindLegacyState>(rewindEntity))
            {
                _entityManager.SetComponentData(rewindEntity, legacy);
            }
            else
            {
                _entityManager.AddComponentData(rewindEntity, legacy);
            }

            // Worker with processing skill.
            var worker = _entityManager.CreateEntity(typeof(SkillSet));
            var workerSkills = new SkillSet();
            workerSkills.SetLevel(SkillId.Processing, 4);
            _entityManager.SetComponentData(worker, workerSkills);

            // Host with maintainer links and one installed module.
            var skilledHost = _entityManager.CreateEntity();
            var skilledSlots = _entityManager.AddBuffer<ModuleSlot>(skilledHost);
            var skilledModule = _entityManager.CreateEntity(typeof(ModuleData), typeof(ModuleRefitRequest));
            skilledSlots.Add(new ModuleSlot
            {
                SlotIndex = 0,
                SlotType = new FixedString64Bytes("engine"),
                InstalledModule = skilledModule
            });
            var skilledMaintainerLinks = _entityManager.AddBuffer<ModuleMaintainerLink>(skilledHost);
            skilledMaintainerLinks.Add(new ModuleMaintainerLink { Worker = worker });

            _entityManager.SetComponentData(skilledModule, new ModuleData
            {
                ModuleId = new FixedString64Bytes("mainhand.hosted"),
                SlotType = ModuleSlotIds.MainHand,
                Status = ModuleStatus.Refit,
                MaxCondition = 100f,
                Condition = 10f,
                DegradationPerSecond = 0f,
                LastServiceTick = 0
            });
            _entityManager.SetComponentData(skilledModule, new ModuleRefitRequest
            {
                WorkRemaining = 2f,
                TargetCondition = 100f,
                SkillId = SkillId.Processing,
                AutoRequested = 0
            });

            // Unskilled host with a module but no maintainer links.
            var unskilledHost = _entityManager.CreateEntity();
            var unskilledSlots = _entityManager.AddBuffer<ModuleSlot>(unskilledHost);
            var unskilledModule = _entityManager.CreateEntity(typeof(ModuleData), typeof(ModuleRefitRequest));
            unskilledSlots.Add(new ModuleSlot
            {
                SlotIndex = 0,
                SlotType = new FixedString64Bytes("engine"),
                InstalledModule = unskilledModule
            });

            _entityManager.SetComponentData(unskilledModule, new ModuleData
            {
                ModuleId = new FixedString64Bytes("mainhand.unhosted"),
                SlotType = ModuleSlotIds.MainHand,
                Status = ModuleStatus.Refit,
                MaxCondition = 100f,
                Condition = 10f,
                DegradationPerSecond = 0f,
                LastServiceTick = 0
            });
            _entityManager.SetComponentData(unskilledModule, new ModuleRefitRequest
            {
                WorkRemaining = 2f,
                TargetCondition = 100f,
                SkillId = SkillId.Processing,
                AutoRequested = 0
            });

            UpdateSystems(
                _world.GetOrCreateSystem<ModuleMaintainerAggregationSystem>(),
                _world.GetOrCreateSystem<ModuleRefitSystem>());

            var skilledRemaining = _entityManager.GetComponentData<ModuleRefitRequest>(skilledModule).WorkRemaining;
            var unskilledRemaining = _entityManager.GetComponentData<ModuleRefitRequest>(unskilledModule).WorkRemaining;

            Assert.Less(skilledRemaining, unskilledRemaining);
            Assert.IsTrue(_entityManager.HasComponent<ModuleHostReference>(skilledModule));
            var hostRef = _entityManager.GetComponentData<ModuleHostReference>(skilledModule);
            Assert.AreEqual(skilledHost, hostRef.Host);
        }

        [Test]
        public void TelemetryRecordsModuleCounters()
        {
            _entityManager.SetComponentData(_entityManager.CreateEntity(typeof(ModuleMaintenanceConfig)), ModuleMaintenanceDefaults.Create());

            var healthy = _entityManager.CreateEntity(typeof(ModuleData));
            _entityManager.SetComponentData(healthy, new ModuleData
            {
                ModuleId = new FixedString64Bytes("engine.healthy"),
                SlotType = new FixedString64Bytes("engine"),
                Status = ModuleStatus.Operational,
                MaxCondition = 100f,
                Condition = 100f,
                DegradationPerSecond = 0f,
                LastServiceTick = 0
            });

            var damaged = _entityManager.CreateEntity(typeof(ModuleData));
            _entityManager.SetComponentData(damaged, new ModuleData
            {
                ModuleId = new FixedString64Bytes("engine.damaged"),
                SlotType = new FixedString64Bytes("engine"),
                Status = ModuleStatus.Operational,
                MaxCondition = 100f,
                Condition = 10f,
                DegradationPerSecond = 0f,
                LastServiceTick = 0
            });

            UpdateSystem(_world.GetOrCreateSystem<ModuleDegradationSystem>());

            var telemetryEntity = GetSingletonEntity<TelemetryStream>();
            var telemetryMetrics = _entityManager.GetBuffer<TelemetryMetric>(telemetryEntity);

            Assert.IsTrue(ContainsTelemetryMetric(telemetryMetrics, ModuleTelemetryKeys.TotalModules, 2));
            Assert.IsTrue(ContainsTelemetryMetric(telemetryMetrics, ModuleTelemetryKeys.DamagedModules, 1));
            Assert.IsTrue(ContainsTelemetryMetric(telemetryMetrics, ModuleTelemetryKeys.OfflineModules, 1));
            Assert.IsTrue(ContainsTelemetryMetric(telemetryMetrics, ModuleTelemetryKeys.RefitsQueued, 1));
        }

        [Test]
        public void RefitProgressRequiresResourcesWhenConfigured()
        {
            var configEntity = _entityManager.CreateEntity(typeof(ModuleMaintenanceConfig));
            _entityManager.SetComponentData(configEntity, new ModuleMaintenanceConfig
            {
                BaseWorkRate = 2f,
                SkillRateBonus = 0f,
                WorkRequiredPerCondition = 1f,
                AutoRepairThreshold = 0.5f,
                CriticalThreshold = 0.1f,
                ResourceCostPerWork = 1f
            });

            var host = _entityManager.CreateEntity(typeof(ModuleResourceWallet));
            _entityManager.SetComponentData(host, new ModuleResourceWallet { Resources = 0.5f });

            var slots = _entityManager.AddBuffer<ModuleSlot>(host);
            var module = _entityManager.CreateEntity(typeof(ModuleData), typeof(ModuleRefitRequest));
            slots.Add(new ModuleSlot
            {
                SlotIndex = 0,
                SlotType = new FixedString64Bytes("weapon"),
                InstalledModule = module
            });
            _entityManager.SetComponentData(module, new ModuleHostReference { Host = host });

            _entityManager.SetComponentData(module, new ModuleData
            {
                ModuleId = new FixedString64Bytes("weapon.cost"),
                SlotType = new FixedString64Bytes("weapon"),
                Status = ModuleStatus.Refit,
                MaxCondition = 100f,
                Condition = 10f,
                DegradationPerSecond = 0f,
                LastServiceTick = 0
            });

            _entityManager.SetComponentData(module, new ModuleRefitRequest
            {
                WorkRemaining = 2f,
                TargetCondition = 100f,
                SkillId = SkillId.Processing,
                AutoRequested = 0
            });

            UpdateSystem(_world.GetOrCreateSystem<ModuleRefitSystem>());

            // Not enough resources, no progress.
            Assert.AreEqual(2f, _entityManager.GetComponentData<ModuleRefitRequest>(module).WorkRemaining, 0.0001f);
            Assert.AreEqual(0.5f, _entityManager.GetComponentData<ModuleResourceWallet>(host).Resources, 0.0001f);

            // Add resources and progress should occur.
            _entityManager.SetComponentData(host, new ModuleResourceWallet { Resources = 5f });
            UpdateSystem(_world.GetOrCreateSystem<ModuleRefitSystem>());
            Assert.Less(_entityManager.GetComponentData<ModuleRefitRequest>(module).WorkRemaining, 2f);
            Assert.Less(_entityManager.GetComponentData<ModuleResourceWallet>(host).Resources, 5f);
        }

        private Entity GetSingletonEntity<T>() where T : unmanaged, IComponentData
        {
            using var query = _entityManager.CreateEntityQuery(ComponentType.ReadOnly<T>());
            return query.GetSingletonEntity();
        }

        private void UpdateSystem(SystemHandle handle)
        {
            _simGroup.RemoveSystemFromUpdateList(handle);
            _simGroup.AddSystemToUpdateList(handle);
            _simGroup.SortSystems();
            _simGroup.Update();
        }

        private void UpdateSystems(params SystemHandle[] handles)
        {
            foreach (var handle in handles)
            {
                _simGroup.RemoveSystemFromUpdateList(handle);
                _simGroup.AddSystemToUpdateList(handle);
            }

            _simGroup.SortSystems();
            _simGroup.Update();
        }

        private static bool ContainsTelemetryMetric(DynamicBuffer<TelemetryMetric> buffer, in FixedString64Bytes key, float expected)
        {
            for (var i = 0; i < buffer.Length; i++)
            {
                if (buffer[i].Key.Equals(key))
                {
                    return math.abs(buffer[i].Value - expected) < 0.0001f;
                }
            }

            return false;
        }
    }
}
