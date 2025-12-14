using Godgame.Environment;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace Godgame.Authoring
{
    /// <summary>
    /// ScriptableObject authoring asset for race/culture biome preferences.
    /// Bakes preferences into a BiomePreferenceBlob.
    /// </summary>
    [CreateAssetMenu(fileName = "BiomePreferenceCatalog", menuName = "Godgame/Biome Preference Catalog", order = 103)]
    public sealed class BiomePreferenceCatalogAuthoring : ScriptableObject
    {
        [System.Serializable]
        public struct BiomePreferenceData
        {
            [Tooltip("Race or culture identifier")]
            public string raceOrCultureId;

            [Tooltip("Preferred biome mask (bitmask: 1=Temperate, 2=Grasslands, 4=Mountains, etc.)")]
            public uint preferredBiomeMask;

            [Tooltip("Avoided biome mask (bitmask)")]
            public uint avoidedBiomeMask;

            [Tooltip("Preference weight multiplier for preferred biomes (1.0 = neutral, >1.0 = preferred)")]
            [Range(0.1f, 2f)]
            public float preferenceWeight;

            [Tooltip("Avoidance penalty multiplier for avoided biomes (1.0 = neutral, <1.0 = penalty)")]
            [Range(0.1f, 2f)]
            public float avoidancePenalty;
        }

        [SerializeField]
        [Tooltip("List of biome preferences")]
        private BiomePreferenceData[] preferences = new BiomePreferenceData[0];

        public BiomePreferenceData[] Preferences => preferences;

        public bool TryBuildBlob(out BlobAssetReference<BiomePreferenceBlob> blobAsset)
        {
            blobAsset = default;
            if (preferences == null || preferences.Length == 0)
            {
#if UNITY_EDITOR
                Debug.LogWarning($"BiomePreferenceCatalogAuthoring '{name}' has no preferences defined.");
#endif
                return false;
            }

            var builder = new BlobBuilder(Allocator.Temp);
            ref var blobRoot = ref builder.ConstructRoot<BiomePreferenceBlob>();
            var blobPreferences = builder.Allocate(ref blobRoot.Preferences, preferences.Length);

            for (int i = 0; i < preferences.Length; i++)
            {
                var prefData = preferences[i];
                blobPreferences[i] = new BiomePreference
                {
                    RaceOrCultureId = prefData.raceOrCultureId,
                    PreferredBiomeMask = prefData.preferredBiomeMask,
                    AvoidedBiomeMask = prefData.avoidedBiomeMask,
                    PreferenceWeight = prefData.preferenceWeight,
                    AvoidancePenalty = prefData.avoidancePenalty
                };
            }

            blobAsset = builder.CreateBlobAssetReference<BiomePreferenceBlob>(Allocator.Persistent);
            builder.Dispose();
            return true;
        }
    }

    [DisallowMultipleComponent]
    public sealed class BiomePreferenceCatalogAuthoringComponent : MonoBehaviour
    {
        [SerializeField] private BiomePreferenceCatalogAuthoring catalog;

        public sealed class Baker : Unity.Entities.Baker<BiomePreferenceCatalogAuthoringComponent>
        {
            public override void Bake(BiomePreferenceCatalogAuthoringComponent authoringComponent)
            {
                if (authoringComponent.catalog == null)
                {
#if UNITY_EDITOR
                    Debug.LogWarning("[BiomePreferenceCatalog] No BiomePreferenceCatalogAuthoring asset assigned.");
#endif
                    return;
                }

                if (!authoringComponent.catalog.TryBuildBlob(out var blobAsset))
                {
                    return;
                }

                var entity = GetEntity(TransformUsageFlags.None);
                AddComponent(entity, new BiomePreferenceSingleton
                {
                    Preferences = blobAsset
                });
            }
        }
    }
}
