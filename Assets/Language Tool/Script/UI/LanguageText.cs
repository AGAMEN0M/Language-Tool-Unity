/*
 * ---------------------------------------------------------------------------
 * Description: Localizes UnityEngine.UI.Text components using the LanguageTools
 *              system. Automatically updates the text, font, font size, and
 *              alignment according to the active language configuration.
 * Author: Lucas Gomes Cecchini
 * Pseudonym: AGAMENOM
 * ---------------------------------------------------------------------------
*/

using UnityEngine.UI;
using LanguageTools;
using UnityEngine;

#if UNITY_EDITOR
using static LanguageTools.Editor.LanguageEditorUtilities;
using UnityEditor;
#endif

using static LanguageTools.LanguageFileManager;
using static LanguageTools.Legacy.FontAndAlignmentUtility;

[AddComponentMenu("Language/UI/Legacy/Language Text (Legacy)")]
public class LanguageText : MonoBehaviour
{
    [Header("UI Components")]
    public Text textComponent; // Reference to the Text component that will be localized.
    [SerializeField] private bool translateText = true; // Determines whether the component should automatically translate the text content.
    [Space(10)]
    [IDExists] public int iD = -7; // Identifier used to retrieve localized strings and metadata from language files.

    private LanguageSettingsData languageData; // Cached instance of the loaded language configuration.

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
}

#if UNITY_EDITOR
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
            if (GUILayout.Button("Import Settings", CreateCustomButtonStyle(15), GUILayout.Height(30)))
            {
                if (IsIDInLanguageList(script.iD) && !EditorUtility.DisplayDialog("Replace ID", "An ID with this number is already saved. Do you want to replace it?", "Yes", "No"))
                    return;

                // Extract properties from the assigned Text component.
                string text = script.textComponent.text;
                int alignment = ConvertToAlignmentCode(script.textComponent.alignment);
                int fontSize = script.textComponent.fontSize;
                int fontListIndex = GetFontIndex(script.textComponent.font);

                // Open the editor window with the retrieved component values.
                OpenEditorWindowWithComponent(script.iD, 1, text, alignment, fontSize, fontListIndex);
            }
        }

        EditorGUILayout.Space(5);
        DrawDefaultInspector();
        serializedObject.ApplyModifiedProperties();
    }
}
#endif