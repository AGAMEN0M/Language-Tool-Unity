/*
 * ---------------------------------------------------------------------------
 * Description: A Unity editor script for converting language files to Excel-compatible format.
 * Author: Lucas Gomes Cecchini
 * Pseudonym: AGAMENOM
 * ---------------------------------------------------------------------------
*/

#if UNITY_EDITOR
using System.Collections.Generic;
using LanguageTools.Editor;
using LanguageTools;
using UnityEditor;
using UnityEngine;
using System.Linq;
using System.IO;
using System;

// Window class for managing language conversion to Excel
public class LanguageExcelConverterWindow : EditorWindow
{
    private string folderPath; // Path to the folder containing language files
    public List<string> fileNames = new(); // List of language file names

    private const string jsonPath = "ProjectSettings/LanguageExcelTableData.json"; // Path for saving JSON data
    private SerializedObject serializedObject; // Serialized object for tracking changes in the editor
    public Vector2 scrollPosition = Vector2.zero; // Stores scroll position for GUI
    public bool firstTime = false; // Flag for first-time window load

    // Adds a menu item to open the Language Excel Converter window
    [MenuItem("Window/Language/Language Excel Converter")]
    public static void ShowEditorWindow()
    {
        var window = GetWindow<LanguageExcelConverterWindow>("Language Excel Converter"); // Opens the window
        window.titleContent = new GUIContent("Language Excel Converter", LanguageEditorUtilities.FindTextureByName("LanguageExcelConverterWindow Icon")); // Sets window title and icon
    }

    // Called when the window is enabled
    private void OnEnable()
    {
        Undo.undoRedoPerformed += OnUndoRedo; // Subscribe to undo/redo events
        serializedObject = new SerializedObject(this); // Create a serialized object to track changes
        folderPath = LanguageFileManager.GetLanguageFilesFolderPath(); // Set folder path for language files
        LoadDataJson(); // Load previously saved data from JSON
        RefreshFileList(); // Refresh the list of language files
    }

    // Called when the window is destroyed
    private void OnDestroy()
    {
        Undo.undoRedoPerformed -= OnUndoRedo; // Unsubscribe from undo/redo events
        SaveDataJson(); // Save current data to JSON
    }

    // Called when an undo or redo action occurs
    private void OnUndoRedo()
    {
        Repaint(); // Repaint the window to reflect the changes
    }

    // Main method for drawing the GUI elements
    private void OnGUI()
    {
        serializedObject.Update(); // Update serialized object to track GUI changes

        if (!firstTime) // If this is the first time the window is opened
        {
            LoadDataJson(); // Load saved data
            firstTime = true;
        }

        Undo.RecordObject(this, "CheckFiles"); // Record changes for undo

        // Display header and scrollable area
        EditorGUILayout.LabelField("Excel Table Manager", LanguageEditorUtilities.CreateLabelStyle(15, true));
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

        // Display the file names property in the GUI
        EditorGUILayout.PropertyField(serializedObject.FindProperty("fileNames"));

        EditorGUILayout.Space(20);

        // Display buttons for converting files and refreshing the file list
        EditorGUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();

        if (GUILayout.Button("Convert to Excel", LanguageEditorUtilities.CreateCustomButtonStyle(15), GUILayout.Width(160), GUILayout.Height(30)))
        {
            ConvertToExcel(); // Trigger conversion to Excel
        }

        EditorGUILayout.Space(10);

        if (GUILayout.Button("Update List", LanguageEditorUtilities.CreateCustomButtonStyle(15), GUILayout.Width(160), GUILayout.Height(30)))
        {
            RefreshFileList(); // Update the file list
        }

        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.EndScrollView(); // End scrollable area

        EditorUtility.SetDirty(this); // Mark object as dirty if any changes occurred
        serializedObject.ApplyModifiedProperties(); // Apply any changes to the serialized object
    }

    // Refreshes the list of text files in the language folder
    private void RefreshFileList()
    {
        if (Directory.Exists(folderPath)) // Check if the folder exists
        {
            var txtFiles = Directory.GetFiles(folderPath, "*.txt"); // Get all text files in the folder
            var existingFiles = new HashSet<string>(fileNames); // Store current file names
            var currentFiles = new HashSet<string>(txtFiles.Select(Path.GetFileName)); // Get the current list of files

            // Add new files to the list
            foreach (var filePath in txtFiles)
            {
                var fileName = Path.GetFileName(filePath);
                if (!existingFiles.Contains(fileName))
                {
                    fileNames.Add(fileName);
                }
            }

            // Remove files that no longer exist
            fileNames.RemoveAll(fileName => !currentFiles.Contains(fileName));
        }
        else
        {
            Debug.LogWarning($"The specified directory '{folderPath}' does not exist."); // Log a warning if the folder does not exist
        }
    }

