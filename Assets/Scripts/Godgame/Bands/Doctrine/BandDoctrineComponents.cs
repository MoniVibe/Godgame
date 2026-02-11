using Unity.Entities;
using Unity.Mathematics;

namespace Godgame.Bands
{
    /// <summary>
    /// Doctrine modules that a band can activate based on context and profile.
    /// </summary>
    public enum BandDoctrineModuleType : byte
    {
        Rotation = 0,
        ElasticFront = 1,
        SplitShell = 2,
        SortieWindow = 3,
        ReservePulse = 4,
        SacrificialScreen = 5
    }

    /// <summary>
    /// High-level command presets used to quickly shape doctrine behavior.
    /// </summary>
    public enum BandDoctrinePreset : byte
    {
        HoldLine = 0,
        Opportunist = 1,
        Zealot = 2
    }

    /// <summary>
    /// Current doctrine preset assignment for a band.
    /// </summary>
    public struct BandDoctrinePresetState : IComponentData
    {
        public BandDoctrinePreset Value;
        public BandDoctrinePreset AppliedValue;
        public uint LastAppliedTick;

        public static BandDoctrinePresetState Default => new BandDoctrinePresetState
        {
            Value = BandDoctrinePreset.HoldLine,
            AppliedValue = (BandDoctrinePreset)byte.MaxValue,
            LastAppliedTick = 0
        };
    }

    /// <summary>
    /// Command profile used to arbitrate doctrine requests.
    /// Higher authoritarian/corruption/cruelty values increase harsh policy choices.
    /// </summary>
    public struct BandDoctrineProfile : IComponentData
    {
        public float AuthoritarianBias;
        public float EgalitarianBias;
        public float CorruptionBias;
        public float CrueltyBias;
        public float GoalRigidity;
        public float CohesionAbstractionThreshold;
        public float MaxThreatVolatilityForAbstraction;
        public float CommunicationCompression;

        public static BandDoctrineProfile Default => new BandDoctrineProfile
        {
            AuthoritarianBias = 0.5f,
            EgalitarianBias = 0.5f,
            CorruptionBias = 0.15f,
            CrueltyBias = 0.15f,
            GoalRigidity = 0.5f,
            CohesionAbstractionThreshold = 0.75f,
            MaxThreatVolatilityForAbstraction = 0.45f,
            CommunicationCompression = 0.45f
        };
    }

    /// <summary>
    /// Runtime tactical context evaluated by doctrine selection.
    /// </summary>
    public struct BandDoctrineContext : IComponentData
    {
        public float FrontThreat;
        public float RearThreat;
        public float AttritionPressure;
        public float AmmoPressure;
        public float ObjectivePressure;
        public float ThreatVolatility;
        public float CasualtyRisk;
        public float RotationDemand;
        public float ScrutinyPressure;
        public float Criticality;
        public uint LastEvaluatedTick;

        public static BandDoctrineContext Default => new BandDoctrineContext
        {
            FrontThreat = 0f,
            RearThreat = 0f,
            AttritionPressure = 0f,
            AmmoPressure = 0f,
            ObjectivePressure = 0f,
            ThreatVolatility = 0f,
            CasualtyRisk = 0f,
            RotationDemand = 0f,
            ScrutinyPressure = 0f,
            Criticality = 0f,
            LastEvaluatedTick = 0
        };
    }

    /// <summary>
    /// Tunable doctrine weight entry. LearnedBias can be adjusted by memory systems.
    /// </summary>
    [InternalBufferCapacity(8)]
    public struct BandDoctrineWeight : IBufferElementData
    {
        public BandDoctrineModuleType Module;
        public float BaseWeight;
        public float LearnedBias;
        public float CooldownPenalty;
        public byte Enabled;

        public static BandDoctrineWeight CreateDefault(BandDoctrineModuleType module)
        {
            return new BandDoctrineWeight
            {
                Module = module,
                BaseWeight = GetDefaultBaseWeight(module),
                LearnedBias = 0f,
                CooldownPenalty = 0f,
                Enabled = 1
            };
        }

        public static float GetDefaultBaseWeight(BandDoctrineModuleType module)
        {
            switch (module)
            {
                case BandDoctrineModuleType.Rotation:
                    return 0.55f;
                case BandDoctrineModuleType.ElasticFront:
                    return 0.5f;
                case BandDoctrineModuleType.SplitShell:
                    return 0.5f;
                case BandDoctrineModuleType.SortieWindow:
                    return 0.45f;
                case BandDoctrineModuleType.ReservePulse:
                    return 0.4f;
                case BandDoctrineModuleType.SacrificialScreen:
                    return 0.2f;
                default:
                    return 0.3f;
            }
        }
    }

    /// <summary>
    /// Output of doctrine arbitration.
    /// </summary>
    public struct BandDoctrineSelection : IComponentData
    {
        public BandDoctrineModuleType ActiveModule;
        public BandDoctrineModuleType PreviousModule;
        public float ActiveScore;
        public float RunnerUpScore;
        public float CommunicationIntentSuppression;
        public float AbstractionConfidence;
        public byte IsAbstractedControl;
        public uint LastSelectionTick;
    }

