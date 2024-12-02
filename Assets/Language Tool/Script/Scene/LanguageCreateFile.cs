/*
 * ---------------------------------------------------------------------------
 * Description: This script creates a language file with specified lines 
 *              and handles updates from the language manager for multilingual support.
 * Author: Lucas Gomes Cecchini
 * Pseudonym: AGAMENOM
 * ---------------------------------------------------------------------------
*/

using System.Collections.Generic;
using LanguageTools;
using UnityEngine;
using System.IO;

#if UNITY_EDITOR
using LanguageTools.Editor;
using UnityEditor;
#endif

// This component creates a language file with the specified lines and handles updates from the language manager.
[AddComponentMenu("Language/3D Object/Language Create File")]
public class LanguageCreateFile : MonoBehaviour
{
    [Header("File Creator Settings")]
    #pragma warning disable CS0414
    [SerializeField] private string folderInUnity = "Editor"; // Folder path for Unity Editor environment.
    [SerializeField] private string folderInBuild = "StreamingAssets"; // Folder path for builds.
    #pragma warning restore CS0414
    [Space(10)]
    public string fileName = "Test File"; // The name of the file to be created.
    public string fileExtension = ".txt"; // File extension for the created file.
    public List<LanguageLines> fileLines = new() { new LanguageLines { text = "Test Language", translateText = true, iD = -5 } }; // List of language lines to be written to the file.
    [Space(10)]
    [Header("Automatic Information")]
    [SerializeField] private string selectedFile; // Path of the selected language file.
    [Space(5)]
    [SerializeField] private string saveFile; // Path to the save file.

    // Subscribes to the OnLanguageUpdate event when the object is enabled.
    private void OnEnable()
    {
        LanguageManagerDelegate.OnLanguageUpdate += LanguageUpdate; // Subscribe to language updates.
        LanguageUpdate(); // Perform an initial update when enabled.
    }

    // Unsubscribes from the OnLanguageUpdate event when the object is disabled.
    private void OnDisable()
    {
        LanguageManagerDelegate.OnLanguageUpdate -= LanguageUpdate; // Unsubscribe from language updates.
    }

    // Updates the script by checking the save file, processing lines, and creating the file.
    public void LanguageUpdate()
    {
        saveFile = LanguageFileManager.GetSaveFilePath(); // Retrieve the path to the save file.

        // If the save file exists, read the selected language file from it.
        if (File.Exists(saveFile))
        {
            string json = File.ReadAllText(saveFile);
            var saveData = JsonUtility.FromJson<LanguageSaveData>(json); // Deserialize the save data.
            selectedFile = saveData.selectedFile; // Set the selected file path.
        }
        else
        {
            selectedFile = LanguageFileManager.FindDefaultLanguageFilePath(); // Use default language file if no save file is found.
        }

        ProcessLine(); // Process the lines in the language file.
        CreateFile(); // Create the output file with the processed lines.
    }

    // Processes each line in the fileLines list, updating the text with the localized version.
    private void ProcessLine()
    {
        foreach (LanguageLines i in fileLines)
        {
            if (i.translateText) // Only update lines where translation is enabled.
            {
                string text = LanguageFileManager.FindLineByID(selectedFile, i.iD); // Fetch the localized line by ID.
                i.text = LanguageFileManager.ExtractTextBetweenBraces(text); // Extract the text inside curly braces.
            }
        }
    }

    // Retrieves the folder path based on the current environment (Editor or Build).
    private string GetFolderPath()
    {
        string folderPath = "";
    #if UNITY_EDITOR
        folderPath = Path.Combine(Application.dataPath, folderInUnity); // Use the Editor folder path in Unity Editor.
    #else
        folderPath = Path.Combine(Application.dataPath, folderInBuild); // Use the StreamingAssets folder in builds.
    #endif
        return folderPath;
    }

    // Creates the output file with the specified lines.
    private void CreateFile()
    {
        string folderPath = GetFolderPath(); // Get the appropriate folder path.
        Directory.CreateDirectory(folderPath); // Ensure the directory exists.
        string filePath = Path.Combine(folderPath, $"{fileName}{fileExtension}"); // Construct the full file path.

        Debug.Log($"File created in: {filePath}");

        // Write each line in fileLines to the file.
        using StreamWriter writer = new(filePath);
        foreach (LanguageLines i in fileLines)
        {
            writer.WriteLine(i.text); // Write each line to the file.
        }
    }
}

