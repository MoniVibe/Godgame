#if UNITY_EDITOR && !ENABLE_LEGACY_INPUT_MANAGER
using UnityEditor;
using UnityEngine;
using Unity.Rendering;
using UResources = UnityEngine.Resources;

namespace Godgame.Diagnostics
{
    [InitializeOnLoad]
    public static class GodgameEntitiesGraphicsStatsDrawerGuard
    {
        static GodgameEntitiesGraphicsStatsDrawerGuard()
        {
            EditorApplication.delayCall += DisableStatsDrawer;
            EditorApplication.hierarchyChanged += DisableStatsDrawer;
            EditorApplication.playModeStateChanged += _ => DisableStatsDrawer();
        }

        private static void DisableStatsDrawer()
        {
            var drawers = UResources.FindObjectsOfTypeAll<EntitiesGraphicsStatsDrawer>();
            if (drawers == null || drawers.Length == 0)
            {
                return;
            }

            for (int i = 0; i < drawers.Length; i++)
            {
                var drawer = drawers[i];
                if (drawer != null && drawer.enabled)
                {
                    drawer.enabled = false;
                }
            }
        }
    }
}
#endif
