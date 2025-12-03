using Godgame.Resources;
using Godgame.Systems;
using PureDOTS.Runtime.Components;
using PureDOTS.Systems;
using Unity.Entities;
using Unity.Mathematics;

namespace Godgame.Systems.Resources
{
    /// <summary>
    /// Converts Divine Hand ground-drip requests into aggregate pile spawn commands when dropping resources on terrain.
    /// Runs before the core hand system so the base dump path does not attempt storehouse routing.
    /// </summary>
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public partial struct HandToAggregatePileSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<AggregatePileConfig>();
            state.RequireForUpdate<AggregatePileRuntimeState>();
            state.RequireForUpdate<DivineHandState>();
            state.RequireForUpdate<DivineHandCommand>();
        }

        public void OnUpdate(ref SystemState state)
        {
            var configEntity = SystemAPI.GetSingletonEntity<AggregatePileConfig>();
            var commands = EnsureSpawnBuffer(ref state, configEntity);
            var config = SystemAPI.GetSingleton<AggregatePileConfig>();

            foreach (var (handStateRef, commandRef) in SystemAPI
                         .Query<RefRW<DivineHandState>, RefRW<DivineHandCommand>>()
                         .WithAll<DivineHandTag>())
            {
                ref var handState = ref handStateRef.ValueRW;
                ref var command = ref commandRef.ValueRW;

                bool shouldSpawn = command.Type == DivineHandCommandType.Dump &&
                                   handState.HeldAmount > config.ConservationEpsilon &&
                                   handState.HeldResourceTypeIndex != DivineHandConstants.NoResourceType;
                if (!shouldSpawn)
                {
                    continue;
                }

                commands.Add(new AggregatePileSpawnCommand
                {
                    ResourceType = handState.HeldResourceTypeIndex,
                    Amount = handState.HeldAmount,
                    Position = handState.CursorPosition
                });

                handState.HeldAmount = 0;
                handState.HeldResourceTypeIndex = DivineHandConstants.NoResourceType;
                command.Type = DivineHandCommandType.None;
                command.TargetEntity = Entity.Null;
                command.TimeSinceIssued = 0f;
            }
        }

        private DynamicBuffer<AggregatePileSpawnCommand> EnsureSpawnBuffer(ref SystemState state, Entity configEntity)
        {
            if (!state.EntityManager.HasBuffer<AggregatePileSpawnCommand>(configEntity))
            {
                state.EntityManager.AddBuffer<AggregatePileSpawnCommand>(configEntity);
            }

            return state.EntityManager.GetBuffer<AggregatePileSpawnCommand>(configEntity);
        }

        public void OnDestroy(ref SystemState state)
        {
        }
    }
}
