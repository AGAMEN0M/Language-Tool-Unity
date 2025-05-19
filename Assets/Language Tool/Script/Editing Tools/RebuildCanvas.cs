/*
 * ---------------------------------------------------------------------------
 * Description: Provides Unity Editor tools to build and manage canvas hierarchies 
 *              based on language-specific JSON data. Enables loading canvas configurations, 
 *              rebuilding hierarchies in the scene, and saving updated layouts.
 * Author: Lucas Gomes Cecchini
 * Pseudonym: AGAMENOM
 * ---------------------------------------------------------------------------
*/

using UnityEngine;

#if UNITY_EDITOR
using static LanguageTools.Editor.LanguageEditorUtilities;
using static LanguageTools.CanvasManager;

using System.Collections.Generic;
using LanguageTools;
using UnityEditor;
using System.Linq;
using System.IO;
#endif

[AddComponentMenu("Language/Editing Tools/Rebuild Canvas")]
public class RebuildCanvas : MonoBehaviour
{
#if UNITY_EDITOR
    [Header("Canvas Settings")]
    public GameObject canvasObject; // The GameObject that will hold the generated canvas hierarchy in the scene.
    [Space(10)]
    public CanvasStructure canvasStructure = new(); // Data structure representing the canvas hierarchy, parsed from or serialized to JSON.

    [HideInInspector] public int canvasID = 0; // Identifier used to track or reference the currently selected canvas data entry.

    /// <summary>
    /// Parses a JSON string and generates the corresponding canvas hierarchy in the scene.
    /// </summary>
    /// <param name="json">The JSON string representing canvas structure data.</param>
    public void CreateCanvasData(string json)
    {
        // Register undo operation for the current object state.
        Undo.RecordObject(this, "Create Canvas Hierarchy");

        // Validate if JSON string is null or empty.
        if (string.IsNullOrEmpty(json))
        {
            Debug.LogError("LanguageCanvas: JSON data is null or empty.", this);
            return;
        }

        // Attempt to parse JSON string into a CanvasStructure object.
        var canvasDataJson = JsonUtility.FromJson<CanvasStructure>(json);
        if (canvasDataJson == null)
        {
            Debug.LogError("LanguageCanvas: Failed to parse JSON into CanvasStructure.", this);
            return;
        }

        // Store the parsed canvas structure.
        canvasStructure = canvasDataJson;

        // If a canvas object already exists, destroy it and register the undo operation.
        if (canvasObject != null) Undo.DestroyObjectImmediate(canvasObject);

        // Create a new canvas object from the parsed structure and register it for undo.
        CreateCanvasFromStructure(canvasStructure, out canvasObject);
        Undo.RegisterCreatedObjectUndo(canvasObject, "Create Canvas Object");
    }

    /// <summary>
    /// Extracts the current canvas structure from the scene and returns it as a JSON string.
    /// </summary>
    /// <returns>JSON representation of the current canvas hierarchy.</returns>
    public string SaveCanvasData()
    {
        // Register undo operation for the current object state.
        Undo.RecordObject(this, "Save Canvas Hierarchy");

        // Extract the structure data from the current canvas object.
        ExtractCanvasData(ref canvasStructure, canvasObject);

        // Convert the structure to JSON if extraction was successful.
        return canvasStructure != null ? JsonUtility.ToJson(canvasStructure) : null;
    }
#else
    private void Start() => Destroy(this);
#endif
}

#if UNITY_EDITOR
[CanEditMultipleObjects]
[CustomEditor(typeof(RebuildCanvas))]
public class RebuildCanvasEditor : Editor
{
    private const string fileData = "ProjectSettings/LanguageFileData.json"; // Path to the JSON file containing canvas configuration data.
    private List<CanvasForEditingSave> canvasSave = new(); // List storing all canvas structures loaded from the external JSON file, used to populate the selection dropdown.
    private RebuildCanvas script; // Reference to the currently edited RebuildCanvas component in the Inspector.

    /// <summary>
    /// Draws the custom inspector GUI for the RebuildCanvas component.
    /// </summary>
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        script = (RebuildCanvas)target;

        // Load canvas data only if a single target is selected.
        if (targets.Length == 1) LoadDataJson();

        // Display instruction message to the user.
        GUI.color = Color.yellow;
        EditorGUILayout.HelpBox("You need to load the data into the 'Language File Manager' window first.", MessageType.Warning);
        GUI.color = Color.white;

        EditorGUILayout.Space(5);
        int selectedIndex = 0;

        // Show canvas selection dropdown if only one object is selected.
        if (targets.Length == 1)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Select Canvas ID", EditorStyles.boldLabel, GUILayout.Width(100));

            string[] options = canvasSave.Select(c => $"ID {c.canvasID}: {c.textContext}").ToArray();
            selectedIndex = Mathf.Max(0, canvasSave.FindIndex(c => c.canvasID == script.canvasID));
            selectedIndex = EditorGUILayout.Popup(selectedIndex, options);
            EditorGUILayout.EndHorizontal();
        }
        else
        {
            // Notify user that canvas selection is unavailable for multi-editing.
            EditorGUILayout.HelpBox("Select Canvas ID is only available when a single object is selected.", MessageType.Info);
        }

        EditorGUILayout.Space(5);

        if (selectedIndex >= 0 && selectedIndex < canvasSave.Count)
        {
            // Assign selected canvas ID to the script.
            script.canvasID = canvasSave[selectedIndex].canvasID;

            // Button to create canvas hierarchy from selected data.
            if (GUILayout.Button("Create Canvas Hierarchy", CreateCustomButtonStyle(15), GUILayout.Height(30)))
            {
                script.CreateCanvasData(canvasSave[selectedIndex].json);
            }
        }

        EditorGUILayout.Space(5);
        EditorGUILayout.HelpBox("For more accurate capture, we recommend that you activate all objects within the Canvas.", MessageType.Info);
        EditorGUILayout.Space(5);

        using (new EditorGUI.DisabledScope(targets.Length > 1))
        {
            // Disable button if no canvas object is set.
            GUI.enabled = script.canvasObject != null;

            // Button to save the current canvas state.
            if (GUILayout.Button($"Replace canvasID: {script.canvasID}", CreateCustomButtonStyle(15), GUILayout.Height(30)))
            {
                // Confirm before overwriting existing data.
                if (!EditorUtility.DisplayDialog("Replace canvasID", "Are you sure you want to save the current state of the canvas?", "Yes", "No"))
                    return;

                var data = script.SaveCanvasData();
                if (data != null) OpenEditorWindowWithCanvas(script.canvasID, data);
            }

            GUI.enabled = true;
        }

        EditorGUILayout.Space(10);

        // Draw default inspector fields below custom UI.
        DrawDefaultInspector();
        serializedObject.ApplyModifiedProperties();
    }

    /// <summary>
    /// Loads canvas configuration data from a JSON file stored in ProjectSettings.
    /// </summary>
    private void LoadDataJson()
    {
        // Exit early if the file doesn't exist.
        if (!File.Exists(fileData)) return;

        // Deserialize the data into a structured format.
        var data = JsonUtility.FromJson<LanguageFileManagerWindowData>(File.ReadAllText(fileData));

        // Clear the list and populate it with the loaded data.
        canvasSave.Clear();
        canvasSave = data.canvasSave;
    }
}
#endif