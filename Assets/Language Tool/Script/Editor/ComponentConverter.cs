/*
 * ---------------------------------------------------------------------------
 * Description: A Unity editor utility that converts various UI and 3D 
 *              components into LanguageTool compatible versions. It scans the selected 
 *              GameObject in the hierarchy for components such as Button, Toggle, 
 *              Dropdown, InputField, Text, TMP_Dropdown, TMP_InputField, TMP_Text, 
 *              Image, RawImage, TextMesh, MeshRenderer, and AudioSource. If a component 
 *              is found, it performs the conversion, adding a new compatible component 
 *              while preserving the original functionality. The script ensures that 
 *              duplicate conversions are avoided by checking for existing LanguageTool 
 *              components, and logs success or error messages accordingly.
 * Author: Lucas Gomes Cecchini
 * Pseudonym: AGAMENOM
 * ---------------------------------------------------------------------------
*/

using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using UnityEditor;
using TMPro;

public class ComponentConverter
{
    [MenuItem("GameObject/Language/Converter to Language Tool")]
    public static void ConvertComponents()
    {
        GameObject selectedObject = Selection.activeGameObject; // Get the currently selected GameObject in the hierarchy.

        // Check if a GameObject is selected.
        if (selectedObject != null)
        {
            // Retrieve UI components from the selected GameObject.
            var buttonComponent = selectedObject.GetComponent<Button>();
            var toggleComponent = selectedObject.GetComponent<Toggle>();
            var dropdownComponent = selectedObject.GetComponent<Dropdown>();
            var inputFieldComponent = selectedObject.GetComponent<InputField>();
            var textComponent = selectedObject.GetComponent<Text>();
            var dropdownComponentTMP = selectedObject.GetComponent<TMP_Dropdown>();
            var inputFieldComponentTMP = selectedObject.GetComponent<TMP_InputField>();
            var textComponentTMP = selectedObject.GetComponent<TMP_Text>();
            var imageComponent = selectedObject.GetComponent<Image>();
            var rawImage = selectedObject.GetComponent<RawImage>();

            // Retrieve 3D components from the selected GameObject.
            var textMesh = selectedObject.GetComponent<TextMesh>();
            var meshRenderer = selectedObject.GetComponent<MeshRenderer>();
            var audioSource = selectedObject.GetComponent<AudioSource>();

            // Convert each component if it exists.
            if (buttonComponent != null) ConvertButton(buttonComponent);
            if (toggleComponent != null) ConvertToggle(toggleComponent);
            if (dropdownComponent != null) ConvertDropdown(dropdownComponent);
            if (inputFieldComponent != null) ConvertInputField(inputFieldComponent);
            if (textComponent != null) ConvertText(textComponent);
            if (dropdownComponentTMP != null) ConvertTMPDropdown(dropdownComponentTMP);
            if (inputFieldComponentTMP != null) ConvertTMPInputField(inputFieldComponentTMP);
            if (rawImage != null) ConvertRawImage(rawImage);
            if (textMesh != null) ConvertTextMesh(textMesh);
            if (audioSource != null) ConvertAudioSource(audioSource);

            // Convert Image component only if no other UI components requiring conversion are present.
            if (imageComponent != null && buttonComponent == null && inputFieldComponent == null && inputFieldComponentTMP == null && dropdownComponent == null && dropdownComponentTMP == null)
            {
                ConvertImage(imageComponent);
            }

            // Convert TMP_Text based on the presence of a MeshRenderer.
            if (textComponentTMP != null && meshRenderer == null)
            {
                ConvertTMPText(textComponentTMP);
            }

            if (textComponentTMP != null && meshRenderer != null)
            {
                ConvertMeshRenderer(meshRenderer, textComponentTMP);
            }
        }
        else
        {
            Debug.LogError("No GameObject selected. Please select a GameObject in the hierarchy to convert.");
        }
    }

    // Convert the Button component to a LanguageTool compatible version.
    private static void ConvertButton(Button button)
    {
        ConvertTextComponent(button, () =>
        {
            Debug.LogError("The Button has no text to convert.", button.gameObject);
        });
    }

    // Convert the Toggle component to a LanguageTool compatible version.
    private static void ConvertToggle(Toggle toggle)
    {
        ConvertTextComponent(toggle, () =>
        {
            Debug.LogError("The Toggle has no text to convert.", toggle.gameObject);
        });
    }

