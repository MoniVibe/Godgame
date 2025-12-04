using UnityEngine;

/// <summary>
/// Legacy debug camera rig script.
/// Stubbed to avoid ambiguous references to CameraRigService between
/// PureDOTS.Camera and PureDOTS.Runtime.
/// </summary>
public class DebugCameraRig : MonoBehaviour
{
    [ContextMenu("Print Debug Rig Status")]
    private void PrintStatus()
    {
        Debug.Log("[DebugCameraRig] Stub active – PureDOTS camera rig integration disabled.");
    }
}
