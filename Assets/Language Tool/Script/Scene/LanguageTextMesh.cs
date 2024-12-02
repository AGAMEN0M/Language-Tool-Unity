/*
 * ---------------------------------------------------------------------------
 * Description: This script manages the translation and rendering of localized 
 *              text for legacy TextMesh objects, enabling efficient localization in Unity.
 * Author: Lucas Gomes Cecchini
 * Pseudonym: AGAMENOM
 * ---------------------------------------------------------------------------
*/

using LanguageTools.Legacy;
using LanguageTools;
using UnityEngine;

#if UNITY_EDITOR
using LanguageTools.Editor;
using UnityEditor;
#endif

// This component handles the translation and text rendering for 3D TextMesh objects using a legacy system.
[AddComponentMenu("Language/3D Object/Legacy/Language TextMesh (Legacy)")]
public class LanguageTextMesh : MonoBehaviour
{
    [Header("Settings")]
    public TextMesh text; // The TextMesh component that displays the translated text.
    public int iD = -10; // The ID used to retrieve the localized text.
    [Tooltip("Disable Text if Necessary.")]
    [SerializeField] private bool translateText = true; // A flag to enable or disable text translation.
    [Space(10)]
    [Header("Automatic Information")]
    [SerializeField] private string selectedFile; // The name of the currently selected language file.
    [Space(5)]
    [SerializeField] private string saveFile; // The path to the save file.

    // Subscribes to the OnLanguageUpdate event when the object is enabled.
    private void OnEnable()
    {
        LanguageManagerDelegate.OnLanguageUpdate += LanguageUpdate; // Subscribe to language updates.
        LanguageUpdate(); // Perform an initial update.
    }

    // Unsubscribes from the OnLanguageUpdate event when the object is disabled.
    private void OnDisable()
    {
        LanguageManagerDelegate.OnLanguageUpdate -= LanguageUpdate; // Unsubscribe from language updates.
    }

    // This method is called when the language is updated, it loads the localized text based on the iD.
    public void LanguageUpdate()
    {
        saveFile = LanguageFileManager.GetSaveFilePath(); // Retrieve the save file path.
        string line = LanguageFileManager.GetLocalizedLineByID(iD, saveFile, ref selectedFile); // Get the localized line by ID.
        ProcessLine(line); // Process the retrieved line for text and settings.
    }

    // Processes the retrieved line of text for display, font size, and font type.
    private void ProcessLine(string line)
    {
        if (translateText)
        {
            text.text = LanguageFileManager.ExtractTextBetweenBraces(line); // Update the text in the TextMesh if translation is enabled.
        }

        // Extract the font size and font list index from the line.
        string lineWithoutCurlyBraces = LanguageFileManager.RemoveTextBetweenBraces(line);
        int fontSize = LanguageFileManager.ExtractIntValue(lineWithoutCurlyBraces, "S:");
        int fontListIndex = LanguageFileManager.ExtractIntValue(lineWithoutCurlyBraces, "Font:");

        // Update the font size and font type if values are valid.
        if (fontSize != 0) text.fontSize = fontSize;
        if (fontListIndex != 0) text.font = FontAndAlignmentUtility.GetFontByIndex(fontListIndex);
    }
}

#if UNITY_EDITOR
// Custom editor for the LanguageTextMesh component in the Unity editor.
[CustomEditor(typeof(LanguageTextMesh))]
public class LanguageTextMeshEditor : Editor
{
    // Overrides the default Inspector GUI with custom elements.
    public override void OnInspectorGUI()
    {
        serializedObject.Update(); // Begin tracking property changes.
        var script = (LanguageTextMesh)target; // Reference the target script.
        LanguageEditorUtilities.DrawReadOnlyMonoScriptField(target); // Draws the script field as read-only.

        EditorGUILayout.Space(5);

        // Button to import the settings for the selected TextMesh.
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

            // Get current text and font settings from the TextMesh.
            string text = script.text.text;
            int fontSize = script.text.fontSize;
            int fontListIndex = FontAndAlignmentUtility.GetFontIndex(script.text.font);

            // Open the editor window with the component settings for modification.
            LanguageEditorUtilities.OpenEditorWindowWithComponent(script.iD, 2, text, 0, fontSize, fontListIndex, true, false, true, true);
        }

        // Display a warning if the TextMesh is missing.
        GUI.color = script.text == null ? Color.red : Color.white;
        EditorGUILayout.PropertyField(serializedObject.FindProperty("text")); // TextMesh property field.
        GUI.color = Color.white;

        // If the ID is already saved, display a warning and highlight the ID field.
        if (LanguageEditorUtilities.IsIDInLanguageList(script.iD))
        {
            GUI.color = Color.yellow;
            EditorGUILayout.PropertyField(serializedObject.FindProperty("iD")); // ID property field.
            EditorGUILayout.HelpBox("There is an ID with this number Saved!", MessageType.Warning); // Warning message.
            GUI.color = Color.white;
        }
        else
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("iD")); // ID property field if no warning.
        }

        // Other fields for additional settings.
        EditorGUILayout.PropertyField(serializedObject.FindProperty("translateText")); // Toggle for enabling/disabling translation.
        EditorGUILayout.PropertyField(serializedObject.FindProperty("selectedFile")); // Field for the selected file path.
        EditorGUILayout.PropertyField(serializedObject.FindProperty("saveFile")); // Field for the save file path.

        serializedObject.ApplyModifiedProperties(); // Apply changes to serialized properties.
    }
}
#endif