using Godgame.DevTools;
using Godgame.Demo;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Godgame.Editor.Tools
{
    /// <summary>
    /// Editor wizard for setting up Godgame development/testing scenes.
    /// </summary>
    public class GodgameDevSceneSetup : EditorWindow
    {
        private const string DevToolsPrefabPath = "Assets/Prefabs/Systems/GodgameDevToolsRig.prefab";
        private const string VillagerPrefabPath = "Assets/Prefabs/Villagers/Villager.prefab";
        private const string StorehousePrefabPath = "Assets/Prefabs/Buildings/Storehouse.prefab";
        private const string VillageCenterPrefabPath = "Assets/Prefabs/Buildings/VillageCenter.prefab";
        private const string GroundPrefabPath = "Assets/Prefabs/Ground/ProceduralGround_Grassland_MoistTemperateFlat.prefab";
        
        // Configuration
        private string _sceneName = "GodgameDevScene";
        private bool _includeGround = true;
        private bool _includeDevTools = true;
        private bool _includeCamera = true;
        private bool _includeLighting = true;
        private bool _includeBootstrap = true;
        private int _villagerCount = 20;
        
        [MenuItem("Tools/Godgame/Setup Dev Scene...", priority = 10)]
        public static void ShowWindow()
        {
            var window = GetWindow<GodgameDevSceneSetup>("Godgame Dev Scene Setup");
            window.minSize = new Vector2(400, 350);
            window.Show();
        }
        
        [MenuItem("Tools/Godgame/Quick: Add Dev Tools to Scene", priority = 20)]
        public static void QuickAddDevTools()
        {
            AddDevToolsRig();
            Debug.Log("[GodgameDevSceneSetup] Dev Tools Rig added to scene.");
        }
        
        private void OnGUI()
        {
            EditorGUILayout.LabelField("Godgame Development Scene Setup", EditorStyles.boldLabel);
            EditorGUILayout.Space(10);
            
            EditorGUILayout.HelpBox(
                "This wizard creates a development scene for testing Godgame systems.\n" +
                "Includes dev tools (F12 menu), performance monitoring, and entity spawning.",
                MessageType.Info);
            
            EditorGUILayout.Space(10);
            
            // Scene name
            _sceneName = EditorGUILayout.TextField("Scene Name", _sceneName);
            
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Components", EditorStyles.boldLabel);
            
            // Options
            _includeGround = EditorGUILayout.Toggle("Ground Plane", _includeGround);
            _includeDevTools = EditorGUILayout.Toggle("Dev Tools Rig (F12)", _includeDevTools);
            _includeCamera = EditorGUILayout.Toggle("Camera", _includeCamera);
            _includeLighting = EditorGUILayout.Toggle("Directional Light", _includeLighting);
            _includeBootstrap = EditorGUILayout.Toggle("Demo Bootstrap", _includeBootstrap);
            
            if (_includeBootstrap)
            {
                EditorGUI.indentLevel++;
                _villagerCount = EditorGUILayout.IntSlider("Initial Villagers", _villagerCount, 0, 100);
                EditorGUI.indentLevel--;
            }
            
            EditorGUILayout.Space(20);
            
            // Create buttons
            EditorGUILayout.BeginHorizontal();
            
            if (GUILayout.Button("Create New Scene", GUILayout.Height(30)))
            {
                CreateNewDevScene();
            }
            
            if (GUILayout.Button("Add to Current Scene", GUILayout.Height(30)))
            {
                AddToCurrentScene();
            }
            
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.Space(10);
            
            EditorGUILayout.LabelField("Quick Actions", EditorStyles.boldLabel);
            
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Add Dev Tools Only"))
            {
                AddDevToolsRig();
            }
            if (GUILayout.Button("Add Camera + Light"))
            {
                AddCameraAndLighting();
            }
            EditorGUILayout.EndHorizontal();
        }
        
        private void CreateNewDevScene()
        {
            // Create new scene
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            scene.name = _sceneName;
            
            // Add components
            if (_includeCamera) AddCamera();
            if (_includeLighting) AddDirectionalLight();
            if (_includeGround) AddGroundPlane();
            if (_includeDevTools) AddDevToolsRig();
            if (_includeBootstrap) AddDemoBootstrap();
            
            // Save scene
            var path = $"Assets/Scenes/{_sceneName}.unity";
            EditorSceneManager.SaveScene(scene, path);
            
            Debug.Log($"[GodgameDevSceneSetup] Created dev scene at {path}");
            EditorUtility.DisplayDialog("Scene Created", $"Development scene created at:\n{path}", "OK");
        }
        
        private void AddToCurrentScene()
        {
            if (_includeCamera && UnityEngine.Camera.main == null) AddCamera();
            if (_includeLighting) AddDirectionalLight();
            if (_includeGround) AddGroundPlane();
            if (_includeDevTools) AddDevToolsRig();
            if (_includeBootstrap) AddDemoBootstrap();
            
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            Debug.Log("[GodgameDevSceneSetup] Added components to current scene.");
        }
        
        private static void AddCameraAndLighting()
        {
            AddCamera();
            AddDirectionalLight();
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        }
        
        private static void AddCamera()
        {
            if (UnityEngine.Camera.main != null) return;
            
            var cameraGo = new GameObject("Main Camera");
            cameraGo.tag = "MainCamera";
            var camera = cameraGo.AddComponent<UnityEngine.Camera>();
            camera.clearFlags = CameraClearFlags.Skybox;
            camera.fieldOfView = 60f;
            
            cameraGo.transform.position = new Vector3(0f, 20f, -30f);
            cameraGo.transform.rotation = Quaternion.Euler(30f, 0f, 0f);
            
            // Add audio listener
            cameraGo.AddComponent<AudioListener>();
            
            Undo.RegisterCreatedObjectUndo(cameraGo, "Add Camera");
        }
        
        private static void AddDirectionalLight()
        {
            // Check if sun already exists
            var existingLights = Object.FindObjectsByType<Light>(FindObjectsSortMode.None);
            foreach (var light in existingLights)
            {
                if (light.type == LightType.Directional)
                {
                    RenderSettings.sun = light;
                    return;
                }
            }
            
            var lightGo = new GameObject("Sun Light");
            var light2 = lightGo.AddComponent<Light>();
            light2.type = LightType.Directional;
            light2.shadows = LightShadows.Soft;
            light2.intensity = 1.2f;
            light2.color = new Color(1f, 0.95f, 0.85f);
            
            lightGo.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
            
            RenderSettings.sun = light2;
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(0.5f, 0.6f, 0.8f);
            RenderSettings.ambientEquatorColor = new Color(0.6f, 0.5f, 0.4f);
            RenderSettings.ambientGroundColor = new Color(0.3f, 0.25f, 0.2f);
            
            Undo.RegisterCreatedObjectUndo(lightGo, "Add Directional Light");
        }
        
        private static void AddGroundPlane()
        {
            // Try to use ground prefab
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(GroundPrefabPath);
            if (prefab != null)
            {
                var instance = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
                if (instance != null)
                {
                    instance.transform.position = Vector3.zero;
                    Undo.RegisterCreatedObjectUndo(instance, "Add Ground");
                    return;
                }
            }
            
            // Fallback: create simple plane
            var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "Ground";
            ground.transform.localScale = new Vector3(20f, 1f, 20f);
            
            var renderer = ground.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material.color = new Color(0.4f, 0.5f, 0.3f);
            }
            
            Undo.RegisterCreatedObjectUndo(ground, "Add Ground");
        }
        
        private static void AddDevToolsRig()
        {
            // Check if already exists
            var existing = Object.FindFirstObjectByType<GodgameDevToolsRig>();
            if (existing != null)
            {
                Debug.Log("[GodgameDevSceneSetup] Dev Tools Rig already exists in scene.");
                return;
            }
            
            // Try to use prefab
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(DevToolsPrefabPath);
            if (prefab != null)
            {
                var instance = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
                if (instance != null)
                {
                    Undo.RegisterCreatedObjectUndo(instance, "Add Dev Tools Rig");
                    return;
                }
            }
            
            // Fallback: create from script
            var devToolsGo = new GameObject("GodgameDevToolsRig");
            devToolsGo.AddComponent<GodgameDevToolsRig>();
            Undo.RegisterCreatedObjectUndo(devToolsGo, "Add Dev Tools Rig");
        }
        
        private void AddDemoBootstrap()
        {
            // Check if already exists
            var existing = Object.FindFirstObjectByType<GodgameDemoBootstrapAuthoring>();
            if (existing != null)
            {
                Debug.Log("[GodgameDevSceneSetup] Demo Bootstrap already exists in scene.");
                return;
            }
            
            var bootstrapGo = new GameObject("GodgameDemoBootstrap");
            var bootstrap = bootstrapGo.AddComponent<GodgameDemoBootstrapAuthoring>();
            
            // Try to assign prefabs
            var villagerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(VillagerPrefabPath);
            var storehousePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(StorehousePrefabPath);
            
            if (villagerPrefab != null)
            {
                var so = new SerializedObject(bootstrap);
                so.FindProperty("villagerPrefab").objectReferenceValue = villagerPrefab;
                so.FindProperty("storehousePrefab").objectReferenceValue = storehousePrefab;
                so.FindProperty("initialVillagerCount").intValue = _villagerCount;
                so.ApplyModifiedPropertiesWithoutUndo();
            }
            
            Undo.RegisterCreatedObjectUndo(bootstrapGo, "Add Demo Bootstrap");
        }
    }
}



















