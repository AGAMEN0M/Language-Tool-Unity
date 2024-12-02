/*
 * ---------------------------------------------------------------------------
 * Description: This script handles loading and updating localized text 
 *              for language scripts in the Unity scene, enabling multilingual support.
 * Author: Lucas Gomes Cecchini
 * Pseudonym: AGAMENOM
 * ---------------------------------------------------------------------------
*/

using System.Collections.Generic;
using UnityEngine.Events;
using LanguageTools;
using UnityEngine;
using System.IO;

#if UNITY_EDITOR
using LanguageTools.Editor;
using UnityEditor;
#endif

// This component is responsible for loading and updating language scripts in the scene.
[AddComponentMenu("Language/3D Object/Language Script")]
public class LanguageScript : MonoBehaviour
{
    [Header("Settings")]
    // List of script texts associated with this component. Contains the text and its ID.
    public List<ScriptText> scriptTexts = new() { new ScriptText { iD = -9, text = "Test Language Script" } };
    [Space(10)]
    [Header("Automatic Information")]
    [SerializeField] private string selectedFile; // Stores the path of the selected language file.
    [Space(5)]
    [SerializeField] private string saveFile; // Stores the path to the save file.

    // Subscribes to the OnLanguageUpdate event when the object is enabled.
    private void OnEnable()
    {
        LanguageManagerDelegate.OnLanguageUpdate += LanguageUpdate; // Subscribe to language updates.
        LanguageUpdate(); // Perform an initial update when enabled.
    }

    // Unsubscribes from the OnLanguageUpdate event when the object is disabled.
    private void OnDisable()
    {
        LanguageManagerDelegate.OnLanguageUpdate -= LanguageUpdate; // Unsubscribe from language updates.
    }

    // Updates the script by loading the localized text based on the selected file and IDs.
    public void LanguageUpdate()
    {
        saveFile = LanguageFileManager.GetSaveFilePath(); // Retrieve the path to the save file.

        // Check if the save file exists, then load the selected language file from it.
        if (File.Exists(saveFile))
        {
            string json = File.ReadAllText(saveFile);
            var saveData = JsonUtility.FromJson<LanguageSaveData>(json); // Deserialize the save data.
            selectedFile = saveData.selectedFile; // Set the selected file path.
        }
        else
        {
            // If no save file exists, find the default language file path.
            selectedFile = LanguageFileManager.FindDefaultLanguageFilePath();
        }

        ProcessLine(); // Process the loaded lines from the file.
    }

    // Processes each script text in the list, fetching and applying the localized text.
    private void ProcessLine()
    {
        foreach (var scriptText in scriptTexts)
        {
            // Find the localized line of text by its ID and extract the text inside curly braces.
            string text = LanguageFileManager.FindLineByID(selectedFile, scriptText.iD);
            scriptText.text = LanguageFileManager.ExtractTextBetweenBraces(text);

            // Apply the text to the target UnityEvent (scripts connected to this event).
            ApplyUnityEvent(scriptText.targetScripts, scriptText.text);
        }
    }

    // Invokes the UnityEvent with the localized text as a parameter.
    private void ApplyUnityEvent(UnityEvent<string> unityEvent, string value)
    {
        // Get the number of persistent calls (methods) registered with the UnityEvent.
        var persistentCalls = unityEvent.GetPersistentEventCount();

        // Loop through each persistent call and invoke the method if valid.
        for (int i = 0; i < persistentCalls; i++)
        {
            var target = unityEvent.GetPersistentTarget(i); // Get the target object.
            var method = unityEvent.GetPersistentMethodName(i); // Get the method name.

            // If the target and method exist, invoke the method with the localized text.
            if (target != null && !string.IsNullOrEmpty(method))
            {
                var methodInfo = target.GetType().GetMethod(method); // Get method info.

                // Check if the method takes one string parameter and invoke it if valid.
                if (methodInfo != null && methodInfo.GetParameters().Length == 1 && methodInfo.GetParameters()[0].ParameterType == typeof(string))
                {
                    methodInfo.Invoke(target, new object[] { value });
                }
            }
        }
    }
}

#if UNITY_EDITOR
// Custom editor for the LanguageScript component in the Unity editor.
[CustomEditor(typeof(LanguageScript))]
public class LanguageScriptEditor : Editor
{
    // Overrides the default Inspector GUI with custom fields and buttons.
    public override void OnInspectorGUI()
    {
        serializedObject.Update(); // Update the serialized properties of the object.
        var script = (LanguageScript)target; // Reference to the target script.
        LanguageEditorUtilities.DrawReadOnlyMonoScriptField(target); // Draws the read-only field for the script reference.

        EditorGUILayout.Space(5);

        // Button to import the settings for the script texts.
        if (GUILayout.Button("Import Settings", LanguageEditorUtilities.CreateCustomButtonStyle(15), GUILayout.Height(30)))
        {
            bool alreadySaved = false;

            // Check if any of the IDs are already saved.
            foreach (ScriptText i in script.scriptTexts)
            {
                if (LanguageEditorUtilities.IsIDInLanguageList(i.iD))
                {
                    alreadySaved = true;
                    break;
                }
            }

            // If any ID is already saved, prompt the user to confirm if they want to replace it.
            if (alreadySaved)
            {
                if (!EditorUtility.DisplayDialog("Replace ID", "An ID with this number is already saved. Do you want to replace it?", "Yes", "No"))
                {
                    return; // Exit if the user chooses not to replace the ID.
                }
            }

            // Open the editor window for each script text's ID for further modification.
            foreach (ScriptText i in script.scriptTexts)
            {
                LanguageEditorUtilities.OpenEditorWindowWithComponent(i.iD, 4, i.text, 0, 0, 0, true, false, false, false);
            }
        }

        // Property field for the list of script texts.
        EditorGUILayout.PropertyField(serializedObject.FindProperty("scriptTexts"));

        // Display a warning if any of the script text IDs are already saved.
        foreach (ScriptText i in script.scriptTexts)
        {
            if (LanguageEditorUtilities.IsIDInLanguageList(i.iD))
            {
                GUI.color = Color.yellow;
                EditorGUILayout.HelpBox($"There is an ID ({i.iD}) with this number Saved!", MessageType.Warning);
                GUI.color = Color.white;
            }
        }

        // Property fields for displaying the selected file and save file paths.
        EditorGUILayout.PropertyField(serializedObject.FindProperty("selectedFile"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("saveFile"));

        serializedObject.ApplyModifiedProperties(); // Apply any modified properties.
    }
}
#endif