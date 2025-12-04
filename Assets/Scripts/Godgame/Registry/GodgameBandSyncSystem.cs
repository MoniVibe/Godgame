using Godgame.Bands;
using PureDOTS.Runtime.Bands;
using PureDOTS.Runtime.Components;
using PureDOTS.Runtime.Registry;
using PureDOTS.Systems;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Godgame.Registry
{
    /// <summary>
    /// Bridges Godgame-specific band components to PureDOTS canonical BandId and BandStats components.
    /// Follows projection pattern: if entity has PureDOTS BandId/BandStats, leave it alone.
    /// If entity has Godgame band components but not canonical, project/add canonical components.
    /// </summary>
    [BurstCompile]
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateBefore(typeof(SpatialSystemGroup))]
    public partial struct GodgameBandSyncSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            // Require PureDOTS registry and time state
            state.RequireForUpdate<BandRegistry>();
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

            // 1. PROJECT: Query entities with Godgame band components but no BandId/BandStats
            foreach (var (ggBand, transform, entity) in SystemAPI
                     .Query<RefRO<Band>, RefRO<LocalTransform>>()
                     .WithNone<BandId, BandStats>()
                     .WithEntityAccess())
            {
                var memberCount = 0;
                if (state.EntityManager.HasBuffer<Godgame.Bands.BandMember>(entity))
                {
                    var memberBuffer = state.EntityManager.GetBuffer<Godgame.Bands.BandMember>(entity);
                    memberCount = memberBuffer.Length;
                }
                
                ecb.AddComponent(entity, new BandId
                {
                    Value = ggBand.ValueRO.Id,
                    FactionId = ggBand.ValueRO.FactionId,
                    Leader = ggBand.ValueRO.Leader
                });
                
                ecb.AddComponent(entity, new BandStats
                {
                    MemberCount = memberCount,
                    AverageDiscipline = 0f, // TODO: Calculate from villager components if available
                    Morale = ggBand.ValueRO.Morale,
                    Cohesion = ggBand.ValueRO.Cohesion,
                    Fatigue = ggBand.ValueRO.Fatigue,
                    Flags = MapStatusFlags(ggBand.ValueRO.Status),
                    LastUpdateTick = timeState.Tick
                });
                
                // Bridge BandFormation if it exists
                if (state.EntityManager.HasComponent<Godgame.Bands.BandFormation>(entity))
                {
                    var ggFormation = state.EntityManager.GetComponentData<Godgame.Bands.BandFormation>(entity);
                    ecb.AddComponent(entity, new PureDOTS.Runtime.Bands.BandFormation
                    {
                        Formation = MapFormationType(ggFormation.Formation),
                        Spacing = ggFormation.Spacing,
                        Width = ggFormation.Width,
                        Depth = ggFormation.Depth,
                        Facing = ggFormation.Facing,
                        Anchor = ggFormation.Anchor,
                        Stability = ggFormation.Stability,
                        LastSolveTick = ggFormation.LastSolveTick
                    });
                }
                
                ecb.AddComponent<SyncedFromGodgame>(entity);
            }

            // 2. UPDATE: Query entities with both Godgame and canonical components
            foreach (var (ggBand, stats, entity) in SystemAPI
                     .Query<RefRO<Band>, RefRW<BandStats>>()
                     .WithChangeFilter<Band>()
                     .WithEntityAccess())
            {
                var memberCount = 0;
                if (state.EntityManager.HasBuffer<Godgame.Bands.BandMember>(entity))
                {
                    var memberBuffer = state.EntityManager.GetBuffer<Godgame.Bands.BandMember>(entity);
                    memberCount = memberBuffer.Length;
                }
                
                stats.ValueRW.MemberCount = memberCount;
                // AverageDiscipline kept as-is (would need villager lookup to recalculate)
                stats.ValueRW.Morale = ggBand.ValueRO.Morale;
                stats.ValueRW.Cohesion = ggBand.ValueRO.Cohesion;
                stats.ValueRW.Fatigue = ggBand.ValueRO.Fatigue;
                stats.ValueRW.Flags = MapStatusFlags(ggBand.ValueRO.Status);
                stats.ValueRW.LastUpdateTick = timeState.Tick;
            }

            // 2a. PROJECT: Add BandFormation if Godgame has it but PureDOTS doesn't
            foreach (var (ggFormation, entity) in SystemAPI
                     .Query<RefRO<Godgame.Bands.BandFormation>>()
                     .WithNone<PureDOTS.Runtime.Bands.BandFormation>()
                     .WithEntityAccess())
            {
                ecb.AddComponent(entity, new PureDOTS.Runtime.Bands.BandFormation
                {
                    Formation = MapFormationType(ggFormation.ValueRO.Formation),
                    Spacing = ggFormation.ValueRO.Spacing,
                    Width = ggFormation.ValueRO.Width,
                    Depth = ggFormation.ValueRO.Depth,
                    Facing = ggFormation.ValueRO.Facing,
                    Anchor = ggFormation.ValueRO.Anchor,
                    Stability = ggFormation.ValueRO.Stability,
                    LastSolveTick = ggFormation.ValueRO.LastSolveTick
                });
            }

            // 2b. UPDATE: Sync BandFormation if it exists
            foreach (var (ggFormation, formation) in SystemAPI
                     .Query<RefRO<Godgame.Bands.BandFormation>, RefRW<PureDOTS.Runtime.Bands.BandFormation>>()
                     .WithChangeFilter<Godgame.Bands.BandFormation>())
            {
                formation.ValueRW.Formation = MapFormationType(ggFormation.ValueRO.Formation);
                formation.ValueRW.Spacing = ggFormation.ValueRO.Spacing;
                formation.ValueRW.Width = ggFormation.ValueRO.Width;
                formation.ValueRW.Depth = ggFormation.ValueRO.Depth;
                formation.ValueRW.Facing = ggFormation.ValueRO.Facing;
                formation.ValueRW.Anchor = ggFormation.ValueRO.Anchor;
                formation.ValueRW.Stability = ggFormation.ValueRO.Stability;
                formation.ValueRW.LastSolveTick = ggFormation.ValueRO.LastSolveTick;
            }

            // 3. CLEANUP: Query entities with BandId/BandStats + SyncedFromGodgame but no Godgame source
            foreach (var (bandId, stats, entity) in SystemAPI
                     .Query<RefRO<BandId>, RefRO<BandStats>>()
                     .WithAll<SyncedFromGodgame>()
                     .WithNone<Band>()
                     .WithEntityAccess())
            {
                ecb.RemoveComponent<BandId>(entity);
                ecb.RemoveComponent<BandStats>(entity);
                // Also remove BandFormation if it was synced
                if (state.EntityManager.HasComponent<PureDOTS.Runtime.Bands.BandFormation>(entity))
                {
                    ecb.RemoveComponent<PureDOTS.Runtime.Bands.BandFormation>(entity);
                }
                ecb.RemoveComponent<SyncedFromGodgame>(entity);
            }
        }

        private static BandStatusFlags MapStatusFlags(BandStatus status)
        {
            var flags = BandStatusFlags.None;
            if ((status & BandStatus.Idle) != 0) flags |= BandStatusFlags.Idle;
            if ((status & BandStatus.Moving) != 0) flags |= BandStatusFlags.Moving;
            if ((status & BandStatus.Engaged) != 0) flags |= BandStatusFlags.Engaged;
            if ((status & BandStatus.Routing) != 0) flags |= BandStatusFlags.Routing;
            if ((status & BandStatus.Resting) != 0) flags |= BandStatusFlags.Resting;
            return flags;
        }

        private static PureDOTS.Runtime.Bands.BandFormationType MapFormationType(Godgame.Bands.BandFormationType formation)
        {
            return formation switch
            {
                Godgame.Bands.BandFormationType.Column => PureDOTS.Runtime.Bands.BandFormationType.Column,
                Godgame.Bands.BandFormationType.Line => PureDOTS.Runtime.Bands.BandFormationType.Line,
                Godgame.Bands.BandFormationType.Wedge => PureDOTS.Runtime.Bands.BandFormationType.Wedge,
                Godgame.Bands.BandFormationType.Circle => PureDOTS.Runtime.Bands.BandFormationType.Circle,
                Godgame.Bands.BandFormationType.Skirmish => PureDOTS.Runtime.Bands.BandFormationType.Line, // Fallback
                Godgame.Bands.BandFormationType.ShieldWall => PureDOTS.Runtime.Bands.BandFormationType.Line, // Fallback
                _ => PureDOTS.Runtime.Bands.BandFormationType.Column
            };
        }
    }
}

