using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor.SceneManagement;
using System.Collections.Generic;

public static class CanvasConverter
{
    [MenuItem("GameObject/Language/Converter/UI/Legacy to LT Legacy")]
    public static void ConverterUiToLT()
    {
        GameObject selectedObject = Selection.activeGameObject; // Get the selected GameObject in the hierarchy.

        if (selectedObject != null)
        {
            // Check for specific UI components on the selected object: Text, Dropdown, Button, Toggle, InputField.
            Text textComponent = selectedObject.GetComponent<Text>();
            Dropdown dropdownComponent = selectedObject.GetComponent<Dropdown>();
            Button buttonComponent = selectedObject.GetComponent<Button>();
            Toggle toggleComponent = selectedObject.GetComponent<Toggle>();
            InputField inputFieldComponent = selectedObject.GetComponent<InputField>();

            if (textComponent != null)
            {
                // Check if the selected object already has the LanguageText component.
                if (textComponent.TryGetComponent<LanguageText>(out _))
                {
                    // If the object already has one of the components, display an error message and return early.
                    Debug.LogError("Selected object already has LanguageText component!");
                    return;
                }

                LanguageText languageTextComponent = textComponent.gameObject.AddComponent<LanguageText>(); // If the object has the Text component, add LanguageText components.
                languageTextComponent.languageText = textComponent; // Set the Text component to the LanguageText component's "languageText" variable.
                Debug.Log("LanguageText component added to Text object successfully."); // Print a message to indicate the success.
                EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene()); // Indicate that the scene has been modified.
            }
            else if (dropdownComponent != null)
            {
                // Check if the selected object already has the LanguageDropdown or AdjustSizeToDropdown component.
                if (dropdownComponent.TryGetComponent<LanguageDropdown>(out _) || dropdownComponent.template.TryGetComponent<AdjustSizeToDropdown>(out _))
                {
                    // If the object already has one of the components, display an error message and return early.
                    Debug.LogError("Selected object already contains LanguageDropdown or AdjustSizeToDropdown component!");
                    return;
                }

                LanguageDropdown languageDropdownComponent = dropdownComponent.gameObject.AddComponent<LanguageDropdown>(); // If the object has the Dropdown component, add LanguageDropdown and AdjustSizeToDropdown components.

                // Set various properties for the LanguageDropdown component based on the Dropdown.
                languageDropdownComponent.languageDropdown = dropdownComponent;
                languageDropdownComponent.itemText = dropdownComponent.itemText;
                languageDropdownComponent.captionText = dropdownComponent.captionText;

                dropdownComponent.template.gameObject.AddComponent<AdjustSizeToDropdown>(); // Adds the "AdjustSizeToDropdown" component directly to the "template" object.

                // Copy the Dropdown options to the LanguageDropdown component's "languageOptions" list.
                List<LanguageOptions> languageSelectedList = new();
                for (int i = 0; i < dropdownComponent.options.Count; i++)
                {
                    LanguageOptions languageOption = new()
                    {
                        text = dropdownComponent.options[i].text,
                        sprite = dropdownComponent.options[i].image
                    };
                    languageSelectedList.Add(languageOption);
                }
                languageDropdownComponent.options = languageSelectedList;

                Debug.Log("LanguageDropdown and AdjustSizeToDropdown component added successfully. Dropdown options copied."); // Print a message to indicate the success.
                EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene()); // Indicate that the scene has been modified.
            }
            else if (buttonComponent != null)
            {
                Text buttonTextComponent = buttonComponent.GetComponentInChildren<Text>(); // Check if the Button object has a child with the Text component.

                if (buttonTextComponent != null)
                {
                    // Checks if the Text object already has either the LanguageText or AdjustRect component.
                    if (buttonComponent.TryGetComponent<LanguageText>(out _) || buttonComponent.TryGetComponent<AdjustRect>(out _))
                    {
                        // If the Text object already has the LanguageText or AdjustRect component, display an error message and return.
                        Debug.LogError("Selected Button object already contains LanguageText or AdjustRect component!");
                        return;
                    }

                    // Add LanguageText and AdjustRect components to the Button's Text and Button objects, respectively.
                    LanguageText languageTextComponent = buttonComponent.gameObject.AddComponent<LanguageText>();
                    AdjustRect adjustRectComponent = buttonComponent.gameObject.AddComponent<AdjustRect>();

                    languageTextComponent.languageText = buttonTextComponent; // Set the Text component to the LanguageText component's "languageText" variable.
                    adjustRectComponent.rectTransform = buttonComponent.GetComponent<RectTransform>(); // Set the RectTransform to the AdjustRect component.
                    languageTextComponent.adjustRectList = new List<AdjustRectList> { new() { adjustRect = adjustRectComponent } }; // Create a list to associate the AdjustRect component with the LanguageText component.
                    Debug.Log("LanguageText and AdjustRect components added to Text and Button objects, respectively."); // Print a success message.
                    EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene()); // Indicate that the scene has been modified.
                }
                else
                {
                    Debug.LogError("Selected Button object does not have Text component!"); // If the Button object doesn't have the Text component, display an error message.
                }
            }
            else if (toggleComponent != null)
            {
                Text toggleTextComponent = toggleComponent.GetComponentInChildren<Text>(); // Check if the Toggle object has a child with the Text component.

                if (toggleTextComponent != null)
                {
                    // Checks if the Text object already has the LanguageText, AdjustRect, or AdjustRect on targetGraphic component.
                    if (toggleComponent.TryGetComponent<LanguageText>(out _) || toggleComponent.TryGetComponent<AdjustRect>(out _) || toggleComponent.targetGraphic.TryGetComponent<AdjustRect>(out _))
                    {
                        // If the Text object already has one of the components, display an error message and return early.
                        Debug.LogError("Selected Toggle object already contains LanguageText, AdjustRect, or AdjustRect on targetGraphic component!");
                        return;
                    }

                    // Add LanguageText and AdjustRect components to the Toggle's Text and Toggle objects, respectively.
                    LanguageText languageTextComponent = toggleComponent.gameObject.AddComponent<LanguageText>();
                    AdjustRect adjustRectComponent = toggleComponent.gameObject.AddComponent<AdjustRect>();
                    AdjustRect targetGraphicAdjustRectComponent = toggleComponent.targetGraphic.gameObject.AddComponent<AdjustRect>();

                    languageTextComponent.languageText = toggleTextComponent; // Set the Text component to the LanguageText component's "languageText" variable.

                    // Set the RectTransforms to the AdjustRect components.
                    adjustRectComponent.rectTransform = toggleComponent.GetComponent<RectTransform>();
                    targetGraphicAdjustRectComponent.rectTransform = toggleComponent.targetGraphic.GetComponent<RectTransform>();

                    // Create a list to associate the AdjustRect components with the LanguageText component.
                    languageTextComponent.adjustRectList = new List<AdjustRectList> 
                    { 
                        new() { adjustRect = adjustRectComponent }, new() { adjustRect = targetGraphicAdjustRectComponent }
                    };

                    Debug.Log("LanguageText and AdjustRect components added to Text and Toggle objects, respectively."); // Print a message to indicate the success.
                    EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene()); // Indicate that the scene has been modified.
                }
                else
                {
                    Debug.LogError("Selected Toggle object does not have Text component!"); // If the Toggle object doesn't have the Text component, display an error message.
                }
            }
            else if (inputFieldComponent != null)
            {
                Text inputFieldTextComponent = inputFieldComponent.textComponent; // Get the Text component from the InputField.

                if (inputFieldTextComponent != null)
                {
                    // Checks if the InputField already has the LanguageTextInputField, AdjustRect, or LanguageText on Placeholder components.
                    if (inputFieldComponent.TryGetComponent<LanguageTextInputField>(out _) || inputFieldComponent.TryGetComponent<AdjustRect>(out _) || inputFieldComponent.placeholder.gameObject.TryGetComponent<LanguageText>(out _))
                    {
                        // If the InputField already has one of the components, display an error message and return early.
                        Debug.LogError("Selected object already contains LanguageTextInputField, AdjustRect, or LanguageText on Placeholder!");
                        return;
                    }

                    // Add LanguageTextInputField, LanguageText, and AdjustRect components to the InputField's Text, Placeholder, and InputField objects, respectively.
                    LanguageTextInputField languageTextInputFieldComponent = inputFieldComponent.gameObject.AddComponent<LanguageTextInputField>();
                    AdjustRect adjustRectComponent = inputFieldComponent.gameObject.AddComponent<AdjustRect>();
                    LanguageText placeholderLanguageTextComponent = inputFieldComponent.placeholder.gameObject.AddComponent<LanguageText>();

                    languageTextInputFieldComponent.textComponent = inputFieldComponent.textComponent; // Set the targetText variable of the LanguageTextInputField component to the Text component.
                    languageTextInputFieldComponent.placeholder = placeholderLanguageTextComponent; // Set the LanguageTextInputField's placeholder Text to the LanguageText component on the InputField's placeholder GameObject.

                    // Set the targetText variable of the LanguageText component to the Placeholder GameObject's Text component.
                    placeholderLanguageTextComponent.languageText = inputFieldComponent.placeholder.gameObject.GetComponent<Text>();
                    adjustRectComponent.rectTransform = inputFieldComponent.GetComponent<RectTransform>();

                    languageTextInputFieldComponent.adjustRectList = new List<AdjustRectList> { new() { adjustRect = adjustRectComponent } }; // Create a list to associate the AdjustRect component with the LanguageTextInputField component.
                    Debug.Log("LanguageTextInputField, LanguageText, and AdjustRect components added successfully."); // Print a message to indicate the success.
                    EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene()); // Indicate that the scene has been modified.
                }
                else
                {
                    Debug.LogError("InputField does not have a Text component!"); // If the InputField doesn't have a Text component, display an error message.
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