using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering.Universal;
using System.Linq;
using System.Reflection;
using System.IO;

public class ListRendererFeatures
{
    [MenuItem("Tools/List Renderer Features")]
    public static void List()
    {
        var features = System.AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(a => a.GetTypes())
            .Where(t => t.IsSubclassOf(typeof(ScriptableRendererFeature)) && !t.IsAbstract)
            .OrderBy(t => t.Name);

        string path = "RendererFeaturesList.txt";
        using (StreamWriter writer = new StreamWriter(path))
        {
            writer.WriteLine("Available Renderer Features:");
            foreach (var feature in features)
            {
                writer.WriteLine($"- {feature.Name} ({feature.FullName})");
            }
        }
    }
}
