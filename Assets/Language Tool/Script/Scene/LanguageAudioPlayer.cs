/*
 * ---------------------------------------------------------------------------
 * Description: This script plays localized audio files based on the current
 *              language settings and user preferences. It dynamically loads audio files
 *              from the specified language folder.
 * Author: Lucas Gomes Cecchini
 * Pseudonym: AGAMENOM
 * ---------------------------------------------------------------------------
*/

using UnityEngine.Networking;
using System.Collections;
using LanguageTools;
using UnityEngine;
using System.IO;

// This component is responsible for playing localized audio files based on the current language settings.
[AddComponentMenu("Language/3D Object/Language Audio Player")]
public class LanguageAudioPlayer : MonoBehaviour
{
    [Header("Settings")]
    public AudioSource audioSource; // The AudioSource component that will play the audio clip.
    [Space(5)]
    [Tooltip("Do not use characters outside the basic ASCII Table")]
    [SerializeField] private string fileName = "Example.wav"; // Name of the audio file to be played.
    [Space(10)]
    [Header("Automatic Information")]
    [SerializeField] private string selectedFile; // The selected language file path.
    [Space(5)]
    [SerializeField] private string saveFile; // The save file path that stores user preferences.
    [Space(5)]
    [SerializeField] private string filePath; // The full file path of the audio file to be played.

    // Subscribes to the OnLanguageUpdate event when the object is enabled.
    private void OnEnable()
    {
        LanguageManagerDelegate.OnLanguageUpdate += LanguageUpdate; // Subscribe to language updates.
        LanguageUpdate(); // Perform an initial update when enabled.
    }

    // Unsubscribes from the OnLanguageUpdate event when the object is disabled.
    private void OnDisable()
    {
        LanguageManagerDelegate.OnLanguageUpdate -= LanguageUpdate; // Unsubscribe from language updates.
    }

    // Updates the language settings and attempts to load and play the audio file based on the current language.
    public void LanguageUpdate()
    {
        // Check if the AudioSource component is assigned.
        if (audioSource == null)
        {
            Debug.LogError("AudioSource is not assigned. Please assign an AudioSource component.");
            return;
        }

        saveFile = LanguageFileManager.GetSaveFilePath(); // Retrieve the path to the save file.

        // If the save file exists, read the selected language file from it.
        if (File.Exists(saveFile))
        {
            string json = File.ReadAllText(saveFile);
            var saveData = JsonUtility.FromJson<LanguageSaveData>(json); // Deserialize the save data.
            selectedFile = saveData.selectedFile; // Set the selected file path.
        }
        else
        {
            // Use the default language file if no save file is found.
            selectedFile = LanguageFileManager.FindDefaultLanguageFilePath();
        }

        // Construct the file path for the audio file in the language-specific folder.
        filePath = $"{LanguageFileManager.GetLanguageFilesFolderPath()}/{LanguageFileManager.GetLanguageTagFromFile(selectedFile)}/Sounds/{fileName}";

        // Check if the audio file exists at the specified path.
        if (!File.Exists(filePath))
        {
            Debug.LogError($"Audio file not found at path: {filePath}");
            return;
        }

        // Start loading the audio file asynchronously.
        StartCoroutine(LoadFile());
    }

    // Coroutine that loads the audio file and plays it through the AudioSource component.
    private IEnumerator LoadFile()
    {
        // Create a UnityWebRequest to load the audio file from the file system.
        using UnityWebRequest request = UnityWebRequestMultimedia.GetAudioClip($"file://{filePath}", AudioType.UNKNOWN);
        yield return request.SendWebRequest(); // Send the request and wait for the result.

        // Check if the request was successful.
        if (request.result == UnityWebRequest.Result.Success)
        {
            // If successful, set the AudioSource clip and play it.
            audioSource.clip = DownloadHandlerAudioClip.GetContent(request);
            audioSource.Play();
        }
        else
        {
            Debug.LogError($"Error loading audio: {request.error}");
        }
    }
}