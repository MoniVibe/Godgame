using UnityEngine;

/// <summary>
/// Legacy helper for old PureDOTS camera rigs.
/// Stubbed out to avoid type collisions between PureDOTS.Camera and PureDOTS.Runtime.
/// </summary>
public class DebugCameraComponents : MonoBehaviour
{
    [ContextMenu("Print Debug Camera Info")]
    private void PrintInfo()
    {
        Debug.Log("[DebugCameraComponents] Stub active – PureDOTS camera debug components disabled.");
    }
}
