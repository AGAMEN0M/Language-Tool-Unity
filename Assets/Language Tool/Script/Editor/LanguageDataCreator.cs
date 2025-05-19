/*
 * ---------------------------------------------------------------------------
 * Description: This script defines tools for creating and ensuring the presence of a 
 *              LanguageSettingsData asset in a Unity project. It provides a menu option 
 *              to manually generate this asset under Assets/Resources, including a prompt 
 *              to overwrite if one already exists. Additionally, it includes an automatic 
 *              initializer that creates the asset on project load if it's missing, helping 
 *              streamline the setup for localization systems.
 * 
 * Author: Lucas Gomes Cecchini
 * Pseudonym: AGAMENOM
 * ---------------------------------------------------------------------------
*/

using UnityEditor;
using UnityEngine;
using System.IO;

/// <summary>
/// Provides a Unity Editor menu option to create the LanguageSettingsData asset in the Resources folder.
/// </summary>
public class LanguageDataCreator
{
    /// <summary>
    /// Creates a LanguageSettingsData asset in the Resources folder. Prompts the user if it already exists.
    /// </summary>
    [MenuItem("Assets/Create/Language/Language Data", false, 1)]
    public static void CreateLanguageDataAsset()
    {
        const string folder = "Assets/Resources";
        string assetPath = $"{folder}/Language Data.asset";

        // Create the Resources folder if it doesn't exist.
        if (!AssetDatabase.IsValidFolder(folder)) AssetDatabase.CreateFolder("Assets", "Resources");

        // Prompt to overwrite the existing asset, if it exists.
        if (AssetDatabase.LoadAssetAtPath<LanguageSettingsData>(assetPath) != null &&
            !EditorUtility.DisplayDialog(
                "Replace File",
                "There is already a 'Language Data'. Do you want to replace it?",
                "Yes","No"))
        {
            return;
        }

        // Create and register the asset.
        var asset = ScriptableObject.CreateInstance<LanguageSettingsData>();
        AssetDatabase.CreateAsset(asset, assetPath);
        EditorUtility.SetDirty(asset);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        // Highlight the new asset in the Project window.
        EditorUtility.FocusProjectWindow();
        Selection.activeObject = asset;
    }
}

/// <summary>
/// Automatically creates the LanguageSettingsData asset on project load if it doesn't exist.
/// </summary>
[InitializeOnLoad]
public static class LanguageDataAutoInitializer
{
    // Static constructor runs on editor load.
    static LanguageDataAutoInitializer()
    {
        EditorApplication.delayCall += () =>
        {
            const string assetPath = "Assets/Resources/Language Data.asset";

            // Create the asset only if it doesn't already exist on disk.
            if (!File.Exists(assetPath)) LanguageDataCreator.CreateLanguageDataAsset();
        };
    }
}