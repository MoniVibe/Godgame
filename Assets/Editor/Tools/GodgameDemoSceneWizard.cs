using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Godgame.Editor.Tools
{
    internal static class GodgameDemoSceneWizard
    {
        private const string TemplateSceneFolder = "Assets/Scenes/Godgame_VillagerDemo.unity";
        private const string TemplateSceneAsset = TemplateSceneFolder + "/Godgame_VillagerDemo.unity";
        private const string DefaultSaveFolder = "Assets/Scenes";
        private const string BiomeProfilesFolder = "Godgame/Assets/Biomes/Profiles";
        private const string FaunaProfilePath = "Godgame/Assets/Fauna/Profiles/FaunaAmbientProfile_Default.asset";
        private const string BiomeAgentTypeName = "Godgame.Biomes.BiomeTerrainAgent, Assembly-CSharp";
        private const string EnvironmentControllerTypeName = "Game.Environment.EnvironmentTimeController, Assembly-CSharp";
        private const string WeatherRigTypeName = "Godgame.Environment.WeatherRigAuthoring, Assembly-CSharp";
        private const string FaunaAuthoringTypeName = "Godgame.Fauna.FaunaAmbientAuthoring, Assembly-CSharp";

        [MenuItem("Tools/Godgame/Create Demo Scene...")]
        private static void CreateDemoScene()
        {
            var template = AssetDatabase.LoadAssetAtPath<SceneAsset>(TemplateSceneAsset);
            if (template == null)
            {
                EditorUtility.DisplayDialog("Godgame Demo Scene", "Template scene could not be found at " + TemplateSceneAsset + ".", "OK");
                return;
            }

            var savePath = EditorUtility.SaveFilePanelInProject(
                "Create Godgame Demo Scene",
                "Godgame_DemoScene",
                "unity",
                "Choose where to create the new demo scene.",
                DefaultSaveFolder);

            if (string.IsNullOrEmpty(savePath))
            {
                return; // user cancelled
            }

            var destinationFolder = BuildDestinationFolder(savePath);
            var destinationScenePath = BuildDestinationScenePath(savePath);

            if (!EnsureOverwriteAllowed(destinationScenePath))
            {
                return;
            }

            if (!CopyTemplateFolder(destinationFolder, destinationScenePath))
            {
                return;
            }

            AssetDatabase.Refresh();

            var scene = EditorSceneManager.OpenScene(destinationScenePath, OpenSceneMode.Single);
            PopulateDemoSceneContent(scene);
            EditorSceneManager.SaveScene(scene);
            EditorSceneManager.SaveOpenScenes();

            EditorUtility.DisplayDialog("Scene Created", "Demo scene created at:\n" + destinationScenePath + "\n\nIt has been opened for editing.", "OK");
        }

        private static string BuildDestinationFolder(string requestedScenePath)
        {
            var directory = Path.GetDirectoryName(requestedScenePath) ?? "Assets";
            var fileName = Path.GetFileNameWithoutExtension(requestedScenePath);
            return Path.Combine(directory, fileName + ".unity").Replace('\\', '/');
        }

        private static string BuildDestinationScenePath(string requestedScenePath)
        {
            var folder = BuildDestinationFolder(requestedScenePath);
            var fileName = Path.GetFileNameWithoutExtension(requestedScenePath);
            return Path.Combine(folder, fileName + ".unity").Replace('\\', '/');
        }

        private static bool EnsureOverwriteAllowed(string targetScenePath)
        {
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(targetScenePath) == null)
            {
                return true;
            }

            return EditorUtility.DisplayDialog(
                "Overwrite Scene?",
                "A scene already exists at:\n" + targetScenePath + "\n\nOverwrite it?",
                "Overwrite",
                "Cancel");
        }

        private static bool CopyTemplateFolder(string destinationFolder, string destinationScenePath)
        {
            if (!AssetDatabase.IsValidFolder(TemplateSceneFolder))
            {
                EditorUtility.DisplayDialog("Godgame Demo Scene", "Template folder missing at " + TemplateSceneFolder + ".", "OK");
                return false;
            }

            var parentFolder = Path.GetDirectoryName(destinationFolder);
            if (string.IsNullOrEmpty(parentFolder) || !AssetDatabase.IsValidFolder(parentFolder))
            {
                EditorUtility.DisplayDialog("Godgame Demo Scene", "Invalid destination folder " + destinationFolder + ".", "OK");
                return false;
            }

            var projectRoot = Directory.GetCurrentDirectory();
            var sourceFolder = Path.Combine(projectRoot, TemplateSceneFolder);
            var destinationAbsolute = Path.Combine(projectRoot, destinationFolder);

            if (Directory.Exists(destinationAbsolute))
            {
                FileUtil.DeleteFileOrDirectory(destinationAbsolute);
            }

            FileUtil.CopyFileOrDirectory(sourceFolder, destinationAbsolute);
            AssetDatabase.ImportAsset(destinationFolder, ImportAssetOptions.ImportRecursive);

            var originalScenePath = Path.Combine(destinationFolder, Path.GetFileName(TemplateSceneAsset)).Replace('\\', '/');
            if (originalScenePath != destinationScenePath)
            {
                AssetDatabase.MoveAsset(originalScenePath, destinationScenePath);
            }

            return true;
        }

        private const string GroundPrefabPath = "Godgame/Assets/Prefabs/Ground/ProceduralGround_Forest_MoistTemperateFlat.prefab";
        private const string StorehousePrefabPath = "Godgame/Assets/Prefabs/Buildings/Storehouse.prefab";
        private const string HousingPrefabPath = "Godgame/Assets/Prefabs/Buildings/Housing.prefab";
        private const string WorshipPrefabPath = "Godgame/Assets/Prefabs/Buildings/WorshipSite.prefab";
        private const string VillageCenterPrefabPath = "Godgame/Assets/Prefabs/Buildings/VillageCenter.prefab";
        private const string VillagerPrefabPath = "Godgame/Assets/Prefabs/Villagers/Villager.prefab";
        private const string WeatherRigName = "WeatherRig";
        private const string EnvironmentControllerName = "EnvironmentTimeController";
        private const string BiomeAgentName = "BiomeTerrainAgent";
        private const string FaunaVolumeName = "FaunaAmbientVolume";

        private static void PopulateDemoSceneContent(Scene scene)
        {
            if (scene.GetRootGameObjects().Any(go => go.name == "GodgameDemoContent"))
            {
                return; // already populated
            }

            var root = new GameObject("GodgameDemoContent");
            SceneManager.MoveGameObjectToScene(root, scene);

            var ground = SpawnPrefab(GroundPrefabPath, root.transform, Vector3.zero, Vector3.zero);
            SpawnPrefab(VillageCenterPrefabPath, root.transform, new Vector3(0f, 0f, 2f), Vector3.zero);
            var storehouse = SpawnPrefab(StorehousePrefabPath, root.transform, new Vector3(-6f, 0f, 4f), Vector3.zero);
            var housing = SpawnPrefab(HousingPrefabPath, root.transform, new Vector3(6f, 0f, 4f), Vector3.zero);
            var worship = SpawnPrefab(WorshipPrefabPath, root.transform, new Vector3(-2f, 0f, -6f), Vector3.zero);

            var villagerPositions = new List<Vector3>
            {
                new(-1.5f, 0f, 0f),
                new(1.5f, 0f, 0.5f),
                new(0f, 0f, -1.5f),
                new(2.5f, 0f, -0.5f)
            };

            foreach (var pos in villagerPositions)
            {
                SpawnPrefab(VillagerPrefabPath, root.transform, pos, Vector3.zero);
            }

            SetupBiomeAgent(scene, root.transform, ground);
            var sun = EnsureDirectionalLight(scene);
            SetupEnvironmentTimeController(scene, root.transform, sun);
            SetupWeatherRig(scene, root.transform);
            SetupFaunaAmbientVolume(scene, root.transform);
        }

        private static GameObject SpawnPrefab(string assetPath, Transform parent, Vector3 position, Vector3 eulerRotation)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
            if (prefab == null)
            {
                Debug.LogWarning("Missing prefab at " + assetPath);
                return null;
            }

            var instance = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
            if (instance == null)
            {
                return null;
            }

            var targetScene = parent.gameObject.scene;
            if (instance.scene != targetScene)
            {
                SceneManager.MoveGameObjectToScene(instance, targetScene);
            }

            instance.transform.SetParent(parent);
            instance.transform.SetPositionAndRotation(position, Quaternion.Euler(eulerRotation));
            return instance;
        }

        private static void SetupBiomeAgent(Scene scene, Transform parent, GameObject groundInstance)
        {
            var profiles = LoadBiomeProfiles();
            if (profiles.Count == 0)
            {
                return;
            }

            var agentGo = new GameObject(BiomeAgentName);
            SceneManager.MoveGameObjectToScene(agentGo, scene);
            agentGo.transform.SetParent(parent);

            var agent = AddComponent(agentGo, BiomeAgentTypeName);
            if (agent == null)
            {
                return;
            }

            var so = new SerializedObject(agent);
            AssignProfiles(so.FindProperty("profiles"), profiles);

            var groundAnchor = groundInstance != null ? groundInstance.transform : agentGo.transform;
            so.FindProperty("groundAnchor").objectReferenceValue = groundAnchor;
            so.FindProperty("vegetationAnchor").objectReferenceValue = parent;
            so.FindProperty("ambientAnchor").objectReferenceValue = parent;

            var renderer = groundInstance != null ? groundInstance.GetComponentInChildren<Renderer>() : null;
            so.FindProperty("groundRenderer").objectReferenceValue = renderer;

            so.FindProperty("applyOnStart").boolValue = true;
            var biomeEnum = typeof(Enum).Assembly.GetType("PureDOTS.Environment.BiomeType") ?? typeof(int);
            var forestValue = biomeEnum.IsEnum ? (int)Enum.Parse(biomeEnum, "Forest") : 4;
            so.FindProperty("initialBiome").intValue = forestValue;
            so.FindProperty("initialMoisture").floatValue = 55f;
            so.FindProperty("initialTemperature").floatValue = 18f;

            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static IReadOnlyList<ScriptableObject> LoadBiomeProfiles()
        {
            if (!AssetDatabase.IsValidFolder(BiomeProfilesFolder))
            {
                return Array.Empty<ScriptableObject>();
            }

            var guids = AssetDatabase.FindAssets("t:BiomeTerrainProfile", new[] { BiomeProfilesFolder });
            var list = new List<ScriptableObject>(guids.Length);
            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var profile = AssetDatabase.LoadAssetAtPath<ScriptableObject>(path);
                if (profile != null)
                {
                    list.Add(profile);
                }
            }

            return list;
        }

        private static void AssignProfiles(SerializedProperty listProperty, IReadOnlyList<ScriptableObject> profiles)
        {
            if (listProperty == null)
            {
                return;
            }

            listProperty.ClearArray();
            for (int i = 0; i < profiles.Count; i++)
            {
                listProperty.InsertArrayElementAtIndex(i);
                listProperty.GetArrayElementAtIndex(i).objectReferenceValue = profiles[i];
            }
        }

        private static Light EnsureDirectionalLight(Scene scene)
        {
            var lights = UnityEngine.Object.FindObjectsByType<Light>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            var sun = lights.FirstOrDefault(l => l.type == LightType.Directional && l.gameObject.scene == scene);
            if (sun != null)
            {
                RenderSettings.sun = sun;
                return sun;
            }

            var sunGo = new GameObject("Sun Light");
            SceneManager.MoveGameObjectToScene(sunGo, scene);
            sunGo.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
            sun = sunGo.AddComponent<Light>();
            sun.type = LightType.Directional;
            sun.shadows = LightShadows.Soft;
            sun.intensity = 1.1f;
            RenderSettings.sun = sun;
            return sun;
        }

        private static void SetupEnvironmentTimeController(Scene scene, Transform parent, Light sun)
        {
            var envType = GetTypeByName(EnvironmentControllerTypeName);
            var existing = envType != null
                ? scene.GetRootGameObjects().FirstOrDefault(go => go.GetComponent(envType) != null)
                : null;
            if (existing != null)
            {
                return;
            }

            var controllerGo = new GameObject(EnvironmentControllerName);
            SceneManager.MoveGameObjectToScene(controllerGo, scene);
            controllerGo.transform.SetParent(parent);
            var controller = AddComponent(controllerGo, EnvironmentControllerTypeName);
            if (controller == null)
            {
                return;
            }

            var so = new SerializedObject(controller);
            so.FindProperty("sunLight").objectReferenceValue = sun;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void SetupWeatherRig(Scene scene, Transform parent)
        {
            var weatherType = GetTypeByName(WeatherRigTypeName);
            if (weatherType != null && scene.GetRootGameObjects().Any(go => go.GetComponent(weatherType) != null))
            {
                return;
            }

            var weatherGo = new GameObject(WeatherRigName);
            SceneManager.MoveGameObjectToScene(weatherGo, scene);
            weatherGo.transform.SetParent(parent);
            AddComponent(weatherGo, WeatherRigTypeName);
        }

        private static void SetupFaunaAmbientVolume(Scene scene, Transform parent)
        {
            var profile = AssetDatabase.LoadAssetAtPath<ScriptableObject>(FaunaProfilePath);
            if (profile == null)
            {
                return;
            }

            var faunaGo = new GameObject(FaunaVolumeName);
            SceneManager.MoveGameObjectToScene(faunaGo, scene);
            faunaGo.transform.SetParent(parent);
            faunaGo.transform.localPosition = Vector3.zero;

            var authoring = AddComponent(faunaGo, FaunaAuthoringTypeName);
            if (authoring == null)
            {
                return;
            }

            var so = new SerializedObject(authoring);
            so.FindProperty("profile").objectReferenceValue = profile;
            so.FindProperty("radius").floatValue = 25f;
            so.FindProperty("spawnIntervalSeconds").floatValue = 8f;
            so.FindProperty("maxAgents").intValue = 6;
            so.FindProperty("alignToGround").boolValue = true;
            so.FindProperty("spawnHeightOffset").floatValue = 0f;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static Component AddComponent(GameObject go, string typeName)
        {
            var type = GetTypeByName(typeName);
            if (type == null)
            {
                Debug.LogWarning($"Unable to find type '{typeName}'. Ensure the assembly is referenced.", go);
                return null;
            }

            return go.AddComponent(type);
        }

        private static Type GetTypeByName(string typeName)
        {
            return Type.GetType(typeName);
        }
    }
}
