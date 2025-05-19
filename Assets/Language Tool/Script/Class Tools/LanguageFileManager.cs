/*
 * ---------------------------------------------------------------------------
 * Description: Manages loading, saving, and parsing of language data in the application.
 *              Handles localization files, user preferences, and extraction of translated strings.
 * Author: Lucas Gomes Cecchini
 * Pseudonym: AGAMENOM
 * ---------------------------------------------------------------------------
*/

using System.Collections.Generic;
using UnityEngine;
using System.IO;
using TSVTools;

using static TSVTools.TabTableUtility;
using System.Linq;

namespace LanguageTools
{
    public class LanguageFileManager
    {
        private static LanguageSettingsData cachedLanguageData = null; // Stores the loaded language settings.
        private static readonly string CultureCodeKey = "SelectedCulture"; // Key for storing the culture code in PlayerPrefs.
        private static string assetsPath = null; // Cached path to the folder containing language assets.

        /// <summary>
        /// Loads the language settings from the Resources folder and caches them for future use.
        /// </summary>
        /// <returns>The loaded <see cref="LanguageSettingsData"/> object or null if loading fails.</returns>
        public static LanguageSettingsData LoadLanguageSettings()
        {
            // Return cached data if already loaded.
            if (cachedLanguageData != null) return cachedLanguageData;

            // Attempt to load the LanguageSettingsData asset from Resources.
            var languageData = Resources.Load<LanguageSettingsData>("Language Data");
            if (languageData == null)
            {
                Debug.LogError("Failed to load LanguageSettingsData from Resources. Ensure the resource exists and is named correctly.");
                return null;
            }

            // Cache and return the loaded data.
            cachedLanguageData = languageData;
            return languageData;
        }

        /// <summary>
        /// Retrieves the currently saved or default culture code from PlayerPrefs.
        /// </summary>
        /// <returns>The culture code string, or null if loading fails.</returns>
        public static string GetSaveCultureCode()
        {
            var languageData = LoadLanguageSettings();
            if (languageData == null)
            {
                Debug.LogError("Failed to load LanguageSettingsData.");
                return null;
            }

            var cultureCode = languageData.defaultLanguage; // Start with the default language.

            // Check if a value was saved previously.
            if (PlayerPrefs.HasKey(CultureCodeKey)) cultureCode = PlayerPrefs.GetString(CultureCodeKey);

            // Update the runtime-selected culture.
            languageData.selectedCulture = cultureCode;
            return cultureCode;
        }

        /// <summary>
        /// Saves the selected culture code to PlayerPrefs and updates the language settings object.
        /// </summary>
        /// <param name="newCode">The culture code to be saved.</param>
        public static void SetSaveCultureCode(string newCode)
        {
            var languageData = LoadLanguageSettings();
            if (languageData != null) languageData.selectedCulture = newCode;

            // Store and persist the new culture code.
            PlayerPrefs.SetString(CultureCodeKey, newCode);
            PlayerPrefs.Save();
        }

        /// <summary>
        /// Gets the full path to the folder where language files are stored.
        /// </summary>
        /// <returns>The language asset folder path, or null if loading fails.</returns>
        public static string GetLanguageAssetsPath()
        {
            if (!string.IsNullOrEmpty(assetsPath)) return assetsPath; // Return cached path if available.

            var languageData = LoadLanguageSettings();
            if (languageData == null)
            {
                Debug.LogError("Failed to load LanguageSettingsData.");
                return null;
            }

            assetsPath = $"{Application.streamingAssetsPath}/{languageData.folderName}"; // Construct the full path.
            if (!Directory.Exists(assetsPath)) Directory.CreateDirectory(assetsPath); // Ensure the directory exists.

            return assetsPath;
        }

