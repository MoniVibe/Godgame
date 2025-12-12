using UnityEngine;
using UnityEditor;
using PureDOTS.Authoring;
using System.IO;
#if LEGACY_PRESENTATION_ARCHIVE_ENABLED
using Godgame.Presentation;
#endif

public class CreateDemoAssets
{
    private const string ConfigFolder = "Assets/Godgame/Config";
    private const string RuntimeConfigPath = ConfigFolder + "/PureDotsRuntimeConfig.asset";
    private const string SpatialProfilePath = ConfigFolder + "/GodgameSpatialProfile.asset";

    [MenuItem("Tools/Create Demo Assets")]
    public static void Execute()
    {
        Directory.CreateDirectory(ConfigFolder);

        // Create PureDotsRuntimeConfig
        if (!File.Exists(RuntimeConfigPath))
        {
            var config = ScriptableObject.CreateInstance<PureDotsRuntimeConfig>();
            AssetDatabase.CreateAsset(config, RuntimeConfigPath);
        }

        // Create GodgameSpatialProfile
        if (!File.Exists(SpatialProfilePath))
        {
            var profile = ScriptableObject.CreateInstance<SpatialPartitionProfile>();
            // These properties are now read-only in PureDOTS and configured internally.
            // We rely on the defaults instead of overriding them here.
            // profile.MaxDirtyOpsForPartialRebuild     = 100;
            // profile.MaxDirtyRatioForPartialRebuild   = 0.1f;
            // profile.MinEntryCountForPartialRebuild   = 10;
            AssetDatabase.CreateAsset(profile, SpatialProfilePath);
        }

#if LEGACY_PRESENTATION_ARCHIVE_ENABLED
        // Create Bindings
        if (!File.Exists("Assets/Bindings/Minimal.asset"))
        {
            var minimal = ScriptableObject.CreateInstance<PresentationBindingSet>();
            minimal.Bindings = new BindingEntry[0];
            AssetDatabase.CreateAsset(minimal, "Assets/Bindings/Minimal.asset");
        }

        if (!File.Exists("Assets/Bindings/Fancy.asset"))
        {
            var fancy = ScriptableObject.CreateInstance<PresentationBindingSet>();
            fancy.Bindings = new BindingEntry[0];
            AssetDatabase.CreateAsset(fancy, "Assets/Bindings/Fancy.asset");
        }
#endif
        
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }
}
