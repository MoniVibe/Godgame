using PureDOTS.Runtime.Components;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;

namespace Godgame.Bands
{
    /// <summary>
    /// Simulates morale-threshold obedience, covert/open mutiny phases, deception effects, and memory persistence/decay.
    /// </summary>
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(BandDoctrineGovernanceSystem))]
    public partial struct BandSocioDynamicsSystem : ISystem
    {
        private const float MoraleObedienceThreshold = 0.10f;
        private const float MoraleOpenMutinyThreshold = 0.05f;
        private const uint CovertCoordinationTicksThreshold = 24;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<TimeState>();
            state.RequireForUpdate<BandSocioProfile>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var tick = SystemAPI.GetSingleton<TimeState>().Tick;

            foreach (var (band, doctrineProfile, doctrineContext, social, socioProfile, discipline, climate, resourceMorality, orderEvents, intelReports, memoryEvents, disciplineEvents) in SystemAPI
                         .Query<
                             RefRW<Band>,
                             RefRO<BandDoctrineProfile>,
                             RefRW<BandDoctrineContext>,
                             RefRW<BandSocialState>,
                             RefRO<BandSocioProfile>,
                             RefRW<BandDisciplineState>,
                             RefRO<BandOrderClimate>,
                             RefRO<BandResourceMorality>,
                             DynamicBuffer<BandOrderEvent>,
                             DynamicBuffer<BandIntelReport>,
                             DynamicBuffer<BandMemoryEvent>,
                             DynamicBuffer<BandDisciplineEvent>>())
            {
                ProcessIntelReports(ref doctrineContext.ValueRW, socioProfile.ValueRO, intelReports, ref discipline.ValueRW);
                ApplyHardshipAndCaptainEffects(ref band.ValueRW, climate.ValueRO, resourceMorality.ValueRO, doctrineProfile.ValueRO);
                ProcessOrderEvents(
                    tick,
                    ref band.ValueRW,
                    ref doctrineContext.ValueRW,
                    ref social.ValueRW,
                    doctrineProfile.ValueRO,
                    socioProfile.ValueRO,
                    ref discipline.ValueRW,
                    climate.ValueRO,
                    orderEvents,
                    memoryEvents);
                EvaluateCompliance(tick, band.ValueRW.Morale, climate.ValueRO, socioProfile.ValueRO, ref discipline.ValueRW, disciplineEvents);
                AdvanceMemory(ref memoryEvents, socioProfile.ValueRO);
                ApplyDriftAndPressure(ref discipline.ValueRW, band.ValueRW.Morale, socioProfile.ValueRO, climate.ValueRO);
            }
        }

        private static void ProcessIntelReports(
            ref BandDoctrineContext context,
            in BandSocioProfile profile,
            DynamicBuffer<BandIntelReport> reports,
            ref BandDisciplineState discipline)
        {
            if (reports.Length == 0)
            {
                return;
            }

            var rumorSusceptibility = math.saturate(profile.RumorImpulse) * math.saturate(math.lerp(0.5f, 1f, profile.ChaosAxis));
            for (var i = 0; i < reports.Length; i++)
            {
                var report = reports[i];
                var qualityTrust = GetQualityTrust(report.Quality, rumorSusceptibility, report.EvidenceStrength);
                var deceptionAmplifier = math.saturate(report.DeceptionIntent) * math.max(0f, rumorSusceptibility - report.EvidenceStrength * 0.45f);
                var influence = math.saturate(qualityTrust + deceptionAmplifier * 0.6f);

                context.FrontThreat = math.saturate(context.FrontThreat + report.ThreatBias * influence);
                context.ObjectivePressure = math.saturate(context.ObjectivePressure + report.ObjectiveBias * influence);
                context.Criticality = math.saturate(context.Criticality + math.abs(report.ThreatBias) * influence * 0.2f);

                if (report.DeceptionIntent > 0.3f)
                {
                    discipline.CorruptionDrift = math.saturate(discipline.CorruptionDrift + report.BeneficiaryBias * 0.08f + deceptionAmplifier * 0.05f);
                }
            }

            reports.Clear();
        }

        private static float GetQualityTrust(BandIntelQuality quality, float rumorSusceptibility, float evidenceStrength)
        {
            switch (quality)
            {
                case BandIntelQuality.Verified:
                    return math.saturate(0.9f + evidenceStrength * 0.1f - rumorSusceptibility * 0.08f);
                case BandIntelQuality.Mixed:
                    return math.saturate(0.55f + evidenceStrength * 0.25f + rumorSusceptibility * 0.05f);
                default:
                    return math.saturate(0.2f + rumorSusceptibility * 0.6f - evidenceStrength * 0.2f);
            }
        }

        private static void ApplyHardshipAndCaptainEffects(
            ref Band band,
            in BandOrderClimate climate,
            in BandResourceMorality resourceMorality,
            in BandDoctrineProfile doctrineProfile)
        {
            var hardshipLoss = math.saturate(climate.HardshipPressure) * 0.05f;
            var captainPenalty = math.saturate(climate.CaptainPenaltyPressure) * 0.04f;
            var captainRelief = math.clamp(climate.CaptainDecisionImpact, -1f, 1f) * 0.04f;
            var allocationBias =
                math.saturate(resourceMorality.AllocationAuthoritarian) * math.saturate(doctrineProfile.AuthoritarianBias) * 0.02f -
                math.saturate(resourceMorality.AllocationEgalitarian) * math.saturate(doctrineProfile.EgalitarianBias) * 0.015f +
                math.saturate(resourceMorality.CorruptionOffset) * math.saturate(doctrineProfile.CorruptionBias) * 0.02f +
                math.saturate(resourceMorality.LogisticianAuthority) * math.saturate(climate.MasterLogisticianPressure) * 0.03f;

            var moraleDelta = -hardshipLoss - captainPenalty + captainRelief - allocationBias;
            band.Morale = math.saturate(band.Morale + moraleDelta);
        }

        private static void ProcessOrderEvents(
            uint tick,
            ref Band band,
            ref BandDoctrineContext context,
            ref BandSocialState social,
            in BandDoctrineProfile doctrineProfile,
            in BandSocioProfile socioProfile,
            ref BandDisciplineState discipline,
            in BandOrderClimate climate,
            DynamicBuffer<BandOrderEvent> orderEvents,
            DynamicBuffer<BandMemoryEvent> memoryEvents)
        {
            if (orderEvents.Length == 0)
            {
                return;
            }

            for (var i = 0; i < orderEvents.Length; i++)
            {
                var order = orderEvents[i];
                var moralePressure = math.max(0f, MoraleObedienceThreshold - band.Morale) * 6f;
                var divergence = math.saturate(order.Divergence + climate.CurrentOrderDivergence * 0.5f);
                var profileResistance =
                    math.saturate(socioProfile.ChaosAxis) * 0.25f +
                    (1f - math.saturate(socioProfile.InstitutionLoyalty)) * 0.25f +
                    discipline.Radicalization * 0.3f +
                    discipline.CorruptionDrift * 0.2f;

                var deviationScore = divergence + moralePressure * 0.12f + profileResistance;
                discipline.Radicalization = math.saturate(discipline.Radicalization + deviationScore * 0.025f);

                if (order.Type == BandOrderEventType.Execution)
                {
                    var sympathy = ComputeExecutionSympathy(doctrineProfile, socioProfile, discipline, order);
                    var penalty = math.saturate(order.Severity) * math.lerp(0.04f, 0.14f, sympathy);
                    if (order.IsPublic != 0)
                    {
                        penalty += 0.02f;
                    }

                    band.Morale = math.saturate(band.Morale - penalty);
                    social.CrewResentment = math.saturate(social.CrewResentment + penalty * math.lerp(0.7f, 1.2f, sympathy));
                    social.CommandTrust = math.saturate(social.CommandTrust - penalty * math.lerp(0.35f, 0.9f, sympathy));
                    context.ScrutinyPressure = math.saturate(context.ScrutinyPressure + penalty * 0.35f);
                    AddMemory(memoryEvents, BandMemoryType.Atrocity, penalty * 1.2f, 0f, BandMemoryFlags.Permanent | BandMemoryFlags.Hardened, tick);
                }
                else
                {
                    var grievance = math.saturate(order.Severity) * math.saturate(order.Divergence) * 0.08f;
                    band.Morale = math.saturate(band.Morale - grievance);
                    social.CrewResentment = math.saturate(social.CrewResentment + grievance * 0.8f);
                    AddMemory(memoryEvents, BandMemoryType.Grievance, grievance * 1.1f, 0.015f, BandMemoryFlags.None, tick);
                }
            }

            orderEvents.Clear();
        }

        private static float ComputeExecutionSympathy(
            in BandDoctrineProfile doctrineProfile,
            in BandSocioProfile socioProfile,
            in BandDisciplineState discipline,
            in BandOrderEvent order)
        {
            var egalitarianLift = math.saturate(doctrineProfile.EgalitarianBias) * 0.25f;
            var chaoticLift = math.saturate(socioProfile.ChaosAxis) * 0.22f;
            var mutinySympathy = math.saturate(socioProfile.MutinySympathy) * 0.28f;
            var resentmentLift = math.saturate(discipline.Radicalization) * 0.2f;
            var lawfulDamp = math.saturate(socioProfile.DueProcessPreference) * math.saturate(1f - socioProfile.ChaosAxis) * 0.12f;
            var authoritarianDamp = math.saturate(doctrineProfile.AuthoritarianBias) * 0.15f;
            var severityLift = math.saturate(order.Severity) * 0.1f;
            return math.saturate(egalitarianLift + chaoticLift + mutinySympathy + resentmentLift + severityLift - lawfulDamp - authoritarianDamp);
        }

        private static void EvaluateCompliance(
            uint tick,
            float morale,
            in BandOrderClimate climate,
            in BandSocioProfile socioProfile,
            ref BandDisciplineState discipline,
            DynamicBuffer<BandDisciplineEvent> disciplineEvents)
        {
            var previous = discipline.ComplianceState;

            if (morale > MoraleObedienceThreshold)
            {
                discipline.LowMoraleTicks = 0;
                discipline.SecretCoordination = math.max(0f, discipline.SecretCoordination - 0.04f);
                discipline.SplinterPressure = math.max(0f, discipline.SplinterPressure - 0.03f);
                discipline.ComplianceState = BandComplianceState.Obedient;
            }
            else
            {
                discipline.LowMoraleTicks++;
                var divergencePressure = math.saturate(climate.CurrentOrderDivergence + climate.AggregateGrievance * 0.5f);
                var harsherChecks = math.saturate((MoraleObedienceThreshold - morale) / MoraleObedienceThreshold);
                var radicalCheck =
                    divergencePressure * 0.45f +
                    discipline.Radicalization * 0.35f +
                    harsherChecks * 0.2f +
                    math.saturate(socioProfile.ChaosAxis) * 0.08f;

                discipline.SecretCoordination = math.saturate(discipline.SecretCoordination + math.max(0f, radicalCheck - 0.2f) * 0.06f);
                discipline.SplinterPressure = math.saturate(discipline.SplinterPressure + math.max(0f, radicalCheck - 0.3f) * 0.05f);

                if (morale <= MoraleOpenMutinyThreshold)
                {
                    discipline.ComplianceState = BandComplianceState.OpenMutiny;
                }
                else if (discipline.LowMoraleTicks >= CovertCoordinationTicksThreshold && discipline.SecretCoordination > 0.45f)
                {
                    discipline.ComplianceState = BandComplianceState.CovertCoordination;
                }
                else if (radicalCheck > 0.25f)
                {
                    discipline.ComplianceState = BandComplianceState.ItalianStrike;
                }
                else
                {
                    discipline.ComplianceState = BandComplianceState.Obedient;
                }
            }

            if (discipline.ComplianceState != previous)
            {
                discipline.LastTransitionTick = tick;
                disciplineEvents.Add(new BandDisciplineEvent
                {
                    State = discipline.ComplianceState,
                    TriggerScore = discipline.Radicalization,
                    MoraleAfter = morale,
                    Tick = tick
                });
            }
        }

        private static void ApplyDriftAndPressure(
            ref BandDisciplineState discipline,
            float morale,
            in BandSocioProfile profile,
            in BandOrderClimate climate)
        {
            var lowMorale = math.max(0f, MoraleObedienceThreshold - morale) * 6f;
            var divergence = math.saturate(climate.CurrentOrderDivergence + climate.AggregateGrievance * 0.4f);
            var driftPulse = lowMorale * 0.02f + divergence * 0.015f + discipline.Radicalization * 0.01f;

            discipline.OrderDrift = math.clamp(
                discipline.OrderDrift - driftPulse + math.saturate(profile.InstitutionLoyalty) * 0.01f,
                -1f,
                1f);

            discipline.CorruptionDrift = math.saturate(
                discipline.CorruptionDrift +
                driftPulse * 0.6f +
                math.max(0f, profile.NepotismTolerance - 0.4f) * 0.01f -
                math.saturate(profile.DueProcessPreference) * 0.008f);
        }

        private static void AdvanceMemory(
            ref DynamicBuffer<BandMemoryEvent> memoryEvents,
            in BandSocioProfile profile)
        {
            for (var i = memoryEvents.Length - 1; i >= 0; i--)
            {
                var memory = memoryEvents[i];
                if (IsPermanentMemory(memory.Type) || (memory.Flags & BandMemoryFlags.Permanent) != 0)
                {
                    memory.Flags |= BandMemoryFlags.Permanent;
                    memory.Flags |= BandMemoryFlags.Hardened;
                    memory.Salience = math.saturate(math.max(memory.Salience, 0.7f));
                    memoryEvents[i] = memory;
                    continue;
                }

                var legendBoost = math.saturate(memory.Legendization) * math.saturate(profile.PropagandaSusceptibility) * 0.02f;
                memory.Salience = math.saturate(memory.Salience + legendBoost - math.max(0.001f, memory.DecayRate));
                memory.Legendization = math.saturate(memory.Legendization + legendBoost * 0.5f);

                if (memory.Salience <= 0.01f)
                {
                    memoryEvents.RemoveAt(i);
                }
                else
                {
                    memoryEvents[i] = memory;
                }
            }
        }

        private static void AddMemory(
            DynamicBuffer<BandMemoryEvent> memoryEvents,
            BandMemoryType type,
            float salience,
            float decay,
            BandMemoryFlags flags,
            uint tick)
        {
            memoryEvents.Add(new BandMemoryEvent
            {
                Type = type,
                Salience = math.saturate(salience),
                DecayRate = math.max(0f, decay),
                Legendization = 0f,
                Flags = flags,
                Tick = tick
            });
        }

        private static bool IsPermanentMemory(BandMemoryType type)
        {
            switch (type)
            {
                case BandMemoryType.Betrayal:
                case BandMemoryType.Heroism:
                case BandMemoryType.Cowardice:
                case BandMemoryType.Enslavement:
                case BandMemoryType.Atrocity:
                case BandMemoryType.MajorLoss:
                case BandMemoryType.MajorVictory:
                    return true;
                default:
                    return false;
            }
        }
    }
}
