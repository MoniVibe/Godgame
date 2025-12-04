using UnityEngine;

namespace Godgame.Presentation
{
    /// <summary>
    /// Placeholder bootstrapper so presentation prefabs load without missing script warnings.
    /// </summary>
    public sealed class GodgamePresentationRuntimeBootstrap : MonoBehaviour
    {
        [SerializeField] GodgamePresentationRuntimeConfig config;
        [SerializeField] PresentationTheme themeOverride;

        public GodgamePresentationRuntimeConfig Config => config;
        public PresentationTheme ThemeOverride => themeOverride;
    }
}
