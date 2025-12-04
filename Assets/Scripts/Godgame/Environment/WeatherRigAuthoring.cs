using System;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.VFX;

namespace Godgame.Environment
{
    /// <summary>
    /// Bridges DOTS weather state to COZY Weather, VFX Graph emitters, and ambient audio loops.
    /// Operates purely in the scene so artists can wire references without touching ECS internals.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class WeatherRigAuthoring : MonoBehaviour
    {
        public static WeatherRigAuthoring Instance { get; private set; }

        [Header("COZY Weather")]
        [SerializeField] private MonoBehaviour cozyWeatherComponent;
        [SerializeField] private float defaultTransitionSeconds = 8f;
        [SerializeField] private ScriptableObject clearProfile;
        [SerializeField] private ScriptableObject rainProfile;
        [SerializeField] private ScriptableObject snowProfile;
        [SerializeField] private ScriptableObject fogProfile;
        [SerializeField] private ScriptableObject stormProfile;

        [Header("VFX Bindings")]
        [SerializeField] private WeatherFxBinding[] fxBindings = Array.Empty<WeatherFxBinding>();

        [Header("Ambient Audio")] 
        [SerializeField] private WeatherAudioBinding[] audioBindings = Array.Empty<WeatherAudioBinding>();

        [Header("Special FX (Miracles / Events)")]
        [SerializeField] private WeatherSpecialFxBinding[] specialFxBindings = Array.Empty<WeatherSpecialFxBinding>();

        private Type _weatherProfileType;
        private System.Reflection.MethodInfo _setWeatherTimed;
        private System.Reflection.MethodInfo _setWeatherInstant;
        private WeatherType _lastAppliedType = WeatherType.Clear;
        private float _lastAppliedIntensity;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Debug.LogWarning("Only one WeatherRigAuthoring can exist at a time. Destroying duplicate.", Instance);
                Destroy(this);
                return;
            }

            Instance = this;
            CacheCozyMethods();
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        private void CacheCozyMethods()
        {
            if (cozyWeatherComponent == null)
            {
                return;
            }

            var type = cozyWeatherComponent.GetType();
            _weatherProfileType = type.Assembly.GetType("DistantLands.Cozy.Data.WeatherProfile") ??
                                   type.Assembly.GetType("DistantLands.Cozy.WeatherProfile");

            if (_weatherProfileType == null)
            {
                Debug.LogWarning("Unable to find COZY WeatherProfile type. Assign the COZY package to this project.", this);
                return;
            }

            _setWeatherTimed = type.GetMethod("SetWeather", new[] { _weatherProfileType, typeof(float) });
            _setWeatherInstant = type.GetMethod("SetWeather", new[] { _weatherProfileType });
        }

        public void ApplySnapshot(in WeatherPresentationSnapshot snapshot)
        {
            ApplyCozyWeather(snapshot);
            ApplyFx(snapshot);
            ApplyAudio(snapshot);
        }

        public void HandleEvent(in WeatherEvent weatherEvent)
        {
            if (weatherEvent.EventType != WeatherEventType.SpecialFxTriggered || specialFxBindings == null)
            {
                return;
            }

            foreach (var binding in specialFxBindings)
            {
                if (binding != null && binding.TrySpawn(this, weatherEvent))
                {
                    break;
                }
            }
        }

        private void ApplyCozyWeather(in WeatherPresentationSnapshot snapshot)
        {
            if (cozyWeatherComponent == null)
            {
                return;
            }

            if (_weatherProfileType == null || (_setWeatherInstant == null && _setWeatherTimed == null))
            {
                CacheCozyMethods();
                if (_weatherProfileType == null)
                {
                    return;
                }
            }

            if (snapshot.Weather == _lastAppliedType && math.abs(snapshot.Intensity - _lastAppliedIntensity) < 0.02f)
            {
                return;
            }

            var profile = ResolveProfile(snapshot.Weather);
            if (profile == null)
            {
                return;
            }

            if (!_weatherProfileType.IsInstanceOfType(profile))
            {
                Debug.LogWarning($"Profile assigned for {snapshot.Weather} is not a COZY WeatherProfile asset.", this);
                return;
            }

            var transition = snapshot.TransitionSeconds > 0f ? snapshot.TransitionSeconds : defaultTransitionSeconds;
            try
            {
                if (_setWeatherTimed != null)
                {
                    _setWeatherTimed.Invoke(cozyWeatherComponent, new object[] { profile, transition });
                }
                else
                {
                    _setWeatherInstant?.Invoke(cozyWeatherComponent, new object[] { profile });
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"Failed to call COZY SetWeather: {ex.Message}", this);
            }

            _lastAppliedType = snapshot.Weather;
            _lastAppliedIntensity = snapshot.Intensity;
        }

        private UnityEngine.Object ResolveProfile(WeatherType type)
        {
            return type switch
            {
                WeatherType.Clear => clearProfile,
                WeatherType.Rain => rainProfile ?? clearProfile,
                WeatherType.Snow => snowProfile ?? rainProfile ?? clearProfile,
                WeatherType.Fog => fogProfile ?? rainProfile ?? clearProfile,
                WeatherType.Storm => stormProfile ?? rainProfile ?? clearProfile,
                _ => clearProfile
            };
        }

        private void ApplyFx(in WeatherPresentationSnapshot snapshot)
        {
            if (fxBindings == null)
            {
                return;
            }

            foreach (var binding in fxBindings)
            {
                binding?.Apply(snapshot);
            }
        }

