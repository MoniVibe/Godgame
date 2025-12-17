using UnityEngine;
using Unity.Entities;
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
                // Other bakers (Mesh/Physics/Transform) already request the appropriate
                // transform usage, so we only need to tag the entity for rendering.
                var entity = GetEntity(TransformUsageFlags.None);

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
