using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using UnityEngine;
using Godgame.Presentation;
using Unity.Rendering;

namespace Godgame.Debugging
{
    public class Coplay_EntityDebugger : MonoBehaviour
    {
        private bool _loggedOnce = false;

        void Start()
        {
#if GODGAME_DEBUG && UNITY_EDITOR
            UnityEngine.Debug.Log("[Coplay_EntityDebugger] Started.");
#endif
        }

        void Update()
        {
#if GODGAME_DEBUG && UNITY_EDITOR
            if (World.DefaultGameObjectInjectionWorld == null)
            {
                UnityEngine.Debug.LogWarning("[Coplay_EntityDebugger] World.DefaultGameObjectInjectionWorld is null.");
                return;
            }

            var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
            var query = entityManager.CreateEntityQuery(
                ComponentType.ReadOnly<LocalTransform>(),
                ComponentType.ReadOnly<VillagerPresentationTag>() 
            );

            var entities = query.ToEntityArray(Unity.Collections.Allocator.Temp);
            var transforms = query.ToComponentDataArray<LocalTransform>(Unity.Collections.Allocator.Temp);

            if (entities.Length > 0)
            {
                if (!_loggedOnce)
                {
                    UnityEngine.Debug.Log($"[Coplay_EntityDebugger] Found {entities.Length} villagers.");
                    for (int i = 0; i < entities.Length; i++)
                    {
                        var entity = entities[i];
                        bool hasMMI = entityManager.HasComponent<MaterialMeshInfo>(entity);
                        bool hasRB = entityManager.HasComponent<RenderBounds>(entity);
                        bool hasL2W = entityManager.HasComponent<LocalToWorld>(entity);
                        float scale = transforms[i].Scale;
                        
                        // Log first 5 only to avoid spam
                        if (i < 5)
                        {
                            UnityEngine.Debug.Log($"[Coplay_EntityDebugger] Villager {entity.Index}: Pos {transforms[i].Position}, Scale: {scale}, HasMMI: {hasMMI}, HasRB: {hasRB}, HasL2W: {hasL2W}");
                        }
                    }
                    _loggedOnce = true;
                }

                for (int i = 0; i < entities.Length; i++)
                {
                    // Draw debug line for all
                    UnityEngine.Debug.DrawLine(transforms[i].Position, transforms[i].Position + new float3(0, 2, 0), Color.red);
                }
            }
            else
            {
                if (!_loggedOnce) 
                {
                    UnityEngine.Debug.Log("[Coplay_EntityDebugger] No villagers found.");
                    _loggedOnce = true;
                }
            }

            entities.Dispose();
            transforms.Dispose();
#endif
        }
    }
}
