using System;
using System.Collections.Generic;
using PureDOTS.Environment;
using UnityEngine;

namespace Godgame.Biomes
{
    /// <summary>
    /// Data container that describes how ground, vegetation, props, and FX should look for a biome/moisture bucket.
    /// Designers author these assets and the runtime agent will swap between them as climates change.
    /// </summary>
    [CreateAssetMenu(fileName = "BiomeTerrainProfile", menuName = "Godgame/Biomes/Terrain Profile", order = 0)]
    public sealed class BiomeTerrainProfile : ScriptableObject
    {
        [Header("Identity")]
        [SerializeField] string displayName = "Forest / Moist";
        [SerializeField] BiomeType biome = BiomeType.Forest;
        [SerializeField] FloatRange moistureRange = new FloatRange(30f, 80f);
        [SerializeField] FloatRange temperatureRange = new FloatRange(-5f, 35f);

        [Header("Presentation Descriptor")]
        [Tooltip("Descriptor pushed into SwappablePresentationBinding so the rest of the presentation stack stays in sync.")]
        [SerializeField] string descriptorKey = "ground.default";
        [SerializeField, HideInInspector] int descriptorHash = Animator.StringToHash("ground.default");

        [Header("Ground")] [SerializeField] BiomeGroundSettings ground = new();

        [Header("Vegetation Clusters")] [SerializeField]
        List<VegetationClusterRule> vegetationClusters = new();

        [Header("Ambient Props")] [SerializeField]
        List<AmbientPropRule> ambientProps = new();

        [Header("Ground FX / Atmospherics")] [SerializeField]
        List<BiomeVfxBinding> vfxBindings = new();

        public string DisplayName => displayName;
        public BiomeType Biome => biome;
        public FloatRange MoistureRange => moistureRange;
        public FloatRange TemperatureRange => temperatureRange;
        public string DescriptorKey => descriptorKey;
        public int DescriptorHash => descriptorHash;
        public BiomeGroundSettings Ground => ground;
        public IReadOnlyList<VegetationClusterRule> VegetationClusters => vegetationClusters;
        public IReadOnlyList<AmbientPropRule> AmbientProps => ambientProps;
        public IReadOnlyList<BiomeVfxBinding> VfxBindings => vfxBindings;

        public bool Matches(BiomeType biomeCandidate, float moisture, float temperature)
        {
            if (Biome != BiomeType.Unknown && biomeCandidate != Biome)
            {
                return false;
            }

            return moistureRange.Contains(moisture) && temperatureRange.Contains(temperature);
        }

        /// <summary>
        /// Returns a normalized fitness score (lower is better) for how well the given moisture/temperature fits this profile.
        /// Used by the runtime agent to pick between overlapping ranges.
        /// </summary>
        public float ComputeMatchScore(float moisture, float temperature)
        {
            var moistureScore = moistureRange.DistanceToCenter01(moisture);
            var temperatureScore = temperatureRange.DistanceToCenter01(temperature);
            return moistureScore + temperatureScore;
        }

        void OnValidate()
        {
            displayName = string.IsNullOrWhiteSpace(displayName) ? name : displayName.Trim();
            descriptorKey = string.IsNullOrWhiteSpace(descriptorKey) ? "ground.default" : descriptorKey.Trim();
            descriptorHash = Animator.StringToHash(descriptorKey);
            moistureRange = FloatRange.Normalize(moistureRange, 0f, 100f);
            temperatureRange = FloatRange.Normalize(temperatureRange, -80f, 80f);
        }
    }

    public enum GroundSurfaceMode
    {
        ProceduralPrefab = 0,
        MicroSplatMaterial = 1,
        MaterialOnly = 2
    }

    [Serializable]
    public sealed class BiomeGroundSettings
    {
        [Tooltip("What drives the terrain look when the biome is active.")]
        public GroundSurfaceMode surfaceMode = GroundSurfaceMode.ProceduralPrefab;

