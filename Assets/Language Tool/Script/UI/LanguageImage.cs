/*
 * ---------------------------------------------------------------------------
 * Description: Dynamically updates a Unity UI Image component with a localized
 *              sprite based on the current language. The component constructs
 *              the appropriate image path and loads the asset asynchronously
 *              from disk when the language changes.
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

[AddComponentMenu("Language/UI/Language Image")]
public class LanguageImage : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("Do not use characters outside the basic ASCII Table.")]
    [SerializeField] private string fileName = "Example.png"; // Name of the image file (with extension) to load.
    [SerializeField] private bool useImage = true; // Flag to determine whether to use and update the Image component.
    [Space(5)]
    public Image image; // Unity UI Image component to which the loaded sprite will be assigned.
    public Texture2D imageTexture; // Holds the loaded texture data from disk.
    public Sprite spriteTexture; // Sprite created from the loaded texture to be used by the Image component.

    private LanguageSettingsData languageData; // Holds the current language settings for determining the culture-specific path.
    private string previousFilePath; // Stores the last loaded file path to prevent redundant reloads.
    private string filePath; // Full file path to the localized image based on the selected language.

    /// <summary>
    /// Subscribes to the language update event and immediately updates the image when the object is enabled.
    /// </summary>
    private void OnEnable()
    {
        LanguageManagerDelegate.OnLanguageUpdate += LanguageUpdate; // Listen for language changes.
        LanguageUpdate(); // Trigger image update immediately.
    }

    /// <summary>
    /// Unsubscribes from the language update event when the object is disabled.
    /// </summary>
    private void OnDisable() => LanguageManagerDelegate.OnLanguageUpdate -= LanguageUpdate;

    /// <summary>
    /// Updates the image based on the selected language.
    /// Avoids reloading if the file path remains unchanged.
    /// </summary>
    public void LanguageUpdate()
    {
        // Validate the Image component reference.
        if (useImage && image == null)
        {
            Debug.LogError("Image is not assigned. Please assign an Image component.", this);
            return;
        }

        // Load current language settings to determine the selected culture.
        languageData = LoadLanguageSettings();
        if (languageData == null)
        {
            Debug.LogError("LanguageImage: Failed to load LanguageSettingsData.", this);
            return;
        }

        // Construct the absolute path to the localized image file.
        filePath = Path.Combine(GetLanguageAssetsPath(), "Assets", languageData.selectedCulture, "Image", fileName);

        // Prevent reloading if the image hasn't changed.
        if (filePath == previousFilePath) return;
        previousFilePath = filePath;

        // Verify that the image file exists at the computed path.
        if (!File.Exists(filePath))
        {
            Debug.LogError($"Image file not found at path: {filePath} (Culture: {languageData.selectedCulture})", this);
            return;
        }

        // Clean up previously loaded resources.
        if (imageTexture != null) Destroy(imageTexture);
        if (spriteTexture != null) Destroy(spriteTexture);

        // Begin loading the image asynchronously.
        StartCoroutine(LoadFileCoroutine());
    }

    /// <summary>
    /// Coroutine that asynchronously loads a texture from disk and applies it as a sprite to the Image component.
    /// </summary>
    private IEnumerator LoadFileCoroutine()
    {
        // Create a UnityWebRequest to load the texture from the file system.
        using UnityWebRequest request = UnityWebRequestTexture.GetTexture($"file://{filePath}");
        yield return request.SendWebRequest();

        // Handle loading errors.
        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError($"Error loading image: {request.error}", this);
            yield break;
        }

        // Extract texture content from the request response.
        imageTexture = DownloadHandlerTexture.GetContent(request);

        // Create a new sprite from the texture.
        spriteTexture = Sprite.Create(imageTexture, new Rect(0, 0, imageTexture.width, imageTexture.height), new Vector2(0.5f, 0.5f));

        // Assign the sprite to the Image component if enabled.
        if (useImage && imageTexture != null) image.sprite = spriteTexture;
    }
}