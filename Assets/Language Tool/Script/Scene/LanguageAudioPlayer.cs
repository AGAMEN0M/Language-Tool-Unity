/*
 * ---------------------------------------------------------------------------
 * Description: This script dynamically loads and plays localized audio files 
 *              based on the user's selected language configuration.
 *              It supports different audio types and can play the clip 
 *              via an AudioSource component if configured.
 * Author: Lucas Gomes Cecchini
 * Pseudonym: AGAMENOM
 * ---------------------------------------------------------------------------
*/

using UnityEngine.Networking;
using System.Collections;
using LanguageTools;
using UnityEngine;
using System.IO;

using static LanguageTools.LanguageFileManager;

[AddComponentMenu("Language/3D Object/Language Audio Player")]
public class LanguageAudioPlayer : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("Do not use characters outside the basic ASCII Table.")]
    [SerializeField] private string fileName = "Example.wav"; // Name of the audio file to be played.
    [SerializeField] private AudioType audioType = AudioType.WAV; // Format of the audio file (WAV, MP3, etc.).
    [SerializeField] private bool useAudioSource = true; // Determines whether to play the audio via AudioSource.
    [Space(5)]
    public AudioSource audioSource; // Optional AudioSource component to play the loaded audio.
    public AudioClip audioClip; // Loaded audio clip reference.

    private LanguageSettingsData languageData; // Cached reference to language settings data.
    private string previousFilePath; // Path to the last loaded audio file to prevent redundant reloads.
    private string filePath; // Current resolved path to the audio file based on selected culture.

    /// <summary>
    /// Subscribes to the language update event when enabled and initiates audio update.
    /// </summary>
    private void OnEnable()
    {
        LanguageManagerDelegate.OnLanguageUpdate += LanguageUpdate; // Subscribe to the global language update delegate.
        LanguageUpdate(); // Trigger audio update immediately in case language was already set.
    }

    /// <summary>
    /// Unsubscribes from the language update event when disabled.
    /// </summary>
    private void OnDisable() => LanguageManagerDelegate.OnLanguageUpdate -= LanguageUpdate;

    /// <summary>
    /// Loads language data, resolves the audio file path, and initiates playback for the current culture.
    /// </summary>
    public void LanguageUpdate()
    {
        // Check for missing AudioSource when its usage is required.
        if (useAudioSource && audioSource == null)
        {
            Debug.LogError("AudioSource is not assigned. Please assign an AudioSource component.", this);
            return;
        }

        // Attempt to load the language configuration from file or cache.
        languageData = LoadLanguageSettings();
        if (languageData == null)
        {
            Debug.LogError("LanguageAudioPlayer: Failed to load LanguageSettingsData.", this);
            return;
        }

        // Build the expected full path to the audio file based on culture and folder structure.
        filePath = Path.Combine(GetLanguageAssetsPath(), "Assets", languageData.selectedCulture, "Sounds", fileName);

        // Skip reloading if the file has already been loaded once.
        if (filePath == previousFilePath) return;
        previousFilePath = filePath;

        // Verify the audio file exists before trying to load it.
        if (!File.Exists(filePath))
        {
            Debug.LogError($"Audio file not found at path: {filePath} (Culture: {languageData.selectedCulture})", this);
            return;
        }

        if (audioClip != null) Destroy(audioClip); // Clean up previously loaded audio clip to free memory.

        StartCoroutine(LoadFileCoroutine()); // Start coroutine to asynchronously load the file and play it.
    }

    /// <summary>
    /// Coroutine to load and optionally play an audio clip from the resolved file path.
    /// </summary>
    private IEnumerator LoadFileCoroutine()
    {
        // Create a UnityWebRequest to load the audio file from local disk.
        using UnityWebRequest request = UnityWebRequestMultimedia.GetAudioClip($"file://{filePath}", audioType);
        yield return request.SendWebRequest(); // Wait for the file to be downloaded asynchronously.

        // Report errors during download.
        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError($"Error loading audio: {request.error}", this);
            yield break;
        }

        // Extract the audio content from the response.
        audioClip = DownloadHandlerAudioClip.GetContent(request);

        // Play the audio via AudioSource if available and enabled.
        if (audioClip != null && useAudioSource)
        {
            audioSource.clip = audioClip;
            audioSource.Play();
        }
    }
}