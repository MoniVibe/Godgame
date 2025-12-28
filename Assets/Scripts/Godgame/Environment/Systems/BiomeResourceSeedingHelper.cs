using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace Godgame.Environment.Systems
{
    /// <summary>
    /// Helper utilities for biome-based resource seeding.
    /// Provides methods to get resource bias weights from current biome.
    /// </summary>
    public static class BiomeResourceSeedingHelper
    {
        /// <summary>
        /// Gets the resource bias weight for a given resource type from the current biome.
        /// </summary>
        /// <param name="biomeDefs">Biome definition singleton</param>
        /// <param name="biomeGrid">Current biome grid</param>
        /// <param name="resourceTypeIndex">Resource type index (1=Wood, 2=Ore, etc.)</param>
        /// <returns>Bias weight (0-1000), or 500 if biome not found (neutral)</returns>
        public static ushort GetResourceBias(
            in BiomeDefinitionSingleton biomeDefs,
            in BiomeGrid biomeGrid,
            ushort resourceTypeIndex)
        {
            if (!biomeDefs.Definitions.IsCreated || !biomeGrid.BiomeIds.IsCreated)
            {
                return 500; // Neutral default
            }

            if (biomeGrid.Width == 0 || biomeGrid.Height == 0)
            {
                return 500;
            }

            // Get current biome ID (1×1 grid for MVP)
            uint currentBiomeId = biomeGrid.BiomeIds.Value[0];

            // Find biome profile
            ref var profiles = ref biomeDefs.Definitions.Value.Profiles;
            for (int i = 0; i < profiles.Length; i++)
            {
                if (profiles[i].BiomeId32 == currentBiomeId)
                {
                    // Map resource type index to bias field
                    // 1 = Wood, 2 = Ore (based on ResourceType enum)
                    if (resourceTypeIndex == 1) // Wood
                    {
                        return profiles[i].ResourceBiasWood;
                    }
                    else if (resourceTypeIndex == 2) // Ore
                    {
                        return profiles[i].ResourceBiasOre;
                    }
                    // Future: add more resource types
                    break;
                }
            }

            return 500; // Neutral default if biome or resource type not found
        }

        /// <summary>
        /// Calculates spawn probability multiplier based on biome resource bias.
        /// </summary>
        /// <param name="bias">Resource bias weight (0-1000)</param>
        /// <param name="baseProbability">Base spawn probability (0-1)</param>
        /// <returns>Adjusted probability (0-1)</returns>
        public static float CalculateSpawnProbability(ushort bias, float baseProbability)
        {
            // Convert bias (0-1000) to multiplier (0.0-2.0)
            // 500 = neutral (1.0x), 1000 = double (2.0x), 0 = no spawn (0.0x)
            float multiplier = bias / 500f;
            return math.clamp(baseProbability * multiplier, 0f, 1f);
        }
    }
}
