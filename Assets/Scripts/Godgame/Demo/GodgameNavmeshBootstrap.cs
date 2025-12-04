using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace Godgame.Demo
{
    /// <summary>
    /// Lightweight runtime navmesh builder so the demo scene has walkable data even without manual editor baking.
    /// Collects mesh renderers/colliders within a configurable bounds and pushes them through NavMeshBuilder.
    /// </summary>
    public sealed class GodgameNavmeshBootstrap : MonoBehaviour
    {
        [SerializeField]
        private LayerMask includedLayers = ~0;

        [SerializeField]
        private NavMeshCollectGeometry collectGeometry = NavMeshCollectGeometry.RenderMeshes;

        [SerializeField]
        private Bounds navmeshBounds = new Bounds(Vector3.zero, new Vector3(200f, 20f, 200f));

        [SerializeField]
        private bool rebuildOnStart = true;

        NavMeshData _navMeshData;
        NavMeshDataInstance _navMeshInstance;

        void OnEnable()
        {
            if (rebuildOnStart)
            {
                BuildNavMesh();
            }
        }

        void OnDisable()
        {
            if (_navMeshInstance.valid)
            {
                _navMeshInstance.Remove();
            }
            _navMeshData = null;
        }

        [ContextMenu("Rebuild NavMesh")]
        public void BuildNavMesh()
        {
            var sources = ListPool<NavMeshBuildSource>.Get();
            var markups = ListPool<NavMeshBuildMarkup>.Get();
            var worldBounds = new Bounds(transform.TransformPoint(navmeshBounds.center), navmeshBounds.size);

            NavMeshBuilder.CollectSources(worldBounds, includedLayers, collectGeometry, 0, markups, sources);

            if (_navMeshData == null)
            {
                _navMeshData = new NavMeshData();
            }

            if (_navMeshInstance.valid)
            {
                _navMeshInstance.Remove();
            }

            NavMeshBuilder.UpdateNavMeshData(_navMeshData, NavMesh.GetSettingsByID(0), sources, worldBounds);
            _navMeshInstance = NavMesh.AddNavMeshData(_navMeshData);

            ListPool<NavMeshBuildSource>.Release(sources);
            ListPool<NavMeshBuildMarkup>.Release(markups);
        }

        static class ListPool<T>
        {
            static readonly Stack<List<T>> Pool = new Stack<List<T>>();

            public static List<T> Get()
            {
                return Pool.Count > 0 ? Pool.Pop() : new List<T>();
            }

            public static void Release(List<T> list)
            {
                list.Clear();
                Pool.Push(list);
            }
        }
    }
}
