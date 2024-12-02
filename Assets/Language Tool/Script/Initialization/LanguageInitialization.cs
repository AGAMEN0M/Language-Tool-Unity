/*
 * ---------------------------------------------------------------------------
 * Description: Manages the initialization of language settings and font loading
 *              for the game. It handles loading, saving, and updating font 
 *              lists based on language files.
 * Author: Lucas Gomes Cecchini
 * Pseudonym: AGAMENOM
 * ---------------------------------------------------------------------------
*/

using System.Collections.Generic;
using System.Globalization;
using LanguageTools;
using UnityEngine;
using System.Linq;
using System.IO;
using TMPro;

public class LanguageInitialization
{
    private static LanguageFontListData fontListData;
    private static LanguageFontListDataTMP fontListDataTMP;

    // Method executed automatically when the game starts.
    [RuntimeInitializeOnLoadMethod]
    public static void InitializeLanguageSettings()
    {
        Debug.Log("Language Startup");
        LoadLanguageSettings(); // Load the language settings data.
        SetupDefaultLanguage(); // Initialize language settings.

    #if !UNITY_EDITOR
        SaveFontListsToFile(); // Save the list of fonts to a file.
        LoadRegularFontsFromAssetBundle(); // Load regular fonts from an AssetBundle.
        LoadTMPFontsFromAssetBundle(); // Load TMP fonts from an AssetBundle.
        LoadFontListsFromFile(); // Load the list of fonts from a file.
    #endif
    }

    // Loads language settings data from resources.
    private static void LoadLanguageSettings()
    {
        var settingsData = LanguageFileManager.LoadLanguageSettings();
        
        if (settingsData == null)
        {
            Debug.LogError("The languageSettingsData is not assigned.");
            return;
        }

        fontListData = settingsData.fontListData; // Assign regular font list data.
        fontListDataTMP = settingsData.fontListDataTMP; // Assign TMP font list data.
    }

    // Initializes language settings based on system language and available files.
    private static void SetupDefaultLanguage()
    {
        string systemLanguage = CultureInfo.InstalledUICulture.DisplayName; // Get system language.
        string saveFile = LanguageFileManager.GetSaveFilePath(); // Get the path to the save file.
        string pathFolder = LanguageFileManager.GetLanguageFilesFolderPath(); // Get the folder path for language files.

        // Ensure the save file directory exists.
        string directoryPath = Path.GetDirectoryName(saveFile);
        if (!Directory.Exists(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }

        string[] files = Directory.GetFiles(pathFolder, "*.txt"); // Get all language files in the specified folder.

        // If no save file exists, create one based on the system language.
        if (!File.Exists(saveFile) && !File.Exists(pathFolder))
        {
            foreach (string buildFilePath in files)
            {
                foreach (string unityFilePath in files)
                {
                    string[] unityFileLines = File.ReadAllLines(unityFilePath);
                    string unityFileLine2 = unityFileLines.Length >= 2 ? unityFileLines[1] : ""; // Second line of the file.
                    string firstLine = File.ReadLines(unityFilePath).First(); // First line of the file.
                    string contentInBrackets = firstLine.Replace("Linguagem - [", "").Replace("]", ""); // Extract the language name.

                    // If the system language matches the second line in the file, save the language data.
                    if (systemLanguage == unityFileLine2)
                    {
                        LanguageSaveData languageDataSave = new()
                        {
                            selectedFile = unityFilePath,
                            selectedLanguage = contentInBrackets
                        };

                        string json = JsonUtility.ToJson(languageDataSave);
                        File.WriteAllText(saveFile, json);
                    }
                }
            }
        }
    }

    // Loads regular fonts from an AssetBundle and adds them to the font list.
    #pragma warning disable IDE0051
    private static void LoadRegularFontsFromAssetBundle()
    #pragma warning restore IDE0051
    {
        string assetBundlePath = Path.Combine(Application.dataPath, "Font");

        if (!Directory.Exists(assetBundlePath))
        {
            Directory.CreateDirectory(assetBundlePath);
        }

        string[] bundleFiles = Directory.GetFiles(assetBundlePath, "*.ltbundle");

        foreach (string bundleFile in bundleFiles)
        {
            Debug.Log($"Bundle file found: {bundleFile}");
        }

        foreach (string bundleFile in bundleFiles)
        {
            var fontBundle = AssetBundle.LoadFromFile(bundleFile);

            if (fontBundle != null)
            {
                string[] fontNames = fontBundle.GetAllAssetNames();

                if (fontListData != null)
                {
                    foreach (string fontName in fontNames)
                    {
                        var font = fontBundle.LoadAsset<Font>(fontName);
                        if (font != null)
                        {
                            fontListData.fontList ??= new List<Font>();
                            fontListData.fontList.Add(font);
                        }
                    }
                }
                else
                {
                    Debug.LogError($"LanguageFontListData not found with the specified name: {bundleFile}");
                }
            }
            else
            {
                Debug.LogError($"Failed to load the font asset bundle from: {bundleFile}");
            }
        }
    }

    // Loads TMP fonts from an AssetBundle and adds them to the TMP font list.
    #pragma warning disable IDE0051
    private static void LoadTMPFontsFromAssetBundle()
    #pragma warning restore IDE0051
    {
        string assetBundlePath = Path.Combine(Application.dataPath, "Font");

        if (!Directory.Exists(assetBundlePath))
        {
            Directory.CreateDirectory(assetBundlePath);
        }

        string[] bundleFiles = Directory.GetFiles(assetBundlePath, "*.tmpltbundle");

        foreach (string bundleFile in bundleFiles)
        {
            Debug.Log($"Bundle file found: {bundleFile}");
        }

        foreach (string bundleFile in bundleFiles)
        {
            var fontBundle = AssetBundle.LoadFromFile(bundleFile);

            if (fontBundle != null)
            {
                string[] fontNames = fontBundle.GetAllAssetNames();

                if (fontListDataTMP != null)
                {
                    foreach (string fontName in fontNames)
                    {
                        var font = fontBundle.LoadAsset<TMP_FontAsset>(fontName);

                        if (font != null)
                        {
                            fontListDataTMP.TMPFontList ??= new List<TMP_FontAsset>();
                            fontListDataTMP.TMPFontList.Add(font);
                        }
                    }
                }
                else
                {
                    Debug.LogError($"LanguageFontListDataTMP not found with the specified name: {bundleFile}");
                }
            }
            else
            {
                Debug.LogError($"Failed to load the font asset bundle from: {bundleFile}");
            }
        }
    }

    // Saves the list of fonts to text files.
    #pragma warning disable IDE0051
    private static void SaveFontListsToFile()
    #pragma warning restore IDE0051
    {
        string folderPath = Path.Combine(Application.dataPath, "Font");

        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }

        string fontListFilePath = Path.Combine(folderPath, $"{fontListData.name}.txt");
        string fontListFilePathTMP = Path.Combine(folderPath, $"{fontListDataTMP.name}.txt");

        // Save regular fonts to a text file if it doesn't already exist.
        if (!File.Exists(fontListFilePath))
        {
            List<string> fontNames = fontListData.fontList.Select(font => font.name).ToList();
            string fontListContent = string.Join("\n", fontNames);

            File.WriteAllText(fontListFilePath, fontListContent);
        }

        // Save TMP fonts to a text file if it doesn't already exist.
        if (!File.Exists(fontListFilePathTMP))
        {
            List<string> fontNamesTMP = fontListDataTMP.TMPFontList.Select(fontTMP => fontTMP.name).ToList();
            string fontListContentTMP = string.Join("\n", fontNamesTMP);

            File.WriteAllText(fontListFilePathTMP, fontListContentTMP);
        }
    }

