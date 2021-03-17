using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

/// <include file="emfdocs.xml" path='extradoc/class[@name="EnhancedMaterialFactory"]/*' />
public static class EnhancedMaterialFactory
{
    public static MaterialContainer activeContainer;
    public static Dictionary<string, Material> AssetMaterials { get; private set; } = new Dictionary<string, Material>();
    public static Dictionary<string, Material> Materials { get; private set; } = new Dictionary<string, Material>();

    internal static bool initialized = false;

    private static List<string> materialNames = new List<string>();

    private static Dictionary<string, int> properties = new Dictionary<string, int>();

    private static Dictionary<string, Material> masterMaterials = new Dictionary<string, Material>();

    private static Dictionary<string, Material> unityDefaultMaterials;

    internal static CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();


    /// <include file="emfdocs.xml" path='extradoc/member[@name="M:EnhancedMaterialFactory.Initialize"]/*' />
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
    public static void Initialize()
    {
        if (initialized)
        {
            UnityEngine.Debug.Log("Already Initialized, Please clear before attempting to reinitialize");
            return;
        }

#if UNITY_EDITOR
        EditorApplication.playModeStateChanged += OnPlayStateChanged;
#endif

        if (activeContainer == null || activeContainer.assetMaterials == null)
        {
            if (Resources.LoadAll<MaterialContainer>("Materials/MaterialContainer").Length > 1)
                UnityEngine.Debug.LogWarning("This function returns the first Material Container found which may be null!");
            activeContainer = Resources.Load<MaterialContainer>("Materials/MaterialContainer");
        }
        if (activeContainer == null)
        {
#if UNITY_EDITOR

            UnityEngine.Debug.Log("Loading Editor materials");
            LoadMaterials();
#else
                Debug.LogError("Active Container Not Found! This Needs To Be Built Prior To Building The Game Using The Enhanced Utils");
#endif
        }

        AssetMaterials = new Dictionary<string, Material>(activeContainer.Count);
        for (int i = 0; i < activeContainer.Count; i++) // adding all of our materials and material names into the dictionary
            AssetMaterials.Add(activeContainer.assetMaterialNames[i], activeContainer.assetMaterials[i]);

        Materials = new Dictionary<string, Material>(AssetMaterials.Count);
        foreach (var item in AssetMaterials.Values)
            Materials.Add(item.name, GenerateInstanceMaterial(item));

        LoadExposedShaderProperties();
        initialized = true;
    }

    internal static void OnPlayStateChanged(PlayModeStateChange state)
    {
        switch (state)
        {
            case PlayModeStateChange.ExitingEditMode:
                cancellationTokenSource.Cancel();
                cancellationTokenSource.Dispose();
                break;
            default:
                break;
        }
    }

    /// <include file="emfdocs.xml" path='extradoc/member[@name="M:EnhancedMaterialFactory.Clear"]/*' />
    public static void Clear()
    {
        initialized = false;
        activeContainer.Clear();
        activeContainer = null;
        AssetMaterials.Clear();
        AssetMaterials = new Dictionary<string, Material>();
        Materials.Clear();
        Materials = new Dictionary<string, Material>();
        materialNames.Clear();
        materialNames = new List<string>();
        properties.Clear();
        properties = new Dictionary<string, int>();
        masterMaterials.Clear();
        masterMaterials = new Dictionary<string, Material>();
    }

    #region Obsolete

    /// <summary>
    /// Will search through a specific directory for any objects of type Material
    /// </summary>
    /// <param name="directoryPath"></param>
    [Obsolete("No Longer Needed, Use activeContainer instead")]
    public static List<Material> LoadMaterialsFromResourcesPath(string directoryPath = "Assets/Resources/Materials")
    {
        if (!Directory.Exists(directoryPath))
        {
            UnityEngine.Debug.LogError($"No Directory Found At {directoryPath}. \n Search will return null.");
            return null;
        }
        return new List<Material>(Resources.LoadAll<Material>(directoryPath));
    }

    /// <summary>
    /// Will search through every directory in the Resources folder for any objects of type Material
    /// </summary>
    /// <returns></returns>
    [Obsolete("No Longer Needed, Use activeContainer instead")]
    public static List<Material> LoadMaterialsFromResourcesDirectory()
    {
        return new List<Material>(Resources.FindObjectsOfTypeAll<Material>());
    }
    #endregion

    #region ShaderKeywords

    /// <include file="emfdocs.xml" path='extradoc/member[@name="M:EnhancedMaterialFactory.LoadExposedShaderProperties"]/*' />
    internal static void LoadExposedShaderProperties()
    {
        if (properties != null)
        {
            properties.Clear();
            properties = new Dictionary<string, int>();
        }

        string propName;
        foreach (var material in AssetMaterials.Values)
        {
            for (int i = 0; i < material.shader.GetPropertyCount(); i++)
            {
                propName = material.shader.GetPropertyName(i);
                if (!properties.ContainsKey(propName))
                    properties.Add(material.shader.GetPropertyName(i), Shader.PropertyToID(propName));
            }
        }
    }

