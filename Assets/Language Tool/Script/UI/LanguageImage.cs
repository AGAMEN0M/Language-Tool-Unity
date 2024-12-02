/*
 * ---------------------------------------------------------------------------
 * Description: This component dynamically updates a UI Image based on the 
 *              selected language. It loads the appropriate image file corresponding 
 *              to the current language setting from disk using asynchronous file loading.
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

// Adds this script to the Unity Component Menu under the specified path.
[AddComponentMenu("Language/UI/Language Image")]
public class LanguageImage : MonoBehaviour
{
    [Header("Settings")]
    public Image image; // Reference to the UI Image component to update.
    [Space(5)]
    [Tooltip("Do not use characters outside the basic ASCII Table")]
    [SerializeField] private string fileName = "Example.png"; // The file name of the image to be loaded.
    [Space(10)]
    [Header("Automatic Information")]
    [SerializeField] private string selectedFile; // Holds the path to the currently selected language file.
    [Space(5)]
    [SerializeField] private string saveFile; // Path to the saved language settings file.
    [Space(5)]
    [SerializeField] private string filePath; // Path where the image file is located.

    // Subscribes to the LanguageUpdate event and calls the LanguageUpdate method when the object is enabled.
    private void OnEnable()
    {
        LanguageManagerDelegate.OnLanguageUpdate += LanguageUpdate;
        LanguageUpdate(); // Call to update the language data immediately when enabled.
    }

    // Unsubscribes from the LanguageUpdate event when the object is disabled.
    private void OnDisable()
    {
        LanguageManagerDelegate.OnLanguageUpdate -= LanguageUpdate;
    }

    // Method that updates the image based on the current language settings.
    public void LanguageUpdate()
    {
        // Check if the Image component is assigned.
        if (image == null)
        {
            Debug.LogError("image is not assigned. Please assign an Image component.");
            return;
        }

        saveFile = LanguageFileManager.GetSaveFilePath(); // Load the file path where the save data is stored.

        // Check if the save file exists, and if so, read the selected language file from it.
        if (File.Exists(saveFile))
        {
            string json = File.ReadAllText(saveFile);
            var saveData = JsonUtility.FromJson<LanguageSaveData>(json);
            selectedFile = saveData.selectedFile; // Store the selected language file.
        }
        else
        {
            selectedFile = LanguageFileManager.FindDefaultLanguageFilePath(); // If no save file is found, use the default language file.
        }

        // Construct the full file path for the image based on the selected language.
        filePath = $"{LanguageFileManager.GetLanguageFilesFolderPath()}/{LanguageFileManager.GetLanguageTagFromFile(selectedFile)}/Image/{fileName}";

        // Check if the image file exists at the constructed file path.
        if (!File.Exists(filePath))
        {
            Debug.LogError($"Image file not found at path: {filePath}");
            return;
        }

        StartCoroutine(LoadFile()); // Start a coroutine to load the image file asynchronously.
    }

    // Coroutine to load the image file asynchronously from disk using UnityWebRequest.
    private IEnumerator LoadFile()
    {
        // Create a UnityWebRequest to load the image file.
        using UnityWebRequest request = UnityWebRequestTexture.GetTexture($"file://{filePath}");

        yield return request.SendWebRequest(); // Wait until the web request is completed.

        // Check if the web request was successful.
        if (request.result == UnityWebRequest.Result.Success)
        {
            // If successful, create a texture from the loaded data and assign it to the Image component.
            Texture2D texture = DownloadHandlerTexture.GetContent(request);
            image.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.one * 0.5f);
        }
        else
        {
            Debug.LogError($"Error loading image: {request.error}");
        }
    }
}