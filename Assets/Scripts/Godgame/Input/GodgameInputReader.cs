using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;
using PureDOTS.Input;

namespace Godgame.Input
{
    /// <summary>
    /// MonoBehaviour bridge that reads New Input System actions and writes to ECS input components.
    /// Place one in your scene to enable input → ECS pipeline.
    /// </summary>
    public class GodgameInputReader : MonoBehaviour
    {
        [Header("Input Actions")]
        [Tooltip("Optional Input Action Asset. If null, uses default bindings.")]
        [SerializeField] private InputActionAsset inputActions;

        [Header("Miracle Settings")]
        [Tooltip("Currently selected miracle slot (0-2)")]
        [Range(0, 2)]
        [SerializeField] private int selectedMiracleSlot = 0;

        [Header("Debug")]
        [SerializeField] private bool logInputEachFrame = false;

        // Input actions
        private InputAction moveAction;
        private InputAction verticalAction;
        private InputAction pointerPositionAction;
        private InputAction pointerDeltaAction;
        private InputAction panDeltaAction;
        private InputAction scrollAction;
        private InputAction leftClickAction;
        private InputAction rightClickAction;
        private InputAction middleClickAction;
        private InputAction focusAction;
        private InputAction yAxisLockToggleAction;
        private InputAction miracleSlot1Action;
        private InputAction miracleSlot2Action;
        private InputAction miracleSlot3Action;
        private InputAction toggleHeatmapAction;
        private InputAction toggleOverlaysAction;
        private InputAction toggleLODAction;
        private InputAction toggleDevMenuAction;
        private InputAction toggleMiracleDesignerAction;
        private InputAction toggleEntityInspectionAction;
        private InputAction togglePresentationMetricsAction;
        private InputAction ctrlModifierAction;
        private InputAction shiftModifierAction;

        [Header("Router (optional)")]
        [SerializeField] private HandCameraInputRouter inputRouter;

        // State tracking for edge detection
        private bool wasLeftClickPressed;
        private bool wasRightClickPressed;

        // ECS world reference
        private World ecsWorld;
        private Entity inputEntity;
        private bool initialized;

        private void Awake()
        {
            SetupInputActions();
        }

        private void OnEnable()
        {
            EnableActions();
        }

        private void OnDisable()
        {
            DisableActions();
        }

        private void Update()
        {
            if (!initialized)
            {
                TryInitializeECS();
            }

            if (!initialized || ecsWorld == null || !ecsWorld.IsCreated)
            {
                return;
            }

            EnsureInputRouter();

            UpdateCameraInput();
            UpdateMiracleInput();
            UpdateSelectionInput();
            UpdateDebugInput();
        }

        private void SetupInputActions()
        {
            if (inputActions != null)
            {
                // Use provided input action asset
                var runtimeAsset = Instantiate(inputActions);
                SetupActionsFromAsset(runtimeAsset);
            }
            else
            {
                // Create default actions programmatically
                SetupDefaultActions();
            }
        }

