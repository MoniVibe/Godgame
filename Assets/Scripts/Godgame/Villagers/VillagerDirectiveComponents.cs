using Unity.Entities;

namespace Godgame.Villagers
{
    /// <summary>
    /// High-level directive used to steer villager routines without hardcoding outcomes.
    /// </summary>
    public enum VillagerDirective : byte
    {
        None = 0,
        Work = 1,
        Rest = 2,
        Socialize = 3,
        Pray = 4,
        Roam = 5
    }

    /// <summary>
    /// Tracks the active directive and timing cadence for changes.
    /// </summary>
    public struct VillagerDirectiveState : IComponentData
    {
        public VillagerDirective Current;
        public VillagerDirective Previous;
        public uint LastChangeTick;
        public uint NextDecisionTick;
    }

    /// <summary>
    /// Weighted mix used to choose directives.
    /// </summary>
    [System.Serializable]
    public struct VillagerDirectiveWeights
    {
        public float Work;
        public float Rest;
        public float Social;
        public float Faith;
        public float Roam;

        public static VillagerDirectiveWeights Default => new VillagerDirectiveWeights
        {
            Work = 1.15f,
            Rest = 1f,
            Social = 1f,
            Faith = 0.75f,
            Roam = 0.6f
        };
    }

    /// <summary>
    /// Directive bias multipliers applied to villager need weights.
    /// </summary>
    [System.Serializable]
    public struct VillagerDirectiveBias : IComponentData
    {
        public float HungerWeight;
        public float RestWeight;
        public float FaithWeight;
        public float SafetyWeight;
        public float SocialWeight;
        public float WorkWeight;

        public static VillagerDirectiveBias Default => new VillagerDirectiveBias
        {
            HungerWeight = 1f,
            RestWeight = 1f,
            FaithWeight = 1f,
            SafetyWeight = 1f,
            SocialWeight = 1f,
            WorkWeight = 1f
        };
    }

    /// <summary>
    /// Optional per-villager directive profile for weighting and cadence.
    /// </summary>
    public struct VillagerDirectiveProfile : IComponentData
    {
        public VillagerDirectiveWeights Weights;
        public float Weight;
        public float MinDurationSeconds;
        public float MaxDurationSeconds;
    }

    /// <summary>
    /// Global directive tuning and per-directive bias defaults.
    /// </summary>
    public struct VillagerDirectiveConfig : IComponentData
    {
        public VillagerDirectiveWeights BaseWeights;
        public float MinDurationSeconds;
        public float MaxDurationSeconds;
        public float ChaosWeight;
        public float OrderWeight;
        public VillagerDirectiveBias WorkBias;
        public VillagerDirectiveBias RestBias;
        public VillagerDirectiveBias SocialBias;
        public VillagerDirectiveBias FaithBias;
        public VillagerDirectiveBias RoamBias;
        public VillagerDirectiveBias IdleBias;

        public static VillagerDirectiveConfig Default => new VillagerDirectiveConfig
        {
            BaseWeights = VillagerDirectiveWeights.Default,
            MinDurationSeconds = 20f,
            MaxDurationSeconds = 60f,
            ChaosWeight = 0.6f,
            OrderWeight = 0.6f,
            WorkBias = new VillagerDirectiveBias
            {
                HungerWeight = 1f,
                RestWeight = 0.9f,
                FaithWeight = 0.9f,
                SafetyWeight = 1f,
                SocialWeight = 0.8f,
                WorkWeight = 1.35f
            },
            RestBias = new VillagerDirectiveBias
            {
                HungerWeight = 1.05f,
                RestWeight = 1.35f,
                FaithWeight = 1f,
                SafetyWeight = 1f,
                SocialWeight = 0.9f,
                WorkWeight = 0.6f
            },
            SocialBias = new VillagerDirectiveBias
            {
                HungerWeight = 1f,
                RestWeight = 0.95f,
                FaithWeight = 0.9f,
                SafetyWeight = 1f,
                SocialWeight = 1.3f,
                WorkWeight = 0.75f
            },
            FaithBias = new VillagerDirectiveBias
            {
                HungerWeight = 1f,
                RestWeight = 0.95f,
                FaithWeight = 1.4f,
                SafetyWeight = 1f,
                SocialWeight = 0.85f,
                WorkWeight = 0.8f
            },
            RoamBias = new VillagerDirectiveBias
            {
                HungerWeight = 1f,
                RestWeight = 1.05f,
                FaithWeight = 0.95f,
                SafetyWeight = 1f,
                SocialWeight = 1.1f,
                WorkWeight = 0.75f
            },
            IdleBias = VillagerDirectiveBias.Default
        };
    }
}
