using UnityEngine;
using System.IO;
using System.Linq;
using System.Globalization;
using System.Collections.Generic;
/**/
using TMPro;
/**/

public class LanguageInitialization
{
    public static LanguageSettingsData languageSettingsData; // Language settings data.

    // Selected and system languages.
    private static string selectedLanguage;
    private static string systemLanguage;

    // File paths.
    private static string savePath;
    private static string selectedFile;
    private static string fileSavePath;

    // Including font list information.
    public static LanguageFontListData fontListData;
    public static TMP_LanguageFontListData TMP_fontListData;

    // File names and folder paths for language data in Unity Editor.
    public static string jsonNameInUnity = "/Language/Editor/LanguageFileSave.json";
    public static string jsonSaveNameInUnity = "/Language/Editor/LanguageSave.json";
    public static string folderNameInUnity = "/StreamingAssets/Language/";

    // File names and folder paths for language data in a build.
    public static string jsonNameInBuild = "/LanguageFileSave.json";
    public static string jsonSaveNameInBuild = "/LanguageSave.json";
    public static string folderNameInBuild = "/StreamingAssets/Language/";

    // Method called when the game initializes.
    [RuntimeInitializeOnLoadMethod]
    public static void RunGameInitialization()
    {
        Debug.Log("Language Initialization");
        LoadSettingsData(); // Load language settings data.
        LanguageStart(); // Start language initialization.

    #if !UNITY_EDITOR
        SaveFontLists(); // Save font lists to text files.
        LoadFontsFromAssetBundle(); // Load fonts from asset bundles.
        /**/
        TMP_LoadFontsFromAssetBundle(); // Load TextMeshPro fonts from asset bundles.
        /**/
        LoadFontLists(); // Load font lists from text files.
    #endif
    }

    // Load language settings data from a Unity asset.
    private static void LoadSettingsData()
    {
        languageSettingsData = Resources.Load<LanguageSettingsData>("Language Data");
        
        if (languageSettingsData == null)
        {
            Debug.LogError("The languageSettingsData is not assigned. \n Please go to 'Window/Language/Language Settings'. And click on the 'Create Language Data' button.");
            return;
        }
        
        fontListData = languageSettingsData.fontListData;
        TMP_fontListData = languageSettingsData.TMP_fontListData;

        // Update the variables with values from the loaded asset.        
        jsonSaveNameInUnity = languageSettingsData.jsonSaveNameInUnity;
        jsonNameInUnity = languageSettingsData.jsonNameInUnity;
        folderNameInUnity = languageSettingsData.folderNameInUnity;
        jsonSaveNameInBuild = languageSettingsData.jsonSaveNameInBuild;
        jsonNameInBuild = languageSettingsData.jsonNameInBuild;
        folderNameInBuild = languageSettingsData.folderNameInBuild;
    }

    private static void LanguageStart()
    {
        // Get the system language of the user's device
        CultureInfo cultureInfo = CultureInfo.InstalledUICulture;
        systemLanguage = cultureInfo.DisplayName;

        // Set file paths and folder paths based on whether in Unity Editor or a build.
    #if UNITY_EDITOR
        savePath = Application.dataPath + jsonSaveNameInUnity;
        fileSavePath = Application.dataPath + jsonNameInUnity;
        string path = Application.dataPath + folderNameInUnity;
    #else
        savePath = Application.dataPath + jsonSaveNameInBuild;
        fileSavePath = Application.dataPath + jsonNameInBuild;
        string path = Application.dataPath + folderNameInBuild;
    #endif

        string[] files = Directory.GetFiles(path, "*.txt"); // List all text files in the specified folder.

        // Check if there is no save data and select the appropriate language file based on the system language.
        if (!File.Exists(savePath) && !File.Exists(fileSavePath))
        {
            foreach (string buildFilePath in files)
            {
                foreach (string unityFilePath in files)
                {
                    string[] unityFileLines = File.ReadAllLines(unityFilePath);
                    string unityFileLine2 = unityFileLines.Length >= 2 ? unityFileLines[1] : "";
                    string firstLine = File.ReadLines(unityFilePath).First();
                    string contentInBrackets = firstLine.Replace("Linguagem - [", "").Replace("]", "");

                    if (systemLanguage == unityFileLine2)
                    {
                        selectedLanguage = contentInBrackets;
                        selectedFile = unityFilePath;

                        // Save the selected language to a JSON file.
                        LanguageSave saveData = new()
                        {
                            selectedLanguage = selectedLanguage
                        };
                        string json = JsonUtility.ToJson(saveData);
                        File.WriteAllText(savePath, json);

                        // Save the selected file to a JSON file.
                        LanguageFileSave fileSaveData = new()
                        {
                            selectedFile = selectedFile
                        };
                        json = JsonUtility.ToJson(fileSaveData);
                        File.WriteAllText(fileSavePath, json);
                    }
                }
            }
        }
    }

