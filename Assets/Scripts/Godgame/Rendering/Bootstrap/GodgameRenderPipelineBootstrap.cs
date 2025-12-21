#if UNITY_EDITOR || DEVELOPMENT_BUILD
using Godgame.Scenario;
using PureDOTS.Runtime.Core;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Godgame.Rendering
{
    internal static class GodgameRenderPipelineBootstrap
    {
        private const string GamePipelineResource = "Rendering/GodgameURP";
        private const string LegacyPipelineResource = "Rendering/ScenarioURP";
#if UNITY_EDITOR
        private const string LegacyPipelineAssetPath = "Assets/Rendering/ScenarioURP.asset";
#endif

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSplashScreen)]
        private static void EnsureOnLoad()
        {
            TryEnsureRenderPipeline(legacyOnly: false);
        }

        internal static bool TryEnsureRenderPipeline(bool legacyOnly)
        {
            if (legacyOnly && !GodgameLegacyScenarioGate.IsEnabled)
            {
                return false;
            }

            if (RuntimeMode.IsHeadless)
            {
                return false;
            }

            if (GraphicsSettings.currentRenderPipeline != null)
            {
                return false;
            }

            var asset = Resources.Load<UniversalRenderPipelineAsset>(GamePipelineResource);
            if (asset == null)
            {
                asset = Resources.Load<UniversalRenderPipelineAsset>(LegacyPipelineResource);
            }

#if UNITY_EDITOR
            if (asset == null)
            {
                asset = AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>(LegacyPipelineAssetPath);
            }
#endif

            if (asset == null)
            {
                Debug.LogWarning("[GodgameRenderPipelineBootstrap] Render pipeline asset not found. Configure GraphicsSettings/QualitySettings for the game.");
                return false;
            }

            QualitySettings.renderPipeline = asset;
            GraphicsSettings.defaultRenderPipeline = asset;
            Debug.Log($"[GodgameRenderPipelineBootstrap] Render pipeline set to {asset.name}.");
            return true;
        }
    }
}
#endif
