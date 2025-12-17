using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using PureDOTS.Rendering;
using Godgame.Rendering;
using Godgame.Demo;
using Godgame.Presentation;
using static Godgame.Rendering.GodgamePresentationUtility;

namespace Godgame.Rendering.Systems
{
    /// <summary>
    /// Ensures demo entities carry RenderSemanticKey/RenderFlags so catalog application can bind meshes.
    /// </summary>
    [BurstCompile]
    [WorldSystemFilter(WorldSystemFilterFlags.Default)]
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public partial struct Godgame_DemoRenderKeyAssignSystem : ISystem
    {
        private EntityQuery _villageCenterQuery;
        private EntityQuery _resourceNodeQuery;
        private EntityQuery _villagerQuery;
        private EntityQuery _rockQuery;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<DemoResourceNode>();

            _villageCenterQuery = state.GetEntityQuery(new EntityQueryDesc
            {
                All = new[] { ComponentType.ReadOnly<VillageCenterPresentationTag>() },
                None = new[] { ComponentType.ReadOnly<RenderSemanticKey>() }
            });

            _resourceNodeQuery = state.GetEntityQuery(new EntityQueryDesc
            {
                All = new[] { ComponentType.ReadOnly<DemoResourceNode>() },
                None = new[] { ComponentType.ReadOnly<RenderSemanticKey>() }
            });

            _villagerQuery = state.GetEntityQuery(new EntityQueryDesc
            {
                All = new[] { ComponentType.ReadOnly<Godgame.Villagers.VillagerId>() },
                None = new[] { ComponentType.ReadOnly<RenderSemanticKey>() }
            });

            _rockQuery = state.GetEntityQuery(new EntityQueryDesc
            {
                All = new[] { ComponentType.ReadOnly<Godgame.Demo.RockTag>() },
                None = new[] { ComponentType.ReadOnly<RenderSemanticKey>() }
            });
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var em = state.EntityManager;

            using (var centers = _villageCenterQuery.ToEntityArray(Allocator.Temp))
            {
                foreach (var center in centers)
                {
                    AssignRenderComponents(em, center, GodgameRenderKeys.VillageCenter);
                }
            }

            using (var nodes = _resourceNodeQuery.ToEntityArray(Allocator.Temp))
            {
                foreach (var node in nodes)
                {
                    AssignRenderComponents(em, node, GodgameRenderKeys.ResourceNode);
                }
            }

            // Assign RenderSemanticKey to Villagers
            using (var villagers = _villagerQuery.ToEntityArray(Allocator.Temp))
            {
                foreach (var villager in villagers)
                {
                    var resolvedKey = VillagerRenderKeyUtility.ResolveVillagerRenderKey(em, villager);
                    AssignRenderComponents(em, villager, resolvedKey);
                }
            }

            // Assign RenderSemanticKey to Rocks (ResourceNodeTag)
            using (var rocks = _rockQuery.ToEntityArray(Allocator.Temp))
            {
                foreach (var rock in rocks)
                {
                    AssignRenderComponents(em, rock, GodgameRenderKeys.ResourceNode);
                }
            }
        }

        public void OnDestroy(ref SystemState state) { }
    }
}



