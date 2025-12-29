using Godgame.Resources;
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
        private ComponentLookup<AggregatePile> _pileLookup;

        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<DivineHandState>();
            state.RequireForUpdate<DivineHandConfig>();
            state.RequireForUpdate<HandCommand>();
            state.RequireForUpdate<HandPayload>();

            _siphonLookup = state.GetComponentLookup<SiphonSource>(false);
            _pileLookup = state.GetComponentLookup<AggregatePile>(false);
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var timeState = SystemAPI.GetSingleton<TimeState>();
            uint currentTick = timeState.Tick;
            float deltaTime = timeState.FixedDeltaTime;

            _siphonLookup.Update(ref state);
            _pileLookup.Update(ref state);

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

                    if (TrySiphon(ref handState, payload, in config, in command, currentTick, deltaTime))
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
            uint currentTick,
            float deltaTime)
        {
            if (handState.HeldAmount >= handState.HeldCapacity)
            {
                return false;
            }

            var target = command.TargetEntity;
            if (target == Entity.Null || (!_siphonLookup.HasComponent(target) && !_pileLookup.HasComponent(target)))
            {
                return true; // remove command when target disappears
            }

            ushort resourceType = DivineHandConstants.NoResourceType;
            float availableAmount = 0f;
            bool hasAggregatePile = _pileLookup.HasComponent(target);
            AggregatePile pile = default;
            SiphonSource source = default;

            if (hasAggregatePile)
            {
                pile = _pileLookup[target];
                resourceType = pile.ResourceTypeIndex;
                availableAmount = pile.Amount;
            }
            else
            {
                source = _siphonLookup[target];
                resourceType = source.ResourceTypeIndex;
                availableAmount = source.Amount;
            }

            if (availableAmount <= 0f)
            {
                return true;
            }

            float rate = math.max(0f, config.SiphonRate);
            float potentialUnits = math.min(rate * deltaTime, availableAmount);
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

            if (hasAggregatePile)
            {
                var removed = pile.Remove(units, currentTick);
                units = math.min(units, removed);
                _pileLookup[target] = pile;
                if (_siphonLookup.HasComponent(target))
                {
                    source = _siphonLookup[target];
                    source.Amount = pile.Amount;
                    source.ResourceTypeIndex = pile.ResourceTypeIndex;
                    _siphonLookup[target] = source;
                }
            }
            else
            {
                source.Amount -= units;
                _siphonLookup[target] = source;
            }

            resourceType = command.ResourceTypeIndex != DivineHandConstants.NoResourceType
                ? command.ResourceTypeIndex
                : resourceType;
            HandPayloadUtility.AddAmount(ref payload, resourceType, units);

            handState.HeldAmount = (int)math.round(HandPayloadUtility.GetTotalAmount(payload));
            handState.HeldResourceTypeIndex = HandPayloadUtility.ResolveDominantType(payload, DivineHandConstants.NoResourceType);

            return true;
        }
    }
}
