using Godgame.Resources;
using Godgame.Systems;
using PureDOTS.Runtime.Components;
using PureDOTS.Runtime.Hand;
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
            state.RequireForUpdate<TimeState>();
        }

        public void OnUpdate(ref SystemState state)
        {
            var currentTick = SystemAPI.GetSingleton<TimeState>().Tick;
            var configEntity = SystemAPI.GetSingletonEntity<AggregatePileConfig>();
            var commands = EnsureSpawnBuffer(ref state, configEntity);
            var config = SystemAPI.GetSingleton<AggregatePileConfig>();

            foreach (var (handStateRef, commandBuffer) in SystemAPI
                         .Query<RefRW<DivineHandState>, DynamicBuffer<HandCommand>>()
                         .WithAll<DivineHandTag>())
            {
                ref var handState = ref handStateRef.ValueRW;

                for (int i = commandBuffer.Length - 1; i >= 0; i--)
                {
                    var handCommand = commandBuffer[i];
                    if (handCommand.Tick != currentTick ||
                        handCommand.Type != HandCommandType.Dump ||
                        handCommand.TargetEntity != Entity.Null)
                    {
                        continue;
                    }

                    if (handState.HeldAmount <= config.ConservationEpsilon ||
                        handState.HeldResourceTypeIndex == DivineHandConstants.NoResourceType)
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
                    commandBuffer.RemoveAt(i);
                    break;
                }
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
