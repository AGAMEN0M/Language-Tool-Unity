using UnityEngine;

// ScriptableObject to hold language settings data.
public class LanguageSettingsData : ScriptableObject
{
    [Header("Font List Data")]
    public LanguageFontListData fontListData; // Holds information about fonts.
    /**/
    public TMP_LanguageFontListData TMP_fontListData; // Holds information about TextMeshPro fonts.
    /**/
    [Space(10)]
    [Header("Archives Location")]
    public string jsonNameInUnity = "/Language/Editor/LanguageFileSave.json"; // Path to the JSON file for language selection in Unity Editor.
    public string jsonSaveNameInUnity = "/Language/Editor/LanguageSave.json"; // Path to the JSON file for selected file in Unity Editor.
    public string folderNameInUnity = "/StreamingAssets/Language/"; // Folder containing language files in Unity Editor.
    [Space(10)]
    public string jsonNameInBuild = "/LanguageFileSave.json"; // Path to the JSON file for language selection in the build.
    public string jsonSaveNameInBuild = "/LanguageSave.json"; // Path to the JSON file for selected file in the build.
    public string folderNameInBuild = "/StreamingAssets/Language/"; // Folder containing language files in the build.
}