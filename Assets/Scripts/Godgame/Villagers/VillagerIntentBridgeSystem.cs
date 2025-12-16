using PureDOTS.Runtime.AI;
using PureDOTS.Runtime.Combat;
using PureDOTS.Runtime.Components;
using PureDOTS.Runtime.Core;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Godgame.Villagers
{
    /// <summary>
    /// Bridges PureDOTS villager intents into Godgame's body/job systems while respecting cadence gates.
    /// </summary>
    [BurstCompile]
    [UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
    [UpdateBefore(typeof(VillagerJobSystem))]
    public partial struct VillagerIntentBridgeSystem : ISystem
    {
        private ComponentLookup<HazardAvoidanceState> _hazardLookup;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<GameWorldTag>();
            state.RequireForUpdate<MindCadenceSettings>();
            state.RequireForUpdate<VillagerGoalState>();
            state.RequireForUpdate<VillagerJobState>();
            state.RequireForUpdate<FocusBudget>();
            _hazardLookup = state.GetComponentLookup<HazardAvoidanceState>(true);
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            if (!SystemAPI.TryGetSingleton<TimeState>(out var timeState))
            {
                return;
            }

            if (SystemAPI.TryGetSingleton<RewindState>(out var rewind) && rewind.Mode != RewindMode.Record)
            {
                return;
            }

            var cadenceSettings = SystemAPI.GetSingleton<MindCadenceSettings>();
            _hazardLookup.Update(ref state);

            var job = new ApplyIntentJob
            {
                CurrentTick = timeState.Tick,
                DefaultCadence = math.max(1, cadenceSettings.EvaluationCadenceTicks),
                HazardLookup = _hazardLookup,
                FleeDistance = 18f,
                FleeSpeed = 7f
            };

            state.Dependency = job.ScheduleParallel(state.Dependency);
        }

        [BurstCompile]
        public partial struct ApplyIntentJob : IJobEntity
        {
            public uint CurrentTick;
            public int DefaultCadence;
            public float FleeDistance;
            public float FleeSpeed;
            [ReadOnly] public ComponentLookup<HazardAvoidanceState> HazardLookup;

            void Execute(
                Entity entity,
                ref VillagerJobState jobState,
                ref Navigation navigation,
                in VillagerGoalState goalState,
                in FocusBudget focusBudget,
                in VillagerFleeIntent fleeIntent,
                in VillagerMindCadence cadence,
                in LocalTransform transform)
            {
                var cadenceTicks = cadence.CadenceTicks > 0 ? cadence.CadenceTicks : DefaultCadence;
                if (!CadenceGate.ShouldRun(CurrentTick, cadenceTicks))
                {
                    return;
                }

                if (focusBudget.IsLocked != 0 || focusBudget.Current <= (focusBudget.Reserved + 0.01f))
                {
                    ResetJob(ref jobState);
                    return;
                }

                if (goalState.CurrentGoal == VillagerGoal.Flee)
                {
                    ResetJob(ref jobState);

                    if (fleeIntent.Urgency > 0.01f)
                    {
                        float3 hazardVector = float3.zero;
                        float hazardUrgency = 0f;
                        if (HazardLookup.HasComponent(entity))
                        {
                            var hazard = HazardLookup[entity];
                            hazardVector = hazard.CurrentAdjustment;
                            hazardUrgency = hazard.AvoidanceUrgency;
                        }

                        var combinedDirection = fleeIntent.ExitDirection + hazardVector * hazardUrgency;
                        navigation.Destination = VillagerSteeringMath.ComputeFleeDestination(transform.Position, combinedDirection, FleeDistance);
                        navigation.Speed = math.max(navigation.Speed, FleeSpeed);
                    }

                    return;
                }

                if (goalState.CurrentGoal != VillagerGoal.Work)
                {
                    ResetJob(ref jobState);
                }
            }

            private static void ResetJob(ref VillagerJobState jobState)
            {
                jobState.Phase = JobPhase.Idle;
                jobState.Target = Entity.Null;
            }
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state) { }
    }
}