    /// <include file="emfdocs.xml" path='extradoc/member[@name="M:EnhancedMaterialFactory.TryGetPropertyID(System.String,System.Int32)"]/*' />
    public static bool TryGetPropertyID(string key, out int ID)
    {
        if (properties.TryGetValue(key, out ID))
            return true;
        else
        {
            ID = -1;
            return false;
        }
    }
    /// <include file="emfdocs.xml" path='extradoc/member[@name="M:EnhancedMaterialFactory.GetPropertyID(System.String)"]/*' />
    public static int GetPropertyID(string key) => properties[key];

    /// <include file="emfdocs.xml" path='extradoc/member[@name="M:EnhancedMaterialFactory.GetPropertyIDS"]/*' />
    public static int[] GetPropertyIDS() => properties.Values.ToArray();

    /// <include file="emfdocs.xml" path='extradoc/member[@name="M:EnhancedMaterialFactory.GetPropertyNames"]/*' />
    public static string[] GetPropertyNames() => properties.Keys.ToArray();

    #endregion

    /// <include file="emfdocs.xml" path='extradoc/member[@name="M:EnhancedMaterialFactory.HasStaticMaterial(System.String)"]/*' />
    public static bool HasStaticMaterial(string key)
    {
        if (!initialized)
            Initialize();

        return Materials.ContainsKey(key);
    }

    /// <include file="emfdocs.xml" path='extradoc/member[@name="M:EnhancedMaterialFactory.FindStaticMaterial(UnityEngine.Material)"]/*' />
    public static Material FindStaticMaterial(Material mat) // Todo clean this shit up
    {
        if (!initialized)
            Initialize();

        if (Materials.ContainsKey(mat.name))
            return Materials[mat.name];

        UnityEngine.Debug.LogError($"Material not found called {mat}");
        return null;
    }

    /// <include file="emfdocs.xml" path='extradoc/member[@name="M:EnhancedMaterialFactory.FindStaticMaterial(System.String)"]/*' />
    public static Material FindStaticMaterial(string key)
    {
        if (!initialized)
            Initialize();

        if (Materials.ContainsKey(key))
            return Materials[key];

        UnityEngine.Debug.LogError($"Material not found called {key}");
        return null;
    }

   

    /// <include file="emfdocs.xml" path='extradoc/member[@name="M:EnhancedMaterialFactory.GenerateInstanceMaterial(UnityEngine.Material)"]/*' />
    public static Material GenerateInstanceMaterial(Material material) => new Material(material);

 

    /// <include file="emfdocs.xml" path='extradoc/member[@name="M:EnhancedMaterialFactory.CleanupInstances"]/*' />
    public static void CleanupInstances()
    {
        foreach (var item in Materials)
            UnityEngine.Object.Destroy(item.Value);

        Materials.Clear();
        initialized = false;
    }

    public static void UpdateGIEnviroment() => DynamicGI.UpdateEnvironment();

    #region MasterMaterials
    /// <include file="emfdocs.xml" path='extradoc/member[@name="M:EnhancedMaterialFactory.GenerateMasterMaterial(UnityEngine.Material,System.String)"]/*' />
    public static Material GenerateMasterMaterial(Material material, string key = "")
    {
        if (string.IsNullOrWhiteSpace(key))
            key = material.name;
        if (masterMaterials.ContainsKey(key))
        {
            UnityEngine.Debug.Log("Cannot create multiple Master materials of the same key, provide a different name!");
            return material;
        }
        Material mat = GenerateInstanceMaterial(material);
        masterMaterials.Add(key, mat);
        return mat;
    }

    public static void SetMasterMaterialsFromRenderer(EnhancedRenderer rend)
    {
        Material[] mats = new Material[rend.Materials.Length];
        for (int i = 0; i < rend.Materials.Length; i++)
            mats[i] = GenerateMasterMaterial(rend.Materials[i], rend.Materials[i].name);

        for (int i = 0; i < mats.Length; i++)
            rend.SetMaterial(mats[i], i);
    }

    /// <include file="emfdocs.xml" path='extradoc/member[@name="M:EnhancedMaterialFactory.FindMasterMaterial(System.String)"]/*' />
    public static Material FindMasterMaterial(string key)
    {
        if (masterMaterials.ContainsKey(key))
            return masterMaterials[key];

        UnityEngine.Debug.LogError($"Material not found called {key}");
        return null;
    }
    #endregion


