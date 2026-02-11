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
            state.RequireForUpdate<BandFormation>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            foreach (var (selection, projection, formation) in SystemAPI
                         .Query<RefRO<BandDoctrineSelection>, RefRW<BandDoctrineProjection>, RefRW<BandFormation>>())
            {
                var next = projection.ValueRO;
                EnsureBaseline(ref next, formation.ValueRO);
                ApplyModuleProjection(selection.ValueRO, ref next);

                var targetSpacing = math.max(0.25f, next.BaselineSpacing * next.TargetSpacingMultiplier);
                var targetWidth = math.max(0.25f, next.BaselineWidth * next.TargetWidthMultiplier);
                var targetDepth = math.max(0.25f, next.BaselineDepth * next.TargetDepthMultiplier);

                var blend = selection.ValueRO.IsAbstractedControl == 1 ? 0.2f : 0.35f;
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

        private static void ApplyModuleProjection(in BandDoctrineSelection selection, ref BandDoctrineProjection projection)
        {
            projection.TargetSpacingMultiplier = 1f;
            projection.TargetWidthMultiplier = 1f;
            projection.TargetDepthMultiplier = 1f;
            projection.RotationPriority = 0.2f;
            projection.ReserveCommitment = 0.25f;
            projection.ExposureBias = 0.5f;
            projection.CommunicationLoadMultiplier = 1f;

            switch (selection.ActiveModule)
            {
                case BandDoctrineModuleType.Rotation:
                    projection.TargetSpacingMultiplier = 0.95f;
                    projection.TargetDepthMultiplier = 1.1f;
                    projection.RotationPriority = 1f;
                    projection.ReserveCommitment = 0.2f;
                    projection.ExposureBias = 0.45f;
                    projection.CommunicationLoadMultiplier = 1.05f;
                    break;

                case BandDoctrineModuleType.ElasticFront:
                    projection.TargetSpacingMultiplier = 1.08f;
                    projection.TargetWidthMultiplier = 1.05f;
                    projection.TargetDepthMultiplier = 1.2f;
                    projection.RotationPriority = 0.35f;
                    projection.ReserveCommitment = 0.45f;
                    projection.ExposureBias = 0.35f;
                    projection.CommunicationLoadMultiplier = 0.85f;
                    break;

                case BandDoctrineModuleType.SplitShell:
                    projection.TargetSpacingMultiplier = 1.2f;
                    projection.TargetWidthMultiplier = 1.25f;
                    projection.TargetDepthMultiplier = 0.9f;
                    projection.RotationPriority = 0.3f;
                    projection.ReserveCommitment = 0.5f;
                    projection.ExposureBias = 0.55f;
                    projection.CommunicationLoadMultiplier = 1.25f;
                    break;

                case BandDoctrineModuleType.SortieWindow:
                    projection.TargetSpacingMultiplier = 0.9f;
                    projection.TargetWidthMultiplier = 1f;
                    projection.TargetDepthMultiplier = 1f;
                    projection.RotationPriority = 0.5f;
                    projection.ReserveCommitment = 0.55f;
                    projection.ExposureBias = 0.65f;
                    projection.CommunicationLoadMultiplier = 1.2f;
                    break;

                case BandDoctrineModuleType.ReservePulse:
                    projection.TargetSpacingMultiplier = 1f;
                    projection.TargetWidthMultiplier = 0.95f;
                    projection.TargetDepthMultiplier = 1.15f;
                    projection.RotationPriority = 0.25f;
                    projection.ReserveCommitment = 0.9f;
                    projection.ExposureBias = 0.3f;
                    projection.CommunicationLoadMultiplier = 0.95f;
                    break;

                case BandDoctrineModuleType.SacrificialScreen:
                    projection.TargetSpacingMultiplier = 1.3f;
                    projection.TargetWidthMultiplier = 1.2f;
                    projection.TargetDepthMultiplier = 1.05f;
                    projection.RotationPriority = 0.15f;
                    projection.ReserveCommitment = 0.2f;
                    projection.ExposureBias = 0.95f;
                    projection.CommunicationLoadMultiplier = 0.8f;
                    break;
            }

            var suppression = math.saturate(selection.CommunicationIntentSuppression);
            projection.CommunicationLoadMultiplier = math.max(0.3f, projection.CommunicationLoadMultiplier * math.lerp(1f, 0.4f, suppression));
        }
    }
}
