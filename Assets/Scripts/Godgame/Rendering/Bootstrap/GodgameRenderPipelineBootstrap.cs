#if UNITY_EDITOR || DEVELOPMENT_BUILD
using Godgame.Scenario;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityDebug = UnityEngine.Debug;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Godgame.Rendering
{
    public static class GodgameRenderPipelineBootstrap
    {
        private const string GamePipelineResource = "Rendering/GodgameURP";
        private const string ScenarioPipelineResource = "Rendering/ScenarioURP";
#if UNITY_EDITOR
        private const string ScenarioPipelineAssetPath = "Assets/Rendering/ScenarioURP.asset";
#endif

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSplashScreen)]
        private static void EnsureOnLoad()
        {
            TryEnsureRenderPipeline(scenarioOnly: false);
        }

        public static bool TryEnsureRenderPipeline(bool scenarioOnly)
        {
            if (scenarioOnly && !GodgameScenarioGate.IsEnabled)
            {
                return false;
            }

            if (PureDOTS.Runtime.Core.RuntimeMode.IsHeadless)
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
                asset = Resources.Load<UniversalRenderPipelineAsset>(ScenarioPipelineResource);
            }

#if UNITY_EDITOR
            if (asset == null)
            {
                asset = AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>(ScenarioPipelineAssetPath);
            }
#endif

            if (asset == null)
            {
                UnityDebug.LogWarning("[GodgameRenderPipelineBootstrap] Render pipeline asset not found. Configure GraphicsSettings/QualitySettings for the game.");
                return false;
            }

            QualitySettings.renderPipeline = asset;
            GraphicsSettings.defaultRenderPipeline = asset;
            UnityDebug.Log($"[GodgameRenderPipelineBootstrap] Render pipeline set to {asset.name}.");
            return true;
        }
    }
}
#endif
