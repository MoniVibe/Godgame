using PureDOTS.Runtime.Components;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;

namespace Godgame.Bands
{
    /// <summary>
    /// Selects the active formation doctrine by combining profile bias, tactical context, and module weights.
    /// </summary>
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(BandFormationSystem))]
    public partial struct BandDoctrineSelectionSystem : ISystem
    {
        private const float CooldownRecoveryPerTick = 0.02f;
        private const float SwitchPenaltyApplied = 0.2f;
        private const float StickinessBonus = 0.08f;

        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<TimeState>();
            state.RequireForUpdate<BandDoctrineWeight>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var tick = SystemAPI.GetSingleton<TimeState>().Tick;

            foreach (var (band, profile, context, weights, selection) in SystemAPI
                         .Query<RefRO<Band>, RefRO<BandDoctrineProfile>, RefRW<BandDoctrineContext>, DynamicBuffer<BandDoctrineWeight>, RefRW<BandDoctrineSelection>>())
            {
                BandDoctrineBootstrapSystem.EnsureModuleCoverage(weights);
                RecoverCooldownPenalties(weights);

                var abstractionSignal = ComputeAbstractionSignal(band.ValueRO, profile.ValueRO, context.ValueRO);
                var abstractionConfidence = math.saturate(abstractionSignal);
                var communicationSuppression = abstractionConfidence * math.saturate(profile.ValueRO.CommunicationCompression);
                var abstractedControl = communicationSuppression >= 0.35f ? (byte)1 : (byte)0;

                var bestScore = float.MinValue;
                var runnerUpScore = float.MinValue;
                var bestModule = selection.ValueRO.ActiveModule;
                var previousModule = selection.ValueRO.ActiveModule;

                for (var i = 0; i < weights.Length; i++)
                {
                    var moduleWeight = weights[i];
                    if (moduleWeight.Enabled == 0)
                    {
                        continue;
                    }

                    var score = ScoreModule(moduleWeight.Module, band.ValueRO, profile.ValueRO, context.ValueRO, moduleWeight);
                    if (moduleWeight.Module == previousModule)
                    {
                        score += StickinessBonus;
                    }

                    if (abstractedControl == 1)
                    {
                        score += ScoreAbstractionAffinity(moduleWeight.Module, communicationSuppression);
                    }

                    if (score > bestScore)
                    {
                        runnerUpScore = bestScore;
                        bestScore = score;
                        bestModule = moduleWeight.Module;
                    }
                    else if (score > runnerUpScore)
                    {
                        runnerUpScore = score;
                    }
                }

                if (bestModule != previousModule)
                {
                    ApplySwitchPenalty(weights, previousModule);
                }

                selection.ValueRW = new BandDoctrineSelection
                {
                    ActiveModule = bestModule,
                    PreviousModule = previousModule,
                    ActiveScore = bestScore,
                    RunnerUpScore = runnerUpScore == float.MinValue ? bestScore : runnerUpScore,
                    CommunicationIntentSuppression = communicationSuppression,
                    AbstractionConfidence = abstractionConfidence,
                    IsAbstractedControl = abstractedControl,
                    LastSelectionTick = tick
                };

                var updatedContext = context.ValueRO;
                updatedContext.LastEvaluatedTick = tick;
                context.ValueRW = updatedContext;
            }
        }

        private static void RecoverCooldownPenalties(DynamicBuffer<BandDoctrineWeight> weights)
        {
            for (var i = 0; i < weights.Length; i++)
            {
                var weight = weights[i];
                if (weight.CooldownPenalty <= 0f)
                {
                    continue;
                }

                weight.CooldownPenalty = math.max(0f, weight.CooldownPenalty - CooldownRecoveryPerTick);
                weights[i] = weight;
            }
        }

        private static void ApplySwitchPenalty(DynamicBuffer<BandDoctrineWeight> weights, BandDoctrineModuleType previousModule)
        {
            for (var i = 0; i < weights.Length; i++)
            {
                var weight = weights[i];
                if (weight.Module != previousModule)
                {
                    continue;
                }

                weight.CooldownPenalty = math.saturate(weight.CooldownPenalty + SwitchPenaltyApplied);
                weights[i] = weight;
                return;
            }
        }

        private static float ComputeAbstractionSignal(in Band band, in BandDoctrineProfile profile, in BandDoctrineContext context)
        {
            if ((band.Status & BandStatus.Forming) != 0 || (band.Status & BandStatus.Routing) != 0)
            {
                return 0f;
            }

            if (context.ThreatVolatility > profile.MaxThreatVolatilityForAbstraction)
            {
                return 0f;
            }

            var threshold = math.saturate(profile.CohesionAbstractionThreshold);
            if (band.Cohesion <= threshold)
            {
                return 0f;
            }

            var range = math.max(0.001f, 1f - threshold);
            var cohesionSignal = math.saturate((band.Cohesion - threshold) / range);
            var volatilityDamping = 1f - math.saturate(context.ThreatVolatility / math.max(0.001f, profile.MaxThreatVolatilityForAbstraction));
            return cohesionSignal * volatilityDamping;
        }

        private static float ScoreAbstractionAffinity(BandDoctrineModuleType module, float communicationSuppression)
        {
            var suppression = math.saturate(communicationSuppression);
            switch (module)
            {
                case BandDoctrineModuleType.ElasticFront:
                    return suppression * 0.08f;
                case BandDoctrineModuleType.ReservePulse:
                    return suppression * 0.08f;
                case BandDoctrineModuleType.SplitShell:
                case BandDoctrineModuleType.SortieWindow:
                    return suppression * -0.08f;
                default:
                    return 0f;
            }
        }

        private static float ScoreModule(
            BandDoctrineModuleType module,
            in Band band,
            in BandDoctrineProfile profile,
            in BandDoctrineContext context,
            in BandDoctrineWeight moduleWeight)
        {
            var score = moduleWeight.BaseWeight + moduleWeight.LearnedBias - moduleWeight.CooldownPenalty;

            var frontThreat = math.saturate(context.FrontThreat);
            var rearThreat = math.saturate(context.RearThreat);
            var attrition = math.saturate(context.AttritionPressure);
            var ammoPressure = math.saturate(context.AmmoPressure);
            var objectivePressure = math.saturate(context.ObjectivePressure);
            var volatility = math.saturate(context.ThreatVolatility);
            var casualtyRisk = math.saturate(context.CasualtyRisk);
            var rotationDemand = math.saturate(context.RotationDemand);

            var authoritarian = math.saturate(profile.AuthoritarianBias);
            var egalitarian = math.saturate(profile.EgalitarianBias);
            var corruption = math.saturate(profile.CorruptionBias);
            var cruelty = math.saturate(profile.CrueltyBias);
            var rigidity = math.saturate(profile.GoalRigidity);

            switch (module)
            {
                case BandDoctrineModuleType.Rotation:
                    score += rotationDemand * 0.65f;
                    score += attrition * 0.2f;
                    score += egalitarian * 0.1f;
                    score -= authoritarian * 0.12f;
                    score -= cruelty * 0.2f;
                    break;

                case BandDoctrineModuleType.ElasticFront:
                    score += frontThreat * 0.45f;
                    score += casualtyRisk * 0.25f;
                    score += volatility * 0.15f;
                    score -= objectivePressure * 0.1f;
                    score -= rigidity * 0.05f;
                    break;

                case BandDoctrineModuleType.SplitShell:
                    score += rearThreat * 0.55f;
                    score += volatility * 0.2f;
                    score += frontThreat * 0.1f;
                    score -= objectivePressure * 0.1f;
                    score += (1f - rigidity) * 0.05f;
                    break;

                case BandDoctrineModuleType.SortieWindow:
                    score += ammoPressure * 0.45f;
                    score += objectivePressure * 0.3f;
                    score += math.saturate(band.Cohesion) * 0.2f;
                    score -= casualtyRisk * 0.15f;
                    score += corruption * 0.04f;
                    break;

                case BandDoctrineModuleType.ReservePulse:
                    score += casualtyRisk * 0.35f;
                    score += objectivePressure * 0.2f;
                    score += attrition * 0.2f;
                    score += (1f - math.saturate(band.Fatigue)) * 0.15f;
                    score += rigidity * 0.05f;
                    break;

                case BandDoctrineModuleType.SacrificialScreen:
                    score += casualtyRisk * 0.35f;
                    score += frontThreat * 0.15f;
                    score += cruelty * 0.3f;
                    score += authoritarian * 0.15f;
                    score += corruption * 0.12f;
                    score -= egalitarian * 0.2f;
                    score -= math.saturate(band.Morale) * 0.08f;
                    break;
            }

            return math.max(0f, score);
        }
    }
}
