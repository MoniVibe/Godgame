using System.Collections;
using System.IO;
using UnityEngine;
using Unity.Entities;
using Unity.Collections;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using PureDOTS.Runtime.Core;

namespace Godgame.Demo
{
    public class DemoBootstrap : MonoBehaviour
    {
        public string defaultScenario = "villager_loop_small.json";
        public bool useFancyBindings = false;

        private Entity demoOptionsEntity;
        private float currentSpeed = 1f;
        private bool isPaused;
        private Coroutine stepRoutine;

        private void Awake()
        {
            if (!RuntimeMode.IsHeadless)
            {
                EnsureInputSystemUI();
                EnsureCamera();
            }
        }

        private void Start()
        {
            if (!RuntimeMode.IsHeadless)
            {
                EnsureInputSystemUI();
            }

            var world = World.DefaultGameObjectInjectionWorld;
            if (world == null) return;

            var entityManager = world.EntityManager;

            using var query = entityManager.CreateEntityQuery(ComponentType.ReadWrite<DemoOptions>());
            if (query.CalculateEntityCount() > 0)
            {
                demoOptionsEntity = query.GetSingletonEntity();
            }
            else
            {
                demoOptionsEntity = entityManager.CreateEntity(typeof(DemoOptions));
            }

            var options = new DemoOptions
            {
                ScenarioPath = BuildScenarioPath(defaultScenario),
                BindingsSet = (byte)(useFancyBindings ? 1 : 0),
                Veteran = 0
            };

            entityManager.SetComponentData(demoOptionsEntity, options);
            
            // Ensure DemoSceneTag exists so systems run
            if (!entityManager.HasComponent<DemoSceneTag>(demoOptionsEntity))
            {
                entityManager.AddComponent<DemoSceneTag>(demoOptionsEntity);
            }

            // Ensure GameWorldTag exists so catalog systems run
            if (!entityManager.HasComponent<GameWorldTag>(demoOptionsEntity))
            {
                entityManager.AddComponent<GameWorldTag>(demoOptionsEntity);
            }

            ApplyTimeScale();
        }

        private void Update()
        {
            if (RuntimeMode.IsHeadless) return;

            var keyboard = Keyboard.current;
            if (keyboard == null) return;

            // Hotkeys
            if (WasPressed(keyboard, Key.B))
            {
                ToggleBindings();
            }

            // Time Controls
            if (WasPressed(keyboard, Key.P))
            {
                TogglePause();
            }
            if (WasPressed(keyboard, Key.LeftBracket))
            {
                StepOnce();
            }
            if (WasPressed(keyboard, Key.RightBracket))
            {
                StepOnce();
            }
            if (WasPressed(keyboard, Key.Digit1))
            {
                SetSpeed(0.5f);
            }
            if (WasPressed(keyboard, Key.Digit2))
            {
                SetSpeed(1f);
            }
            if (WasPressed(keyboard, Key.Digit3))
            {
                SetSpeed(2f);
            }
            if (WasPressed(keyboard, Key.R))
            {
                RequestRewind();
            }
            if (WasPressed(keyboard, Key.G))
            {
                SpawnConstructionGhost();
            }
        }

        public void LoadScenario(string scenarioName)
        {
            var world = World.DefaultGameObjectInjectionWorld;
            if (world == null) return;

            var entityManager = world.EntityManager;
            
            if (entityManager.Exists(demoOptionsEntity))
            {
                var options = entityManager.GetComponentData<DemoOptions>(demoOptionsEntity);
                var path = BuildScenarioPath(scenarioName);
                var fullPath = Path.Combine(Application.dataPath, path.ToString());
                if (!File.Exists(fullPath))
                {
                    Debug.LogWarning($"[ScenarioRunner] Scenario file missing: '{fullPath}'. Running world without scenario.");
                    return;
                }

                options.ScenarioPath = path;
                entityManager.SetComponentData(demoOptionsEntity, options);
            }
        }

        public void ToggleBindings()
        {
            var world = World.DefaultGameObjectInjectionWorld;
            if (world == null) return;

            var entityManager = world.EntityManager;
            
            if (entityManager.Exists(demoOptionsEntity))
            {
                var options = entityManager.GetComponentData<DemoOptions>(demoOptionsEntity);
                options.BindingsSet = (byte)(options.BindingsSet == 0 ? 1 : 0);
                entityManager.SetComponentData(demoOptionsEntity, options);
                Debug.Log($"Swapped bindings to: {(options.BindingsSet == 0 ? "Minimal" : "Fancy")}");
            }
        }

        public void TogglePause()
        {
            isPaused = !isPaused;
            ApplyTimeScale();
        }

        public void StepOnce()
        {
            if (stepRoutine != null)
            {
                return;
            }

            if (!isPaused)
            {
                isPaused = true;
                ApplyTimeScale();
            }

            stepRoutine = StartCoroutine(StepCoroutine());
        }

        public void SetSpeed(float multiplier)
        {
            currentSpeed = Mathf.Max(0.1f, multiplier);
            if (!isPaused)
            {
                ApplyTimeScale();
            }
        }

        public void RequestRewind()
        {
            Debug.LogWarning("[DemoBootstrap] Rewind hotkey pressed, but the time spine integration is not wired yet.");
        }

        public void SpawnConstructionGhost()
        {
            Debug.LogWarning("[DemoBootstrap] Construction ghost spawn is not implemented yet.");
        }

        private IEnumerator StepCoroutine()
        {
            UnityEngine.Time.timeScale = currentSpeed;
            yield return null;
            UnityEngine.Time.timeScale = 0f;
            stepRoutine = null;
        }

        private static bool WasPressed(Keyboard keyboard, Key key)
        {
            var control = keyboard[key];
            return control != null && control.wasPressedThisFrame;
        }

        private static void EnsureInputSystemUI()
        {
            var eventSystem = EventSystem.current;
            if (eventSystem == null) return;

            var legacyModule = eventSystem.GetComponent<StandaloneInputModule>();
            if (legacyModule != null)
            {
                legacyModule.enabled = false;
            }

            var inputSystemModule = eventSystem.GetComponent<InputSystemUIInputModule>();
            if (inputSystemModule == null)
            {
                inputSystemModule = eventSystem.gameObject.AddComponent<InputSystemUIInputModule>();
            }

            if (!inputSystemModule.enabled)
            {
                inputSystemModule.enabled = true;
            }

            if (inputSystemModule.actionsAsset == null)
            {
                inputSystemModule.AssignDefaultActions();
            }
        }

        private void ApplyTimeScale()
        {
            UnityEngine.Time.timeScale = isPaused ? 0f : currentSpeed;
        }

        private static FixedString64Bytes BuildScenarioPath(string scenarioName)
        {
            return new FixedString64Bytes("Scenarios/godgame/" + scenarioName);
        }

        private static void EnsureCamera()
        {
#if UNITY_EDITOR
            // Editor-only safety: ensure there is at least one camera so demo scenes are viewable.
            if (UnityEngine.Camera.main != null || FindFirstObjectByType<UnityEngine.Camera>() != null)
            {
                return;
            }

            var cameraGo = new GameObject("DemoCamera (Auto)");
            var camera = cameraGo.AddComponent<UnityEngine.Camera>();
            camera.transform.position = new Vector3(0f, 12f, -12f);
            camera.transform.LookAt(Vector3.zero);
            camera.clearFlags = CameraClearFlags.Skybox;
            Debug.LogWarning("[DemoBootstrap] Spawned a fallback Camera for the demo. Add a camera to the scene to override this.");
#endif
        }
    }
}
