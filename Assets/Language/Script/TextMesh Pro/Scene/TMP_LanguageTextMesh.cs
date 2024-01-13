using UnityEngine;
using System.IO;
using System;
using TMPro;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using System.Collections.Generic;
using System.Linq;
#endif

[AddComponentMenu("Language/TextMesh Pro/Language TextMesh (TMP)")]
public class TMP_LanguageTextMesh : MonoBehaviour
{
    #if UNITY_EDITOR
    [CustomEditor(typeof(TMP_LanguageTextMesh))]
    public class TMP_LanguageTextMeshEditor : Editor
    {
        // List to store IDs for language options.
        public static List<float> IDs = new();
        private bool savedID;

        private void OnEnable()
        {
            LanguageClassToolsEditor.LoadDataFromFile(ref IDs); // Load data from file when the Editor script is enabled.
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update(); // Update serialized object.
            TMP_LanguageTextMesh script = (TMP_LanguageTextMesh)target; // Get the target LanguageTextMesh script.

            EditorGUI.BeginDisabledGroup(true); // Begin a disabled group.
            EditorGUILayout.ObjectField("Script", MonoScript.FromMonoBehaviour((MonoBehaviour)target), typeof(MonoBehaviour), false); // Display script object field.
            EditorGUI.EndDisabledGroup(); // End the disabled group.

            EditorGUILayout.Space(15f); // Add space between elements.

            // Button to import settings.
            if (GUILayout.Button("Import Settings"))
            {
                script.SavaTextMesh(); // Save TextMesh settings.
                EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene()); // Mark the scene as dirty.
                LanguageClassToolsEditor.LoadDataFromFile(ref IDs); // Load data from file.
                script.wasImported = true; // Set the flag to indicate settings were imported.
            }

            // Button to save settings.
            GUI.enabled = script.wasImported; // Enable the button only if settings were previously imported.
            if (GUILayout.Button("Save Settings"))
            {
                bool shouldSave = true;

                if (savedID == true)
                {
                    // Confirmation dialog if ID already exists.
                    shouldSave = EditorUtility.DisplayDialog("Save Confirmation", "An ID with this value already exists. Are you sure you want to save?\nThis action will overwrite the currently saved ID.", "Yes", "No");
                }

                if (shouldSave)
                {
                    EditorApplication.ExecuteMenuItem("Window/Language/Language Save Editor"); // Execute menu item.
                    AddLanguageSaveID(script); // Add language save ID.
                }
            }
            GUI.enabled = true; // Re-enable the GUI after the button.

            // Display property fields for various settings.
            EditorGUILayout.PropertyField(serializedObject.FindProperty("TMP_languageTextMesh"));

            // Check if the ID is saved and display appropriate UI.
            if (LanguageClassToolsEditor.IsIDInTextIDList(script.ID, ref IDs))
            {
                savedID = true;
                GUI.color = Color.yellow; // Change GUI color.
                EditorGUILayout.PropertyField(serializedObject.FindProperty("ID"));// Display ID property field.
                EditorGUILayout.HelpBox("There is an ID with this number Saved!", MessageType.Warning); // Display a help box.
                GUI.color = Color.white; // Reset GUI color.
            }
            else
            {
                savedID = false;
                EditorGUILayout.PropertyField(serializedObject.FindProperty("ID")); // Display ID property field.
            }

