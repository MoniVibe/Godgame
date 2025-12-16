using UnityEngine;
using UnityEditor;

public class FixGodgameMaterials
{
    public static void Execute()
    {
        Shader urpLit = Shader.Find("Universal Render Pipeline/Lit");
        if (urpLit == null)
        {
            Debug.LogError("Could not find 'Universal Render Pipeline/Lit' shader.");
            // Try fallback name if exact name changed in recent URP versions
            urpLit = Shader.Find("Universal Render Pipeline/Lit"); 
        }

        if (urpLit == null)
        {
             // List all shaders to find the correct one
             foreach (var shader in Shader.Find("Universal Render Pipeline/Lit") ? new[] { Shader.Find("Universal Render Pipeline/Lit") } : Resources.FindObjectsOfTypeAll<Shader>())
             {
                 if (shader.name.Contains("Lit") && shader.name.Contains("Universal"))
                 {
                     Debug.Log($"Found candidate shader: {shader.name}");
                     if (urpLit == null) urpLit = shader;
                 }
             }
        }

        if (urpLit == null)
        {
            Debug.LogError("Still could not find URP Lit shader.");
            return;
        }

        CreateOrUpdateMaterial("Assets/Materials/Godgame/Villager_Orange.mat", new Color(1f, 0.5f, 0f), urpLit);
        CreateOrUpdateMaterial("Assets/Materials/Godgame/VillageCenter_Gray.mat", new Color(0.5f, 0.5f, 0.5f), urpLit);
        CreateOrUpdateMaterial("Assets/Materials/Godgame/ResourceNode_Cyan.mat", Color.cyan, urpLit);
        CreateOrUpdateMaterial("Assets/Materials/Godgame/ResourceChunk_Gold.mat", new Color(1f, 0.84f, 0f), urpLit);
        CreateOrUpdateMaterial("Assets/Materials/Godgame/Vegetation_Green.mat", Color.green, urpLit);
        
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    private static void CreateOrUpdateMaterial(string path, Color color, Shader shader)
    {
        Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (mat == null)
        {
            mat = new Material(shader);
            AssetDatabase.CreateAsset(mat, path);
        }
        else
        {
            mat.shader = shader;
        }

        mat.SetColor("_BaseColor", color);
        mat.enableInstancing = true;
        EditorUtility.SetDirty(mat);
        Debug.Log($"Updated material at {path} with shader {shader.name} and color {color}");
    }
}
