using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class AnisotropicKuwaharaRenderFeature : ScriptableRendererFeature
{


    [SerializeField] private KuwaharaSettings settings;
    [SerializeField] private Shader shader;
    private Material material;
    private AnisotropicKuwaharaRenderPass kuwaharaRenderPass;

    public override void Create()
    {
        if (shader == null)
        {
            return;
        }
        material = new Material(shader);
        kuwaharaRenderPass = new AnisotropicKuwaharaRenderPass(material, settings);
        kuwaharaRenderPass.renderPassEvent = RenderPassEvent.AfterRenderingSkybox;
    
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (kuwaharaRenderPass == null)
        { 
            return;
        }    
        if (renderingData.cameraData.cameraType == CameraType.Game)
        {
            renderer.EnqueuePass(kuwaharaRenderPass);
        }
    }
    protected override void Dispose(bool disposing)
    {
        if (Application.isPlaying)
        {
            Destroy(material);
        }
        else
        {
            DestroyImmediate(material);
        }
    }
}

[Serializable]
public class KuwaharaSettings
{
    [Range(2, 20)] public int kernelSize = 2;
    [Range(1f, 18f)] public float sharpness = 8f;
    [Range(1f, 100f)] public float hardness = 8f;
    [Range(0.01f, 2f)] public float alpha = 1f;
    [Range(0.01f, 2f)] public float zeroCrossing = 0.58f;
    public bool useZeta = false;
    [Range(0.01f, 3f)] public float zeta = 1f;
    [Range(1, 4)] public int passes = 1;
}
