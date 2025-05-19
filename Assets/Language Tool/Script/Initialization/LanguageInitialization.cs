/*
 * ---------------------------------------------------------------------------
 * Description: Handles language initialization including loading language settings,
 *              managing font lists for both regular and TMP fonts, saving and 
 *              loading font lists from files, and loading fonts from asset bundles.
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

using static LanguageTools.LanguageFileManager;

public class LanguageInitialization
{
    private static LanguageSettingsData settingsData; // Holds the loaded language settings data from resources.
    private static LanguageFontListData fontListData; // Holds the list of regular fonts loaded from asset bundles or resources.
    private static LanguageFontListDataTMP fontListDataTMP; // Holds the list of TMP fonts loaded from asset bundles or resources.

    private static string folderPath; // Full system path to the folder where font list files and bundles are stored.

    /// <summary>
    /// Initializes language settings and font loading on game start.
    /// </summary>
    [RuntimeInitializeOnLoadMethod]
    public static void InitializeLanguageSettings()
    {
        // Define the full path to the folder where font data will be stored.
        folderPath = Path.Combine(Application.dataPath, "FontData");

        Debug.Log("Language Startup");
        LoadLanguageSettings(); // Define the full path to the folder where font data will be stored.
        SetupDefaultLanguage(); // Set the application's language based on saved preference or system culture.

    #if !UNITY_EDITOR
        // Create the font data folder if it does not exist.
        if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);

        SaveFontListsToFile(); // Save current font lists to text files for later reuse.

        // Load regular and TMP fonts from corresponding asset bundles.
        LoadFontsFromAssetBundleLegacy();
        LoadFontsFromAssetBundleTMP();

        LoadFontListsFromFile(); // Reload and validate font list data from saved files.
    #endif
    }

    /// <summary>
    /// Loads language settings data from resources.
    /// </summary>
    private static void LoadLanguageSettings()
    {
        // Retrieve language settings data from the LanguageFileManager.
        settingsData = LanguageFileManager.LoadLanguageSettings();

        if (settingsData == null)
        {
            Debug.LogError("The languageSettingsData is not assigned.");
            return;
        }

        // Extract both regular and TMP font lists from the settings data.
        fontListData = settingsData.fontListData;
        fontListDataTMP = settingsData.fontListDataTMP;
    }

    /// <summary>
    /// Sets up the default language based on system culture or saved preferences.
    /// </summary>
    private static void SetupDefaultLanguage()
    {
        // Get the current system language culture code (e.g., "en-US").
        string systemCulture = CultureInfo.InstalledUICulture.Name;

        GetAvailableLanguages(); // Populate availableLanguages list in settingsData.

        // If a saved culture exists, use it; otherwise fall back to system culture.
        if (PlayerPrefs.HasKey("SelectedCulture"))
        {
            settingsData.selectedCulture = GetSaveCultureCode();
        }
        else
        {
            // Check whether the system culture is available in the list of supported languages.
            bool systemCultureAvailable = settingsData.availableLanguages.Any(lang => lang.culture == systemCulture && lang.isAvailable);

            // Select either the system culture or the default language as fallback.
            settingsData.selectedCulture = systemCultureAvailable ? systemCulture : settingsData.defaultLanguage;
            SetSaveCultureCode(settingsData.selectedCulture); // Save the selected culture for future launches.
        }

        GetAllData(); // Load all related language data into memory.
    }

    /// <summary>
    /// Loads regular fonts from asset bundles in the font data folder.
    /// </summary>
    #pragma warning disable IDE0051
    private static void LoadFontsFromAssetBundleLegacy()
    #pragma warning restore IDE0051
    {
        // Get all asset bundle files for legacy fonts (extension: .ltbundle).
        var bundleFiles = Directory.GetFiles(folderPath, "*.ltbundle");

        foreach (var bundleFile in bundleFiles) Debug.Log($"Bundle file found: {bundleFile}");

        foreach (var bundleFile in bundleFiles)
        {
            // Attempt to load the font asset bundle.
            var fontBundle = AssetBundle.LoadFromFile(bundleFile);
            if (fontBundle == null)
            {
                Debug.LogError($"Failed to load the font asset bundle from: {bundleFile}");
                continue;
            }

            // Ensure the font list is ready to receive assets.
            if (fontListData == null)
            {
                Debug.LogError($"LanguageFontListData not found with the specified name: {bundleFile}");
                continue;
            }

            fontListData.fontList ??= new List<Font>();

            // Load each font asset and add it to the font list.
            var fontNames = fontBundle.GetAllAssetNames();
            foreach (var fontName in fontNames)
            {
                var font = fontBundle.LoadAsset<Font>(fontName);
                if (font != null) fontListData.fontList.Add(font);
            }
        }
    }

    /// <summary>
    /// Loads TMP fonts from asset bundles in the font data folder.
    /// </summary>
    #pragma warning disable IDE0051
    private static void LoadFontsFromAssetBundleTMP()
    #pragma warning restore IDE0051
    {
        // Get all TMP font bundle files (extension: .tmpltbundle).
        var bundleFiles = Directory.GetFiles(folderPath, "*.tmpltbundle");

        foreach (var bundleFile in bundleFiles) Debug.Log($"Bundle file found: {bundleFile}");

        foreach (var bundleFile in bundleFiles)
        {
            // Attempt to load the TMP font asset bundle.
            var fontBundle = AssetBundle.LoadFromFile(bundleFile);
            if (fontBundle == null)
            {
                Debug.LogError($"Failed to load the font asset bundle from: {bundleFile}");
                continue;
            }

            // Ensure the TMP font list is initialized.
            if (fontListDataTMP == null)
            {
                Debug.LogError($"LanguageFontListDataTMP not found with the specified name: {bundleFile}");
                continue;
            }

            fontListDataTMP.TMPFontList ??= new List<TMP_FontAsset>();

            // Load each TMP font asset and add it to the font list.
            var fontNames = fontBundle.GetAllAssetNames();
            foreach (var fontName in fontNames)
            {
                var font = fontBundle.LoadAsset<TMP_FontAsset>(fontName);
                if (font != null) fontListDataTMP.TMPFontList.Add(font);
            }
        }
    }

    /// <summary>
    /// Saves current font lists to text files in the font data folder if not already present.
    /// </summary>
    #pragma warning disable IDE0051
    private static void SaveFontListsToFile()
    #pragma warning restore IDE0051
    {
        // Generate full file paths for regular and TMP font lists.
        string fontListFilePath = Path.Combine(folderPath, $"{fontListData.name}.txt");
        string fontListFilePathTMP = Path.Combine(folderPath, $"{fontListDataTMP.name}.txt");

        // Save regular font names to file if it doesn't exist.
        if (!File.Exists(fontListFilePath))
        {
            var fontNames = fontListData.fontList.Select(f => f.name);
            File.WriteAllText(fontListFilePath, string.Join("\n", fontNames));
        }

        // Save TMP font names to file if it doesn't exist.
        if (!File.Exists(fontListFilePathTMP))
        {
            var fontNamesTMP = fontListDataTMP.TMPFontList.Select(f => f.name);
            File.WriteAllText(fontListFilePathTMP, string.Join("\n", fontNamesTMP));
        }
    }

    /// <summary>
    /// Loads font lists from text files and updates the font lists accordingly.
    /// </summary>
    #pragma warning disable IDE0051
    private static void LoadFontListsFromFile()
    #pragma warning restore IDE0051
    {
        // If font data folder is missing, skip loading.
        if (!Directory.Exists(folderPath)) return;

        // Get expected paths for regular and TMP font list files.
        string fontListFilePath = Path.Combine(folderPath, $"{fontListData.name}.txt");
        string fontListFilePathTMP = Path.Combine(folderPath, $"{fontListDataTMP.name}.txt");

        // Load and filter regular fonts based on saved names.
        if (File.Exists(fontListFilePath))
        {
            var loadedFontNames = File.ReadAllLines(fontListFilePath).Select(line => line.Trim()).Where(line => !string.IsNullOrEmpty(line)).ToList();
            var updatedFontList = fontListData.fontList.Where(f => loadedFontNames.Contains(f.name)).ToList();

            fontListData.fontList.Clear();
            fontListData.fontList.AddRange(updatedFontList);
        }

        // Load and filter TMP fonts based on saved names.
        if (File.Exists(fontListFilePathTMP))
        {
            var loadedFontNamesTMP = File.ReadAllLines(fontListFilePathTMP).Select(line => line.Trim()).Where(line => !string.IsNullOrEmpty(line)).ToList();
            var updatedFontListTMP = fontListDataTMP.TMPFontList.Where(f => loadedFontNamesTMP.Contains(f.name)).ToList();

            fontListDataTMP.TMPFontList.Clear();
            fontListDataTMP.TMPFontList.AddRange(updatedFontListTMP);
        }
    }
}