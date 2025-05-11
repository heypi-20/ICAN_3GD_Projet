using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class PixelationRenderFeature : ScriptableRendererFeature
{
    class PixelationPass : ScriptableRenderPass
    {
        private Material material;
        private RenderTargetIdentifier source;
        private RenderTargetHandle tempTexture;

        public PixelationPass(Material material)
        {
            this.material = material;
            tempTexture.Init("_TempPixelTex");
        }

        public void Setup(RenderTargetIdentifier source)
        {
            this.source = source;
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            // Ne pas appliquer l'effet sur les caméras de l'éditeur (Scene view, etc.)
            if (renderingData.cameraData.cameraType != CameraType.Game)
                return;

            CommandBuffer cmd = CommandBufferPool.Get("PixelationPass");

            RenderTextureDescriptor desc = renderingData.cameraData.cameraTargetDescriptor;
            desc.depthBufferBits = 0;

            cmd.GetTemporaryRT(tempTexture.id, desc);
            cmd.Blit(source, tempTexture.Identifier(), material);  // Applique le shader
            cmd.Blit(tempTexture.Identifier(), source);             // Re-projette sur l'écran

            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }

        public override void FrameCleanup(CommandBuffer cmd)
        {
            if (cmd == null) return;
            cmd.ReleaseTemporaryRT(tempTexture.id);
        }
    }

    [System.Serializable]
    public class PixelationSettings
    {
        public Material pixelationMaterial;
        public RenderPassEvent renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;
    }

    public PixelationSettings settings = new PixelationSettings();
    private PixelationPass pixelationPass;

    public override void Create()
    {
        if (settings.pixelationMaterial == null)
        {
            Debug.LogError("PixelationRenderFeature: No material assigned.");
            return;
        }

        pixelationPass = new PixelationPass(settings.pixelationMaterial)
        {
            renderPassEvent = settings.renderPassEvent
        };
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
#if UNITY_2022_2_OR_NEWER
        var cameraColorTarget = renderer.cameraColorTargetHandle;
        Debug.Log("AddRenderPasses called on: " + renderingData.cameraData.camera.name);
        pixelationPass.Setup(cameraColorTarget);
#else
    var cameraColorTarget = renderer.cameraColorTarget;
    pixelationPass.Setup(cameraColorTarget);
#endif
        renderer.EnqueuePass(pixelationPass);
    }
}
