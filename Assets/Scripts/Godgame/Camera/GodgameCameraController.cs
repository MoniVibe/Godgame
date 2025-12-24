using UnityEngine;
using Unity.Entities;
using Godgame.Input;
using PureDOTS.Runtime.Camera;
using PureDOTS.Input;

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

        [Header("Y-Axis Lock")]
        [SerializeField] private bool _yAxisLocked = true;

        [Header("Input")]
        [SerializeField] private HandCameraInputRouter _inputRouter;

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
        private Plane _panPlane = new Plane(Vector3.up, Vector3.zero);
        private Vector3 _panWorldStart;
        private Vector3 _panPivotStart;

        private void Awake()
        {
            // Assign camera for zoom/drag functionality
            if (_unityCamera == null)
                _unityCamera = UnityEngine.Camera.main;
            
            if (_unityCamera != null && _unityCamera.gameObject.CompareTag("MainCamera"))
            {
                // Already registered, do nothing
            }
            else if (_unityCamera != null)
            {
                // Ensure tag is set if not already
                if (!_unityCamera.gameObject.CompareTag("MainCamera"))
                {
                    if (Godgame.Core.DefaultTagRegistryGuard.TryEnter())
                    {
                        _unityCamera.gameObject.tag = "MainCamera";
                    }
                }
            }

            // Camera resolution and initialization happens in OnEnable
            EnsureInputRouter();
        }

        private void OnEnable()
        {
            // Resolve the camera that actually renders the scene
            _unityCamera = UnityEngine.Camera.main
                           ?? UnityEngine.Object.FindFirstObjectByType<UnityEngine.Camera>();

            if (_unityCamera != null)
            {
                _cameraTransform = _unityCamera.transform;
                if (_unityCamera.GetComponent<CameraRigApplier>() == null)
                {
                    _unityCamera.gameObject.AddComponent<CameraRigApplier>();
                }

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
            if (_inputRouter == null)
            {
                EnsureInputRouter();
            }

            // Guard against BW2StyleCameraController taking over
            // Direct call now that duplicates are unified - no reflection needed
            if (BW2StyleCameraController.HasActiveRig)
            {
                return;
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

            if (cameraInput.ToggleYAxisLock == 1)
            {
                _yAxisLocked = !_yAxisLocked;
            }

            float dt = UnityEngine.Time.deltaTime;

            // -------- MMB drag rotation (yaw / pitch) --------
            Vector2 rotateInput = new Vector2(cameraInput.Rotate.x, cameraInput.Rotate.y);
            if (rotateInput.sqrMagnitude > 1e-4f)
            {
                _yaw += rotateInput.x * _rotationSpeed;
                _yaw = NormalizeDegrees(_yaw); // Keep yaw in [-180, 180] range

                _pitch -= rotateInput.y * _rotationSpeed; // invert Y so dragging up looks down
                _pitch = Mathf.Clamp(_pitch, _minPitch, _maxPitch);
            }

            _rotation = Quaternion.Euler(_pitch, _yaw, 0f);

            // -------- WASD movement respecting lock state --------
            Vector2 moveInput = new Vector2(cameraInput.Move.x, cameraInput.Move.y);
            if (moveInput.sqrMagnitude > 1e-4f)
            {
                moveInput = moveInput.normalized;
            }

            Quaternion yawRotation = Quaternion.Euler(0f, _yaw, 0f);
            Quaternion movementRotation = _yAxisLocked ? yawRotation : _rotation;

            Vector3 camForward = movementRotation * Vector3.forward;
            Vector3 camRight = movementRotation * Vector3.right;
            Vector3 worldMove = (camForward * moveInput.y + camRight * moveInput.x) * (_moveSpeed * dt);

            // -------- Q/E vertical movement respecting lock state --------
            Vector3 upVector = _yAxisLocked ? Vector3.up : movementRotation * Vector3.up;
            Vector3 verticalMove = upVector * (cameraInput.Vertical * _verticalSpeed * dt);

            // Apply translation to internal position
            _position += worldMove + verticalMove;

            // -------- Zoom from ECS input --------
            if (Mathf.Abs(cameraInput.Zoom) > 0.01f)
            {
                HandleZoom(cameraInput, dt);
            }

            // -------- LMB drag pan from ECS input --------
            if (cameraInput.Pan.x != 0f || cameraInput.Pan.y != 0f)
            {
                HandleDragPan(cameraInput, dt);
            }

            // Publish to CameraRigService (for DOTS systems to read)
            PublishCurrentState();
        }

        private void PublishCurrentState()
        {
            var state = new CameraRigState
            {
                Focus = _position,
                Pitch = _pitch,
                Yaw = _yaw,
                Roll = 0f,
                Distance = 0f,
                Mode = CameraRigMode.FreeFly,
                PerspectiveMode = true,
                FieldOfView = _unityCamera != null ? _unityCamera.fieldOfView : 60f,
                RigType = CameraRigType.Godgame
            };

            // Direct call to CameraRigService - no reflection needed now that duplicates are unified
            CameraRigService.Publish(state);
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
                Focus = controller._position,
                Pitch = controller._pitch,
                Yaw = controller._yaw,
                Roll = 0f,
                Distance = 0f,
                Mode = CameraRigMode.FreeFly,
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
        }

        private static float NormalizeDegrees(float degrees)
        {
            while (degrees > 180f) degrees -= 360f;
            while (degrees < -180f) degrees += 360f;
            return degrees;
        }

        private void ApplyCameraPose()
        {
#if UNITY_EDITOR && GODGAME_DEBUG_CAMERA
            // TEMP DEBUG: Log camera position and yaw periodically
            if (UnityEngine.Time.frameCount % 120 == 0) // Every 2 seconds at 60fps
            {
                Debug.Log($"[GodgameCamera] (Publish-only) Pos: {_position:F1}, Yaw: {_yaw:F1}°, Pitch: {_pitch:F1}°");
            }
#endif
        }

        private void HandleZoom(in CameraInput cameraInput, float dt)
        {
            float scroll = cameraInput.Zoom;
            if (Mathf.Abs(scroll) < 0.01f)
                return;

            var context = _inputRouter != null ? _inputRouter.CurrentContext : default;
            if (_inputRouter != null && context.PointerOverUI)
            {
                return;
            }

            // Pointer scroll is a per-frame delta (device units, typically ~120 per notch). Do NOT multiply by dt.
            // Convert to "notches" so zoom feels stable across framerates.
            float scrollNotches = scroll / 120f;
            float zoomAmount = scrollNotches * (_zoomSpeed * 2f);

            Vector3 zoomTarget;
            if (cameraInput.HasPointerWorld != 0)
            {
                zoomTarget = new Vector3(
                    cameraInput.PointerWorldPosition.x,
                    cameraInput.PointerWorldPosition.y,
                    cameraInput.PointerWorldPosition.z);
            }
            else if (_inputRouter != null && context.HasWorldHit)
            {
                zoomTarget = (Vector3)context.WorldPoint;
            }
            else
            {
                // Fallback: original ground plane intersection
                var pointer = cameraInput.PointerPosition;
                Ray ray = _unityCamera.ScreenPointToRay(new Vector3(pointer.x, pointer.y, 0f));
                if (_groundPlane.Raycast(ray, out float enter))
                {
                    zoomTarget = ray.GetPoint(enter);
                }
                else
                {
                    var yawRot = Quaternion.Euler(0f, _yaw, 0f);
                    zoomTarget = _position + (yawRot * Vector3.forward * 10f);
                }
            }

            Vector3 camToHit = zoomTarget - _position;
            float dist = camToHit.magnitude;
            if (dist < _minZoomDistance && zoomAmount > 0f)
                return;

            Vector3 dir = camToHit.normalized;
            _position += dir * zoomAmount;
        }

        private void HandleDragPan(in CameraInput cameraInput, float dt)
        {
            var context = _inputRouter != null ? _inputRouter.CurrentContext : default;

            if (_inputRouter != null && context.PointerOverUI)
            {
                _isDraggingPan = false;
                return;
            }

            if (cameraInput.Pan.x == 0f && cameraInput.Pan.y == 0f)
            {
                _isDraggingPan = false;
                return;
            }

            // First frame of drag: just mark, don't move yet
            if (!_isDraggingPan)
            {
                _isDraggingPan = true;
                if (context.HasWorldHit && context.HitGround)
                {
                    _panWorldStart = (Vector3)context.WorldPoint;
                    _panPivotStart = _position;
                    _panPlane = new Plane(Vector3.up, _panWorldStart);
                }
                return;
            }

            if (context.HasWorldHit)
            {
                Vector3 worldNow = (Vector3)context.WorldPoint;
                if (_panPlane.Raycast(context.PointerRay, out float enter))
                {
                    worldNow = context.PointerRay.GetPoint(enter);
                }
                Vector3 deltaWorld = _panWorldStart - worldNow;
                _position = _panPivotStart + deltaWorld;
            }
            else
            {
                Vector2 delta = new Vector2(cameraInput.Pan.x, cameraInput.Pan.y);
                if (delta.sqrMagnitude < 0.0001f)
                    return;

                Quaternion yawRot = Quaternion.Euler(0f, _yaw, 0f);
                Vector3 right = yawRot * Vector3.right;
                Vector3 forward = yawRot * Vector3.forward;

                float scale = _panSpeedPerPixel;
                Vector3 pan =
                    (-right * delta.x +
                     -forward * delta.y)
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
        }

        private void EnsureInputRouter()
        {
            if (_inputRouter != null)
            {
                return;
            }

            _inputRouter = GetComponent<HandCameraInputRouter>() ??
                           GetComponentInChildren<HandCameraInputRouter>() ??
                           UnityEngine.Object.FindFirstObjectByType<HandCameraInputRouter>();
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
