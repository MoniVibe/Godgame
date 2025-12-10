using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using PureDOTS.Rendering;
using Godgame.Rendering;
using Godgame.Demo;
using Godgame.Presentation;

namespace Godgame.Rendering.Systems
{
    /// <summary>
    /// Ensures demo entities carry RenderKey/RenderFlags so catalog application can bind meshes.
    /// </summary>
    [BurstCompile]
    [WorldSystemFilter(WorldSystemFilterFlags.Default)]
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public partial struct Godgame_DemoRenderKeyAssignSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<DemoResourceNode>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var em = state.EntityManager;

            var centerQuery = SystemAPI.QueryBuilder()
                .WithAll<VillageCenterPresentationTag>()
                .WithNone<RenderKey>()
                .Build();

            using (var centers = centerQuery.ToEntityArray(Allocator.Temp))
            {
                foreach (var center in centers)
                {
                    em.AddComponentData(center, new RenderKey
                    {
                        ArchetypeId = GodgameRenderKeys.VillageCenter,
                        LOD = 0
                    });
                    em.AddComponentData(center, new RenderFlags
                    {
                        Visible = 1,
                        ShadowCaster = 1,
                        HighlightMask = 0
                    });
                }
            }

            var nodeQuery = SystemAPI.QueryBuilder()
                .WithAll<DemoResourceNode>()
                .WithNone<RenderKey>()
                .Build();

            using (var nodes = nodeQuery.ToEntityArray(Allocator.Temp))
            {
                foreach (var node in nodes)
                {
                    em.AddComponentData(node, new RenderKey
                    {
                        ArchetypeId = GodgameRenderKeys.ResourceNode,
                        LOD = 0
                    });
                    em.AddComponentData(node, new RenderFlags
                    {
                        Visible = 1,
                        ShadowCaster = 1,
                        HighlightMask = 0
                    });
                }
            }
        }

        public void OnDestroy(ref SystemState state) { }
    }
}




