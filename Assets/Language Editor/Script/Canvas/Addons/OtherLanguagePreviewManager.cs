using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using UnityEngine;

public class OtherLanguagePreviewManager : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private List<OtherLanguagePreviewList> otherLanguagePreviewList = new(); // List to store language preview entries.
    [Space(10)]
    [Header("Folder Base Path")]
    #pragma warning disable CS0414
    [SerializeField] private string fileNameInUnity = "/StreamingAssets/Language/"; // File path in Unity Editor.
    [SerializeField] private string fileNameInBuild = "/StreamingAssets/Language/"; // File path in built application.
    #pragma warning restore CS0414

    // Adds language preview objects in the scene to the list.
    public void AddOtherLanguagePreviewsInSceneToList()
    {
        string filePath = OpenFile(); // Opens a file dialog to select a text file.

        if (File.Exists(filePath))
        {
            otherLanguagePreviewList.Clear(); // Clears the list before updating.
            OtherLanguagePreview[] previewsInScene = FindObjectsOfType<OtherLanguagePreview>(); // Finds all OtherLanguagePreview objects in the scene.

            foreach (OtherLanguagePreview preview in previewsInScene)
            {
                // Creates an entry for each preview object in the list with its ID.
                OtherLanguagePreviewList previewListEntry = new()
                {
                    otherLanguagePreview = preview,
                    id = preview.organizeIDS.ID
                };

                otherLanguagePreviewList.Add(previewListEntry); // Adds the entry to the list.
            }

            ReadFile(filePath); // Reads the selected file.
            UpdateText(); // Updates text fields based on the read language options.
        }
    }

    // Generates the file path based on Unity Editor or built application.
    private string GetFilePath()
    {
    #if UNITY_EDITOR
        return UnityEngine.Application.dataPath + fileNameInUnity;
    #else
        return UnityEngine.Application.dataPath + fileNameInBuild;
    #endif
    }

    // Opens a file dialog to select a text file.
    private string OpenFile()
    {
        OpenFileDialog openFileDialog = new()
        {
            InitialDirectory = Path.GetDirectoryName(GetFilePath()),
            Filter = "Text Files (*.txt)|*.txt|All files (*.*)|*.*",
            FilterIndex = 1,
            RestoreDirectory = true
        };

        if (openFileDialog.ShowDialog() == DialogResult.OK)
        {
            string selectedFilePath = openFileDialog.FileName;
            UnityEngine.Debug.Log("Selected File: " + selectedFilePath);
            openFileDialog.Dispose();
            return selectedFilePath; // Returns the selected file path.
        }
        else
        {
            UnityEngine.Debug.Log("No file selected.");
            return null;
        }
    }

    // Reads the selected file and processes language options.
    private void ReadFile(string filePath)
    {
        string[] lines = File.ReadAllLines(filePath);

        foreach (string line in lines)
        {
            if (line.StartsWith("id:"))
            {
                ProcessLanguageOption(line); // Processes lines starting with "id:".
            }
        }
    }

    // Processes language options from a line in the file.
    private void ProcessLanguageOption(string line)
    {
        string[] parts = line.Split(';');
        float id = float.Parse(parts[0].Replace("id:", ""));

        foreach (OtherLanguagePreviewList previewEntry in otherLanguagePreviewList)
        {
            if (previewEntry.id == id)
            {
                string text = LanguageClassTools.ExtractTextWithinBraces(line);
                previewEntry.text = text; // Assigns language text to the corresponding preview object.
                break;
            }
        }
    }

    // Updates text fields of OtherLanguagePreview objects with assigned language text.
    private void UpdateText()
    {
        foreach (OtherLanguagePreviewList previewEntry in otherLanguagePreviewList)
        {
            previewEntry.otherLanguagePreview.text.text = previewEntry.text; // Assigns the language text to the text field of the OtherLanguagePreview object.
            previewEntry.otherLanguagePreview.text.interactable = previewEntry.otherLanguagePreview.enterText.interactable; // Sets the text field's interactability based on the enterText field of the OtherLanguagePreview object.
        }
    }
}

// Represents an entry in the language preview list.
[System.Serializable]
public class OtherLanguagePreviewList
{
    public OtherLanguagePreview otherLanguagePreview; // Reference to the OtherLanguagePreview object.
    public float id; // Unique identifier for the language option.
    public string text; // The language text associated with the ID.
}