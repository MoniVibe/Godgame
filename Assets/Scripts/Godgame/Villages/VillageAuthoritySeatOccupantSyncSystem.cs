using PureDOTS.Runtime.Authority;
using PureDOTS.Runtime.Components;
using Unity.Burst;
using Unity.Entities;

namespace Godgame.Villages
{
    /// <summary>
    /// Syncs the village executive seat occupant from the current "influence" entity.
    /// This is the initial bridge from existing village leadership heuristics into the shared authority-seat model.
    /// </summary>
    [BurstCompile]
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(VillageNeedAwarenessSystem))]
    [UpdateBefore(typeof(VillageAIDecisionSystem))]
    public partial struct VillageAuthoritySeatOccupantSyncSystem : ISystem
    {
        private ComponentLookup<AuthoritySeatOccupant> _occupantLookup;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<TimeState>();
            state.RequireForUpdate<RewindState>();
            state.RequireForUpdate<AuthorityBody>();
            state.RequireForUpdate<VillageNeedAwareness>();

            _occupantLookup = state.GetComponentLookup<AuthoritySeatOccupant>(false);
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var timeState = SystemAPI.GetSingleton<TimeState>();
            var rewindState = SystemAPI.GetSingleton<RewindState>();
            if (timeState.IsPaused || rewindState.Mode != RewindMode.Record)
            {
                return;
            }

            _occupantLookup.Update(ref state);

            foreach (var (body, awareness) in SystemAPI.Query<RefRO<AuthorityBody>, RefRO<VillageNeedAwareness>>())
            {
                var seatEntity = body.ValueRO.ExecutiveSeat;
                if (seatEntity == Entity.Null || !_occupantLookup.HasComponent(seatEntity))
                {
                    continue;
                }

                var desiredOccupant = awareness.ValueRO.InfluenceEntity;
                var occupant = _occupantLookup[seatEntity];
                if (occupant.OccupantEntity == desiredOccupant)
                {
                    continue;
                }

                occupant.OccupantEntity = desiredOccupant;
                occupant.AssignedTick = timeState.Tick;
                occupant.LastChangedTick = timeState.Tick;
                occupant.IsActing = 0;
                _occupantLookup[seatEntity] = occupant;
            }
        }
    }
}

