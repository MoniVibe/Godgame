using Godgame.Environment.Vegetation;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;
using System.Collections.Generic;

namespace Godgame.Authoring
{
    /// <summary>
    /// ScriptableObject authoring asset for plant specifications.
    /// Bakes plant specs into a PlantSpecBlob with VariantOf support.
    /// </summary>
    [CreateAssetMenu(fileName = "PlantSpecCatalog", menuName = "Godgame/Plant Spec Catalog", order = 101)]
    public sealed class PlantSpecCatalogAuthoring : ScriptableObject
    {
        [System.Serializable]
        public struct PlantSpecData
        {
            [Tooltip("Unique plant identifier (e.g., 'Oak', 'Pine', 'BerryBush')")]
            public string id;

            [Tooltip("If set, inherits from this plant and applies deltas only")]
            public string variantOf;

            [Tooltip("Biome mask (bitmask of allowed biome IDs: 1=Temperate, 2=Grasslands, 4=Mountains, etc.)")]
            public uint biomeMask;

            [Tooltip("Temperature range (Celsius)")]
            public float tempMin;
            public float tempMax;

            [Tooltip("Moisture range (0-1)")]
            [Range(0f, 1f)]
            public float moistMin;
            [Range(0f, 1f)]
            public float moistMax;

            [Tooltip("Sunlight requirement (0-1)")]
            [Range(0f, 1f)]
            public float sunMin;
            [Range(0f, 1f)]
            public float sunMax;

            [Tooltip("Elevation range (meters)")]
            public float elevMin;
            public float elevMax;

            [Tooltip("Maximum slope angle (degrees)")]
            public float slopeMaxDeg;

            [Tooltip("Soil type mask (bitmask)")]
            public uint soilMask;

            [Tooltip("Hazard flags")]
            public PlantHazardFlags hazardFlags;

            [Tooltip("Growth stage durations (seconds)")]
            public float stageSecSeedling;
            public float stageSecSapling;
            public float stageSecMature;

            [Tooltip("Reproduction: seeds per day when mature")]
            public float reproSeedsPerDay;

            [Tooltip("Reproduction: cluster radius (meters)")]
            public float reproRadius;

            [Tooltip("Season mask (bitmask: 1=Spring, 2=Summer, 4=Fall, 8=Winter)")]
            public uint seasonMask;

            [Tooltip("Yield entries (harvestable materials)")]
            public PlantLootEntryData[] yields;

            [Tooltip("Style tokens")]
            public PlantStyleTokensData style;
        }

        [System.Serializable]
        public struct PlantLootEntryData
        {
            public string materialId;
            [Range(0f, 1f)]
            public float weight;
            public float minAmount;
            public float maxAmount;
        }

        [System.Serializable]
        public struct PlantStyleTokensData
        {
            public byte stylePalette;
            public byte stylePattern;
            public byte familyToken;
        }

        [SerializeField]
        [Tooltip("List of plant specifications")]
        private PlantSpecData[] plants = new PlantSpecData[0];

        public PlantSpecData[] Plants => plants;

