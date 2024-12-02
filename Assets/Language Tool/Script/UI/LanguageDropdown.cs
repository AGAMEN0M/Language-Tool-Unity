/*
 * ---------------------------------------------------------------------------
 * Description: This script integrates language settings into a legacy Unity 
 *              UI Dropdown. It dynamically updates the dropdown options and styles (alignment, 
 *              font, font size) based on the currently selected language, ensuring that the 
 *              UI adapts to different languages.
 * Author: Lucas Gomes Cecchini
 * Pseudonym: AGAMENOM
 * ---------------------------------------------------------------------------
*/

using System.Collections.Generic;
using LanguageTools.Legacy;
using UnityEngine.UI;
using LanguageTools;
using UnityEngine;

#if UNITY_EDITOR
using LanguageTools.Editor;
using UnityEditor;
#endif

// Adds this script to the Unity Component Menu.
[AddComponentMenu("Language/UI/Legacy/Language Dropdown (Legacy)")]
public class LanguageDropdown : MonoBehaviour
{
    [Header("Settings")]
    public Dropdown dropdown; // Reference to the UI Dropdown component to update.
    [Space(10)]
    public List<LanguageOptions> options = new()
    {
        // Predefined options for the dropdown.
        new LanguageOptions { text = "Option A", sprite = null, iD = -2 },
        new LanguageOptions { text = "Option B", sprite = null, iD = -3 },
        new LanguageOptions { text = "Option C", sprite = null, iD = -4 }
    };
    [Space(10)]
    [Header("Automatic Information")]
    [SerializeField] private string selectedFile; // Holds the path to the currently selected language file.
    [Space(5)]
    [SerializeField] private string saveFile; // Path to the saved language settings file.

    private Text captionText; // Reference to the caption text of the dropdown.
    private Text itemText; // Reference to the text for each dropdown item.

    // Subscribes to the LanguageUpdate event and calls the LanguageUpdate method when the object is enabled.
    private void OnEnable()
    {
        LanguageManagerDelegate.OnLanguageUpdate += LanguageUpdate;
        LanguageUpdate(); // Call to update language data immediately when enabled.
    }

    // Unsubscribes from the LanguageUpdate event when the object is disabled.
    private void OnDisable()
    {
        LanguageManagerDelegate.OnLanguageUpdate -= LanguageUpdate;
    }

    // Method that updates the dropdown's appearance and options based on the current language settings.
    public void LanguageUpdate()
    {
        // Assign caption text from the dropdown component.
        captionText = dropdown.captionText;
        if (captionText == null)
        {
            Debug.LogError("Caption Text is null.");
            return;
        }

        // Assign item text from the dropdown component.
        itemText = dropdown.itemText;
        if (itemText == null)
        {
            Debug.LogError("Item Text is null.");
            return;
        }

        // Check if options are available.
        if (options == null || options.Count == 0)
        {
            Debug.LogWarning("Options list is null or empty.");
            return;
        }

        saveFile = LanguageFileManager.GetSaveFilePath(); // Load the save file path.

        // Get the localized text for the first option and update the caption text.
        string line = LanguageFileManager.GetLocalizedLineByID(options[0].iD, saveFile, ref selectedFile);
        ProcessLine(line);
        ProcessOption();

        // Set the caption text to the first option's localized text if no selection is made.
        if (dropdown.value == 0) captionText.text = LanguageFileManager.ExtractTextBetweenBraces(line);
    }

    // Method to process and apply the alignment, font, and font size to the dropdown components.
    private void ProcessLine(string line)
    {
        // Extract formatting data from the line (alignment, font size, font index).
        string lineWithoutCurlyBraces = LanguageFileManager.RemoveTextBetweenBraces(line);
        int alignment = LanguageFileManager.ExtractIntValue(lineWithoutCurlyBraces, "Ali:");
        int fontSize = LanguageFileManager.ExtractIntValue(lineWithoutCurlyBraces, "S:");
        int fontListIndex = LanguageFileManager.ExtractIntValue(lineWithoutCurlyBraces, "Font:");

        // Apply alignment settings if specified.
        if (alignment != 0)
        {
            TextAnchor correctAlignment = FontAndAlignmentUtility.ConvertToTextAnchor(alignment);
            captionText.alignment = correctAlignment;
            itemText.alignment = correctAlignment;
        }

        // Apply font size if specified.
        if (fontSize != 0)
        {
            captionText.fontSize = fontSize;
            itemText.fontSize = fontSize;
        }

        // Apply font if specified.
        if (fontListIndex != 0)
        {
            Font correctFont = FontAndAlignmentUtility.GetFontByIndex(fontListIndex);
            captionText.font = correctFont;
            itemText.font = correctFont;
        }
    }

