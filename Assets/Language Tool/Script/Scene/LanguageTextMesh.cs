/*
 * ---------------------------------------------------------------------------
 * Description: Enables automatic translation and localization for legacy 
 *              TextMesh components using LanguageTools. Dynamically updates 
 *              text, font, and font size based on current language settings.
 * Author: Lucas Gomes Cecchini
 * Pseudonym: AGAMENOM
 * ---------------------------------------------------------------------------
*/

using LanguageTools;
using UnityEngine;

#if UNITY_EDITOR
using static LanguageTools.Editor.LanguageEditorUtilities;
using UnityEditor;
#endif

using static LanguageTools.LanguageFileManager;
using static LanguageTools.Legacy.FontAndAlignmentUtility;

[AddComponentMenu("Language/3D Object/Legacy/Language TextMesh (Legacy)")]
public class LanguageTextMesh : MonoBehaviour
{
    [Header("TextMesh Component")]
    public TextMesh textComponent; // Reference to the legacy TextMesh to be localized.
    [SerializeField] private bool translateText = true; // Whether to apply translation on this text component.
    [Space(10)]
    [IDExists] public int iD = -10; // Unique ID to map translation and style metadata in language tables.

    private LanguageSettingsData languageData; // Loaded language configuration used to localize this component.

    /// <summary>
    /// Subscribes to language update event and updates text when enabled.
    /// </summary>
    private void OnEnable()
    {
        LanguageManagerDelegate.OnLanguageUpdate += LanguageUpdate; // Register this component to receive language change notifications.
        LanguageUpdate(); // Apply localization immediately on enable.
    }

    /// <summary>
    /// Unsubscribes from language update event when disabled.
    /// </summary>
    private void OnDisable() => LanguageManagerDelegate.OnLanguageUpdate -= LanguageUpdate;

    /// <summary>
    /// Updates text, font, and font size from language settings.
    /// </summary>
    public void LanguageUpdate()
    {
        // Validate component assignment.
        if (textComponent == null)
        {
            Debug.LogError("LanguageTextMesh: TextMesh component is not assigned.", this);
            return;
        }

        // Load the active language settings.
        languageData = LoadLanguageSettings();
        if (languageData == null)
        {
            Debug.LogError("LanguageTextMesh: Failed to load LanguageSettingsData.", this);
            return;
        }

        // Apply translation if enabled.
        if (translateText)
        {
            string translated = GetIDText(languageData.idData, iD);
            if (!string.IsNullOrEmpty(translated)) textComponent.text = translated;
        }

        // Apply font styling from metadata.
        var meta = GetIDMeta(languageData.idMetaData, iD);
        if (meta.fontSize != 0) textComponent.fontSize = meta.fontSize;

        if (meta.fontListIndex != 0)
        {
            var font = GetFontByIndex(meta.fontListIndex);
            if (font != null)
            {
                textComponent.font = font;

                // Update the font material texture on the MeshRenderer if needed.
                var renderer = textComponent.GetComponent<MeshRenderer>();
                if (renderer != null && font.material != null) renderer.material.mainTexture = font.material.mainTexture;
            }
        }
    }
}

#if UNITY_EDITOR
[CanEditMultipleObjects]
[CustomEditor(typeof(LanguageTextMesh))]
public class LanguageTextMeshEditor : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        var script = (LanguageTextMesh)target;

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
                int fontSize = script.textComponent.fontSize;
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