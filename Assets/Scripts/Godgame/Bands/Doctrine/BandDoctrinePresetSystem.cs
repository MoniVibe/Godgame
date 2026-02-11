using PureDOTS.Runtime.Components;
using Unity.Burst;
using Unity.Entities;

namespace Godgame.Bands
{
    /// <summary>
    /// Applies high-level doctrine presets to profile/autonomy/hierarchy knobs.
    /// Presets are only written when changed, so hand-tuned values remain stable afterwards.
    /// </summary>
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(BandDoctrineBootstrapSystem))]
    [UpdateBefore(typeof(BandDoctrineSelectionSystem))]
    public partial struct BandDoctrinePresetSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<TimeState>();
            state.RequireForUpdate<BandDoctrinePresetState>();
            state.RequireForUpdate<BandDoctrineProfile>();
            state.RequireForUpdate<BandCommandAutonomy>();
            state.RequireForUpdate<BandCommandHierarchy>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var tick = SystemAPI.GetSingleton<TimeState>().Tick;

            foreach (var (preset, profile, autonomy, hierarchy) in SystemAPI.Query<
                         RefRW<BandDoctrinePresetState>,
                         RefRW<BandDoctrineProfile>,
                         RefRW<BandCommandAutonomy>,
                         RefRW<BandCommandHierarchy>>())
            {
                if (preset.ValueRO.Value == preset.ValueRO.AppliedValue)
                {
                    continue;
                }

                ApplyPreset(
                    preset.ValueRO.Value,
                    ref profile.ValueRW,
                    ref autonomy.ValueRW,
                    ref hierarchy.ValueRW);

                var next = preset.ValueRW;
                next.AppliedValue = next.Value;
                next.LastAppliedTick = tick;
                preset.ValueRW = next;
            }
        }

        private static void ApplyPreset(
            BandDoctrinePreset preset,
            ref BandDoctrineProfile profile,
            ref BandCommandAutonomy autonomy,
            ref BandCommandHierarchy hierarchy)
        {
            switch (preset)
            {
                case BandDoctrinePreset.Opportunist:
                    profile.AuthoritarianBias = 0.42f;
                    profile.EgalitarianBias = 0.35f;
                    profile.CorruptionBias = 0.45f;
                    profile.CrueltyBias = 0.26f;
                    profile.GoalRigidity = 0.38f;
                    profile.CohesionAbstractionThreshold = 0.70f;
                    profile.MaxThreatVolatilityForAbstraction = 0.55f;
                    profile.CommunicationCompression = 0.5f;

                    autonomy.CaptainAssertiveness = 0.62f;
                    autonomy.CaptainEmpathy = 0.34f;
                    autonomy.CaptainOpportunism = 0.78f;
                    autonomy.CaptainCharisma = 0.58f;
                    autonomy.CaptainIntegrity = 0.38f;
                    autonomy.WhistleblowRiskTolerance = 0.42f;
                    autonomy.RequestThreshold = 0.48f;

                    hierarchy.StrikeGroupApprovalBias = 0.58f;
                    hierarchy.FleetAdmiralApprovalBias = 0.54f;
                    hierarchy.ApprovalThresholdBase = 0.45f;
                    hierarchy.ApprovalThresholdScale = 0.18f;
                    hierarchy.HighCommandCorruption = 0.4f;
                    hierarchy.HighCommandSuppression = 0.28f;
                    hierarchy.WhistleblowThresholdBase = 0.68f;
                    hierarchy.MinTicksBetweenRequests = 10;
                    break;

                case BandDoctrinePreset.Zealot:
                    profile.AuthoritarianBias = 0.88f;
                    profile.EgalitarianBias = 0.08f;
                    profile.CorruptionBias = 0.22f;
                    profile.CrueltyBias = 0.74f;
                    profile.GoalRigidity = 0.9f;
                    profile.CohesionAbstractionThreshold = 0.82f;
                    profile.MaxThreatVolatilityForAbstraction = 0.35f;
                    profile.CommunicationCompression = 0.68f;

                    autonomy.CaptainAssertiveness = 0.86f;
                    autonomy.CaptainEmpathy = 0.1f;
                    autonomy.CaptainOpportunism = 0.3f;
                    autonomy.CaptainCharisma = 0.62f;
                    autonomy.CaptainIntegrity = 0.24f;
                    autonomy.WhistleblowRiskTolerance = 0.2f;
                    autonomy.RequestThreshold = 0.42f;

                    hierarchy.StrikeGroupApprovalBias = 0.72f;
                    hierarchy.FleetAdmiralApprovalBias = 0.78f;
                    hierarchy.ApprovalThresholdBase = 0.7f;
                    hierarchy.ApprovalThresholdScale = 0.3f;
                    hierarchy.HighCommandCorruption = 0.22f;
                    hierarchy.HighCommandSuppression = 0.62f;
                    hierarchy.WhistleblowThresholdBase = 0.78f;
                    hierarchy.MinTicksBetweenRequests = 8;
                    break;

                default:
                    profile.AuthoritarianBias = 0.45f;
                    profile.EgalitarianBias = 0.55f;
                    profile.CorruptionBias = 0.1f;
                    profile.CrueltyBias = 0.08f;
                    profile.GoalRigidity = 0.72f;
                    profile.CohesionAbstractionThreshold = 0.78f;
                    profile.MaxThreatVolatilityForAbstraction = 0.42f;
                    profile.CommunicationCompression = 0.38f;

                    autonomy.CaptainAssertiveness = 0.45f;
                    autonomy.CaptainEmpathy = 0.62f;
                    autonomy.CaptainOpportunism = 0.18f;
                    autonomy.CaptainCharisma = 0.55f;
                    autonomy.CaptainIntegrity = 0.65f;
                    autonomy.WhistleblowRiskTolerance = 0.58f;
                    autonomy.RequestThreshold = 0.58f;

                    hierarchy.StrikeGroupApprovalBias = 0.55f;
                    hierarchy.FleetAdmiralApprovalBias = 0.6f;
                    hierarchy.ApprovalThresholdBase = 0.52f;
                    hierarchy.ApprovalThresholdScale = 0.22f;
                    hierarchy.HighCommandCorruption = 0.12f;
                    hierarchy.HighCommandSuppression = 0.2f;
                    hierarchy.WhistleblowThresholdBase = 0.62f;
                    hierarchy.MinTicksBetweenRequests = 14;
                    break;
            }
        }
    }
}
