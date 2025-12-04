using Godgame.Miracles.Presentation;
using PureDOTS.Environment;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using AuthoringEnvironmentGridConfig = PureDOTS.Authoring.EnvironmentGridConfig;

namespace Godgame.Demo
{
    /// <summary>
    /// Generates ground tile ECS entities that correspond to every environment grid cell so
    /// runtime systems can sample climate data and drive swappable presentation variants.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class GroundTileAuthoring : MonoBehaviour
    {
        [Tooltip("Environment grid configuration that defines the world bounds/resolution.")]
        public AuthoringEnvironmentGridConfig gridConfig;

        [Tooltip("Optional vertical offset applied to every ground tile center.")]
        public float heightOffset = 0f;

        [Tooltip("Multiplier applied to the raw cell size when assigning LocalTransform scale.")]
        public float scaleMultiplier = 0.9f;

        [SerializeField]
        [Tooltip("Descriptor key assigned to tiles on creation before runtime systems derive biome-specific descriptors.")]
        private string defaultDescriptor = "ground.default";

        public string DefaultDescriptor => string.IsNullOrWhiteSpace(defaultDescriptor)
            ? "ground.default"
            : defaultDescriptor;
    }

    public sealed class GroundTileAuthoringBaker : Baker<GroundTileAuthoring>
    {
        public override void Bake(GroundTileAuthoring authoring)
        {
            if (authoring.gridConfig == null)
            {
                Debug.LogWarning("GroundTileAuthoring requires an EnvironmentGridConfig asset.", authoring);
                return;
            }

            var configData = authoring.gridConfig.ToComponent();
            var metadata = ResolvePrimaryMetadata(in configData);
            var resolution = metadata.Resolution;

            if (resolution.x <= 0 || resolution.y <= 0)
            {
                Debug.LogWarning("GroundTileAuthoring grid resolution is invalid.", authoring);
                return;
            }

            var descriptor = SanitizeDescriptor(authoring.DefaultDescriptor);
            if (descriptor.Length == 0)
            {
                descriptor = "ground.default";
            }

            var descriptorHash = Animator.StringToHash(descriptor);
            if (descriptorHash == 0)
            {
                descriptorHash = Animator.StringToHash("ground.default");
                descriptor = "ground.default";
            }

            FixedString64Bytes descriptorKey = descriptor;
            float scale = math.max(0.1f, metadata.CellSize * math.max(0.01f, authoring.scaleMultiplier));

            var width = resolution.x;
            var height = resolution.y;

            for (int z = 0; z < height; z++)
            {
                for (int x = 0; x < width; x++)
                {
                    int index = z * width + x;
                    var center = EnvironmentGridMath.GetCellCenter(metadata, index);
                    center.y += authoring.heightOffset;

                    var entity = CreateAdditionalEntity(TransformUsageFlags.Dynamic);
                    AddComponent(entity, LocalTransform.FromPositionRotationScale(center, quaternion.identity, scale));
                    AddComponent(entity, new GroundTile
                    {
                        CellIndex = index,
                        CellCoord = new int2(x, z),
                        Center = center,
                        CellSize = metadata.CellSize
                    });
                    AddComponent(entity, new GroundMoisture { Value = 0f });
                    AddComponent(entity, new GroundTemperature { Value = 0f });
                    AddComponent(entity, new GroundTerrainHeight { Value = 0f });
                    AddComponent(entity, GroundBiome.FromBiome(BiomeType.Unknown));

                    AddComponent(entity, new SwappablePresentationBinding
                    {
                        DescriptorKey = descriptorKey,
                        DescriptorHash = descriptorHash
                    });

                    AddComponent<SwappablePresentationDirtyTag>(entity);
                }
            }
        }

        private static EnvironmentGridMetadata ResolvePrimaryMetadata(in EnvironmentGridConfigData configData)
        {
            if (configData.BiomeEnabled != 0 && configData.Biome.CellCount > 0)
            {
                return configData.Biome;
            }

            if (configData.Moisture.CellCount > 0)
            {
                return configData.Moisture;
            }

            if (configData.Temperature.CellCount > 0)
            {
                return configData.Temperature;
            }

            if (configData.Sunlight.CellCount > 0)
            {
                return configData.Sunlight;
            }

            if (configData.Wind.CellCount > 0)
            {
                return configData.Wind;
            }

            return configData.Moisture;
        }

        private static string SanitizeDescriptor(string descriptor)
        {
            return string.IsNullOrWhiteSpace(descriptor)
                ? "ground.default"
                : descriptor.Trim().ToLowerInvariant();
        }
    }
}
