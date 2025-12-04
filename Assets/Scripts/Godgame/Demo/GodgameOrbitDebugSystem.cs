using Unity.Entities;
using Unity.Transforms;
using Unity.Rendering;
using PureDOTS.Demo.Orbit;
using PureDOTS.Demo.Rendering;
using UnityEngine;

namespace Godgame
{
    /// <summary>
    /// Debug system that introspects orbit cube entities spawned by PureDOTS demo systems.
    /// Logs transform and render component state for orbit cubes.
    /// </summary>
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public partial struct GodgameOrbitDebugSystem : ISystem
    {
        private bool _loggedOnce;
        private bool _spawnedProxies;

        public void OnUpdate(ref SystemState state)
        {
#if GODGAME_DEBUG && UNITY_EDITOR
            if (!_spawnedProxies)
            {
                _spawnedProxies = true;
                int proxyCount = 0;
                foreach (var (xf, entity) in SystemAPI.Query<RefRO<LocalTransform>>()
                                                      .WithAll<OrbitCubeTag>()
                                                      .WithEntityAccess())
                {
                    var pos = xf.ValueRO.Position;
                    var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    go.name = $"ProxyCube_{entity.Index}";
                    go.transform.position = pos;
                    go.transform.localScale = Vector3.one;
                    proxyCount++;
                }
                if (proxyCount > 0)
                {
                    Godgame.GodgameDebug.Log($"[GodgameOrbitDebug] Spawned {proxyCount} proxy GameObjects for orbit cubes.");
                }
            }

            if (_loggedOnce)
                return;

            _loggedOnce = true;

            int count = 0;
            int meshCount = -1;
            int materialCount = -1;

            var rmaQuery = state.GetEntityQuery(ComponentType.ReadOnly<RenderMeshArraySingleton>());
            if (!rmaQuery.IsEmptyIgnoreFilter)
            {
                var rmaEntity = rmaQuery.GetSingletonEntity();
                var rmaSingleton = state.EntityManager.GetSharedComponentManaged<RenderMeshArraySingleton>(rmaEntity);
                var rma = rmaSingleton.Value;
                if (rma.Meshes != null) meshCount = rma.Meshes.Length;
                if (rma.Materials != null) materialCount = rma.Materials.Length;
            }

            foreach (var (xf, entity) in SystemAPI.Query<RefRO<LocalTransform>>()
                                                  .WithAll<OrbitCubeTag>()
                                                  .WithEntityAccess())
            {
                count++;
                var pos = xf.ValueRO.Position;
                var scale = xf.ValueRO.Scale;
                bool hasMMI = state.EntityManager.HasComponent<MaterialMeshInfo>(entity);
                bool hasRB = state.EntityManager.HasComponent<RenderBounds>(entity);

                if (hasMMI)
                {
                    var mmi = state.EntityManager.GetComponentData<MaterialMeshInfo>(entity);
                    Godgame.GodgameDebug.Log(
                        $"[GodgameOrbitDebug] Ent {entity.Index}: " +
                        $"Pos={pos}, Scale={scale}, " +
                        $"MMI={mmi}, " +
                        $"MeshCount={meshCount}, MaterialCount={materialCount}, " +
                        $"HasRenderBounds={hasRB}"
                    );
                }
                else
                {
                    Godgame.GodgameDebug.Log(
                        $"[GodgameOrbitDebug] Ent {entity.Index}: Pos={pos}, Scale={scale}, " +
                        $"NO MaterialMeshInfo, HasRenderBounds={hasRB}"
                    );
                }
            }

            if (count > 0)
            {
                Godgame.GodgameDebug.Log($"[GodgameOrbitDebug] Total OrbitCubeTag entities: {count}");
            }
#endif
        }
    }
}
