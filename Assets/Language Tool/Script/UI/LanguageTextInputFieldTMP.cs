/*
 * ---------------------------------------------------------------------------
 * Description: Localizes TMP_InputField text and placeholder using the LanguageTools 
 *              system. Automatically updates the placeholder text, font, alignment, 
 *              and font size based on the selected language configuration.
 * Author: Lucas Gomes Cecchini
 * Pseudonym: AGAMENOM
 * ---------------------------------------------------------------------------
*/

using LanguageTools;
using UnityEngine;
using TMPro;

#if UNITY_EDITOR
using static LanguageTools.Editor.LanguageEditorUtilities;
using UnityEditor;
#endif

using static LanguageTools.LanguageFileManager;
using static LanguageTools.TMP.FontAndAlignmentUtilityTMP;

[AddComponentMenu("Language/UI/TextMesh Pro/Language Text Input Field (TMP)")]
public class LanguageTextInputFieldTMP : MonoBehaviour
{
    [Header("UI Components")]
    public TMP_InputField inputField; // Reference to the TMP_InputField to localize.
    [SerializeField] private bool translateText = true; // Whether or not the placeholder text should be translated.
    [Space(10)]
    [IDExists] public int iD = -6; // LanguageTools ID used to fetch localization data.

    private LanguageSettingsData languageData; // Cached language configuration data.
    private TMP_Text text; // Reference to the input field's text component.
    private TMP_Text placeholder; // Reference to the input field's placeholder text component.

    /// <summary>
    /// Subscribes to the language update event and applies localization.
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

    /// <summary>
    /// Updates placeholder and input text with localized text, font, alignment, and font size.
    /// </summary>
    public void LanguageUpdate()
    {
        // Validate input field.
        if (inputField == null)
        {
            Debug.LogError("LanguageTextInputFieldTMP: InputField is not assigned.", this);
            return;
        }

        // Try to get placeholder TMP_Text component.
        if (!inputField.placeholder.TryGetComponent(out placeholder))
        {
            Debug.LogError("LanguageTextInputFieldTMP: Placeholder component is missing or not a TMP_Text.", this);
            return;
        }

        // Try to get input text TMP_Text component.
        if (!inputField.textComponent.TryGetComponent(out text))
        {
            Debug.LogError("LanguageTextInputFieldTMP: Text component is missing or not a TMP_Text.", this);
            return;
        }

        // Load language data from LanguageTools system.
        languageData = LoadLanguageSettings();
        if (languageData == null)
        {
            Debug.LogError("LanguageTextInputFieldTMP: Failed to load LanguageSettingsData.", this);
            return;
        }

        // Update placeholder text if translation is enabled.
        if (translateText)
        {
            var translated = GetIDText(languageData.idData, iD);
            if (!string.IsNullOrEmpty(translated)) placeholder.text = translated;
        }

        // Apply meta settings (alignment, font size, font).
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
}

#if UNITY_EDITOR
/// <summary>
/// Custom editor for the LanguageTextInputFieldTMP component.
/// </summary>
[CanEditMultipleObjects]
[CustomEditor(typeof(LanguageTextInputFieldTMP))]
public class LanguageTextInputFieldTMPEditor : Editor
{
    /// <summary>
    /// Draws the custom Inspector with an import button and fallback validation.
    /// </summary>
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        var script = (LanguageTextInputFieldTMP)target;

        using (new EditorGUI.DisabledScope(targets.Length > 1))
        {
            // Draw "Import Settings" button.
            if (GUILayout.Button("Import Settings", CreateCustomButtonStyle(15), GUILayout.Height(30)))
            {
                // Validate ID before importing.
                if (IsIDInLanguageList(script.iD) && !EditorUtility.DisplayDialog("Replace ID", "An ID with this number is already saved. Do you want to replace it?", "Yes", "No"))
                    return;

                // Extract and pass current values to the editor window if valid.
                if (script.inputField != null && script.inputField.placeholder.TryGetComponent<TMP_Text>(out var placeholder))
                {
                    string text = placeholder.text;
                    int alignment = ConvertToAlignmentCode(placeholder.alignment);
                    int fontSize = (int)script.inputField.pointSize;
                    int fontListIndex = GetFontIndex(script.inputField.fontAsset);

                    OpenEditorWindowWithComponent(script.iD, 3, text, alignment, fontSize, fontListIndex);
                }
                else
                {
                    Debug.LogError("LanguageTextInputFieldTMP: InputField or placeholder is not assigned or valid.");
                }
            }
        }

        // Draw default Inspector UI.
        EditorGUILayout.Space(5);
        DrawDefaultInspector();
        serializedObject.ApplyModifiedProperties();
    }
}
#endif