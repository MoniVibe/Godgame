#if UNITY_EDITOR || DEVELOPMENT_BUILD
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using PureDOTS.Runtime.Core;

[DefaultExecutionOrder(-10000)]
public sealed class DemoEnsureSRP : MonoBehaviour
{
    public UniversalRenderPipelineAsset fallbackAsset;

    void Awake()
    {
        if (RuntimeMode.IsHeadless)
        {
            enabled = false;
            return;
        }

        if (GraphicsSettings.currentRenderPipeline != null) return;

        // Load pre-made asset, or create one on the fly
        var asset = fallbackAsset ?? Resources.Load<UniversalRenderPipelineAsset>("Rendering/DemoURP");
        if (asset == null) asset = ScriptableObject.CreateInstance<UniversalRenderPipelineAsset>();

        QualitySettings.renderPipeline = asset;   // assign for this session only
        Debug.Log($"[DemoEnsureSRP] Assigned URP: {asset.name}");
    }
}
#endif
