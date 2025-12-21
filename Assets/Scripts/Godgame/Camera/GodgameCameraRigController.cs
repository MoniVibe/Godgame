using Godgame.CameraRig;
using PureDOTS.Runtime.Camera;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Godgame.CameraRig
{
    [DisallowMultipleComponent]
    public class GodgameCameraRigController : MonoBehaviour
    {
        [SerializeField] private UnityEngine.Camera targetCamera;
        [SerializeField] private float orbitSensitivity = 0.25f;   // degrees per pixel
        [SerializeField] private float panSpeed = 12f;
        [SerializeField] private float zoomSpeed = 24f;
        [SerializeField] private float minDistance = 6f;
        [SerializeField] private float maxDistance = 120f;
        [SerializeField] private float minPitch = -80f;
        [SerializeField] private float maxPitch = 80f;

        private EntityManager _entityManager;
        private Entity _rigEntity;

        private void Awake()
        {
            if (targetCamera == null)
                targetCamera = GetComponentInChildren<UnityEngine.Camera>();
            
            if (targetCamera == null)
                targetCamera = UnityEngine.Camera.main;

            if (targetCamera != null && targetCamera.GetComponent<CameraRigApplier>() == null)
            {
                targetCamera.gameObject.AddComponent<CameraRigApplier>();
            }

            var world = World.DefaultGameObjectInjectionWorld;
            if (world == null)
            {
                enabled = false;
                Debug.LogWarning($"{nameof(GodgameCameraRigController)} disabled — no DefaultGameObjectInjectionWorld available.");
                return;
            }

            _entityManager = world.EntityManager;

            if (!TryFindRigEntity(out _rigEntity))
            {
                _rigEntity = _entityManager.CreateEntity(typeof(CameraRigState), typeof(CameraRigCommand), typeof(CameraRigSettings));
                _entityManager.SetComponentData(_rigEntity, new CameraRigState
                {
                    FocusPoint = float3.zero,
                    Distance = math.clamp(15f, minDistance, maxDistance),
                    Yaw = 0f,
                    Pitch = 30f
                });
                _entityManager.SetComponentData(_rigEntity, BuildSettings());
                _entityManager.SetComponentData(_rigEntity, default(CameraRigCommand));
            }
            else
            {
                _entityManager.SetComponentData(_rigEntity, BuildSettings());
            }
        }

        private CameraRigSettings BuildSettings()
        {
            return new CameraRigSettings
            {
                OrbitSensitivity = orbitSensitivity,
                PanSpeed = panSpeed,
                ZoomSpeed = zoomSpeed,
                MinDistance = minDistance,
                MaxDistance = maxDistance,
                MinPitch = minPitch,
                MaxPitch = maxPitch
            };
        }

        private bool TryFindRigEntity(out Entity entity)
        {
            var query = _entityManager.CreateEntityQuery(ComponentType.ReadOnly<CameraRigState>(), ComponentType.ReadOnly<CameraRigCommand>(), ComponentType.ReadOnly<CameraRigSettings>());
            entity = query.IsEmpty ? Entity.Null : query.GetSingletonEntity();
            query.Dispose();
            return entity != Entity.Null;
        }

        private void Update()
        {
            if (_rigEntity == Entity.Null)
                return;

            var orbit = float2.zero;
            var pan = float2.zero;
            float zoom = 0f;

            var mouse = Mouse.current;
            if (mouse != null)
            {
                if (mouse.rightButton.isPressed)
                    orbit = mouse.delta.ReadValue();
                zoom = mouse.scroll.ReadValue().y;
            }

            var kb = Keyboard.current;
            if (kb != null)
            {
                if (kb.wKey.isPressed) pan.y += 1f;
                if (kb.sKey.isPressed) pan.y -= 1f;
                if (kb.dKey.isPressed) pan.x += 1f;
                if (kb.aKey.isPressed) pan.x -= 1f;
                if (kb.leftShiftKey.isPressed) pan *= 2f;
            }

            var command = new CameraRigCommand
            {
                OrbitInput = orbit,
                PanInput = pan,
                ZoomInput = zoom
            };

            _entityManager.SetComponentData(_rigEntity, command);
        }

        private void LateUpdate()
        {
            if (_rigEntity == Entity.Null || targetCamera == null)
                return;

            var rigState = _entityManager.GetComponentData<CameraRigState>(_rigEntity);

            var state = new PureDOTS.Runtime.Camera.CameraRigState
            {
                Focus = new Vector3(rigState.FocusPoint.x, rigState.FocusPoint.y, rigState.FocusPoint.z),
                Pitch = rigState.Pitch,
                Yaw = rigState.Yaw,
                Roll = 0f,
                Distance = rigState.Distance,
                Mode = CameraRigMode.Orbit,
                PerspectiveMode = true,
                FieldOfView = targetCamera.fieldOfView,
                RigType = CameraRigType.Godgame
            };

            CameraRigService.Publish(state);
        }
    }
}
