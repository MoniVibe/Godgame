using PureDOTS.Runtime.Components;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;

namespace Godgame.Bands
{
    /// <summary>
    /// Handles captain request generation, command arbitration, social consequences, and corruption disclosure escalation.
    /// </summary>
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(BandDoctrineSelectionSystem))]
    public partial struct BandDoctrineGovernanceSystem : ISystem
    {
        private BufferLookup<BandMember> _bandMembersLookup;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<TimeState>();
            state.RequireForUpdate<BandDoctrineProfile>();
            state.RequireForUpdate<BandDoctrineContext>();
            _bandMembersLookup = state.GetBufferLookup<BandMember>(true);
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var tick = SystemAPI.GetSingleton<TimeState>().Tick;
            _bandMembersLookup.Update(ref state);

            foreach (var (band, profile, context, hierarchy, autonomy, social, requests, events, escalationEvents, selection, entity) in SystemAPI
                         .Query<
                             RefRW<Band>,
                             RefRO<BandDoctrineProfile>,
                             RefRO<BandDoctrineContext>,
                             RefRO<BandCommandHierarchy>,
                             RefRW<BandCommandAutonomy>,
                             RefRW<BandSocialState>,
                             DynamicBuffer<BandDoctrineRequest>,
                             DynamicBuffer<BandDoctrineDecisionEvent>,
                             DynamicBuffer<BandCommandEscalationEvent>,
                             RefRW<BandDoctrineSelection>>()
                         .WithEntityAccess())
            {
                var knownMemberRatio = ComputeKnownMemberRatio(entity, tick);
                SynthesizeCaptainRotationRequest(
                    tick,
                    band.ValueRO,
                    profile.ValueRO,
                    context.ValueRO,
                    hierarchy.ValueRO,
                    knownMemberRatio,
                    ref autonomy.ValueRW,
                    requests);

                ProcessRequests(
                    tick,
                    band,
                    profile.ValueRO,
                    context.ValueRO,
                    hierarchy.ValueRO,
                    knownMemberRatio,
                    social,
                    requests,
                    events,
                    selection);

                EvaluateWhistleblowAttempt(
                    tick,
                    band,
                    context.ValueRO,
                    hierarchy.ValueRO,
                    ref autonomy.ValueRW,
                    ref social.ValueRW,
                    escalationEvents);

                ApplySocialDecay(ref social.ValueRW);
            }
        }

        private float ComputeKnownMemberRatio(Entity bandEntity, uint tick)
        {
            if (!_bandMembersLookup.HasBuffer(bandEntity))
            {
                return 0.5f;
            }

            var members = _bandMembersLookup[bandEntity];
            if (members.Length == 0)
            {
                return 0.5f;
            }

            var knownCount = 0;
            for (var i = 0; i < members.Length; i++)
            {
                var member = members[i];
                var tenure = tick > member.JoinedTick ? tick - member.JoinedTick : 0u;
                if (member.LoyaltyScore >= 60 || tenure >= 120u)
                {
                    knownCount++;
                }
            }

            return math.saturate(knownCount / (float)members.Length);
        }

        private static void SynthesizeCaptainRotationRequest(
            uint tick,
            in Band band,
            in BandDoctrineProfile profile,
            in BandDoctrineContext context,
            in BandCommandHierarchy hierarchy,
            float knownMemberRatio,
            ref BandCommandAutonomy autonomy,
            DynamicBuffer<BandDoctrineRequest> requests)
        {
            if ((band.Status & BandStatus.Engaged) == 0 || (band.Status & BandStatus.Routing) != 0)
            {
                return;
            }

            if (autonomy.LastRequestTick > 0 && tick - autonomy.LastRequestTick < hierarchy.MinTicksBetweenRequests)
            {
                return;
            }

            var corruption = math.saturate(profile.CorruptionBias);
            var knownWeightedCasualty = math.saturate(context.CasualtyRisk) * math.lerp(0.35f, 1f, knownMemberRatio);
            var corruptionSuppression = corruption * (1f - knownMemberRatio) * 0.22f;

            var rotationPressure = math.saturate(context.RotationDemand) * 0.4f;
            var attritionPressure = math.saturate(context.AttritionPressure) * 0.2f;
            var casualtyPressure = knownWeightedCasualty * 0.22f;
            var moralePressure = math.saturate(1f - band.Morale) * 0.08f;
            var empathyLift = math.saturate(autonomy.CaptainEmpathy) * 0.1f;
            var integrityLift = math.saturate(autonomy.CaptainIntegrity) * 0.07f;
            var rigidityPenalty = math.saturate(profile.GoalRigidity) * 0.1f;
            var authoritarianPenalty = math.saturate(profile.AuthoritarianBias) * 0.08f;
            var opportunismLift = math.saturate(autonomy.CaptainOpportunism) * math.saturate(context.ObjectivePressure) * 0.05f;

            var captainIntentScore =
                rotationPressure +
                attritionPressure +
                casualtyPressure +
                moralePressure +
                empathyLift +
                integrityLift +
                opportunismLift -
                rigidityPenalty -
                authoritarianPenalty -
                corruptionSuppression;

            var threshold =
                math.saturate(autonomy.RequestThreshold) +
                math.saturate(profile.GoalRigidity) * 0.1f -
                math.saturate(autonomy.CaptainAssertiveness) * 0.08f;

            if (captainIntentScore < threshold)
            {
                return;
            }

            requests.Add(new BandDoctrineRequest
            {
                RequestedModule = BandDoctrineModuleType.Rotation,
                RequesterRole = BandCommandRole.Captain,
                RequesterEntity = band.Leader,
                Urgency = math.saturate(context.RotationDemand + context.AttritionPressure * 0.5f),
                CasualtyConcern = knownWeightedCasualty,
                MoraleConcern = math.saturate(1f - band.Morale),
                RequestTick = tick,
                CooldownUntilTick = tick + hierarchy.MinTicksBetweenRequests
            });

            autonomy.LastRequestTick = tick;
        }

        private static void ProcessRequests(
            uint tick,
            RefRW<Band> band,
            in BandDoctrineProfile profile,
            in BandDoctrineContext context,
            in BandCommandHierarchy hierarchy,
            float knownMemberRatio,
            RefRW<BandSocialState> social,
            DynamicBuffer<BandDoctrineRequest> requests,
            DynamicBuffer<BandDoctrineDecisionEvent> events,
            RefRW<BandDoctrineSelection> selection)
        {
            if (requests.Length == 0)
            {
                return;
            }

            for (var i = 0; i < requests.Length; i++)
            {
                var request = requests[i];
                if (request.CooldownUntilTick > 0 && request.CooldownUntilTick > tick && request.RequestTick != tick)
                {
                    continue;
                }

                var threshold = ComputeApprovalThreshold(profile, hierarchy, context);
                var approvalScore = ComputeApprovalScore(profile, hierarchy, context, request, knownMemberRatio);
                var approved = approvalScore >= threshold;

                var resentmentDelta = 0f;
                var trustDelta = 0f;
                var corruptionEvidenceDelta = 0f;

                if (approved)
                {
                    trustDelta = math.saturate(0.08f + request.Urgency * 0.1f + request.CasualtyConcern * 0.06f);
                    resentmentDelta = -math.saturate(0.04f + request.Urgency * 0.05f);
                    selection.ValueRW = UpdateSelectionForApproval(selection.ValueRO, request.RequestedModule, tick);
                }
                else
                {
                    var understanding = ComputeUnderstandingFactor(profile, context, band.ValueRO, social.ValueRO, knownMemberRatio);
                    var blatantCorruption = math.saturate(profile.CrueltyBias * 0.5f + profile.CorruptionBias * 0.5f);
                    var grievanceBase = math.saturate(
                        request.Urgency * 0.32f +
                        request.CasualtyConcern * 0.28f +
                        request.MoraleConcern * 0.14f +
                        math.saturate(profile.AuthoritarianBias) * 0.16f +
                        math.saturate(profile.CrueltyBias) * 0.25f +
                        math.saturate(profile.CorruptionBias) * 0.16f);

                    var appreciationHeadroom = understanding - grievanceBase;
                    if (appreciationHeadroom > 0.08f && blatantCorruption < 0.55f)
                    {
                        resentmentDelta = -math.saturate(appreciationHeadroom * 0.35f);
                        trustDelta = math.saturate(appreciationHeadroom * 0.25f);
                    }
                    else
                    {
                        resentmentDelta = math.saturate(grievanceBase - understanding);
                        trustDelta = -math.saturate(resentmentDelta * 0.75f);
                    }

                    corruptionEvidenceDelta = math.saturate(
                        blatantCorruption * 0.45f +
                        request.CasualtyConcern * 0.2f +
                        request.Urgency * 0.15f -
                        understanding * 0.2f);
                }

                ApplySocialAndBandEffects(
                    tick,
                    ref band.ValueRW,
                    ref social.ValueRW,
                    resentmentDelta,
                    trustDelta,
                    corruptionEvidenceDelta,
                    approved);

                events.Add(new BandDoctrineDecisionEvent
                {
                    Module = request.RequestedModule,
                    Disposition = approved ? BandRequestDisposition.Approved : BandRequestDisposition.Denied,
                    RequesterRole = request.RequesterRole,
                    ApprovalScore = approvalScore,
                    ApprovalThreshold = threshold,
                    ResentmentDelta = resentmentDelta,
                    TrustDelta = trustDelta,
                    Tick = tick
                });
            }

            requests.Clear();
        }

        private static float ComputeUnderstandingFactor(
            in BandDoctrineProfile profile,
            in BandDoctrineContext context,
            in Band band,
            in BandSocialState social,
            float knownMemberRatio)
        {
            var blatantCorruption = math.saturate(profile.CrueltyBias * 0.5f + profile.CorruptionBias * 0.5f);
            var necessityFactor =
                math.saturate(context.ObjectivePressure) * 0.34f +
                math.saturate(profile.GoalRigidity) * 0.18f +
                math.saturate(band.Cohesion) * 0.2f +
                math.saturate(knownMemberRatio) * 0.12f +
                math.saturate(social.NecessityAcceptance) * 0.16f;

            if (blatantCorruption > 0.65f)
            {
                necessityFactor *= 0.45f;
            }

            return math.saturate(necessityFactor);
        }

        private static BandDoctrineSelection UpdateSelectionForApproval(
            in BandDoctrineSelection current,
            BandDoctrineModuleType module,
            uint tick)
        {
            var updated = current;
            updated.PreviousModule = current.ActiveModule;
            updated.ActiveModule = module;
            updated.LastSelectionTick = tick;
            updated.ActiveScore = math.max(current.ActiveScore, 0.65f);
            return updated;
        }

        private static float ComputeApprovalThreshold(
            in BandDoctrineProfile profile,
            in BandCommandHierarchy hierarchy,
            in BandDoctrineContext context)
        {
            var threshold =
                hierarchy.ApprovalThresholdBase +
                math.saturate(profile.AuthoritarianBias) * hierarchy.ApprovalThresholdScale * 0.4f +
                math.saturate(profile.GoalRigidity) * hierarchy.ApprovalThresholdScale * 0.5f +
                math.saturate(hierarchy.HighCommandCorruption) * 0.08f +
                math.saturate(context.ObjectivePressure) * 0.2f -
                math.saturate(profile.EgalitarianBias) * 0.2f;

            return math.saturate(threshold);
        }

        private static float ComputeApprovalScore(
            in BandDoctrineProfile profile,
            in BandCommandHierarchy hierarchy,
            in BandDoctrineContext context,
            in BandDoctrineRequest request,
            float knownMemberRatio)
        {
            var corruption = math.saturate(profile.CorruptionBias);
            var casualtyWeight = math.saturate(request.CasualtyConcern) * math.lerp(0.35f, 1f, knownMemberRatio);
            var crewPreservationFloor = knownMemberRatio * math.lerp(0.18f, 0.06f, corruption);
            var unknownDisregardPenalty = (1f - knownMemberRatio) * corruption * 0.18f;

            var operationalNeed =
                math.saturate(request.Urgency) * 0.38f +
                casualtyWeight * 0.28f +
                math.saturate(request.MoraleConcern) * 0.12f +
                math.saturate(context.AttritionPressure) * 0.12f +
                crewPreservationFloor;

            var empathyLift = math.saturate(profile.EgalitarianBias) * 0.18f;
            var strikeBias = math.saturate(hierarchy.StrikeGroupApprovalBias) * 0.09f;
            var fleetBias = math.saturate(hierarchy.FleetAdmiralApprovalBias) * 0.09f;

            var hardlinePenalty =
                math.saturate(profile.AuthoritarianBias) * 0.12f +
                math.saturate(profile.CrueltyBias) * 0.22f +
                corruption * 0.1f +
                math.saturate(profile.GoalRigidity) * 0.11f +
                unknownDisregardPenalty;

            var confidence = operationalNeed + empathyLift + strikeBias + fleetBias - hardlinePenalty;
            return math.saturate(confidence);
        }

        private static void EvaluateWhistleblowAttempt(
            uint tick,
            RefRW<Band> band,
            in BandDoctrineContext context,
            in BandCommandHierarchy hierarchy,
            ref BandCommandAutonomy autonomy,
            ref BandSocialState social,
            DynamicBuffer<BandCommandEscalationEvent> escalationEvents)
        {
            if (autonomy.LastWhistleblowTick > 0 && tick - autonomy.LastWhistleblowTick < 24u)
            {
                return;
            }

            var scrutiny = math.saturate(context.ScrutinyPressure);
            var criticality = math.saturate(math.max(context.Criticality, math.max(context.CasualtyRisk, context.AttritionPressure)));
            var evidence = math.saturate(social.ObservedCorruptionEvidence);
            if (scrutiny < 0.45f || criticality < 0.5f || evidence < 0.2f)
            {
                return;
            }

            var willingness =
                math.saturate(autonomy.CaptainIntegrity) * 0.35f +
                math.saturate(autonomy.WhistleblowRiskTolerance) * 0.22f +
                scrutiny * 0.2f +
                criticality * 0.2f -
                math.saturate(hierarchy.HighCommandSuppression) * 0.22f;

            if (willingness < 0.45f)
            {
                return;
            }

            var attemptScore =
                math.saturate(autonomy.CaptainCharisma) * 0.24f +
                evidence * 0.32f +
                scrutiny * 0.2f +
                criticality * 0.12f +
                math.saturate(autonomy.CaptainIntegrity) * 0.14f +
                math.saturate(social.CommandTrust) * 0.08f -
                math.saturate(hierarchy.HighCommandCorruption) * 0.22f -
                math.saturate(hierarchy.HighCommandSuppression) * 0.14f;

            var threshold =
                math.saturate(hierarchy.WhistleblowThresholdBase) +
                math.saturate(hierarchy.HighCommandCorruption) * 0.16f +
                math.saturate(hierarchy.HighCommandSuppression) * 0.1f -
                scrutiny * 0.2f;

            var succeeded = attemptScore >= threshold;
            autonomy.LastWhistleblowTick = tick;

            if (succeeded)
            {
                social.CommandTrust = math.saturate(social.CommandTrust + 0.12f);
                social.CrewResentment = math.max(0f, social.CrewResentment - 0.08f);
                social.WhistleblowSupport = math.saturate(social.WhistleblowSupport + 0.1f);
                social.ObservedCorruptionEvidence = math.max(0f, social.ObservedCorruptionEvidence - 0.25f);
                band.ValueRW.Morale = math.saturate(band.ValueRO.Morale + 0.03f);
                band.ValueRW.Cohesion = math.saturate(band.ValueRO.Cohesion + 0.02f);
                band.ValueRW.LastUpdateTick = tick;
            }
            else
            {
                social.CommandTrust = math.max(0f, social.CommandTrust - 0.06f);
                social.CrewResentment = math.saturate(social.CrewResentment + 0.05f);
                social.WhistleblowSupport = math.max(0f, social.WhistleblowSupport - 0.08f);
                social.ObservedCorruptionEvidence = math.saturate(social.ObservedCorruptionEvidence + 0.05f);
            }

            escalationEvents.Add(new BandCommandEscalationEvent
            {
                Type = BandEscalationType.CorruptionDisclosure,
                Disposition = succeeded ? BandEscalationDisposition.Succeeded : BandEscalationDisposition.Failed,
                AttemptScore = attemptScore,
                Threshold = threshold,
                EvidenceStrength = evidence,
                ScrutinyStrength = scrutiny,
                Tick = tick
            });
        }

        private static void ApplySocialAndBandEffects(
            uint tick,
            ref Band band,
            ref BandSocialState social,
            float resentmentDelta,
            float trustDelta,
            float corruptionEvidenceDelta,
            bool approved)
        {
            social.CrewResentment = math.saturate(social.CrewResentment + resentmentDelta);
            social.CommandTrust = math.saturate(social.CommandTrust + trustDelta);
            social.CohesionPenalty = math.saturate(social.CrewResentment * 0.35f);
            social.LoyaltyDrift = math.clamp(social.CommandTrust - social.CrewResentment, -1f, 1f);
            social.NecessityAcceptance = math.saturate(social.NecessityAcceptance + (approved ? 0.01f : trustDelta > 0f ? 0.02f : -0.015f));
            social.ObservedCorruptionEvidence = math.saturate(social.ObservedCorruptionEvidence + corruptionEvidenceDelta);
            social.LastGovernanceTick = tick;

            var moraleDelta = approved ? 0.03f : -0.04f - math.max(0f, resentmentDelta) * 0.05f + math.max(0f, trustDelta) * 0.03f;
            var cohesionDelta = approved ? 0.02f : -social.CohesionPenalty * 0.08f + math.max(0f, trustDelta) * 0.02f;
            band.Morale = math.saturate(band.Morale + moraleDelta);
            band.Cohesion = math.saturate(band.Cohesion + cohesionDelta);
            band.LastUpdateTick = tick;
        }

        private static void ApplySocialDecay(ref BandSocialState social)
        {
            social.CrewResentment = math.max(0f, social.CrewResentment - 0.003f);
            social.CohesionPenalty = math.max(0f, social.CohesionPenalty - 0.002f);
            social.ObservedCorruptionEvidence = math.max(0f, social.ObservedCorruptionEvidence - 0.001f);
        }
    }
}
