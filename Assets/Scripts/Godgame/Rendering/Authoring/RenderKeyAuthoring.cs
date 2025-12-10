using UnityEngine;
using Unity.Entities;
using Unity.Transforms;
using PureDOTS.Rendering;

namespace Godgame.Rendering.Authoring
{
    public class RenderKeyAuthoring : MonoBehaviour
    {
        public ushort ArchetypeId = 110; // e.g. GodgameRenderKeys.VillageCenter
        public byte Lod           = 0;

        class Baker : Baker<RenderKeyAuthoring>
        {
            public override void Bake(RenderKeyAuthoring authoring)
            {
                // Dynamic → ends up in Default World
                var entity = GetEntity(TransformUsageFlags.Dynamic);

                AddComponent(entity, LocalTransform.FromPositionRotationScale(
                    authoring.transform.position,
                    authoring.transform.rotation,
                    1f
                ));

                AddComponent(entity, new RenderKey
                {
                    ArchetypeId = authoring.ArchetypeId,
                    LOD         = authoring.Lod
                });

                AddComponent(entity, new RenderFlags
                {
                    Visible       = 1,
                    ShadowCaster  = 1,
                    HighlightMask = 0
                });
            }
        }
    }
}
