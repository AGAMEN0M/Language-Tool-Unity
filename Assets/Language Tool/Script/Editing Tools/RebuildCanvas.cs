/*
 * ---------------------------------------------------------------------------
 * Description: A Unity editor script that provides functionality to create 
 *              and manage canvas hierarchies based on language-specific data. It allows 
 *              the loading of language files, creating canvas structures from JSON data, 
 *              and saving the current canvas state. The script integrates with Unity's 
 *              undo system and offers a custom editor interface for easy interaction.
 * Author: Lucas Gomes Cecchini
 * Pseudonym: AGAMENOM
 * ---------------------------------------------------------------------------
*/

using UnityEngine;

#if UNITY_EDITOR
using System.Collections.Generic;
using LanguageTools.Editor;
using LanguageTools;
using UnityEngine.UI;
using UnityEditor;
using System.Linq;
using System.IO;
using System;
#endif

[AddComponentMenu("Language/Editing Tools/Rebuild Canvas")]
public class RebuildCanvas : MonoBehaviour
{
#if UNITY_EDITOR
    [Header("Canvas Settings")]
    [SerializeField] private GameObject canvasGameObject; // Reference to the canvas GameObject.
    [Space(10)]
    public CanvasData canvasData = new(); // Holds the data for the canvas.
    public List<CanvasDataList> savedCanvasData = new(); // List of saved canvas data.

    [HideInInspector] public string saveFilePath; // Path for saving canvas data files.
    [HideInInspector] public string fileName; // Name of the current file being worked on.
    [HideInInspector] public int selectedCanvasIndex = 0; // Index of the selected canvas in the UI.
    [HideInInspector] public List<CanvasSave> canvasSave = new(); // List of canvas saves.

    // Method to create a canvas hierarchy from JSON data.
    public void CreateCanvasHierarchy(string json)
    {
        Undo.RecordObject(this, "Create Canvas Hierarchy"); // Record the action for undo.

        // Deserialize the JSON into canvas data objects.
        var canvasDataSave = JsonUtility.FromJson<CanvasDataSave>(json);
        if (canvasDataSave == null)
        {
            Debug.LogError("Failed to parse JSON into CanvasDataSave.");
            return; // Exit if parsing fails.
        }

        // Assign loaded canvas data.
        canvasData = canvasDataSave.canvasData;
        savedCanvasData = canvasDataSave.savedCanvasData;

        // If a canvas already exists, destroy it.
        if (canvasGameObject != null)
        {
            Undo.DestroyObjectImmediate(canvasGameObject);
        }

        // Create a new GameObject for the canvas.
        GameObject canvasObject = new(canvasData.canvasName);
        Undo.RegisterCreatedObjectUndo(canvasObject, "Create Canvas Object");
        canvasGameObject = canvasObject;

        // Add necessary components to the canvas object.
        var rectTransform = canvasObject.AddComponent<RectTransform>();
        var canvas = canvasObject.AddComponent<Canvas>();
        var canvasScaler = canvasObject.AddComponent<CanvasScaler>();
        canvasObject.AddComponent<GraphicRaycaster>();

        canvas.renderMode = RenderMode.ScreenSpaceOverlay; // Set the canvas rendering mode.

        // Configure the canvas scaler based on the loaded data.
        canvasScaler.uiScaleMode = canvasData.uiScaleMode;
        canvasScaler.referencePixelsPerUnit = canvasData.referencePixelsPerUnit;
        canvasScaler.scaleFactor = canvasData.scaleFactor;
        canvasScaler.referenceResolution = canvasData.referenceResolution;
        canvasScaler.screenMatchMode = canvasData.screenMatchMode;
        canvasScaler.matchWidthOrHeight = canvasData.matchWidthOrHeight;
        canvasScaler.physicalUnit = canvasData.physicalUnit;
        canvasScaler.fallbackScreenDPI = canvasData.fallbackScreenDPI;
        canvasScaler.defaultSpriteDPI = canvasData.defaultSpriteDPI;
        canvasScaler.dynamicPixelsPerUnit = canvasData.dynamicPixelsPerUnit;

        // Set the rect transform properties based on loaded data.
        rectTransform.localPosition = canvasData.localPosition;
        rectTransform.localRotation = canvasData.localRotation;
        rectTransform.localScale = canvasData.localScale;
        rectTransform.anchorMin = canvasData.anchorMin;
        rectTransform.anchorMax = canvasData.anchorMax;
        rectTransform.anchoredPosition = canvasData.anchoredPosition;
        rectTransform.sizeDelta = canvasData.sizeDelta;
        rectTransform.pivot = canvasData.pivot;

        // Create the GameObject hierarchy based on the canvas hierarchy data.
        CreateGameObjectFromHierarchy(canvasData.canvasHierarchy, canvasObject.transform);
    }

