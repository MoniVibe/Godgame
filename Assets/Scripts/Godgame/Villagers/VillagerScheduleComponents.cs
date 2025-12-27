using Unity.Entities;

namespace Godgame.Villagers
{
    /// <summary>
    /// Schedule tuning that biases villager needs based on time-of-day.
    /// Values are in normalized day fractions [0,1).
    /// </summary>
    public struct VillagerScheduleConfig : IComponentData
    {
        public float WorkStart01;
        public float WorkEnd01;
        public float SocialStart01;
        public float SocialEnd01;
        public float RestStart01;
        public float RestEnd01;

        public float WorkBias;
        public float SocialBias;
        public float RestBias;
        public float FaithBias;

        public float AdherenceMin;
        public float AdherenceMax;
        public float ScheduleOffsetMax01;
        public float ScheduleOffsetPatienceWeight;
        public float ScheduleOffsetOrderWeight;
        public float ScheduleChaosJitterMax01;

        public float NeedMoveSpeed;
        public float NeedSatisfyRate;
        public float WorkSatisfactionPerDelivery;
        public float WorkRecoveryPerSecond;
        public float WorkRecoveryOffHoursMultiplier;
        public float WorkCompletionWorkDrop;
        public float WorkCompletionRestBoost;
        public float WorkCompletionSocialBoost;
        public float WorkCompletionFaithBoost;
        public float DeliberationMinSeconds;
        public float DeliberationMaxSeconds;
        public float PatienceCadenceBonusMax;
        public float NeedWanderRadius;
        public float NeedSocialRadius;
        public float NeedLingerMinSeconds;
        public float NeedLingerMaxSeconds;
        public float NeedRepathMinSeconds;
        public float NeedRepathMaxSeconds;
        public float SocialPickMinSeconds;
        public float SocialPickMaxSeconds;
        public float SocialPreferRelationWeight;
        public float SocialPreferGoalBonus;

        public static VillagerScheduleConfig Default => new VillagerScheduleConfig
        {
            WorkStart01 = 0.22f,
            WorkEnd01 = 0.68f,
            SocialStart01 = 0.68f,
            SocialEnd01 = 0.82f,
            RestStart01 = 0.82f,
            RestEnd01 = 0.22f,
            WorkBias = 1.25f,
            SocialBias = 1.2f,
            RestBias = 1.2f,
            FaithBias = 1.05f,
            AdherenceMin = 0.45f,
            AdherenceMax = 1.0f,
            ScheduleOffsetMax01 = 0.06f,
            ScheduleOffsetPatienceWeight = 0.6f,
            ScheduleOffsetOrderWeight = 0.8f,
            ScheduleChaosJitterMax01 = 0.04f,
            NeedMoveSpeed = 0.19f,
            NeedSatisfyRate = 0.4f,
            WorkSatisfactionPerDelivery = 0.2f,
            WorkRecoveryPerSecond = 0.04f,
            WorkRecoveryOffHoursMultiplier = 1.5f,
            WorkCompletionWorkDrop = 0.25f,
            WorkCompletionRestBoost = 0.08f,
            WorkCompletionSocialBoost = 0.1f,
            WorkCompletionFaithBoost = 0.03f,
            DeliberationMinSeconds = 0.6f,
            DeliberationMaxSeconds = 2.4f,
            PatienceCadenceBonusMax = 6f,
            NeedWanderRadius = 6f,
            NeedSocialRadius = 9f,
            NeedLingerMinSeconds = 1.1f,
            NeedLingerMaxSeconds = 4f,
            NeedRepathMinSeconds = 1.6f,
            NeedRepathMaxSeconds = 5.5f,
            SocialPickMinSeconds = 4f,
            SocialPickMaxSeconds = 12f,
            SocialPreferRelationWeight = 0.35f,
            SocialPreferGoalBonus = 2.5f
        };
    }
}
