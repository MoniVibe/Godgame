using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering.Universal;
using System.Reflection;
using System.Linq;
using System.IO;

[InitializeOnLoad]
public class AutoFixRendererFeature
{
    static AutoFixRendererFeature()
    {
        ListFeatures();
    }

    public static void ListFeatures()
    {
        string logPath = "Assets/RendererFeaturesList.txt";
        using (StreamWriter writer = new StreamWriter(logPath))
        {
            writer.WriteLine("Listing all ScriptableRendererFeatures:");
            var features = System.AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a => a.GetTypes())
                .Where(t => t.IsSubclassOf(typeof(ScriptableRendererFeature)) && !t.IsAbstract)
                .OrderBy(t => t.Name);

            foreach (var feature in features)
            {
                writer.WriteLine($"- {feature.Name} ({feature.FullName}) in {feature.Assembly.GetName().Name}");
            }
        }
    }
}
