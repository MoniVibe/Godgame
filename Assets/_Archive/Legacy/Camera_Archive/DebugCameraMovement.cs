using UnityEngine;
using PureDOTS.Runtime.Camera;

public class DebugCameraMovement : MonoBehaviour
{
    private Vector3 lastPos;
    private Quaternion lastRot;

    void Start()
    {
        lastPos = transform.position;
        lastRot = transform.rotation;
    }

    void LateUpdate()
    {
        if (Vector3.Distance(transform.position, lastPos) > 0.001f || Quaternion.Angle(transform.rotation, lastRot) > 0.1f)
        {
            Debug.Log($"[DebugCameraMovement] Camera moved to {transform.position}, Rot: {transform.rotation.eulerAngles}");
            lastPos = transform.position;
            lastRot = transform.rotation;
        }
    }
}
