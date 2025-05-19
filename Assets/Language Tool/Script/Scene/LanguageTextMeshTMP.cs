/*
 * ---------------------------------------------------------------------------
 * Description: Enables automatic translation and localization for TextMeshPro
 *              components using LanguageTools. Dynamically updates text, font,
 *              and font size based on selected language settings.
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

[AddComponentMenu("Language/3D Object/TextMesh Pro/Language TextMesh (TMP)")]
public class LanguageTextMeshTMP : MonoBehaviour
{
    [Header("TextMesh Component")]
    public TMP_Text textComponent; // Reference to the TMP component whose content will be localized.
    [SerializeField] private bool translateText = true; // Determines whether this component should translate its text content.
    [Space(10)]
    [IDExists] public int iD = -10; // Localization key ID used to fetch translated text and metadata.

    private LanguageSettingsData languageData; // Cached language data loaded at runtime.

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
}

#if UNITY_EDITOR
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
            if (GUILayout.Button("Import Settings", CreateCustomButtonStyle(15), GUILayout.Height(30)))
            {
                // If the ID already exists, confirm overwrite with the user.
                if (IsIDInLanguageList(script.iD) && !EditorUtility.DisplayDialog("Replace ID", "An ID with this number is already saved. Do you want to replace it?", "Yes", "No"))
                    return;

                // Gather current TextMesh settings to be saved.
                string text = script.textComponent.text;
                int fontSize = (int)script.textComponent.fontSize;
                int fontListIndex = GetFontIndex(script.textComponent.font);

                // Open the custom language editor window with extracted data.
                OpenEditorWindowWithComponent(script.iD, 2, text, 0, fontSize, fontListIndex);
            }
        }

        EditorGUILayout.Space(5);

        DrawDefaultInspector();
        serializedObject.ApplyModifiedProperties();
    }
}
#endif