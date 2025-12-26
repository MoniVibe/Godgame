using Godgame.Temporal;
using UnityEngine;
using UnityEngine.UI;
using Unity.Entities;

namespace Godgame.Presentation
{
    /// <summary>
    /// Hooks UI buttons to the ECS time control commands.
    /// </summary>
    public sealed class GodgameTimeControls : MonoBehaviour
    {
        [Header("Buttons")]
        [SerializeField] private Button _pauseButton;
        [SerializeField] private Button _rewindButton;
        [SerializeField] private Button _speed1Button;
        [SerializeField] private Button _speed2Button;
        [SerializeField] private Button _speed3Button;

        [Header("Time Settings")]
        [SerializeField] private float _speed1 = 1f;
        [SerializeField] private float _speed2 = 2f;
        [SerializeField] private float _speed3 = 4f;
        [SerializeField] private uint _rewindTicks = 120;

        private void OnEnable()
        {
            ResolveButtonRefs();
            BindButtons(true);
        }

        private void OnDisable()
        {
            BindButtons(false);
        }

        private void ResolveButtonRefs()
        {
            _pauseButton ??= FindButton("PauseButton");
            _rewindButton ??= FindButton("RewindButton");
            _speed1Button ??= FindButton("Speed1Button");
            _speed2Button ??= FindButton("Speed2Button");
            _speed3Button ??= FindButton("Speed3Button");
        }

        private static Button FindButton(string name)
        {
            var go = GameObject.Find(name);
            return go != null ? go.GetComponent<Button>() : null;
        }

        private void BindButtons(bool bind)
        {
            if (_pauseButton != null)
            {
                if (bind)
                    _pauseButton.onClick.AddListener(OnPauseClicked);
                else
                    _pauseButton.onClick.RemoveListener(OnPauseClicked);
            }

            if (_rewindButton != null)
            {
                if (bind)
                    _rewindButton.onClick.AddListener(OnRewindClicked);
                else
                    _rewindButton.onClick.RemoveListener(OnRewindClicked);
            }

            if (_speed1Button != null)
            {
                if (bind)
                    _speed1Button.onClick.AddListener(OnSpeed1Clicked);
                else
                    _speed1Button.onClick.RemoveListener(OnSpeed1Clicked);
            }

            if (_speed2Button != null)
            {
                if (bind)
                    _speed2Button.onClick.AddListener(OnSpeed2Clicked);
                else
                    _speed2Button.onClick.RemoveListener(OnSpeed2Clicked);
            }

            if (_speed3Button != null)
            {
                if (bind)
                    _speed3Button.onClick.AddListener(OnSpeed3Clicked);
                else
                    _speed3Button.onClick.RemoveListener(OnSpeed3Clicked);
            }
        }

        private void OnPauseClicked()
        {
            if (!TryGetWorld(out var world))
            {
                return;
            }

            GodgameTimeAPI.TogglePause(world);
        }

        private void OnRewindClicked()
        {
            if (!TryGetWorld(out var world))
            {
                return;
            }

            var ticksBack = _rewindTicks > 0 ? _rewindTicks : 1u;
            GodgameTimeAPI.RequestGlobalRewind(world, ticksBack);
        }

        private void OnSpeed1Clicked() => ApplySpeed(_speed1);
        private void OnSpeed2Clicked() => ApplySpeed(_speed2);
        private void OnSpeed3Clicked() => ApplySpeed(_speed3);

        private void ApplySpeed(float speed)
        {
            if (!TryGetWorld(out var world))
            {
                return;
            }

            var entityManager = world.EntityManager;
            using var timeQuery = entityManager.CreateEntityQuery(ComponentType.ReadOnly<PureDOTS.Runtime.Components.TimeState>());
            if (!timeQuery.IsEmptyIgnoreFilter)
            {
                var timeState = timeQuery.GetSingleton<PureDOTS.Runtime.Components.TimeState>();
                if (timeState.IsPaused)
                {
                    GodgameTimeAPI.TogglePause(world);
                }
            }

            GodgameTimeAPI.SetGlobalTimeSpeed(world, speed);
        }

        private static bool TryGetWorld(out World world)
        {
            world = World.DefaultGameObjectInjectionWorld;
            return world != null && world.IsCreated;
        }
    }
}
