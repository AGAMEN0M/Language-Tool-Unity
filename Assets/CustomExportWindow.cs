#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System;

public class CustomExportWindow : EditorWindow
{
    public readonly Dictionary<string, bool> folderStates = new(); // Dictionary to store the selection state (true/false) of folders and files.
    public readonly Dictionary<string, int> folderIndentationLevels = new(); // Dictionary to store the indentation level of folders in the hierarchy.

    // Arrays to store folder GUIDs and asset file paths.
    public string[] folderGuids;
    public string[] assetFiles;
    public string[] files;

    public GUIStyle folderLabelStyle; // GUI style for folder labels.
    public Vector2 scrollPosition = Vector2.zero; // Scroll position for the GUI window.
    public bool onEnable; // Flag to track if the OnEnable method has been called.

    // Menu item to open the custom export window.
    [MenuItem("Assets/Custom Export Package...", false, 23)]
    public static void ShowWindow()
    {
        GetWindow<CustomExportWindow>("Custom Export Package");
    }

    // Method called to draw the GUI.
    void OnGUI()
    {
        if (onEnable == false)
        {
            // Initialize folder label style and folder selection states.
            folderLabelStyle = new GUIStyle(EditorStyles.label);
            folderLabelStyle.padding.left = 5;

            // Find all folder assets and initialize their selection states.
            folderGuids = AssetDatabase.FindAssets("t:Folder");
            foreach (string folderGuid in folderGuids)
            {
                string folderPath = AssetDatabase.GUIDToAssetPath(folderGuid);
                if (folderPath.StartsWith("Assets/"))
                {
                    folderStates[folderPath] = false;
                }
            }
            onEnable = true;
        }

        Tools(); // Draw tools section (e.g., select all, select none).

        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

        // Draw files and folders selection section.
        DrawBlackBorder(() =>
        {
            Files();
            FilesInsideFolders();
        });

        EditorGUILayout.EndScrollView();

        MoreTools(); // Draw additional tools (e.g., export, save, load).
    }

    // Method to display tools for selecting assets and folders.
    private void Tools()
    {
        GUIStyle style = new(EditorStyles.wordWrappedLabel){fontSize = 16};

        GUILayout.Space(10);
        EditorGUILayout.LabelField("Items to Export", style);
        GUILayout.Space(15);

        EditorGUILayout.BeginHorizontal();

        GUILayout.Space(10);

        if (GUILayout.Button("All"))
        {
            SetAllItemsSelection(true);
        }

        GUILayout.Space(10);

        if (GUILayout.Button("None"))
        {
            SetAllItemsSelection(false);
        }

        GUILayout.Space(10);

        EditorGUILayout.EndHorizontal();
        GUILayout.Space(10);

    }

    // Method to draw a black border around content.
    private void DrawBlackBorder(System.Action content)
    {
        Rect rect = EditorGUILayout.BeginVertical();
        Color BlackColor = new(0.2f, 0.2f, 0.2f, 1f);

        EditorGUI.DrawRect(rect, BlackColor);
        content.Invoke();
        GUILayout.EndVertical();
    }

    // Method to display individual asset files.
    private void Files()
    {
        assetFiles = Directory.GetFiles("Assets/");
        foreach (string assetFile in assetFiles)
        {
            if (assetFile.StartsWith("Assets/") && !assetFile.EndsWith(".meta"))
            {
                string fileName = Path.GetFileName(assetFile);
                EditorGUILayout.BeginHorizontal();
                GUILayout.Space(0);

                GUIContent content = new(fileName, GetFileIcon(fileName));
                bool isSelected = folderStates.ContainsKey(assetFile) && folderStates[assetFile];
                bool newSelection = EditorGUILayout.ToggleLeft(content, isSelected, folderLabelStyle);
                EditorGUILayout.EndHorizontal();

                if (newSelection != isSelected)
                {
                    folderStates[assetFile] = newSelection;
                }

                string metaFilePath = assetFile + ".meta";
                if (newSelection && File.Exists(metaFilePath))
                {
                    folderStates[metaFilePath] = true;
                }
            }
        }
    }

