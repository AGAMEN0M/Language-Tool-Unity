using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

public class Translator : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private Dropdown languageDropdown;
    [SerializeField] private Button translatorButton;
    [SerializeField] private List<string> availableLanguages = new();

    private void Start()
    {
        InitializeDropdown();
        translatorButton.onClick.AddListener(TranslateTextInInputFields);
    }

    public void InitializeDropdown()
    {
        languageDropdown.ClearOptions();
        availableLanguages.Clear();

        CultureInfo[] cultures = CultureInfo.GetCultures(CultureTypes.AllCultures);
        foreach (CultureInfo cultureInfo in cultures)
        {
            if (!availableLanguages.Contains(cultureInfo.DisplayName))
            {
                availableLanguages.Add(cultureInfo.DisplayName);
            }
        }

        availableLanguages = availableLanguages.OrderBy(lang => lang).ToList();
        languageDropdown.AddOptions(availableLanguages);
        languageDropdown.value = 0;
    }

    public void TranslateTextInInputFields()
    {
        // Get the selected language from the dropdown.
        string selectedLanguage = availableLanguages[languageDropdown.value];

        // Find all objects in the scene with the tag "Respawn" that have an InputField component.
        InputField[] inputFieldsToTranslate = GameObject.FindGameObjectsWithTag("Respawn").Select(go => go.GetComponent<InputField>()).Where(inputField => inputField != null).ToArray();

        // Loop through each InputField and translate its text.
        foreach (InputField inputField in inputFieldsToTranslate)
        {
            string originalText = inputField.text;
            string translatedText = TranslateText(originalText, selectedLanguage);
            inputField.text = translatedText;
        }
    }

    // Implement your own translation logic here.
    // You can use any translation API or service to perform the actual translation.
    private string TranslateText(string text, string targetLanguage)
    {
        // Your translation logic goes here.
        // You can use any translation API or service to perform the actual translation.
        // For this example, we'll just return a dummy translation.
        return "Translated: " + text;
    }
}