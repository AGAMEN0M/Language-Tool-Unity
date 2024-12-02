/*
 * ---------------------------------------------------------------------------
 * Description: This ScriptableObject holds language settings data, including
 *              paths for language files, default language settings, and font information 
 *              for localization purposes in Unity applications.
 * Author: Lucas Gomes Cecchini
 * Pseudonym: AGAMENOM
 * ---------------------------------------------------------------------------
*/

using UnityEngine;
using System.Linq;

#if UNITY_EDITOR
using LanguageTools;
using UnityEditor;
#endif

// ScriptableObject to hold language settings data.
public class LanguageSettingsData : ScriptableObject
{
    [Header("Archives Location")]
    public string saveNameInUnity = "Editor/LanguageSave"; // Path to the JSON file for selected file in Unity Editor.
    [Space(5)]
    public string saveNameInBuild = "LanguageSave"; // Path to the JSON file for selected file in the build.
    [Space(5)]
    public string folderName = "Language"; // Folder containing language files.
    [Space(10)]
    [Header("Default Language")]
    public string defaultLanguage = "English"; // Default language to be used.
    [Space(10)]
    [Header("Font List Data")]
    public LanguageFontListData fontListData; // Holds information about fonts.
    public LanguageFontListDataTMP fontListDataTMP; // Holds information about TextMeshPro fonts.
    [Space(10)]
    [Header("Canvas Log")]
    public GameObject errorLanguageTool; // Field to reference a GameObject.
}

#if UNITY_EDITOR
public class LanguageSettingsEditor
{
    [MenuItem("Window/Language/Language Settings")]
    public static void OpenLanguageSettingsData()
    {
        // Retrieve the LanguageSettingsData object using a custom tool or method.
        var settingsData = LanguageFileManager.LoadLanguageSettings();

        // If the settings data is found, open it in Unity's Property Editor.
        if (settingsData != null)
        {
            // Attempt to find an existing open window of the Property Editor.
            EditorWindow existingWindow = Resources.FindObjectsOfTypeAll<EditorWindow>().FirstOrDefault(window => window.titleContent.text == settingsData.name);

            if (existingWindow != null)
            {
                existingWindow.Focus(); // If the window is already open, focus on it.
            }
            else
            {
                EditorUtility.OpenPropertyEditor(settingsData); // If not, open a new Property Editor window.
            }
        }
        else
        {
            Debug.LogError("Failed to find or load LanguageSettingsData. Ensure that the ScriptableObject exists and is properly referenced.");
        }
    }
}
#endif