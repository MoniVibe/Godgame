using Godgame.MoveAct;
using PureDOTS.Input;
using PureDOTS.Runtime.Components;
using PureDOTS.Systems;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Godgame.MoveAct
{
    /// <summary>
    /// Lightweight keyboard/mouse fallback that seeds DOTS input when no InputSnapshotBridge is present.
    /// </summary>
    [UpdateInGroup(typeof(CameraInputSystemGroup), OrderFirst = true)]
    public partial struct MoveActInputFallbackSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<TimeState>();
            state.RequireForUpdate<RewindState>();
            state.RequireForUpdate<BandSpawnConfig>();
        }

        public void OnUpdate(ref SystemState state)
        {
            var timeState = SystemAPI.GetSingleton<TimeState>();
            var rewindState = SystemAPI.GetSingleton<RewindState>();
            if (timeState.IsPaused || rewindState.Mode != RewindMode.Record)
            {
                return;
            }

#if !ENABLE_LEGACY_INPUT_MANAGER
            return;
#else
            if (Object.FindFirstObjectByType<InputSnapshotBridge>(FindObjectsInactive.Include) != null)
            {
                return;
            }

            uint tick = timeState.Tick;
            bool appFocused = Application.isFocused;

            foreach (var (cameraInput, edges) in SystemAPI
                         .Query<RefRW<CameraInputState>, DynamicBuffer<CameraInputEdge>>())
            {
                var value = cameraInput.ValueRW;
                value.SampleTick = tick;
                value.AppHasFocus = (byte)(appFocused ? 1 : 0);
                value.PointerPosition = new float2(Input.mousePosition.x, Input.mousePosition.y);
                value.PointerOverUI = 0;
                value.OrbitDelta = float2.zero;
                value.PanDelta = float2.zero;
                value.ZoomDelta = Input.mouseScrollDelta.y;

                float2 move = float2.zero;
                if (Input.GetKey(KeyCode.W)) move.y += 1f;
                if (Input.GetKey(KeyCode.S)) move.y -= 1f;
                if (Input.GetKey(KeyCode.D)) move.x += 1f;
                if (Input.GetKey(KeyCode.A)) move.x -= 1f;
                value.MoveInput = move;

                value.VerticalMove = 0f;
                value.YAxisUnlocked = 0;
                value.ToggleYAxisTriggered = 0;

                cameraInput.ValueRW = value;
                edges.Clear();
            }

            float3 fallbackSpawn = float3.zero;
            if (SystemAPI.TryGetSingleton<BandSpawnState>(out var spawnState))
            {
                fallbackSpawn = spawnState.NextPosition;
            }

            foreach (var (handInput, handEdges) in SystemAPI
                         .Query<RefRW<DivineHandInput>, DynamicBuffer<HandInputEdge>>())
            {
                var value = handInput.ValueRW;
                value.SampleTick = tick;
                value.AppHasFocus = (byte)(appFocused ? 1 : 0);
                value.PointerPosition = new float2(Input.mousePosition.x, Input.mousePosition.y);
                value.PointerDelta = float2.zero;
                value.PrimaryHeld = (byte)(Input.GetMouseButton(0) ? 1 : 0);

                float3 cursor = fallbackSpawn;
                var camera = UnityEngine.Camera.main;
                if (camera != null)
                {
                    var ray = camera.ScreenPointToRay(Input.mousePosition);
                    var ground = new Plane(Vector3.up, Vector3.zero);
                    if (ground.Raycast(ray, out var enter))
                    {
                        var hit = ray.GetPoint(enter);
                        cursor = new float3(hit.x, hit.y, hit.z);
                    }
                }

                value.CursorWorldPosition = cursor;
                handInput.ValueRW = value;

                handEdges.Clear();
                if (Input.GetMouseButtonDown(0))
                {
                    handEdges.Add(new HandInputEdge
                    {
                        Button = InputButton.Primary,
                        Kind = InputEdgeKind.Down,
                        Tick = tick,
                        PointerPosition = value.PointerPosition
                    });
                }

                if (Input.GetMouseButtonUp(0))
                {
                    handEdges.Add(new HandInputEdge
                    {
                        Button = InputButton.Primary,
                        Kind = InputEdgeKind.Up,
                        Tick = tick,
                        PointerPosition = value.PointerPosition
                    });
                }
            }

            if (SystemAPI.TryGetSingletonRW<BandActionHotkeyState>(out var hotkey))
            {
                var value = hotkey.ValueRO;
                if (Input.GetKeyDown(KeyCode.Q))
                {
                    value.PlayPingRequested = 1;
                }

                hotkey.ValueRW = value;
            }
#endif
        }

        public void OnDestroy(ref SystemState state)
        {
        }
    }
}