    // Loads the list of fonts from text files and updates the font lists.
    #pragma warning disable IDE0051
    private static void LoadFontListsFromFile()
    #pragma warning restore IDE0051
    {
        string fontFolderPath = Path.Combine(Application.dataPath, "Font");

        if (Directory.Exists(fontFolderPath))
        {
            string fontListFilePath = Path.Combine(fontFolderPath, $"{fontListData.name}.txt");
            string fontListFilePathTMP = Path.Combine(fontFolderPath, $"{fontListDataTMP.name}.txt");

            // Load regular fonts from the text file.
            if (File.Exists(fontListFilePath))
            {
                string fontListContent = File.ReadAllText(fontListFilePath);
                string[] fontListLines = fontListContent.Split('\n');

                List<string> loadedFontNames = new();
                foreach (string line in fontListLines)
                {
                    string fontName = line.Trim();
                    loadedFontNames.Add(fontName);
                }

                List<Font> fontList = fontListData.fontList;
                List<Font> updatedFontList = new();

                // Match loaded font names with existing fonts and update the list.
                foreach (string fontName in loadedFontNames)
                {
                    Font matchingFont = fontList.Find(font => font.name == fontName);
                    if (matchingFont != null)
                    {
                        updatedFontList.Add(matchingFont);
                    }
                }

                fontListData.fontList.Clear();
                fontListData.fontList.AddRange(updatedFontList);
            }

            // Load TMP fonts from the text file.
            if (File.Exists(fontListFilePathTMP))
            {
                string fontListContentTMP = File.ReadAllText(fontListFilePathTMP);
                string[] fontListLinesTMP = fontListContentTMP.Split('\n');

                List<string> loadedFontNamesTMP = new();
                foreach (string line in fontListLinesTMP)
                {
                    string fontNameTMP = line.Trim();
                    loadedFontNamesTMP.Add(fontNameTMP);
                }

                List<TMP_FontAsset> fontListTMP = fontListDataTMP.TMPFontList;
                List<TMP_FontAsset> updatedFontListTMP = new();

                // Match loaded TMP font names with existing fonts and update the list.
                foreach (string fontNameTMP in loadedFontNamesTMP)
                {
                    TMP_FontAsset matchingFontTMP = fontListTMP.Find(fontTMP => fontTMP.name == fontNameTMP);
                    if (matchingFontTMP != null)
                    {
                        updatedFontListTMP.Add(matchingFontTMP);
                    }
                }

                fontListDataTMP.TMPFontList.Clear();
                fontListDataTMP.TMPFontList.AddRange(updatedFontListTMP);
            }
        }
    }
}