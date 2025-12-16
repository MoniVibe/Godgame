using PureDOTS.Runtime.AI;
using PureDOTS.Runtime.Components;
using PureDOTS.Runtime.Core;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;

namespace Godgame.AI
{
    /// <summary>
    /// Configures shared Mind cadence values for Godgame worlds using the behavior registry.
    /// </summary>
    [BurstCompile]
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    [UpdateAfter(typeof(BehaviorConfigBootstrapSystem))]
    public partial struct GodgameMindCadenceSetupSystem : ISystem
    {
        private const int DefaultMindCadenceTicks = 5;
        private const int DefaultAggregateCadenceTicks = 25;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<GameWorldTag>();
            state.RequireForUpdate<MindCadenceSettings>();
            state.RequireForUpdate<BehaviorConfigRegistry>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var mindConfig = SystemAPI.GetSingleton<BehaviorConfigRegistry>().Mind;
            var targetMindCadence = math.max(1, mindConfig.MindCadenceTicks > 0 ? mindConfig.MindCadenceTicks : DefaultMindCadenceTicks);
            var targetAggregateCadence = math.max(1, mindConfig.AggregateCadenceTicks > 0 ? mindConfig.AggregateCadenceTicks : DefaultAggregateCadenceTicks);

            var cadenceRW = SystemAPI.GetSingletonRW<MindCadenceSettings>();
            var current = cadenceRW.ValueRO;
            var changed = false;

            if (current.SensorCadenceTicks != targetMindCadence)
            {
                current.SensorCadenceTicks = targetMindCadence;
                changed = true;
            }

            if (current.EvaluationCadenceTicks != targetMindCadence)
            {
                current.EvaluationCadenceTicks = targetMindCadence;
                changed = true;
            }

            if (current.ResolutionCadenceTicks != targetMindCadence)
            {
                current.ResolutionCadenceTicks = targetMindCadence;
                changed = true;
            }

            if (current.AggregateCadenceTicks != targetAggregateCadence)
            {
                current.AggregateCadenceTicks = targetAggregateCadence;
                changed = true;
            }

            if (changed)
            {
                cadenceRW.ValueRW = current;
            }

            state.Enabled = false;
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state) { }
    }
}
