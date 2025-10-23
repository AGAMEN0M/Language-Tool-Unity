/*
 * ---------------------------------------------------------------------------
 * Description: Defines data structures used for handling language localization
 *              and UI canvas configurations within the application. It includes
 *              serializable classes for saving language IDs, canvas states,
 *              user-defined translations, and Unity UI settings.
 *              
 * Author: Lucas Gomes Cecchini
 * Pseudonym: AGAMENOM
 * ---------------------------------------------------------------------------
*/

using System.Collections.Generic;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine;
using System;

namespace LanguageTools
{
    #region === Language Structures ===

    /// <summary>
    /// Represents a language option available for selection.
    /// </summary>
    [Serializable]
    public class LanguageAvailable
    {
        [Tooltip("Culture code (e.g., \"en-US\").")]
        public string culture;

        [Tooltip("Display native name of the language.")]
        public string nativeName;

        [Tooltip("Display name of the language.")]
        public string name;

        [Tooltip("Whether the language is currently available for use.")]
        public bool isAvailable;

        [Tooltip("Column index in the localization table.")]
        public int columnIndex;
    }

    /// <summary>
    /// Stores an ID and its associated localized text.
    /// </summary>
    [Serializable]
    public class IdData
    {
        [Tooltip("Unique identifier for the text entry.")]
        public int iD;

        [Tooltip("Localized text string.")]
        public string text;
    }

    /// <summary>
    /// Holds metadata settings related to a specific ID.
    /// </summary>
    [Serializable]
    public class IdMetaData
    {
        [Header("Custom ID")]
        [Tooltip("Unique identifier for the text metadata.")]
        public int iD;

        [Space(5)]

        [Header("Text Settings")]
        [Tooltip("Text alignment setting.")]
        public int alignment;

        [Tooltip("Font size setting.")]
        public int fontSize;

        [Tooltip("Index for selecting font from a predefined list.")]
        public int fontListIndex;

        [Tooltip("Type of component using this metadata (e.g., Text, TMP, etc.).")]
        public int componentType;
    }

    #endregion

    #region === Canvas & UI Structures ===

    /// <summary>
    /// Represents the serialized state of a UI GameObject in a canvas.
    /// </summary>
    [Serializable]
    public class CanvasDataList
    {
        [Header("Game Object Settings")]
        [Tooltip("Name of the UI GameObject.")]
        public string gameObjectName;

        [Tooltip("Whether the GameObject is enabled.")]
        public bool gameObjectEnable;

        [Space(10)]

        [Tooltip("Reference to the RectTransform component.")]
        public RectTransform rectTransform;

        [Tooltip("Unique instance ID of the GameObject.")]
        public int instanceID;

        [Space(5)]

        [Tooltip("Local rotation of the GameObject.")]
        public Quaternion localRotation;

        [Tooltip("Local position of the GameObject.")]
        public Vector3 localPosition;

        [Tooltip("Local scale of the GameObject.")]
        public Vector3 localScale;

        [Space(5)]

        [Tooltip("Minimum anchor point.")]
        public Vector2 anchorMin;

        [Tooltip("Maximum anchor point.")]
        public Vector2 anchorMax;

        [Tooltip("Anchored position within the parent.")]
        public Vector2 anchoredPosition;

        [Tooltip("Size delta of the RectTransform.")]
        public Vector2 sizeDelta;

        [Tooltip("Pivot point of the RectTransform.")]
        public Vector2 pivot;
    }

    /// <summary>
    /// Container for saving the entire canvas structure and configuration.
    /// </summary>
    [Serializable]
    public class CanvasStructure
    {
        [Header("Canvas Structure")]
        [Tooltip("Name of the canvas.")]
        public string canvasName;

        [Tooltip("Array of canvas layers.")]
        public CanvasLayers[] canvasLayers;

        [Space(10)]

        [Tooltip("Layout data of the canvas RectTransform.")]
        public RectTransformData rectTransform;

        [Space(10)]

        [Tooltip("Serialized data for the Canvas component.")]
        public CanvasData canvas;

        [Space(10)]

        [Tooltip("Serialized data for the CanvasScaler component.")]
        public CanvasScalerData canvasScaler;

