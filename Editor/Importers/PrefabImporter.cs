﻿using UnityEngine;
using UnityEditor;
using System.Xml;
using System.IO;

namespace SUBlime
{

class PrefabImporter : AAssetImporter
{
    public override void CreateDependencies(string assetPath)
    {
        XmlDocument doc = new XmlDocument();
        doc.Load(assetPath);
        XmlNode root = doc.DocumentElement;

        // Add materials dependencies
        XmlNodeList materialsNode = root.SelectSingleNode("Materials").ChildNodes;
        for (int i = 0; i < materialsNode.Count; i++)
        {
            string path = materialsNode[i].SelectSingleNode("Path").InnerText;
            string name = materialsNode[i].SelectSingleNode("Name").InnerText;
            string materialPath = Path.Combine(path, name + ".mat");
            AddDependency<Material>(materialPath); 
        }

        // Add mesh dependency
        string meshPath = root.SelectSingleNode("Model").InnerText + ".fbx";
        AddDependency<Mesh>(meshPath); 

        // Add prefabs dependencies
        SmallImporterUtils.RecursiveGetTransformDependecies(this, root);
    }

    public override void OnPreImport(string assetPath)
    {
        SmallImporterUtils.CreatePrefabFromXml(assetPath);
    }

    public override void OnPostImport(string assetPath)
    {
        XmlDocument doc = new XmlDocument();
        doc.Load(assetPath);
        XmlNode root = doc.DocumentElement;

        string prefabPath = root.SelectSingleNode("Path").InnerText;
        string fileName = Path.GetFileNameWithoutExtension(assetPath);
        string fullPath = Path.Combine(prefabPath, fileName + ".prefab");

        // Load the prefab asset
        GameObject prefab = PrefabUtility.LoadPrefabContents(fullPath);
        if (prefab == null)
        {
            Debug.LogWarning("[PrefabImporter] There is no prefab at path " + fullPath);
            return;
        }

        // Load and assign the mesh
        string meshPath = root.SelectSingleNode("Model").InnerText + ".fbx";
        Mesh mesh = AssetDatabase.LoadAssetAtPath<Mesh>(meshPath);
        MeshFilter meshFilter = prefab.AddComponent<MeshFilter>();
        meshFilter.mesh = mesh;

        // Load and assign materials
        XmlNodeList materialsNode = root.SelectSingleNode("Materials").ChildNodes;
        Material[] materials = new Material[materialsNode.Count];
        for (int i = 0; i < materialsNode.Count; i++)
        {
            string path = materialsNode[i].SelectSingleNode("Path").InnerText;
            string name = materialsNode[i].SelectSingleNode("Name").InnerText;
            string materialPath = Path.Combine(path, name + ".mat");

            materials[i] = AssetDatabase.LoadAssetAtPath<Material>(materialPath);
        }

        MeshRenderer renderer = prefab.AddComponent<MeshRenderer>();
        renderer.sharedMaterials = materials;

        // Load and set children
        SmallParserUtils.RecursiveParseTransformXml(root, prefab);

        // Save and unload prefab asset
        PrefabUtility.SaveAsPrefabAsset(prefab, fullPath);
        PrefabUtility.UnloadPrefabContents(prefab);

        // Force Unity to update the asset, without this we have to manually reload unity (by losing and gaining focus on the editor)
        AssetDatabase.ImportAsset(fullPath);
    }
}

}