    // Load fonts from an asset bundle in the "Font" directory.
    public static void LoadFontsFromAssetBundle()
    {
        string assetBundlePath = Path.Combine(Application.dataPath, "Font"); // Define the path to the "Font" directory.

        // Create the directory if it doesn't exist.
        if (!Directory.Exists(assetBundlePath))
        {
            Directory.CreateDirectory(assetBundlePath);
        }

        // Get an array of filenames with the ".ltbundle" extension in the "Font" directory.
        string[] bundleFiles = Directory.GetFiles(assetBundlePath, "*.ltbundle");

        // Log the presence of each bundle file.
        foreach (string bundleFile in bundleFiles)
        {
            Debug.Log("Bundle file found: " + bundleFile);
        }

        // Iterate through each bundle file and load fonts from the asset bundle.
        foreach (string bundleFile in bundleFiles)
        {
            AssetBundle fontBundle = AssetBundle.LoadFromFile(bundleFile); // Load the asset bundle from the file path.

            // Check if the asset bundle is successfully loaded.
            if (fontBundle != null)
            {
                string[] fontNames = fontBundle.GetAllAssetNames(); // Get an array of font asset names in the bundle.

                // Check if LanguageFontListData is available.
                if (fontListData != null)
                {
                    // Iterate through each font asset name and load the font.
                    foreach (string fontName in fontNames)
                    {
                        Font font = fontBundle.LoadAsset<Font>(fontName);

                        // Check if the font is successfully loaded.
                        if (font != null)
                        {
                            // Ensure the font list is initialized and add the font.
                            fontListData.fontList ??= new List<Font>();
                            fontListData.fontList.Add(font);
                        }
                    }
                }
                else
                {
                    // Log an error if LanguageFontListData is not found.
                    Debug.LogError("LanguageFontListData not found with the specified name: " + bundleFile);
                }
            }
            else
            {
                // Log an error if the font asset bundle fails to load.
                Debug.LogError("Failed to load the font asset bundle from: " + bundleFile);
            }
        }
    }

    /**/
    // Load TextMeshPro fonts from an asset bundle in the "Font" directory.
    public static void TMP_LoadFontsFromAssetBundle()
    {
        string assetBundlePath = Path.Combine(Application.dataPath, "Font"); // Define the path to the "Font" directory.

        // Create the directory if it doesn't exist.
        if (!Directory.Exists(assetBundlePath))
        {
            Directory.CreateDirectory(assetBundlePath);
        }

        // Get an array of filenames with the ".tmpltbundle" extension in the "Font" directory.
        string[] bundleFiles = Directory.GetFiles(assetBundlePath, "*.tmpltbundle");

        // Log the presence of each bundle file.
        foreach (string bundleFile in bundleFiles)
        {
            Debug.Log("Bundle file found: " + bundleFile);
        }

        // Iterate through each bundle file and load TextMeshPro fonts from the asset bundle.
        foreach (string bundleFile in bundleFiles)
        {
            AssetBundle fontBundle = AssetBundle.LoadFromFile(bundleFile); // Load the asset bundle from the file path.

            // Check if the asset bundle is successfully loaded.
            if (fontBundle != null)
            {
                string[] fontNames = fontBundle.GetAllAssetNames(); // Get an array of TextMeshPro font asset names in the bundle.

                // Check if TMP_LanguageFontListData is available.
                if (TMP_fontListData != null)
                {
                    // Iterate through each TextMeshPro font asset name and load the font.
                    foreach (string fontName in fontNames)
                    {
                        TMP_FontAsset font = fontBundle.LoadAsset<TMP_FontAsset>(fontName);

                        // Check if the font is successfully loaded.
                        if (font != null)
                        {
                            // Ensure the TextMeshPro font list is initialized and add the font.
                            TMP_fontListData.TMP_fontList ??= new List<TMP_FontAsset>();
                            TMP_fontListData.TMP_fontList.Add(font);
                        }
                    }
                }
                else
                {
                    // Log an error if TMP_LanguageFontListData is not found.
                    Debug.LogError("TMP_LanguageFontListData not found with the specified name: " + bundleFile);
                }
            }
            else
            {
                // Log an error if the TextMeshPro font asset bundle fails to load.
                Debug.LogError("Failed to load the font asset bundle from: " + bundleFile);
            }
        }
    }
    /**/

