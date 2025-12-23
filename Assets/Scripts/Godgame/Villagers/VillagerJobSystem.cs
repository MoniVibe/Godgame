using Godgame.Registry;
using Godgame.Resources;
using Godgame.Villagers;
using Godgame.Logistics;
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
        private ComponentLookup<VillagerGoalState> _goalLookup;
        private ComponentLookup<HazardAvoidanceState> _hazardLookup;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<VillagerJobState>();
            state.RequireForUpdate<ResourceTypeIndex>();
            state.RequireForUpdate<BehaviorConfigRegistry>();
            _resourceNodeLookup = state.GetComponentLookup<GodgameResourceNodeMirror>(false);
            _storehouseLookup = state.GetComponentLookup<GodgameStorehouse>(isReadOnly: true);
            _inventoryLookup = state.GetBufferLookup<StorehouseInventoryItem>(false);
            _capacityLookup = state.GetBufferLookup<StorehouseCapacityElement>(isReadOnly: true);
            _goalLookup = state.GetComponentLookup<VillagerGoalState>(true);
            _hazardLookup = state.GetComponentLookup<HazardAvoidanceState>(true);

            _resourceNodeQuery = SystemAPI.QueryBuilder()
                .WithAll<GodgameResourceNodeMirror, LocalTransform>()
                .Build();

            _storehouseQuery = SystemAPI.QueryBuilder()
                .WithAll<GodgameStorehouse, LocalTransform>()
                .Build();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            if (SystemAPI.TryGetSingleton<RewindState>(out var rewindState) && rewindState.Mode != RewindMode.Record)
            {
                return;
            }

            _resourceNodeLookup.Update(ref state);
            _storehouseLookup.Update(ref state);
            _inventoryLookup.Update(ref state);
            _capacityLookup.Update(ref state);
            _goalLookup.Update(ref state);
            _hazardLookup.Update(ref state);

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
            var dropoffCooldown = math.max(0f, gatherConfig.DropoffCooldownSeconds);
            var arrivalDistance = movementConfig.ArrivalDistance > 0f
                ? movementConfig.ArrivalDistance
                : StepJob.DefaultArrivalDistance;

            var ecb = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>()
                .CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter();

            var deltaTime = SystemAPI.Time.DeltaTime;

            // Collect resource nodes and storehouses for job
            var resourceNodeEntities = _resourceNodeQuery.ToEntityArray(state.WorldUpdateAllocator);
            var resourceNodeTransforms = _resourceNodeQuery.ToComponentDataArray<LocalTransform>(state.WorldUpdateAllocator);
            var resourceNodeMirrors = _resourceNodeQuery.ToComponentDataArray<GodgameResourceNodeMirror>(state.WorldUpdateAllocator);

            var storehouseEntities = _storehouseQuery.ToEntityArray(state.WorldUpdateAllocator);
            var storehouseTransforms = _storehouseQuery.ToComponentDataArray<LocalTransform>(state.WorldUpdateAllocator);

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
                GatherRatePerSecond = gatherRate,
                CarryCapacityOverride = carryCapacityOverride,
                ReturnThresholdPercent = returnThreshold,
                StorehouseSearchRadius = storehouseRadius,
                DropoffCooldownSeconds = dropoffCooldown,
                ArrivalDistance = arrivalDistance,
                MoveSpeed = StepJob.DefaultMoveSpeed,
                GoalLookup = _goalLookup,
                HazardLookup = _hazardLookup
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
        }

        [BurstCompile]
        [WithNone(typeof(LogisticsHaulerTag))]
        public partial struct StepJob : IJobEntity
        {
            public const float DefaultGatherRate = 8f;
            public const float DefaultCarryCapacity = 50f;
            public const float DefaultReturnThreshold = 0.95f;
            public const float DefaultStorehouseRadius = 250f;
            public const float DefaultArrivalDistance = 2f;
            public const float DefaultDeliverDistance = 3f;
            public const float DefaultMoveSpeed = 5f;

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

            public float GatherRatePerSecond;
            public float CarryCapacityOverride;
            public float ReturnThresholdPercent;
            public float StorehouseSearchRadius;
            public float DropoffCooldownSeconds;
            public float ArrivalDistance;
            public float MoveSpeed;
            [ReadOnly] public ComponentLookup<VillagerGoalState> GoalLookup;
            [ReadOnly] public ComponentLookup<HazardAvoidanceState> HazardLookup;

            [BurstCompile]
            void Execute([ChunkIndexInQuery] int ciq, Entity e, ref VillagerJobState job, ref LocalTransform tx, ref Navigation nav, ref GatherDeliverTelemetry telemetry)
            {
                if (GoalLookup.HasComponent(e))
                {
                    var goal = GoalLookup[e];
                    if (goal.CurrentGoal != VillagerGoal.Work)
                    {
                        ResetJob(ref job);
                        return;
                    }
                }

                float3 hazardVector = float3.zero;
                float hazardUrgency = 0f;
                if (HazardLookup.HasComponent(e))
                {
                    var hazard = HazardLookup[e];
                    hazardVector = hazard.CurrentAdjustment;
                    hazardUrgency = hazard.AvoidanceUrgency;
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
                var moveSpeed = MoveSpeed > 0f ? MoveSpeed : DefaultMoveSpeed;

                if (job.DropoffCooldown > 0f)
                {
                    job.DropoffCooldown = math.max(0f, job.DropoffCooldown - Delta);
                    if (job.DropoffCooldown > 0f && job.Phase == JobPhase.Idle)
                    {
                        return;
                    }
                }

                switch (job.Phase)
                {
                    case JobPhase.Idle:
                        Entity nearestNode = Entity.Null;
                        float minDistanceSq = float.MaxValue;
                        for (int i = 0; i < ResourceNodeEntities.Length; i++)
                        {
                            var mirror = ResourceNodeMirrors[i];
                            if (mirror.ResourceTypeIndex == job.ResourceTypeIndex && mirror.RemainingAmount > 0f && mirror.IsDepleted == 0)
                            {
                                float distSq = math.distancesq(tx.Position, ResourceNodeTransforms[i].Position);
                                if (distSq < minDistanceSq)
                                {
                                    minDistanceSq = distSq;
                                    nearestNode = ResourceNodeEntities[i];
                                }
                            }
                        }

                        if (nearestNode != Entity.Null)
                        {
                            job.Target = nearestNode;
                            int nodeIndex = ResourceNodeEntities.IndexOf(nearestNode);
                            nav.Destination = ResourceNodeTransforms[nodeIndex].Position;
                            nav.Speed = moveSpeed;
                            job.Phase = JobPhase.NavigateToNode;
                            job.ResourceTypeIndex = ResourceNodeMirrors[nodeIndex].ResourceTypeIndex;
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
                        }
                        else
                        {
                            job.Phase = JobPhase.Gather;
                        }
                        break;

                    case JobPhase.Gather:
                        if (job.Target != Entity.Null && ResourceNodeLookup.HasComponent(job.Target))
                        {
                            var nodeIndex = ResourceNodeEntities.IndexOf(job.Target);
                            if (nodeIndex >= 0)
                            {
                                var node = ResourceNodeMirrors[nodeIndex];
                                if (node.RemainingAmount > 0f && job.CarryCount < carryCapacity)
                                {
                                    var gatherAmount = math.min(gatherRate * Delta, math.min(node.RemainingAmount, carryCapacity - job.CarryCount));
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
                                }

                                if (job.CarryCount >= carryCapacity * returnThreshold || node.RemainingAmount <= 0f)
                                {
                                    Entity nearestStorehouse = Entity.Null;
                                    float minStorehouseDistSq = float.MaxValue;
                                    for (int i = 0; i < StorehouseEntities.Length; i++)
                                    {
                                        float distSq = math.distancesq(tx.Position, StorehouseTransforms[i].Position);
                                        if (distSq <= storehouseRadiusSq && distSq < minStorehouseDistSq)
                                        {
                                            minStorehouseDistSq = distSq;
                                            nearestStorehouse = StorehouseEntities[i];
                                        }
                                    }

                                    if (nearestStorehouse != Entity.Null)
                                    {
                                        job.Target = nearestStorehouse;
                                        int storehouseIndex = StorehouseEntities.IndexOf(nearestStorehouse);
                                        nav.Destination = StorehouseTransforms[storehouseIndex].Position;
                                        nav.Speed = moveSpeed;
                                        job.Phase = JobPhase.NavigateToStorehouse;
                                    }
                                    else
                                    {
                                        job.CarryCount = 0f;
                                        job.Phase = JobPhase.Idle;
                                        job.Target = Entity.Null;
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
                            var resourceId = Godgame.Resources.StorehouseApi.ResolveResourceId(Catalog, job.ResourceTypeIndex);
                            for (int i = 0; i < capacities.Length; i++)
                            {
                                if (capacities[i].ResourceTypeId.Equals(resourceId))
                                {
                                    capacity = capacities[i].MaxCapacity;
                                    break;
                                }
                            }

                            var depositAmount = job.CarryCount;
                            var deposited = Godgame.Resources.StorehouseApi.TryDeposit(ref inventory, Catalog, job.ResourceTypeIndex, depositAmount, capacity);
                            if (deposited && depositAmount > 0f)
                            {
                                telemetry.DepositedAmountMilliInterval += BehaviorTelemetryMath.ToMilli(depositAmount);
                                job.CarryCount = 0f;
                            }
                        }
                        else
                        {
                            job.CarryCount = 0f;
                        }

                        telemetry.CarrierCargoMilliSnapshot = BehaviorTelemetryMath.ToMilli(job.CarryCount);

                        job.Phase = JobPhase.Idle;
                        job.DropoffCooldown = DropoffCooldownSeconds > 0f ? DropoffCooldownSeconds : 0f;
                        job.Target = Entity.Null;
                        break;
                }
            }

            private static void ResetJob(ref VillagerJobState job)
            {
                job.Phase = JobPhase.Idle;
                job.Target = Entity.Null;
            }
        }
    }
}
