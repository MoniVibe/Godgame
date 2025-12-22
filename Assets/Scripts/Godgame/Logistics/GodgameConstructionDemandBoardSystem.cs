using Godgame.Construction;
using PureDOTS.Runtime.Components;
using PureDOTS.Runtime.Transport;
using PureDOTS.Runtime.Transport.Systems;
using PureDOTS.Systems;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Godgame.Logistics
{
    /// <summary>
    /// Populates logistics board demand entries from active construction ghosts.
    /// </summary>
    [BurstCompile]
    [UpdateInGroup(typeof(ConstructionSystemGroup))]
    [UpdateBefore(typeof(LogisticsBoardClaimSystem))]
    public partial struct GodgameConstructionDemandBoardSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<LogisticsBoard>();
            state.RequireForUpdate<ConstructionGhost>();
            state.RequireForUpdate<TimeState>();
            state.RequireForUpdate<RewindState>();
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

            foreach (var (board, demands, reservations, boardEntity) in
                     SystemAPI.Query<RefRW<LogisticsBoard>, DynamicBuffer<LogisticsDemandEntry>,
                             DynamicBuffer<LogisticsReservationEntry>>()
                         .WithEntityAccess())
            {
                demands.Clear();

                foreach (var (ghost, transform, siteEntity) in SystemAPI.Query<RefRO<ConstructionGhost>, RefRO<LocalTransform>>()
                             .WithEntityAccess())
                {
                    var required = math.max(0f, ghost.ValueRO.Cost);
                    var delivered = math.max(0f, ghost.ValueRO.Paid);
                    if (required <= delivered)
                    {
                        continue;
                    }

                    var reserved = SumReserved(reservations, siteEntity, ghost.ValueRO.ResourceTypeIndex);
                    var outstanding = math.max(0f, required - delivered - reserved);
                    if (outstanding <= 0f)
                    {
                        continue;
                    }

                    demands.Add(new LogisticsDemandEntry
                    {
                        SiteEntity = siteEntity,
                        ResourceTypeIndex = ghost.ValueRO.ResourceTypeIndex,
                        RequiredUnits = required,
                        DeliveredUnits = delivered,
                        ReservedUnits = reserved,
                        OutstandingUnits = outstanding,
                        Priority = 1,
                        LastUpdateTick = timeState.Tick,
                        ContextHash = BuildContextHash(siteEntity, ghost.ValueRO.ResourceTypeIndex)
                    });
                }

                board.ValueRW.LastUpdateTick = timeState.Tick;
            }
        }

        private static float SumReserved(DynamicBuffer<LogisticsReservationEntry> reservations, Entity site, ushort resourceTypeIndex)
        {
            var sum = 0f;
            for (int i = 0; i < reservations.Length; i++)
            {
                var entry = reservations[i];
                if (entry.Status != LogisticsReservationStatus.Active)
                {
                    continue;
                }

                if (entry.SiteEntity == site && entry.ResourceTypeIndex == resourceTypeIndex)
                {
                    sum += entry.Units;
                }
            }

            return sum;
        }

        private static uint BuildContextHash(Entity site, ushort resourceTypeIndex)
        {
            var seed = math.hash(new uint2((uint)site.Index + 1u, resourceTypeIndex));
            return seed == 0 ? 1u : seed;
        }
    }
}
