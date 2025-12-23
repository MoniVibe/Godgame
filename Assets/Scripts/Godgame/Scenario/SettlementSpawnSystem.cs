using Godgame.AI;
using Godgame.Economy;
using Godgame.Presentation;
using Godgame.Rendering;
using PureDOTS.Runtime.AI;
using PureDOTS.Runtime.Components;
using PureDOTS.Runtime.Aggregate;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Rendering;
using UnityEngine;
using PureDOTS.Rendering;
using static Godgame.Rendering.GodgamePresentationUtility;

namespace Godgame.Scenario
{
    /// <summary>
    /// Burst-safe IDs and labels used by the scenario settlement spawner.
    /// </summary>
    public struct SettlementIdsSingleton : IComponentData
    {
        public FixedString32Bytes SettlementLabel;
        public FixedString32Bytes NodeLabelPrefix;
        public FixedString32Bytes SpawnFxId;
    }

    /// <summary>
    /// Spawns the building layout, ambient resource nodes, and initial villager population
    /// so the settlement scenario has something to animate during play mode.
    /// </summary>
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    // Burst disabled: this scenario system uses managed logging.
    public partial struct SettlementSpawnSystem : ISystem
    {
        private const float VillagerScale = 0.6f;
        private const float VillagerHeightOffset = 0.5f;
        private const float BuildingScale = 6f;
        private const float ResourceNodeScale = 3f;

        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<ScenarioSceneTag>();
            state.RequireForUpdate<SettlementConfig>();
            var entityManager = state.EntityManager;

            if (!SystemAPI.TryGetSingleton<SettlementIdsSingleton>(out _))
            {
                var idsEntity = entityManager.CreateEntity(typeof(SettlementIdsSingleton));
                entityManager.SetComponentData(idsEntity, new SettlementIdsSingleton
                {
                    SettlementLabel = new FixedString32Bytes("Settlement"),
                    NodeLabelPrefix = new FixedString32Bytes("node."),
                    SpawnFxId = new FixedString32Bytes("fx.spawn.settlement")
                });
            }
        }

