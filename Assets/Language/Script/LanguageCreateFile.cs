using UnityEngine;
using System.IO;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
using System.Linq;
#endif

[AddComponentMenu("Language/Language Create File")]
public class LanguageCreateFile : MonoBehaviour
{    
    #if UNITY_EDITOR
    [CustomEditor(typeof(LanguageCreateFile))]
    public class LanguageCreateFileEditor : Editor
    {
        // Store IDs for language lines.
        public static List<float> IDs = new();
        private bool savedID;

        private void OnEnable()
        {
            LanguageClassToolsEditor.LoadDataFromFile(ref IDs); // Load data from file into the IDs list.
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update(); // Update serialized object.
            LanguageCreateFile script = (LanguageCreateFile)target; // Reference to the LanguageCreateFile script.

            // Display the script field (disabled).
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.ObjectField("Script", MonoScript.FromMonoBehaviour((MonoBehaviour)target), typeof(MonoBehaviour), false);
            EditorGUI.EndDisabledGroup();

            EditorGUILayout.Space(15f); // Add space between elements.

            // Button to save settings.
            if (GUILayout.Button("Save Settings"))
            {
                bool shouldSave = true;

                if (savedID == true)
                {
                    // Display confirmation dialog if ID already exists.
                    shouldSave = EditorUtility.DisplayDialog("Save Confirmation", "An ID with this value already exists. Are you sure you want to save?\nThis action will overwrite the currently saved ID.", "Yes", "No");
                }

                if (shouldSave)
                {
                    // Execute menu item and add language save ID.
                    EditorApplication.ExecuteMenuItem("Window/Language/Language Save Editor");
                    AddLanguageSaveID(script);
                }
            }

            // Display language lines and associated IDs.
            EditorGUILayout.PropertyField(serializedObject.FindProperty("languageLines"));

            foreach (var lines in script.languageLines)
            {
                if (IDs.Contains(lines.ID))
                {
                    savedID = true;
                    GUI.color = Color.yellow;
                    EditorGUILayout.HelpBox($"ID: {lines.ID}\nThere is an ID with this number Saved!", MessageType.Warning); // Display warning if ID is already saved.
                    GUI.color = Color.white;
                }
                else
                {
                    savedID = false;
                }
            }

            // Display other properties related to language file creation.
            EditorGUILayout.PropertyField(serializedObject.FindProperty("standardFile"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("selectedFile"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("savePath"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("defaultFile"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("jsonNameInUnity"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("folderNameInUnity"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("jsonNameInBuild"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("folderNameInBuild"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("folderCreateNameInUnity"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("folderCreateNameInBuild"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("fileName"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("fileExtension"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("fileLines"));

            serializedObject.ApplyModifiedProperties(); // Apply changes to the serialized object.
        }

        // Method to add language save IDs.
        public void AddLanguageSaveID(LanguageCreateFile script)
        {
            var window = EditorWindow.GetWindow<LanguageSaveEditorWindow>(); // Get the LanguageSaveEditorWindow.

            // Iterate through language lines to add/save language IDs.
            foreach (LanguageLines id in script.languageLines)
            {
                var existingLanguageSaveID = window.textID.FirstOrDefault(existingID => existingID.ID == id.ID); // Check if the ID already exists in the language save window.

                if (existingLanguageSaveID != null)
                {
                    // If ID exists, clear existing values and update.
                    LanguageClassToolsEditor.ClearExistingLanguageSaveIDValues(existingLanguageSaveID);
                    existingLanguageSaveID.componentType = 5;
                    existingLanguageSaveID.text = id.text;
                    existingLanguageSaveID.textWrite = true;
                }
                else
                {
                    // If ID doesn't exist, create a new language save ID.
                    var languageSaveID = new LanguageSaveID
                    {
                        ID = id.ID,
                        componentType = 5,
                        text = id.text,
                        textWrite = true
                    };
                    window.textID.Add(languageSaveID); // Add the new ID to the list in the window.
                }
            }

            window.SaveDataJson(); // Save the updated data to the file.
            LanguageClassToolsEditor.LoadDataFromFile(ref IDs); // Reload IDs after changes.
        }
    }
    #endif

    [Header("Settings")]
    public List<LanguageLines> languageLines = new() { new LanguageLines { text = "Test Language", ID = -7, line = 0 } }; // A list of LanguageLines objects that hold localized text data.
    [Space(10)]
    [Header("Default language if there is no save")]
    [SerializeField] private string standardFile = "Linguagem - [ENGLISH]"; // The default language file to use if no save data is available.
    [Space(10)]
    [Header("Automatic Information")]
    [SerializeField] private string selectedFile; // The path to the currently selected language file.
    [SerializeField] private string savePath; // The path to the JSON file where the language selection will be saved.
    [SerializeField] private string defaultFile; // The path to the default language file based on the StandardFile setting.
    [Space(10)]
    [Header("Archives Location")]
    public string jsonNameInUnity = "/Language/Editor/LanguageFileSave.json"; // Path to the JSON file in Unity Editor.
    public string folderNameInUnity = "/StreamingAssets/Language/"; // Folder containing language files in Unity Editor.
    [Space(10)]
    public string jsonNameInBuild = "/LanguageFileSave.json"; // Path to the JSON file in the build.
    public string folderNameInBuild = "/StreamingAssets/Language/"; // Folder containing language files in the build.
    [Space(15)]
    [Header("File Creator Settings")]    
    #pragma warning disable CS0414
    [SerializeField] private string folderCreateNameInUnity = "Language/Editor"; // The folder name to save the created file in Unity Editor.
    [SerializeField] private string folderCreateNameInBuild = "StreamingAssets"; // The folder name to save the created file in the build.
    #pragma warning restore CS0414
    [Space(10)]
    [SerializeField] private string fileName = "Test File"; // The name of the file to be created.
    [SerializeField] private string fileExtension = ".txt"; // The extension of the file to be created.
    [SerializeField] private List<string> fileLines = new() { "Test Language" }; // The list of lines for the file to be created.

    private void OnEnable()
    {
        LanguageUpdate(); // Update the language when the GameObject is enabled.
    }

    public void LanguageUpdate()
    {
        string path = LanguageClassTools.GetFolderPath(folderNameInUnity, folderNameInBuild); // Set the path to locate language files in Unity Editor or in the build.
        savePath = LanguageClassTools.GetJsonPath(jsonNameInUnity, jsonNameInBuild); // Set the save path for the Unity Editor or for the build.
        defaultFile = LanguageClassTools.FindDefaultLanguageFile(path, standardFile); // Find the default language file based on the StandardFile setting.

        // Restart all Lines.
        foreach (LanguageLines p in languageLines)
        {
            p.foundID = false;
        }

        // Load language data from the saved JSON file if it exists, otherwise use the default language.
        if (File.Exists(savePath))
        {
            string json = File.ReadAllText(savePath);
            LanguageFileSave fileSaveData = JsonUtility.FromJson<LanguageFileSave>(json);
            selectedFile = fileSaveData.selectedFile;

            ReadFile(selectedFile);
        }
        else
        {
            selectedFile = defaultFile;
            ReadFile(defaultFile);
        }
    }

    private void ReadFile(string filePath)
    {
        string[] lines = File.ReadAllLines(filePath); // Read and process the language file specified by filePath.

        // Check that all LanguageLines objects have foundID set to true
        bool allIDsFound = false;
        foreach (LanguageLines lang in languageLines)
        {
            if (lang.foundID)
            {
                allIDsFound = true;
            }
        }

        // Iterate through each line in the file.
        foreach (string line in lines)
        {
            if (line.StartsWith("id:"))
            {
                ProcessLanguageOption(line); // Process the language option.
                if (allIDsFound)
                {
                    break;
                }
            }
        }

        CreateFile(); // Create the file after updating the language lines.
    }

    // Process a language option defined in the given line.
    private void ProcessLanguageOption(string line)
    {
        // Split the line into parts separated by ';'.
        string[] parts = line.Split(';');
        float id = float.Parse(parts[0].Replace("id:", ""));

        // Find the LanguageLines object with the matching ID and update its text.
        for (int i = 0; i < languageLines.Count; i++)
        {
            LanguageLines p = languageLines[i];
            if (p.ID == id)
            {
                p.text = LanguageClassTools.ExtractTextWithinBraces(line);
                languageLines[i] = p;
                p.foundID = true;
                break;
            }
        }
    }

    private void CreateFile()
    {
    #if UNITY_EDITOR
        string folderPath = Path.Combine(Application.dataPath, folderCreateNameInUnity); // Set the folder path for Unity Editor.
    #else
        string folderPath = Path.Combine(Application.dataPath, folderCreateNameInBuild); // Set the folder path for the build.
    #endif

        Directory.CreateDirectory(folderPath); // Create the directory if it doesn't exist.
        string filePath = Path.Combine(folderPath, fileName + fileExtension); // Combine folder path and file name with extension.

        Debug.Log($"File created in: {filePath}");

        // Update the lines in the FileLines list based on the localized text in the languageLines list.
        foreach (LanguageLines lang in languageLines)
        {
            int index = Mathf.RoundToInt(lang.line);
            if (index >= 0 && index < fileLines.Count)
            {
                fileLines[index] = lang.text;
            }
        }

        // Create the file and write the lines.
        using StreamWriter writer = new(filePath);
        foreach (string line in fileLines)
        {
            writer.WriteLine(line);
        }
    }
}

[System.Serializable]
public class LanguageLines
{
    [Header("Warning, do not use duplicate IDs")]
    [Space(5)]
    public string text; // The localized text for this line.
    public float ID = -10f; // The identifier for the text you want to display.
    public float line = 0f; // The line index for this text in the file to be created.
    [HideInInspector] public bool foundID; // Flag to track whether the language text ID was found for this line.
}