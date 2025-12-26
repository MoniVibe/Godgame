using Godgame.Resources;
using Unity.Entities;
using UnityEngine;

namespace Godgame.Authoring
{
    /// <summary>
    /// Authoring component for global tree felling tuning.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class TreeFellingTuningAuthoring : MonoBehaviour
    {
        [Header("Safety Bias")]
        [Range(0f, 1f)] public float baseSafety = 0.5f;
        [Range(0f, 1f)] public float alignmentSafetyWeight = 0.25f;
        [Range(0f, 1f)] public float boldSafetyWeight = 0.25f;
        [Range(0f, 1f)] public float memorySafetyWeight = 0.4f;
        [Range(0f, 1f)] public float minSafetyFactor = 0.05f;
        [Range(0f, 1f)] public float maxSafetyFactor = 0.95f;

        [Header("Speed Modifiers")]
        [Range(0.1f, 2f)] public float safeSpeedMultiplier = 0.85f;
        [Range(0.1f, 2f)] public float riskySpeedMultiplier = 1.1f;
        [Range(0f, 0.05f)] public float strengthRateScalar = 0.01f;
        [Range(0f, 0.05f)] public float agilityRateScalar = 0.008f;

        [Header("Hazard Shape")]
        [Range(0.1f, 3f)] public float safeWidthMultiplier = 0.75f;
        [Range(0.1f, 3f)] public float riskyWidthMultiplier = 1.25f;
        [Range(0.1f, 3f)] public float safeLengthMultiplier = 0.9f;
        [Range(0.1f, 3f)] public float riskyLengthMultiplier = 1.1f;
        [Range(0.1f, 3f)] public float nearMissRadiusMultiplier = 1.4f;

        [Header("Learning")]
        [Range(0f, 1f)] public float memoryGainOnHit = 0.35f;
        [Range(0f, 1f)] public float memoryGainOnNearMiss = 0.15f;
        [Range(0f, 1f)] public float villageMemoryGain = 0.2f;
        [Range(0f, 0.1f)] public float memoryDecayPerSecond = 0.003f;
        [Range(0f, 10f)] public float incidentCooldownSeconds = 1.5f;

        private sealed class Baker : Baker<TreeFellingTuningAuthoring>
        {
            public override void Bake(TreeFellingTuningAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.None);
                AddComponent(entity, new TreeFellingTuning
                {
                    BaseSafety = Mathf.Clamp01(authoring.baseSafety),
                    AlignmentSafetyWeight = Mathf.Clamp01(authoring.alignmentSafetyWeight),
                    BoldSafetyWeight = Mathf.Clamp01(authoring.boldSafetyWeight),
                    MemorySafetyWeight = Mathf.Clamp01(authoring.memorySafetyWeight),
                    MinSafetyFactor = Mathf.Clamp01(authoring.minSafetyFactor),
                    MaxSafetyFactor = Mathf.Clamp01(authoring.maxSafetyFactor),
                    SafeSpeedMultiplier = Mathf.Max(0.1f, authoring.safeSpeedMultiplier),
                    RiskySpeedMultiplier = Mathf.Max(0.1f, authoring.riskySpeedMultiplier),
                    StrengthRateScalar = Mathf.Max(0f, authoring.strengthRateScalar),
                    AgilityRateScalar = Mathf.Max(0f, authoring.agilityRateScalar),
                    SafeWidthMultiplier = Mathf.Max(0.1f, authoring.safeWidthMultiplier),
                    RiskyWidthMultiplier = Mathf.Max(0.1f, authoring.riskyWidthMultiplier),
                    SafeLengthMultiplier = Mathf.Max(0.1f, authoring.safeLengthMultiplier),
                    RiskyLengthMultiplier = Mathf.Max(0.1f, authoring.riskyLengthMultiplier),
                    NearMissRadiusMultiplier = Mathf.Max(0.1f, authoring.nearMissRadiusMultiplier),
                    MemoryGainOnHit = Mathf.Clamp01(authoring.memoryGainOnHit),
                    MemoryGainOnNearMiss = Mathf.Clamp01(authoring.memoryGainOnNearMiss),
                    MemoryDecayPerSecond = Mathf.Max(0f, authoring.memoryDecayPerSecond),
                    IncidentCooldownSeconds = Mathf.Max(0f, authoring.incidentCooldownSeconds),
                    VillageMemoryGain = Mathf.Clamp01(authoring.villageMemoryGain)
                });
            }
        }
    }
}
