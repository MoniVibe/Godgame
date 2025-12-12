#if LEGACY_PRESENTATION_ARCHIVE_ENABLED
using System.Collections.Generic;
using UnityEngine;

namespace Godgame.Presentation
{
    /// <summary>
    /// Placeholder runtime config used by the presentation bootstrapper.
    /// </summary>
    [CreateAssetMenu(fileName = "GodgamePresentationRuntimeConfig", menuName = "Godgame/Presentation/Runtime Config")]
    public sealed class GodgamePresentationRuntimeConfig : ScriptableObject
    {
        [SerializeField] PresentationRegistry m_Registry;
        [SerializeField] PresentationTheme m_DefaultTheme;
        [SerializeField] List<PresentationVariantSet> m_VariantSets = new();

        public PresentationRegistry Registry => m_Registry;
        public PresentationTheme DefaultTheme => m_DefaultTheme;
        public IReadOnlyList<PresentationVariantSet> VariantSets => m_VariantSets;
    }
}
#endif
