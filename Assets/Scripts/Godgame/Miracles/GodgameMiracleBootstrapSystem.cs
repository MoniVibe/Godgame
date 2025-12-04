using PureDOTS.Runtime.Components;
using Unity.Collections;
using Unity.Entities;

namespace Godgame.Miracles
{
    /// <summary>
    /// Creates singleton buffers required for miracle input/release flows
    /// (rain command queue and release event queue) so dependent systems can run.
    /// </summary>
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    public partial struct GodgameMiracleBootstrapSystem : ISystem
    {
        private EntityQuery _rainQueueQuery;
        private EntityQuery _releaseEventsQuery;

        public void OnCreate(ref SystemState state)
        {
            _rainQueueQuery = state.GetEntityQuery(ComponentType.ReadOnly<RainMiracleCommandQueue>());
            _releaseEventsQuery = state.GetEntityQuery(ComponentType.ReadWrite<MiracleReleaseEvent>());
        }

        public void OnUpdate(ref SystemState state)
        {
            var ecb = new EntityCommandBuffer(Allocator.Temp);

            if (_rainQueueQuery.IsEmptyIgnoreFilter)
            {
                var rainQueue = ecb.CreateEntity();
                ecb.AddComponent<RainMiracleCommandQueue>(rainQueue);
                ecb.AddBuffer<RainMiracleCommand>(rainQueue);
            }

            if (_releaseEventsQuery.IsEmptyIgnoreFilter)
            {
                var releaseEntity = ecb.CreateEntity();
                ecb.AddBuffer<MiracleReleaseEvent>(releaseEntity);
            }

            ecb.Playback(state.EntityManager);
            ecb.Dispose();
            state.Enabled = false;
        }
    }
}
