using UnityEngine;
using UnityEngine.InputSystem;

namespace Godgame.Controls
{
    /// <summary>
    /// Snapshot of camera input state for a single frame.
    /// </summary>
    public struct CameraInputSnapshot
    {
        public Vector2 PointerPosition;
        public Vector2 PointerDelta;
        public float Scroll;
        public bool HasPointerWorld;
        public Vector3 PointerWorld;
        public Vector2 Move;
        public float Vertical;
        public bool ToggleMode;
        public bool PlaneLockToggle;
        public bool ThrowModifier;
    }
    /// <summary>
    /// Minimal camera input bridge for pan (XZ) and zoom using the HandCamera action map.
    /// </summary>
    public class GodgameCameraInputBehaviour : MonoBehaviour
    {
        [Header("Bindings")]
        [SerializeField] private UnityEngine.Camera targetCamera;
        [SerializeField] private InputActionAsset inputActions;
        [SerializeField] private string actionMapName = "HandCamera";

        [Header("Tuning")]
        [SerializeField] private bool logInputEachFrame = false;
        [SerializeField] private float panSpeed = 0.1f;
        [SerializeField] private float zoomSpeed = 8f;
        [SerializeField] private float minDistance = 4f;
        [SerializeField] private float maxDistance = 120f;
        [SerializeField] private Vector3 defaultFocus = Vector3.zero;
        [SerializeField] private bool enableMovement = false;

        private InputActionAsset runtimeInputAsset;
        private InputAction pointerPositionAction;
        private InputAction pointerDeltaAction;
        private InputAction scrollAction;
        private InputAction middleClickAction;
        private InputAction rightClickAction;
        private InputAction toggleModeAction;
        private InputAction planeLockToggleAction;
        private InputAction throwModifierAction;

        private UnityEngine.Camera activeCamera;
        private Vector3 focusPosition;
        private Quaternion orbitRotation = Quaternion.identity;
        private float currentDistance = 12f;
        private bool initialized;
        private int snapshotLogCounter;
        private const int SnapshotLogInterval = 60;
        private bool loggedMissingCamera;

        private void Awake()
        {
            activeCamera = targetCamera != null ? targetCamera : AcquireCamera();
            InitializeFocusFromCamera();
            SetupInputActions();
        }

        private void Start()
        {
            // Force re-initialization from current camera transform on Start
            // This ensures we respect any manual positioning done in Editor or by other scripts
            if (activeCamera != null)
            {
                orbitRotation = activeCamera.transform.rotation;
                focusPosition = ProjectForwardToGround(activeCamera.transform.position, activeCamera.transform.forward);
                currentDistance = Mathf.Clamp(Vector3.Distance(activeCamera.transform.position, focusPosition), minDistance, maxDistance);
                
                // Don't snap immediately, let the first Update handle it if needed, 
                // or just let it be to avoid jarring jumps if the camera is already correct.
                // UpdateCameraPose(); 
            }
        }

        private void OnEnable()
        {
            runtimeInputAsset?.Enable();
        }

        private void OnDisable()
        {
            runtimeInputAsset?.Disable();
        }

        private void OnDestroy()
        {
            if (runtimeInputAsset != null && runtimeInputAsset != inputActions)
            {
                Destroy(runtimeInputAsset);
            }
        }

        private void Update()
        {
            if (!initialized)
            {
                return;
            }

            var camera = AcquireCamera();
            if (camera == null)
            {
                return;
            }

            var panButtonHeld = (middleClickAction?.IsPressed() ?? false) || (rightClickAction?.IsPressed() ?? false);
            var pointerDelta = panButtonHeld && pointerDeltaAction != null ? pointerDeltaAction.ReadValue<Vector2>() : Vector2.zero;
            var scroll = scrollAction != null ? scrollAction.ReadValue<Vector2>().y : 0f;

            if (logInputEachFrame)
            {
                Debug.Log($"[CameraInput] panDelta={pointerDelta}, scroll={scroll}, focus={focusPosition}, distance={currentDistance}");
            }

            if (enableMovement)
            {
                var moved = ApplyPan(pointerDelta);
                var zoomed = ApplyZoom(scroll);

                if (moved || zoomed)
                {
                    UpdateCameraPose();
                }
            }
        }

