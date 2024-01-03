using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

[AddComponentMenu("Language/Language Audio Player")]
public class LanguageAudioPlayer : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private AudioSource audioSource; // Reference to the AudioSource component used to play the audio.
    [Space(5)]
    [SerializeField][Tooltip("Do not use characters outside the basic ASCII Table")] private string fileName = "Example.wav"; // The name of the audio file to be played.
    [Space(10)]
    [Header("Archives Location")]
    [SerializeField] private string languageName; // The name of the language.
    [Space(10)]
    [Header("Default language if there is no save")]
    [SerializeField] private string standardFile = "Linguagem - [ENGLISH]"; // The default language file to use if no save data is available.
    [Space(10)]
    [Header("Automatic Information")]
    [SerializeField] private string selectedFile; // The path to the currently selected language file.
    [SerializeField] private string savePath; // The path to the JSON file where the selected language will be saved.
    [SerializeField] private string defaultFile; // The path to the default language file.
    [Space(10)]
    [Header("Archives Location")]
    public string jsonNameInUnity = "/Language/Editor/LanguageFileSave.json"; // Path to the JSON file for language selection in Unity Editor.
    public string folderNameInUnity = "/StreamingAssets/Language/"; // Folder containing language files in Unity Editor.
    [Space(10)]
    public string jsonNameInBuild = "/LanguageFileSave.json"; // Path to the JSON file for language selection in the build.
    public string folderNameInBuild = "/StreamingAssets/Language/"; // Folder containing language files in the build.

    private void Start()
    {
        LanguageUpdate(); // Update the language Audio Player.
    }

    private void OnEnable()
    {
        LanguageUpdate(); // Update the language Audio Player when the component is enabled.
    }

    public void LanguageUpdate()
    {
        StartCoroutine(LoadAndPlayAudio()); // Start the coroutine to load and play the audio.
    }

    private IEnumerator LoadAndPlayAudio()
    {
        string path = LanguageClassTools.GetFolderPath(folderNameInUnity, folderNameInBuild); // Set the path to locate language files in Unity Editor or in the build.
        savePath = LanguageClassTools.GetJsonPath(jsonNameInUnity, jsonNameInBuild); // Set the save path for the Unity Editor or for the build.
        defaultFile = LanguageClassTools.FindDefaultLanguageFile(path, standardFile); // Find the default language file based on the StandardFile setting.

        // Check if the save file exists, and load the selected language if available; otherwise, use the default language.
        if (File.Exists(savePath))
        {
            string json = File.ReadAllText(savePath);
            LanguageFileSave fileSaveData = JsonUtility.FromJson<LanguageFileSave>(json);
            selectedFile = fileSaveData.selectedFile;

            languageName = LanguageClassTools.ExtractLanguageName(selectedFile);
        }
        else
        {
            selectedFile = defaultFile;
            languageName = LanguageClassTools.ExtractLanguageName(defaultFile);
        }

        // Get the path to the audio file based on the language and platform.
    #if UNITY_EDITOR
        string filePath = Application.dataPath + folderNameInUnity + languageName + "/Sounds/" + fileName;
    #else
        string filePath = Application.dataPath + folderNameInBuild + languageName + "/Sounds/" + fileName;
    #endif
                
        string loadedPath = "file://" + filePath;

        // Load the audio file from the specified path and play it in the audio source.
        using UnityWebRequest request = UnityWebRequestMultimedia.GetAudioClip(loadedPath, AudioType.UNKNOWN);
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            audioSource.clip = DownloadHandlerAudioClip.GetContent(request);
            audioSource.Play();
        }
        else
        {
            Debug.LogError("Error loading audio: " + request.error);
        }
    }
}