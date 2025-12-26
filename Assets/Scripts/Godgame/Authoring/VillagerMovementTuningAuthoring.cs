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

        private sealed class Baker : Baker<VillagerMovementTuningAuthoring>
        {
            public override void Bake(VillagerMovementTuningAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.None);
                AddComponent(entity, new VillagerMovementTuning
                {
                    WalkSpeedMultiplier = Mathf.Max(0.05f, authoring.walkSpeedMultiplier),
                    RunSpeedMultiplier = Mathf.Max(0.05f, authoring.runSpeedMultiplier),
                    PatienceSpeedWeight = Mathf.Clamp01(authoring.patienceSpeedWeight),
                    StaminaRunThreshold = Mathf.Clamp01(authoring.staminaRunThreshold),
                    StaminaDrainPerSecond = Mathf.Max(0f, authoring.staminaDrainPerSecond),
                    StaminaRecoverPerSecond = Mathf.Max(0f, authoring.staminaRecoverPerSecond),
                    StatSpeedScalar = Mathf.Max(0f, authoring.statSpeedScalar),
                    StatStaminaEfficiencyScalar = Mathf.Max(0f, authoring.statStaminaEfficiencyScalar)
                });
            }
        }
    }
}
