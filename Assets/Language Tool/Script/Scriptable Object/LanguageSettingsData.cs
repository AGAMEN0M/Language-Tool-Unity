/*
 * ---------------------------------------------------------------------------
 * Description: ScriptableObject that stores localization settings for Unity projects.
 *              Includes folder paths, default language, font configuration, culture,
 *              available languages, and metadata for text and canvas localization.
 * Author: Lucas Gomes Cecchini
 * Pseudonym: AGAMENOM
 * ---------------------------------------------------------------------------
*/

using System.Collections.Generic;
using LanguageTools;
using UnityEngine;
using System.Linq;

#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Stores localization settings and data for supported languages and culture-specific text rendering.
/// Used by localization tools and runtime language loading systems.
/// </summary>
public class LanguageSettingsData : ScriptableObject
{
    [Header("Archives Location")]
    public string folderName = "Languages"; // Directory containing the language files.
    [Space(10)]
    [Header("Default Language")]
    public string defaultLanguage = "en"; // Default language used when no user preference is stored.
    [Space(10)]
    [Header("Font List Data")]
    public LanguageFontListData fontListData; // Stores the list of fonts used for standard Unity UI elements.
    public LanguageFontListDataTMP fontListDataTMP; // Stores the list of TMP fonts used for TextMeshPro UI elements.
    [Space(10)]
    [Header("Canvas Log")]
    public GameObject errorLanguageTool; // UI prefab shown when language loading or configuration fails.
    [Space(10)]
    [Header("Extracted Data")]
    public string selectedCulture; // The current language culture selected by the player (e.g., "en-US").
    public List<LanguageAvailable> availableLanguages = new(); // All languages defined and optionally available in the build.
    public List<IdData> idData = new(); // Text elements for dynamic localization in non-canvas components.
    public List<IdMetaData> idMetaData = new(); // Additional information or attributes related to each localized ID (e.g., font size or tags).
    public List<IdData> idCanvasData = new(); // Text elements specifically used within Unity Canvas UI components.
}

#if UNITY_EDITOR
/// <summary>
/// Provides a menu option to quickly open the LanguageSettingsData asset in the Unity Editor.
/// </summary>
public class LanguageSettingsEditor
{
    [MenuItem("Window/Language/Language Settings", false, 2029)]
    public static void OpenLanguageSettingsData()
    {
        // Attempt to load the LanguageSettingsData ScriptableObject from resources.
        var settingsData = LanguageFileManager.LoadLanguageSettings();
        if (settingsData == null)
        {
            Debug.LogError("Failed to load LanguageSettingsData. Ensure the ScriptableObject exists.");
            return;
        }

        // Check if the editor window for the asset is already open.
        var existingWindow = Resources.FindObjectsOfTypeAll<EditorWindow>().FirstOrDefault(window => window.titleContent.text == settingsData.name);

        if (existingWindow != null)
        {
            existingWindow.Focus(); // Focus the existing window if found.
        }
        else
        {
            EditorUtility.OpenPropertyEditor(settingsData); // Otherwise, open the ScriptableObject in the default property editor.
        }
    }
}
#endif