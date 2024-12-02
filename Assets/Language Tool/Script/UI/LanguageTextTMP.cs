/*
 * ---------------------------------------------------------------------------
 * Description: This component handles the localization of TextMesh Pro (TMP) 
 *              components based on the selected language. It updates the text 
 *              and associated properties such as alignment and font size according 
 *              to the localization settings.
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

// This component handles the localization of TextMesh Pro (TMP) components based on the selected language.
[AddComponentMenu("Language/UI/TextMesh Pro/Language Text (TMP)")]
public class LanguageTextTMP : MonoBehaviour
{
    [Header("Settings")]
    public TMP_Text text; // Reference to the TMP_Text component that will display the localized text.
    public int iD = -7; // The ID used to reference the specific line of text in the language file.
    [Tooltip("Disable Text if Necessary.")]
    [SerializeField] private bool translateText = true; // Option to disable translation if necessary.
    [Space(10)]
    [Header("Automatic Information")]
    [SerializeField] private string selectedFile; // The selected language file path.
    [Space(5)]
    [SerializeField] private string saveFile; // Path to the save file that stores user preferences.

    // Subscribes to the OnLanguageUpdate event when the object is enabled.
    private void OnEnable()
    {
        LanguageManagerDelegate.OnLanguageUpdate += LanguageUpdate; // Subscribe to language updates.
        LanguageUpdate(); // Perform an initial update when enabled.
    }

    // Unsubscribes from the OnLanguageUpdate event when the object is disabled.
    private void OnDisable()
    {
        LanguageManagerDelegate.OnLanguageUpdate -= LanguageUpdate; // Unsubscribe from language updates.
    }

    // Updates the language settings and applies the localized text to the TMP_Text component.
    public void LanguageUpdate()
    {
        saveFile = LanguageFileManager.GetSaveFilePath(); // Retrieve the path to the save file.
        string line = LanguageFileManager.GetLocalizedLineByID(iD, saveFile, ref selectedFile); // Get the localized line of text by ID.
        ProcessLine(line); // Process and apply the localized line.
    }

    // Processes the retrieved line of text and updates the TMP_Text component's properties (text, alignment, font, etc.).
    private void ProcessLine(string line)
    {
        // If translation is enabled, update the TMP_Text component with the localized text inside the curly braces.
        if (translateText) text.text = LanguageFileManager.ExtractTextBetweenBraces(line);

        // Extract properties such as alignment, font size, and font index from the remaining line.
        string lineWithoutCurlyBraces = LanguageFileManager.RemoveTextBetweenBraces(line);
        int alignment = LanguageFileManager.ExtractIntValue(lineWithoutCurlyBraces, "Ali:");
        int fontSize = LanguageFileManager.ExtractIntValue(lineWithoutCurlyBraces, "S:");
        int fontListIndex = LanguageFileManager.ExtractIntValue(lineWithoutCurlyBraces, "Font:");

        // Update the alignment of the TMP_Text component if a valid alignment is found.
        if (alignment != 0) text.alignment = FontAndAlignmentUtilityTMP.ConvertToTextAnchor(alignment);

        // Update the font size if a valid size is found.
        if (fontSize != 0) text.fontSize = fontSize;

        // Update the font if a valid font index is found.
        if (fontListIndex != 0) text.font = FontAndAlignmentUtilityTMP.GetFontByIndex(fontListIndex);
    }
}

#if UNITY_EDITOR
// Custom editor for the LanguageTextTMP component in the Unity editor.
[CustomEditor(typeof(LanguageTextTMP))]
public class LanguageTextTMPEditor : Editor
{
    // Overrides the default Inspector GUI with custom fields and buttons.
    public override void OnInspectorGUI()
    {
        serializedObject.Update(); // Update the serialized properties of the object.
        var script = (LanguageTextTMP)target; // Reference to the target LanguageTextTMP script.
        LanguageEditorUtilities.DrawReadOnlyMonoScriptField(target); // Draw a read-only field for the script reference.

        EditorGUILayout.Space(5);

        // Button to import the settings for the TMP_Text component.
        if (GUILayout.Button("Import Settings", LanguageEditorUtilities.CreateCustomButtonStyle(15), GUILayout.Height(30)))
        {
            // Check if the current ID is already saved in the language list.
            if (LanguageEditorUtilities.IsIDInLanguageList(script.iD))
            {
                // Ask the user if they want to replace the existing ID with the new one.
                if (!EditorUtility.DisplayDialog("Replace ID", "An ID with this number is already saved. Do you want to replace it?", "Yes", "No"))
                {
                    return; // Exit if the user chooses not to replace the ID.
                }
            }

            // Get the text, alignment, font size, and font index from the current TMP_Text component.
            string text = script.text.text;
            int alignment = FontAndAlignmentUtilityTMP.ConvertToAlignmentCode(script.text.alignment);
            int fontSize = (int)script.text.fontSize;
            int fontListIndex = FontAndAlignmentUtilityTMP.GetFontIndex(script.text.font);

            // Open the editor window to modify these settings.
            LanguageEditorUtilities.OpenEditorWindowWithComponent(script.iD, 1, text, alignment, fontSize, fontListIndex, true, true, true, true);
        }

        // Highlight the text field in red if it is not assigned.
        GUI.color = script.text == null ? Color.red : Color.white;
        EditorGUILayout.PropertyField(serializedObject.FindProperty("text"));
        GUI.color = Color.white;

        // Display a warning if the current ID is already saved in the language list.
        if (LanguageEditorUtilities.IsIDInLanguageList(script.iD))
        {
            GUI.color = Color.yellow;
            EditorGUILayout.PropertyField(serializedObject.FindProperty("iD"));
            EditorGUILayout.HelpBox("There is an ID with this number Saved!", MessageType.Warning);
            GUI.color = Color.white;
        }
        else
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("iD"));
        }

        // Fields for enabling/disabling translation, and for displaying the selected and saved file paths.
        EditorGUILayout.PropertyField(serializedObject.FindProperty("translateText"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("selectedFile"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("saveFile"));

        serializedObject.ApplyModifiedProperties(); // Apply any modified properties.
    }
}
#endif