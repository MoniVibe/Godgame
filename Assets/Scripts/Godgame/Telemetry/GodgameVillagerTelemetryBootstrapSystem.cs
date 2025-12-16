using Godgame.Villagers;
using PureDOTS.Runtime.Telemetry;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

namespace Godgame.Telemetry
{
    /// <summary>
    /// Ensures every villager participating in the gather/deliver loop has telemetry attached.
    /// </summary>
    [BurstCompile]
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    public partial struct GodgameVillagerTelemetryBootstrapSystem : ISystem
    {
        private EntityQuery _missingTelemetryQuery;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            _missingTelemetryQuery = SystemAPI.QueryBuilder()
                .WithAll<VillagerJobState>()
                .WithNone<GatherDeliverTelemetry>()
                .Build();
            state.RequireForUpdate<VillagerJobState>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            if (_missingTelemetryQuery.IsEmptyIgnoreFilter)
            {
                return;
            }

            var ecb = new EntityCommandBuffer(Allocator.Temp);
            ecb.AddComponent(_missingTelemetryQuery, new GatherDeliverTelemetry());
            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state) { }
    }
}
