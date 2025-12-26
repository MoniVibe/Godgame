using PureDOTS.Runtime.Core;
using Unity.Burst;
using Unity.Entities;

namespace Godgame.Villagers
{
    /// <summary>
    /// Ensures a default villager schedule config exists in Godgame worlds.
    /// </summary>
    [BurstCompile]
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    public partial struct VillagerScheduleBootstrapSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<GameWorldTag>();

            if (!SystemAPI.HasSingleton<VillagerScheduleConfig>())
            {
                var entity = state.EntityManager.CreateEntity();
                state.EntityManager.AddComponentData(entity, VillagerScheduleConfig.Default);
            }
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state) { }
    }
}
