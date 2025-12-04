#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Godgame.Editor
{
    /// <summary>
    /// Warn once per editor session when the external PureDOTS package folder is missing.
    /// Godgame relies on com.moni.puredots (TimeState/RewindState/registry types) via a file reference in manifest.json.
    /// </summary>
    [InitializeOnLoad]
    internal static class PureDotsDependencyChecker
    {
        private const string SessionKey = "Godgame.PureDotsDependencyChecker.Warned";

        static PureDotsDependencyChecker()
        {
            // Dependency check disabled – the project now uses a manually configured
            // local package path in Packages/manifest.json.
            // If you want this back later, make it read the manifest instead of hardcoding paths.
            // Godgame.Debug.LogWarning("PureDOTS dependency check disabled.");
            
            // Original check code (commented out):
            // if (SessionState.GetBool(SessionKey, false))
            // {
            //     return;
            // }
            //
            // var projectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
            // var pureDotsPath = Path.GetFullPath(Path.Combine(projectRoot, "..", "..", "PureDOTS", "Packages", "com.moni.puredots"));
            //
            // if (!Directory.Exists(pureDotsPath))
            // {
            //     SessionState.SetBool(SessionKey, true);
            //
            //     Debug.LogWarning(
            //         $"PureDOTS package not found at '{pureDotsPath}'. " +
            //         "manifest.json points to file:../../PureDOTS/Packages/com.moni.puredots — clone the PureDOTS repo as a sibling to Godgame or update the package path so shared types resolve before running tests.");
            // }
        }
    }
}
#endif
