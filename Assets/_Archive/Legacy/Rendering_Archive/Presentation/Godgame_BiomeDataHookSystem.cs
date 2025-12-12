#if LEGACY_PRESENTATION_ARCHIVE_ENABLED
using PureDOTS.Environment;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace Godgame.Presentation
{
    /// <summary>
    /// System that reads GroundBiome, GroundMoisture, GroundTemperature from GroundTile entities
    /// and writes BiomePresentationData for presentation systems.
    /// Also samples ClimateState singleton for global climate data.
    /// </summary>
    [BurstCompile]
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    [UpdateAfter(typeof(Godgame.Demo.GroundTileDescriptorSystem))]
    public partial struct Godgame_BiomeDataHookSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<Godgame.Demo.GroundTile>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            // Get global climate state if available
            float globalTemperature = 20f; // Default
            if (SystemAPI.TryGetSingleton<ClimateState>(out var climateState))
            {
                globalTemperature = climateState.GlobalTemperature;
            }

            var job = new UpdateBiomePresentationDataJob
            {
                GlobalTemperature = globalTemperature
            };
            job.ScheduleParallel();
        }
    }

    /// <summary>
    /// Job that updates BiomePresentationData from GroundTile components.
    /// </summary>
    [BurstCompile]
    public partial struct UpdateBiomePresentationDataJob : IJobEntity
    {
        public float GlobalTemperature;

        public void Execute(
            ref BiomePresentationData presentationData,
            in Godgame.Demo.GroundBiome biome,
            in Godgame.Demo.GroundMoisture moisture,
            in Godgame.Demo.GroundTemperature temperature)
        {
            // Update presentation data
            presentationData.Biome = biome.Biome;
            presentationData.Moisture = moisture.Value;
            presentationData.Temperature = temperature.Value;
            
            // Calculate fertility (0-100) from moisture and temperature
            // Optimal: high moisture (50-80), moderate temperature (15-25°C)
            float moistureFactor = math.saturate(moisture.Value / 100f);
            float tempOptimal = 20f; // Optimal temperature
            float tempDeviation = math.abs(temperature.Value - tempOptimal);
            float tempFactor = math.saturate(1f - (tempDeviation / 30f)); // 30°C deviation = 0 factor
            
            presentationData.Fertility = (moistureFactor * 0.6f + tempFactor * 0.4f) * 100f;
            
            // Clear dirty flag
            presentationData.IsDirty = 0;
        }
    }
}
#endif