        /// <summary>
        /// Determines the number of rows and columns in a vertical table structure.
        /// </summary>
        /// <param name="table">The vertical table to measure.</param>
        /// <param name="rowCount">The output number of rows.</param>
        /// <param name="columnCount">The output number of columns.</param>
        public static void RowAndColumnCounter(VerticalTable[] table, out int rowCount, out int columnCount)
        {
            if (table != null && table.Length > 0)
            {
                rowCount = table.Length;
                columnCount = table[0].horizontalTable?.Length ?? 0;
            }
            else
            {
                rowCount = 0;
                columnCount = 0;
            }
        }

        /// <summary>
        /// Parses the LanguageData.tsv file and populates available languages in the settings.
        /// </summary>
        public static void GetAvailableLanguages()
        {
            var languageData = LoadLanguageSettings();
            var assetsPath = GetLanguageAssetsPath();

            if (languageData == null || string.IsNullOrEmpty(assetsPath))
            {
                Debug.LogError("Language settings or assets path could not be loaded.");
                return;
            }

            // Load the table file from disk.
            var TSVfile = $"{assetsPath}/LanguageData.tsv";
            VerticalTable[] table = null;
            LoadTableFile(TSVfile, ref table);

            if (table == null || table.Length == 0)
            {
                Debug.LogWarning("Table is empty or not loaded. Please load a TSV file first.");
                return;
            }

            // Get the dimensions of the table.
            RowAndColumnCounter(table, out int rowCount, out int columnCount);
            if (rowCount < 3 || columnCount < 3)
            {
                Debug.LogWarning("The table does not have enough columns to extract the expected data.");
                return;
            }

            // Initialize the available languages list.
            languageData.availableLanguages = new();
            for (int col = 0; col < columnCount; col++)
            {
                if (col == 0 || col == 1) continue; // Skip ID and label columns.

                // Extract values from the current column.
                var culture = GetText(table, 0, col);
                var name = GetText(table, 1, col);
                var isAvailableText = GetText(table, 2, col);
                bool isAvailable = bool.TryParse(isAvailableText, out bool result) && result;

                if (isAvailable)
                {
                    // Add valid available language.
                    languageData.availableLanguages.Add(new()
                    {
                        culture = culture,
                        name = name,
                        isAvailable = true,
                        columnIndex = col
                    });
                }
            }
        }

        /// <summary>
        /// Extracts a list of localized ID data from a given table and culture code.
        /// </summary>
        /// <param name="table">The TSV table to parse.</param>
        /// <param name="languages">The list of available languages.</param>
        /// <param name="cultureCode">The culture code to extract data for.</param>
        /// <returns>A list of <see cref="IdData"/> objects, or null if extraction fails.</returns>
        public static List<IdData> ExtractIDs(VerticalTable[] table, List<LanguageAvailable> languages, string cultureCode)
        {
            if (table == null || table.Length == 0)
            {
                Debug.LogWarning("Table is empty or not loaded. Please load a TSV file first.");
                return null;
            }

            // Get dimensions.
            RowAndColumnCounter(table, out int rowCount, out int columnCount);
            if (rowCount < 4 || columnCount < 2)
            {
                Debug.LogWarning("The table does not have enough data to extract IDs.");
                return null;
            }

            // Find the correct column for the selected culture.
            int columnIndex = languages.FirstOrDefault(lang => lang.culture == cultureCode)?.columnIndex ?? -1;
            if (columnIndex == -1)
            {
                Debug.LogWarning($"Selected culture '{cultureCode}' not found in the table.");
                return null;
            }

            // Extract ID and corresponding localized text.
            List<IdData> ids = new();
            for (int row = 3; row < rowCount; row++)
            {
                var idText = GetText(table, row, 1);
                var text = GetText(table, row, columnIndex);

                if (int.TryParse(idText, out int id))
                {
                    ids.Add(new() { iD = id, text = text });
                }
                else
                {
                    Debug.LogWarning($"Invalid ID value at row {row}: '{idText}'");
                }
            }

            return ids;
        }

