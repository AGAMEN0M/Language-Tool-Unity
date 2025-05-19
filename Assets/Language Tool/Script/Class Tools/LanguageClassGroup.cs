/*
 * ---------------------------------------------------------------------------
 * Description: Defines data structures used for handling language localization
 *              and UI canvas configurations within the application. It includes
 *              serializable classes for saving language IDs, canvas states,
 *              user-defined translations, and Unity UI settings.
 * Author: Lucas Gomes Cecchini
 * Pseudonym: AGAMENOM
 * ---------------------------------------------------------------------------
*/

using System.Collections.Generic;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine;

namespace LanguageTools
{
    /// <summary>
    /// Represents a language option available for selection.
    /// </summary>
    [System.Serializable]
    public class LanguageAvailable
    {
        public string culture; // Culture code (e.g., "en-US").
        public string name; // Display name of the language.
        public bool isAvailable; // Whether the language is currently available for use.
        public int columnIndex; // Column index in the localization table.
    }

    /// <summary>
    /// Stores an ID and its associated localized text.
    /// </summary>
    [System.Serializable]
    public class IdData
    {
        public int iD; // Unique identifier for the text entry.
        public string text; // Localized text string.
    }

    /// <summary>
    /// Holds metadata settings related to a specific ID.
    /// </summary>
    [System.Serializable]
    public class IdMetaData
    {
        [Header("Custom ID")]
        public int iD; // Unique identifier for the text metadata.
        [Space(5)]
        [Header("Text Settings")]
        public int alignment; // Text alignment setting.
        public int fontSize; // Font size setting.
        public int fontListIndex; // Index for selecting font from a predefined list.
        public int componentType; // Type of component using this metadata (e.g., Text, TMP, etc.).
    }

    /// <summary>
    /// Represents the serialized state of a UI GameObject in a canvas.
    /// </summary>
    [System.Serializable]
    public class CanvasDataList
    {
        [Header("Game Object Settings")]
        public string gameObjectName; // Name of the UI GameObject.
        public bool gameObjectEnable; // Whether the GameObject is enabled.
        [Space(10)]
        public RectTransform rectTransform; // Reference to the RectTransform component.
        public int instanceID; // Unique instance ID of the GameObject.
        [Space(5)]
        public Quaternion localRotation; // Local rotation of the GameObject.
        public Vector3 localPosition; // Local position of the GameObject.
        public Vector3 localScale; // Local scale of the GameObject.
        [Space(5)]
        public Vector2 anchorMin; // Minimum anchor point.
        public Vector2 anchorMax; // Maximum anchor point.
        public Vector2 anchoredPosition; // Anchored position within the parent.
        public Vector2 sizeDelta; // Size delta of the RectTransform.
        public Vector2 pivot; // Pivot point of the RectTransform.
    }

    /// <summary>
    /// Container for saving the entire canvas structure and configuration.
    /// </summary>
    [System.Serializable]
    public class CanvasStructure
    {
        [Header("Canvas Structure")]
        public string canvasName; // Name of the canvas.
        public CanvasLayers[] canvasLayers; // Array of canvas layers.
        [Space(10)]
        public RectTransformData rectTransform; // Layout data of the canvas RectTransform.
        [Space(10)]
        public CanvasData canvas; // Serialized data for the Canvas component.
        [Space(10)]
        public CanvasScalerData canvasScaler; // Serialized data for the CanvasScaler component.
        [Space(10)]
        public GraphicRaycasterData graphicRaycaster; // Serialized data for the GraphicRaycaster component.
    }

    /// <summary>
    /// Represents a group of objects and layout data within a canvas layer.
    /// </summary>
    [System.Serializable]
    public class CanvasLayers
    {
        public string[] CanvasObjectsLayers; // Names of objects in the canvas layer.
        public RectTransformData[] rectTransforms; // Layout data for each object.
    }

    /// <summary>
    /// Serialized structure representing a RectTransform's layout properties.
    /// </summary>
    [System.Serializable]
    public class RectTransformData
    {
        [Header("RectTransform")]
        public Quaternion localRotation; // Local rotation of the RectTransform.
        public Vector3 localPosition; // Local position of the RectTransform.
        public Vector3 localScale; // Local scale of the RectTransform.
        [Space(5)]
        public Vector2 anchorMin; // Minimum anchor point.
        public Vector2 anchorMax; // Maximum anchor point.
        public Vector2 anchoredPosition; // Anchored position within the parent.
        public Vector2 sizeDelta; // Size delta of the RectTransform.
        public Vector2 pivot; // Pivot point of the RectTransform.
    }

