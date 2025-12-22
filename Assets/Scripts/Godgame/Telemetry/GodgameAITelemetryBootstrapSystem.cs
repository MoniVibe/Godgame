using Unity.Collections;
using Unity.Entities;
using VillagerAIState = PureDOTS.Runtime.Components.VillagerAIState;

namespace Godgame.Telemetry
{
    /// <summary>
    /// Ensures every villager has a GodgameAITelemetryState component before telemetry runs.
    /// </summary>
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    public partial struct GodgameAITelemetryBootstrapSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<VillagerAIState>();
        }

        public void OnUpdate(ref SystemState state)
        {
            var ecb = new EntityCommandBuffer(Allocator.Temp);

            foreach (var (aiState, entity) in SystemAPI
                         .Query<RefRO<VillagerAIState>>()
                         .WithNone<GodgameAITelemetryState>()
                         .WithEntityAccess())
            {
                ecb.AddComponent(entity, new GodgameAITelemetryState
                {
                    LastGoal = aiState.ValueRO.CurrentGoal,
                    LastGoalTick = 0,
                    LastRoleId = 0,
                    LastDoctrineId = 0,
                    LastProfileHash = 0,
                    ProfileLogged = 0
                });
            }

            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }
    }
}
