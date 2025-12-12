#if LEGACY_PRESENTATION_ARCHIVE_ENABLED
using System.Collections.Generic;
using UnityEngine;

namespace Godgame.Presentation
{
    /// <summary>
    /// Placeholder theme asset used to list presentation tags active for this build.
    /// </summary>
    [CreateAssetMenu(fileName = "PresentationTheme", menuName = "Godgame/Presentation/Theme")]
    public sealed class PresentationTheme : ScriptableObject
    {
        [SerializeField] List<string> m_ActiveTags = new();

        public IReadOnlyList<string> ActiveTags => m_ActiveTags;
    }
}
#endif
