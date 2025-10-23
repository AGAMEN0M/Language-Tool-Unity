/*
 * ---------------------------------------------------------------------------
 * Description: Localizes UnityEngine.UI.Text components using the LanguageTools
 *              system. Automatically updates the text, font, font size, and
 *              alignment according to the active language configuration.
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

[AddComponentMenu("Language/UI/Legacy/Language Text (Legacy)")]
public class LanguageText : MonoBehaviour
{
    #region === Serialized Fields ===

    [Header("UI Components")]
    [SerializeField, Tooltip("The Text component that will be localized.")]
    private Text textComponent;

    [SerializeField, Tooltip("Enable automatic translation of the text content.")]
    private bool translateText = true;

    [Space(10)]

    [SerializeField, IDExists, Tooltip("ID used to retrieve localized text and metadata.")]
    private int iD = -7;

    #endregion

    #region === Private Fields ===

    private LanguageSettingsData languageData; // Cached instance of the loaded language configuration.

    #endregion

    #region === Properties ===

    /// <summary>
    /// Gets or sets the Text component that will be localized.
    /// </summary>
    public Text TextComponent
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
    /// Subscribes to the language update event and immediately applies localization.
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

    /// <summary>
    /// Applies localized text, font, alignment, and font size to the Text component.
    /// </summary>
    public void LanguageUpdate()
    {
        // Check if the UI Text component is assigned before applying changes.
        if (textComponent == null)
        {
            Debug.LogError("LanguageText: Text component is not assigned.", this);
            return;
        }

        // Load the language data asset from resources or configuration.
        languageData = LoadLanguageSettings();
        if (languageData == null)
        {
            Debug.LogError("LanguageText: Failed to load LanguageSettingsData.", this);
            return;
        }

        // Update text content if translation is enabled.
        if (translateText)
        {
            var translated = GetIDText(languageData.idData, iD);
            if (!string.IsNullOrEmpty(translated)) textComponent.text = translated;
        }

        // Apply alignment, font size, and font using metadata from language files.
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
/// Custom editor for the LanguageText component.
/// </summary>
[CanEditMultipleObjects]
[CustomEditor(typeof(LanguageText))]
public class LanguageTextEditor : Editor
{
    /// <summary>
    /// Draws the custom Inspector interface for LanguageText.
    /// </summary>
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        var script = (LanguageText)target;

        using (new EditorGUI.DisabledScope(targets.Length > 1))
        {
            // Button to import the current Text component settings into the language editor.
            if (GUILayout.Button(new GUIContent("Import Settings", "Imports the current Text settings into the language editor"), CreateCustomButtonStyle(15), GUILayout.Height(30)))
            {
                if (IsIDInLanguageList(script.ID) && !EditorUtility.DisplayDialog("Replace ID", "An ID with this number is already saved. Do you want to replace it?", "Yes", "No"))
                    return;

                // Extract properties from the assigned Text component.
                string text = script.TextComponent.text;
                int alignment = ConvertToAlignmentCode(script.TextComponent.alignment);
                int fontSize = script.TextComponent.fontSize;
                int fontListIndex = GetFontIndex(script.TextComponent.font);

                // Open the editor window with the retrieved component values.
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