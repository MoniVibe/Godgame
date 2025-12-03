using Godgame.Environment;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace Godgame.Authoring
{
    /// <summary>
    /// ScriptableObject authoring asset for biome definitions.
    /// Bakes biome profiles into a BiomeDefinitionBlob.
    /// </summary>
    [CreateAssetMenu(fileName = "BiomeDefinition", menuName = "Godgame/Biome Definition", order = 100)]
    public sealed class BiomeDefinitionAuthoring : ScriptableObject
    {
        [System.Serializable]
        public struct BiomeProfileData
        {
            [Tooltip("Unique biome identifier (e.g., 'Temperate', 'Grasslands', 'Mountains')")]
            public string id;

            [Tooltip("Climate temperature range (Celsius)")]
            public float tempMin;
            public float tempMax;

            [Tooltip("Moisture range (0-1)")]
            [Range(0f, 1f)]
            public float moistMin;
            [Range(0f, 1f)]
            public float moistMax;

            [Tooltip("Elevation range (meters)")]
            public float elevMin;
            public float elevMax;

            [Tooltip("Maximum slope angle (degrees)")]
            public float slopeMaxDeg;

            [Tooltip("Villager stamina drain modifier (100 = no change, >100 = more drain, <100 = less drain)")]
            [Range(0, 255)]
            public byte villagerStaminaDrainPct;

            [Tooltip("Disease risk percentage (0-100)")]
            [Range(0, 100)]
            public byte diseaseRiskPct;

            [Tooltip("Wood resource seeding bias weight (0-1000)")]
            [Range(0, 1000)]
            public ushort resourceBiasWood;

            [Tooltip("Ore resource seeding bias weight (0-1000)")]
            [Range(0, 1000)]
            public ushort resourceBiasOre;

            [Tooltip("Minimap palette index")]
            public byte minimapPalette;

            [Tooltip("Ground material/style token")]
            public byte groundStyle;

            [Tooltip("Weather FX profile token")]
            public byte weatherProfileToken;
        }

        [SerializeField]
        [Tooltip("List of biome profiles")]
        private BiomeProfileData[] profiles = new BiomeProfileData[0];

        public BiomeProfileData[] Profiles => profiles;

        /// <summary>
        /// Baker that converts BiomeDefinitionAuthoring to BiomeDefinitionBlob.
        /// </summary>
        public sealed class Baker : Unity.Entities.Baker<BiomeDefinitionAuthoring>
        {
            public override void Bake(BiomeDefinitionAuthoring authoring)
            {
                if (authoring.profiles == null || authoring.profiles.Length == 0)
                {
                    Debug.LogWarning($"BiomeDefinitionAuthoring '{authoring.name}' has no profiles defined.");
                    return;
                }

                // Create blob builder
                var builder = new BlobBuilder(Allocator.Temp);
                ref var blobRoot = ref builder.ConstructRoot<BiomeDefinitionBlob>();
                var blobProfiles = builder.Allocate(ref blobRoot.Profiles, authoring.profiles.Length);

                // Convert profiles to blob data
                for (int i = 0; i < authoring.profiles.Length; i++)
                {
                    var profileData = authoring.profiles[i];
                    uint biomeId32 = (uint)(i + 1); // 1-based IDs

                    blobProfiles[i] = new BiomeProfile
                    {
                        Id = profileData.id,
                        BiomeId32 = biomeId32,
                        TempMin = profileData.tempMin,
                        TempMax = profileData.tempMax,
                        MoistMin = profileData.moistMin,
                        MoistMax = profileData.moistMax,
                        ElevMin = profileData.elevMin,
                        ElevMax = profileData.elevMax,
                        SlopeMaxDeg = profileData.slopeMaxDeg,
                        VillagerStaminaDrainPct = profileData.villagerStaminaDrainPct,
                        DiseaseRiskPct = profileData.diseaseRiskPct,
                        ResourceBiasWood = profileData.resourceBiasWood,
                        ResourceBiasOre = profileData.resourceBiasOre,
                        MinimapPalette = profileData.minimapPalette,
                        GroundStyle = profileData.groundStyle,
                        WeatherProfileToken = profileData.weatherProfileToken
                    };
                }

                // Create blob asset reference
                var blobAsset = builder.CreateBlobAssetReference<BiomeDefinitionBlob>(Allocator.Persistent);
                builder.Dispose();

                // Add singleton component with blob reference
                var entity = GetEntity(TransformUsageFlags.None);
                AddComponent(entity, new BiomeDefinitionSingleton
                {
                    Definitions = blobAsset
                });
            }
        }
    }
}

