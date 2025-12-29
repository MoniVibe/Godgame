using Godgame.AI;
using Godgame.Economy;
using Godgame.Presentation;
using Godgame.Rendering;
using Godgame.Villages;
using PureDOTS.Runtime.AI;
using PureDOTS.Runtime.Components;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using PureDOTS.Rendering;
using static Godgame.Rendering.GodgamePresentationUtility;

namespace Godgame.Scenario
{
    /// <summary>
    /// Spawns entities from scenario JSON data for headless testing and scenario execution.
    /// Replaces the logger stub with actual entity spawning logic.
    /// </summary>
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    public partial struct GodgameScenarioSpawnSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<GodgameScenarioSpawnConfig>();
        }

        public void OnUpdate(ref SystemState state)
        {
            var ecb = new EntityCommandBuffer(Allocator.Temp);
            var entityManager = state.EntityManager;
            const float villagerScale = 0.9f;
            const float buildingScale = 3f;

            foreach (var (config, runtime, entity) in SystemAPI.Query<
                RefRO<GodgameScenarioSpawnConfig>,
                RefRW<GodgameScenarioRuntime>>()
                .WithEntityAccess())
            {
                if (runtime.ValueRO.HasSpawned != 0)
                {
                    continue;
                }

                var configValue = config.ValueRO;
                var runtimeValue = runtime.ValueRO;
                var villagerPresentation = GetPrefabPresentationState(entityManager, configValue.VillagerPrefab);
                var storePresentation = GetPrefabPresentationState(entityManager, configValue.StorehousePrefab);
                var centerPresentation = GetPrefabPresentationState(entityManager, configValue.VillageCenterPrefab);
                var housingPresentation = GetPrefabPresentationState(entityManager, configValue.HousingPrefab);
                var worshipPresentation = GetPrefabPresentationState(entityManager, configValue.WorshipPrefab);

                var center = float3.zero;
                var random = new Unity.Mathematics.Random(configValue.Seed != 0 ? configValue.Seed : (uint)entity.Index + 1u);
                var prefabHasSettlementState = configValue.VillagerPrefab != Entity.Null &&
                                               entityManager.HasComponent<SettlementVillagerState>(configValue.VillagerPrefab);
                var hasOverrides = entityManager.HasBuffer<GodgameScenarioSpawnOverride>(entity);
                var overrides = hasOverrides ? entityManager.GetBuffer<GodgameScenarioSpawnOverride>(entity) : default;
                hasOverrides = hasOverrides && overrides.Length > 0;
                ResolveOverrideCounts(overrides, out var overrideVillagers, out var overrideCenters, out var overrideStorehouses, out var overrideHousing, out var overrideWorship);
                var resolvedVillagerCount = hasOverrides ? overrideVillagers : configValue.VillagerCount;

                var settlementRuntime = default(SettlementRuntime);
                var settlementEntity = Entity.Null;
                var hasSettlement = TryGetExistingSettlement(entityManager, out settlementEntity, out settlementRuntime);
                if (!hasSettlement)
                {
                    settlementEntity = ecb.CreateEntity();
                    settlementRuntime = new SettlementRuntime { HasSpawned = 1 };
                    ecb.AddComponent(settlementEntity, LocalTransform.FromPosition(center));
                    ecb.AddComponent(settlementEntity, settlementRuntime);
                    ecb.AddBuffer<SettlementResource>(settlementEntity);
                }
                else
                {
                    settlementRuntime.HasSpawned = 1;
                    if (!entityManager.HasComponent<LocalTransform>(settlementEntity))
                    {
                        ecb.AddComponent(settlementEntity, LocalTransform.FromPosition(center));
                    }

                    if (!entityManager.HasBuffer<SettlementResource>(settlementEntity))
                    {
                        ecb.AddBuffer<SettlementResource>(settlementEntity);
                    }
                }

                var villageEntity = Entity.Null;
                if (!TryGetExistingVillage(entityManager, out villageEntity))
                {
                    villageEntity = ecb.CreateEntity();
                    ecb.AddComponent(villageEntity, LocalTransform.FromPosition(center));
                    ecb.AddComponent(villageEntity, new Village
                    {
                        VillageId = new FixedString64Bytes("scenario_village"),
                        VillageName = new FixedString64Bytes("Scenario Village"),
                        Phase = VillagePhase.Growing,
                        CenterPosition = center,
                        InfluenceRadius = math.max(10f, configValue.SpawnRadius),
                        MemberCount = math.max(0, resolvedVillagerCount),
                        LastUpdateTick = 0u
                    });
                    ecb.AddComponent(villageEntity, new VillageAIDecision
                    {
                        CurrentPriority = 0,
                        DecisionType = 0,
                        TargetEntity = Entity.Null,
                        TargetPosition = center,
                        DecisionTick = 0u,
                        DecisionDuration = 10f
                    });
                    ecb.AddBuffer<VillageResource>(villageEntity);
                    ecb.AddBuffer<VillageMember>(villageEntity);
                    ecb.AddBuffer<VillageExpansionRequest>(villageEntity);
                }

                var primaryVillageCenter = Entity.Null;
                var primaryHousing = Entity.Null;
                var primaryWorship = Entity.Null;
                var primaryStorehouse = Entity.Null;

                if (hasOverrides)
                {
                    SpawnOverrides(ref ecb, entityManager, overrides, configValue, villagerPresentation, storePresentation, centerPresentation,
                        housingPresentation, worshipPresentation, buildingScale, villagerScale, prefabHasSettlementState, settlementEntity, ref random,
                        ref primaryVillageCenter, ref primaryHousing, ref primaryWorship, ref primaryStorehouse);
                }

                var centerRing = math.max(1f, configValue.SpawnRadius * 0.2f);
                var housingRing = math.max(1f, configValue.SpawnRadius * 0.35f);
                var worshipRing = math.max(1f, configValue.SpawnRadius * 0.35f);

                if (!hasOverrides && configValue.VillageCenterPrefab != Entity.Null && configValue.VillageCenterCount > 0)
                {
                    for (int i = 0; i < configValue.VillageCenterCount; i++)
                    {
                        var angle = (i * math.PI * 2f) / math.max(1, configValue.VillageCenterCount);
                        var pos = center + new float3(
                            math.cos(angle) * centerRing,
                            0f,
                            math.sin(angle) * centerRing);
                        var instance = ecb.Instantiate(configValue.VillageCenterPrefab);
                        ecb.SetComponent(instance, LocalTransform.FromPositionRotationScale(pos, quaternion.identity, buildingScale));
                        ApplyScenarioRenderContract(ref ecb, instance, GodgameSemanticKeys.VillageCenter, centerPresentation);
                        AddOrSet(ref ecb, instance, new RenderTint
                        {
                            Value = GodgamePresentationColors.ForBuilding(GodgameSemanticKeys.VillageCenter)
                        }, centerPresentation.HasRenderTint);

                        if (primaryVillageCenter == Entity.Null)
                        {
                            primaryVillageCenter = instance;
                        }
                    }
                }

                if (!hasOverrides && configValue.HousingPrefab != Entity.Null && configValue.HousingCount > 0)
                {
                    var startAngle = math.PI * 0.5f;
                    for (int i = 0; i < configValue.HousingCount; i++)
                    {
                        var angle = startAngle + (i * math.PI * 2f) / math.max(1, configValue.HousingCount);
                        var pos = center + new float3(
                            math.cos(angle) * housingRing,
                            0f,
                            math.sin(angle) * housingRing);
                        var instance = ecb.Instantiate(configValue.HousingPrefab);
                        ecb.SetComponent(instance, LocalTransform.FromPositionRotationScale(pos, quaternion.identity, buildingScale));
                        ApplyScenarioRenderContract(ref ecb, instance, GodgameSemanticKeys.Housing, housingPresentation);
                        AddOrSet(ref ecb, instance, new RenderTint
                        {
                            Value = GodgamePresentationColors.ForBuilding(GodgameSemanticKeys.Housing)
                        }, housingPresentation.HasRenderTint);

                        if (primaryHousing == Entity.Null)
                        {
                            primaryHousing = instance;
                        }
                    }
                }

                if (!hasOverrides && configValue.WorshipPrefab != Entity.Null && configValue.WorshipCount > 0)
                {
                    var startAngle = math.PI * 1.5f;
                    for (int i = 0; i < configValue.WorshipCount; i++)
                    {
                        var angle = startAngle + (i * math.PI * 2f) / math.max(1, configValue.WorshipCount);
                        var pos = center + new float3(
                            math.cos(angle) * worshipRing,
                            0f,
                            math.sin(angle) * worshipRing);
                        var instance = ecb.Instantiate(configValue.WorshipPrefab);
                        ecb.SetComponent(instance, LocalTransform.FromPositionRotationScale(pos, quaternion.identity, buildingScale));
                        ApplyScenarioRenderContract(ref ecb, instance, GodgameSemanticKeys.Worship, worshipPresentation);
                        AddOrSet(ref ecb, instance, new RenderTint
                        {
                            Value = GodgamePresentationColors.ForBuilding(GodgameSemanticKeys.Worship)
                        }, worshipPresentation.HasRenderTint);

                        if (primaryWorship == Entity.Null)
                        {
                            primaryWorship = instance;
                        }
                    }
                }

                // Spawn villagers from scenario data
                if (!hasOverrides && configValue.VillagerPrefab != Entity.Null && configValue.VillagerCount > 0)
                {
                    for (int i = 0; i < configValue.VillagerCount; i++)
                    {
                        var villager = ecb.Instantiate(configValue.VillagerPrefab);
                        var spawnAngle = random.NextFloat(0f, math.PI * 2f);
                        var spawnRadius = random.NextFloat(2f, configValue.SpawnRadius);
                        var spawnPos = center + new float3(
                            math.cos(spawnAngle) * spawnRadius,
                            0f,
                            math.sin(spawnAngle) * spawnRadius);

                        ecb.SetComponent(villager, LocalTransform.FromPositionRotationScale(spawnPos, quaternion.identity, villagerScale));
                        ecb.AddComponent<LocalToWorld>(villager);

                        // Initialize villager components
                        ecb.AddComponent(villager, new Godgame.Villagers.VillagerNeeds
                        {
                            Health = 100f,
                            MaxHealth = 100f,
                            Energy = 800f,
                            Morale = 700f
                        });

                        var role = VillagerRenderKeyUtility.GetDefaultRoleForIndex(i);
                        ecb.AddComponent(villager, new VillagerRenderRole { Value = role });
                        var roleAssignment = GodgameAIRoleDefinitions.ResolveForVillager(role);
                        ecb.AddComponent(villager, new AIRole { RoleId = roleAssignment.RoleId });
                        ecb.AddComponent(villager, new AIDoctrine { DoctrineId = roleAssignment.DoctrineId });
                        ecb.AddComponent(villager, new AIBehaviorProfile
                        {
                            ProfileId = roleAssignment.ProfileId,
                            ProfileHash = roleAssignment.ProfileHash,
                            ProfileEntity = Entity.Null,
                            SourceId = GodgameAIRoleDefinitions.SourceScenario
                        });
                        var renderKeyId = VillagerRenderKeyUtility.GetRenderKeyForRole(role);
                        var dotsJobType = VillagerRenderKeyUtility.GetDefaultPureDotsJobForRole(role);

                        ecb.AddComponent(villager, new PureDOTS.Runtime.Components.VillagerJob
                        {
                            Type = dotsJobType,
                            Phase = PureDOTS.Runtime.Components.VillagerJob.JobPhase.Idle,
                            Productivity = 1f,
                            LastStateChangeTick = 0
                        });

                        ecb.AddComponent(villager, new PureDOTS.Runtime.Components.VillagerAIState
                        {
                            CurrentState = PureDOTS.Runtime.Components.VillagerAIState.State.Idle,
                            CurrentGoal = PureDOTS.Runtime.Components.VillagerAIState.Goal.None,
                            StateTimer = 0f,
                            StateStartTick = 0
                        });

                        ecb.AddComponent(villager, new PureDOTS.Runtime.Components.VillagerFlags
                        {
                            IsIdle = true,
                            IsWorking = false
                        });

                        ecb.AddComponent(villager, new PureDOTS.Runtime.Components.VillagerAvailability
                        {
                            IsAvailable = 1,
                            LastChangeTick = 0
                        });
                        ecb.AddComponent(villager, Godgame.Villagers.VillagerBehavior.Neutral);

                        if (settlementEntity != Entity.Null)
                        {
                            var villagerState = new SettlementVillagerState
                            {
                                Settlement = settlementEntity,
                                Phase = SettlementVillagerPhase.Idle,
                                RandomState = random.NextUInt(1, uint.MaxValue)
                            };

                            if (prefabHasSettlementState)
                            {
                                ecb.SetComponent(villager, villagerState);
                            }
                            else
                            {
                                ecb.AddComponent(villager, villagerState);
                            }
                        }

                        ApplyScenarioRenderContract(ref ecb, villager, renderKeyId, villagerPresentation);
                    }
                }

                // Spawn storehouses from scenario data
                if (!hasOverrides && configValue.StorehouseCount > 0)
                {
                    for (int i = 0; i < configValue.StorehouseCount; i++)
                    {
                        Entity storehouse;
                        var angle = (i * math.PI * 2f) / math.max(1, configValue.StorehouseCount);
                        var pos = center + new float3(
                            math.cos(angle) * configValue.SpawnRadius * 0.5f,
                            0f,
                            math.sin(angle) * configValue.SpawnRadius * 0.5f);

                        var storehouseIsFallback = configValue.StorehousePrefab == Entity.Null;
                        if (!storehouseIsFallback)
                        {
                            storehouse = ecb.Instantiate(configValue.StorehousePrefab);
                            ecb.SetComponent(storehouse, LocalTransform.FromPositionRotationScale(pos, quaternion.identity, buildingScale));
                            ApplyScenarioRenderContract(ref ecb, storehouse, GodgameSemanticKeys.Storehouse, storePresentation);
                            AddOrSet(ref ecb, storehouse, new RenderTint
                            {
                                Value = GodgamePresentationColors.ForBuilding(GodgameSemanticKeys.Storehouse)
                            }, storePresentation.HasRenderTint);
                        }
                        else
                        {
                            storehouse = CreateFallbackStorehouse(ref ecb, pos, buildingScale);
                        }

                        if (!storehouseIsFallback)
                        {
                            EnsureStorehouseComponents(entityManager, ref ecb, configValue.StorehousePrefab, storehouse);
                        }

                        if (primaryStorehouse == Entity.Null)
                        {
                            primaryStorehouse = storehouse;
                        }
                    }
                }

                if (primaryVillageCenter != Entity.Null && settlementRuntime.VillageCenterInstance == Entity.Null)
                {
                    settlementRuntime.VillageCenterInstance = primaryVillageCenter;
                }

                if (primaryHousing != Entity.Null && settlementRuntime.HousingInstance == Entity.Null)
                {
                    settlementRuntime.HousingInstance = primaryHousing;
                }

                if (primaryWorship != Entity.Null && settlementRuntime.WorshipInstance == Entity.Null)
                {
                    settlementRuntime.WorshipInstance = primaryWorship;
                }

                if (primaryStorehouse != Entity.Null)
                {
                    if (settlementRuntime.StorehouseInstance == Entity.Null)
                    {
                        settlementRuntime.StorehouseInstance = primaryStorehouse;
                    }

                    if (settlementRuntime.VillageCenterInstance == Entity.Null)
                    {
                        settlementRuntime.VillageCenterInstance = primaryStorehouse;
                    }

                    if (settlementRuntime.HousingInstance == Entity.Null)
                    {
                        settlementRuntime.HousingInstance = primaryStorehouse;
                    }

                    if (settlementRuntime.WorshipInstance == Entity.Null)
                    {
                        settlementRuntime.WorshipInstance = primaryStorehouse;
                    }
                }

                if (settlementEntity != Entity.Null)
                {
                    ecb.SetComponent(settlementEntity, settlementRuntime);
                }

                // Spawn resource nodes from scenario data
                if (configValue.ResourceNodeCount > 0)
                {
                    for (int i = 0; i < configValue.ResourceNodeCount; i++)
                    {
                        var nodeEntity = ecb.CreateEntity();
                        var angle = (i * math.PI * 2f) / math.max(1, configValue.ResourceNodeCount);
                        var pos = center + new float3(
                            math.cos(angle) * configValue.SpawnRadius,
                            0f,
                            math.sin(angle) * configValue.SpawnRadius);

                        ecb.AddComponent(nodeEntity, LocalTransform.FromPositionRotationScale(pos, quaternion.identity, 1f));
                        ecb.AddComponent(nodeEntity, new GodgameScenarioResourceNode
                        {
                            Position = pos,
                            ResourceType = ResourceType.IronOre,
                            Capacity = 100
                        });
                        ApplyScenarioRenderContract(ref ecb, nodeEntity, GodgameSemanticKeys.ResourceNode, default);
                        ecb.AddComponent(nodeEntity, new RenderTint
                        {
                            Value = GodgamePresentationColors.ForResourceType(ResourceType.IronOre)
                        });

                        if (settlementEntity != Entity.Null)
                        {
                            ecb.AppendToBuffer(settlementEntity, new SettlementResource { Node = nodeEntity });
                        }
                    }
                }

                runtimeValue.HasSpawned = 1;
                runtime.ValueRW = runtimeValue;
            }

            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }

        private static bool TryGetExistingSettlement(EntityManager entityManager, out Entity settlement, out SettlementRuntime runtime)
        {
            settlement = Entity.Null;
            runtime = default;

            using var query = entityManager.CreateEntityQuery(ComponentType.ReadOnly<SettlementRuntime>());
            if (query.IsEmptyIgnoreFilter)
            {
                return false;
            }

            using var entities = query.ToEntityArray(Allocator.Temp);
            if (entities.Length == 0)
            {
                return false;
            }

            settlement = entities[0];
            if (settlement != Entity.Null && entityManager.HasComponent<SettlementRuntime>(settlement))
            {
                runtime = entityManager.GetComponentData<SettlementRuntime>(settlement);
            }

            return settlement != Entity.Null;
        }

        private static bool TryGetExistingVillage(EntityManager entityManager, out Entity village)
        {
            village = Entity.Null;
            using var query = entityManager.CreateEntityQuery(ComponentType.ReadOnly<Village>());
            if (query.IsEmptyIgnoreFilter)
            {
                return false;
            }

            using var entities = query.ToEntityArray(Allocator.Temp);
            if (entities.Length == 0)
            {
                return false;
            }

            village = entities[0];
            return village != Entity.Null;
        }

        private static Entity CreateFallbackStorehouse(ref EntityCommandBuffer ecb, in float3 position, float scale)
        {
            var storehouse = ecb.CreateEntity();
            ecb.AddComponent(storehouse, LocalTransform.FromPositionRotationScale(position, quaternion.identity, scale));
            ecb.AddComponent(storehouse, new StorehouseConfig
            {
                ShredRate = 0f,
                MaxShredQueueSize = 0,
                InputRate = 0f,
                OutputRate = 0f,
                Label = default
            });
            ecb.AddComponent(storehouse, new StorehouseInventory
            {
                TotalStored = 0f,
                TotalCapacity = 250f,
                ItemTypeCount = 0,
                IsShredding = 0,
                LastUpdateTick = 0
            });
            var capacities = ecb.AddBuffer<StorehouseCapacityElement>(storehouse);
            capacities.Add(new StorehouseCapacityElement
            {
                ResourceTypeId = new FixedString64Bytes("wood"),
                MaxCapacity = 250f
            });
            ecb.AddBuffer<StorehouseInventoryItem>(storehouse);
            return storehouse;
        }

        private static void EnsureStorehouseComponents(EntityManager entityManager, ref EntityCommandBuffer ecb, Entity prefab, Entity instance)
        {
            if (instance == Entity.Null)
            {
                return;
            }

            if (prefab == Entity.Null || !entityManager.HasComponent<StorehouseConfig>(prefab))
            {
                ecb.AddComponent(instance, new StorehouseConfig
                {
                    ShredRate = 0f,
                    MaxShredQueueSize = 0,
                    InputRate = 0f,
                    OutputRate = 0f,
                    Label = default
                });
            }

            if (prefab == Entity.Null || !entityManager.HasComponent<StorehouseInventory>(prefab))
            {
                ecb.AddComponent(instance, new StorehouseInventory
                {
                    TotalStored = 0f,
                    TotalCapacity = 250f,
                    ItemTypeCount = 0,
                    IsShredding = 0,
                    LastUpdateTick = 0
                });
            }

            if (prefab == Entity.Null || !entityManager.HasComponent<StorehouseCapacityElement>(prefab))
            {
                ecb.AddBuffer<StorehouseCapacityElement>(instance);
            }

            if (prefab == Entity.Null || !entityManager.HasComponent<StorehouseInventoryItem>(prefab))
            {
                ecb.AddBuffer<StorehouseInventoryItem>(instance);
            }
        }
    }

    /// <summary>
    /// Configuration for scenario spawn system.
    /// Maps scenario JSON data to entity spawn parameters.
    /// </summary>
    public struct GodgameScenarioSpawnConfig : IComponentData
    {
        public Entity VillagerPrefab;
        public Entity VillageCenterPrefab;
        public Entity StorehousePrefab;
        public Entity HousingPrefab;
        public Entity WorshipPrefab;
        public int VillagerCount;
        public int VillageCenterCount;
        public int StorehouseCount;
        public int HousingCount;
        public int WorshipCount;
        public int ResourceNodeCount;
        public float SpawnRadius;
        public uint Seed;
    }

    public enum GodgameScenarioSpawnKind : byte
    {
        None = 0,
        Villager = 1,
        VillageCenter = 2,
        Storehouse = 3,
        Housing = 4,
        Worship = 5
    }

    public struct GodgameScenarioSpawnOverride : IBufferElementData
    {
        public GodgameScenarioSpawnKind Kind;
        public float3 Position;
        public int Count;
    }

    /// <summary>
    /// Runtime state for scenario spawn system.
    /// </summary>
    public struct GodgameScenarioRuntime : IComponentData
    {
        public byte HasSpawned;
    }
}
