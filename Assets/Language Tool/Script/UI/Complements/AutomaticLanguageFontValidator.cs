/*
 * ---------------------------------------------------------------------------
 * Description: Automatically validates and updates a UI Text component's font
 *              to ensure all characters in the current language are supported.
 * Author: Lucas Gomes Cecchini
 * Pseudonym: AGAMENOM
 * ---------------------------------------------------------------------------
*/

using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

using LanguageTools;
using static LanguageTools.LanguageFileManager;

/// <summary>
/// Validates a Unity UI Text component's font against displayed text, replacing the font or fallback language name if necessary.
/// </summary>
[AddComponentMenu("Language/UI/Complements/Automatic Language Font Validator (Legacy)")]
public class AutomaticLanguageFontValidator : MonoBehaviour
{
    [Header("Target Text to Monitor")]
    [SerializeField] private Text textComponent; // Target Text component to monitor and validate font characters for.

    [Header("Enable language fallback name resolution")]
    [SerializeField] private bool isLanguageManager; // Whether to allow replacing native names with fallback names based on supported languages.

    private List<LanguageAvailable> supportedLanguages; // List of languages available from settings, used for fallback logic.
    private LanguageSettingsData localizationSettings; // Loaded localization configuration data including font settings.

    private string lastValidatedText; // Stores the last validated text to prevent redundant validation.

    /// <summary> Loads language settings and initializes the validator. </summary>
    private void Start()
    {
        if (textComponent == null)
        {
            Debug.LogError("AutomaticLanguageFontValidator: TextComponent is not assigned.", this);
            return;
        }

        localizationSettings = LoadLanguageSettings();
        if (localizationSettings == null)
        {
            Debug.LogError("AutomaticLanguageFontValidator: Failed to load LanguageSettingsData.", this);
            return;
        }

        // Load available languages if fallback logic is enabled.
        if (isLanguageManager)
        {
            supportedLanguages = localizationSettings.availableLanguages;
        }

        ValidateFontSupport(textComponent.text); // Perform an initial validation of the current text.
    }

    /// <summary>
    /// Continuously checks for text changes and re-validates font support when needed.
    /// </summary>
    private void Update()
    {
        if (textComponent == null || localizationSettings == null) return;

        // Re-validate only if the text has changed.
        if (textComponent.text != lastValidatedText)
        {
            lastValidatedText = textComponent.text;
            ValidateFontSupport(lastValidatedText);
        }
    }

    /// <summary>
    /// Validates that the font supports all characters in the provided text.
    /// Falls back to alternate fonts or language names as necessary.
    /// </summary>
    /// <param name="text">The text to validate.</param>
    private void ValidateFontSupport(string text)
    {
        if (textComponent == null || localizationSettings == null || localizationSettings.fontListData == null)
        {
            Debug.LogWarning("AutomaticLanguageFontValidator: Missing textComponent or font list.");
            return;
        }

        var currentFont = textComponent.font;
        var fallbackFonts = localizationSettings.fontListData.fontList;

        foreach (char character in text)
        {
            // If the current font does not support the character.
            if (!currentFont.HasCharacter(character))
            {
                // Try to find a fallback font that does.
                foreach (var fallbackFont in fallbackFonts)
                {
                    if (fallbackFont != null && fallbackFont.HasCharacter(character))
                    {
                        textComponent.font = fallbackFont;
                        Debug.LogWarning($"AutomaticLanguageFontValidator: Applied fallback font '{fallbackFont.name}'.", this);
                        return;
                    }
                }

                // If no font supports the character and fallback name resolution is enabled.
                if (isLanguageManager)
                {
                    var matchingLanguage = supportedLanguages.Find(lang => lang.nativeName == text);
                    if (matchingLanguage != null)
                    {
                        textComponent.text = matchingLanguage.name;
                        Debug.LogWarning($"AutomaticLanguageFontValidator: Unsupported characters detected. Replaced native name with fallback name: '{matchingLanguage.name}'.", this);
                    }
                }

                return;
            }
        }
    }
}