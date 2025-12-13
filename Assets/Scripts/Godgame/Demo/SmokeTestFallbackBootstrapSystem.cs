using Godgame.Resources;
using Godgame.Villages;
using Godgame.Villagers;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace Godgame.Demo
{
    /// <summary>
    /// Spawns a minimal set of gameplay entities when running headless without scene content.
    /// Helps smoke tests run even if the scene is empty.
    /// </summary>
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    public partial class SmokeTestFallbackBootstrapSystem : SystemBase
    {
        bool _hasRun;

        protected override void OnUpdate()
        {
            if (_hasRun)
            {
                Enabled = false;
                return;
            }

            if (!Application.isBatchMode)
            {
                Enabled = false;
                return;
            }

            using var villageQuery = SystemAPI.QueryBuilder().WithAll<Village>().Build();
            using var villagerQuery = SystemAPI.QueryBuilder().WithAll<VillagerNeeds>().Build();
            using var resourceQuery = SystemAPI.QueryBuilder().WithAll<ResourceSourceConfig>().Build();

            if (!villageQuery.IsEmptyIgnoreFilter &&
                !villagerQuery.IsEmptyIgnoreFilter &&
                !resourceQuery.IsEmptyIgnoreFilter)
            {
                Enabled = false;
                return;
            }

            var ecb = new EntityCommandBuffer(Allocator.Temp);

            SpawnVillage(ref ecb);
            SpawnVillagers(ref ecb);
            SpawnResources(ref ecb);

            ecb.Playback(EntityManager);
            ecb.Dispose();

            _hasRun = true;
            Enabled = false;
            Debug.Log("[SmokeTestFallbackBootstrapSystem] Spawned minimal fallback entities.");
        }

        static void SpawnVillage(ref EntityCommandBuffer ecb)
        {
            var entity = ecb.CreateEntity();
            ecb.AddComponent(entity, LocalTransform.FromPositionRotationScale(float3.zero, quaternion.identity, 1f));

            var id = new FixedString64Bytes("SMOKETEST_VILLAGE");
            ecb.AddComponent(entity, new Village
            {
                VillageId = id,
                VillageName = id,
                Phase = VillagePhase.Growing,
                CenterPosition = float3.zero,
                InfluenceRadius = 25f,
                MemberCount = 0,
                LastUpdateTick = 0
            });

            ecb.AddComponent(entity, new VillageState
            {
                CurrentState = VillageStateType.Nascent,
                StateEntryTick = 0,
                SurplusThreshold = 0f
            });
        }

        static void SpawnVillagers(ref EntityCommandBuffer ecb)
        {
            var positions = new[]
            {
                new float3(2f, 0f, 2f),
                new float3(-2f, 0f, -1f)
            };

            for (int i = 0; i < positions.Length; i++)
            {
                var entity = ecb.CreateEntity();
                ecb.AddComponent(entity, LocalTransform.FromPositionRotationScale(positions[i], quaternion.identity, 1f));
                ecb.AddComponent(entity, new VillagerNeeds
                {
                    Food = 80,
                    Rest = 80,
                    Sleep = 80,
                    GeneralHealth = 90,
                    Health = 90f,
                    MaxHealth = 100f,
                    Energy = 80f,
                    Morale = 75f
                });
            }
        }

        static void SpawnResources(ref EntityCommandBuffer ecb)
        {
            SpawnResourceNode(ref ecb, new float3(5f, 0f, 0f), "wood");
            SpawnResourceNode(ref ecb, new float3(-5f, 0f, 3f), "stone");
        }

        static void SpawnResourceNode(ref EntityCommandBuffer ecb, in float3 position, FixedString64Bytes resourceId)
        {
            var entity = ecb.CreateEntity();
            ecb.AddComponent(entity, LocalTransform.FromPositionRotationScale(position, quaternion.identity, 1f));
            ecb.AddComponent(entity, new ResourceSourceConfig
            {
                ResourceTypeId = resourceId,
                Amount = 100f,
                MaxAmount = 100f,
                RegenRate = 1f
            });
        }
    }
}
