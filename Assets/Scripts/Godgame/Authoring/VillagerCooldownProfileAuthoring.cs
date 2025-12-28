using System;
using Godgame.Villagers;
using PureDOTS.Runtime.Identity;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Godgame.Authoring
{
    /// <summary>
    /// Authoring tunables for villager work cooldown and leisure pacing.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class VillagerCooldownProfileAuthoring : MonoBehaviour
    {
        [Header("Cooldown Ticks")]
        [Min(0f)] public int minCooldownTicks = 120;
        [Min(0f)] public int maxCooldownTicks = 240;

        [Header("Work Bias Curve")]
        public Vector2 workBiasToCooldownCurve = new Vector2(1f, 0f);

        [Header("Leisure Weights")]
        [Min(0f)] public float baseWanderWeight = 1f;
        [Min(0f)] public float baseSocializeWeight = 1f;
        [Min(0f)] public float needBiasWeight = 0.35f;
        [Min(0f)] public float orderAxisWeight = 0.25f;
        [Min(0f)] public float loyaltyWeight = 0.2f;
        [Min(0f)] public float patienceWeight = 0.2f;

        [Header("Leisure Movement Multipliers")]
        [Min(0f)] public float leisureMoveSpeedMultiplier = 0.65f;
        [Min(0f)] public float leisureWanderRadiusMultiplier = 0.6f;
        [Min(0f)] public float leisureSocialRadiusMultiplier = 0.8f;
        [Min(0f)] public float leisureTidyRadiusMultiplier = 0.45f;
        [Min(0f)] public float leisureObserveRadiusMultiplier = 0.35f;
        [Min(0f)] public float leisureLingerMinMultiplier = 1.4f;
        [Min(0f)] public float leisureLingerMaxMultiplier = 1.7f;
        [Min(0f)] public float leisureRepathMinMultiplier = 1.2f;
        [Min(0f)] public float leisureRepathMaxMultiplier = 1.4f;

        [Header("Leisure Cadence (Ticks)")]
        [Min(1f)] public int leisureCadenceMinTicks = 90;
        [Min(1f)] public int leisureCadenceMaxTicks = 180;
        [Min(0f)] public int leisureMinDwellTicks = 60;
        [Min(0f)] public float leisureArrivalDistance = 0.6f;
        [Min(0f)] public float leisureSocialTargetDistanceMultiplier = 1.25f;
        [Min(0f)] public float leisureTidyWeight = 0.35f;
        [Min(0f)] public float leisureObserveWeight = 0.25f;
        [Min(0f)] public float leisureCrowdingNeighborCap = 5f;
        [Min(0f)] public float leisureCrowdingPressureThreshold = 0.65f;

        [Header("Cooldown Pressure Guardrails")]
        [Min(0f)] public float pressureWorkUrgencyThreshold = 0.7f;
        [Min(0f)] public float pressureWorkUrgencyExitThreshold = 0.55f;
        [Min(0f)] public float pressureThreatUrgencyThreshold = 0.2f;
        [Min(0f)] public float pressureThreatUrgencyExitThreshold = 0.1f;
        [Min(0f)] public float pressureFoodRatioThreshold = 0.6f;
        [Min(0f)] public float pressureFoodRatioExitThreshold = 0.8f;
        [Min(0f)] public int pressureCooldownMaxRemainingTicks = 30;

        [Serializable]
        public struct OutlookRule
        {
            public OutlookType OutlookType;
            public float CooldownScale;
            public float SocializeWeight;
            public float WanderWeight;
            public float PressureEnterScale;
            public float PressureExitScale;
            public float CrowdingNeighborCapScale;
            public float CadenceScale;
        }

        [Serializable]
        public struct ArchetypeModifier
        {
            public string ArchetypeName;
            public float CooldownScale;
            public float PressureEnterScale;
            public float PressureExitScale;
            public float CrowdingNeighborCapScale;
            public float CadenceScale;
        }

        [Header("Outlook Rules")]
        public OutlookRule[] outlookRules = Array.Empty<OutlookRule>();

        [Header("Archetype Modifiers")]
        public ArchetypeModifier[] archetypeModifiers = Array.Empty<ArchetypeModifier>();

        private sealed class Baker : Baker<VillagerCooldownProfileAuthoring>
        {
            public override void Bake(VillagerCooldownProfileAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.None);
                var minTicks = (uint)math.max(0, authoring.minCooldownTicks);
                var maxTicks = (uint)math.max(0, authoring.maxCooldownTicks);

                AddComponent(entity, new VillagerCooldownProfile
                {
                    MinCooldownTicks = minTicks,
                    MaxCooldownTicks = maxTicks,
                    WorkBiasToCooldownCurve = new float2(authoring.workBiasToCooldownCurve.x, authoring.workBiasToCooldownCurve.y),
                    BaseWanderWeight = math.max(0f, authoring.baseWanderWeight),
                    BaseSocializeWeight = math.max(0f, authoring.baseSocializeWeight),
                    NeedBiasWeight = math.max(0f, authoring.needBiasWeight),
                    OrderAxisWeight = math.max(0f, authoring.orderAxisWeight),
                    LoyaltyWeight = math.max(0f, authoring.loyaltyWeight),
                    PatienceWeight = math.max(0f, authoring.patienceWeight),
                    LeisureMoveSpeedMultiplier = math.max(0f, authoring.leisureMoveSpeedMultiplier),
                    LeisureWanderRadiusMultiplier = math.max(0f, authoring.leisureWanderRadiusMultiplier),
                    LeisureSocialRadiusMultiplier = math.max(0f, authoring.leisureSocialRadiusMultiplier),
                    LeisureTidyRadiusMultiplier = math.max(0f, authoring.leisureTidyRadiusMultiplier),
                    LeisureObserveRadiusMultiplier = math.max(0f, authoring.leisureObserveRadiusMultiplier),
                    LeisureLingerMinMultiplier = math.max(0f, authoring.leisureLingerMinMultiplier),
                    LeisureLingerMaxMultiplier = math.max(0f, authoring.leisureLingerMaxMultiplier),
                    LeisureRepathMinMultiplier = math.max(0f, authoring.leisureRepathMinMultiplier),
                    LeisureRepathMaxMultiplier = math.max(0f, authoring.leisureRepathMaxMultiplier),
                    LeisureCadenceMinTicks = (uint)math.max(1, authoring.leisureCadenceMinTicks),
                    LeisureCadenceMaxTicks = (uint)math.max(1, authoring.leisureCadenceMaxTicks),
                    LeisureMinDwellTicks = (uint)math.max(0, authoring.leisureMinDwellTicks),
                    LeisureArrivalDistance = math.max(0f, authoring.leisureArrivalDistance),
                    LeisureSocialTargetDistanceMultiplier = math.max(0f, authoring.leisureSocialTargetDistanceMultiplier),
                    LeisureTidyWeight = math.max(0f, authoring.leisureTidyWeight),
                    LeisureObserveWeight = math.max(0f, authoring.leisureObserveWeight),
                    LeisureCrowdingNeighborCap = math.max(0f, authoring.leisureCrowdingNeighborCap),
                    LeisureCrowdingPressureThreshold = math.max(0f, authoring.leisureCrowdingPressureThreshold),
                    PressureWorkUrgencyThreshold = math.max(0f, authoring.pressureWorkUrgencyThreshold),
                    PressureWorkUrgencyExitThreshold = math.max(0f, authoring.pressureWorkUrgencyExitThreshold),
                    PressureThreatUrgencyThreshold = math.max(0f, authoring.pressureThreatUrgencyThreshold),
                    PressureThreatUrgencyExitThreshold = math.max(0f, authoring.pressureThreatUrgencyExitThreshold),
                    PressureFoodRatioThreshold = math.max(0f, authoring.pressureFoodRatioThreshold),
                    PressureFoodRatioExitThreshold = math.max(0f, authoring.pressureFoodRatioExitThreshold),
                    PressureCooldownMaxRemainingTicks = (uint)math.max(0, authoring.pressureCooldownMaxRemainingTicks)
                });

                var outlookBuffer = AddBuffer<VillagerCooldownOutlookRule>(entity);
                if (authoring.outlookRules != null)
                {
                    foreach (var rule in authoring.outlookRules)
                    {
                        if (rule.OutlookType == OutlookType.None)
                        {
                            continue;
                        }

                        outlookBuffer.Add(new VillagerCooldownOutlookRule
                        {
                            OutlookType = (byte)rule.OutlookType,
                            CooldownScale = math.max(0f, rule.CooldownScale),
                            SocializeWeight = rule.SocializeWeight,
                            WanderWeight = rule.WanderWeight,
                            PressureEnterScale = math.max(0f, rule.PressureEnterScale),
                            PressureExitScale = math.max(0f, rule.PressureExitScale),
                            CrowdingNeighborCapScale = math.max(0f, rule.CrowdingNeighborCapScale),
                            CadenceScale = math.max(0f, rule.CadenceScale)
                        });
                    }
                }

                var archetypeBuffer = AddBuffer<VillagerCooldownArchetypeModifier>(entity);
                if (authoring.archetypeModifiers != null)
                {
                    foreach (var modifier in authoring.archetypeModifiers)
                    {
                        if (string.IsNullOrEmpty(modifier.ArchetypeName))
                        {
                            continue;
                        }

                        var name = new FixedString64Bytes(modifier.ArchetypeName);
                        archetypeBuffer.Add(new VillagerCooldownArchetypeModifier
                        {
                            ArchetypeName = name,
                            CooldownScale = math.max(0f, modifier.CooldownScale),
                            PressureEnterScale = math.max(0f, modifier.PressureEnterScale),
                            PressureExitScale = math.max(0f, modifier.PressureExitScale),
                            CrowdingNeighborCapScale = math.max(0f, modifier.CrowdingNeighborCapScale),
                            CadenceScale = math.max(0f, modifier.CadenceScale)
                        });
                    }
                }
            }
        }
    }
}
