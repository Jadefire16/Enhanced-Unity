public static class Keywords
{
    public const string Default_Diffuse = "Default-Diffuse";
    public const string Default_Material = "Default-Material";
    public const string Default_Terrain_Diffuse = "Default-Terrain-Diffuse";
    public const string Default_Terrain_Specular = "Default-Terrain-Specular";
    public const string Default_Terrain_Standard = "Default-Terrain-Standard";
    public const string Emission_Colour = "_EmissionColor";
    public static string Colour
    {
        get
        {
            if (Data.renderPipelineType == RenderPipelineType.URP)
                return "_BaseColor";
            else if (Data.renderPipelineType == RenderPipelineType.HDRP)
                return "_BaseColor";
            else
                return "_Color";
        }
    }
}
