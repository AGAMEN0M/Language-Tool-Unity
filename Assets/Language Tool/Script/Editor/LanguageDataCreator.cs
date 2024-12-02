/*
 * ---------------------------------------------------------------------------
 * Description: A class for creating and managing a LanguageSettingsData 
 *              asset in the Unity project. It provides a menu option to create a new 
 *              Language Data asset within the Assets/Resources folder. The script checks 
 *              for the existence of the asset and prompts the user to replace it if it 
 *              already exists. Additionally, it includes an automatic initializer that 
 *              ensures the Language Data asset is created on project load if it doesn't 
 *              already exist, streamlining the setup for language management tools in Unity.
 * Author: Lucas Gomes Cecchini
 * Pseudonym: AGAMENOM
 * ---------------------------------------------------------------------------
*/

using UnityEditor;
using UnityEngine;
using System.IO;

public class LanguageDataCreator
{
    [MenuItem("Assets/Create/Language/Language Data")]
    public static void CreateLanguageDataAsset()
    {
        // Specify the path where the asset will be saved.
        string resourcesFolderPath = "Assets/Resources";
        string languageDataAssetPath = $"{resourcesFolderPath}/Language Data.asset";

        // Check if the Resources folder exists; if not, create it.
        if (!AssetDatabase.IsValidFolder(resourcesFolderPath)) AssetDatabase.CreateFolder("Assets", "Resources");

        // Check if an asset with the same name already exists.
        if (AssetDatabase.LoadAssetAtPath<LanguageSettingsData>(languageDataAssetPath) != null)
        {
            // Display a dialog to confirm replacing the existing asset.
            if (!EditorUtility.DisplayDialog("Replace File", "There is already a 'Language Data'. Do you want to replace it?", "Yes", "No"))
            {
                return;
            }
        }

        // Create an instance of LanguageSettingsData and save it as an asset.
        var asset = ScriptableObject.CreateInstance<LanguageSettingsData>();
        AssetDatabase.CreateAsset(asset, languageDataAssetPath);
        EditorUtility.SetDirty(asset);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        // Focus on the Project window in the Unity Editor and select the created asset.
        EditorUtility.FocusProjectWindow();
        Selection.activeObject = asset;
    }
}

[InitializeOnLoad]
public static class LanguageDataAutoInitializer
{
    static LanguageDataAutoInitializer()
    {
        EditorApplication.delayCall += () =>
        {
            // Specify the path where the asset should be checked.
            string assetPath = "Assets/Resources/Language Data.asset";

            // Check if the asset already exists; if not, create it.
            if (!File.Exists(assetPath))
            {
                LanguageDataCreator.CreateLanguageDataAsset();
            }
        };
    }
}