    // Method to display asset files inside folders.
    private void FilesInsideFolders()
    {
        foreach (string folderGuid in folderGuids)
        {
            string folderPath = AssetDatabase.GUIDToAssetPath(folderGuid);
            if (folderPath.StartsWith("Assets/"))
            {
                bool isRootFolder = folderPath.Split('/').Length == 2;
                bool isParentActive = isRootFolder || (folderStates.ContainsKey(GetParentFolderPath(folderPath)) && folderStates[GetParentFolderPath(folderPath)]);
                bool isSelected = folderStates.ContainsKey(folderPath) ? folderStates[folderPath] : isRootFolder;
                int indentationLevel = isRootFolder ? 0 : GetIndentationLevel(folderPath);

                if (!isRootFolder && !isParentActive)
                {
                    folderStates[folderPath] = false;
                    continue;
                }

                string folderName = Path.GetFileName(folderPath);

                EditorGUILayout.BeginHorizontal();
                GUILayout.Space(indentationLevel * 20);
                bool newSelection = EditorGUILayout.ToggleLeft(new GUIContent(folderName, EditorGUIUtility.FindTexture("Folder Icon")), isSelected, folderLabelStyle);
                EditorGUILayout.EndHorizontal();

                if (newSelection != isSelected)
                {
                    folderStates[folderPath] = newSelection;
                }

                if (newSelection)
                {
                    DisplayFilesInFolder(folderPath, indentationLevel);
                }
            }
        }
    }

    // Method to display additional tools (e.g., export, save, load).
    private void MoreTools()
    {
        GUILayout.Space(10);
        EditorGUILayout.BeginHorizontal();

        GUILayout.Space(10);

        if (GUILayout.Button("Exportar")) // Export button.
        {
            ExportPackages();
            Close();
        }

        GUILayout.Space(10);

        if (GUILayout.Button("Save")) // Save button.
        {
            SaveData();
        }

        GUILayout.Space(10);

        if (GUILayout.Button("Load Save")) // Load button.
        {
            LoadData();
        }

        GUILayout.Space(10);

        EditorGUILayout.EndHorizontal();
        GUILayout.Space(10);
    }

    // Method to display files inside a folder.
    private void DisplayFilesInFolder(string folderPath, int indentationLevel)
    {
        files = Directory.GetFiles(folderPath);
        foreach (string filePath in files)
        {
            if (filePath.StartsWith("Assets/") && !filePath.EndsWith(".meta"))
            {
                string fileName = Path.GetFileName(filePath);
                EditorGUILayout.BeginHorizontal();
                GUILayout.Space((indentationLevel + 1) * 20);

                GUIContent content = new(fileName, GetFileIcon(filePath));
                bool isSelected = folderStates.ContainsKey(filePath) && folderStates[filePath];
                bool newSelection = EditorGUILayout.ToggleLeft(content, isSelected, folderLabelStyle);
                EditorGUILayout.EndHorizontal();

                if (newSelection != isSelected)
                {
                    folderStates[filePath] = newSelection;
                }

                string metaFilePath = filePath + ".meta";
                if (newSelection && File.Exists(metaFilePath))
                {
                    folderStates[metaFilePath] = true;
                }
            }
        }
    }

    // Method to export selected assets and folders as a Unity package.
    private void ExportPackages()
    {
        List<string> selectedItems = new();
        foreach (var kvp in folderStates)
        {
            if (kvp.Value)
            {
                selectedItems.Add(kvp.Key);
            }
        }

        if (selectedItems.Count > 0)
        {
            string initialDirectory = "Assets/";

            List<string> filesToExport = new();
            foreach (string selectedItem in selectedItems)
            {
                if (File.Exists(selectedItem) || selectedItem.EndsWith(".meta"))
                {
                    filesToExport.Add(selectedItem);
                }
            }

            string exportPath = EditorUtility.SaveFilePanel("Export Package", initialDirectory, "ExportedPackage", "unitypackage");

            if (!string.IsNullOrEmpty(exportPath))
            {
                AssetDatabase.ExportPackage(filesToExport.ToArray(), exportPath, ExportPackageOptions.Recurse);
            }
        }
        else
        {
            Debug.Log("No items selected for export.");
        }
    }

