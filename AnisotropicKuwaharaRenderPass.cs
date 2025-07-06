using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.RenderGraphModule.Util;
using UnityEngine.Rendering.Universal;
using UnityEngine.Assertions;

// SRP multi-pass shader pipeline for URP adapted from Unity example
// https://docs.unity3d.com/6000.1/Documentation/Manual/urp/renderer-features/create-custom-renderer-feature.html
public class AnisotropicKuwaharaRenderPass : ScriptableRenderPass
{
    private static readonly int kernelSizeId = Shader.PropertyToID("_KernelSize");
    private static readonly int nId = Shader.PropertyToID("_N");
    private static readonly int sharpnessId = Shader.PropertyToID("_Q");
    private static readonly int hardnessId = Shader.PropertyToID("_Hardness");
    private static readonly int alphaId = Shader.PropertyToID("_Alpha");
    private static readonly int zeroCrossingId = Shader.PropertyToID("_ZeroCrossing");
    private static readonly int zetaId = Shader.PropertyToID("_Zeta");

    private const string k_KuwaharaTextureName = "_KuwaharaTexture";
    private const string k_VerticalPassName = "VerticalRenderPass";
    private const string k_HorizontalPassName = "HorizontalRenderPass";
    private const string k_EigenvectorPassName = "EigenvectorRenderPass";
    private const string k_KuwaharaPassName = "KuwaharaRenderPass";

    private KuwaharaSettings defaultSettings;
    private Material material;

    private TextureDesc kuwaharaTextureDescriptor;

    public AnisotropicKuwaharaRenderPass(Material material, KuwaharaSettings defaultSettings)
    {
        this.material = material;
        this.defaultSettings = defaultSettings;
    }

    private void UpdateKuwaharaSettings()
    {
        if (material == null) return;

        material.SetInt(kernelSizeId, defaultSettings.kernelSize);
        material.SetInt(nId, 8);
        material.SetFloat(sharpnessId, defaultSettings.sharpness);
        material.SetFloat(hardnessId, defaultSettings.hardness);
        material.SetFloat(alphaId, defaultSettings.alpha);
        material.SetFloat(zeroCrossingId, defaultSettings.zeroCrossing);
        material.SetFloat(zetaId, defaultSettings.useZeta ? defaultSettings.zeta : 2.0f / 2.0f / (defaultSettings.kernelSize / 2.0f));
    }

    // We need to define our own AddBlitPass to pass the required TFM texture for the Kuwahara passes.
    // The latest Render Graph API does not allow setting material textures outside the Render Graph Context.
    // Helper functions by OrangeLightning219: https://discussions.unity.com/t/using-multiple-input-textures-with-rendergraph-addblitpass/1545501/4

    void ExecutePass( PassData data, RasterGraphContext context )
    {
        foreach ( TextureBindInfo info in data.additionalTextures )
        {
            material.SetTexture( info.slot, info.texture );
        }

        Blitter.BlitTexture( context.cmd, data.source, new Vector4( 1, 1, 0, 0 ), material, data.passIndex );
    }

    void AddBlitPass( RenderGraph renderGraph, TextureHandle source, TextureHandle destination, string passName, int passIndex, params TextureBindInfo[] additionalTextures )
    {
        using var builder = renderGraph.AddRasterRenderPass( passName, out PassData passData );

        builder.UseTexture( source );
        passData.source = source;
        passData.passIndex = passIndex;

        foreach ( TextureBindInfo info in additionalTextures )
        {
            Assert.IsTrue( info.texture.IsValid() );
            builder.UseTexture( info.texture );
        }

        passData.additionalTextures = additionalTextures;

        builder.SetRenderAttachment( destination, 0 );
        builder.SetRenderFunc< PassData >( ExecutePass );
    }

    public class TextureBindInfo
    {
        public int slot;
        public TextureHandle texture;
    }

    class PassData
    {
        public TextureHandle source;
        public TextureBindInfo[] additionalTextures;
        public int passIndex;
    }

    public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
    {
        UniversalResourceData resourceData = frameData.Get<UniversalResourceData>();

        UniversalCameraData cameraData = frameData.Get<UniversalCameraData>();

        // The following line ensures that the render pass doesn't blit
        // from the back buffer.
        if (resourceData.isActiveTargetBackBuffer)
            return;

        TextureHandle srcCamColor = resourceData.activeColorTexture;
        kuwaharaTextureDescriptor = resourceData.activeColorTexture.GetDescriptor(renderGraph);
        kuwaharaTextureDescriptor.name = k_KuwaharaTextureName;
        kuwaharaTextureDescriptor.depthBufferBits = 0;
        var structureTensor = renderGraph.CreateTexture(kuwaharaTextureDescriptor);
        var eigenvectors1 = renderGraph.CreateTexture(kuwaharaTextureDescriptor);
        var eigenvectors2 = renderGraph.CreateTexture(kuwaharaTextureDescriptor);
        var kuwaharaTextures = new[] {
            renderGraph.CreateTexture(kuwaharaTextureDescriptor),
            renderGraph.CreateTexture(kuwaharaTextureDescriptor),
            renderGraph.CreateTexture(kuwaharaTextureDescriptor),
            renderGraph.CreateTexture(kuwaharaTextureDescriptor)
        };

        
        // Update the settings in the material
        UpdateKuwaharaSettings();

        // This check is to avoid an error from the material preview in the scene
        if (!srcCamColor.IsValid())
            return;

        RenderGraphUtils.BlitMaterialParameters paraEigenvector = new(srcCamColor, structureTensor, material, 0);
        renderGraph.AddBlitPass(paraEigenvector, k_EigenvectorPassName);

        RenderGraphUtils.BlitMaterialParameters paraHorizontal = new(structureTensor, eigenvectors1, material, 1);
        renderGraph.AddBlitPass(paraHorizontal, k_HorizontalPassName);
        
        RenderGraphUtils.BlitMaterialParameters paraVertical = new(eigenvectors1, eigenvectors2, material, 2);
        renderGraph.AddBlitPass(paraVertical, k_VerticalPassName);

        int additionalTextureId = Shader.PropertyToID("_TFM");
        TextureBindInfo info = new TextureBindInfo{ slot = additionalTextureId, texture = eigenvectors2 };

        AddBlitPass(renderGraph, srcCamColor, kuwaharaTextures[0], k_KuwaharaPassName, 3, info);

        for (int i = 1; i < defaultSettings.passes; i++)
        {
            AddBlitPass(renderGraph, kuwaharaTextures[i-1], kuwaharaTextures[i], k_KuwaharaPassName, 3, info);
        }

        AddBlitPass(renderGraph, kuwaharaTextures[defaultSettings.passes-1], srcCamColor, k_KuwaharaPassName, 3, info);
    }
}
