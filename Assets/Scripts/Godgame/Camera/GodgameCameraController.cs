using System;
using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Mathematics;
using Unity.Entities;
using Moni.Godgame.CameraSystems;
using Godgame.Input;
using PureDOTS.Runtime.Camera;

namespace Godgame
{
    /// <summary>
    /// Godgame-specific camera controller supporting RTS/Free-fly and orbital modes.
    /// </summary>
    public class GodgameCameraController : MonoBehaviour
    {
        [Header("Movement")]
        [SerializeField] private float _moveSpeed = 30f;
        [SerializeField] private float _verticalSpeed = 20f;
        [SerializeField] private float _panSpeedPerPixel = 0.1f;

        [Header("Rotation")]
        [SerializeField] private float _rotationSpeed = 0.2f; // degrees per pixel
        [SerializeField] private float _minPitch = -80f;
        [SerializeField] private float _maxPitch = 80f;

        [Header("Default Start Pose")]
        [SerializeField] private bool _resetToDefaultOnStart = true;
        [SerializeField] private Vector3 _defaultFocus = Vector3.zero;
        [SerializeField] private float _defaultDistance = 30f;
        [SerializeField] private float _defaultYawDegrees = 45f;
        [SerializeField] private float _defaultPitchDegrees = 45f;

        [Header("Zoom")]
        [SerializeField] private float _zoomSpeed = 40f;
        [SerializeField] private float _minZoomDistance = 5f;

        // Internal state
        private float _yaw;
        private float _pitch;
        private Vector3 _position;
        private Quaternion _rotation;
        private UnityEngine.Camera _unityCamera;
        private Transform _cameraTransform;
        private bool _loggedMissingCamera;

        // Zoom and drag state
        private Plane _groundPlane = new Plane(Vector3.up, Vector3.zero);
        private bool _isDragging;
        private Vector3 _dragStartCameraPos;
        private Vector3 _dragStartHit;
        private bool _isDraggingPan;

        private void Awake()
        {
            // Assign camera for zoom/drag functionality
            if (_unityCamera == null)
                _unityCamera = UnityEngine.Camera.main;

            // Camera resolution and initialization happens in OnEnable
        }

        private void OnEnable()
        {
            // Resolve the camera that actually renders the scene
            _unityCamera = UnityEngine.Camera.main
                           ?? UnityEngine.Object.FindFirstObjectByType<UnityEngine.Camera>();

            if (_unityCamera != null)
            {
                _cameraTransform = _unityCamera.transform;

                if (_resetToDefaultOnStart)
                {
                    ResetToDefaultPose();
                }
                else
                {
                    // Initialize from current camera transform
                    _position = _cameraTransform.position;
                    _rotation = _cameraTransform.rotation;
                    var euler = _rotation.eulerAngles;
                    _yaw = euler.y;
                    _pitch = euler.x;
                }

                if (_loggedMissingCamera)
                {
                    Debug.Log("[GodgameCamera] Render camera resolved.");
                    _loggedMissingCamera = false;
                }
            }
            else
            {
                Debug.LogWarning("[GodgameCamera] No Unity Camera found; controller will run headless.");
                _loggedMissingCamera = true;
            }
        }

