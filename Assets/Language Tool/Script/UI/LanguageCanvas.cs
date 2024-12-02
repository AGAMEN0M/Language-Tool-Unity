/*
 * ---------------------------------------------------------------------------
 * Description: This script manages language settings for a Unity canvas. It
 *              updates the canvas and its child elements' data (position, scale, etc.) based 
 *              on the current language and configuration, ensuring the UI adapts to localized 
 *              settings. The script also handles saving and loading canvas data.
 * Author: Lucas Gomes Cecchini
 * Pseudonym: AGAMENOM
 * ---------------------------------------------------------------------------
*/

using System.Collections.Generic;
using LanguageTools;
using UnityEngine.UI;
using UnityEngine;

#if UNITY_EDITOR
using LanguageTools.Editor;
using UnityEditor;
#endif

// This class is responsible for managing the language aspects of a canvas within Unity.
[RequireComponent(typeof(RectTransform))]
[RequireComponent(typeof(Canvas))]
[RequireComponent(typeof(CanvasScaler))]
[RequireComponent(typeof(GraphicRaycaster))]
[AddComponentMenu("Language/UI/Language Canvas")]
public class LanguageCanvas : MonoBehaviour
{
    [Header("Settings")]
    public int canvasID = 0; // The unique identifier for this canvas.
    [Space(10)]
    [Header("Automatic Information")]
    [SerializeField] private string selectedFile; // Holds the currently selected file for language settings.
    [Space(5)]
    [SerializeField] private string saveFile; // Holds the file path for saving the canvas state.
    [Space(10)]
    [Header("Canvas Data")]
    public CanvasData canvasData = new(); // Stores data related to the canvas.
    public List<CanvasDataList> savedCanvasData = new(); // A list containing saved data for child canvas objects.

    // Called when the object is enabled, subscribing to the language update event.
    private void OnEnable()
    {
        LanguageManagerDelegate.OnLanguageUpdate += LanguageUpdate;
        LanguageUpdate();
    }

    // Called when the object is disabled, unsubscribing from the language update event.
    private void OnDisable()
    {
        LanguageManagerDelegate.OnLanguageUpdate -= LanguageUpdate;
    }

    // Updates the canvas language data when the language is changed.
    public void LanguageUpdate()
    {
        // Loads the file path where the canvas save data is stored.
        saveFile = LanguageFileManager.GetSaveFilePath();
        // Loads all canvas-related data from the saved JSON file.
        LoadAllCanvasDataList(LanguageFileManager.GetLocalizedJsonByID(canvasID, saveFile, ref selectedFile));
    }

