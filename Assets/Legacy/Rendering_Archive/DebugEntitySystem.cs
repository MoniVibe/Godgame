#if LEGACY_RENDERING_ARCHIVE_DISABLED
using Godgame.Presentation;
using PureDOTS.Demo.Rendering;
using Unity.Burst;
using Unity.Entities;
using Unity.Entities.Graphics;
using Unity.Rendering;
using Unity.Transforms;

namespace Godgame.Demo
{
    [BurstCompile]
    [WorldSystemFilter(WorldSystemFilterFlags.Default)]
    [DisableAutoCreation]
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public partial struct DebugEntitySystem : ISystem
    {
        private bool _villagerDebugLogged;

        public void OnCreate(ref SystemState state) {}

        public void OnDestroy(ref SystemState state) {}

        public void OnUpdate(ref SystemState state)
        {
#if GODGAME_DEBUG && UNITY_EDITOR
            // General entity count logging
            int count = 0;
            foreach (var transform in SystemAPI.Query<RefRO<LocalTransform>>())
            {
                count++;
                if (count <= 10)
                {
                    Godgame.GodgameDebug.Log(
                        $"[DebugEntitySystem] Entity with LocalTransform at {transform.ValueRO.Position}"
                    );
                }
            }
            Godgame.GodgameDebug.Log($"[DebugEntitySystem] Total Entities with LocalTransform: {count}");

            // Villager-specific debug logging (run once)
            if (!_villagerDebugLogged)
            {
                _villagerDebugLogged = true;
                int villagerCount = 0;
                int withMaterialMeshInfo = 0;
                int withRenderBounds = 0;
                foreach (var (transform, tag, entity) in SystemAPI.Query<RefRO<LocalTransform>, RefRO<VillagerPresentationTag>>()
                    .WithEntityAccess())
                {
                    villagerCount++;
                    var pos = transform.ValueRO.Position;
                    bool hasMMI = state.EntityManager.HasComponent<MaterialMeshInfo>(entity);
                    bool hasRB = state.EntityManager.HasComponent<RenderBounds>(entity);
                    if (hasMMI) withMaterialMeshInfo++;
                    if (hasRB) withRenderBounds++;
                    
                    if (villagerCount <= 20) // Log first 20 villagers
                    {
                        Godgame.GodgameDebug.Log(
                            $"[VillagerDebug] Villager entity {entity.Index} at {pos}, HasMaterialMeshInfo={hasMMI}, HasRenderBounds={hasRB}"
                        );
                    }
                }
                Godgame.GodgameDebug.Log($"[VillagerDebug] Total Villagers: {villagerCount}, With MaterialMeshInfo: {withMaterialMeshInfo}, With RenderBounds: {withRenderBounds}");
                
                // Check RenderMeshArray singleton
                var rmaQuery = state.GetEntityQuery(ComponentType.ReadOnly<RenderMeshArraySingleton>());
                if (!rmaQuery.IsEmptyIgnoreFilter)
                {
                    var rmaEntity = rmaQuery.GetSingletonEntity();
                    var rmaSingleton = state.EntityManager.GetSharedComponentManaged<RenderMeshArraySingleton>(rmaEntity);
                    var rma = rmaSingleton.Value;
                    bool hasMeshes = rma.MeshReferences != null && rma.MeshReferences.Length > 0;
                    bool hasMaterials = rma.MaterialReferences != null && rma.MaterialReferences.Length > 0;
                    Godgame.GodgameDebug.Log($"[VillagerDebug] RenderMeshArray exists: Meshes={hasMeshes} ({rma.MeshReferences?.Length ?? 0}), Materials={hasMaterials} ({rma.MaterialReferences?.Length ?? 0})");
                }
                else
                {
                    Godgame.GodgameDebug.LogWarning("[VillagerDebug] RenderMeshArraySingleton not found!");
                }
            }
#endif
        }
    }
}
#endif
