/*
 * ---------------------------------------------------------------------------
 * Description: A utility class for building asset bundles containing Font 
 *              and TMP_FontAsset assets within Unity. It provides a menu option to 
 *              initiate the asset bundle building process, searching for Font and 
 *              TMP_FontAsset files in a designated "AssetBundles" folder. The script 
 *              validates font file types, constructs asset bundle names based on font 
 *              names, and handles the building process with chunk-based compression. 
 *              Successful builds are logged in the console, along with error handling 
 *              for any failures during the process.
 * Author: Lucas Gomes Cecchini
 * Pseudonym: AGAMENOM
 * ---------------------------------------------------------------------------
*/

using UnityEngine;
using UnityEditor;
using System.Linq;
using System.IO;
using System;

public static class FontAssetBundleBuilder
{
    [MenuItem("Assets/Build Font Asset Bundles [Language Tool Unity]")]
    public static void BuildFontAssetBundles()
    {
        // Find all folders labeled as AssetBundles in the project.
        string[] guids = AssetDatabase.FindAssets("AssetBundles t:Folder");
        if (guids.Length == 0) // Check if any asset bundle folder exists.
        {
            Debug.LogError("Source folder does not exist.");
            return; // Exit if no folders are found.
        }

        string fontFolderPath = AssetDatabase.GUIDToAssetPath(guids[0]); // Get the path of the first found asset bundle folder.

        // Find all Font and TMP_FontAsset assets in the specified folder.
        string[] fontPaths = AssetDatabase.FindAssets("t:Font", new string[] { fontFolderPath });
        string[] fontPathsTMP = AssetDatabase.FindAssets("t:TMP_FontAsset", new string[] { fontFolderPath });

        // Build asset bundles for each found Font asset.
        foreach (string fontPath in fontPaths)
        {
            BuildFontAssetBundle(fontPath, fontFolderPath, false); // False indicates it is not a TMP font.
        }

        // Build asset bundles for each found TMP_FontAsset.
        foreach (string fontPath in fontPathsTMP)
        {
            BuildFontAssetBundle(fontPath, fontFolderPath, true); // True indicates it is a TMP font.
        }

        AssetDatabase.Refresh(); // Refresh the AssetDatabase to reflect changes.
    }

    // Builds a single font asset bundle.
    private static void BuildFontAssetBundle(string fontPath, string fontFolderPath, bool isTMP)
    {
        // Convert the font path GUID to its corresponding asset path.
        string assetPath = AssetDatabase.GUIDToAssetPath(fontPath);
        string extension = Path.GetExtension(assetPath); // Get the file extension of the font.

        // If it's not a TMP font, check if the file extension is valid.
        if (!isTMP && !IsValidFontFile(extension)) return; // Exit if the font file is invalid.

        // Determine the asset bundle name based on the font's name and type.
        string assetBundleName = Path.GetFileNameWithoutExtension(assetPath) + (isTMP ? ".tmpltbundle" : ".ltbundle");

        // Create a new AssetBundleBuild object with the asset bundle name and asset path.
        AssetBundleBuild build = new()
        {
            assetBundleName = assetBundleName.ToLower(), // Convert the name to lowercase for consistency.
            assetNames = new string[] { assetPath } // Include the asset path.
        };

        try
        {
            // Build the asset bundles with chunk-based compression for the specified platform.
            BuildPipeline.BuildAssetBundles(fontFolderPath, new AssetBundleBuild[] { build }, BuildAssetBundleOptions.ChunkBasedCompression, BuildTarget.StandaloneWindows);

            // Log success message based on the type of font being processed.
            string message = isTMP ? "TMP Font Asset Bundle created successfully:" : "Font Asset Bundle created successfully:";
            Debug.Log($"{message} {build.assetBundleName}");
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to create Asset Bundle for {assetPath}: {e.Message}");
        }
    }

    // Validates whether the file has a valid font file extension.
    private static bool IsValidFontFile(string extension)
    {
        string[] fontExtensions = { ".ttf", ".otf", ".ttc" }; // List of valid font file extensions.

        // Check if the provided extension matches any of the valid font extensions.
        return fontExtensions.Any(ext => ext.Equals(extension, StringComparison.OrdinalIgnoreCase));
    }
}