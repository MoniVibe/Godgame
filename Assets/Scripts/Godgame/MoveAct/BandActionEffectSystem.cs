using Godgame.Presentation;
using PureDOTS.Runtime.Components;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Godgame.MoveAct
{
    /// <summary>
    /// Emits a ping effect request against the current selection when the hotkey is triggered.
    /// </summary>
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(BandSpawnSystem))]
    public partial struct BandActionEffectSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<BandActionHotkeyState>();
            state.RequireForUpdate<BandSelection>();
            state.RequireForUpdate<PresentationRequestHub>();
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

            var hotkeyEntity = SystemAPI.GetSingletonEntity<BandActionHotkeyState>();
            var hotkey = SystemAPI.GetComponentRW<BandActionHotkeyState>(hotkeyEntity);
            if (hotkey.ValueRO.PlayPingRequested == 0)
            {
                return;
            }

            var selection = SystemAPI.GetSingleton<BandSelection>();
            if (selection.Selected == Entity.Null || !state.EntityManager.Exists(selection.Selected))
            {
                hotkey.ValueRW.PlayPingRequested = 0;
                return;
            }

            float3 targetPosition = selection.Position;
            if (state.EntityManager.HasComponent<LocalTransform>(selection.Selected))
            {
                targetPosition = state.EntityManager.GetComponentData<LocalTransform>(selection.Selected).Position;
            }

            var requestEntity = SystemAPI.GetSingletonEntity<PresentationRequestHub>();
            var requests = state.EntityManager.GetBuffer<PlayEffectRequest>(requestEntity);
            requests.Add(new PlayEffectRequest
            {
                EffectId = GodgamePresentationIds.MiraclePingEffectId,
                Target = selection.Selected,
                Position = targetPosition,
                Rotation = quaternion.identity,
                DurationSeconds = 1.5f,
                StyleOverride = GodgamePresentationIds.MiraclePingStyle
            });

            hotkey.ValueRW.PlayPingRequested = 0;
        }
    }
}
