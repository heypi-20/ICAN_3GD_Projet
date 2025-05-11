using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class PixelationRenderFeature : ScriptableRendererFeature
{
    class PixelationPass : ScriptableRenderPass
    {
        private Material material;
        private RenderTargetHandle  source;
        private RenderTargetHandle tempTexture;

        public PixelationPass(Material material)
        {
            this.material = material;
            tempTexture.Init("_TempPixelTex");
        }

        public void Setup(RenderTargetHandle  source)
        {
            this.source = source;
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            CommandBuffer cmd = CommandBufferPool.Get("PixelationPass");

            // Récupérer le target lorsque c'est dans un contexte de ScriptableRenderPass
            var cameraTarget = renderingData.cameraData.renderer.cameraColorTarget;

            RenderTextureDescriptor desc = renderingData.cameraData.cameraTargetDescriptor;
            desc.depthBufferBits = 0;

            cmd.GetTemporaryRT(tempTexture.id, desc);
            cmd.Blit(cameraTarget, tempTexture.Identifier(), material);  // Apply pixelation
            cmd.Blit(tempTexture.Identifier(), cameraTarget);            // Copy back to screen

            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }

        public override void FrameCleanup(CommandBuffer cmd)
        {
            cmd.ReleaseTemporaryRT(tempTexture.id);
        }
    }

    public Material pixelationMaterial;
    private PixelationPass pixelationPass;

    public override void Create()
    {
        if (pixelationMaterial == null)
            Debug.LogError("Pixelation material not assigned.");

        pixelationPass = new PixelationPass(pixelationMaterial)
        {
            renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing
        };
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        Debug.Log(">> AddRenderPasses called for camera: " + renderingData.cameraData.camera.name);

        // Initialiser un RenderTargetHandle et définir son identifiant avec cameraColorTarget
        RenderTargetHandle cameraColorTargetHandle = new RenderTargetHandle();
        // cameraColorTargetHandle.SetIdentifier(renderer.cameraColorTarget);

        // Passer ce RenderTargetHandle à Setup
        pixelationPass.Setup(cameraColorTargetHandle);

        // Enqueue la passe de pixelation
        renderer.EnqueuePass(pixelationPass);
    }


}
