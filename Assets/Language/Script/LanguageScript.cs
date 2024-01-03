using UnityEngine;
using System.IO;
using System.Reflection;

#if UNITY_EDITOR
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
#endif

[AddComponentMenu("Language/Language Script")]
public class LanguageScript : MonoBehaviour
{    
    #if UNITY_EDITOR
    [CustomEditor(typeof(LanguageScript))]
    public class LanguageScriptEditor : Editor
    {
        // Static list to hold IDs for language options.
        public static List<float> IDs = new();
        private bool savedID;

        private void OnEnable()
        {
            LanguageClassToolsEditor.LoadDataFromFile(ref IDs); // Load data from file into the IDs list.
        }

        // Custom Inspector GUI for LanguageScript.
        public override void OnInspectorGUI()
        {
            // Update serialized object.
            serializedObject.Update();
            LanguageScript script = (LanguageScript)target;

            // Display the script object field (disabled).
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.ObjectField("Script", MonoScript.FromMonoBehaviour((MonoBehaviour)target), typeof(MonoBehaviour), false);
            EditorGUI.EndDisabledGroup();

            EditorGUILayout.Space(15f);

            // Button to save settings.
            if (GUILayout.Button("Save Settings"))
            {
                bool shouldSave = true;

                // Confirmation dialog for overwriting existing ID.
                if (savedID == true)
                {
                    shouldSave = EditorUtility.DisplayDialog("Save Confirmation", "An ID with this value already exists. Are you sure you want to save?\nThis action will overwrite the currently saved ID.", "Yes", "No");
                }

                if (shouldSave)
                {
                    // Open Language Save Editor window and add language save ID.
                    EditorApplication.ExecuteMenuItem("Window/Language/Language Save Editor");
                    AddLanguageSaveID(script);
                }
            }

            // Display the serialized properties.
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_object"));

            // Check for empty textToSave field.
            if (script.textToSave == "")
            {
                GUI.color = Color.yellow;
                EditorGUILayout.PropertyField(serializedObject.FindProperty("textToSave"));
                EditorGUILayout.HelpBox("It is recommended to put some Text to Save", MessageType.Warning);
                GUI.color = Color.white;
            }
            else
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("textToSave"));
            }

