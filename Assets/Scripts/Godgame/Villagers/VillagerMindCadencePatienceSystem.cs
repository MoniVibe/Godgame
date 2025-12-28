using PureDOTS.Runtime.AI;
using PureDOTS.Runtime.Components;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;

namespace Godgame.Villagers
{
    /// <summary>
    /// Scales mind cadence per villager using patience to introduce deliberation.
    /// </summary>
    [UpdateInGroup(typeof(VillagerMindSystemGroup))]
    [UpdateBefore(typeof(VillagerGoalSelectionSystem))]
    public partial struct VillagerMindCadencePatienceSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<MindCadenceSettings>();
            state.RequireForUpdate<VillagerMindCadence>();
            state.RequireForUpdate<VillagerScheduleConfig>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var cadenceSettings = SystemAPI.GetSingleton<MindCadenceSettings>();
            var schedule = SystemAPI.GetSingleton<VillagerScheduleConfig>();

            var baseCadence = math.max(1, cadenceSettings.EvaluationCadenceTicks);
            var bonusMax = math.max(0f, schedule.PatienceCadenceBonusMax);

            foreach (var (behavior, cadence) in SystemAPI.Query<RefRO<VillagerBehavior>, RefRW<VillagerMindCadence>>())
            {
                var patience01 = math.saturate((behavior.ValueRO.PatienceScore + 100f) * 0.005f);
                var bonus = (int)math.round(math.lerp(0f, bonusMax, patience01));
                cadence.ValueRW.CadenceTicks = math.max(1, baseCadence + bonus);
            }
        }
    }
}
