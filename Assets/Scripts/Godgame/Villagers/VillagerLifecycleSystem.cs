using Godgame.Villages;
using PureDOTS.Runtime.AI;
using PureDOTS.Runtime.Components;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;

namespace Godgame.Villagers
{
    /// <summary>
    /// Advances villager age, life stage, and pregnancy progress.
    /// </summary>
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateBefore(typeof(VillageWorkforceAssignmentSystem))]
    public partial struct VillagerLifecycleSystem : ISystem
    {
        private ComponentLookup<VillagerReproductionState> _reproductionLookup;

        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<TimeState>();
            state.RequireForUpdate<VillagerLifecycleState>();
            _reproductionLookup = state.GetComponentLookup<VillagerReproductionState>(false);
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var timeState = SystemAPI.GetSingleton<TimeState>();
            if (timeState.IsPaused)
            {
                return;
            }

            if (SystemAPI.TryGetSingleton<RewindState>(out var rewind) && rewind.Mode != RewindMode.Record)
            {
                return;
            }

            var tuning = SystemAPI.HasSingleton<VillagerLifecycleTuning>()
                ? SystemAPI.GetSingleton<VillagerLifecycleTuning>()
                : new VillagerLifecycleTuning
                {
                    SecondsPerDay = 480f,
                    ChildStageDays = 10f,
                    YouthStageDays = 20f,
                    AdultStageDays = 60f,
                    ElderStageDays = 90f,
                    PregnancyDays = 3f,
                    LifecycleCadenceTicks = 30
                };

            var cadenceTicks = math.max(1, tuning.LifecycleCadenceTicks);
            if (!CadenceGate.ShouldRun(timeState.Tick, cadenceTicks))
            {
                return;
            }

            var secondsPerDay = math.max(1f, tuning.SecondsPerDay);
            var secondsPerTick = math.max(1e-4f, timeState.FixedDeltaTime);
            var pregnancyDays = math.max(0.1f, tuning.PregnancyDays);

            _reproductionLookup.Update(ref state);

            foreach (var (lifecycle, entity) in SystemAPI.Query<RefRW<VillagerLifecycleState>>().WithEntityAccess())
            {
                var lastTick = lifecycle.ValueRO.LastUpdateTick;
                if (lastTick == 0u)
                {
                    lifecycle.ValueRW.LastUpdateTick = timeState.Tick;
                    continue;
                }

                var deltaTicks = timeState.Tick > lastTick ? timeState.Tick - lastTick : 0u;
                if (deltaTicks == 0u)
                {
                    continue;
                }

                var deltaDays = (deltaTicks * secondsPerTick) / secondsPerDay;
                lifecycle.ValueRW.AgeDays += deltaDays;
                lifecycle.ValueRW.Stage = ResolveStage(lifecycle.ValueRW.AgeDays, in tuning);
                lifecycle.ValueRW.LastUpdateTick = timeState.Tick;

                if (_reproductionLookup.HasComponent(entity))
                {
                    var reproduction = _reproductionLookup[entity];
                    if (reproduction.IsPregnant != 0)
                    {
                        reproduction.PregnancyDays += deltaDays;
                        if (reproduction.PregnancyDays >= pregnancyDays)
                        {
                            reproduction.IsPregnant = 0;
                            reproduction.PregnancyDays = 0f;
                            reproduction.Partner = Entity.Null;
                            reproduction.BirthCount++;
                            if (reproduction.PendingBirths < byte.MaxValue)
                            {
                                reproduction.PendingBirths++;
                            }
                        }

                        _reproductionLookup[entity] = reproduction;
                    }
                }
            }
        }

        private static VillagerLifeStage ResolveStage(float ageDays, in VillagerLifecycleTuning tuning)
        {
            var childMax = math.max(0f, tuning.ChildStageDays);
            var youthMax = childMax + math.max(0f, tuning.YouthStageDays);
            var adultMax = youthMax + math.max(0f, tuning.AdultStageDays);

            if (ageDays < childMax) return VillagerLifeStage.Child;
            if (ageDays < youthMax) return VillagerLifeStage.Youth;
            if (ageDays < adultMax) return VillagerLifeStage.Adult;
            return VillagerLifeStage.Elder;
        }
    }
}
