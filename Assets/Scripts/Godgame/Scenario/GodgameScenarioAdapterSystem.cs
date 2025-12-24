using Godgame.Villagers;
using PureDOTS.Runtime;
using PureDOTS.Runtime.Components;
using PureDOTS.Runtime.Interrupts;
using PureDOTS.Runtime.Scenarios;
using PureDOTS.Runtime.Village;
using PureDOTS.Systems;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using GG_VillagerId = Godgame.Villagers.VillagerId;
using GG_VillagerJob = Godgame.Villagers.VillagerJob;
using GG_VillagerNeeds = Godgame.Villagers.VillagerNeeds;
using RuntimeVillagerJob = PureDOTS.Runtime.Components.VillagerJob;

namespace Godgame.Scenario
{
    /// <summary>
    /// Adapter system that spawns entities from ScenarioRunner's ScenarioEntityCountElement buffer.
    /// Reads registry IDs and spawns corresponding entities with appropriate components.
    /// </summary>
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    [UpdateAfter(typeof(PureDOTS.Systems.CoreSingletonBootstrapSystem))]
    public partial struct GodgameScenarioAdapterSystem : ISystem
    {
        private bool _hasSpawned;

        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<ScenarioInfo>();
            state.RequireForUpdate<ScenarioEntityCountElement>();
        }

        private static bool ContainsSubstring(in FixedString64Bytes str, in FixedString64Bytes substring)
        {
            FixedString64Bytes substringMutable = substring;
            return str.IndexOf(ref substringMutable) >= 0;
        }

        private static bool MatchesRegistryId(in FixedString64Bytes registryId, in FixedString64Bytes exactMatch, in FixedString64Bytes substring)
        {
            return registryId.Equals(exactMatch) || ContainsSubstring(registryId, substring);
        }

