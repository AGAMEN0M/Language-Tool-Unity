/*
 * ---------------------------------------------------------------------------
 * Description: Integrates LanguageTools with a legacy UnityEngine.UI.Dropdown 
 *              to localize dropdown option texts and adjust font styling 
 *              (font, alignment, size) based on the current language metadata.
 * Author: Lucas Gomes Cecchini
 * Pseudonym: AGAMENOM
 * ---------------------------------------------------------------------------
*/

using System.Collections.Generic;
using UnityEngine.UI;
using LanguageTools;
using UnityEngine;

#if UNITY_EDITOR
using static LanguageTools.Editor.LanguageEditorUtilities;
using UnityEditor;
#endif

using static LanguageTools.LanguageFileManager;
using static LanguageTools.Legacy.FontAndAlignmentUtility;

[AddComponentMenu("Language/UI/Legacy/Language Dropdown (Legacy)")]
public class LanguageDropdown : MonoBehaviour
{
    [Header("UI Components")]
    public Dropdown dropdown; // Reference to the legacy UnityEngine.UI Dropdown component to localize.
    public bool translateText = true; // Determines whether to translate the option texts using LanguageTools.
    [Space(10)]
    public List<LanguageOptions> options = new()
    {
        new() { text = "Option A", sprite = null, iD = -2 },
        new() { text = "Option B", sprite = null, iD = -3 },
        new() { text = "Option C", sprite = null, iD = -4 }
    }; // List of dropdown options with their texts, sprites, and associated localization IDs.

    private LanguageSettingsData languageData; // Holds the currently loaded language data including text and metadata.
    private Text captionText; // Reference to the dropdown's caption Text component.
    private Text itemText; // Reference to the dropdown's item Text component.

    /// <summary>
    /// Registers for language change events and updates dropdown localization.
    /// </summary>
    private void OnEnable()
    {
        LanguageManagerDelegate.OnLanguageUpdate += LanguageUpdate;
        LanguageUpdate();
    }

    /// <summary>
    /// Unregisters from language change events when the object is disabled.
    /// </summary>
    private void OnDisable() => LanguageManagerDelegate.OnLanguageUpdate -= LanguageUpdate;

    /// <summary>
    /// Updates dropdown options, font, alignment, and size based on selected language metadata.
    /// </summary>
    public void LanguageUpdate()
    {
        // Check if the dropdown component is assigned.
        if (dropdown == null)
        {
            Debug.LogError("LanguageDropdown: Dropdown component is not assigned.", this);
            return;
        }

        // Try to get the caption Text component for updating display.
        if (!dropdown.captionText.TryGetComponent(out captionText))
        {
            Debug.LogError("LanguageDropdown: Caption Text component is missing or invalid.", this);
            return;
        }

        // Try to get the item Text component for dropdown options.
        if (!dropdown.itemText.TryGetComponent(out itemText))
        {
            Debug.LogError("LanguageDropdown: Item Text component is missing or invalid.", this);
            return;
        }

        // Load the language settings data.
        languageData = LoadLanguageSettings();
        if (languageData == null)
        {
            Debug.LogError("LanguageDropdown: Failed to load LanguageSettingsData.", this);
            return;
        }

        // Verify that the options list is assigned and not empty.
        if (options == null || options.Count == 0)
        {
            Debug.LogWarning("LanguageDropdown: Options list is empty or not assigned.", this);
            return;
        }

        // Update option texts with localized strings if translation is enabled.
        if (translateText) UpdateLocalizedOptions();

        // Retrieve metadata for the first option's ID to apply styling.
        var meta = GetIDMeta(languageData.idMetaData, options[0].iD);

        // Apply text alignment if specified in metadata.
        if (meta.alignment != 0)
        {
            var align = ConvertToTextAnchor(meta.alignment);
            captionText.alignment = align;
            itemText.alignment = align;
        }

        // Apply font size if specified in metadata.
        if (meta.fontSize != 0)
        {
            captionText.fontSize = meta.fontSize;
            itemText.fontSize = meta.fontSize;
        }

        // Apply font type if specified in metadata.
        if (meta.fontListIndex != 0)
        {
            var font = GetFontByIndex(meta.fontListIndex);
            captionText.font = font;
            itemText.font = font;
        }
    }

    /// <summary>
    /// Updates dropdown option texts with localized strings and restores the previous selection.
    /// </summary>
    private void UpdateLocalizedOptions()
    {
        // Translate each option's text using its localization ID.
        foreach (var option in options)
        {
            var translated = GetIDText(languageData.idData, option.iD);
            if (!string.IsNullOrEmpty(translated)) option.text = translated;
        }

        int previousIndex = dropdown.value; // Store the current selected index to restore after updating options.
        dropdown.ClearOptions(); // Clear existing options from the dropdown.

        // Add updated localized options back into the dropdown.
        foreach (var option in options)
        {
            dropdown.options.Add(new(){ text = option.text, image = option.sprite });
        }

        captionText.text = options[previousIndex].text; // Update caption text to match the restored selection.
        dropdown.SetValueWithoutNotify(previousIndex); // Set dropdown value without triggering any callbacks.
    }
}

#if UNITY_EDITOR
/// <summary>
/// Custom Inspector for LanguageDropdown. Adds import functionality for language metadata.
/// </summary>
[CanEditMultipleObjects]
[CustomEditor(typeof(LanguageDropdown))]
public class LanguageDropdownEditor : Editor
{
    /// <summary>
    /// Draws the custom inspector and handles metadata import for dropdown options.
    /// </summary>
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        var script = (LanguageDropdown)target;

        // Disable import button when multiple objects are selected.
        using (new EditorGUI.DisabledScope(targets.Length > 1))
        {
            // Draw "Import Settings" button with custom style.
            if (GUILayout.Button("Import Settings", CreateCustomButtonStyle(15), GUILayout.Height(30)))
            {
                // Check if any option IDs already exist in the language list.
                bool alreadyExists = script.options.Exists(opt => IsIDInLanguageList(opt.iD));

                // Confirm with the user before replacing existing IDs.
                if (alreadyExists && !EditorUtility.DisplayDialog("Replace ID", "An ID with this number is already saved. Do you want to replace it?", "Yes", "No"))
                    return;

                // Retrieve current alignment, font size, and font index from captionText.
                int alignment = ConvertToAlignmentCode(script.dropdown.captionText.alignment);
                int fontSize = script.dropdown.captionText.fontSize;
                int fontListIndex = GetFontIndex(script.dropdown.captionText.font);

                // Open the language editor window for the first option.
                OpenEditorWindowWithComponent(script.options[0].iD, 6, script.options[0].text, alignment, fontSize, fontListIndex);

                // For additional options, open editor windows without style metadata.
                if (script.translateText)
                {
                    for (int i = 1; i < script.options.Count; i++)
                    {
                        var option = script.options[i];
                        OpenEditorWindowWithComponent(option.iD, 6, option.text, 0, 0, 0);
                    }
                }
            }
        }

        EditorGUILayout.Space(5);

        // Draw the default inspector for other properties.
        DrawDefaultInspector();
        serializedObject.ApplyModifiedProperties();
    }
}
#endif