        private void SetupActionsFromAsset(InputActionAsset asset)
        {
            var cameraMap = asset.FindActionMap("Camera", throwIfNotFound: false);
            var selectionMap = asset.FindActionMap("Selection", throwIfNotFound: false);
            var miracleMap = asset.FindActionMap("Miracle", throwIfNotFound: false);
            var debugMap = asset.FindActionMap("Debug", throwIfNotFound: false);

            // Camera actions
            if (cameraMap != null)
            {
                moveAction = cameraMap.FindAction("Move", throwIfNotFound: false);
                verticalAction = cameraMap.FindAction("Vertical", throwIfNotFound: false);
                pointerPositionAction = cameraMap.FindAction("PointerPosition", throwIfNotFound: false);
                pointerDeltaAction = cameraMap.FindAction("PointerDelta", throwIfNotFound: false);
                panDeltaAction = cameraMap.FindAction("PanDelta", throwIfNotFound: false);
                scrollAction = cameraMap.FindAction("Scroll", throwIfNotFound: false);
                focusAction = cameraMap.FindAction("Focus", throwIfNotFound: false);
                middleClickAction = cameraMap.FindAction("MiddleClick", throwIfNotFound: false);
                yAxisLockToggleAction = cameraMap.FindAction("YAxisLockToggle", throwIfNotFound: false);
            }

            // Selection actions
            if (selectionMap != null)
            {
                leftClickAction = selectionMap.FindAction("Select", throwIfNotFound: false);
                ctrlModifierAction = selectionMap.FindAction("CtrlModifier", throwIfNotFound: false);
                shiftModifierAction = selectionMap.FindAction("ShiftModifier", throwIfNotFound: false);
            }

            // Miracle actions
            if (miracleMap != null)
            {
                rightClickAction = miracleMap.FindAction("CastMiracle", throwIfNotFound: false);
                miracleSlot1Action = miracleMap.FindAction("MiracleSlot1", throwIfNotFound: false);
                miracleSlot2Action = miracleMap.FindAction("MiracleSlot2", throwIfNotFound: false);
                miracleSlot3Action = miracleMap.FindAction("MiracleSlot3", throwIfNotFound: false);
            }

            // Debug actions
            if (debugMap != null)
            {
                toggleHeatmapAction = debugMap.FindAction("ToggleHeatmap", throwIfNotFound: false);
                toggleOverlaysAction = debugMap.FindAction("ToggleOverlays", throwIfNotFound: false);
                toggleLODAction = debugMap.FindAction("ToggleLOD", throwIfNotFound: false);
                toggleDevMenuAction = debugMap.FindAction("ToggleDevMenu", throwIfNotFound: false);
                toggleMiracleDesignerAction = debugMap.FindAction("ToggleMiracleDesigner", throwIfNotFound: false);
                toggleEntityInspectionAction = debugMap.FindAction("ToggleEntityInspection", throwIfNotFound: false);
                togglePresentationMetricsAction = debugMap.FindAction("TogglePresentationMetrics", throwIfNotFound: false);
            }

            asset.Enable();
        }

        private void SetupDefaultActions()
        {
            // Create minimal default actions using keyboard/mouse
            moveAction = new InputAction("Move", InputActionType.Value);
            moveAction.AddCompositeBinding("2DVector")
                .With("Up", "<Keyboard>/w")
                .With("Down", "<Keyboard>/s")
                .With("Left", "<Keyboard>/a")
                .With("Right", "<Keyboard>/d");

            verticalAction = new InputAction("Vertical", InputActionType.Value);
            var verticalComposite = verticalAction.AddCompositeBinding("1DAxis");
            verticalComposite.With("Negative", "<Keyboard>/q");
            verticalComposite.With("Positive", "<Keyboard>/e");

            pointerPositionAction = new InputAction("PointerPosition", InputActionType.Value, "<Mouse>/position");
            pointerDeltaAction = new InputAction("PointerDelta", InputActionType.Value, "<Mouse>/delta");
            panDeltaAction = new InputAction("PanDelta", InputActionType.Value, "<Mouse>/delta");
            scrollAction = new InputAction("Scroll", InputActionType.Value, "<Mouse>/scroll/y");
            leftClickAction = new InputAction("LeftClick", InputActionType.Button, "<Mouse>/leftButton");
            rightClickAction = new InputAction("RightClick", InputActionType.Button, "<Mouse>/rightButton");
            middleClickAction = new InputAction("MiddleClick", InputActionType.Button, "<Mouse>/middleButton");
            focusAction = new InputAction("Focus", InputActionType.Button, "<Keyboard>/f");
            yAxisLockToggleAction = new InputAction("YAxisLockToggle", InputActionType.Button, "<Keyboard>/y");

            miracleSlot1Action = new InputAction("MiracleSlot1", InputActionType.Button, "<Keyboard>/1");
            miracleSlot2Action = new InputAction("MiracleSlot2", InputActionType.Button, "<Keyboard>/2");
            miracleSlot3Action = new InputAction("MiracleSlot3", InputActionType.Button, "<Keyboard>/3");

            toggleHeatmapAction = new InputAction("ToggleHeatmap", InputActionType.Button, "<Keyboard>/h");
            toggleOverlaysAction = new InputAction("ToggleOverlays", InputActionType.Button, "<Keyboard>/o");
            toggleLODAction = new InputAction("ToggleLOD", InputActionType.Button, "<Keyboard>/l");
            toggleDevMenuAction = new InputAction("ToggleDevMenu", InputActionType.Button, "<Keyboard>/f12");
            toggleMiracleDesignerAction = new InputAction("ToggleMiracleDesigner", InputActionType.Button, "<Keyboard>/f4");
            toggleEntityInspectionAction = new InputAction("ToggleEntityInspection", InputActionType.Button, "<Keyboard>/i");
            togglePresentationMetricsAction = new InputAction("TogglePresentationMetrics", InputActionType.Button, "<Keyboard>/o");

            ctrlModifierAction = new InputAction("CtrlModifier", InputActionType.Button, "<Keyboard>/leftCtrl");
            shiftModifierAction = new InputAction("ShiftModifier", InputActionType.Button, "<Keyboard>/leftShift");
        }

