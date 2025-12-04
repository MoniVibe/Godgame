using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace Godgame.Presentation
{
    /// <summary>
    /// Instantiates Unity GameObject visuals for entities that declare a <see cref="StaticVisualPrefab"/>.
    /// Keeps the GameObject poses in sync with the entity transforms each frame so the art follows DOTS data.
    /// </summary>
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    public sealed partial class StaticVisualRuntimeSystem : SystemBase
    {
        private readonly Dictionary<Entity, GameObject> _activeVisuals = new();
        private readonly List<Entity> _recycleBuffer = new();
        private readonly HashSet<Entity> _seenEntities = new();
        private Transform _runtimeAnchor;
        private EntityQuery _visualQuery;

        protected override void OnCreate()
        {
            _visualQuery = GetEntityQuery(new EntityQueryDesc
            {
                All = new[]
                {
                    ComponentType.ReadOnly<StaticVisualPrefab>(),
                    ComponentType.ReadOnly<StaticVisualPrefabReference>(),
                    ComponentType.ReadOnly<LocalToWorld>()
                }
            });

            RequireForUpdate(_visualQuery);
        }

        protected override void OnDestroy()
        {
            foreach (var kvp in _activeVisuals)
            {
                if (kvp.Value != null)
                {
                    Object.DestroyImmediate(kvp.Value);
                }
            }

            _activeVisuals.Clear();
            _recycleBuffer.Clear();

            if (_runtimeAnchor != null)
            {
                Object.DestroyImmediate(_runtimeAnchor.gameObject);
                _runtimeAnchor = null;
            }
        }

        protected override void OnUpdate()
        {
            EnsureAnchorExists();

            var entityManager = EntityManager;
            var entities = _visualQuery.ToEntityArray(Allocator.Temp);
            _seenEntities.Clear();

            foreach (var entity in entities)
            {
                _seenEntities.Add(entity);
                if (!_activeVisuals.TryGetValue(entity, out var visual) || visual == null)
                {
                    var prefabRef = entityManager.GetComponentData<StaticVisualPrefabReference>(entity);
                    if (!prefabRef.Prefab.IsValid())
                    {
                        continue;
                    }

                    var prefab = prefabRef.Prefab.Value;
                    if (prefab == null)
                    {
                        continue;
                    }

                    visual = Object.Instantiate(prefab, _runtimeAnchor);
                    visual.name = $"{prefab.name}_Visual";
                    _activeVisuals[entity] = visual;
                }

                SyncTransform(entity, visual.transform);
            }

            // Destroy visuals for entities that disappeared or lost the required components.
            foreach (var kvp in _activeVisuals)
            {
                if (!_seenEntities.Contains(kvp.Key) || !entityManager.Exists(kvp.Key))
                {
                    if (kvp.Value != null)
                    {
                        Object.Destroy(kvp.Value);
                    }

                    _recycleBuffer.Add(kvp.Key);
                }
            }

            for (int i = 0; i < _recycleBuffer.Count; i++)
            {
                _activeVisuals.Remove(_recycleBuffer[i]);
            }

            _recycleBuffer.Clear();
            entities.Dispose();
        }

        private void SyncTransform(Entity entity, Transform visualTransform)
        {
            var entityManager = EntityManager;

            if (!entityManager.HasComponent<LocalToWorld>(entity))
            {
                return;
            }

            var localToWorld = entityManager.GetComponentData<LocalToWorld>(entity);
            var visualData = entityManager.GetComponentData<StaticVisualPrefab>(entity);

            var position = localToWorld.Position;
            var rotation = localToWorld.Rotation;

            if (math.lengthsq(visualData.LocalOffset) > 1e-6f)
            {
                position += math.mul(rotation, visualData.LocalOffset);
            }

            rotation = math.mul(rotation, visualData.LocalRotationOffset);

            float scale = visualData.ScaleMultiplier;
            if (visualData.ShouldInheritScale && entityManager.HasComponent<LocalTransform>(entity))
            {
                var localTransform = entityManager.GetComponentData<LocalTransform>(entity);
                scale *= localTransform.Scale;
            }

            var unityPos = new Vector3(position.x, position.y, position.z);
            var unityRot = new Quaternion(rotation.value.x, rotation.value.y, rotation.value.z, rotation.value.w);

            visualTransform.SetPositionAndRotation(unityPos, unityRot);
            visualTransform.localScale = new Vector3(scale, scale, scale);
        }

        private void EnsureAnchorExists()
        {
            if (_runtimeAnchor != null)
            {
                return;
            }

            var anchorGO = new GameObject("[StaticVisuals]");
            Object.DontDestroyOnLoad(anchorGO);
            _runtimeAnchor = anchorGO.transform;
        }
    }
}
