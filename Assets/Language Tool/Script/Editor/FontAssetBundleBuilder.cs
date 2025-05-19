/*
 * ---------------------------------------------------------------------------
 * Description: A utility class for generating AssetBundles from Font and TMP_FontAsset assets 
 *              within Unity. It includes an editor menu option that searches for these font types 
 *              inside a designated "AssetBundles" folder. Each valid asset is bundled individually 
 *              with a specific naming pattern. The process includes validation, custom naming, 
 *              chunk-based compression, and success/error logging.
 * 
 * Author: Lucas Gomes Cecchini
 * Pseudonym: AGAMENOM
 * ---------------------------------------------------------------------------
*/

using UnityEngine;
using UnityEditor;
using System.Linq;
using System.IO;
using System;

/// <summary>
/// Provides functionality to build AssetBundles for Font and TMP_FontAsset assets from a specific folder.
/// </summary>
public static class FontAssetBundleBuilder
{
    /// <summary>
    /// Searches for Font and TMP_FontAsset assets in the "AssetBundles" folder and creates asset bundles for each.
    /// </summary>
    [MenuItem("Assets/Build Font Asset Bundles [Language Tool Unity]", false, 20)]
    public static void BuildFontAssetBundles()
    {
        // Attempt to locate the folder named "AssetBundles" in the project.
        string[] guids = AssetDatabase.FindAssets("AssetBundles t:Folder");
        if (guids.Length == 0)
        {
            Debug.LogError("Source folder does not exist.");
            return;
        }

        // Convert the found GUID to an asset path.
        string folderPath = AssetDatabase.GUIDToAssetPath(guids[0]);

        // Find all regular Font assets inside the folder and build bundles for each.
        foreach (string guid in AssetDatabase.FindAssets("t:Font", new[] { folderPath }))
            BuildFontAssetBundle(guid, folderPath, false);

        // Find all TMP_FontAsset assets inside the folder and build bundles for each.
        foreach (string guid in AssetDatabase.FindAssets("t:TMP_FontAsset", new[] { folderPath }))
            BuildFontAssetBundle(guid, folderPath, true);

        AssetDatabase.Refresh(); // Refresh the asset database to reflect the new bundles.
    }

    /// <summary>
    /// Builds an AssetBundle for a specific Font or TMP_FontAsset.
    /// </summary>
    /// <param name="fontGuid">The asset GUID of the font.</param>
    /// <param name="outputPath">The output folder for the asset bundle.</param>
    /// <param name="isTMP">Whether the font is a TextMeshPro font asset.</param>
    private static void BuildFontAssetBundle(string fontGuid, string outputPath, bool isTMP)
    {
        // Resolve the asset path from the provided GUID.
        string assetPath = AssetDatabase.GUIDToAssetPath(fontGuid);
        string extension = Path.GetExtension(assetPath);

        // Skip the file if it's not a TMP asset and not a valid font file.
        if (!isTMP && !IsValidFontFile(extension)) return;

        // Construct the bundle name with a special extension depending on asset type.
        string bundleName = Path.GetFileNameWithoutExtension(assetPath) + (isTMP ? ".tmpltbundle" : ".ltbundle");

        // Prepare the build definition for this asset bundle.
        AssetBundleBuild build = new()
        {
            assetBundleName = bundleName.ToLower(),
            assetNames = new[] { assetPath }
        };

        try
        {
            // Use chunk-based compression and build the asset bundle for Windows standalone.
            BuildPipeline.BuildAssetBundles(outputPath, new[] { build }, BuildAssetBundleOptions.ChunkBasedCompression, BuildTarget.StandaloneWindows);
            Debug.Log($"{(isTMP ? "TMP Font" : "Font")} Asset Bundle created successfully: {build.assetBundleName}");
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to create Asset Bundle for {assetPath}: {e.Message}");
        }
    }

    /// <summary>
    /// Checks if a file extension represents a valid font file.
    /// </summary>
    /// <param name="extension">File extension to validate.</param>
    /// <returns>True if the extension is a valid font format; otherwise, false.</returns>
    private static bool IsValidFontFile(string extension)
    {
        // Check against common font file extensions.
        return new[] { ".ttf", ".otf", ".ttc" }.Any(ext => ext.Equals(extension, StringComparison.OrdinalIgnoreCase));
    }
}