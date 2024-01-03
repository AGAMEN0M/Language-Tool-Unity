using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

[AddComponentMenu("Language/UI/Language Image")]
public class LanguageImage : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private Image image; // Reference to the Image component used to display the language-specific image.
    [Space(5)]
    [SerializeField][Tooltip("Do not use characters outside the basic ASCII Table")] private string imageFileName = "Example.png"; // The name of the image file to be displayed.
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
        LanguageUpdate(); // Update the Language Image.
    }

    private void OnEnable()
    {
        LanguageUpdate(); // Update the Language Image when the component is enabled.
    }

    public void LanguageUpdate()
    {
        StartCoroutine(LoadAndSetImage()); // Start the coroutine to load and set the image.
    }

    private IEnumerator LoadAndSetImage()
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

        // Get the path to the image file based on the language and platform.
    #if UNITY_EDITOR
        string filePath = Application.dataPath + folderNameInUnity + languageName + "/Image/" + imageFileName;
    #else
        string filePath = Application.dataPath + folderNameInBuild + languageName + "/Image/" + imageFileName;
    #endif

        string loadedPath = "file://" + filePath;

        // Load the image file from the specified path and set it as the sprite of the Image component.
        using UnityWebRequest request = UnityWebRequestTexture.GetTexture(loadedPath);
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            Texture2D texture = DownloadHandlerTexture.GetContent(request);
            image.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.one * 0.5f);
        }
        else
        {
            Debug.LogError("Error loading image: " + request.error);
        }
    }
}