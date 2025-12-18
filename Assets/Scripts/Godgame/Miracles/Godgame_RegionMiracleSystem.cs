using Godgame.Presentation;
using PureDOTS.Environment;
using PureDOTS.Runtime.Components;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using MiracleType = PureDOTS.Runtime.Components.MiracleType;

namespace Godgame.Miracles
{
    /// <summary>
    /// System that applies region miracles to biome/vegetation.
    /// Updates BiomePresentationData (fertility, moisture) and VegetationVisualState (health).
    /// </summary>
    [BurstCompile]
    [UpdateInGroup(typeof(Unity.Entities.PresentationSystemGroup))]
    [UpdateAfter(typeof(Godgame_MiraclePresentationSystem))]
    public partial struct Godgame_RegionMiracleSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<RegionMiracleEffect>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            float deltaTime = SystemAPI.Time.DeltaTime;
            var ecb = new EntityCommandBuffer(Allocator.Temp);

            // Process active region miracles
            foreach (var (effectRef, entity) in SystemAPI.Query<RefRW<RegionMiracleEffect>>().WithEntityAccess())
            {
                ref var effect = ref effectRef.ValueRW;

                effect.RemainingDuration -= deltaTime;
                effect.Intensity = math.saturate(effect.RemainingDuration / effect.Duration);

                // Apply effects to biome/vegetation in region
                ApplyRegionMiracleEffects(ref state, effect);

                if (effect.RemainingDuration <= 0)
                {
                    ecb.DestroyEntity(entity);
                }
            }

            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }

        [BurstCompile]
        private void ApplyRegionMiracleEffects(ref SystemState state, RegionMiracleEffect effect)
        {
            // Find ground tiles/biomes within radius
            foreach (var (biomeDataRef, tile, transform) in SystemAPI.Query<RefRW<BiomePresentationData>, RefRO<Godgame.Scenario.GroundTile>, RefRO<Unity.Transforms.LocalTransform>>())
            {
                float distance = math.distance(transform.ValueRO.Position, effect.CenterPosition);
                if (distance <= effect.Radius)
                {
                    ref var biomeData = ref biomeDataRef.ValueRW;
                    float influence = 1f - (distance / effect.Radius); // 1 at center, 0 at edge

                    // Apply miracle effects based on type
                    switch (effect.Type)
                    {
                        case MiracleType.BlessRegion:
                            // Increase fertility and moisture
                            biomeData.Fertility = math.min(100f, biomeData.Fertility + influence * effect.Intensity * 10f);
                            biomeData.Moisture = math.min(100f, biomeData.Moisture + influence * effect.Intensity * 5f);
                            break;

                        case MiracleType.CurseRegion:
                            // Decrease fertility and moisture
                            biomeData.Fertility = math.max(0f, biomeData.Fertility - influence * effect.Intensity * 10f);
                            biomeData.Moisture = math.max(0f, biomeData.Moisture - influence * effect.Intensity * 5f);
                            break;

                        case MiracleType.RestoreBiome:
                            // Restore fertility and moisture to healthy levels
                            biomeData.Fertility = math.lerp(biomeData.Fertility, 70f, influence * effect.Intensity * 0.1f);
                            biomeData.Moisture = math.lerp(biomeData.Moisture, 60f, influence * effect.Intensity * 0.1f);
                            break;
                    }
                }
            }

            // Apply to vegetation health
            foreach (var (vegetationStateRef, transform) in SystemAPI.Query<RefRW<VegetationVisualState>, RefRO<Unity.Transforms.LocalTransform>>().WithAll<VegetationPresentationTag>())
            {
                float distance = math.distance(transform.ValueRO.Position, effect.CenterPosition);
                if (distance <= effect.Radius)
                {
                    ref var vegetationState = ref vegetationStateRef.ValueRW;
                    float influence = 1f - (distance / effect.Radius);

                    switch (effect.Type)
                    {
                        case MiracleType.BlessRegion:
                        case MiracleType.RestoreBiome:
                            // Increase health
                            vegetationState.Health = math.min(1f, vegetationState.Health + influence * effect.Intensity * 0.1f);
                            break;

                        case MiracleType.CurseRegion:
                            // Decrease health
                            vegetationState.Health = math.max(0f, vegetationState.Health - influence * effect.Intensity * 0.1f);
                            break;
                    }
                }
            }
        }
    }
}

