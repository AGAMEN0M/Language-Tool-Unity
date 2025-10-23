/*
 * ---------------------------------------------------------------------------
 * Description: This script dynamically creates a language file by writing
 *              predefined lines, optionally translated, based on current 
 *              language settings. Supports multilingual file creation for 
 *              both Editor and runtime environments.
 *              
 * Author: Lucas Gomes Cecchini
 * Pseudonym: AGAMENOM
 * ---------------------------------------------------------------------------
*/

using System.Collections.Generic;
using LanguageTools;
using UnityEngine;
using System.IO;

using static LanguageTools.LanguageFileManager;

#if UNITY_EDITOR
using UnityEditor;
using static LanguageTools.Editor.LanguageEditorUtilities;
#endif

[AddComponentMenu("Language/3D Object/Language Create File")]
public class LanguageCreateFile : MonoBehaviour
{
    #region === Serialized Fields ===

    [Header("File Creator Settings")]
    [SerializeField, Tooltip("Output file name.")]
    private string fileName = "Test File";

    [SerializeField, Tooltip("Output file extension.")]
    private string fileExtension = ".txt";

    #pragma warning disable CS0414
    [SerializeField, Tooltip("Folder path in Unity Editor.")]
    private string folderInUnity = "Editor";

    [SerializeField, Tooltip("Folder path in builds.")]
    private string folderInBuild = "StreamingAssets";
    #pragma warning restore CS0414

    [Space(5)]

    [SerializeField, Tooltip("List of lines to include in the generated file.")]
    private List<LanguageLines> fileLines = new()
    {
        new() { text = "Language Create File:" },
        new() { iD = -5, text = "Test Language", translateText = true }
    }; // List of file lines, some of which may be localized.
    
    #endregion

    #region === Private Fields ===

    private LanguageSettingsData languageData; // Current language settings.

    #endregion

    #region === Properties ===

    /// <summary>
    /// Gets or sets the file name.
    /// </summary>
    public string FileName
    {
        get => fileName;
        set => fileName = value;
    }

    /// <summary>
    /// Gets or sets the file extension.
    /// </summary>
    public string FileExtension
    {
        get => fileExtension;
        set => fileExtension = value;
    }

    /// <summary>
    /// Gets or sets the folder path used in the Unity Editor.
    /// </summary>
    public string FolderInUnity
    {
        get => folderInUnity;
        set => folderInUnity = value;
    }

    /// <summary>
    /// Gets or sets the folder path used in builds.
    /// </summary>
    public string FolderInBuild
    {
        get => folderInBuild;
        set => folderInBuild = value;
    }

    /// <summary>
    /// Gets or sets the list of lines to include in the generated file.
    /// </summary>
    public List<LanguageLines> FileLines
    {
        get => fileLines;
        set => fileLines = value;
    }

    #endregion

    #region === Unity Events ===

    /// <summary>
    /// Subscribes to language update event when enabled.
    /// </summary>
    private void OnEnable()
    {
        LanguageManagerDelegate.OnLanguageUpdate += LanguageUpdate; // Subscribe to language update delegate.
        LanguageUpdate(); // Perform immediate update to ensure translated content is written.
    }

    /// <summary>
    /// Unsubscribes from language update event when disabled.
    /// </summary>
    private void OnDisable() => LanguageManagerDelegate.OnLanguageUpdate -= LanguageUpdate;
    
    #endregion

    #region === Core Methods ===

    /// <summary>
    /// Updates translated lines and writes them to a file based on current language settings.
    /// </summary>
    public void LanguageUpdate()
    {
        // Attempt to load language settings.
        languageData = LoadLanguageSettings();
        if (languageData == null)
        {
            Debug.LogError("LanguageCreateFile: Failed to load LanguageSettingsData.", this);
            return;
        }

        // Iterate over each line and apply translation if required.
        foreach (var line in fileLines)
        {
            if (line.translateText)
            {
                var translated = GetIDText(languageData.idData, line.iD); // Retrieve translated text based on ID.
                if (!string.IsNullOrEmpty(translated)) line.text = translated; // Only apply the translation if it's non-empty.
            }
        }

        CreateFile(GetFolderPath()); // Create or overwrite the output file with translated content.
    }

