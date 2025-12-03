using Godgame.Registry;
using Godgame.Resources;
using Godgame.Villagers;
using PureDOTS.Runtime.Components;
using PureDOTS.Runtime.Resource;
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
        private ComponentLookup<LocalTransform> _transformLookup;
        private ComponentLookup<GodgameStorehouse> _storehouseLookup;
        private BufferLookup<StorehouseInventoryItem> _inventoryLookup;
        private BufferLookup<StorehouseCapacityElement> _capacityLookup;
        private EntityQuery _resourceNodeQuery;
        private EntityQuery _storehouseQuery;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<VillagerJobState>();
            state.RequireForUpdate<ResourceTypeIndex>();
            _resourceNodeLookup = state.GetComponentLookup<GodgameResourceNodeMirror>(false);
            _transformLookup = state.GetComponentLookup<LocalTransform>(isReadOnly: true);
            _storehouseLookup = state.GetComponentLookup<GodgameStorehouse>(isReadOnly: true);
            _inventoryLookup = state.GetBufferLookup<StorehouseInventoryItem>(false);
            _capacityLookup = state.GetBufferLookup<StorehouseCapacityElement>(isReadOnly: true);

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
            _resourceNodeLookup.Update(ref state);
            _transformLookup.Update(ref state);
            _storehouseLookup.Update(ref state);
            _inventoryLookup.Update(ref state);
            _capacityLookup.Update(ref state);

            var catalog = SystemAPI.GetSingleton<ResourceTypeIndex>().Catalog;
            if (!catalog.IsCreated)
            {
                return;
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

            new StepJob
            {
                Delta = deltaTime,
                Ecb = ecb,
                ResourceNodeLookup = _resourceNodeLookup,
                TransformLookup = _transformLookup,
                StorehouseLookup = _storehouseLookup,
                InventoryLookup = _inventoryLookup,
                CapacityLookup = _capacityLookup,
                Catalog = catalog,
                ResourceNodeEntities = resourceNodeEntities,
                ResourceNodeTransforms = resourceNodeTransforms,
                ResourceNodeMirrors = resourceNodeMirrors,
                StorehouseEntities = storehouseEntities,
                StorehouseTransforms = storehouseTransforms
            }.ScheduleParallel();

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
        public partial struct StepJob : IJobEntity
        {
            public float Delta;
            public EntityCommandBuffer.ParallelWriter Ecb;
            public ComponentLookup<GodgameResourceNodeMirror> ResourceNodeLookup;
            [ReadOnly] public ComponentLookup<LocalTransform> TransformLookup;
            [ReadOnly] public ComponentLookup<GodgameStorehouse> StorehouseLookup;
            public BufferLookup<StorehouseInventoryItem> InventoryLookup;
            [ReadOnly] public BufferLookup<StorehouseCapacityElement> CapacityLookup;
            [ReadOnly] public BlobAssetReference<ResourceTypeIndexBlob> Catalog;

            [DeallocateOnJobCompletion] [ReadOnly] public NativeArray<Entity> ResourceNodeEntities;
            [DeallocateOnJobCompletion] [ReadOnly] public NativeArray<LocalTransform> ResourceNodeTransforms;
            [DeallocateOnJobCompletion] public NativeArray<GodgameResourceNodeMirror> ResourceNodeMirrors;
            [DeallocateOnJobCompletion] [ReadOnly] public NativeArray<Entity> StorehouseEntities;
            [DeallocateOnJobCompletion] [ReadOnly] public NativeArray<LocalTransform> StorehouseTransforms;

            private const float GatherDistance = 2f;
            private const float DeliverDistance = 3f;
            private const float MoveSpeed = 5f;
            private const float GatherRate = 10f; // units per second

            [BurstCompile]
            void Execute([ChunkIndexInQuery] int ciq, Entity e, ref VillagerJobState job, ref LocalTransform tx, ref Navigation nav)
            {
                switch (job.Phase)
                {
                    case JobPhase.Idle:
                        // Find nearest resource node of matching type
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
                            nav.Speed = MoveSpeed;
                            job.Phase = JobPhase.NavigateToNode;
                            job.ResourceTypeIndex = ResourceNodeMirrors[nodeIndex].ResourceTypeIndex;
                        }
                        break;

                    case JobPhase.NavigateToNode:
                        // Move toward destination
                        var direction = nav.Destination - tx.Position;
                        var distance = math.length(direction);
                        if (distance > GatherDistance)
                        {
                            var moveDelta = math.normalize(direction) * nav.Speed * Delta;
                            tx.Position += moveDelta;
                        }
                        else
                        {
                            // Reached node, start gathering
                            job.Phase = JobPhase.Gather;
                        }
                        break;

                    case JobPhase.Gather:
                        // Gather resources from node
                        if (job.Target != Entity.Null && ResourceNodeLookup.HasComponent(job.Target))
                        {
                            var nodeIndex = ResourceNodeEntities.IndexOf(job.Target);
                            if (nodeIndex >= 0)
                            {
                                var node = ResourceNodeMirrors[nodeIndex];
                                if (node.RemainingAmount > 0f && job.CarryCount < job.CarryMax)
                                {
                                    var gatherAmount = math.min(GatherRate * Delta, math.min(node.RemainingAmount, job.CarryMax - job.CarryCount));
                                    job.CarryCount += gatherAmount;
                                    node.RemainingAmount = math.max(0f, node.RemainingAmount - gatherAmount);
                                    if (node.RemainingAmount <= 0f)
                                    {
                                        node.IsDepleted = 1;
                                    }
                                    // Update lookup for later sync
                                    ResourceNodeLookup[job.Target] = node;
                                }

                                if (job.CarryCount >= job.CarryMax || node.RemainingAmount <= 0f)
                                {
                                    // Find nearest storehouse
                                    Entity nearestStorehouse = Entity.Null;
                                    float minStorehouseDistSq = float.MaxValue;
                                    for (int i = 0; i < StorehouseEntities.Length; i++)
                                    {
                                        float distSq = math.distancesq(tx.Position, StorehouseTransforms[i].Position);
                                        if (distSq < minStorehouseDistSq)
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
                                        nav.Speed = MoveSpeed;
                                        job.Phase = JobPhase.NavigateToStorehouse;
                                    }
                                    else
                                    {
                                        // No storehouse found, drop resources and reset
                                        job.CarryCount = 0f;
                                        job.Phase = JobPhase.Idle;
                                        job.Target = Entity.Null;
                                    }
                                }
                            }
                        }
                        break;

                    case JobPhase.NavigateToStorehouse:
                        // Move toward storehouse
                        direction = nav.Destination - tx.Position;
                        distance = math.length(direction);
                        if (distance > DeliverDistance)
                        {
                            var moveDelta = math.normalize(direction) * nav.Speed * Delta;
                            tx.Position += moveDelta;
                        }
                        else
                        {
                            // Reached storehouse, deliver
                            job.Phase = JobPhase.Deliver;
                        }
                        break;

                    case JobPhase.Deliver:
                        // Deposit resources into storehouse
                        if (job.Target != Entity.Null && InventoryLookup.HasBuffer(job.Target) && CapacityLookup.HasBuffer(job.Target))
                        {
                            var inventory = InventoryLookup[job.Target];
                            var capacities = CapacityLookup[job.Target];
                            
                            // Find capacity for this resource type
                            float capacity = 1000f; // Default fallback
                            var resourceId = Godgame.Resources.StorehouseApi.ResolveResourceId(Catalog, job.ResourceTypeIndex);
                            for (int i = 0; i < capacities.Length; i++)
                            {
                                if (capacities[i].ResourceTypeId.Equals(resourceId))
                                {
                                    capacity = capacities[i].MaxCapacity;
                                    break;
                                }
                            }

                            var deposited = Godgame.Resources.StorehouseApi.TryDeposit(ref inventory, Catalog, job.ResourceTypeIndex, job.CarryCount, capacity);
                            if (deposited)
                            {
                                job.CarryCount = 0f;
                            }
                        }
                        else
                        {
                            // No storehouse found, drop resources
                            job.CarryCount = 0f;
                        }

                        job.Phase = JobPhase.Idle;
                        job.Target = Entity.Null;
                        break;
                }
            }
        }
    }
}

