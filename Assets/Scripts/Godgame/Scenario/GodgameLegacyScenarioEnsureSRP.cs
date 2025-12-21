#if UNITY_EDITOR || DEVELOPMENT_BUILD
using Godgame.Rendering;
using Godgame.Scenario;
using PureDOTS.Runtime.Core;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Scripting.APIUpdating;

[DefaultExecutionOrder(-10000)]
[MovedFrom(true, null, null, "DemoEnsureSRP")]
public sealed class GodgameLegacyScenarioEnsureSRP : MonoBehaviour
{
    public UniversalRenderPipelineAsset fallbackAsset;

    void Awake()
    {
        if (!GodgameLegacyScenarioGate.IsEnabled)
        {
            enabled = false;
            return;
        }

        if (RuntimeMode.IsHeadless)
        {
            enabled = false;
            return;
        }

        if (GraphicsSettings.currentRenderPipeline != null)
        {
            enabled = false;
            return;
        }

        if (fallbackAsset != null)
        {
            QualitySettings.renderPipeline = fallbackAsset;
            GraphicsSettings.defaultRenderPipeline = fallbackAsset;
            Debug.Log($"[GodgameLegacyScenarioEnsureSRP] Render pipeline set to {fallbackAsset.name}.");
        }
        else
        {
            GodgameRenderPipelineBootstrap.TryEnsureRenderPipeline(legacyOnly: true);
        }

        enabled = false;
    }
}
#endif
