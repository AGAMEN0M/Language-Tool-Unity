/*
 * ---------------------------------------------------------------------------
 * Description: Component responsible for managing localized texts in the scene.
 *              It synchronizes UI or 3D elements with translated content using
 *              language IDs and applies changes through UnityEvents.
 *              
 * Author: Lucas Gomes Cecchini
 * Pseudonym: AGAMENOM
 * ---------------------------------------------------------------------------
*/

using System.Collections.Generic;
using UnityEngine.Events;
using LanguageTools;
using UnityEngine;

using static LanguageTools.LanguageFileManager;

#if UNITY_EDITOR
using UnityEditor;
using static LanguageTools.Editor.LanguageEditorUtilities;
#endif

/// <summary>
/// Handles localization of UI or 3D texts in the scene by updating content
/// from LanguageSettingsData and applying changes via UnityEvents.
/// </summary>
[AddComponentMenu("Language/3D Object/Language Script")]
public class LanguageScript : MonoBehaviour
{
    #region === Serialized Fields ===

    [Header("Script Components")]
    [SerializeField, Tooltip("Show debug messages when updating texts.")]
    private bool debug = false;

    [Space(5)]

    [SerializeField, Tooltip("List of localized texts with assigned language IDs.")]
    private List<ScriptText> scriptTexts = new() { new() { iD = -9, text = "Test Language Script" } };

    #endregion

    #region === Private Fields ===

    private LanguageSettingsData languageData; // Current language settings.

    #endregion

    #region === Properties ===

    /// <summary>
    /// Gets or sets whether debug messages should be displayed.
    /// </summary>
    public bool DebugLog
    {
        get => debug;
        set => debug = value;
    }

    /// <summary>
    /// Gets or sets the list of localized ScriptText entries.
    /// </summary>
    public List<ScriptText> ScriptTexts
    {
        get => scriptTexts;
        set => scriptTexts = value;
    }

    #endregion

    #region === Unity Events ===

    /// <summary>
    /// Subscribes to the language update event and immediately updates the text.
    /// </summary>
    private void OnEnable()
    {
        LanguageManagerDelegate.OnLanguageUpdate += LanguageUpdate; // Subscribe to the language change event.
        LanguageUpdate(); // Perform an initial update to reflect the current language settings.
    }

    /// <summary>
    /// Unsubscribes from the language update event.
    /// </summary>
    private void OnDisable() => LanguageManagerDelegate.OnLanguageUpdate -= LanguageUpdate;

    #endregion

    #region === Core Methods ===

    /// <summary>
    /// Updates all registered scriptTexts using the selected language settings.
    /// </summary>
    public void LanguageUpdate()
    {
        // Load the current language settings.
        languageData = LoadLanguageSettings();
        if (languageData == null)
        {
            Debug.LogError("LanguageScript: Failed to load LanguageSettingsData.", this);
            return;
        }

        // Iterate through each scriptText entry.
        foreach (var scriptText in scriptTexts)
        {
            // Retrieve the localized text from the language data using the provided ID.
            scriptText.text = GetIDText(languageData.idData, scriptText.iD);

            // If debugging is enabled, log the applied translation.
            if (debug) Debug.LogWarning($"LanguageScript(ID:{scriptText.iD}): {scriptText.text}", this);

            // Apply the localized text to any bound UnityEvent callbacks.
            ApplyUnityEvent(scriptText.targetScripts, scriptText.text);
        }
    }

    #endregion

    #region === Utility Methods ===

    /// <summary>
    /// Invokes all methods in a UnityEvent with the provided string value.
    /// </summary>
    /// <param name="unityEvent">UnityEvent to be invoked.</param>
    /// <param name="value">String value to be passed to the event listeners.</param>
    private void ApplyUnityEvent(UnityEvent<string> unityEvent, string value)
    {
        // Get the number of persistent listeners assigned to this UnityEvent.
        int persistentCalls = unityEvent.GetPersistentEventCount();

        // Iterate over each registered callback.
        for (int i = 0; i < persistentCalls; i++)
        {
            var target = unityEvent.GetPersistentTarget(i);
            var method = unityEvent.GetPersistentMethodName(i);

            // Validate the target and method name before invoking.
            if (target != null && !string.IsNullOrEmpty(method))
            {
                var methodInfo = target.GetType().GetMethod(method);

                // Ensure method accepts a single string parameter before calling it.
                if (methodInfo != null && methodInfo.GetParameters().Length == 1 && methodInfo.GetParameters()[0].ParameterType == typeof(string))
                {
                    methodInfo.Invoke(target, new object[] { value });
                }
            }
        }
    }

    #endregion
}

#if UNITY_EDITOR

#region === Custom Editor ===

/// <summary>
/// Custom inspector for LanguageScript. Provides buttons for importing and editing language settings.
/// </summary>
[CanEditMultipleObjects]
[CustomEditor(typeof(LanguageScript))]
public class LanguageScriptEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // Sync serialized fields with the inspector.
        serializedObject.Update();
        var script = (LanguageScript)target;

        using (new EditorGUI.DisabledScope(targets.Length > 1))
        {
            // Draw an import button and assign click behavior.
            if (GUILayout.Button(new GUIContent("Import Settings", "Imports or updates language entries into the current LanguageScript component based on existing IDs."), CreateCustomButtonStyle(15), GUILayout.Height(30)))
            {
                // Check if any IDs in scriptTexts already exist in the language system.
                bool alreadySaved = script.ScriptTexts.Exists(i => IsIDInLanguageList(i.iD));

                // Ask user whether to overwrite existing IDs.
                if (alreadySaved && !EditorUtility.DisplayDialog("Replace ID", "An ID with this number is already saved. Do you want to replace it?", "Yes", "No"))
                {
                    return;
                }

                // Open the editor window for each entry to allow manual editing.
                foreach (var i in script.ScriptTexts) OpenEditorWindowWithComponent(i.iD, 4, i.text, 0, 0, 0);
            }
        }

        // Draw other default inspector fields.
        EditorGUILayout.Space(5);
        DrawDefaultInspector();

        // Apply property changes back to the serialized object.
        serializedObject.ApplyModifiedProperties();
    }
}

#endregion

#endif