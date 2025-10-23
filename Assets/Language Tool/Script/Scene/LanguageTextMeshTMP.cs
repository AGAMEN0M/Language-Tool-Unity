/*
 * ---------------------------------------------------------------------------
 * Description: Enables automatic translation and localization for TextMeshPro
 *              components using LanguageTools. Dynamically updates text, font,
 *              and font size based on selected language settings.
 *              
 * Author: Lucas Gomes Cecchini
 * Pseudonym: AGAMENOM
 * ---------------------------------------------------------------------------
*/

using LanguageTools;
using UnityEngine;
using TMPro;

using static LanguageTools.TMP.FontAndAlignmentUtilityTMP;
using static LanguageTools.LanguageFileManager;

#if UNITY_EDITOR
using UnityEditor;
using static LanguageTools.Editor.LanguageEditorUtilities;
#endif

[AddComponentMenu("Language/3D Object/TextMesh Pro/Language TextMesh (TMP)")]
public class LanguageTextMeshTMP : MonoBehaviour
{
    #region === Serialized Fields ===

    [Header("TextMesh Component")]
    [SerializeField, Tooltip("Reference to the TMP_Text component to be automatically localized.")]
    private TMP_Text textComponent; // Reference to the TMP component whose content will be localized.

    [SerializeField, Tooltip("Determines whether this component should apply automatic translation.")]
    private bool translateText = true; // Whether to apply translation on this text component.

    [Space(10)]

    [SerializeField, IDExists, Tooltip("Unique ID that maps this TextMesh to its translation and metadata in the language tables.")]
    private int iD = -10; // Unique ID to map translation and style metadata in language tables.

    #endregion

    #region === Private Fields ===

    private LanguageSettingsData languageData; // Loaded language configuration used to localize this component.

    #endregion

    #region === Properties ===

    /// <summary>
    /// Gets or sets the TMP_Text component being localized.
    /// </summary>
    public TMP_Text TextComponent
    {
        get => textComponent;
        set => textComponent = value;
    }

    /// <summary>
    /// Gets or sets whether the text should be translated automatically.
    /// </summary>
    public bool TranslateText
    {
        get => translateText;
        set => translateText = value;
    }

    /// <summary>
    /// Gets or sets the unique ID used to locate the text and metadata in the language system.
    /// </summary>
    public int ID
    {
        get => iD;
        set => iD = value;
    }

    #endregion

    #region === Unity Events ===

    /// <summary>
    /// Subscribes to language update events and immediately applies localization.
    /// </summary>
    private void OnEnable()
    {
        LanguageManagerDelegate.OnLanguageUpdate += LanguageUpdate; // Register to receive language update notifications.
        LanguageUpdate(); // Apply language data as soon as the object is enabled.
    }

    /// <summary>
    /// Unsubscribes from language update events when disabled.
    /// </summary>
    private void OnDisable() => LanguageManagerDelegate.OnLanguageUpdate -= LanguageUpdate;

    #endregion

    #region === Localization Logic ===

    /// <summary>
    /// Updates text content, font size, and font reference based on current language settings.
    /// </summary>
    public void LanguageUpdate()
    {
        // Ensure the TMP_Text reference is valid.
        if (textComponent == null)
        {
            Debug.LogError("LanguageTextMeshTMP: TextMesh component is not assigned.", this);
            return;
        }

        // Load the active language configuration.
        languageData = LoadLanguageSettings();
        if (languageData == null)
        {
            Debug.LogError("LanguageTextMeshTMP: Failed to load LanguageSettingsData.", this);
            return;
        }

        // Apply translated text from localization table.
        if (translateText)
        {
            var translated = GetIDText(languageData.idData, iD);
            if (!string.IsNullOrEmpty(translated)) textComponent.text = translated;
        }

        // Apply font metadata if available.
        var meta = GetIDMeta(languageData.idMetaData, iD);
        if (meta.fontSize != 0) textComponent.fontSize = meta.fontSize;
        if (meta.fontListIndex != 0) textComponent.font = GetFontByIndex(meta.fontListIndex);
    }

    #endregion
}

#if UNITY_EDITOR

#region === Custom Editor ===

[CanEditMultipleObjects]
[CustomEditor(typeof(LanguageTextMeshTMP))]
public class LanguageTextMeshTMPEditor : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        var script = (LanguageTextMeshTMP)target;

        // Disable the import button if multiple objects are selected.
        using (new EditorGUI.DisabledScope(targets.Length > 1))
        {
            // Draw "Import Settings" button with custom style and height.
            if (GUILayout.Button(new GUIContent("Import Settings", "Imports or updates this TMP_Text translation into the language system.\nIf an existing ID is found, the user will be prompted to confirm replacement."), CreateCustomButtonStyle(15), GUILayout.Height(30)))
            {
                // If the ID already exists, confirm overwrite with the user.
                if (IsIDInLanguageList(script.ID) && !EditorUtility.DisplayDialog("Replace ID", "An ID with this number is already saved. Do you want to replace it?", "Yes", "No"))
                    return;

                // Gather current TextMesh settings to be saved.
                string text = script.TextComponent.text;
                int fontSize = (int)script.TextComponent.fontSize;
                int fontListIndex = GetFontIndex(script.TextComponent.font);

                // Open the custom language editor window with extracted data.
                OpenEditorWindowWithComponent(script.ID, 2, text, 0, fontSize, fontListIndex);
            }
        }

        EditorGUILayout.Space(5);

        DrawDefaultInspector();
        serializedObject.ApplyModifiedProperties();
    }
}

#endregion

#endif