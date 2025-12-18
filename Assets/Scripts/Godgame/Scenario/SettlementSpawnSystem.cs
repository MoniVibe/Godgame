using Godgame.AI;
using Godgame.Presentation;
using Godgame.Rendering;
using PureDOTS.Runtime.AI;
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

                var runtimeValue = runtime.ValueRO;
                var entityManager = state.EntityManager;
                var centerPresentation = GetPrefabPresentationState(entityManager, configValue.VillageCenterPrefab);
                var storePresentation = GetPrefabPresentationState(entityManager, configValue.StorehousePrefab);
                var housingPresentation = GetPrefabPresentationState(entityManager, configValue.HousingPrefab);
                var worshipPresentation = GetPrefabPresentationState(entityManager, configValue.WorshipPrefab);
                var villagerPresentation = GetPrefabPresentationState(entityManager, configValue.VillagerPrefab);

                runtimeValue.VillageCenterInstance = InstantiatePrefab(ref ecb, configValue.VillageCenterPrefab, center);
                runtimeValue.StorehouseInstance = InstantiatePrefab(ref ecb, configValue.StorehousePrefab, center + OffsetOnCircle(0f, configValue.BuildingRingRadius));
                runtimeValue.HousingInstance = InstantiatePrefab(ref ecb, configValue.HousingPrefab, center + OffsetOnCircle(math.PI * 2f / 3f, configValue.BuildingRingRadius));
                runtimeValue.WorshipInstance = InstantiatePrefab(ref ecb, configValue.WorshipPrefab, center + OffsetOnCircle(math.PI * 4f / 3f, configValue.BuildingRingRadius));

                if (runtimeValue.VillageCenterInstance != Entity.Null)
                {
                    AssignRenderComponents(ref ecb, runtimeValue.VillageCenterInstance, GodgameSemanticKeys.VillageCenter, centerPresentation);
                }

                if (runtimeValue.StorehouseInstance != Entity.Null)
                {
                    AssignRenderComponents(ref ecb, runtimeValue.StorehouseInstance, GodgameSemanticKeys.Storehouse, storePresentation);
                }

                if (runtimeValue.HousingInstance != Entity.Null)
                {
                    AssignRenderComponents(ref ecb, runtimeValue.HousingInstance, GodgameSemanticKeys.Housing, housingPresentation);
                }

                if (runtimeValue.WorshipInstance != Entity.Null)
                {
                    AssignRenderComponents(ref ecb, runtimeValue.WorshipInstance, GodgameSemanticKeys.Worship, worshipPresentation);
                }

                resources.Clear();
                var resourceCount = math.max(3, math.min(6, configValue.InitialVillagers));
                var angleStep = math.PI * 2f / math.max(1, resourceCount);
                for (int i = 0; i < resourceCount; i++)
                {
                    var angle = i * angleStep + random.NextFloat(-0.25f, 0.25f);
                    var nodePos = center + OffsetOnCircle(angle, configValue.ResourceRingRadius);
                    var nodeEntity = ecb.CreateEntity();
                    ecb.AddComponent(nodeEntity, LocalTransform.FromPositionRotationScale(nodePos, quaternion.identity, 1f));
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
                    AssignRenderComponents(ref ecb, nodeEntity, GodgameSemanticKeys.ResourceNode, default);
                    resources.Add(new SettlementResource { Node = nodeEntity });
                }

                for (int i = 0; i < configValue.InitialVillagers; i++)
                {
                    var villager = ecb.Instantiate(configValue.VillagerPrefab);
                    var spawnPos = center + SampleSpawnOffset(ref random, configValue.VillagerSpawnRadius);
                    // Float villagers above ground so they don't overlap housing cubes
                    var villagerPos = spawnPos + new float3(0f, 1.5f, 0f);
                    // Scale up to 5f to be visible
                    ecb.SetComponent(villager, LocalTransform.FromPositionRotationScale(villagerPos, quaternion.identity, 5f));

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
                        AlignmentTint = new float4(1, 1, 1, 1),
                        TaskIconIndex = 0,
                        AnimationState = 0,
                        EffectIntensity = 0f
                    });
                    AssignRenderComponents(ref ecb, villager, villagerRenderKey, villagerPresentation);
                    if (i == 0)
                    {
                        ecb.SetComponent(villager, new RenderThemeOverride { Value = 1 });
                        ecb.SetComponentEnabled<RenderThemeOverride>(villager, true);
                    }
                    AddOrSet(ref ecb, villager, new RenderTint { Value = ResolveRoleTint(role) }, villagerPresentation.HasRenderTint);
                }

                runtimeValue.HasSpawned = 1;
                runtime.ValueRW = runtimeValue;
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
    }
}