    // Method to create GameObjects based on the provided hierarchy.
    private void CreateGameObjectFromHierarchy(string[] hierarchy, Transform parent)
    {
        Dictionary<int, Color> colorMap = new(); // Map to store colors for each depth level.

        // Iterate through each path in the hierarchy.
        foreach (var path in hierarchy)
        {
            // Split the path into parts.
            string[] pathParts = path.Split(new[] { "'/'" }, StringSplitOptions.RemoveEmptyEntries);
            Transform currentParent = parent; // Start with the given parent transform.

            // Iterate through each part of the path.
            foreach (var part in pathParts)
            {
                // Continue if the part does not contain a wildcard.
                if (!part.Contains("*[")) continue; // Skip to the next part.

                // Extract the game object name and instance ID.
                string gameObjectName = part[..part.IndexOf("*[")]; // Get the name before the wildcard.
                string instanceIDString = LanguageEditorUtilities.ExtractTextBetweenAsterisks(part); // Extract the instance ID.

                // Try to parse the instance ID; log an error if it fails.
                if (!int.TryParse(instanceIDString, out int instanceID))
                {
                    Debug.LogError($"{gameObjectName} ID: {instanceIDString}");
                }

                Transform existingChild = currentParent.Find(gameObjectName); // Check if a child with the same name already exists.

                // If the child does not exist, create a new GameObject.
                if (existingChild == null)
                {
                    GameObject newGameObject = new(gameObjectName);
                    newGameObject.transform.SetParent(currentParent, false); // Set the new GameObject's parent.

                    // Add necessary components to the new GameObject.
                    var rectTransform = newGameObject.AddComponent<RectTransform>();
                    var image = newGameObject.AddComponent<Image>();
                    var outline = newGameObject.AddComponent<Outline>();

                    // Determine the depth of the current path to assign a color.
                    int depth = LanguageEditorUtilities.GetHierarchyDepth(pathParts);
                    if (!colorMap.ContainsKey(depth))
                    {
                        colorMap[depth] = new Color(UnityEngine.Random.value, UnityEngine.Random.value, UnityEngine.Random.value);
                    }
                    image.color = colorMap[depth]; // Set the color of the image.

                    outline.effectDistance = new(2, 2); // Set the outline effect distance.

                    // Find saved canvas data for the instance ID.
                    CanvasDataList canvasDataList = savedCanvasData.Find(data => data.instanceID == instanceID);
                    if (canvasDataList != null) // If found, assign properties from the saved data.
                    {
                        rectTransform.localPosition = canvasDataList.localPosition;
                        rectTransform.localRotation = canvasDataList.localRotation;
                        rectTransform.localScale = canvasDataList.localScale;
                        rectTransform.anchorMin = canvasDataList.anchorMin;
                        rectTransform.anchorMax = canvasDataList.anchorMax;
                        rectTransform.anchoredPosition = canvasDataList.anchoredPosition;
                        rectTransform.sizeDelta = canvasDataList.sizeDelta;
                        rectTransform.pivot = canvasDataList.pivot;

                        newGameObject.SetActive(canvasDataList.gameObjectEnable); // Set the active state of the GameObject.

                        // Assign the RectTransform reference for later use.
                        canvasDataList.rectTransform = rectTransform;
                    }

                    // Set the current parent to the newly created GameObject.
                    currentParent = newGameObject.transform;
                }
                else
                {
                    // If the child already exists, set the current parent to the existing child.
                    currentParent = existingChild;
                }
            }
        }
    }

    // Method to save the current canvas hierarchy to a JSON string.
    public string SaveCanvasHierarchy()
    {
        Undo.RecordObject(this, "Save Canvas Hierarchy"); // Record the action for undo.

        // Update the saved canvas data with the current RectTransform values.
        foreach (var canvasDataItem in savedCanvasData)
        {
            if (canvasDataItem.rectTransform != null)
            {
                // Copy values from the RectTransform to the canvas data.
                canvasDataItem.localPosition = canvasDataItem.rectTransform.localPosition;
                canvasDataItem.localRotation = canvasDataItem.rectTransform.localRotation;
                canvasDataItem.localScale = canvasDataItem.rectTransform.localScale;
                canvasDataItem.anchorMin = canvasDataItem.rectTransform.anchorMin;
                canvasDataItem.anchorMax = canvasDataItem.rectTransform.anchorMax;
                canvasDataItem.anchoredPosition = canvasDataItem.rectTransform.anchoredPosition;
                canvasDataItem.sizeDelta = canvasDataItem.rectTransform.sizeDelta;
                canvasDataItem.pivot = canvasDataItem.rectTransform.pivot;
            }
            else
            {
                Debug.LogWarning($"RectTransform is null for instanceID: {canvasDataItem.instanceID}");
            }
        }

        // Create a new CanvasDataSave object to hold the canvas data.
        CanvasDataSave canvasDataSave = new()
        {
            canvasData = canvasData,
            savedCanvasData = savedCanvasData
        };

        return JsonUtility.ToJson(canvasDataSave); // Serialize the canvas data to a JSON string.
    }
#else
    private void Start() Destroy(this); // If not in the editor, destroy this component.
#endif
}

#if UNITY_EDITOR
[CustomEditor(typeof(RebuildCanvas))]
public class RebuildCanvasEditor : Editor
{
    RebuildCanvas script; // Reference to the RebuildCanvas script.

