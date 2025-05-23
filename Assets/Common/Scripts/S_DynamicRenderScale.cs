using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class DynamicRenderScale : MonoBehaviour
{
    [Header("Reference: 1440p corresponds to 0.5 render scale")]
    public float baseHeight = 1440f;
    public float baseScale  = 0.5f;

    private UniversalRenderPipelineAsset urpAsset;
    private int lastHeight = 0;

    void Awake()
    {
        // Get the active URP asset
        urpAsset = GraphicsSettings.currentRenderPipeline as UniversalRenderPipelineAsset;
        if (urpAsset == null)
        {
            enabled = false;
            return;
        }

        ApplyScale(Screen.height);
        lastHeight = Screen.height;
    }

    void Update()
    {
        // Detect resolution changes and update scale
        if (Screen.height != lastHeight)
        {
            ApplyScale(Screen.height);
            lastHeight = Screen.height;
        }
    }

    private void ApplyScale(int height)
    {
        // Inverse ratio: lower resolution => higher render scale
        float scale = baseScale * (baseHeight / height);

        // Clamp to [0.1, 2.0] to avoid extreme values
        scale = Mathf.Clamp(scale, 0.1f, 2f);

        urpAsset.renderScale = scale;
    }
}