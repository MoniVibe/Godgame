using UnityEngine;
using UnityEditor;

public class SetupVillagerMaterial
{
    [MenuItem("Tools/Setup Villager Material")]
    public static void Setup()
    {
        string materialPath = "Assets/Materials/VillagerRed.mat";
        Material mat = AssetDatabase.LoadAssetAtPath<Material>(materialPath);

        if (mat == null)
        {
            Debug.LogError($"Material not found at {materialPath}");
            return;
        }

        Shader shader = Shader.Find("Universal Render Pipeline/Lit");
        if (shader == null)
        {
            Debug.LogError("Shader 'Universal Render Pipeline/Lit' not found. Trying 'Universal Render Pipeline/Simple Lit'");
            shader = Shader.Find("Universal Render Pipeline/Simple Lit");
        }
        
        if (shader == null)
        {
             Debug.LogError("Shader 'Universal Render Pipeline/Simple Lit' not found. Trying 'Hidden/Universal Render Pipeline/Lit'");
             shader = Shader.Find("Hidden/Universal Render Pipeline/Lit");
        }

        if (shader != null)
        {
            mat.shader = shader;
            mat.enableInstancing = true;
            mat.color = Color.red;
            EditorUtility.SetDirty(mat);
            AssetDatabase.SaveAssets();
            Debug.Log($"Configured {mat.name} with shader {shader.name}");
        }
        else
        {
            Debug.LogError("Could not find a valid URP Lit shader.");
        }
    }
}