        private void ApplyAudio(in WeatherPresentationSnapshot snapshot)
        {
            if (audioBindings == null)
            {
                return;
            }

            var dt = UnityEngine.Time.deltaTime;
            foreach (var binding in audioBindings)
            {
                binding?.Apply(snapshot, dt);
            }
        }
    }

    [Serializable]
    public sealed class WeatherFxBinding
    {
        [SerializeField] private WeatherType type = WeatherType.Rain;
        [SerializeField] private VisualEffect visualEffect;
        [SerializeField] private ParticleSystem particleSystem;
        [SerializeField] private GameObject rootObject;
        [SerializeField] private float minIntensity = 0.05f;
        [SerializeField] private float maxEmissionRate = 30f;
        [SerializeField] private AnimationCurve intensityCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);
        [SerializeField] private string intensityProperty = "Intensity";

        public void Apply(in WeatherPresentationSnapshot snapshot)
        {
            bool active = snapshot.Weather == type && snapshot.Intensity >= minIntensity;

            if (rootObject != null)
            {
                rootObject.SetActive(active);
            }

            if (visualEffect != null)
            {
                visualEffect.enabled = active;
                if (active && !string.IsNullOrEmpty(intensityProperty))
                {
                    visualEffect.SetFloat(intensityProperty, snapshot.Intensity);
                }
            }

            if (particleSystem != null)
            {
                var emission = particleSystem.emission;
                emission.enabled = active;
                if (active)
                {
                    var remap = intensityCurve != null ? intensityCurve.Evaluate(snapshot.Intensity) : snapshot.Intensity;
                    emission.rateOverTime = maxEmissionRate * math.saturate(remap);
                }
            }
        }
    }

    [Serializable]
    public sealed class WeatherAudioBinding
    {
        [SerializeField] private WeatherType type = WeatherType.Rain;
        [SerializeField] private AudioSource loopSource;
        [SerializeField] private float maxVolume = 0.8f;
        [SerializeField] private float fadeSpeed = 1.5f;
        [SerializeField] private float minIntensity = 0.05f;
        [SerializeField] private AudioClip enterOneShot;
        [SerializeField] private float enterVolume = 0.9f;

        [NonSerialized] private bool _wasActive;

        public void Apply(in WeatherPresentationSnapshot snapshot, float deltaTime)
        {
            if (loopSource == null)
            {
                return;
            }

            bool active = snapshot.Weather == type && snapshot.Intensity >= minIntensity;
            float targetVolume = active ? maxVolume * math.saturate(snapshot.Intensity) : 0f;
            loopSource.volume = Mathf.MoveTowards(loopSource.volume, targetVolume, fadeSpeed * deltaTime);

            if (active && !loopSource.isPlaying)
            {
                loopSource.Play();
            }
            else if (!active && loopSource.volume <= 0.001f && loopSource.isPlaying)
            {
                loopSource.Stop();
            }

            if (active && !_wasActive && enterOneShot != null)
            {
                loopSource.PlayOneShot(enterOneShot, enterVolume);
            }

            _wasActive = active;
        }
    }

    [Serializable]
    public sealed class WeatherSpecialFxBinding
    {
        [SerializeField] private string payloadId = "miracle.storm";
        [SerializeField] private WeatherType fallbackType = WeatherType.Storm;
        [SerializeField] private GameObject effectPrefab;
        [SerializeField] private bool parentToRig;
        [SerializeField] private float lifetimeSeconds = 6f;
        [SerializeField] private AudioClip audioClip;
        [SerializeField] private float audioVolume = 1f;

        public bool TrySpawn(WeatherRigAuthoring rig, in WeatherEvent evt)
        {
            if (!Matches(evt))
            {
                return false;
            }

            var position = parentToRig ? rig.transform.position : new Vector3(evt.Position.x, evt.Position.y, evt.Position.z);

            if (effectPrefab != null)
            {
                var parent = parentToRig ? rig.transform : null;
                var instance = UnityEngine.Object.Instantiate(effectPrefab, position, Quaternion.identity, parent);
                if (lifetimeSeconds > 0f)
                {
                    UnityEngine.Object.Destroy(instance, lifetimeSeconds);
                }
            }

            if (audioClip != null)
            {
                AudioSource.PlayClipAtPoint(audioClip, position, audioVolume * math.saturate(evt.Intensity));
            }

            return true;
        }

        private bool Matches(in WeatherEvent evt)
        {
            if (!string.IsNullOrWhiteSpace(payloadId) && evt.Payload.ToString().Equals(payloadId, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            return evt.Weather == fallbackType;
        }
    }

    public readonly struct WeatherPresentationSnapshot
    {
        public WeatherPresentationSnapshot(WeatherType weather, float intensity, TimeOfDayPhase phase, float temperature, float moisture, float transitionSeconds, WeatherRequestSource source, string payload, Vector3 overridePosition)
        {
            Weather = weather;
            Intensity = intensity;
            Phase = phase;
            Temperature = temperature;
            Moisture = moisture;
            TransitionSeconds = transitionSeconds;
            Source = source;
            Payload = payload;
            OverridePosition = overridePosition;
        }

        public WeatherType Weather { get; }
        public float Intensity { get; }
        public TimeOfDayPhase Phase { get; }
        public float Temperature { get; }
        public float Moisture { get; }
        public float TransitionSeconds { get; }
        public WeatherRequestSource Source { get; }
        public string Payload { get; }
        public Vector3 OverridePosition { get; }
    }
}
