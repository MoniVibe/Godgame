using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

namespace Godgame.Villages
{
    /// <summary>
    /// Adds tree safety memory to villages for aggregate learning.
    /// </summary>
    [BurstCompile]
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    public partial struct VillageTreeSafetyBootstrapSystem : ISystem
    {
        private EntityQuery _missingMemory;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<Village>();
            _missingMemory = SystemAPI.QueryBuilder()
                .WithAll<Village>()
                .WithNone<VillageTreeSafetyMemory>()
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
            ecb.AddComponent(_missingMemory, new VillageTreeSafetyMemory
            {
                CautionBias = 0f,
                RecentSeverity = 0f,
                LastIncidentTick = 0u,
                IncidentCount = 0u
            });
            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }
    }
}
