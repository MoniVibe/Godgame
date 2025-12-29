using Godgame.Construction;
using Godgame.Runtime;
using PureDOTS.Runtime.Components;
using PureDOTS.Runtime.Hand;
using PureDOTS.Runtime.Time;
using PureDOTS.Systems.Hand;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace Godgame.Systems.Interaction
{
    /// <summary>
    /// Handles dump commands targeting construction sites by feeding ConstructionIntake.
    /// </summary>
    [UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
    [UpdateAfter(typeof(HandCommandEmitterSystem))]
    public partial struct HandDumpToConstructionSystem : ISystem
    {
        private ComponentLookup<ConstructionIntake> _intakeLookup;
        private ComponentLookup<ConstructionGhost> _ghostLookup;

        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<Godgame.Runtime.DivineHandState>();
            state.RequireForUpdate<DivineHandConfig>();
            state.RequireForUpdate<HandCommand>();
            state.RequireForUpdate<HandPayload>();

            _intakeLookup = state.GetComponentLookup<ConstructionIntake>(false);
            _ghostLookup = state.GetComponentLookup<ConstructionGhost>(false);
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var timeState = SystemAPI.GetSingleton<TimeState>();
            uint currentTick = timeState.Tick;
            float deltaTime = timeState.FixedDeltaTime;

            _intakeLookup.Update(ref state);
            _ghostLookup.Update(ref state);

            foreach (var (handStateRef, configRef, payloadBuffer, commandBuffer) in SystemAPI
                         .Query<RefRW<Godgame.Runtime.DivineHandState>, RefRO<DivineHandConfig>, DynamicBuffer<HandPayload>, DynamicBuffer<HandCommand>>())
            {
                var handState = handStateRef.ValueRW;
                var config = configRef.ValueRO;
                var payload = payloadBuffer;
                var commands = commandBuffer;

                for (int i = commands.Length - 1; i >= 0; i--)
                {
                    var command = commands[i];
                    if (command.Tick != currentTick || command.Type != HandCommandType.Dump)
                    {
                        continue;
                    }

                    if (TryDumpToConstruction(ref handState, payload, in config, in command, deltaTime))
                    {
                        commands.RemoveAt(i);
                    }
                }

                handState.HeldAmount = (int)math.round(HandPayloadUtility.GetTotalAmount(payload));
                handState.HeldResourceTypeIndex = HandPayloadUtility.ResolveDominantType(payload, DivineHandConstants.NoResourceType);

                handStateRef.ValueRW = handState;
            }
        }

        private bool TryDumpToConstruction(ref Godgame.Runtime.DivineHandState handState,
            DynamicBuffer<HandPayload> payload,
            in DivineHandConfig config,
            in HandCommand command,
            float deltaTime)
        {
            var target = command.TargetEntity;
            if (target == Entity.Null || (!_intakeLookup.HasComponent(target) && !_ghostLookup.HasComponent(target)))
            {
                return false;
            }

            ConstructionIntake intake = default;
            ConstructionGhost ghost = default;
            bool hasIntake = _intakeLookup.HasComponent(target);
            bool hasGhost = _ghostLookup.HasComponent(target);

            if (hasIntake)
            {
                intake = _intakeLookup[target];
                if (intake.Paid >= intake.Cost)
                {
                    return true;
                }
            }

            if (hasGhost)
            {
                ghost = _ghostLookup[target];
                if (ghost.Paid >= ghost.Cost)
                {
                    return true;
                }
            }

            ushort resourceType = hasIntake ? intake.ResourceTypeIndex : ghost.ResourceTypeIndex;
            int remainingCost = hasIntake ? (intake.Cost - intake.Paid) : (ghost.Cost - ghost.Paid);

            float dumpRate = math.max(0f, config.DumpRate);
            int desiredUnits = math.max(1, (int)math.floor(dumpRate * deltaTime));
            desiredUnits = math.min(desiredUnits, remainingCost);
            if (desiredUnits <= 0)
            {
                return false;
            }

            float removed = HandPayloadUtility.RemoveAmount(ref payload, resourceType, desiredUnits);
            int removableUnits = (int)math.floor(removed);
            if (removableUnits <= 0)
            {
                return false;
            }

            if (hasIntake)
            {
                intake.Paid += removableUnits;
                intake.Paid = math.min(intake.Paid, intake.Cost);
                _intakeLookup[target] = intake;
            }

            if (hasGhost)
            {
                ghost.Paid += removableUnits;
                ghost.Paid = math.min(ghost.Paid, ghost.Cost);
                _ghostLookup[target] = ghost;
            }

            return true;
        }
    }
}
