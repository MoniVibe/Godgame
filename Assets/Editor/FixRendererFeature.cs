using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering.Universal;
using System.Reflection;
using System.Linq;
using System.IO;

public class FixRendererFeature
{
    [MenuItem("Tools/Fix Renderer Feature")]
    public static void Fix()
    {
        string logPath = "Assets/FixLog.txt";
        using (StreamWriter writer = new StreamWriter(logPath))
        {
            writer.WriteLine("Starting Fix...");

            var rendererData = AssetDatabase.LoadAssetAtPath<UniversalRendererData>("Assets/Resources/Rendering/DemoURP_Renderer.asset");
            if (rendererData == null)
            {
                writer.WriteLine("Renderer Data not found!");
                return;
            }
            writer.WriteLine("Renderer Data found.");

            // Try to find the type
            var featureType = System.AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a => a.GetTypes())
                .FirstOrDefault(t => t.Name == "EntitiesGraphicsRendererFeature");

            if (featureType == null)
            {
                writer.WriteLine("EntitiesGraphicsRendererFeature type not found via Name search!");
                var assembly = System.AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(a => a.GetName().Name.Contains("Entities.Graphics"));
                if (assembly != null)
                {
                     writer.WriteLine($"Found assembly: {assembly.FullName}");
                     foreach(var t in assembly.GetTypes()) writer.WriteLine($" - {t.Name}");
                }
                else
                {
                    writer.WriteLine("Entities.Graphics assembly not found!");
                }
            }
            else
            {
                writer.WriteLine($"Found type: {featureType.FullName}");
            }

            if (featureType == null) return;

            // Check if feature already exists
            bool exists = rendererData.rendererFeatures.Any(f => f != null && f.GetType() == featureType);
            if (exists)
            {
                writer.WriteLine("EntitiesGraphicsRendererFeature already exists.");
                return;
            }

            // Add feature
            var feature = ScriptableObject.CreateInstance(featureType) as ScriptableRendererFeature;
            if (feature != null)
            {
                feature.name = "EntitiesGraphics";
                AssetDatabase.AddObjectToAsset(feature, rendererData);
                rendererData.rendererFeatures.Add(feature);
                rendererData.SetDirty();
                AssetDatabase.SaveAssets();
                writer.WriteLine("Added EntitiesGraphicsRendererFeature to renderer.");
            }
            else
            {
                writer.WriteLine("Failed to create instance of feature.");
            }
        }
        AssetDatabase.Refresh();
    }
}
