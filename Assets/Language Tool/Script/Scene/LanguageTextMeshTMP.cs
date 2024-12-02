/*
 * ---------------------------------------------------------------------------
 * Description: This script handles the translation and rendering of localized 
 *              text for TextMesh Pro objects, enabling easy localization in Unity.
 * Author: Lucas Gomes Cecchini
 * Pseudonym: AGAMENOM
 * ---------------------------------------------------------------------------
*/

using LanguageTools.TMP;
using LanguageTools;
using UnityEngine;
using TMPro;

#if UNITY_EDITOR
using LanguageTools.Editor;
using UnityEditor;
#endif

// This component handles the translation and text rendering for TextMesh Pro (TMP) objects.
[AddComponentMenu("Language/3D Object/TextMesh Pro/Language TextMesh (TMP)")]
public class LanguageTextMeshTMP : MonoBehaviour
{
    [Header("Settings")]
    public TMP_Text text; // The TMP_Text component that will display the localized text.
    public int iD = -10; // The ID used to retrieve the localized text from the file.
    [Tooltip("Disable Text if Necessary.")]
    [SerializeField] private bool translateText = true; // A flag to enable or disable text translation.
    [Space(10)]
    [Header("Automatic Information")]
    [SerializeField] private string selectedFile; // Stores the path of the selected language file.
    [Space(5)]
    [SerializeField] private string saveFile; // Stores the path to the save file.

    // Subscribes to the language update event when the object is enabled.
    private void OnEnable()
    {
        LanguageManagerDelegate.OnLanguageUpdate += LanguageUpdate; // Subscribe to language updates.
        LanguageUpdate(); // Perform an initial update when enabled.
    }

    // Unsubscribes from the language update event when the object is disabled.
    private void OnDisable()
    {
        LanguageManagerDelegate.OnLanguageUpdate -= LanguageUpdate; // Unsubscribe from language updates.
    }

    // Updates the text and formatting based on the current language settings.
    public void LanguageUpdate()
    {
        saveFile = LanguageFileManager.GetSaveFilePath(); // Retrieve the path to the save file.
        string line = LanguageFileManager.GetLocalizedLineByID(iD, saveFile, ref selectedFile); // Fetch the localized line by ID.
        ProcessLine(line); // Process and apply the line's content to the TMP_Text component.
    }

    // Processes the localized line, extracting the text, font size, and font to apply to the TMP_Text component.
    private void ProcessLine(string line)
    {
        if (translateText)
        {
            text.text = LanguageFileManager.ExtractTextBetweenBraces(line); // Extract the text within curly braces for translation.
        }

        // Extract additional information such as font size and font index.
        string lineWithoutCurlyBraces = LanguageFileManager.RemoveTextBetweenBraces(line); // Remove the text within braces.
        int fontSize = LanguageFileManager.ExtractIntValue(lineWithoutCurlyBraces, "S:"); // Extract the font size.
        int fontListIndex = LanguageFileManager.ExtractIntValue(lineWithoutCurlyBraces, "Font:"); // Extract the font index.

        // Apply the extracted font size and font type if valid values are found.
        if (fontSize != 0) text.fontSize = fontSize;
        if (fontListIndex != 0) text.font = FontAndAlignmentUtilityTMP.GetFontByIndex(fontListIndex);
    }
}

#if UNITY_EDITOR
// Custom editor for the LanguageTextMeshTMP component in the Unity editor.
[CustomEditor(typeof(LanguageTextMeshTMP))]
public class LanguageTextMeshTMPEditor : Editor
{
    // Overrides the default Inspector GUI with custom fields and buttons.
    public override void OnInspectorGUI()
    {
        serializedObject.Update(); // Update the serialized properties of the object.
        var script = (LanguageTextMeshTMP)target; // Reference to the target script.
        LanguageEditorUtilities.DrawReadOnlyMonoScriptField(target); // Draw the read-only field for the script reference.

        EditorGUILayout.Space(5);

        // Button to import and apply the text and font settings for the TextMeshPro component.
        if (GUILayout.Button("Import Settings", LanguageEditorUtilities.CreateCustomButtonStyle(15), GUILayout.Height(30)))
        {
            // Check if the ID is already saved.
            if (LanguageEditorUtilities.IsIDInLanguageList(script.iD))
            {
                // Prompt the user to confirm if they want to replace the saved ID.
                if (!EditorUtility.DisplayDialog("Replace ID", "An ID with this number is already saved. Do you want to replace it?", "Yes", "No"))
                {
                    return; // Exit if the user chooses not to replace the ID.
                }
            }

            // Retrieve the current text, font size, and font index from the TMP_Text component.
            string text = script.text.text;
            int fontSize = (int)script.text.fontSize;
            int fontListIndex = FontAndAlignmentUtilityTMP.GetFontIndex(script.text.font);

            // Open the editor window with the component settings for modification.
            LanguageEditorUtilities.OpenEditorWindowWithComponent(script.iD, 2, text, 0, fontSize, fontListIndex, true, false, true, true);
        }

        // Display a warning if the TMP_Text component is missing.
        GUI.color = script.text == null ? Color.red : Color.white;
        EditorGUILayout.PropertyField(serializedObject.FindProperty("text")); // Field for assigning the TMP_Text component.
        GUI.color = Color.white;

        // If the ID is already saved, display a warning and highlight the ID field.
        if (LanguageEditorUtilities.IsIDInLanguageList(script.iD))
        {
            GUI.color = Color.yellow;
            EditorGUILayout.PropertyField(serializedObject.FindProperty("iD")); // Field for assigning the text ID.
            EditorGUILayout.HelpBox("There is an ID with this number Saved!", MessageType.Warning); // Warning message if the ID already exists.
            GUI.color = Color.white;
        }
        else
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("iD")); // Field for assigning the text ID.
        }

        // Other fields for additional settings.
        EditorGUILayout.PropertyField(serializedObject.FindProperty("translateText")); // Field for enabling or disabling text translation.
        EditorGUILayout.PropertyField(serializedObject.FindProperty("selectedFile")); // Field showing the selected file.
        EditorGUILayout.PropertyField(serializedObject.FindProperty("saveFile")); // Field showing the path to the save file.

        serializedObject.ApplyModifiedProperties(); // Apply any modified properties.
    }
}
#endif