        private void Update()
        {
            // Guard against BW2StyleCameraController taking over
            // Use Type.GetType with assembly-qualified name to resolve ambiguity
            var bw2ControllerType = Type.GetType("PureDOTS.Runtime.Camera.BW2StyleCameraController, PureDOTS.Runtime, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null");
            if (bw2ControllerType != null)
            {
                var hasActiveRigProperty = bw2ControllerType.GetProperty("HasActiveRig", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
                if (hasActiveRigProperty != null && (bool)hasActiveRigProperty.GetValue(null))
                {
                    return;
                }
            }

            // Read input from ECS
            var world = World.DefaultGameObjectInjectionWorld;
            if (world == null || !world.IsCreated)
            {
                return;
            }

            var em = world.EntityManager;
            var query = em.CreateEntityQuery(typeof(CameraInput));
            if (query.IsEmpty)
            {
                return;
            }

            var cameraInput = query.GetSingleton<CameraInput>();

            // Handle focus key (F) to reset camera to default pose
            if (cameraInput.Focus == 1)
            {
                ResetToDefaultPose();
                return; // Skip normal movement this frame
            }

            // Handle zoom and drag (still using mouse directly for these)
            var mouse = Mouse.current;
            if (mouse != null)
            {
                HandleZoom(mouse, UnityEngine.Time.deltaTime);
                HandleDragPan(mouse, UnityEngine.Time.deltaTime);
            }

            float dt = UnityEngine.Time.deltaTime;

            // -------- WASD ground-plane movement from ECS input --------
            Vector2 moveInput = new Vector2(cameraInput.Move.x, cameraInput.Move.y);

            // Normalize input so diagonal movement isn't faster
            if (moveInput.sqrMagnitude > 1e-4f)
            {
                moveInput = moveInput.normalized;
            }

            // Build yaw-only rotation (ignore pitch for ground movement)
            Quaternion yawRotation = Quaternion.Euler(0f, _yaw, 0f);

            // Camera-relative directions on XZ plane
            Vector3 camForward = yawRotation * Vector3.forward; // "forward" in world space
            Vector3 camRight = yawRotation * Vector3.right;     // "right" in world space

            // Compose movement
            Vector3 worldMove = camForward * moveInput.y +     // W/S
                               camRight * moveInput.x;         // A/D

            // Apply speed & delta time
            worldMove *= _moveSpeed * dt;

            // -------- Q/E vertical movement (still using keyboard directly for now) --------
            // TODO: Add vertical movement to CameraInput component
            float vertical = 0f;
            var kb = Keyboard.current;
            if (kb != null)
            {
                if (kb.eKey.isPressed) vertical += 1f;
                if (kb.qKey.isPressed) vertical -= 1f;
            }

            Vector3 verticalMove = Vector3.up * (vertical * _verticalSpeed * dt);

            // Apply translation to internal position
            _position += worldMove + verticalMove;

            // -------- MMB drag rotation (yaw / pitch) --------
            if (mouse.middleButton.isPressed)
            {
                Vector2 delta = mouse.delta.ReadValue();
                _yaw += delta.x * _rotationSpeed;
                _yaw = NormalizeDegrees(_yaw); // Keep yaw in [-180, 180] range

                _pitch -= delta.y * _rotationSpeed; // invert Y so dragging up looks down
                _pitch = Mathf.Clamp(_pitch, _minPitch, _maxPitch);
            }

            _rotation = Quaternion.Euler(_pitch, _yaw, 0f);

            // Apply the pose to the actual camera
            ApplyCameraPose();

            // Publish to CameraRigService (for DOTS systems to read)
            PublishCurrentState();
        }

        private void PublishCurrentState()
        {
            var state = new CameraRigState
            {
                Position = _position,
                Rotation = _rotation,
                Pitch = math.radians(_pitch),
                Yaw = math.radians(_yaw),
                Distance = 10f, // Default distance
                PerspectiveMode = true,
                FieldOfView = _unityCamera != null ? _unityCamera.fieldOfView : 60f,
                RigType = CameraRigType.Godgame
            };

            // Note: CameraRigService exists in both PureDOTS.Camera and PureDOTS.Runtime
            // Using reflection to resolve ambiguity - this is a workaround for duplicate type definitions
            var cameraRigServiceType = Type.GetType("PureDOTS.Runtime.Camera.CameraRigService, PureDOTS.Runtime, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null");
            if (cameraRigServiceType != null)
            {
                var publishMethod = cameraRigServiceType.GetMethod("Publish", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
                if (publishMethod != null)
                {
                    publishMethod.Invoke(null, new object[] { state });
                }
            }
        }

        /// <summary>
        /// Gets the current camera state. Used by DOTS systems to mirror Mono state.
        /// </summary>
        public static bool TryGetCurrentState(out CameraRigState state)
        {
            var controller = UnityEngine.Object.FindFirstObjectByType<GodgameCameraController>();
            if (controller == null)
            {
                state = default;
                return false;
            }

            state = new CameraRigState
            {
                Position = controller._position,
                Rotation = controller._rotation,
                Pitch = math.radians(controller._pitch),
                Yaw = math.radians(controller._yaw),
                Distance = 10f, // Default distance
                PerspectiveMode = true,
                FieldOfView = controller._unityCamera != null ? controller._unityCamera.fieldOfView : 60f,
                RigType = CameraRigType.Godgame
            };

            return true;
        }

        private void ResetToDefaultPose()
        {
            _yaw = _defaultYawDegrees;
            _pitch = _defaultPitchDegrees;

            _rotation = Quaternion.Euler(_pitch, _yaw, 0f);
            _position = _defaultFocus - _rotation * (Vector3.forward * _defaultDistance);

            if (_cameraTransform != null)
            {
                _cameraTransform.SetPositionAndRotation(_position, _rotation);
            }
        }

        private static float NormalizeDegrees(float degrees)
        {
            while (degrees > 180f) degrees -= 360f;
            while (degrees < -180f) degrees += 360f;
            return degrees;
        }

        private void ApplyCameraPose()
        {
            if (_cameraTransform != null)
            {
                _cameraTransform.SetPositionAndRotation(_position, _rotation);

#if UNITY_EDITOR && GODGAME_DEBUG_CAMERA
                // TEMP DEBUG: Log camera position and yaw periodically
                if (UnityEngine.Time.frameCount % 120 == 0) // Every 2 seconds at 60fps
                {
                    Debug.Log($"[GodgameCamera] Pos: {_position:F1}, Yaw: {_yaw:F1}°, Pitch: {_pitch:F1}°");
                }
#endif
            }
            else
            {
                Debug.LogWarning("[GodgameCamera] No _cameraTransform set; cannot apply pose.");
            }
        }

        private void HandleZoom(Mouse mouse, float dt)
        {
            float scroll = mouse.scroll.ReadValue().y;
            if (Mathf.Abs(scroll) < 0.01f)
                return;

            // Normalize scroll and scale
            float zoomAmount = scroll * _zoomSpeed * dt;

            // Ray from camera through cursor
            Ray ray = _unityCamera.ScreenPointToRay(mouse.position.ReadValue());

            if (_groundPlane.Raycast(ray, out float enter))
            {
                Vector3 hit = ray.GetPoint(enter);
                Vector3 camToHit = hit - _position;

                // If we're very close, don't zoom in further
                float dist = camToHit.magnitude;
                if (dist < _minZoomDistance && zoomAmount > 0f)
                    return;

                Vector3 dir = camToHit.normalized;
                _position += dir * zoomAmount;

#if UNITY_EDITOR && GODGAME_DEBUG_CAMERA
                // Debug logging for zoom
                if (UnityEngine.Time.frameCount % 30 == 0) // Log every half second
                {
                    Debug.Log($"[GodgameCamera] Zoom: {zoomAmount:F2}, Hit: {hit:F1}, NewPos: {_position:F1}");
                }
#endif
            }
            else
            {
                // Fallback: simple zoom along camera forward on ground plane
                Quaternion yawRot = Quaternion.Euler(0f, _yaw, 0f);
                Vector3 forward = yawRot * Vector3.forward;
                _position += forward * zoomAmount;
            }
        }

        private void HandleDragPan(Mouse mouse, float dt)
        {
            // If button not held, reset state
            if (!mouse.leftButton.isPressed)
            {
                _isDraggingPan = false;
                return;
            }

            // First frame of drag: just mark, don't move yet
            if (!_isDraggingPan)
            {
                _isDraggingPan = true;
                // Optional: swallow the first delta to avoid jump
                return;
            }

            Vector2 delta = mouse.delta.ReadValue();
            if (delta.sqrMagnitude < 0.0001f)
                return;

            // We pan opposite to drag (drag right -> world goes right, camera moves left)
            // Only use yaw so we stay on the ground plane
            Quaternion yawRot = Quaternion.Euler(0f, _yaw, 0f);
            Vector3 right = yawRot * Vector3.right;
            Vector3 forward = yawRot * Vector3.forward;

            // Convert pixels to world movement
            float scale = _panSpeedPerPixel; // no dt here: delta is per-frame already
            Vector3 pan =
                (-right * delta.x +   // dragging mouse right moves camera left
                 -forward * delta.y)  // dragging mouse up moves camera forward/back
                * scale;

            _position += pan;

#if UNITY_EDITOR && GODGAME_DEBUG_CAMERA
            // Debug logging for pan (less frequent than drag was)
            if (UnityEngine.Time.frameCount % 30 == 0)
            {
                Debug.Log($"[GodgameCamera] Pan: Delta: {delta}, Pan: {pan:F2}, NewPos: {_position:F1}");
            }
#endif
        }

        private bool TryGetGroundHit(Vector2 screenPos, out Vector3 hitPoint)
        {
            Ray ray = _unityCamera.ScreenPointToRay(screenPos);
            if (_groundPlane.Raycast(ray, out float enter))
            {
                hitPoint = ray.GetPoint(enter);
                return true;
            }

            hitPoint = default;
            return false;
        }

        private void ResolveCamera()
        {
            if (_unityCamera != null && _cameraTransform != null)
            {
                return;
            }

            _unityCamera = UnityEngine.Camera.main ?? UnityEngine.Object.FindFirstObjectByType<UnityEngine.Camera>();
            if (_unityCamera != null)
            {
                _cameraTransform = _unityCamera.transform;
                if (_loggedMissingCamera)
                {
                    Debug.Log("[GodgameCamera] Render camera resolved.");
                    _loggedMissingCamera = false;
                }
            }
            else if (!_loggedMissingCamera)
            {
                Debug.LogWarning("[GodgameCamera] No Camera found; controller will run headless until one appears.");
                _loggedMissingCamera = true;
            }
        }
    }
}
