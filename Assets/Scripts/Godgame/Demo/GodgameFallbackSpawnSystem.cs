#if false // Legacy fallback replaced by SmokeTestFallbackBootstrapSystem (see SmokeTestFallbackBootstrapSystem.cs)
using Godgame.Demo;
using Godgame.Economy;
using Godgame.Rendering;
using PureDOTS.Runtime.Components;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using RenderKey = PureDOTS.Rendering.RenderKey;
using RenderFlags = PureDOTS.Rendering.RenderFlags;

namespace Godgame.Demo
{
    /// <summary>
    /// Headless/dev fallback that seeds a minimal village so smoketests never start empty.
    /// Only runs when explicitly enabled via scripting define or when in batchmode.
    /// </summary>
    [BurstCompile]
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    public partial struct GodgameFallbackSpawnSystem : ISystem
    {
        private EntityQuery _villagerQuery;
        private EntityQuery _villageQuery;
        private EntityQuery _resourceNodeQuery;
        private bool _spawned;

        public void OnCreate(ref SystemState state)
        {
            if (!IsFallbackEnabled())
            {
                state.Enabled = false;
                return;
            }

            state.RequireForUpdate<BeginSimulationEntityCommandBufferSystem.Singleton>();
            _villagerQuery = state.GetEntityQuery(ComponentType.ReadOnly<VillagerTag>());
            _villageQuery = state.GetEntityQuery(ComponentType.ReadOnly<VillageTag>());
            _resourceNodeQuery = state.GetEntityQuery(ComponentType.ReadOnly<GodgameDemoResourceNode>());
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            if (_spawned || !ShouldSpawnFallback())
            {
                state.Enabled = false;
                return;
            }

            var ecb = new EntityCommandBuffer(Allocator.Temp);
            SpawnVillageEntities(ecb);
            SpawnVillagers(ecb);
            SpawnResourceNodes(ecb);
            ecb.Playback(state.EntityManager);
            ecb.Dispose();

            _spawned = true;
            state.Enabled = false;

#if UNITY_EDITOR
            Debug.Log("[GodgameFallbackSpawnSystem] Spawned minimal smoketest setup.");
#endif
        }

        private bool ShouldSpawnFallback()
        {
            return _villagerQuery.IsEmptyIgnoreFilter &&
                   _villageQuery.IsEmptyIgnoreFilter &&
                   _resourceNodeQuery.IsEmptyIgnoreFilter;
        }

        private static void SpawnVillageEntities(EntityCommandBuffer ecb)
        {
            var village = ecb.CreateEntity();
            ecb.AddComponent(village, new VillageTag());
            ecb.AddComponent(
                village,
                LocalTransform.FromPositionRotationScale(
                    new float3(0f, 0f, 0f),
                    quaternion.identity,
                    6f));
            ecb.AddComponent(village, new RenderKey
            {
                ArchetypeId = GodgameRenderKeys.VillageCenter,
                LOD = 0
            });
            ecb.AddComponent(village, RenderFlagsVisible());
        }

        private static void SpawnVillagers(EntityCommandBuffer ecb)
        {
            for (int i = 0; i < 2; i++)
            {
                var angle = i == 0 ? 0f : math.PI;
                var homePos = new float3(math.cos(angle) * 4f, 0f, math.sin(angle) * 2f);
                var workPos = homePos + new float3(2f, 0f, 4f);
                var startPos = homePos + new float3(0f, 1.5f, 0f);

                var villager = ecb.CreateEntity();
                ecb.AddComponent(villager, LocalTransform.FromPositionRotationScale(startPos, quaternion.identity, 1.5f));
                ecb.AddComponent(villager, new VillagerTag());
                ecb.AddComponent(villager, new VillagerHome { Position = homePos });
                ecb.AddComponent(villager, new VillagerWork { Position = workPos });
                ecb.AddComponent(villager, new VillagerState { Phase = 0 });
                ecb.AddComponent(villager, new VillagerJob
                {
                    Type = VillagerJob.JobType.Gatherer,
                    Phase = VillagerJob.JobPhase.Idle,
                    Productivity = 1f,
                    ActiveTicketId = 0
                });
                ecb.AddComponent(villager, new VillagerNeeds
                {
                    Health = 100f,
                    MaxHealth = 100f,
                    Energy = 100f,
                    Morale = 100f
                });
                ecb.AddComponent(villager, new VillagerMood
                {
                    Band = VillagerMood.MoodBand.Stable,
                    WorkSpeedModifier = 1f,
                    InitiativeModifier = 1f,
                    FaithGainModifier = 1f,
                    BreakdownRisk = 0
                });
                ecb.AddComponent(villager, new VillagerAIState
                {
                    CurrentState = VillagerAIState.State.Idle,
                    CurrentGoal = VillagerAIState.Goal.Work,
                    TargetEntity = Entity.Null
                });
                ecb.AddComponent(villager, new VillagerAvailability
                {
                    IsAvailable = 1,
                    IsReserved = 0,
                    LastChangeTick = 0
                });
                ecb.AddComponent(villager, new VillagerFlags
                {
                    Value = 0,
                    IsIdle = true,
                    IsWorking = false
                });
                ecb.AddComponent(villager, RenderFlagsVisible());
                ecb.AddComponent(villager, new RenderKey
                {
                    ArchetypeId = GodgameRenderKeys.Villager,
                    LOD = 0
                });
            }
        }

        private static void SpawnResourceNodes(EntityCommandBuffer ecb)
        {
            var resourceTypes = new[] { ResourceType.Wood, ResourceType.Stone };

            for (int i = 0; i < resourceTypes.Length; i++)
            {
                var radius = 8f;
                var angle = (math.PI * 0.5f) + i * math.PI * 0.75f;
                var position = new float3(
                    math.cos(angle) * radius,
                    0f,
                    math.sin(angle) * radius);

                var node = ecb.CreateEntity();
                ecb.AddComponent(node, LocalTransform.FromPositionRotationScale(position, quaternion.identity, 1.25f));
                ecb.AddComponent(node, new GodgameDemoResourceNode
                {
                    Position = position,
                    ResourceType = resourceTypes[i],
                    Capacity = 50
                });
                ecb.AddComponent(node, new RenderKey
                {
                    ArchetypeId = GodgameRenderKeys.ResourceNode,
                    LOD = 0
                });
                ecb.AddComponent(node, RenderFlagsVisible());
            }
        }

        private static RenderFlags RenderFlagsVisible()
        {
            return new RenderFlags
            {
                Visible = 1,
                ShadowCaster = 1,
                HighlightMask = 0
            };
        }

        private static bool IsFallbackEnabled()
        {
#if SMOKETEST_FALLBACK_SPAWN
            return true;
#else
            return Application.isBatchMode;
#endif
        }
    }
}
#endif
