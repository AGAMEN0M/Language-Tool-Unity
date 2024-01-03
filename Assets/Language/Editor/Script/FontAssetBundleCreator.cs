using UnityEngine;
using UnityEditor;
using System;
using System.Linq;
using System.IO;

public static class FontAssetBundleCreator
{
    // MenuItem to create AssetBundle from fonts in Unity Editor.
    [MenuItem("Assets/Create Font Asset Bundles (Language-Unity-Package)")]
    public static void CreateFontAssetBundles()
    {
        string fontFolderPath = "Assets/Language/AssetBundles"; // Base directory for AssetBundles.

        string[] fontPaths = AssetDatabase.FindAssets("t:Font", new string[] { fontFolderPath }); // Search for regular sources.
        string[] TMP_fontPaths = AssetDatabase.FindAssets("t:TMP_FontAsset", new string[] { fontFolderPath }); // Search for TMP_FontAssets.

        // Processes regular fonts.
        foreach (string fontPath in fontPaths)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(fontPath);
            string extension = Path.GetExtension(assetPath);

            // Checks if it is a valid font file.
            if (IsFontFile(extension))
            {
                string assetBundleName = Path.GetFileNameWithoutExtension(assetPath);

                // AssetBundle configuration for regular sources.
                AssetBundleBuild build = new()
                {
                    assetBundleName = $"{assetBundleName.ToLower()}.ltbundle",
                    assetNames = new string[] { assetPath }
                };

                // AssetBundle creation for regular sources.
                BuildPipeline.BuildAssetBundles(fontFolderPath, new AssetBundleBuild[] { build }, BuildAssetBundleOptions.None, BuildTarget.StandaloneWindows);
                Debug.Log($"Asset Bundle created successfully: {build.assetBundleName}"); // Success message.
            }
        }

        // Processes TMP_FontAssets.
        foreach (string fontPath in TMP_fontPaths)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(fontPath);
            string assetBundleName = Path.GetFileNameWithoutExtension(assetPath);

            // AssetBundle configuration for TMP_FontAssets.
            AssetBundleBuild build = new()
            {
                assetBundleName = $"{assetBundleName.ToLower()}.tmpltbundle",
                assetNames = new string[] { assetPath }
            };

            // Creation of the AssetBundle for TMP_FontAssets.
            BuildPipeline.BuildAssetBundles(fontFolderPath, new AssetBundleBuild[] { build }, BuildAssetBundleOptions.None, BuildTarget.StandaloneWindows);            
            Debug.Log($"Asset Bundle created successfully: {build.assetBundleName}"); // Success message.
        }

        AssetDatabase.Refresh(); // Updates the Asset Database to reflect changes.
    }

    // Checks whether the file extension is a valid font extension.
    private static bool IsFontFile(string extension)
    {
        string[] fontExtensions = { ".ttf", ".otf", ".ttc" };
        return fontExtensions.Any(ext => ext.Equals(extension, StringComparison.OrdinalIgnoreCase));
    }
}