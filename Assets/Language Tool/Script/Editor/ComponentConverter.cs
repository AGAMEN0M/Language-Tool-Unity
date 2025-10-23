/*
 * ---------------------------------------------------------------------------
 * Description: Unity editor tool that converts standard UI and 3D components
 *              (e.g., Text, Image, TMP components) into LanguageTool-compatible
 *              components. Ensures no duplicates are added, handles Undo
 *              registration, and logs conversion results for user feedback.
 *              
 * Author: Lucas Gomes Cecchini
 * Pseudonym: AGAMENOM
 * ---------------------------------------------------------------------------
*/

using System.Collections.Generic;
using UnityEngine.UI;
using LanguageTools;
using UnityEngine;
using UnityEditor;
using TMPro;

public class ComponentConverter
{
    #region === Variables ===

    private static int callCount = 0; // Tracks the number of times the conversion method has been called during a multi-object selection.
    private static int expectedCalls = 0; // Stores the expected number of calls to ensure conversion is only triggered once per selection.

    #endregion

    #region === Entry Point ===

    /// <summary>
    /// Entry point to convert selected GameObjects' components into LanguageTool-compatible components.
    /// </summary>
    [MenuItem("GameObject/Language/Converter to Language Tool", false, 3)]
    public static void ConvertComponents()
    {
        var selectedObjects = Selection.gameObjects;

        // Set expected call count only on first method call.
        if (callCount == 0) expectedCalls = selectedObjects.Length;

        callCount++;
        if (callCount < expectedCalls) return; // Defer execution until all expected calls are made.
        callCount = 0; // Reset for next conversion session.

        if (selectedObjects.Length == 0)
        {
            Debug.LogError("No GameObject selected. Please select a GameObject in the hierarchy to convert.");
            return;
        }

        foreach (var obj in selectedObjects) ObjectAnalysis(obj);
    }

    #endregion

    #region === Object Analysis ===

    /// <summary>
    /// Analyzes the selected GameObject and applies component conversion if applicable.
    /// </summary>
    /// <param name="selectedObject">The GameObject to be analyzed.</param>
    private static void ObjectAnalysis(GameObject selectedObject)
    {
        if (selectedObject == null) return;

        // Check for and convert various supported UI components.
        if (selectedObject.TryGetComponent(out Text text)) ConvertText(text);
        if (selectedObject.TryGetComponent(out Button button)) ConvertButton(button);
        if (selectedObject.TryGetComponent(out Toggle toggle)) ConvertToggle(toggle);
        if (selectedObject.TryGetComponent(out RawImage rawImage)) ConvertRawImage(rawImage);
        if (selectedObject.TryGetComponent(out TextMesh textMesh)) ConvertTextMesh(textMesh);
        if (selectedObject.TryGetComponent(out Dropdown dropdown)) ConvertDropdown(dropdown);
        if (selectedObject.TryGetComponent(out InputField inputField)) ConvertInputField(inputField);
        if (selectedObject.TryGetComponent(out AudioSource audioSource)) ConvertAudioSource(audioSource);
        if (selectedObject.TryGetComponent(out TMP_Dropdown tmpDropdown)) ConvertTMPDropdown(tmpDropdown);
        if (selectedObject.TryGetComponent(out TMP_InputField tmpInputField)) ConvertTMPInputField(tmpInputField);

        // Fallback for Image and TMP_Text/MeshRenderer if no specialized components were found.
        if (button == null && inputField == null && tmpInputField == null && dropdown == null && tmpDropdown == null)
        {
            if (selectedObject.TryGetComponent(out Image image)) ConvertImage(image);

            var tmpText = selectedObject.GetComponent<TMP_Text>();
            var meshRenderer = selectedObject.GetComponent<MeshRenderer>();

            // Decide whether to convert TMP_Text or MeshRenderer + TMP_Text.
            if (tmpText != null && meshRenderer == null) ConvertTMPText(tmpText);
            else if (tmpText != null) ConvertMeshRenderer(meshRenderer, tmpText);
        }
    }

    #endregion

    #region === UI Component Converters ===

    /// <summary>
    /// Converts a Button by converting its child Text or TMP_Text.
    /// </summary>
    private static void ConvertButton(Button button) => ConvertTextComponent(button, () => Debug.LogError("The Button has no text to convert.", button.gameObject));

    /// <summary>
    /// Converts a Toggle by converting its child Text or TMP_Text.
    /// </summary>
    private static void ConvertToggle(Toggle toggle) => ConvertTextComponent(toggle, () => Debug.LogError("The Toggle has no text to convert.", toggle.gameObject));