    // Generic method to convert any UI component that has a text representation.
    private static void ConvertTextComponent<T>(T uiComponent, System.Action logError) where T : Component
    {
        // Get Text or TMP_Text component from the children of the UI component.
        Text textComponent = uiComponent.GetComponentInChildren<Text>();
        TMP_Text tmpTextComponent = uiComponent.GetComponentInChildren<TMP_Text>();

        // Convert the text component if found.
        if (textComponent != null)
        {
            ConvertText(textComponent);
        }
        else if (tmpTextComponent != null)
        {
            ConvertTMPText(tmpTextComponent);
        }
        else
        {
            logError(); // Log error if no text component is found.
        }
    }

    // Convert the Dropdown component to a LanguageTool compatible version.
    private static void ConvertDropdown(Dropdown dropdown)
    {
        // Check if the LanguageDropdown component is already attached to the GameObject.
        if (dropdown.TryGetComponent<LanguageDropdown>(out _))
        {
            Debug.LogError("The selected object already has the LanguageDropdown component!", dropdown.gameObject);
            return; // Exit if already converted.
        }

        // Check if the AdjustSizeToDropdown component is already attached to the template.
        if (dropdown.template.TryGetComponent<AdjustSizeToDropdown>(out _))
        {
            Debug.LogError("The selected object already has the AdjustSizeToDropdown component!", dropdown.template.gameObject);
            return; // Exit if already converted.
        }

        // Add AdjustSizeToDropdown component to the dropdown template.
        Undo.AddComponent<AdjustSizeToDropdown>(dropdown.template.gameObject);

        // Create a new LanguageDropdown component and assign the dropdown.
        LanguageDropdown LanguageDropdownComponent = Undo.AddComponent<LanguageDropdown>(dropdown.gameObject);
        LanguageDropdownComponent.dropdown = dropdown;

        // Create a list of LanguageOptions from the dropdown options.
        List<LanguageOptions> optionsList = new();
        foreach (var option in dropdown.options)
        {
            optionsList.Add(new LanguageOptions
            {
                text = option.text,
                sprite = option.image
            });
        }
        LanguageDropdownComponent.options = optionsList; // Assign options to the LanguageDropdown component.

        // Register the undo action for creating the LanguageDropdown component.
        Undo.RegisterCreatedObjectUndo(LanguageDropdownComponent, "Add LanguageDropdown");

        Debug.Log("Dropdown converted.", dropdown.gameObject); // Log success message.
    }

    // Convert the InputField component to a LanguageTool compatible version.
    private static void ConvertInputField(InputField inputField)
    {
        // Check if the LanguageTextInputField component is already attached to the GameObject.
        if (inputField.TryGetComponent<LanguageTextInputField>(out _))
        {
            Debug.LogError("The selected object already has the LanguageTextInputField component!", inputField.gameObject);
            return; // Exit if already converted.
        }

        // Create a new LanguageTextInputField component and assign the inputField.
        LanguageTextInputField LanguageTextInputFieldComponent = Undo.AddComponent<LanguageTextInputField>(inputField.gameObject);
        LanguageTextInputFieldComponent.inputField = inputField;

        // Register the undo action for creating the LanguageTextInputField component.
        Undo.RegisterCreatedObjectUndo(LanguageTextInputFieldComponent, "Add LanguageTextInputField");

        Debug.Log("InputField converted.", inputField.gameObject); // Log success message.
    }

    // Convert the Text component to a LanguageTool compatible version.
    private static void ConvertText(Text text)
    {
        // Check if the LanguageText component is already attached to the GameObject.
        if (text.TryGetComponent<LanguageText>(out _))
        {
            Debug.LogError("The selected object already has the LanguageText component!", text.gameObject);
            return; // Exit if already converted.
        }

        // Create a new LanguageText component and assign the Text component.
        LanguageText languageTextComponent = Undo.AddComponent<LanguageText>(text.gameObject);
        languageTextComponent.text = text;

        // Register the undo action for creating the LanguageText component.
        Undo.RegisterCreatedObjectUndo(languageTextComponent, "Add LanguageText");

        Debug.Log("Text converted.", text.gameObject); // Log success message.
    }

