using UnityEngine;
using UnityEditor;
using Godgame.Controls;

public class FixCamera
{
    [MenuItem("Tools/Fix Camera")]
    public static void Execute()
    {
        var rig = Object.FindFirstObjectByType<GodgameCameraInputBehaviour>();
        if (rig != null)
        {
            rig.enabled = false;
            Debug.Log("Disabled GodgameCameraInputBehaviour.");
        }

        var camera = Camera.main;
        if (camera != null)
        {
            // Set position and look at
            camera.transform.position = new Vector3(-19.5f, 10f, 15.1f);
            camera.transform.LookAt(new Vector3(-19.5f, 0f, 15.1f));
            
            // Set culling mask to Everything
            camera.cullingMask = -1; // -1 is Everything
            
            // Set clipping planes
            camera.nearClipPlane = 0.1f;
            camera.farClipPlane = 1000f;

            Debug.Log("Set Camera Transform to look at villager position.");
            Debug.Log("Set Culling Mask to Everything.");
        }
        else
        {
            Debug.LogError("Main Camera not found!");
        }
    }
}