        /// <summary>
        /// Loads and parses all localization-related TSV files into the language settings structure.
        /// </summary>
        public static void GetAllData()
        {
            var languageData = LoadLanguageSettings();
            var assetsPath = GetLanguageAssetsPath();

            if (languageData == null || string.IsNullOrEmpty(assetsPath))
            {
                Debug.LogError("Language settings or assets path could not be loaded.");
                return;
            }

            // Define paths to TSV files.
            var TSVlanguage = $"{assetsPath}/LanguageData.tsv";
            var TSVmeta = $"{assetsPath}/MetaData.tsv";
            var TSVcanvas = $"{assetsPath}/CanvasData.tsv";

            // Verify file existence.
            bool allFilesExist = true;
            if (!File.Exists(TSVlanguage)) { Debug.LogError("The file 'LanguageData.tsv' was not found."); allFilesExist = false; }
            if (!File.Exists(TSVmeta)) { Debug.LogError("The file 'MetaData.tsv' was not found."); allFilesExist = false; }
            if (!File.Exists(TSVcanvas)) { Debug.LogError("The file 'CanvasData.tsv' was not found."); allFilesExist = false; }

            if (!allFilesExist) return;

            // Load all three tables.
            VerticalTable[] tableLanguage = null, tableMeta = null, tableCanvas = null;
            LoadTableFile(TSVlanguage, ref tableLanguage);
            LoadTableFile(TSVmeta, ref tableMeta);
            LoadTableFile(TSVcanvas, ref tableCanvas);

            var al = languageData.availableLanguages;
            string code = languageData.selectedCulture;

            // Fallback to default language if selected language is missing.
            if (!al.Any(lang => lang.culture == code))
            {
                languageData.selectedCulture = languageData.defaultLanguage;
                code = languageData.defaultLanguage;
                LanguageManagerDelegate.NotifyLanguageUpdate();
            }

            // Populate localization data.
            languageData.idData = ExtractIDs(tableLanguage, al, code);
            var idMetaData = ExtractIDs(tableMeta, al, code);
            languageData.idCanvasData = ExtractIDs(tableCanvas, al, code);

            // Parse metadata JSON strings into structured objects.
            List<IdMetaData> metaData = new();
            foreach (var id in idMetaData)
            {
                try
                {
                    var dataJson = JsonUtility.FromJson<IdMetaData>(id.text);
                    metaData.Add(new()
                    {
                        iD = id.iD,
                        alignment = dataJson.alignment,
                        fontSize = dataJson.fontSize,
                        fontListIndex = dataJson.fontListIndex,
                        componentType = dataJson.componentType
                    });
                }
                catch
                {
                    Debug.LogWarning($"Failed to parse JSON for ID {id.iD}");
                    metaData.Add(new() { iD = id.iD });
                }
            }

            languageData.idMetaData = metaData; // Assign parsed metadata.
        }

        /// <summary>
        /// Retrieves the localized string for a given ID from the list.
        /// </summary>
        /// <param name="ids">The list of ID data to search.</param>
        /// <param name="id">The ID to find.</param>
        /// <returns>The localized text or null if not found.</returns>
        public static string GetIDText(List<IdData> ids, int id)
        {
            // Ensure the list is populated.
            if (ids.Count == 0)
            {
                Debug.LogWarning("No IDs available. Ensure you've extracted IDs before calling this method.");
                return null;
            }

            // Find and return the text for the given ID.
            return ids.Find(data => data.iD == id)?.text;
        }

        /// <summary>
        /// Retrieves the metadata for a given ID.
        /// </summary>
        /// <param name="ids">The list of metadata to search.</param>
        /// <param name="id">The ID to find.</param>
        /// <returns>The associated <see cref="IdMetaData"/> or null if not found.</returns>
        public static IdMetaData GetIDMeta(List<IdMetaData> ids, int id)
        {
            // Ensure the list is populated.
            if (ids.Count == 0)
            {
                Debug.LogWarning("No IDs available. Ensure you've extracted IDs before calling this method.");
                return null;
            }

            // Find and return metadata for the given ID.
            return ids.Find(data => data.iD == id);
        }
    }
}