/*
 * ---------------------------------------------------------------------------
 * Description: This component localizes the InputField text and placeholder 
 *              in a legacy UI system. It updates the displayed text and properties such 
 *              as alignment and font size based on localization settings retrieved from 
 *              the language files.
 * Author: Lucas Gomes Cecchini
 * Pseudonym: AGAMENOM
 * ---------------------------------------------------------------------------
*/

using LanguageTools.Legacy;
using LanguageTools;
using UnityEngine.UI;
using UnityEngine;

#if UNITY_EDITOR
using LanguageTools.Editor;
using UnityEditor;
#endif

// This component localizes the InputField text and placeholder in a legacy UI system.
[AddComponentMenu("Language/UI/Legacy/Language Text Input Field (Legacy)")]
public class LanguageTextInputField : MonoBehaviour
{
    [Header("Settings")]
    public InputField inputField; // The InputField to be localized.
    public int iD = -6; // The ID used to identify the localized text in the language files.
    [Space(10)]
    [Header("Automatic Information")]
    [SerializeField] private string selectedFile; // Path to the selected language file.
    [Space(5)]
    [SerializeField] private string saveFile; // Path to the save file containing user preferences.

    private Text text; // The InputField's main text component.
    private Text placeholder; // The InputField's placeholder text component.

    // Subscribes to the OnLanguageUpdate event when the component is enabled.
    private void OnEnable()
    {
        LanguageManagerDelegate.OnLanguageUpdate += LanguageUpdate;
        LanguageUpdate(); // Perform an initial language update.
    }

    // Unsubscribes from the OnLanguageUpdate event when the component is disabled.
    private void OnDisable()
    {
        LanguageManagerDelegate.OnLanguageUpdate -= LanguageUpdate;
    }

    // Updates the language settings for the InputField's text and placeholder.
    public void LanguageUpdate()
    {
        // Try to get the placeholder component from the InputField.
        if (!inputField.placeholder.TryGetComponent(out placeholder))
        {
            Debug.LogError("Placeholder component is missing or not a Text.");
            return;
        }

        // Try to get the text component from the InputField.
        if (!inputField.textComponent.TryGetComponent(out text))
        {
            Debug.LogError("Text component is missing or not a Text.");
            return;
        }

        saveFile = LanguageFileManager.GetSaveFilePath(); // Retrieve the path to the save file.
        string line = LanguageFileManager.GetLocalizedLineByID(iD, saveFile, ref selectedFile); // Get the localized line by ID.
        ProcessLine(line); // Process and apply the localized line.
    }

    // Processes the retrieved line of text and updates the InputField's placeholder and main text.
    private void ProcessLine(string line)
    {
        // Extract the text inside curly braces and assign it to the placeholder.
        placeholder.text = LanguageFileManager.ExtractTextBetweenBraces(line);

        // Remove the curly braces and extract alignment, font size, and font index from the remaining line.
        string lineWithoutCurlyBraces = LanguageFileManager.RemoveTextBetweenBraces(line);
        int alignment = LanguageFileManager.ExtractIntValue(lineWithoutCurlyBraces, "Ali:");
        int fontSize = LanguageFileManager.ExtractIntValue(lineWithoutCurlyBraces, "S:");
        int fontListIndex = LanguageFileManager.ExtractIntValue(lineWithoutCurlyBraces, "Font:");

        // If an alignment is found, apply it to both the placeholder and main text.
        if (alignment != 0)
        {
            TextAnchor correctAlignment = FontAndAlignmentUtility.ConvertToTextAnchor(alignment);
            text.alignment = correctAlignment;
            placeholder.alignment = correctAlignment;
        }

        // If a font size is found, apply it to both the placeholder and main text.
        if (fontSize != 0)
        {
            text.fontSize = fontSize;
            placeholder.fontSize = fontSize;
        }

        // If a font index is found, apply the corresponding font to both the placeholder and main text.
        if (fontListIndex != 0)
        {
            Font correctFont = FontAndAlignmentUtility.GetFontByIndex(fontListIndex);
            text.font = correctFont;
            placeholder.font = correctFont;
        }
    }
}

#if UNITY_EDITOR
// Custom editor for the LanguageTextInputField component in the Unity editor.
[CustomEditor(typeof(LanguageTextInputField))]
public class LanguageTextInputFieldEditor : Editor
{
    // Overrides the default Inspector GUI with custom fields and buttons.
    public override void OnInspectorGUI()
    {
        serializedObject.Update(); // Update the serialized properties of the object.
        var script = (LanguageTextInputField)target; // Reference to the target LanguageTextInputField script.
        LanguageEditorUtilities.DrawReadOnlyMonoScriptField(target); // Draw a read-only field for the script reference.

        EditorGUILayout.Space(5);

        // Button to import the settings for the InputField's placeholder text.
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

            // Try to retrieve the InputField and placeholder components and import their settings.
            if (script.inputField != null && script.inputField.placeholder.TryGetComponent<Text>(out var placeholder))
            {
                string text = placeholder.text;
                int alignment = FontAndAlignmentUtility.ConvertToAlignmentCode(placeholder.alignment);
                int fontSize = placeholder.fontSize;
                int fontListIndex = FontAndAlignmentUtility.GetFontIndex(placeholder.font);
                LanguageEditorUtilities.OpenEditorWindowWithComponent(script.iD, 3, text, alignment, fontSize, fontListIndex, true, true, true, true);
            }
            else
            {
                Debug.LogError("InputField or Placeholder component is missing.");
            }
        }

        // Highlight the inputField field in red if it is not assigned.
        GUI.color = script.inputField == null ? Color.red : Color.white;
        EditorGUILayout.PropertyField(serializedObject.FindProperty("inputField"));
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

        // Fields for displaying the selected and saved file paths.
        EditorGUILayout.PropertyField(serializedObject.FindProperty("selectedFile"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("saveFile"));

        serializedObject.ApplyModifiedProperties(); // Apply any modified properties.
    }
}
#endif