    // Method to update the options in the dropdown with the localized text.
    private void ProcessOption()
    {
        // Iterate through each option and update its text with the localized value.
        foreach (LanguageOptions i in options)
        {
            string text = LanguageFileManager.FindLineByID(selectedFile, i.iD);
            i.text = LanguageFileManager.ExtractTextBetweenBraces(text);
        }

        // Preserve the previously selected index.
        int previousIndex = dropdown.value;
        dropdown.ClearOptions(); // Clear existing dropdown options.

        // Add each option to the dropdown.
        foreach (LanguageOptions i in options)
        {
            Dropdown.OptionData optionData = new()
            {
                text = i.text,
                image = i.sprite
            };
            dropdown.options.Add(optionData);
        }

        dropdown.value = previousIndex; // Restore the previously selected value.
    }
}

#if UNITY_EDITOR
// Custom Editor class for the LanguageDropdown component.
[CustomEditor(typeof(LanguageDropdown))]
public class LanguageDropdownEditor : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update(); // Update serialized properties.
        var script = (LanguageDropdown)target; // Reference to the LanguageDropdown script.
        LanguageEditorUtilities.DrawReadOnlyMonoScriptField(target); // Display a read-only field for the script.

        EditorGUILayout.Space(5);

        // Button to import settings for the dropdown component from language settings.
        if (GUILayout.Button("Import Settings", LanguageEditorUtilities.CreateCustomButtonStyle(15), GUILayout.Height(30)))
        {
            bool alreadySaved = false;
            foreach (LanguageOptions i in script.options)
            {
                if (LanguageEditorUtilities.IsIDInLanguageList(i.iD))
                {
                    alreadySaved = true;
                    break;
                }
            }

            // Show a dialog to confirm replacement of an existing ID.
            if (alreadySaved)
            {
                if (!EditorUtility.DisplayDialog("Replace ID", "An ID with this number is already saved. Do you want to replace it?", "Yes", "No"))
                {
                    return;
                }
            }

            // Import the alignment, font size, and font settings.
            int alignment = FontAndAlignmentUtility.ConvertToAlignmentCode(script.dropdown.captionText.alignment);
            int fontSize = script.dropdown.captionText.fontSize;
            int fontListIndex = FontAndAlignmentUtility.GetFontIndex(script.dropdown.captionText.font);

            // Open the editor window to customize the dropdown's first option.
            LanguageEditorUtilities.OpenEditorWindowWithComponent(script.options[0].iD, 6, script.options[0].text, alignment, fontSize, fontListIndex, true, true, true, true);

            // Loop through remaining options and open the editor for each.
            for (int i = 1; i < script.options.Count; i++)
            {
                LanguageOptions option = script.options[i];
                LanguageEditorUtilities.OpenEditorWindowWithComponent(option.iD, 6, option.text, 0, 0, 0, true, false, false, false);
            }
        }

        // Display a warning if the dropdown is null.
        GUI.color = script.dropdown == null ? Color.red : Color.white;
        EditorGUILayout.PropertyField(serializedObject.FindProperty("dropdown")); // Field to assign the dropdown.
        GUI.color = Color.white;

        // Display and allow editing of the options list.
        EditorGUILayout.PropertyField(serializedObject.FindProperty("options"));
        foreach (LanguageOptions i in script.options)
        {
            // Highlight the option if the ID already exists in the saved list.
            if (LanguageEditorUtilities.IsIDInLanguageList(i.iD))
            {
                GUI.color = Color.yellow;
                EditorGUILayout.HelpBox($"There is an ID ({i.iD}) with this number Saved!", MessageType.Warning);
                GUI.color = Color.white;
            }
        }

        // Display fields for file selection and saving.
        EditorGUILayout.PropertyField(serializedObject.FindProperty("selectedFile"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("saveFile"));

        serializedObject.ApplyModifiedProperties(); // Apply any modified properties.
    }
}
#endif