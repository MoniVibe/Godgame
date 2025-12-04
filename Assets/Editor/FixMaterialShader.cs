using UnityEngine;
using UnityEditor;

public class FixMaterialShader
{
    [MenuItem("Tools/Fix Material Shader")]
    public static void Fix()
    {
        string materialPath = "Assets/Materials/GreenURP.mat";
        Material mat = AssetDatabase.LoadAssetAtPath<Material>(materialPath);

        if (mat == null)
        {
            Debug.LogError($"Material not found at {materialPath}");
            return;
        }

        // Try Unlit first to rule out lighting issues
        Shader newShader = Shader.Find("Universal Render Pipeline/Unlit");
        if (newShader == null)
        {
            Debug.LogError("Shader 'Universal Render Pipeline/Unlit' not found.");
            return;
        }

        mat.shader = newShader;
        mat.enableInstancing = true;
        
        // Set a color to ensure it's visible
        if (mat.HasProperty("_BaseColor"))
            mat.SetColor("_BaseColor", Color.red);
        
        EditorUtility.SetDirty(mat);
        AssetDatabase.SaveAssets();
        
        Debug.Log($"Switched shader of {mat.name} to {newShader.name} and enabled instancing.");
    }
}
