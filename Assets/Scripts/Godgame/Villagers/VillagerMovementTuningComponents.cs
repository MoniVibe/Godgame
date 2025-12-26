using Unity.Entities;

namespace Godgame.Villagers
{
    /// <summary>
    /// Tunables for villager walking/running cadence, stamina drain, and stat influence.
    /// </summary>
    public struct VillagerMovementTuning : IComponentData
    {
        public float WalkSpeedMultiplier;
        public float RunSpeedMultiplier;
        public float PatienceSpeedWeight;
        public float StaminaRunThreshold;
        public float StaminaDrainPerSecond;
        public float StaminaRecoverPerSecond;
        public float StatSpeedScalar;
        public float StatStaminaEfficiencyScalar;

        public static VillagerMovementTuning Default => new VillagerMovementTuning
        {
            WalkSpeedMultiplier = 0.7f,
            RunSpeedMultiplier = 1.35f,
            PatienceSpeedWeight = 1f,
            StaminaRunThreshold = 0.2f,
            StaminaDrainPerSecond = 1f,
            StaminaRecoverPerSecond = 1.5f,
            StatSpeedScalar = 0.003f,
            StatStaminaEfficiencyScalar = 0.01f
        };
    }
}