    /// <summary>
    /// Converts any UI component that may have a child Text or TMP_Text.
    /// </summary>
    private static void ConvertTextComponent<T>(T uiComponent, System.Action logError) where T : Component
    {
        // Try converting a child Text or TMP_Text.
        if (uiComponent.GetComponentInChildren<Text>() is Text text) ConvertText(text);
        else if (uiComponent.GetComponentInChildren<TMP_Text>() is TMP_Text tmp) ConvertTMPText(tmp);
        else logError(); // Log if neither text type was found.
    }

    /// <summary>
    /// Converts a standard Unity Text component to LanguageText.
    /// </summary>
    private static void ConvertText(Text text)
    {
        if (text.TryGetComponent<LanguageText>(out _))
        {
            Debug.LogError("The selected object already has the LanguageText component!", text.gameObject);
            return;
        }

        // Add LanguageText component and assign reference.
        var component = Undo.AddComponent<LanguageText>(text.gameObject);
        component.TextComponent = text;

        Undo.RegisterCreatedObjectUndo(component, "Add LanguageText");
        Debug.Log("Text converted.", text.gameObject);
    }

    /// <summary>
    /// Converts an Image component to LanguageImage.
    /// </summary>
    private static void ConvertImage(Image image)
    {
        if (image.TryGetComponent<LanguageImage>(out _))
        {
            Debug.LogError("The selected object already has the LanguageImage component!", image.gameObject);
            return;
        }

        // Add LanguageImage component and assign reference.
        var component = Undo.AddComponent<LanguageImage>(image.gameObject);
        component.Image = image;

        Undo.RegisterCreatedObjectUndo(component, "Add LanguageImage");
        Debug.Log("Image converted.", image.gameObject);
    }

    /// <summary>
    /// Converts a RawImage component to LanguageRawImage.
    /// </summary>
    private static void ConvertRawImage(RawImage rawImage)
    {
        if (rawImage.TryGetComponent<LanguageRawImage>(out _))
        {
            Debug.LogError("The selected object already has the LanguageRawImage component!", rawImage.gameObject);
            return;
        }

        var component = Undo.AddComponent<LanguageRawImage>(rawImage.gameObject);
        component.RawImage = rawImage;

        Undo.RegisterCreatedObjectUndo(component, "Add LanguageRawImage");
        Debug.Log("RawImage converted.", rawImage.gameObject);
    }

    /// <summary>
    /// Converts a Dropdown component to LanguageDropdown and adds AdjustSizeToDropdown.
    /// </summary>
    private static void ConvertDropdown(Dropdown dropdown)
    {
        if (dropdown.TryGetComponent<LanguageDropdown>(out _))
        {
            Debug.LogError("The selected object already has the LanguageDropdown component!", dropdown.gameObject);
            return;
        }

        if (dropdown.template.TryGetComponent<AdjustSizeToDropdown>(out _))
        {
            Debug.LogError("The selected object already has the AdjustSizeToDropdown component!", dropdown.template.gameObject);
            return;
        }

        // Add sizing helper to template object.
        Undo.AddComponent<AdjustSizeToDropdown>(dropdown.template.gameObject);
        var component = Undo.AddComponent<LanguageDropdown>(dropdown.gameObject);
        component.Dropdown = dropdown;

        // Convert each dropdown option to LanguageOptions.
        List<LanguageOptions> optionsList = new();
        foreach (var option in dropdown.options)
            optionsList.Add(new() { text = option.text, sprite = option.image });

        component.Options = optionsList;

        Undo.RegisterCreatedObjectUndo(component, "Add LanguageDropdown");
        Debug.Log("Dropdown converted.", dropdown.gameObject);
    }

    #endregion

    #region === TMP Converters ===

    /// <summary>
    /// Converts a TMP_Dropdown to LanguageDropdownTMP and adds AdjustSizeToDropdownTMP.
    /// </summary>
    private static void ConvertTMPDropdown(TMP_Dropdown tmpDropdown)
    {
        if (tmpDropdown.TryGetComponent<LanguageDropdownTMP>(out _))
        {
            Debug.LogError("The selected object already has the LanguageDropdownTMP component!", tmpDropdown.gameObject);
            return;
        }

        if (tmpDropdown.template.TryGetComponent<AdjustSizeToDropdownTMP>(out _))
        {
            Debug.LogError("The selected object already has the AdjustSizeToDropdownTMP component!", tmpDropdown.template.gameObject);
            return;
        }

        Undo.AddComponent<AdjustSizeToDropdownTMP>(tmpDropdown.template.gameObject);
        var component = Undo.AddComponent<LanguageDropdownTMP>(tmpDropdown.gameObject);
        component.Dropdown = tmpDropdown;

        List<LanguageOptions> optionsList = new();
        foreach (var option in tmpDropdown.options)
            optionsList.Add(new() { text = option.text, sprite = option.image });

        component.Options = optionsList;

        Undo.RegisterCreatedObjectUndo(component, "Add LanguageDropdownTMP");
        Debug.Log("TMP_Dropdown converted.", tmpDropdown.gameObject);
    }

