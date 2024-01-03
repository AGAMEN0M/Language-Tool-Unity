using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using TMPro;

public static class TMP_SceneConverter
{
    [MenuItem("GameObject/Language/Converter/Scene/TextMeshPro to LT TextMeshPro")]
    public static void TMP_ConverterUiToLT()
    {
        GameObject selectedObject = Selection.activeGameObject; // Get the selected GameObject in the hierarchy.

        if (selectedObject != null)
        {
            // Check if the selected object already has the TMP_LanguageTextMesh component.
            if (selectedObject.TryGetComponent<TMP_LanguageTextMesh>(out _))
            {
                // If the object already has the TMP_LanguageTextMesh component, display an error message and return immediately.
                Debug.LogError("The selected object already has the TMP_LanguageTextMesh component!");
                return;
            }

            // Check if the selected object has the TMP_Text component (TextMeshPro component).
            if (selectedObject.TryGetComponent<TMP_Text>(out var textComponent))
            {
                TMP_LanguageTextMesh languageTextComponent = selectedObject.AddComponent<TMP_LanguageTextMesh>(); // Add the LanguageTextMesh component to the object.
                languageTextComponent.TMP_languageTextMesh = textComponent; // Assign the TextMesh component to the LanguageTextMesh component's "targetTextMesh" variable.
                Debug.Log("TMP_LanguageTextMesh components added, and TMP_Text component assigned!"); // Display a message to indicate successful conversion.
                EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene()); // Mark the scene as dirty to indicate modifications.
            }
            else
            {
                Debug.LogError("The selected object has no TMP_Text component (TextMeshPro component)!"); // If the object does not have the TMP_Text component, display an error message.
            }
        }
        else
        {
            Debug.LogError("No object selected!"); // If no object is selected, display an error message.
        }
    }
}