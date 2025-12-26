using Godgame.Resources;
using Godgame.Villages;
using PureDOTS.Runtime.Components;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;

namespace Godgame.Villagers
{
    /// <summary>
    /// Gradually decays tree safety memories for villagers and villages.
    /// </summary>
    [BurstCompile]
    [UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
    [UpdateAfter(typeof(Godgame.Resources.TreeFallHazardSystem))]
    public partial struct VillagerTreeSafetyDecaySystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<TimeState>();
            state.RequireForUpdate<TreeFellingTuning>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            if (SystemAPI.TryGetSingleton<RewindState>(out var rewindState) && rewindState.Mode != RewindMode.Record)
            {
                return;
            }

            var timeState = SystemAPI.GetSingleton<TimeState>();
            var tuning = SystemAPI.GetSingleton<TreeFellingTuning>();
            if (tuning.MemoryDecayPerSecond <= 0f)
            {
                return;
            }

            var decay = tuning.MemoryDecayPerSecond * math.max(timeState.FixedDeltaTime, 1e-4f);
            if (decay <= 0f)
            {
                return;
            }

            foreach (var memory in SystemAPI.Query<RefRW<VillagerTreeSafetyMemory>>())
            {
                memory.ValueRW.CautionBias = math.max(0f, memory.ValueRW.CautionBias - decay);
                memory.ValueRW.RecentSeverity = math.max(0f, memory.ValueRW.RecentSeverity - decay);
            }

            foreach (var memory in SystemAPI.Query<RefRW<VillageTreeSafetyMemory>>())
            {
                memory.ValueRW.CautionBias = math.max(0f, memory.ValueRW.CautionBias - decay);
                memory.ValueRW.RecentSeverity = math.max(0f, memory.ValueRW.RecentSeverity - decay);
            }
        }
    }
}
