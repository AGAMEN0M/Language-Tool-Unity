/*
 * ---------------------------------------------------------------------------
 * Description: A custom editor for the LanguageSettingsData ScriptableObject 
 *              in Unity. This script provides a user-friendly interface for managing 
 *              language settings, including default language selection, font lists, 
 *              and archive locations for both Unity Editor and builds. It facilitates 
 *              editing and visualizing properties related to language settings and 
 *              ensures that changes can be easily tracked and saved within the Unity Editor.
 * Author: Lucas Gomes Cecchini
 * Pseudonym: AGAMENOM
 * ---------------------------------------------------------------------------
*/

using System.Globalization;
using UnityEditor;
using UnityEngine;
using System;

// Custom inspector for the LanguageSettingsData ScriptableObject.
[CustomEditor(typeof(LanguageSettingsData))]
public class LanguageSettingsDataEditor : Editor
{
    private string[] availableCultureDisplayNames; // Array to store culture display names for the dropdown.
    private CultureInfo[] availableCultures; // Array to store all available CultureInfo objects.
    private int currentSelectedCultureIndex; // Index of the currently selected culture in the dropdown.

    private void OnEnable()
    {
        // Initialize the cultures and culture names for the dropdown.
        availableCultures = CultureInfo.GetCultures(CultureTypes.AllCultures); // Get all available cultures.
        availableCultureDisplayNames = new string[availableCultures.Length]; // Initialize the array for culture names.

        // Populate the cultureNames array with display names of the cultures.
        for (int i = 0; i < availableCultures.Length; i++)
        {
            availableCultureDisplayNames[i] = availableCultures[i].DisplayName;
        }
    }

    public override void OnInspectorGUI()
    {
        var script = (LanguageSettingsData)target; // Cast the target object to LanguageSettingsData.

        // Create a style for titles with bold font and size 14.
        GUIStyle titleStyle = new(EditorStyles.boldLabel)
        {
            fontSize = 14
        };

        Undo.RecordObject(script, "Language Settings Data"); // Record the object to allow undo operations.

        // Disable the script field to prevent editing.
        EditorGUI.BeginDisabledGroup(true);
        EditorGUILayout.ObjectField("Script", MonoScript.FromScriptableObject(script), typeof(MonoScript), false);
        EditorGUI.EndDisabledGroup();

        EditorGUILayout.Space(10);

        EditorGUILayout.LabelField("Archives Location", titleStyle);
        // Draw and edit the path for Unity Editor.
        EditorGUILayout.LabelField("in Unity Editor", EditorStyles.boldLabel);
        script.saveNameInUnity = EditorGUILayout.TextField("Save Name:", script.saveNameInUnity);
        // Draw and edit the path for build.
        EditorGUILayout.LabelField("In Build", EditorStyles.boldLabel);
        script.saveNameInBuild = EditorGUILayout.TextField("Save Name:", script.saveNameInBuild);
        // Draw and edit the folder name.
        EditorGUILayout.LabelField("Assets", EditorStyles.boldLabel);
        script.folderName = EditorGUILayout.TextField("Folder Name:", script.folderName);

        EditorGUILayout.Space(15);

        EditorGUILayout.LabelField("Default Language", titleStyle);
        string currentLanguage = script.defaultLanguage;
        currentSelectedCultureIndex = Array.FindIndex(availableCultures, c => c.DisplayName == currentLanguage); // Find the index of the current language.
        currentSelectedCultureIndex = EditorGUILayout.Popup("Language:", currentSelectedCultureIndex, availableCultureDisplayNames); // Show the dropdown for selecting language.
        script.defaultLanguage = availableCultures[currentSelectedCultureIndex].DisplayName; // Update the default language based on selection.

        EditorGUILayout.Space(15);

        EditorGUILayout.LabelField("Font List Data", titleStyle);

        // Draw and edit the LanguageFontListData reference.
        script.fontListData = (LanguageFontListData)EditorGUILayout.ObjectField("Font List Data:", script.fontListData, typeof(LanguageFontListData), false);

        // If fontListData is assigned, draw its properties.
        if (script.fontListData != null)
        {
            SerializedObject serializedFontListData = new(script.fontListData); // Create a SerializedObject for the fontListData.
            SerializedProperty fontListProperty = serializedFontListData.FindProperty("fontList"); // Find the "fontList" property.
            EditorGUILayout.PropertyField(fontListProperty, new GUIContent("Fonts"), true); // Draw the property field with a label.
            serializedFontListData.ApplyModifiedProperties(); // Apply any changes made to the SerializedObject.
        }

        EditorGUILayout.Space(10);

        // Draw and edit the LanguageFontListDataTMP reference.
        script.fontListDataTMP = (LanguageFontListDataTMP)EditorGUILayout.ObjectField("Font List Data (TMP):", script.fontListDataTMP, typeof(LanguageFontListDataTMP), false);

        // If fontListDataTMP is assigned, draw its properties.
        if (script.fontListDataTMP != null)
        {
            SerializedObject serializedTMPFontListData = new(script.fontListDataTMP); // Create a SerializedObject for the fontListDataTMP.
            SerializedProperty tmpFontListProperty = serializedTMPFontListData.FindProperty("TMPFontList"); // Find the "TMPFontList" property.
            EditorGUILayout.PropertyField(tmpFontListProperty, new GUIContent("Fonts (TMP)"), true); // Draw the property field with a label.
            serializedTMPFontListData.ApplyModifiedProperties(); // Apply any changes made to the SerializedObject.
        }

        EditorGUILayout.Space(15);

        // Draw and edit the error language tool GameObject reference.
        EditorGUILayout.LabelField("Canvas Log", titleStyle);
        script.errorLanguageTool = (GameObject)EditorGUILayout.ObjectField("Error Language Tool:", script.errorLanguageTool, typeof(GameObject), true);

        // If any changes have been made in the inspector, mark the script as dirty and save assets.
        if (GUI.changed)
        {
            EditorUtility.SetDirty(script);
            AssetDatabase.SaveAssets();
        }
    }
}