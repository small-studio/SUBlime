﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

namespace SUBlime
{

class SmallAssetPostprocessor : AssetPostprocessor
{
    static Dictionary<string, IAssetImporter> _importers = new Dictionary<string, IAssetImporter>();

    IAssetImporter GetAssetImporter(string path)
    {
        // TODO use constant for extension, replace extension for small
        if (path.EndsWith(SmallImporterUtils.SMALL_MATERIAL_EXTENSION))
        {
            return new MaterialImporter();
        }
        else if (path.EndsWith(SmallImporterUtils.SMALL_PREFAB_EXTENSION))
        {
            return new PrefabImporter();
        }
        else if (path.EndsWith(SmallImporterUtils.SMALL_SCENE_EXTENSION))
        {
            return new SceneImporter();
        }
        return null;
    }

#region PreProcess
    void OnPreprocessAsset()
    {
        // Ignore directory
        if (!Directory.Exists(assetPath))
        {
            // During this phase we create the needed assets without linking them together
            IAssetImporter importer = GetAssetImporter(assetPath);
            if (importer != null)
            {
                Debug.Log("[OnPreprocessAsset] Importing asset: " + assetPath);
                importer.OnPreImport(assetPath, assetImporter);
                if (_importers.ContainsKey(assetPath))
                {
                    _importers[assetPath] = importer;
                }
                else
                {
                    _importers.Add(assetPath, importer);
                }
            }
            else
            {
                Debug.Log("[OnPreprocessAsset] No importer for asset: " + assetPath);
            }
        }
    }

    void OnPreprocessTexture()
    {
        // TODO find a better way to do this
        if (assetPath.Contains("_Transparent"))
        {
            TextureImporter textureImporter  = (TextureImporter)assetImporter;
            textureImporter.alphaIsTransparency = true;
        }
    }

    void OnPreprocessModel()
    {
        // Don't import materials for models
        ModelImporter modelImporter = assetImporter as ModelImporter;
        modelImporter.materialImportMode = ModelImporterMaterialImportMode.None;
    }
#endregion

#region PostProcess
    static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
    {
        Debug.Log("[OnPostprocessAllAssets]");
        // During this phase all assets are already created, we must link them together
        foreach (string assetPath in importedAssets)
        {
            // Ignore directory
            if (!Directory.Exists(assetPath))
            {
                if (_importers.ContainsKey(assetPath))
                {
                    Debug.Log("[OnPostprocessAllAssets] Importing asset: " + assetPath);
                    IAssetImporter importer = _importers[assetPath];
                    importer.OnPostImport(assetPath);
                    _importers.Remove(assetPath);
                }
            }
        }
    }
#endregion
}

}