        /// <summary>
        /// Attempts to get the current camera input snapshot.
        /// Returns false if input is not available or disabled.
        /// </summary>
        public static bool TryGetSnapshot(out CameraInputSnapshot snapshot)
        {
            snapshot = default;

            var behaviour = Object.FindFirstObjectByType<GodgameCameraInputBehaviour>();
            if (behaviour == null || !behaviour.initialized)
            {
                return false;
            }

            snapshot.PointerPosition = behaviour.pointerPositionAction?.ReadValue<Vector2>() ?? Vector2.zero;
            snapshot.PointerDelta = behaviour.IsPanPressed()
                ? behaviour.pointerDeltaAction?.ReadValue<Vector2>() ?? Vector2.zero
                : Vector2.zero;
            snapshot.Scroll = behaviour.scrollAction?.ReadValue<Vector2>().y ?? 0f;
            snapshot.Move = ReadMoveInput();
            snapshot.Vertical = ReadVerticalInput();

            var camera = behaviour.AcquireCamera();
            if (camera != null)
            {
                var ray = camera.ScreenPointToRay(snapshot.PointerPosition);
                var plane = new Plane(Vector3.up, behaviour.defaultFocus);
                if (plane.Raycast(ray, out var distance))
                {
                    snapshot.HasPointerWorld = true;
                    snapshot.PointerWorld = ray.GetPoint(distance);
                }
                else
                {
                    snapshot.HasPointerWorld = false;
                    snapshot.PointerWorld = Vector3.zero;
                }
            }
            else
            {
                snapshot.HasPointerWorld = false;
                snapshot.PointerWorld = Vector3.zero;
            }

            snapshot.ToggleMode = behaviour.toggleModeAction?.WasPressedThisFrame() ?? false;
            snapshot.PlaneLockToggle = behaviour.planeLockToggleAction?.WasPressedThisFrame() ?? false;
            snapshot.ThrowModifier = behaviour.throwModifierAction?.IsPressed() ?? false;

            behaviour.snapshotLogCounter++;
            if (behaviour.snapshotLogCounter % SnapshotLogInterval == 0)
            {
                Debug.Log($"[CameraInput] move={snapshot.Move}, vertical={snapshot.Vertical}, scroll={snapshot.Scroll}, hasPointer={snapshot.HasPointerWorld}");
            }

            return true;
        }

        private void SetupInputActions()
        {
            runtimeInputAsset = inputActions != null
                ? Instantiate(inputActions)
                : InputActionAsset.FromJson(DefaultHandCameraActionsJson);

            var map = runtimeInputAsset.FindActionMap(actionMapName, throwIfNotFound: false);
            if (map == null)
            {
                Debug.LogWarning($"[CameraInput] Action map '{actionMapName}' not found; disabling input.");
                initialized = false;
                return;
            }

            pointerPositionAction = map.FindAction("PointerPosition", throwIfNotFound: false);
            pointerDeltaAction = map.FindAction("PointerDelta", throwIfNotFound: false);
            scrollAction = map.FindAction("ScrollWheel", throwIfNotFound: false);
            middleClickAction = map.FindAction("MiddleClick", throwIfNotFound: false);
            rightClickAction = map.FindAction("RightClick", throwIfNotFound: false);
            toggleModeAction = map.FindAction("ToggleMode", throwIfNotFound: false);
            planeLockToggleAction = map.FindAction("PlaneLockToggle", throwIfNotFound: false);
            throwModifierAction = map.FindAction("ThrowModifier", throwIfNotFound: false);

            runtimeInputAsset.Enable();
            initialized = true;
        }

        private void InitializeFocusFromCamera()
        {
            var camera = AcquireCamera();
            if (camera == null)
            {
                if (!loggedMissingCamera)
                {
                    Debug.LogWarning("[CameraInput] No camera found; projection will be disabled until one appears.");
                    loggedMissingCamera = true;
                }
                return;
            }

            orbitRotation = camera.transform.rotation;
            focusPosition = ProjectForwardToGround(camera.transform.position, camera.transform.forward);
            currentDistance = Mathf.Clamp(Vector3.Distance(camera.transform.position, focusPosition), minDistance, maxDistance);
            UpdateCameraPose();
        }

        private Vector3 ProjectForwardToGround(Vector3 origin, Vector3 forward)
        {
            var plane = new Plane(Vector3.up, defaultFocus);
            var ray = new Ray(origin, forward);
            if (plane.Raycast(ray, out var distanceAlongRay))
            {
                var hit = ray.GetPoint(distanceAlongRay);
                hit.y = defaultFocus.y;
                return hit;
            }

            var fallback = origin + forward * currentDistance;
            fallback.y = defaultFocus.y;
            return fallback;
        }

