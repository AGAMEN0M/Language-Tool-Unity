/*
 * ---------------------------------------------------------------------------
 * Description: This component integrates with a TMP_Dropdown to allow users to 
 *              select a language at runtime. It loads available languages from 
 *              LanguageSettingsData, populates the dropdown, applies the selected 
 *              language, and saves the choice for future sessions. It also triggers 
 *              a language update across the system.
 * Author: Lucas Gomes Cecchini
 * Pseudonym: AGAMENOM
 * ---------------------------------------------------------------------------
*/

using System.Collections.Generic;
using LanguageTools;
using UnityEngine;
using TMPro;

using static LanguageTools.LanguageFileManager;

[AddComponentMenu("Language/UI/TextMesh Pro/Language Manager (TMP)")]
public class LanguageManagerTMP : MonoBehaviour
{
    [Header("UI Components")]
    [SerializeField] private TMP_Dropdown languageDropdown; // TMP Dropdown used for selecting the current language.

    private List<LanguageAvailable> availableLanguages; // Cached list of available languages loaded from LanguageSettingsData.
    private LanguageSettingsData languageData; // Language configuration loaded from resources.

    /// <summary>
    /// Initializes the dropdown with available languages and applies the saved selection.
    /// </summary>
    private void Start()
    {
        // Verify if the Dropdown component is assigned in the inspector.
        if (languageDropdown == null)
        {
            Debug.LogError("LanguageManagerTMP: TMP_Dropdown is not assigned.", this);
            return;
        }

        // Load language settings data from resources.
        languageData = LoadLanguageSettings();
        if (languageData == null)
        {
            Debug.LogError("LanguageManagerTMP: Failed to load LanguageSettingsData.", this);
            return;
        }

        // Populate the list of available languages from the loaded settings.
        GetAvailableLanguages();
        availableLanguages = languageData.availableLanguages;

        PopulateDropdown(); // Fill the dropdown UI with the language names and select saved culture.
        SetupDropdownSelection(); // Set up event listener to handle user language selection changes.
    }

    /// <summary>
    /// Populates the dropdown with localized language names and selects the saved culture.
    /// </summary>
    private void PopulateDropdown()
    {
        languageDropdown.ClearOptions(); // Clear any existing dropdown options.

        List<string> options = new();
        int selectedIndex = 0; // Default index to select (0) if no saved culture found.
        string savedCulture = GetSaveCultureCode(); // Retrieve saved culture code to restore dropdown selection.

        // Sort available languages alphabetically by name.
        availableLanguages.Sort((a, b) => a.name.CompareTo(b.name));

        // Populate options with available language names.
        for (int i = 0; i < availableLanguages.Count; i++)
        {
            options.Add(availableLanguages[i].name);

            // Match saved culture to set initial dropdown value.
            if (availableLanguages[i].culture == savedCulture) selectedIndex = i;
        }

        languageDropdown.AddOptions(options); // Add all language names to the dropdown options.
        languageDropdown.SetValueWithoutNotify(selectedIndex); // Set dropdown to saved or default selection index without firing events.
    }

    /// <summary>
    /// Subscribes to the TMP_Dropdown value change event to handle user selection.
    /// </summary>
    private void SetupDropdownSelection()
    {
        languageDropdown.onValueChanged.RemoveListener(OnLanguageChanged); // Remove any previous listeners to avoid duplicates.
        languageDropdown.onValueChanged.AddListener(OnLanguageChanged); // Add listener for dropdown value changes.
    }

    /// <summary>
    /// Called when the language dropdown value changes. Updates the selected culture,
    /// reloads language data, and triggers a system-wide language update.
    /// </summary>
    /// <param name="index">Index of the selected language in the dropdown.</param>
    private void OnLanguageChanged(int index)
    {
        // Check that the selected index is within the valid range.
        if (index < 0 || index >= availableLanguages.Count)
        {
            Debug.LogWarning("LanguageManagerTMP: Selected index is out of range.", this);
            return;
        }

        string selectedCulture = availableLanguages[index].culture; // Get the culture code for the selected language.
        SetSaveCultureCode(selectedCulture); // Persist the selected culture code.
        GetAllData(); // Reload all language data based on new selection.
        LanguageManagerDelegate.NotifyLanguageUpdate(); // Notify other components to update localized content.
    }
}