    // This method is called to draw the custom inspector UI for the RebuildCanvas component.
    public override void OnInspectorGUI()
    {
        serializedObject.Update(); // Update the serialized object to reflect any changes made to the object.
        script = (RebuildCanvas)target; // Cast the target object to RebuildCanvas to access its properties.
        LanguageEditorUtilities.DrawReadOnlyMonoScriptField(target); // Draw a read-only field for the MonoBehaviour script type.

        // If the save file path is not set, initialize it with the language files folder path.
        if (string.IsNullOrEmpty(script.saveFilePath))
        {
            script.saveFilePath = LanguageFileManager.GetLanguageFilesFolderPath();
        }

        EditorGUILayout.Space(5);

        // Button to load a language file. Resets relevant properties before loading.
        if (GUILayout.Button("Load Language File", LanguageEditorUtilities.CreateCustomButtonStyle(15), GUILayout.Height(30)))
        {
            script.fileName = "";
            script.selectedCanvasIndex = 0;
            script.canvasSave.Clear();
            script.savedCanvasData.Clear();
            LoadLanguageFile(); // Load the language file.
        }

        EditorGUILayout.Space(5);

        EditorGUI.BeginDisabledGroup(script.canvasSave.Count == 0); // Disable button if no canvas data is available.

        // Prepare the dropdown options for canvas selection.
        string[] canvasOptions = script.canvasSave.Count > 0 ? script.canvasSave.Select(canvas => $"{script.fileName} | Canvas ID: {canvas.canvasID}").ToArray() : new[] { "No canvas loaded" };

        // Create a custom style for the popup.
        GUIStyle popupStyle = new(EditorStyles.popup)
        {
            fontSize = 12,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleLeft,
            normal = { textColor = Color.white },
            hover = { textColor = Color.red }
        };

        // Display the canvas options in a dropdown menu.
        script.selectedCanvasIndex = EditorGUILayout.Popup(script.selectedCanvasIndex, canvasOptions, popupStyle);

        EditorGUILayout.Space(5);

        // Button to create the canvas hierarchy from the selected canvas.
        if (GUILayout.Button("Create Canvas Hierarchy", LanguageEditorUtilities.CreateCustomButtonStyle(15), GUILayout.Height(30)))
        {
            // Ensure a valid canvas index is selected before creating the hierarchy.
            if (script.selectedCanvasIndex >= 0 && script.selectedCanvasIndex < script.canvasSave.Count)
            {
                script.CreateCanvasHierarchy(script.canvasSave[script.selectedCanvasIndex].json); // Create the canvas hierarchy from the JSON.
            }
            else
            {
                Debug.LogError("No canvas selected or invalid selection.");
            }
        }

        EditorGUILayout.Space(5);

        // Button to replace the current canvasID in the language file.
        if (GUILayout.Button("Replace canvasID", LanguageEditorUtilities.CreateCustomButtonStyle(15), GUILayout.Height(30)))
        {
            // Confirm the user's intent to replace the canvasID.
            if (!EditorUtility.DisplayDialog("Replace canvasID", "Are you sure you want to save the current state of the canvas?", "Yes", "No"))
            {
                return;// Exit if the user cancels.
            }

            // Open the editor window with the current canvas state saved.
            LanguageEditorUtilities.OpenEditorWindowWithCanvas(script.canvasSave[script.selectedCanvasIndex].canvasID, script.SaveCanvasHierarchy());
        }

        EditorGUI.EndDisabledGroup();  // End the disabled group for canvas buttons.

        EditorGUILayout.Space(10);

        // Display properties for serialized fields in the inspector.
        EditorGUILayout.PropertyField(serializedObject.FindProperty("canvasGameObject"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("canvasData"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("savedCanvasData"));

        serializedObject.ApplyModifiedProperties(); // Apply any changes made in the inspector.
    }

    // Loads a language file and parses its contents.
    private void LoadLanguageFile()
    {
        // Check if the specified folder path exists.
        if (!Directory.Exists(script.saveFilePath))
        {
            Debug.LogError($"Folder path does not exist: {script.saveFilePath}");
            return; // Exit if the folder does not exist.
        }

        // Open a file panel for the user to select a language file.
        string filePath = EditorUtility.OpenFilePanel("Select Language File", script.saveFilePath, "txt");

        // Validate the selected file.
        if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath) || Path.GetExtension(filePath) != ".txt")
        {
            Debug.LogError("Invalid file selected. Please select a valid .txt file.");
            return; // Exit if the file is invalid.
        }

        try
        {
            // Read all lines from the selected file.
            string[] lines = File.ReadAllLines(filePath);
            script.canvasSave.Clear(); // Clear any existing canvas saves.

            // Extract the file name without the extension.
            script.fileName = Path.GetFileNameWithoutExtension(filePath);

            // Parse each line for canvas data.
            foreach (string line in lines)
            {
                if (!line.StartsWith("canvasID:")) continue; // Skip lines that do not start with "canvasID:".
                var canvas = LanguageEditorUtilities.ParseCanvasSave(line); // Parse the canvas save data.
                if (canvas != null) script.canvasSave.Add(canvas); // Add the parsed canvas to the list.
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Error while loading file: {e.Message}");
        }
    }
}
#endif