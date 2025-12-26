using Godgame.Registry;
using Godgame.Resources;
using Godgame.Villagers;
using Godgame.Logistics;
using GodgameVillagerAttributes = Godgame.Villagers.VillagerAttributes;
using GodgameVillagerCombatStats = Godgame.Villagers.VillagerCombatStats;
using PureDOTS.Runtime.AI;
using PureDOTS.Runtime.Components;
using PureDOTS.Runtime.Combat;
using PureDOTS.Runtime.Resource;
using PureDOTS.Runtime.Telemetry;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Godgame.Villagers
{
    /// <summary>
    /// Villager job system handling Idle→Navigate→Gather→Deliver state machine.
    /// Runs in FixedStep group with parallel IJobEntity execution.
    /// </summary>
    [BurstCompile]
    [UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
    public partial struct VillagerJobSystem : ISystem
    {
        private ComponentLookup<GodgameResourceNodeMirror> _resourceNodeLookup;
        private ComponentLookup<GodgameStorehouse> _storehouseLookup;
        private BufferLookup<StorehouseInventoryItem> _inventoryLookup;
        private BufferLookup<StorehouseCapacityElement> _capacityLookup;
        private EntityQuery _resourceNodeQuery;
        private EntityQuery _storehouseQuery;
        private EntityQuery _pileQuery;
        private ComponentLookup<VillagerGoalState> _goalLookup;
        private ComponentLookup<HazardAvoidanceState> _hazardLookup;
        private ComponentLookup<VillagerResourceAwareness> _resourceAwarenessLookup;
        private ComponentLookup<VillagerCooperationIntent> _cooperationLookup;
        private ComponentLookup<AggregatePile> _pileLookup;
        private ComponentLookup<VillagerWorkRole> _roleLookup;
        private ComponentLookup<VillagerHaulPreference> _haulLookup;
        private ComponentLookup<TreeFellingProfile> _treeProfileLookup;
        private ComponentLookup<VillagerTreeSafetyMemory> _treeSafetyLookup;
        private ComponentLookup<VillagerAlignment> _alignmentLookup;
        private ComponentLookup<VillagerBehavior> _behaviorLookup;
        private ComponentLookup<GodgameVillagerAttributes> _attributesLookup;
        private ComponentLookup<VillagerDerivedAttributes> _derivedLookup;
        private ComponentLookup<GodgameVillagerCombatStats> _combatLookup;
        private ComponentLookup<VillagerNeedState> _needStateLookup;
        private ComponentLookup<JobTicket> _ticketLookup;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<VillagerJobState>();
            state.RequireForUpdate<ResourceTypeIndex>();
            state.RequireForUpdate<BehaviorConfigRegistry>();
            state.RequireForUpdate<VillagerScheduleConfig>();
            state.RequireForUpdate<JobAssignment>();
            _resourceNodeLookup = state.GetComponentLookup<GodgameResourceNodeMirror>(false);
            _storehouseLookup = state.GetComponentLookup<GodgameStorehouse>(isReadOnly: true);
            _inventoryLookup = state.GetBufferLookup<StorehouseInventoryItem>(false);
            _capacityLookup = state.GetBufferLookup<StorehouseCapacityElement>(isReadOnly: true);
            _goalLookup = state.GetComponentLookup<VillagerGoalState>(true);
            _hazardLookup = state.GetComponentLookup<HazardAvoidanceState>(true);
            _resourceAwarenessLookup = state.GetComponentLookup<VillagerResourceAwareness>(true);
            _cooperationLookup = state.GetComponentLookup<VillagerCooperationIntent>(true);
            _pileLookup = state.GetComponentLookup<AggregatePile>(false);
            _roleLookup = state.GetComponentLookup<VillagerWorkRole>(true);
            _haulLookup = state.GetComponentLookup<VillagerHaulPreference>(false);
            _treeProfileLookup = state.GetComponentLookup<TreeFellingProfile>(true);
            _treeSafetyLookup = state.GetComponentLookup<VillagerTreeSafetyMemory>(true);
            _alignmentLookup = state.GetComponentLookup<VillagerAlignment>(true);
            _behaviorLookup = state.GetComponentLookup<VillagerBehavior>(true);
            _attributesLookup = state.GetComponentLookup<GodgameVillagerAttributes>(true);
            _derivedLookup = state.GetComponentLookup<VillagerDerivedAttributes>(true);
            _combatLookup = state.GetComponentLookup<GodgameVillagerCombatStats>(false);
            _needStateLookup = state.GetComponentLookup<VillagerNeedState>(false);
            _ticketLookup = state.GetComponentLookup<JobTicket>(false);

            _resourceNodeQuery = SystemAPI.QueryBuilder()
                .WithAll<GodgameResourceNodeMirror, LocalTransform>()
                .Build();

            _storehouseQuery = SystemAPI.QueryBuilder()
                .WithAll<GodgameStorehouse, LocalTransform>()
                .Build();

            _pileQuery = SystemAPI.QueryBuilder()
                .WithAll<AggregatePile, LocalTransform>()
                .Build();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            if (SystemAPI.TryGetSingleton<RewindState>(out var rewindState) && rewindState.Mode != RewindMode.Record)
            {
                return;
            }

            if (!SystemAPI.TryGetSingleton<TimeState>(out var timeState))
            {
                return;
            }

            _resourceNodeLookup.Update(ref state);
            _storehouseLookup.Update(ref state);
            _inventoryLookup.Update(ref state);
            _capacityLookup.Update(ref state);
            _goalLookup.Update(ref state);
            _hazardLookup.Update(ref state);
            _resourceAwarenessLookup.Update(ref state);
            _cooperationLookup.Update(ref state);
            _pileLookup.Update(ref state);
            _roleLookup.Update(ref state);
            _haulLookup.Update(ref state);
            _treeProfileLookup.Update(ref state);
            _treeSafetyLookup.Update(ref state);
            _alignmentLookup.Update(ref state);
            _behaviorLookup.Update(ref state);
            _attributesLookup.Update(ref state);
            _derivedLookup.Update(ref state);
            _combatLookup.Update(ref state);
            _needStateLookup.Update(ref state);
            _ticketLookup.Update(ref state);

            var catalog = SystemAPI.GetSingleton<ResourceTypeIndex>().Catalog;
            if (!catalog.IsCreated)
            {
                return;
            }

            var behaviorConfig = SystemAPI.GetSingleton<BehaviorConfigRegistry>();
            var gatherConfig = behaviorConfig.GatherDeliver;
            var movementConfig = behaviorConfig.Movement;
            var gatherRate = gatherConfig.DefaultGatherRatePerSecond > 0f
                ? gatherConfig.DefaultGatherRatePerSecond
                : StepJob.DefaultGatherRate;
            var carryCapacityOverride = gatherConfig.CarryCapacityOverride;
            var returnThreshold = gatherConfig.ReturnThresholdPercent > 0f
                ? math.clamp(gatherConfig.ReturnThresholdPercent, 0.1f, 1f)
                : StepJob.DefaultReturnThreshold;
            var storehouseRadius = gatherConfig.StorehouseSearchRadius > 0f
                ? gatherConfig.StorehouseSearchRadius
                : StepJob.DefaultStorehouseRadius;
            var resourceScanRadius = math.max(StepJob.DefaultResourceScanRadius, storehouseRadius * 0.6f);
            var dropoffCooldown = math.max(0f, gatherConfig.DropoffCooldownSeconds);
            var arrivalDistance = movementConfig.ArrivalDistance > 0f
                ? movementConfig.ArrivalDistance
                : StepJob.DefaultArrivalDistance;
            var movementTuning = SystemAPI.TryGetSingleton<VillagerMovementTuning>(out var movementTuningValue)
                ? movementTuningValue
                : VillagerMovementTuning.Default;

            var scheduleConfig = SystemAPI.HasSingleton<VillagerScheduleConfig>()
                ? SystemAPI.GetSingleton<VillagerScheduleConfig>()
                : VillagerScheduleConfig.Default;

            var hasWorkTuning = SystemAPI.HasSingleton<VillagerWorkTuning>();
            var workTuning = hasWorkTuning ? SystemAPI.GetSingleton<VillagerWorkTuning>() : default;
            var pileDropMinUnits = math.max(0f, hasWorkTuning ? workTuning.PileDropMinUnits : 6f);
            var pileDropMaxUnits = hasWorkTuning ? workTuning.PileDropMaxUnits : 30f;
            if (pileDropMaxUnits > 0f)
            {
                pileDropMaxUnits = math.max(pileDropMaxUnits, pileDropMinUnits);
            }
            var pilePickupMinUnits = math.max(0f, hasWorkTuning ? workTuning.PilePickupMinUnits : 3f);
            var pileSearchRadius = hasWorkTuning ? workTuning.PileSearchRadius : 60f;
            var pileSearchRadiusSq = pileSearchRadius > 0f ? pileSearchRadius * pileSearchRadius : float.PositiveInfinity;

            var hasTreeTuning = SystemAPI.TryGetSingleton<TreeFellingTuning>(out var treeTuning);
            var hasTreeEventBuffer = SystemAPI.TryGetSingletonEntity<TreeFallEventBuffer>(out var treeEventEntity);
            if (hasTreeEventBuffer && !state.EntityManager.HasBuffer<TreeFallEvent>(treeEventEntity))
            {
                state.EntityManager.AddBuffer<TreeFallEvent>(treeEventEntity);
            }

            var hasPileConfig = SystemAPI.TryGetSingletonEntity<AggregatePileConfig>(out var pileConfigEntity);
            var pileMinSpawnAmount = 0f;
            if (hasPileConfig)
            {
                if (!state.EntityManager.HasBuffer<AggregatePileSpawnCommand>(pileConfigEntity))
                {
                    state.EntityManager.AddBuffer<AggregatePileSpawnCommand>(pileConfigEntity);
                }

                var pileConfig = SystemAPI.GetSingleton<AggregatePileConfig>();
                pileMinSpawnAmount = math.max(0f, pileConfig.MinSpawnAmount);
            }

            var ecb = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>()
                .CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter();

            var deltaTime = SystemAPI.Time.DeltaTime;

            // Collect resource nodes and storehouses for job
            var resourceNodeEntities = _resourceNodeQuery.ToEntityArray(state.WorldUpdateAllocator);
            var resourceNodeTransforms = _resourceNodeQuery.ToComponentDataArray<LocalTransform>(state.WorldUpdateAllocator);
            var resourceNodeMirrors = _resourceNodeQuery.ToComponentDataArray<GodgameResourceNodeMirror>(state.WorldUpdateAllocator);

            var storehouseEntities = _storehouseQuery.ToEntityArray(state.WorldUpdateAllocator);
            var storehouseTransforms = _storehouseQuery.ToComponentDataArray<LocalTransform>(state.WorldUpdateAllocator);

            var pileEntities = _pileQuery.ToEntityArray(state.WorldUpdateAllocator);
            var pileTransforms = _pileQuery.ToComponentDataArray<LocalTransform>(state.WorldUpdateAllocator);
            var pileData = _pileQuery.ToComponentDataArray<AggregatePile>(state.WorldUpdateAllocator);

            state.Dependency = new StepJob
            {
                Delta = deltaTime,
                Ecb = ecb,
                ResourceNodeLookup = _resourceNodeLookup,
                StorehouseLookup = _storehouseLookup,
                InventoryLookup = _inventoryLookup,
                CapacityLookup = _capacityLookup,
                Catalog = catalog,
                ResourceNodeEntities = resourceNodeEntities,
                ResourceNodeTransforms = resourceNodeTransforms,
                ResourceNodeMirrors = resourceNodeMirrors,
                StorehouseEntities = storehouseEntities,
                StorehouseTransforms = storehouseTransforms,
                PileEntities = pileEntities,
                PileTransforms = pileTransforms,
                PileData = pileData,
                GatherRatePerSecond = gatherRate,
                CarryCapacityOverride = carryCapacityOverride,
                ReturnThresholdPercent = returnThreshold,
                StorehouseSearchRadius = storehouseRadius,
                ResourceScanRadius = resourceScanRadius,
                DropoffCooldownSeconds = dropoffCooldown,
                ArrivalDistance = arrivalDistance,
                MoveSpeed = StepJob.DefaultMoveSpeed,
                GoalLookup = _goalLookup,
                HazardLookup = _hazardLookup,
                AwarenessLookup = _resourceAwarenessLookup,
                CooperationLookup = _cooperationLookup,
                PileLookup = _pileLookup,
                RoleLookup = _roleLookup,
                HaulLookup = _haulLookup,
                TreeProfileLookup = _treeProfileLookup,
                TreeSafetyLookup = _treeSafetyLookup,
                AlignmentLookup = _alignmentLookup,
                BehaviorLookup = _behaviorLookup,
                AttributesLookup = _attributesLookup,
                DerivedLookup = _derivedLookup,
                CombatLookup = _combatLookup,
                NeedLookup = _needStateLookup,
                TicketLookup = _ticketLookup,
                MovementTuning = movementTuning,
                HasTreeTuning = hasTreeTuning ? (byte)1 : (byte)0,
                TreeTuning = hasTreeTuning ? treeTuning : TreeFellingTuning.Default,
                HasTreeEventBuffer = hasTreeEventBuffer ? (byte)1 : (byte)0,
                TreeEventEntity = hasTreeEventBuffer ? treeEventEntity : Entity.Null,
                HasPileConfig = hasPileConfig ? (byte)1 : (byte)0,
                PileConfigEntity = hasPileConfig ? pileConfigEntity : Entity.Null,
                PileDropMinUnits = pileDropMinUnits,
                PileDropMaxUnits = pileDropMaxUnits,
                PilePickupMinUnits = pilePickupMinUnits,
                PileSearchRadiusSq = pileSearchRadiusSq,
                PileMinSpawnAmount = pileMinSpawnAmount,
                WorkSatisfactionPerDelivery = math.max(0f, scheduleConfig.WorkSatisfactionPerDelivery),
                DeliberationMinSeconds = math.max(0f, scheduleConfig.DeliberationMinSeconds),
                DeliberationMaxSeconds = math.max(0f, scheduleConfig.DeliberationMaxSeconds),
                CurrentTick = timeState.Tick,
                FixedDeltaTime = timeState.FixedDeltaTime
            }.Schedule(state.Dependency);

            state.Dependency.Complete();

            // Update resource node mirrors from the modified lookup
            for (int i = 0; i < resourceNodeEntities.Length; i++)
            {
                if (_resourceNodeLookup.HasComponent(resourceNodeEntities[i]))
                {
                    var updatedMirror = _resourceNodeLookup[resourceNodeEntities[i]];
                    state.EntityManager.SetComponentData(resourceNodeEntities[i], updatedMirror);
                }
            }

            for (int i = 0; i < pileEntities.Length; i++)
            {
                if (_pileLookup.HasComponent(pileEntities[i]))
                {
                    var updatedPile = _pileLookup[pileEntities[i]];
                    state.EntityManager.SetComponentData(pileEntities[i], updatedPile);
                }
            }
        }

        [BurstCompile]
        [WithNone(typeof(LogisticsHaulerTag))]
        public partial struct StepJob : IJobEntity
        {
            public const float DefaultGatherRate = 8f;
            public const float DefaultCarryCapacity = 50f;
            public const float DefaultReturnThreshold = 0.95f;
            public const float DefaultStorehouseRadius = 250f;
            public const float DefaultResourceScanRadius = 120f;
            public const float DefaultArrivalDistance = 2f;
            public const float DefaultDeliverDistance = 3f;
            public const float DefaultMoveSpeed = 2.5f;
            public const float DefaultCooperationNodeSpacing = 2.2f;
            public const float DefaultCooperationStorehouseSpacing = 3.6f;

            public float Delta;
            public EntityCommandBuffer.ParallelWriter Ecb;
            public ComponentLookup<GodgameResourceNodeMirror> ResourceNodeLookup;
            [ReadOnly] public ComponentLookup<GodgameStorehouse> StorehouseLookup;
            public BufferLookup<StorehouseInventoryItem> InventoryLookup;
            [ReadOnly] public BufferLookup<StorehouseCapacityElement> CapacityLookup;
            [ReadOnly] public BlobAssetReference<ResourceTypeIndexBlob> Catalog;

            [ReadOnly] public NativeArray<Entity> ResourceNodeEntities;
            [ReadOnly] public NativeArray<LocalTransform> ResourceNodeTransforms;
            public NativeArray<GodgameResourceNodeMirror> ResourceNodeMirrors;
            [ReadOnly] public NativeArray<Entity> StorehouseEntities;
            [ReadOnly] public NativeArray<LocalTransform> StorehouseTransforms;
            [ReadOnly] public NativeArray<Entity> PileEntities;
            [ReadOnly] public NativeArray<LocalTransform> PileTransforms;
            [ReadOnly] public NativeArray<AggregatePile> PileData;

            public float GatherRatePerSecond;
            public float CarryCapacityOverride;
            public float ReturnThresholdPercent;
            public float StorehouseSearchRadius;
            public float ResourceScanRadius;
            public float DropoffCooldownSeconds;
            public float ArrivalDistance;
            public float MoveSpeed;
            [ReadOnly] public ComponentLookup<VillagerGoalState> GoalLookup;
            [ReadOnly] public ComponentLookup<HazardAvoidanceState> HazardLookup;
            [ReadOnly] public ComponentLookup<VillagerResourceAwareness> AwarenessLookup;
            [ReadOnly] public ComponentLookup<VillagerCooperationIntent> CooperationLookup;
            public ComponentLookup<AggregatePile> PileLookup;
            [ReadOnly] public ComponentLookup<VillagerWorkRole> RoleLookup;
            public ComponentLookup<VillagerHaulPreference> HaulLookup;
            [ReadOnly] public ComponentLookup<TreeFellingProfile> TreeProfileLookup;
            [ReadOnly] public ComponentLookup<VillagerTreeSafetyMemory> TreeSafetyLookup;
            [ReadOnly] public ComponentLookup<VillagerAlignment> AlignmentLookup;
            [ReadOnly] public ComponentLookup<VillagerBehavior> BehaviorLookup;
            [ReadOnly] public ComponentLookup<GodgameVillagerAttributes> AttributesLookup;
            [ReadOnly] public ComponentLookup<VillagerDerivedAttributes> DerivedLookup;
            [NativeDisableParallelForRestriction] public ComponentLookup<GodgameVillagerCombatStats> CombatLookup;
            public ComponentLookup<VillagerNeedState> NeedLookup;
            [NativeDisableParallelForRestriction] public ComponentLookup<JobTicket> TicketLookup;
            public VillagerMovementTuning MovementTuning;
            public TreeFellingTuning TreeTuning;
            public byte HasTreeTuning;
            public byte HasTreeEventBuffer;
            public Entity TreeEventEntity;
            public Entity PileConfigEntity;
            public byte HasPileConfig;
            public float PileDropMinUnits;
            public float PileDropMaxUnits;
            public float PilePickupMinUnits;
            public float PileSearchRadiusSq;
            public float PileMinSpawnAmount;
            public float WorkSatisfactionPerDelivery;
            public float DeliberationMinSeconds;
            public float DeliberationMaxSeconds;
            public uint CurrentTick;
            public float FixedDeltaTime;

            [BurstCompile]
            void Execute([ChunkIndexInQuery] int ciq, Entity e, ref VillagerJobState job, ref JobAssignment assignment, ref LocalTransform tx, ref Navigation nav, ref GatherDeliverTelemetry telemetry)
            {
                var patienceScore = BehaviorLookup.HasComponent(e) ? BehaviorLookup[e].PatienceScore : 0f;

                if (GoalLookup.HasComponent(e))
                {
                    var goal = GoalLookup[e];
                    if (goal.CurrentGoal != VillagerGoal.Work)
                    {
                        ReleaseTicket(ref assignment, e, JobTicketState.Open);
                        ResetJob(ref job, e, patienceScore);
                        return;
                    }
                }

                JobTicket ticket = default;
                if (assignment.Ticket == Entity.Null)
                {
                    if (job.DecisionCooldown > 0f)
                    {
                        job.DecisionCooldown = math.max(0f, job.DecisionCooldown - Delta);
                    }
                    job.Phase = JobPhase.Idle;
                    job.Target = Entity.Null;
                    assignment.CommitTick = 0;
                    return;
                }

                if (!TicketLookup.HasComponent(assignment.Ticket))
                {
                    assignment.Ticket = Entity.Null;
                    assignment.CommitTick = 0;
                    ResetJob(ref job, e, patienceScore);
                    return;
                }

                ticket = TicketLookup[assignment.Ticket];
                if (ticket.Assignee != e)
                {
                    assignment.Ticket = Entity.Null;
                    assignment.CommitTick = 0;
                    ResetJob(ref job, e, patienceScore);
                    return;
                }

                if (ticket.State == JobTicketState.Cancelled || ticket.State == JobTicketState.Done)
                {
                    ReleaseTicket(ref assignment, e, ticket.State);
                    ResetJob(ref job, e, patienceScore);
                    return;
                }

                if (ticket.State == JobTicketState.Claimed
                    && ticket.ClaimExpiresTick != 0
                    && ticket.ClaimExpiresTick <= CurrentTick)
                {
                    ReleaseTicket(ref assignment, e, JobTicketState.Open);
                    ResetJob(ref job, e, patienceScore);
                    return;
                }

                if (job.Type == JobType.None)
                {
                    ReleaseTicket(ref assignment, e, JobTicketState.Open);
                    ResetJob(ref job, e, patienceScore);
                    return;
                }

                float3 hazardVector = float3.zero;
                float hazardUrgency = 0f;
                if (HazardLookup.HasComponent(e))
                {
                    var hazard = HazardLookup[e];
                    hazardVector = hazard.CurrentAdjustment;
                    hazardUrgency = hazard.AvoidanceUrgency;
                }

                var roleKind = RoleLookup.HasComponent(e) ? RoleLookup[e].Value : VillagerWorkRoleKind.None;
                var isHauler = roleKind == VillagerWorkRoleKind.Hauler;

                var ticketTarget = ticket.TargetEntity;
                var hasNodeTarget = ticketTarget != Entity.Null && ResourceNodeLookup.HasComponent(ticketTarget);
                var hasPileTarget = ticketTarget != Entity.Null && PileLookup.HasComponent(ticketTarget);
                if (!hasNodeTarget && !hasPileTarget)
                {
                    ReleaseTicket(ref assignment, e, JobTicketState.Cancelled);
                    ResetJob(ref job, e, patienceScore);
                    return;
                }

                if (hasNodeTarget)
                {
                    var node = ResourceNodeLookup[ticketTarget];
                    if (node.IsDepleted != 0 || node.RemainingAmount <= 0f)
                    {
                        ReleaseTicket(ref assignment, e, JobTicketState.Done);
                        ResetJob(ref job, e, patienceScore);
                        return;
                    }
                }

                if (hasPileTarget)
                {
                    var pile = PileLookup[ticketTarget];
                    if (pile.Amount <= 0f)
                    {
                        ReleaseTicket(ref assignment, e, JobTicketState.Done);
                        ResetJob(ref job, e, patienceScore);
                        return;
                    }
                }

                if (hasPileTarget && !isHauler)
                {
                    ReleaseTicket(ref assignment, e, JobTicketState.Open);
                    ResetJob(ref job, e, patienceScore);
                    return;
                }

                if (!hasPileTarget && isHauler)
                {
                    ReleaseTicket(ref assignment, e, JobTicketState.Open);
                    ResetJob(ref job, e, patienceScore);
                    return;
                }

                if (!isHauler && job.ResourceTypeIndex != ticket.ResourceTypeIndex)
                {
                    ReleaseTicket(ref assignment, e, JobTicketState.Open);
                    ResetJob(ref job, e, patienceScore);
                    return;
                }

                if (ticket.ResourceTypeIndex != ushort.MaxValue)
                {
                    if (job.ResourceTypeIndex == ushort.MaxValue || isHauler)
                    {
                        job.ResourceTypeIndex = ticket.ResourceTypeIndex;
                    }

                    if (job.OutputResourceTypeIndex == ushort.MaxValue)
                    {
                        job.OutputResourceTypeIndex = ticket.ResourceTypeIndex;
                    }
                }
                var hasHaulPreference = HaulLookup.HasComponent(e);
                var haulPreference = hasHaulPreference ? HaulLookup[e] : default;
                if (!hasHaulPreference)
                {
                    haulPreference.ForceHaul = 1;
                }

                if (job.ResourceTypeIndex == ushort.MaxValue && !isHauler)
                {
                    ReleaseTicket(ref assignment, e, JobTicketState.Open);
                    ResetJob(ref job, e, patienceScore);
                    return;
                }

                var gatherRate = GatherRatePerSecond > 0f ? GatherRatePerSecond : DefaultGatherRate;
                var carryCapacity = job.CarryMax > 0f
                    ? job.CarryMax
                    : (CarryCapacityOverride > 0f ? CarryCapacityOverride : DefaultCarryCapacity);
                if (job.CarryMax <= 0f)
                {
                    job.CarryMax = carryCapacity;
                }
                carryCapacity = math.max(carryCapacity, 1e-3f);
                job.CarryCount = math.clamp(job.CarryCount, 0f, carryCapacity);

                var returnThreshold = ReturnThresholdPercent > 0f
                    ? math.clamp(ReturnThresholdPercent, 0.1f, 1f)
                    : DefaultReturnThreshold;
                var storehouseRadiusSq = StorehouseSearchRadius > 0f
                    ? StorehouseSearchRadius * StorehouseSearchRadius
                    : float.PositiveInfinity;
                var arrivalDistance = ArrivalDistance > 0f ? ArrivalDistance : DefaultArrivalDistance;
                var deliverDistance = math.max(arrivalDistance, DefaultDeliverDistance);
                var baseMoveSpeed = MoveSpeed > 0f ? MoveSpeed : DefaultMoveSpeed;
                var statAverage = ResolveStatAverage(e);
                var hasStamina = TryGetStamina(e, out var combatStats, out var currentStamina, out var maxStamina, out var staminaRatio);
                var moveSpeed = ResolveMoveSpeed(e, baseMoveSpeed, hazardUrgency, statAverage, staminaRatio, out var runIntensity);
                nav.Speed = moveSpeed;
                var movedThisTick = false;

                if (job.DropoffCooldown > 0f)
                {
                    job.DropoffCooldown = math.max(0f, job.DropoffCooldown - Delta);
                    if (job.DropoffCooldown > 0f && job.Phase == JobPhase.Idle)
                    {
                        return;
                    }
                }

                var hasCooperation = CooperationLookup.HasComponent(e);
                var cooperation = hasCooperation ? CooperationLookup[e] : default;
                var useCooperation = hasCooperation
                    && cooperation.ResourceTypeIndex != ushort.MaxValue
                    && cooperation.Urgency > 0f;
                if (useCooperation && ticket.ResourceTypeIndex == ushort.MaxValue)
                {
                    job.ResourceTypeIndex = cooperation.ResourceTypeIndex;
                    if (job.OutputResourceTypeIndex == ushort.MaxValue)
                    {
                        job.OutputResourceTypeIndex = job.ResourceTypeIndex;
                    }
                }

                var hasAwareness = AwarenessLookup.HasComponent(e);
                var awareness = hasAwareness ? AwarenessLookup[e] : default;

                switch (job.Phase)
                {
                    case JobPhase.Idle:
                        if (job.DecisionCooldown > 0f)
                        {
                            job.DecisionCooldown = math.max(0f, job.DecisionCooldown - Delta);
                            break;
                        }
                        if (!TryResolveTicketTargetPosition(ticket.TargetEntity, out var ticketTargetPosition))
                        {
                            ReleaseTicket(ref assignment, e, JobTicketState.Cancelled);
                            EnterIdle(ref job, e, patienceScore, 0f);
                            break;
                        }

                        job.Target = ticket.TargetEntity;
                        var offset = useCooperation
                            ? BuildCooperationOffset(ticket.TargetEntity, e, DefaultCooperationNodeSpacing)
                            : float3.zero;
                        nav.Destination = ticketTargetPosition + offset;
                        nav.Speed = moveSpeed;
                        job.Phase = JobPhase.NavigateToNode;

                        if (ticket.State == JobTicketState.Claimed)
                        {
                            ticket.State = JobTicketState.InProgress;
                            ticket.LastStateTick = CurrentTick;
                            TicketLookup[assignment.Ticket] = ticket;
                        }
                        break;

                    case JobPhase.NavigateToNode:
                        var direction = nav.Destination - tx.Position;
                        var distance = math.length(direction);
                        if (distance > arrivalDistance)
                        {
                            var moveDir = VillagerSteeringMath.BlendDirection(direction, hazardVector, hazardUrgency);
                            var moveDelta = moveDir * nav.Speed * Delta;
                            tx.Position += moveDelta;
                            movedThisTick = true;
                        }
                        else
                        {
                            job.Phase = JobPhase.Gather;
                        }
                        break;

                    case JobPhase.Gather:
                        if (job.Target != Entity.Null && HasPileConfig != 0 && PileLookup.HasComponent(job.Target))
                        {
                            var pile = PileLookup[job.Target];
                            if (pile.Amount > 0f && job.CarryCount < carryCapacity)
                            {
                                var pickupAmount = math.min(pile.Amount, carryCapacity - job.CarryCount);
                                job.CarryCount = math.min(carryCapacity, job.CarryCount + pickupAmount);
                                pile.Amount = math.max(0f, pile.Amount - pickupAmount);
                                pile.UpdateVisualSize();
                                PileLookup[job.Target] = pile;
                                telemetry.CarrierCargoMilliSnapshot = BehaviorTelemetryMath.ToMilli(job.CarryCount);
                            }

                            if (job.CarryCount > 0f || pile.Amount <= 0f)
                            {
                                if (TryFindStorehouse(tx.Position, useCooperation, in cooperation, hasAwareness, in awareness, storehouseRadiusSq,
                                        out var nearestStorehouse, out var nearestStorehousePosition))
                                {
                                    job.Target = nearestStorehouse;
                                    var offset = useCooperation || (hasCooperation && cooperation.SharedStorehouse != Entity.Null)
                                        ? BuildCooperationOffset(nearestStorehouse, e, DefaultCooperationStorehouseSpacing)
                                        : float3.zero;
                                    nav.Destination = nearestStorehousePosition + offset;
                                    nav.Speed = moveSpeed;
                                    job.Phase = JobPhase.NavigateToStorehouse;
                                }
                                else
                                {
                                    job.CarryCount = 0f;
                                    ReleaseTicket(ref assignment, e, JobTicketState.Open);
                                    EnterIdle(ref job, e, patienceScore, 0f);
                                }
                            }
                            break;
                        }

                        if (job.Target != Entity.Null && ResourceNodeLookup.HasComponent(job.Target))
                        {
                            var nodeIndex = ResourceNodeEntities.IndexOf(job.Target);
                            if (nodeIndex >= 0)
                            {
                                var node = ResourceNodeMirrors[nodeIndex];
                                var wasDepleted = node.IsDepleted != 0 || node.RemainingAmount <= 0f;
                                var effectiveGatherRate = gatherRate;
                                var isTree = TreeProfileLookup.HasComponent(job.Target);
                                var treeProfile = isTree ? TreeProfileLookup[job.Target] : default;
                                var safetyFactor = 0.5f;

                                if (isTree)
                                {
                                    var difficulty = math.max(0.1f, treeProfile.ChopDifficulty);
                                    var speedMultiplier = 1f;
                                    var statMultiplier = 1f;
                                    if (HasTreeTuning != 0)
                                    {
                                        safetyFactor = ResolveTreeSafetyFactor(e, TreeTuning);
                                        speedMultiplier = math.lerp(TreeTuning.RiskySpeedMultiplier, TreeTuning.SafeSpeedMultiplier, safetyFactor);
                                        statMultiplier = ResolveTreeStatMultiplier(e, TreeTuning);
                                    }

                                    effectiveGatherRate = math.max(0.01f, gatherRate * speedMultiplier * statMultiplier / difficulty);
                                }

                                if (node.RemainingAmount > 0f && job.CarryCount < carryCapacity)
                                {
                                    var gatherAmount = math.min(effectiveGatherRate * Delta, math.min(node.RemainingAmount, carryCapacity - job.CarryCount));
                                    job.CarryCount = math.min(carryCapacity, job.CarryCount + gatherAmount);
                                    if (gatherAmount > 0f)
                                    {
                                        telemetry.MinedAmountMilliInterval += BehaviorTelemetryMath.ToMilli(gatherAmount);
                                        telemetry.CarrierCargoMilliSnapshot = BehaviorTelemetryMath.ToMilli(job.CarryCount);
                                    }
                                    node.RemainingAmount = math.max(0f, node.RemainingAmount - gatherAmount);
                                    if (node.RemainingAmount <= 0f)
                                    {
                                        node.IsDepleted = 1;
                                    }
                                    ResourceNodeLookup[job.Target] = node;

                                    if (!wasDepleted && node.IsDepleted != 0 && isTree && HasTreeEventBuffer != 0)
                                    {
                                        var treePosition = ResourceNodeTransforms[nodeIndex].Position;
                                        var fallDirection = ResolveTreeFallDirection(tx.Position, treePosition, safetyFactor);
                                        var fallLength = math.max(0.1f, treeProfile.FallLength);
                                        var fallWidth = math.max(0.05f, treeProfile.FallWidth);
                                        if (HasTreeTuning != 0)
                                        {
                                            fallLength *= math.lerp(TreeTuning.RiskyLengthMultiplier, TreeTuning.SafeLengthMultiplier, safetyFactor);
                                            fallWidth *= math.lerp(TreeTuning.RiskyWidthMultiplier, TreeTuning.SafeWidthMultiplier, safetyFactor);
                                        }

                                        Ecb.AppendToBuffer(ciq, TreeEventEntity, new TreeFallEvent
                                        {
                                            TreeEntity = job.Target,
                                            WorkerEntity = e,
                                            Position = treePosition,
                                            FallDirection = fallDirection,
                                            FallLength = fallLength,
                                            FallWidth = fallWidth,
                                            BaseDamage = treeProfile.BaseDamage,
                                            AwarenessRadius = treeProfile.AwarenessRadius,
                                            SafetyFactor = safetyFactor,
                                            TriggerTick = CurrentTick
                                        });
                                    }
                                }

                                if (job.CarryCount >= carryCapacity * returnThreshold || node.RemainingAmount <= 0f)
                                {
                                    var outputResourceIndex = ResolveOutputIndex(job);
                                    var shouldHaul = isHauler || haulPreference.ForceHaul != 0;

                                    if (!shouldHaul && hasHaulPreference)
                                    {
                                        shouldHaul = DetermineHaulDecision(e, in haulPreference);
                                        if (shouldHaul)
                                        {
                                            ApplyHaulCooldown(ref haulPreference);
                                            HaulLookup[e] = haulPreference;
                                        }
                                    }
                                    else if (shouldHaul && hasHaulPreference)
                                    {
                                        ApplyHaulCooldown(ref haulPreference);
                                        HaulLookup[e] = haulPreference;
                                    }

                                    var dropMax = PileDropMaxUnits > 0f ? PileDropMaxUnits : float.PositiveInfinity;
                                    if (!shouldHaul && job.CarryCount > dropMax)
                                    {
                                        shouldHaul = true;
                                        if (hasHaulPreference)
                                        {
                                            ApplyHaulCooldown(ref haulPreference);
                                            HaulLookup[e] = haulPreference;
                                        }
                                    }

                                    var dropMin = math.max(PileDropMinUnits, PileMinSpawnAmount);
                                    if (!shouldHaul && HasPileConfig != 0 && outputResourceIndex != ushort.MaxValue && job.CarryCount >= dropMin)
                                    {
                                        Ecb.AppendToBuffer(ciq, PileConfigEntity, new AggregatePileSpawnCommand
                                        {
                                            ResourceType = outputResourceIndex,
                                            Amount = job.CarryCount,
                                            Position = tx.Position
                                        });
                                        job.CarryCount = 0f;
                                        telemetry.CarrierCargoMilliSnapshot = BehaviorTelemetryMath.ToMilli(job.CarryCount);
                                        ReleaseTicket(ref assignment, e, ResolveCompletionState(ticket.TargetEntity));
                                        EnterIdle(ref job, e, patienceScore, DropoffCooldownSeconds);
                                        ApplyWorkSatisfaction(e);
                                        break;
                                    }

                                    if (TryFindStorehouse(tx.Position, useCooperation, in cooperation, hasAwareness, in awareness, storehouseRadiusSq,
                                            out var nearestStorehouse, out var nearestStorehousePosition))
                                    {
                                        job.Target = nearestStorehouse;
                                        var offset = useCooperation || (hasCooperation && cooperation.SharedStorehouse != Entity.Null)
                                            ? BuildCooperationOffset(nearestStorehouse, e, DefaultCooperationStorehouseSpacing)
                                            : float3.zero;
                                        nav.Destination = nearestStorehousePosition + offset;
                                        nav.Speed = moveSpeed;
                                        job.Phase = JobPhase.NavigateToStorehouse;
                                    }
                                else
                                {
                                    job.CarryCount = 0f;
                                    ReleaseTicket(ref assignment, e, JobTicketState.Open);
                                    EnterIdle(ref job, e, patienceScore, 0f);
                                }
                                }
                            }
                        }
                        break;

                    case JobPhase.NavigateToStorehouse:
                        direction = nav.Destination - tx.Position;
                        distance = math.length(direction);
                        if (distance > deliverDistance)
                        {
                            var moveDir = VillagerSteeringMath.BlendDirection(direction, hazardVector, hazardUrgency);
                            var moveDelta = moveDir * nav.Speed * Delta;
                            tx.Position += moveDelta;
                            movedThisTick = true;
                        }
                        else
                        {
                            job.Phase = JobPhase.Deliver;
                        }
                        break;

                    case JobPhase.Deliver:
                        if (job.Target != Entity.Null && InventoryLookup.HasBuffer(job.Target) && CapacityLookup.HasBuffer(job.Target))
                        {
                            var inventory = InventoryLookup[job.Target];
                            var capacities = CapacityLookup[job.Target];

                            float capacity = 1000f;
                            var outputResourceIndex = ResolveOutputIndex(job);
                            if (outputResourceIndex == ushort.MaxValue)
                            {
                                job.CarryCount = 0f;
                            }
                            else
                            {
                                var resourceId = Godgame.Resources.StorehouseApi.ResolveResourceId(Catalog, outputResourceIndex);
                                for (int i = 0; i < capacities.Length; i++)
                                {
                                    if (capacities[i].ResourceTypeId.Equals(resourceId))
                                    {
                                        capacity = capacities[i].MaxCapacity;
                                        break;
                                    }
                                }

                                var depositAmount = job.CarryCount;
                                var deposited = Godgame.Resources.StorehouseApi.TryDeposit(ref inventory, Catalog, outputResourceIndex, depositAmount, capacity);
                                if (deposited && depositAmount > 0f)
                                {
                                    telemetry.DepositedAmountMilliInterval += BehaviorTelemetryMath.ToMilli(depositAmount);
                                    job.CarryCount = 0f;
                                }
                            }
                        }
                        else
                        {
                            job.CarryCount = 0f;
                        }

                        telemetry.CarrierCargoMilliSnapshot = BehaviorTelemetryMath.ToMilli(job.CarryCount);

                        ReleaseTicket(ref assignment, e, ResolveCompletionState(ticket.TargetEntity));
                        EnterIdle(ref job, e, patienceScore, DropoffCooldownSeconds);
                        ApplyWorkSatisfaction(e);
                        break;
                }

                if (hasStamina)
                {
                    currentStamina = UpdateStamina(currentStamina, maxStamina, statAverage, runIntensity, movedThisTick);
                    combatStats.CurrentStamina = (byte)math.round(currentStamina);
                    CombatLookup[e] = combatStats;
                }
            }

            private static ushort ResolveOutputIndex(in VillagerJobState job)
            {
                if (job.OutputResourceTypeIndex == ushort.MaxValue)
                {
                    return job.ResourceTypeIndex;
                }

                if (job.OutputResourceTypeIndex == 0 && job.ResourceTypeIndex != 0)
                {
                    return job.ResourceTypeIndex;
                }

                return job.OutputResourceTypeIndex;
            }

            private void ReleaseTicket(ref JobAssignment assignment, Entity entity, JobTicketState nextState)
            {
                if (assignment.Ticket == Entity.Null)
                {
                    return;
                }

                if (TicketLookup.HasComponent(assignment.Ticket))
                {
                    var ticket = TicketLookup[assignment.Ticket];
                    if (ticket.Assignee == entity)
                    {
                        ticket.Assignee = Entity.Null;
                        ticket.State = nextState;
                        ticket.ClaimExpiresTick = 0;
                        ticket.LastStateTick = CurrentTick;
                        TicketLookup[assignment.Ticket] = ticket;
                    }
                }

                assignment.Ticket = Entity.Null;
                assignment.CommitTick = 0;
            }

            private JobTicketState ResolveCompletionState(Entity target)
            {
                if (target != Entity.Null)
                {
                    if (ResourceNodeLookup.HasComponent(target))
                    {
                        var node = ResourceNodeLookup[target];
                        if (node.IsDepleted != 0 || node.RemainingAmount <= 0f)
                        {
                            return JobTicketState.Done;
                        }

                        return JobTicketState.Open;
                    }

                    if (PileLookup.HasComponent(target))
                    {
                        var pile = PileLookup[target];
                        if (pile.Amount <= 0f)
                        {
                            return JobTicketState.Done;
                        }

                        return JobTicketState.Open;
                    }
                }

                return JobTicketState.Cancelled;
            }

            private bool TryResolveTicketTargetPosition(Entity target, out float3 position)
            {
                position = float3.zero;
                if (target == Entity.Null)
                {
                    return false;
                }

                var nodeIndex = ResourceNodeEntities.IndexOf(target);
                if (nodeIndex >= 0)
                {
                    position = ResourceNodeTransforms[nodeIndex].Position;
                    return true;
                }

                var pileIndex = PileEntities.IndexOf(target);
                if (pileIndex >= 0)
                {
                    position = PileTransforms[pileIndex].Position;
                    return true;
                }

                return false;
            }

            private bool DetermineHaulDecision(Entity entity, in VillagerHaulPreference haulPreference)
            {
                if (haulPreference.ForceHaul != 0)
                {
                    return true;
                }

                if (CurrentTick < haulPreference.NextHaulAllowedTick)
                {
                    return false;
                }

                var chance = math.clamp(haulPreference.HaulChance, 0f, 1f);
                if (chance <= 0f)
                {
                    return false;
                }

                if (chance >= 1f)
                {
                    return true;
                }

                var seed = math.hash(new uint2((uint)(entity.Index + 1), CurrentTick + 97u));
                var random = Unity.Mathematics.Random.CreateFromIndex(seed == 0u ? 1u : seed);
                return random.NextFloat() <= chance;
            }

            private void ApplyHaulCooldown(ref VillagerHaulPreference haulPreference)
            {
                if (haulPreference.HaulCooldownSeconds <= 0f)
                {
                    return;
                }

                var secondsPerTick = math.max(FixedDeltaTime, 1e-4f);
                var cooldownTicks = (uint)math.ceil(haulPreference.HaulCooldownSeconds / secondsPerTick);
                if (cooldownTicks > 0u)
                {
                    haulPreference.NextHaulAllowedTick = CurrentTick + cooldownTicks;
                }
            }

            private float ResolveTreeSafetyFactor(Entity entity, in TreeFellingTuning tuning)
            {
                var safety = tuning.BaseSafety;

                if (AlignmentLookup.HasComponent(entity))
                {
                    var alignment = AlignmentLookup[entity];
                    safety += (alignment.OrderAxis / 100f) * tuning.AlignmentSafetyWeight;
                }

                if (BehaviorLookup.HasComponent(entity))
                {
                    var behavior = BehaviorLookup[entity];
                    safety += (-behavior.BoldScore / 100f) * tuning.BoldSafetyWeight;
                }

                if (TreeSafetyLookup.HasComponent(entity))
                {
                    var memory = TreeSafetyLookup[entity];
                    safety += memory.CautionBias * tuning.MemorySafetyWeight;
                }

                var min = math.min(tuning.MinSafetyFactor, tuning.MaxSafetyFactor);
                var max = math.max(tuning.MinSafetyFactor, tuning.MaxSafetyFactor);
                return math.clamp(safety, min, max);
            }

            private float ResolveTreeStatMultiplier(Entity entity, in TreeFellingTuning tuning)
            {
                if (!DerivedLookup.HasComponent(entity))
                {
                    return 1f;
                }

                var derived = DerivedLookup[entity];
                var strengthDelta = (derived.Strength - 50) * tuning.StrengthRateScalar;
                var agilityDelta = (derived.Agility - 50) * tuning.AgilityRateScalar;
                return math.max(0.25f, 1f + strengthDelta + agilityDelta);
            }

            private static float3 ResolveTreeFallDirection(float3 workerPosition, float3 treePosition, float safetyFactor)
            {
                var awayFromWorker = math.normalizesafe(treePosition - workerPosition, new float3(0f, 0f, 1f));
                var towardWorker = -awayFromWorker;
                return math.normalizesafe(math.lerp(towardWorker, awayFromWorker, math.saturate(safetyFactor)), awayFromWorker);
            }

            private bool TryFindNearestPile(float3 origin, ushort desiredType, float radiusSq, float minUnits,
                out Entity pileEntity, out float3 position, out ushort resourceType)
            {
                pileEntity = Entity.Null;
                position = float3.zero;
                resourceType = ushort.MaxValue;
                float best = float.MaxValue;

                for (int i = 0; i < PileEntities.Length; i++)
                {
                    var pile = PileData[i];
                    if (pile.Amount < minUnits)
                    {
                        continue;
                    }

                    if (desiredType != ushort.MaxValue && pile.ResourceTypeIndex != desiredType)
                    {
                        continue;
                    }

                    float distSq = math.distancesq(origin, PileTransforms[i].Position);
                    if (distSq > radiusSq)
                    {
                        continue;
                    }

                    if (distSq < best)
                    {
                        best = distSq;
                        pileEntity = PileEntities[i];
                        position = PileTransforms[i].Position;
                        resourceType = pile.ResourceTypeIndex;
                    }
                }

                return pileEntity != Entity.Null;
            }

            private bool TryFindStorehouse(float3 origin, bool useCooperation, in VillagerCooperationIntent cooperation,
                bool hasAwareness, in VillagerResourceAwareness awareness, float storehouseRadiusSq,
                out Entity storehouse, out float3 storehousePosition)
            {
                storehouse = Entity.Null;
                storehousePosition = float3.zero;
                float minDistSq = float.MaxValue;

                if (useCooperation && cooperation.SharedStorehouse != Entity.Null)
                {
                    storehouse = cooperation.SharedStorehouse;
                    int storehouseIndex = StorehouseEntities.IndexOf(storehouse);
                    if (storehouseIndex >= 0)
                    {
                        storehousePosition = StorehouseTransforms[storehouseIndex].Position;
                        minDistSq = math.distancesq(origin, storehousePosition);
                    }
                    else
                    {
                        storehouse = Entity.Null;
                    }
                }
                else if (hasAwareness && awareness.KnownStorehouse != Entity.Null)
                {
                    storehouse = awareness.KnownStorehouse;
                    int storehouseIndex = StorehouseEntities.IndexOf(storehouse);
                    if (storehouseIndex >= 0)
                    {
                        storehousePosition = StorehouseTransforms[storehouseIndex].Position;
                        minDistSq = math.distancesq(origin, storehousePosition);
                    }
                    else
                    {
                        storehouse = Entity.Null;
                    }
                }

                if (storehouse == Entity.Null)
                {
                    for (int i = 0; i < StorehouseEntities.Length; i++)
                    {
                        float distSq = math.distancesq(origin, StorehouseTransforms[i].Position);
                        if (distSq <= storehouseRadiusSq && distSq < minDistSq)
                        {
                            minDistSq = distSq;
                            storehouse = StorehouseEntities[i];
                            storehousePosition = StorehouseTransforms[i].Position;
                        }
                    }
                }

                return storehouse != Entity.Null;
            }

            private void ResetJob(ref VillagerJobState job, Entity entity, float patienceScore)
            {
                EnterIdle(ref job, entity, patienceScore, 0f);
            }

            private void EnterIdle(ref VillagerJobState job, Entity entity, float patienceScore, float dropoffCooldown)
            {
                job.Phase = JobPhase.Idle;
                job.Target = Entity.Null;
                job.DropoffCooldown = dropoffCooldown > 0f ? dropoffCooldown : 0f;
                job.DecisionCooldown = ResolveDeliberationSeconds(entity, patienceScore);
                job.LastDecisionTick = CurrentTick;
            }

            private float ResolveDeliberationSeconds(Entity entity, float patienceScore)
            {
                var min = math.max(0f, DeliberationMinSeconds);
                var max = math.max(min, DeliberationMaxSeconds);
                if (max <= 0f)
                {
                    return 0f;
                }

                var patience01 = math.saturate((patienceScore + 100f) * 0.005f);
                var seed = math.hash(new uint2((uint)(entity.Index + 1), CurrentTick + 17u));
                var random = Unity.Mathematics.Random.CreateFromIndex(seed == 0u ? 1u : seed);
                var sample = random.NextFloat(min, max);
                return math.lerp(min, sample, math.lerp(0.6f, 1.2f, patience01));
            }

            private void ApplyWorkSatisfaction(Entity entity)
            {
                if (WorkSatisfactionPerDelivery <= 0f || !NeedLookup.HasComponent(entity))
                {
                    return;
                }

                var needs = NeedLookup[entity];
                needs.WorkUrgency = math.max(0f, needs.WorkUrgency - WorkSatisfactionPerDelivery);
                NeedLookup[entity] = needs;
            }

            private float ResolveStatAverage(Entity entity)
            {
                float physique = 50f;
                float finesse = 50f;
                float agility = 50f;

                if (AttributesLookup.HasComponent(entity))
                {
                    var attributes = AttributesLookup[entity];
                    physique = attributes.Physique;
                    finesse = attributes.Finesse;
                }

                if (DerivedLookup.HasComponent(entity))
                {
                    agility = DerivedLookup[entity].Agility;
                }

                return (physique + finesse + agility) / 3f;
            }

            private bool TryGetStamina(Entity entity, out GodgameVillagerCombatStats combatStats, out float currentStamina,
                out float maxStamina, out float staminaRatio)
            {
                combatStats = default;
                currentStamina = 0f;
                maxStamina = 0f;
                staminaRatio = 1f;

                if (!CombatLookup.HasComponent(entity))
                {
                    return false;
                }

                combatStats = CombatLookup[entity];
                if (combatStats.Stamina == 0)
                {
                    return false;
                }

                maxStamina = math.max(1f, combatStats.Stamina);
                currentStamina = math.clamp(combatStats.CurrentStamina, 0f, maxStamina);
                staminaRatio = currentStamina / maxStamina;
                return true;
            }

            private float ResolveMoveSpeed(Entity entity, float baseSpeed, float hazardUrgency, float statAverage,
                float staminaRatio, out float runIntensity)
            {
                var walkMultiplier = math.max(0.05f, MovementTuning.WalkSpeedMultiplier);
                var runMultiplier = math.max(walkMultiplier, MovementTuning.RunSpeedMultiplier);
                var statMultiplier = math.max(0.1f, 1f + (statAverage - 50f) * MovementTuning.StatSpeedScalar);
                var walkSpeed = baseSpeed * walkMultiplier * statMultiplier;
                var runSpeed = baseSpeed * runMultiplier * statMultiplier;

                var patienceScore = 0f;
                if (BehaviorLookup.HasComponent(entity))
                {
                    patienceScore = BehaviorLookup[entity].PatienceScore;
                }

                var patience01 = math.saturate((patienceScore + 100f) * 0.005f);
                var patienceWeight = math.clamp(MovementTuning.PatienceSpeedWeight, 0f, 1f);
                var speedBlend = math.lerp(0.5f, 1f - patience01, patienceWeight);
                speedBlend = math.max(speedBlend, math.saturate(hazardUrgency));

                var staminaThreshold = math.clamp(MovementTuning.StaminaRunThreshold, 0f, 1f);
                if (staminaThreshold > 0f)
                {
                    var staminaBlend = math.saturate((staminaRatio - staminaThreshold) / math.max(1f - staminaThreshold, 1e-3f));
                    speedBlend = math.min(speedBlend, staminaBlend);
                }

                var moveSpeed = math.lerp(walkSpeed, runSpeed, speedBlend);
                runIntensity = runSpeed > walkSpeed
                    ? math.saturate((moveSpeed - walkSpeed) / math.max(0.001f, runSpeed - walkSpeed))
                    : 0f;
                return math.max(0.05f, moveSpeed);
            }

            private float UpdateStamina(float currentStamina, float maxStamina, float statAverage, float runIntensity, bool movedThisTick)
            {
                var efficiency = math.clamp(1f + (statAverage - 50f) * MovementTuning.StatStaminaEfficiencyScalar, 0.2f, 2f);
                var regenScale = movedThisTick ? (1f - runIntensity) : 1f;
                currentStamina += MovementTuning.StaminaRecoverPerSecond * regenScale * Delta * efficiency;
                currentStamina -= MovementTuning.StaminaDrainPerSecond * runIntensity * Delta / efficiency;
                return math.clamp(currentStamina, 0f, maxStamina);
            }

            private static float3 BuildCooperationOffset(Entity target, Entity self, float spacing)
            {
                if (target == Entity.Null || spacing <= 0f)
                {
                    return float3.zero;
                }

                var seed = math.hash(new uint2((uint)(target.Index + 1), (uint)(self.Index + 17)));
                var random = Unity.Mathematics.Random.CreateFromIndex(seed == 0u ? 1u : seed);
                var angle = random.NextFloat(0f, math.PI * 2f);
                var radius = random.NextFloat(0.4f, 1f) * spacing;
                return new float3(math.cos(angle) * radius, 0f, math.sin(angle) * radius);
            }
        }
    }
}
