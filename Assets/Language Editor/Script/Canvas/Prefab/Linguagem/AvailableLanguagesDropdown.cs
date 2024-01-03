using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

public class AvailableLanguagesDropdown : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private Dropdown languageDropdown;
    public string DefaultText = "Default";
    [Space(5)]
    [SerializeField] private InputField inputField;
    [Space(10)]
    [Header("Available Languages")]
    [SerializeField] private List<string> availableLanguages = new();

    private void Start()
    {
        // Call the public method InitializeDropdown() in Start() to set up the Dropdown correctly.
        InitializeDropdown();
    }

    // Public method for configuring Dropdown with desired options.
    public void InitializeDropdown()
    {
        // Clear the Dropdown options before adding the new options.
        languageDropdown.ClearOptions();

        // Clear the list of available languages before adding them again.
        availableLanguages.Clear();

        // Add the "Standard" option to the list of available languages.
        availableLanguages.Add(DefaultText);

        // Get the available languages from the system and add them to the language list.
        CultureInfo[] cultures = CultureInfo.GetCultures(CultureTypes.AllCultures);
        foreach (CultureInfo cultureInfo in cultures)
        {
            if (!availableLanguages.Contains(cultureInfo.DisplayName))
            {
                availableLanguages.Add(cultureInfo.DisplayName);
            }
        }

        // Sort the list of languages alphabetically, except "Default" which should be at the top.
        availableLanguages = availableLanguages.OrderBy(lang => lang == DefaultText ? "" : lang).ToList();

        // Configure Dropdown options.
        languageDropdown.AddOptions(availableLanguages);

        // Set Dropdown's default value to "Default".
        languageDropdown.value = 0;

        // Add an event listener to detect when an option is selected.
        languageDropdown.onValueChanged.AddListener(OnLanguageDropdownValueChanged);
    }

    private void OnLanguageDropdownValueChanged(int index)
    {
        // Make sure the selected option is not the default option.
        if (index != 0) // Index 0 represents the "Default" option.
        {
            // Update the InputField text with the selected option.
            string selectedLanguage = availableLanguages[index];
            inputField.text = selectedLanguage;
        }
    }
}