    #endregion

    #region === Helper Methods ===

    /// <summary>
    /// Returns the appropriate folder path depending on build environment.
    /// </summary>
    private string GetFolderPath()
    {
    #if UNITY_EDITOR
        return Path.Combine(Application.dataPath, folderInUnity); // Use Unity-specific data path in Editor.
    #else
        return Path.Combine(Application.streamingAssetsPath, folderInBuild); // Use streaming assets path for builds.
    #endif
    }

    /// <summary>
    /// Writes all file lines to disk at the specified path.
    /// </summary>
    /// <param name="folderPath">Destination folder path.</param>
    private void CreateFile(string folderPath)
    {
        Directory.CreateDirectory(folderPath); // Ensure the directory exists before writing.
        var filePath = Path.Combine(folderPath, $"{fileName}{fileExtension}"); // Compose the full path to the file.

        Debug.Log($"Language file created at: {filePath}");

        // Write each line to the file.
        using StreamWriter writer = new(filePath);
        foreach (var line in fileLines) writer.WriteLine(line.text);
    }

    #endregion
}

#if UNITY_EDITOR

#region === Custom Editor ===

/// <summary>
/// Draws the custom inspector with file preview and import functionality.
/// </summary>
[CanEditMultipleObjects]
[CustomEditor(typeof(LanguageCreateFile))]
public class LanguageCreateFileEditor : Editor
{
    private LanguageCreateFile script; // Reference for the script.

    public override void OnInspectorGUI()
    {
        // Sync serialized fields with inspector.
        serializedObject.Update();
        script = (LanguageCreateFile)target;

        // Create GUIContent with tooltip for the Import Settings button.
        GUIContent importButtonContent = new("Import Settings", "Imports or updates translatable lines based on LanguageSettingsData.\nIf an existing ID is found, you will be prompted to confirm replacement.");

        using (new EditorGUI.DisabledScope(targets.Length > 1))
        {
            // Button to import settings into the fileLines list.
            if (GUILayout.Button(importButtonContent, CreateCustomButtonStyle(15), GUILayout.Height(30)))
            {
                // Prevent accidental overwrite of existing IDs.
                if (script.FileLines.Exists(line => IsIDInLanguageList(line.iD)) && !EditorUtility.DisplayDialog("Replace ID", "An ID already exists. Do you want to replace it?", "Yes", "No"))
                {
                    return;
                }

                // Open editor window for each translatable line.
                foreach (var line in script.FileLines)
                {
                    if (line.translateText) OpenEditorWindowWithComponent(line.iD, 5, line.text, 0, 0, 0);
                }
            }
        }

        // Draw Unity's default inspector UI.
        EditorGUILayout.Space(5);
        DrawDefaultInspector();

        EditorGUILayout.Space(10);

        // File preview section.
        DrawColoredBox(() =>
        {
            EditorGUILayout.LabelField("File Preview", CreateLabelStyle(13, true));

            if (targets.Length == 1)
            {
                DrawFilePreview();
            }
            else
            {
                EditorGUILayout.HelpBox("File Preview is only available when a single object is selected.", MessageType.Info);
            }
        }, new(0, 0, 0, 0.2f));

        serializedObject.ApplyModifiedProperties(); // Apply serialized property changes.
    }

    /// <summary>
    /// Renders the preview of the file content in the Unity Editor.
    /// </summary>
    private void DrawFilePreview()
    {
        GUIStyle labelStyle = new(EditorStyles.label){ wordWrap = true, stretchWidth = true };
        var color = new Color(0, 0, 0, 0.15f);

        // Render file name heading.
        DrawColoredBox(() =>
        {
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField($"{script.FileName}{script.FileExtension}", CreateLabelStyle(13, true));
            EditorGUILayout.Space(10);
        }, color);

        EditorGUILayout.Space(10);

        // Render each line of the file.
        DrawColoredBox(() =>
        {
            EditorGUILayout.Space(10);
            foreach (var line in script.FileLines) EditorGUILayout.LabelField(line.text, labelStyle);
            EditorGUILayout.Space(10);
        }, color);
    }
}

#endregion

#endif