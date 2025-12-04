#if false
// DISABLED: Uses outdated Entities Graphics API (RenderMeshArraySingleton, MaterialMeshInfo manipulation)
// This system was modifying MaterialMeshInfo in a way that can cause batch index -1 errors
// TODO: Rewrite for URP + Entities Graphics 1.4 with correct APIs

using Unity.Entities;
using Unity.Transforms;
using Unity.Rendering;
using Unity.Collections;
using Unity.Mathematics;
using PureDOTS.Demo.Village;
using PureDOTS.Demo.Rendering;

namespace Godgame.Demo
{
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    [UpdateAfter(typeof(GodgameDemoRenderSetupSystem))]
    public partial struct FixVillageRenderingSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<RenderMeshArraySingleton>();
        }

        public void OnUpdate(ref SystemState state)
        {
            var em = state.EntityManager;
            var rmaQuery = SystemAPI.QueryBuilder().WithAll<RenderMeshArraySingleton>().Build();
            if (rmaQuery.IsEmpty) return;

            var rmaEntity = rmaQuery.GetSingletonEntity();
            var rmaSingleton = em.GetSharedComponentManaged<RenderMeshArraySingleton>(rmaEntity);
            var rma = rmaSingleton.Value;

            // Fix Villagers
            var villagerQuery = SystemAPI.QueryBuilder()
                .WithAll<VillagerTag>()
                .WithNone<MaterialMeshInfo>()
                .Build();

            if (!villagerQuery.IsEmpty)
            {
                var entities = villagerQuery.ToEntityArray(Allocator.Temp);
                // Index 3 is Villager (Blue)
                var mmi = MaterialMeshInfo.FromRenderMeshArrayIndices(3, 3);

                for (int i = 0; i < entities.Length; i++)
                {
                    var e = entities[i];
                    em.AddComponentData(e, new LocalToWorld { Value = float4x4.identity });
                    em.AddComponentData(e, mmi);
                    em.AddSharedComponentManaged(e, rma);
                    em.AddComponentData(e, new RenderBounds { Value = new AABB { Center = float3.zero, Extents = new float3(0.5f) } });
                }
                entities.Dispose();
                UnityEngine.Debug.Log($"[FixVillageRenderingSystem] Fixed {villagerQuery.CalculateEntityCount()} villagers.");
            }

            // Fix Homes
            var homeQuery = SystemAPI.QueryBuilder()
                .WithAll<HomeLot>()
                .WithNone<MaterialMeshInfo>()
                .Build();

            if (!homeQuery.IsEmpty)
            {
                var entities = homeQuery.ToEntityArray(Allocator.Temp);
                // Index 1 is Home (Red)
                var mmi = MaterialMeshInfo.FromRenderMeshArrayIndices(1, 1);

                for (int i = 0; i < entities.Length; i++)
                {
                    var e = entities[i];
                    em.AddComponentData(e, new LocalToWorld { Value = float4x4.identity });
                    em.AddComponentData(e, mmi);
                    em.AddSharedComponentManaged(e, rma);
                    em.AddComponentData(e, new RenderBounds { Value = new AABB { Center = float3.zero, Extents = new float3(0.5f) } });
                }
                entities.Dispose();
                UnityEngine.Debug.Log($"[FixVillageRenderingSystem] Fixed {homeQuery.CalculateEntityCount()} homes.");
            }

            // Fix Works
            var workQuery = SystemAPI.QueryBuilder()
                .WithAll<WorkLot>()
                .WithNone<MaterialMeshInfo>()
                .Build();

            if (!workQuery.IsEmpty)
            {
                var entities = workQuery.ToEntityArray(Allocator.Temp);
                // Index 2 is Work (Green)
                var mmi = MaterialMeshInfo.FromRenderMeshArrayIndices(2, 2);

                for (int i = 0; i < entities.Length; i++)
                {
                    var e = entities[i];
                    em.AddComponentData(e, new LocalToWorld { Value = float4x4.identity });
                    em.AddComponentData(e, mmi);
                    em.AddSharedComponentManaged(e, rma);
                    em.AddComponentData(e, new RenderBounds { Value = new AABB { Center = float3.zero, Extents = new float3(0.5f) } });
                }
                entities.Dispose();
                UnityEngine.Debug.Log($"[FixVillageRenderingSystem] Fixed {workQuery.CalculateEntityCount()} works.");
            }
        }
    }
}
#endif
