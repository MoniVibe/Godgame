using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using PureDOTS.Rendering;

namespace Godgame.Rendering.Authoring
{
    public class RenderKeyAuthoring : MonoBehaviour
    {
        public ushort ArchetypeId = 110; // e.g. GodgameRenderKeys.VillageCenter

        class Baker : Baker<RenderKeyAuthoring>
        {
            public override void Bake(RenderKeyAuthoring authoring)
            {
                // Other bakers (Mesh/Physics/Transform) already request the appropriate
                // transform usage, so we only need to tag the entity for rendering.
                var entity = GetEntity(TransformUsageFlags.None);

                AddComponent(entity, new RenderKey
                {
                    ArchetypeId = authoring.ArchetypeId,
                    LOD = 0
                });

                AddComponent(entity, new RenderSemanticKey { Value = authoring.ArchetypeId });
                AddComponent(entity, new RenderVariantKey { Value = 0 });
                AddComponent<RenderThemeOverride>(entity);
                SetComponentEnabled<RenderThemeOverride>(entity, false);
                AddComponent<MeshPresenter>(entity);
                AddComponent<SpritePresenter>(entity);
                SetComponentEnabled<SpritePresenter>(entity, false);
                AddComponent<DebugPresenter>(entity);
                SetComponentEnabled<DebugPresenter>(entity, false);

                AddComponent(entity, new RenderFlags
                {
                    Visible       = 1,
                    ShadowCaster  = 1,
                    HighlightMask = 0
                });

                AddComponent(entity, new RenderTint { Value = new float4(1f) });
                AddComponent(entity, new RenderTexSlice { Value = 0 });
                AddComponent(entity, new RenderUvTransform { Value = new float4(1f, 1f, 0f, 0f) });
            }
        }
    }
}