    #region Editor
#if UNITY_EDITOR
    /// <include file="emfdocs.xml" path='extradoc/member[@name="M:EnhancedMaterialFactory.LoadMaterials"]/*' />
    [MenuItem("Enhanced/Utils/Load Materials")]
    internal static void LoadMaterials()
    {
        Stopwatch watch = Stopwatch.StartNew();
        if (!Directory.Exists("Assets/Resources/Materials/"))
        {
            GenerateAssetPath();
        }

        if (activeContainer != null)
            activeContainer.Clear();
        else
            activeContainer = CreateAsset<MaterialContainer>("Assets/Resources/Materials");

        if (unityDefaultMaterials == null)
            LoadDefaultUnityMaterials(); // Grabbing Unity's default materials prior to instantiation
        materialNames = new List<string>(AssetDatabase.FindAssets("t:Material"));

        for (int i = 0; i < materialNames.Count; i++)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(materialNames[i]);
            activeContainer.assetMaterials.Add(AssetDatabase.LoadAssetAtPath(assetPath, typeof(Material)) as Material);
            activeContainer.assetMaterialNames.Add(activeContainer.assetMaterials[i].name);
        }
        LoadDefaultsToContainer();
        EditorUtility.SetDirty(activeContainer);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        watch.Stop();
        UnityEngine.Debug.Log("Completed in: " + watch.ElapsedMilliseconds + "ms", activeContainer);
    }

    /// <include file="emfdocs.xml" path='extradoc/member[@name="M:EnhancedMaterialFactory.LoadDefaultsToContainer"]/*' />
    internal static void LoadDefaultsToContainer()
    {
        activeContainer.Add(Keywords.Default_Diffuse, unityDefaultMaterials[Keywords.Default_Diffuse]);
        activeContainer.Add(Keywords.Default_Material, unityDefaultMaterials[Keywords.Default_Material]);
        activeContainer.Add(Keywords.Default_Terrain_Diffuse, unityDefaultMaterials[Keywords.Default_Terrain_Diffuse]);
        activeContainer.Add(Keywords.Default_Terrain_Specular, unityDefaultMaterials[Keywords.Default_Terrain_Specular]);
        activeContainer.Add(Keywords.Default_Terrain_Standard, unityDefaultMaterials[Keywords.Default_Terrain_Standard]);
    }

    /// <include file="emfdocs.xml" path='extradoc/member[@name="M:EnhancedMaterialFactory.LoadDefaultUnityMaterials"]/*' />
    internal static void LoadDefaultUnityMaterials()
    {
        unityDefaultMaterials = new Dictionary<string, Material>
        {
            { Keywords.Default_Diffuse, AssetDatabase.GetBuiltinExtraResource<Material>("Default-Diffuse.mat") },
            { Keywords.Default_Material, AssetDatabase.GetBuiltinExtraResource<Material>("Default-Material.mat") },
            { Keywords.Default_Terrain_Diffuse, AssetDatabase.GetBuiltinExtraResource<Material>("Default-Terrain-Diffuse.mat") },
            { Keywords.Default_Terrain_Specular, AssetDatabase.GetBuiltinExtraResource<Material>("Default-Terrain-Specular.mat") },
            { Keywords.Default_Terrain_Standard, AssetDatabase.GetBuiltinExtraResource<Material>("Default-Terrain-Standard.mat") }
        };
    }

    /// <include file="emfdocs.xml" path='extradoc/member[@name="M:EnhancedMaterialFactory.GenerateAssetPath"]/*' />
    internal static void GenerateAssetPath()
    {
        UnityEngine.Debug.Log("Generating Path");
        if (!Directory.Exists("Assets/Resources"))
        {
            AssetDatabase.CreateFolder("Assets", "Resources");
            AssetDatabase.Refresh();
        }
        AssetDatabase.CreateFolder("Assets/Resources", "Materials");
        AssetDatabase.Refresh();
    }
    /// <include file="emfdocs.xml" path='extradoc/member[@name="M:EnhancedMaterialFactory.CreateAsset(System.String)"]/*' />
    public static T CreateAsset<T>(string desiredAssetPath) where T : ScriptableObject
    {
        T asset = ScriptableObject.CreateInstance<T>();

        string assetPathAndName = AssetDatabase.GenerateUniqueAssetPath(desiredAssetPath + "/ " + typeof(T).ToString() + ".asset");

        AssetDatabase.CreateAsset(asset, assetPathAndName);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        EditorUtility.FocusProjectWindow();
        Selection.activeObject = asset;
        return asset;
    }


#endif
    #endregion


}


//Todo Integrate a materials so which can be loaded in at runtime, allowing you to change all of the properties without having to create a new instance
//Todo Fix error with whitespace in names not being loaded into dictionary // Where would I begin with this one


