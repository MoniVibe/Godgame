using Godgame.Villagers;
using Unity.Entities;
using UnityEngine;

namespace Godgame.Authoring
{
    /// <summary>
    /// Authoring tunables for villager movement pacing.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class VillagerMovementTuningAuthoring : MonoBehaviour
    {
        [Header("Baseline")]
        [Range(0.05f, 0.5f)] public float baseMoveSpeedMultiplier = 0.09f;

        [Header("Speed Multipliers")]
        [Range(0.2f, 1f)] public float walkSpeedMultiplier = 0.7f;
        [Range(0.5f, 3f)] public float runSpeedMultiplier = 1.35f;

        [Header("Patience Influence")]
        [Range(0f, 1f)] public float patienceSpeedWeight = 1f;

        [Header("Stamina")]
        [Range(0f, 1f)] public float staminaRunThreshold = 0.2f;
        [Range(0f, 10f)] public float staminaDrainPerSecond = 1f;
        [Range(0f, 10f)] public float staminaRecoverPerSecond = 1.5f;

        [Header("Stat Scaling")]
        [Range(0f, 0.02f)] public float statSpeedScalar = 0.003f;
        [Range(0f, 0.02f)] public float statStaminaEfficiencyScalar = 0.01f;

        [Header("Organic Variance")]
        [Range(0f, 0.3f)] public float speedVarianceAmplitude = 0.08f;
        [Range(0f, 30f)] public float speedVariancePeriodSeconds = 6f;

        [Header("Arrive Slowdown")]
        [Range(0f, 8f)] public float arriveSlowdownRadius = 2.4f;
        [Range(0.1f, 1f)] public float arriveMinSpeedMultiplier = 0.35f;

        [Header("Separation")]
        [Range(0f, 4f)] public float separationRadius = 1.2f;
        [Range(0f, 2f)] public float separationWeight = 0.65f;
        [Range(0f, 2f)] public float separationMaxPush = 0.7f;
        [Range(0.5f, 6f)] public float separationCellSize = 1.8f;

        private sealed class Baker : Baker<VillagerMovementTuningAuthoring>
        {
            public override void Bake(VillagerMovementTuningAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.None);
                AddComponent(entity, new VillagerMovementTuning
                {
                    BaseMoveSpeedMultiplier = Mathf.Max(0.1f, authoring.baseMoveSpeedMultiplier),
                    WalkSpeedMultiplier = Mathf.Max(0.05f, authoring.walkSpeedMultiplier),
                    RunSpeedMultiplier = Mathf.Max(0.05f, authoring.runSpeedMultiplier),
                    PatienceSpeedWeight = Mathf.Clamp01(authoring.patienceSpeedWeight),
                    StaminaRunThreshold = Mathf.Clamp01(authoring.staminaRunThreshold),
                    StaminaDrainPerSecond = Mathf.Max(0f, authoring.staminaDrainPerSecond),
                    StaminaRecoverPerSecond = Mathf.Max(0f, authoring.staminaRecoverPerSecond),
                    StatSpeedScalar = Mathf.Max(0f, authoring.statSpeedScalar),
                    StatStaminaEfficiencyScalar = Mathf.Max(0f, authoring.statStaminaEfficiencyScalar),
                    SpeedVarianceAmplitude = Mathf.Max(0f, authoring.speedVarianceAmplitude),
                    SpeedVariancePeriodSeconds = Mathf.Max(0f, authoring.speedVariancePeriodSeconds),
                    ArriveSlowdownRadius = Mathf.Max(0f, authoring.arriveSlowdownRadius),
                    ArriveMinSpeedMultiplier = Mathf.Clamp(authoring.arriveMinSpeedMultiplier, 0.1f, 1f),
                    SeparationRadius = Mathf.Max(0f, authoring.separationRadius),
                    SeparationWeight = Mathf.Max(0f, authoring.separationWeight),
                    SeparationMaxPush = Mathf.Max(0f, authoring.separationMaxPush),
                    SeparationCellSize = Mathf.Max(0.1f, authoring.separationCellSize)
                });
            }
        }
    }
}
