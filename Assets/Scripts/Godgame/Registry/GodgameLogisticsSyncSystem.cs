using Godgame.Logistics;
using PureDOTS.Runtime.Components;
using PureDOTS.Runtime.Transport;
using PureDOTS.Runtime.Registry;
using PureDOTS.Systems;
using Unity.Burst;
using Unity.Entities;

namespace Godgame.Registry
{
    /// <summary>
    /// Bridges Godgame-specific logistics components to PureDOTS canonical LogisticsRequest components.
    /// Follows projection pattern: if entity has PureDOTS LogisticsRequest, leave it alone.
    /// If entity has Godgame logistics components but not canonical, project/add canonical components.
    /// </summary>
    [BurstCompile]
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateBefore(typeof(TransportPhaseGroup))]
    public partial struct GodgameLogisticsSyncSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            // Require PureDOTS registry and time state
            state.RequireForUpdate<LogisticsRequestRegistry>();
            state.RequireForUpdate<TimeState>();
            state.RequireForUpdate<RewindState>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var timeState = SystemAPI.GetSingleton<TimeState>();
            var rewindState = SystemAPI.GetSingleton<RewindState>();
            
            // Skip if paused or not recording
            if (timeState.IsPaused || rewindState.Mode != RewindMode.Record)
            {
                return;
            }

            var ecb = SystemAPI
                .GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>()
                .CreateCommandBuffer(state.WorldUnmanaged);

            // 1. PROJECT: Query entities with Godgame logistics components but no LogisticsRequest
            foreach (var (ggTransport, entity) in SystemAPI
                     .Query<RefRO<TransportOrder>>()
                     .WithNone<LogisticsRequest>()
                     .WithEntityAccess())
            {
                ecb.AddComponent(entity, new LogisticsRequest
                {
                    SourceEntity = ggTransport.ValueRO.SourceEntity,
                    DestinationEntity = ggTransport.ValueRO.DestinationEntity,
                    SourcePosition = ggTransport.ValueRO.SourcePosition,
                    DestinationPosition = ggTransport.ValueRO.DestinationPosition,
                    ResourceTypeIndex = ggTransport.ValueRO.ResourceTypeIndex,
                    RequestedUnits = ggTransport.ValueRO.RequestedUnits,
                    FulfilledUnits = ggTransport.ValueRO.FulfilledUnits,
                    Priority = MapPriority(ggTransport.ValueRO.Priority),
                    Flags = MapFlags(ggTransport.ValueRO.Flags),
                    CreatedTick = ggTransport.ValueRO.CreatedTick != 0 ? ggTransport.ValueRO.CreatedTick : timeState.Tick,
                    LastUpdateTick = timeState.Tick
                });
                ecb.AddComponent<SyncedFromGodgame>(entity);
            }

            // 2. UPDATE: Query entities with both Godgame and canonical components
            foreach (var (ggTransport, req) in SystemAPI
                     .Query<RefRO<TransportOrder>, RefRW<LogisticsRequest>>()
                     .WithChangeFilter<TransportOrder>())
            {
                req.ValueRW.SourceEntity = ggTransport.ValueRO.SourceEntity;
                req.ValueRW.DestinationEntity = ggTransport.ValueRO.DestinationEntity;
                req.ValueRW.SourcePosition = ggTransport.ValueRO.SourcePosition;
                req.ValueRW.DestinationPosition = ggTransport.ValueRO.DestinationPosition;
                req.ValueRW.ResourceTypeIndex = ggTransport.ValueRO.ResourceTypeIndex;
                req.ValueRW.RequestedUnits = ggTransport.ValueRO.RequestedUnits;
                req.ValueRW.FulfilledUnits = ggTransport.ValueRO.FulfilledUnits;
                req.ValueRW.Priority = MapPriority(ggTransport.ValueRO.Priority);
                req.ValueRW.Flags = MapFlags(ggTransport.ValueRO.Flags);
                req.ValueRW.LastUpdateTick = timeState.Tick;
            }

            // 3. CLEANUP: Query entities with LogisticsRequest + SyncedFromGodgame but no Godgame source
            foreach (var (req, entity) in SystemAPI
                     .Query<RefRO<LogisticsRequest>>()
                     .WithAll<SyncedFromGodgame>()
                     .WithNone<TransportOrder>()
                     .WithEntityAccess())
            {
                ecb.RemoveComponent<LogisticsRequest>(entity);
                ecb.RemoveComponent<SyncedFromGodgame>(entity);
            }
        }

        private static LogisticsRequestPriority MapPriority(TransportPriority priority)
        {
            return priority switch
            {
                TransportPriority.Low => LogisticsRequestPriority.Low,
                TransportPriority.Normal => LogisticsRequestPriority.Normal,
                TransportPriority.High => LogisticsRequestPriority.High,
                TransportPriority.Critical => LogisticsRequestPriority.Critical,
                _ => LogisticsRequestPriority.Normal
            };
        }

        private static LogisticsRequestFlags MapFlags(TransportFlags flags)
        {
            var result = LogisticsRequestFlags.None;
            if ((flags & TransportFlags.Urgent) != 0) result |= LogisticsRequestFlags.Urgent;
            if ((flags & TransportFlags.Blocking) != 0) result |= LogisticsRequestFlags.Blocking;
            if ((flags & TransportFlags.PlayerPinned) != 0) result |= LogisticsRequestFlags.PlayerPinned;
            return result;
        }
    }
}

