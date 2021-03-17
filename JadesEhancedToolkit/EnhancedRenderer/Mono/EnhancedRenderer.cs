using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[RequireComponent(typeof(Renderer))]
public class EnhancedRenderer : MonoBehaviour
{
    /// <summary>
    /// Enabling this will cause this renderer to initialize its materials from a static pool of materials. <para/>
    /// Editing a material with this active will change properties for other renderers.
    /// </summary>
    public bool keepStaticAtRuntime = true;
    public bool setToMasterAtRuntime = false;

    [HideInInspector] public bool requestsReset = false;

    /// <summary>
    /// This will set the base colour of the first index on the renderer. This will return if the renderer is static without applying changes.
    /// </summary>
    public Color Colour
    {
        set
        {
            if (IsStatic)
                return;
            ResetBlock();
            SetColour(value, Keywords.Colour);
            ApplyBlock(0, false);
        }
    }
    public Color Emission
    {
        set
        {
            if (IsStatic)
                return;
            ResetBlock();
            SetColour(value, Keywords.Emission_Colour);
            ApplyBlock(0, false);
        }
    }

    public Material BaseMaterial => materials[0];
    public Material[] Materials => materials;

    public RendererState State => state;
    public bool IsStatic => state == RendererState.Static;
    public bool IsMaster => state == RendererState.MasterInstance;

    private Renderer rend;
    private RendererState state = RendererState.NotInit;
    private MaterialPropertyBlock block;
    private Material[] materials;
    private int count;

    private void Awake()
    {
        if (keepStaticAtRuntime && setToMasterAtRuntime)
        {
            Debug.LogError($"Cannot set Renderer to both static and Master!\n {gameObject} will be set to Static!");
            setToMasterAtRuntime = false;
        }
        Initialize();
    }

    private void LateUpdate()
    {
        if (requestsReset)
            rend.GetPropertyBlock(block); 
    }

    /// <summary>
    /// Initializes the Enhanced Renderer component
    /// </summary>
    private void Initialize()
    {
        if ((rend = GetComponent<Renderer>()) is SpriteRenderer)
        {
            rend = null;
            throw new Exception("Sprite Renderers cannot be used!");
        }
        
        if (block == null)
            block = new MaterialPropertyBlock();
        if (keepStaticAtRuntime)
        {
            materials = new Material[rend.sharedMaterials.Length]; // setting it to the shared materials length because accessing rend.material causes it to generate new materials
            for (int i = 0; i < rend.sharedMaterials.Length; i++)
            {
                materials[i] = EnhancedMaterialFactory.FindStaticMaterial(rend.sharedMaterials[i]); // find the static equivilent in the factory and assign it
            }
            rend.materials = materials;
            state = RendererState.Static;
        }
        else if (!keepStaticAtRuntime && setToMasterAtRuntime)
        {
            EnhancedMaterialFactory.SetMasterMaterialsFromRenderer(this);
            state = RendererState.MasterInstance;
        }        
        else // Here we can safely allow unity to Instance it as long as we cleanup later
        {
            materials = rend.materials;
            state = RendererState.Instanced;
        }
        count = materials.Length;
    }

    /// <summary>
    /// Sets the Renderer to Instanced mode. This will generate a new Material per Material in the Renderer.
    /// </summary>
    public void SetRendererToInstanced()
    {
        if (materials == null)
            Debug.LogError(materials + " is null! Something went seriously wrong");
        for (int i = 0; i < materials.Length; i++)
        {
            materials[i] = EnhancedMaterialFactory.GenerateInstanceMaterial(materials[i]);
            materials[i].name = materials[i].name;
        }
        rend.materials = materials;
        state = RendererState.Instanced;
    }