    // Loads the saved canvas data list from the specified JSON string.
    private void LoadAllCanvasDataList(string json)
    {
        if (json == "CanvasID not found") return;

        var canvasDataSave = JsonUtility.FromJson<CanvasDataSave>(json); // Deserialize the JSON data to CanvasDataSave object.

        if (canvasDataSave == null)
        {
            Debug.LogError("Failed to parse CanvasDataSave.");
            return;
        }

        canvasData = canvasDataSave.canvasData; // Loads the canvas data into the local variables.

        // Ensures that the number of saved canvas items matches the expected count.
        if (canvasDataSave.savedCanvasData.Count != savedCanvasData.Count)
        {
            Debug.LogWarning($"Warning: Expected {savedCanvasData.Count} savedCanvasData items but found {canvasDataSave.savedCanvasData.Count} in the loaded data.");
        }

        // Updates the saved canvas data with the new values.
        for (int i = 0; i < savedCanvasData.Count; i++)
        {
            var existingCanvasData = savedCanvasData[i];
            var sourceCanvasData = canvasDataSave.savedCanvasData.Find(s => s.instanceID == existingCanvasData.instanceID);

            if (sourceCanvasData != null)
            {
                existingCanvasData.localPosition = sourceCanvasData.localPosition;
                existingCanvasData.localRotation = sourceCanvasData.localRotation;
                existingCanvasData.localScale = sourceCanvasData.localScale;
                existingCanvasData.anchorMin = sourceCanvasData.anchorMin;
                existingCanvasData.anchorMax = sourceCanvasData.anchorMax;
                existingCanvasData.anchoredPosition = sourceCanvasData.anchoredPosition;
                existingCanvasData.sizeDelta = sourceCanvasData.sizeDelta;
                existingCanvasData.pivot = sourceCanvasData.pivot;
            }
        }

        // Applies the loaded values to the canvas's RectTransform components.
        foreach (var canvasData in savedCanvasData)
        {
            RectTransform rectTransform = canvasData.rectTransform;

            if (rectTransform != null)
            {
                rectTransform.localPosition = canvasData.localPosition;
                rectTransform.localRotation = canvasData.localRotation;
                rectTransform.localScale = canvasData.localScale;
                rectTransform.anchorMin = canvasData.anchorMin;
                rectTransform.anchorMax = canvasData.anchorMax;
                rectTransform.anchoredPosition = canvasData.anchoredPosition;
                rectTransform.sizeDelta = canvasData.sizeDelta;
                rectTransform.pivot = canvasData.pivot;
            }
            else
            {
                Debug.LogError("RectTransform component is missing.");
            }
        }
    }

#if UNITY_EDITOR
    // Saves the current hierarchy of the canvas and its child objects.
    public string SaveCanvasHierarchy()
    {
        Undo.RecordObject(this, "Save Canvas Hierarchy");
        savedCanvasData.Clear();
        var allTransforms = GetComponentsInChildren<RectTransform>(true);

        canvasData.canvasHierarchy = new string[0];
        List<string> hierarchyList = new();

        // Loop through each child object of the canvas and record its properties.
        foreach (var rectTransform in allTransforms)
        {
            GameObject obj = rectTransform.gameObject;
            hierarchyList.Add(LanguageEditorUtilities.GetGameObjectPath(rectTransform));

            if (obj.GetComponent<Canvas>() && obj.TryGetComponent(out CanvasScaler canvasScaler) && obj.GetComponent<GraphicRaycaster>())
            {
                // Store the main canvas information.
                canvasData.canvasName = obj.name;
                canvasData.localPosition = rectTransform.localPosition;
                canvasData.localRotation = rectTransform.localRotation;
                canvasData.localScale = rectTransform.localScale;
                canvasData.anchorMin = rectTransform.anchorMin;
                canvasData.anchorMax = rectTransform.anchorMax;
                canvasData.anchoredPosition = rectTransform.anchoredPosition;
                canvasData.sizeDelta = rectTransform.sizeDelta;
                canvasData.pivot = rectTransform.pivot;

                // Store the canvas scaler settings.
                canvasData.uiScaleMode = canvasScaler.uiScaleMode;
                canvasData.referencePixelsPerUnit = canvasScaler.referencePixelsPerUnit;
                canvasData.scaleFactor = canvasScaler.scaleFactor;
                canvasData.referenceResolution = canvasScaler.referenceResolution;
                canvasData.screenMatchMode = canvasScaler.screenMatchMode;
                canvasData.matchWidthOrHeight = canvasScaler.matchWidthOrHeight;
                canvasData.physicalUnit = canvasScaler.physicalUnit;
                canvasData.fallbackScreenDPI = canvasScaler.fallbackScreenDPI;
                canvasData.defaultSpriteDPI = canvasScaler.defaultSpriteDPI;
                canvasData.dynamicPixelsPerUnit = canvasScaler.dynamicPixelsPerUnit;
            }
            else
            {
                // Store the properties of child canvas objects.
                CanvasDataList canvasData = new()
                {
                    gameObjectName = obj.name,
                    gameObjectEnable = obj.activeSelf,
                    rectTransform = rectTransform,
                    instanceID = rectTransform.GetInstanceID(),
                    localPosition = rectTransform.localPosition,
                    localRotation = rectTransform.localRotation,
                    localScale = rectTransform.localScale,
                    anchorMin = rectTransform.anchorMin,
                    anchorMax = rectTransform.anchorMax,
                    anchoredPosition = rectTransform.anchoredPosition,
                    sizeDelta = rectTransform.sizeDelta,
                    pivot = rectTransform.pivot
                };

                savedCanvasData.Add(canvasData);
            }
        }

        canvasData.canvasHierarchy = hierarchyList.ToArray();

        // Create an object that contains all the saved canvas data and return it as a JSON string.
        CanvasDataSave canvasDataSave = new()
        {
            canvasData = canvasData,
            savedCanvasData = savedCanvasData
        };

        return JsonUtility.ToJson(canvasDataSave);
    }
#endif
}

#if UNITY_EDITOR
// Custom editor to enhance the Unity editor's Inspector view for the LanguageCanvas component.
[CustomEditor(typeof(LanguageCanvas))]
public class LanguageCanvasEditor : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        var script = (LanguageCanvas)target;
        LanguageEditorUtilities.DrawReadOnlyMonoScriptField(target);

        EditorGUILayout.Space(5);

        // Button to import settings for the canvas.
        if (GUILayout.Button("Import Settings", LanguageEditorUtilities.CreateCustomButtonStyle(15), GUILayout.Height(30)))
        {
            if (LanguageEditorUtilities.IsIDInCanvasList(script.canvasID))
            {
                if (!EditorUtility.DisplayDialog("Replace ID", "An ID with this number is already saved. Do you want to replace it?", "Yes", "No"))
                {
                    return;
                }
            }

            LanguageEditorUtilities.OpenEditorWindowWithCanvas(script.canvasID, script.SaveCanvasHierarchy());
        }

        EditorGUILayout.Space(5);
        EditorGUILayout.HelpBox("This script uses 'GetInstanceID()' to build an appropriate database. However, 'GetInstanceID()' is unique to this session. \nMake sure to use it only after 'Canvas' is 100% configured. Otherwise, you will have to rewrite all the language files every session.", MessageType.Warning);
        EditorGUILayout.Space(5);

        if (LanguageEditorUtilities.IsIDInCanvasList(script.canvasID))
        {
            GUI.color = Color.yellow;
            EditorGUILayout.PropertyField(serializedObject.FindProperty("canvasID"));
            EditorGUILayout.HelpBox("There is an canvasID with this number Saved!", MessageType.Warning);
            GUI.color = Color.white;
        }
        else
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("canvasID"));
        }

        // Display other relevant properties in the inspector.
        EditorGUILayout.PropertyField(serializedObject.FindProperty("selectedFile"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("saveFile"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("canvasData"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("savedCanvasData"));

        serializedObject.ApplyModifiedProperties();
    }
}
#endif