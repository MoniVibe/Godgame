using System.Collections.Generic;
using Godgame.Miracles.Presentation;
using PureDOTS.Environment;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Godgame.Biomes
{
    /// <summary>
    /// Central coordinator that watches the current biome/moisture sample, spawns the right ground prefab,
    /// toggles MicroSplat/material presets, manages lightweight vegetation/prop clusters, and pushes the
    /// descriptor into any swappable presentation bindings tagged with <see cref="BiomeTerrainBinding"/>.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class BiomeTerrainAgent : MonoBehaviour
    {
        [Header("Profile Library")]
        [SerializeField] List<BiomeTerrainProfile> profiles = new();

        [Header("Anchors")] [SerializeField]
        Transform groundAnchor;

        [SerializeField] Transform vegetationAnchor;
        [SerializeField] Transform ambientAnchor;
        [SerializeField] Renderer groundRenderer;

        [Header("Initial State")]
        [SerializeField] bool applyOnStart = true;
        [SerializeField] BiomeType initialBiome = BiomeType.Forest;
        [SerializeField, Range(0f, 100f)] float initialMoisture = 55f;
        [SerializeField, Range(-80f, 80f)] float initialTemperature = 18f;

        [Header("Spawn Configuration")]
        [SerializeField] Vector2 vegetationAreaSize = new(60f, 60f);
        [SerializeField] Vector2 ambientAreaSize = new(70f, 70f);
        [SerializeField] float spawnHeightOffset = 0f;
        [SerializeField, Min(1)] int maxVegetationInstances = 96;
        [SerializeField, Min(1)] int maxAmbientInstances = 64;
        [SerializeField] uint randomSeed = 1337u;
        [SerializeField] bool respawnOnSameProfile = true;

        [Header("Presentation Binding")]
        [SerializeField] bool updatePresentationBindings = true;
        [SerializeField] int regionIdFilter = 0;

        [Header("Debug")] [SerializeField]
        bool logTransitions = false;

        readonly List<GameObject> _spawnedVegetation = new();
        readonly List<GameObject> _spawnedAmbientProps = new();
        BiomeTerrainProfile _activeProfile;
        GameObject _activeGround;
        float _lastMoisture;
        float _lastTemperature;

        Unity.Mathematics.Random _random;
        EntityManager _entityManager;
        EntityQuery _bindingQuery;
        bool _queryInitialized;

        void Awake()
        {
            if (profiles == null)
            {
                profiles = new List<BiomeTerrainProfile>();
            }

            var seed = randomSeed == 0 ? 1u : randomSeed;
            _random = new Unity.Mathematics.Random(seed);
        }

        void Start()
        {
            if (applyOnStart)
            {
                ApplySample(initialBiome, initialMoisture, initialTemperature, force: true);
            }
        }

        void OnDisable()
        {
            ClearSpawnedContent(_spawnedVegetation);
            ClearSpawnedContent(_spawnedAmbientProps);
            ReleaseGroundInstance();
            DisposeQuery();
        }

        public void ApplySample(BiomeType biome, float moisture, float temperature, bool force = false)
        {
            if (profiles == null || profiles.Count == 0)
            {
                Debug.LogWarning("BiomeTerrainAgent has no profiles configured.", this);
                return;
            }

            var profile = ResolveProfile(biome, moisture, temperature);
            if (profile == null)
            {
                Debug.LogWarning($"No biome terrain profile matched biome {biome} (moisture {moisture:0.0}, temp {temperature:0.0}).", this);
                return;
            }

            bool descriptorChanged = _activeProfile != profile;
            bool climateChanged = descriptorChanged || force || math.abs(moisture - _lastMoisture) > 0.1f || math.abs(temperature - _lastTemperature) > 0.1f;
            if (!climateChanged)
            {
                return;
            }

            _lastMoisture = moisture;
            _lastTemperature = temperature;

            ApplyGround(profile);

            if (descriptorChanged || respawnOnSameProfile)
            {
                RespawnVegetation(profile, moisture, temperature);
                RespawnAmbientProps(profile, moisture, temperature);
            }

            UpdatePresentationBindings(profile);
            _activeProfile = profile;

            if (logTransitions)
            {
                Debug.Log($"BiomeTerrainAgent applied profile '{profile.DisplayName}' ({profile.DescriptorKey})", this);
            }
        }

        BiomeTerrainProfile ResolveProfile(BiomeType biome, float moisture, float temperature)
        {
            BiomeTerrainProfile best = null;
            float bestScore = float.MaxValue;

            foreach (var profile in profiles)
            {
                if (profile == null)
                {
                    continue;
                }

                if (!profile.Matches(biome, moisture, temperature))
                {
                    continue;
                }

                var score = profile.ComputeMatchScore(moisture, temperature);
                if (score < bestScore)
                {
                    bestScore = score;
                    best = profile;
                }
            }

            if (best != null)
            {
                return best;
            }

            // Fallback to same-biome even if ranges did not match.
            foreach (var profile in profiles)
            {
                if (profile != null && profile.Biome == biome)
                {
                    return profile;
                }
            }

            // Absolute fallback: first valid profile.
            foreach (var profile in profiles)
            {
                if (profile != null)
                {
                    return profile;
                }
            }

            return null;
        }

        void ApplyGround(BiomeTerrainProfile profile)
        {
            var settings = profile.Ground;
            var parent = groundAnchor ? groundAnchor : transform;

            switch (settings.surfaceMode)
            {
                case GroundSurfaceMode.ProceduralPrefab:
                    ReleaseGroundInstance();
                    if (settings.proceduralPrefab != null)
                    {
                        _activeGround = InstantiateRuntime(settings.proceduralPrefab, parent);
                        if (_activeGround != null)
                        {
                            var tr = _activeGround.transform;
                            tr.localPosition = settings.spawnOffset;
                            tr.localRotation = Quaternion.Euler(settings.spawnRotationEuler);
                            tr.localScale = settings.spawnScale;
                        }
                    }

                    ApplyMaterial(settings.fallbackMaterial, string.Empty);
                    break;

                case GroundSurfaceMode.MicroSplatMaterial:
                    ReleaseGroundInstance();
                    ApplyMaterial(settings.microSplatMaterial != null ? settings.microSplatMaterial : settings.fallbackMaterial, settings.microSplatProfileKeyword);
                    break;

                case GroundSurfaceMode.MaterialOnly:
                    ReleaseGroundInstance();
                    ApplyMaterial(settings.fallbackMaterial, string.Empty);
                    break;
            }
        }

        void ApplyMaterial(Material material, string keyword)
        {
            if (groundRenderer == null || material == null)
            {
                return;
            }

            groundRenderer.sharedMaterial = material;
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                groundRenderer.sharedMaterial.EnableKeyword(keyword);
            }
        }

        void ReleaseGroundInstance()
        {
            if (_activeGround == null)
            {
                return;
            }

#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                DestroyImmediate(_activeGround);
            }
            else
#endif
            {
                Destroy(_activeGround);
            }

            _activeGround = null;
        }

        void RespawnVegetation(BiomeTerrainProfile profile, float moisture, float temperature)
        {
            ClearSpawnedContent(_spawnedVegetation);
            if (profile.VegetationClusters == null || profile.VegetationClusters.Count == 0)
            {
                return;
            }

            var parent = vegetationAnchor ? vegetationAnchor : (groundAnchor ? groundAnchor : transform);
            if (parent == null)
            {
                parent = transform;
            }

            ResetRandom(profile, 0u);

            foreach (var rule in profile.VegetationClusters)
            {
                if (rule == null || rule.clusterPrefab == null)
                {
                    continue;
                }

                if (!_spawnedVegetation.CountLessThan(maxVegetationInstances))
                {
                    break;
                }

                if (!rule.heightRange.Contains(parent.position.y))
                {
                    continue;
                }

                var normalizedMoisture = rule.moistureRange.Normalize(moisture);
                var curveValue = rule.densityByMoisture != null ? Mathf.Max(0f, rule.densityByMoisture.Evaluate(normalizedMoisture)) : 1f;
                var desiredCount = Mathf.Clamp(Mathf.RoundToInt(rule.baseDensity * Mathf.Max(0.1f, rule.densityMultiplier) * curveValue), 0, maxVegetationInstances - _spawnedVegetation.Count);
                if (desiredCount <= 0)
                {
                    continue;
                }

                for (int i = 0; i < desiredCount; i++)
                {
                    var position = SamplePosition(parent.position, vegetationAreaSize);
                    position.y += spawnHeightOffset;
                    var rotation = rule.alignToNormal ? Quaternion.identity : Quaternion.Euler(0f, _random.NextFloat(0f, 360f), 0f);
                    var instance = InstantiateRuntime(rule.clusterPrefab, parent);
                    if (instance == null)
                    {
                        continue;
                    }

                    var tr = instance.transform;
                    tr.position = position;
                    tr.rotation = rotation;
                    _spawnedVegetation.Add(instance);

                    if (!_spawnedVegetation.CountLessThan(maxVegetationInstances))
                    {
                        break;
                    }
                }
            }
        }

        void RespawnAmbientProps(BiomeTerrainProfile profile, float moisture, float temperature)
        {
            ClearSpawnedContent(_spawnedAmbientProps);
            if (profile.AmbientProps == null || profile.AmbientProps.Count == 0)
            {
                return;
            }

            var parent = ambientAnchor ? ambientAnchor : (groundAnchor ? groundAnchor : transform);
            if (parent == null)
            {
                parent = transform;
            }

            ResetRandom(profile, 0x9E3779B9u);

            foreach (var rule in profile.AmbientProps)
            {
                if (rule == null)
                {
                    continue;
                }

                if (!_spawnedAmbientProps.CountLessThan(maxAmbientInstances))
                {
                    break;
                }

                var prefab = rule.spawnerPrefab != null ? rule.spawnerPrefab : rule.propPrefab;
                if (prefab == null)
                {
                    continue;
                }

                if (_random.NextFloat() > rule.probability)
                {
                    continue;
                }

                var minCount = math.max(0, rule.countRange.x);
                var maxCount = math.max(minCount, rule.countRange.y);
                var spawnCount = _random.NextInt(minCount, maxCount + 1);
                spawnCount = math.min(spawnCount, maxAmbientInstances - _spawnedAmbientProps.Count);
                if (spawnCount <= 0)
                {
                    continue;
                }

                for (int i = 0; i < spawnCount; i++)
                {
                    var position = SamplePosition(parent.position, ambientAreaSize);
                    position.y += spawnHeightOffset;
                    var rotation = rule.alignToNormal ? Quaternion.identity : Quaternion.Euler(0f, _random.NextFloat(0f, 360f), 0f);
                    var instance = InstantiateRuntime(prefab, parent);
                    if (instance == null)
                    {
                        continue;
                    }

                    var tr = instance.transform;
                    tr.position = position;
                    tr.rotation = rotation;
                    var scale = Mathf.Lerp(rule.scaleRange.x, rule.scaleRange.y, _random.NextFloat());
                    tr.localScale = Vector3.one * scale;
                    _spawnedAmbientProps.Add(instance);

                    if (!_spawnedAmbientProps.CountLessThan(maxAmbientInstances))
                    {
                        break;
                    }
                }
            }
        }

        void UpdatePresentationBindings(BiomeTerrainProfile profile)
        {
            if (!updatePresentationBindings || profile == null || string.IsNullOrWhiteSpace(profile.DescriptorKey))
            {
                return;
            }

            if (!TryEnsureQuery())
            {
                return;
            }

            using var entities = _bindingQuery.ToEntityArray(Allocator.Temp);
            using var regions = _bindingQuery.ToComponentDataArray<BiomeTerrainBinding>(Allocator.Temp);

            var descriptor = ToFixedString(profile.DescriptorKey);
            for (int i = 0; i < entities.Length; i++)
            {
                if (regionIdFilter >= 0 && regions[i].RegionId != regionIdFilter)
                {
                    continue;
                }

                var binding = _entityManager.GetComponentData<SwappablePresentationBinding>(entities[i]);
                binding.DescriptorKey = descriptor;
                binding.DescriptorHash = profile.DescriptorHash;
                _entityManager.SetComponentData(entities[i], binding);

                if (!_entityManager.HasComponent<SwappablePresentationDirtyTag>(entities[i]))
                {
                    _entityManager.AddComponent<SwappablePresentationDirtyTag>(entities[i]);
                }
            }
        }

        void ClearSpawnedContent(List<GameObject> list)
        {
            if (list == null)
            {
                return;
            }

            for (int i = 0; i < list.Count; i++)
            {
                var go = list[i];
                if (go == null)
                {
                    continue;
                }

#if UNITY_EDITOR
                if (!Application.isPlaying)
                {
                    DestroyImmediate(go);
                }
                else
#endif
                {
                    Destroy(go);
                }
            }

            list.Clear();
        }

        Vector3 SamplePosition(Vector3 center, Vector2 area)
        {
            var half = area * 0.5f;
            var x = _random.NextFloat(-half.x, half.x);
            var z = _random.NextFloat(-half.y, half.y);
            return center + new Vector3(x, 0f, z);
        }

        GameObject InstantiateRuntime(GameObject prefab, Transform parent)
        {
            if (prefab == null)
            {
                return null;
            }

#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                var instance = (GameObject)UnityEditor.PrefabUtility.InstantiatePrefab(prefab, parent);
                return instance;
            }