        private void EnableActions()
        {
            moveAction?.Enable();
            verticalAction?.Enable();
            pointerPositionAction?.Enable();
            pointerDeltaAction?.Enable();
            panDeltaAction?.Enable();
            scrollAction?.Enable();
            leftClickAction?.Enable();
            rightClickAction?.Enable();
            middleClickAction?.Enable();
            focusAction?.Enable();
            yAxisLockToggleAction?.Enable();
            miracleSlot1Action?.Enable();
            miracleSlot2Action?.Enable();
            miracleSlot3Action?.Enable();
            toggleHeatmapAction?.Enable();
            toggleOverlaysAction?.Enable();
            toggleLODAction?.Enable();
            toggleDevMenuAction?.Enable();
            toggleMiracleDesignerAction?.Enable();
            toggleEntityInspectionAction?.Enable();
            togglePresentationMetricsAction?.Enable();
            ctrlModifierAction?.Enable();
            shiftModifierAction?.Enable();
        }

        private void DisableActions()
        {
            moveAction?.Disable();
            verticalAction?.Disable();
            pointerPositionAction?.Disable();
            pointerDeltaAction?.Disable();
            panDeltaAction?.Disable();
            scrollAction?.Disable();
            leftClickAction?.Disable();
            rightClickAction?.Disable();
            middleClickAction?.Disable();
            focusAction?.Disable();
            yAxisLockToggleAction?.Disable();
            miracleSlot1Action?.Disable();
            miracleSlot2Action?.Disable();
            miracleSlot3Action?.Disable();
            toggleHeatmapAction?.Disable();
            toggleOverlaysAction?.Disable();
            toggleLODAction?.Disable();
            toggleDevMenuAction?.Disable();
            toggleMiracleDesignerAction?.Disable();
            toggleEntityInspectionAction?.Disable();
            togglePresentationMetricsAction?.Disable();
            ctrlModifierAction?.Disable();
            shiftModifierAction?.Disable();
        }

        private void EnsureInputRouter()
        {
            if (inputRouter != null)
            {
                return;
            }

            inputRouter = FindFirstObjectByType<HandCameraInputRouter>();
        }

        private void TryInitializeECS()
        {
            ecsWorld = World.DefaultGameObjectInjectionWorld;
            if (ecsWorld == null || !ecsWorld.IsCreated)
            {
                return;
            }

            var em = ecsWorld.EntityManager;

            // Find or create input singleton entity
            var query = em.CreateEntityQuery(typeof(GodgameInputSingleton));
            if (query.IsEmpty)
            {
                inputEntity = em.CreateEntity();
                em.AddComponent<GodgameInputSingleton>(inputEntity);
                em.AddComponent<CameraInput>(inputEntity);
                em.AddComponent<MiracleInput>(inputEntity);
                em.AddComponent<SelectionInput>(inputEntity);
                em.AddComponent<DebugInput>(inputEntity);
                em.SetName(inputEntity, "GodgameInputSingleton");
            }
            else
            {
                inputEntity = query.GetSingletonEntity();
            }

            initialized = true;
        }

