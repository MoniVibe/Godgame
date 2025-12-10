using PureDOTS.Runtime.Components;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace Godgame.Presentation
{
    /// <summary>
    /// Legacy presentation identifiers used by existing gameplay systems.
    /// </summary>
    public static class GodgamePresentationIds
    {
        public const int MiraclePingEffectId = 1001;
        public const int JobsiteGhostEffectId = 1002;
        public const int ModuleRefitSparksEffectId = 1003;
        public const int HandAffordanceEffectId = 1004;

        public static readonly PresentationStyleOverride MiraclePingStyle = default;
        public static readonly PresentationStyleOverride JobsiteGhostStyle = default;
        public static readonly PresentationStyleOverride ModuleRefitSparksStyle = default;
        public static readonly PresentationStyleOverride HandAffordanceStyle = default;
    }

    /// <summary>
    /// Minimal HUD metrics display placeholder to unblock compilation.
    /// </summary>
    public sealed class PresentationMetricsDisplay : MonoBehaviour
    {
        private void Awake()
        {
            enabled = false;
        }
    }
}

