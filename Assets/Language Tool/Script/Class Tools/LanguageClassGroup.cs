/*
 * ---------------------------------------------------------------------------
 * Description: This script defines data structures for managing language 
 *              settings and canvas state in the application. It includes classes for 
 *              saving language options, script texts, and canvas configurations.
 * Author: Lucas Gomes Cecchini
 * Pseudonym: AGAMENOM
 * ---------------------------------------------------------------------------
*/

using System.Collections.Generic;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine;
using System;

[System.Serializable]
public class LanguageSaveData
{
    public string selectedFile; // Stores the filename of the selected language file.
    public string selectedLanguage; // Stores the name of the selected language.
}

[System.Serializable]
public class LanguageOptions
{
    public string text; // Text representation of the language option.
    public Sprite sprite; // Associated image (sprite) for the language option.
    public int iD; // Unique identifier for the language option.
}

[System.Serializable]
public class ScriptText
{
    public string text; // Text content for the script.
    public int iD; // Unique identifier for the script.
    [Space(10)]
    public UnityEvent<string> targetScripts; // Unity event triggered by the script, passing a string parameter.
}

[System.Serializable]
public class LanguageLines
{
    public string text; // Text content for a language line.
    public bool translateText; // Determines whether the text should be translated.
    public int iD; // Unique identifier for the language line.
}

[System.Serializable]
public class LanguageFileData
{
    public string fileName; // The name of the language file.
    public string computerLanguage; // The name of the programming or system language used.
    public bool firstTime; // Boolean flag indicating if it's the first time this language file is being used.
    public int idIndex; // Index to track the current ID used for components in the language file.
    public List<LanguageComponentSave> componentSave; // List of components saved in the language file.
    public List<CanvasSave> canvasSave; // List of canvas elements saved in the language file.
}

[System.Serializable]
public class LanguageComponentSave
{
    [Header("Custom ID")]
    public int iD; // Unique identifier for the language component.
    [Space(5)]
    [Header("Text Settings")]
    [Multiline] public string text; // The text content of the language component (supports multiline).
    public int alignment; // The alignment setting of the text in the language component.
    public int fontSize; // The font size of the text.
    public int fontListIndex; // Index pointing to the font used in the component.
    [HideInInspector] public int componentType; // The type of component (e.g., text, dropdown, etc.).
    [HideInInspector] public bool textWrite; // Determines if the text should be written to the language file.
    [HideInInspector] public bool alignmentWrite; // Determines if the alignment should be written to the language file.
    [HideInInspector] public bool fontSizeWrite; // Determines if the font size should be written to the language file.
    [HideInInspector] public bool fontListIndexWrite; // Determines if the font index should be written to the language file.
}

[Serializable]
public class ExcelData
{
    public string language; // The language this data corresponds to.
    public List<KeyValuePair<int, string>> lines; // List of key-value pairs, where the key is the unique ID and the value is the text in the language.
}

[Serializable]
public class ExcelTableData
{
    public List<string> filesData; // A list containing the names of files in the Excel table.
}

[System.Serializable]
public class CanvasSave
{
    public int canvasID; // Unique identifier for the canvas.
    public string json; // JSON string representing the saved state of the canvas.
}

[System.Serializable]
public class CanvasDataSave
{
    public CanvasData canvasData; // Stores information about the canvas itself.
    public List<CanvasDataList> savedCanvasData; // List of data corresponding to each canvas in the hierarchy.
}

[System.Serializable]
public class CanvasData
{
    [Header("Canvas")]
    public string canvasName; // The name of the canvas.
    public string[] canvasHierarchy; // Array representing the hierarchy of objects within the canvas.
    [Space(10)]
    public Quaternion localRotation; // Rotation of the canvas in local space.
    public Vector3 localPosition; // Position of the canvas in local space.
    public Vector3 localScale; // Scale of the canvas in local space.
    [Space(5)]
    public Vector2 anchorMin; // Minimum anchor point for the RectTransform of the canvas.
    public Vector2 anchorMax; // Maximum anchor point for the RectTransform of the canvas.
    public Vector2 anchoredPosition; // Position of the canvas in its anchored space.
    public Vector2 sizeDelta; // The size of the RectTransform.
    public Vector2 pivot; // Pivot point of the RectTransform.

    [Header("Canvas Scaler")]
    public CanvasScaler.ScaleMode uiScaleMode; // UI scaling mode for the canvas.
    public float referencePixelsPerUnit; // Reference pixels per unit for the canvas scaler.
    public float scaleFactor; // Scaling factor for the canvas.
    public Vector2 referenceResolution; // Resolution reference for scaling the canvas.
    public CanvasScaler.ScreenMatchMode screenMatchMode; // Screen match mode to determine how to scale the UI based on screen size.
    public float matchWidthOrHeight; // Determines whether to match width or height during scaling.
    public CanvasScaler.Unit physicalUnit; // Physical unit setting for the canvas scaler.
    public float fallbackScreenDPI; // Fallback DPI for the screen.
    public float defaultSpriteDPI; // Default DPI for sprites.
    public float dynamicPixelsPerUnit; // Dynamic pixels per unit setting for canvas scaling.
    public bool presetInfoIsWorld; // Indicates whether the canvas preset is using world space or screen space.
}

[System.Serializable]
public class CanvasDataList
{
    [Header("Game Object Settings")]
    public string gameObjectName; // The name of the GameObject associated with the canvas element.
    public bool gameObjectEnable; // Flag indicating if the GameObject is enabled or disabled.
    [Space(10)]
    public RectTransform rectTransform; // The RectTransform associated with the GameObject.
    public int instanceID; // Instance ID of the GameObject.
    [Space(5)]
    public Quaternion localRotation; // Local rotation of the GameObject.
    public Vector3 localPosition; // Local position of the GameObject.
    public Vector3 localScale; // Local scale of the GameObject.
    [Space(5)]
    public Vector2 anchorMin; // Minimum anchor point for the RectTransform of the GameObject.
    public Vector2 anchorMax; // Maximum anchor point for the RectTransform of the GameObject.
    public Vector2 anchoredPosition; // Position of the GameObject in its anchored space.
    public Vector2 sizeDelta; // The size of the RectTransform.
    public Vector2 pivot; // Pivot point of the RectTransform.
}