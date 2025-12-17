using PureDOTS.Rendering;
using Unity.Entities;
using Unity.Mathematics;

namespace Godgame.Rendering
{
    internal static class GodgamePresentationUtility
    {
        internal struct PrefabPresentationState
        {
            public bool HasRenderKey;
            public bool HasRenderVariantKey;
            public bool HasSemanticKey;
            public bool HasRenderFlags;
            public bool HasMeshPresenter;
            public bool HasSpritePresenter;
            public bool HasDebugPresenter;
            public bool HasRenderTint;
            public bool HasRenderTexSlice;
            public bool HasRenderUv;
            public bool HasRenderThemeOverride;
        }

        internal static PrefabPresentationState GetPrefabPresentationState(EntityManager entityManager, Entity prefab)
        {
            if (prefab == Entity.Null)
                return default;

            return new PrefabPresentationState
            {
                HasRenderKey = entityManager.HasComponent<RenderKey>(prefab),
                HasRenderVariantKey = entityManager.HasComponent<RenderVariantKey>(prefab),
                HasSemanticKey = entityManager.HasComponent<RenderSemanticKey>(prefab),
                HasRenderFlags = entityManager.HasComponent<RenderFlags>(prefab),
                HasMeshPresenter = entityManager.HasComponent<MeshPresenter>(prefab),
                HasSpritePresenter = entityManager.HasComponent<SpritePresenter>(prefab),
                HasDebugPresenter = entityManager.HasComponent<DebugPresenter>(prefab),
                HasRenderTint = entityManager.HasComponent<RenderTint>(prefab),
                HasRenderTexSlice = entityManager.HasComponent<RenderTexSlice>(prefab),
                HasRenderUv = entityManager.HasComponent<RenderUvTransform>(prefab),
                HasRenderThemeOverride = entityManager.HasComponent<RenderThemeOverride>(prefab)
            };
        }

        internal static void AssignRenderComponents(
            ref EntityCommandBuffer ecb,
            Entity entity,
            ushort semanticKey,
            PrefabPresentationState prefabState)
        {
            AddOrSet(ref ecb, entity, new RenderKey { ArchetypeId = semanticKey, LOD = 0 }, prefabState.HasRenderKey);
            AddOrSet(ref ecb, entity, new RenderSemanticKey { Value = semanticKey }, prefabState.HasSemanticKey);
            AddOrSet(ref ecb, entity, new RenderVariantKey { Value = 0 }, prefabState.HasRenderVariantKey);
            AddOrSet(ref ecb, entity, new RenderFlags
            {
                Visible = 1,
                ShadowCaster = 1,
                HighlightMask = 0
            }, prefabState.HasRenderFlags);

            EnsurePresenter<MeshPresenter>(ref ecb, entity, prefabState.HasMeshPresenter, true);
            EnsurePresenter<SpritePresenter>(ref ecb, entity, prefabState.HasSpritePresenter, false);
            EnsurePresenter<DebugPresenter>(ref ecb, entity, prefabState.HasDebugPresenter, false);
            EnsureRenderThemeOverride(ref ecb, entity, prefabState.HasRenderThemeOverride, new RenderThemeOverride
            {
                Value = 0
            }, false);

            AddOrSet(ref ecb, entity, new RenderTint { Value = new float4(1f, 1f, 1f, 1f) }, prefabState.HasRenderTint);
            AddOrSet(ref ecb, entity, new RenderTexSlice { Value = 0 }, prefabState.HasRenderTexSlice);
            AddOrSet(ref ecb, entity, new RenderUvTransform { Value = new float4(1f, 1f, 0f, 0f) }, prefabState.HasRenderUv);
        }

        internal static void AssignRenderComponents(
            EntityManager entityManager,
            Entity entity,
            ushort semanticKey,
            PrefabPresentationState prefabState = default)
        {
            AddOrSet(entityManager, entity, new RenderKey { ArchetypeId = semanticKey, LOD = 0 }, prefabState.HasRenderKey);
            AddOrSet(entityManager, entity, new RenderSemanticKey { Value = semanticKey }, prefabState.HasSemanticKey);
            AddOrSet(entityManager, entity, new RenderVariantKey { Value = 0 }, prefabState.HasRenderVariantKey);
            AddOrSet(entityManager, entity, new RenderFlags
            {
                Visible = 1,
                ShadowCaster = 1,
                HighlightMask = 0
            }, prefabState.HasRenderFlags);

            EnsurePresenter<MeshPresenter>(entityManager, entity, prefabState.HasMeshPresenter, true);
            EnsurePresenter<SpritePresenter>(entityManager, entity, prefabState.HasSpritePresenter, false);
            EnsurePresenter<DebugPresenter>(entityManager, entity, prefabState.HasDebugPresenter, false);
            EnsureRenderThemeOverride(entityManager, entity, prefabState.HasRenderThemeOverride, new RenderThemeOverride
            {
                Value = 0
            }, false);

            AddOrSet(entityManager, entity, new RenderTint { Value = new float4(1f, 1f, 1f, 1f) }, prefabState.HasRenderTint);
            AddOrSet(entityManager, entity, new RenderTexSlice { Value = 0 }, prefabState.HasRenderTexSlice);
            AddOrSet(entityManager, entity, new RenderUvTransform { Value = new float4(1f, 1f, 0f, 0f) }, prefabState.HasRenderUv);
        }

        internal static void AddOrSet<T>(ref EntityCommandBuffer ecb, Entity entity, T value, bool hasComponent)
            where T : unmanaged, IComponentData
        {
            if (hasComponent)
            {
                ecb.SetComponent(entity, value);
            }
            else
            {
                ecb.AddComponent(entity, value);
            }
        }

        internal static void EnsurePresenter<T>(ref EntityCommandBuffer ecb, Entity entity, bool hasComponent, bool enabled)
            where T : unmanaged, IEnableableComponent
        {
            if (!hasComponent)
            {
                ecb.AddComponent<T>(entity);
            }
            ecb.SetComponentEnabled<T>(entity, enabled);
        }

        internal static void AddOrSet<T>(EntityManager entityManager, Entity entity, T value, bool hasComponent)
            where T : unmanaged, IComponentData
        {
            if (hasComponent)
            {
                entityManager.SetComponentData(entity, value);
            }
            else
            {
                entityManager.AddComponentData(entity, value);
            }
        }

        internal static void EnsurePresenter<T>(EntityManager entityManager, Entity entity, bool hasComponent, bool enabled)
            where T : unmanaged, IEnableableComponent
        {
            if (!hasComponent)
            {
                entityManager.AddComponent<T>(entity);
            }
            entityManager.SetComponentEnabled<T>(entity, enabled);
        }

        internal static void EnsureRenderThemeOverride(
            ref EntityCommandBuffer ecb,
            Entity entity,
            bool hasComponent,
            RenderThemeOverride value,
            bool enabled)
        {
            if (hasComponent)
            {
                ecb.SetComponent(entity, value);
            }
            else
            {
                ecb.AddComponent(entity, value);
            }

            ecb.SetComponentEnabled<RenderThemeOverride>(entity, enabled);
        }

        internal static void EnsureRenderThemeOverride(
            EntityManager entityManager,
            Entity entity,
            bool hasComponent,
            RenderThemeOverride value,
            bool enabled)
        {
            if (hasComponent)
            {
                entityManager.SetComponentData(entity, value);
            }
            else
            {
                entityManager.AddComponentData(entity, value);
            }

            entityManager.SetComponentEnabled<RenderThemeOverride>(entity, enabled);
        }
    }
}