    // Convert the TMP_Dropdown component to a LanguageTool compatible version.
    private static void ConvertTMPDropdown(TMP_Dropdown tmpDropdown)
    {
        // Check if the LanguageDropdownTMP component is already attached to the GameObject.
        if (tmpDropdown.TryGetComponent<LanguageDropdownTMP>(out _))
        {
            Debug.LogError("The selected object already has the LanguageDropdownTMP component!", tmpDropdown.gameObject);
            return; // Exit if already converted.
        }

        // Check if the AdjustSizeToDropdownTMP component is already attached to the template.
        if (tmpDropdown.template.TryGetComponent<AdjustSizeToDropdownTMP>(out _))
        {
            Debug.LogError("The selected object already has the AdjustSizeToDropdownTMP component!", tmpDropdown.template.gameObject);
            return; // Exit if already converted.
        }

        // Add AdjustSizeToDropdownTMP component to the dropdown template.
        Undo.AddComponent<AdjustSizeToDropdownTMP>(tmpDropdown.template.gameObject);

        // Create a new LanguageDropdownTMP component and assign the TMP_Dropdown.
        LanguageDropdownTMP LanguageDropdownComponent = Undo.AddComponent<LanguageDropdownTMP>(tmpDropdown.gameObject);
        LanguageDropdownComponent.dropdown = tmpDropdown;

        // Create a list of LanguageOptions from the TMP_Dropdown options.
        List<LanguageOptions> optionsList = new();
        foreach (var option in tmpDropdown.options)
        {
            optionsList.Add(new LanguageOptions
            {
                text = option.text,
                sprite = option.image
            });
        }
        LanguageDropdownComponent.options = optionsList; // Assign options to the LanguageDropdownTMP component.

        // Register the undo action for creating the LanguageDropdownTMP component.
        Undo.RegisterCreatedObjectUndo(LanguageDropdownComponent, "Add LanguageDropdownTMP");

        Debug.Log("TMP_Dropdown converted.", tmpDropdown.gameObject); // Log success message.
    }

    // Convert the TMP_InputField component to a LanguageTool compatible version.
    private static void ConvertTMPInputField(TMP_InputField tmpInputField)
    {
        // Check if the LanguageTextInputFieldTMP component is already attached to the GameObject.
        if (tmpInputField.TryGetComponent<LanguageTextInputFieldTMP>(out _))
        {
            Debug.LogError("The selected object already has the LanguageTextInputFieldTMP component!", tmpInputField.gameObject);
            return; // Exit if already converted.
        }

        // Create a new LanguageTextInputFieldTMP component and assign the TMP_InputField.
        var LanguageTextInputFieldComponent = Undo.AddComponent<LanguageTextInputFieldTMP>(tmpInputField.gameObject);
        LanguageTextInputFieldComponent.inputField = tmpInputField;

        // Register the undo action for creating the LanguageTextInputFieldTMP component.
        Undo.RegisterCreatedObjectUndo(LanguageTextInputFieldComponent, "Add LanguageTextInputFieldTMP");

        Debug.Log("TMP_InputField converted.", tmpInputField.gameObject); // Log success message.
    }

    // Convert the TMP_Text component to a LanguageTool compatible version.
    private static void ConvertTMPText(TMP_Text tmpText)
    {
        // Check if the LanguageTextTMP component is already attached to the GameObject.
        if (tmpText.TryGetComponent<LanguageTextTMP>(out _))
        {
            Debug.LogError("The selected object already has the LanguageTextTMP component!", tmpText.gameObject);
            return; // Exit if already converted.
        }

        // Create a new LanguageTextTMP component and assign the TMP_Text component.
        LanguageTextTMP languageTextComponent = Undo.AddComponent<LanguageTextTMP>(tmpText.gameObject);
        languageTextComponent.text = tmpText;

        // Register the undo action for creating the LanguageTextTMP component.
        Undo.RegisterCreatedObjectUndo(languageTextComponent, "Add LanguageTextTMP");

        Debug.Log("TMP_Text converted.", tmpText.gameObject); // Log success message.
    }

    // Convert the Image component to a LanguageTool compatible version.
    private static void ConvertImage(Image image)
    {
        // Check if the LanguageImage component is already attached to the GameObject.
        if (image.TryGetComponent<LanguageImage>(out _))
        {
            Debug.LogError("The selected object already has the LanguageImage component!", image.gameObject);
            return; // Exit if already converted.
        }

        // Create a new LanguageImage component and assign the Image component.
        LanguageImage languageImageComponent = Undo.AddComponent<LanguageImage>(image.gameObject);
        languageImageComponent.image = image;

        // Register the undo action for creating the LanguageImage component.
        Undo.RegisterCreatedObjectUndo(languageImageComponent, "Add LanguageImage");

        Debug.Log("Image converted.", image.gameObject); // Log success message.
    }

