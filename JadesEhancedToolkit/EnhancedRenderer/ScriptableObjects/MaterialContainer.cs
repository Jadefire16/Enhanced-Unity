using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class MaterialContainer : ScriptableObject
{
    public int Count => assetMaterials.Count;
    public List<Material> assetMaterials = new List<Material>();
    public List<string> assetMaterialNames = new List<string>();

    private void OnEnable()
    {
        hideFlags = HideFlags.NotEditable;
    }

    public void Add(string name, Material mat)
    {
        assetMaterialNames.Add(name);
        assetMaterials.Add(mat);
    }

    public void Clear()
    {
        assetMaterials.Clear();
        assetMaterialNames.Clear();
    }

}
