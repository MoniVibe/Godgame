using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

namespace Godgame.Villagers
{
    /// <summary>
    /// Adds tree safety memory to villagers so incidents can be tracked headlessly.
    /// </summary>
    [BurstCompile]
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    public partial struct VillagerTreeSafetyBootstrapSystem : ISystem
    {
        private EntityQuery _missingMemory;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<VillagerId>();
            _missingMemory = SystemAPI.QueryBuilder()
                .WithAll<VillagerId>()
                .WithNone<VillagerTreeSafetyMemory>()
                .Build();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            if (_missingMemory.IsEmptyIgnoreFilter)
            {
                state.Enabled = false;
                return;
            }

            var ecb = new EntityCommandBuffer(Allocator.Temp);
            ecb.AddComponent(_missingMemory, new VillagerTreeSafetyMemory
            {
                CautionBias = 0f,
                RecentSeverity = 0f,
                LastIncidentTick = 0u,
                NextIncidentAllowedTick = 0u,
                IncidentCount = 0,
                NearMissCount = 0
            });
            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }
    }
}
