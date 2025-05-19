/*
 * ---------------------------------------------------------------------------
 * Description: This component localizes TMP_Text components using the
 *              LanguageTools system. It automatically updates text content,
 *              alignment, font, and font size based on the selected language.
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

[AddComponentMenu("Language/UI/TextMesh Pro/Language Text (TMP)")]
public class LanguageTextTMP : MonoBehaviour
{
    [Header("UI Components")]
    public TMP_Text textComponent; // Reference to the TMP_Text component that will be localized.
    [SerializeField] private bool translateText = true; // Indicates whether the text should be translated using language data.
    [Space(10)]
    [IDExists] public int iD = -7; // Unique identifier used to fetch localized content and metadata.

    private LanguageSettingsData languageData; // Stores loaded language settings for use during updates.

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
}

#if UNITY_EDITOR
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
            if (GUILayout.Button("Import Settings", CreateCustomButtonStyle(15), GUILayout.Height(30)))
            {
                if (IsIDInLanguageList(script.iD) && !EditorUtility.DisplayDialog("Replace ID", "An ID with this number is already saved. Do you want to replace it?", "Yes", "No"))
                    return;

                // Extract text, alignment, font size, and font index from TMP_Text.
                string text = script.textComponent.text;
                int alignment = ConvertToAlignmentCode(script.textComponent.alignment);
                int fontSize = (int)script.textComponent.fontSize;
                int fontListIndex = GetFontIndex(script.textComponent.font);

                // Open the language editor with the current component values.
                OpenEditorWindowWithComponent(script.iD, 1, text, alignment, fontSize, fontListIndex);
            }
        }

        EditorGUILayout.Space(5);
        DrawDefaultInspector();
        serializedObject.ApplyModifiedProperties();
    }
}
#endif