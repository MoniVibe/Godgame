using Godgame.Resources;
using PureDOTS.Runtime.Components;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

namespace Godgame.Systems.Resources
{
    /// <summary>
    /// Keeps SiphonSource data in sync with AggregatePile state for hand siphon affordances.
    /// </summary>
    [BurstCompile]
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(AggregatePileSystem))]
    public partial struct AggregatePileSiphonSourceSyncSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<AggregatePile>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var ecb = new EntityCommandBuffer(Allocator.Temp);

            foreach (var (pile, entity) in SystemAPI.Query<RefRO<AggregatePile>>()
                         .WithNone<SiphonSource>()
                         .WithEntityAccess())
            {
                ecb.AddComponent(entity, new SiphonSource
                {
                    ResourceTypeIndex = pile.ValueRO.ResourceTypeIndex,
                    Amount = pile.ValueRO.Amount,
                    MinChunkSize = 1f,
                    SiphonResistance = 0f
                });
            }

            foreach (var (pile, siphon) in SystemAPI.Query<RefRO<AggregatePile>, RefRW<SiphonSource>>())
            {
                var value = siphon.ValueRW;
                value.ResourceTypeIndex = pile.ValueRO.ResourceTypeIndex;
                value.Amount = pile.ValueRO.Amount;
                siphon.ValueRW = value;
            }

            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }
    }
}
