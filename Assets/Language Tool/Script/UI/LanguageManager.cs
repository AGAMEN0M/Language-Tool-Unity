/*
 * ---------------------------------------------------------------------------
 * Description: This component manages language selection via a legacy 
 *              UnityEngine.UI.Dropdown. It loads available languages from 
 *              LanguageSettingsData, populates the dropdown, applies the 
 *              selected language, and stores the user's choice for future 
 *              sessions. It also triggers a global language update when the 
 *              selection changes.
 *              
 * Author: Lucas Gomes Cecchini
 * Pseudonym: AGAMENOM
 * ---------------------------------------------------------------------------
*/

using System.Collections.Generic;
using UnityEngine.UI;
using LanguageTools;
using UnityEngine;

using static LanguageTools.LanguageFileManager;

[AddComponentMenu("Language/UI/Legacy/Language Manager (Legacy)")]
public class LanguageManager : MonoBehaviour
{
    #region === Serialized Fields ===

    [Header("UI Components")]
    [SerializeField, Tooltip("UI Dropdown used for selecting the current language.")]
    private Dropdown languageDropdown;

    #endregion

    #region === Private Fields ===

    private List<LanguageAvailable> availableLanguages; // Cached list of available languages loaded from LanguageSettingsData.
    private LanguageSettingsData languageData; // Language configuration loaded from resources.

    #endregion

    #region === Properties ===

    /// <summary>
    /// Reference to the Dropdown UI for language selection.
    /// </summary>
    public Dropdown LanguageDropdown
    {
        get => languageDropdown;
        set => languageDropdown = value;
    }

    #endregion

    #region === Unity Events ===

    /// <summary>
    /// Initializes language data and populates the dropdown with available options.
    /// </summary>
    private void Start()
    {
        // Verify if the Dropdown component is assigned in the inspector.
        if (languageDropdown == null)
        {
            Debug.LogError("LanguageDropdownManager: Dropdown component is not assigned.", this);
            return;
        }

        // Load language settings data from resources.
        languageData = LoadLanguageSettings();
        if (languageData == null)
        {
            Debug.LogError("LanguageDropdownManager: Failed to load LanguageSettingsData.", this);
            return;
        }

        // Populate the list of available languages from the loaded settings.
        GetAvailableLanguages();
        availableLanguages = languageData.availableLanguages;

        PopulateDropdown(); // Fill the dropdown UI with the language names and select saved culture.
        SetupDropdownSelection(); // Set up event listener to handle user language selection changes.
    }

    #endregion

    #region === Dropdown Population ===

    /// <summary>
    /// Populates the dropdown with the list of available languages and restores the saved selection.
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
            options.Add(availableLanguages[i].nativeName);

            // Match saved culture to set initial dropdown value.
            if (availableLanguages[i].culture == savedCulture) selectedIndex = i;
        }

        languageDropdown.AddOptions(options); // Add all language names to the dropdown options.
        languageDropdown.SetValueWithoutNotify(selectedIndex); // Set dropdown to saved or default selection index without firing events.
    }

    #endregion

    #region === Dropdown Selection Handling ===

    /// <summary>
    /// Registers the event listener to respond when the user selects a new language.
    /// </summary>
    private void SetupDropdownSelection()
    {
        languageDropdown.onValueChanged.RemoveListener(OnLanguageChanged); // Remove any previous listeners to avoid duplicates.
        languageDropdown.onValueChanged.AddListener(OnLanguageChanged); // Add listener for dropdown value changes.
    }

    /// <summary>
    /// Handles language changes made via the dropdown. Saves the selection,
    /// reloads language data, and notifies the system to apply the update.
    /// </summary>
    /// <param name="index">The index of the newly selected language.</param>
    private void OnLanguageChanged(int index)
    {
        // Check that the selected index is within the valid range.
        if (index < 0 || index >= availableLanguages.Count)
        {
            Debug.LogWarning("LanguageDropdownManager: Selected index is out of range.", this);
            return;
        }

        string selectedCulture = availableLanguages[index].culture; // Get the culture code for the selected language.
        SetSaveCultureCode(selectedCulture); // Persist the selected culture code.
        GetAllData(); // Reload all language data based on new selection.
        LanguageManagerDelegate.NotifyLanguageUpdate(); // Notify other components to update localized content.
    }

    #endregion
}