            // Display property fields in the Inspector for various settings.
            EditorGUILayout.PropertyField(serializedObject.FindProperty("scriptName"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("variableName"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("updateMethod"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("methodName"));

            // Check if the script's ID exists in the list of IDs.
            if (LanguageClassToolsEditor.IsIDInTextIDList(script.ID, ref IDs))
            {
                // If the ID exists, mark it as saved, change GUI color, and display a warning message.
                savedID = true;
                GUI.color = Color.yellow;
                EditorGUILayout.PropertyField(serializedObject.FindProperty("ID"));
                EditorGUILayout.HelpBox("There is an ID with this number Saved!", MessageType.Warning);
                GUI.color = Color.white;
            }
            else
            {
                // If the ID doesn't exist, mark it as unsaved and display the ID field normally.
                savedID = false;
                EditorGUILayout.PropertyField(serializedObject.FindProperty("ID"));
            }

            // Display property fields for file paths and settings.
            EditorGUILayout.PropertyField(serializedObject.FindProperty("standardFile"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("selectedFile"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("savePath"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("defaultFile"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("jsonNameInUnity"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("folderNameInUnity"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("jsonNameInBuild"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("folderNameInBuild"));

            serializedObject.ApplyModifiedProperties(); // Apply any changes made in the Inspector to the serialized properties.
        }

        public void AddLanguageSaveID(LanguageScript script)
        {
            var window = EditorWindow.GetWindow<LanguageSaveEditorWindow>(); // Get the LanguageSaveEditorWindow instance.

            var existingLanguageSaveID = window.textID.FirstOrDefault(id => id.ID == script.ID); // Check if the language save ID already exists.

            if (existingLanguageSaveID != null)
            {
                // Clear existing language save ID values and update properties.
                LanguageClassToolsEditor.ClearExistingLanguageSaveIDValues(existingLanguageSaveID);
                existingLanguageSaveID.componentType = 4;
                existingLanguageSaveID.textWrite = true;
                existingLanguageSaveID.text = script.textToSave;
            }
            else
            {
                // Create a new language save ID if it doesn't exist.
                var languageSaveID = new LanguageSaveID
                {
                    ID = script.ID,
                    componentType = 4,
                    textWrite = true,
                    text = script.textToSave
                };
                window.textID.Add(languageSaveID);
            }

            // Save data to file and update IDs list.
            window.SaveDataJson();
            LanguageClassToolsEditor.LoadDataFromFile(ref IDs);
        }
    }
    #endif

    [Header("Settings")]
    [SerializeField] private GameObject _object; // The GameObject to which the script with the localized variable belongs.
    [Space(5)]
    [SerializeField][Multiline] private string textToSave = "Write the Text Here..."; // Holds the text that will be saved or used for localization purposes.
    [Space(5)]
    [SerializeField] private string scriptName; // The name of the script attached to the GameObject.
    [SerializeField] private string variableName; // The name of the variable in the script that will hold the localized text.
    [Space(5)]
    [SerializeField] private bool updateMethod = false; // If true, the method specified by MethodName will be called after updating the variable.
    [SerializeField] private string methodName; // The name of the method to be called after updating the variable.
    [SerializeField] private float ID = -16; // The identifier for the text you want to display.
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

    private bool foundID; // Flag to track if the language text option ID has been found.

    private void Start()
    {
        LanguageUpdate(); // Update the language when the script starts.
    }

    private void OnEnable()
    {
        LanguageUpdate(); // Update the language when the GameObject is enabled.
    }

    public void LanguageUpdate()
    {
        string path = LanguageClassTools.GetFolderPath(folderNameInUnity, folderNameInBuild); // Set the path to locate language files in Unity Editor or in the build.
        savePath = LanguageClassTools.GetJsonPath(jsonNameInUnity, jsonNameInBuild); // Set the save path for the Unity Editor or for the build.
        defaultFile = LanguageClassTools.FindDefaultLanguageFile(path, standardFile); // Find the default language file based on the StandardFile setting.
        foundID = false; // Reset the loop.

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

        // Iterate through each line in the file.
        foreach (string line in lines)
        {
            if (line.StartsWith("id:"))
            {
                ProcessLanguageOption(line); // Process the language option.
                if (foundID)
                {
                    break;
                }
            }
        }
    }

    // Process a language option defined in the given line.
    private void ProcessLanguageOption(string line)
    {
        // Split the line into parts separated by ';'.
        string[] parts = line.Split(';');
        float id = float.Parse(parts[0].Replace("id:", ""));

        if (ID != id)
        {
            return; // Skip processing if the ID does not match the current language text option ID.
        }

        foundID = true; // Mark that the language option ID has been found.

        string text = LanguageClassTools.ExtractTextWithinBraces(line); // Extract text from the line.
        textToSave = $"[Text Preview] {text}"; // Display a text preview in the Unity Inspector.

        Component myScript = _object.GetComponent(scriptName); // Get the script component attached to the specified GameObject.
        FieldInfo field = myScript.GetType().GetField(variableName); // Get the FieldInfo of the specified variable in the script.
        field.SetValue(myScript, text); // Set the localized text to the variable in the script.

        // If UpdateMethod is true, invoke the specified method in the script.
        if (updateMethod == true)
        {
            MethodInfo method = myScript.GetType().GetMethod(methodName); // Get the MethodInfo of the specified method in the script.
            if (method != null)
            {
                method.Invoke(myScript, null); // Invoke the method in the script.
            }
            else
            {
                Debug.LogError($"Method '{methodName}' not found in the script '{scriptName}'.");
            }
        }
    }
}