        [Tooltip("Procedural ground prefab or mesh to instantiate/spawn when this biome activates.")]
        public GameObject proceduralPrefab;

        [Tooltip("Local position offset for the procedural prefab when spawned.")]
        public Vector3 spawnOffset = Vector3.zero;

        [Tooltip("Local Euler rotation offset for the procedural prefab.")]
        public Vector3 spawnRotationEuler = Vector3.zero;

        [Tooltip("Local scale applied to the procedural prefab.")]
        public Vector3 spawnScale = Vector3.one;

        [Tooltip("MicroSplat material instance to assign when using MicroSplat mode.")]
        public Material microSplatMaterial;

        [Tooltip("Optional MicroSplat profile keyword/preset that should be enabled once the material is active.")]
        public string microSplatProfileKeyword;

        [Tooltip("Fallback lit material when you only need a simple override (or when MicroSplat fails to load).")]
        public Material fallbackMaterial;

        [Tooltip("Should the runtime allow height-based blending (wetness/snow/etc.) for this biome.")]
        public bool allowRuntimeHeightBlending = true;
    }

    [Serializable]
    public sealed class VegetationClusterRule
    {
        public string label = "Cluster";
        public GameObject clusterPrefab;
        public FloatRange heightRange = new FloatRange(-10f, 1200f);
        public FloatRange slopeRange = new FloatRange(0f, 30f);
        public FloatRange moistureRange = new FloatRange(0f, 100f);
        [Min(0f)] public float baseDensity = 1f;
        public AnimationCurve densityByMoisture = AnimationCurve.Linear(0f, 0f, 1f, 1f);
        [Min(0.1f)] public float noiseScale = 16f;
        [Min(0f)] public float densityMultiplier = 1f;
        public bool alignToNormal = true;
    }

    [Serializable]
    public sealed class AmbientPropRule
    {
        public string label = "Props";
        [Tooltip("Single prop or pooled prefab to place.")]
        public GameObject propPrefab;

        [Tooltip("Optional placement script prefab (PolyWorld/Azure spawners, GPU instancers, etc.)")]
        public GameObject spawnerPrefab;

        [Range(0f, 1f)] public float probability = 0.5f;
        public Vector2Int countRange = new Vector2Int(1, 3);
        public Vector2 scaleRange = new Vector2(0.9f, 1.2f);
        public FloatRange heightRange = new FloatRange(-10f, 1200f);
        public FloatRange slopeRange = new FloatRange(0f, 45f);
        public bool alignToNormal = true;
    }

    [Serializable]
    public sealed class BiomeVfxBinding
    {
        public string label = "FX";
        public string vfxDescriptorKey = "fx.default";
        public GameObject vfxPrefab;
        public Vector3 localOffset = Vector3.zero;
        public Vector3 localRotation = Vector3.zero;
        public Vector3 localScale = Vector3.one;
    }

    [Serializable]
    public struct FloatRange
    {
        public float min;
        public float max;

        public FloatRange(float min, float max)
        {
            this.min = min;
            this.max = max;
        }

        public bool Contains(float value) => value >= min && value <= max;

        public float Clamp(float value)
        {
            if (value < min) return min;
            if (value > max) return max;
            return value;
        }

        public float Normalize(float value)
        {
            var clamped = Clamp(value);
            if (Mathf.Approximately(max, min))
            {
                return 0f;
            }

            return Mathf.InverseLerp(min, max, clamped);
        }

        public float DistanceToCenter01(float value)
        {
            return Mathf.Abs(Normalize(value) - 0.5f);
        }

        public static FloatRange Normalize(FloatRange value, float minLimit, float maxLimit)
        {
            if (value.max < value.min)
            {
                (value.min, value.max) = (value.max, value.min);
            }

            value.min = Mathf.Clamp(value.min, minLimit, maxLimit);
            value.max = Mathf.Clamp(value.max, minLimit, maxLimit);
            if (value.max < value.min)
            {
                value.max = value.min;
            }

            return value;
        }
    }
}
