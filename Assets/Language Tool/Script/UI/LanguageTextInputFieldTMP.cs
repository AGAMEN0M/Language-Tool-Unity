/*
 * ---------------------------------------------------------------------------
 * Description: This component localizes the TMP_InputField text and placeholder 
 *              within the TextMeshPro system based on the selected language. It updates 
 *              the displayed text and properties such as alignment and font size 
 *              according to localization settings retrieved from the language files.
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

// This component localizes the TMP_InputField text and placeholder in the TextMeshPro system.
[AddComponentMenu("Language/UI/TextMesh Pro/Language Text Input Field (TMP)")]
public class LanguageTextInputFieldTMP : MonoBehaviour
{
    [Header("Settings")]
    public TMP_InputField inputField; // The TMP_InputField to be localized.
    public int iD = -6; // The ID used to identify the localized text in the language files.
    [Space(10)]
    [Header("Automatic Information")]
    [SerializeField] private string selectedFile; // Path to the selected language file.
    [Space(5)]
    [SerializeField] private string saveFile; // Path to the save file containing user preferences.

    private TMP_Text text; // The TMP_InputField's main text component.
    private TMP_Text placeholder; // The TMP_InputField's placeholder text component.

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

    // Updates the language settings for the TMP_InputField's text and placeholder.
    public void LanguageUpdate()
    {
        // Try to get the placeholder component from the TMP_InputField.
        if (!inputField.placeholder.TryGetComponent(out placeholder))
        {
            Debug.LogError("Placeholder component is missing or not a TMP_Text.");
            return;
        }

        // Try to get the text component from the TMP_InputField.
        if (!inputField.textComponent.TryGetComponent(out text))
        {
            Debug.LogError("Text component is missing or not a TMP_Text.");
            return;
        }

        saveFile = LanguageFileManager.GetSaveFilePath(); // Retrieve the path to the save file.
        string line = LanguageFileManager.GetLocalizedLineByID(iD, saveFile, ref selectedFile); // Get the localized line by ID.
        ProcessLine(line); // Process and apply the localized line.
    }

    // Processes the retrieved line of text and updates the TMP_InputField's placeholder and main text.
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
            TextAlignmentOptions correctAlignment = FontAndAlignmentUtilityTMP.ConvertToTextAnchor(alignment);
            text.alignment = correctAlignment;
            placeholder.alignment = correctAlignment;
        }

        // If a font size is found, apply it to the TMP_InputField.
        if (fontSize != 0) inputField.pointSize = fontSize;

        // If a font index is found, apply the corresponding font to the TMP_InputField.
        if (fontListIndex != 0) inputField.fontAsset = FontAndAlignmentUtilityTMP.GetFontByIndex(fontListIndex);
    }
}

#if UNITY_EDITOR
// Custom editor for the LanguageTextInputFieldTMP component in the Unity editor.
[CustomEditor(typeof(LanguageTextInputFieldTMP))]
public class LanguageTextInputFieldTMPEditor : Editor
{
    // Overrides the default Inspector GUI with custom fields and buttons.
    public override void OnInspectorGUI()
    {
        serializedObject.Update(); // Update the serialized properties of the object.
        var script = (LanguageTextInputFieldTMP)target; // Reference to the target LanguageTextInputFieldTMP script.
        LanguageEditorUtilities.DrawReadOnlyMonoScriptField(target); // Draw a read-only field for the script reference.

        EditorGUILayout.Space(5);

        // Button to import the settings for the TMP_InputField's placeholder text.
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

            // Try to retrieve the TMP_InputField and placeholder components and import their settings.
            if (script.inputField != null && script.inputField.placeholder.TryGetComponent<TMP_Text>(out var placeholder))
            {
                string text = placeholder.text;
                int alignment = FontAndAlignmentUtilityTMP.ConvertToAlignmentCode(placeholder.alignment);
                int fontSize = (int)script.inputField.pointSize;
                int fontListIndex = FontAndAlignmentUtilityTMP.GetFontIndex(script.inputField.fontAsset);
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