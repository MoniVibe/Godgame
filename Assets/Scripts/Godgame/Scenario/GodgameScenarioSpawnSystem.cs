using Godgame.Demo;
using Godgame.Economy;
using Godgame.Rendering;
using PureDOTS.Runtime.Components;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Godgame.Scenario
{
    /// <summary>
    /// Spawns entities from scenario JSON data for headless testing and scenario execution.
    /// Replaces the logger stub with actual entity spawning logic.
    /// </summary>
    [BurstCompile]
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    public partial struct GodgameScenarioSpawnSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<GodgameScenarioConfig>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var ecb = new EntityCommandBuffer(Allocator.Temp);

            foreach (var (config, runtime, entity) in SystemAPI.Query<
                RefRO<GodgameScenarioConfig>,
                RefRW<GodgameScenarioRuntime>>()
                .WithEntityAccess())
            {
                if (runtime.ValueRO.HasSpawned != 0)
                {
                    continue;
                }

                var configValue = config.ValueRO;
                var runtimeValue = runtime.ValueRO;

                var center = float3.zero;
                var random = new Unity.Mathematics.Random(configValue.Seed != 0 ? configValue.Seed : (uint)entity.Index + 1u);

                // Spawn villagers from scenario data
                if (configValue.VillagerPrefab != Entity.Null && configValue.VillagerCount > 0)
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

                        ecb.SetComponent(villager, LocalTransform.FromPositionRotationScale(spawnPos, quaternion.identity, 1f));
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
                        ecb.AddComponent(villager, new PureDOTS.Rendering.RenderKey
                        {
                            ArchetypeId = renderKeyId,
                            LOD = 0
                        });
                        ecb.AddComponent(villager, new PureDOTS.Rendering.RenderFlags
                        {
                            Visible = 1,
                            ShadowCaster = 1,
                            HighlightMask = 0
                        });
                    }
                }

                // Spawn storehouses from scenario data
                if (configValue.StorehousePrefab != Entity.Null && configValue.StorehouseCount > 0)
                {
                    for (int i = 0; i < configValue.StorehouseCount; i++)
                    {
                        var storehouse = ecb.Instantiate(configValue.StorehousePrefab);
                        var angle = (i * math.PI * 2f) / math.max(1, configValue.StorehouseCount);
                        var pos = center + new float3(
                            math.cos(angle) * configValue.SpawnRadius * 0.5f,
                            0f,
                            math.sin(angle) * configValue.SpawnRadius * 0.5f);

                        ecb.SetComponent(storehouse, LocalTransform.FromPositionRotationScale(pos, quaternion.identity, 1f));
                        ecb.AddComponent(storehouse, new PureDOTS.Rendering.RenderKey
                        {
                            ArchetypeId = GodgameRenderKeys.Storehouse,
                            LOD = 0
                        });
                        ecb.AddComponent(storehouse, new PureDOTS.Rendering.RenderFlags
                        {
                            Visible = 1,
                            ShadowCaster = 1,
                            HighlightMask = 0
                        });
                    }
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
                        ecb.AddComponent(nodeEntity, new Godgame.Demo.GodgameDemoResourceNode
                        {
                            Position = pos,
                            ResourceType = ResourceType.IronOre,
                            Capacity = 100
                        });
                        ecb.AddComponent(nodeEntity, new PureDOTS.Rendering.RenderKey
                        {
                            ArchetypeId = GodgameRenderKeys.ResourceNode,
                            LOD = 0
                        });
                        ecb.AddComponent(nodeEntity, new PureDOTS.Rendering.RenderFlags
                        {
                            Visible = 1,
                            ShadowCaster = 1,
                            HighlightMask = 0
                        });
                    }
                }

                runtimeValue.HasSpawned = 1;
                runtime.ValueRW = runtimeValue;
            }

            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }
    }

    /// <summary>
    /// Configuration for scenario spawn system.
    /// Maps scenario JSON data to entity spawn parameters.
    /// </summary>
    public struct GodgameScenarioConfig : IComponentData
    {
        public Entity VillagerPrefab;
        public Entity StorehousePrefab;
        public int VillagerCount;
        public int StorehouseCount;
        public int ResourceNodeCount;
        public float SpawnRadius;
        public uint Seed;
    }

    /// <summary>
    /// Runtime state for scenario spawn system.
    /// </summary>
    public struct GodgameScenarioRuntime : IComponentData
    {
        public byte HasSpawned;
    }
}