        [Space(10)]

        [Tooltip("Serialized data for the GraphicRaycaster component.")]
        public GraphicRaycasterData graphicRaycaster;
    }

    /// <summary>
    /// Represents a group of objects and layout data within a canvas layer.
    /// </summary>
    [Serializable]
    public class CanvasLayers
    {
        [Tooltip("Names of objects in the canvas layer.")]
        public string[] CanvasObjectsLayers;

        [Tooltip("Layout data for each object.")]
        public RectTransformData[] rectTransforms;
    }

    /// <summary>
    /// Serialized structure representing a RectTransform's layout properties.
    /// </summary>
    [Serializable]
    public class RectTransformData
    {
        [Header("RectTransform")]
        [Tooltip("Local rotation of the RectTransform.")]
        public Quaternion localRotation;

        [Tooltip("Local position of the RectTransform.")]
        public Vector3 localPosition;

        [Tooltip("Local scale of the RectTransform.")]
        public Vector3 localScale;

        [Space(5)]

        [Tooltip("Minimum anchor point.")]
        public Vector2 anchorMin;

        [Tooltip("Maximum anchor point.")]
        public Vector2 anchorMax;

        [Tooltip("Anchored position within the parent.")]
        public Vector2 anchoredPosition;

        [Tooltip("Size delta of the RectTransform.")]
        public Vector2 sizeDelta;

        [Tooltip("Pivot point of the RectTransform.")]
        public Vector2 pivot;
    }

    #endregion

    #region === Canvas Component Data ===

    /// <summary>
    /// Contains properties related to the Canvas component.
    /// </summary>
    [Serializable]
    public class CanvasData
    {
        [Header("Canvas")]
        [Tooltip("Mode in which the canvas is rendered.")]
        public RenderMode renderMode;

        [Tooltip("Distance of the canvas plane from the camera.")]
        public float planeDistance;

        [Tooltip("Enables pixel-perfect rendering.")]
        public bool pixelPerfect;

        [Tooltip("Whether to override sorting behavior.")]
        public bool overrideSorting;

        [Tooltip("Overrides global pixel-perfect settings.")]
        public bool overridePixelPerfect;

        [Tooltip("Normalized size for sorting bucket.")]
        public float sortingBucketNormalizedSize;

        [Tooltip("Forces vertex color to be in gamma space.")]
        public bool vertexColorAlwaysGammaSpace;

        [Tooltip("Additional shader channels.")]
        public AdditionalCanvasShaderChannels additionalShaderChannels;

        [Tooltip("Controls resizing on standalone platforms.")]
        public StandaloneRenderResize updateRectTransformForStandalone;
    }

    /// <summary>
    /// Contains properties related to the CanvasScaler component.
    /// </summary>
    [Serializable]
    public class CanvasScalerData
    {
        [Header("Canvas Scaler")]
        [Tooltip("UI scaling mode.")]
        public CanvasScaler.ScaleMode uiScaleMode;

        [Tooltip("Reference pixels per unit.")]
        public float referencePixelsPerUnit;

        [Tooltip("Scale factor for the canvas.")]
        public float scaleFactor;

        [Tooltip("Reference resolution for UI scaling.")]
        public Vector2 referenceResolution;

        [Tooltip("Mode to match screen size.")]
        public CanvasScaler.ScreenMatchMode screenMatchMode;

        [Tooltip("Weight of width vs. height in scaling.")]
        public float matchWidthOrHeight;

        [Tooltip("Unit used for physical sizes.")]
        public CanvasScaler.Unit physicalUnit;

        [Tooltip("Fallback DPI when screen DPI is unavailable.")]
        public float fallbackScreenDPI;

        [Tooltip("Default DPI for sprites.")]
        public float defaultSpriteDPI;

        [Tooltip("Dynamic pixels per unit setting.")]
        public float dynamicPixelsPerUnit;

        [Tooltip("Indicates if the preset info is for world space.")]
        public bool presetInfoIsWorld;
    }

    /// <summary>
    /// Contains properties related to the GraphicRaycaster component.
    /// </summary>
    [Serializable]
    public class GraphicRaycasterData
    {
        [Header("Graphic Raycaster")]
        [Tooltip("Whether to ignore graphics facing away.")]
        public bool ignoreReversedGraphics;

