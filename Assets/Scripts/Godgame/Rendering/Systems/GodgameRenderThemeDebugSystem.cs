using PureDOTS.Input;
using PureDOTS.Rendering;
using Unity.Entities;
using UnityDebug = UnityEngine.Debug;

namespace Godgame.Rendering.Systems
{
    /// <summary>
    /// Simple harness to flip between two render themes via keyboard shortcut (F6).
    /// </summary>
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public partial struct GodgameRenderThemeDebugSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
#if !UNITY_EDITOR
            state.Enabled = false;
#endif
        }

        public void OnUpdate(ref SystemState state)
        {
#if UNITY_EDITOR
            if (!SystemAPI.TryGetSingletonRW<ActiveRenderTheme>(out var theme))
                return;

            if (Hotkeys.F6Down())
            {
                var newTheme = (ushort)(theme.ValueRO.ThemeId == 0 ? 1 : 0);
                theme.ValueRW = new ActiveRenderTheme { ThemeId = newTheme };
                UnityDebug.Log($"[GodgameRenderThemeDebugSystem] Swapped render theme to {newTheme}.");
            }
#endif
        }

        public void OnDestroy(ref SystemState state) { }
    }
}
