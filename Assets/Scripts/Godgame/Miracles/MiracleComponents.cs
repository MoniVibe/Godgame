using PureDOTS.Runtime.Components;
using Unity.Entities;
using Unity.Mathematics;

namespace Godgame.Miracles
{
    /// <summary>
    /// Godgame-specific miracle token component representing an active miracle instance.
    /// Bridges to PureDOTS MiracleDefinition and MiracleRuntimeState components.
    /// </summary>
    public struct MiracleToken : IComponentData
    {
        public MiracleType Type;
        public MiracleCastingMode CastingMode;
        public float BaseRadius;
        public float BaseIntensity;
        public float BaseCost;
        public float SustainedCostPerSecond;
        public MiracleLifecycleState Lifecycle;
        public float ChargePercent;
        public float CurrentRadius;
        public float CurrentIntensity;
        public float CooldownSecondsRemaining;
        public uint LastCastTick;
        public byte AlignmentDelta;
        
        // Godgame-specific fields
        public Entity CasterHand;      // Divine hand that cast this miracle
        public float3 CastPosition;   // Position where miracle was cast
        public float PrayerCost;       // Prayer/faith cost (Godgame-specific resource)
        public float AlignmentRequirement; // Minimum alignment required to cast
        public uint DurationTicks;      // How long the miracle lasts (for sustained/instant types)
    }
}

