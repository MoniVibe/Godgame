using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace Godgame.Environment.Systems
{
    /// <summary>
    /// Resolves which biome should be active based on current climate conditions.
    /// Uses best-match scoring to select the most appropriate biome.
    /// </summary>
    [UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
    [BurstCompile]
    public partial struct BiomeResolveSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<BiomeDefinitionSingleton>();
            state.RequireForUpdate<ClimateState>();
            state.RequireForUpdate<BiomeGrid>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var biomeDefs = SystemAPI.GetSingleton<BiomeDefinitionSingleton>();
            if (!biomeDefs.Definitions.IsCreated)
            {
                return;
            }

            var climate = SystemAPI.GetSingleton<ClimateState>();
            ref var biomeGrid = ref SystemAPI.GetSingletonRW<BiomeGrid>().ValueRW;

            // Find best-matching biome
            uint chosenBiomeId = ChooseBiomeId(
                in biomeDefs.Definitions,
                climate.TempC,
                climate.Moisture01,
                climate.ElevationMean);

            // Update biome grid (1×1 for MVP)
            if (biomeGrid.Width != 1 || biomeGrid.Height != 1)
            {
                return;
            }

            if (biomeGrid.BiomeIds.IsCreated &&
                biomeGrid.BiomeIds.Value.Length == 1 &&
                biomeGrid.BiomeIds.Value[0] == chosenBiomeId)
            {
                return;
            }

            using var builder = new BlobBuilder(Allocator.Temp);
            ref var root = ref builder.ConstructRoot<BlobArray<uint>>();
            var biomeArray = builder.Allocate(ref root, 1);
            biomeArray[0] = chosenBiomeId;
            var newBlob = builder.CreateBlobAssetReference<BlobArray<uint>>(Allocator.Persistent);

            if (biomeGrid.BiomeIds.IsCreated)
            {
                biomeGrid.BiomeIds.Dispose();
            }

            biomeGrid.BiomeIds = newBlob;
        }

        [BurstCompile]
        internal static uint ChooseBiomeId(
            in BlobAssetReference<BiomeDefinitionBlob> definitions,
            float tempC,
            float moisture01,
            float elevation)
        {
            if (!definitions.IsCreated)
            {
                return 1; // Default to first biome
            }

            ref var profiles = ref definitions.Value.Profiles;
            if (profiles.Length == 0)
            {
                return 1;
            }

            float bestScore = float.MinValue;
            uint bestBiomeId = 1;

            // Score each biome based on how well it matches current conditions
            for (int i = 0; i < profiles.Length; i++)
            {
                ref var profile = ref profiles[i];

                // Check if conditions are within range
                bool inTempRange = tempC >= profile.TempMin && tempC <= profile.TempMax;
                bool inMoistureRange = moisture01 >= profile.MoistMin && moisture01 <= profile.MoistMax;
                bool inElevationRange = elevation >= profile.ElevMin && elevation <= profile.ElevMax;

                // If outside all ranges, skip this biome
                if (!inTempRange && !inMoistureRange && !inElevationRange)
                {
                    continue;
                }

                // Calculate match score (higher = better match)
                float score = 0f;

                // Temperature match (closer to center of range = higher score)
                if (inTempRange)
                {
                    float tempCenter = (profile.TempMin + profile.TempMax) * 0.5f;
                    float tempRange = profile.TempMax - profile.TempMin;
                    float tempDistance = math.abs(tempC - tempCenter) / math.max(tempRange, 0.1f);
                    score += (1f - math.min(tempDistance, 1f)) * 0.4f; // 40% weight
                }

                // Moisture match
                if (inMoistureRange)
                {
                    float moistCenter = (profile.MoistMin + profile.MoistMax) * 0.5f;
                    float moistRange = profile.MoistMax - profile.MoistMin;
                    float moistDistance = math.abs(moisture01 - moistCenter) / math.max(moistRange, 0.01f);
                    score += (1f - math.min(moistDistance, 1f)) * 0.4f; // 40% weight
                }

                // Elevation match
                if (inElevationRange)
                {
                    float elevCenter = (profile.ElevMin + profile.ElevMax) * 0.5f;
                    float elevRange = profile.ElevMax - profile.ElevMin;
                    float elevDistance = math.abs(elevation - elevCenter) / math.max(elevRange, 0.1f);
                    score += (1f - math.min(elevDistance, 1f)) * 0.2f; // 20% weight
                }

                // Prefer biomes that match more conditions
                if (inTempRange) score += 0.1f;
                if (inMoistureRange) score += 0.1f;
                if (inElevationRange) score += 0.1f;

                if (score > bestScore)
                {
                    bestScore = score;
                    bestBiomeId = profile.BiomeId32;
                }
            }

            // If no biome matched, default to first biome
            if (bestScore == float.MinValue)
            {
                return 1;
            }

            return bestBiomeId;
        }
    }
}

