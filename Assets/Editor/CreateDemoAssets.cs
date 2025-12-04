using UnityEngine;
using UnityEditor;
using PureDOTS.Authoring;
using Godgame.Presentation;
using System.IO;

public class CreateDemoAssets
{
    [MenuItem("Tools/Create Demo Assets")]
    public static void Execute()
    {
        // Create PureDotsRuntimeConfig
        if (!File.Exists("Assets/Settings/PureDotsRuntimeConfig.asset"))
        {
            var config = ScriptableObject.CreateInstance<PureDotsRuntimeConfig>();
            AssetDatabase.CreateAsset(config, "Assets/Settings/PureDotsRuntimeConfig.asset");
        }

        // Create GodgameSpatialProfile
        if (!File.Exists("Assets/Settings/GodgameSpatialProfile.asset"))
        {
            var profile = ScriptableObject.CreateInstance<SpatialPartitionProfile>();
            // These properties are now read-only in PureDOTS and configured internally.
            // We rely on the defaults instead of overriding them here.
            // profile.MaxDirtyOpsForPartialRebuild     = 100;
            // profile.MaxDirtyRatioForPartialRebuild   = 0.1f;
            // profile.MinEntryCountForPartialRebuild   = 10;
            AssetDatabase.CreateAsset(profile, "Assets/Settings/GodgameSpatialProfile.asset");
        }

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
        
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }
}