        private bool ApplyPan(Vector2 pointerDelta)
        {
            var camera = AcquireCamera();
            if (pointerDelta.sqrMagnitude <= float.Epsilon || camera == null)
            {
                return false;
            }

            var dt = UnityEngine.Time.unscaledDeltaTime > 0f ? UnityEngine.Time.unscaledDeltaTime : UnityEngine.Time.deltaTime;
            var right = camera.transform.right;
            right.y = 0f;
            if (right.sqrMagnitude > 0f)
            {
                right.Normalize();
            }

            var forward = camera.transform.forward;
            forward.y = 0f;
            if (forward.sqrMagnitude > 0f)
            {
                forward.Normalize();
            }

            if (right.sqrMagnitude == 0f && forward.sqrMagnitude == 0f)
            {
                return false;
            }

            var move = (-pointerDelta.x * right + -pointerDelta.y * forward) * panSpeed * dt;
            focusPosition += move;
            focusPosition.y = defaultFocus.y;
            return move.sqrMagnitude > 0f;
        }

        private bool ApplyZoom(float scroll)
        {
            if (Mathf.Abs(scroll) < 0.001f)
            {
                return false;
            }

            var dt = UnityEngine.Time.unscaledDeltaTime > 0f ? UnityEngine.Time.unscaledDeltaTime : UnityEngine.Time.deltaTime;
            currentDistance = Mathf.Clamp(currentDistance - scroll * zoomSpeed * dt, minDistance, maxDistance);
            return true;
        }

        private void UpdateCameraPose()
        {
            var camera = AcquireCamera();
            if (camera == null)
            {
                return;
            }

            var forward = orbitRotation * Vector3.forward;
            if (forward.sqrMagnitude == 0f)
            {
                forward = Vector3.forward;
            }

            forward.Normalize();
            var position = focusPosition - forward * currentDistance;
            camera.transform.SetPositionAndRotation(position, Quaternion.LookRotation(forward, Vector3.up));
        }

        private bool IsPanPressed()
        {
            return (middleClickAction?.IsPressed() ?? false) || (rightClickAction?.IsPressed() ?? false);
        }

        private static Vector2 ReadMoveInput()
        {
            var keyboard = Keyboard.current;
            if (keyboard == null)
            {
                return Vector2.zero;
            }

            Vector2 move = Vector2.zero;
            if (keyboard.wKey.isPressed) move.y += 1f;
            if (keyboard.sKey.isPressed) move.y -= 1f;
            if (keyboard.dKey.isPressed) move.x += 1f;
            if (keyboard.aKey.isPressed) move.x -= 1f;
            move = Vector2.ClampMagnitude(move, 1f);
            return move;
        }

        private static float ReadVerticalInput()
        {
            var keyboard = Keyboard.current;
            if (keyboard == null)
            {
                return 0f;
            }

            float vertical = 0f;
            if (keyboard.qKey.isPressed) vertical -= 1f;
            if (keyboard.eKey.isPressed) vertical += 1f;
            return Mathf.Clamp(vertical, -1f, 1f);
        }

        private UnityEngine.Camera AcquireCamera()
        {
            if (targetCamera != null)
            {
                if (activeCamera != targetCamera)
                {
                    activeCamera = targetCamera;
                }
                return activeCamera;
            }

            if (activeCamera == null)
            {
                activeCamera = UnityEngine.Camera.main;
                if (activeCamera == null)
                {
                    activeCamera = Object.FindFirstObjectByType<UnityEngine.Camera>();
                }
            }

            return activeCamera;
        }

