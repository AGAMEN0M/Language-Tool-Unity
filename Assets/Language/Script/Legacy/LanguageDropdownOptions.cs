using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Linq;
using System.Globalization;

[AddComponentMenu("Language/UI/Legacy/Language Dropdown Options")]
public class LanguageDropdownOptions : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private Dropdown languageDropdown; // Reference to the dropdown UI component to show language options.
    [Space(10)]
    [Header("Default language if there is no save")]
    [SerializeField] private string languageName = "ENGLISH"; // The default language to use if no save data is available.
    [Space(10)]
    [Header("Automatic Information")]
    [SerializeField] private string selectedLanguage; // The currently selected language.
    [SerializeField] private string systemLanguage; // The system language of the user's device.
    [Space(5)]
    [SerializeField] private string savePath; // The path to the JSON file where the selected language will be saved.
    [SerializeField] private string selectedFile; // The path to the currently selected language file.
    [SerializeField] private string fileSavePath; // The path to the JSON file where the selected file will be saved.
    [Space(10)]
    [Header("Archives Location")]
    public string jsonNameInUnity = "/Language/Editor/LanguageFileSave.json"; // Path to the JSON file for language selection in Unity Editor.
    public string jsonSaveNameInUnity = "/Language/Editor/LanguageSave.json"; // Path to the JSON file for selected file in Unity Editor.
    public string folderNameInUnity = "/StreamingAssets/Language/"; // Folder containing language files in Unity Editor.
    [Space(10)]
    public string jsonNameInBuild = "/LanguageFileSave.json"; // Path to the JSON file for language selection in the build.
    public string jsonSaveNameInBuild = "/LanguageSave.json"; // Path to the JSON file for selected file in the build.
    public string folderNameInBuild = "/StreamingAssets/Language/"; // Folder containing language files in the build.

    private void Start()
    {
        CultureInfo cultureInfo = CultureInfo.InstalledUICulture;
        systemLanguage = cultureInfo.DisplayName; // Get the system language of the user's device.

        languageDropdown.ClearOptions(); // Clear any existing options in the dropdown.

    #if UNITY_EDITOR
        savePath = Application.dataPath + jsonSaveNameInUnity; // Set the save path for Unity Editor.
        fileSavePath = Application.dataPath + jsonNameInUnity; // Set the file save path for Unity Editor.
        string path = Application.dataPath + folderNameInUnity; // Set the path to find language files in Unity Editor.
    #else
        savePath = Application.dataPath + jsonSaveNameInBuild; // Set the save path for the build.
        fileSavePath = Application.dataPath + jsonNameInBuild; // Set the file save path for the build.
        string path = Application.dataPath + folderNameInBuild; // Set the path to find language files in the build.
    #endif

        string[] files = Directory.GetFiles(path, "*.txt");

        // Check if there is no save data and select the appropriate language file based on the system language.
        if (!File.Exists(savePath) && !File.Exists(fileSavePath))
        {
            foreach (string buildFilePath in files)
            {
                foreach (string unityFilePath in files)
                {
                    string[] unityFileLines = File.ReadAllLines(unityFilePath);
                    string unityFileLine2 = unityFileLines.Length >= 2 ? unityFileLines[1] : "";
                    string firstLine = File.ReadLines(unityFilePath).First();
                    string contentInBrackets = firstLine.Replace("Linguagem - [", "").Replace("]", "");

                    if (systemLanguage == unityFileLine2)
                    {
                        selectedLanguage = contentInBrackets;
                        selectedFile = unityFilePath;

                        // Save the selected language to JSON.
                        LanguageSave saveData = new()
                        {
                            selectedLanguage = selectedLanguage
                        };
                        string json = JsonUtility.ToJson(saveData);
                        File.WriteAllText(savePath, json);

                        // Save the selected file to JSON.
                        LanguageFileSave fileSaveData = new()
                        {
                            selectedFile = selectedFile
                        };
                        json = JsonUtility.ToJson(fileSaveData);
                        File.WriteAllText(fileSavePath, json);

                        LanguageUpdate(); // Update the language using the selected file.
                    }
                }
            }
        }

        int defaultLanguageIndex = -1;
        for (int i = 0; i < files.Length; i++)
        {
            string[] lines = File.ReadAllLines(files[i]);
            foreach (string line in lines)
            {
                if (line.StartsWith("Linguagem - ["))
                {
                    string language = line.Replace("Linguagem - [", "").Replace("]", "");
                    languageDropdown.options.Add(new Dropdown.OptionData(language)); // Add language options to the dropdown.

                    if (language == languageName)
                    {
                        defaultLanguageIndex = i; // Store the index of the default language.
                    }
                }
            }
        }

        // Load saved language selection from JSON if available.
        if (File.Exists(savePath) && File.Exists(fileSavePath))
        {
            string json = File.ReadAllText(savePath);
            LanguageSave saveData = JsonUtility.FromJson<LanguageSave>(json);
            selectedLanguage = saveData.selectedLanguage;

            json = File.ReadAllText(fileSavePath);
            LanguageFileSave fileSaveData = JsonUtility.FromJson<LanguageFileSave>(json);
            selectedFile = fileSaveData.selectedFile;

            for (int i = 0; i < languageDropdown.options.Count; i++)
            {
                if (languageDropdown.options[i].text == selectedLanguage)
                {
                    languageDropdown.value = i; // Set the dropdown to the saved language.
                    languageDropdown.RefreshShownValue();
                    break;
                }
            }
        }
        else
        {
            if (defaultLanguageIndex >= 0)
            {
                languageDropdown.value = defaultLanguageIndex; // Set the dropdown to the default language.
                languageDropdown.RefreshShownValue();
                selectedLanguage = languageName;
                selectedFile = files[defaultLanguageIndex];
                OnLanguageChanged(); // Update the language based on the selected file.
                OnFileChanged(); // Save the selected file to JSON.
            }
        }
        languageDropdown.onValueChanged.AddListener(delegate {OnLanguageChanged(); } ); // Add listener to handle language selection changes.
    }

    public void OnLanguageChanged()
    {
        selectedLanguage = languageDropdown.options[languageDropdown.value].text; // Get the selected language from the dropdown.
        LanguageSave saveData = new()
        {
            selectedLanguage = selectedLanguage
        };
        string json = JsonUtility.ToJson(saveData);
        File.WriteAllText(savePath, json); // Save the selected language to JSON.

    #if UNITY_EDITOR
        string path = Application.dataPath + folderNameInUnity; // Set the path to find language files in Unity Editor.
    #else
        string path = Application.dataPath + folderNameInBuild; // Set the path to find language files in the build.
    #endif
        string[] files = Directory.GetFiles(path, "*.txt");
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
                        selectedFile = files[i]; // Set the selected file based on the selected language.
                        break;
                    }
                }
            }
        }
        OnFileChanged(); // Save the selected file to JSON.
        LanguageUpdate(); // Update the language using the selected file.
    }

    private void LanguageUpdate()
    {
        // Find all scripts that require a language update and call their LanguageUpdate() method.
        LanguageScript[] languageScript = FindObjectsOfType<LanguageScript>();
        LanguageCreateFile[] languageCreateFile = FindObjectsOfType<LanguageCreateFile>();
        LanguageText[] languageTexts = FindObjectsOfType<LanguageText>();
        LanguageTextInputField[] languageTextInputField = FindObjectsOfType<LanguageTextInputField>();
        LanguageTextMesh[] languageTextMesh = FindObjectsOfType<LanguageTextMesh>();
        LanguageDropdown[] languageDropdown = FindObjectsOfType<LanguageDropdown>();
        LanguageAudioPlayer[] languageAudioPlayer = FindObjectsOfType<LanguageAudioPlayer>();
        LanguageRawImage[] languageRawImage = FindObjectsOfType<LanguageRawImage>();
        LanguageImage[] languageImage = FindObjectsOfType<LanguageImage>();
        AdjustRect[] adjustRect = FindObjectsOfType<AdjustRect>();
        /**/
        // If you have TextMeshPro installed.
        TMP_LanguageTextMesh[] tmp_LanguageTextMesh = FindObjectsOfType<TMP_LanguageTextMesh>();
        TMP_LanguageTextInputField[] tmp_LanguageTextInputField = FindObjectsOfType<TMP_LanguageTextInputField>();
        TMP_LanguageText[] tmp_LanguageText = FindObjectsOfType<TMP_LanguageText>();
        TMP_LanguageDropdown[] tmp_LanguageDropdown = FindObjectsOfType<TMP_LanguageDropdown>();
        /**/

        // Call the LanguageUpdate() method for each script that requires language update.
        foreach (LanguageScript language in languageScript)
        {
            language.LanguageUpdate();
        }

        foreach (LanguageCreateFile language in languageCreateFile)
        {
            language.LanguageUpdate();
        }

        foreach (LanguageText language in languageTexts)
        {
            language.LanguageUpdate();
        }

        foreach (LanguageTextInputField language in languageTextInputField)
        {
            language.LanguageUpdate();
        }

        foreach (LanguageTextMesh language in languageTextMesh)
        {
            language.LanguageUpdate();
        }

        foreach (LanguageDropdown language in languageDropdown)
        {
            language.LanguageUpdate();
        }

        foreach (LanguageAudioPlayer language in languageAudioPlayer)
        {
            language.LanguageUpdate();
        }

        foreach (LanguageRawImage language in languageRawImage)
        {
            language.LanguageUpdate();
        }

        foreach (LanguageImage language in languageImage)
        {
            language.LanguageUpdate();
        }

        foreach (AdjustRect language in adjustRect)
        {
            language.LanguageUpdate();
        }
        /**/
        // If you have TextMeshPro installed.
        foreach (TMP_LanguageTextMesh language in tmp_LanguageTextMesh)
        {
            language.LanguageUpdate();
        }

        foreach (TMP_LanguageTextInputField language in tmp_LanguageTextInputField)
        {
            language.LanguageUpdate();
        }

        foreach (TMP_LanguageText language in tmp_LanguageText)
        {
            language.LanguageUpdate();
        }

        foreach (TMP_LanguageDropdown language in tmp_LanguageDropdown)
        {
            language.LanguageUpdate();
        }
        /**/
    }

    public void OnFileChanged()
    {
        LanguageFileSave fileSaveData = new()
        {
            selectedFile = selectedFile
        };
        string json = JsonUtility.ToJson(fileSaveData);
        File.WriteAllText(fileSavePath, json); // Save the selected file to JSON.
    }
}

public class LanguageSave
{
    public string selectedLanguage; // The selected language to be saved.
}

public class LanguageFileSave
{
    public string selectedFile; // The selected file to be saved.
}