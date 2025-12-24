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

        private static bool ContainsSubstring(FixedString64Bytes str, FixedString64Bytes substring)
        {
            return str.IndexOf(substring) >= 0;
        }

        private static bool MatchesRegistryId(FixedString64Bytes registryId, FixedString64Bytes exactMatch, FixedString64Bytes substring)
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

            // Burst-safe FixedString construction: no managed `string` ctor calls in Burst paths.
            var villagerSubstring = new FixedString64Bytes();     villagerSubstring.Append('v'); villagerSubstring.Append('i'); villagerSubstring.Append('l'); villagerSubstring.Append('l'); villagerSubstring.Append('a'); villagerSubstring.Append('g'); villagerSubstring.Append('e'); villagerSubstring.Append('r');
            var villagerExact = new FixedString64Bytes();         villagerExact.Append('g'); villagerExact.Append('o'); villagerExact.Append('d'); villagerExact.Append('g'); villagerExact.Append('a'); villagerExact.Append('m'); villagerExact.Append('e'); villagerExact.Append('.'); villagerExact.Append('v'); villagerExact.Append('i'); villagerExact.Append('l'); villagerExact.Append('l'); villagerExact.Append('a'); villagerExact.Append('g'); villagerExact.Append('e'); villagerExact.Append('r');

            var storehouseSubstring = new FixedString64Bytes();   storehouseSubstring.Append('s'); storehouseSubstring.Append('t'); storehouseSubstring.Append('o'); storehouseSubstring.Append('r'); storehouseSubstring.Append('e'); storehouseSubstring.Append('h'); storehouseSubstring.Append('o'); storehouseSubstring.Append('u'); storehouseSubstring.Append('s'); storehouseSubstring.Append('e');
            var storehouseExact = new FixedString64Bytes();       storehouseExact.Append('g'); storehouseExact.Append('o'); storehouseExact.Append('d'); storehouseExact.Append('g'); storehouseExact.Append('a'); storehouseExact.Append('m'); storehouseExact.Append('e'); storehouseExact.Append('.'); storehouseExact.Append('s'); storehouseExact.Append('t'); storehouseExact.Append('o'); storehouseExact.Append('r'); storehouseExact.Append('e'); storehouseExact.Append('h'); storehouseExact.Append('o'); storehouseExact.Append('u'); storehouseExact.Append('s'); storehouseExact.Append('e');

            var resourceNodeSubstring = new FixedString64Bytes(); resourceNodeSubstring.Append('r'); resourceNodeSubstring.Append('e'); resourceNodeSubstring.Append('s'); resourceNodeSubstring.Append('o'); resourceNodeSubstring.Append('u'); resourceNodeSubstring.Append('r'); resourceNodeSubstring.Append('c'); resourceNodeSubstring.Append('e'); resourceNodeSubstring.Append('_'); resourceNodeSubstring.Append('n'); resourceNodeSubstring.Append('o'); resourceNodeSubstring.Append('d'); resourceNodeSubstring.Append('e');
            var resourceChunkSubstring = new FixedString64Bytes();resourceChunkSubstring.Append('r'); resourceChunkSubstring.Append('e'); resourceChunkSubstring.Append('s'); resourceChunkSubstring.Append('o'); resourceChunkSubstring.Append('u'); resourceChunkSubstring.Append('r'); resourceChunkSubstring.Append('c'); resourceChunkSubstring.Append('e'); resourceChunkSubstring.Append('_'); resourceChunkSubstring.Append('c'); resourceChunkSubstring.Append('h'); resourceChunkSubstring.Append('u'); resourceChunkSubstring.Append('n'); resourceChunkSubstring.Append('k');

            var resourceNodeExact = new FixedString64Bytes();     resourceNodeExact.Append('g'); resourceNodeExact.Append('o'); resourceNodeExact.Append('d'); resourceNodeExact.Append('g'); resourceNodeExact.Append('a'); resourceNodeExact.Append('m'); resourceNodeExact.Append('e'); resourceNodeExact.Append('.'); resourceNodeExact.Append('r'); resourceNodeExact.Append('e'); resourceNodeExact.Append('s'); resourceNodeExact.Append('o'); resourceNodeExact.Append('u'); resourceNodeExact.Append('r'); resourceNodeExact.Append('c'); resourceNodeExact.Append('e'); resourceNodeExact.Append('_'); resourceNodeExact.Append('n'); resourceNodeExact.Append('o'); resourceNodeExact.Append('d'); resourceNodeExact.Append('e');
            var resourceChunkExact = new FixedString64Bytes();    resourceChunkExact.Append('g'); resourceChunkExact.Append('o'); resourceChunkExact.Append('d'); resourceChunkExact.Append('g'); resourceChunkExact.Append('a'); resourceChunkExact.Append('m'); resourceChunkExact.Append('e'); resourceChunkExact.Append('.'); resourceChunkExact.Append('r'); resourceChunkExact.Append('e'); resourceChunkExact.Append('s'); resourceChunkExact.Append('o'); resourceChunkExact.Append('u'); resourceChunkExact.Append('r'); resourceChunkExact.Append('c'); resourceChunkExact.Append('e'); resourceChunkExact.Append('_'); resourceChunkExact.Append('c'); resourceChunkExact.Append('h'); resourceChunkExact.Append('u'); resourceChunkExact.Append('n'); resourceChunkExact.Append('k');

            var villageSubstring = new FixedString64Bytes();      villageSubstring.Append('v'); villageSubstring.Append('i'); villageSubstring.Append('l'); villageSubstring.Append('l'); villageSubstring.Append('a'); villageSubstring.Append('g'); villageSubstring.Append('e');
            var villageExact = new FixedString64Bytes();          villageExact.Append('g'); villageExact.Append('o'); villageExact.Append('d'); villageExact.Append('g'); villageExact.Append('a'); villageExact.Append('m'); villageExact.Append('e'); villageExact.Append('.'); villageExact.Append('v'); villageExact.Append('i'); villageExact.Append('l'); villageExact.Append('l'); villageExact.Append('a'); villageExact.Append('g'); villageExact.Append('e'); villageExact.Append('_'); villageExact.Append('c'); villageExact.Append('e'); villageExact.Append('n'); villageExact.Append('t'); villageExact.Append('e'); villageExact.Append('r');

            var storehouseLabel = new FixedString64Bytes();       storehouseLabel.Append('S'); storehouseLabel.Append('t'); storehouseLabel.Append('o'); storehouseLabel.Append('r'); storehouseLabel.Append('e'); storehouseLabel.Append('h'); storehouseLabel.Append('o'); storehouseLabel.Append('u'); storehouseLabel.Append('s'); storehouseLabel.Append('e');

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
                            Label = storehouseLabel
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



