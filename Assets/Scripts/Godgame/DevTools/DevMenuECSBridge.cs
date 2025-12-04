using System.Collections.Generic;
using Godgame.Combat;
using Godgame.Demo;
using Godgame.Economy;
using Godgame.Villages;
using Godgame.Villagers;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using BuildingDurability = Godgame.Buildings.BuildingDurability;
using VillagerAIState = PureDOTS.Runtime.Components.VillagerAIState;
using VillagerAvailability = PureDOTS.Runtime.Components.VillagerAvailability;
using VillagerFlags = PureDOTS.Runtime.Components.VillagerFlags;
using VillagerJob = PureDOTS.Runtime.Components.VillagerJob;
using VillagerNeeds = PureDOTS.Runtime.Components.VillagerNeeds;
using EntityFocus = Godgame.Combat.EntityFocus;

namespace Godgame.DevTools
{
    /// <summary>
    /// ECS bridge for the dev menu. Handles entity spawning, queries, and manipulation.
    /// </summary>
    [UpdateInGroup(typeof(InitializationSystemGroup), OrderLast = true)]
    public partial class DevMenuECSBridgeSystem : SystemBase
    {
        private EntityQuery _villagerQuery;
        private EntityQuery _villageQuery;
        private EntityQuery _buildingQuery;
        
        public static DevMenuECSBridge Bridge { get; private set; }
        
        protected override void OnCreate()
        {
            _villagerQuery = GetEntityQuery(ComponentType.ReadOnly<VillagerNeeds>());
            _villageQuery = GetEntityQuery(ComponentType.ReadOnly<Village>());
            _buildingQuery = GetEntityQuery(ComponentType.ReadOnly<BuildingDurability>());
            
            Bridge = new DevMenuECSBridge();
        }
        
        protected override void OnDestroy()
        {
            Bridge?.Dispose();
            Bridge = null;
        }
        
        protected override void OnUpdate()
        {
            if (Bridge == null) return;
            
            // Process pending spawn requests
            ProcessSpawnRequests();
            
            // Update entity counts
            Bridge.VillagerCount = _villagerQuery.CalculateEntityCount();
            Bridge.VillageCount = _villageQuery.CalculateEntityCount();
            Bridge.BuildingCount = _buildingQuery.CalculateEntityCount();
            Bridge.TotalEntityCount = EntityManager.UniversalQuery.CalculateEntityCount();
        }
        
        private void ProcessSpawnRequests()
        {
            while (Bridge.PendingSpawnRequests.Count > 0)
            {
                var request = Bridge.PendingSpawnRequests.Dequeue();
                ExecuteSpawnRequest(request);
            }
        }
        
        private void ExecuteSpawnRequest(DevSpawnRequest request)
        {
            var ecb = new EntityCommandBuffer(Allocator.Temp);
            var random = new Unity.Mathematics.Random((uint)(SystemAPI.Time.ElapsedTime * 1000 + 1));
            
            switch (request.SpawnType)
            {
                case DevSpawnType.Villager:
                    SpawnVillagers(ecb, ref random, request.Position, request.Count, request.Randomize);
                    break;
                case DevSpawnType.Village:
                    SpawnVillage(ecb, ref random, request.Position, request.Count);
                    break;
                case DevSpawnType.Band:
                    SpawnBand(ecb, ref random, request.Position, request.Count, request.IsFriendly);
                    break;
                case DevSpawnType.StressTest:
                    SpawnStressTestEntities(ecb, ref random, request.Position, request.Count);
                    break;
            }
            
            ecb.Playback(EntityManager);
            ecb.Dispose();
        }
        