        public void OnUpdate(ref SystemState state)
        {
            var ecb = new EntityCommandBuffer(Allocator.Temp);

            foreach (var (config, runtime, transform, resources, entity) in SystemAPI
                         .Query<RefRO<SettlementConfig>, RefRW<SettlementRuntime>, RefRO<LocalTransform>, DynamicBuffer<SettlementResource>>()
                         .WithEntityAccess())
            {
                if (runtime.ValueRO.HasSpawned != 0)
                {
                    continue;
                }

                var configValue = config.ValueRO;
                var ids = SystemAPI.GetSingleton<SettlementIdsSingleton>();
                if (configValue.VillagerPrefab == Entity.Null)
                {
                    // Logging disabled – Burst does not allow managed logging from this system.
                    // Godgame.GodgameDebug.LogWarning("[SettlementSpawnSystem] Villager prefab reference is missing. Settlement scenario cannot spawn population.");
                    continue;
                }

                var center = transform.ValueRO.Position;
                var random = CreateRandom(configValue.Seed, (uint)entity.Index);
                var buildingRingRadius = math.max(1f, configValue.BuildingRingRadius);
                var resourceRingRadius = math.max(1f, configValue.ResourceRingRadius);
                var villagerSpawnRadius = math.max(1f, configValue.VillagerSpawnRadius);

                var runtimeValue = runtime.ValueRO;
                var entityManager = state.EntityManager;
                var centerPresentation = GetPrefabPresentationState(entityManager, configValue.VillageCenterPrefab);
                var storePresentation = GetPrefabPresentationState(entityManager, configValue.StorehousePrefab);
                var housingPresentation = GetPrefabPresentationState(entityManager, configValue.HousingPrefab);
                var worshipPresentation = GetPrefabPresentationState(entityManager, configValue.WorshipPrefab);
                var villagerPresentation = GetPrefabPresentationState(entityManager, configValue.VillagerPrefab);

                var aggregateFlags =
                    CollectiveAggregateFlags.HasWorkOrders |
                    CollectiveAggregateFlags.HasHaulingNetwork |
                    CollectiveAggregateFlags.HasConstructionOffice |
                    CollectiveAggregateFlags.HasSocialVenues |
                    CollectiveAggregateFlags.TracksHistory;

                var aggregate = new CollectiveAggregate
                {
                    Owner = entity,
                    Anchor = Entity.Null,
                    State = CollectiveAggregateState.Active,
                    Flags = aggregateFlags,
                    EstablishedTick = 0,
                    LastStateChangeTick = 0,
                    MemberCount = 0,
                    BuildingCount = 0,
                    DependentStructureCount = 0,
                    PendingWorkOrders = 0,
                    PendingHaulingRoutes = 0,
                    PendingApprovals = 0
                };

                ecb.AddComponent(entity, aggregate);
                var aggregateMembers = ecb.AddBuffer<CollectiveAggregateMember>(entity);
                ecb.AddBuffer<CollectiveWorkOrder>(entity);
                ecb.AddBuffer<CollectiveHaulingRoute>(entity);
                ecb.AddBuffer<CollectiveConstructionApproval>(entity);
                var socialVenues = ecb.AddBuffer<CollectiveSocialVenue>(entity);
                var historyEntries = ecb.AddBuffer<CollectiveAggregateHistoryEntry>(entity);
                historyEntries.Add(new CollectiveAggregateHistoryEntry
                {
                    Tick = 0,
                    EventType = CollectiveHistoryEventType.Revived,
                    Magnitude = 1f,
                    RelatedEntity = entity,
                    Context = new FixedString64Bytes("scenario.spawn")
                });

                var venueCapacity = (byte)math.clamp(configValue.InitialVillagers, 1, 200);
                int buildingCount = 0;
                int dependentStructureCount = 0;

                runtimeValue.VillageCenterInstance = InstantiatePrefab(ref ecb, configValue.VillageCenterPrefab, center);
                var storehousePos = center + OffsetOnCircle(0f, buildingRingRadius);
                runtimeValue.StorehouseInstance = InstantiatePrefab(ref ecb, configValue.StorehousePrefab, storehousePos);
                if (runtimeValue.StorehouseInstance == Entity.Null)
                {
                    runtimeValue.StorehouseInstance = CreateFallbackStorehouse(ref ecb, storehousePos);
                }
                else
                {
                    EnsureStorehouseComponents(entityManager, ref ecb, configValue.StorehousePrefab, runtimeValue.StorehouseInstance);
                }
                runtimeValue.HousingInstance = InstantiatePrefab(ref ecb, configValue.HousingPrefab, center + OffsetOnCircle(math.PI * 2f / 3f, buildingRingRadius));
                runtimeValue.WorshipInstance = InstantiatePrefab(ref ecb, configValue.WorshipPrefab, center + OffsetOnCircle(math.PI * 4f / 3f, buildingRingRadius));

                if (runtimeValue.VillageCenterInstance != Entity.Null)
                {
                    ApplyScenarioRenderContract(ref ecb, runtimeValue.VillageCenterInstance, GodgameSemanticKeys.VillageCenter, centerPresentation);
                    AddOrSet(ref ecb, runtimeValue.VillageCenterInstance, new RenderTint
                    {
                        Value = GodgamePresentationColors.ForBuilding(GodgameSemanticKeys.VillageCenter)
                    }, centerPresentation.HasRenderTint);
                    ecb.SetComponent(runtimeValue.VillageCenterInstance,
                        LocalTransform.FromPositionRotationScale(center, quaternion.identity, BuildingScale));
                    buildingCount++;

                    socialVenues.Add(new CollectiveSocialVenue
                    {
                        VenueId = new FixedString32Bytes("council_hall"),
                        Type = CollectiveSocialVenueType.CouncilHall,
                        Building = runtimeValue.VillageCenterInstance,
                        Capacity = venueCapacity,
                        Occupancy = 0,
                        Priority = 3,
                        LastActivityTick = 0
                    });
                }

                if (runtimeValue.StorehouseInstance != Entity.Null)
                {
                    ApplyScenarioRenderContract(ref ecb, runtimeValue.StorehouseInstance, GodgameSemanticKeys.Storehouse, storePresentation);
                    AddOrSet(ref ecb, runtimeValue.StorehouseInstance, new RenderTint
                    {
                        Value = GodgamePresentationColors.ForBuilding(GodgameSemanticKeys.Storehouse)
                    }, storePresentation.HasRenderTint);
                    ecb.SetComponent(runtimeValue.StorehouseInstance,
                        LocalTransform.FromPositionRotationScale(storehousePos, quaternion.identity, BuildingScale));

                    socialVenues.Add(new CollectiveSocialVenue
                    {
                        VenueId = new FixedString32Bytes("storehouse"),
                        Type = CollectiveSocialVenueType.Market,
                        Building = runtimeValue.StorehouseInstance,
                        Capacity = venueCapacity,
                        Occupancy = 0,
                        Priority = 2,
                        LastActivityTick = 0
                    });
                    buildingCount++;
                }

                if (runtimeValue.HousingInstance != Entity.Null)
                {
                    ApplyScenarioRenderContract(ref ecb, runtimeValue.HousingInstance, GodgameSemanticKeys.Housing, housingPresentation);
                    AddOrSet(ref ecb, runtimeValue.HousingInstance, new RenderTint
                    {
                        Value = GodgamePresentationColors.ForBuilding(GodgameSemanticKeys.Housing)
                    }, housingPresentation.HasRenderTint);
                    var housingPos = center + OffsetOnCircle(math.PI * 2f / 3f, buildingRingRadius);
                    ecb.SetComponent(runtimeValue.HousingInstance,
                        LocalTransform.FromPositionRotationScale(housingPos, quaternion.identity, BuildingScale));

                    socialVenues.Add(new CollectiveSocialVenue
                    {
                        VenueId = new FixedString32Bytes("hearth"),
                        Type = CollectiveSocialVenueType.Hearth,
                        Building = runtimeValue.HousingInstance,
                        Capacity = venueCapacity,
                        Occupancy = 0,
                        Priority = 1,
                        LastActivityTick = 0
                    });
                    buildingCount++;
                }

                if (runtimeValue.WorshipInstance != Entity.Null)
                {
                    ApplyScenarioRenderContract(ref ecb, runtimeValue.WorshipInstance, GodgameSemanticKeys.Worship, worshipPresentation);
                    AddOrSet(ref ecb, runtimeValue.WorshipInstance, new RenderTint
                    {
                        Value = GodgamePresentationColors.ForBuilding(GodgameSemanticKeys.Worship)
                    }, worshipPresentation.HasRenderTint);
                    var worshipPos = center + OffsetOnCircle(math.PI * 4f / 3f, buildingRingRadius);
                    ecb.SetComponent(runtimeValue.WorshipInstance,
                        LocalTransform.FromPositionRotationScale(worshipPos, quaternion.identity, BuildingScale));

                    socialVenues.Add(new CollectiveSocialVenue
                    {
                        VenueId = new FixedString32Bytes("shrine"),
                        Type = CollectiveSocialVenueType.Shrine,
                        Building = runtimeValue.WorshipInstance,
                        Capacity = venueCapacity,
                        Occupancy = 0,
                        Priority = 2,
                        LastActivityTick = 0
                    });
                    buildingCount++;
                }

                resources.Clear();
                var resourceCount = math.max(3, math.min(6, configValue.InitialVillagers));
                var angleStep = math.PI * 2f / math.max(1, resourceCount);
                for (int i = 0; i < resourceCount; i++)
                {
                    var angle = i * angleStep + random.NextFloat(-0.25f, 0.25f);
                    var nodePos = center + OffsetOnCircle(angle, resourceRingRadius);
                    var nodeEntity = ecb.CreateEntity();
                    ecb.AddComponent(nodeEntity, LocalTransform.FromPositionRotationScale(nodePos, quaternion.identity, ResourceNodeScale));
                    var label = default(FixedString32Bytes);
                    for (int c = 0; c < ids.NodeLabelPrefix.Length; c++)
                    {
                        label.Append(ids.NodeLabelPrefix[c]);
                    }
                    label.Append(i + 1);
                    ecb.AddComponent(nodeEntity, new SettlementResourceNode
                    {
                        Settlement = entity,
                        Position = nodePos,
                        Label = label
                    });
                    ApplyScenarioRenderContract(ref ecb, nodeEntity, GodgameSemanticKeys.ResourceNode, default);
                    var nodeTint = GodgamePresentationColors.ForResourceType(ResolveFallbackResourceType(i));
                    ecb.AddComponent(nodeEntity, new RenderTint { Value = nodeTint });
                    // IMPORTANT: nodeEntity is ECB-deferred until playback. Append via ECB so the reference is remapped.
                    ecb.AppendToBuffer(entity, new SettlementResource { Node = nodeEntity });
                    dependentStructureCount++;
                }

                for (int i = 0; i < configValue.InitialVillagers; i++)
                {
                    var villager = ecb.Instantiate(configValue.VillagerPrefab);
                    var spawnPos = center + SampleSpawnOffset(ref random, villagerSpawnRadius);
                    // Keep villagers smaller than buildings but still readable.
                    var villagerPos = spawnPos + new float3(0f, VillagerHeightOffset, 0f);
                    ecb.SetComponent(villager, LocalTransform.FromPositionRotationScale(villagerPos, quaternion.identity, VillagerScale));

                    ecb.AddComponent(villager, new SettlementVillagerState
                    {
                        Settlement = entity,
                        Phase = SettlementVillagerPhase.Idle,
                        RandomState = random.NextUInt(1, uint.MaxValue)
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
                    var villagerRenderKey = VillagerRenderKeyUtility.GetRenderKeyForRole(role);

                    // Add presentation components so they are picked up by the presentation system and debugger
                    ecb.AddComponent<VillagerPresentationTag>(villager);
                    ecb.AddComponent(villager, new PresentationLODState
                    {
                        CurrentLOD = PresentationLOD.LOD0_Full,
                        ShouldRender = 1,
                        DistanceToCamera = 0f
                    });
                    ecb.AddComponent(villager, new VillagerVisualState
                    {
                        AlignmentTint = ResolveRoleTint(role),
                        TaskIconIndex = 0,
                        AnimationState = 0,
                        EffectIntensity = 0f
                    });
                    ApplyScenarioRenderContract(ref ecb, villager, villagerRenderKey, villagerPresentation);
                    if (i == 0)
                    {
                        ecb.SetComponent(villager, new RenderThemeOverride { Value = 1 });
                        ecb.SetComponentEnabled<RenderThemeOverride>(villager, true);
                    }
                    AddOrSet(ref ecb, villager, new RenderTint { Value = ResolveRoleTint(role) }, villagerPresentation.HasRenderTint);

                    var roleLabel = new FixedString32Bytes(role.ToString());
                    var memberFlags = CollectiveAggregateMemberFlags.IsResident | CollectiveAggregateMemberFlags.IsWorker;
                    if (role == VillagerRenderRoleId.Combatant || role == VillagerRenderRoleId.Peacekeeper)
                    {
                        memberFlags |= CollectiveAggregateMemberFlags.IsCombatant;
                    }

                    aggregateMembers.Add(new CollectiveAggregateMember
                    {
                        MemberEntity = villager,
                        RoleId = roleLabel,
                        Flags = memberFlags,
                        JoinedTick = 0,
                        LastSeenTick = 0
                    });
                }

                aggregate.MemberCount = configValue.InitialVillagers;
                aggregate.BuildingCount = buildingCount;
                aggregate.DependentStructureCount = dependentStructureCount;
                aggregate.Anchor = runtimeValue.VillageCenterInstance;
                ecb.SetComponent(entity, aggregate);

                runtimeValue.HasSpawned = 1;
                // IMPORTANT: runtimeValue contains ECB-deferred entity references from Instantiate/CreateEntity.
                // Set the component via ECB so playback remaps those references to the real entities.
                ecb.SetComponent(entity, runtimeValue);
            }

            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }

        private static float4 ResolveRoleTint(VillagerRenderRoleId role)
        {
            return role switch
            {
                VillagerRenderRoleId.Miner => new float4(1f, 0.8f, 0.2f, 1f),
                VillagerRenderRoleId.Farmer => new float4(0.4f, 1f, 0.3f, 1f),
                VillagerRenderRoleId.Forester => new float4(0.3f, 0.8f, 0.3f, 1f),
                VillagerRenderRoleId.Breeder => new float4(1f, 0.4f, 0.8f, 1f),
                VillagerRenderRoleId.Worshipper => new float4(0.6f, 0.6f, 1f, 1f),
                VillagerRenderRoleId.Refiner => new float4(1f, 0.6f, 0.2f, 1f),
                VillagerRenderRoleId.Peacekeeper => new float4(0.8f, 0.9f, 1f, 1f),
                VillagerRenderRoleId.Combatant => new float4(1f, 0.3f, 0.3f, 1f),
                _ => new float4(1f)
            };
        }

        private static Unity.Mathematics.Random CreateRandom(uint seed, uint fallbackSalt)
        {
            var value = seed != 0 ? seed : fallbackSalt + 1u;
            value = math.max(1u, value);
            return new Unity.Mathematics.Random(value);
        }

        private static float3 OffsetOnCircle(float angle, float radius)
        {
            return new float3(math.cos(angle) * radius, 0f, math.sin(angle) * radius);
        }

        private static float3 SampleSpawnOffset(ref Unity.Mathematics.Random random, float radius)
        {
            var r = random.NextFloat(0.5f, math.max(0.5f, radius));
            var angle = random.NextFloat(0f, math.PI * 2f);
            return new float3(math.cos(angle) * r, 0f, math.sin(angle) * r);
        }

        private static ResourceType ResolveFallbackResourceType(int index)
        {
            return (index % 3) switch
            {
                0 => ResourceType.Oak,
                1 => ResourceType.IronOre,
                _ => ResourceType.Limestone
            };
        }

        private static Entity InstantiatePrefab(ref EntityCommandBuffer ecb, Entity prefab, float3 position)
        {
            if (prefab == Entity.Null)
            {
                return Entity.Null;
            }

            var instance = ecb.Instantiate(prefab);
            ecb.SetComponent(instance, LocalTransform.FromPositionRotationScale(position, quaternion.identity, 1f));
            return instance;
        }

        private static Entity CreateFallbackStorehouse(ref EntityCommandBuffer ecb, float3 position)
        {
            var entity = ecb.CreateEntity();
            ecb.AddComponent(entity, LocalTransform.FromPositionRotationScale(position, quaternion.identity, 1f));
            ecb.AddComponent(entity, new StorehouseConfig
            {
                ShredRate = 0f,
                MaxShredQueueSize = 0,
                InputRate = 0f,
                OutputRate = 0f,
                Label = default
            });
            ecb.AddComponent(entity, new StorehouseInventory
            {
                TotalStored = 0f,
                TotalCapacity = 250f,
                ItemTypeCount = 0,
                IsShredding = 0,
                LastUpdateTick = 0
            });
            ecb.AddBuffer<StorehouseCapacityElement>(entity);
            ecb.AddBuffer<StorehouseInventoryItem>(entity);
            return entity;
        }

        private static void EnsureStorehouseComponents(EntityManager entityManager, ref EntityCommandBuffer ecb, Entity prefab, Entity instance)
        {
            if (prefab == Entity.Null || instance == Entity.Null)
            {
                return;
            }

            if (!entityManager.HasComponent<StorehouseConfig>(prefab))
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

            if (!entityManager.HasComponent<StorehouseInventory>(prefab))
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

            if (!entityManager.HasComponent<StorehouseCapacityElement>(prefab))
            {
                ecb.AddBuffer<StorehouseCapacityElement>(instance);
            }

            if (!entityManager.HasComponent<StorehouseInventoryItem>(prefab))
            {
                ecb.AddBuffer<StorehouseInventoryItem>(instance);
            }
        }
    }
}
