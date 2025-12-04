using PureDOTS.Runtime.Components;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;

namespace Godgame.Villagers
{
    /// <summary>
    /// Integrates villager needs into utility calculations for job selection.
    /// Extends PureDOTS utility scheduler with needs-based priority weighting.
    /// </summary>
    [BurstCompile]
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(VillagerNeedsSystem))]
    public partial struct VillagerUtilitySchedulerIntegration : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<TimeState>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var timeState = SystemAPI.GetSingleton<TimeState>();
            if (timeState.IsPaused)
            {
                return;
            }

            // Calculate utility weights based on needs
            foreach (var (needs, job, interrupt) in SystemAPI.Query<
                RefRO<VillagerNeeds>,
                RefRW<VillagerJob>,
                RefRO<VillagerInterrupt>>())
            {
                var needsValue = needs.ValueRO;
                var jobValue = job.ValueRO;
                var interruptValue = interrupt.ValueRO;

                // Skip if interrupted
                if (interruptValue.Type != VillagerInterruptType.None)
                {
                    continue;
                }

                // Calculate utility multiplier based on needs
                var energyPercent = math.saturate(needsValue.Energy / 1000f);
                var moralePercent = math.saturate(needsValue.Morale / 1000f);
                var healthPercent = needsValue.MaxHealth > 0f 
                    ? math.saturate(needsValue.Health / needsValue.MaxHealth) 
                    : 1f;

                // Low energy reduces productivity
                var productivityMultiplier = energyPercent;
                if (energyPercent < 0.3f)
                {
                    productivityMultiplier *= 0.5f; // 50% productivity when very tired
                }

                // Low morale reduces productivity
                productivityMultiplier *= math.lerp(0.7f, 1f, moralePercent);

                // Low health reduces productivity
                productivityMultiplier *= math.lerp(0.5f, 1f, healthPercent);

                // Update job productivity
                jobValue.Productivity = math.clamp(productivityMultiplier, 0f, 1f);
                job.ValueRW = jobValue;
            }
        }

        /// <summary>
        /// Calculates utility score for a job based on needs and job type.
        /// Higher utility = more likely to be selected.
        /// </summary>
        [BurstCompile]
        public static float CalculateJobUtility(
            VillagerJob.JobType jobType,
            float energyPercent,
            float moralePercent,
            float healthPercent,
            float distanceToTarget)
        {
            var baseUtility = 1f;

            // Energy requirements by job type
            var energyRequirement = jobType switch
            {
                VillagerJob.JobType.Gatherer => 0.4f,
                VillagerJob.JobType.Builder => 0.6f,
                VillagerJob.JobType.Guard => 0.5f,
                _ => 0.3f
            };

            // Penalize if energy is too low for this job
            if (energyPercent < energyRequirement)
            {
                baseUtility *= energyPercent / energyRequirement;
            }

            // Distance penalty (prefer closer jobs)
            var distancePenalty = math.saturate(distanceToTarget / 50f); // 50m = max penalty
            baseUtility *= (1f - distancePenalty * 0.3f);

            // Morale bonus (higher morale = more willing to work)
            baseUtility *= math.lerp(0.8f, 1.2f, moralePercent);

            return baseUtility;
        }
    }
}

