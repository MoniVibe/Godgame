using PureDOTS.Runtime.AI;
using PureDOTS.Runtime.Telemetry;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Godgame.Villagers;

namespace Godgame.Telemetry
{
    public enum GodgameDecisionReason : byte
    {
        Unknown = 0,
        ResourceShortage = 1,
        LowMorale = 2,
        GrowthPhaseExpansion = 3,
        StableMaintenance = 4,
        MicroNeedPressure = 5,
        ThreatReported = 6
    }

    public struct GodgameDecisionScoreEntry
    {
        public FixedString64Bytes Label;
        public float Score;
    }

    [InternalBufferCapacity(32)]
    public struct GodgameDecisionTransitionRecord : IBufferElementData
    {
        public uint Tick;
        public FixedString64Bytes AgentId;
        public byte OldState;
        public byte NewState;
        public GodgameDecisionReason Reason;
        public Entity Target;
        public FixedList128Bytes<GodgameDecisionScoreEntry> Scores;
        public byte Priority;
    }

    public enum GodgameActionLifecycleEvent : byte
    {
        Start = 0,
        End = 1,
        Fail = 2
    }

    public enum GodgameActionFailureReason : byte
    {
        None = 0,
        PathBlocked = 1,
        ResourceMissing = 2,
        InvalidTarget = 3,
        Cooldown = 4,
        Cancelled = 5
    }

    public enum GodgameVillagerLivelinessEvent : byte
    {
        StartedWork = 0,
        CooldownStart = 1,
        WanderStart = 2,
        SocializeStart = 3,
        CooldownCleared = 4
    }

    [InternalBufferCapacity(64)]
    public struct GodgameVillagerLivelinessRecord : IBufferElementData
    {
        public uint Tick;
        public FixedString64Bytes AgentId;
        public GodgameVillagerLivelinessEvent Event;
        public Entity Target;
        public VillagerWorkCooldownMode CooldownMode;
        public uint CooldownRemainingTicks;
        public VillagerWorkCooldownMode LeisureAction;
        public Entity LeisureTarget;
        public VillagerCooldownClearReason CooldownClearReason;
    }

    [InternalBufferCapacity(64)]
    public struct GodgameActionLifecycleRecord : IBufferElementData
    {
        public uint Tick;
        public uint ActionId;
        public FixedString64Bytes AgentId;
        public JobPhase Phase;
        public GodgameActionLifecycleEvent Event;
        public uint DurationTicks;
        public GodgameActionFailureReason FailureReason;
        public Entity Target;
    }

    [InternalBufferCapacity(32)]
    public struct GodgameQueuePressureRecord : IBufferElementData
    {
        public uint Tick;
        public FixedString64Bytes QueueName;
        public ushort Length;
        public ushort DuplicateCount;
        public ushort StaleCount;
        public float AverageWaitTicks;
    }

    public enum GodgameLogicAuditKind : byte
    {
        InvalidEntityReference = 0,
        NaNTransform = 1,
        MissingComponent = 2,
        InvalidState = 3
    }

    [InternalBufferCapacity(32)]
    public struct GodgameLogicAuditRecord : IBufferElementData
    {
        public uint Tick;
        public GodgameLogicAuditKind Kind;
        public int Count;
        public FixedString64Bytes Details;
    }

    public enum GodgameTicketAuditEvent : byte
    {
        DoubleClaim = 0,
        ClaimReleased = 1,
        ClaimStuck = 2
    }

    [InternalBufferCapacity(32)]
    public struct GodgameTicketClaimRecord : IBufferElementData
    {
        public uint Tick;
        public GodgameTicketAuditEvent Event;
        public uint TicketId;
        public FixedString64Bytes AgentId;
        public uint DurationTicks;
        public FixedString64Bytes Details;
    }

    public struct VillagerActionTelemetryState : IComponentData
    {
        public JobPhase LastPhase;
        public uint PhaseStartTick;
        public uint ActiveActionId;
        public Entity LastTarget;
        public float LastCarryCount;
        public ushort LastResourceTypeIndex;
        public uint GatherStartTick;
        public float GatherStartCarry;
        public float3 GatherStartPosition;
        public uint HaulStartTick;
        public float3 HaulStartPosition;
        public float HaulStartCarry;
    }

    /// <summary>
    /// Ensures the behavior telemetry singleton owns buffers for AI audit records.
    /// </summary>
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    public partial struct GodgameAIAuditBootstrapSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<BehaviorTelemetryState>();
        }

        public void OnUpdate(ref SystemState state)
        {
            var telemetryEntity = SystemAPI.GetSingletonEntity<BehaviorTelemetryState>();
            var entityManager = state.EntityManager;

            EnsureBuffer<GodgameDecisionTransitionRecord>(entityManager, telemetryEntity);
            EnsureBuffer<GodgameDecisionTraceRecord>(entityManager, telemetryEntity);
            EnsureBuffer<GodgameDecisionOscillationState>(entityManager, telemetryEntity);
            EnsureBuffer<GodgameActionLifecycleRecord>(entityManager, telemetryEntity);
            EnsureBuffer<GodgameActionEffectRecord>(entityManager, telemetryEntity);
            EnsureBuffer<GodgameQueuePressureRecord>(entityManager, telemetryEntity);
            EnsureBuffer<GodgameLogicAuditRecord>(entityManager, telemetryEntity);
            EnsureBuffer<GodgameTicketClaimRecord>(entityManager, telemetryEntity);
            EnsureBuffer<GodgameVillagerLivelinessRecord>(entityManager, telemetryEntity);
            EnsureBuffer<GodgameCapabilityGrantedRecord>(entityManager, telemetryEntity);
            EnsureBuffer<GodgameCapabilityRevokedRecord>(entityManager, telemetryEntity);
            EnsureBuffer<GodgameCapabilitySnapshotRecord>(entityManager, telemetryEntity);
            EnsureBuffer<GodgameCapabilityUsageSample>(entityManager, telemetryEntity);
            EnsureBuffer<GodgameActionFailureSample>(entityManager, telemetryEntity);
            EnsureBuffer<GodgameGatherAttemptRecord>(entityManager, telemetryEntity);
            EnsureBuffer<GodgameGatherYieldRecord>(entityManager, telemetryEntity);
            EnsureBuffer<GodgameGatherFailureRecord>(entityManager, telemetryEntity);
            EnsureBuffer<GodgameHaulTripRecord>(entityManager, telemetryEntity);
            EnsureComponent(entityManager, telemetryEntity, new GodgameTelemetrySummary());
            EnsureComponent(entityManager, telemetryEntity, new GodgameTelemetryAccumulator());
            EnsureComponent(entityManager, telemetryEntity, new GodgameTelemetryBudgets
            {
                MinResourcesPerMinute = 25f,
                MaxStarvation = 0,
                MaxOscillation = 5,
                Enforce = 1
            });

            state.Enabled = false;
        }

        private static void EnsureBuffer<T>(EntityManager entityManager, Entity target) where T : unmanaged, IBufferElementData
        {
            if (!entityManager.HasBuffer<T>(target))
            {
                entityManager.AddBuffer<T>(target);
            }
        }

        private static void EnsureComponent<T>(EntityManager entityManager, Entity target, T defaultValue = default) where T : unmanaged, IComponentData
        {
            if (!entityManager.HasComponent<T>(target))
            {
                entityManager.AddComponentData(target, defaultValue);
            }
        }
    }
}
