using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;

namespace Godgame.Bands
{
    /// <summary>
    /// Projects selected doctrine into formation shaping and communication load hints.
    /// </summary>
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(BandDoctrineSelectionSystem))]
    public partial struct BandDoctrineProjectionSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<BandDoctrineSelection>();
            state.RequireForUpdate<BandDoctrineContext>();
            state.RequireForUpdate<BandFormation>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            foreach (var (selection, context, profile, socio, projection, formation) in SystemAPI
                         .Query<
                             RefRO<BandDoctrineSelection>,
                             RefRO<BandDoctrineContext>,
                             RefRO<BandDoctrineProfile>,
                             RefRO<BandSocioProfile>,
                             RefRW<BandDoctrineProjection>,
                             RefRW<BandFormation>>())
            {
                var next = projection.ValueRO;
                EnsureBaseline(ref next, formation.ValueRO);
                ApplyModuleProjection(selection.ValueRO, context.ValueRO, profile.ValueRO, socio.ValueRO, ref next);

                var targetSpacing = math.max(0.25f, next.BaselineSpacing * next.TargetSpacingMultiplier);
                var targetWidth = math.max(0.25f, next.BaselineWidth * next.TargetWidthMultiplier);
                var targetDepth = math.max(0.25f, next.BaselineDepth * next.TargetDepthMultiplier);

                var blend = selection.ValueRO.IsAbstractedControl == 1 ? 0.2f : 0.35f;
                blend = math.lerp(blend, 0.45f, next.SplitRejoinProgress);
                var updatedFormation = formation.ValueRO;
                updatedFormation.Spacing = math.lerp(updatedFormation.Spacing, targetSpacing, blend);
                updatedFormation.Width = math.lerp(updatedFormation.Width, targetWidth, blend);
                updatedFormation.Depth = math.lerp(updatedFormation.Depth, targetDepth, blend);
                formation.ValueRW = updatedFormation;

                projection.ValueRW = next;
            }
        }

        private static void EnsureBaseline(ref BandDoctrineProjection projection, in BandFormation formation)
        {
            if (projection.BaselineInitialized != 0)
            {
                return;
            }

            projection.BaselineSpacing = math.max(0.25f, formation.Spacing);
            projection.BaselineWidth = math.max(0.25f, formation.Width);
            projection.BaselineDepth = math.max(0.25f, formation.Depth);
            projection.BaselineInitialized = 1;
        }

        private static void ApplyModuleProjection(
            in BandDoctrineSelection selection,
            in BandDoctrineContext context,
            in BandDoctrineProfile profile,
            in BandSocioProfile socio,
            ref BandDoctrineProjection projection)
        {
            projection.TargetSpacingMultiplier = 1f;
            projection.TargetWidthMultiplier = 1f;
            projection.TargetDepthMultiplier = 1f;
            projection.RotationPriority = 0.2f;
            projection.ReserveCommitment = 0.25f;
            projection.ExposureBias = 0.5f;
            projection.CommunicationLoadMultiplier = 1f;
            projection.FrontlineCommitment = 0.9f;
            projection.RearGuardCommitment = 0.1f;
            projection.SplitRejoinProgress = 1f;
            projection.SortieValidationStrength = 1f;

            switch (selection.ActiveModule)
            {
                case BandDoctrineModuleType.Rotation:
                    projection.TargetSpacingMultiplier = 0.95f;
                    projection.TargetDepthMultiplier = 1.1f;
                    projection.RotationPriority = 1f;
                    projection.ReserveCommitment = 0.2f;
                    projection.ExposureBias = 0.45f;
                    projection.CommunicationLoadMultiplier = 1.05f;
                    projection.FrontlineCommitment = 0.85f;
                    projection.RearGuardCommitment = 0.15f;
                    projection.SplitRejoinProgress = 0.85f;
                    break;

                case BandDoctrineModuleType.ElasticFront:
                    projection.TargetSpacingMultiplier = 1.08f;
                    projection.TargetWidthMultiplier = 1.05f;
                    projection.TargetDepthMultiplier = 1.2f;
                    projection.RotationPriority = 0.35f;
                    projection.ReserveCommitment = 0.45f;
                    projection.ExposureBias = 0.35f;
                    projection.CommunicationLoadMultiplier = 0.85f;
                    projection.FrontlineCommitment = 0.92f;
                    projection.RearGuardCommitment = 0.08f;
                    projection.SplitRejoinProgress = 0.92f;
                    break;

                case BandDoctrineModuleType.SplitShell:
                {
                    var rearThreat = math.saturate(context.RearThreat);
                    var frontThreat = math.saturate(context.FrontThreat);
                    var splitWeight = math.saturate(rearThreat * 1.25f);
                    var totalThreat = math.max(0.001f, rearThreat + frontThreat);
                    var rearShare = math.saturate((rearThreat / totalThreat) * splitWeight);

                    projection.TargetSpacingMultiplier = math.lerp(1f, 1.2f, splitWeight);
                    projection.TargetWidthMultiplier = math.lerp(1f, 1.25f, splitWeight);
                    projection.TargetDepthMultiplier = math.lerp(1f, 0.9f, splitWeight);
                    projection.RotationPriority = 0.3f;
                    projection.ReserveCommitment = 0.5f;
                    projection.ExposureBias = 0.55f;
                    projection.CommunicationLoadMultiplier = 1.25f;
                    projection.RearGuardCommitment = math.clamp(rearShare, 0f, 0.55f);
                    projection.FrontlineCommitment = math.saturate(1f - projection.RearGuardCommitment);
                    projection.SplitRejoinProgress = math.saturate(1f - splitWeight);
                    break;
                }

                case BandDoctrineModuleType.SortieWindow:
                {
                    projection.TargetSpacingMultiplier = 0.9f;
                    projection.TargetWidthMultiplier = 1f;
                    projection.TargetDepthMultiplier = 1f;
                    projection.RotationPriority = 0.5f;
                    projection.ReserveCommitment = 0.55f;
                    projection.ExposureBias = 0.65f;
                    projection.CommunicationLoadMultiplier = 1.2f;
                    projection.FrontlineCommitment = 0.88f;
                    projection.RearGuardCommitment = 0.12f;
                    projection.SplitRejoinProgress = 0.9f;
                    var chaos = math.saturate(socio.ChaosAxis);
                    var validation =
                        0.85f -
                        chaos * 0.75f +
                        math.saturate(profile.AuthoritarianBias) * 0.1f +
                        math.saturate(profile.CorruptionBias) * 0.05f;
                    if (chaos >= 0.88f)
                    {
                        validation = math.min(validation, 0.25f);
                    }
                    projection.SortieValidationStrength = math.saturate(validation);
                    break;
                }

                case BandDoctrineModuleType.ReservePulse:
                    projection.TargetSpacingMultiplier = 1f;
                    projection.TargetWidthMultiplier = 0.95f;
                    projection.TargetDepthMultiplier = 1.15f;
                    projection.RotationPriority = 0.25f;
                    projection.ReserveCommitment = 0.9f;
                    projection.ExposureBias = 0.3f;
                    projection.CommunicationLoadMultiplier = 0.95f;
                    projection.FrontlineCommitment = 0.84f;
                    projection.RearGuardCommitment = 0.16f;
                    projection.SplitRejoinProgress = 0.88f;
                    break;

                case BandDoctrineModuleType.SacrificialScreen:
                    projection.TargetSpacingMultiplier = 1.3f;
                    projection.TargetWidthMultiplier = 1.2f;
                    projection.TargetDepthMultiplier = 1.05f;
                    projection.RotationPriority = 0.15f;
                    projection.ReserveCommitment = 0.2f;
                    projection.ExposureBias = 0.95f;
                    projection.CommunicationLoadMultiplier = 0.8f;
                    projection.FrontlineCommitment = 0.7f;
                    projection.RearGuardCommitment = 0.3f;
                    projection.SplitRejoinProgress = 0.7f;
                    break;
            }

            var suppression = math.saturate(selection.CommunicationIntentSuppression);
            projection.CommunicationLoadMultiplier = math.max(0.3f, projection.CommunicationLoadMultiplier * math.lerp(1f, 0.4f, suppression));
        }
    }
}