        [Tooltip("Types of objects that can block raycasts.")]
        public GraphicRaycaster.BlockingObjects blockingObjects;

        [Tooltip("Layer mask used for raycast blocking.")]
        public LayerMask blockingMask;
    }

    #endregion

    #region === Language Components & Scripts ===

    /// <summary>
    /// Represents a selectable language option including its visual and ID.
    /// </summary>
    [Serializable]
    public class LanguageOptions
    {
        [Tooltip("Display text of the language option.")]
        public string text;

        [Tooltip("Icon representing the language.")]
        public Sprite sprite;

        [IDExists, Tooltip("ID associated with the language.")]
        public int iD;
    }

    /// <summary>
    /// Holds script text content and an associated UnityEvent.
    /// </summary>
    [Serializable]
    public class ScriptText
    {
        [IDExists, Tooltip("Unique identifier for the text.")] 
        public int iD;

        [TextArea, Tooltip("Script content text.")]
        public string text;

        [Space(5)]

        [Tooltip("Event triggered with the text.")]
        public UnityEvent<string> targetScripts;
    }

    /// <summary>
    /// Represents a localized line of text that can be optionally translated.
    /// </summary>
    [Serializable]
    public class LanguageLines
    {
        [IDExists, Tooltip("Unique identifier for the line.")]
        public int iD;

        [Tooltip("Line text.")]
        public string text;

        [Tooltip("Whether this line should be translated.")]
        public bool translateText;
    }

    #endregion

    #region === Language File Manager Data ===

    /// <summary>
    /// Represents language component data for editing and saving.
    /// </summary>
    [Serializable]
    public class LanguageForEditingSave
    {
        [Header("Custom ID")]
        [Tooltip("Unique ID for the component text.")]
        public int iD;

        [Header("Text Settings")]
        [Multiline, Tooltip("Editable text content.")]
        public string text;

        [Tooltip("Text alignment index.")]
        public int alignment;

        [Tooltip("Font size.")]
        public int fontSize;

        [Tooltip("Index of font from list.")]
        public int fontListIndex;

        [Header("Other Systems")]
        [Tooltip("Context of where the text is used.")]
        public string textContext;

        [Tooltip("Type of component this text is bound to.")]
        public int componentType;
    }

    /// <summary>
    /// Represents a canvas element's saved data including context and state.
    /// </summary>
    [Serializable]
    public class CanvasForEditingSave
    {
        [Header("Custom ID")]
        [Tooltip("ID of the canvas item.")]
        public int canvasID;

        [Header("Canvas Settings")]
        [Tooltip("Contextual label for canvas usage.")]
        public string textContext;

        [Tooltip("Serialized canvas state in JSON.")]
        public string json;
    }

    /// <summary>
    /// Root object for language file data used in the Language File Manager window.
    /// </summary>
    [Serializable]
    public class LanguageFileManagerWindowData
    {
        [Tooltip("Language currently being edited.")]
        public string languageForEditing;

        [Tooltip("Saved language components.")]
        public List<LanguageForEditingSave> componentSave;

        [Tooltip("Saved canvas states.")]
        public List<CanvasForEditingSave> canvasSave;

        [Tooltip("List of available languages.")]
        public List<LanguageAvailable> availableLanguages;

        [Tooltip("Whether this is the first-time setup.")]
        public bool firstTime;

        [Tooltip("Index for assigning new IDs.")]
        public int idIndex;

        [Tooltip("UI toggle for showing text data.")]
        public bool showTextData;

        [Tooltip("UI toggle for showing canvas data.")]
        public bool showCanvasData;

        [Tooltip("Whether the file is currently saved.")]
        public bool fileIsSaved;
    }

    /// <summary>
    /// Data used for managing a language ID, including context and text.
    /// </summary>
    [Serializable]
    public class ManagerLanguageIdData
    {
        [Tooltip("Unique ID for the managed text.")]
        public int iD;

        [Tooltip("Localized or reference text.")]
        public string text;

        [Tooltip("Contextual info about the text usage.")]
        public string textContext;
    }

    #endregion
}