        /// <summary>
        /// Baker that converts PlantSpecCatalogAuthoring to PlantSpecBlob.
        /// Handles VariantOf inheritance by applying deltas.
        /// </summary>
        public sealed class Baker : Unity.Entities.Baker<PlantSpecCatalogAuthoring>
        {
            public override void Bake(PlantSpecCatalogAuthoring authoring)
            {
                if (authoring.plants == null || authoring.plants.Length == 0)
                {
                    Debug.LogWarning($"PlantSpecCatalogAuthoring '{authoring.name}' has no plants defined.");
                    return;
                }

                // Store yields separately (keyed by plant ID)
                var yieldsMap = new Dictionary<string, List<PlantLootEntry>>();

                // First pass: build base specs (non-variants) and collect yields
                var resolvedSpecs = new List<PlantSpec>();
                var specLookup = new Dictionary<string, PlantSpec>();

                for (int i = 0; i < authoring.plants.Length; i++)
                {
                    var plantData = authoring.plants[i];
                    if (string.IsNullOrEmpty(plantData.variantOf))
                    {
                        var spec = ConvertToSpec(plantData, i);
                        resolvedSpecs.Add(spec);
                        specLookup[plantData.id] = spec;

                        // Store yields
                        if (plantData.yields != null && plantData.yields.Length > 0)
                        {
                            var yields = new List<PlantLootEntry>();
                            for (int j = 0; j < plantData.yields.Length; j++)
                            {
                                yields.Add(new PlantLootEntry
                                {
                                    MaterialId = plantData.yields[j].materialId,
                                    Weight = plantData.yields[j].weight,
                                    MinAmount = plantData.yields[j].minAmount,
                                    MaxAmount = plantData.yields[j].maxAmount
                                });
                            }
                            yieldsMap[plantData.id] = yields;
                        }
                    }
                }

                // Second pass: resolve variants
                for (int i = 0; i < authoring.plants.Length; i++)
                {
                    var plantData = authoring.plants[i];
                    if (!string.IsNullOrEmpty(plantData.variantOf))
                    {
                        if (specLookup.TryGetValue(plantData.variantOf, out var baseSpec))
                        {
                            var variantSpec = ApplyVariantDelta(baseSpec, plantData, i);
                            resolvedSpecs.Add(variantSpec);
                            specLookup[plantData.id] = variantSpec;

                            // Variant yields override base yields if provided
                            if (plantData.yields != null && plantData.yields.Length > 0)
                            {
                                var yields = new List<PlantLootEntry>();
                                for (int j = 0; j < plantData.yields.Length; j++)
                                {
                                    yields.Add(new PlantLootEntry
                                    {
                                        MaterialId = plantData.yields[j].materialId,
                                        Weight = plantData.yields[j].weight,
                                        MinAmount = plantData.yields[j].minAmount,
                                        MaxAmount = plantData.yields[j].maxAmount
                                    });
                                }
                                yieldsMap[plantData.id] = yields;
                            }
                            else if (yieldsMap.TryGetValue(plantData.variantOf, out var baseYields))
                            {
                                // Inherit yields from base
                                yieldsMap[plantData.id] = new List<PlantLootEntry>(baseYields);
                            }
                        }
                        else
                        {
                            Debug.LogWarning($"Plant '{plantData.id}' references unknown VariantOf '{plantData.variantOf}'. Creating as base spec.");
                            var spec = ConvertToSpec(plantData, i);
                            resolvedSpecs.Add(spec);
                            specLookup[plantData.id] = spec;

                            if (plantData.yields != null && plantData.yields.Length > 0)
                            {
                                var yields = new List<PlantLootEntry>();
                                for (int j = 0; j < plantData.yields.Length; j++)
                                {
                                    yields.Add(new PlantLootEntry
                                    {
                                        MaterialId = plantData.yields[j].materialId,
                                        Weight = plantData.yields[j].weight,
                                        MinAmount = plantData.yields[j].minAmount,
                                        MaxAmount = plantData.yields[j].maxAmount
                                    });
                                }
                                yieldsMap[plantData.id] = yields;
                            }
                        }
                    }
                }

                // Create blob builder
                var builder = new BlobBuilder(Allocator.Temp);
                ref var blobRoot = ref builder.ConstructRoot<PlantSpecBlob>();
                var blobPlants = builder.Allocate(ref blobRoot.Plants, resolvedSpecs.Count);

                // Copy resolved specs to blob with yields allocated
                for (int i = 0; i < resolvedSpecs.Count; i++)
                {
                    var spec = resolvedSpecs[i];
                    var plantId = spec.Id.ToString();

                    // Allocate yields array in blob
                    if (yieldsMap.TryGetValue(plantId, out var yields) && yields.Count > 0)
                    {
                        ref var yieldsArray = ref builder.Allocate(ref spec.Yields, yields.Count);
                        for (int j = 0; j < yields.Count; j++)
                        {
                            yieldsArray[j] = yields[j];
                        }
                    }
                    else
                    {
                        builder.Allocate(ref spec.Yields, 0);
                    }

                    blobPlants[i] = spec;
                }

                // Create blob asset reference
                var blobAsset = builder.CreateBlobAssetReference<PlantSpecBlob>(Allocator.Persistent);
                builder.Dispose();

                // Add singleton component with blob reference
                var entity = GetEntity(TransformUsageFlags.None);
                AddComponent(entity, new PlantSpecSingleton
                {
                    Specs = blobAsset
                });
            }

