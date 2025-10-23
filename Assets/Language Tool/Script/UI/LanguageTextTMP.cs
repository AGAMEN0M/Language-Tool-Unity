/*
 * ---------------------------------------------------------------------------
 * Description: This component localizes TMP_Text components using the
 *              LanguageTools system. It automatically updates text content,
 *              alignment, font, and font size based on the selected language.
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

[AddComponentMenu("Language/UI/TextMesh Pro/Language Text (TMP)")]
public class LanguageTextTMP : MonoBehaviour
{
    #region === Serialized Fields ===

    [Header("UI Components")]
    [SerializeField, Tooltip("The TMP_Text component that will be localized.")]
    private TMP_Text textComponent;
    
    [SerializeField, Tooltip("Enable automatic translation of the text content.")]
    private bool translateText = true;
    
    [Space(10)]
    
    [SerializeField, IDExists, Tooltip("ID used to retrieve localized text and metadata.")]
    private int iD = -7;

    #endregion

    #region === Private Fields ===

    private LanguageSettingsData languageData; // Stores loaded language settings for use during updates.

    #endregion

    #region === Properties ===

    /// <summary>
    /// Gets or sets the TMP_Text component that will be localized.
    /// </summary>
    public TMP_Text TextComponent
    {
        get => textComponent;
        set => textComponent = value;
    }

    /// <summary>
    /// Gets or sets a value indicating whether the text should be automatically translated.
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
    /// Subscribes to language update event and immediately applies localization.
    /// </summary>
    private void OnEnable()
    {
        LanguageManagerDelegate.OnLanguageUpdate += LanguageUpdate;
        LanguageUpdate();
    }

    /// <summary>
    /// Unsubscribes from the language update event.
    /// </summary>
    private void OnDisable() => LanguageManagerDelegate.OnLanguageUpdate -= LanguageUpdate;

    #endregion

    #region === Localization ===

    //// <summary>
    /// Applies localized text, font, alignment, and font size to the TMP_Text component.
    /// </summary>
    public void LanguageUpdate()
    {
        // Ensure the text component is assigned before proceeding.
        if (textComponent == null)
        {
            Debug.LogError("LanguageTextTMP: Text component is not assigned.", this);
            return;
        }

        // Load language configuration data.
        languageData = LoadLanguageSettings();
        if (languageData == null)
        {
            Debug.LogError("LanguageTextTMP: Failed to load LanguageSettingsData.", this);
            return;
        }

        // Apply translated text if translation is enabled.
        if (translateText)
        {
            var translated = GetIDText(languageData.idData, iD);
            if (!string.IsNullOrEmpty(translated)) textComponent.text = translated;
        }

        // Apply additional metadata (alignment, font, size) if available.
        var meta = GetIDMeta(languageData.idMetaData, iD);
        if (meta.alignment != 0) textComponent.alignment = ConvertToTextAnchor(meta.alignment);
        if (meta.fontSize != 0) textComponent.fontSize = meta.fontSize;
        if (meta.fontListIndex != 0) textComponent.font = GetFontByIndex(meta.fontListIndex);
    }

    #endregion
}

#if UNITY_EDITOR

#region === Custom Inspector ===

/// <summary>
/// Custom editor for the LanguageTextTMP component.
/// </summary>
[CanEditMultipleObjects]
[CustomEditor(typeof(LanguageTextTMP))]
public class LanguageTextTMPEditor : Editor
{
    /// <summary>
    /// Draws the custom Inspector interface for LanguageTextTMP.
    /// </summary>
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        var script = (LanguageTextTMP)target;

        using (new EditorGUI.DisabledScope(targets.Length > 1))
        {
            // Button to import current TMP_Text settings into the language editor.
            if (GUILayout.Button(new GUIContent("Import Settings", "Imports the current Text settings into the language editor"), CreateCustomButtonStyle(15), GUILayout.Height(30)))
            {
                if (IsIDInLanguageList(script.ID) && !EditorUtility.DisplayDialog("Replace ID", "An ID with this number is already saved. Do you want to replace it?", "Yes", "No"))
                    return;

                // Extract text, alignment, font size, and font index from TMP_Text.
                string text = script.TextComponent.text;
                int alignment = ConvertToAlignmentCode(script.TextComponent.alignment);
                int fontSize = (int)script.TextComponent.fontSize;
                int fontListIndex = GetFontIndex(script.TextComponent.font);

                // Open the language editor with the current component values.
                OpenEditorWindowWithComponent(script.ID, 1, text, alignment, fontSize, fontListIndex);
            }
        }

        EditorGUILayout.Space(5);
        DrawDefaultInspector();
        serializedObject.ApplyModifiedProperties();
    }
}

#endregion

#endif