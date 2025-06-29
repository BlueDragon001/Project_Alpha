using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.Universal;

public class RayMarchingRendererFeature : ScriptableRendererFeature
{
    [System.Serializable]
    public class Settings
    {
        public Material rayMarchingMaterial;
        public RenderPassEvent renderPassEvent = RenderPassEvent.AfterRenderingTransparents;
    }

    public Settings settings = new Settings();
    private RayMarchingRenderPass rayMarchingPass;
    private static readonly ProfilingSampler profilingSampler = new ProfilingSampler("Ray Marching Pass");

    public override void Create()
    {
        rayMarchingPass = new RayMarchingRenderPass(settings);
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (settings.rayMarchingMaterial == null) return;
        renderer.EnqueuePass(rayMarchingPass);
    }
}

// Render Pass - The actual rendering logic
public class RayMarchingRenderPass : ScriptableRenderPass
{
    private RayMarchingRendererFeature.Settings settings;
    private string profilerTag = "Ray Marching Pass";

    public RayMarchingRenderPass(RayMarchingRendererFeature.Settings settings)
    {
        this.settings = settings;
        this.renderPassEvent = settings.renderPassEvent;
    }

    public void Setup() { /* No-op for render graph */ }

    public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
    {
        if (settings.rayMarchingMaterial == null) return;

        var resourceData = frameData.Get<UniversalResourceData>();
        var cameraData = frameData.Get<UniversalCameraData>();

        var passData = new PassData
        {
            material = settings.rayMarchingMaterial
        };

        using (var builder = renderGraph.AddRasterRenderPass<PassData>(profilerTag, out var passData2))
        {
            passData2.material = settings.rayMarchingMaterial;
            builder.SetRenderAttachment(resourceData.activeColorTexture, 0, AccessFlags.ReadWrite);
            builder.SetRenderFunc((PassData data, RasterGraphContext context) =>
            {
                var cmd = context.cmd;
                cmd.DrawProcedural(Matrix4x4.identity, data.material, 0, MeshTopology.Triangles, 3);
            });
        }
    }

    private class PassData
    {
        public Material material;
    }

    public override void OnCameraCleanup(CommandBuffer cmd) { }
    public void Dispose() { }
}