            private static PlantSpec ConvertToSpec(PlantSpecData data, int index)
            {
                return new PlantSpec
                {
                    Id = data.id,
                    VariantOf = data.variantOf,
                    BiomeMask = data.biomeMask,
                    TempMin = data.tempMin,
                    TempMax = data.tempMax,
                    MoistMin = data.moistMin,
                    MoistMax = data.moistMax,
                    SunMin = data.sunMin,
                    SunMax = data.sunMax,
                    ElevMin = data.elevMin,
                    ElevMax = data.elevMax,
                    SlopeMaxDeg = data.slopeMaxDeg,
                    SoilMask = data.soilMask,
                    HazardFlags = data.hazardFlags,
                    StageSecSeedling = data.stageSecSeedling,
                    StageSecSapling = data.stageSecSapling,
                    StageSecMature = data.stageSecMature,
                    ReproSeedsPerDay = data.reproSeedsPerDay,
                    ReproRadius = data.reproRadius,
                    SeasonMask = data.seasonMask,
                    Style = new PlantStyleTokens
                    {
                        StylePalette = data.style.stylePalette,
                        StylePattern = data.style.stylePattern,
                        FamilyToken = data.style.familyToken
                    }
                };
            }

            private static PlantSpec ApplyVariantDelta(PlantSpec baseSpec, PlantSpecData variantData, int index)
            {
                // Start with base spec
                var variant = baseSpec;
                
                // Override with variant data (only non-zero/non-empty values override)
                variant.Id = variantData.id;
                variant.VariantOf = variantData.variantOf;

                if (variantData.biomeMask != 0) variant.BiomeMask = variantData.biomeMask;
                if (variantData.tempMin != 0 || variantData.tempMax != 0)
                {
                    variant.TempMin = variantData.tempMin != 0 ? variantData.tempMin : variant.TempMin;
                    variant.TempMax = variantData.tempMax != 0 ? variantData.tempMax : variant.TempMax;
                }
                if (variantData.moistMin != 0 || variantData.moistMax != 0)
                {
                    variant.MoistMin = variantData.moistMin != 0 ? variantData.moistMin : variant.MoistMin;
                    variant.MoistMax = variantData.moistMax != 0 ? variantData.moistMax : variant.MoistMax;
                }
                if (variantData.sunMin != 0 || variantData.sunMax != 0)
                {
                    variant.SunMin = variantData.sunMin != 0 ? variantData.sunMin : variant.SunMin;
                    variant.SunMax = variantData.sunMax != 0 ? variantData.sunMax : variant.SunMax;
                }
                if (variantData.elevMin != 0 || variantData.elevMax != 0)
                {
                    variant.ElevMin = variantData.elevMin != 0 ? variantData.elevMin : variant.ElevMin;
                    variant.ElevMax = variantData.elevMax != 0 ? variantData.elevMax : variant.ElevMax;
                }
                if (variantData.slopeMaxDeg != 0) variant.SlopeMaxDeg = variantData.slopeMaxDeg;
                if (variantData.soilMask != 0) variant.SoilMask = variantData.soilMask;
                variant.HazardFlags = variantData.hazardFlags; // Flags always override
                if (variantData.stageSecSeedling != 0) variant.StageSecSeedling = variantData.stageSecSeedling;
                if (variantData.stageSecSapling != 0) variant.StageSecSapling = variantData.stageSecSapling;
                if (variantData.stageSecMature != 0) variant.StageSecMature = variantData.stageSecMature;
                if (variantData.reproSeedsPerDay != 0) variant.ReproSeedsPerDay = variantData.reproSeedsPerDay;
                if (variantData.reproRadius != 0) variant.ReproRadius = variantData.reproRadius;
                if (variantData.seasonMask != 0) variant.SeasonMask = variantData.seasonMask;
                if (variantData.style.stylePalette != 0) variant.Style.StylePalette = variantData.style.stylePalette;
                if (variantData.style.stylePattern != 0) variant.Style.StylePattern = variantData.style.stylePattern;
                if (variantData.style.familyToken != 0) variant.Style.FamilyToken = variantData.style.familyToken;

                return variant;
            }
        }
    }
}
