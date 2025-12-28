using Godgame.Construction;
using Godgame.Resources;
using Godgame.Villagers;
using PureDOTS.Runtime.Components;
using PureDOTS.Runtime.Transport;
using PureDOTS.Systems;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Godgame.Logistics
{
    /// <summary>
    /// Simple hauling loop: claim reservation -> pickup from storehouse -> deliver to site.
    /// </summary>
    [UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
    public partial struct GodgameLogisticsHaulerSystem : ISystem
    {
        private ComponentLookup<LocalTransform> _transformLookup;
        private BufferLookup<StorehouseInventoryItem> _inventoryLookup;
        private ComponentLookup<ConstructionGhost> _constructionLookup;

        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<LogisticsHauler>();
            state.RequireForUpdate<TimeState>();
            state.RequireForUpdate<RewindState>();
            state.RequireForUpdate<ResourceTypeIndex>();

            _transformLookup = state.GetComponentLookup<LocalTransform>(true);
            _inventoryLookup = state.GetBufferLookup<StorehouseInventoryItem>(false);
            _constructionLookup = state.GetComponentLookup<ConstructionGhost>(false);
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var timeState = SystemAPI.GetSingleton<TimeState>();
            if (timeState.IsPaused)
            {
                return;
            }

            var rewindState = SystemAPI.GetSingleton<RewindState>();
            if (rewindState.Mode != RewindMode.Record)
            {
                return;
            }

            var catalog = SystemAPI.GetSingleton<ResourceTypeIndex>().Catalog;
            if (!catalog.IsCreated)
            {
                return;
            }

            _transformLookup.Update(ref state);
            _inventoryLookup.Update(ref state);
            _constructionLookup.Update(ref state);

            var deltaTime = math.max(timeState.FixedDeltaTime, 1e-4f);

            var boardQuery = SystemAPI.QueryBuilder()
                .WithAll<LogisticsBoard, LocalTransform>()
                .Build();
            var boardEntities = boardQuery.ToEntityArray(state.WorldUpdateAllocator);
            var boardTransforms = boardQuery.ToComponentDataArray<LocalTransform>(state.WorldUpdateAllocator);

            var storehouseQuery = SystemAPI.QueryBuilder()
                .WithAll<StorehouseInventory, LocalTransform>()
                .Build();
            var storehouseEntities = storehouseQuery.ToEntityArray(state.WorldUpdateAllocator);
            var storehouseTransforms = storehouseQuery.ToComponentDataArray<LocalTransform>(state.WorldUpdateAllocator);

            foreach (var (hauler, haulState, nav, transform, entity) in SystemAPI
                         .Query<RefRO<LogisticsHauler>, RefRW<LogisticsHaulState>, RefRW<Navigation>, RefRW<LocalTransform>>()
                         .WithAll<LogisticsHaulerTag>()
                         .WithEntityAccess())
            {
                var stateData = haulState.ValueRW;
                var haulerData = hauler.ValueRO;
                var interactRangeSq = math.max(0.1f, haulerData.InteractRange * haulerData.InteractRange);

                switch (stateData.Phase)
                {
                    case LogisticsHaulPhase.Idle:
                        ResetIfStale(ref stateData);
                        if (EnsureBoard(ref stateData, boardEntities, boardTransforms, transform.ValueRO.Position))
                        {
                            stateData.Phase = LogisticsHaulPhase.MoveToBoard;
                        }
                        break;
                    case LogisticsHaulPhase.MoveToBoard:
                        if (stateData.BoardEntity == Entity.Null)
                        {
                            stateData.Phase = LogisticsHaulPhase.Idle;
                            break;
                        }
                        if (MoveTowards(ref transform.ValueRW, ref nav.ValueRW, stateData.BoardEntity, haulerData.MoveSpeed, deltaTime, interactRangeSq, _transformLookup))
                        {
                            stateData.Phase = LogisticsHaulPhase.Claiming;
                        }
                        break;
                    case LogisticsHaulPhase.Claiming:
                        if (stateData.BoardEntity == Entity.Null || !state.EntityManager.Exists(stateData.BoardEntity))
                        {
                            stateData.Phase = LogisticsHaulPhase.Idle;
                            break;
                        }
                        if (timeState.Tick - stateData.LastClaimTick >= haulerData.ClaimCooldownTicks)
                        {
                            if (state.EntityManager.HasBuffer<LogisticsClaimRequest>(stateData.BoardEntity))
                            {
                                var claims = state.EntityManager.GetBuffer<LogisticsClaimRequest>(stateData.BoardEntity);
                                claims.Add(new LogisticsClaimRequest
                                {
                                    Requester = entity,
                                    ResourceTypeIndex = ushort.MaxValue,
                                    DesiredMinUnits = 0f,
                                    DesiredMaxUnits = haulerData.CarryCapacity,
                                    CarryCapacity = haulerData.CarryCapacity,
                                    SiteFilter = Entity.Null,
                                    RequestTick = timeState.Tick,
                                    Priority = 1
                                });
                                stateData.LastClaimTick = timeState.Tick;
                            }
                        }

                        if (TryConsumeReservation(stateData.BoardEntity, entity, ref stateData, storehouseEntities, storehouseTransforms,
                                transform.ValueRO.Position, catalog, _inventoryLookup, state.EntityManager))
                        {
                            stateData.Phase = LogisticsHaulPhase.MoveToSource;
                        }
                        break;
                    case LogisticsHaulPhase.MoveToSource:
                        if (stateData.SourceEntity == Entity.Null)
                        {
                            CancelReservation(stateData.BoardEntity, stateData.ReservationId, state.EntityManager);
                            stateData.Phase = LogisticsHaulPhase.Idle;
                            break;
                        }
                        if (MoveTowards(ref transform.ValueRW, ref nav.ValueRW, stateData.SourceEntity, haulerData.MoveSpeed, deltaTime, interactRangeSq, _transformLookup))
                        {
                            stateData.Phase = LogisticsHaulPhase.Pickup;
                        }
                        break;
                    case LogisticsHaulPhase.Pickup:
                        if (!TryPickup(ref stateData, haulerData, catalog, _inventoryLookup, state.EntityManager))
                        {
                            CancelReservation(stateData.BoardEntity, stateData.ReservationId, state.EntityManager);
                            stateData.Phase = LogisticsHaulPhase.Idle;
                        }
                        else
                        {
                            stateData.Phase = LogisticsHaulPhase.MoveToSite;
                        }
                        break;
                    case LogisticsHaulPhase.MoveToSite:
                        if (stateData.SiteEntity == Entity.Null)
                        {
                            CancelReservation(stateData.BoardEntity, stateData.ReservationId, state.EntityManager);
                            stateData.Phase = LogisticsHaulPhase.Idle;
                            break;
                        }
                        if (MoveTowards(ref transform.ValueRW, ref nav.ValueRW, stateData.SiteEntity, haulerData.MoveSpeed, deltaTime, interactRangeSq, _transformLookup))
                        {
                            stateData.Phase = LogisticsHaulPhase.Dropoff;
                        }
                        break;
                    case LogisticsHaulPhase.Dropoff:
                        DeliverToSite(ref stateData, _constructionLookup, state.EntityManager);
                        FulfillReservation(stateData.BoardEntity, stateData.ReservationId, state.EntityManager);
                        stateData.Phase = LogisticsHaulPhase.Idle;
                        break;
                }

                stateData.LastProgressTick = timeState.Tick;
                haulState.ValueRW = stateData;
            }
        }

        private static void ResetIfStale(ref LogisticsHaulState state)
        {
            state.SiteEntity = Entity.Null;
            state.SourceEntity = Entity.Null;
            state.ResourceTypeIndex = ushort.MaxValue;
            state.ReservedUnits = 0f;
            state.CarryingUnits = 0f;
            state.ReservationId = 0;
        }

        private static bool EnsureBoard(ref LogisticsHaulState state, NativeArray<Entity> boards, NativeArray<LocalTransform> transforms, float3 position)
        {
            if (state.BoardEntity != Entity.Null)
            {
                return true;
            }

            var bestIndex = -1;
            var bestDistSq = float.MaxValue;
            for (int i = 0; i < boards.Length; i++)
            {
                var distSq = math.distancesq(position, transforms[i].Position);
                if (distSq < bestDistSq)
                {
                    bestDistSq = distSq;
                    bestIndex = i;
                }
            }

            if (bestIndex < 0)
            {
                return false;
            }

            state.BoardEntity = boards[bestIndex];
            return true;
        }

        private static bool MoveTowards(
            ref LocalTransform transform,
            ref Navigation nav,
            Entity targetEntity,
            float speed,
            float deltaTime,
            float arriveDistanceSq,
            ComponentLookup<LocalTransform> lookup)
        {
            if (!lookup.HasComponent(targetEntity))
            {
                return false;
            }

            var targetPosition = lookup[targetEntity].Position;
            nav.Destination = targetPosition;
            nav.Speed = speed;

            var current = transform.Position;
            targetPosition.y = current.y;
            var toTarget = targetPosition - current;
            toTarget.y = 0f;
            var distSq = math.lengthsq(toTarget);
            if (distSq <= arriveDistanceSq)
            {
                return true;
            }

            var dist = math.sqrt(distSq);
            var step = math.min(speed * deltaTime, dist);
            var dir = toTarget / math.max(1e-5f, dist);
            transform.Position = current + dir * step;
            return false;
        }

        private static bool TryConsumeReservation(
            Entity boardEntity,
            Entity hauler,
            ref LogisticsHaulState state,
            NativeArray<Entity> storehouses,
            NativeArray<LocalTransform> storehouseTransforms,
            float3 origin,
            BlobAssetReference<ResourceTypeIndexBlob> catalog,
            BufferLookup<StorehouseInventoryItem> inventoryLookup,
            EntityManager entityManager)
        {
            if (boardEntity == Entity.Null || !entityManager.Exists(boardEntity))
            {
                return false;
            }

            if (!entityManager.HasBuffer<LogisticsReservationEntry>(boardEntity))
            {
                return false;
            }

            var reservations = entityManager.GetBuffer<LogisticsReservationEntry>(boardEntity);
            for (int i = 0; i < reservations.Length; i++)
            {
                var entry = reservations[i];
                if (entry.Status != LogisticsReservationStatus.Active || entry.HaulerEntity != hauler)
                {
                    continue;
                }

                var sourceEntity = FindNearestStorehouse(storehouses, storehouseTransforms, origin, entry.ResourceTypeIndex, catalog, inventoryLookup);
                if (sourceEntity == Entity.Null)
                {
                    entry.Status = LogisticsReservationStatus.Cancelled;
                    reservations[i] = entry;
                    return false;
                }

                entry.SourceEntity = sourceEntity;
                reservations[i] = entry;

                state.ReservationId = entry.ReservationId;
                state.SiteEntity = entry.SiteEntity;
                state.SourceEntity = sourceEntity;
                state.ResourceTypeIndex = entry.ResourceTypeIndex;
                state.ReservedUnits = entry.Units;
                return true;
            }

            return false;
        }

        private static Entity FindNearestStorehouse(
            NativeArray<Entity> storehouses,
            NativeArray<LocalTransform> transforms,
            float3 origin,
            ushort resourceTypeIndex,
            BlobAssetReference<ResourceTypeIndexBlob> catalog,
            BufferLookup<StorehouseInventoryItem> inventoryLookup)
        {
            var resourceId = StorehouseApi.ResolveResourceId(catalog, resourceTypeIndex);
            if (resourceId.Length == 0)
            {
                return Entity.Null;
            }

            var bestIndex = -1;
            var bestDistSq = float.MaxValue;
            for (int i = 0; i < storehouses.Length; i++)
            {
                var storehouse = storehouses[i];
                if (!inventoryLookup.HasBuffer(storehouse))
                {
                    continue;
                }

                var inv = inventoryLookup[storehouse];
                if (!HasResource(inv, resourceId))
                {
                    continue;
                }

                var distSq = math.distancesq(transforms[i].Position, origin);
                if (distSq < bestDistSq)
                {
                    bestDistSq = distSq;
                    bestIndex = i;
                }
            }

            return bestIndex >= 0 ? storehouses[bestIndex] : Entity.Null;
        }

        private static bool HasResource(DynamicBuffer<StorehouseInventoryItem> inventory, FixedString64Bytes resourceId)
        {
            for (int i = 0; i < inventory.Length; i++)
            {
                if (inventory[i].ResourceTypeId.Equals(resourceId) && inventory[i].Amount > 0f)
                {
                    return true;
                }
            }

            return false;
        }

        private static bool TryPickup(
            ref LogisticsHaulState state,
            in LogisticsHauler hauler,
            BlobAssetReference<ResourceTypeIndexBlob> catalog,
            BufferLookup<StorehouseInventoryItem> inventoryLookup,
            EntityManager entityManager)
        {
            if (state.SourceEntity == Entity.Null || !entityManager.Exists(state.SourceEntity))
            {
                return false;
            }

            if (!inventoryLookup.HasBuffer(state.SourceEntity))
            {
                return false;
            }

            var inv = inventoryLookup[state.SourceEntity];
            var want = math.min(state.ReservedUnits, hauler.CarryCapacity);
            var withdrawn = StorehouseApi.Withdraw(ref inv, catalog, state.ResourceTypeIndex, want);
            if (withdrawn <= 0f)
            {
                return false;
            }

            state.CarryingUnits = withdrawn;
            return true;
        }

        private static void DeliverToSite(
            ref LogisticsHaulState state,
            ComponentLookup<ConstructionGhost> constructionLookup,
            EntityManager entityManager)
        {
            if (state.SiteEntity == Entity.Null || !entityManager.Exists(state.SiteEntity))
            {
                state.CarryingUnits = 0f;
                return;
            }

            if (!constructionLookup.HasComponent(state.SiteEntity))
            {
                state.CarryingUnits = 0f;
                return;
            }

            var ghost = constructionLookup[state.SiteEntity];
            var remaining = math.max(0f, ghost.Cost - ghost.Paid);
            var delivered = math.min(state.CarryingUnits, remaining);
            if (delivered > 0f)
            {
                ghost.Paid += (int)math.floor(delivered);
                constructionLookup[state.SiteEntity] = ghost;
            }

            state.CarryingUnits = 0f;
        }

        private static void FulfillReservation(Entity boardEntity, uint reservationId, EntityManager entityManager)
        {
            if (boardEntity == Entity.Null || reservationId == 0 || !entityManager.Exists(boardEntity))
            {
                return;
            }

            if (!entityManager.HasBuffer<LogisticsReservationEntry>(boardEntity))
            {
                return;
            }

            var reservations = entityManager.GetBuffer<LogisticsReservationEntry>(boardEntity);
            for (int i = 0; i < reservations.Length; i++)
            {
                if (reservations[i].ReservationId == reservationId)
                {
                    var entry = reservations[i];
                    entry.Status = LogisticsReservationStatus.Fulfilled;
                    reservations[i] = entry;
                    return;
                }
            }
        }

        private static void CancelReservation(Entity boardEntity, uint reservationId, EntityManager entityManager)
        {
            if (boardEntity == Entity.Null || reservationId == 0 || !entityManager.Exists(boardEntity))
            {
                return;
            }

            if (!entityManager.HasBuffer<LogisticsReservationEntry>(boardEntity))
            {
                return;
            }

            var reservations = entityManager.GetBuffer<LogisticsReservationEntry>(boardEntity);
            for (int i = 0; i < reservations.Length; i++)
            {
                if (reservations[i].ReservationId == reservationId)
                {
                    var entry = reservations[i];
                    entry.Status = LogisticsReservationStatus.Cancelled;
                    reservations[i] = entry;
                    return;
                }
            }
        }
    }
}
