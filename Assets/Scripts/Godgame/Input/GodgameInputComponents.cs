using Unity.Entities;
using Unity.Mathematics;

namespace Godgame.Input
{
    /// <summary>
    /// ECS component for camera input state.
    /// Written by GodgameInputReader MonoBehaviour, read by ECS systems.
    /// </summary>
    public struct CameraInput : IComponentData
    {
        /// <summary>WASD movement input</summary>
        public float2 Move;
        /// <summary>Vertical movement input (Q/E)</summary>
        public float Vertical;
        /// <summary>MMB drag rotation input</summary>
        public float2 Rotate;
        /// <summary>Scroll wheel zoom input</summary>
        public float Zoom;
        /// <summary>LMB drag pan input</summary>
        public float2 Pan;
        /// <summary>F key reset (edge trigger)</summary>
        public byte Focus; // 0 = false, 1 = true
        /// <summary>Current pointer screen position</summary>
        public float2 PointerPosition;
        /// <summary>Pointer world position (if valid)</summary>
        public float3 PointerWorldPosition;
        /// <summary>Whether pointer world position is valid</summary>
        public byte HasPointerWorld; // 0 = false, 1 = true
        /// <summary>Edge trigger for toggling Y-axis lock state</summary>
        public byte ToggleYAxisLock;
    }

    /// <summary>
    /// ECS component for miracle input state.
    /// Written by GodgameInputReader MonoBehaviour, read by miracle systems.
    /// </summary>
    public struct MiracleInput : IComponentData
    {
        /// <summary>Currently selected miracle slot (0-2)</summary>
        public byte SelectedSlot;
        /// <summary>Target world position (from raycast)</summary>
        public float3 TargetPosition;
        /// <summary>Target entity (villager, chunk, etc.)</summary>
        public Entity TargetEntity;
        /// <summary>Edge trigger for cast (1 on frame of click)</summary>
        public byte CastTriggered;
        /// <summary>Continuous hold state for sustained cast</summary>
        public byte SustainedCastHeld;
        /// <summary>Edge trigger for throw cast (1 on frame of release)</summary>
        public byte ThrowCastTriggered;
        /// <summary>Whether target position is valid</summary>
        public byte HasValidTarget;
    }

    /// <summary>
    /// ECS component for selection input state.
    /// Written by GodgameInputReader MonoBehaviour, read by selection systems.
    /// </summary>
    public struct SelectionInput : IComponentData
    {
        /// <summary>Click screen position</summary>
        public float2 ScreenPosition;
        /// <summary>Raycast hit entity</summary>
        public Entity SelectedEntity;
        /// <summary>Ctrl modifier for village selection</summary>
        public byte VillageSelect; // 0 = false, 1 = true
        /// <summary>Shift modifier for region selection</summary>
        public byte RegionSelect; // 0 = false, 1 = true
        /// <summary>Edge trigger for selection click</summary>
        public byte SelectTriggered;
        /// <summary>World position of selection</summary>
        public float3 WorldPosition;
    }

    /// <summary>
    /// ECS component for debug input state.
    /// </summary>
    public struct DebugInput : IComponentData
    {
        /// <summary>Toggle heatmap overlay (H key)</summary>
        public byte ToggleHeatmap;
        /// <summary>Toggle debug overlays (O key)</summary>
        public byte ToggleOverlays;
        /// <summary>Toggle LOD visualization (L key)</summary>
        public byte ToggleLOD;
        /// <summary>Toggle density sampling visualization (D key)</summary>
        public byte ToggleDensity;
        /// <summary>Toggle pathfinding debug (P key)</summary>
        public byte TogglePathfinding;
        /// <summary>Toggle dev menu (F12)</summary>
        public byte ToggleDevMenu;
        /// <summary>Toggle miracle designer (F4)</summary>
        public byte ToggleMiracleDesigner;
        /// <summary>Toggle entity inspection (I key)</summary>
        public byte ToggleEntityInspection;
        /// <summary>Toggle presentation metrics (O key)</summary>
        public byte TogglePresentationMetrics;
    }

    /// <summary>
    /// Tag component identifying the input singleton entity.
    /// </summary>
    public struct GodgameInputSingleton : IComponentData { }
}

