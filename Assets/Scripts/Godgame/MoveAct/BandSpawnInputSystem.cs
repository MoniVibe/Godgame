using Godgame.MoveAct;
using PureDOTS.Runtime.Components;
using PureDOTS.Runtime.Hand;
using PureDOTS.Systems.Input;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;

namespace Godgame.MoveAct
{
    /// <summary>
    /// Converts selection input into deterministic band spawn requests.
    /// </summary>
    [UpdateInGroup(typeof(SimulationSystemGroup), OrderFirst = true)]
    public partial struct BandSpawnInputSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<BandSpawnConfig>();
            state.RequireForUpdate<BandSpawnState>();
            state.RequireForUpdate<BandSpawnRequest>();
            state.RequireForUpdate<TimeState>();
            state.RequireForUpdate<RewindState>();
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

            var configEntity = SystemAPI.GetSingletonEntity<BandSpawnConfig>();
            var config = SystemAPI.GetComponent<BandSpawnConfig>(configEntity);
            var spawnState = SystemAPI.GetComponent<BandSpawnState>(configEntity);
            var requests = SystemAPI.GetBuffer<BandSpawnRequest>(configEntity);

            bool requested = false;

            foreach (var intent in SystemAPI.Query<RefRO<GodIntent>>().WithAll<DivineHandTag>())
            {
                var data = intent.ValueRO;
                if (data.StartSelect == 0)
                {
                    continue;
                }

                var requestedPosition = data.SelectPosition;
                if (!math.isfinite(requestedPosition.x) ||
                    !math.isfinite(requestedPosition.y) ||
                    !math.isfinite(requestedPosition.z))
                {
                    requestedPosition = spawnState.NextPosition;
                }

                requests.Add(new BandSpawnRequest { Position = requestedPosition });
                spawnState.NextPosition = requestedPosition + config.PositionStep;
                requested = true;
            }

            if (requested)
            {
                SystemAPI.SetComponent(configEntity, spawnState);
            }
        }
    }
}
