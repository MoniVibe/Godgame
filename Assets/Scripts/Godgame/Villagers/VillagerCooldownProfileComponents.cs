using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace Godgame.Villagers
{
    /// <summary>
    /// Data-driven cooldown tuning and leisure pacing.
    /// </summary>
    public struct VillagerCooldownProfile : IComponentData
    {
        public uint MinCooldownTicks;
        public uint MaxCooldownTicks;
        public float2 WorkBiasToCooldownCurve;

        public float BaseWanderWeight;
        public float BaseSocializeWeight;
        public float NeedBiasWeight;
        public float OrderAxisWeight;
        public float LoyaltyWeight;
        public float PatienceWeight;

        public float LeisureMoveSpeedMultiplier;
        public float LeisureWanderRadiusMultiplier;
        public float LeisureSocialRadiusMultiplier;
        public float LeisureTidyRadiusMultiplier;
        public float LeisureObserveRadiusMultiplier;
        public float LeisureLingerMinMultiplier;
        public float LeisureLingerMaxMultiplier;
        public float LeisureRepathMinMultiplier;
        public float LeisureRepathMaxMultiplier;
        public uint LeisureCadenceMinTicks;
        public uint LeisureCadenceMaxTicks;
        public uint LeisureMinDwellTicks;
        public float LeisureArrivalDistance;
        public float LeisureSocialTargetDistanceMultiplier;
        public float LeisureTidyWeight;
        public float LeisureObserveWeight;
        public float LeisureCrowdingNeighborCap;
        public float LeisureCrowdingPressureThreshold;
        public float PressureWorkUrgencyThreshold;
        public float PressureWorkUrgencyExitThreshold;
        public float PressureThreatUrgencyThreshold;
        public float PressureThreatUrgencyExitThreshold;
        public float PressureFoodRatioThreshold;
        public float PressureFoodRatioExitThreshold;
        public uint PressureCooldownMaxRemainingTicks;

        public static VillagerCooldownProfile Default => new VillagerCooldownProfile
        {
            MinCooldownTicks = 120,
            MaxCooldownTicks = 240,
            WorkBiasToCooldownCurve = new float2(1f, 0f),
            BaseWanderWeight = 1f,
            BaseSocializeWeight = 1f,
            NeedBiasWeight = 0.35f,
            OrderAxisWeight = 0.25f,
            LoyaltyWeight = 0.2f,
            PatienceWeight = 0.2f,
            LeisureMoveSpeedMultiplier = 0.65f,
            LeisureWanderRadiusMultiplier = 0.6f,
            LeisureSocialRadiusMultiplier = 0.8f,
            LeisureTidyRadiusMultiplier = 0.45f,
            LeisureObserveRadiusMultiplier = 0.35f,
            LeisureLingerMinMultiplier = 1.4f,
            LeisureLingerMaxMultiplier = 1.7f,
            LeisureRepathMinMultiplier = 1.2f,
            LeisureRepathMaxMultiplier = 1.4f,
            LeisureCadenceMinTicks = 90,
            LeisureCadenceMaxTicks = 180,
            LeisureMinDwellTicks = 60,
            LeisureArrivalDistance = 0.6f,
            LeisureSocialTargetDistanceMultiplier = 1.25f,
            LeisureTidyWeight = 0.35f,
            LeisureObserveWeight = 0.25f,
            LeisureCrowdingNeighborCap = 5f,
            LeisureCrowdingPressureThreshold = 0.65f,
            PressureWorkUrgencyThreshold = 0.7f,
            PressureWorkUrgencyExitThreshold = 0.55f,
            PressureThreatUrgencyThreshold = 0.2f,
            PressureThreatUrgencyExitThreshold = 0.1f,
            PressureFoodRatioThreshold = 0.6f,
            PressureFoodRatioExitThreshold = 0.8f,
            PressureCooldownMaxRemainingTicks = 30
        };

        public static uint ResolveCooldownTicks(in VillagerCooldownProfile profile, float workBias01, float cooldownScale)
        {
            var minTicks = profile.MinCooldownTicks;
            var maxTicks = math.max(profile.MaxCooldownTicks, minTicks);
            if (maxTicks == 0)
            {
                return 0;
            }

            var bias = math.saturate(workBias01);
            var curve = math.lerp(profile.WorkBiasToCooldownCurve.x, profile.WorkBiasToCooldownCurve.y, bias);
            curve = math.saturate(curve);
            var baseTicks = math.lerp(minTicks, maxTicks, curve);
            baseTicks *= math.max(0f, cooldownScale);

            var clamped = math.clamp(baseTicks, minTicks, maxTicks);
            return (uint)math.round(clamped);
        }
    }

    [InternalBufferCapacity(8)]
    public struct VillagerCooldownOutlookRule : IBufferElementData
    {
        public byte OutlookType;
        public float CooldownScale;
        public float SocializeWeight;
        public float WanderWeight;
        public float PressureEnterScale;
        public float PressureExitScale;
        public float CrowdingNeighborCapScale;
        public float CadenceScale;
    }

    [InternalBufferCapacity(4)]
    public struct VillagerCooldownArchetypeModifier : IBufferElementData
    {
        public FixedString64Bytes ArchetypeName;
        public float CooldownScale;
        public float PressureEnterScale;
        public float PressureExitScale;
        public float CrowdingNeighborCapScale;
        public float CadenceScale;
    }
}
