using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;

namespace Godgame.Rendering.Bootstrap
{
    /// <summary>
    /// Minimal ECS rendering bootstrap to validate Entities Graphics in Godgame.
    /// </summary>
    public class SimpleRenderAuthoring : MonoBehaviour
    {
        public Mesh mesh;
        public Material material;
        [Range(1, 128)] public int countX = 8;
        [Range(1, 128)] public int countZ = 8;
        public float spacing = 2f;

        private class Baker : Baker<SimpleRenderAuthoring>
        {
            public override void Bake(SimpleRenderAuthoring authoring)
            {
                if (authoring.mesh == null || authoring.material == null)
                    return;

                var renderMeshArray = new RenderMeshArray(new[] { authoring.material }, new[] { authoring.mesh });
                var mmi = MaterialMeshInfo.FromRenderMeshArrayIndices(0, 0);
                var meshBounds = authoring.mesh.bounds;
                var renderBounds = new RenderBounds
                {
                    Value = new AABB
                    {
                        Center = meshBounds.center,
                        Extents = meshBounds.extents
                    }
                };

                for (int x = 0; x < authoring.countX; x++)
                {
                    for (int z = 0; z < authoring.countZ; z++)
                    {
                        var entity = CreateAdditionalEntity(TransformUsageFlags.Renderable);
                        var position = new float3(x * authoring.spacing, 0f, z * authoring.spacing);

                        AddComponent(entity, LocalTransform.FromPosition(position));
                        AddSharedComponentManaged(entity, renderMeshArray);
                        AddComponent(entity, mmi);
                        AddComponent(entity, renderBounds);
                        AddComponent(entity, new WorldRenderBounds { Value = new AABB { Center = position + renderBounds.Value.Center, Extents = renderBounds.Value.Extents } });
                        AddComponent(entity, new ChunkWorldRenderBounds { Value = new AABB { Center = position + renderBounds.Value.Center, Extents = renderBounds.Value.Extents } });
                    }
                }
            }
        }
    }
}