#if UNITY_EDITOR
// Custom editor for the LanguageCreateFile component in the Unity editor.
[CustomEditor(typeof(LanguageCreateFile))]
public class LanguageCreateFileEditor : Editor
{
    LanguageCreateFile script; // Reference to the LanguageCreateFile script.

    // Overrides the default Inspector GUI with custom fields and buttons.
    public override void OnInspectorGUI()
    {
        serializedObject.Update(); // Update the serialized properties of the object.
        script = (LanguageCreateFile)target; // Reference to the target script.
        LanguageEditorUtilities.DrawReadOnlyMonoScriptField(target); // Draws the read-only field for the script reference.

        EditorGUILayout.Space(5);

        // Button to import the settings for the language lines.
        if (GUILayout.Button("Import Settings", LanguageEditorUtilities.CreateCustomButtonStyle(15), GUILayout.Height(30)))
        {
            bool alreadySaved = false;

            // Check if any of the IDs are already saved.
            foreach (LanguageLines i in script.fileLines)
            {
                if (LanguageEditorUtilities.IsIDInLanguageList(i.iD))
                {
                    alreadySaved = true;
                    break;
                }
            }

            // If any ID is already saved, prompt the user to confirm if they want to replace it.
            if (alreadySaved)
            {
                if (!EditorUtility.DisplayDialog("Replace ID", "An ID with this number is already saved. Do you want to replace it?", "Yes", "No"))
                {
                    return; // Exit if the user chooses not to replace the ID.
                }
            }

            // Open the editor window for each line's ID where translation is enabled.
            foreach (LanguageLines i in script.fileLines)
            {
                if (i.translateText)
                {
                    LanguageEditorUtilities.OpenEditorWindowWithComponent(i.iD, 5, i.text, 0, 0, 0, true, false, false, false);
                }
            }
        }

        // Fields for folder paths, file name, and file extension.
        EditorGUILayout.PropertyField(serializedObject.FindProperty("folderInUnity"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("folderInBuild"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("fileName"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("fileExtension"));

        // Field for the list of language lines.
        EditorGUILayout.PropertyField(serializedObject.FindProperty("fileLines"));
        foreach (LanguageLines i in script.fileLines)
        {
            // Display a warning if any of the IDs are already saved and translation is enabled.
            if (i.translateText && LanguageEditorUtilities.IsIDInLanguageList(i.iD))
            {
                GUI.color = Color.yellow;
                EditorGUILayout.HelpBox($"There is an ID ({i.iD}) with this number Saved!", MessageType.Warning);
                GUI.color = Color.white;
            }
        }

        // Fields for displaying the selected file and save file paths.
        EditorGUILayout.PropertyField(serializedObject.FindProperty("selectedFile"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("saveFile"));

        // Display a preview of the file content.
        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("File Preview", LanguageEditorUtilities.CreateLabelStyle(13, true));
        DrawFilePreview();

        serializedObject.ApplyModifiedProperties(); // Apply any modified properties.
    }

    // Draws a preview of the file content in the editor.
    private void DrawFilePreview()
    {
        GUIStyle labelStyle = new(EditorStyles.label)
        {
            wordWrap = true, // Enable word wrapping for the text.
            stretchWidth = true
        };

        // Display the file name and extension.
        LanguageEditorUtilities.DrawColoredBox(() =>
        {
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField($"{script.fileName}{script.fileExtension}", LanguageEditorUtilities.CreateLabelStyle(13, true));
            EditorGUILayout.Space(10);
        }, new(0, 0, 0, 0.15f));

        EditorGUILayout.Space(5);

        // Display the text content of each line.
        LanguageEditorUtilities.DrawColoredBox(() =>
        {
            EditorGUILayout.Space(10);
            foreach (LanguageLines i in script.fileLines)
            {
                EditorGUILayout.LabelField($"{i.text}", labelStyle); // Display each line's text.
            }
            EditorGUILayout.Space(10);
        }, new(0, 0, 0, 0.15f));
    }
}
#endif