    // Converts the language files to an Excel-compatible CSV format
    private void ConvertToExcel()
    {
        string[] languages = fileNames.ConvertAll(file => Path.GetFileNameWithoutExtension(file)).ToArray(); // Extract language names from file names
        List<int> iDs = new(); // List to store component IDs
        List<ExcelData> excelDataList = new(); // List to store Excel data

        try
        {
            // Read the first language file to extract IDs
            string[] lines = File.ReadAllLines($"{folderPath}/{fileNames[0]}");

            // Extract component IDs from the first file
            foreach (string line in lines)
            {
                if (line.StartsWith("id:"))
                {
                    string[] parts = line.Split(';');
                    if (int.TryParse(parts[0].Replace("id:", "").Trim(), out int id))
                    {
                        iDs.Add(id);
                    }
                }
            }

            // Extract text data from each language file and build Excel data
            foreach (string file in fileNames)
            {
                string filePath = $"{folderPath}/{file}";

                foreach (int id in iDs)
                {
                    string lineText = LanguageFileManager.FindLineByID(filePath, id); // Find line by ID
                    string text = LanguageFileManager.ExtractTextBetweenBraces(lineText); // Extract text within braces

                    ExcelData data = new()
                    {
                        language = Path.GetFileNameWithoutExtension(file),
                        lines = new List<KeyValuePair<int, string>> { new(id, text) }
                    };

                    excelDataList.Add(data); // Add data to the list
                }
            }

            // Open save panel to allow user to select save location for the CSV
            string savePath = EditorUtility.SaveFilePanel("Save CSV File", folderPath, "LanguageData.csv", "csv");
            if (!string.IsNullOrEmpty(savePath))
            {
                ExportToCsv(excelDataList, languages, savePath); // Export data to CSV
                EditorUtility.RevealInFinder(savePath); // Reveal the saved file in the file explorer
            }
        }
        catch (Exception ex) // Handle any errors during the process
        {
            Debug.LogError($"An error occurred while converting to Excel: {ex.Message}");
        }
    }

    // Exports the Excel data to a CSV file
    private void ExportToCsv(List<ExcelData> dataList, string[] languages, string filePath)
    {
        try
        {
            using StreamWriter writer = new(filePath, false, System.Text.Encoding.UTF8); // Open a stream to write the CSV file

            // Write the header row
            writer.Write("ID");
            foreach (var lang in languages)
            {
                writer.Write($";{lang.ToUpper()}");
            }
            writer.WriteLine();

            Dictionary<int, List<string>> dictionary = new(); // Dictionary to store ID and corresponding language text

            // Populate dictionary with text data for each ID
            foreach (var data in dataList)
            {
                foreach (var line in data.lines)
                {
                    if (!dictionary.ContainsKey(line.Key))
                    {
                        dictionary[line.Key] = new List<string>(new string[languages.Length]); // Initialize list for each language
                    }
                    int index = Array.IndexOf(languages, data.language); // Find index of the language
                    //dictionary[line.Key][index] = line.Value;
                    dictionary[line.Key][index] = line.Value.Replace(";", "¦"); // Store text, replacing semicolons with a different character
                }
            }

            // Write the data rows to the CSV file
            foreach (var entry in dictionary)
            {
                writer.Write($"{entry.Key}");
                foreach (var text in entry.Value)
                {
                    writer.Write($";{text ?? ""}");
                }
                writer.WriteLine();
            }
        }
        catch (Exception ex) // Handle errors during CSV export
        {
            Debug.LogError($"Error while saving CSV file: {ex.Message}");
        }
    }

    // Saves the current list of files to a JSON file
    private void SaveDataJson()
    {
        ExcelTableData data = new()
        {
            filesData = fileNames
        };
        string jsonData = JsonUtility.ToJson(data); // Convert data to JSON

        try
        {
            File.WriteAllText(jsonPath, jsonData); // Write JSON data to the file
        }
        catch (Exception e) // Handle errors during the save process
        {
            Debug.LogError($"Error while saving data: {e.Message}");
        }
    }

    // Loads the file list data from a JSON file
    private void LoadDataJson()
    {
        if (File.Exists(jsonPath)) // Check if the JSON file exists
        {
            string jsonData = File.ReadAllText(jsonPath); // Read JSON data from the file
            var data = JsonUtility.FromJson<ExcelTableData>(jsonData); // Deserialize the JSON data

            fileNames.Clear(); // Clear current file list
            fileNames = data.filesData; // Load file names from JSON data
        }
    }
}
#endif