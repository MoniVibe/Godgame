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
        /// Builds the PlantSpec blob from the serialized catalog data.
        /// </summary>
        private struct PlantSpecBuildData
        {
            public FixedString64Bytes Id;
            public FixedString64Bytes VariantOf;
            public uint BiomeMask;
            public float TempMin;
            public float TempMax;
            public float MoistMin;
            public float MoistMax;
            public float SunMin;
            public float SunMax;
            public float ElevMin;
            public float ElevMax;
            public float SlopeMaxDeg;
            public uint SoilMask;
            public PlantHazardFlags HazardFlags;
            public float StageSecSeedling;
            public float StageSecSapling;
            public float StageSecMature;
            public float ReproSeedsPerDay;
            public float ReproRadius;
            public uint SeasonMask;
            public PlantStyleTokens Style;
            public List<PlantLootEntry> Yields;
        }

        public bool TryBuildBlob(out BlobAssetReference<PlantSpecBlob> blobAsset)
        {
            blobAsset = default;
            if (plants == null || plants.Length == 0)
            {
#if UNITY_EDITOR
                Debug.LogWarning($"PlantSpecCatalogAuthoring '{name}' has no plants defined.");
#endif
                return false;
            }

            var resolvedSpecs = new List<PlantSpecBuildData>();
            var specLookup = new Dictionary<string, PlantSpecBuildData>();

            for (int i = 0; i < plants.Length; i++)
            {
                var plantData = plants[i];
                if (string.IsNullOrEmpty(plantData.variantOf))
                {
                    var spec = ConvertToBuildData(plantData);
                    spec.Yields = BuildYieldList(plantData.yields);
                    resolvedSpecs.Add(spec);
                    specLookup[plantData.id] = spec;
                }
            }

            for (int i = 0; i < plants.Length; i++)
            {
                var plantData = plants[i];
                if (string.IsNullOrEmpty(plantData.variantOf))
                    continue;

                if (specLookup.TryGetValue(plantData.variantOf, out var baseSpec))
                {
                    var variantSpec = ApplyVariantDelta(baseSpec, plantData);
                    variantSpec.Yields = BuildYieldList(plantData.yields, baseSpec.Yields);
                    resolvedSpecs.Add(variantSpec);
                    specLookup[plantData.id] = variantSpec;
                }
                else
                {
#if UNITY_EDITOR
                    Debug.LogWarning($"Plant '{plantData.id}' references unknown VariantOf '{plantData.variantOf}'. Creating as base spec.");
#endif
                    var spec = ConvertToBuildData(plantData);
                    spec.Yields = BuildYieldList(plantData.yields);
                    resolvedSpecs.Add(spec);
                    specLookup[plantData.id] = spec;
                }
            }

            var builder = new BlobBuilder(Allocator.Temp);
            ref var blobRoot = ref builder.ConstructRoot<PlantSpecBlob>();
            var blobPlants = builder.Allocate(ref blobRoot.Plants, resolvedSpecs.Count);

            for (int i = 0; i < resolvedSpecs.Count; i++)
            {
                var source = resolvedSpecs[i];
                ref PlantSpec spec = ref blobPlants[i];
                CopyToPlantSpec(ref spec, source);

                if (source.Yields != null && source.Yields.Count > 0)
                {
                    var yieldsArray = builder.Allocate(ref spec.Yields, source.Yields.Count);
                    for (int j = 0; j < source.Yields.Count; j++)
                    {
                        yieldsArray[j] = source.Yields[j];
                    }
                }
                else
                {
                    builder.Allocate(ref spec.Yields, 0);
                }
            }

            blobAsset = builder.CreateBlobAssetReference<PlantSpecBlob>(Allocator.Persistent);
            builder.Dispose();
            return true;
        }

        private static List<PlantLootEntry> BuildYieldList(PlantLootEntryData[] entries, List<PlantLootEntry> inherit = null)
        {
            if (entries != null && entries.Length > 0)
            {
                var yields = new List<PlantLootEntry>(entries.Length);
                for (int i = 0; i < entries.Length; i++)
                {
                    yields.Add(new PlantLootEntry
                    {
                        MaterialId = entries[i].materialId,
                        Weight = entries[i].weight,
                        MinAmount = entries[i].minAmount,
                        MaxAmount = entries[i].maxAmount
                    });
                }
                return yields;
            }

            if (inherit != null && inherit.Count > 0)
            {
                return new List<PlantLootEntry>(inherit);
            }

            return null;
        }

        private static PlantSpecBuildData ConvertToBuildData(PlantSpecData data)
        {
            return new PlantSpecBuildData
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
                },
                Yields = null
            };
        }

        private static PlantSpecBuildData ApplyVariantDelta(PlantSpecBuildData baseSpec, PlantSpecData variantData)
        {
            var variant = baseSpec;
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
            variant.HazardFlags = variantData.hazardFlags;
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

        private static void CopyToPlantSpec(ref PlantSpec target, in PlantSpecBuildData source)
        {
            target.Id = source.Id;
            target.VariantOf = source.VariantOf;
            target.BiomeMask = source.BiomeMask;
            target.TempMin = source.TempMin;
            target.TempMax = source.TempMax;
            target.MoistMin = source.MoistMin;
            target.MoistMax = source.MoistMax;
            target.SunMin = source.SunMin;
            target.SunMax = source.SunMax;
            target.ElevMin = source.ElevMin;
            target.ElevMax = source.ElevMax;
            target.SlopeMaxDeg = source.SlopeMaxDeg;
            target.SoilMask = source.SoilMask;
            target.HazardFlags = source.HazardFlags;
            target.StageSecSeedling = source.StageSecSeedling;
            target.StageSecSapling = source.StageSecSapling;
            target.StageSecMature = source.StageSecMature;
            target.ReproSeedsPerDay = source.ReproSeedsPerDay;
            target.ReproRadius = source.ReproRadius;
            target.SeasonMask = source.SeasonMask;
            target.Style = source.Style;
        }
    }

    [DisallowMultipleComponent]
    public sealed class PlantSpecCatalogAuthoringComponent : MonoBehaviour
    {
        [SerializeField] private PlantSpecCatalogAuthoring catalog;

        public sealed class Baker : Unity.Entities.Baker<PlantSpecCatalogAuthoringComponent>
        {
            public override void Bake(PlantSpecCatalogAuthoringComponent authoringComponent)
            {
                if (authoringComponent.catalog == null)
                {
#if UNITY_EDITOR
                    Debug.LogWarning("[PlantSpecCatalog] No PlantSpecCatalogAuthoring asset assigned.");
#endif
                    return;
                }

                if (!authoringComponent.catalog.TryBuildBlob(out var blobAsset))
                {
                    return;
                }

                var entity = GetEntity(TransformUsageFlags.None);
                AddComponent(entity, new PlantSpecSingleton
                {
                    Specs = blobAsset
                });
            }
        }
    }
}
