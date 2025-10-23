/*
 * ---------------------------------------------------------------------------
 * Description: Automatically validates and updates a TMP Text component's font
 *              to ensure all characters in the current language are supported.
 *              
 * Author: Lucas Gomes Cecchini
 * Pseudonym: AGAMENOM
 * ---------------------------------------------------------------------------
*/

using System.Collections.Generic;
using LanguageTools;
using UnityEngine;
using TMPro;

using static LanguageTools.LanguageFileManager;

/// <summary>
/// Validates a TextMeshPro Text component's font against displayed text, replacing the font or fallback language name if necessary.
/// </summary>
[AddComponentMenu("Language/UI/Complements/Automatic Language Font Validator (TMP)")]
public class AutomaticLanguageFontValidatorTMP : MonoBehaviour
{
    #region === Serialized Fields ===

    [Header("Target Text to Monitor")]
    [SerializeField, Tooltip("Reference to the UI TMP_Text component to validate and update automatically.")]
    private TMP_Text textComponent; // Target TMP_Text component to monitor and validate font characters for.

    [Header("Enable language fallback name resolution")]
    [SerializeField, Tooltip("Determines whether native language names can be replaced with fallback names if characters are unsupported.")]
    private bool isLanguageManager; // Whether to allow replacing native names with fallback names based on supported languages.

    #endregion

    #region === Private Fields ===

    private List<LanguageAvailable> supportedLanguages; // List of languages available from settings, used for fallback logic.
    private LanguageSettingsData localizationSettings; // Loaded localization configuration data including TMP font settings.
    private string lastValidatedText; // Stores the last validated text to avoid redundant checks.

    #endregion

    #region === Properties ===

    /// <summary>
    /// Gets or sets the target TMP_Text component monitored by this validator.
    /// </summary>
    public TMP_Text TextComponent
    {
        get => textComponent;
        set => textComponent = value;
    }

    /// <summary>
    /// Gets or sets whether the validator should allow replacing native names with fallback names.
    /// </summary>
    public bool IsLanguageManager
    {
        get => isLanguageManager;
        set => isLanguageManager = value;
    }

    #endregion

    #region === Unity Events ===

    /// <summary> Loads language settings and initializes the validator. </summary>
    private void Start()
    {
        if (textComponent == null)
        {
            Debug.LogError("AutomaticLanguageFontValidatorTMP: TextComponent is not assigned.", this);
            return;
        }

        localizationSettings = LoadLanguageSettings();
        if (localizationSettings == null)
        {
            Debug.LogError("AutomaticLanguageFontValidatorTMP: Failed to load LanguageSettingsData.", this);
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

        // Use textComponent's internal string reference for comparison to avoid string allocation.
        string currentText = textComponent.text;
        if (!ReferenceEquals(currentText, lastValidatedText))
        {
            lastValidatedText = currentText;
            ValidateFontSupport(currentText);
        }
    }

    #endregion

    #region === Font Validation ===

    /// <summary>
    /// Validates that the font supports all characters in the provided text.
    /// Falls back to alternate fonts or language names as necessary.
    /// </summary>
    /// <param name="text">The text to validate.</param>
    private void ValidateFontSupport(string text)
    {
        if (textComponent == null || localizationSettings == null || localizationSettings.fontListData == null)
        {
            Debug.LogWarning("AutomaticLanguageFontValidatorTMP: Missing textComponent or font list.", this);
            return;
        }

        var currentFont = textComponent.font;
        var fallbackFonts = localizationSettings.fontListDataTMP.TMPFontList;

        // Check if current font supports all characters.
        bool isCurrentFontValid = true;
        foreach (char c in text)
        {
            if (!currentFont.HasCharacter(c))
            {
                isCurrentFontValid = false;
                break;
            }
        }

        if (isCurrentFontValid) return; // Current font is valid, no need to change.

        // Try each fallback font to see if any supports all characters.
        foreach (var fallbackFont in fallbackFonts)
        {
            if (fallbackFont == null) continue;

            bool fontSupportsAll = true;
            foreach (char c in text)
            {
                if (!fallbackFont.HasCharacter(c))
                {
                    fontSupportsAll = false;
                    break;
                }
            }

            if (fontSupportsAll)
            {
                textComponent.font = fallbackFont;
                Debug.LogWarning($"AutomaticLanguageFontValidatorTMP: Applied fallback font '{fallbackFont.name}'.", this);
                return;
            }
        }

        // If no font supports all characters, fallback to language name.
        if (isLanguageManager)
        {
            var matchingLanguage = supportedLanguages.Find(lang => lang.nativeName == text);
            if (matchingLanguage != null)
            {
                textComponent.text = matchingLanguage.name;
                Debug.LogWarning($"AutomaticLanguageFontValidatorTMP: Unsupported characters detected. Replaced native name with fallback name: '{matchingLanguage.name}'.", this);
            }
        }
    }

    #endregion
}