using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace Godgame.Telemetry
{
    public enum GodgameCapabilitySource : byte
    {
        Unknown = 0,
        Experience = 1,
        Quest = 2,
        Research = 3,
        Script = 4
    }

    public struct GodgameCapabilityGrantedRecord : IBufferElementData
    {
        public uint Tick;
        public FixedString64Bytes AgentId;
        public FixedString64Bytes CapabilityId;
        public GodgameCapabilitySource Source;
        public byte Level;
        public FixedString64Bytes SourceId;
        public uint SeedHash;
    }

    public struct GodgameCapabilityRevokedRecord : IBufferElementData
    {
        public uint Tick;
        public FixedString64Bytes AgentId;
        public FixedString64Bytes CapabilityId;
        public GodgameCapabilitySource Source;
        public byte Level;
        public FixedString64Bytes Reason;
    }

    public struct GodgameCapabilitySnapshotRecord : IBufferElementData
    {
        public uint Tick;
        public FixedString64Bytes AgentId;
        public ulong BitsetHash;
        public byte Level;
        public uint Experience;
        public FixedString64Bytes Context;
    }

    public struct GodgameCapabilityTelemetryCache : IComponentData
    {
        public Godgame.Combat.MilitaryRank LastRank;
        public byte LastDisciplineLevel;
        public uint LastSnapshotTick;
        public uint LastHonorPoints;
        public uint LastSeedHash;
    }

    public enum GodgameDecisionDomain : byte
    {
        Unknown = 0,
        Pilot = 1,
        Soldier = 2,
        Mining = 3,
        Village = 4
    }

    public struct GodgameDecisionTraceEntry
    {
        public FixedString64Bytes Id;
        public float Score;
        public uint ReasonMask;
    }

    [InternalBufferCapacity(64)]
    public struct GodgameDecisionTraceRecord : IBufferElementData
    {
        public uint Tick;
        public FixedString64Bytes AgentId;
        public GodgameDecisionDomain Domain;
        public FixedString64Bytes ChosenId;
        public ushort ReasonCode;
        public FixedList128Bytes<GodgameDecisionScoreEntry> TopChoices;
        public uint ContextHash;
    }

    [InternalBufferCapacity(64)]
    public struct GodgameDecisionOscillationState : IBufferElementData
    {
        public FixedString64Bytes AgentId;
        public FixedString64Bytes LastDecisionId;
        public uint LastDecisionTick;
    }

    public enum GodgameActionOutcome : byte
    {
        Unknown = 0,
        Success = 1,
        Failure = 2,
        Aborted = 3
    }

    public struct GodgameActionEffectRecord : IBufferElementData
    {
        public uint Tick;
        public uint ActionId;
        public FixedString64Bytes AgentId;
        public FixedString64Bytes ActionLabel;
        public float DeltaHealth;
        public float DeltaCargo;
        public float DeltaThreat;
        public float DeltaResource;
    }

    public struct GodgameGatherAttemptRecord : IBufferElementData
    {
        public uint Tick;
        public FixedString64Bytes AgentId;
        public FixedString32Bytes MethodId;
        public FixedString32Bytes ResourceType;
        public FixedString64Bytes NodeId;
    }

    public struct GodgameGatherYieldRecord : IBufferElementData
    {
        public uint Tick;
        public FixedString64Bytes AgentId;
        public FixedString32Bytes MethodId;
        public FixedString32Bytes ResourceType;
        public float Amount;
        public uint TimeSpentTicks;
    }

    public enum GodgameGatherFailureReason : byte
    {
        None = 0,
        InvalidNode = 1,
        Exhausted = 2,
        Blocked = 3,
        Cancelled = 4,
        NotUnlocked = 5
    }

    public struct GodgameGatherFailureRecord : IBufferElementData
    {
        public uint Tick;
        public FixedString64Bytes AgentId;
        public FixedString32Bytes MethodId;
        public FixedString32Bytes ResourceType;
        public GodgameGatherFailureReason Reason;
    }

    public struct GodgameHaulTripRecord : IBufferElementData
    {
        public uint StartTick;
        public uint EndTick;
        public FixedString64Bytes AgentId;
        public float CarriedAmount;
        public float Distance;
        public float Congestion;
    }

    public struct GodgameCapabilityUsageSample : IBufferElementData
    {
        public FixedString64Bytes CapabilityId;
        public uint Count;
    }

    public struct GodgameActionFailureSample : IBufferElementData
    {
        public FixedString64Bytes ActionId;
        public GodgameActionFailureReason Reason;
        public uint Count;
    }

    public struct GodgameTelemetrySummary : IComponentData
    {
        public float ResourceTotal;
        public float CombatTotal;
        public uint TaskCount;
        public uint StarvationCount;
        public uint OscillationCount;
        public float ResourcesPerMinute;
        public float CombatPerMinute;
        public float TasksPerMinute;
        public uint CapabilityUsageBudgetFailures;
        public uint ActionFailureBudgetFailures;
        public byte BudgetsFailed;
        public uint LastSummaryTick;
        public byte SummaryDirty;
    }

    public struct GodgameTelemetryAccumulator : IComponentData
    {
        public uint DecisionTraceIndex;
        public uint ActionIndex;
        public uint CapabilityIndex;
        public uint GatherYieldIndex;
    }

    public struct GodgameTelemetryBudgets : IComponentData
    {
        public float MinResourcesPerMinute;
        public uint MaxStarvation;
        public uint MaxOscillation;
        public byte Enforce;
    }

    public static class GodgameTelemetryStringHelpers
    {
        public static FixedString64Bytes BuildAgentId(int villagerId)
        {
            FixedString64Bytes id = default;
            id.Append("villager/");
            id.Append(villagerId);
            return id;
        }

        public static FixedString64Bytes BuildEntityLabel(Entity entity)
        {
            FixedString64Bytes label = default;
            if (entity == Entity.Null)
            {
                return label;
            }

            label.Append("entity/");
            label.Append(entity.Index);
            label.Append('.');
            label.Append(entity.Version);
            return label;
        }

        public static uint HashContext(FixedString64Bytes value, uint seed)
        {
            var bytes = new FixedString128Bytes(value);
            unsafe
            {
                return math.hash(bytes.GetUnsafePtr(), value.Length) ^ seed;
            }
        }
    }
}