            EditorGUILayout.PropertyField(serializedObject.FindProperty("fontSize"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("font"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("fontListObject"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("reverseText"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("standardFile"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("selectedFile"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("savePath"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("defaultFile"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("jsonNameInUnity"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("folderNameInUnity"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("jsonNameInBuild"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("folderNameInBuild"));

            serializedObject.ApplyModifiedProperties(); // Apply modified properties to serialized object.
        }

        // Method to add language save ID
        public void AddLanguageSaveID(TMP_LanguageTextMesh script)
        {
            var window = EditorWindow.GetWindow<LanguageSaveEditorWindow>(); // Get LanguageSaveEditorWindow instance.

            var existingLanguageSaveID = window.textID.FirstOrDefault(id => id.ID == script.ID); // Check if the language save ID already exists.

            if (existingLanguageSaveID != null)
            {
                // Update existing language save ID values.
                LanguageClassToolsEditor.ClearExistingLanguageSaveIDValues(existingLanguageSaveID);
                existingLanguageSaveID.componentType = 2;
                existingLanguageSaveID.textWrite = true;
                existingLanguageSaveID.text = script.TMP_languageTextMesh.text;
                existingLanguageSaveID.fontSizeWrite = true;
                existingLanguageSaveID.fontSize = script.fontSize;
                existingLanguageSaveID.fontWrite = true;
                existingLanguageSaveID.font = script.font;
                existingLanguageSaveID.reverseWrite = true;
                existingLanguageSaveID.reverse = script.reverseText ? 2 : 1;
            }
            else
            {
                // Create a new language save ID.
                var languageSaveID = new LanguageSaveID
                {
                    ID = script.ID,
                    componentType = 2,
                    textWrite = true,
                    text = script.TMP_languageTextMesh.text,
                    fontSizeWrite = true,
                    fontSize = script.fontSize,
                    fontWrite = true,
                    font = script.font,
                    reverseWrite = true,
                    reverse = script.reverseText ? 2 : 1
                };
                window.textID.Add(languageSaveID);
            }

            // Save data to file and reload IDs.
            window.SaveDataJson();
            LanguageClassToolsEditor.LoadDataFromFile(ref IDs);
        }
    }
    #endif

    [Header("Settings")]
    public TMP_Text TMP_languageTextMesh; // Reference to the attached TMP_Text component for displaying localized text.
    [SerializeField] private float ID = -17; // The identifier for the text you want to display.
    [Space(10)]
    [Header("Text Settings")]
    [SerializeField] private int fontSize; // The font size to apply to the TextMesh component.
    [SerializeField] private int font; // The index of the font to apply to the TextMesh component.
    public TMP_LanguageFontListData fontListObject; // Scriptable object containing a list of fonts for TextMesh.
    [SerializeField] private bool reverseText = false; // If true, the text will be reversed before displaying.
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
    [HideInInspector] public bool wasImported; // Flag to track whether settings were imported.

    private bool foundID; // Flag to track if the language text option ID has been found.

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

        // Extract and process various properties of the language option.
        string text = LanguageClassTools.ExtractTextWithinBraces(line);
        string lineWithoutCurlyBraces = LanguageClassTools.RemoveContentWithinCurlyBraces(line);
        fontSize = LanguageClassTools.ExtractIntValue(lineWithoutCurlyBraces, "S:");
        font = LanguageClassTools.ExtractIntValue(lineWithoutCurlyBraces, "Font:");
        reverseText = LanguageClassTools.ExtractBooleanValue(lineWithoutCurlyBraces, "Rev:", reverseText);

        // Set various properties of the language text component based on extracted values.
        if (fontSize != 0)
        {
            TMP_languageTextMesh.fontSize = fontSize;
        }

        if (fontListObject != null && font > 0 && font <= fontListObject.TMP_fontList.Count)
        {
            TMP_languageTextMesh.font = fontListObject.TMP_fontList[font - 1];
        }

        if (reverseText)
        {
            char[] charArray = text.ToCharArray();
            Array.Reverse(charArray);
            text = new string(charArray);
        }

        TMP_languageTextMesh.text = text; // Update the text of the language text component.
    }

    public void SavaTextMesh()
    {
        fontSize = (int)TMP_languageTextMesh.fontSize; // Store the current font size of the TextMesh component.
        TMP_FontAsset fontTemp = TMP_languageTextMesh.font; // Get the currently assigned font in the TextMesh component.
        int index = fontListObject.TMP_fontList.IndexOf(fontTemp) + 1; // Find the index of the font in the font list object.
        if (index > 0) { font = index; } // If a valid index is found, update the 'font' variable with the index value.
    }
}