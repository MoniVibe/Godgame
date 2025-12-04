using PureDOTS.Input;
using PureDOTS.Runtime.Components;
using PureDOTS.Systems;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Godgame.Miracles
{
    /// <summary>
    /// Reads hand input for miracle casting and spawns miracle tokens into the hand.
    /// </summary>
    [UpdateInGroup(typeof(HandSystemGroup))]
    [UpdateAfter(typeof(DivineHandSystem))]
    public partial struct GodgameMiracleInputSystem : ISystem
    {
        ComponentLookup<DivineHandState> _handLookup;

        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<MiracleCasterState>();
            state.RequireForUpdate<DivineHandInput>();
            state.RequireForUpdate<MiracleSlotDefinition>();
            _handLookup = state.GetComponentLookup<DivineHandState>(true);
        }

        public void OnUpdate(ref SystemState state)
        {
            _handLookup.Update(ref state);

            foreach (var (casterStateRef, inputRO, slotBuffer) in SystemAPI.Query<RefRW<MiracleCasterState>, RefRO<DivineHandInput>, DynamicBuffer<MiracleSlotDefinition>>())
            {
                var input = inputRO.ValueRO;
                ref var casterState = ref casterStateRef.ValueRW;

                // TODO: DivineHandInput doesn't currently have miracle-specific fields (SelectedSlot, SustainedCastHeld, ThrowCastTriggered)
                // These need to be added to DivineHandInput or read from a different source (e.g., keyboard input, UI input)
                // For now, using placeholder logic based on existing DivineHandInput fields:
                
                // Use PrimaryHeld for sustained cast (holding primary button)
                casterState.SustainedCastHeld = input.PrimaryHeld > 0 ? (byte)1 : (byte)0;
                
                // Use SecondaryHeld for throw cast trigger (secondary button press)
                // Note: This doesn't detect edge events, only continuous state
                // TODO: Use HandInputEdge buffer or add edge detection for proper trigger behavior
                if (input.SecondaryHeld > 0 && casterState.ThrowCastTriggered == 0)
                {
                    casterState.ThrowCastTriggered = 1;
                }

                if (casterState.ThrowCastTriggered == 1)
                {
                    if (TrySpawnMiracleToken(ref state, ref casterState, slotBuffer))
                    {
                        casterState.ThrowCastTriggered = 0;
                    }
                }
            }
        }

        private bool TrySpawnMiracleToken(ref SystemState state, ref MiracleCasterState casterState, DynamicBuffer<MiracleSlotDefinition> slots)
        {
            if (casterState.SelectedSlot >= slots.Length)
            {
                return false;
            }

            var slot = slots[casterState.SelectedSlot];
            if (casterState.HandEntity == Entity.Null)
            {
                return false;
            }

            if (!_handLookup.HasComponent(casterState.HandEntity))
            {
                return false;
            }

            var handState = _handLookup[casterState.HandEntity];
            if (handState.HeldEntity != Entity.Null)
            {
                return false;
            }

            var ecb = new EntityCommandBuffer(Allocator.Temp);
            Entity token;
            if (slot.MiraclePrefab != Entity.Null)
            {
                token = ecb.Instantiate(slot.MiraclePrefab);
            }
            else
            {
                token = ecb.CreateEntity();
                ecb.AddComponent(token, Unity.Transforms.LocalTransform.FromPositionRotationScale(handState.CursorPosition, quaternion.identity, 1f));
            }

            ecb.AddComponent(token, new MiracleCaster
            {
                CasterEntity = casterState.HandEntity,
                HandEntity = casterState.HandEntity
            });
            ecb.AddComponent(token, new MiracleToken
            {
                Type = slot.Type,
                CastingMode = MiracleCastingMode.Thrown, // Default for throw cast
                BaseRadius = 0f, // Will be populated from config/prefab
                BaseIntensity = 0f,
                BaseCost = 0f,
                SustainedCostPerSecond = 0f,
                Lifecycle = MiracleLifecycleState.Charging,
                ChargePercent = 0f,
                CurrentRadius = 0f,
                CurrentIntensity = 0f,
                CooldownSecondsRemaining = 0f,
                LastCastTick = 0,
                AlignmentDelta = 0,
                CasterHand = casterState.HandEntity,
                CastPosition = handState.CursorPosition,
                PrayerCost = 0f,
                AlignmentRequirement = 0f,
                DurationTicks = 0
            });
            ecb.AddComponent(token, new HandHeldTag
            {
                Holder = casterState.HandEntity
            });

            handState.HeldEntity = token;
            handState.HeldResourceTypeIndex = DivineHandConstants.NoResourceType;
            handState.HeldAmount = 0;
            _handLookup[casterState.HandEntity] = handState;
            ecb.Playback(state.EntityManager);
            ecb.Dispose();
            return true;
        }
    }
}
