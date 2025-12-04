using Godgame.Environment;
using PureDOTS.Environment;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace Godgame.Miracles.Presentation
{
    /// <summary>
    /// Applies environment-aware descriptor overrides so miracle visuals respond to biomes + time of day.
    /// </summary>
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public partial class MiraclePresentationSystem : SystemBase
    {
        private ComponentLookup<LocalToWorld> _localToWorldLookup;

        protected override void OnCreate()
        {
            RequireForUpdate<MiraclePresentationBinding>();
            _localToWorldLookup = GetComponentLookup<LocalToWorld>(true);
        }

        protected override void OnUpdate()
        {
            _localToWorldLookup.Update(this);

            var hasClimate = SystemAPI.TryGetSingleton(out ClimateState climateState);
            var hasBiome = SystemAPI.TryGetSingleton(out BiomeGrid biomeGrid);
            var ecb = new EntityCommandBuffer(Allocator.Temp);

            foreach (var (bindingRW, swapRW, entity) in SystemAPI
                         .Query<RefRW<MiraclePresentationBinding>, RefRW<SwappablePresentationBinding>>()
                         .WithEntityAccess())
            {
                var profile = bindingRW.ValueRO.Profile.Value;
                if (profile == null)
                {
                    continue;
                }

                var descriptor = bindingRW.ValueRO.BaseDescriptor;
                var variantSuffix = string.Empty;
                var variantIntensity = 1f;

                var timeOfDay = hasClimate ? climateState.TimeOfDayHours : 12f;
                var biome = BiomeType.Unknown;

                if (hasBiome && _localToWorldLookup.HasComponent(entity))
                {
                    biome = biomeGrid.SampleNearest(_localToWorldLookup[entity].Position);
                }

                if (TryPickVariant(profile, biome, timeOfDay, out var variant))
                {
                    variantSuffix = variant.descriptorSuffix;
                    variantIntensity = variant.intensityMultiplier;
                }

                var finalDescriptor = BuildDescriptor(descriptor, variantSuffix);
                var finalHash = ComputeHash(finalDescriptor);

                if (!swapRW.ValueRO.DescriptorKey.Equals(finalDescriptor) || swapRW.ValueRO.DescriptorHash != finalHash)
                {
                    swapRW.ValueRW.DescriptorKey = finalDescriptor;
                    swapRW.ValueRW.DescriptorHash = finalHash;

                    if (!SystemAPI.HasComponent<SwappablePresentationDirtyTag>(entity))
                    {
                        ecb.AddComponent<SwappablePresentationDirtyTag>(entity);
                    }
                }

                if (math.abs(bindingRW.ValueRO.LastIntensity - variantIntensity) > 0.01f)
                {
                    bindingRW.ValueRW.LastIntensity = variantIntensity;
                }

                bindingRW.ValueRW.LastDescriptorHash = finalHash;
            }

            ecb.Playback(EntityManager);
            ecb.Dispose();
        }

        private static bool TryPickVariant(MiracleVisualProfile profile, BiomeType biome, float timeOfDay, out MiracleVisualProfile.EnvironmentVariant variant)
        {
            var entries = profile.Variants;
            for (int i = 0; i < entries.Count; i++)
            {
                var candidate = entries[i];
                if (candidate == null)
                {
                    continue;
                }

                if (!candidate.allowedBiomes.Allows(biome))
                {
                    continue;
                }

                if (!TimeOfDayWindowUtility.IsActive(candidate.timeWindow.ToData(), timeOfDay))
                {
                    continue;
                }

                variant = candidate;
                return true;
            }

            variant = null;
            return false;
        }

        private static FixedString64Bytes BuildDescriptor(in FixedString64Bytes baseDescriptor, string suffix)
        {
            if (string.IsNullOrWhiteSpace(suffix))
            {
                return baseDescriptor;
            }

            var baseString = baseDescriptor.ToString();
            if (string.IsNullOrEmpty(baseString))
            {
                baseString = suffix.Trim();
            }
            else
            {
                baseString = $"{baseString}.{suffix.Trim()}";
            }

            FixedString64Bytes result = default;
            result.CopyFromTruncated(baseString);
            return result;
        }

        private static int ComputeHash(in FixedString64Bytes descriptor)
        {
            var key = descriptor.ToString();
            return string.IsNullOrEmpty(key) ? 0 : Animator.StringToHash(key);
        }
    }
}
