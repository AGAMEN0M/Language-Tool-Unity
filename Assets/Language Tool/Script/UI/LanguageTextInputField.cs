/*
 * ---------------------------------------------------------------------------
 * Description: Localizes Legacy Unity UI InputField and its placeholder using the 
 *              LanguageTools system. Automatically updates placeholder text, font, 
 *              alignment, and font size according to the selected language settings.
 *              
 * Author: Lucas Gomes Cecchini
 * Pseudonym: AGAMENOM
 * ---------------------------------------------------------------------------
*/

using UnityEngine.UI;
using LanguageTools;
using UnityEngine;

using static LanguageTools.Legacy.FontAndAlignmentUtility;
using static LanguageTools.LanguageFileManager;

#if UNITY_EDITOR
using UnityEditor;
using static LanguageTools.Editor.LanguageEditorUtilities;
#endif

[AddComponentMenu("Language/UI/Legacy/Language Text Input Field (Legacy)")]
public class LanguageTextInputField : MonoBehaviour
{
    #region === Serialized Fields ===

    [Header("UI Components")]
    [SerializeField, Tooltip("Reference to the legacy Unity UI InputField to localize.")]
    private InputField inputField;

    [SerializeField, Tooltip("Determines whether the placeholder text should be automatically translated.")]
    private bool translateText = true;
    
    [Space(10)]

    [SerializeField, IDExists, Tooltip("LanguageTools ID used to fetch localized text and metadata.")]
    private int iD = -6;

    #endregion

    #region === Private Fields ===

    private LanguageSettingsData languageData; // Cached language configuration data.
    private Text text; // Reference to the input field's text component.
    private Text placeholder; // Reference to the input field's placeholder component.

    #endregion

    #region === Properties ===

    /// <summary>
    /// Gets or sets the legacy InputField component to localize.
    /// </summary>
    public InputField InputField
    {
        get => inputField;
        set => inputField = value;
    }

    /// <summary>
    /// Gets or sets a value indicating whether the placeholder text should be automatically translated.
    /// </summary>
    public bool TranslateText
    {
        get => translateText;
        set => translateText = value;
    }

    /// <summary>
    /// Gets or sets the ID used to retrieve localized text and metadata from language files.
    /// </summary>
    public int ID
    {
        get => iD;
        set => iD = value;
    }

    #endregion

    #region === Unity Events ===

    /// <summary>
    /// Registers to the language update event and applies initial localization.
    /// </summary>
    private void OnEnable()
    {
        LanguageManagerDelegate.OnLanguageUpdate += LanguageUpdate;
        LanguageUpdate();
    }

    /// <summary>
    /// Deregisters from the language update event.
    /// </summary>
    private void OnDisable() => LanguageManagerDelegate.OnLanguageUpdate -= LanguageUpdate;

    #endregion

    #region === Localization ===

    /// <summary>
    /// Applies localized text, font, alignment, and font size to the InputField and its placeholder.
    /// </summary>
    public void LanguageUpdate()
    {
        // Validate that the input field is assigned.
        if (inputField == null)
        {
            Debug.LogError("LanguageTextInputField: InputField is not assigned.", this);
            return;
        }

        // Try to get the placeholder Text component.
        if (!inputField.placeholder.TryGetComponent(out placeholder))
        {
            Debug.LogError("LanguageTextInputField: Placeholder component is missing or not a Text.", this);
            return;
        }

        // Try to get the input Text component.
        if (!inputField.textComponent.TryGetComponent(out text))
        {
            Debug.LogError("LanguageTextInputField: Text component is missing or not a Text.", this);
            return;
        }

        // Load language settings data.
        languageData = LoadLanguageSettings();
        if (languageData == null)
        {
            Debug.LogError("LanguageTextInputField: Failed to load LanguageSettingsData.", this);
            return;
        }

        // Update placeholder text if translation is enabled.
        if (translateText)
        {
            var translated = GetIDText(languageData.idData, iD);
            if (!string.IsNullOrEmpty(translated)) placeholder.text = translated;
        }

        // Apply metadata settings (alignment, font size, font).
        var meta = GetIDMeta(languageData.idMetaData, iD);

        if (meta.alignment != 0)
        {
            var alignment = ConvertToTextAnchor(meta.alignment);
            placeholder.alignment = alignment;
            text.alignment = alignment;
        }

        if (meta.fontSize != 0)
        {
            placeholder.fontSize = meta.fontSize;
            text.fontSize = meta.fontSize;
        }

        if (meta.fontListIndex != 0)
        {
            var font = GetFontByIndex(meta.fontListIndex);
            placeholder.font = font;
            text.font = font;
        }
    }

    #endregion
}

#if UNITY_EDITOR

#region === Custom Editor ===

/// <summary>
/// Custom editor for the LanguageTextInputField component.
/// </summary>
[CanEditMultipleObjects]
[CustomEditor(typeof(LanguageTextInputField))]
public class LanguageTextInputFieldEditor : Editor
{
    /// <summary>
    /// Renders the custom inspector with import button for placeholder settings.
    /// </summary>
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        var script = (LanguageTextInputField)target;

        using (new EditorGUI.DisabledScope(targets.Length > 1))
        {
            // Draw the "Import Settings" button.
            if (GUILayout.Button(new GUIContent("Import Settings", "Imports the current InputField placeholder settings (text, font, size, alignment) into the LanguageTools editor."), CreateCustomButtonStyle(15), GUILayout.Height(30)))
            {
                // Prompt user before overwriting existing ID.
                if (IsIDInLanguageList(script.ID) && !EditorUtility.DisplayDialog("Replace ID", "An ID with this number is already saved. Do you want to replace it?", "Yes", "No"))
                    return;

                // Extract and pass data from the input field's placeholder to the editor window.
                if (script.InputField != null && script.InputField.placeholder.TryGetComponent<Text>(out var placeholder))
                {
                    string text = placeholder.text;
                    int alignment = ConvertToAlignmentCode(placeholder.alignment);
                    int fontSize = placeholder.fontSize;
                    int fontListIndex = GetFontIndex(placeholder.font);

                    OpenEditorWindowWithComponent(script.ID, 3, text, alignment, fontSize, fontListIndex);
                }
                else
                {
                    Debug.LogError("LanguageTextInputField: InputField or Placeholder component is missing.");
                }
            }
        }

        // Draw default Inspector layout.
        EditorGUILayout.Space(5);
        DrawDefaultInspector();
        serializedObject.ApplyModifiedProperties();
    }
}

#endregion

#endif