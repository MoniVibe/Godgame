using Godgame.Villagers;
using Unity.Entities;
using UnityEngine;

namespace Godgame.Authoring
{
    /// <summary>
    /// Authoring for per-villager schedule overrides.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class VillagerScheduleProfileAuthoring : MonoBehaviour
    {
        [Range(0f, 1f)] public float weight = 1f;

        [Header("Work Window")]
        [Range(0f, 1f)] public float workStart01 = 0.22f;
        [Range(0f, 1f)] public float workEnd01 = 0.68f;

        [Header("Social Window")]
        [Range(0f, 1f)] public float socialStart01 = 0.68f;
        [Range(0f, 1f)] public float socialEnd01 = 0.82f;

        [Header("Rest Window")]
        [Range(0f, 1f)] public float restStart01 = 0.82f;
        [Range(0f, 1f)] public float restEnd01 = 0.22f;

        [Header("Bias")]
        [Range(0f, 3f)] public float workBias = 1.25f;
        [Range(0f, 3f)] public float socialBias = 1.2f;
        [Range(0f, 3f)] public float restBias = 1.2f;
        [Range(0f, 3f)] public float faithBias = 1.05f;
        [Range(0f, 1f)] public float needInterruptUrgency = 0.7f;
        [Range(0f, 1f)] public float needInterruptWorkWeight = 0.35f;

        [Header("Adherence")]
        [Range(0f, 1f)] public float adherenceMin = 0.45f;
        [Range(0f, 1f)] public float adherenceMax = 1f;
        [Range(0f, 0.25f)] public float scheduleOffsetMax01 = 0.06f;
        [Range(0f, 1f)] public float scheduleOffsetPatienceWeight = 0.6f;
        [Range(0f, 1f)] public float scheduleOffsetOrderWeight = 0.8f;
        [Range(0f, 0.2f)] public float scheduleChaosJitterMax01 = 0.04f;

        [Header("Work Completion Effects")]
        [Range(0f, 1f)] public float workCompletionWorkDrop = 0.25f;
        [Range(0f, 1f)] public float workCompletionRestBoost = 0.08f;
        [Range(0f, 1f)] public float workCompletionSocialBoost = 0.1f;
        [Range(0f, 1f)] public float workCompletionFaithBoost = 0.03f;

        private sealed class Baker : Baker<VillagerScheduleProfileAuthoring>
        {
            public override void Bake(VillagerScheduleProfileAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.None);
                AddComponent(entity, new VillagerScheduleProfile
                {
                    Weight = Mathf.Clamp01(authoring.weight),
                    Schedule = new VillagerScheduleConfig
                    {
                        WorkStart01 = Mathf.Clamp01(authoring.workStart01),
                        WorkEnd01 = Mathf.Clamp01(authoring.workEnd01),
                        SocialStart01 = Mathf.Clamp01(authoring.socialStart01),
                        SocialEnd01 = Mathf.Clamp01(authoring.socialEnd01),
                        RestStart01 = Mathf.Clamp01(authoring.restStart01),
                        RestEnd01 = Mathf.Clamp01(authoring.restEnd01),
                        WorkBias = Mathf.Max(0f, authoring.workBias),
                        SocialBias = Mathf.Max(0f, authoring.socialBias),
                        RestBias = Mathf.Max(0f, authoring.restBias),
                        FaithBias = Mathf.Max(0f, authoring.faithBias),
                        NeedInterruptUrgency = Mathf.Clamp01(authoring.needInterruptUrgency),
                        NeedInterruptWorkWeight = Mathf.Clamp01(authoring.needInterruptWorkWeight),
                        AdherenceMin = Mathf.Clamp01(authoring.adherenceMin),
                        AdherenceMax = Mathf.Clamp01(authoring.adherenceMax),
                        ScheduleOffsetMax01 = Mathf.Max(0f, authoring.scheduleOffsetMax01),
                        ScheduleOffsetPatienceWeight = Mathf.Clamp01(authoring.scheduleOffsetPatienceWeight),
                        ScheduleOffsetOrderWeight = Mathf.Clamp01(authoring.scheduleOffsetOrderWeight),
                        ScheduleChaosJitterMax01 = Mathf.Max(0f, authoring.scheduleChaosJitterMax01),
                        NeedMoveSpeed = VillagerScheduleConfig.Default.NeedMoveSpeed,
                        NeedSatisfyRate = VillagerScheduleConfig.Default.NeedSatisfyRate,
                        WorkSatisfactionPerDelivery = VillagerScheduleConfig.Default.WorkSatisfactionPerDelivery,
                        WorkRecoveryPerSecond = VillagerScheduleConfig.Default.WorkRecoveryPerSecond,
                        WorkRecoveryOffHoursMultiplier = VillagerScheduleConfig.Default.WorkRecoveryOffHoursMultiplier,
                        WorkCompletionWorkDrop = Mathf.Max(0f, authoring.workCompletionWorkDrop),
                        WorkCompletionRestBoost = Mathf.Max(0f, authoring.workCompletionRestBoost),
                        WorkCompletionSocialBoost = Mathf.Max(0f, authoring.workCompletionSocialBoost),
                        WorkCompletionFaithBoost = Mathf.Max(0f, authoring.workCompletionFaithBoost),
                        DeliberationMinSeconds = VillagerScheduleConfig.Default.DeliberationMinSeconds,
                        DeliberationMaxSeconds = VillagerScheduleConfig.Default.DeliberationMaxSeconds,
                        PatienceCadenceBonusMax = VillagerScheduleConfig.Default.PatienceCadenceBonusMax,
                        NeedWanderRadius = VillagerScheduleConfig.Default.NeedWanderRadius,
                        NeedSocialRadius = VillagerScheduleConfig.Default.NeedSocialRadius,
                        NeedLingerMinSeconds = VillagerScheduleConfig.Default.NeedLingerMinSeconds,
                        NeedLingerMaxSeconds = VillagerScheduleConfig.Default.NeedLingerMaxSeconds,
                        NeedRepathMinSeconds = VillagerScheduleConfig.Default.NeedRepathMinSeconds,
                        NeedRepathMaxSeconds = VillagerScheduleConfig.Default.NeedRepathMaxSeconds,
                        SocialPickMinSeconds = VillagerScheduleConfig.Default.SocialPickMinSeconds,
                        SocialPickMaxSeconds = VillagerScheduleConfig.Default.SocialPickMaxSeconds,
                        SocialPreferRelationWeight = VillagerScheduleConfig.Default.SocialPreferRelationWeight,
                        SocialPreferGoalBonus = VillagerScheduleConfig.Default.SocialPreferGoalBonus
                    }
                });
            }
        }
    }
}
