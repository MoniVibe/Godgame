using UnityEngine;

namespace Godgame.DebugTools
{
    /// <summary>
    /// Stubbed-out camera rig applier to avoid type collisions between
    /// PureDOTS.Camera and PureDOTS.Runtime CameraRigApplier.
    /// </summary>
    public class DebugCameraRigApplier : MonoBehaviour
    {
        [ContextMenu("Apply Camera Rig (stub)")]
        private void ApplyRig()
        {
            Debug.Log(
                "[DebugCameraRigApplier] Stub called. " +
                "PureDOTS CameraRigApplier integration is currently disabled.");
        }
    }
}