        public void OnUpdate(ref SystemState state)
        {
            if (_hasSpawned)
            {
                state.Enabled = false;
                return;
            }

            if (!SystemAPI.TryGetSingleton<ScenarioInfo>(out var scenarioInfo))
            {
                return;
            }

            var counts = SystemAPI.GetSingletonBuffer<ScenarioEntityCountElement>(true);
            if (counts.Length == 0)
            {
                _hasSpawned = true;
                state.Enabled = false;
                return;
            }

            var ecb = new EntityCommandBuffer(Allocator.Temp);
            var random = new Unity.Mathematics.Random(scenarioInfo.Seed);
            var spawnCenter = float3.zero;
            const float spawnRadius = 20f;

            var villagerSubstring = new FixedString64Bytes("villager");
            var villagerExact = new FixedString64Bytes("godgame.villager");
            var storehouseSubstring = new FixedString64Bytes("storehouse");
            var storehouseExact = new FixedString64Bytes("godgame.storehouse");
            var resourceNodeSubstring = new FixedString64Bytes("resource_node");
            var resourceChunkSubstring = new FixedString64Bytes("resource_chunk");
            var resourceNodeExact = new FixedString64Bytes("godgame.resource_node");
            var resourceChunkExact = new FixedString64Bytes("godgame.resource_chunk");
            var villageSubstring = new FixedString64Bytes("village");
            var villageExact = new FixedString64Bytes("godgame.village_center");

            for (int i = 0; i < counts.Length; i++)
            {
                var entry = counts[i];
                var registryId = entry.RegistryId;
                var count = entry.Count;

                // Map registry IDs to entity archetypes
                // Phase 0: Hardcoded mappings (will be replaced with registry system in Phase 0.5)
                if (MatchesRegistryId(registryId, villagerExact, villagerSubstring))
                {
                    // Spawn villagers
                    for (int j = 0; j < count; j++)
                    {
                        var angle = random.NextFloat(0f, math.PI * 2f);
                        var distance = random.NextFloat(0f, spawnRadius);
                        var position = spawnCenter + new float3(
                            math.cos(angle) * distance,
                            0f,
                            math.sin(angle) * distance
                        );

                        var villager = ecb.CreateEntity();
                        ecb.AddComponent(villager, LocalTransform.FromPositionRotationScale(position, quaternion.identity, 1f));
                        
                        ecb.AddComponent(villager, new GG_VillagerId
                        {
                            Value = j + 1,
                            FactionId = 0
                        });

                        ecb.AddComponent(villager, new RuntimeVillagerJob
                        {
                            Type = RuntimeVillagerJob.JobType.Gatherer,
                            Phase = RuntimeVillagerJob.JobPhase.Idle,
                            ActiveTicketId = 0,
                            Productivity = 1f,
                            LastStateChangeTick = 0
                        });

                        ecb.AddComponent(villager, new VillagerAIState
                        {
                            CurrentState = VillagerAIState.State.Idle,
                            CurrentGoal = VillagerAIState.Goal.Work,
                            TargetEntity = Entity.Null,
                            TargetPosition = float3.zero,
                            StateTimer = 0f,
                            StateStartTick = 0
                        });

                        ecb.AddComponent(villager, new GG_VillagerNeeds
                        {
                            Food = 50,
                            Rest = 80,
                            Sleep = 70,
                            GeneralHealth = 100,
                            Health = 100f,
                            MaxHealth = 100f,
                            Energy = 80f,
                            Morale = 75f
                        });

                        ecb.AddComponent(villager, new EntityIntent
                        {
                            Mode = IntentMode.Idle,
                            TargetEntity = Entity.Null,
                            TargetPosition = float3.zero,
                            TriggeringInterrupt = InterruptType.None,
                            IntentSetTick = 0,
                            Priority = InterruptPriority.Low,
                            IsValid = 0
                        });
                    }
                }
                else if (MatchesRegistryId(registryId, storehouseExact, storehouseSubstring))
                {
                    // Spawn storehouses
                    for (int j = 0; j < count; j++)
                    {
                        var angle = random.NextFloat(0f, math.PI * 2f);
                        var distance = random.NextFloat(spawnRadius * 0.5f, spawnRadius);
                        var position = spawnCenter + new float3(
                            math.cos(angle) * distance,
                            0f,
                            math.sin(angle) * distance
                        );

                        var storehouse = ecb.CreateEntity();
                        ecb.AddComponent(storehouse, LocalTransform.FromPositionRotationScale(position, quaternion.identity, 1f));
                        
                        // Add storehouse components (minimal set for Phase 0)
                        ecb.AddComponent(storehouse, new StorehouseConfig
                        {
                            ShredRate = 0f,
                            MaxShredQueueSize = 0,
                            InputRate = 100f,
                            OutputRate = 100f,
                            Label = new FixedString64Bytes("Storehouse")
                        });
                        ecb.AddBuffer<StorehouseInventoryItem>(storehouse);
                        ecb.AddBuffer<StorehouseCapacityElement>(storehouse);
                    }
                }
                else if (MatchesRegistryId(registryId, resourceNodeExact, resourceNodeSubstring) ||
                         MatchesRegistryId(registryId, resourceChunkExact, resourceChunkSubstring))
                {
                    // Spawn resource nodes
                    for (int j = 0; j < count; j++)
                    {
                        var angle = random.NextFloat(0f, math.PI * 2f);
                        var distance = random.NextFloat(spawnRadius, spawnRadius * 1.5f);
                        var position = spawnCenter + new float3(
                            math.cos(angle) * distance,
                            0f,
                            math.sin(angle) * distance
                        );

                        var node = ecb.CreateEntity();
                        ecb.AddComponent(node, LocalTransform.FromPositionRotationScale(position, quaternion.identity, 1f));
                        
                        ecb.AddComponent<ResourceNodeTag>(node);
                        
                        // Alternate resource types
                        var resourceType = j % 3; // 0=wood, 1=stone, 2=ore
                        ecb.AddComponent(node, new ResourceDeposit
                        {
                            ResourceTypeId = resourceType,
                            CurrentAmount = 100f,
                            MaxAmount = 100f,
                            RegenPerSecond = 0f
                        });
                    }
                }
                else if (MatchesRegistryId(registryId, villageExact, villageSubstring))
                {
                    // Spawn villages
                    for (int j = 0; j < count; j++)
                    {
                        var angle = (float)j / count * math.PI * 2f;
                        var distance = random.NextFloat(spawnRadius * 0.3f, spawnRadius * 0.7f);
                        var position = spawnCenter + new float3(
                            math.cos(angle) * distance,
                            0f,
                            math.sin(angle) * distance
                        );

                        var village = ecb.CreateEntity();
                        ecb.AddComponent(village, LocalTransform.FromPositionRotationScale(position, quaternion.identity, 1f));
                        
                        // Minimal village components
                        ecb.AddComponent<VillageTag>(village);
                        ecb.AddComponent(village, new PureDOTS.Runtime.Village.VillageId
                        {
                            Value = j + 1
                        });
                    }
                }
            }

            ecb.Playback(state.EntityManager);
            ecb.Dispose();

            _hasSpawned = true;
            state.Enabled = false;
        }

    }
}