#endif

            return Instantiate(prefab, parent);
        }

        void ResetRandom(BiomeTerrainProfile profile, uint salt)
        {
            var hashSeed = (uint)(profile.DescriptorHash == 0 ? 1 : profile.DescriptorHash);
            var combined = randomSeed ^ hashSeed ^ (salt == 0 ? 0u : salt);
            combined = combined == 0 ? 1u : combined;
            _random = new Unity.Mathematics.Random(combined);
        }

        bool TryEnsureQuery()
        {
            if (_queryInitialized && _entityManager == default)
            {
                _queryInitialized = false;
            }

            if (_queryInitialized)
            {
                return true;
            }

            if (World.DefaultGameObjectInjectionWorld == null)
            {
                return false;
            }

            _entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
            if (_entityManager == default)
            {
                return false;
            }

            _bindingQuery = _entityManager.CreateEntityQuery(new EntityQueryDesc
            {
                All = new[]
                {
                    ComponentType.ReadWrite<SwappablePresentationBinding>(),
                    ComponentType.ReadOnly<BiomeTerrainBinding>()
                }
            });

            _queryInitialized = true;
            return true;
        }

        void DisposeQuery()
        {
            if (_queryInitialized)
            {
                _bindingQuery.Dispose();
            }

            _queryInitialized = false;
            _entityManager = default;
        }

        static FixedString64Bytes ToFixedString(string value)
        {
            // Avoid managed string APIs inside Burst; fill via iterator to truncate safely.
            FixedString64Bytes result = default;
            if (string.IsNullOrEmpty(value))
            {
                return result;
            }

            int dstIndex = 0;
            var maxBytes = result.Capacity;
            for (int i = 0; i < value.Length && dstIndex < maxBytes; i++)
            {
                char c = value[i];
                // Skip whitespace trimming on both ends
                if (char.IsWhiteSpace(c))
                {
                    continue;
                }

                if (c > 0x7F)
                {
                    break; // Stop on non-ASCII to avoid multi-byte logic inside Burst
                }

                result.Append((byte)c);
                dstIndex++;
            }

            result.Length = (ushort)dstIndex;
            return result;
        }
    }

    static class BiomeTerrainAgentListExtensions
    {
        public static bool CountLessThan(this List<GameObject> list, int max)
        {
            return list.Count < math.max(1, max);
        }
    }
}