        private void SpawnVillagers(EntityCommandBuffer ecb, ref Unity.Mathematics.Random random, 
            float3 center, int count, bool randomize)
        {
            for (int i = 0; i < count; i++)
            {
                var entity = ecb.CreateEntity();
                
                // Position in a circle
                var angle = random.NextFloat(0f, math.PI * 2f);
                var radius = random.NextFloat(1f, 5f);
                var pos = center + new float3(math.cos(angle) * radius, 0f, math.sin(angle) * radius);
                
                ecb.AddComponent(entity, LocalTransform.FromPosition(pos));
                ecb.AddComponent<LocalToWorld>(entity);
                
                // Core villager components
                ecb.AddComponent(entity, new VillagerNeeds
                {
                    Health = randomize ? random.NextFloat(50f, 100f) : 100f,
                    MaxHealth = 100f,
                    Energy = randomize ? random.NextFloat(400f, 1000f) : 800f,
                    Morale = randomize ? random.NextFloat(400f, 900f) : 700f
                });
                
                ecb.AddComponent(entity, new VillagerJob
                {
                    Type = (VillagerJob.JobType)random.NextInt(0, 6),
                    Phase = VillagerJob.JobPhase.Idle,
                    Productivity = 1f
                });
                
                ecb.AddComponent(entity, new VillagerAIState
                {
                    CurrentState = VillagerAIState.State.Idle,
                    CurrentGoal = VillagerAIState.Goal.None
                });
                
                ecb.AddComponent(entity, new VillagerFlags { IsIdle = true });
                ecb.AddComponent(entity, new VillagerAvailability { IsAvailable = 1 });
                
                // Behavior with randomization
                var behavior = randomize 
                    ? VillagerBehavior.Random(ref random, 60f)
                    : VillagerBehavior.Neutral;
                behavior.RecalculateInitiative();
                ecb.AddComponent(entity, behavior);
                
                ecb.AddComponent(entity, VillagerCombatBehavior.FromBehavior(in behavior));
                ecb.AddBuffer<VillagerGrudge>(entity);
                
                // Focus for combat/profession abilities
                ecb.AddComponent(entity, new EntityFocus
                {
                    CurrentFocus = randomize ? random.NextFloat(50f, 100f) : 100f,
                    MaxFocus = 100f,
                    BaseRegenRate = 0.5f,
                    CurrentRegenRate = 0.5f
                });
                ecb.AddBuffer<ActiveFocusAbility>(entity);
                
                // Tag for identification
                ecb.AddComponent(entity, new DevSpawnedTag());
            }
            
            Debug.Log($"[DevMenu] Spawned {count} villagers at {center}");
        }
        
        private void SpawnVillage(EntityCommandBuffer ecb, ref Unity.Mathematics.Random random,
            float3 center, int villagerCount)
        {
            // Create village entity
            var villageEntity = ecb.CreateEntity();
            ecb.AddComponent(villageEntity, LocalTransform.FromPosition(center));
            ecb.AddComponent<LocalToWorld>(villageEntity);
            ecb.AddComponent(villageEntity, new Village
            {
                VillageId = $"DevVillage_{random.NextInt(1000, 9999)}",
                Phase = VillagePhase.Growing,
                CenterPosition = center,
                InfluenceRadius = 25f,
                MemberCount = villagerCount
            });
            ecb.AddBuffer<VillageResource>(villageEntity);
            ecb.AddBuffer<VillageMember>(villageEntity);
            ecb.AddComponent(villageEntity, new DevSpawnedTag());
            
            // Spawn villagers around center
            SpawnVillagers(ecb, ref random, center, villagerCount, true);
            
            Debug.Log($"[DevMenu] Spawned village with {villagerCount} villagers at {center}");
        }
        
        private void SpawnBand(EntityCommandBuffer ecb, ref Unity.Mathematics.Random random,
            float3 center, int count, bool isFriendly)
        {
            // Create band entity
            var bandEntity = ecb.CreateEntity();
            ecb.AddComponent(bandEntity, LocalTransform.FromPosition(center));
            ecb.AddComponent<LocalToWorld>(bandEntity);
            ecb.AddComponent(bandEntity, new DevSpawnedTag());
            
            // Spawn band members
            for (int i = 0; i < count; i++)
            {
                var entity = ecb.CreateEntity();
                
                var angle = random.NextFloat(0f, math.PI * 2f);
                var radius = random.NextFloat(1f, 3f);
                var pos = center + new float3(math.cos(angle) * radius, 0f, math.sin(angle) * radius);
                
                ecb.AddComponent(entity, LocalTransform.FromPosition(pos));
                ecb.AddComponent<LocalToWorld>(entity);
                
                // Combat-focused villager
                ecb.AddComponent(entity, new VillagerNeeds
                {
                    Health = 100f,
                    MaxHealth = 100f,
                    Energy = 900f,
                    Morale = 800f
                });
                
                ecb.AddComponent(entity, new VillagerJob
                {
                    Type = VillagerJob.JobType.Guard,
                    Phase = VillagerJob.JobPhase.Idle,
                    Productivity = 1f
                });
                
                var behavior = VillagerBehavior.Random(ref random, 40f);
                behavior.BoldScore = math.clamp(behavior.BoldScore + 30, -100, 100); // More bold for warriors
                behavior.RecalculateInitiative();
                ecb.AddComponent(entity, behavior);
                
                ecb.AddComponent(entity, VillagerCombatBehavior.FromBehavior(in behavior));
                
                // Strong focus for combat
                ecb.AddComponent(entity, new EntityFocus
                {
                    CurrentFocus = 100f,
                    MaxFocus = 120f, // Higher cap for warriors
                    BaseRegenRate = 0.7f,
                    CurrentRegenRate = 0.7f
                });
                ecb.AddBuffer<ActiveFocusAbility>(entity);
                
                ecb.AddComponent(entity, new VillagerAIState
                {
                    CurrentState = VillagerAIState.State.Idle,
                    CurrentGoal = VillagerAIState.Goal.None
                });
                
                ecb.AddComponent(entity, new VillagerFlags());
                ecb.AddComponent(entity, new VillagerAvailability { IsAvailable = 1 });
                ecb.AddBuffer<VillagerGrudge>(entity);
                ecb.AddComponent(entity, new DevSpawnedTag());
                
                // Faction tag
                if (!isFriendly)
                {
                    ecb.AddComponent(entity, new EnemyFactionTag());
                }
            }
            
            Debug.Log($"[DevMenu] Spawned {(isFriendly ? "friendly" : "enemy")} band ({count}) at {center}");
        }
        
