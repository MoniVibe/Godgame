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

        public float NeedMoveSpeed;
        public float NeedSatisfyRate;
        public float WorkSatisfactionPerDelivery;
        public float DeliberationMinSeconds;
        public float DeliberationMaxSeconds;
        public float PatienceCadenceBonusMax;

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
            NeedMoveSpeed = 2.2f,
            NeedSatisfyRate = 0.4f,
            WorkSatisfactionPerDelivery = 0.2f,
            DeliberationMinSeconds = 0.4f,
            DeliberationMaxSeconds = 1.6f,
            PatienceCadenceBonusMax = 6f
        };
    }
}
