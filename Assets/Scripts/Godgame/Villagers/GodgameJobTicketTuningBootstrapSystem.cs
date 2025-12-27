using Unity.Burst;
using Unity.Entities;

namespace Godgame.Villagers
{
    /// <summary>
    /// Ensures default ticket tuning exists when no authoring is present.
    /// </summary>
    [BurstCompile]
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    public partial struct GodgameJobTicketTuningBootstrapSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            if (!SystemAPI.HasSingleton<GodgameJobTicketTuning>())
            {
                var entity = state.EntityManager.CreateEntity();
                state.EntityManager.AddComponentData(entity, GodgameJobTicketTuning.Default);
            }
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
        }
    }
}
