using Unity.Entities;
using Unity.Mathematics;

namespace Godgame.Resources
{
    /// <summary>
    /// Per-tree profile controlling felling hazard and work difficulty.
    /// </summary>
    public struct TreeFellingProfile : IComponentData
    {
        public float FallLength;
        public float FallWidth;
        public float BaseDamage;
        public float ChopDifficulty;
        public float AwarenessRadius;
    }

    /// <summary>
    /// Global tuning knobs for tree felling safety, speed, and learning.
    /// </summary>
    public struct TreeFellingTuning : IComponentData
    {
        public float BaseSafety;
        public float AlignmentSafetyWeight;
        public float BoldSafetyWeight;
        public float MemorySafetyWeight;
        public float MinSafetyFactor;
        public float MaxSafetyFactor;
        public float SafeSpeedMultiplier;
        public float RiskySpeedMultiplier;
        public float SafeWidthMultiplier;
        public float RiskyWidthMultiplier;
        public float SafeLengthMultiplier;
        public float RiskyLengthMultiplier;
        public float StrengthRateScalar;
        public float AgilityRateScalar;
        public float MemoryGainOnHit;
        public float MemoryGainOnNearMiss;
        public float MemoryDecayPerSecond;
        public float IncidentCooldownSeconds;
        public float NearMissRadiusMultiplier;
        public float VillageMemoryGain;

        public static TreeFellingTuning Default => new TreeFellingTuning
        {
            BaseSafety = 0.5f,
            AlignmentSafetyWeight = 0.25f,
            BoldSafetyWeight = 0.25f,
            MemorySafetyWeight = 0.4f,
            MinSafetyFactor = 0.05f,
            MaxSafetyFactor = 0.95f,
            SafeSpeedMultiplier = 0.85f,
            RiskySpeedMultiplier = 1.1f,
            SafeWidthMultiplier = 0.75f,
            RiskyWidthMultiplier = 1.25f,
            SafeLengthMultiplier = 0.9f,
            RiskyLengthMultiplier = 1.1f,
            StrengthRateScalar = 0.01f,
            AgilityRateScalar = 0.008f,
            MemoryGainOnHit = 0.35f,
            MemoryGainOnNearMiss = 0.15f,
            MemoryDecayPerSecond = 0.003f,
            IncidentCooldownSeconds = 1.5f,
            NearMissRadiusMultiplier = 1.4f,
            VillageMemoryGain = 0.2f
        };
    }

    /// <summary>
    /// Event emitted when a tree finishes falling so hazards can be resolved.
    /// </summary>
    public struct TreeFallEvent : IBufferElementData
    {
        public Entity TreeEntity;
        public Entity WorkerEntity;
        public float3 Position;
        public float3 FallDirection;
        public float FallLength;
        public float FallWidth;
        public float BaseDamage;
        public float AwarenessRadius;
        public float SafetyFactor;
        public uint TriggerTick;
    }

    /// <summary>
    /// Singleton tag for the tree fall event buffer.
    /// </summary>
    public struct TreeFallEventBuffer : IComponentData
    {
    }
}
