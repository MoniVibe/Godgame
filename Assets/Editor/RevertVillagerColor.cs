using UnityEngine;
using UnityEditor;

public class RevertVillagerColor
{
    public static void Execute()
    {
        var mat = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/Godgame/Villager_Orange.mat");
        if (mat != null)
        {
            mat.SetColor("_BaseColor", new Color(1f, 0.5f, 0f)); // Orange
            EditorUtility.SetDirty(mat);
            Debug.Log("Reverted Villager_Orange to Orange");
        }
        AssetDatabase.SaveAssets();
    }
}