    /// <summary>
    /// Sets the renderer to Static mode.<para /> This will destroy any Instanced Materials and set the Renderers Materials' to the Static equivilent (losing changes in the process)
    /// </summary>
    public void SetRendererToStatic()
    {
        Cleanup();
        materials = new Material[rend.sharedMaterials.Length]; // setting it to the shared materials length because accessing rend.material causes it to generate new materials
        for (int i = 0; i < rend.sharedMaterials.Length; i++)
        {
            materials[i] = EnhancedMaterialFactory.FindStaticMaterial(rend.sharedMaterials[i]); // find the static equivilent in the factory and assign it
        }
        rend.materials = materials;
        state = RendererState.Static;
    }
    /// <summary>
    /// Will set a Material in the Renderer to the Material provided.
    /// <para /> This will only work if the Renderer is not static!
    /// </summary>
    /// <param name="mat"></param>
    /// <param name="index"></param>
    public void SetMaterial(Material mat, int index = 0)
    {
        if (IsStatic)
            return;
        rend.materials[index] = mat;
    }

    public void SetTexture(Texture2D tex, string key) => block.SetTexture(key, tex);
    public void SetTexture(Texture2D tex, int key) => block.SetTexture(key, tex);
    public void SetColour(Color col, string key) => block.SetColor(key, col);
    public void SetColour(Color col, int key) => block.SetColor(key, col);
    public void SetColour(Color32 col, string key) => SetColour(col, key);
    public void SetFloat(float val, string key) => block.SetFloat(key, val);
    public void SetFloat(float val, int key) => block.SetFloat(key, val);
    public void SetInt(int val, string key) => block.SetInt(key, val);
    public void SetInt(int val, int key) => block.SetInt(key, val);

    public void ApplyTexture(Texture2D tex, string key) => ApplyTexture(tex, EnhancedMaterialFactory.GetPropertyID(key));

    public void ApplyTexture(Texture2D tex, int key) 
    {
        rend.GetPropertyBlock(block);
        block.SetTexture(key, tex);
        rend.SetPropertyBlock(block);
    }

    public void ApplyColour(Color col, string key) => ApplyColour(col, EnhancedMaterialFactory.GetPropertyID(key));
    public void ApplyColour(Color32 col, string key) => ApplyColour(col, key);
    public void ApplyColour(Color col, int key)
    {
        rend.GetPropertyBlock(block);
        block.SetColor(key, col);
        rend.SetPropertyBlock(block);
    }
    public void ApplyFloat(float val, string key) => ApplyFloat(val, EnhancedMaterialFactory.GetPropertyID(key));
    public void ApplyFloat(float val, int key)
    {
        rend.GetPropertyBlock(block);
        block.SetFloat(key, val);
        rend.SetPropertyBlock(block);
    }
    public void ApplyInt(int val, string key) => ApplyInt(val, EnhancedMaterialFactory.GetPropertyID(key));
    public void ApplyInt(int val, int key)
    {
        rend.GetPropertyBlock(block);
        block.SetInt(key, val);
        rend.SetPropertyBlock(block);
    }

    public void ApplyBlock(int index, bool staticOverride = false)
    {      
        if (IsStatic && staticOverride)
            SetRendererToInstanced();
        else if (IsStatic && !staticOverride)
        {
            Debug.LogWarning($"Attempting to set a parameter in a static {nameof(EnhancedRenderer)} without overriding is not allowed!");
            return;
        }
        rend.SetPropertyBlock(block, index);      
    }
    /// <summary>
    /// WARNING: This function is designed only to cover edge cases!<para /> You likely want to use ApplyBlock() instead!
    /// </summary>
    /// <param name="index"></param>
    /// <param name="clearBlock"></param>
    public void ForceApplyBlock(int index)
    {
        rend.SetPropertyBlock(block, index);
    }
    /// <summary>
    /// Resets the Material Property Block to the Renderer default settings.
    /// </summary>
    public void ResetBlock() => requestsReset = true;

    /// <summary>
    /// Clears all values from block. This produces unintended side effects when used during runtime like set calls not being executed.
    /// </summary>
    public void ClearBlock() => block.Clear();

    public void Cleanup()
    {
        for (int i = 0; i < count; i++)
        {
            Destroy(materials[i]);
            materials[i] = null;
        }
    }
    private void OnDestroy()
    {
        if (!IsStatic && !IsMaster)
            Cleanup();
    }
}
public enum RendererState
{
    NotInit = 0,
    Static = 1,
    Instanced = 2,
    MasterInstance = 3
}