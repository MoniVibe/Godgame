#if UNITY_EDITOR
using System.IO;
using Godgame.Authoring;
using PureDOTS.Authoring;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Godgame.Editor
{
    /// <summary>
    /// Creates a minimal SubScene with registry + spatial authoring so bridge tests have authored data.
    /// </summary>
    public static class GodgameRegistrySubSceneWizard
    {
        private const string ScenesFolder = "Assets/Scenes";
        private const string SettingsFolder = "Assets/Settings";
        private const string ScenePath = ScenesFolder + "/GodgameRegistrySubScene.unity";

        [MenuItem("Tools/Godgame/Create Registry SubScene...", priority = 10)]
        public static void CreateRegistrySubScene()
        {
            EnsureFolder(ScenesFolder);
            EnsureFolder(SettingsFolder);

            var resourceTypes = LoadOrCreate<ResourceTypeCatalog>(SettingsFolder, "PureDotsResourceTypes", configure: catalog =>
            {
                var serialized = new SerializedObject(catalog);
                var schemaProp = serialized.FindProperty("_schemaVersion");
                if (schemaProp != null)
                {
                    schemaProp.intValue = ResourceTypeCatalog.LatestSchemaVersion;
                }
                serialized.ApplyModifiedPropertiesWithoutUndo();
            });

            var recipeCatalog = LoadOrCreate<ResourceRecipeCatalog>(SettingsFolder, "ResourceRecipeCatalog", configure: catalog =>
            {
                var serialized = new SerializedObject(catalog);
                var families = serialized.FindProperty("_families");
                var recipes = serialized.FindProperty("_recipes");
                if (families != null)
                {
                    families.ClearArray();
                }
                if (recipes != null)
                {
                    recipes.ClearArray();
                }
                serialized.ApplyModifiedPropertiesWithoutUndo();
            });

            var runtimeConfig = LoadOrCreate<PureDotsRuntimeConfig>(SettingsFolder, "PureDotsRuntimeConfig", configure: config =>
            {
                var serialized = new SerializedObject(config);
                var schema = serialized.FindProperty("_schemaVersion");
                if (schema != null) schema.intValue = PureDotsRuntimeConfig.LatestSchemaVersion;
                var typesProp = serialized.FindProperty("_resourceTypes");
                if (typesProp != null) typesProp.objectReferenceValue = resourceTypes;
                var recipesProp = serialized.FindProperty("_recipeCatalog");
                if (recipesProp != null) recipesProp.objectReferenceValue = recipeCatalog;
                serialized.ApplyModifiedPropertiesWithoutUndo();
            });

            var spatialProfile = LoadOrCreate<SpatialPartitionProfile>(SettingsFolder, "GodgameSpatialProfile", configure: profile =>
            {
                var serialized = new SerializedObject(profile);
                var schema = serialized.FindProperty("_schemaVersion");
                if (schema != null) schema.intValue = SpatialPartitionProfile.LatestSchemaVersion;
                var center = serialized.FindProperty("_center");
                if (center != null) center.vector3Value = Vector3.zero;
                var extent = serialized.FindProperty("_extent");
                if (extent != null) extent.vector3Value = new Vector3(64f, 16f, 64f);
                var cellSize = serialized.FindProperty("_cellSize");
                if (cellSize != null) cellSize.floatValue = 8f;
                var minCellSize = serialized.FindProperty("_minCellSize");
                if (minCellSize != null) minCellSize.floatValue = 1f;
                var overrideCells = serialized.FindProperty("_overrideCellCounts");
                if (overrideCells != null) overrideCells.boolValue = false;
                var lockY = serialized.FindProperty("_lockYAxisToOne");
                if (lockY != null) lockY.boolValue = true;
                serialized.ApplyModifiedPropertiesWithoutUndo();
            });

            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            scene.name = "GodgameRegistrySubScene";

            var root = new GameObject("Godgame DOTS Registry");
            var configAuthoring = root.AddComponent<PureDotsConfigAuthoring>();
            configAuthoring.config = runtimeConfig;

            root.AddComponent<GodgameSampleRegistryAuthoring>();

            var spatialAuthoring = root.AddComponent<SpatialPartitionAuthoring>();
            spatialAuthoring.profile = spatialProfile;

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, ScenePath);
            AssetDatabase.SaveAssets();

            Debug.Log($"[GodgameRegistrySubSceneWizard] Created/updated SubScene with registry + spatial authoring at '{ScenePath}'.", root);
        }

        private static void EnsureFolder(string folder)
        {
            if (AssetDatabase.IsValidFolder(folder))
            {
                return;
            }

            var parent = Path.GetDirectoryName(folder)?.Replace("\\", "/");
            var leaf = Path.GetFileName(folder);
            if (!string.IsNullOrEmpty(parent) && !AssetDatabase.IsValidFolder(parent))
            {
                EnsureFolder(parent);
            }

            AssetDatabase.CreateFolder(parent ?? "Assets", leaf);
        }

        private static T LoadOrCreate<T>(string folder, string fileName, System.Action<T> configure = null) where T : ScriptableObject
        {
            var assets = AssetDatabase.FindAssets($"t:{typeof(T).Name}");
            foreach (var guid in assets)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var asset = AssetDatabase.LoadAssetAtPath<T>(path);
                if (asset != null)
                {
                    return asset;
                }
            }

            EnsureFolder(folder);
            var assetPath = Path.Combine(folder, fileName + ".asset").Replace("\\", "/");
            var instance = ScriptableObject.CreateInstance<T>();
            configure?.Invoke(instance);
            AssetDatabase.CreateAsset(instance, assetPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            return instance;
        }
    }
}
#endif
