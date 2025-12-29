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
using PureDOTS.Runtime.Villagers;
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
        private EntityQuery _ticketQuery;
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
        private ComponentLookup<VillagerNeedBias> _needBiasLookup;
        private ComponentLookup<VillagerArchetypeResolved> _archetypeLookup;
        private ComponentLookup<VillagerOutlook> _outlookLookup;
        private BufferLookup<VillagerCooldownOutlookRule> _cooldownOutlookLookup;
        private BufferLookup<VillagerCooldownArchetypeModifier> _cooldownArchetypeLookup;
        private ComponentLookup<GodgameVillagerAttributes> _attributesLookup;
        private ComponentLookup<VillagerDerivedAttributes> _derivedLookup;
        private ComponentLookup<GodgameVillagerCombatStats> _combatLookup;
        private ComponentLookup<VillagerNeedState> _needStateLookup;
        private ComponentLookup<VillagerNeedTuning> _needTuningLookup;
        private ComponentLookup<JobTicket> _ticketLookup;
        private BufferLookup<JobTicketGroupMember> _groupLookup;
        private ComponentLookup<LocalTransform> _transformLookup;
        private ComponentLookup<VillagerCarryCapacity> _carryLookup;
        private ComponentLookup<VillagerPonderState> _ponderLookup;
        private BufferLookup<VillagerJobDecisionEvent> _jobTraceLookup;

        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<VillagerJobState>();
            state.RequireForUpdate<ResourceTypeIndex>();
            state.RequireForUpdate<BehaviorConfigRegistry>();
            state.RequireForUpdate<VillagerScheduleConfig>();
            state.RequireForUpdate<JobAssignment>();
            state.RequireForUpdate<GodgameJobTicketTuning>();
            state.RequireForUpdate<VillagerCooldownProfile>();
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
            _needBiasLookup = state.GetComponentLookup<VillagerNeedBias>(true);
            _archetypeLookup = state.GetComponentLookup<VillagerArchetypeResolved>(true);
            _outlookLookup = state.GetComponentLookup<VillagerOutlook>(true);
            _cooldownOutlookLookup = state.GetBufferLookup<VillagerCooldownOutlookRule>(true);
            _cooldownArchetypeLookup = state.GetBufferLookup<VillagerCooldownArchetypeModifier>(true);
            _attributesLookup = state.GetComponentLookup<GodgameVillagerAttributes>(true);
            _derivedLookup = state.GetComponentLookup<VillagerDerivedAttributes>(true);
            _combatLookup = state.GetComponentLookup<GodgameVillagerCombatStats>(false);
            _needStateLookup = state.GetComponentLookup<VillagerNeedState>(false);
            _needTuningLookup = state.GetComponentLookup<VillagerNeedTuning>(true);
            _ticketLookup = state.GetComponentLookup<JobTicket>(false);
            _groupLookup = state.GetBufferLookup<JobTicketGroupMember>(true);
            _transformLookup = state.GetComponentLookup<LocalTransform>(true);
            _carryLookup = state.GetComponentLookup<VillagerCarryCapacity>(true);
            _ponderLookup = state.GetComponentLookup<VillagerPonderState>(false);
            _jobTraceLookup = state.GetBufferLookup<VillagerJobDecisionEvent>(false);

            _resourceNodeQuery = SystemAPI.QueryBuilder()
                .WithAll<GodgameResourceNodeMirror, LocalTransform>()
                .Build();

            _storehouseQuery = SystemAPI.QueryBuilder()
                .WithAll<GodgameStorehouse, LocalTransform>()
                .Build();

            _pileQuery = SystemAPI.QueryBuilder()
                .WithAll<AggregatePile, LocalTransform>()
                .Build();

            _ticketQuery = SystemAPI.QueryBuilder()
                .WithAll<JobTicket>()
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
            _needBiasLookup.Update(ref state);
            _archetypeLookup.Update(ref state);
            _outlookLookup.Update(ref state);
            _cooldownOutlookLookup.Update(ref state);
            _cooldownArchetypeLookup.Update(ref state);
            _attributesLookup.Update(ref state);
            _derivedLookup.Update(ref state);
            _combatLookup.Update(ref state);
            _needStateLookup.Update(ref state);
            _needTuningLookup.Update(ref state);
            _ticketLookup.Update(ref state);
            _groupLookup.Update(ref state);
            _transformLookup.Update(ref state);
            _carryLookup.Update(ref state);
            _ponderLookup.Update(ref state);
            _jobTraceLookup.Update(ref state);

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
            var separationRadius = math.max(0f, movementTuning.SeparationRadius);
            var separationWeight = math.max(0f, movementTuning.SeparationWeight);
            var separationMaxPush = math.max(0f, movementTuning.SeparationMaxPush);
            var separationCellSize = math.max(0.1f, movementTuning.SeparationCellSize);

            var scheduleConfig = SystemAPI.HasSingleton<VillagerScheduleConfig>()
                ? SystemAPI.GetSingleton<VillagerScheduleConfig>()
                : VillagerScheduleConfig.Default;
            var jobTicketTuning = SystemAPI.GetSingleton<GodgameJobTicketTuning>();
            var minCommitSeconds = math.max(0f, jobTicketTuning.MinCommitSeconds);
            var secondsPerTick = math.max(timeState.FixedDeltaTime, 1e-4f);
            var minCommitTicks = (uint)math.max(0f, math.ceil(minCommitSeconds / secondsPerTick));
            var failureBackoffBaseTicks = (uint)math.max(1f, math.ceil(StepJob.DefaultFailureBackoffSeconds / secondsPerTick));
            var failureBackoffMaxTicks = (uint)math.max(failureBackoffBaseTicks,
                math.ceil(StepJob.DefaultFailureBackoffMaxSeconds / secondsPerTick));
            var cooldownProfile = SystemAPI.GetSingleton<VillagerCooldownProfile>();
            var cooldownProfileEntity = SystemAPI.GetSingletonEntity<VillagerCooldownProfile>();

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

            var villagerQuery = SystemAPI.QueryBuilder()
                .WithAll<VillagerJobState, LocalTransform>()
                .WithNone<HandHeldTag, Godgame.Runtime.HandQueuedTag>()
                .Build();
            var villagerEntities = villagerQuery.ToEntityArray(state.WorldUpdateAllocator);
            var villagerTransforms = villagerQuery.ToComponentDataArray<LocalTransform>(state.WorldUpdateAllocator);
            var villagerSpatialMap = new NativeParallelMultiHashMap<int, int>(math.max(1, villagerEntities.Length * 2), Allocator.TempJob);
            BuildSeparationMap(villagerTransforms, separationCellSize, villagerSpatialMap);

            var ticketCount = math.max(1, _ticketQuery.CalculateEntityCount());
            var targetPositions = new NativeParallelHashMap<Entity, float3>(ticketCount * 2, Allocator.TempJob);
            foreach (var ticket in SystemAPI.Query<RefRO<JobTicket>>())
            {
                var target = ticket.ValueRO.TargetEntity;
                if (target == Entity.Null || targetPositions.ContainsKey(target))
                {
                    continue;
                }

                if (_transformLookup.HasComponent(target))
                {
                    targetPositions.TryAdd(target, _transformLookup[target].Position);
                }
            }

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
                NeedBiasLookup = _needBiasLookup,
                ArchetypeLookup = _archetypeLookup,
                OutlookLookup = _outlookLookup,
                CooldownOutlookLookup = _cooldownOutlookLookup,
                CooldownArchetypeLookup = _cooldownArchetypeLookup,
                CooldownProfile = cooldownProfile,
                CooldownProfileEntity = cooldownProfileEntity,
                AttributesLookup = _attributesLookup,
                DerivedLookup = _derivedLookup,
                CombatLookup = _combatLookup,
                NeedLookup = _needStateLookup,
                NeedTuningLookup = _needTuningLookup,
                TicketLookup = _ticketLookup,
                GroupLookup = _groupLookup,
                TargetPositions = targetPositions,
                CarryLookup = _carryLookup,
                PonderLookup = _ponderLookup,
                JobTraceLookup = _jobTraceLookup,
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
                WorkCompletionWorkDrop = math.max(0f, scheduleConfig.WorkCompletionWorkDrop),
                WorkCompletionRestBoost = math.max(0f, scheduleConfig.WorkCompletionRestBoost),
                WorkCompletionSocialBoost = math.max(0f, scheduleConfig.WorkCompletionSocialBoost),
                WorkCompletionFaithBoost = math.max(0f, scheduleConfig.WorkCompletionFaithBoost),
                DeliberationMinSeconds = math.max(0f, scheduleConfig.DeliberationMinSeconds),
                DeliberationMaxSeconds = math.max(0f, scheduleConfig.DeliberationMaxSeconds),
                GroupCohesionBase = jobTicketTuning.GroupCohesionBase,
                GroupCohesionOrderWeight = jobTicketTuning.GroupCohesionOrderWeight,
                GroupCohesionMin = jobTicketTuning.GroupCohesionMin,
                GroupCohesionMax = jobTicketTuning.GroupCohesionMax,
                FailureBackoffBaseTicks = failureBackoffBaseTicks,
                FailureBackoffMaxTicks = failureBackoffMaxTicks,
                MinCommitTicks = minCommitTicks,
                CurrentTick = timeState.Tick,
                FixedDeltaTime = timeState.FixedDeltaTime,
                WorldSeconds = timeState.WorldSeconds,
                VillagerEntities = villagerEntities,
                VillagerTransforms = villagerTransforms,
                VillagerSpatialMap = villagerSpatialMap,
                SeparationRadius = separationRadius,
                SeparationWeight = separationWeight,
                SeparationMaxPush = separationMaxPush,
                SeparationCellSize = separationCellSize
            }.Schedule(state.Dependency);

            state.Dependency = targetPositions.Dispose(state.Dependency);
            state.Dependency = villagerSpatialMap.Dispose(state.Dependency);

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
        [WithNone(typeof(LogisticsHaulerTag), typeof(HandHeldTag), typeof(Godgame.Runtime.HandQueuedTag))]
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
            public const float DefaultFailureBackoffSeconds = 0.75f;
            public const float DefaultFailureBackoffMaxSeconds = 6f;
            private const byte PreconditionHasTicket = 1 << 0;
            private const byte PreconditionTargetValid = 1 << 1;
            private const byte PreconditionGroupReady = 1 << 2;
            private const byte PreconditionRoleMatch = 1 << 3;
            private const byte PreconditionResourceMatch = 1 << 4;
            private const byte PreconditionStorehouse = 1 << 5;

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
            [ReadOnly] public ComponentLookup<VillagerNeedBias> NeedBiasLookup;
            [ReadOnly] public ComponentLookup<VillagerArchetypeResolved> ArchetypeLookup;
            [ReadOnly] public ComponentLookup<VillagerOutlook> OutlookLookup;
            [ReadOnly] public BufferLookup<VillagerCooldownOutlookRule> CooldownOutlookLookup;
            [ReadOnly] public BufferLookup<VillagerCooldownArchetypeModifier> CooldownArchetypeLookup;
            public VillagerCooldownProfile CooldownProfile;
            public Entity CooldownProfileEntity;
            [ReadOnly] public ComponentLookup<GodgameVillagerAttributes> AttributesLookup;
            [ReadOnly] public ComponentLookup<VillagerDerivedAttributes> DerivedLookup;
            [NativeDisableParallelForRestriction] public ComponentLookup<GodgameVillagerCombatStats> CombatLookup;
            public ComponentLookup<VillagerNeedState> NeedLookup;
            [ReadOnly] public ComponentLookup<VillagerNeedTuning> NeedTuningLookup;
            [NativeDisableParallelForRestriction] public ComponentLookup<JobTicket> TicketLookup;
            [ReadOnly] public BufferLookup<JobTicketGroupMember> GroupLookup;
            [ReadOnly] public NativeParallelHashMap<Entity, float3> TargetPositions;
            [ReadOnly] public ComponentLookup<VillagerCarryCapacity> CarryLookup;
            public ComponentLookup<VillagerPonderState> PonderLookup;
            [NativeDisableParallelForRestriction] public BufferLookup<VillagerJobDecisionEvent> JobTraceLookup;
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
            public float WorkCompletionWorkDrop;
            public float WorkCompletionRestBoost;
            public float WorkCompletionSocialBoost;
            public float WorkCompletionFaithBoost;
            public float DeliberationMinSeconds;
            public float DeliberationMaxSeconds;
            public float GroupCohesionBase;
            public float GroupCohesionOrderWeight;
            public float GroupCohesionMin;
            public float GroupCohesionMax;
            public uint FailureBackoffBaseTicks;
            public uint FailureBackoffMaxTicks;
            public uint MinCommitTicks;
            public uint CurrentTick;
            public float FixedDeltaTime;
            public float WorldSeconds;
            [ReadOnly] public NativeArray<Entity> VillagerEntities;
            [ReadOnly] public NativeArray<LocalTransform> VillagerTransforms;
            [ReadOnly] public NativeParallelMultiHashMap<int, int> VillagerSpatialMap;
            public float SeparationRadius;
            public float SeparationWeight;
            public float SeparationMaxPush;
            public float SeparationCellSize;

            [BurstCompile]
            void Execute([ChunkIndexInQuery] int ciq, Entity e, ref VillagerJobState job, ref VillagerWorkCooldown workCooldown, ref JobAssignment assignment,
                DynamicBuffer<JobBatchEntry> batch,
                ref LocalTransform tx, ref Navigation nav, ref GatherDeliverTelemetry telemetry, ref MoveIntent moveIntent, ref MovePlan movePlan)
            {
                var patienceScore = BehaviorLookup.HasComponent(e) ? BehaviorLookup[e].PatienceScore : 0f;

                if (assignment.Ticket == Entity.Null && batch.Length > 0)
                {
                    var nextTicket = batch[0].Ticket;
                    batch.RemoveAt(0);
                    assignment.Ticket = nextTicket;
                    assignment.CommitTick = CurrentTick;
                }

                if (GoalLookup.HasComponent(e))
                {
                    var goal = GoalLookup[e];
                    if (goal.CurrentGoal != VillagerGoal.Work)
                    {
                        var committedTicks = CurrentTick > assignment.CommitTick
                            ? CurrentTick - assignment.CommitTick
                            : 0u;
                        if (assignment.Ticket != Entity.Null && MinCommitTicks > 0 && committedTicks < MinCommitTicks)
                        {
                            // Honor short commitment window to avoid thrash on goal flips.
                        }
                        else
                        {
                            ReleaseTicket(ref assignment, batch, e, JobTicketState.Open);
                            ResetJob(ref job, e, patienceScore, tx.Position);
                            nav.Velocity = float3.zero;
                            moveIntent = new MoveIntent
                            {
                                TargetEntity = Entity.Null,
                                TargetPosition = tx.Position,
                                IntentType = MoveIntentType.None
                            };
                            movePlan = default;
                            return;
                        }
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
                    nav.Velocity = float3.zero;
                    moveIntent = new MoveIntent
                    {
                        TargetEntity = Entity.Null,
                        TargetPosition = tx.Position,
                        IntentType = MoveIntentType.None
                    };
                    movePlan = default;
                    return;
                }

                if (!TicketLookup.HasComponent(assignment.Ticket))
                {
                    var preconditionMask = SetMask(0, PreconditionHasTicket, true);
                    RecordFailure(ref job, e, VillagerJobFailCode.TicketInvalid, assignment.Ticket, preconditionMask);
                    assignment.Ticket = Entity.Null;
                    assignment.CommitTick = 0;
                    ResetJob(ref job, e, patienceScore, tx.Position);
                    nav.Velocity = float3.zero;
                    moveIntent = new MoveIntent
                    {
                        TargetEntity = Entity.Null,
                        TargetPosition = tx.Position,
                        IntentType = MoveIntentType.None
                    };
                    movePlan = default;
                    return;
                }

                ticket = TicketLookup[assignment.Ticket];
                if (!IsTicketAssignedToEntity(assignment.Ticket, ticket, e))
                {
                    var preconditionMask = SetMask(0, PreconditionHasTicket, true);
                    RecordFailure(ref job, e, VillagerJobFailCode.TicketInvalid, assignment.Ticket, preconditionMask);
                    assignment.Ticket = Entity.Null;
                    assignment.CommitTick = 0;
                    ResetJob(ref job, e, patienceScore, tx.Position);
                    nav.Velocity = float3.zero;
                    moveIntent = new MoveIntent
                    {
                        TargetEntity = Entity.Null,
                        TargetPosition = tx.Position,
                        IntentType = MoveIntentType.None
                    };
                    movePlan = default;
                    return;
                }

                if (ticket.State == JobTicketState.Cancelled || ticket.State == JobTicketState.Done)
                {
                    var preconditionMask = SetMask(0, PreconditionHasTicket, true);
                    if (ticket.State == JobTicketState.Cancelled)
                    {
                        RecordFailure(ref job, e, VillagerJobFailCode.TicketCancelled, assignment.Ticket, preconditionMask);
                    }
                    else
                    {
                        RecordCompletion(ref job, e, assignment.Ticket, preconditionMask);
                    }
                    ReleaseTicket(ref assignment, batch, e, ticket.State);
                    ResetJob(ref job, e, patienceScore, tx.Position);
                    nav.Velocity = float3.zero;
                    moveIntent = new MoveIntent
                    {
                        TargetEntity = Entity.Null,
                        TargetPosition = tx.Position,
                        IntentType = MoveIntentType.None
                    };
                    movePlan = default;
                    return;
                }

                if (ticket.State == JobTicketState.Claimed
                    && ticket.ClaimExpiresTick != 0
                    && ticket.ClaimExpiresTick <= CurrentTick)
                {
                    var preconditionMask = SetMask(0, PreconditionHasTicket, true);
                    RecordFailure(ref job, e, VillagerJobFailCode.Timeout, assignment.Ticket, preconditionMask);
                    ReleaseTicket(ref assignment, batch, e, JobTicketState.Open);
                    ResetJob(ref job, e, patienceScore, tx.Position);
                    return;
                }

                if (ticket.IsSingleItem == 0 && ticket.WorkAmount <= 0f)
                {
                    var preconditionMask = SetMask(0, PreconditionHasTicket, true);
                    RecordCompletion(ref job, e, assignment.Ticket, preconditionMask);
                    ReleaseTicket(ref assignment, batch, e, JobTicketState.Done);
                    ResetJob(ref job, e, patienceScore, tx.Position);
                    return;
                }

                if (job.Type == JobType.None)
                {
                    var preconditionMask = SetMask(0, PreconditionHasTicket, true);
                    RecordFailure(ref job, e, VillagerJobFailCode.NoTicket, assignment.Ticket, preconditionMask);
                    ReleaseTicket(ref assignment, batch, e, JobTicketState.Open);
                    ResetJob(ref job, e, patienceScore, tx.Position);
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
                var hasGenericTarget = ticket.IsSingleItem != 0 && ticketTarget != Entity.Null && TargetPositions.ContainsKey(ticketTarget);
                if (!hasNodeTarget && !hasPileTarget && !hasGenericTarget)
                {
                    var preconditionMask = SetMask(0, PreconditionHasTicket, true);
                    preconditionMask = SetMask(preconditionMask, PreconditionTargetValid, false);
                    RecordFailure(ref job, e, VillagerJobFailCode.TargetInvalid, ticketTarget, preconditionMask);
                    ReleaseTicket(ref assignment, batch, e, JobTicketState.Cancelled);
                    ResetJob(ref job, e, patienceScore, tx.Position);
                    return;
                }

                if (hasNodeTarget && ticket.IsSingleItem == 0)
                {
                    var node = ResourceNodeLookup[ticketTarget];
                    if (node.IsDepleted != 0 || node.RemainingAmount <= 0f)
                    {
                        var preconditionMask = SetMask(0, PreconditionHasTicket, true);
                        preconditionMask = SetMask(preconditionMask, PreconditionTargetValid, true);
                        RecordFailure(ref job, e, VillagerJobFailCode.TargetDepleted, ticketTarget, preconditionMask);
                        ReleaseTicket(ref assignment, batch, e, JobTicketState.Done);
                        ResetJob(ref job, e, patienceScore, tx.Position);
                        return;
                    }
                }

                if (hasPileTarget && ticket.IsSingleItem == 0)
                {
                    var pile = PileLookup[ticketTarget];
                    if (pile.Amount <= 0f)
                    {
                        var preconditionMask = SetMask(0, PreconditionHasTicket, true);
                        preconditionMask = SetMask(preconditionMask, PreconditionTargetValid, true);
                        RecordFailure(ref job, e, VillagerJobFailCode.TargetDepleted, ticketTarget, preconditionMask);
                        ReleaseTicket(ref assignment, batch, e, JobTicketState.Done);
                        ResetJob(ref job, e, patienceScore, tx.Position);
                        return;
                    }
                }

                if (ticket.IsSingleItem == 0 && hasPileTarget && !isHauler)
                {
                    var preconditionMask = SetMask(0, PreconditionHasTicket, true);
                    preconditionMask = SetMask(preconditionMask, PreconditionRoleMatch, false);
                    RecordFailure(ref job, e, VillagerJobFailCode.RoleMismatch, ticketTarget, preconditionMask);
                    ReleaseTicket(ref assignment, batch, e, JobTicketState.Open);
                    ResetJob(ref job, e, patienceScore, tx.Position);
                    return;
                }

                if (ticket.IsSingleItem == 0 && !hasPileTarget && isHauler)
                {
                    var preconditionMask = SetMask(0, PreconditionHasTicket, true);
                    preconditionMask = SetMask(preconditionMask, PreconditionRoleMatch, false);
                    RecordFailure(ref job, e, VillagerJobFailCode.RoleMismatch, ticketTarget, preconditionMask);
                    ReleaseTicket(ref assignment, batch, e, JobTicketState.Open);
                    ResetJob(ref job, e, patienceScore, tx.Position);
                    return;
                }

                if (ticket.IsSingleItem == 0 && !isHauler && job.ResourceTypeIndex != ticket.ResourceTypeIndex)
                {
                    var preconditionMask = SetMask(0, PreconditionHasTicket, true);
                    preconditionMask = SetMask(preconditionMask, PreconditionResourceMatch, false);
                    RecordFailure(ref job, e, VillagerJobFailCode.ResourceMismatch, ticketTarget, preconditionMask);
                    ReleaseTicket(ref assignment, batch, e, JobTicketState.Open);
                    ResetJob(ref job, e, patienceScore, tx.Position);
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
                var groupReady = IsGroupReady(assignment.Ticket, ticket);
                var hasHaulPreference = HaulLookup.HasComponent(e);
                var haulPreference = hasHaulPreference ? HaulLookup[e] : default;
                if (!hasHaulPreference)
                {
                    haulPreference.ForceHaul = 1;
                }

                if (ticket.IsSingleItem == 0 && job.ResourceTypeIndex == ushort.MaxValue && !isHauler)
                {
                    var preconditionMask = SetMask(0, PreconditionHasTicket, true);
                    preconditionMask = SetMask(preconditionMask, PreconditionResourceMatch, false);
                    RecordFailure(ref job, e, VillagerJobFailCode.ResourceMismatch, ticketTarget, preconditionMask);
                    ReleaseTicket(ref assignment, batch, e, JobTicketState.Open);
                    ResetJob(ref job, e, patienceScore, tx.Position);
                    return;
                }

                var gatherRate = GatherRatePerSecond > 0f ? GatherRatePerSecond : DefaultGatherRate;
                var carryCapacity = job.CarryMax > 0f
                    ? job.CarryMax
                    : (CarryCapacityOverride > 0f ? CarryCapacityOverride : DefaultCarryCapacity);
                if (CarryLookup.HasComponent(e))
                {
                    var modifier = CarryLookup[e];
                    carryCapacity = carryCapacity * math.max(0.1f, modifier.Multiplier) + modifier.Bonus;
                }
                if (job.CarryMax <= 0f)
                {
                    job.CarryMax = carryCapacity;
                }
                carryCapacity = math.max(carryCapacity, 1e-3f);
                job.CarryCount = math.clamp(job.CarryCount, 0f, carryCapacity);

                var returnThreshold = ReturnThresholdPercent > 0f
                    ? math.clamp(ReturnThresholdPercent, 0.1f, 1f)
                    : DefaultReturnThreshold;
                var ticketCarryTarget = ResolveTicketCarryTarget(ticket, carryCapacity);
                var returnCarryThreshold = carryCapacity * returnThreshold;
                if (ticketCarryTarget > 0f)
                {
                    returnCarryThreshold = math.min(returnCarryThreshold, ticketCarryTarget);
                }
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
                var accelMultiplier = math.max(0.1f, MovementTuning.AccelerationMultiplier);
                var decelMultiplier = math.max(0.1f, MovementTuning.DecelerationMultiplier);
                var turnBlendSpeed = math.max(0.1f, MovementTuning.TurnBlendSpeed);
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
                            var preconditionMask = SetMask(0, PreconditionHasTicket, true);
                            preconditionMask = SetMask(preconditionMask, PreconditionTargetValid, false);
                            RecordFailure(ref job, e, VillagerJobFailCode.TargetInvalid, ticket.TargetEntity, preconditionMask);
                            ReleaseTicket(ref assignment, batch, e, JobTicketState.Cancelled);
                            EnterIdle(ref job, e, patienceScore, 0f, tx.Position);
                            break;
                        }

                        job.Target = ticket.TargetEntity;
                        var nodeOffset = useCooperation
                            ? BuildCooperationOffset(ticket.TargetEntity, e, DefaultCooperationNodeSpacing)
                            : float3.zero;
                        nav.Destination = ticketTargetPosition + nodeOffset;
                        nav.Speed = moveSpeed;
                        job.Phase = JobPhase.NavigateToNode;

                        var decisionMask = SetMask(0, PreconditionHasTicket, true);
                        decisionMask = SetMask(decisionMask, PreconditionTargetValid, true);
                        decisionMask = SetMask(decisionMask, PreconditionGroupReady, groupReady);
                        decisionMask = SetMask(decisionMask, PreconditionRoleMatch, true);
                        decisionMask = SetMask(decisionMask, PreconditionResourceMatch, true);
                        RecordDecision(ref job, e, job.Phase, ticket.TargetEntity, default, decisionMask);

                        if (ticket.State == JobTicketState.Claimed && groupReady)
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
                            var separation = ResolveSeparation(e, tx.Position);
                            moveDir = math.normalizesafe(moveDir + separation);
                            var travelSpeed = ApplyArriveSlowdown(nav.Speed, distance);
                            var currentVelocity = nav.Velocity;
                            currentVelocity.y = 0f;
                            if (math.lengthsq(currentVelocity) > 1e-4f)
                            {
                                var currentDir = math.normalizesafe(currentVelocity);
                                var turnLerp = math.saturate(Delta * turnBlendSpeed);
                                moveDir = math.normalizesafe(math.lerp(currentDir, moveDir, turnLerp), moveDir);
                            }

                            var desiredVelocity = moveDir * travelSpeed;
                            var currentSpeed = math.length(currentVelocity);
                            var acceleration = math.max(0.1f, nav.Speed * accelMultiplier);
                            var deceleration = math.max(0.1f, nav.Speed * decelMultiplier);
                            var accelLimit = math.length(desiredVelocity) > currentSpeed ? acceleration : deceleration;
                            var maxDelta = accelLimit * Delta;
                            var deltaV = desiredVelocity - currentVelocity;
                            var deltaSq = math.lengthsq(deltaV);
                            if (maxDelta > 0f && deltaSq > maxDelta * maxDelta)
                            {
                                deltaV = math.normalizesafe(deltaV) * maxDelta;
                            }

                            currentVelocity += deltaV;
                            nav.Velocity = currentVelocity;
                            tx.Position += currentVelocity * Delta;
                            movedThisTick = math.lengthsq(currentVelocity) > 1e-5f;
                        }
                        else if (groupReady)
                        {
                            nav.Velocity = float3.zero;
                            job.Phase = JobPhase.Gather;
                        }
                        break;

                    case JobPhase.Gather:
                        if (ticket.IsSingleItem != 0 && ticket.ItemMass > 0f)
                        {
                            if (ticket.Assignee == e)
                            {
                                job.CarryCount = math.max(job.CarryCount, ticket.ItemMass);
                            }
                            else
                            {
                                job.CarryCount = 0f;
                            }

                            if (TryFindStorehouse(tx.Position, useCooperation, in cooperation, hasAwareness, in awareness, storehouseRadiusSq,
                                    out var nearestStorehouse, out var nearestStorehousePosition, out var storehouseCandidates))
                            {
                                job.Target = nearestStorehouse;
                                var storehouseOffset = useCooperation || (hasCooperation && cooperation.SharedStorehouse != Entity.Null)
                                    ? BuildCooperationOffset(nearestStorehouse, e, DefaultCooperationStorehouseSpacing)
                                    : float3.zero;
                                nav.Destination = nearestStorehousePosition + storehouseOffset;
                                nav.Speed = moveSpeed;
                                job.Phase = JobPhase.NavigateToStorehouse;

                                var decisionMask = SetMask(0, PreconditionStorehouse, true);
                                RecordDecision(ref job, e, job.Phase, nearestStorehouse, storehouseCandidates, decisionMask);
                            }
                            else
                            {
                                var preconditionMask = SetMask(0, PreconditionStorehouse, false);
                                RecordFailure(ref job, e, VillagerJobFailCode.NoStorehouse, Entity.Null, preconditionMask);
                                job.CarryCount = 0f;
                                ReleaseTicket(ref assignment, batch, e, JobTicketState.Open);
                                EnterIdle(ref job, e, patienceScore, 0f, tx.Position);
                            }
                            break;
                        }

                        if (job.Target != Entity.Null && HasPileConfig != 0 && PileLookup.HasComponent(job.Target))
                        {
                            var pile = PileLookup[job.Target];
                            if (pile.Amount > 0f && job.CarryCount < carryCapacity)
                            {
                                var pickupAmount = math.min(pile.Amount, carryCapacity - job.CarryCount);
                                if (ticket.WorkAmount > 0f)
                                {
                                    pickupAmount = math.min(pickupAmount, ticket.WorkAmount);
                                }

                                if (pickupAmount > 0f)
                                {
                                    job.CarryCount = math.min(carryCapacity, job.CarryCount + pickupAmount);
                                    pile.Amount = math.max(0f, pile.Amount - pickupAmount);
                                    if (ticket.WorkAmount > 0f && ticket.Assignee == e)
                                    {
                                        ticket.WorkAmount = math.max(0f, ticket.WorkAmount - pickupAmount);
                                        TicketLookup[assignment.Ticket] = ticket;
                                    }
                                }
                                pile.UpdateVisualSize();
                                PileLookup[job.Target] = pile;
                                telemetry.CarrierCargoMilliSnapshot = BehaviorTelemetryMath.ToMilli(job.CarryCount);
                            }

                            if ((ticketCarryTarget > 0f && job.CarryCount >= ticketCarryTarget)
                                || (ticketCarryTarget <= 0f && job.CarryCount > 0f)
                                || pile.Amount <= 0f)
                            {
                                if (TryFindStorehouse(tx.Position, useCooperation, in cooperation, hasAwareness, in awareness, storehouseRadiusSq,
                                        out var nearestStorehouse, out var nearestStorehousePosition, out var storehouseCandidates))
                                {
                                    job.Target = nearestStorehouse;
                                    var storehouseOffset = useCooperation || (hasCooperation && cooperation.SharedStorehouse != Entity.Null)
                                        ? BuildCooperationOffset(nearestStorehouse, e, DefaultCooperationStorehouseSpacing)
                                        : float3.zero;
                                    nav.Destination = nearestStorehousePosition + storehouseOffset;
                                    nav.Speed = moveSpeed;
                                    job.Phase = JobPhase.NavigateToStorehouse;

                                    var decisionMask = SetMask(0, PreconditionStorehouse, true);
                                    RecordDecision(ref job, e, job.Phase, nearestStorehouse, storehouseCandidates, decisionMask);
                                }
                                else
                                {
                                    var preconditionMask = SetMask(0, PreconditionStorehouse, false);
                                    RecordFailure(ref job, e, VillagerJobFailCode.NoStorehouse, Entity.Null, preconditionMask);
                                    job.CarryCount = 0f;
                                    ReleaseTicket(ref assignment, batch, e, JobTicketState.Open);
                                    EnterIdle(ref job, e, patienceScore, 0f, tx.Position);
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
                                    if (ticket.WorkAmount > 0f)
                                    {
                                        gatherAmount = math.min(gatherAmount, ticket.WorkAmount);
                                    }
                                    job.CarryCount = math.min(carryCapacity, job.CarryCount + gatherAmount);
                                    if (gatherAmount > 0f)
                                    {
                                        telemetry.MinedAmountMilliInterval += BehaviorTelemetryMath.ToMilli(gatherAmount);
                                        telemetry.CarrierCargoMilliSnapshot = BehaviorTelemetryMath.ToMilli(job.CarryCount);
                                        if (ticket.WorkAmount > 0f && ticket.Assignee == e)
                                        {
                                            ticket.WorkAmount = math.max(0f, ticket.WorkAmount - gatherAmount);
                                            TicketLookup[assignment.Ticket] = ticket;
                                        }
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

                                if (job.CarryCount >= returnCarryThreshold || node.RemainingAmount <= 0f)
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
                                        RecordCompletion(ref job, e, ticket.TargetEntity, 0);
                                        ReleaseTicket(ref assignment, batch, e, ResolveCompletionState(ticket.TargetEntity, ticket));
                                        EnterIdle(ref job, e, patienceScore, DropoffCooldownSeconds, tx.Position);
                                        StartWorkCooldown(ref workCooldown, e, job.Type, patienceScore);
                                        ApplyWorkSatisfaction(e);
                                        break;
                                    }

                                    if (TryFindStorehouse(tx.Position, useCooperation, in cooperation, hasAwareness, in awareness, storehouseRadiusSq,
                                            out var nearestStorehouse, out var nearestStorehousePosition, out var storehouseCandidates))
                                    {
                                        job.Target = nearestStorehouse;
                                        var storehouseOffset = useCooperation || (hasCooperation && cooperation.SharedStorehouse != Entity.Null)
                                            ? BuildCooperationOffset(nearestStorehouse, e, DefaultCooperationStorehouseSpacing)
                                            : float3.zero;
                                        nav.Destination = nearestStorehousePosition + storehouseOffset;
                                        nav.Speed = moveSpeed;
                                        job.Phase = JobPhase.NavigateToStorehouse;

                                        var decisionMask = SetMask(0, PreconditionStorehouse, true);
                                        RecordDecision(ref job, e, job.Phase, nearestStorehouse, storehouseCandidates, decisionMask);
                                    }
                                else
                                {
                                    var preconditionMask = SetMask(0, PreconditionStorehouse, false);
                                    RecordFailure(ref job, e, VillagerJobFailCode.NoStorehouse, Entity.Null, preconditionMask);
                                    job.CarryCount = 0f;
                                    ReleaseTicket(ref assignment, batch, e, JobTicketState.Open);
                                    EnterIdle(ref job, e, patienceScore, 0f, tx.Position);
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
                            var separation = ResolveSeparation(e, tx.Position);
                            moveDir = math.normalizesafe(moveDir + separation);
                            var travelSpeed = ApplyArriveSlowdown(nav.Speed, distance);
                            var currentVelocity = nav.Velocity;
                            currentVelocity.y = 0f;
                            if (math.lengthsq(currentVelocity) > 1e-4f)
                            {
                                var currentDir = math.normalizesafe(currentVelocity);
                                var turnLerp = math.saturate(Delta * turnBlendSpeed);
                                moveDir = math.normalizesafe(math.lerp(currentDir, moveDir, turnLerp), moveDir);
                            }

                            var desiredVelocity = moveDir * travelSpeed;
                            var currentSpeed = math.length(currentVelocity);
                            var acceleration = math.max(0.1f, nav.Speed * accelMultiplier);
                            var deceleration = math.max(0.1f, nav.Speed * decelMultiplier);
                            var accelLimit = math.length(desiredVelocity) > currentSpeed ? acceleration : deceleration;
                            var maxDelta = accelLimit * Delta;
                            var deltaV = desiredVelocity - currentVelocity;
                            var deltaSq = math.lengthsq(deltaV);
                            if (maxDelta > 0f && deltaSq > maxDelta * maxDelta)
                            {
                                deltaV = math.normalizesafe(deltaV) * maxDelta;
                            }

                            currentVelocity += deltaV;
                            nav.Velocity = currentVelocity;
                            tx.Position += currentVelocity * Delta;
                            movedThisTick = math.lengthsq(currentVelocity) > 1e-5f;
                        }
                        else
                        {
                            nav.Velocity = float3.zero;
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
                                else if (!deposited && depositAmount > 0f)
                                {
                                    var preconditionMask = SetMask(0, PreconditionStorehouse, true);
                                    RecordFailure(ref job, e, VillagerJobFailCode.StorehouseFull, job.Target, preconditionMask);
                                }
                            }
                        }
                        else
                        {
                            var preconditionMask = SetMask(0, PreconditionStorehouse, false);
                            RecordFailure(ref job, e, VillagerJobFailCode.TargetInvalid, job.Target, preconditionMask);
                            job.CarryCount = 0f;
                        }

                        telemetry.CarrierCargoMilliSnapshot = BehaviorTelemetryMath.ToMilli(job.CarryCount);

                        if (job.CarryCount <= 0f)
                        {
                            RecordCompletion(ref job, e, job.Target, 0);
                        }

                        ReleaseTicket(ref assignment, batch, e, ResolveCompletionState(ticket.TargetEntity, ticket));
                        EnterIdle(ref job, e, patienceScore, DropoffCooldownSeconds, tx.Position);
                        StartWorkCooldown(ref workCooldown, e, job.Type, patienceScore);
                        ApplyWorkSatisfaction(e);
                        break;
                }

                if (job.Phase == JobPhase.Idle)
                {
                    nav.Velocity = float3.zero;
                }

                UpdateMovementDebug(ref moveIntent, ref movePlan, in job, in nav, in tx);

                if (hasStamina)
                {
                    currentStamina = UpdateStamina(currentStamina, maxStamina, statAverage, runIntensity, movedThisTick);
                    combatStats.CurrentStamina = (byte)math.round(currentStamina);
                    CombatLookup[e] = combatStats;
                }
            }

            private void UpdateMovementDebug(ref MoveIntent moveIntent, ref MovePlan movePlan, in VillagerJobState job, in Navigation nav, in LocalTransform tx)
            {
                if (job.Phase == JobPhase.Idle || job.Target == Entity.Null)
                {
                    moveIntent = new MoveIntent
                    {
                        TargetEntity = Entity.Null,
                        TargetPosition = tx.Position,
                        IntentType = MoveIntentType.None
                    };
                    movePlan = default;
                    return;
                }

                var intentType = MoveIntentType.Work;
                moveIntent = new MoveIntent
                {
                    TargetEntity = job.Target,
                    TargetPosition = nav.Destination,
                    IntentType = intentType
                };

                var toTarget = nav.Destination - tx.Position;
                toTarget.y = 0f;
                var distance = math.length(toTarget);
                var speed = math.max(0f, nav.Speed);
                var travelSpeed = ApplyArriveSlowdown(speed, distance);
                var desiredVel = distance > 0.01f && travelSpeed > 0f
                    ? math.normalizesafe(toTarget) * travelSpeed
                    : float3.zero;
                var accelLimit = math.max(0.1f, speed * math.max(0.1f, MovementTuning.AccelerationMultiplier));

                var mode = job.Phase == JobPhase.NavigateToNode || job.Phase == JobPhase.NavigateToStorehouse
                    ? MovePlanMode.Approach
                    : job.Phase == JobPhase.Deliver || job.Phase == JobPhase.Gather
                        ? MovePlanMode.Latch
                        : MovePlanMode.None;

                movePlan = new MovePlan
                {
                    Mode = mode,
                    DesiredVelocity = desiredVel,
                    MaxAccel = accelLimit,
                    EtaSeconds = travelSpeed > 0f ? distance / travelSpeed : 0f
                };
            }

            private float ApplyArriveSlowdown(float speed, float distance)
            {
                var radius = math.max(0f, MovementTuning.ArriveSlowdownRadius);
                if (radius <= 0f || speed <= 0f)
                {
                    return speed;
                }

                var minMultiplier = math.clamp(MovementTuning.ArriveMinSpeedMultiplier, 0.1f, 1f);
                var t = math.saturate(distance / radius);
                var scale = math.lerp(minMultiplier, 1f, t);
                return speed * scale;
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

            private static float ResolveTicketCarryTarget(in JobTicket ticket, float carryCapacity)
            {
                if (ticket.IsSingleItem != 0 && ticket.ItemMass > 0f)
                {
                    return ticket.ItemMass;
                }

                if (ticket.WorkAmount <= 0f)
                {
                    return carryCapacity;
                }

                return math.min(carryCapacity, ticket.WorkAmount);
            }

            private bool IsTicketAssignedToEntity(Entity ticketEntity, in JobTicket ticket, Entity entity)
            {
                if (ticket.Assignee == entity)
                {
                    return true;
                }

                if (ticket.RequiredWorkers > 1 && GroupLookup.HasBuffer(ticketEntity))
                {
                    var group = GroupLookup[ticketEntity];
                    for (int i = 0; i < group.Length; i++)
                    {
                        if (group[i].Villager == entity)
                        {
                            return true;
                        }
                    }
                }

                return false;
            }

            private bool IsGroupReady(Entity ticketEntity, in JobTicket ticket)
            {
                if (ticket.IsSingleItem != 0 && ticket.ItemMass > 0f)
                {
                    if (!GroupLookup.HasBuffer(ticketEntity))
                    {
                        return false;
                    }

                    var groupBuffer = GroupLookup[ticketEntity];
                    return ResolveGroupCarryCapacity(groupBuffer) >= ticket.ItemMass;
                }

                if (ticket.RequiredWorkers <= 1)
                {
                    return true;
                }

                if (!GroupLookup.HasBuffer(ticketEntity))
                {
                    return false;
                }

                var groupMembers = GroupLookup[ticketEntity];
                var minWorkers = math.max(1, ticket.MinWorkers);
                return groupMembers.Length >= minWorkers;
            }

            private float ResolveGroupCarryCapacity(DynamicBuffer<JobTicketGroupMember> group)
            {
                float totalCapacity = 0f;
                float orderSum = 0f;
                int count = 0;

                for (int i = 0; i < group.Length; i++)
                {
                    var member = group[i].Villager;
                    var capacity = CarryCapacityOverride > 0f ? CarryCapacityOverride : DefaultCarryCapacity;
                    if (CarryLookup.HasComponent(member))
                    {
                        var modifier = CarryLookup[member];
                        capacity = capacity * math.max(0.1f, modifier.Multiplier) + modifier.Bonus;
                    }

                    if (capacity <= 0f)
                    {
                        continue;
                    }

                    totalCapacity += capacity;
                    if (AlignmentLookup.HasComponent(member))
                    {
                        orderSum += AlignmentLookup[member].OrderAxis;
                    }
                    count++;
                }

                if (count == 0 || totalCapacity <= 0f)
                {
                    return 0f;
                }

                var orderAvg = (orderSum / count) / 100f;
                var cohesion = math.clamp(GroupCohesionBase + orderAvg * GroupCohesionOrderWeight,
                    GroupCohesionMin,
                    GroupCohesionMax);
                return totalCapacity * math.max(0.1f, 1f + cohesion);
            }

            private void ReleaseTicket(ref JobAssignment assignment, DynamicBuffer<JobBatchEntry> batch, Entity entity, JobTicketState nextState)
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
                if (batch.Length > 0)
                {
                    var nextTicket = batch[0].Ticket;
                    batch.RemoveAt(0);
                    assignment.Ticket = nextTicket;
                    assignment.CommitTick = CurrentTick;
                }
            }

            private JobTicketState ResolveCompletionState(Entity target, in JobTicket ticket)
            {
                if (ticket.IsSingleItem != 0 && ticket.ItemMass > 0f)
                {
                    return JobTicketState.Done;
                }

                if (ticket.WorkAmount <= 0f)
                {
                    return JobTicketState.Done;
                }

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

                if (TargetPositions.TryGetValue(target, out var cachedPosition))
                {
                    position = cachedPosition;
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

            private struct StorehouseCandidates
            {
                public Entity CandidateA;
                public Entity CandidateB;
                public Entity CandidateC;
                public float ScoreA;
                public float ScoreB;
                public float ScoreC;
            }

            private bool TryFindStorehouse(float3 origin, bool useCooperation, in VillagerCooperationIntent cooperation,
                bool hasAwareness, in VillagerResourceAwareness awareness, float storehouseRadiusSq,
                out Entity storehouse, out float3 storehousePosition, out StorehouseCandidates candidates)
            {
                storehouse = Entity.Null;
                storehousePosition = float3.zero;
                candidates = default;
                float minDistSq = float.MaxValue;

                if (useCooperation && cooperation.SharedStorehouse != Entity.Null)
                {
                    storehouse = cooperation.SharedStorehouse;
                    int storehouseIndex = StorehouseEntities.IndexOf(storehouse);
                    if (storehouseIndex >= 0)
                    {
                        storehousePosition = StorehouseTransforms[storehouseIndex].Position;
                        minDistSq = math.distancesq(origin, storehousePosition);
                        UpdateStorehouseCandidates(ref candidates, storehouse, minDistSq);
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
                        UpdateStorehouseCandidates(ref candidates, storehouse, minDistSq);
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
                        UpdateStorehouseCandidates(ref candidates, StorehouseEntities[i], distSq);
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

            private void UpdateStorehouseCandidates(ref StorehouseCandidates candidates, Entity candidate, float distSq)
            {
                var score = 1f / (1f + distSq);
                if (score > candidates.ScoreA)
                {
                    candidates.CandidateC = candidates.CandidateB;
                    candidates.ScoreC = candidates.ScoreB;
                    candidates.CandidateB = candidates.CandidateA;
                    candidates.ScoreB = candidates.ScoreA;
                    candidates.CandidateA = candidate;
                    candidates.ScoreA = score;
                }
                else if (score > candidates.ScoreB)
                {
                    candidates.CandidateC = candidates.CandidateB;
                    candidates.ScoreC = candidates.ScoreB;
                    candidates.CandidateB = candidate;
                    candidates.ScoreB = score;
                }
                else if (score > candidates.ScoreC)
                {
                    candidates.CandidateC = candidate;
                    candidates.ScoreC = score;
                }
            }

            private byte SetMask(byte mask, byte flag, bool value)
            {
                return value ? (byte)(mask | flag) : mask;
            }

            private void RecordDecision(ref VillagerJobState job, Entity entity, JobPhase phase, Entity target, in StorehouseCandidates candidates, byte preconditionMask)
            {
                if (job.LastChosenJob != job.Type || job.LastTarget != target)
                {
                    job.RepeatCount = 0;
                }

                job.LastChosenJob = job.Type;
                job.LastTarget = target;
                job.LastFailCode = VillagerJobFailCode.None;
                job.NextEligibleTick = 0;

                var evt = new VillagerJobDecisionEvent
                {
                    Tick = CurrentTick,
                    JobType = job.Type,
                    Phase = phase,
                    FailCode = VillagerJobFailCode.None,
                    Target = target,
                    CandidateA = candidates.CandidateA,
                    CandidateB = candidates.CandidateB,
                    CandidateC = candidates.CandidateC,
                    ScoreA = candidates.ScoreA,
                    ScoreB = candidates.ScoreB,
                    ScoreC = candidates.ScoreC,
                    PreconditionMask = preconditionMask
                };

                PushJobTrace(entity, evt);
            }

            private void RecordFailure(ref VillagerJobState job, Entity entity, VillagerJobFailCode failCode, Entity target, byte preconditionMask)
            {
                var isRepeat = job.LastChosenJob == job.Type && job.LastTarget == target && job.LastFailCode == failCode;
                if (isRepeat && job.LastFailTick > 0u && CurrentTick > job.LastFailTick)
                {
                    job.RepeatCount = (byte)math.min(255, job.RepeatCount + 1);
                }
                else
                {
                    job.RepeatCount = 1;
                }

                job.LastChosenJob = job.Type;
                job.LastTarget = target;
                job.LastFailCode = failCode;
                job.LastFailTick = CurrentTick;

                if (ShouldBackoff(failCode))
                {
                    var backoffTicks = ResolveFailureBackoffTicks(job.RepeatCount);
                    if (backoffTicks > 0u)
                    {
                        job.NextEligibleTick = CurrentTick + backoffTicks;
                    }
                }

                var evt = new VillagerJobDecisionEvent
                {
                    Tick = CurrentTick,
                    JobType = job.Type,
                    Phase = job.Phase,
                    FailCode = failCode,
                    Target = target,
                    CandidateA = Entity.Null,
                    CandidateB = Entity.Null,
                    CandidateC = Entity.Null,
                    ScoreA = 0f,
                    ScoreB = 0f,
                    ScoreC = 0f,
                    PreconditionMask = preconditionMask
                };

                PushJobTrace(entity, evt);
            }

            private void RecordCompletion(ref VillagerJobState job, Entity entity, Entity target, byte preconditionMask)
            {
                job.RepeatCount = 0;
                job.LastFailCode = VillagerJobFailCode.None;
                job.LastFailTick = CurrentTick;
                job.NextEligibleTick = 0;

                var evt = new VillagerJobDecisionEvent
                {
                    Tick = CurrentTick,
                    JobType = job.Type,
                    Phase = job.Phase,
                    FailCode = VillagerJobFailCode.None,
                    Target = target,
                    CandidateA = Entity.Null,
                    CandidateB = Entity.Null,
                    CandidateC = Entity.Null,
                    ScoreA = 0f,
                    ScoreB = 0f,
                    ScoreC = 0f,
                    PreconditionMask = preconditionMask
                };

                PushJobTrace(entity, evt);
            }

            private void PushJobTrace(Entity entity, in VillagerJobDecisionEvent evt)
            {
                if (!JobTraceLookup.HasBuffer(entity))
                {
                    return;
                }

                var buffer = JobTraceLookup[entity];
                buffer.Add(evt);
                if (buffer.Length > 16)
                {
                    buffer.RemoveAt(0);
                }
            }

            private bool ShouldBackoff(VillagerJobFailCode code)
            {
                switch (code)
                {
                    case VillagerJobFailCode.NoStorehouse:
                    case VillagerJobFailCode.StorehouseFull:
                    case VillagerJobFailCode.ReservationDenied:
                    case VillagerJobFailCode.TargetInvalid:
                    case VillagerJobFailCode.Timeout:
                        return true;
                    default:
                        return false;
                }
            }

            private uint ResolveFailureBackoffTicks(byte repeatCount)
            {
                var baseTicks = FailureBackoffBaseTicks;
                if (baseTicks == 0u)
                {
                    return 0u;
                }
                var maxTicks = math.max(baseTicks, FailureBackoffMaxTicks);
                var shift = math.clamp(repeatCount, (byte)0, (byte)5);
                var scaled = baseTicks * (uint)(1 << shift);
                return math.min(maxTicks, scaled);
            }

            private void ResetJob(ref VillagerJobState job, Entity entity, float patienceScore, float3 position)
            {
                EnterIdle(ref job, entity, patienceScore, 0f, position);
            }

            private void EnterIdle(ref VillagerJobState job, Entity entity, float patienceScore, float dropoffCooldown, float3 position)
            {
                job.Phase = JobPhase.Idle;
                job.Target = Entity.Null;
                job.DropoffCooldown = dropoffCooldown > 0f ? dropoffCooldown : 0f;
                job.DecisionCooldown = ResolveDeliberationSeconds(entity, patienceScore);
                job.LastDecisionTick = CurrentTick;

                if (PonderLookup.HasComponent(entity))
                {
                    var ponder = PonderLookup[entity];
                    ponder.RemainingSeconds = job.DecisionCooldown;
                    ponder.AnchorPosition = position;
                    PonderLookup[entity] = ponder;
                }
            }

            private void StartWorkCooldown(ref VillagerWorkCooldown cooldown, Entity entity, JobType jobType, float patienceScore)
            {
                var ticks = ResolveWorkCooldownTicks(entity, jobType);
                if (ticks == 0)
                {
                    cooldown = default;
                    return;
                }

                cooldown.StartTick = CurrentTick;
                cooldown.EndTick = CurrentTick + ticks;
                cooldown.Mode = ResolveCooldownMode(entity, patienceScore);
            }

            private uint ResolveWorkCooldownTicks(Entity entity, JobType jobType)
            {
                var maxTicks = math.max(CooldownProfile.MaxCooldownTicks, CooldownProfile.MinCooldownTicks);
                if (maxTicks == 0)
                {
                    return 0;
                }

                var workBias01 = ResolveWorkBias01(entity, jobType);
                var cooldownScale = ResolveOutlookCooldownScale(entity) * ResolveArchetypeCooldownScale(entity);
                return VillagerCooldownProfile.ResolveCooldownTicks(CooldownProfile, workBias01, cooldownScale);
            }

            private float ResolveWorkBias01(Entity entity, JobType jobType)
            {
                if (!NeedBiasLookup.HasComponent(entity))
                {
                    return 0.5f;
                }

                var bias = NeedBiasLookup[entity];
                var workWeight = math.max(0f, bias.WorkWeight);
                var offWeight = math.max(0f, bias.RestWeight)
                                + math.max(0f, bias.SocialWeight)
                                + math.max(0f, bias.FaithWeight);

                var archetypeWeight01 = ResolveArchetypeWorkWeight01(entity, jobType);
                if (archetypeWeight01 > 0f)
                {
                    workWeight += archetypeWeight01;
                }

                var total = workWeight + offWeight;
                if (total <= 0f)
                {
                    return 0.5f;
                }

                return math.saturate(workWeight / total);
            }

            private float ResolveArchetypeWorkWeight01(Entity entity, JobType jobType)
            {
                if (!ArchetypeLookup.HasComponent(entity))
                {
                    return 0f;
                }

                var data = ArchetypeLookup[entity].Data;
                byte weight = jobType switch
                {
                    JobType.Gather => data.GatherJobWeight,
                    _ => (byte)0
                };
                return math.saturate(weight / 100f);
            }

            private float ResolveArchetypeLoyalty01(Entity entity)
            {
                if (!ArchetypeLookup.HasComponent(entity))
                {
                    return 0.5f;
                }

                var data = ArchetypeLookup[entity].Data;
                return math.saturate(data.BaseLoyalty / 100f);
            }

            private float ResolveOutlookCooldownScale(Entity entity)
            {
                if (CooldownProfileEntity == Entity.Null ||
                    !CooldownOutlookLookup.HasBuffer(CooldownProfileEntity) ||
                    !OutlookLookup.HasComponent(entity))
                {
                    return 1f;
                }

                var rules = CooldownOutlookLookup[CooldownProfileEntity];
                var outlook = OutlookLookup[entity];
                var slotCount = math.min(outlook.OutlookTypes.Length, outlook.OutlookValues.Length);
                if (slotCount == 0 || rules.Length == 0)
                {
                    return 1f;
                }

                float weightedDelta = 0f;
                float weight = 0f;
                for (int i = 0; i < slotCount; i++)
                {
                    var typeId = outlook.OutlookTypes[i];
                    if (typeId == 0)
                    {
                        continue;
                    }

                    if (!TryGetOutlookRule(typeId, rules, out var rule))
                    {
                        continue;
                    }

                    var value01 = math.abs(outlook.OutlookValues[i]) / 100f;
                    if (value01 <= 0f)
                    {
                        continue;
                    }

                    var scale = rule.CooldownScale;
                    if (scale <= 0f)
                    {
                        continue;
                    }

                    weightedDelta += (scale - 1f) * value01;
                    weight += value01;
                }

                if (weight <= 0f)
                {
                    return 1f;
                }

                return math.max(0f, 1f + (weightedDelta / weight));
            }

            private float ResolveArchetypeCooldownScale(Entity entity)
            {
                if (CooldownProfileEntity == Entity.Null ||
                    !CooldownArchetypeLookup.HasBuffer(CooldownProfileEntity) ||
                    !ArchetypeLookup.HasComponent(entity))
                {
                    return 1f;
                }

                var modifiers = CooldownArchetypeLookup[CooldownProfileEntity];
                if (modifiers.Length == 0)
                {
                    return 1f;
                }

                var archetypeName = ArchetypeLookup[entity].Data.ArchetypeName;
                if (archetypeName.Length == 0)
                {
                    return 1f;
                }

                for (int i = 0; i < modifiers.Length; i++)
                {
                    if (modifiers[i].ArchetypeName.Equals(archetypeName))
                    {
                        return math.max(0f, modifiers[i].CooldownScale);
                    }
                }

                return 1f;
            }

            private VillagerWorkCooldownMode ResolveCooldownMode(Entity entity, float patienceScore)
            {
                var wanderWeight = math.max(0.01f, CooldownProfile.BaseWanderWeight);
                var socializeWeight = math.max(0.01f, CooldownProfile.BaseSocializeWeight);

                if (NeedBiasLookup.HasComponent(entity))
                {
                    var bias = NeedBiasLookup[entity];
                    var socialNeed = math.max(0f, bias.SocialWeight);
                    var restNeed = math.max(0f, bias.RestWeight);
                    var faithNeed = math.max(0f, bias.FaithWeight);
                    var total = socialNeed + restNeed + faithNeed;
                    if (total > 0f)
                    {
                        var socialBias = math.saturate(socialNeed / total);
                        var biasWeight = math.max(0f, CooldownProfile.NeedBiasWeight);
                        socializeWeight += socialBias * biasWeight;
                        wanderWeight += (1f - socialBias) * biasWeight;
                    }
                }

                if (AlignmentLookup.HasComponent(entity))
                {
                    var order01 = math.saturate((AlignmentLookup[entity].OrderAxis + 100f) * 0.005f);
                    var orderWeight = math.max(0f, CooldownProfile.OrderAxisWeight);
                    socializeWeight += order01 * orderWeight;
                    wanderWeight += (1f - order01) * orderWeight;
                }

                var loyalty01 = ResolveArchetypeLoyalty01(entity);
                var loyaltyWeight = math.max(0f, CooldownProfile.LoyaltyWeight);
                socializeWeight += loyalty01 * loyaltyWeight;
                wanderWeight += (1f - loyalty01) * loyaltyWeight;

                var patience01 = math.saturate((patienceScore + 100f) * 0.005f);
                var patienceWeight = math.max(0f, CooldownProfile.PatienceWeight);
                socializeWeight += patience01 * patienceWeight;
                wanderWeight += (1f - patience01) * patienceWeight;

                ApplyOutlookLeisureWeights(entity, ref socializeWeight, ref wanderWeight);

                socializeWeight = math.max(0.01f, socializeWeight);
                wanderWeight = math.max(0.01f, wanderWeight);
                var combined = socializeWeight + wanderWeight;
                var socializeChance = combined > 0f ? socializeWeight / combined : 0.5f;

                var seed = math.hash(new uint2((uint)(entity.Index + 1), CurrentTick + 101u));
                var random = Unity.Mathematics.Random.CreateFromIndex(seed == 0u ? 1u : seed);
                return random.NextFloat() < socializeChance
                    ? VillagerWorkCooldownMode.Socialize
                    : VillagerWorkCooldownMode.Wander;
            }

            private void ApplyOutlookLeisureWeights(Entity entity, ref float socializeWeight, ref float wanderWeight)
            {
                if (CooldownProfileEntity == Entity.Null ||
                    !CooldownOutlookLookup.HasBuffer(CooldownProfileEntity) ||
                    !OutlookLookup.HasComponent(entity))
                {
                    return;
                }

                var rules = CooldownOutlookLookup[CooldownProfileEntity];
                if (rules.Length == 0)
                {
                    return;
                }

                var outlook = OutlookLookup[entity];
                var slotCount = math.min(outlook.OutlookTypes.Length, outlook.OutlookValues.Length);
                for (int i = 0; i < slotCount; i++)
                {
                    var typeId = outlook.OutlookTypes[i];
                    if (typeId == 0)
                    {
                        continue;
                    }

                    if (!TryGetOutlookRule(typeId, rules, out var rule))
                    {
                        continue;
                    }

                    var value01 = outlook.OutlookValues[i] / 100f;
                    if (math.abs(value01) <= 1e-4f)
                    {
                        continue;
                    }

                    socializeWeight += rule.SocializeWeight * value01;
                    wanderWeight += rule.WanderWeight * value01;
                }
            }

            private static bool TryGetOutlookRule(byte outlookType, in DynamicBuffer<VillagerCooldownOutlookRule> rules,
                out VillagerCooldownOutlookRule rule)
            {
                for (int i = 0; i < rules.Length; i++)
                {
                    if (rules[i].OutlookType == outlookType)
                    {
                        rule = rules[i];
                        return true;
                    }
                }

                rule = default;
                return false;
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
                if (!NeedLookup.HasComponent(entity))
                {
                    return;
                }

                var needs = NeedLookup[entity];
                var maxUrgency = 1f;
                if (NeedTuningLookup.HasComponent(entity))
                {
                    maxUrgency = math.max(0.01f, NeedTuningLookup[entity].MaxUrgency);
                }

                var workDrop = math.max(WorkSatisfactionPerDelivery, WorkCompletionWorkDrop);
                if (workDrop > 0f)
                {
                    needs.WorkUrgency = math.max(0f, needs.WorkUrgency - workDrop);
                }

                if (WorkCompletionRestBoost > 0f)
                {
                    needs.RestUrgency = math.min(maxUrgency, needs.RestUrgency + WorkCompletionRestBoost);
                }
                if (WorkCompletionSocialBoost > 0f)
                {
                    needs.SocialUrgency = math.min(maxUrgency, needs.SocialUrgency + WorkCompletionSocialBoost);
                }
                if (WorkCompletionFaithBoost > 0f)
                {
                    needs.FaithUrgency = math.min(maxUrgency, needs.FaithUrgency + WorkCompletionFaithBoost);
                }
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
                baseSpeed *= math.max(0.1f, MovementTuning.BaseMoveSpeedMultiplier);
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
                moveSpeed = ApplySpeedVariance(entity, moveSpeed);
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

            private float ApplySpeedVariance(Entity entity, float baseSpeed)
            {
                var amplitude = math.max(0f, MovementTuning.SpeedVarianceAmplitude);
                var periodSeconds = math.max(0f, MovementTuning.SpeedVariancePeriodSeconds);
                if (amplitude <= 0f || periodSeconds <= 0f || baseSpeed <= 0f)
                {
                    return baseSpeed;
                }

                var seed = math.hash(new uint2((uint)(entity.Index + 1), 0x5f21u));
                var phase01 = (seed & 0xffffu) / 65535f;
                var angle = (WorldSeconds / periodSeconds + phase01) * math.PI * 2f;
                var variance = 1f + math.sin(angle) * amplitude;
                return baseSpeed * math.max(0.1f, variance);
            }

            private float3 ResolveSeparation(Entity self, float3 position)
            {
                if (SeparationRadius <= 0f || SeparationWeight <= 0f || VillagerEntities.Length == 0)
                {
                    return float3.zero;
                }

                var radiusSq = SeparationRadius * SeparationRadius;
                var baseCell = CellCoord(position, SeparationCellSize);
                float3 separation = float3.zero;

                for (int x = -1; x <= 1; x++)
                {
                    for (int z = -1; z <= 1; z++)
                    {
                        var neighborCell = baseCell + new int2(x, z);
                        var key = HashCell(neighborCell);
                        if (!VillagerSpatialMap.TryGetFirstValue(key, out var index, out var iterator))
                        {
                            continue;
                        }

                        do
                        {
                            if (VillagerEntities[index] == self)
                            {
                                continue;
                            }

                            var other = VillagerTransforms[index].Position;
                            var diff = position - other;
                            diff.y = 0f;
                            var distSq = math.lengthsq(diff);
                            if (distSq <= 1e-4f || distSq >= radiusSq)
                            {
                                continue;
                            }

                            var dist = math.sqrt(distSq);
                            var push = (SeparationRadius - dist) / SeparationRadius;
                            separation += diff / math.max(dist, 1e-4f) * push;
                        } while (VillagerSpatialMap.TryGetNextValue(out index, ref iterator));
                    }
                }

                var magnitude = math.length(separation);
                if (magnitude <= 1e-4f)
                {
                    return float3.zero;
                }

                var scaled = math.min(SeparationMaxPush, magnitude * SeparationWeight);
                return separation / magnitude * scaled;
            }

            private static int2 CellCoord(float3 position, float cellSize)
            {
                if (cellSize <= 0f)
                {
                    return int2.zero;
                }

                return (int2)math.floor(position.xz / cellSize);
            }

            private static int HashCell(int2 cell)
            {
                return (int)math.hash(cell);
            }
        }

        private static void BuildSeparationMap(NativeArray<LocalTransform> transforms, float cellSize, NativeParallelMultiHashMap<int, int> map)
        {
            map.Clear();
            if (cellSize <= 0f)
            {
                return;
            }

            for (int i = 0; i < transforms.Length; i++)
            {
                var cell = (int2)math.floor(transforms[i].Position.xz / cellSize);
                var key = (int)math.hash(cell);
                map.Add(key, i);
            }
        }
    }
}
