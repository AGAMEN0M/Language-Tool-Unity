/*
 * ---------------------------------------------------------------------------
 * Description: This script integrates language settings into a TextMesh Pro (TMP) 
 *              dropdown in Unity. It dynamically updates the dropdown options and text styles 
 *              (alignment, font, font size) based on the currently selected language, ensuring 
 *              that the interface adapts to different languages.
 * Author: Lucas Gomes Cecchini
 * Pseudonym: AGAMENOM
 * ---------------------------------------------------------------------------
*/

using System.Collections.Generic;
using LanguageTools.TMP;
using LanguageTools;
using UnityEngine;
using TMPro;

#if UNITY_EDITOR
using LanguageTools.Editor;
using UnityEditor;
#endif

// Adds this script to Unity's Component Menu under "Language" and "UI/TextMesh Pro".
[AddComponentMenu("Language/UI/TextMesh Pro/Language Dropdown (TMP)")]
public class LanguageDropdownTMP : MonoBehaviour
{
    [Header("Settings")]
    public TMP_Dropdown dropdown; // Reference to the TMP_Dropdown component to update.
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
    [SerializeField] private string selectedFile; // Path to the currently selected language file.
    [Space(5)]
    [SerializeField] private string saveFile; // Path to the save file storing language settings.

    private TMP_Text captionText; // Reference to the caption text of the TMP dropdown.
    private TMP_Text itemText; // Reference to the item text of the TMP dropdown.

    // Subscribes to the LanguageUpdate event and immediately calls LanguageUpdate upon enabling the object.
    private void OnEnable()
    {
        LanguageManagerDelegate.OnLanguageUpdate += LanguageUpdate;
        LanguageUpdate(); // Immediate call to update dropdown content based on language settings.
    }

    // Unsubscribes from the LanguageUpdate event when the object is disabled.
    private void OnDisable()
    {
        LanguageManagerDelegate.OnLanguageUpdate -= LanguageUpdate;
    }

    // Updates the dropdown options and styles (alignment, font size, font) based on the selected language.
    public void LanguageUpdate()
    {
        // Assign the caption text from the TMP dropdown.
        captionText = dropdown.captionText;
        if (captionText == null)
        {
            Debug.LogError("Caption Text is null.");
            return;
        }

        // Assign the item text from the TMP dropdown.
        itemText = dropdown.itemText;
        if (itemText == null)
        {
            Debug.LogError("Item Text is null.");
            return;
        }

        // Check if the options list is empty or null.
        if (options == null || options.Count == 0)
        {
            Debug.LogWarning("Options list is null or empty.");
            return;
        }

        saveFile = LanguageFileManager.GetSaveFilePath(); // Load the save file path to retrieve the current language settings.

        // Get the localized text for the first dropdown option and update the caption.
        string line = LanguageFileManager.GetLocalizedLineByID(options[0].iD, saveFile, ref selectedFile);
        ProcessLine(line); // Update the caption's style and font based on the localized settings.
        ProcessOption(); // Update the dropdown options with the localized values.

        // Set the caption text to the first option if no selection has been made.
        if (dropdown.value == 0) captionText.text = LanguageFileManager.ExtractTextBetweenBraces(line);
    }

    // Processes and applies the alignment, font size, and font settings to the dropdown caption and items.
    private void ProcessLine(string line)
    {
        // Extract alignment, font size, and font data from the line.
        string lineWithoutCurlyBraces = LanguageFileManager.RemoveTextBetweenBraces(line);
        int alignment = LanguageFileManager.ExtractIntValue(lineWithoutCurlyBraces, "Ali:");
        int fontSize = LanguageFileManager.ExtractIntValue(lineWithoutCurlyBraces, "S:");
        int fontListIndex = LanguageFileManager.ExtractIntValue(lineWithoutCurlyBraces, "Font:");

        // Apply alignment to the caption and item text if specified.
        if (alignment != 0)
        {
            TextAlignmentOptions correctAlignment = FontAndAlignmentUtilityTMP.ConvertToTextAnchor(alignment);
            captionText.alignment = correctAlignment;
            itemText.alignment = correctAlignment;
        }

        // Apply font size to the caption and item text if specified.
        if (fontSize != 0)
        {
            captionText.fontSize = fontSize;
            itemText.fontSize = fontSize;
        }

        // Apply the font to the caption and item text if specified.
        if (fontListIndex != 0)
        {
            TMP_FontAsset correctFont = FontAndAlignmentUtilityTMP.GetFontByIndex(fontListIndex);
            captionText.font = correctFont;
            itemText.font = correctFont;
        }
    }

