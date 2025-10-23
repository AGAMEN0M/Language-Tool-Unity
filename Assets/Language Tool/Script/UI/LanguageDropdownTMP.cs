/*
 * ---------------------------------------------------------------------------
 * Description: Integrates LanguageTools with a TMP_Dropdown to dynamically 
 *              update text content, font, alignment, and size according to the 
 *              current language settings. Supports localized display of dropdown 
 *              options using metadata and ID-linked translations.
 *              
 * Author: Lucas Gomes Cecchini
 * Pseudonym: AGAMENOM
 * ---------------------------------------------------------------------------
*/

using System.Collections.Generic;
using LanguageTools;
using UnityEngine;
using TMPro;

using static LanguageTools.TMP.FontAndAlignmentUtilityTMP;
using static LanguageTools.LanguageFileManager;

#if UNITY_EDITOR
using UnityEditor;
using static LanguageTools.Editor.LanguageEditorUtilities;
#endif

[AddComponentMenu("Language/UI/TextMesh Pro/Language Dropdown (TMP)")]
public class LanguageDropdownTMP : MonoBehaviour
{
    #region === Serialized Fields ===

    [Header("UI Components")]
    [SerializeField, Tooltip("TMP_Dropdown component to localize.")]
    private TMP_Dropdown dropdown;
    
    [SerializeField, Tooltip("Whether to translate option texts.")]
    private bool translateText = true;
    
    [Space(10)]

    [SerializeField, Tooltip("List of dropdown options with text, sprite, and localization ID.")]
    private List<LanguageOptions> options = new()
    {
        new() { text = "Option A", sprite = null, iD = -2 },
        new() { text = "Option B", sprite = null, iD = -3 },
        new() { text = "Option C", sprite = null, iD = -4 }
    };

    #endregion

    #region === Private Fields ===

    private LanguageSettingsData languageData; // Holds the currently loaded language data including texts and metadata.
    private TMP_Text captionText; // Reference to the TMP_Text component used for the dropdown caption.
    private TMP_Text itemText; // Reference to the TMP_Text component used for dropdown item labels.

    #endregion

    #region === Properties ===

    /// <summary>
    /// Gets or sets the TMP_Dropdown component that will be localized.
    /// </summary>
    public TMP_Dropdown Dropdown
    {
        get => dropdown;
        set => dropdown = value;
    }

    /// <summary>
    /// Gets or sets whether the dropdown option texts should be translated
    /// based on the current language settings.
    /// </summary>
    public bool TranslateText
    {
        get => translateText;
        set => translateText = value;
    }

    /// <summary>
    /// Gets or sets the list of options for the TMP_Dropdown, including
    /// text, associated sprites, and localization IDs.
    /// </summary>
    public List<LanguageOptions> Options
    {
        get => options;
        set => options = value;
    }

    #endregion

    #region === Unity Events ===

    /// <summary>
    /// Subscribes to the language update event and applies localization.
    /// </summary>
    private void OnEnable()
    {
        LanguageManagerDelegate.OnLanguageUpdate += LanguageUpdate;
        LanguageUpdate();
    }

    /// <summary>
    /// Unsubscribes from the language update event.
    /// </summary>
    private void OnDisable() => LanguageManagerDelegate.OnLanguageUpdate -= LanguageUpdate;

    #endregion

    #region === Language Update ===