        private void UpdateCameraInput()
        {
            var em = ecsWorld.EntityManager;
            var cameraInput = em.GetComponentData<CameraInput>(inputEntity);

            // Movement
            var move = moveAction?.ReadValue<Vector2>() ?? Vector2.zero;
            cameraInput.Move = new float2(move.x, move.y);

            // Vertical movement
            float vertical = 0f;
            if (verticalAction != null)
            {
                vertical = verticalAction.ReadValue<float>();
            }
            else
            {
                var kb = Keyboard.current;
                if (kb != null)
                {
                    if (kb.eKey.isPressed) vertical += 1f;
                    if (kb.qKey.isPressed) vertical -= 1f;
                }
            }
            cameraInput.Vertical = vertical;

            // Pointer position
            var pointerPos = pointerPositionAction?.ReadValue<Vector2>() ?? Vector2.zero;
            cameraInput.PointerPosition = new float2(pointerPos.x, pointerPos.y);

            // Rotation (MMB drag)
            var isMiddlePressed = middleClickAction?.IsPressed() ?? false;
            var delta = isMiddlePressed ? (pointerDeltaAction?.ReadValue<Vector2>() ?? Vector2.zero) : Vector2.zero;
            cameraInput.Rotate = new float2(delta.x, delta.y);

            // Zoom
            cameraInput.Zoom = scrollAction?.ReadValue<float>() ?? 0f;

            // Pan (LMB drag delta)
            var isLeftPressed = leftClickAction?.IsPressed() ?? false;
            var panDelta = isLeftPressed && panDeltaAction != null ? panDeltaAction.ReadValue<Vector2>() : Vector2.zero;
            cameraInput.Pan = new float2(panDelta.x, panDelta.y);

            // Focus (edge trigger)
            cameraInput.Focus = (focusAction?.WasPressedThisFrame() ?? false) ? (byte)1 : (byte)0;
            cameraInput.ToggleYAxisLock = (yAxisLockToggleAction?.WasPressedThisFrame() ?? false) ? (byte)1 : (byte)0;

            // Pointer world position (prefer router context)
            if (inputRouter != null && inputRouter.CurrentContext.HasWorldHit)
            {
                var worldPoint = inputRouter.CurrentContext.WorldPoint;
                cameraInput.PointerWorldPosition = new float3(worldPoint.x, worldPoint.y, worldPoint.z);
                cameraInput.HasPointerWorld = 1;
            }
            else
            {
                var camera = UnityEngine.Camera.main;
                if (camera != null)
                {
                    var ray = camera.ScreenPointToRay(new Vector3(pointerPos.x, pointerPos.y, 0));
                    var groundPlane = new Plane(Vector3.up, Vector3.zero);
                    if (groundPlane.Raycast(ray, out float distance))
                    {
                        var hitPoint = ray.GetPoint(distance);
                        cameraInput.PointerWorldPosition = new float3(hitPoint.x, hitPoint.y, hitPoint.z);
                        cameraInput.HasPointerWorld = 1;
                    }
                    else
                    {
                        cameraInput.HasPointerWorld = 0;
                    }
                }
                else
                {
                    cameraInput.HasPointerWorld = 0;
                }
            }

            em.SetComponentData(inputEntity, cameraInput);

            if (logInputEachFrame && (math.lengthsq(cameraInput.Move) > 0.001f || math.abs(cameraInput.Zoom) > 0.001f))
            {
                Debug.Log($"[GodgameInput] Camera: Move={cameraInput.Move}, Zoom={cameraInput.Zoom}");
            }
        }

