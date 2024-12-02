/*
 * ---------------------------------------------------------------------------
 * Description: This component manages language selection using a 
 *              TMP_Dropdown. It loads available language files, allows the user 
 *              to select a language, and saves the selected language for future 
 *              sessions. It also updates the application's language based on 
 *              the user's choice.
 * Author: Lucas Gomes Cecchini
 * Pseudonym: AGAMENOM
 * ---------------------------------------------------------------------------
*/

using System.Globalization;
using LanguageTools;
using UnityEngine;
using System.IO;
using TMPro;

[AddComponentMenu("Language/UI/TextMesh Pro/Language Manager (TMP)")]
public class LanguageManagerTMP : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private TMP_Dropdown dropdown; // Dropdown for selecting language.
    [Space(10)]
    [Header("Automatic Information")]
    [SerializeField] private string systemLanguage; // System's UI language.
    [SerializeField] private string selectedFile; // File path of the selected language.
    [SerializeField] private string selectedLanguage; // Name of the selected language.
    [Space(5)]
    [SerializeField] private string pathFolder; // Folder path containing language files.
    [SerializeField] private string saveFile; // Path to save the selected language data.

    private void Start()
    {
        // Ensure the dropdown is assigned.
        if (dropdown == null)
        {
            Debug.LogError("Dropdown not assigned");
            return;
        }

        dropdown.ClearOptions(); // Clear any existing options in the dropdown.

        // Get system language, paths, and default language.
        systemLanguage = CultureInfo.InstalledUICulture.DisplayName;
        pathFolder = LanguageFileManager.GetLanguageFilesFolderPath();
        saveFile = LanguageFileManager.GetSaveFilePath();
        string defaultLanguage = LanguageFileManager.LoadLanguageSettings().defaultLanguage;

        string[] files = Directory.GetFiles(pathFolder, "*.txt"); // Get all language files in the specified folder.

        // Check if a saved language file exists.
        if (!File.Exists(saveFile))
        {
            // Find a language file that matches the system language and save it.
            foreach (string filePath in files)
            {
                string[] lines = File.ReadAllLines(filePath);
                if (lines.Length > 1 && systemLanguage == lines[1])
                {
                    string languageName = lines[0].Replace("Linguagem - [", "").Replace("]", "");

                    LanguageSaveData languageDataSave = new()
                    {
                        selectedFile = filePath,
                        selectedLanguage = languageName
                    };

                    string json = JsonUtility.ToJson(languageDataSave);
                    File.WriteAllText(saveFile, json);

                    break;
                }
            }
        }

        int defaultLanguageIndex = -1; // Initialize index for default language.
        // Populate the dropdown with available languages.
        foreach (string filePath in files)
        {
            string[] lines = File.ReadAllLines(filePath);
            if (lines.Length > 0 && lines[0].StartsWith("Linguagem - ["))
            {
                string language = lines[0].Replace("Linguagem - [", "").Replace("]", "");
                dropdown.options.Add(new TMP_Dropdown.OptionData(language));
                string languageIndex = lines[1];

                // Set the default language index if it matches.
                if (languageIndex == defaultLanguage)
                {
                    defaultLanguageIndex = dropdown.options.Count - 1;
                }
            }
        }

        // If a saved language file exists, load the saved language.
        if (File.Exists(saveFile))
        {
            string json = File.ReadAllText(saveFile);
            var saveData = JsonUtility.FromJson<LanguageSaveData>(json);
            selectedFile = saveData.selectedFile;
            selectedLanguage = saveData.selectedLanguage;

            // Set the dropdown to the saved language.
            for (int i = 0; i < dropdown.options.Count; i++)
            {
                if (dropdown.options[i].text == selectedLanguage)
                {
                    dropdown.value = i;
                    dropdown.RefreshShownValue();
                    break;
                }
            }
        }
        else if (defaultLanguageIndex >= 0) // Otherwise, set the dropdown to the default language.
        {
            dropdown.value = defaultLanguageIndex;
            dropdown.RefreshShownValue();
            selectedLanguage = defaultLanguage;
            selectedFile = files[defaultLanguageIndex];
            OnLanguageChanged(defaultLanguageIndex);
        }

        // Add listener to handle language changes through the dropdown.
        dropdown.onValueChanged.RemoveListener(OnLanguageChanged);
        dropdown.onValueChanged.AddListener(OnLanguageChanged);

        LanguageManagerDelegate.NotifyLanguageUpdate();
    }

    // Method to handle changes in selected language.
    private void OnLanguageChanged(int index)
    {
        selectedLanguage = dropdown.options[index].text;

        // Find the file corresponding to the selected language.
        string[] files = Directory.GetFiles(pathFolder, "*.txt");
        for (int i = 0; i < files.Length; i++)
        {
            string[] lines = File.ReadAllLines(files[i]);
            foreach (string line in lines)
            {
                if (line.StartsWith("Linguagem - ["))
                {
                    string language = line.Replace("Linguagem - [", "").Replace("]", "");
                    if (language == selectedLanguage)
                    {
                        selectedFile = files[i];
                        break;
                    }
                }
            }
        }

        // Save the selected language data.
        LanguageSaveData saveData = new()
        {
            selectedFile = selectedFile,
            selectedLanguage = selectedLanguage
        };
        string json = JsonUtility.ToJson(saveData);
        File.WriteAllText(saveFile, json);

        LanguageManagerDelegate.NotifyLanguageUpdate();
    }
}