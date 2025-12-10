#if LEGACY_RENDERING_ARCHIVE_DISABLED
using Godgame.Economy;
using Godgame.Input;
using Godgame.Presentation;
using Godgame.Villages;
using Godgame.Villagers;
using PureDOTS.Demo.Rendering;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Entities.Graphics;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;

namespace Godgame.Demo
{
    /// <summary>
    /// Bootstrap system for Godgame Demo_01.
    /// Sets up the presentation config, spawns demo villages, villagers, and resource nodes.
    /// This demonstrates the core fantasy: villages, villagers, resources, and miracles.
    /// </summary>
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    public partial struct Godgame_Demo01_BootstrapSystem : ISystem
    {
        private bool _initialized;

        public void OnCreate(ref SystemState state)
        {
            // Only run once
            _initialized = false;
        }

        public void OnUpdate(ref SystemState state)
        {
            if (_initialized)
            {
                state.Enabled = false;
                return;
            }

            _initialized = true;

            // Create presentation config singleton if not exists
            EnsurePresentationConfig(ref state);

            // Create input singleton if not exists
            EnsureInputSingleton(ref state);

            // Spawn demo content
            SpawnDemoVillages(ref state);
            SpawnDemoResourceNodes(ref state);

            state.Enabled = false;
        }

        private DemoConfigBlob GetDemoConfig(ref SystemState state)
        {
            // Try to get config from blob reference
            var query = state.GetEntityQuery(typeof(DemoConfigBlobReference));
            if (!query.IsEmpty)
            {
                var blobRef = query.GetSingleton<DemoConfigBlobReference>();
                if (blobRef.Config.IsCreated)
                {
                    return blobRef.Config.Value;
                }
            }

            // Return defaults if no config found
            return new DemoConfigBlob
            {
                Mode = DemoScenarioMode.Demo01,
                VillageCount = 3,
                VillagersPerVillageMin = 20,
                VillagersPerVillageMax = 40,
                VillageSpacing = 80f,
                VillageInfluenceRadius = 40f,
                ResourceNodeCount = 30,
                ResourceChunkCount = 20,
                RandomSeed = 12345
            };
        }

        private void EnsurePresentationConfig(ref SystemState state)
        {
            var query = state.GetEntityQuery(typeof(PresentationConfig));
            if (!query.IsEmpty)
            {
                return;
            }

            var configEntity = state.EntityManager.CreateEntity();
            state.EntityManager.AddComponentData(configEntity, PresentationConfig.Default);
            state.EntityManager.SetName(configEntity, "PresentationConfig");
        }

        private void EnsureInputSingleton(ref SystemState state)
        {
            var query = state.GetEntityQuery(typeof(GodgameInputSingleton));
            if (!query.IsEmpty)
            {
                return;
            }

            var inputEntity = state.EntityManager.CreateEntity();
            state.EntityManager.AddComponent<GodgameInputSingleton>(inputEntity);
            state.EntityManager.AddComponent<CameraInput>(inputEntity);
            state.EntityManager.AddComponent<MiracleInput>(inputEntity);
            state.EntityManager.AddComponent<SelectionInput>(inputEntity);
            state.EntityManager.AddComponent<DebugInput>(inputEntity);
            state.EntityManager.SetName(inputEntity, "GodgameInputSingleton");
        }

        private void SpawnDemoVillages(ref SystemState state)
        {
            // Get demo config (use defaults if not found)
            DemoConfigBlob config = GetDemoConfig(ref state);
            var random = new Random(config.RandomSeed);

            // Use EntityCommandBuffer for all structural changes to avoid buffer invalidation
            var ecb = new EntityCommandBuffer(Allocator.Temp);

            // Spawn villages in a triangle formation
            int villageCount = config.VillageCount;
            var villagePositions = new NativeArray<float3>(villageCount, Allocator.Temp);
            var villagePhases = new NativeArray<VillagePhase>(villageCount, Allocator.Temp);

            // Calculate positions in triangle formation
            float spacing = config.VillageSpacing;
            villagePositions[0] = new float3(0, 0, 0); // Center village
            if (villageCount > 1)
            {
                villagePositions[1] = new float3(spacing, 0, spacing * 0.6f); // Northeast
            }
            if (villageCount > 2)
            {
                villagePositions[2] = new float3(-spacing, 0, spacing * 0.6f); // Northwest
            }
            // Additional villages placed in a circle around center
            for (int i = 3; i < villageCount; i++)
            {
                float angle = (i - 3) * (math.PI * 2f / math.max(1, villageCount - 3));
                float distance = spacing * 1.2f;
                villagePositions[i] = new float3(math.cos(angle) * distance, 0, math.sin(angle) * distance);
            }

            // Assign phases
            for (int i = 0; i < villageCount; i++)
            {
                villagePhases[i] = (VillagePhase)(i % 6); // Cycle through phases
            }

            // Create all village entities via ECB
            for (int v = 0; v < villageCount; v++)
            {
                // Create village center entity
                var villageEntity = ecb.CreateEntity();
                
                ecb.AddComponent(villageEntity, new Village
                {
                    VillageId = $"Village_{v}",
                    Phase = villagePhases[v],
                    CenterPosition = villagePositions[v],
                    InfluenceRadius = config.VillageInfluenceRadius,
                    MemberCount = 0,
                    LastUpdateTick = 0
                });
                ecb.AddComponent(villageEntity, LocalTransform.FromPosition(villagePositions[v]));
                ecb.AddComponent<VillageCenterPresentationTag>(villageEntity);
                ecb.AddComponent(villageEntity, new PresentationLODState
                {
                    CurrentLOD = PresentationLOD.LOD0_Full,
                    DistanceToCamera = 0f,
                    ShouldRender = 1
                });
                ecb.AddComponent(villageEntity, new VillageCenterVisualState
                {
                    PhaseTint = GetPhaseColor(villagePhases[v]),
                    InfluenceRadius = config.VillageInfluenceRadius,
                    Intensity = 1f,
                    AggregateState = VillageAggregateState.Normal,
                    AggregateStateIntensity = 0f
                });
                
                // Render components for Entities Graphics - village center uses village home mesh
                ecb.AddComponent(villageEntity, MaterialMeshInfo.FromRenderMeshArrayIndices(
                    DemoMeshIndices.DemoMaterialIndex,
                    DemoMeshIndices.VillageHomeMeshIndex
                ));
                
                // Explicit RenderBounds for village center (larger structure)
                ecb.AddComponent(villageEntity, new RenderBounds
                {
                    Value = new AABB
                    {
                        Center = float3.zero, // relative to LocalTransform
                        Extents = new float3(2f, 2f, 2f) // structure-sized bounds
                    }
                });
                
                ecb.AddBuffer<VillageResource>(villageEntity);
                var memberBuffer = ecb.AddBuffer<VillageMember>(villageEntity);

                // Set name via ECB (Burst-safe, no deferred entity issue)
                ecb.SetName(villageEntity, new FixedString64Bytes($"Village_{v}"));

                // Spawn villagers for this village and add to buffer via ECB
                int villagerCount = random.NextInt(config.VillagersPerVillageMin, config.VillagersPerVillageMax + 1);
                
                for (int i = 0; i < villagerCount; i++)
                {
                    var villagerEntity = SpawnVillagerViaECB(ecb, ref random, villagePositions[v], v, i);
                    memberBuffer.Add(new VillageMember { VillagerEntity = villagerEntity });
                }

                // Update member count via ECB
                ecb.SetComponent(villageEntity, new Village
                {
                    VillageId = $"Village_{v}",
                    Phase = villagePhases[v],
                    CenterPosition = villagePositions[v],
                    InfluenceRadius = config.VillageInfluenceRadius,
                    MemberCount = villagerCount,
                    LastUpdateTick = 0
                });
            }

            // Playback all changes at once
            ecb.Playback(state.EntityManager);
            ecb.Dispose();

            villagePositions.Dispose();
            villagePhases.Dispose();
        }

        private Entity SpawnVillagerViaECB(EntityCommandBuffer ecb, ref Random random, float3 villageCenter, int villageIndex, int villagerIndex)
        {
            // Random position within village influence radius
            float angle = random.NextFloat(0, math.PI * 2);
            float distance = random.NextFloat(5f, 35f);
            float3 position = villageCenter + new float3(math.cos(angle) * distance, 0, math.sin(angle) * distance);

            var villagerEntity = ecb.CreateEntity();

            // Transform
            ecb.AddComponent(villagerEntity, LocalTransform.FromPosition(position));

            // Presentation components
            ecb.AddComponent<VillagerPresentationTag>(villagerEntity);
            ecb.AddComponent(villagerEntity, new PresentationLODState
            {
                CurrentLOD = PresentationLOD.LOD0_Full,
                DistanceToCamera = 0f,
                ShouldRender = 1
            });
            ecb.AddComponent(villagerEntity, new VillagerVisualState
            {
                AlignmentTint = new float4(0.7f, 0.7f, 0.7f, 1f),
                TaskIconIndex = 0,
                AnimationState = 0,
                EffectIntensity = 0f,
                TaskState = VillagerTaskState.Idle,
                TaskStateIntensity = 0f
            });

            // Behavior components (for visual state updates)
            var behavior = VillagerBehavior.Random(ref random, 60f);
            ecb.AddComponent(villagerEntity, behavior);

            // Render components for Entities Graphics
            // Use DemoMeshIndices to reference the correct mesh/material in RenderMeshArray
            ecb.AddComponent(villagerEntity, MaterialMeshInfo.FromRenderMeshArrayIndices(
                DemoMeshIndices.DemoMaterialIndex,
                DemoMeshIndices.VillageVillagerMeshIndex
            ));
            
            // Explicit RenderBounds for villager (conservative bounds for a person-sized entity)
            ecb.AddComponent(villagerEntity, new RenderBounds
            {
                Value = new AABB
                {
                    Center = float3.zero, // relative to LocalTransform
                    Extents = new float3(0.5f, 1f, 0.5f) // person-sized bounds
                }
            });

            // Set name via ECB (Burst-safe, no deferred entity issue)
            ecb.SetName(villagerEntity, new FixedString64Bytes($"Villager_V{villageIndex}_{villagerIndex}"));

            return villagerEntity;
        }

        // Legacy method kept for compatibility - now calls ECB version
        private Entity SpawnVillager(ref SystemState state, ref Random random, float3 villageCenter, int villageIndex, int villagerIndex)
        {
            // This method is deprecated - use SpawnVillagerViaECB instead
            // Keeping for now in case it's called elsewhere, but it should be refactored
            var ecb = new EntityCommandBuffer(Allocator.Temp);
            var entity = SpawnVillagerViaECB(ecb, ref random, villageCenter, villageIndex, villagerIndex);
            ecb.Playback(state.EntityManager);
            ecb.Dispose();
            // Name is already set via ECB in SpawnVillagerViaECB, no need to set again
            return entity;
        }

        private void SpawnDemoResourceNodes(ref SystemState state)
        {
            // Get demo config (use defaults if not found)
            DemoConfigBlob config = GetDemoConfig(ref state);
            var random = new Random(config.RandomSeed + 10000); // Offset seed for nodes

            // Spawn resource nodes around the map
            var nodeTypes = new NativeArray<ResourceType>(5, Allocator.Temp);
            nodeTypes[0] = ResourceType.Oak;        // Wood
            nodeTypes[1] = ResourceType.IronOre;    // Ore
            nodeTypes[2] = ResourceType.Wheat;      // Food
            nodeTypes[3] = ResourceType.Granite;    // Stone
            nodeTypes[4] = ResourceType.Aloe;       // Herbs

            // Spawn resource nodes scattered around
            for (int i = 0; i < config.ResourceNodeCount; i++)
            {
                float angle = random.NextFloat(0, math.PI * 2);
                float distance = random.NextFloat(20f, 100f);
                float3 position = new float3(math.cos(angle) * distance, 0, math.sin(angle) * distance);

                var resourceType = nodeTypes[i % 5];

                var nodeEntity = state.EntityManager.CreateEntity();

                // Transform
                state.EntityManager.AddComponentData(nodeEntity, LocalTransform.FromPosition(position));

                // Resource data
                state.EntityManager.AddComponentData(nodeEntity, new ExtractedResource
                {
                    Type = resourceType,
                    Purity = (byte)random.NextInt(50, 100),
                    Quantity = (ushort)random.NextInt(10, 100),
                    ExtractorBusiness = Entity.Null
                });

                // Presentation components
                state.EntityManager.AddComponent<ResourceNodePresentationTag>(nodeEntity);
                state.EntityManager.AddComponentData(nodeEntity, new PresentationLODState
                {
                    CurrentLOD = PresentationLOD.LOD0_Full,
                    DistanceToCamera = 0f,
                    ShouldRender = 1
                });
                
                // Render components for Entities Graphics
                state.EntityManager.AddComponent(
                    nodeEntity,
                    new ComponentTypeSet(typeof(MaterialMeshInfo)));
                state.EntityManager.SetComponentData(nodeEntity, MaterialMeshInfo.FromRenderMeshArrayIndices(
                    DemoMeshIndices.DemoMaterialIndex,
                    DemoMeshIndices.VillageVillagerMeshIndex // Use villager mesh for resource nodes
                ));
                
                // Explicit RenderBounds for resource node
                state.EntityManager.AddComponentData(nodeEntity, new RenderBounds
                {
                    Value = new AABB
                    {
                        Center = float3.zero,
                        Extents = new float3(0.5f, 0.5f, 0.5f) // cube-sized bounds
                    }
                });

                state.EntityManager.SetName(nodeEntity, $"ResourceNode_{resourceType}_{i}");
            }

            // Spawn some resource chunks (as if being gathered)
            for (int i = 0; i < config.ResourceChunkCount; i++)
            {
                float angle = random.NextFloat(0, math.PI * 2);
                float distance = random.NextFloat(10f, 60f);
                float3 position = new float3(math.cos(angle) * distance, 0.5f, math.sin(angle) * distance);

                var resourceType = nodeTypes[i % 5];

                var chunkEntity = state.EntityManager.CreateEntity();

                // Transform
                state.EntityManager.AddComponentData(chunkEntity, LocalTransform.FromPosition(position));

                // Resource data
                state.EntityManager.AddComponentData(chunkEntity, new ExtractedResource
                {
                    Type = resourceType,
                    Purity = (byte)random.NextInt(50, 100),
                    Quantity = (ushort)random.NextInt(1, 20),
                    ExtractorBusiness = Entity.Null
                });

                // Presentation components
                state.EntityManager.AddComponent<ResourceChunkPresentationTag>(chunkEntity);
                state.EntityManager.AddComponentData(chunkEntity, new PresentationLODState
                {
                    CurrentLOD = PresentationLOD.LOD0_Full,
                    DistanceToCamera = 0f,
                    ShouldRender = 1
                });
                state.EntityManager.AddComponentData(chunkEntity, new ResourceChunkVisualState
                {
                    ResourceTypeTint = GetResourceTypeColor(resourceType),
                    QuantityScale = 0.5f + random.NextFloat(0, 0.5f),
                    IsCarried = 0
                });
                
                // Render components for Entities Graphics
                state.EntityManager.AddComponent(
                    chunkEntity,
                    new ComponentTypeSet(typeof(MaterialMeshInfo)));
                state.EntityManager.SetComponentData(chunkEntity, MaterialMeshInfo.FromRenderMeshArrayIndices(
                    DemoMeshIndices.DemoMaterialIndex,
                    DemoMeshIndices.VillageVillagerMeshIndex // Use villager mesh for resource chunks
                ));
                
                // Explicit RenderBounds for resource chunk (smaller than node)
                state.EntityManager.AddComponentData(chunkEntity, new RenderBounds
                {
                    Value = new AABB
                    {
                        Center = float3.zero,
                        Extents = new float3(0.25f, 0.25f, 0.25f) // small chunk bounds
                    }
                });

                state.EntityManager.SetName(chunkEntity, $"ResourceChunk_{resourceType}_{i}");
            }

            nodeTypes.Dispose();
        }

        private static float4 GetPhaseColor(VillagePhase phase)
        {
            return phase switch
            {
                VillagePhase.Forming => new float4(0.5f, 0.5f, 0.5f, 1f),
                VillagePhase.Growing => new float4(0.2f, 0.8f, 0.2f, 1f),
                VillagePhase.Stable => new float4(0.2f, 0.4f, 0.8f, 1f),
                VillagePhase.Expanding => new float4(0.2f, 0.9f, 0.9f, 1f),
                VillagePhase.Crisis => new float4(0.9f, 0.2f, 0.2f, 1f),
                VillagePhase.Declining => new float4(0.6f, 0.3f, 0.1f, 1f),
                _ => new float4(1f, 1f, 1f, 1f)
            };
        }

        private static float4 GetResourceTypeColor(ResourceType type)
        {
            byte typeValue = (byte)type;

            // Ores: Gray
            if (typeValue >= 1 && typeValue <= 7)
                return new float4(0.5f, 0.5f, 0.5f, 1f);

            // Wood: Brown
            if (typeValue >= 10 && typeValue <= 14)
                return new float4(0.55f, 0.27f, 0.07f, 1f);

            // Stone: Dark gray
            if (typeValue >= 20 && typeValue <= 23)
                return new float4(0.4f, 0.4f, 0.4f, 1f);

            // Herbs: Green
            if (typeValue >= 30 && typeValue <= 33)
                return new float4(0.2f, 0.6f, 0.2f, 1f);

            // Agricultural: Yellow
            if (typeValue >= 40 && typeValue <= 44)
                return new float4(0.9f, 0.8f, 0.2f, 1f);

            return new float4(1f, 1f, 1f, 1f);
        }
    }
}
#endif
