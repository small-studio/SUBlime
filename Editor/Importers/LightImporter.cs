using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Xml;
using System.IO;
using System.Globalization;

namespace SUBlime
{

class LightImporter : SUBlime.IAssetImporter
{
    void UpdatePrefab(string xmlPath)
    {
        XmlDocument doc = new XmlDocument();
        doc.Load(xmlPath);
        XmlNode root = doc.DocumentElement;

        string prefabPath = root.SelectSingleNode("Path").InnerText;
        string fileName = Path.GetFileNameWithoutExtension(xmlPath);
        string fullPath = Path.Combine(prefabPath, fileName + ".prefab");

        // Load the prefab asset
        GameObject prefab = PrefabUtility.LoadPrefabContents(fullPath);
        if (prefab == null)
        {
            Debug.LogWarning("[PrefabImporter] There is no prefab at path " + fullPath);
            return;
        }

        Light light = prefab.AddComponent<Light>();

        // Light type
        string type = root.SelectSingleNode("Type").InnerText;
        if (type == "POINT")
        {
            light.type = LightType.Point;
        }
        else if (type == "SPOT")
        {
            float spotSize = SmallImporterUtils.ParseFloatXml(root.SelectSingleNode("SpotSize").InnerText);
            light.spotAngle = spotSize;
            light.type = LightType.Spot;
        }
        else if (type == "SUN")
        {
            light.type = LightType.Directional;
        }
        else if (type == "AREA")
        {
            string shape = root.SelectSingleNode("Shape").InnerText;
            if (shape == "RECTANGLE")
            {
                light.type = LightType.Rectangle;
                float width = SmallImporterUtils.ParseFloatXml(root.SelectSingleNode("Width").InnerText);
                float height = SmallImporterUtils.ParseFloatXml(root.SelectSingleNode("Height").InnerText);
                light.areaSize = new Vector2(width, height);
            }
            else if (shape == "DISC")
            {
                light.type = LightType.Disc;
                float radius = SmallImporterUtils.ParseFloatXml(root.SelectSingleNode("Radius").InnerText);
                light.areaSize = new Vector2(radius, radius);
            }
        }

        // Light color
        Color color = SmallImporterUtils.ParseColorXml(root.SelectSingleNode("Color").InnerText);
        light.color = color;

        float power = SmallImporterUtils.ParseFloatXml(root.SelectSingleNode("Power").InnerText);
        light.intensity = power;
        light.range = power / 3.0f;

        // Save and unload prefab asset
        PrefabUtility.SaveAsPrefabAsset(prefab, fullPath);
        PrefabUtility.UnloadPrefabContents(prefab);
    }

    public void OnPreImport(string assetPath, AssetImporter importer)
    {
        SmallImporterUtils.CreatePrefabXml(assetPath);
    }

    public void OnPostImport(string assetPath)
    {
        UpdatePrefab(assetPath);
    }
}

}