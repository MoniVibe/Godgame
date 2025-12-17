using Godgame.Demo;
using Godgame.Presentation;
using Godgame.Rendering;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Rendering;
using UnityEngine;
using PureDOTS.Rendering;
using static Godgame.Rendering.GodgamePresentationUtility;

namespace Godgame.Demo
{
    /// <summary>
    /// Burst-safe IDs and labels used by the demo settlement spawner.
    /// </summary>
    public struct DemoSettlementIdsSingleton : IComponentData
    {
        public FixedString32Bytes SettlementLabel;
        public FixedString32Bytes NodeLabelPrefix;
        public FixedString32Bytes SpawnFxId;
    }

    /// <summary>
    /// Spawns the building layout, ambient resource nodes, and initial villager population
    /// so the settlement demo has something to animate during play mode.
    /// </summary>
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    // Burst disabled: this demo system uses managed logging.
    public partial struct DemoSettlementSpawnSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<DemoSceneTag>();
            state.RequireForUpdate<DemoSettlementConfig>();
            var entityManager = state.EntityManager;

            if (!SystemAPI.TryGetSingleton<DemoSettlementIdsSingleton>(out _))
            {
                var idsEntity = entityManager.CreateEntity(typeof(DemoSettlementIdsSingleton));
                entityManager.SetComponentData(idsEntity, new DemoSettlementIdsSingleton
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
                         .Query<RefRO<DemoSettlementConfig>, RefRW<DemoSettlementRuntime>, RefRO<LocalTransform>, DynamicBuffer<DemoSettlementResource>>()
                         .WithEntityAccess())
            {
                if (runtime.ValueRO.HasSpawned != 0)
                {
                    continue;
                }

                var configValue = config.ValueRO;
                var ids = SystemAPI.GetSingleton<DemoSettlementIdsSingleton>();
                if (configValue.VillagerPrefab == Entity.Null)
                {
                    // Demo log disabled – Burst does not allow managed logging from this system.
                    // Godgame.GodgameDebug.LogWarning("[DemoSettlementSpawnSystem] Villager prefab reference is missing. Settlement demo cannot spawn population.");
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
                    AssignRenderComponents(ref ecb, runtimeValue.VillageCenterInstance, GodgameRenderKeys.VillageCenter, centerPresentation);
                }

                if (runtimeValue.StorehouseInstance != Entity.Null)
                {
                    AssignRenderComponents(ref ecb, runtimeValue.StorehouseInstance, GodgameRenderKeys.Storehouse, storePresentation);
                }

                if (runtimeValue.HousingInstance != Entity.Null)
                {
                    AssignRenderComponents(ref ecb, runtimeValue.HousingInstance, GodgameRenderKeys.Housing, housingPresentation);
                }

                if (runtimeValue.WorshipInstance != Entity.Null)
                {
                    AssignRenderComponents(ref ecb, runtimeValue.WorshipInstance, GodgameRenderKeys.Worship, worshipPresentation);
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
                    ecb.AddComponent(nodeEntity, new DemoResourceNode
                    {
                        Settlement = entity,
                        Position = nodePos,
                        Label = label
                    });
                    AssignRenderComponents(ref ecb, nodeEntity, GodgameRenderKeys.ResourceNode, default);
                    resources.Add(new DemoSettlementResource { Node = nodeEntity });
                }

                for (int i = 0; i < configValue.InitialVillagers; i++)
                {
                    var villager = ecb.Instantiate(configValue.VillagerPrefab);
                    var spawnPos = center + SampleSpawnOffset(ref random, configValue.VillagerSpawnRadius);
                    // Float villagers above ground so they don't overlap housing cubes
                    var villagerPos = spawnPos + new float3(0f, 1.5f, 0f);
                    // Scale up to 5f to be visible
                    ecb.SetComponent(villager, LocalTransform.FromPositionRotationScale(villagerPos, quaternion.identity, 5f));

                    // IMPORTANT: initialize LocalToWorld so the first frame has valid data
                    ecb.AddComponent(villager, new LocalToWorld
                    {
                        Value = float4x4.TRS(villagerPos, quaternion.identity, new float3(5f))
                    });

                    // Set RenderBounds (your existing size)
                    ecb.AddComponent(villager, new RenderBounds
                    {
                        Value = new AABB
                        {
                            Center = float3.zero,
                            Extents = new float3(0.5f)
                        }
                    });

                    ecb.AddComponent(villager, new DemoVillagerState
                    {
                        Settlement = entity,
                        Phase = DemoVillagerPhase.Idle,
                        RandomState = random.NextUInt(1, uint.MaxValue)
                    });
                    var role = VillagerRenderKeyUtility.GetDefaultRoleForIndex(i);
                    ecb.AddComponent(villager, new VillagerRenderRole { Value = role });
                    var villagerRenderKey = VillagerRenderKeyUtility.GetRenderKeyForRole(role);
                    if (i == 0)
                    {
                        ecb.SetComponent(villager, new RenderThemeOverride { Value = 1 });
                        ecb.SetComponentEnabled<RenderThemeOverride>(villager, true);
                    }

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