    /// <summary>
    /// Projection of doctrine to formation controls consumed by other systems.
    /// </summary>
    public struct BandDoctrineProjection : IComponentData
    {
        public float BaselineSpacing;
        public float BaselineWidth;
        public float BaselineDepth;
        public float TargetSpacingMultiplier;
        public float TargetWidthMultiplier;
        public float TargetDepthMultiplier;
        public float RotationPriority;
        public float ReserveCommitment;
        public float ExposureBias;
        public float CommunicationLoadMultiplier;
        public float FrontlineCommitment;
        public float RearGuardCommitment;
        public float SplitRejoinProgress;
        public float SortieValidationStrength;
        public byte BaselineInitialized;
    }

    /// <summary>
    /// Role of an entity issuing a tactical request.
    /// </summary>
    public enum BandCommandRole : byte
    {
        Member = 0,
        Captain = 1,
        StrikeLead = 2,
        FleetAdmiral = 3
    }

    /// <summary>
    /// Status of a processed doctrine request.
    /// </summary>
    public enum BandRequestDisposition : byte
    {
        Pending = 0,
        Approved = 1,
        Denied = 2
    }

    /// <summary>
    /// Captain/crew tactical request, typically emitted by captain intent systems.
    /// </summary>
    [InternalBufferCapacity(8)]
    public struct BandDoctrineRequest : IBufferElementData
    {
        public BandDoctrineModuleType RequestedModule;
        public BandCommandRole RequesterRole;
        public Entity RequesterEntity;
        public float Urgency;
        public float CasualtyConcern;
        public float MoraleConcern;
        public uint RequestTick;
        public uint CooldownUntilTick;
    }

    /// <summary>
    /// Governance decision trace for processed requests.
    /// </summary>
    [InternalBufferCapacity(8)]
    public struct BandDoctrineDecisionEvent : IBufferElementData
    {
        public BandDoctrineModuleType Module;
        public BandRequestDisposition Disposition;
        public BandCommandRole RequesterRole;
        public float ApprovalScore;
        public float ApprovalThreshold;
        public float ResentmentDelta;
        public float TrustDelta;
        public uint Tick;
    }

    /// <summary>
    /// Command hierarchy profile used in request arbitration.
    /// </summary>
    public struct BandCommandHierarchy : IComponentData
    {
        public float StrikeGroupApprovalBias;
        public float FleetAdmiralApprovalBias;
        public float ApprovalThresholdBase;
        public float ApprovalThresholdScale;
        public float HighCommandCorruption;
        public float HighCommandSuppression;
        public float WhistleblowThresholdBase;
        public uint MinTicksBetweenRequests;

        public static BandCommandHierarchy Default => new BandCommandHierarchy
        {
            StrikeGroupApprovalBias = 0.5f,
            FleetAdmiralApprovalBias = 0.5f,
            ApprovalThresholdBase = 0.5f,
            ApprovalThresholdScale = 0.25f,
            HighCommandCorruption = 0.15f,
            HighCommandSuppression = 0.2f,
            WhistleblowThresholdBase = 0.62f,
            MinTicksBetweenRequests = 12
        };
    }

    /// <summary>
    /// Captures social consequences of command choices.
    /// </summary>
    public struct BandSocialState : IComponentData
    {
        public float CrewResentment;
        public float CommandTrust;
        public float CohesionPenalty;
        public float LoyaltyDrift;
        public float NecessityAcceptance;
        public float ObservedCorruptionEvidence;
        public float WhistleblowSupport;
        public uint LastGovernanceTick;

        public static BandSocialState Default => new BandSocialState
        {
            CrewResentment = 0f,
            CommandTrust = 0.5f,
            CohesionPenalty = 0f,
            LoyaltyDrift = 0f,
            NecessityAcceptance = 0.5f,
            ObservedCorruptionEvidence = 0f,
            WhistleblowSupport = 0.5f,
            LastGovernanceTick = 0
        };
    }

    /// <summary>
    /// Captain autonomy and cadence for self-initiated requests.
    /// </summary>
    public struct BandCommandAutonomy : IComponentData
    {
        public float CaptainAssertiveness;
        public float CaptainEmpathy;
        public float CaptainOpportunism;
        public float CaptainCharisma;
        public float CaptainIntegrity;
        public float WhistleblowRiskTolerance;
        public float RequestThreshold;
        public uint LastRequestTick;
        public uint LastWhistleblowTick;

        public static BandCommandAutonomy Default => new BandCommandAutonomy
        {
            CaptainAssertiveness = 0.5f,
            CaptainEmpathy = 0.5f,
            CaptainOpportunism = 0.2f,
            CaptainCharisma = 0.5f,
            CaptainIntegrity = 0.5f,
            WhistleblowRiskTolerance = 0.5f,
            RequestThreshold = 0.55f,
            LastRequestTick = 0,
            LastWhistleblowTick = 0
        };
    }

    public enum BandEscalationType : byte
    {
        CorruptionDisclosure = 0
    }

    public enum BandEscalationDisposition : byte
    {
        NotAttempted = 0,
        Failed = 1,
        Succeeded = 2
    }

    [InternalBufferCapacity(4)]
    public struct BandCommandEscalationEvent : IBufferElementData
    {
        public BandEscalationType Type;
        public BandEscalationDisposition Disposition;
        public float AttemptScore;
        public float Threshold;
        public float EvidenceStrength;
        public float ScrutinyStrength;
        public uint Tick;
    }
}
