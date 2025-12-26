using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

namespace Godgame.Villagers
{
    /// <summary>
    /// Ensures villagers have alignment data for relation and behavior systems.
    /// </summary>
    [BurstCompile]
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    public partial struct VillagerAlignmentBootstrapSystem : ISystem
    {
        private EntityQuery _missingAlignment;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<VillagerId>();
            _missingAlignment = SystemAPI.QueryBuilder()
                .WithAll<VillagerId>()
                .WithNone<VillagerAlignment>()
                .Build();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            if (_missingAlignment.IsEmptyIgnoreFilter)
            {
                return;
            }

            var ecb = new EntityCommandBuffer(Allocator.Temp);
            ecb.AddComponent(_missingAlignment, VillagerAlignment.Neutral);
            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }
    }
}
