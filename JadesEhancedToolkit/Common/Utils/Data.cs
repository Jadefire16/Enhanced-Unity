using UnityEngine;
using UnityEngine.Rendering;
using UnityEditor;
public static class Data
{
    public static RenderPipelineType renderPipelineType = RenderPipelineType.URP;
    private static bool initialized = false;

#if UNITY_EDITOR
    [InitializeOnLoadMethod]
#else
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
#endif
    public static void Initialize()
    {
        if (initialized)
            return;
#if UNITY_EDITOR
        renderPipelineType = GetCurrentRenderPipelineInUse();
#endif
        initialized = true;
    }

    internal static RenderPipelineType GetCurrentRenderPipelineInUse()
    {
        RenderPipelineAsset rpa = GraphicsSettings.renderPipelineAsset;
        if (rpa != null)
        {
            switch (rpa.GetType().Name)
            {
                case "UniversalRenderPipelineAsset": return RenderPipelineType.URP;
                case "HDRenderPipelineAsset": return RenderPipelineType.HDRP; 
            }
        }
        return RenderPipelineType.Legacy;
    }
}