        private const string DefaultHandCameraActionsJson = @"{
    ""name"": ""GodgameHandCamera"",
    ""maps"": [
        {
            ""name"": ""HandCamera"",
            ""id"": ""bccaaf9f-5cde-42ec-8739-706825c60c1f"",
            ""actions"": [
                {
                    ""name"": ""PointerPosition"",
                    ""type"": ""PassThrough"",
                    ""id"": ""fc16d1d3-ef8c-41b5-b3de-acb3fc64d643"",
                    ""expectedControlType"": ""Vector2"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": true
                },
                {
                    ""name"": ""PointerDelta"",
                    ""type"": ""PassThrough"",
                    ""id"": ""03d5db60-fb32-419a-a735-359599305045"",
                    ""expectedControlType"": ""Vector2"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": true
                },
                {
                    ""name"": ""LeftClick"",
                    ""type"": ""Button"",
                    ""id"": ""5dbccdbe-1c27-4967-af69-e0fbe3ff0c96"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""MiddleClick"",
                    ""type"": ""Button"",
                    ""id"": ""60b8e8a5-09bd-47c1-98b4-1b894540779d"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""RightClick"",
                    ""type"": ""Button"",
                    ""id"": ""65d2bbd0-aff9-4261-8cee-dda259288174"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""ScrollWheel"",
                    ""type"": ""PassThrough"",
                    ""id"": ""715ff306-82a6-4e32-a98c-7eeb815c1fa6"",
                    ""expectedControlType"": ""Vector2"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""ToggleMode"",
                    ""type"": ""Button"",
                    ""id"": ""toggle-mode-1234"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""PlaneLockToggle"",
                    ""type"": ""Button"",
                    ""id"": ""plane-lock-toggle-5678"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""ThrowModifier"",
                    ""type"": ""Button"",
                    ""id"": ""throw-modifier-9012"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                }
            ],
            ""bindings"": [
                {
                    ""name"": """",
                    ""id"": ""26bb043b-544b-4aff-baf7-6a1d012ab921"",
                    ""path"": ""<Mouse>/position"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": "";Keyboard&Mouse"",
                    ""action"": ""PointerPosition"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""ae8642c3-243b-4b57-89f7-d99a43eb3ab2"",
                    ""path"": ""<Mouse>/delta"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": "";Keyboard&Mouse"",
                    ""action"": ""PointerDelta"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""5182c174-970e-4f5a-ab1f-b619a407d27d"",
                    ""path"": ""<Mouse>/leftButton"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": "";Keyboard&Mouse"",
                    ""action"": ""LeftClick"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""b763b503-dab1-4d6e-b242-c6ea9a254b00"",
                    ""path"": ""<Mouse>/middleButton"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": "";Keyboard&Mouse"",
                    ""action"": ""MiddleClick"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""aa6ef8d8-084f-4ce0-a4c3-1dc300a98eab"",
                    ""path"": ""<Mouse>/rightButton"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": "";Keyboard&Mouse"",
                    ""action"": ""RightClick"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""7f0d5ae6-0d71-4b5a-b832-85490dde57ab"",
                    ""path"": ""<Mouse>/scroll"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": "";Keyboard&Mouse"",
                    ""action"": ""ScrollWheel"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""toggle-mode-binding"",
                    ""path"": ""<Keyboard>/tab"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": "";Keyboard&Mouse"",
                    ""action"": ""ToggleMode"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""plane-lock-binding"",
                    ""path"": ""<Keyboard>/t"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": "";Keyboard&Mouse"",
                    ""action"": ""PlaneLockToggle"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""throw-modifier-binding"",
                    ""path"": ""<Keyboard>/leftShift"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": "";Keyboard&Mouse"",
                    ""action"": ""ThrowModifier"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""throw-modifier-binding-right"",
                    ""path"": ""<Keyboard>/rightShift"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": "";Keyboard&Mouse"",
                    ""action"": ""ThrowModifier"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                }
            ]
        }
    ],
    ""controlSchemes"": [
        {
            ""name"": ""Keyboard&Mouse"",
            ""bindingGroup"": ""Keyboard&Mouse"",
            ""devices"": [
                {
                    ""devicePath"": ""<Keyboard>"",
                    ""isOptional"": false,
                    ""isOR"": false
                },
                {
                    ""devicePath"": ""<Mouse>"",
                    ""isOptional"": false,
                    ""isOR"": false
                }
            ]
        }
    ]
}";
    }

    /// <summary>
    /// Ensures a camera input rig is spawned even if the scene forgot to place one.
    /// </summary>
    public static class GodgameCameraInputRuntimeBootstrap
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void EnsureCameraInputRig()
        {
            if (Object.FindFirstObjectByType<GodgameCameraInputBehaviour>() != null)
            {
                return;
            }

            var prefab = UnityEngine.Resources.Load<GameObject>("Prefabs/CameraInputRig");
            if (prefab != null)
            {
                Object.Instantiate(prefab);
                Debug.Log("[CameraInput] Spawned CameraInputRig prefab from Resources.");
                return;
            }

            var go = new GameObject("CameraInputRig (Auto)");
            go.AddComponent<GodgameCameraInputBehaviour>();
            Debug.LogWarning("[CameraInput] No CameraInputRig prefab found; created an auto rig instead.");
        }
    }
}