    // Convert the RawImage component to a LanguageTool compatible version.
    private static void ConvertRawImage(RawImage rawImage)
    {
        // Check if the LanguageRawImage component is already attached to the GameObject.
        if (rawImage.TryGetComponent<LanguageRawImage>(out _))
        {
            Debug.LogError("The selected object already has the LanguageRawImage component!", rawImage.gameObject);
            return; // Exit if already converted.
        }

        // Create a new LanguageRawImage component and assign the RawImage component.
        LanguageRawImage languageRawImageComponent = Undo.AddComponent<LanguageRawImage>(rawImage.gameObject);
        languageRawImageComponent.rawImage = rawImage;

        // Register the undo action for creating the LanguageRawImage component.
        Undo.RegisterCreatedObjectUndo(languageRawImageComponent, "Add LanguageRawImage");

        Debug.Log("RawImage converted.", rawImage.gameObject); // Log success message.
    }

    // Convert the TextMesh component to a LanguageTool compatible version.
    private static void ConvertTextMesh(TextMesh textMesh)
    {
        // Check if the LanguageTextMesh component is already attached to the GameObject.
        if (textMesh.TryGetComponent<LanguageTextMesh>(out _))
        {
            Debug.LogError("The selected object already has the LanguageTextMesh component!", textMesh.gameObject);
            return; // Exit if already converted.
        }

        // Create a new LanguageTextMesh component and assign the TextMesh component.
        LanguageTextMesh languageTextComponent = Undo.AddComponent<LanguageTextMesh>(textMesh.gameObject);
        languageTextComponent.text = textMesh;

        // Register the undo action for creating the LanguageTextMesh component.
        Undo.RegisterCreatedObjectUndo(languageTextComponent, "Add LanguageTextMesh");

        Debug.Log("TextMesh converted.", textMesh.gameObject); // Log success message.
    }

    // Convert the MeshRenderer component with associated TMP_Text to a LanguageTool compatible version.
    private static void ConvertMeshRenderer(MeshRenderer meshRenderer, TMP_Text textComponent)
    {
        // Check if the LanguageTextMeshTMP component is already attached to the GameObject.
        if (meshRenderer.TryGetComponent<LanguageTextMeshTMP>(out _))
        {
            Debug.LogError("The selected object already has the LanguageTextMeshTMP component!", meshRenderer.gameObject);
            return; // Exit if already converted.
        }

        // Create a new LanguageTextMeshTMP component and assign the TMP_Text component.
        LanguageTextMeshTMP languageTextComponent = Undo.AddComponent<LanguageTextMeshTMP>(meshRenderer.gameObject);
        languageTextComponent.text = textComponent;

        // Register the undo action for creating the LanguageTextMeshTMP component.
        Undo.RegisterCreatedObjectUndo(languageTextComponent, "Add LanguageTextMeshTMP");

        Debug.Log("TMP_TextMesh converted.", meshRenderer.gameObject); // Log success message.
    }

    // Convert the AudioSource component to a LanguageTool compatible version.
    private static void ConvertAudioSource(AudioSource audioSource)
    {
        // Check if the LanguageAudioPlayer component is already attached to the GameObject.
        if (audioSource.TryGetComponent<LanguageAudioPlayer>(out _))
        {
            Debug.LogError("The selected object already has the LanguageAudioPlayer component!", audioSource.gameObject);
            return; // Exit if already converted.
        }

        // Create a new LanguageAudioPlayer component and assign the AudioSource component.
        LanguageAudioPlayer languageAudioPlayerComponent = Undo.AddComponent<LanguageAudioPlayer>(audioSource.gameObject);
        languageAudioPlayerComponent.audioSource = audioSource;

        // Register the undo action for creating the LanguageAudioPlayer component.
        Undo.RegisterCreatedObjectUndo(languageAudioPlayerComponent, "Add LanguageAudioPlayer");

        Debug.Log("AudioSource converted.", audioSource.gameObject); // Log success message.
    }
}