using Godgame.Resources;
using Godgame.Villages;
using Godgame.Villagers;
using PureDOTS.Runtime.AI;
using PureDOTS.Runtime.Core;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using DotsComponents = PureDOTS.Runtime.Components;
using System;
using SystemEnv = System.Environment;

namespace Godgame.Scenario
{
    /// <summary>
    /// Spawns a minimal set of gameplay entities when running headless without scene content.
    /// Helps smoke tests run even if the scene is empty.
    /// </summary>
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    public partial class SmokeTestFallbackBootstrapSystem : SystemBase
    {
        private const string ScenarioEnvVar = "GODGAME_SCENARIO_PATH";
        private const string ScenarioArg = "--scenario";

        private bool _hasRun;
        private EntityQuery _villageQuery;
        private EntityQuery _villagerQuery;
        private EntityQuery _resourceQuery;
        private EntityQuery _settlementQuery;
        private EntityQuery _settlementVillagerQuery;
        private EntityQuery _storehouseQuery;
        private EntityQuery _mindVillagerQuery;

        protected override void OnCreate()
        {
            _villageQuery = GetEntityQuery(ComponentType.ReadOnly<Village>());
            _villagerQuery = GetEntityQuery(ComponentType.ReadOnly<VillagerNeeds>());
            _resourceQuery = GetEntityQuery(ComponentType.ReadOnly<ResourceSourceConfig>());
            _settlementQuery = GetEntityQuery(ComponentType.ReadOnly<SettlementConfig>());
            _settlementVillagerQuery = GetEntityQuery(ComponentType.ReadOnly<SettlementVillagerState>());
            _storehouseQuery = GetEntityQuery(ComponentType.ReadOnly<DotsComponents.StorehouseInventory>());
            _mindVillagerQuery = GetEntityQuery(ComponentType.ReadOnly<VillagerNeedState>());
        }

        protected override void OnUpdate()
        {
            if (_hasRun)
            {
                Enabled = false;
                return;
            }

            if (!Application.isBatchMode)
            {
                Enabled = false;
                return;
            }

            // If an explicit scenario was requested, don't spawn fallbacks while SubScenes stream in.
            // Headless scenarios rely on their own spawn systems and should fail loudly if wiring is broken.
            if (IsScenarioRequested())
            {
                Enabled = false;
                return;
            }

            if (!_villageQuery.IsEmptyIgnoreFilter ||
                !_villagerQuery.IsEmptyIgnoreFilter ||
                !_resourceQuery.IsEmptyIgnoreFilter ||
                !_settlementQuery.IsEmptyIgnoreFilter ||
                !_settlementVillagerQuery.IsEmptyIgnoreFilter ||
                !_storehouseQuery.IsEmptyIgnoreFilter ||
                !_mindVillagerQuery.IsEmptyIgnoreFilter)
            {
                Enabled = false;
                return;
            }

            var ecb = new EntityCommandBuffer(Allocator.Temp);

            SpawnVillage(ref ecb);
            var settlement = SpawnSettlement(ref ecb, out var storehouse, out var housing, out var worship, out var villageCenter);
            SpawnVillagers(ref ecb, settlement);
            SpawnResources(ref ecb, settlement);

            ecb.Playback(EntityManager);
            ecb.Dispose();

            _hasRun = true;
            Enabled = false;
            Debug.Log("[SmokeTestFallbackBootstrapSystem] Spawned minimal fallback entities.");
        }

        private static bool IsScenarioRequested()
        {
            if (!string.IsNullOrWhiteSpace(SystemEnv.GetEnvironmentVariable(ScenarioEnvVar)))
            {
                return true;
            }

            var args = SystemEnv.GetCommandLineArgs();
            for (int i = 0; i < args.Length; i++)
            {
                var arg = args[i];
                if (string.Equals(arg, ScenarioArg, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }

                var prefix = ScenarioArg + "=";
                if (arg.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        static Entity SpawnSettlement(ref EntityCommandBuffer ecb, out Entity storehouse, out Entity housing, out Entity worship, out Entity villageCenter)
        {
            storehouse = SpawnStorehouse(ref ecb, new float3(0f, 0f, 0f));
            housing = SpawnBuilding(ref ecb, new float3(4f, 0f, -2f));
            worship = SpawnBuilding(ref ecb, new float3(-4f, 0f, -2f));
            villageCenter = SpawnBuilding(ref ecb, new float3(0f, 0f, 4f));

            var settlement = ecb.CreateEntity();
            ecb.AddComponent(settlement, LocalTransform.FromPositionRotationScale(float3.zero, quaternion.identity, 1f));
            ecb.AddComponent(settlement, new SettlementConfig
            {
                VillageCenterPrefab = Entity.Null,
                StorehousePrefab = Entity.Null,
                HousingPrefab = Entity.Null,
                WorshipPrefab = Entity.Null,
                VillagerPrefab = Entity.Null,
                InitialVillagers = 2,
                VillagerSpawnRadius = 3f,
                BuildingRingRadius = 6f,
                ResourceRingRadius = 8f,
                Seed = 1u
            });
            ecb.AddComponent(settlement, new SettlementRuntime
            {
                HasSpawned = 1,
                VillageCenterInstance = villageCenter,
                StorehouseInstance = storehouse,
                HousingInstance = housing,
                WorshipInstance = worship
            });
            ecb.AddBuffer<SettlementResource>(settlement);
            ecb.AddComponent<ScenarioSceneTag>(settlement);
            ecb.AddComponent<GameWorldTag>(settlement);

            return settlement;
        }

        static Entity SpawnBuilding(ref EntityCommandBuffer ecb, in float3 position)
        {
            var entity = ecb.CreateEntity();
            ecb.AddComponent(entity, LocalTransform.FromPositionRotationScale(position, quaternion.identity, 1f));
            return entity;
        }

        static Entity SpawnStorehouse(ref EntityCommandBuffer ecb, in float3 position)
        {
            var entity = ecb.CreateEntity();
            ecb.AddComponent(entity, LocalTransform.FromPositionRotationScale(position, quaternion.identity, 1f));
            ecb.AddComponent(entity, new DotsComponents.StorehouseInventory
            {
                TotalStored = 0f,
                TotalCapacity = 250f,
                ItemTypeCount = 0,
                IsShredding = 0,
                LastUpdateTick = 0
            });
            ecb.AddBuffer<DotsComponents.StorehouseInventoryItem>(entity);
            var capacities = ecb.AddBuffer<DotsComponents.StorehouseCapacityElement>(entity);
            capacities.Add(new DotsComponents.StorehouseCapacityElement
            {
                ResourceTypeId = new FixedString64Bytes("wood"),
                MaxCapacity = 250f
            });
            return entity;
        }

        static void SpawnVillage(ref EntityCommandBuffer ecb)
        {
            var entity = ecb.CreateEntity();
            ecb.AddComponent(entity, LocalTransform.FromPositionRotationScale(float3.zero, quaternion.identity, 1f));

            var id = new FixedString64Bytes("SMOKETEST_VILLAGE");
            ecb.AddComponent(entity, new Village
            {
                VillageId = id,
                VillageName = id,
                Phase = VillagePhase.Growing,
                CenterPosition = float3.zero,
                InfluenceRadius = 25f,
                MemberCount = 0,
                LastUpdateTick = 0
            });

            ecb.AddComponent(entity, new VillageState
            {
                CurrentState = VillageStateType.Nascent,
                StateEntryTick = 0,
                SurplusThreshold = 0f
            });
        }

        static void SpawnVillagers(ref EntityCommandBuffer ecb, Entity settlement)
        {
            var positions = new[]
            {
                new float3(2f, 0f, 2f),
                new float3(-2f, 0f, -1f)
            };

            for (int i = 0; i < positions.Length; i++)
            {
                var entity = ecb.CreateEntity();
                ecb.AddComponent(entity, LocalTransform.FromPositionRotationScale(positions[i], quaternion.identity, 1f));
                ecb.AddComponent(entity, new SettlementVillagerState
                {
                    Settlement = settlement,
                    Phase = SettlementVillagerPhase.Idle,
                    PhaseTimer = 0f,
                    RandomState = (uint)(i + 1)
                });
                ecb.AddComponent(entity, new VillagerNeeds
                {
                    Food = 80,
                    Rest = 80,
                    Sleep = 80,
                    GeneralHealth = 90,
                    Health = 90f,
                    MaxHealth = 100f,
                    Energy = 80f,
                    Morale = 75f
                });
                ecb.AddComponent(entity, new DotsComponents.VillagerAIState
                {
                    CurrentState = DotsComponents.VillagerAIState.State.Idle,
                    CurrentGoal = DotsComponents.VillagerAIState.Goal.None,
                    TargetEntity = Entity.Null,
                    TargetPosition = float3.zero,
                    StateTimer = 0f,
                    StateStartTick = 0
                });
                ecb.AddComponent(entity, new DotsComponents.VillagerJob
                {
                    Type = DotsComponents.VillagerJob.JobType.Gatherer,
                    Phase = DotsComponents.VillagerJob.JobPhase.Idle,
                    ActiveTicketId = 0,
                    Productivity = 1f,
                    LastStateChangeTick = 0
                });
                ecb.AddComponent(entity, new DotsComponents.VillagerJobTicket
                {
                    TicketId = 0,
                    JobType = DotsComponents.VillagerJob.JobType.Gatherer,
                    ResourceTypeIndex = 0,
                    ResourceEntity = Entity.Null,
                    StorehouseEntity = Entity.Null,
                    Priority = 0,
                    Phase = 0,
                    ReservedUnits = 0f,
                    AssignedTick = 0,
                    LastProgressTick = 0
                });
                var flags = new DotsComponents.VillagerFlags();
                flags.IsIdle = true;
                flags.IsWorking = false;
                ecb.AddComponent(entity, flags);
                ecb.AddComponent(entity, new DotsComponents.VillagerAvailability
                {
                    IsAvailable = 1,
                    IsReserved = 0,
                    LastChangeTick = 0,
                    BusyTime = 0f
                });
                ecb.AddComponent(entity, new VillagerNeedState
                {
                    HungerUrgency = 0.25f + (i * 0.05f),
                    RestUrgency = 0.15f,
                    FaithUrgency = 0.1f,
                    SafetyUrgency = 0.1f,
                    SocialUrgency = 0.1f,
                    WorkUrgency = 0.35f
                });
                ecb.AddComponent(entity, new VillagerNeedTuning
                {
                    HungerDecayPerTick = 0.003f,
                    RestDecayPerTick = 0.0015f,
                    FaithDecayPerTick = 0.0007f,
                    SafetyDecayPerTick = 0.0004f,
                    SocialDecayPerTick = 0.0008f,
                    WorkPressurePerTick = 0.001f,
                    MaxUrgency = 1f
                });
                ecb.AddComponent(entity, new FocusBudget
                {
                    Current = 1f,
                    Max = 1f,
                    RegenPerTick = 0.05f,
                    Reserved = 0f,
                    IsLocked = 0
                });
                ecb.AddBuffer<FocusBudgetReservation>(entity);
                ecb.AddComponent(entity, new VillagerGoalState
                {
                    CurrentGoal = VillagerGoal.Idle,
                    PreviousGoal = VillagerGoal.Idle,
                    LastGoalChangeTick = 0,
                    CurrentGoalUrgency = 0f
                });
                ecb.AddComponent(entity, new VillagerFleeIntent
                {
                    ThreatEntity = Entity.Null,
                    ExitDirection = new float3(0f, 0f, 1f),
                    Urgency = 0f,
                    RequiresLineOfSight = 0
                });
                ecb.AddComponent(entity, new VillagerMindCadence
                {
                    CadenceTicks = 1,
                    LastRunTick = 0
                });
                ecb.AddComponent(entity, new VillagerThreatState
                {
                    ThreatEntity = Entity.Null,
                    ThreatDirection = new float3(0f, 0f, 1f),
                    Urgency = 0f,
                    HasLineOfSight = 0
                });
                ecb.AddComponent(entity, new Navigation
                {
                    Destination = positions[i],
                    Speed = 5f,
                    FeatureFlags = NavigationFeatureFlags.LocomotionSmoothing | NavigationFeatureFlags.ArrivalOffset
                });
                ecb.AddComponent(entity, new VillagerCombatStats
                {
                    Attack = 60,
                    Defense = 35,
                    MaxHealth = 100f,
                    CurrentHealth = 100f,
                    Stamina = 10,
                    CurrentStamina = 10,
                    MaxMana = 0,
                    CurrentMana = 0,
                    AttackDamage = 6f,
                    AttackSpeed = 1.2f,
                    CurrentTarget = Entity.Null
                });
            }
        }

        static void SpawnResources(ref EntityCommandBuffer ecb, Entity settlement)
        {
            SpawnResourceNode(ref ecb, settlement, new float3(7f, 0f, 0f), "wood", 1);
            SpawnResourceNode(ref ecb, settlement, new float3(-6f, 0f, 4f), "stone", 2);
        }

        static void SpawnResourceNode(ref EntityCommandBuffer ecb, Entity settlement, in float3 position, FixedString64Bytes resourceId, int index)
        {
            var entity = ecb.CreateEntity();
            ecb.AddComponent(entity, LocalTransform.FromPositionRotationScale(position, quaternion.identity, 1f));
            ecb.AddComponent(entity, new ResourceSourceConfig
            {
                ResourceTypeId = resourceId,
                Amount = 100f,
                MaxAmount = 100f,
                RegenRate = 1f
            });
            var label = new FixedString32Bytes();
            label.Append('N');
            label.Append('o');
            label.Append('d');
            label.Append('e');
            label.Append(' ');
            label.Append(index);
            ecb.AddComponent(entity, new SettlementResourceNode
            {
                Settlement = settlement,
                Position = position,
                Label = label
            });
            ecb.AppendToBuffer(settlement, new SettlementResource { Node = entity });
        }
    }
}
