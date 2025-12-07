/*
 * ---------------------------------------------------------------------------
 * Description: Ensures that all TMP_FontAssets referenced inside the LanguageSettingsData 
 *              are properly imported and included in the Unity build. This is achieved by 
 *              automatically generating temporary TextMeshPro objects for every detected 
 *              font, forcing Unity to recognize and cache those assets during import. 
 *              
 *              This component is intended for Editor use only and should be placed in an 
 *              initialization scene (such as a splash or main menu) to guarantee that all 
 *              fonts required for multilingual text rendering are safely embedded in the 
 *              final build.
 *
 * Author: Lucas Gomes Cecchini
 * Pseudonym: AGAMENOM
 * ---------------------------------------------------------------------------
 */

using UnityEngine;
using System;
using TMPro;

using static LanguageTools.LanguageFileManager;

#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteInEditMode]
[AddComponentMenu("Language/UI/Complements/Force Import Of Fonts (TMP)")]
public class ForceImportOfFontsTMP : MonoBehaviour
{
#if UNITY_EDITOR

    #region === Structs ===

    /// <summary>
    /// Represents a pair of text content and its corresponding TMP_FontAsset.
    /// This allows the system to detect when a specific font should use a
    /// custom preview string instead of the default font name.
    /// </summary>
    [Serializable]
    public struct FontText
    {
        [Tooltip("Sample text that will be displayed when this font is imported.")]
        public string text;

        [Tooltip("The TMP_FontAsset that this sample text belongs to.")]
        public TMP_FontAsset font;
    }

    #endregion

    #region === Fields ===

    [Header("References")]
    [SerializeField, Tooltip("Reference TextMeshPro GameObject used as the base for font import testing.")]
    private GameObject textMesh;

    [Space(10)]

    [SerializeField, Tooltip("Optional sample texts for specific fonts.")]
    private FontText[] fontTexts;

    [Space(10)]

    [SerializeField, Tooltip("Holds all generated GameObjects to prevent duplication after reload.")]
    private GameObject[] gameObjects;

    #endregion

    #region === Properties ===

    /// <summary>
    /// Reference to the base TextMeshPro object used for font cloning.
    /// </summary>
    public GameObject TextMesh
    {
        get => textMesh;
        set => textMesh = value;
    }

    /// <summary>
    /// Optional dictionary of sample texts associated with specific TMP_FontAssets.
    /// </summary>
    public FontText[] FontTexts
    {
        get => fontTexts;
        set => fontTexts = value;
    }

    /// <summary>
    /// Array that stores all generated TextMeshPro objects to prevent duplication.
    /// </summary>
    public GameObject[] GameObjects
    {
        get => gameObjects;
        set => gameObjects = value;
    }

    #endregion

#endif

    #region === Unity Lifecycle ===

    /// <summary>
    /// Called manually or on component initialization to rebuild all TMP font instances.
    /// </summary>
    public void Start()
    {
#if UNITY_EDITOR
        // Validate the TextMesh reference.
        if (textMesh == null)
        {
            Debug.LogError("ForceImportOfFontsTMP: 'textMesh' reference is missing.", this);
            return;
        }

        // Destroy previously generated clones to avoid duplication.
        if (gameObjects != null && gameObjects.Length > 0)
        {
            foreach (var obj in gameObjects)
            {
                if (obj != null) DestroyImmediate(obj);
            }
        }

        // Load LanguageSettingsData from the LanguageFileManager.
        var data = LoadLanguageSettings();
        if (data == null)
        {
            Debug.LogError("ForceImportOfFontsTMP: Failed to load LanguageSettingsData.", this);
            return;
        }

        // Validate TMP font list existence.
        if (data.fontListDataTMP == null || data.fontListDataTMP.TMPFontList.Count == 0)
        {
            Debug.LogWarning("ForceImportOfFontsTMP: No TMP fonts found in LanguageSettingsData.", this);
            return;
        }

        // Prepare new list to store clones.
        gameObjects = new GameObject[data.fontListDataTMP.TMPFontList.Count];
        int index = 0;

        // Instantiate a clone of textMesh for each TMP font.
        foreach (var fontData in data.fontListDataTMP.TMPFontList)
        {
            if (fontData == null)
            {
                Debug.LogWarning($"ForceImportOfFontsTMP: Null font detected in LanguageSettingsData entry '{fontData.name}'.", this);
                continue;
            }

            // Create clone under this component’s transform.
            GameObject clone = Instantiate(textMesh, transform);

            // Try to retrieve the TMP_Text component.
            if (!clone.TryGetComponent<TMP_Text>(out var tmp))
            {
                Debug.LogError($"ForceImportOfFontsTMP: The provided TextMesh '{textMesh.name}' does not contain a TMP_Text component.", this);
                DestroyImmediate(clone);
                continue;
            }

            string finalText = fontData.name; // Default text.

            if (fontTexts != null && fontTexts.Length > 0)
            {
                foreach (var ft in fontTexts)
                {
                    if (ft.font == fontData)
                    {
                        finalText = $"Text ({fontData.name}) - [{ft.text}]";
                        break;
                    }
                }
            }

            // Apply font asset.
            tmp.font = fontData;

            // Apply final text.
            tmp.text = finalText;

            // Set clone name for hierarchy clarity.
            clone.name = $"Text ({fontData.name})";

            // Activate clone object.
            clone.SetActive(true);

            // Store clone reference.
            gameObjects[index++] = clone;
        }

        Debug.Log($"ForceImportOfFontsTMP: Successfully created {index} TMP font import instances.", this);

#else
        // Automatically remove this script at runtime.
        Destroy(this);
#endif
    }

    #endregion
}

#if UNITY_EDITOR

#region === Custom Inspector ===

/// <summary>
/// Custom inspector for ForceImportOfFontsTMP providing contextual information.
/// </summary>
[CanEditMultipleObjects]
[CustomEditor(typeof(ForceImportOfFontsTMP))]
public class ForceImportOfFontsTMPEditor : Editor
{
    public override void OnInspectorGUI()
    {
        var script = (ForceImportOfFontsTMP)target;

        serializedObject.Update();

        // Display help box explaining the script’s function.
        GUI.color = Color.yellow;
        EditorGUILayout.HelpBox(
            "Due to how Unity imports TMP_FontAssets, many fonts may not be included in the Build.\n" +
            "The recommended fix is to pre-import them in the initial scene or main menu using this tool.",
            MessageType.Warning
        );
        GUI.color = Color.white;

        EditorGUILayout.Space(10);

        // Reload button that rebuilds all TMP font instances.
        if (GUILayout.Button(new GUIContent("Reload (Font List Data)", "Rebuilds all TMP font instances from the LanguageSettingsData."), GUILayout.Height(30)))
        {
            script.Start();
        }

        EditorGUILayout.Space(10);

        // Draw inspector fields.
        DrawDefaultInspector();

        serializedObject.ApplyModifiedProperties();
    }
}

#endregion

#endif