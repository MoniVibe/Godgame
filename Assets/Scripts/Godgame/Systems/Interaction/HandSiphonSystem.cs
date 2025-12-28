using Godgame.Runtime;
using PureDOTS.Runtime.Components;
using PureDOTS.Runtime.Hand;
using PureDOTS.Runtime.Time;
using PureDOTS.Systems.Hand;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using DivineHandState = Godgame.Runtime.DivineHandState;
using DivineHandConfig = Godgame.Runtime.DivineHandConfig;

namespace Godgame.Systems.Interaction
{
    /// <summary>
    /// Consumes HandCommand.Siphon commands and transfers resources from SiphonSource entities into the hand payload.
    /// </summary>
    [UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
    [UpdateAfter(typeof(HandCommandEmitterSystem))]
    public partial struct HandSiphonSystem : ISystem
    {
        private ComponentLookup<SiphonSource> _siphonLookup;

        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<DivineHandState>();
            state.RequireForUpdate<DivineHandConfig>();
            state.RequireForUpdate<HandCommand>();
            state.RequireForUpdate<HandPayload>();

            _siphonLookup = state.GetComponentLookup<SiphonSource>(false);
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var timeState = SystemAPI.GetSingleton<TimeState>();
            uint currentTick = timeState.Tick;
            float deltaTime = timeState.FixedDeltaTime;

            _siphonLookup.Update(ref state);

            foreach (var (handStateRef, configRef, payloadBuffer, commandBuffer) in SystemAPI
                         .Query<RefRW<DivineHandState>, RefRO<DivineHandConfig>, DynamicBuffer<HandPayload>, DynamicBuffer<HandCommand>>())
            {
                var handState = handStateRef.ValueRW;
                var config = configRef.ValueRO;
                var payload = payloadBuffer;
                var commands = commandBuffer;

                for (int i = commands.Length - 1; i >= 0; i--)
                {
                    var command = commands[i];
                    if (command.Tick != currentTick || command.Type != HandCommandType.Siphon)
                    {
                        continue;
                    }

                    if (TrySiphon(ref handState, payload, in config, in command, deltaTime))
                    {
                        commands.RemoveAt(i);
                    }
                }

                handStateRef.ValueRW = handState;
            }
        }

        private bool TrySiphon(ref DivineHandState handState,
            DynamicBuffer<HandPayload> payload,
            in DivineHandConfig config,
            in HandCommand command,
            float deltaTime)
        {
            if (handState.HeldAmount >= handState.HeldCapacity)
            {
                return false;
            }

            var target = command.TargetEntity;
            if (target == Entity.Null || !_siphonLookup.HasComponent(target))
            {
                return true; // remove command when target disappears
            }

            var source = _siphonLookup[target];
            if (source.Amount <= 0f)
            {
                return true;
            }

            float rate = math.max(0f, config.SiphonRate);
            float potentialUnits = math.min(rate * deltaTime, source.Amount);
            int capacityRemaining = math.max(0, handState.HeldCapacity - handState.HeldAmount);
            if (capacityRemaining <= 0)
            {
                return false;
            }

            float units = math.min(capacityRemaining, math.floor(potentialUnits));
            if (units <= 0f)
            {
                return false;
            }

            source.Amount -= units;
            _siphonLookup[target] = source;

            ushort resourceType = command.ResourceTypeIndex != DivineHandConstants.NoResourceType
                ? command.ResourceTypeIndex
                : source.ResourceTypeIndex;
            HandPayloadUtility.AddAmount(ref payload, resourceType, units);

            handState.HeldAmount = (int)math.round(HandPayloadUtility.GetTotalAmount(payload));
            handState.HeldResourceTypeIndex = HandPayloadUtility.ResolveDominantType(payload, DivineHandConstants.NoResourceType);

            return true;
        }
    }
}