    // Updates the dropdown's options with the localized text and images.
    private void ProcessOption()
    {
        // Loop through each dropdown option and update its text with localized values.
        foreach (LanguageOptions i in options)
        {
            string text = LanguageFileManager.FindLineByID(selectedFile, i.iD);
            i.text = LanguageFileManager.ExtractTextBetweenBraces(text);
        }

        // Save the previously selected dropdown index.
        int previousIndex = dropdown.value;
        dropdown.ClearOptions(); // Clear existing dropdown options.

        // Add each localized option back into the dropdown.
        foreach (LanguageOptions i in options)
        {
            TMP_Dropdown.OptionData optionData = new()
            {
                text = i.text,
                image = i.sprite
            };
            dropdown.options.Add(optionData);
        }

        dropdown.value = previousIndex; // Restore the previously selected dropdown value.
    }
}

#if UNITY_EDITOR
// Custom Editor class for the LanguageDropdownTMP component.
[CustomEditor(typeof(LanguageDropdownTMP))]
public class LanguageDropdownTMPEditor : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update(); // Update the serialized properties.
        var script = (LanguageDropdownTMP)target; // Get reference to the LanguageDropdownTMP script.
        LanguageEditorUtilities.DrawReadOnlyMonoScriptField(target); // Draw a read-only field for the script.

        EditorGUILayout.Space(5);

        // Button to import settings from the language tool to customize the dropdown options.
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

            // Display a dialog if the options are already saved.
            if (alreadySaved)
            {
                if (!EditorUtility.DisplayDialog("Replace ID", "An ID with this number is already saved. Do you want to replace it?", "Yes", "No"))
                {
                    return;
                }
            }

            // Retrieve the alignment, font size, and font data from the caption text.
            int alignment = FontAndAlignmentUtilityTMP.ConvertToAlignmentCode(script.dropdown.captionText.alignment);
            int fontSize = (int)script.dropdown.captionText.fontSize;
            int fontListIndex = FontAndAlignmentUtilityTMP.GetFontIndex(script.dropdown.captionText.font);

            // Open the editor window to customize the dropdown's first option.
            LanguageEditorUtilities.OpenEditorWindowWithComponent(script.options[0].iD, 6, script.options[0].text, alignment, fontSize, fontListIndex, true, true, true, true);

            // Loop through remaining options and open the editor for each.
            for (int i = 1; i < script.options.Count; i++)
            {
                LanguageOptions option = script.options[i];
                LanguageEditorUtilities.OpenEditorWindowWithComponent(option.iD, 6, option.text, 0, 0, 0, true, false, false, false);
            }
        }

        // Display a warning if the dropdown component is not assigned.
        GUI.color = script.dropdown == null ? Color.red : Color.white;
        EditorGUILayout.PropertyField(serializedObject.FindProperty("dropdown")); // Dropdown field to assign the TMP_Dropdown component.
        GUI.color = Color.white;

        // Display the list of dropdown options.
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

        // Display fields for selecting the file and saving the data.
        EditorGUILayout.PropertyField(serializedObject.FindProperty("selectedFile"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("saveFile"));

        serializedObject.ApplyModifiedProperties(); // Apply any modified properties.
    }
}
#endif