using Unity.Entities;

namespace Godgame.Villagers
{
    /// <summary>
    /// Tunables for villager walking/running cadence, stamina drain, and stat influence.
    /// </summary>
    public struct VillagerMovementTuning : IComponentData
    {
        public float BaseMoveSpeedMultiplier;
        public float WalkSpeedMultiplier;
        public float RunSpeedMultiplier;
        public float PatienceSpeedWeight;
        public float StaminaRunThreshold;
        public float StaminaDrainPerSecond;
        public float StaminaRecoverPerSecond;
        public float StatSpeedScalar;
        public float StatStaminaEfficiencyScalar;
        public float SpeedVarianceAmplitude;
        public float SpeedVariancePeriodSeconds;
        public float ArriveSlowdownRadius;
        public float ArriveMinSpeedMultiplier;
        public float SeparationRadius;
        public float SeparationWeight;
        public float SeparationMaxPush;
        public float SeparationCellSize;

        public static VillagerMovementTuning Default => new VillagerMovementTuning
        {
            BaseMoveSpeedMultiplier = 0.085f,
            WalkSpeedMultiplier = 0.7f,
            RunSpeedMultiplier = 1.35f,
            PatienceSpeedWeight = 1f,
            StaminaRunThreshold = 0.2f,
            StaminaDrainPerSecond = 1f,
            StaminaRecoverPerSecond = 1.5f,
            StatSpeedScalar = 0.003f,
            StatStaminaEfficiencyScalar = 0.01f,
            SpeedVarianceAmplitude = 0.12f,
            SpeedVariancePeriodSeconds = 7f,
            ArriveSlowdownRadius = 2.4f,
            ArriveMinSpeedMultiplier = 0.35f,
            SeparationRadius = 1.2f,
            SeparationWeight = 0.65f,
            SeparationMaxPush = 0.7f,
            SeparationCellSize = 1.8f
        };
    }
}