        private void SpawnStressTestEntities(EntityCommandBuffer ecb, ref Unity.Mathematics.Random random,
            float3 center, int count)
        {
            // Spawn in batches for better distribution
            var gridSize = (int)math.ceil(math.sqrt(count));
            var spacing = 2f;
            var offset = (gridSize * spacing) / 2f;
            
            for (int i = 0; i < count; i++)
            {
                var x = (i % gridSize) * spacing - offset;
                var z = (i / gridSize) * spacing - offset;
                var pos = center + new float3(x, 0f, z);
                
                var entity = ecb.CreateEntity();
                ecb.AddComponent(entity, LocalTransform.FromPosition(pos));
                ecb.AddComponent<LocalToWorld>(entity);
                
                // Minimal components for stress testing
                ecb.AddComponent(entity, new VillagerNeeds
                {
                    Health = 100f,
                    MaxHealth = 100f,
                    Energy = random.NextFloat(500f, 1000f),
                    Morale = random.NextFloat(500f, 1000f)
                });
                
                ecb.AddComponent(entity, new VillagerJob
                {
                    Type = (VillagerJob.JobType)random.NextInt(0, 6),
                    Phase = VillagerJob.JobPhase.Idle,
                    Productivity = 1f
                });
                
                ecb.AddComponent(entity, new VillagerAIState
                {
                    CurrentState = VillagerAIState.State.Idle,
                    CurrentGoal = VillagerAIState.Goal.None
                });
                
                ecb.AddComponent(entity, new VillagerFlags { IsIdle = true });
                ecb.AddComponent(entity, new DevSpawnedTag());
            }
            
            Debug.Log($"[DevMenu] Stress test: spawned {count} entities at {center}");
        }
    }
    
    /// <summary>
    /// Tag for entities spawned by dev tools (allows easy cleanup).
    /// </summary>
    public struct DevSpawnedTag : IComponentData { }
    
    /// <summary>
    /// Tag for enemy faction entities.
    /// </summary>
    public struct EnemyFactionTag : IComponentData { }
    
    /// <summary>
    /// Shared bridge data between MonoBehaviour and ECS.
    /// </summary>
    public class DevMenuECSBridge : System.IDisposable
    {
        public int TotalEntityCount;
        public int VillagerCount;
        public int VillageCount;
        public int BuildingCount;
        public int BandCount;
        
        public Queue<DevSpawnRequest> PendingSpawnRequests = new();
        
        public void RequestSpawn(DevSpawnType type, float3 position, int count, bool randomize = true, bool isFriendly = true)
        {
            PendingSpawnRequests.Enqueue(new DevSpawnRequest
            {
                SpawnType = type,
                Position = position,
                Count = count,
                Randomize = randomize,
                IsFriendly = isFriendly
            });
        }
        
        public void Dispose()
        {
            PendingSpawnRequests.Clear();
        }
    }
    
    public enum DevSpawnType
    {
        Villager,
        Village,
        Band,
        Army,
        Building,
        Resource,
        Creature,
        StressTest
    }
    
    public struct DevSpawnRequest
    {
        public DevSpawnType SpawnType;
        public float3 Position;
        public int Count;
        public bool Randomize;
        public bool IsFriendly;
    }
    
    /// <summary>
    /// System that cleans up dev-spawned entities on request.
    /// </summary>
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    public partial struct DevEntityCleanupSystem : ISystem
    {
        public static bool CleanupRequested;
        
        public void OnUpdate(ref SystemState state)
        {
            if (!CleanupRequested) return;
            CleanupRequested = false;
            
            var ecb = new EntityCommandBuffer(Allocator.Temp);
            
            foreach (var (tag, entity) in SystemAPI.Query<RefRO<DevSpawnedTag>>().WithEntityAccess())
            {
                ecb.DestroyEntity(entity);
            }
            
            ecb.Playback(state.EntityManager);
            ecb.Dispose();
            
            Debug.Log("[DevMenu] Cleaned up all dev-spawned entities.");
        }
    }
}
