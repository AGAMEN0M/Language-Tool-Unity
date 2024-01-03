using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

public static class SceneConverter
{
    [MenuItem("GameObject/Language/Converter/Scene/Legacy to LT Legacy")]
    public static void ConverterUiToLT()
    {
        GameObject selectedObject = Selection.activeGameObject; // Get the selected GameObject in the hierarchy.

        if (selectedObject != null)
        {
            // Check if the selected object already has the LanguageTextMesh component.
            if (selectedObject.TryGetComponent<LanguageTextMesh>(out _))
            {
                // If the object already has the LanguageTextMesh component, display an error message and return immediately.
                Debug.LogError("The selected object already has the LanguageTextMesh component!");
                return;
            }

            // Check if the selected object has the TextMesh component.
            if (selectedObject.TryGetComponent<TextMesh>(out var textComponent))
            {
                LanguageTextMesh languageTextComponent = selectedObject.AddComponent<LanguageTextMesh>(); // Add the LanguageTextMesh component to the object.
                languageTextComponent.languageTextMesh = textComponent; // Assign the TextMesh component to the LanguageTextMesh component's "targetTextMesh" variable.
                Debug.Log("LanguageTextMesh components added, and Text component assigned!"); // Display a message to indicate successful conversion.
                EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene()); // Mark the scene as dirty to indicate modifications.
            }
            else
            {
                Debug.LogError("The selected object has no TextMesh component!"); // If the object does not have the TextMesh component, display an error message.
            }
        }
        else
        {
            Debug.LogError("No object selected!"); // If no object is selected, display an error message.
        }
    }
}