    /// <summary>
    /// Updates dropdown options and text styles based on the current language.
    /// </summary>
    public void LanguageUpdate()
    {
        // Ensure dropdown component is assigned.
        if (dropdown == null)
        {
            Debug.LogError("LanguageDropdownTMP: Dropdown is not assigned.", this);
            return;
        }

        // Try to get the TMP_Text component for the dropdown's caption.
        if (!dropdown.captionText.TryGetComponent(out captionText))
        {
            Debug.LogError("LanguageDropdownTMP: Caption text component is missing or invalid.", this);
            return;
        }

        // Try to get the TMP_Text component for dropdown item labels.
        if (!dropdown.itemText.TryGetComponent(out itemText))
        {
            Debug.LogError("LanguageDropdownTMP: Item text component is missing or invalid.", this);
            return;
        }

        // Load language settings data including texts and metadata.
        languageData = LoadLanguageSettings();
        if (languageData == null)
        {
            Debug.LogError("LanguageDropdownTMP: Failed to load LanguageSettingsData.", this);
            return;
        }

        // Check if options list is assigned and not empty.
        if (options == null || options.Count == 0)
        {
            Debug.LogWarning("LanguageDropdownTMP: Options list is empty.", this);
            return;
        }

        // If translation is enabled, update localized option texts.
        if (translateText) UpdateLocalizedOptions();

        // Retrieve metadata for the first option's ID to style the dropdown texts.
        var meta = GetIDMeta(languageData.idMetaData, options[0].iD);

        // Apply text alignment from metadata if specified.
        if (meta.alignment != 0)
        {
            var alignment = ConvertToTextAnchor(meta.alignment);
            captionText.alignment = alignment;
            itemText.alignment = alignment;
        }

        // Apply font size from metadata if specified.
        if (meta.fontSize != 0)
        {
            captionText.fontSize = meta.fontSize;
            itemText.fontSize = meta.fontSize;
        }

        // Apply font from metadata if specified.
        if (meta.fontListIndex != 0)
        {
            var font = GetFontByIndex(meta.fontListIndex);
            captionText.font = font;
            itemText.font = font;
        }
    }

    /// <summary>
    /// Translates the dropdown option texts and restores the previous selection index.
    /// </summary>
    private void UpdateLocalizedOptions()
    {
        // Translate each option's text based on its localization ID.
        foreach (var option in options)
        {
            var translated = GetIDText(languageData.idData, option.iD);
            if (!string.IsNullOrEmpty(translated)) option.text = translated;
        }

        int previousIndex = dropdown.value; // Save the currently selected dropdown index.
        dropdown.ClearOptions(); // Clear all existing options from the dropdown.

        // Add all updated localized options back to the dropdown.
        foreach (var option in options)
        {
            dropdown.options.Add(new(){ text = option.text, image = option.sprite });
        }

        captionText.text = options[previousIndex].text; // Set caption text to match the previously selected option.
        dropdown.SetValueWithoutNotify(previousIndex); // Restore dropdown selection index without triggering callbacks.
    }

    #endregion
}

#if UNITY_EDITOR

#region === Custom Inspector ===

/// <summary>
/// Custom Inspector for LanguageDropdownTMP. Provides an Import Settings button.
/// </summary>
[CanEditMultipleObjects]
[CustomEditor(typeof(LanguageDropdownTMP))]
public class LanguageDropdownTMPEditor : Editor
{
    /// <summary>
    /// Draws the custom inspector UI and handles the Import Settings functionality.
    /// </summary>
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        var script = (LanguageDropdownTMP)target;

        // Disable Import Settings button if multiple objects are selected.
        using (new EditorGUI.DisabledScope(targets.Length > 1))
        {
            // Draw Import Settings button with custom style.
            if (GUILayout.Button(new GUIContent("Import Settings", "Import the dropdown options and their metadata into the LanguageTools system."), CreateCustomButtonStyle(15), GUILayout.Height(30)))
            {
                // Check if any option IDs already exist in the language list.
                bool alreadyExists = script.Options.Exists(opt => IsIDInLanguageList(opt.iD));

                // Ask user if they want to replace existing IDs.
                if (alreadyExists && !EditorUtility.DisplayDialog("Replace ID", "An ID with this number is already saved. Do you want to replace it?", "Yes", "No"))
                    return;

                // Retrieve alignment, font size, and font index from the caption text.
                int alignment = ConvertToAlignmentCode(script.Dropdown.captionText.alignment);
                int fontSize = (int)script.Dropdown.captionText.fontSize;
                int fontListIndex = GetFontIndex(script.Dropdown.captionText.font);

                // Open language editor window with style data for the first option.
                OpenEditorWindowWithComponent(script.Options[0].iD, 6, script.Options[0].text, alignment, fontSize, fontListIndex);

                // For other options, open editor windows without style metadata.
                if (script.TranslateText)
                {
                    for (int i = 1; i < script.Options.Count; i++)
                    {
                        var option = script.Options[i];
                        OpenEditorWindowWithComponent(option.iD, 6, option.text, 0, 0, 0);
                    }
                }
            }
        }

        EditorGUILayout.Space(5);

        // Draw default inspector for remaining serialized fields.
        DrawDefaultInspector();
        serializedObject.ApplyModifiedProperties();
    }
}

#endregion

#endif