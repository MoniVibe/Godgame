using UnityEngine;
using UnityEditor;

public class CheckShader
{
    public static void Execute()
    {
        var shaderName = "Universal Render Pipeline/Lit";
        var shader = Shader.Find(shaderName);
        if (shader != null)
        {
            Debug.Log($"Shader '{shaderName}' found.");
        }
        else
        {
            Debug.LogError($"Shader '{shaderName}' NOT found.");
            
            // List some URP shaders
            Debug.Log("Searching for URP shaders...");
            foreach (var s in Shader.Find("Universal Render Pipeline/Simple Lit") ? new[]{"Universal Render Pipeline/Simple Lit"} : new string[]{})
            {
                 Debug.Log($"Found: {s}");
            }
             foreach (var s in Shader.Find("Universal Render Pipeline/Unlit") ? new[]{"Universal Render Pipeline/Unlit"} : new string[]{})
            {
                 Debug.Log($"Found: {s}");
            }
        }
    }
}
