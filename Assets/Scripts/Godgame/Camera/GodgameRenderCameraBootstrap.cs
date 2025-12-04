using System;
using UnityEngine;

namespace Moni.Godgame.CameraSystems
{
    /// <summary>
    /// Ensures a render camera exists at runtime and is driven by CameraRigApplier.
    /// </summary>
    public static class GodgameRenderCameraBootstrap
    {
        // TEMPORARILY DISABLED: CameraRigApplier conflicts with direct controller
        // TODO: Re-enable when CameraRigService state is properly driven by controller
        private const bool ENABLE_RIG_APPLIER = false;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void EnsureRenderCamera()
        {
            var main = UnityEngine.Camera.main;
            if (main == null)
            {
                var fallback = UnityEngine.Object.FindFirstObjectByType<UnityEngine.Camera>();
                if (fallback != null)
                {
                    main = fallback;
                    main.tag = "MainCamera";
                    Debug.Log("[GodgameCamera] Tagged existing camera as MainCamera.");
                }
                else
                {
                    var go = new GameObject("Godgame Render Camera");
                    main = go.AddComponent<UnityEngine.Camera>();
                    main.tag = "MainCamera";
                    main.clearFlags = CameraClearFlags.Skybox;
                    Debug.Log("[GodgameCamera] Spawned fallback render camera.");
                }
            }

#if false
            // TEMPORARILY DISABLED: CameraRigApplier type ambiguity between PureDOTS.Camera and PureDOTS.Runtime
            // TODO: Resolve duplicate type definitions or use reflection/extern aliases
            if (ENABLE_RIG_APPLIER && main.GetComponent<global::PureDOTS.Runtime.Camera.CameraRigApplier>() == null)
            {
                main.gameObject.AddComponent<global::PureDOTS.Runtime.Camera.CameraRigApplier>();
                Debug.Log("[GodgameCamera] Attached CameraRigApplier to render camera.");
            }
            else if (!ENABLE_RIG_APPLIER)
            {
                // Remove any existing applier to prevent conflicts
                var applier = main.GetComponent<global::PureDOTS.Runtime.Camera.CameraRigApplier>();
                if (applier != null)
                {
                    UnityEngine.Object.Destroy(applier);
                    Debug.Log("[GodgameCamera] Removed CameraRigApplier to prevent conflicts.");
                }
            }
#endif
        }
    }
}

