using PureDOTS.Runtime.Components;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

namespace Godgame.Construction
{
    /// <summary>
    /// Ensures construction ghosts expose dump affordances and intake data for divine hand feed.
    /// </summary>
    [BurstCompile]
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(ConstructionSystem))]
    public partial struct ConstructionHandIntakeSyncSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<ConstructionGhost>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var ecb = new EntityCommandBuffer(Allocator.Temp);

            foreach (var (ghost, entity) in SystemAPI.Query<RefRO<ConstructionGhost>>().WithEntityAccess())
            {
                if (!state.EntityManager.HasComponent<DumpTargetConstruction>(entity))
                {
                    ecb.AddComponent<DumpTargetConstruction>(entity);
                }

                if (!state.EntityManager.HasComponent<ConstructionIntake>(entity))
                {
                    ecb.AddComponent(entity, new ConstructionIntake
                    {
                        ResourceTypeIndex = ghost.ValueRO.ResourceTypeIndex,
                        Cost = ghost.ValueRO.Cost,
                        Paid = ghost.ValueRO.Paid
                    });
                }
                else
                {
                    var intake = state.EntityManager.GetComponentData<ConstructionIntake>(entity);
                    intake.ResourceTypeIndex = ghost.ValueRO.ResourceTypeIndex;
                    intake.Cost = ghost.ValueRO.Cost;
                    intake.Paid = ghost.ValueRO.Paid;
                    ecb.SetComponent(entity, intake);
                }
            }

            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }
    }
}
