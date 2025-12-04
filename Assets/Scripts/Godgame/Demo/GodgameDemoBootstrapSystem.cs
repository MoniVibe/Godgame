using Godgame.Economy;
using Godgame.Villagers;
using PureDOTS.Runtime.Components;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using VillagerAIState = PureDOTS.Runtime.Components.VillagerAIState;
using VillagerAvailability = PureDOTS.Runtime.Components.VillagerAvailability;
using VillagerFlags = PureDOTS.Runtime.Components.VillagerFlags;
using VillagerJob = PureDOTS.Runtime.Components.VillagerJob;
using VillagerNeeds = PureDOTS.Runtime.Components.VillagerNeeds;

namespace Godgame.Demo
{
    /// <summary>
    /// Bootstrap system that spawns demo entities (villagers, storehouses, resource nodes)
    /// at scene startup for gameplay validation.
    /// </summary>
    [BurstCompile]
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    public partial struct GodgameDemoBootstrapSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<GodgameDemoBootstrapConfig>();
            EnsureDemoOptions(ref state);
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var ecb = new EntityCommandBuffer(Allocator.Temp);

            foreach (var (config, runtime, entity) in SystemAPI.Query<
                         RefRO<GodgameDemoBootstrapConfig>,
                         RefRW<GodgameDemoBootstrapRuntime>>()
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

                // Spawn storehouse
                if (configValue.StorehousePrefab != Entity.Null)
                {
                    var storehousePos = center + new float3(0f, 0f, -5f);
                    var storehouseEntity = ecb.Instantiate(configValue.StorehousePrefab);
                    ecb.SetComponent(storehouseEntity, LocalTransform.FromPositionRotationScale(storehousePos, quaternion.identity, 1f));
                    runtimeValue.StorehouseEntity = storehouseEntity;
                }

                // Spawn resource nodes
                var resourceCount = math.max(3, configValue.ResourceNodeCount);
                var angleStep = math.PI * 2f / resourceCount;
                for (int i = 0; i < resourceCount; i++)
                {
                    var angle = i * angleStep + random.NextFloat(-0.2f, 0.2f);
                    var radius = configValue.ResourceNodeRadius;
                    var nodePos = center + new float3(
                        math.cos(angle) * radius,
                        0f,
                        math.sin(angle) * radius);

                    var nodeEntity = ecb.CreateEntity();
                    ecb.AddComponent(nodeEntity, LocalTransform.FromPositionRotationScale(nodePos, quaternion.identity, 1f));
                    ecb.AddComponent(nodeEntity, new GodgameDemoResourceNode
                    {
                        Position = nodePos,
                        ResourceType = ResourceType.IronOre,
                        Capacity = 100
                    });
                }

                // Spawn villagers
                if (configValue.VillagerPrefab != Entity.Null)
                {
                    for (int i = 0; i < configValue.InitialVillagerCount; i++)
                    {
                        var villager = ecb.Instantiate(configValue.VillagerPrefab);
                        var spawnAngle = random.NextFloat(0f, math.PI * 2f);
                        var spawnRadius = random.NextFloat(2f, configValue.VillagerSpawnRadius);
                        var spawnPos = center + new float3(
                            math.cos(spawnAngle) * spawnRadius,
                            0f,
                            math.sin(spawnAngle) * spawnRadius);

                        ecb.SetComponent(villager, LocalTransform.FromPositionRotationScale(spawnPos, quaternion.identity, 1f));

                        // Initialize villager needs
                        ecb.AddComponent(villager, new VillagerNeeds
                        {
                            Health = 100f,
                            MaxHealth = 100f,
                            Energy = 800f, // Start with decent energy
                            Morale = 700f  // Start with decent morale
                        });

                        // Initialize villager job state
                        ecb.AddComponent(villager, new VillagerJob
                        {
                            Type = VillagerJob.JobType.Gatherer,
                            Phase = VillagerJob.JobPhase.Idle,
                            Productivity = 1f,
                            LastStateChangeTick = 0
                        });

                        // Initialize villager AI state
                        ecb.AddComponent(villager, new VillagerAIState
                        {
                            CurrentState = VillagerAIState.State.Idle,
                            CurrentGoal = VillagerAIState.Goal.None,
                            StateTimer = 0f,
                            StateStartTick = 0
                        });

                        // Initialize villager flags
                        ecb.AddComponent(villager, new VillagerFlags
                        {
                            IsIdle = true,
                            IsWorking = false
                        });

                        // Initialize villager availability
                        ecb.AddComponent(villager, new VillagerAvailability
                        {
                            IsAvailable = 1,
                            LastChangeTick = 0
                        });

                        // Initialize villager behavior traits (randomized for variety)
                        var behavior = VillagerBehavior.Random(ref random, configValue.BehaviorRandomizationRange);
                        behavior.RecalculateInitiative();
                        ecb.AddComponent(villager, behavior);

                        // Add combat behavior derived from personality
                        ecb.AddComponent(villager, VillagerCombatBehavior.FromBehavior(in behavior));

                        // Add empty grudge buffer
                        ecb.AddBuffer<VillagerGrudge>(villager);
                    }
                }

                runtimeValue.HasSpawned = 1;
                runtime.ValueRW = runtimeValue;
            }

            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }

        [BurstDiscard]
        private static void EnsureDemoOptions(ref SystemState state)
        {
            var query = state.GetEntityQuery(ComponentType.ReadOnly<DemoOptions>());
            if (!query.IsEmptyIgnoreFilter)
                return;

            var entity = state.EntityManager.CreateEntity();
            state.EntityManager.AddComponentData(entity, new DemoOptions
            {
                ScenarioPath = BuildDefaultScenarioPath(),
                BindingsSet = 0,
                Veteran = 0
            });
        }

        [BurstDiscard]
        private static FixedString64Bytes BuildDefaultScenarioPath()
        {
            FixedString64Bytes path = default;
            path.Append("Scenarios/godgame/villager_loop_small.json");
            return path;
        }
    }

    /// <summary>
    /// Configuration for demo bootstrap system.
    /// </summary>
    public struct GodgameDemoBootstrapConfig : IComponentData
    {
        public Entity VillagerPrefab;
        public Entity StorehousePrefab;
        public int InitialVillagerCount;
        public int ResourceNodeCount;
        public float VillagerSpawnRadius;
        public float ResourceNodeRadius;
        public uint Seed;

        /// <summary>
        /// Range for randomizing villager behavior traits (Bold/Vengeful).
        /// Traits will be randomized in the range [-value, +value].
        /// Default: 60 gives good variety.
        /// </summary>
        public float BehaviorRandomizationRange;
    }

    /// <summary>
    /// Runtime state for demo bootstrap.
    /// </summary>
    public struct GodgameDemoBootstrapRuntime : IComponentData
    {
        public byte HasSpawned;
        public Entity StorehouseEntity;
    }

    /// <summary>
    /// Resource node for demo scene.
    /// </summary>
    public struct GodgameDemoResourceNode : IComponentData
    {
        public float3 Position;
        public ResourceType ResourceType;
        public int Capacity;
    }
}
