/*
 * ---------------------------------------------------------------------------
 * Description: Manages language-specific UI updates for a Unity Canvas.
 *              Loads localized canvas configuration (position, scale, layout)
 *              from language settings and applies it at runtime. Also supports
 *              saving canvas layout during development via a custom editor.
 * Author: Lucas Gomes Cecchini
 * Pseudonym: AGAMENOM
 * ---------------------------------------------------------------------------
*/

using UnityEngine.UI;
using LanguageTools;
using UnityEngine;

using static LanguageTools.LanguageFileManager;
using static LanguageTools.CanvasManager;

#if UNITY_EDITOR
using static LanguageTools.Editor.LanguageEditorUtilities;
using UnityEditor;
#endif

[RequireComponent(typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler))]
[RequireComponent(typeof(GraphicRaycaster))]
[AddComponentMenu("Language/UI/Language Canvas")]
public class LanguageCanvas : MonoBehaviour
{
    [Header("Settings")]
    [IDExists(true)] public int canvasID = 0; // Identifier for linking this canvas with localized layout data.
    [Header("Canvas Data")]
    public CanvasStructure canvasStructure = new(); // Holds the current structure/layout data of this canvas.

    /// <summary>
    /// Subscribes to language updates and applies localized canvas data on enable.
    /// </summary>
    private void OnEnable()
    {
        LanguageManagerDelegate.OnLanguageUpdate += LanguageUpdate; // Register this canvas to respond to language updates.
        LanguageUpdate(); // Immediately apply localized canvas layout.
    }

    /// <summary>
    /// Unsubscribes from language update event on disable.
    /// </summary>
    private void OnDisable() => LanguageManagerDelegate.OnLanguageUpdate -= LanguageUpdate;

    /// <summary>
    /// Loads and applies localized canvas data based on current language.
    /// </summary>
    public void LanguageUpdate()
    {
        var languageData = LoadLanguageSettings(); // Retrieve the current language settings.
        LoadCanvasData(GetIDText(languageData.idCanvasData, canvasID)); // Fetch the canvas layout data using the ID and apply it.
    }

    /// <summary>
    /// Loads and parses canvas data from JSON and applies it to the GameObject.
    /// </summary>
    /// <param name="json">Serialized CanvasStructure JSON string.</param>
    private void LoadCanvasData(string json)
    {
        // Check if the provided data is valid.
        if (string.IsNullOrEmpty(json))
        {
            Debug.LogError("LanguageCanvas: JSON data is null or empty.", this);
            return;
        }

        // Attempt to parse JSON into a CanvasStructure object.
        var parsed = JsonUtility.FromJson<CanvasStructure>(json);
        if (parsed == null)
        {
            Debug.LogError("LanguageCanvas: Failed to parse JSON into CanvasStructure.", this);
            return;
        }

        // Store parsed data and apply it to the canvas.
        canvasStructure = parsed;
        ApplyCanvasData(canvasStructure, gameObject);
    }

#if UNITY_EDITOR
    /// <summary>
    /// Extracts canvas data from the GameObject and returns it as a JSON string.
    /// </summary>
    /// <returns>Serialized CanvasStructure JSON string.</returns>
    public string SaveCanvasData()
    {
        Undo.RecordObject(this, "Save Canvas Hierarchy"); // Record undo state for editor support.
        ExtractCanvasData(ref canvasStructure, gameObject); // Extract the current layout data into the canvasStructure object.
        return canvasStructure != null ? JsonUtility.ToJson(canvasStructure) : null; // Return the serialized JSON, or null if extraction failed.
    }
#endif
}

#if UNITY_EDITOR
[CanEditMultipleObjects]
[CustomEditor(typeof(LanguageCanvas))]
public class LanguageCanvasEditor : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        var script = (LanguageCanvas)target;

        // Display a helpful message for proper data capture.
        EditorGUILayout.HelpBox("For accurate capture, activate all objects within the Canvas before importing.", MessageType.Info);
        EditorGUILayout.Space(5);

        using (new EditorGUI.DisabledScope(targets.Length > 1))
        {
            // Draw the "Import Settings" button.
            if (GUILayout.Button("Import Settings", CreateCustomButtonStyle(15), GUILayout.Height(30)))
            {
                // Check for duplicate ID and prompt user confirmation.
                if (IsIDInCanvasList(script.canvasID) && !EditorUtility.DisplayDialog("Replace ID", "An ID with this number is already saved. Do you want to replace it?", "Yes", "No"))
                    return;

                // Save current canvas layout and open the editor window for editing.
                var data = script.SaveCanvasData();
                if (data != null) OpenEditorWindowWithCanvas(script.canvasID, data);
            }
        }

        EditorGUILayout.Space(5);

        // Draw default inspector fields.
        DrawDefaultInspector();
        serializedObject.ApplyModifiedProperties();
    }
}
#endif