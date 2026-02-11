using System;
using Unity.Entities;
using Unity.Mathematics;

namespace Godgame.Bands
{
    public enum BandComplianceState : byte
    {
        Obedient = 0,
        ItalianStrike = 1,
        CovertCoordination = 2,
        OpenMutiny = 3
    }

    public enum BandOrderEventType : byte
    {
        GeneralOrder = 0,
        RotationDenied = 1,
        HardshipDirective = 2,
        Execution = 3,
        Punishment = 4,
        Reward = 5
    }

    public enum BandJusticeOutcome : byte
    {
        Fine = 0,
        Demotion = 1,
        Confinement = 2,
        Execution = 3
    }

    public enum BandJusticeTargetClass : byte
    {
        Crew = 0,
        Officer = 1,
        Elite = 2
    }

    public enum BandIntelQuality : byte
    {
        Verified = 0,
        Mixed = 1,
        Rumor = 2
    }

    [Flags]
    public enum BandMemoryFlags : byte
    {
        None = 0,
        Permanent = 1 << 0,
        Hardened = 1 << 1,
        Propagandized = 1 << 2
    }

    public enum BandMemoryType : byte
    {
        Betrayal = 0,
        Heroism = 1,
        Cowardice = 2,
        Enslavement = 3,
        Atrocity = 4,
        MajorLoss = 5,
        MajorVictory = 6,
        MinorConfrontation = 7,
        ShiftOvertime = 8,
        FrontlineOverstay = 9,
        Grievance = 10
    }

    public enum BandSplinterIntentType : byte
    {
        None = 0,
        Escape = 1,
        CaptureLeader = 2,
        SurvivalCluster = 3
    }

    public struct BandSocioProfile : IComponentData
    {
        public float ChaosAxis;
        public float RumorImpulse;
        public float FamilyLoyalty;
        public float InstitutionLoyalty;
        public float NepotismTolerance;
        public float DueProcessPreference;
        public float PropagandaSusceptibility;
        public float MutinySympathy;

        public static BandSocioProfile Default => new BandSocioProfile
        {
            ChaosAxis = 0.5f,
            RumorImpulse = 0.35f,
            FamilyLoyalty = 0.5f,
            InstitutionLoyalty = 0.5f,
            NepotismTolerance = 0.3f,
            DueProcessPreference = 0.5f,
            PropagandaSusceptibility = 0.35f,
            MutinySympathy = 0.4f
        };
    }

    public struct BandDisciplineState : IComponentData
    {
        public BandComplianceState ComplianceState;
        public float Radicalization;
        public float OrderDrift;
        public float CorruptionDrift;
        public float SecretCoordination;
        public float SplinterPressure;
        public uint LowMoraleTicks;
        public uint LastTransitionTick;

        public static BandDisciplineState Default => new BandDisciplineState
        {
            ComplianceState = BandComplianceState.Obedient,
            Radicalization = 0f,
            OrderDrift = 0f,
            CorruptionDrift = 0f,
            SecretCoordination = 0f,
            SplinterPressure = 0f,
            LowMoraleTicks = 0,
            LastTransitionTick = 0
        };
    }

    public struct BandOrderClimate : IComponentData
    {
        public float CurrentOrderDivergence;
        public float HardshipPressure;
        public float CaptainDecisionImpact;
        public float AggregateGrievance;
        public float CaptainPenaltyPressure;
        public float MasterLogisticianPressure;

        public static BandOrderClimate Default => new BandOrderClimate
        {
            CurrentOrderDivergence = 0f,
            HardshipPressure = 0f,
            CaptainDecisionImpact = 0f,
            AggregateGrievance = 0f,
            CaptainPenaltyPressure = 0f,
            MasterLogisticianPressure = 0f
        };
    }

    public struct BandResourceMorality : IComponentData
    {
        public float AllocationAuthoritarian;
        public float AllocationEgalitarian;
        public float CorruptionOffset;
        public float LogisticianAuthority;

        public static BandResourceMorality Default => new BandResourceMorality
        {
            AllocationAuthoritarian = 0.5f,
            AllocationEgalitarian = 0.5f,
            CorruptionOffset = 0.2f,
            LogisticianAuthority = 0.5f
        };
    }

    public struct BandGovernancePulse : IComponentData
    {
        public float NepotismBias;
        public float RankMeritBias;
        public float ScapegoatBias;
        public float JusticeCredibility;
        public float InternalEliteSupport;
        public float ExternalLegitimacy;
        public float PublicFear;

        public static BandGovernancePulse Default => new BandGovernancePulse
        {
            NepotismBias = 0f,
            RankMeritBias = 0.5f,
            ScapegoatBias = 0f,
            JusticeCredibility = 0.5f,
            InternalEliteSupport = 0.5f,
            ExternalLegitimacy = 0.5f,
            PublicFear = 0.2f
        };
    }

    public struct BandSplinterMeans : IComponentData
    {
        public float OwnShipAccess;
        public float SeizureCapability;
        public float ProvisioningReadiness;
        public float TravelNetworkAccess;

        public static BandSplinterMeans Default => new BandSplinterMeans
        {
            OwnShipAccess = 0.2f,
            SeizureCapability = 0.25f,
            ProvisioningReadiness = 0.35f,
            TravelNetworkAccess = 0.3f
        };
    }

    public struct BandSplinterIntentState : IComponentData
    {
        public BandSplinterIntentType ActiveIntent;
        public float EscapeReadiness;
        public float CaptureReadiness;
        public float SurvivalReadiness;
        public uint LastIntentTick;

        public static BandSplinterIntentState Default => new BandSplinterIntentState
        {
            ActiveIntent = BandSplinterIntentType.None,
            EscapeReadiness = 0f,
            CaptureReadiness = 0f,
            SurvivalReadiness = 0f,
            LastIntentTick = 0
        };
    }

    [InternalBufferCapacity(8)]
    public struct BandOrderEvent : IBufferElementData
    {
        public BandOrderEventType Type;
        public float Divergence;
        public float Severity;
        public byte IsPublic;
        public byte BypassChain;
        public uint Tick;
    }

    [InternalBufferCapacity(8)]
    public struct BandJusticeEvent : IBufferElementData
    {
        public BandJusticeOutcome Outcome;
        public BandJusticeTargetClass TargetClass;
        public float Severity;
        public float EvidenceStrength;
        public float TargetAffinity;
        public byte IsPublic;
        public uint Tick;
    }

    [InternalBufferCapacity(8)]
    public struct BandIntelReport : IBufferElementData
    {
        public BandIntelQuality Quality;
        public float ThreatBias;
        public float ObjectiveBias;
        public float DeceptionIntent;
        public float BeneficiaryBias;
        public float EvidenceStrength;
        public uint Tick;
    }

    [InternalBufferCapacity(16)]
    public struct BandMemoryEvent : IBufferElementData
    {
        public BandMemoryType Type;
        public float Salience;
        public float DecayRate;
        public float Legendization;
        public BandMemoryFlags Flags;
        public uint Tick;
    }

    [InternalBufferCapacity(8)]
    public struct BandDisciplineEvent : IBufferElementData
    {
        public BandComplianceState State;
        public float TriggerScore;
        public float MoraleAfter;
        public uint Tick;
    }

    [InternalBufferCapacity(8)]
    public struct BandSplinterIntentEvent : IBufferElementData
    {
        public BandSplinterIntentType Intent;
        public float IntentScore;
        public float MeansScore;
        public uint Tick;
    }
}