    #pragma warning disable IDE0051
    // Save font lists, both regular fonts and TextMeshPro fonts, to text files.
    private static void SaveFontLists()
    {        
        string fontFolderPath = Path.Combine(Application.dataPath, "Font"); // Define the folder path where font lists will be saved.

        // Ensure the directory exists; create it if it doesn't.
        if (!Directory.Exists(fontFolderPath))
        {
            Directory.CreateDirectory(fontFolderPath);
        }

        // Regular Fonts:
        // Construct the file name and file path for the regular font list.
        string fontListFileName = fontListData.name + ".txt";
        string fontListFilePath = Path.Combine(fontFolderPath, fontListFileName);

        // Check if the file doesn't exist to avoid overwriting existing data.
        if (!File.Exists(fontListFilePath))
        {
            // Extract font names from the fontListObject and convert them to a string.
            List<string> fontNames = fontListData.fontList.Select(font => font.name).ToList();
            string fontListContent = string.Join("\n", fontNames);

            File.WriteAllText(fontListFilePath, fontListContent); // Write the font list to a text file.
        }

        /**/
        // TextMesh Pro Fonts (TMP):
        // Construct the file name and file path for the TextMeshPro font list.
        string TMP_fontListFileName = TMP_fontListData.name + ".txt";
        string TMP_fontListFilePath = Path.Combine(fontFolderPath, TMP_fontListFileName);

        // Check if the file doesn't exist to avoid overwriting existing data.
        if (!File.Exists(TMP_fontListFilePath))
        {
            // Extract TextMesh Pro font names from the TMP_fontListObject and convert them to a string.
            List<string> TMPFontNames = TMP_fontListData.TMP_fontList.Select(tmpFont => tmpFont.name).ToList();
            string TMP_fontListContent = string.Join("\n", TMPFontNames);

            File.WriteAllText(TMP_fontListFilePath, TMP_fontListContent); // Write the TextMesh Pro font list to a text file.
        }
        /**/
    }

    // Load font lists, both regular fonts and TextMeshPro fonts, from the saved text files.
    private static void LoadFontLists()
    {
        string fontFolderPath = Path.Combine(Application.dataPath, "Font"); // Define the folder path where font lists are stored.

        // Check if the directory exists.
        if (Directory.Exists(fontFolderPath))
        {
            // Regular Fonts:
            // Construct the file name and file path for the regular font list.
            string fontListFileName = fontListData.name + ".txt";
            string fontListFilePath = Path.Combine(fontFolderPath, fontListFileName);

            // Check if the file exists.
            if (File.Exists(fontListFilePath))
            {
                // Read the content of the regular font list text file.
                string fontListContent = File.ReadAllText(fontListFilePath);
                string[] fontListLines = fontListContent.Split('\n');

                // Extract font names from the loaded text file and add them to a list.
                List<string> loadedFontNames = new();
                foreach (string line in fontListLines)
                {
                    string fontName = line.Trim();
                    loadedFontNames.Add(fontName);
                }

                List<Font> fontList = fontListData.fontList;
                List<Font> updatedFontList = new();

                // Update the font list with fonts matching the loaded font names.
                foreach (string fontName in loadedFontNames)
                {
                    Font matchingFont = fontList.Find(font => font.name == fontName);
                    if (matchingFont != null)
                    {
                        updatedFontList.Add(matchingFont);
                    }
                }

                // Replace the original font list with the updated list.
                fontListData.fontList.Clear();
                fontListData.fontList.AddRange(updatedFontList);
            }

            /**/
            // TextMesh Pro Fonts (TMP):
            // Construct the file name and file path for the TextMeshPro font list.
            string TMP_fontListFileName = TMP_fontListData.name + ".txt";
            string TMP_fontListFilePath = Path.Combine(fontFolderPath, TMP_fontListFileName);

            // Check if the file exists.
            if (File.Exists(TMP_fontListFilePath))
            {
                // Read the content of the TextMesh Pro font list text file.
                string TMP_fontListContent = File.ReadAllText(TMP_fontListFilePath);
                string[] TMP_fontListLines = TMP_fontListContent.Split('\n');

                // Extract TextMesh Pro font names from the loaded text file and add them to a list.
                List<string> loadedTMPFontNames = new();
                foreach (string line in TMP_fontListLines)
                {
                    string tmpFontName = line.Trim();
                    loadedTMPFontNames.Add(tmpFontName);
                }

                List<TMP_FontAsset> TMPFontList = TMP_fontListData.TMP_fontList;
                List<TMP_FontAsset> updatedTMPFontList = new();

                // Update the TextMesh Pro font list with fonts matching the loaded font names.
                foreach (string tmpFontName in loadedTMPFontNames)
                {
                    TMP_FontAsset matchingTMPFont = TMPFontList.Find(tmpFont => tmpFont.name == tmpFontName);
                    if (matchingTMPFont != null)
                    {
                        updatedTMPFontList.Add(matchingTMPFont);
                    }
                }

                // Replace the original TextMesh Pro font list with the updated list.
                TMP_fontListData.TMP_fontList.Clear();
                TMP_fontListData.TMP_fontList.AddRange(updatedTMPFontList);
            }
            /**/
        }
    }
    #pragma warning restore IDE0051
}