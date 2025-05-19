/*
 * ---------------------------------------------------------------------------
 * Description: This component loads and displays localized images using a 
 *              RawImage component. It dynamically constructs the path to the 
 *              image file based on the selected language and loads it asynchronously. 
 *              If the file cannot be found or fails to load, an error is logged.
 * Author: Lucas Gomes Cecchini
 * Pseudonym: AGAMENOM
 * ---------------------------------------------------------------------------
*/

using UnityEngine.Networking;
using System.Collections;
using UnityEngine.UI;
using LanguageTools;
using UnityEngine;
using System.IO;

using static LanguageTools.LanguageFileManager;

[AddComponentMenu("Language/UI/Language Raw Image")]
public class LanguageRawImage : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("Do not use characters outside the basic ASCII Table.")]
    [SerializeField] private string fileName = "Example.png"; // Name of the image file to load.
    [SerializeField] private bool useRawImage = true; // Whether to apply the texture to a RawImage component.
    [Space(5)]
    public RawImage rawImage; // Reference to the RawImage component to display the texture.
    public Texture2D imageTexture; // Loaded texture from disk.

    private LanguageSettingsData languageData; // Stores the loaded language configuration.
    private string previousFilePath; // Stores the last used file path to avoid reloading the same image.
    private string filePath; // Full path to the localized image file for the selected culture.

    /// <summary>
    /// Subscribes to the language update event and triggers image update when the component is enabled.
    /// </summary>
    private void OnEnable()
    {
        LanguageManagerDelegate.OnLanguageUpdate += LanguageUpdate; // Register to receive language change updates.
        LanguageUpdate(); // Perform initial image update.
    }

    /// <summary>
    /// Unsubscribes from the language update event when the component is disabled.
    /// </summary>
    private void OnDisable() => LanguageManagerDelegate.OnLanguageUpdate -= LanguageUpdate;

    /// <summary>
    /// Updates the displayed image based on the currently selected language.
    /// If the image path hasn't changed, it skips reloading.
    /// </summary>
    public void LanguageUpdate()
    {
        // Check if RawImage is required but not assigned.
        if (useRawImage && rawImage == null)
        {
            Debug.LogError("RawImage is not assigned. Please assign a RawImage component.", this);
            return;
        }

        // Load language settings to get current culture.
        languageData = LoadLanguageSettings();
        if (languageData == null)
        {
            Debug.LogError("LanguageRawImage: Failed to load LanguageSettingsData.", this);
            return;
        }

        // Construct the path to the localized image.
        filePath = Path.Combine(GetLanguageAssetsPath(), "Assets", languageData.selectedCulture, "Image", fileName);

        // Avoid reloading if the same file was already loaded.
        if (filePath == previousFilePath) return;
        previousFilePath = filePath;

        // Confirm that the file actually exists on disk.
        if (!File.Exists(filePath))
        {
            Debug.LogError($"Image file not found at path: {filePath} (Culture: {languageData.selectedCulture})", this);
            return;
        }

        if (imageTexture != null) Destroy(imageTexture); // Destroy previously loaded texture to free memory.

        StartCoroutine(LoadFileCoroutine()); // Start asynchronous image loading.
    }

    /// <summary>
    /// Coroutine to load a localized image texture from disk and assign it to the RawImage component.
    /// </summary>
    private IEnumerator LoadFileCoroutine()
    {
        // Create a UnityWebRequest to load the image file from local path.
        using UnityWebRequest request = UnityWebRequestTexture.GetTexture($"file://{filePath}");
        yield return request.SendWebRequest(); // Wait until the request completes.

        // Log error if the request failed.
        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError($"Error loading image: {request.error}", this);
            yield break;
        }

        // Extract the Texture2D from the response.
        imageTexture = DownloadHandlerTexture.GetContent(request);

        // Assign the texture to the RawImage if enabled.
        if (imageTexture != null && useRawImage) rawImage.texture = imageTexture;
    }
}