        private void UpdateMiracleInput()
        {
            var em = ecsWorld.EntityManager;
            var miracleInput = em.GetComponentData<MiracleInput>(inputEntity);

            // Miracle slot selection
            if (miracleSlot1Action?.WasPressedThisFrame() ?? false) selectedMiracleSlot = 0;
            if (miracleSlot2Action?.WasPressedThisFrame() ?? false) selectedMiracleSlot = 1;
            if (miracleSlot3Action?.WasPressedThisFrame() ?? false) selectedMiracleSlot = 2;
            miracleInput.SelectedSlot = (byte)selectedMiracleSlot;

            // Cast trigger (RMB click)
            bool isRightPressed = rightClickAction?.IsPressed() ?? false;
            miracleInput.CastTriggered = (!wasRightClickPressed && isRightPressed) ? (byte)1 : (byte)0;
            miracleInput.ThrowCastTriggered = (wasRightClickPressed && !isRightPressed) ? (byte)1 : (byte)0;
            miracleInput.SustainedCastHeld = isRightPressed ? (byte)1 : (byte)0;
            wasRightClickPressed = isRightPressed;

            // Target position (from camera input)
            var cameraInput = em.GetComponentData<CameraInput>(inputEntity);
            miracleInput.TargetPosition = cameraInput.PointerWorldPosition;
            miracleInput.HasValidTarget = cameraInput.HasPointerWorld;

            // Target entity would be set by raycast system (not implemented here)
            // miracleInput.TargetEntity = Entity.Null;

            em.SetComponentData(inputEntity, miracleInput);

            if (logInputEachFrame && miracleInput.CastTriggered == 1)
            {
                Debug.Log($"[GodgameInput] Miracle: Slot={miracleInput.SelectedSlot}, Triggered at {miracleInput.TargetPosition}");
            }
        }

        private void UpdateSelectionInput()
        {
            var em = ecsWorld.EntityManager;
            var selectionInput = em.GetComponentData<SelectionInput>(inputEntity);

            // Screen position
            var pointerPos = pointerPositionAction?.ReadValue<Vector2>() ?? Vector2.zero;
            selectionInput.ScreenPosition = new float2(pointerPos.x, pointerPos.y);

            // Selection trigger (LMB click)
            bool isLeftPressed = leftClickAction?.IsPressed() ?? false;
            selectionInput.SelectTriggered = (!wasLeftClickPressed && isLeftPressed) ? (byte)1 : (byte)0;
            wasLeftClickPressed = isLeftPressed;

            // Modifiers
            selectionInput.VillageSelect = (ctrlModifierAction?.IsPressed() ?? false) ? (byte)1 : (byte)0;
            selectionInput.RegionSelect = (shiftModifierAction?.IsPressed() ?? false) ? (byte)1 : (byte)0;

            // World position (from camera input)
            var cameraInput = em.GetComponentData<CameraInput>(inputEntity);
            selectionInput.WorldPosition = cameraInput.PointerWorldPosition;

            em.SetComponentData(inputEntity, selectionInput);
        }

        private void UpdateDebugInput()
        {
            var em = ecsWorld.EntityManager;
            var debugInput = em.GetComponentData<DebugInput>(inputEntity);

            debugInput.ToggleHeatmap = (toggleHeatmapAction?.WasPressedThisFrame() ?? false) ? (byte)1 : (byte)0;
            debugInput.ToggleOverlays = (toggleOverlaysAction?.WasPressedThisFrame() ?? false) ? (byte)1 : (byte)0;
            debugInput.ToggleLOD = (toggleLODAction?.WasPressedThisFrame() ?? false) ? (byte)1 : (byte)0;
            debugInput.ToggleDevMenu = (toggleDevMenuAction?.WasPressedThisFrame() ?? false) ? (byte)1 : (byte)0;
            debugInput.ToggleMiracleDesigner = (toggleMiracleDesignerAction?.WasPressedThisFrame() ?? false) ? (byte)1 : (byte)0;
            debugInput.ToggleEntityInspection = (toggleEntityInspectionAction?.WasPressedThisFrame() ?? false) ? (byte)1 : (byte)0;
            debugInput.TogglePresentationMetrics = (togglePresentationMetricsAction?.WasPressedThisFrame() ?? false) ? (byte)1 : (byte)0;

            em.SetComponentData(inputEntity, debugInput);
        }

        /// <summary>
        /// Gets the currently selected miracle slot (for UI display).
        /// </summary>
        public int GetSelectedMiracleSlot() => selectedMiracleSlot;

        /// <summary>
        /// Sets the selected miracle slot (from UI).
        /// </summary>
        public void SetSelectedMiracleSlot(int slot)
        {
            selectedMiracleSlot = Mathf.Clamp(slot, 0, 2);
        }
    }
}