    /// <summary>
    /// Converts an InputField to LanguageTextInputField.
    /// </summary>
    private static void ConvertInputField(InputField inputField)
    {
        if (inputField.TryGetComponent<LanguageTextInputField>(out _))
        {
            Debug.LogError("The selected object already has the LanguageTextInputField component!", inputField.gameObject);
            return;
        }

        var component = Undo.AddComponent<LanguageTextInputField>(inputField.gameObject);
        component.InputField = inputField;

        Undo.RegisterCreatedObjectUndo(component, "Add LanguageTextInputField");
        Debug.Log("InputField converted.", inputField.gameObject);
    }

    /// <summary>
    /// Converts a TMP_InputField to LanguageTextInputFieldTMP.
    /// </summary>
    private static void ConvertTMPInputField(TMP_InputField tmpInputField)
    {
        if (tmpInputField.TryGetComponent<LanguageTextInputFieldTMP>(out _))
        {
            Debug.LogError("The selected object already has the LanguageTextInputFieldTMP component!", tmpInputField.gameObject);
            return;
        }

        var component = Undo.AddComponent<LanguageTextInputFieldTMP>(tmpInputField.gameObject);
        component.InputField = tmpInputField;

        Undo.RegisterCreatedObjectUndo(component, "Add LanguageTextInputFieldTMP");
        Debug.Log("TMP_InputField converted.", tmpInputField.gameObject);
    }

    /// <summary>
    /// Converts a TMP_Text to LanguageTextTMP.
    /// </summary>
    private static void ConvertTMPText(TMP_Text tmpText)
    {
        if (tmpText.TryGetComponent<LanguageTextTMP>(out _))
        {
            Debug.LogError("The selected object already has the LanguageTextTMP component!", tmpText.gameObject);
            return;
        }

        var component = Undo.AddComponent<LanguageTextTMP>(tmpText.gameObject);
        component.TextComponent = tmpText;

        Undo.RegisterCreatedObjectUndo(component, "Add LanguageTextTMP");
        Debug.Log("TMP_Text converted.", tmpText.gameObject);
    }

    #endregion

    #region === 3D and Audio Converters ===

    /// <summary>
    /// Converts a TextMesh to LanguageTextMesh.
    /// </summary>
    private static void ConvertTextMesh(TextMesh textMesh)
    {
        if (textMesh.TryGetComponent<LanguageTextMesh>(out _))
        {
            Debug.LogError("The selected object already has the LanguageTextMesh component!", textMesh.gameObject);
            return;
        }

        var component = Undo.AddComponent<LanguageTextMesh>(textMesh.gameObject);
        component.TextComponent = textMesh;

        Undo.RegisterCreatedObjectUndo(component, "Add LanguageTextMesh");
        Debug.Log("TextMesh converted.", textMesh.gameObject);
    }

    /// <summary>
    /// Converts a MeshRenderer paired with a TMP_Text to LanguageTextMeshTMP.
    /// </summary>
    private static void ConvertMeshRenderer(MeshRenderer meshRenderer, TMP_Text textComponent)
    {
        if (meshRenderer.TryGetComponent<LanguageTextMeshTMP>(out _))
        {
            Debug.LogError("The selected object already has the LanguageTextMeshTMP component!", meshRenderer.gameObject);
            return;
        }

        var component = Undo.AddComponent<LanguageTextMeshTMP>(meshRenderer.gameObject);
        component.TextComponent = textComponent;

        Undo.RegisterCreatedObjectUndo(component, "Add LanguageTextMeshTMP");
        Debug.Log("TMP_TextMesh converted.", meshRenderer.gameObject);
    }

    /// <summary>
    /// Converts an AudioSource to LanguageAudioPlayer.
    /// </summary>
    private static void ConvertAudioSource(AudioSource audioSource)
    {
        if (audioSource.TryGetComponent<LanguageAudioPlayer>(out _))
        {
            Debug.LogError("The selected object already has the LanguageAudioPlayer component!", audioSource.gameObject);
            return;
        }

        var component = Undo.AddComponent<LanguageAudioPlayer>(audioSource.gameObject);
        component.AudioSource = audioSource;

        Undo.RegisterCreatedObjectUndo(component, "Add LanguageAudioPlayer");
        Debug.Log("AudioSource converted.", audioSource.gameObject);
    }

    #endregion
}