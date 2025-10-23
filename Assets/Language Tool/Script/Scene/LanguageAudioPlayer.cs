/*
 * ---------------------------------------------------------------------------
 * Description: This script dynamically loads and plays localized audio files 
 *              based on the user's selected language configuration.
 *              It supports different audio types and can play the clip 
 *              via an AudioSource component if configured.
 *              
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

#if UNITY_EDITOR
using UnityEditor;
#endif

[AddComponentMenu("Language/3D Object/Language Audio Player")]
public class LanguageAudioPlayer : MonoBehaviour
{
    #region === Inspector Fields ===

    [Header("Settings")]
    [SerializeField, Tooltip("Name of the audio file to be loaded and played. The extension should match the selected AudioType (e.g., 'Example.wav').")]
    private string fileName = "Example.wav"; // Name of the audio file to be played.

    [SerializeField, Tooltip("Specifies the format of the audio file (e.g., WAV, MP3, OGG).")]
    private AudioType audioType = AudioType.WAV; // Format of the audio file (WAV, MP3, etc.).

    [SerializeField, Tooltip("If enabled, the audio will be played through the assigned AudioSource component.")]
    private bool useAudioSource = true; // Determines whether to play the audio via AudioSource.

    [Space(5)]

    [SerializeField, Tooltip("Optional AudioSource component that will play the loaded audio clip if 'Use Audio Source' is enabled.")]
    private AudioSource audioSource; // Optional AudioSource component to play the loaded audio.

    [SerializeField, Tooltip("Reference to the currently loaded audio clip. Automatically updated when a file is loaded.")]
    private AudioClip audioClip; // Loaded audio clip reference.

    #endregion

    #region === Private Fields ===

    private LanguageSettingsData languageData; // Cached reference to language settings data.
    private string previousFilePath; // Path to the last loaded audio file to prevent redundant reloads.
    private string filePath; // Current resolved path to the audio file based on selected culture.

    #endregion

    #region === Public Properties ===

    /// <summary>
    /// Gets or sets the audio file name to load and play.
    /// </summary>
    public string FileName
    {
        get => fileName;
        set => fileName = value;
    }

    /// <summary>
    /// Gets or sets the audio file type (e.g., WAV, MP3, OGG).
    /// </summary>
    public AudioType AudioType
    {
        get => audioType;
        set => audioType = value;
    }

    /// <summary>
    /// Gets or sets whether this component uses an AudioSource for playback.
    /// </summary>
    public bool UseAudioSource
    {
        get => useAudioSource;
        set => useAudioSource = value;
    }

    /// <summary>
    /// Gets or sets the AudioSource component used for playback.
    /// </summary>
    public AudioSource AudioSource
    {
        get => audioSource;
        set => audioSource = value;
    }

    /// <summary>
    /// Gets or sets the loaded AudioClip.
    /// </summary>
    public AudioClip AudioClip
    {
        get => audioClip;
        set => audioClip = value;
    }

    #endregion

    #region === Unity Methods ===

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
    
    #endregion

    #region === Core Methods ===

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

    #endregion
}

#if UNITY_EDITOR

#region === Custom Inspector ===

/// <summary>
/// Custom inspector for LanguageAudioPlayer to enhance usability within Unity Editor.
/// </summary>
[CanEditMultipleObjects]
[CustomEditor(typeof(LanguageAudioPlayer))]
public class LanguageAudioPlayerEditor : Editor
{
    /// <summary>
    /// Draws the custom inspector layout.
    /// </summary>
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        // Display warning about file naming.
        GUI.color = Color.yellow;
        EditorGUILayout.HelpBox("Do not use characters outside the basic ASCII Table in the 'File Name'.", MessageType.Warning);
        GUI.color = Color.white;

        EditorGUILayout.Space(10);

        DrawDefaultInspector(); // Draw default inspector fields.
        serializedObject.ApplyModifiedProperties();
    }
}

#endregion

#endif