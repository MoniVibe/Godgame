using PureDOTS.Runtime.Components;
using Unity.Collections;

namespace Godgame.Presentation
{
    /// <summary>
    /// Centralised identifiers for Godgame presentation bindings.
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
}
