/*
 * ---------------------------------------------------------------------------
 * Description: This component loads and assigns localized textures to a 
 *              RawImage component based on the current language setting. It retrieves 
 *              the appropriate image file path from the language files and loads the 
 *              texture asynchronously. If the specified image file is not found, 
 *              an error is logged.
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

// This component is responsible for loading and assigning localized textures to a RawImage component.
[AddComponentMenu("Language/UI/Language Raw Image")]
public class LanguageRawImage : MonoBehaviour
{
    [Header("Settings")]
    public RawImage rawImage; // The RawImage component that will display the localized image.
    [Space(5)]
    [Tooltip("Do not use characters outside the basic ASCII Table")]
    [SerializeField] private string fileName = "Example.png"; // The name of the image file to load.
    [Space(10)]
    [Header("Automatic Information")]
    [SerializeField] private string selectedFile; // Stores the path of the selected language file.
    [Space(5)]
    [SerializeField] private string saveFile; // Stores the path of the save file containing user preferences.
    [Space(5)]
    [SerializeField] private string filePath; // Stores the path of the image file to load.

    // Subscribes to the OnLanguageUpdate event when the object is enabled.
    private void OnEnable()
    {
        LanguageManagerDelegate.OnLanguageUpdate += LanguageUpdate;
        LanguageUpdate(); // Perform an initial update when the component is enabled.
    }

    // Unsubscribes from the OnLanguageUpdate event when the object is disabled.
    private void OnDisable()
    {
        LanguageManagerDelegate.OnLanguageUpdate -= LanguageUpdate;
    }

    // Updates the image source based on the current language and loads the corresponding file.
    public void LanguageUpdate()
    {
        // Ensure that the RawImage component is assigned.
        if (rawImage == null)
        {
            Debug.LogError("rawImage is not assigned. Please assign an RawImage component.");
            return;
        }

        saveFile = LanguageFileManager.GetSaveFilePath(); // Retrieve the path to the save file.

        // Check if the save file exists, and load the selected language file path from it.
        if (File.Exists(saveFile))
        {
            string json = File.ReadAllText(saveFile);
            var saveData = JsonUtility.FromJson<LanguageSaveData>(json);
            selectedFile = saveData.selectedFile;
        }
        else
        {
            // Fallback to the default language file if the save file does not exist.
            selectedFile = LanguageFileManager.FindDefaultLanguageFilePath();
        }

        // Construct the file path to the image based on the language tag and file name.
        filePath = $"{LanguageFileManager.GetLanguageFilesFolderPath()}/{LanguageFileManager.GetLanguageTagFromFile(selectedFile)}/Image/{fileName}";

        // If the file does not exist at the specified path, log an error and exit the method.
        if (!File.Exists(filePath))
        {
            Debug.LogError($"Image file not found at path: {filePath}");
            return;
        }

        // Start a coroutine to asynchronously load the image file.
        StartCoroutine(LoadFile());
    }

    // Coroutine to load the image file asynchronously.
    private IEnumerator LoadFile()
    {
        // Create a UnityWebRequest to load the image file as a texture.
        using UnityWebRequest request = UnityWebRequestTexture.GetTexture($"file://{filePath}");
        yield return request.SendWebRequest(); // Send the request and wait for it to complete.

        // Check if the request was successful.
        if (request.result == UnityWebRequest.Result.Success)
        {
            // If successful, retrieve the texture from the request and assign it to the RawImage component.
            Texture2D texture = DownloadHandlerTexture.GetContent(request);
            rawImage.texture = texture;
        }
        else
        {
            // Log an error if the request failed.
            Debug.LogError($"Error loading image: {request.error}");
        }
    }
}