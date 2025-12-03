using Godgame.Environment;
using Godgame.Presentation.Bindings;
using NUnit.Framework;
using Unity.Collections;
using Unity.Entities;

namespace Godgame.Tests.Environment
{
    /// <summary>
    /// Tests that all BiomeIds in definition exist in both Minimal and Fancy binding sets.
    /// </summary>
    [TestFixture]
    public class BiomeBindingParityTests
    {
        [Test]
        public void BiomeBindings_HaveParity_WithDefinitions()
        {
            // Create test biome definitions
            var defBuilder = new BlobBuilder(Allocator.Temp);
            ref var defRoot = ref defBuilder.ConstructRoot<BiomeDefinitionBlob>();
            var defProfiles = defBuilder.Allocate(ref defRoot.Profiles, 3);
            defProfiles[0] = new BiomeProfile { BiomeId32 = 1, Id = "Temperate" };
            defProfiles[1] = new BiomeProfile { BiomeId32 = 2, Id = "Grasslands" };
            defProfiles[2] = new BiomeProfile { BiomeId32 = 3, Id = "Mountains" };
            var defBlob = defBuilder.CreateBlobAssetReference<BiomeDefinitionBlob>(Allocator.Persistent);
            defBuilder.Dispose();

            // Create Minimal bindings
            var minBuilder = new BlobBuilder(Allocator.Temp);
            ref var minRoot = ref minBuilder.ConstructRoot<BiomeBindingBlob>();
            var minEntries = minBuilder.Allocate(ref minRoot.Entries, 3);
            minEntries[0] = new BiomeBindingEntry { BiomeId32 = 1, GroundStyle = 1, WeatherProfile = 1, MinimapPalette = 1 };
            minEntries[1] = new BiomeBindingEntry { BiomeId32 = 2, GroundStyle = 2, WeatherProfile = 2, MinimapPalette = 2 };
            minEntries[2] = new BiomeBindingEntry { BiomeId32 = 3, GroundStyle = 3, WeatherProfile = 3, MinimapPalette = 3 };
            var minBlob = minBuilder.CreateBlobAssetReference<BiomeBindingBlob>(Allocator.Persistent);
            minBuilder.Dispose();

            // Create Fancy bindings
            var fancyBuilder = new BlobBuilder(Allocator.Temp);
            ref var fancyRoot = ref fancyBuilder.ConstructRoot<BiomeBindingBlob>();
            var fancyEntries = fancyBuilder.Allocate(ref fancyRoot.Entries, 3);
            fancyEntries[0] = new BiomeBindingEntry { BiomeId32 = 1, GroundStyle = 10, WeatherProfile = 10, MinimapPalette = 10 };
            fancyEntries[1] = new BiomeBindingEntry { BiomeId32 = 2, GroundStyle = 20, WeatherProfile = 20, MinimapPalette = 20 };
            fancyEntries[2] = new BiomeBindingEntry { BiomeId32 = 3, GroundStyle = 30, WeatherProfile = 30, MinimapPalette = 30 };
            var fancyBlob = fancyBuilder.CreateBlobAssetReference<BiomeBindingBlob>(Allocator.Persistent);
            fancyBuilder.Dispose();

            // Verify parity: all biome IDs in definition exist in both binding sets
            ref var defProfilesRef = ref defBlob.Value.Profiles;
            ref var minEntriesRef = ref minBlob.Value.Entries;
            ref var fancyEntriesRef = ref fancyBlob.Value.Entries;

            for (int i = 0; i < defProfilesRef.Length; i++)
            {
                uint biomeId = defProfilesRef[i].BiomeId32;

                // Check Minimal bindings
                bool foundInMinimal = false;
                for (int j = 0; j < minEntriesRef.Length; j++)
                {
                    if (minEntriesRef[j].BiomeId32 == biomeId)
                    {
                        foundInMinimal = true;
                        break;
                    }
                }
                Assert.IsTrue(foundInMinimal, $"Biome ID {biomeId} ({defProfilesRef[i].Id}) should exist in Minimal bindings");

                // Check Fancy bindings
                bool foundInFancy = false;
                for (int j = 0; j < fancyEntriesRef.Length; j++)
                {
                    if (fancyEntriesRef[j].BiomeId32 == biomeId)
                    {
                        foundInFancy = true;
                        break;
                    }
                }
                Assert.IsTrue(foundInFancy, $"Biome ID {biomeId} ({defProfilesRef[i].Id}) should exist in Fancy bindings");
            }

            defBlob.Dispose();
            minBlob.Dispose();
            fancyBlob.Dispose();
        }
    }
}

