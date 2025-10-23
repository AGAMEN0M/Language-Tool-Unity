/*
 * ---------------------------------------------------------------------------
 * Description: Forces the import and caching of all TMP_FontAssets listed in
 *              the LanguageSettingsData by creating temporary TextMeshPro
 *              objects to ensure all fonts are referenced in the build.
 *              
 * Author: Lucas Gomes Cecchini
 * Pseudonym: AGAMENOM
 * ---------------------------------------------------------------------------
*/

using UnityEngine;
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

    #region === Fields ===

    [Header("References")]
    [SerializeField, Tooltip("Reference TextMeshPro GameObject used as the base for font import testing.")]
    private GameObject textMesh;

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
    [ContextMenu("Reload (Font List Data)")]
    private void Start()
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
            clone.name = $"Text ({fontData.name})";

            // Try to retrieve the TMP_Text component.
            if (!clone.TryGetComponent<TMP_Text>(out var tmp))
            {
                Debug.LogError($"ForceImportOfFontsTMP: The provided TextMesh '{textMesh.name}' does not contain a TMP_Text component.", this);
                DestroyImmediate(clone);
                continue;
            }

            // Apply font asset and display name.
            tmp.font = fontData;
            tmp.text = fontData.name;
            clone.SetActive(true);

            // Store the clone reference.
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
        serializedObject.Update();

        // Display help box explaining the script’s function.
        GUI.color = Color.yellow;
        EditorGUILayout.HelpBox(
            "Due to how Unity imports TMP_FontAssets, many fonts may not be included in the Build.\n" +
            "The recommended fix is to pre-import them in the initial scene or in the main menu using this tool.",
            MessageType.Warning
        );
        GUI.color = Color.white;

        EditorGUILayout.Space(10);

        // Draw the default inspector (fields).
        DrawDefaultInspector();

        serializedObject.ApplyModifiedProperties();
    }
}

#endregion

#endif