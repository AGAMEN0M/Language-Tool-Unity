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
    private string[] availableCultureDisplayNames; // Stores display names for available cultures.
    private CultureInfo[] availableCultures; // Stores all available system cultures.
    private int currentSelectedCultureIndex; // Tracks selected culture index.

    private void OnEnable()
    {
        // Load all cultures and prepare display names.
        availableCultures = CultureInfo.GetCultures(CultureTypes.AllCultures);
        availableCultureDisplayNames = new string[availableCultures.Length];

        for (int i = 0; i < availableCultures.Length; i++)
            availableCultureDisplayNames[i] = availableCultures[i].EnglishName;
    }

    public override void OnInspectorGUI()
    {
        var script = (LanguageSettingsData)target;

        // Style for section headers.
        GUIStyle titleStyle = new(EditorStyles.boldLabel) { fontSize = 14 };

        Undo.RecordObject(script, "Edit Language Settings");

        // Display the script reference field (read-only).
        EditorGUI.BeginDisabledGroup(true);
        EditorGUILayout.ObjectField("Script", MonoScript.FromScriptableObject(script), typeof(MonoScript), false);
        EditorGUI.EndDisabledGroup();

        EditorGUILayout.Space(10);

        // Folder path section.
        EditorGUILayout.LabelField("Archives Location", titleStyle);
        EditorGUILayout.LabelField("Assets", EditorStyles.boldLabel);
        script.folderName = EditorGUILayout.TextField("Folder Name:", script.folderName);

        EditorGUILayout.Space(15);

        // Default language section using dropdown.
        EditorGUILayout.LabelField("Default Language", titleStyle);
        currentSelectedCultureIndex = Array.FindIndex(availableCultures, c => c.Name == script.defaultLanguage);
        currentSelectedCultureIndex = EditorGUILayout.Popup("Language:", currentSelectedCultureIndex, availableCultureDisplayNames);
        script.defaultLanguage = availableCultures[currentSelectedCultureIndex].Name;

        EditorGUILayout.Space(15);

        // Font data (regular).
        EditorGUILayout.LabelField("Font List Data", titleStyle);
        script.fontListData = (LanguageFontListData)EditorGUILayout.ObjectField("Font List Data:", script.fontListData, typeof(LanguageFontListData), false);

        if (script.fontListData != null)
        {
            SerializedObject fontDataSO = new(script.fontListData);
            EditorGUILayout.PropertyField(fontDataSO.FindProperty("fontList"), new GUIContent("Fonts"), true);
            fontDataSO.ApplyModifiedProperties();
        }

        EditorGUILayout.Space(10);

        // Font data (TextMeshPro).
        script.fontListDataTMP = (LanguageFontListDataTMP)EditorGUILayout.ObjectField("Font List Data (TMP):", script.fontListDataTMP, typeof(LanguageFontListDataTMP), false);

        if (script.fontListDataTMP != null)
        {
            SerializedObject tmpFontDataSO = new(script.fontListDataTMP);
            EditorGUILayout.PropertyField(tmpFontDataSO.FindProperty("TMPFontList"), new GUIContent("Fonts (TMP)"), true);
            tmpFontDataSO.ApplyModifiedProperties();
        }

        EditorGUILayout.Space(15);

        // Canvas log reference.
        EditorGUILayout.LabelField("Canvas Log", titleStyle);
        script.errorLanguageTool = (GameObject)EditorGUILayout.ObjectField("Error Language Tool:", script.errorLanguageTool, typeof(GameObject), true);

        EditorGUILayout.Space(10);

        // Visualization of extracted runtime data.
        GUI.enabled = false;
        EditorGUILayout.LabelField("Extracted Data", titleStyle);
        script.selectedCulture = EditorGUILayout.TextField("Selected Culture:", script.selectedCulture);

        SerializedObject scriptSO = new(script);
        EditorGUILayout.PropertyField(scriptSO.FindProperty("availableLanguages"), new GUIContent("Available Languages"), true);
        EditorGUILayout.PropertyField(scriptSO.FindProperty("idData"), new GUIContent("IDs"), true);
        EditorGUILayout.PropertyField(scriptSO.FindProperty("idMetaData"), new GUIContent("IDs Meta"), true);
        EditorGUILayout.PropertyField(scriptSO.FindProperty("idCanvasData"), new GUIContent("IDs Canvas"), true);
        scriptSO.ApplyModifiedProperties();
        GUI.enabled = true;

        // Save changes if GUI changed.
        if (GUI.changed)
        {
            EditorUtility.SetDirty(script);
            AssetDatabase.SaveAssets();
        }
    }
}