using PureDOTS.Runtime.AI;
using PureDOTS.Runtime.Components;
using Unity.Burst;
using Unity.Entities;

namespace Godgame.Villagers
{
    /// <summary>
    /// Clears expired work cooldowns.
    /// </summary>
    [BurstCompile]
    [UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
    [UpdateAfter(typeof(VillagerJobSystem))]
    [UpdateBefore(typeof(VillagerSocialFocusSystem))]
    public partial struct VillagerWorkCooldownSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<TimeState>();
            state.RequireForUpdate<VillagerWorkCooldown>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            if (!SystemAPI.TryGetSingleton<TimeState>(out var timeState) || timeState.IsPaused)
            {
                return;
            }

            if (SystemAPI.TryGetSingleton<RewindState>(out var rewindState) && rewindState.Mode != RewindMode.Record)
            {
                return;
            }

            var tick = timeState.Tick;

            foreach (var cooldown in SystemAPI.Query<RefRW<VillagerWorkCooldown>>())
            {
                var cooldownValue = cooldown.ValueRO;
                if (cooldownValue.EndTick == 0)
                {
                    continue;
                }

                if (tick >= cooldownValue.EndTick)
                {
                    cooldown.ValueRW = default;
                    continue;
                }
            }
        }
    }
}