    /// <summary>
    /// Contains properties related to the Canvas component.
    /// </summary>
    [System.Serializable]
    public class CanvasData
    {
        [Header("Canvas")]
        public RenderMode renderMode; // Mode in which the canvas is rendered.
        public float planeDistance; // Distance of the canvas plane from the camera.
        public bool pixelPerfect; // Enables pixel-perfect rendering.
        public bool overrideSorting; // Whether to override sorting behavior.
        public bool overridePixelPerfect; // Overrides global pixel-perfect settings.
        public float sortingBucketNormalizedSize; // Normalized size for sorting bucket.
        public bool vertexColorAlwaysGammaSpace; // Forces vertex color to be in gamma space.
        public AdditionalCanvasShaderChannels additionalShaderChannels; // Additional shader channels.
        public StandaloneRenderResize updateRectTransformForStandalone; // Controls resizing on standalone platforms.
    }

    /// <summary>
    /// Contains properties related to the CanvasScaler component.
    /// </summary>
    [System.Serializable]
    public class CanvasScalerData
    {
        [Header("Canvas Scaler")]
        public CanvasScaler.ScaleMode uiScaleMode; // UI scaling mode.
        public float referencePixelsPerUnit; // Reference pixels per unit.
        public float scaleFactor; // Scale factor for the canvas.
        public Vector2 referenceResolution; // Reference resolution for UI scaling.
        public CanvasScaler.ScreenMatchMode screenMatchMode; // Mode to match screen size.
        public float matchWidthOrHeight; // Weight of width vs. height in scaling.
        public CanvasScaler.Unit physicalUnit; // Unit used for physical sizes.
        public float fallbackScreenDPI; // Fallback DPI when screen DPI is unavailable.
        public float defaultSpriteDPI; // Default DPI for sprites.
        public float dynamicPixelsPerUnit; // Dynamic pixels per unit setting.
        public bool presetInfoIsWorld; // Indicates if the preset info is for world space.
    }

    /// <summary>
    /// Contains properties related to the GraphicRaycaster component.
    /// </summary>
    [System.Serializable]
    public class GraphicRaycasterData
    {
        [Header("Graphic Raycaster")]
        public bool ignoreReversedGraphics; // Whether to ignore graphics facing away.
        public GraphicRaycaster.BlockingObjects blockingObjects; // Types of objects that can block raycasts.
        public LayerMask blockingMask; // Layer mask used for raycast blocking.
    }

    /// <summary>
    /// Represents a selectable language option including its visual and ID.
    /// </summary>
    [System.Serializable]
    public class LanguageOptions
    {
        public string text; // Display text of the language option.
        public Sprite sprite; // Icon representing the language.
        [IDExists] public int iD; // ID associated with the language.
    }

    /// <summary>
    /// Holds script text content and an associated UnityEvent.
    /// </summary>
    [System.Serializable]
    public class ScriptText
    {
        [IDExists] public int iD; // Unique identifier for the text.
        [TextArea] public string text; // Script content text.
        [Space(5)]
        public UnityEvent<string> targetScripts; // Event triggered with the text.
    }

    /// <summary>
    /// Represents a localized line of text that can be optionally translated.
    /// </summary>
    [System.Serializable]
    public class LanguageLines
    {
        [IDExists] public int iD; // Unique identifier for the line.
        public string text; // Line text.
        public bool translateText; // Whether this line should be translated.
    }

    /// <summary>
    /// Represents language component data for editing and saving.
    /// </summary>
    [System.Serializable]
    public class LanguageForEditingSave
    {
        [Header("Custom ID")]
        public int iD; // Unique ID for the component text.
        [Header("Text Settings")]
        [Multiline] public string text; // Editable text content.
        public int alignment; // Text alignment index.
        public int fontSize; // Font size.
        public int fontListIndex; // Index of font from list.
        [Header("Other Systems")]
        public string textContext; // Context of where the text is used.
        public int componentType; // Type of component this text is bound to.
    }

    /// <summary>
    /// Represents a canvas element's saved data including context and state.
    /// </summary>
    [System.Serializable]
    public class CanvasForEditingSave
    {
        [Header("Custom ID")]
        public int canvasID; // ID of the canvas item.
        [Header("Canvas Settings")]
        public string textContext; // Contextual label for canvas usage.
        public string json; // Serialized canvas state in JSON.
    }

    /// <summary>
    /// Root object for language file data used in the Language File Manager window.
    /// </summary>
    [System.Serializable]
    public class LanguageFileManagerWindowData
    {
        public string languageForEditing; // Language currently being edited.
        public List<LanguageForEditingSave> componentSave; // Saved language components.
        public List<CanvasForEditingSave> canvasSave; // Saved canvas states.
        public List<LanguageAvailable> availableLanguages; // List of available languages.
        public bool firstTime; // Whether this is the first-time setup.
        public int idIndex; // Index for assigning new IDs.
        public bool showTextData; // UI toggle for showing text data.
        public bool showCanvasData; // UI toggle for showing canvas data.
        public bool fileIsSaved; // Whether the file is currently saved.
    }

    /// <summary>
    /// Data used for managing a language ID, including context and text.
    /// </summary>
    [System.Serializable]
    public class ManagerLanguageIdData
    {
        public int iD; // Unique ID for the managed text.
        public string text; // Localized or reference text.
        public string textContext; // Contextual info about the text usage.
    }
}