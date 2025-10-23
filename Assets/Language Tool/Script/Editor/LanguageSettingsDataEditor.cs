/*
 * ---------------------------------------------------------------------------
 * Description: Custom Unity Editor inspector for the LanguageSettingsData 
 *              ScriptableObject. This interface allows streamlined configuration 
 *              and visualization of language settings such as:
 *              - Default language selection using system cultures.
 *              - Management of font data (standard and TMP).
 *              - Folder path configuration for localization archives.
 *              - Real-time display of runtime language extraction data.
 *              
 *              The editor enhances user workflow by offering intuitive fields, 
 *              dropdowns, and property views, maintaining clean integration with 
 *              Unity's undo and asset save systems.
 *              
 * Author: Lucas Gomes Cecchini
 * Pseudonym: AGAMENOM
 * ---------------------------------------------------------------------------
*/

using System.Globalization;
using UnityEditor;
using UnityEngine;
using System;

/// <summary>
/// Custom Inspector for LanguageSettingsData, enabling management of language preferences,
/// fonts, archive locations, and visualization of runtime language data.
/// </summary>
[CanEditMultipleObjects]
[CustomEditor(typeof(LanguageSettingsData))]
public class LanguageSettingsDataEditor : Editor
{
    #region === Fields ===

    private string[] availableCultureDisplayNames; // Stores display names for available cultures.
    private CultureInfo[] availableCultures; // Stores all available system cultures.
    private int currentSelectedCultureIndex; // Tracks selected culture index.

    #endregion

    #region === Unity Callbacks ===

    private void OnEnable()
    {
        // Load all cultures and prepare display names.
        availableCultures = CultureInfo.GetCultures(CultureTypes.AllCultures);
        availableCultureDisplayNames = new string[availableCultures.Length];

        for (int i = 0; i < availableCultures.Length; i++)
            availableCultureDisplayNames[i] = availableCultures[i].NativeName;
    }

    public override void OnInspectorGUI()
    {
        var script = (LanguageSettingsData)target;

        // Style for section headers.
        GUIStyle titleStyle = new(EditorStyles.boldLabel) { fontSize = 14 };

        Undo.RecordObject(script, "Edit Language Settings");

        #region === Script Reference ===

        // Display the script reference field (read-only).
        EditorGUI.BeginDisabledGroup(true);
        EditorGUILayout.ObjectField("Script", MonoScript.FromScriptableObject(script), typeof(MonoScript), false);
        EditorGUI.EndDisabledGroup();

        #endregion

        EditorGUILayout.Space(10);

        #region === Archives Location ===

        // Folder path section.
        EditorGUILayout.LabelField("Archives Location", titleStyle);
        EditorGUILayout.LabelField("Assets", EditorStyles.boldLabel);
        script.folderName = EditorGUILayout.TextField(new GUIContent("Folder Name:", "Folder where localization archives are stored."), script.folderName);

        #endregion

        EditorGUILayout.Space(15);

        #region === Default Language Selection ===

        // Default language section using dropdown.
        EditorGUILayout.LabelField("Default Language", titleStyle);
        currentSelectedCultureIndex = Array.FindIndex(availableCultures, c => c.Name == script.defaultLanguage);
        currentSelectedCultureIndex = EditorGUILayout.Popup(new GUIContent("Language:", "Select the default language for the project."), currentSelectedCultureIndex, availableCultureDisplayNames);
        script.defaultLanguage = availableCultures[currentSelectedCultureIndex].Name;

        #endregion

        EditorGUILayout.Space(15);

        #region === Font List Data (Regular) ===

        EditorGUILayout.LabelField("Font List Data", titleStyle);
        script.fontListData = (LanguageFontListData)EditorGUILayout.ObjectField(new GUIContent("Font List Data:", "Assign a ScriptableObject that stores regular font references."), script.fontListData, typeof(LanguageFontListData), false);

        if (script.fontListData != null)
        {
            SerializedObject fontDataSO = new(script.fontListData);
            EditorGUILayout.PropertyField(fontDataSO.FindProperty("fontList"), new GUIContent("Fonts"), true);
            fontDataSO.ApplyModifiedProperties();
        }

        #endregion

        EditorGUILayout.Space(10);

        #region === Font List Data (TMP) ===

        script.fontListDataTMP = (LanguageFontListDataTMP)EditorGUILayout.ObjectField(new GUIContent("Font List Data (TMP):", "Assign a ScriptableObject that stores TMP font references."), script.fontListDataTMP, typeof(LanguageFontListDataTMP), false);

        if (script.fontListDataTMP != null)
        {
            SerializedObject tmpFontDataSO = new(script.fontListDataTMP);
            EditorGUILayout.PropertyField(tmpFontDataSO.FindProperty("TMPFontList"), new GUIContent("Fonts (TMP)"), true);
            tmpFontDataSO.ApplyModifiedProperties();
        }

        #endregion

        EditorGUILayout.Space(15);

        #region === Canvas Log Reference ===

        EditorGUILayout.LabelField("Canvas Log", titleStyle);
        script.errorLanguageTool = (GameObject)EditorGUILayout.ObjectField(new GUIContent("Error Language Tool:", "Optional GameObject used to display language errors in UI."), script.errorLanguageTool, typeof(GameObject), true);

        #endregion

        EditorGUILayout.Space(10);

        #region === Extracted Runtime Data (Read-Only) ===

        // Visualization of extracted runtime data.
        EditorGUILayout.LabelField("Extracted Data", titleStyle);
        GUI.enabled = false;
        script.selectedCulture = EditorGUILayout.TextField(new GUIContent("Selected Culture:", "The culture currently extracted at runtime."), script.selectedCulture);

        SerializedObject scriptSO = new(script);
        EditorGUILayout.PropertyField(scriptSO.FindProperty("availableLanguages"), new GUIContent("Available Languages", "List of languages detected in the project."), true);
        EditorGUILayout.PropertyField(scriptSO.FindProperty("idData"), new GUIContent("IDs", "Runtime extracted IDs for localization."), true);
        EditorGUILayout.PropertyField(scriptSO.FindProperty("idMetaData"), new GUIContent("IDs Meta", "Additional metadata for IDs."), true);
        EditorGUILayout.PropertyField(scriptSO.FindProperty("idCanvasData"), new GUIContent("IDs Canvas", "Canvas-related ID mappings."), true);
        scriptSO.ApplyModifiedProperties();
        GUI.enabled = true;

        #endregion

        #region === Save Changes ===

        // Save changes if GUI changed.
        if (GUI.changed)
        {
            EditorUtility.SetDirty(script);
            AssetDatabase.SaveAssets();
        }

        #endregion
    }

#endregion
}