    // Method to get the parent folder path from a given path.
    private string GetParentFolderPath(string path)
    {
        int lastSlashIndex = path.LastIndexOf("/");
        if (lastSlashIndex >= 0)
        {
            return path[..lastSlashIndex];
        }
        return "";
    }

    // Method to get the indentation level of a folder in the hierarchy.
    private int GetIndentationLevel(string folderPath)
    {
        if (folderIndentationLevels.ContainsKey(folderPath))
        {
            return folderIndentationLevels[folderPath];
        }
        else
        {
            string parentPath = GetParentFolderPath(folderPath);
            int parentIndentationLevel = parentPath == "" ? 0 : GetIndentationLevel(parentPath) + 1;
            folderIndentationLevels[folderPath] = parentIndentationLevel;
            return parentIndentationLevel;
        }
    }

    // Method to get the icon for a file based on its path.
    private Texture2D GetFileIcon(string filePath)
    {
        UnityEngine.Object objectToDisplay;

        if (filePath.StartsWith("Assets/"))
        {
            objectToDisplay = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(filePath);
        }
        else
        {
            objectToDisplay = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>("Assets/" + filePath);
        }

        if (objectToDisplay != null)
        {
            Texture2D icon = EditorGUIUtility.ObjectContent(objectToDisplay, objectToDisplay.GetType()).image as Texture2D;

            if (icon != null)
            {
                return icon;
            }
        }

        return EditorGUIUtility.IconContent("DefaultAsset Icon").image as Texture2D;
    }

    // Method to set the selection state for all items (select all or deselect all).
    private void SetAllItemsSelection(bool isSelected)
    {
        foreach (var key in folderStates.Keys.ToList())
        {
            folderStates[key] = isSelected;
        }

        foreach (string assetFile in assetFiles)
        {
            if (assetFile.StartsWith("Assets/") && !assetFile.EndsWith(".meta"))
            {
                folderStates[assetFile] = isSelected;

                string metaFilePath = assetFile + ".meta";
                if (isSelected && File.Exists(metaFilePath))
                {
                    folderStates[metaFilePath] = true;
                }
            }
        }

        foreach (string folderGuid in folderGuids)
        {
            string folderPath = AssetDatabase.GUIDToAssetPath(folderGuid);
            if (folderPath.StartsWith("Assets/"))
            {
                string[] filesInFolder = Directory.GetFiles(folderPath);
                foreach (string filePath in filesInFolder)
                {
                    if (filePath.StartsWith("Assets/") && !filePath.EndsWith(".meta"))
                    {
                        folderStates[filePath] = isSelected;

                        string metaFilePath = filePath + ".meta";
                        if (isSelected && File.Exists(metaFilePath))
                        {
                            folderStates[metaFilePath] = true;
                        }
                    }
                }
            }
        }
    }

    // Method to save selection data to a text file.
    private void SaveData()
    {
        string filePath = EditorUtility.SaveFilePanel("Save Custom Export Data", "UserSettings/", "CustomExportData.txt", "txt");

        if (!string.IsNullOrEmpty(filePath))
        {
            try
            {
                using StreamWriter writer = new(filePath);

                foreach (var kvp in folderStates)
                {
                    writer.WriteLine(kvp.Key + "," + kvp.Value);
                }

                Debug.Log($"Selection data successfully saved to: {filePath}");
            }
            catch (Exception e)
            {
                Debug.LogError($"Error saving selection data: {e.Message}");
            }
        }
    }

    // Method to load selection data from a text file.
    private void LoadData()
    {
        string filePath = EditorUtility.OpenFilePanel("Save Custom Export Data", "UserSettings/", "txt");

        if (!string.IsNullOrEmpty(filePath))
        {
            try
            {
                if (File.Exists(filePath))
                {
                    using StreamReader reader = new(filePath);

                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        string[] parts = line.Split(',');
                        if (parts.Length == 2)
                        {
                            string key = parts[0];
                            bool value = bool.Parse(parts[1]);

                            folderStates[key] = value;
                        }
                    }

                    Debug.Log($"Selection data loaded successfully from: {filePath}");
                }
                else
                {
                    Debug.LogWarning($"The selection data file was not found in: {filePath}");
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Error loading selection data: {e.Message}");
            }
        }
    }
}
#endif