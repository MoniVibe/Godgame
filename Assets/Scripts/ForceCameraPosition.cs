using UnityEngine;

public class ForceCameraPosition : MonoBehaviour
{
    public Vector3 TargetPosition = new Vector3(0, 10, -20);
    public Vector3 TargetRotation = new Vector3(30, 0, 0);

    void LateUpdate()
    {
        transform.position = TargetPosition;
        transform.rotation = Quaternion.Euler(TargetRotation);
    }
}
