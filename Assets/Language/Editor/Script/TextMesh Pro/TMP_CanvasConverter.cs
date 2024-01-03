using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public static class TMP_CanvasConverter
{
    [MenuItem("GameObject/Language/Converter/UI/TextMeshPro to LT TextMeshPro")]
    public static void TMP_ConverterUiToLT()
    {
        GameObject selectedObject = Selection.activeGameObject; // Get the selected game object in the hierarchy.

        if (selectedObject != null)
        {
            // Check for specific UI components on the selected object: TMP_Text, TMP_Dropdown, Button, TMP_InputField.
            TMP_Text textComponent = selectedObject.GetComponent<TMP_Text>();
            TMP_Dropdown dropdownComponent = selectedObject.GetComponent<TMP_Dropdown>();
            Button buttonComponent = selectedObject.GetComponent<Button>();
            TMP_InputField inputFieldComponent = selectedObject.GetComponent<TMP_InputField>();

            if (textComponent != null)
            {
                // Check if the selected object already has the TMP_LanguageText component.
                if (selectedObject.TryGetComponent<TMP_LanguageText>(out _))
                {
                    // If the object already has one of the components, display an error message and return early.
                    Debug.LogError("Selected object already has TMP_LanguageText component!");
                    return;
                }

                TMP_LanguageText languageTextComponent = textComponent.gameObject.AddComponent<TMP_LanguageText>(); // If the object has the Text component, add TMP_LanguageText components.
                languageTextComponent.TMP_languageText = textComponent; // Set the TMP_Text component to the TMP_LanguageText component's "TMP_languageText" variable.
                Debug.Log("TMP_LanguageText component added to TMP_Text object successfully."); // Print a message to indicate the success.
                EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene()); // Indicate that the scene has been modified.
            }
            else if (dropdownComponent != null)
            {
                // Check if the selected object already has the TMP_LanguageDropdown or TMP_AdjustSizeToDropdown component.
                if (dropdownComponent.TryGetComponent<TMP_LanguageDropdown>(out _) || dropdownComponent.template.TryGetComponent<TMP_AdjustSizeToDropdown>(out _))
                {
                    // If the object already has one of the components, display an error message and return early.
                    Debug.LogError("Selected object already contains TMP_LanguageDropdown or TMP_AdjustSizeToDropdown component!");
                    return;
                }

                TMP_LanguageDropdown languageDropdownComponent = dropdownComponent.gameObject.AddComponent<TMP_LanguageDropdown>(); // If the object has the Dropdown component, add TMP_LanguageDropdown and TMP_AdjustSizeToDropdown components.

                // Set various properties for the TMP_LanguageDropdown component based on the TMP_Dropdown.
                languageDropdownComponent.TMP_languageDropdown = dropdownComponent;
                languageDropdownComponent.TMP_itemText = dropdownComponent.itemText;
                languageDropdownComponent.TMP_captionText = dropdownComponent.captionText;

                dropdownComponent.template.gameObject.AddComponent<TMP_AdjustSizeToDropdown>(); // Adds the "TMP_AdjustSizeToDropdown" component directly to the "template" object.

                // Copy the TMP_Dropdown options to the TMP_LanguageDropdown component's "TMP_languageOptions" list.
                List<TMP_LanguageOptions> languageSelectedList = new();
                for (int i = 0; i < dropdownComponent.options.Count; i++)
                {
                    TMP_LanguageOptions languageOption = new()
                    {
                        text = dropdownComponent.options[i].text,
                        sprite = dropdownComponent.options[i].image
                    };
                    languageSelectedList.Add(languageOption);
                }
                languageDropdownComponent.options = languageSelectedList;

                Debug.Log("TMP_LanguageDropdown and TMP_AdjustSizeToDropdown component added successfully. TMP_Dropdown options copied."); // Print a message to indicate the success.
                EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene()); // Indicate that the scene has been modified.                
            }
            else if (buttonComponent != null)
            {
                TMP_Text buttonTextComponent = buttonComponent.GetComponentInChildren<TMP_Text>(); // Check if the Button object has a child with the TMP_Text component.

                if (buttonTextComponent != null)
                {
                    // Checks if the TMP_Text object already has either the TMP_LanguageText or AdjustRect component.
                    if (buttonComponent.TryGetComponent<TMP_LanguageText>(out _) || buttonComponent.TryGetComponent<AdjustRect>(out _))
                    {
                        // If the TMP_Text object already has the TMP_LanguageText or AdjustRect component, display an error message and return.
                        Debug.LogError("Selected Button object already contains TMP_LanguageText or AdjustRect component!");
                        return;
                    }

                    // Add TMP_LanguageText and AdjustRect components to the Button's TMP_Text and Button objects, respectively.
                    TMP_LanguageText languageTextComponent = buttonComponent.gameObject.AddComponent<TMP_LanguageText>();
                    AdjustRect adjustRectComponent = buttonComponent.gameObject.AddComponent<AdjustRect>();

                    languageTextComponent.TMP_languageText = buttonTextComponent; // Set the TMP_Text component to the TMP_LanguageText component's "TMP_languageText" variable.
                    adjustRectComponent.rectTransform = buttonComponent.GetComponent<RectTransform>(); // Set the RectTransform to the AdjustRect component.
                    languageTextComponent.adjustRectList = new List<AdjustRectList> { new() { adjustRect = adjustRectComponent } }; // Create a list to associate the AdjustRect component with the TMP_LanguageText component.
                    Debug.Log("TMP_LanguageText and AdjustRect components added to Text and Button objects, respectively."); // Print a success message.
                    EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene()); // Indicate that the scene has been modified.
                }
                else
                {
                    Debug.LogError("Selected Button object does not have TMP_Text component!"); // If the Button object doesn't have the TMP_Text component, display an error message.
                }
            }            
            else if (inputFieldComponent != null)
            {
                TMP_Text inputFieldTextComponent = inputFieldComponent.textComponent; // Get the TMP_Text component from the InputField.

                if (inputFieldTextComponent != null)
                {
                    // Checks if the TMP_InputField already has the TMP_LanguageTextInputField, AdjustRect, or TMP_LanguageText on Placeholder components.
                    if (inputFieldComponent.TryGetComponent<TMP_LanguageTextInputField>(out _) || inputFieldComponent.TryGetComponent<AdjustRect>(out _) || inputFieldComponent.placeholder.gameObject.TryGetComponent<TMP_LanguageText>(out _))
                    {
                        // If the TMP_InputField already has one of the components, display an error message and return early.
                        Debug.LogError("Selected object already contains TMP_LanguageTextInputField, AdjustRect, or TMP_LanguageText on Placeholder!");
                        return;
                    }

                    // Add TMP_LanguageTextInputField, TMP_LanguageText, and AdjustRect components to the TMP_InputField's TMP_Text, Placeholder, and TMP_InputField objects, respectively.
                    TMP_LanguageTextInputField languageTextInputFieldComponent = inputFieldComponent.gameObject.AddComponent<TMP_LanguageTextInputField>();
                    AdjustRect adjustRectComponent = inputFieldComponent.gameObject.AddComponent<AdjustRect>();
                    TMP_LanguageText placeholderLanguageTextComponent = inputFieldComponent.placeholder.gameObject.AddComponent<TMP_LanguageText>();

                    languageTextInputFieldComponent.TMP_textComponent = inputFieldComponent.textComponent; // Set the TMP_targetText variable of the TMP_LanguageTextInputField component to the Text component.
                    languageTextInputFieldComponent.TMP_placeholder = placeholderLanguageTextComponent; // Set the TMP_LanguageTextInputField's placeholder Text to the TMP_LanguageText component on the InputField's placeholder GameObject.

                    // Set the TMP_targetText variable of the TMP_LanguageText component to the TMP_Placeholder GameObject's TMP_Text component.
                    placeholderLanguageTextComponent.TMP_languageText = inputFieldComponent.placeholder.gameObject.GetComponent<TMP_Text>();
                    adjustRectComponent.rectTransform = inputFieldComponent.GetComponent<RectTransform>();

                    languageTextInputFieldComponent.adjustRectList = new List<AdjustRectList> { new() { adjustRect = adjustRectComponent } }; // Create a list to associate the AdjustRect component with the TMP_LanguageTextInputField component.
                    Debug.Log("TMP_LanguageTextInputField, TMP_LanguageText, and AdjustRect components added successfully."); // Print a message to indicate the success.
                    EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene()); // Indicate that the scene has been modified.
                }
                else
                {
                    Debug.LogError("TMP_InputField does not have a TMP_Text component!"); // If the TMP_InputField doesn't have a TMP_Text component, display an error message.
                }
            }
            else
            {
                Debug.LogError("The selected object is not valid!"); // If the object doesn't contain any valid components for conversion, display an error message.
            }
        }
        else
        {
            Debug.LogError("No object selected!"); // If no object is selected, display an error message.
        }
    }
}