using System;
using System.Collections.Generic;
using Godgame.Environment;
using PureDOTS.Runtime.Components;
using UnityEngine;
using MiracleType = PureDOTS.Runtime.Components.MiracleType;

namespace Godgame.Miracles.Presentation
{
    /// <summary>
    /// Maps miracle types to descriptor keys + environment-aware overrides so designers can drive VFX via ScriptableObjects.
    /// </summary>
    [CreateAssetMenu(menuName = "Godgame/Miracles/Visual Profile", fileName = "MiracleVisualProfile")]
    public sealed class MiracleVisualProfile : ScriptableObject
    {
        [Serializable]
        public sealed class EnvironmentVariant
        {
            [Tooltip("Friendly identifier for the inspector.")]
            public string id = "default";

            [Tooltip("Suffix appended to the base descriptor (e.g., godgame.miracle.rain.night). Leave empty to reuse base descriptor.")]
            public string descriptorSuffix = "default";

            [Tooltip("Optional override for the VFX Graph asset key consumed by MCP tooling.")]
            public string vfxGraphId;

            [Tooltip("Target biomes for this variant.")]
            public BiomeMask allowedBiomes = BiomeMask.All;

            [Tooltip("Time-of-day filter.")]
            public TimeOfDayWindow timeWindow = TimeOfDayWindow.Always;

            [Tooltip("Scales intensity/brightness when this variant is active.")]
            public float intensityMultiplier = 1f;
        }

        [SerializeField] private MiracleType miracleType = MiracleType.None; // TODO: Replace Rain with valid PureDOTS miracle type
        [SerializeField] private string baseDescriptor = "godgame.miracle.rain";
        [SerializeField] private List<EnvironmentVariant> variants = new();

        public MiracleType MiracleType => miracleType;
        public string BaseDescriptor => baseDescriptor;
        public IReadOnlyList<EnvironmentVariant> Variants => variants;
    }
}
