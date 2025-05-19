/*
 * ---------------------------------------------------------------------------
 * Description: Utility class for managing localization data within Unity Editor.
 *              Provides prefab search, custom GUI styles, TSV validation and parsing,
 *              and manipulation of multilingual tables for editing and display.
 * Author: Lucas Gomes Cecchini
 * Pseudonym: AGAMENOM
 * ---------------------------------------------------------------------------
*/

#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System;
using UnityEngine.SceneManagement;
using UnityEngine;
using UnityEditor;
using TSVTools;

using static LanguageTools.LanguageFileManager;
using static TSVTools.TabTableUtility;

namespace LanguageTools.Editor
{
    public class LanguageEditorUtilities
    {
        private const string fileData = "ProjectSettings/LanguageFileData.json"; // file location.

        /// <summary>
        /// Searches the asset database for a prefab by its name and returns the loaded GameObject.
        /// </summary>
        public static GameObject FindPrefabByName(string prefabName)
        {
            // Search for all prefab assets matching the given name.
            string[] guids = AssetDatabase.FindAssets($"{prefabName} t:Prefab");

            foreach (string guid in guids)
            {
                // Convert the GUID to a full asset path.
                string path = AssetDatabase.GUIDToAssetPath(guid);

                // Check if the prefab name matches (ignoring file extension and case).
                if (Path.GetFileNameWithoutExtension(path).Equals(prefabName, StringComparison.OrdinalIgnoreCase))
                    return AssetDatabase.LoadAssetAtPath<GameObject>(path); // Return the loaded GameObject.
            }

            // Log an error if no matching prefab is found.
            Debug.LogError($"Prefab with the name '{prefabName}' not found.");
            return null;
        }

        /// <summary>
        /// Creates a custom GUI label style with specified font size, optional bold and center alignment.
        /// </summary>
        public static GUIStyle CreateLabelStyle(int fontSize, bool bold = false, bool centerAlignment = false)
        {
            // Create a new GUIStyle based on the default label style.
            return new GUIStyle(GUI.skin.label)
            {
                // Set the font size and style based on the parameters.
                fontSize = fontSize,
                fontStyle = bold ? FontStyle.Bold : FontStyle.Normal,

                // Set alignment based on the parameter.
                alignment = centerAlignment ? TextAnchor.MiddleCenter : TextAnchor.MiddleLeft
            };
        }

        /// <summary>
        /// Checks whether the provided file path is a valid TSV file.
        /// </summary>
        public static bool IsInvalidTSV(string filePath) => string.IsNullOrEmpty(filePath) || !File.Exists(filePath) || Path.GetExtension(filePath) != ".tsv";

        /// <summary>
        /// Extracts language entries from a vertical TSV table using a given culture code.
        /// </summary>
        public static List<ManagerLanguageIdData> ExtractIDsEditor(VerticalTable[] table, List<LanguageAvailable> languages, string cultureCode)
        {
            // Validate if the table is loaded and not empty.
            if (table == null || table.Length == 0)
            {
                Debug.LogWarning("Table is empty or not loaded. Please load a TSV file first.");
                return null;
            }

            // Get the actual number of rows and columns in the table.
            RowAndColumnCounter(table, out int rowCount, out int columnCount);
            if (rowCount < 4 || columnCount < 2)
            {
                Debug.LogWarning("The table does not have enough data to extract IDs.");
                return null;
            }

            // Find the column index that matches the provided culture code.
            int columnIndex = languages.FirstOrDefault(l => l.culture == cultureCode)?.columnIndex ?? -1;
            if (columnIndex == -1)
            {
                Debug.LogWarning($"Selected culture '{cultureCode}' not found in the table.");
                return null;
            }

            // Prepare a list to store the extracted ID data.
            List<ManagerLanguageIdData> ids = new();
            for (int row = 3; row < rowCount; row++)
            {
                // Read values from the TSV table: ID, main text, and context.
                var idText = GetText(table, row, 1);
                var text = GetText(table, row, columnIndex);
                var textContext = GetText(table, row, 0);

                // Try to parse the ID and store the data if successful.
                if (int.TryParse(idText, out int id))
                {
                    ids.Add(new(){ iD = id, text = text, textContext = textContext });
                }
                else
                {
                    Debug.LogWarning($"Invalid ID value at row {row}: '{idText}'");
                }
            }

            return ids;
        }

        /// <summary>
        /// Inserts or updates language ID entries into the given vertical table for a specific culture.
        /// </summary>
        public static void InsertIDsEditor(ref VerticalTable[] table, List<ManagerLanguageIdData> ids, List<LanguageAvailable> languages, string cultureCode)
        {
            // Validate if the table is present.
            if (table == null || table.Length == 0)
            {
                Debug.LogWarning("Table is empty or not loaded.");
                return;
            }

            // Validate if the ID list is provided.
            if (ids == null || ids.Count == 0)
            {
                Debug.LogWarning("No ID data provided to insert.");
                return;
            }

            // Get the column index for the specified culture code.
            int columnIndex = languages.FirstOrDefault(l => l.culture == cultureCode)?.columnIndex ?? -1;
            if (columnIndex == -1)
            {
                Debug.LogWarning($"Culture '{cultureCode}' not found in language list.");
                return;
            }

            // Ensure the table has enough rows to fit the new data.
            int requiredRowCount = ids.Count + 3;
            if (table.Length < requiredRowCount)
            {
                Array.Resize(ref table, requiredRowCount);
            }

            // Ensure each row and its horizontal table is properly initialized and has enough columns.
            for (int i = 0; i < table.Length; i++)
            {
                table[i] ??= new VerticalTable();
                if (table[i].horizontalTable == null || table[i].horizontalTable.Length <= columnIndex)
                    Array.Resize(ref table[i].horizontalTable, columnIndex + 1);
            }

            // Insert the values for context, ID, and localized text into the appropriate columns.
            for (int i = 0; i < ids.Count; i++)
            {
                int row = i + 3;
                SetText(ref table, row, 0, ids[i].textContext); // Context column
                SetText(ref table, row, 1, ids[i].iD.ToString()); // ID column
                SetText(ref table, row, columnIndex, ids[i].text); // Culture-specific text column
            }
        }

        /// <summary>
        /// Synchronizes a vertical table with a new list of language ID entries, adding and removing rows as needed.
        /// </summary>
        public static void SyncTableWithIds(ref VerticalTable[] table, List<ManagerLanguageIdData> ids)
        {
            // Ensure the table is valid and has the necessary header rows.
            if (table == null || table.Length < 3)
            {
                Debug.LogWarning("Table is invalid or does not contain enough rows.");
                return;
            }

            // Build a map of existing IDs to their corresponding row indices.
            Dictionary<int, int> tableIdToRow = new();
            for (int i = 3; i < table.Length; i++)
            {
                if (int.TryParse(GetText(table, i, 1), out int id)) tableIdToRow[id] = i;
            }

            // Determine which IDs need to be removed and which need to be added.
            var currentIds = tableIdToRow.Keys.ToHashSet();
            var newIds = ids.Select(d => d.iD).ToHashSet();

            // Remove rows for IDs that are no longer present in the new data.
            foreach (var id in currentIds.Except(newIds).OrderByDescending(id => tableIdToRow[id]))
                RemoveLine(ref table, tableIdToRow[id], LineDirection.Vertical);

            // Rebuild the ID-to-row mapping after removal.
            tableIdToRow.Clear();
            for (int i = 3; i < table.Length; i++)
            {
                if (int.TryParse(GetText(table, i, 1), out int id)) tableIdToRow[id] = i;
            }

            // Add new rows for IDs not present in the current table.
            foreach (var newId in newIds.Except(tableIdToRow.Keys).OrderBy(id => id))
            {
                int insertIndex = table.Length;

                // Find the correct index to keep the table sorted by ID.
                for (int i = 3; i < table.Length; i++)
                {
                    if (int.TryParse(GetText(table, i, 1), out int existingId) && newId < existingId)
                    {
                        insertIndex = i;
                        break;
                    }
                }

                AddLine(ref table, insertIndex, LineDirection.Vertical);
            }
        }

        /// <summary>
        /// Builds a localized table header from a list of available languages.
        /// </summary>
        public static VerticalTable[] BuildTableFromAvailableLanguages(List<LanguageAvailable> availableLanguages, int isAvailable = -1)
        {
            const int rowCount = 3;

            // Ensure the language list is not null or empty.
            if (availableLanguages == null || availableLanguages.Count == 0)
            {
                Debug.LogWarning("AvailableLanguages is empty. Cannot build table.");
                return new VerticalTable[rowCount];
            }

            // Determine the total number of columns needed based on the highest language index.
            int columnCount = availableLanguages.Max(lang => lang.columnIndex) + 1;
            var table = new VerticalTable[rowCount];

            // Initialize all rows with the appropriate number of columns.
            for (int i = 0; i < rowCount; i++) table[i] = new VerticalTable { horizontalTable = new string[columnCount] };

            // Set fixed values in the first two columns (metadata and header labels).
            if (columnCount >= 2)
            {
                table[0].horizontalTable[0] = "Do not modify '% tab %' and '% newline %'. They are components to avoid reading and writing errors.";
                table[0].horizontalTable[1] = "[=]";
                table[1].horizontalTable[0] = "[=]";
                table[1].horizontalTable[1] = "[=]";
                table[2].horizontalTable[0] = "Text Context";
                table[2].horizontalTable[1] = "ID";
            }

            // Fill in language-specific data for each available language.
            foreach (var lang in availableLanguages)
            {
                int col = lang.columnIndex;
                table[0].horizontalTable[col] = lang.culture;
                table[1].horizontalTable[col] = lang.name;
                table[2].horizontalTable[col] = (col == isAvailable) ? "True" : lang.isAvailable.ToString();
            }

            return table;
        }

        /// <summary>
        /// Replaces the first 3 rows of a vertical table with another set of top rows.
        /// </summary>
        public static VerticalTable[] ReplaceTopRows(VerticalTable[] original, VerticalTable[] newTopRows)
        {
            // Validate that the original table has the expected structure.
            if (original == null || original.Length < 3) throw new ArgumentException("Original table must have at least 3 rows.");

            // Clone the original table to avoid modifying the input directly.
            var result = new VerticalTable[original.Length];
            Array.Copy(original, result, original.Length);

            // Replace the first three rows with the new ones.
            for (int i = 0; i < 3; i++) result[i] = newTopRows[i];

            return result;
        }

        /// <summary>
        /// Opens the Language File Manager and adds a language component for editing.
        /// </summary>
        public static void OpenEditorWindowWithComponent(int iD, int type, string text, int alignment, int size, int fontIndex)
        {
            // Create a new component configuration for editing.
            var data = new LanguageForEditingSave
            {
                iD = iD,
                componentType = type,
                text = text,
                alignment = alignment,
                fontSize = size,
                fontListIndex = fontIndex
            };

            // Open the editor window and inject the component into it.
            LanguageFileManagerWindow.ShowEditorWindow();
            EditorWindow.GetWindow<LanguageFileManagerWindow>().AddComponent(data);
        }

        /// <summary>
        /// Opens the Language File Manager and loads a saved canvas from JSON.
        /// </summary>
        public static void OpenEditorWindowWithCanvas(int iD, string json)
        {
            // Create a new CanvasForEditingSave instance with the provided ID and JSON data.
            var data = new CanvasForEditingSave { canvasID = iD, json = json };

            // Show the Language File Manager editor window.
            LanguageFileManagerWindow.ShowEditorWindow();

            // Retrieve the opened window and add the canvas data to it.
            EditorWindow.GetWindow<LanguageFileManagerWindow>().AddCanvas(data);
        }

        /// <summary>
        /// Creates the default database structure used to store and map available languages and cultures.
        /// </summary>
        public static VerticalTable[] CreateLanguageDataBase()
        {
            // Initialize the vertical table array with default values.
            // First table contains identifiers, keys, and locale codes.
            // Second table contains the corresponding human-readable language and culture names.
            var tableData = new VerticalTable[]
            {
                new() { horizontalTable = new string[] { "Do not modify '% tab %' and '% newline %'. They are components to avoid reading and writing errors.", "[=]", "ar", "bg", "ca", "zh-Hans", "zh-CHS", "cs", "da", "de", "el", "en", "es", "fi", "fr", "he", "hu", "is", "it", "ja", "ko", "nl", "no", "pl", "pt", "rm", "ro", "ru", "hr", "sk", "sq", "sv", "th", "tr", "ur", "id", "uk", "be", "sl", "et", "lv", "lt", "tg", "fa", "vi", "hy", "az", "eu", "hsb", "mk", "st", "ts", "tn", "xh", "zu", "af", "ka", "fo", "hi", "mt", "se", "ga", "ms", "kk", "ky", "sw", "tk", "uz", "tt", "bn", "pa", "gu", "or", "ta", "te", "kn", "ml", "as", "mr", "mn", "bo", "cy", "km", "lo", "my", "gl", "kok", "si", "chr", "am", "tzm", "ne", "fy", "ps", "fil", "ff", "ha", "yo", "nso", "lb", "kl", "ig", "om", "ti", "haw", "so", "ii", "br", "ug", "gsw", "sah", "rw", "gd", "ar-SA", "bg-BG", "ca-ES", "zh-TW", "cs-CZ", "da-DK", "de-DE", "el-GR", "en-US", "fi-FI", "fr-FR", "he-IL", "hu-HU", "is-IS", "it-IT", "ja-JP", "ko-KR", "nl-NL", "nb-NO", "pl-PL", "pt-BR", "rm-CH", "ro-RO", "ru-RU", "hr-HR", "sk-SK", "sq-AL", "sv-SE", "th-TH", "tr-TR", "ur-PK", "id-ID", "uk-UA", "be-BY", "sl-SI", "et-EE", "lv-LV", "lt-LT", "tg-Cyrl-TJ", "fa-IR", "vi-VN", "hy-AM", "az-Latn-AZ", "eu-ES", "hsb-DE", "mk-MK", "st-ZA", "ts-ZA", "tn-ZA", "xh-ZA", "zu-ZA", "af-ZA", "ka-GE", "fo-FO", "hi-IN", "mt-MT", "se-NO", "ms-MY", "kk-KZ", "ky-KG", "sw-KE", "tk-TM", "uz-Latn-UZ", "tt-RU", "bn-IN", "gu-IN", "or-IN", "ta-IN", "te-IN", "kn-IN", "ml-IN", "as-IN", "mr-IN", "mn-MN", "bo-CN", "cy-GB", "km-KH", "lo-LA", "my-MM", "gl-ES", "kok-IN", "si-LK", "am-ET", "ne-NP", "fy-NL", "ps-AF", "fil-PH", "ha-Latn-NG", "yo-NG", "nso-ZA", "lb-LU", "kl-GL", "ig-NG", "om-ET", "ti-ET", "haw-US", "so-SO", "ii-CN", "br-FR", "ug-CN", "gsw-FR", "sah-RU", "rw-RW", "gd-GB", "ar-IQ", "ca-ES-valencia", "zh-CN", "de-CH", "en-GB", "es-MX", "fr-BE", "it-CH", "nl-BE", "nn-NO", "pt-PT", "ro-MD", "ru-MD", "sv-FI", "ur-IN", "az-Cyrl-AZ", "dsb-DE", "tn-BW", "se-SE", "ga-IE", "ms-BN", "uz-Cyrl-UZ", "bn-BD", "pa-Arab-PK", "ta-LK", "ne-IN", "ti-ER", "ar-EG", "zh-HK", "de-AT", "en-AU", "es-ES", "fr-CA", "se-FI", "ar-LY", "zh-SG", "de-LU", "en-CA", "es-GT", "fr-CH", "hr-BA", "ar-DZ", "zh-MO", "de-LI", "en-NZ", "es-CR", "fr-LU", "bs-Latn-BA", "ar-MA", "en-IE", "es-PA", "fr-MC", "sr-Latn-BA", "ar-TN", "en-ZA", "es-DO", "sr-Cyrl-BA", "ar-OM", "en-JM", "es-VE", "fr-RE", "bs-Cyrl-BA", "ar-YE", "es-CO", "fr-CD", "sr-Latn-RS", "smn-FI", "ar-SY", "en-BZ", "es-PE", "fr-SN", "sr-Cyrl-RS", "ar-JO", "en-TT", "es-AR", "fr-CM", "sr-Latn-ME", "ar-LB", "en-ZW", "es-EC", "fr-CI", "sr-Cyrl-ME", "ar-KW", "en-PH", "es-CL", "fr-ML", "ar-AE", "es-UY", "fr-MA", "ar-BH", "en-HK", "es-PY", "fr-HT", "ar-QA", "en-IN", "es-BO", "en-MY", "es-SV", "en-SG", "es-HN", "es-NI", "es-PR", "es-US", "es-CU", "bs-Cyrl", "bs-Latn", "sr-Cyrl", "sr-Latn", "smn", "az-Cyrl", "zh", "nn", "bs", "az-Latn", "uz-Cyrl", "mn-Cyrl", "zh-Hant", "zh-CHT", "nb", "sr", "tg-Cyrl", "dsb", "uz-Latn", "pa-Arab", "tzm-Latn", "ha-Latn" } },
                new() { horizontalTable = new string[] { "[=]", "[=]", "Arabic", "Bulgarian", "Catalan", "Chinese (Simplified)", "Chinese (Simplified) Legacy", "Czech", "Danish", "German", "Greek", "English", "Spanish", "Finnish", "French", "Hebrew", "Hungarian", "Icelandic", "Italian", "Japanese", "Korean", "Dutch", "Norwegian", "Polish", "Portuguese", "Romansh", "Romanian", "Russian", "Croatian", "Slovak", "Albanian", "Swedish", "Thai", "Turkish", "Urdu", "Indonesian", "Ukrainian", "Belarusian", "Slovenian", "Estonian", "Latvian", "Lithuanian", "Tajik", "Persian", "Vietnamese", "Armenian", "Azerbaijani", "Basque", "Upper Sorbian", "Macedonian", "Southern Sotho", "Tsonga", "Tswana", "Xhosa", "Zulu", "Afrikaans", "Georgian", "Faroese", "Hindi", "Maltese", "Northern Sami", "Irish", "Malay", "Kazakh", "Kyrgyz", "Swahili", "Turkmen", "Uzbek", "Tatar", "Bangla", "Punjabi", "Gujarati", "Odia", "Tamil", "Telugu", "Kannada", "Malayalam", "Assamese", "Marathi", "Mongolian", "Tibetan", "Welsh", "Khmer", "Lao", "Burmese", "Galician", "Konkani", "Sinhala", "Cherokee", "Amharic", "Central Atlas Tamazight", "Nepali", "Western Frisian", "Pashto", "Filipino", "Fulah", "Hausa", "Yoruba", "Northern Sotho", "Luxembourgish", "Kalaallisut", "Igbo", "Oromo", "Tigrinya", "Hawaiian", "Somali", "Sichuan Yi", "Breton", "Uyghur", "Swiss German", "Sakha", "Kinyarwanda", "Scottish Gaelic", "Arabic (Saudi Arabia)", "Bulgarian (Bulgaria)", "Catalan (Spain)", "Chinese (Traditional)", "Czech (Czech Republic)", "Danish (Denmark)", "German (Germany)", "Greek (Greece)", "English (United States)", "Finnish (Finland)", "French (France)", "Hebrew (Israel)", "Hungarian (Hungary)", "Icelandic (Iceland)", "Italian (Italy)", "Japanese (Japan)", "Korean (South Korea)", "Dutch (Netherlands)", "Norwegian Bokm\u00e5l (Norway)", "Polish (Poland)", "Portuguese (Brazil)", "Romansh (Switzerland)", "Romanian (Romania)", "Russian (Russia)", "Croatian (Croatia)", "Slovak (Slovakia)", "Albanian (Albania)", "Swedish (Sweden)", "Thai (Thailand)", "Turkish (Turkey)", "Urdu (Pakistan)", "Indonesian (Indonesia)", "Ukrainian (Ukraine)", "Belarusian (Belarus)", "Slovenian (Slovenia)", "Estonian (Estonia)", "Latvian (Latvia)", "Lithuanian (Lithuania)", "Tajik (Cyrillic, Tajikistan)", "Persian (Iran)", "Vietnamese (Vietnam)", "Armenian (Armenia)", "Azerbaijani (Latin, Azerbaijan)", "Basque (Spain)", "Upper Sorbian (Germany)", "Macedonian (Macedonia)", "Southern Sotho (South Africa)", "Tsonga (South Africa)", "Tswana (South Africa)", "Xhosa (South Africa)", "Zulu (South Africa)", "Afrikaans (South Africa)", "Georgian (Georgia)", "Faroese (Faroe Islands)", "Hindi (India)", "Maltese (Malta)", "Northern Sami (Norway)", "Malay (Malaysia)", "Kazakh (Kazakhstan)", "Kyrgyz (Kyrgyzstan)", "Swahili (Kenya)", "Turkmen (Turkmenistan)", "Uzbek (Latin, Uzbekistan)", "Tatar (Russia)", "Bangla (India)", "Gujarati (India)", "Odia (India)", "Tamil (India)", "Telugu (India)", "Kannada (India)", "Malayalam (India)", "Assamese (India)", "Marathi (India)", "Mongolian (Mongolia)", "Tibetan (China)", "Welsh (United Kingdom)", "Khmer (Cambodia)", "Lao (Laos)", "Burmese (Myanmar (Burma))", "Galician (Spain)", "Konkani (India)", "Sinhala (Sri Lanka)", "Amharic (Ethiopia)", "Nepali (Nepal)", "Western Frisian (Netherlands)", "Pashto (Afghanistan)", "Filipino (Philippines)", "Hausa (Latin, Nigeria)", "Yoruba (Nigeria)", "Northern Sotho (South Africa)", "Luxembourgish (Luxembourg)", "Kalaallisut (Greenland)", "Igbo (Nigeria)", "Oromo (Ethiopia)", "Tigrinya (Ethiopia)", "Hawaiian (United States)", "Somali (Somalia)", "Sichuan Yi (China)", "Breton (France)", "Uyghur (China)", "Swiss German (France)", "Sakha (Russia)", "Kinyarwanda (Rwanda)", "Scottish Gaelic (United Kingdom)", "Arabic (Iraq)", "Catalan (Spain)", "Chinese (Simplified)", "German (Switzerland)", "English (United Kingdom)", "Spanish (Mexico)", "French (Belgium)", "Italian (Switzerland)", "Dutch (Belgium)", "Norwegian Nynorsk (Norway)", "Portuguese (Portugal)", "Romanian (Moldova)", "Russian (Moldova)", "Swedish (Finland)", "Urdu (India)", "Azerbaijani (Cyrillic, Azerbaijan)", "Lower Sorbian (Germany)", "Tswana (Botswana)", "Northern Sami (Sweden)", "Irish (Ireland)", "Malay (Brunei)", "Uzbek (Cyrillic, Uzbekistan)", "Bangla (Bangladesh)", "Punjabi (Arabic, Pakistan)", "Tamil (Sri Lanka)", "Nepali (India)", "Tigrinya (Eritrea)", "Arabic (Egypt)", "Chinese (Traditional, Hong Kong SAR China)", "German (Austria)", "English (Australia)", "Spanish (Spain)", "French (Canada)", "Northern Sami (Finland)", "Arabic (Libya)", "Chinese (Simplified, Singapore)", "German (Luxembourg)", "English (Canada)", "Spanish (Guatemala)", "French (Switzerland)", "Croatian (Bosnia & Herzegovina)", "Arabic (Algeria)", "Chinese (Traditional, Macau SAR China)", "German (Liechtenstein)", "English (New Zealand)", "Spanish (Costa Rica)", "French (Luxembourg)", "Bosnian (Latin, Bosnia & Herzegovina)", "Arabic (Morocco)", "English (Ireland)", "Spanish (Panama)", "French (Monaco)", "Serbian (Latin, Bosnia & Herzegovina)", "Arabic (Tunisia)", "English (South Africa)", "Spanish (Dominican Republic)", "Serbian (Cyrillic, Bosnia & Herzegovina)", "Arabic (Oman)", "English (Jamaica)", "Spanish (Venezuela)", "French (R\u00e9union)", "Bosnian (Cyrillic, Bosnia & Herzegovina)", "Arabic (Yemen)", "Spanish (Colombia)", "French (Congo - Kinshasa)", "Serbian (Latin, Serbia)", "Inari Sami (Finland)", "Arabic (Syria)", "English (Belize)", "Spanish (Peru)", "French (Senegal)", "Serbian (Cyrillic, Serbia)", "Arabic (Jordan)", "English (Trinidad & Tobago)", "Spanish (Argentina)", "French (Cameroon)", "Serbian (Latin, Montenegro)", "Arabic (Lebanon)", "English (Zimbabwe)", "Spanish (Ecuador)", "French (C\u00f4te d\u2019Ivoire)", "Serbian (Cyrillic, Montenegro)", "Arabic (Kuwait)", "English (Philippines)", "Spanish (Chile)", "French (Mali)", "Arabic (United Arab Emirates)", "Spanish (Uruguay)", "French (Morocco)", "Arabic (Bahrain)", "English (Hong Kong SAR China)", "Spanish (Paraguay)", "French (Haiti)", "Arabic (Qatar)", "English (India)", "Spanish (Bolivia)", "English (Malaysia)", "Spanish (El Salvador)", "English (Singapore)", "Spanish (Honduras)", "Spanish (Nicaragua)", "Spanish (Puerto Rico)", "Spanish (United States)", "Spanish (Cuba)", "Bosnian (Cyrillic)", "Bosnian (Latin)", "Serbian (Cyrillic)", "Serbian (Latin)", "Inari Sami", "Azerbaijani (Cyrillic)", "Chinese (Simplified)", "Norwegian Nynorsk", "Bosnian", "Azerbaijani (Latin)", "Uzbek (Cyrillic)", "Mongolian (Cyrillic)", "Chinese (Traditional)", "Chinese (Traditional) Legacy", "Norwegian Bokm\u00e5l", "Serbian", "Tajik (Cyrillic)", "Lower Sorbian", "Uzbek (Latin)", "Punjabi (Arabic)", "Central Atlas Tamazight (Latin)", "Hausa (Latin)" } },
                new() { horizontalTable = new string[] { "Text Context", "ID", "False", "False", "False", "False", "False", "False", "False", "False", "False", "False", "False", "False", "False", "False", "False", "False", "False", "False", "False", "False", "False", "False", "False", "False", "False", "False", "False", "False", "False", "False", "False", "False", "False", "False", "False", "False", "False", "False", "False", "False", "False", "False", "False", "False", "False", "False", "False", "False", "False", "False", "False", "False", "False", "False", "False", "False", "False", "False", "False", "False", "False", "False", "False", "False", "False", "False", "False", "False", "False", "False", "False", "False", "False", "False", "False", "False", "False", "False", "False", "False", "False", "False", "False", "False", "False", "False", "False", "False", "False", "False", "False", "False", "False", "False", "False", "False", "False", "False", "False", "False", "False", "False", "False", "False", "False", "False", "False", "False", "False", "False", "False", "False", "False", "False", "False", "False", "False", "False", "False", "False", "False", "False", "False", "False", "False", "False", "False", "False", "False", "False", "False", "False", "False", "False", "False", "False", "False", "False", "False", "False", "False", "False", "False", "False", "False", "False", "False", "False", "False", "False", "False", "False", "False", "False", "False", "False", "False", "False", "False", "False", "False", "False", "False", "False", "False", "False", "False", "False", "False", "False", "False", "False", "False", "False", "False", "False", "False", "False", "False", "False", "False", "False", "False", "False", "False", "False", "False", "False", "False", "False", "False", "False", "False", "False", "False", "False", "False", "False", "False", "False", "False", "False", "False", "False", "False", "False", "False", "False", "False", "False", "False", "False", "False", "False", "False", "False", "False", "False", "False", "False", "False", "False", "False", "False", "False", "False", "False", "False", "False", "False", "False", "False", "False", "False", "False", "False", "False", "False", "False", "False", "False", "False", "False", "False", "False", "False", "False", "False", "False", "False", "False", "False", "False", "False", "False", "False", "False", "False", "False", "False", "False", "False", "False", "False", "False", "False", "False", "False", "False", "False", "False", "False", "False", "False", "False", "False", "False", "False", "False", "False", "False", "False", "False", "False", "False", "False", "False", "False", "False", "False", "False", "False", "False", "False", "False", "False", "False", "False", "False", "False", "False", "False", "False", "False", "False", "False", "False", "False", "False", "False", "False", "False", "False", "False", "False", "False", "False", "False", "False", "False", "False", "False", "False", "False", "False", "False", "False", "False", "False", "False", "False", "False", "False", "False", "False", "False", "False", "False", "False", "False", "False" } },
            };

            return tableData; // Return the assembled vertical table array representing the language database.
        }

        /// <summary>
        /// Creates a GUIStyle for custom buttons with specified font size and formatting.
        /// </summary>
        /// <param name="fontSize">Font size to use in the button style.</param>
        /// <returns>Custom GUIStyle object.</returns>
        public static GUIStyle CreateCustomButtonStyle(int fontSize)
        {
            // Create and return a GUIStyle object with custom formatting.
            return new GUIStyle(GUI.skin.button)
            {
                fontSize = fontSize,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = Color.white },
                hover = { textColor = Color.red }
            };
        }

        /// <summary>
        /// Checks whether a specific language ID exists in the saved language component list.
        /// </summary>
        /// <param name="iD">ID to check.</param>
        /// <returns>True if ID is present, false otherwise.</returns>
        public static bool IsIDInLanguageList(float iD)
        {
            // Load all saved language IDs and check for the target ID.
            var ids = LoadLanguageIDs();
            return ids.Contains(iD);
        }

        /// <summary>
        /// Checks whether a specific canvas ID exists in the saved canvas list from JSON data.
        /// </summary>
        /// <param name="iD">Canvas ID to check.</param>
        /// <returns>True if ID is found, false otherwise.</returns>
        public static bool IsIDInCanvasList(float iD)
        {
            List<float> ids = new();

            // Check if the data file exists before attempting to read it.
            if (File.Exists(fileData))
            {
                var data = JsonUtility.FromJson<LanguageFileManagerWindowData>(File.ReadAllText(fileData));

                // If canvas data exists, extract the canvas IDs.
                if (data?.componentSave != null)
                {
                    foreach (var languageSaveID in data.canvasSave)
                    {
                        ids.Add(languageSaveID.canvasID);
                    }
                }
            }

            return ids.Contains(iD); // Return whether the specified ID is in the list.
        }

        /// <summary>
        /// Identifies duplicate IDs from an array of integer IDs.
        /// </summary>
        /// <param name="iDs">Array of IDs to analyze.</param>
        /// <returns>List of duplicate IDs.</returns>
        public static List<int> FindDuplicateIDs(int[] iDs)
        {
            var unique = new HashSet<int>();
            var duplicates = new List<int>();

            // Iterate through each ID and check if it already exists in the set.
            foreach (var id in iDs)
            {
                if (!unique.Add(id) && !duplicates.Contains(id)) duplicates.Add(id); // Add to duplicates if seen more than once.
            }

            return duplicates;
        }

        /// <summary>
        /// Loads all language IDs stored in the JSON configuration file.
        /// </summary>
        /// <returns>List of loaded language IDs.</returns>
        public static List<float> LoadLanguageIDs()
        {
            List<float> iDs = new();

            // Read the data file only if it exists.
            if (File.Exists(fileData))
            {
                var data = JsonUtility.FromJson<LanguageFileManagerWindowData>(File.ReadAllText(fileData));

                // If component data is available, extract the IDs.
                if (data?.componentSave != null)
                {
                    foreach (var languageSaveID in data.componentSave) iDs.Add(languageSaveID.iD);
                }
            }

            return iDs;
        }

        /// <summary>
        /// Searches and loads a texture by filename from the Unity asset database.
        /// </summary>
        /// <param name="fileName">Name of the texture file to find (without extension).</param>
        /// <returns>Texture2D object if found, otherwise null.</returns>
        public static Texture2D FindTextureByName(string fileName)
        {
            // Search for all Texture2D assets with the given name.
            string[] guids = AssetDatabase.FindAssets($"{fileName} t:Texture2D");

            foreach (string guid in guids)
            {
                string texturePath = AssetDatabase.GUIDToAssetPath(guid);
                string nameWithoutExtension = Path.GetFileNameWithoutExtension(texturePath);

                // Compare names case-insensitively to find the match.
                if (nameWithoutExtension.Equals(fileName, StringComparison.OrdinalIgnoreCase))
                {
                    return AssetDatabase.LoadAssetAtPath<Texture2D>(texturePath);
                }
            }

            // Log an error if the texture is not found.
            Debug.LogError($"Texture with the name '{fileName}' not found.");
            return null;
        }

        /// <summary>
        /// Draws a labeled text field inside a horizontal layout in the Unity Editor.
        /// </summary>
        /// <param name="label">Label displayed beside the field.</param>
        /// <param name="currentString">Current value of the text field.</param>
        /// <param name="labelSize">Width of the label area.</param>
        /// <param name="textSize">Width of the text input field.</param>
        /// <returns>Updated text string.</returns>
        public static string DrawLabeledTextField(string label, string currentString, int labelSize, int textSize)
        {
            // Begin a horizontal layout to align label and field.
            EditorGUILayout.BeginHorizontal();

            // Display the label and the editable text field.
            GUILayout.Label(label, GUILayout.Width(labelSize));
            string newText = GUILayout.TextField(currentString, GUILayout.Width(textSize));

            EditorGUILayout.EndHorizontal();
            return newText;
        }

        /// <summary>
        /// Draws a labeled integer input field in the Unity Editor.
        /// </summary>
        /// <param name="label">Label for the field.</param>
        /// <param name="currentValue">Current integer value.</param>
        /// <param name="labelSize">Width of the label section.</param>
        /// <param name="textSize">Width of the input field.</param>
        /// <returns>Updated integer value.</returns>
        public static int DrawLabeledIntField(string label, int currentValue, int labelSize, int textSize)
        {
            // Begin a horizontal layout for label and field.
            EditorGUILayout.BeginHorizontal();

            // Display the label and the integer input field.
            GUILayout.Label(label, GUILayout.Width(labelSize));
            int newValue = EditorGUILayout.IntField(currentValue, GUILayout.Width(textSize));

            EditorGUILayout.EndHorizontal();
            return newValue;
        }

        /// <summary>
        /// Draws a rectangular colored box around a block of content in the Unity Editor.
        /// </summary>
        /// <param name="content">Delegate containing the UI content to draw.</param>
        /// <param name="color">Color used to draw the box background.</param>
        public static void DrawColoredBox(Action content, Color color)
        {
            // Begin a vertical layout and draw a colored rectangle as background.
            Rect rect = EditorGUILayout.BeginVertical();
            EditorGUI.DrawRect(rect, color);
            content.Invoke(); // Invoke the content drawing action.
            EditorGUILayout.EndVertical();
        }

        /// <summary>
        /// Draws a navigation arrow button and modifies the ID index if pressed.
        /// </summary>
        /// <param name="label">Text label ("<--" or "-->") for the arrow.</param>
        /// <param name="idIndex">Current index to be modified (passed by reference).</param>
        /// <param name="minID">Minimum allowed index value.</param>
        /// <param name="maxID">Maximum allowed index value.</param>
        /// <param name="undoContext">Object to register for undo operations.</param>
        /// <param name="action">Action to invoke if the button is clicked.</param>
        public static void DrawArrowButton(string label, ref int idIndex, int minID, int maxID, UnityEngine.Object undoContext, Action action)
        {
            // Enable button only if within valid ID range based on direction.
            GUI.enabled = (label == "<--") ? idIndex > minID : idIndex < maxID;

            // Draw the button and perform action if clicked.
            if (GUILayout.Button(label, CreateCustomButtonStyle(15), GUILayout.Width(50)))
            {
                Undo.RecordObject(undoContext, "Change ID Index"); // Register undo operation.
                action(); // Invoke the provided action to change index.
                GUI.FocusControl(null); // Clear GUI focus to avoid unexpected field selections.
                EditorUtility.SetDirty(undoContext); // Mark object as dirty so Unity detects the change.
            }

            GUI.enabled = true; // Reset GUI state to enabled.
        }

        /// <summary>
        /// Adds default language component and canvas data to the provided lists.
        /// </summary>
        /// <param name="componentSave">List to receive default language components.</param>
        /// <param name="canvasSave">List to receive default canvas data.</param>
        public static void AddDefaultLanguageComponents(ref List<LanguageForEditingSave> componentSave, ref List<CanvasForEditingSave> canvasSave)
        {
            Debug.Log("Add Default Language Components");

            // Define a list of default language components using JSON strings.
            var languageDefaults = new List<LanguageForEditingSave>
            {
                JsonUtility.FromJson<LanguageForEditingSave>("{\"iD\":-10,\"text\":\"Hello World\",\"alignment\":0,\"fontSize\":0,\"fontListIndex\":1,\"textContext\":\"Base System\",\"componentType\":2}"),
                JsonUtility.FromJson<LanguageForEditingSave>("{\"iD\":-9,\"text\":\"Test Language Script\",\"alignment\":0,\"fontSize\":0,\"fontListIndex\":0,\"textContext\":\"Base System\",\"componentType\":4}"),
                JsonUtility.FromJson<LanguageForEditingSave>("{\"iD\":-8,\"text\":\"Toggle\",\"alignment\":0,\"fontSize\":0,\"fontListIndex\":1,\"textContext\":\"Base System\",\"componentType\":1}"),
                JsonUtility.FromJson<LanguageForEditingSave>("{\"iD\":-7,\"text\":\"New Text\",\"alignment\":0,\"fontSize\":0,\"fontListIndex\":1,\"textContext\":\"Base System\",\"componentType\":1}"),
                JsonUtility.FromJson<LanguageForEditingSave>("{\"iD\":-6,\"text\":\"Enter text...\",\"alignment\":0,\"fontSize\":0,\"fontListIndex\":1,\"textContext\":\"Base System\",\"componentType\":3}"),
                JsonUtility.FromJson<LanguageForEditingSave>("{\"iD\":-5,\"text\":\"Test Language\",\"alignment\":0,\"fontSize\":0,\"fontListIndex\":0,\"textContext\":\"Base System\",\"componentType\":5}"),
                JsonUtility.FromJson<LanguageForEditingSave>("{\"iD\":-4,\"text\":\"Option C\",\"alignment\":0,\"fontSize\":0,\"fontListIndex\":0,\"textContext\":\"Base System\",\"componentType\":6}"),
                JsonUtility.FromJson<LanguageForEditingSave>("{\"iD\":-3,\"text\":\"Option B\",\"alignment\":0,\"fontSize\":0,\"fontListIndex\":0,\"textContext\":\"Base System\",\"componentType\":6}"),
                JsonUtility.FromJson<LanguageForEditingSave>("{\"iD\":-2,\"text\":\"Option A\",\"alignment\":0,\"fontSize\":0,\"fontListIndex\":1,\"textContext\":\"Base System\",\"componentType\":6}"),
                JsonUtility.FromJson<LanguageForEditingSave>("{\"iD\":-1,\"text\":\"Button\",\"alignment\":0,\"fontSize\":0,\"fontListIndex\":1,\"textContext\":\"Base System\",\"componentType\":1}"),
                JsonUtility.FromJson<LanguageForEditingSave>("{\"iD\":0,\"text\":\"No language ID\",\"alignment\":0,\"fontSize\":0,\"fontListIndex\":1,\"textContext\":\"Base System\",\"componentType\":1}")
            };

            // Create default canvas data. Currently only one empty entry as a placeholder.
            var canvasDefaults = new List<CanvasForEditingSave>
            {
                JsonUtility.FromJson<CanvasForEditingSave>("{\"canvasID\":0,\"textContext\":\"Base System\",\"json\":\"{\\\"canvasName\\\":\\\"Canvas\\\",\\\"canvasLayers\\\":[{\\\"CanvasObjectsLayers\\\":[\\\"[Legacy]\\\",\\\"Language Manager (LT) [Legacy]\\\",\\\"Label\\\"],\\\"rectTransforms\\\":[{\\\"localRotation\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0,\\\"z\\\":0.0,\\\"w\\\":1.0},\\\"localPosition\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0,\\\"z\\\":0.0},\\\"localScale\\\":{\\\"x\\\":1.0,\\\"y\\\":1.0,\\\"z\\\":1.0},\\\"anchorMin\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0},\\\"anchorMax\\\":{\\\"x\\\":1.0,\\\"y\\\":1.0},\\\"anchoredPosition\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0},\\\"sizeDelta\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0},\\\"pivot\\\":{\\\"x\\\":0.5,\\\"y\\\":0.5}},{\\\"localRotation\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0,\\\"z\\\":0.0,\\\"w\\\":1.0},\\\"localPosition\\\":{\\\"x\\\":260.0,\\\"y\\\":135.0,\\\"z\\\":0.0},\\\"localScale\\\":{\\\"x\\\":1.0,\\\"y\\\":1.0,\\\"z\\\":1.0},\\\"anchorMin\\\":{\\\"x\\\":0.5,\\\"y\\\":0.5},\\\"anchorMax\\\":{\\\"x\\\":0.5,\\\"y\\\":0.5},\\\"anchoredPosition\\\":{\\\"x\\\":260.0,\\\"y\\\":135.0},\\\"sizeDelta\\\":{\\\"x\\\":160.0,\\\"y\\\":30.0},\\\"pivot\\\":{\\\"x\\\":0.5,\\\"y\\\":0.5}},{\\\"localRotation\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0,\\\"z\\\":0.0,\\\"w\\\":1.0},\\\"localPosition\\\":{\\\"x\\\":-7.5,\\\"y\\\":-0.5,\\\"z\\\":0.0},\\\"localScale\\\":{\\\"x\\\":1.0,\\\"y\\\":1.0,\\\"z\\\":1.0},\\\"anchorMin\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0},\\\"anchorMax\\\":{\\\"x\\\":1.0,\\\"y\\\":1.0},\\\"anchoredPosition\\\":{\\\"x\\\":-7.5,\\\"y\\\":-0.5},\\\"sizeDelta\\\":{\\\"x\\\":-35.0,\\\"y\\\":-13.0},\\\"pivot\\\":{\\\"x\\\":0.5,\\\"y\\\":0.5}}]},{\\\"CanvasObjectsLayers\\\":[\\\"[Legacy]\\\",\\\"Language Manager (LT) [Legacy]\\\",\\\"Arrow\\\"],\\\"rectTransforms\\\":[{\\\"localRotation\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0,\\\"z\\\":0.0,\\\"w\\\":1.0},\\\"localPosition\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0,\\\"z\\\":0.0},\\\"localScale\\\":{\\\"x\\\":1.0,\\\"y\\\":1.0,\\\"z\\\":1.0},\\\"anchorMin\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0},\\\"anchorMax\\\":{\\\"x\\\":1.0,\\\"y\\\":1.0},\\\"anchoredPosition\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0},\\\"sizeDelta\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0},\\\"pivot\\\":{\\\"x\\\":0.5,\\\"y\\\":0.5}},{\\\"localRotation\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0,\\\"z\\\":0.0,\\\"w\\\":1.0},\\\"localPosition\\\":{\\\"x\\\":260.0,\\\"y\\\":135.0,\\\"z\\\":0.0},\\\"localScale\\\":{\\\"x\\\":1.0,\\\"y\\\":1.0,\\\"z\\\":1.0},\\\"anchorMin\\\":{\\\"x\\\":0.5,\\\"y\\\":0.5},\\\"anchorMax\\\":{\\\"x\\\":0.5,\\\"y\\\":0.5},\\\"anchoredPosition\\\":{\\\"x\\\":260.0,\\\"y\\\":135.0},\\\"sizeDelta\\\":{\\\"x\\\":160.0,\\\"y\\\":30.0},\\\"pivot\\\":{\\\"x\\\":0.5,\\\"y\\\":0.5}},{\\\"localRotation\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0,\\\"z\\\":0.0,\\\"w\\\":1.0},\\\"localPosition\\\":{\\\"x\\\":65.0,\\\"y\\\":0.0,\\\"z\\\":0.0},\\\"localScale\\\":{\\\"x\\\":1.0,\\\"y\\\":1.0,\\\"z\\\":1.0},\\\"anchorMin\\\":{\\\"x\\\":1.0,\\\"y\\\":0.5},\\\"anchorMax\\\":{\\\"x\\\":1.0,\\\"y\\\":0.5},\\\"anchoredPosition\\\":{\\\"x\\\":-15.0,\\\"y\\\":0.0},\\\"sizeDelta\\\":{\\\"x\\\":20.0,\\\"y\\\":20.0},\\\"pivot\\\":{\\\"x\\\":0.5,\\\"y\\\":0.5}}]},{\\\"CanvasObjectsLayers\\\":[\\\"[Legacy]\\\",\\\"Language Manager (LT) [Legacy]\\\",\\\"Template\\\",\\\"Viewport\\\",\\\"Content\\\",\\\"Item\\\",\\\"Item Background\\\"],\\\"rectTransforms\\\":[{\\\"localRotation\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0,\\\"z\\\":0.0,\\\"w\\\":1.0},\\\"localPosition\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0,\\\"z\\\":0.0},\\\"localScale\\\":{\\\"x\\\":1.0,\\\"y\\\":1.0,\\\"z\\\":1.0},\\\"anchorMin\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0},\\\"anchorMax\\\":{\\\"x\\\":1.0,\\\"y\\\":1.0},\\\"anchoredPosition\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0},\\\"sizeDelta\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0},\\\"pivot\\\":{\\\"x\\\":0.5,\\\"y\\\":0.5}},{\\\"localRotation\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0,\\\"z\\\":0.0,\\\"w\\\":1.0},\\\"localPosition\\\":{\\\"x\\\":260.0,\\\"y\\\":135.0,\\\"z\\\":0.0},\\\"localScale\\\":{\\\"x\\\":1.0,\\\"y\\\":1.0,\\\"z\\\":1.0},\\\"anchorMin\\\":{\\\"x\\\":0.5,\\\"y\\\":0.5},\\\"anchorMax\\\":{\\\"x\\\":0.5,\\\"y\\\":0.5},\\\"anchoredPosition\\\":{\\\"x\\\":260.0,\\\"y\\\":135.0},\\\"sizeDelta\\\":{\\\"x\\\":160.0,\\\"y\\\":30.0},\\\"pivot\\\":{\\\"x\\\":0.5,\\\"y\\\":0.5}},{\\\"localRotation\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0,\\\"z\\\":0.0,\\\"w\\\":1.0},\\\"localPosition\\\":{\\\"x\\\":0.0,\\\"y\\\":-13.0,\\\"z\\\":0.0},\\\"localScale\\\":{\\\"x\\\":1.0,\\\"y\\\":1.0,\\\"z\\\":1.0},\\\"anchorMin\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0},\\\"anchorMax\\\":{\\\"x\\\":1.0,\\\"y\\\":0.0},\\\"anchoredPosition\\\":{\\\"x\\\":0.0,\\\"y\\\":2.0},\\\"sizeDelta\\\":{\\\"x\\\":0.0,\\\"y\\\":150.0},\\\"pivot\\\":{\\\"x\\\":0.5,\\\"y\\\":1.0}},{\\\"localRotation\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0,\\\"z\\\":0.0,\\\"w\\\":1.0},\\\"localPosition\\\":{\\\"x\\\":-80.0,\\\"y\\\":0.0,\\\"z\\\":0.0},\\\"localScale\\\":{\\\"x\\\":1.0,\\\"y\\\":1.0,\\\"z\\\":1.0},\\\"anchorMin\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0},\\\"anchorMax\\\":{\\\"x\\\":1.0,\\\"y\\\":1.0},\\\"anchoredPosition\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0},\\\"sizeDelta\\\":{\\\"x\\\":-18.0,\\\"y\\\":0.0},\\\"pivot\\\":{\\\"x\\\":0.0,\\\"y\\\":1.0}},{\\\"localRotation\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0,\\\"z\\\":0.0,\\\"w\\\":1.0},\\\"localPosition\\\":{\\\"x\\\":71.0,\\\"y\\\":0.0,\\\"z\\\":0.0},\\\"localScale\\\":{\\\"x\\\":1.0,\\\"y\\\":1.0,\\\"z\\\":1.0},\\\"anchorMin\\\":{\\\"x\\\":0.0,\\\"y\\\":1.0},\\\"anchorMax\\\":{\\\"x\\\":1.0,\\\"y\\\":1.0},\\\"anchoredPosition\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0},\\\"sizeDelta\\\":{\\\"x\\\":0.0,\\\"y\\\":28.0},\\\"pivot\\\":{\\\"x\\\":0.5,\\\"y\\\":1.0}},{\\\"localRotation\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0,\\\"z\\\":0.0,\\\"w\\\":1.0},\\\"localPosition\\\":{\\\"x\\\":0.0,\\\"y\\\":-14.0,\\\"z\\\":0.0},\\\"localScale\\\":{\\\"x\\\":1.0,\\\"y\\\":1.0,\\\"z\\\":1.0},\\\"anchorMin\\\":{\\\"x\\\":0.0,\\\"y\\\":0.5},\\\"anchorMax\\\":{\\\"x\\\":1.0,\\\"y\\\":0.5},\\\"anchoredPosition\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0},\\\"sizeDelta\\\":{\\\"x\\\":0.0,\\\"y\\\":20.0},\\\"pivot\\\":{\\\"x\\\":0.5,\\\"y\\\":0.5}},{\\\"localRotation\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0,\\\"z\\\":0.0,\\\"w\\\":1.0},\\\"localPosition\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0,\\\"z\\\":0.0},\\\"localScale\\\":{\\\"x\\\":1.0,\\\"y\\\":1.0,\\\"z\\\":1.0},\\\"anchorMin\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0},\\\"anchorMax\\\":{\\\"x\\\":1.0,\\\"y\\\":1.0},\\\"anchoredPosition\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0},\\\"sizeDelta\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0},\\\"pivot\\\":{\\\"x\\\":0.5,\\\"y\\\":0.5}}]},{\\\"CanvasObjectsLayers\\\":[\\\"[Legacy]\\\",\\\"Language Manager (LT) [Legacy]\\\",\\\"Template\\\",\\\"Viewport\\\",\\\"Content\\\",\\\"Item\\\",\\\"Item Checkmark\\\"],\\\"rectTransforms\\\":[{\\\"localRotation\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0,\\\"z\\\":0.0,\\\"w\\\":1.0},\\\"localPosition\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0,\\\"z\\\":0.0},\\\"localScale\\\":{\\\"x\\\":1.0,\\\"y\\\":1.0,\\\"z\\\":1.0},\\\"anchorMin\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0},\\\"anchorMax\\\":{\\\"x\\\":1.0,\\\"y\\\":1.0},\\\"anchoredPosition\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0},\\\"sizeDelta\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0},\\\"pivot\\\":{\\\"x\\\":0.5,\\\"y\\\":0.5}},{\\\"localRotation\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0,\\\"z\\\":0.0,\\\"w\\\":1.0},\\\"localPosition\\\":{\\\"x\\\":260.0,\\\"y\\\":135.0,\\\"z\\\":0.0},\\\"localScale\\\":{\\\"x\\\":1.0,\\\"y\\\":1.0,\\\"z\\\":1.0},\\\"anchorMin\\\":{\\\"x\\\":0.5,\\\"y\\\":0.5},\\\"anchorMax\\\":{\\\"x\\\":0.5,\\\"y\\\":0.5},\\\"anchoredPosition\\\":{\\\"x\\\":260.0,\\\"y\\\":135.0},\\\"sizeDelta\\\":{\\\"x\\\":160.0,\\\"y\\\":30.0},\\\"pivot\\\":{\\\"x\\\":0.5,\\\"y\\\":0.5}},{\\\"localRotation\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0,\\\"z\\\":0.0,\\\"w\\\":1.0},\\\"localPosition\\\":{\\\"x\\\":0.0,\\\"y\\\":-13.0,\\\"z\\\":0.0},\\\"localScale\\\":{\\\"x\\\":1.0,\\\"y\\\":1.0,\\\"z\\\":1.0},\\\"anchorMin\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0},\\\"anchorMax\\\":{\\\"x\\\":1.0,\\\"y\\\":0.0},\\\"anchoredPosition\\\":{\\\"x\\\":0.0,\\\"y\\\":2.0},\\\"sizeDelta\\\":{\\\"x\\\":0.0,\\\"y\\\":150.0},\\\"pivot\\\":{\\\"x\\\":0.5,\\\"y\\\":1.0}},{\\\"localRotation\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0,\\\"z\\\":0.0,\\\"w\\\":1.0},\\\"localPosition\\\":{\\\"x\\\":-80.0,\\\"y\\\":0.0,\\\"z\\\":0.0},\\\"localScale\\\":{\\\"x\\\":1.0,\\\"y\\\":1.0,\\\"z\\\":1.0},\\\"anchorMin\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0},\\\"anchorMax\\\":{\\\"x\\\":1.0,\\\"y\\\":1.0},\\\"anchoredPosition\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0},\\\"sizeDelta\\\":{\\\"x\\\":-18.0,\\\"y\\\":0.0},\\\"pivot\\\":{\\\"x\\\":0.0,\\\"y\\\":1.0}},{\\\"localRotation\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0,\\\"z\\\":0.0,\\\"w\\\":1.0},\\\"localPosition\\\":{\\\"x\\\":71.0,\\\"y\\\":0.0,\\\"z\\\":0.0},\\\"localScale\\\":{\\\"x\\\":1.0,\\\"y\\\":1.0,\\\"z\\\":1.0},\\\"anchorMin\\\":{\\\"x\\\":0.0,\\\"y\\\":1.0},\\\"anchorMax\\\":{\\\"x\\\":1.0,\\\"y\\\":1.0},\\\"anchoredPosition\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0},\\\"sizeDelta\\\":{\\\"x\\\":0.0,\\\"y\\\":28.0},\\\"pivot\\\":{\\\"x\\\":0.5,\\\"y\\\":1.0}},{\\\"localRotation\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0,\\\"z\\\":0.0,\\\"w\\\":1.0},\\\"localPosition\\\":{\\\"x\\\":0.0,\\\"y\\\":-14.0,\\\"z\\\":0.0},\\\"localScale\\\":{\\\"x\\\":1.0,\\\"y\\\":1.0,\\\"z\\\":1.0},\\\"anchorMin\\\":{\\\"x\\\":0.0,\\\"y\\\":0.5},\\\"anchorMax\\\":{\\\"x\\\":1.0,\\\"y\\\":0.5},\\\"anchoredPosition\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0},\\\"sizeDelta\\\":{\\\"x\\\":0.0,\\\"y\\\":20.0},\\\"pivot\\\":{\\\"x\\\":0.5,\\\"y\\\":0.5}},{\\\"localRotation\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0,\\\"z\\\":0.0,\\\"w\\\":1.0},\\\"localPosition\\\":{\\\"x\\\":-61.0,\\\"y\\\":0.0,\\\"z\\\":0.0},\\\"localScale\\\":{\\\"x\\\":1.0,\\\"y\\\":1.0,\\\"z\\\":1.0},\\\"anchorMin\\\":{\\\"x\\\":0.0,\\\"y\\\":0.5},\\\"anchorMax\\\":{\\\"x\\\":0.0,\\\"y\\\":0.5},\\\"anchoredPosition\\\":{\\\"x\\\":10.0,\\\"y\\\":0.0},\\\"sizeDelta\\\":{\\\"x\\\":20.0,\\\"y\\\":20.0},\\\"pivot\\\":{\\\"x\\\":0.5,\\\"y\\\":0.5}}]},{\\\"CanvasObjectsLayers\\\":[\\\"[Legacy]\\\",\\\"Language Manager (LT) [Legacy]\\\",\\\"Template\\\",\\\"Viewport\\\",\\\"Content\\\",\\\"Item\\\",\\\"Item Label\\\"],\\\"rectTransforms\\\":[{\\\"localRotation\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0,\\\"z\\\":0.0,\\\"w\\\":1.0},\\\"localPosition\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0,\\\"z\\\":0.0},\\\"localScale\\\":{\\\"x\\\":1.0,\\\"y\\\":1.0,\\\"z\\\":1.0},\\\"anchorMin\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0},\\\"anchorMax\\\":{\\\"x\\\":1.0,\\\"y\\\":1.0},\\\"anchoredPosition\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0},\\\"sizeDelta\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0},\\\"pivot\\\":{\\\"x\\\":0.5,\\\"y\\\":0.5}},{\\\"localRotation\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0,\\\"z\\\":0.0,\\\"w\\\":1.0},\\\"localPosition\\\":{\\\"x\\\":260.0,\\\"y\\\":135.0,\\\"z\\\":0.0},\\\"localScale\\\":{\\\"x\\\":1.0,\\\"y\\\":1.0,\\\"z\\\":1.0},\\\"anchorMin\\\":{\\\"x\\\":0.5,\\\"y\\\":0.5},\\\"anchorMax\\\":{\\\"x\\\":0.5,\\\"y\\\":0.5},\\\"anchoredPosition\\\":{\\\"x\\\":260.0,\\\"y\\\":135.0},\\\"sizeDelta\\\":{\\\"x\\\":160.0,\\\"y\\\":30.0},\\\"pivot\\\":{\\\"x\\\":0.5,\\\"y\\\":0.5}},{\\\"localRotation\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0,\\\"z\\\":0.0,\\\"w\\\":1.0},\\\"localPosition\\\":{\\\"x\\\":0.0,\\\"y\\\":-13.0,\\\"z\\\":0.0},\\\"localScale\\\":{\\\"x\\\":1.0,\\\"y\\\":1.0,\\\"z\\\":1.0},\\\"anchorMin\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0},\\\"anchorMax\\\":{\\\"x\\\":1.0,\\\"y\\\":0.0},\\\"anchoredPosition\\\":{\\\"x\\\":0.0,\\\"y\\\":2.0},\\\"sizeDelta\\\":{\\\"x\\\":0.0,\\\"y\\\":150.0},\\\"pivot\\\":{\\\"x\\\":0.5,\\\"y\\\":1.0}},{\\\"localRotation\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0,\\\"z\\\":0.0,\\\"w\\\":1.0},\\\"localPosition\\\":{\\\"x\\\":-80.0,\\\"y\\\":0.0,\\\"z\\\":0.0},\\\"localScale\\\":{\\\"x\\\":1.0,\\\"y\\\":1.0,\\\"z\\\":1.0},\\\"anchorMin\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0},\\\"anchorMax\\\":{\\\"x\\\":1.0,\\\"y\\\":1.0},\\\"anchoredPosition\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0},\\\"sizeDelta\\\":{\\\"x\\\":-18.0,\\\"y\\\":0.0},\\\"pivot\\\":{\\\"x\\\":0.0,\\\"y\\\":1.0}},{\\\"localRotation\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0,\\\"z\\\":0.0,\\\"w\\\":1.0},\\\"localPosition\\\":{\\\"x\\\":71.0,\\\"y\\\":0.0,\\\"z\\\":0.0},\\\"localScale\\\":{\\\"x\\\":1.0,\\\"y\\\":1.0,\\\"z\\\":1.0},\\\"anchorMin\\\":{\\\"x\\\":0.0,\\\"y\\\":1.0},\\\"anchorMax\\\":{\\\"x\\\":1.0,\\\"y\\\":1.0},\\\"anchoredPosition\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0},\\\"sizeDelta\\\":{\\\"x\\\":0.0,\\\"y\\\":28.0},\\\"pivot\\\":{\\\"x\\\":0.5,\\\"y\\\":1.0}},{\\\"localRotation\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0,\\\"z\\\":0.0,\\\"w\\\":1.0},\\\"localPosition\\\":{\\\"x\\\":0.0,\\\"y\\\":-14.0,\\\"z\\\":0.0},\\\"localScale\\\":{\\\"x\\\":1.0,\\\"y\\\":1.0,\\\"z\\\":1.0},\\\"anchorMin\\\":{\\\"x\\\":0.0,\\\"y\\\":0.5},\\\"anchorMax\\\":{\\\"x\\\":1.0,\\\"y\\\":0.5},\\\"anchoredPosition\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0},\\\"sizeDelta\\\":{\\\"x\\\":0.0,\\\"y\\\":20.0},\\\"pivot\\\":{\\\"x\\\":0.5,\\\"y\\\":0.5}},{\\\"localRotation\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0,\\\"z\\\":0.0,\\\"w\\\":1.0},\\\"localPosition\\\":{\\\"x\\\":5.0,\\\"y\\\":-0.5,\\\"z\\\":0.0},\\\"localScale\\\":{\\\"x\\\":1.0,\\\"y\\\":1.0,\\\"z\\\":1.0},\\\"anchorMin\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0},\\\"anchorMax\\\":{\\\"x\\\":1.0,\\\"y\\\":1.0},\\\"anchoredPosition\\\":{\\\"x\\\":5.0,\\\"y\\\":-0.5},\\\"sizeDelta\\\":{\\\"x\\\":-30.0,\\\"y\\\":-3.0},\\\"pivot\\\":{\\\"x\\\":0.5,\\\"y\\\":0.5}}]},{\\\"CanvasObjectsLayers\\\":[\\\"[Legacy]\\\",\\\"Language Manager (LT) [Legacy]\\\",\\\"Template\\\",\\\"Scrollbar\\\",\\\"Sliding Area\\\",\\\"Handle\\\"],\\\"rectTransforms\\\":[{\\\"localRotation\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0,\\\"z\\\":0.0,\\\"w\\\":1.0},\\\"localPosition\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0,\\\"z\\\":0.0},\\\"localScale\\\":{\\\"x\\\":1.0,\\\"y\\\":1.0,\\\"z\\\":1.0},\\\"anchorMin\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0},\\\"anchorMax\\\":{\\\"x\\\":1.0,\\\"y\\\":1.0},\\\"anchoredPosition\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0},\\\"sizeDelta\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0},\\\"pivot\\\":{\\\"x\\\":0.5,\\\"y\\\":0.5}},{\\\"localRotation\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0,\\\"z\\\":0.0,\\\"w\\\":1.0},\\\"localPosition\\\":{\\\"x\\\":260.0,\\\"y\\\":135.0,\\\"z\\\":0.0},\\\"localScale\\\":{\\\"x\\\":1.0,\\\"y\\\":1.0,\\\"z\\\":1.0},\\\"anchorMin\\\":{\\\"x\\\":0.5,\\\"y\\\":0.5},\\\"anchorMax\\\":{\\\"x\\\":0.5,\\\"y\\\":0.5},\\\"anchoredPosition\\\":{\\\"x\\\":260.0,\\\"y\\\":135.0},\\\"sizeDelta\\\":{\\\"x\\\":160.0,\\\"y\\\":30.0},\\\"pivot\\\":{\\\"x\\\":0.5,\\\"y\\\":0.5}},{\\\"localRotation\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0,\\\"z\\\":0.0,\\\"w\\\":1.0},\\\"localPosition\\\":{\\\"x\\\":0.0,\\\"y\\\":-13.0,\\\"z\\\":0.0},\\\"localScale\\\":{\\\"x\\\":1.0,\\\"y\\\":1.0,\\\"z\\\":1.0},\\\"anchorMin\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0},\\\"anchorMax\\\":{\\\"x\\\":1.0,\\\"y\\\":0.0},\\\"anchoredPosition\\\":{\\\"x\\\":0.0,\\\"y\\\":2.0},\\\"sizeDelta\\\":{\\\"x\\\":0.0,\\\"y\\\":150.0},\\\"pivot\\\":{\\\"x\\\":0.5,\\\"y\\\":1.0}},{\\\"localRotation\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0,\\\"z\\\":0.0,\\\"w\\\":1.0},\\\"localPosition\\\":{\\\"x\\\":80.0,\\\"y\\\":0.0,\\\"z\\\":0.0},\\\"localScale\\\":{\\\"x\\\":1.0,\\\"y\\\":1.0,\\\"z\\\":1.0},\\\"anchorMin\\\":{\\\"x\\\":1.0,\\\"y\\\":0.0},\\\"anchorMax\\\":{\\\"x\\\":1.0,\\\"y\\\":1.0},\\\"anchoredPosition\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0},\\\"sizeDelta\\\":{\\\"x\\\":20.0,\\\"y\\\":0.0},\\\"pivot\\\":{\\\"x\\\":1.0,\\\"y\\\":1.0}},{\\\"localRotation\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0,\\\"z\\\":0.0,\\\"w\\\":1.0},\\\"localPosition\\\":{\\\"x\\\":-10.0,\\\"y\\\":-75.0,\\\"z\\\":0.0},\\\"localScale\\\":{\\\"x\\\":1.0,\\\"y\\\":1.0,\\\"z\\\":1.0},\\\"anchorMin\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0},\\\"anchorMax\\\":{\\\"x\\\":1.0,\\\"y\\\":1.0},\\\"anchoredPosition\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0},\\\"sizeDelta\\\":{\\\"x\\\":-20.0,\\\"y\\\":-20.0},\\\"pivot\\\":{\\\"x\\\":0.5,\\\"y\\\":0.5}},{\\\"localRotation\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0,\\\"z\\\":0.0,\\\"w\\\":1.0},\\\"localPosition\\\":{\\\"x\\\":0.0,\\\"y\\\":-52.0,\\\"z\\\":0.0},\\\"localScale\\\":{\\\"x\\\":1.0,\\\"y\\\":1.0,\\\"z\\\":1.0},\\\"anchorMin\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0},\\\"anchorMax\\\":{\\\"x\\\":1.0,\\\"y\\\":0.20000000298023225},\\\"anchoredPosition\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0},\\\"sizeDelta\\\":{\\\"x\\\":20.0,\\\"y\\\":20.0},\\\"pivot\\\":{\\\"x\\\":0.5,\\\"y\\\":0.5}}]},{\\\"CanvasObjectsLayers\\\":[\\\"[Legacy]\\\",\\\"Button (LT) [Legacy]\\\",\\\"Text (LT) [Legacy]\\\"],\\\"rectTransforms\\\":[{\\\"localRotation\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0,\\\"z\\\":0.0,\\\"w\\\":1.0},\\\"localPosition\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0,\\\"z\\\":0.0},\\\"localScale\\\":{\\\"x\\\":1.0,\\\"y\\\":1.0,\\\"z\\\":1.0},\\\"anchorMin\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0},\\\"anchorMax\\\":{\\\"x\\\":1.0,\\\"y\\\":1.0},\\\"anchoredPosition\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0},\\\"sizeDelta\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0},\\\"pivot\\\":{\\\"x\\\":0.5,\\\"y\\\":0.5}},{\\\"localRotation\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0,\\\"z\\\":0.0,\\\"w\\\":1.0},\\\"localPosition\\\":{\\\"x\\\":260.0,\\\"y\\\":85.0,\\\"z\\\":0.0},\\\"localScale\\\":{\\\"x\\\":1.0,\\\"y\\\":1.0,\\\"z\\\":1.0},\\\"anchorMin\\\":{\\\"x\\\":0.5,\\\"y\\\":0.5},\\\"anchorMax\\\":{\\\"x\\\":0.5,\\\"y\\\":0.5},\\\"anchoredPosition\\\":{\\\"x\\\":260.0,\\\"y\\\":85.0},\\\"sizeDelta\\\":{\\\"x\\\":160.0,\\\"y\\\":30.0},\\\"pivot\\\":{\\\"x\\\":0.5,\\\"y\\\":0.5}},{\\\"localRotation\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0,\\\"z\\\":0.0,\\\"w\\\":1.0},\\\"localPosition\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0,\\\"z\\\":0.0},\\\"localScale\\\":{\\\"x\\\":1.0,\\\"y\\\":1.0,\\\"z\\\":1.0},\\\"anchorMin\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0},\\\"anchorMax\\\":{\\\"x\\\":1.0,\\\"y\\\":1.0},\\\"anchoredPosition\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0},\\\"sizeDelta\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0},\\\"pivot\\\":{\\\"x\\\":0.5,\\\"y\\\":0.5}}]},{\\\"CanvasObjectsLayers\\\":[\\\"[Legacy]\\\",\\\"Dropdown (LT) [Legacy]\\\",\\\"Label\\\"],\\\"rectTransforms\\\":[{\\\"localRotation\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0,\\\"z\\\":0.0,\\\"w\\\":1.0},\\\"localPosition\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0,\\\"z\\\":0.0},\\\"localScale\\\":{\\\"x\\\":1.0,\\\"y\\\":1.0,\\\"z\\\":1.0},\\\"anchorMin\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0},\\\"anchorMax\\\":{\\\"x\\\":1.0,\\\"y\\\":1.0},\\\"anchoredPosition\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0},\\\"sizeDelta\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0},\\\"pivot\\\":{\\\"x\\\":0.5,\\\"y\\\":0.5}},{\\\"localRotation\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0,\\\"z\\\":0.0,\\\"w\\\":1.0},\\\"localPosition\\\":{\\\"x\\\":260.0,\\\"y\\\":30.0,\\\"z\\\":0.0},\\\"localScale\\\":{\\\"x\\\":1.0,\\\"y\\\":1.0,\\\"z\\\":1.0},\\\"anchorMin\\\":{\\\"x\\\":0.5,\\\"y\\\":0.5},\\\"anchorMax\\\":{\\\"x\\\":0.5,\\\"y\\\":0.5},\\\"anchoredPosition\\\":{\\\"x\\\":260.0,\\\"y\\\":30.0},\\\"sizeDelta\\\":{\\\"x\\\":160.0,\\\"y\\\":30.0},\\\"pivot\\\":{\\\"x\\\":0.5,\\\"y\\\":0.5}},{\\\"localRotation\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0,\\\"z\\\":0.0,\\\"w\\\":1.0},\\\"localPosition\\\":{\\\"x\\\":-7.5,\\\"y\\\":-0.5,\\\"z\\\":0.0},\\\"localScale\\\":{\\\"x\\\":1.0,\\\"y\\\":1.0,\\\"z\\\":1.0},\\\"anchorMin\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0},\\\"anchorMax\\\":{\\\"x\\\":1.0,\\\"y\\\":1.0},\\\"anchoredPosition\\\":{\\\"x\\\":-7.5,\\\"y\\\":-0.5},\\\"sizeDelta\\\":{\\\"x\\\":-35.0,\\\"y\\\":-13.0},\\\"pivot\\\":{\\\"x\\\":0.5,\\\"y\\\":0.5}}]},{\\\"CanvasObjectsLayers\\\":[\\\"[Legacy]\\\",\\\"Dropdown (LT) [Legacy]\\\",\\\"Arrow\\\"],\\\"rectTransforms\\\":[{\\\"localRotation\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0,\\\"z\\\":0.0,\\\"w\\\":1.0},\\\"localPosition\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0,\\\"z\\\":0.0},\\\"localScale\\\":{\\\"x\\\":1.0,\\\"y\\\":1.0,\\\"z\\\":1.0},\\\"anchorMin\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0},\\\"anchorMax\\\":{\\\"x\\\":1.0,\\\"y\\\":1.0},\\\"anchoredPosition\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0},\\\"sizeDelta\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0},\\\"pivot\\\":{\\\"x\\\":0.5,\\\"y\\\":0.5}},{\\\"localRotation\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0,\\\"z\\\":0.0,\\\"w\\\":1.0},\\\"localPosition\\\":{\\\"x\\\":260.0,\\\"y\\\":30.0,\\\"z\\\":0.0},\\\"localScale\\\":{\\\"x\\\":1.0,\\\"y\\\":1.0,\\\"z\\\":1.0},\\\"anchorMin\\\":{\\\"x\\\":0.5,\\\"y\\\":0.5},\\\"anchorMax\\\":{\\\"x\\\":0.5,\\\"y\\\":0.5},\\\"anchoredPosition\\\":{\\\"x\\\":260.0,\\\"y\\\":30.0},\\\"sizeDelta\\\":{\\\"x\\\":160.0,\\\"y\\\":30.0},\\\"pivot\\\":{\\\"x\\\":0.5,\\\"y\\\":0.5}},{\\\"localRotation\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0,\\\"z\\\":0.0,\\\"w\\\":1.0},\\\"localPosition\\\":{\\\"x\\\":65.0,\\\"y\\\":0.0,\\\"z\\\":0.0},\\\"localScale\\\":{\\\"x\\\":1.0,\\\"y\\\":1.0,\\\"z\\\":1.0},\\\"anchorMin\\\":{\\\"x\\\":1.0,\\\"y\\\":0.5},\\\"anchorMax\\\":{\\\"x\\\":1.0,\\\"y\\\":0.5},\\\"anchoredPosition\\\":{\\\"x\\\":-15.0,\\\"y\\\":0.0},\\\"sizeDelta\\\":{\\\"x\\\":20.0,\\\"y\\\":20.0},\\\"pivot\\\":{\\\"x\\\":0.5,\\\"y\\\":0.5}}]},{\\\"CanvasObjectsLayers\\\":[\\\"[Legacy]\\\",\\\"Dropdown (LT) [Legacy]\\\",\\\"Template\\\",\\\"Viewport\\\",\\\"Content\\\",\\\"Item\\\",\\\"Item Background\\\"],\\\"rectTransforms\\\":[{\\\"localRotation\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0,\\\"z\\\":0.0,\\\"w\\\":1.0},\\\"localPosition\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0,\\\"z\\\":0.0},\\\"localScale\\\":{\\\"x\\\":1.0,\\\"y\\\":1.0,\\\"z\\\":1.0},\\\"anchorMin\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0},\\\"anchorMax\\\":{\\\"x\\\":1.0,\\\"y\\\":1.0},\\\"anchoredPosition\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0},\\\"sizeDelta\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0},\\\"pivot\\\":{\\\"x\\\":0.5,\\\"y\\\":0.5}},{\\\"localRotation\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0,\\\"z\\\":0.0,\\\"w\\\":1.0},\\\"localPosition\\\":{\\\"x\\\":260.0,\\\"y\\\":30.0,\\\"z\\\":0.0},\\\"localScale\\\":{\\\"x\\\":1.0,\\\"y\\\":1.0,\\\"z\\\":1.0},\\\"anchorMin\\\":{\\\"x\\\":0.5,\\\"y\\\":0.5},\\\"anchorMax\\\":{\\\"x\\\":0.5,\\\"y\\\":0.5},\\\"anchoredPosition\\\":{\\\"x\\\":260.0,\\\"y\\\":30.0},\\\"sizeDelta\\\":{\\\"x\\\":160.0,\\\"y\\\":30.0},\\\"pivot\\\":{\\\"x\\\":0.5,\\\"y\\\":0.5}},{\\\"localRotation\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0,\\\"z\\\":0.0,\\\"w\\\":1.0},\\\"localPosition\\\":{\\\"x\\\":0.0,\\\"y\\\":-13.0,\\\"z\\\":0.0},\\\"localScale\\\":{\\\"x\\\":1.0,\\\"y\\\":1.0,\\\"z\\\":1.0},\\\"anchorMin\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0},\\\"anchorMax\\\":{\\\"x\\\":1.0,\\\"y\\\":0.0},\\\"anchoredPosition\\\":{\\\"x\\\":0.0,\\\"y\\\":2.0},\\\"sizeDelta\\\":{\\\"x\\\":0.0,\\\"y\\\":150.0},\\\"pivot\\\":{\\\"x\\\":0.5,\\\"y\\\":1.0}},{\\\"localRotation\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0,\\\"z\\\":0.0,\\\"w\\\":1.0},\\\"localPosition\\\":{\\\"x\\\":-80.0,\\\"y\\\":0.0,\\\"z\\\":0.0},\\\"localScale\\\":{\\\"x\\\":1.0,\\\"y\\\":1.0,\\\"z\\\":1.0},\\\"anchorMin\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0},\\\"anchorMax\\\":{\\\"x\\\":1.0,\\\"y\\\":1.0},\\\"anchoredPosition\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0},\\\"sizeDelta\\\":{\\\"x\\\":-18.0,\\\"y\\\":0.0},\\\"pivot\\\":{\\\"x\\\":0.0,\\\"y\\\":1.0}},{\\\"localRotation\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0,\\\"z\\\":0.0,\\\"w\\\":1.0},\\\"localPosition\\\":{\\\"x\\\":71.0,\\\"y\\\":0.0,\\\"z\\\":0.0},\\\"localScale\\\":{\\\"x\\\":1.0,\\\"y\\\":1.0,\\\"z\\\":1.0},\\\"anchorMin\\\":{\\\"x\\\":0.0,\\\"y\\\":1.0},\\\"anchorMax\\\":{\\\"x\\\":1.0,\\\"y\\\":1.0},\\\"anchoredPosition\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0},\\\"sizeDelta\\\":{\\\"x\\\":0.0,\\\"y\\\":28.0},\\\"pivot\\\":{\\\"x\\\":0.5,\\\"y\\\":1.0}},{\\\"localRotation\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0,\\\"z\\\":0.0,\\\"w\\\":1.0},\\\"localPosition\\\":{\\\"x\\\":0.0,\\\"y\\\":-14.0,\\\"z\\\":0.0},\\\"localScale\\\":{\\\"x\\\":1.0,\\\"y\\\":1.0,\\\"z\\\":1.0},\\\"anchorMin\\\":{\\\"x\\\":0.0,\\\"y\\\":0.5},\\\"anchorMax\\\":{\\\"x\\\":1.0,\\\"y\\\":0.5},\\\"anchoredPosition\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0},\\\"sizeDelta\\\":{\\\"x\\\":0.0,\\\"y\\\":20.0},\\\"pivot\\\":{\\\"x\\\":0.5,\\\"y\\\":0.5}},{\\\"localRotation\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0,\\\"z\\\":0.0,\\\"w\\\":1.0},\\\"localPosition\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0,\\\"z\\\":0.0},\\\"localScale\\\":{\\\"x\\\":1.0,\\\"y\\\":1.0,\\\"z\\\":1.0},\\\"anchorMin\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0},\\\"anchorMax\\\":{\\\"x\\\":1.0,\\\"y\\\":1.0},\\\"anchoredPosition\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0},\\\"sizeDelta\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0},\\\"pivot\\\":{\\\"x\\\":0.5,\\\"y\\\":0.5}}]},{\\\"CanvasObjectsLayers\\\":[\\\"[Legacy]\\\",\\\"Dropdown (LT) [Legacy]\\\",\\\"Template\\\",\\\"Viewport\\\",\\\"Content\\\",\\\"Item\\\",\\\"Item Checkmark\\\"],\\\"rectTransforms\\\":[{\\\"localRotation\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0,\\\"z\\\":0.0,\\\"w\\\":1.0},\\\"localPosition\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0,\\\"z\\\":0.0},\\\"localScale\\\":{\\\"x\\\":1.0,\\\"y\\\":1.0,\\\"z\\\":1.0},\\\"anchorMin\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0},\\\"anchorMax\\\":{\\\"x\\\":1.0,\\\"y\\\":1.0},\\\"anchoredPosition\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0},\\\"sizeDelta\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0},\\\"pivot\\\":{\\\"x\\\":0.5,\\\"y\\\":0.5}},{\\\"localRotation\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0,\\\"z\\\":0.0,\\\"w\\\":1.0},\\\"localPosition\\\":{\\\"x\\\":260.0,\\\"y\\\":30.0,\\\"z\\\":0.0},\\\"localScale\\\":{\\\"x\\\":1.0,\\\"y\\\":1.0,\\\"z\\\":1.0},\\\"anchorMin\\\":{\\\"x\\\":0.5,\\\"y\\\":0.5},\\\"anchorMax\\\":{\\\"x\\\":0.5,\\\"y\\\":0.5},\\\"anchoredPosition\\\":{\\\"x\\\":260.0,\\\"y\\\":30.0},\\\"sizeDelta\\\":{\\\"x\\\":160.0,\\\"y\\\":30.0},\\\"pivot\\\":{\\\"x\\\":0.5,\\\"y\\\":0.5}},{\\\"localRotation\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0,\\\"z\\\":0.0,\\\"w\\\":1.0},\\\"localPosition\\\":{\\\"x\\\":0.0,\\\"y\\\":-13.0,\\\"z\\\":0.0},\\\"localScale\\\":{\\\"x\\\":1.0,\\\"y\\\":1.0,\\\"z\\\":1.0},\\\"anchorMin\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0},\\\"anchorMax\\\":{\\\"x\\\":1.0,\\\"y\\\":0.0},\\\"anchoredPosition\\\":{\\\"x\\\":0.0,\\\"y\\\":2.0},\\\"sizeDelta\\\":{\\\"x\\\":0.0,\\\"y\\\":150.0},\\\"pivot\\\":{\\\"x\\\":0.5,\\\"y\\\":1.0}},{\\\"localRotation\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0,\\\"z\\\":0.0,\\\"w\\\":1.0},\\\"localPosition\\\":{\\\"x\\\":-80.0,\\\"y\\\":0.0,\\\"z\\\":0.0},\\\"localScale\\\":{\\\"x\\\":1.0,\\\"y\\\":1.0,\\\"z\\\":1.0},\\\"anchorMin\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0},\\\"anchorMax\\\":{\\\"x\\\":1.0,\\\"y\\\":1.0},\\\"anchoredPosition\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0},\\\"sizeDelta\\\":{\\\"x\\\":-18.0,\\\"y\\\":0.0},\\\"pivot\\\":{\\\"x\\\":0.0,\\\"y\\\":1.0}},{\\\"localRotation\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0,\\\"z\\\":0.0,\\\"w\\\":1.0},\\\"localPosition\\\":{\\\"x\\\":71.0,\\\"y\\\":0.0,\\\"z\\\":0.0},\\\"localScale\\\":{\\\"x\\\":1.0,\\\"y\\\":1.0,\\\"z\\\":1.0},\\\"anchorMin\\\":{\\\"x\\\":0.0,\\\"y\\\":1.0},\\\"anchorMax\\\":{\\\"x\\\":1.0,\\\"y\\\":1.0},\\\"anchoredPosition\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0},\\\"sizeDelta\\\":{\\\"x\\\":0.0,\\\"y\\\":28.0},\\\"pivot\\\":{\\\"x\\\":0.5,\\\"y\\\":1.0}},{\\\"localRotation\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0,\\\"z\\\":0.0,\\\"w\\\":1.0},\\\"localPosition\\\":{\\\"x\\\":0.0,\\\"y\\\":-14.0,\\\"z\\\":0.0},\\\"localScale\\\":{\\\"x\\\":1.0,\\\"y\\\":1.0,\\\"z\\\":1.0},\\\"anchorMin\\\":{\\\"x\\\":0.0,\\\"y\\\":0.5},\\\"anchorMax\\\":{\\\"x\\\":1.0,\\\"y\\\":0.5},\\\"anchoredPosition\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0},\\\"sizeDelta\\\":{\\\"x\\\":0.0,\\\"y\\\":20.0},\\\"pivot\\\":{\\\"x\\\":0.5,\\\"y\\\":0.5}},{\\\"localRotation\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0,\\\"z\\\":0.0,\\\"w\\\":1.0},\\\"localPosition\\\":{\\\"x\\\":-61.0,\\\"y\\\":0.0,\\\"z\\\":0.0},\\\"localScale\\\":{\\\"x\\\":1.0,\\\"y\\\":1.0,\\\"z\\\":1.0},\\\"anchorMin\\\":{\\\"x\\\":0.0,\\\"y\\\":0.5},\\\"anchorMax\\\":{\\\"x\\\":0.0,\\\"y\\\":0.5},\\\"anchoredPosition\\\":{\\\"x\\\":10.0,\\\"y\\\":0.0},\\\"sizeDelta\\\":{\\\"x\\\":20.0,\\\"y\\\":20.0},\\\"pivot\\\":{\\\"x\\\":0.5,\\\"y\\\":0.5}}]},{\\\"CanvasObjectsLayers\\\":[\\\"[Legacy]\\\",\\\"Dropdown (LT) [Legacy]\\\",\\\"Template\\\",\\\"Viewport\\\",\\\"Content\\\",\\\"Item\\\",\\\"Item Label\\\"],\\\"rectTransforms\\\":[{\\\"localRotation\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0,\\\"z\\\":0.0,\\\"w\\\":1.0},\\\"localPosition\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0,\\\"z\\\":0.0},\\\"localScale\\\":{\\\"x\\\":1.0,\\\"y\\\":1.0,\\\"z\\\":1.0},\\\"anchorMin\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0},\\\"anchorMax\\\":{\\\"x\\\":1.0,\\\"y\\\":1.0},\\\"anchoredPosition\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0},\\\"sizeDelta\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0},\\\"pivot\\\":{\\\"x\\\":0.5,\\\"y\\\":0.5}},{\\\"localRotation\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0,\\\"z\\\":0.0,\\\"w\\\":1.0},\\\"localPosition\\\":{\\\"x\\\":260.0,\\\"y\\\":30.0,\\\"z\\\":0.0},\\\"localScale\\\":{\\\"x\\\":1.0,\\\"y\\\":1.0,\\\"z\\\":1.0},\\\"anchorMin\\\":{\\\"x\\\":0.5,\\\"y\\\":0.5},\\\"anchorMax\\\":{\\\"x\\\":0.5,\\\"y\\\":0.5},\\\"anchoredPosition\\\":{\\\"x\\\":260.0,\\\"y\\\":30.0},\\\"sizeDelta\\\":{\\\"x\\\":160.0,\\\"y\\\":30.0},\\\"pivot\\\":{\\\"x\\\":0.5,\\\"y\\\":0.5}},{\\\"localRotation\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0,\\\"z\\\":0.0,\\\"w\\\":1.0},\\\"localPosition\\\":{\\\"x\\\":0.0,\\\"y\\\":-13.0,\\\"z\\\":0.0},\\\"localScale\\\":{\\\"x\\\":1.0,\\\"y\\\":1.0,\\\"z\\\":1.0},\\\"anchorMin\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0},\\\"anchorMax\\\":{\\\"x\\\":1.0,\\\"y\\\":0.0},\\\"anchoredPosition\\\":{\\\"x\\\":0.0,\\\"y\\\":2.0},\\\"sizeDelta\\\":{\\\"x\\\":0.0,\\\"y\\\":150.0},\\\"pivot\\\":{\\\"x\\\":0.5,\\\"y\\\":1.0}},{\\\"localRotation\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0,\\\"z\\\":0.0,\\\"w\\\":1.0},\\\"localPosition\\\":{\\\"x\\\":-80.0,\\\"y\\\":0.0,\\\"z\\\":0.0},\\\"localScale\\\":{\\\"x\\\":1.0,\\\"y\\\":1.0,\\\"z\\\":1.0},\\\"anchorMin\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0},\\\"anchorMax\\\":{\\\"x\\\":1.0,\\\"y\\\":1.0},\\\"anchoredPosition\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0},\\\"sizeDelta\\\":{\\\"x\\\":-18.0,\\\"y\\\":0.0},\\\"pivot\\\":{\\\"x\\\":0.0,\\\"y\\\":1.0}},{\\\"localRotation\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0,\\\"z\\\":0.0,\\\"w\\\":1.0},\\\"localPosition\\\":{\\\"x\\\":71.0,\\\"y\\\":0.0,\\\"z\\\":0.0},\\\"localScale\\\":{\\\"x\\\":1.0,\\\"y\\\":1.0,\\\"z\\\":1.0},\\\"anchorMin\\\":{\\\"x\\\":0.0,\\\"y\\\":1.0},\\\"anchorMax\\\":{\\\"x\\\":1.0,\\\"y\\\":1.0},\\\"anchoredPosition\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0},\\\"sizeDelta\\\":{\\\"x\\\":0.0,\\\"y\\\":28.0},\\\"pivot\\\":{\\\"x\\\":0.5,\\\"y\\\":1.0}},{\\\"localRotation\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0,\\\"z\\\":0.0,\\\"w\\\":1.0},\\\"localPosition\\\":{\\\"x\\\":0.0,\\\"y\\\":-14.0,\\\"z\\\":0.0},\\\"localScale\\\":{\\\"x\\\":1.0,\\\"y\\\":1.0,\\\"z\\\":1.0},\\\"anchorMin\\\":{\\\"x\\\":0.0,\\\"y\\\":0.5},\\\"anchorMax\\\":{\\\"x\\\":1.0,\\\"y\\\":0.5},\\\"anchoredPosition\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0},\\\"sizeDelta\\\":{\\\"x\\\":0.0,\\\"y\\\":20.0},\\\"pivot\\\":{\\\"x\\\":0.5,\\\"y\\\":0.5}},{\\\"localRotation\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0,\\\"z\\\":0.0,\\\"w\\\":1.0},\\\"localPosition\\\":{\\\"x\\\":5.0,\\\"y\\\":-0.5,\\\"z\\\":0.0},\\\"localScale\\\":{\\\"x\\\":1.0,\\\"y\\\":1.0,\\\"z\\\":1.0},\\\"anchorMin\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0},\\\"anchorMax\\\":{\\\"x\\\":1.0,\\\"y\\\":1.0},\\\"anchoredPosition\\\":{\\\"x\\\":5.0,\\\"y\\\":-0.5},\\\"sizeDelta\\\":{\\\"x\\\":-30.0,\\\"y\\\":-3.0},\\\"pivot\\\":{\\\"x\\\":0.5,\\\"y\\\":0.5}}]},{\\\"CanvasObjectsLayers\\\":[\\\"[Legacy]\\\",\\\"Dropdown (LT) [Legacy]\\\",\\\"Template\\\",\\\"Scrollbar\\\",\\\"Sliding Area\\\",\\\"Handle\\\"],\\\"rectTransforms\\\":[{\\\"localRotation\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0,\\\"z\\\":0.0,\\\"w\\\":1.0},\\\"localPosition\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0,\\\"z\\\":0.0},\\\"localScale\\\":{\\\"x\\\":1.0,\\\"y\\\":1.0,\\\"z\\\":1.0},\\\"anchorMin\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0},\\\"anchorMax\\\":{\\\"x\\\":1.0,\\\"y\\\":1.0},\\\"anchoredPosition\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0},\\\"sizeDelta\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0},\\\"pivot\\\":{\\\"x\\\":0.5,\\\"y\\\":0.5}},{\\\"localRotation\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0,\\\"z\\\":0.0,\\\"w\\\":1.0},\\\"localPosition\\\":{\\\"x\\\":260.0,\\\"y\\\":30.0,\\\"z\\\":0.0},\\\"localScale\\\":{\\\"x\\\":1.0,\\\"y\\\":1.0,\\\"z\\\":1.0},\\\"anchorMin\\\":{\\\"x\\\":0.5,\\\"y\\\":0.5},\\\"anchorMax\\\":{\\\"x\\\":0.5,\\\"y\\\":0.5},\\\"anchoredPosition\\\":{\\\"x\\\":260.0,\\\"y\\\":30.0},\\\"sizeDelta\\\":{\\\"x\\\":160.0,\\\"y\\\":30.0},\\\"pivot\\\":{\\\"x\\\":0.5,\\\"y\\\":0.5}},{\\\"localRotation\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0,\\\"z\\\":0.0,\\\"w\\\":1.0},\\\"localPosition\\\":{\\\"x\\\":0.0,\\\"y\\\":-13.0,\\\"z\\\":0.0},\\\"localScale\\\":{\\\"x\\\":1.0,\\\"y\\\":1.0,\\\"z\\\":1.0},\\\"anchorMin\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0},\\\"anchorMax\\\":{\\\"x\\\":1.0,\\\"y\\\":0.0},\\\"anchoredPosition\\\":{\\\"x\\\":0.0,\\\"y\\\":2.0},\\\"sizeDelta\\\":{\\\"x\\\":0.0,\\\"y\\\":150.0},\\\"pivot\\\":{\\\"x\\\":0.5,\\\"y\\\":1.0}},{\\\"localRotation\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0,\\\"z\\\":0.0,\\\"w\\\":1.0},\\\"localPosition\\\":{\\\"x\\\":80.0,\\\"y\\\":0.0,\\\"z\\\":0.0},\\\"localScale\\\":{\\\"x\\\":1.0,\\\"y\\\":1.0,\\\"z\\\":1.0},\\\"anchorMin\\\":{\\\"x\\\":1.0,\\\"y\\\":0.0},\\\"anchorMax\\\":{\\\"x\\\":1.0,\\\"y\\\":1.0},\\\"anchoredPosition\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0},\\\"sizeDelta\\\":{\\\"x\\\":20.0,\\\"y\\\":0.0},\\\"pivot\\\":{\\\"x\\\":1.0,\\\"y\\\":1.0}},{\\\"localRotation\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0,\\\"z\\\":0.0,\\\"w\\\":1.0},\\\"localPosition\\\":{\\\"x\\\":-10.0,\\\"y\\\":-75.0,\\\"z\\\":0.0},\\\"localScale\\\":{\\\"x\\\":1.0,\\\"y\\\":1.0,\\\"z\\\":1.0},\\\"anchorMin\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0},\\\"anchorMax\\\":{\\\"x\\\":1.0,\\\"y\\\":1.0},\\\"anchoredPosition\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0},\\\"sizeDelta\\\":{\\\"x\\\":-20.0,\\\"y\\\":-20.0},\\\"pivot\\\":{\\\"x\\\":0.5,\\\"y\\\":0.5}},{\\\"localRotation\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0,\\\"z\\\":0.0,\\\"w\\\":1.0},\\\"localPosition\\\":{\\\"x\\\":0.0,\\\"y\\\":-52.0,\\\"z\\\":0.0},\\\"localScale\\\":{\\\"x\\\":1.0,\\\"y\\\":1.0,\\\"z\\\":1.0},\\\"anchorMin\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0},\\\"anchorMax\\\":{\\\"x\\\":1.0,\\\"y\\\":0.20000000298023225},\\\"anchoredPosition\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0},\\\"sizeDelta\\\":{\\\"x\\\":20.0,\\\"y\\\":20.0},\\\"pivot\\\":{\\\"x\\\":0.5,\\\"y\\\":0.5}}]},{\\\"CanvasObjectsLayers\\\":[\\\"[Legacy]\\\",\\\"InputField (LT) [Legacy]\\\",\\\"Placeholder\\\"],\\\"rectTransforms\\\":[{\\\"localRotation\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0,\\\"z\\\":0.0,\\\"w\\\":1.0},\\\"localPosition\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0,\\\"z\\\":0.0},\\\"localScale\\\":{\\\"x\\\":1.0,\\\"y\\\":1.0,\\\"z\\\":1.0},\\\"anchorMin\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0},\\\"anchorMax\\\":{\\\"x\\\":1.0,\\\"y\\\":1.0},\\\"anchoredPosition\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0},\\\"sizeDelta\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0},\\\"pivot\\\":{\\\"x\\\":0.5,\\\"y\\\":0.5}},{\\\"localRotation\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0,\\\"z\\\":0.0,\\\"w\\\":1.0},\\\"localPosition\\\":{\\\"x\\\":260.0,\\\"y\\\":-20.0,\\\"z\\\":0.0},\\\"localScale\\\":{\\\"x\\\":1.0,\\\"y\\\":1.0,\\\"z\\\":1.0},\\\"anchorMin\\\":{\\\"x\\\":0.5,\\\"y\\\":0.5},\\\"anchorMax\\\":{\\\"x\\\":0.5,\\\"y\\\":0.5},\\\"anchoredPosition\\\":{\\\"x\\\":260.0,\\\"y\\\":-20.0},\\\"sizeDelta\\\":{\\\"x\\\":160.0,\\\"y\\\":30.0},\\\"pivot\\\":{\\\"x\\\":0.5,\\\"y\\\":0.5}},{\\\"localRotation\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0,\\\"z\\\":0.0,\\\"w\\\":1.0},\\\"localPosition\\\":{\\\"x\\\":0.0,\\\"y\\\":-0.5,\\\"z\\\":0.0},\\\"localScale\\\":{\\\"x\\\":1.0,\\\"y\\\":1.0,\\\"z\\\":1.0},\\\"anchorMin\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0},\\\"anchorMax\\\":{\\\"x\\\":1.0,\\\"y\\\":1.0},\\\"anchoredPosition\\\":{\\\"x\\\":0.0,\\\"y\\\":-0.5},\\\"sizeDelta\\\":{\\\"x\\\":-20.0,\\\"y\\\":-13.0},\\\"pivot\\\":{\\\"x\\\":0.5,\\\"y\\\":0.5}}]},{\\\"CanvasObjectsLayers\\\":[\\\"[Legacy]\\\",\\\"InputField (LT) [Legacy]\\\",\\\"Text (Legacy)\\\"],\\\"rectTransforms\\\":[{\\\"localRotation\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0,\\\"z\\\":0.0,\\\"w\\\":1.0},\\\"localPosition\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0,\\\"z\\\":0.0},\\\"localScale\\\":{\\\"x\\\":1.0,\\\"y\\\":1.0,\\\"z\\\":1.0},\\\"anchorMin\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0},\\\"anchorMax\\\":{\\\"x\\\":1.0,\\\"y\\\":1.0},\\\"anchoredPosition\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0},\\\"sizeDelta\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0},\\\"pivot\\\":{\\\"x\\\":0.5,\\\"y\\\":0.5}},{\\\"localRotation\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0,\\\"z\\\":0.0,\\\"w\\\":1.0},\\\"localPosition\\\":{\\\"x\\\":260.0,\\\"y\\\":-20.0,\\\"z\\\":0.0},\\\"localScale\\\":{\\\"x\\\":1.0,\\\"y\\\":1.0,\\\"z\\\":1.0},\\\"anchorMin\\\":{\\\"x\\\":0.5,\\\"y\\\":0.5},\\\"anchorMax\\\":{\\\"x\\\":0.5,\\\"y\\\":0.5},\\\"anchoredPosition\\\":{\\\"x\\\":260.0,\\\"y\\\":-20.0},\\\"sizeDelta\\\":{\\\"x\\\":160.0,\\\"y\\\":30.0},\\\"pivot\\\":{\\\"x\\\":0.5,\\\"y\\\":0.5}},{\\\"localRotation\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0,\\\"z\\\":0.0,\\\"w\\\":1.0},\\\"localPosition\\\":{\\\"x\\\":0.0,\\\"y\\\":-0.5,\\\"z\\\":0.0},\\\"localScale\\\":{\\\"x\\\":1.0,\\\"y\\\":1.0,\\\"z\\\":1.0},\\\"anchorMin\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0},\\\"anchorMax\\\":{\\\"x\\\":1.0,\\\"y\\\":1.0},\\\"anchoredPosition\\\":{\\\"x\\\":0.0,\\\"y\\\":-0.5},\\\"sizeDelta\\\":{\\\"x\\\":-20.0,\\\"y\\\":-13.0},\\\"pivot\\\":{\\\"x\\\":0.5,\\\"y\\\":0.5}}]},{\\\"CanvasObjectsLayers\\\":[\\\"[Legacy]\\\",\\\"Text (LT) [Legacy]\\\"],\\\"rectTransforms\\\":[{\\\"localRotation\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0,\\\"z\\\":0.0,\\\"w\\\":1.0},\\\"localPosition\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0,\\\"z\\\":0.0},\\\"localScale\\\":{\\\"x\\\":1.0,\\\"y\\\":1.0,\\\"z\\\":1.0},\\\"anchorMin\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0},\\\"anchorMax\\\":{\\\"x\\\":1.0,\\\"y\\\":1.0},\\\"anchoredPosition\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0},\\\"sizeDelta\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0},\\\"pivot\\\":{\\\"x\\\":0.5,\\\"y\\\":0.5}},{\\\"localRotation\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0,\\\"z\\\":0.0,\\\"w\\\":1.0},\\\"localPosition\\\":{\\\"x\\\":260.0,\\\"y\\\":-70.0,\\\"z\\\":0.0},\\\"localScale\\\":{\\\"x\\\":1.0,\\\"y\\\":1.0,\\\"z\\\":1.0},\\\"anchorMin\\\":{\\\"x\\\":0.5,\\\"y\\\":0.5},\\\"anchorMax\\\":{\\\"x\\\":0.5,\\\"y\\\":0.5},\\\"anchoredPosition\\\":{\\\"x\\\":260.0,\\\"y\\\":-70.0},\\\"sizeDelta\\\":{\\\"x\\\":160.0,\\\"y\\\":30.0},\\\"pivot\\\":{\\\"x\\\":0.5,\\\"y\\\":0.5}}]},{\\\"CanvasObjectsLayers\\\":[\\\"[Legacy]\\\",\\\"Toggle (LT) [Legacy]\\\",\\\"Background\\\",\\\"Checkmark\\\"],\\\"rectTransforms\\\":[{\\\"localRotation\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0,\\\"z\\\":0.0,\\\"w\\\":1.0},\\\"localPosition\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0,\\\"z\\\":0.0},\\\"localScale\\\":{\\\"x\\\":1.0,\\\"y\\\":1.0,\\\"z\\\":1.0},\\\"anchorMin\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0},\\\"anchorMax\\\":{\\\"x\\\":1.0,\\\"y\\\":1.0},\\\"anchoredPosition\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0},\\\"sizeDelta\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0},\\\"pivot\\\":{\\\"x\\\":0.5,\\\"y\\\":0.5}},{\\\"localRotation\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0,\\\"z\\\":0.0,\\\"w\\\":1.0},\\\"localPosition\\\":{\\\"x\\\":260.0,\\\"y\\\":-110.0,\\\"z\\\":0.0},\\\"localScale\\\":{\\\"x\\\":1.0,\\\"y\\\":1.0,\\\"z\\\":1.0},\\\"anchorMin\\\":{\\\"x\\\":0.5,\\\"y\\\":0.5},\\\"anchorMax\\\":{\\\"x\\\":0.5,\\\"y\\\":0.5},\\\"anchoredPosition\\\":{\\\"x\\\":260.0,\\\"y\\\":-110.0},\\\"sizeDelta\\\":{\\\"x\\\":160.0,\\\"y\\\":20.0},\\\"pivot\\\":{\\\"x\\\":0.5,\\\"y\\\":0.5}},{\\\"localRotation\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0,\\\"z\\\":0.0,\\\"w\\\":1.0},\\\"localPosition\\\":{\\\"x\\\":-70.0,\\\"y\\\":0.0,\\\"z\\\":0.0},\\\"localScale\\\":{\\\"x\\\":1.0,\\\"y\\\":1.0,\\\"z\\\":1.0},\\\"anchorMin\\\":{\\\"x\\\":0.0,\\\"y\\\":1.0},\\\"anchorMax\\\":{\\\"x\\\":0.0,\\\"y\\\":1.0},\\\"anchoredPosition\\\":{\\\"x\\\":10.0,\\\"y\\\":-10.0},\\\"sizeDelta\\\":{\\\"x\\\":20.0,\\\"y\\\":20.0},\\\"pivot\\\":{\\\"x\\\":0.5,\\\"y\\\":0.5}},{\\\"localRotation\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0,\\\"z\\\":0.0,\\\"w\\\":1.0},\\\"localPosition\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0,\\\"z\\\":0.0},\\\"localScale\\\":{\\\"x\\\":1.0,\\\"y\\\":1.0,\\\"z\\\":1.0},\\\"anchorMin\\\":{\\\"x\\\":0.5,\\\"y\\\":0.5},\\\"anchorMax\\\":{\\\"x\\\":0.5,\\\"y\\\":0.5},\\\"anchoredPosition\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0},\\\"sizeDelta\\\":{\\\"x\\\":20.0,\\\"y\\\":20.0},\\\"pivot\\\":{\\\"x\\\":0.5,\\\"y\\\":0.5}}]},{\\\"CanvasObjectsLayers\\\":[\\\"[Legacy]\\\",\\\"Toggle (LT) [Legacy]\\\",\\\"Label\\\"],\\\"rectTransforms\\\":[{\\\"localRotation\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0,\\\"z\\\":0.0,\\\"w\\\":1.0},\\\"localPosition\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0,\\\"z\\\":0.0},\\\"localScale\\\":{\\\"x\\\":1.0,\\\"y\\\":1.0,\\\"z\\\":1.0},\\\"anchorMin\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0},\\\"anchorMax\\\":{\\\"x\\\":1.0,\\\"y\\\":1.0},\\\"anchoredPosition\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0},\\\"sizeDelta\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0},\\\"pivot\\\":{\\\"x\\\":0.5,\\\"y\\\":0.5}},{\\\"localRotation\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0,\\\"z\\\":0.0,\\\"w\\\":1.0},\\\"localPosition\\\":{\\\"x\\\":260.0,\\\"y\\\":-110.0,\\\"z\\\":0.0},\\\"localScale\\\":{\\\"x\\\":1.0,\\\"y\\\":1.0,\\\"z\\\":1.0},\\\"anchorMin\\\":{\\\"x\\\":0.5,\\\"y\\\":0.5},\\\"anchorMax\\\":{\\\"x\\\":0.5,\\\"y\\\":0.5},\\\"anchoredPosition\\\":{\\\"x\\\":260.0,\\\"y\\\":-110.0},\\\"sizeDelta\\\":{\\\"x\\\":160.0,\\\"y\\\":20.0},\\\"pivot\\\":{\\\"x\\\":0.5,\\\"y\\\":0.5}},{\\\"localRotation\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0,\\\"z\\\":0.0,\\\"w\\\":1.0},\\\"localPosition\\\":{\\\"x\\\":9.0,\\\"y\\\":-0.5,\\\"z\\\":0.0},\\\"localScale\\\":{\\\"x\\\":1.0,\\\"y\\\":1.0,\\\"z\\\":1.0},\\\"anchorMin\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0},\\\"anchorMax\\\":{\\\"x\\\":1.0,\\\"y\\\":1.0},\\\"anchoredPosition\\\":{\\\"x\\\":9.0,\\\"y\\\":-0.5},\\\"sizeDelta\\\":{\\\"x\\\":-28.0,\\\"y\\\":-3.0},\\\"pivot\\\":{\\\"x\\\":0.5,\\\"y\\\":0.5}}]},{\\\"CanvasObjectsLayers\\\":[\\\"[TMP]\\\",\\\"Language Manager (LT) [TMP]\\\",\\\"Label\\\"],\\\"rectTransforms\\\":[{\\\"localRotation\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0,\\\"z\\\":0.0,\\\"w\\\":1.0},\\\"localPosition\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0,\\\"z\\\":0.0},\\\"localScale\\\":{\\\"x\\\":1.0,\\\"y\\\":1.0,\\\"z\\\":1.0},\\\"anchorMin\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0},\\\"anchorMax\\\":{\\\"x\\\":1.0,\\\"y\\\":1.0},\\\"anchoredPosition\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0},\\\"sizeDelta\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0},\\\"pivot\\\":{\\\"x\\\":0.5,\\\"y\\\":0.5}},{\\\"localRotation\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0,\\\"z\\\":0.0,\\\"w\\\":1.0},\\\"localPosition\\\":{\\\"x\\\":-260.0,\\\"y\\\":135.0,\\\"z\\\":0.0},\\\"localScale\\\":{\\\"x\\\":1.0,\\\"y\\\":1.0,\\\"z\\\":1.0},\\\"anchorMin\\\":{\\\"x\\\":0.5,\\\"y\\\":0.5},\\\"anchorMax\\\":{\\\"x\\\":0.5,\\\"y\\\":0.5},\\\"anchoredPosition\\\":{\\\"x\\\":-260.0,\\\"y\\\":135.0},\\\"sizeDelta\\\":{\\\"x\\\":160.0,\\\"y\\\":30.0},\\\"pivot\\\":{\\\"x\\\":0.5,\\\"y\\\":0.5}},{\\\"localRotation\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0,\\\"z\\\":0.0,\\\"w\\\":1.0},\\\"localPosition\\\":{\\\"x\\\":-7.5,\\\"y\\\":-0.5,\\\"z\\\":0.0},\\\"localScale\\\":{\\\"x\\\":1.0,\\\"y\\\":1.0,\\\"z\\\":1.0},\\\"anchorMin\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0},\\\"anchorMax\\\":{\\\"x\\\":1.0,\\\"y\\\":1.0},\\\"anchoredPosition\\\":{\\\"x\\\":-7.5,\\\"y\\\":-0.5},\\\"sizeDelta\\\":{\\\"x\\\":-35.0,\\\"y\\\":-13.0},\\\"pivot\\\":{\\\"x\\\":0.5,\\\"y\\\":0.5}}]},{\\\"CanvasObjectsLayers\\\":[\\\"[TMP]\\\",\\\"Language Manager (LT) [TMP]\\\",\\\"Arrow\\\"],\\\"rectTransforms\\\":[{\\\"localRotation\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0,\\\"z\\\":0.0,\\\"w\\\":1.0},\\\"localPosition\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0,\\\"z\\\":0.0},\\\"localScale\\\":{\\\"x\\\":1.0,\\\"y\\\":1.0,\\\"z\\\":1.0},\\\"anchorMin\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0},\\\"anchorMax\\\":{\\\"x\\\":1.0,\\\"y\\\":1.0},\\\"anchoredPosition\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0},\\\"sizeDelta\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0},\\\"pivot\\\":{\\\"x\\\":0.5,\\\"y\\\":0.5}},{\\\"localRotation\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0,\\\"z\\\":0.0,\\\"w\\\":1.0},\\\"localPosition\\\":{\\\"x\\\":-260.0,\\\"y\\\":135.0,\\\"z\\\":0.0},\\\"localScale\\\":{\\\"x\\\":1.0,\\\"y\\\":1.0,\\\"z\\\":1.0},\\\"anchorMin\\\":{\\\"x\\\":0.5,\\\"y\\\":0.5},\\\"anchorMax\\\":{\\\"x\\\":0.5,\\\"y\\\":0.5},\\\"anchoredPosition\\\":{\\\"x\\\":-260.0,\\\"y\\\":135.0},\\\"sizeDelta\\\":{\\\"x\\\":160.0,\\\"y\\\":30.0},\\\"pivot\\\":{\\\"x\\\":0.5,\\\"y\\\":0.5}},{\\\"localRotation\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0,\\\"z\\\":0.0,\\\"w\\\":1.0},\\\"localPosition\\\":{\\\"x\\\":65.0,\\\"y\\\":0.0,\\\"z\\\":0.0},\\\"localScale\\\":{\\\"x\\\":1.0,\\\"y\\\":1.0,\\\"z\\\":1.0},\\\"anchorMin\\\":{\\\"x\\\":1.0,\\\"y\\\":0.5},\\\"anchorMax\\\":{\\\"x\\\":1.0,\\\"y\\\":0.5},\\\"anchoredPosition\\\":{\\\"x\\\":-15.0,\\\"y\\\":0.0},\\\"sizeDelta\\\":{\\\"x\\\":20.0,\\\"y\\\":20.0},\\\"pivot\\\":{\\\"x\\\":0.5,\\\"y\\\":0.5}}]},{\\\"CanvasObjectsLayers\\\":[\\\"[TMP]\\\",\\\"Language Manager (LT) [TMP]\\\",\\\"Template\\\",\\\"Viewport\\\",\\\"Content\\\",\\\"Item\\\",\\\"Item Background\\\"],\\\"rectTransforms\\\":[{\\\"localRotation\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0,\\\"z\\\":0.0,\\\"w\\\":1.0},\\\"localPosition\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0,\\\"z\\\":0.0},\\\"localScale\\\":{\\\"x\\\":1.0,\\\"y\\\":1.0,\\\"z\\\":1.0},\\\"anchorMin\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0},\\\"anchorMax\\\":{\\\"x\\\":1.0,\\\"y\\\":1.0},\\\"anchoredPosition\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0},\\\"sizeDelta\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0},\\\"pivot\\\":{\\\"x\\\":0.5,\\\"y\\\":0.5}},{\\\"localRotation\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0,\\\"z\\\":0.0,\\\"w\\\":1.0},\\\"localPosition\\\":{\\\"x\\\":-260.0,\\\"y\\\":135.0,\\\"z\\\":0.0},\\\"localScale\\\":{\\\"x\\\":1.0,\\\"y\\\":1.0,\\\"z\\\":1.0},\\\"anchorMin\\\":{\\\"x\\\":0.5,\\\"y\\\":0.5},\\\"anchorMax\\\":{\\\"x\\\":0.5,\\\"y\\\":0.5},\\\"anchoredPosition\\\":{\\\"x\\\":-260.0,\\\"y\\\":135.0},\\\"sizeDelta\\\":{\\\"x\\\":160.0,\\\"y\\\":30.0},\\\"pivot\\\":{\\\"x\\\":0.5,\\\"y\\\":0.5}},{\\\"localRotation\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0,\\\"z\\\":0.0,\\\"w\\\":1.0},\\\"localPosition\\\":{\\\"x\\\":0.0,\\\"y\\\":-13.0,\\\"z\\\":0.0},\\\"localScale\\\":{\\\"x\\\":1.0,\\\"y\\\":1.0,\\\"z\\\":1.0},\\\"anchorMin\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0},\\\"anchorMax\\\":{\\\"x\\\":1.0,\\\"y\\\":0.0},\\\"anchoredPosition\\\":{\\\"x\\\":0.0,\\\"y\\\":2.0},\\\"sizeDelta\\\":{\\\"x\\\":0.0,\\\"y\\\":150.0},\\\"pivot\\\":{\\\"x\\\":0.5,\\\"y\\\":1.0}},{\\\"localRotation\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0,\\\"z\\\":0.0,\\\"w\\\":1.0},\\\"localPosition\\\":{\\\"x\\\":-80.0,\\\"y\\\":0.0,\\\"z\\\":0.0},\\\"localScale\\\":{\\\"x\\\":1.0,\\\"y\\\":1.0,\\\"z\\\":1.0},\\\"anchorMin\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0},\\\"anchorMax\\\":{\\\"x\\\":1.0,\\\"y\\\":1.0},\\\"anchoredPosition\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0},\\\"sizeDelta\\\":{\\\"x\\\":-18.0,\\\"y\\\":0.0},\\\"pivot\\\":{\\\"x\\\":0.0,\\\"y\\\":1.0}},{\\\"localRotation\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0,\\\"z\\\":0.0,\\\"w\\\":1.0},\\\"localPosition\\\":{\\\"x\\\":71.0,\\\"y\\\":0.0,\\\"z\\\":0.0},\\\"localScale\\\":{\\\"x\\\":1.0,\\\"y\\\":1.0,\\\"z\\\":1.0},\\\"anchorMin\\\":{\\\"x\\\":0.0,\\\"y\\\":1.0},\\\"anchorMax\\\":{\\\"x\\\":1.0,\\\"y\\\":1.0},\\\"anchoredPosition\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0},\\\"sizeDelta\\\":{\\\"x\\\":0.0,\\\"y\\\":28.0},\\\"pivot\\\":{\\\"x\\\":0.5,\\\"y\\\":1.0}},{\\\"localRotation\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0,\\\"z\\\":0.0,\\\"w\\\":1.0},\\\"localPosition\\\":{\\\"x\\\":0.0,\\\"y\\\":-14.0,\\\"z\\\":0.0},\\\"localScale\\\":{\\\"x\\\":1.0,\\\"y\\\":1.0,\\\"z\\\":1.0},\\\"anchorMin\\\":{\\\"x\\\":0.0,\\\"y\\\":0.5},\\\"anchorMax\\\":{\\\"x\\\":1.0,\\\"y\\\":0.5},\\\"anchoredPosition\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0},\\\"sizeDelta\\\":{\\\"x\\\":0.0,\\\"y\\\":20.0},\\\"pivot\\\":{\\\"x\\\":0.5,\\\"y\\\":0.5}},{\\\"localRotation\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0,\\\"z\\\":0.0,\\\"w\\\":1.0},\\\"localPosition\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0,\\\"z\\\":0.0},\\\"localScale\\\":{\\\"x\\\":1.0,\\\"y\\\":1.0,\\\"z\\\":1.0},\\\"anchorMin\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0},\\\"anchorMax\\\":{\\\"x\\\":1.0,\\\"y\\\":1.0},\\\"anchoredPosition\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0},\\\"sizeDelta\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0},\\\"pivot\\\":{\\\"x\\\":0.5,\\\"y\\\":0.5}}]},{\\\"CanvasObjectsLayers\\\":[\\\"[TMP]\\\",\\\"Language Manager (LT) [TMP]\\\",\\\"Template\\\",\\\"Viewport\\\",\\\"Content\\\",\\\"Item\\\",\\\"Item Checkmark\\\"],\\\"rectTransforms\\\":[{\\\"localRotation\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0,\\\"z\\\":0.0,\\\"w\\\":1.0},\\\"localPosition\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0,\\\"z\\\":0.0},\\\"localScale\\\":{\\\"x\\\":1.0,\\\"y\\\":1.0,\\\"z\\\":1.0},\\\"anchorMin\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0},\\\"anchorMax\\\":{\\\"x\\\":1.0,\\\"y\\\":1.0},\\\"anchoredPosition\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0},\\\"sizeDelta\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0},\\\"pivot\\\":{\\\"x\\\":0.5,\\\"y\\\":0.5}},{\\\"localRotation\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0,\\\"z\\\":0.0,\\\"w\\\":1.0},\\\"localPosition\\\":{\\\"x\\\":-260.0,\\\"y\\\":135.0,\\\"z\\\":0.0},\\\"localScale\\\":{\\\"x\\\":1.0,\\\"y\\\":1.0,\\\"z\\\":1.0},\\\"anchorMin\\\":{\\\"x\\\":0.5,\\\"y\\\":0.5},\\\"anchorMax\\\":{\\\"x\\\":0.5,\\\"y\\\":0.5},\\\"anchoredPosition\\\":{\\\"x\\\":-260.0,\\\"y\\\":135.0},\\\"sizeDelta\\\":{\\\"x\\\":160.0,\\\"y\\\":30.0},\\\"pivot\\\":{\\\"x\\\":0.5,\\\"y\\\":0.5}},{\\\"localRotation\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0,\\\"z\\\":0.0,\\\"w\\\":1.0},\\\"localPosition\\\":{\\\"x\\\":0.0,\\\"y\\\":-13.0,\\\"z\\\":0.0},\\\"localScale\\\":{\\\"x\\\":1.0,\\\"y\\\":1.0,\\\"z\\\":1.0},\\\"anchorMin\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0},\\\"anchorMax\\\":{\\\"x\\\":1.0,\\\"y\\\":0.0},\\\"anchoredPosition\\\":{\\\"x\\\":0.0,\\\"y\\\":2.0},\\\"sizeDelta\\\":{\\\"x\\\":0.0,\\\"y\\\":150.0},\\\"pivot\\\":{\\\"x\\\":0.5,\\\"y\\\":1.0}},{\\\"localRotation\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0,\\\"z\\\":0.0,\\\"w\\\":1.0},\\\"localPosition\\\":{\\\"x\\\":-80.0,\\\"y\\\":0.0,\\\"z\\\":0.0},\\\"localScale\\\":{\\\"x\\\":1.0,\\\"y\\\":1.0,\\\"z\\\":1.0},\\\"anchorMin\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0},\\\"anchorMax\\\":{\\\"x\\\":1.0,\\\"y\\\":1.0},\\\"anchoredPosition\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0},\\\"sizeDelta\\\":{\\\"x\\\":-18.0,\\\"y\\\":0.0},\\\"pivot\\\":{\\\"x\\\":0.0,\\\"y\\\":1.0}},{\\\"localRotation\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0,\\\"z\\\":0.0,\\\"w\\\":1.0},\\\"localPosition\\\":{\\\"x\\\":71.0,\\\"y\\\":0.0,\\\"z\\\":0.0},\\\"localScale\\\":{\\\"x\\\":1.0,\\\"y\\\":1.0,\\\"z\\\":1.0},\\\"anchorMin\\\":{\\\"x\\\":0.0,\\\"y\\\":1.0},\\\"anchorMax\\\":{\\\"x\\\":1.0,\\\"y\\\":1.0},\\\"anchoredPosition\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0},\\\"sizeDelta\\\":{\\\"x\\\":0.0,\\\"y\\\":28.0},\\\"pivot\\\":{\\\"x\\\":0.5,\\\"y\\\":1.0}},{\\\"localRotation\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0,\\\"z\\\":0.0,\\\"w\\\":1.0},\\\"localPosition\\\":{\\\"x\\\":0.0,\\\"y\\\":-14.0,\\\"z\\\":0.0},\\\"localScale\\\":{\\\"x\\\":1.0,\\\"y\\\":1.0,\\\"z\\\":1.0},\\\"anchorMin\\\":{\\\"x\\\":0.0,\\\"y\\\":0.5},\\\"anchorMax\\\":{\\\"x\\\":1.0,\\\"y\\\":0.5},\\\"anchoredPosition\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0},\\\"sizeDelta\\\":{\\\"x\\\":0.0,\\\"y\\\":20.0},\\\"pivot\\\":{\\\"x\\\":0.5,\\\"y\\\":0.5}},{\\\"localRotation\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0,\\\"z\\\":0.0,\\\"w\\\":1.0},\\\"localPosition\\\":{\\\"x\\\":-61.0,\\\"y\\\":0.0,\\\"z\\\":0.0},\\\"localScale\\\":{\\\"x\\\":1.0,\\\"y\\\":1.0,\\\"z\\\":1.0},\\\"anchorMin\\\":{\\\"x\\\":0.0,\\\"y\\\":0.5},\\\"anchorMax\\\":{\\\"x\\\":0.0,\\\"y\\\":0.5},\\\"anchoredPosition\\\":{\\\"x\\\":10.0,\\\"y\\\":0.0},\\\"sizeDelta\\\":{\\\"x\\\":20.0,\\\"y\\\":20.0},\\\"pivot\\\":{\\\"x\\\":0.5,\\\"y\\\":0.5}}]},{\\\"CanvasObjectsLayers\\\":[\\\"[TMP]\\\",\\\"Language Manager (LT) [TMP]\\\",\\\"Template\\\",\\\"Viewport\\\",\\\"Content\\\",\\\"Item\\\",\\\"Item Label\\\"],\\\"rectTransforms\\\":[{\\\"localRotation\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0,\\\"z\\\":0.0,\\\"w\\\":1.0},\\\"localPosition\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0,\\\"z\\\":0.0},\\\"localScale\\\":{\\\"x\\\":1.0,\\\"y\\\":1.0,\\\"z\\\":1.0},\\\"anchorMin\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0},\\\"anchorMax\\\":{\\\"x\\\":1.0,\\\"y\\\":1.0},\\\"anchoredPosition\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0},\\\"sizeDelta\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0},\\\"pivot\\\":{\\\"x\\\":0.5,\\\"y\\\":0.5}},{\\\"localRotation\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0,\\\"z\\\":0.0,\\\"w\\\":1.0},\\\"localPosition\\\":{\\\"x\\\":-260.0,\\\"y\\\":135.0,\\\"z\\\":0.0},\\\"localScale\\\":{\\\"x\\\":1.0,\\\"y\\\":1.0,\\\"z\\\":1.0},\\\"anchorMin\\\":{\\\"x\\\":0.5,\\\"y\\\":0.5},\\\"anchorMax\\\":{\\\"x\\\":0.5,\\\"y\\\":0.5},\\\"anchoredPosition\\\":{\\\"x\\\":-260.0,\\\"y\\\":135.0},\\\"sizeDelta\\\":{\\\"x\\\":160.0,\\\"y\\\":30.0},\\\"pivot\\\":{\\\"x\\\":0.5,\\\"y\\\":0.5}},{\\\"localRotation\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0,\\\"z\\\":0.0,\\\"w\\\":1.0},\\\"localPosition\\\":{\\\"x\\\":0.0,\\\"y\\\":-13.0,\\\"z\\\":0.0},\\\"localScale\\\":{\\\"x\\\":1.0,\\\"y\\\":1.0,\\\"z\\\":1.0},\\\"anchorMin\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0},\\\"anchorMax\\\":{\\\"x\\\":1.0,\\\"y\\\":0.0},\\\"anchoredPosition\\\":{\\\"x\\\":0.0,\\\"y\\\":2.0},\\\"sizeDelta\\\":{\\\"x\\\":0.0,\\\"y\\\":150.0},\\\"pivot\\\":{\\\"x\\\":0.5,\\\"y\\\":1.0}},{\\\"localRotation\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0,\\\"z\\\":0.0,\\\"w\\\":1.0},\\\"localPosition\\\":{\\\"x\\\":-80.0,\\\"y\\\":0.0,\\\"z\\\":0.0},\\\"localScale\\\":{\\\"x\\\":1.0,\\\"y\\\":1.0,\\\"z\\\":1.0},\\\"anchorMin\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0},\\\"anchorMax\\\":{\\\"x\\\":1.0,\\\"y\\\":1.0},\\\"anchoredPosition\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0},\\\"sizeDelta\\\":{\\\"x\\\":-18.0,\\\"y\\\":0.0},\\\"pivot\\\":{\\\"x\\\":0.0,\\\"y\\\":1.0}},{\\\"localRotation\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0,\\\"z\\\":0.0,\\\"w\\\":1.0},\\\"localPosition\\\":{\\\"x\\\":71.0,\\\"y\\\":0.0,\\\"z\\\":0.0},\\\"localScale\\\":{\\\"x\\\":1.0,\\\"y\\\":1.0,\\\"z\\\":1.0},\\\"anchorMin\\\":{\\\"x\\\":0.0,\\\"y\\\":1.0},\\\"anchorMax\\\":{\\\"x\\\":1.0,\\\"y\\\":1.0},\\\"anchoredPosition\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0},\\\"sizeDelta\\\":{\\\"x\\\":0.0,\\\"y\\\":28.0},\\\"pivot\\\":{\\\"x\\\":0.5,\\\"y\\\":1.0}},{\\\"localRotation\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0,\\\"z\\\":0.0,\\\"w\\\":1.0},\\\"localPosition\\\":{\\\"x\\\":0.0,\\\"y\\\":-14.0,\\\"z\\\":0.0},\\\"localScale\\\":{\\\"x\\\":1.0,\\\"y\\\":1.0,\\\"z\\\":1.0},\\\"anchorMin\\\":{\\\"x\\\":0.0,\\\"y\\\":0.5},\\\"anchorMax\\\":{\\\"x\\\":1.0,\\\"y\\\":0.5},\\\"anchoredPosition\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0},\\\"sizeDelta\\\":{\\\"x\\\":0.0,\\\"y\\\":20.0},\\\"pivot\\\":{\\\"x\\\":0.5,\\\"y\\\":0.5}},{\\\"localRotation\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0,\\\"z\\\":0.0,\\\"w\\\":1.0},\\\"localPosition\\\":{\\\"x\\\":5.0,\\\"y\\\":-0.5,\\\"z\\\":0.0},\\\"localScale\\\":{\\\"x\\\":1.0,\\\"y\\\":1.0,\\\"z\\\":1.0},\\\"anchorMin\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0},\\\"anchorMax\\\":{\\\"x\\\":1.0,\\\"y\\\":1.0},\\\"anchoredPosition\\\":{\\\"x\\\":5.0,\\\"y\\\":-0.5},\\\"sizeDelta\\\":{\\\"x\\\":-30.0,\\\"y\\\":-3.0},\\\"pivot\\\":{\\\"x\\\":0.5,\\\"y\\\":0.5}}]},{\\\"CanvasObjectsLayers\\\":[\\\"[TMP]\\\",\\\"Language Manager (LT) [TMP]\\\",\\\"Template\\\",\\\"Scrollbar\\\",\\\"Sliding Area\\\",\\\"Handle\\\"],\\\"rectTransforms\\\":[{\\\"localRotation\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0,\\\"z\\\":0.0,\\\"w\\\":1.0},\\\"localPosition\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0,\\\"z\\\":0.0},\\\"localScale\\\":{\\\"x\\\":1.0,\\\"y\\\":1.0,\\\"z\\\":1.0},\\\"anchorMin\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0},\\\"anchorMax\\\":{\\\"x\\\":1.0,\\\"y\\\":1.0},\\\"anchoredPosition\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0},\\\"sizeDelta\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0},\\\"pivot\\\":{\\\"x\\\":0.5,\\\"y\\\":0.5}},{\\\"localRotation\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0,\\\"z\\\":0.0,\\\"w\\\":1.0},\\\"localPosition\\\":{\\\"x\\\":-260.0,\\\"y\\\":135.0,\\\"z\\\":0.0},\\\"localScale\\\":{\\\"x\\\":1.0,\\\"y\\\":1.0,\\\"z\\\":1.0},\\\"anchorMin\\\":{\\\"x\\\":0.5,\\\"y\\\":0.5},\\\"anchorMax\\\":{\\\"x\\\":0.5,\\\"y\\\":0.5},\\\"anchoredPosition\\\":{\\\"x\\\":-260.0,\\\"y\\\":135.0},\\\"sizeDelta\\\":{\\\"x\\\":160.0,\\\"y\\\":30.0},\\\"pivot\\\":{\\\"x\\\":0.5,\\\"y\\\":0.5}},{\\\"localRotation\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0,\\\"z\\\":0.0,\\\"w\\\":1.0},\\\"localPosition\\\":{\\\"x\\\":0.0,\\\"y\\\":-13.0,\\\"z\\\":0.0},\\\"localScale\\\":{\\\"x\\\":1.0,\\\"y\\\":1.0,\\\"z\\\":1.0},\\\"anchorMin\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0},\\\"anchorMax\\\":{\\\"x\\\":1.0,\\\"y\\\":0.0},\\\"anchoredPosition\\\":{\\\"x\\\":0.0,\\\"y\\\":2.0},\\\"sizeDelta\\\":{\\\"x\\\":0.0,\\\"y\\\":150.0},\\\"pivot\\\":{\\\"x\\\":0.5,\\\"y\\\":1.0}},{\\\"localRotation\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0,\\\"z\\\":0.0,\\\"w\\\":1.0},\\\"localPosition\\\":{\\\"x\\\":80.0,\\\"y\\\":0.0,\\\"z\\\":0.0},\\\"localScale\\\":{\\\"x\\\":1.0,\\\"y\\\":1.0,\\\"z\\\":1.0},\\\"anchorMin\\\":{\\\"x\\\":1.0,\\\"y\\\":0.0},\\\"anchorMax\\\":{\\\"x\\\":1.0,\\\"y\\\":1.0},\\\"anchoredPosition\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0},\\\"sizeDelta\\\":{\\\"x\\\":20.0,\\\"y\\\":0.0},\\\"pivot\\\":{\\\"x\\\":1.0,\\\"y\\\":1.0}},{\\\"localRotation\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0,\\\"z\\\":0.0,\\\"w\\\":1.0},\\\"localPosition\\\":{\\\"x\\\":-10.0,\\\"y\\\":-75.0,\\\"z\\\":0.0},\\\"localScale\\\":{\\\"x\\\":1.0,\\\"y\\\":1.0,\\\"z\\\":1.0},\\\"anchorMin\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0},\\\"anchorMax\\\":{\\\"x\\\":1.0,\\\"y\\\":1.0},\\\"anchoredPosition\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0},\\\"sizeDelta\\\":{\\\"x\\\":-20.0,\\\"y\\\":-20.0},\\\"pivot\\\":{\\\"x\\\":0.5,\\\"y\\\":0.5}},{\\\"localRotation\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0,\\\"z\\\":0.0,\\\"w\\\":1.0},\\\"localPosition\\\":{\\\"x\\\":0.0,\\\"y\\\":-52.0,\\\"z\\\":0.0},\\\"localScale\\\":{\\\"x\\\":1.0,\\\"y\\\":1.0,\\\"z\\\":1.0},\\\"anchorMin\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0},\\\"anchorMax\\\":{\\\"x\\\":1.0,\\\"y\\\":0.20000000298023225},\\\"anchoredPosition\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0},\\\"sizeDelta\\\":{\\\"x\\\":20.0,\\\"y\\\":20.0},\\\"pivot\\\":{\\\"x\\\":0.5,\\\"y\\\":0.5}}]},{\\\"CanvasObjectsLayers\\\":[\\\"[TMP]\\\",\\\"Button (LT) [TMP]\\\",\\\"Text (LT) [TMP]\\\"],\\\"rectTransforms\\\":[{\\\"localRotation\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0,\\\"z\\\":0.0,\\\"w\\\":1.0},\\\"localPosition\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0,\\\"z\\\":0.0},\\\"localScale\\\":{\\\"x\\\":1.0,\\\"y\\\":1.0,\\\"z\\\":1.0},\\\"anchorMin\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0},\\\"anchorMax\\\":{\\\"x\\\":1.0,\\\"y\\\":1.0},\\\"anchoredPosition\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0},\\\"sizeDelta\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0},\\\"pivot\\\":{\\\"x\\\":0.5,\\\"y\\\":0.5}},{\\\"localRotation\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0,\\\"z\\\":0.0,\\\"w\\\":1.0},\\\"localPosition\\\":{\\\"x\\\":-260.0,\\\"y\\\":85.0,\\\"z\\\":0.0},\\\"localScale\\\":{\\\"x\\\":1.0,\\\"y\\\":1.0,\\\"z\\\":1.0},\\\"anchorMin\\\":{\\\"x\\\":0.5,\\\"y\\\":0.5},\\\"anchorMax\\\":{\\\"x\\\":0.5,\\\"y\\\":0.5},\\\"anchoredPosition\\\":{\\\"x\\\":-260.0,\\\"y\\\":85.0},\\\"sizeDelta\\\":{\\\"x\\\":160.0,\\\"y\\\":30.0},\\\"pivot\\\":{\\\"x\\\":0.5,\\\"y\\\":0.5}},{\\\"localRotation\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0,\\\"z\\\":0.0,\\\"w\\\":1.0},\\\"localPosition\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0,\\\"z\\\":0.0},\\\"localScale\\\":{\\\"x\\\":1.0,\\\"y\\\":1.0,\\\"z\\\":1.0},\\\"anchorMin\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0},\\\"anchorMax\\\":{\\\"x\\\":1.0,\\\"y\\\":1.0},\\\"anchoredPosition\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0},\\\"sizeDelta\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0},\\\"pivot\\\":{\\\"x\\\":0.5,\\\"y\\\":0.5}}]},{\\\"CanvasObjectsLayers\\\":[\\\"[TMP]\\\",\\\"Dropdown (LT) [TMP]\\\",\\\"Label\\\"],\\\"rectTransforms\\\":[{\\\"localRotation\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0,\\\"z\\\":0.0,\\\"w\\\":1.0},\\\"localPosition\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0,\\\"z\\\":0.0},\\\"localScale\\\":{\\\"x\\\":1.0,\\\"y\\\":1.0,\\\"z\\\":1.0},\\\"anchorMin\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0},\\\"anchorMax\\\":{\\\"x\\\":1.0,\\\"y\\\":1.0},\\\"anchoredPosition\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0},\\\"sizeDelta\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0},\\\"pivot\\\":{\\\"x\\\":0.5,\\\"y\\\":0.5}},{\\\"localRotation\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0,\\\"z\\\":0.0,\\\"w\\\":1.0},\\\"localPosition\\\":{\\\"x\\\":-260.0,\\\"y\\\":30.0,\\\"z\\\":0.0},\\\"localScale\\\":{\\\"x\\\":1.0,\\\"y\\\":1.0,\\\"z\\\":1.0},\\\"anchorMin\\\":{\\\"x\\\":0.5,\\\"y\\\":0.5},\\\"anchorMax\\\":{\\\"x\\\":0.5,\\\"y\\\":0.5},\\\"anchoredPosition\\\":{\\\"x\\\":-260.0,\\\"y\\\":30.0},\\\"sizeDelta\\\":{\\\"x\\\":160.0,\\\"y\\\":30.0},\\\"pivot\\\":{\\\"x\\\":0.5,\\\"y\\\":0.5}},{\\\"localRotation\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0,\\\"z\\\":0.0,\\\"w\\\":1.0},\\\"localPosition\\\":{\\\"x\\\":-7.5,\\\"y\\\":-0.5,\\\"z\\\":0.0},\\\"localScale\\\":{\\\"x\\\":1.0,\\\"y\\\":1.0,\\\"z\\\":1.0},\\\"anchorMin\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0},\\\"anchorMax\\\":{\\\"x\\\":1.0,\\\"y\\\":1.0},\\\"anchoredPosition\\\":{\\\"x\\\":-7.5,\\\"y\\\":-0.5},\\\"sizeDelta\\\":{\\\"x\\\":-35.0,\\\"y\\\":-13.0},\\\"pivot\\\":{\\\"x\\\":0.5,\\\"y\\\":0.5}}]},{\\\"CanvasObjectsLayers\\\":[\\\"[TMP]\\\",\\\"Dropdown (LT) [TMP]\\\",\\\"Arrow\\\"],\\\"rectTransforms\\\":[{\\\"localRotation\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0,\\\"z\\\":0.0,\\\"w\\\":1.0},\\\"localPosition\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0,\\\"z\\\":0.0},\\\"localScale\\\":{\\\"x\\\":1.0,\\\"y\\\":1.0,\\\"z\\\":1.0},\\\"anchorMin\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0},\\\"anchorMax\\\":{\\\"x\\\":1.0,\\\"y\\\":1.0},\\\"anchoredPosition\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0},\\\"sizeDelta\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0},\\\"pivot\\\":{\\\"x\\\":0.5,\\\"y\\\":0.5}},{\\\"localRotation\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0,\\\"z\\\":0.0,\\\"w\\\":1.0},\\\"localPosition\\\":{\\\"x\\\":-260.0,\\\"y\\\":30.0,\\\"z\\\":0.0},\\\"localScale\\\":{\\\"x\\\":1.0,\\\"y\\\":1.0,\\\"z\\\":1.0},\\\"anchorMin\\\":{\\\"x\\\":0.5,\\\"y\\\":0.5},\\\"anchorMax\\\":{\\\"x\\\":0.5,\\\"y\\\":0.5},\\\"anchoredPosition\\\":{\\\"x\\\":-260.0,\\\"y\\\":30.0},\\\"sizeDelta\\\":{\\\"x\\\":160.0,\\\"y\\\":30.0},\\\"pivot\\\":{\\\"x\\\":0.5,\\\"y\\\":0.5}},{\\\"localRotation\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0,\\\"z\\\":0.0,\\\"w\\\":1.0},\\\"localPosition\\\":{\\\"x\\\":65.0,\\\"y\\\":0.0,\\\"z\\\":0.0},\\\"localScale\\\":{\\\"x\\\":1.0,\\\"y\\\":1.0,\\\"z\\\":1.0},\\\"anchorMin\\\":{\\\"x\\\":1.0,\\\"y\\\":0.5},\\\"anchorMax\\\":{\\\"x\\\":1.0,\\\"y\\\":0.5},\\\"anchoredPosition\\\":{\\\"x\\\":-15.0,\\\"y\\\":0.0},\\\"sizeDelta\\\":{\\\"x\\\":20.0,\\\"y\\\":20.0},\\\"pivot\\\":{\\\"x\\\":0.5,\\\"y\\\":0.5}}]},{\\\"CanvasObjectsLayers\\\":[\\\"[TMP]\\\",\\\"Dropdown (LT) [TMP]\\\",\\\"Template\\\",\\\"Viewport\\\",\\\"Content\\\",\\\"Item\\\",\\\"Item Background\\\"],\\\"rectTransforms\\\":[{\\\"localRotation\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0,\\\"z\\\":0.0,\\\"w\\\":1.0},\\\"localPosition\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0,\\\"z\\\":0.0},\\\"localScale\\\":{\\\"x\\\":1.0,\\\"y\\\":1.0,\\\"z\\\":1.0},\\\"anchorMin\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0},\\\"anchorMax\\\":{\\\"x\\\":1.0,\\\"y\\\":1.0},\\\"anchoredPosition\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0},\\\"sizeDelta\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0},\\\"pivot\\\":{\\\"x\\\":0.5,\\\"y\\\":0.5}},{\\\"localRotation\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0,\\\"z\\\":0.0,\\\"w\\\":1.0},\\\"localPosition\\\":{\\\"x\\\":-260.0,\\\"y\\\":30.0,\\\"z\\\":0.0},\\\"localScale\\\":{\\\"x\\\":1.0,\\\"y\\\":1.0,\\\"z\\\":1.0},\\\"anchorMin\\\":{\\\"x\\\":0.5,\\\"y\\\":0.5},\\\"anchorMax\\\":{\\\"x\\\":0.5,\\\"y\\\":0.5},\\\"anchoredPosition\\\":{\\\"x\\\":-260.0,\\\"y\\\":30.0},\\\"sizeDelta\\\":{\\\"x\\\":160.0,\\\"y\\\":30.0},\\\"pivot\\\":{\\\"x\\\":0.5,\\\"y\\\":0.5}},{\\\"localRotation\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0,\\\"z\\\":0.0,\\\"w\\\":1.0},\\\"localPosition\\\":{\\\"x\\\":0.0,\\\"y\\\":-13.0,\\\"z\\\":0.0},\\\"localScale\\\":{\\\"x\\\":1.0,\\\"y\\\":1.0,\\\"z\\\":1.0},\\\"anchorMin\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0},\\\"anchorMax\\\":{\\\"x\\\":1.0,\\\"y\\\":0.0},\\\"anchoredPosition\\\":{\\\"x\\\":0.0,\\\"y\\\":2.0},\\\"sizeDelta\\\":{\\\"x\\\":0.0,\\\"y\\\":150.0},\\\"pivot\\\":{\\\"x\\\":0.5,\\\"y\\\":1.0}},{\\\"localRotation\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0,\\\"z\\\":0.0,\\\"w\\\":1.0},\\\"localPosition\\\":{\\\"x\\\":-80.0,\\\"y\\\":0.0,\\\"z\\\":0.0},\\\"localScale\\\":{\\\"x\\\":1.0,\\\"y\\\":1.0,\\\"z\\\":1.0},\\\"anchorMin\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0},\\\"anchorMax\\\":{\\\"x\\\":1.0,\\\"y\\\":1.0},\\\"anchoredPosition\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0},\\\"sizeDelta\\\":{\\\"x\\\":-18.0,\\\"y\\\":0.0},\\\"pivot\\\":{\\\"x\\\":0.0,\\\"y\\\":1.0}},{\\\"localRotation\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0,\\\"z\\\":0.0,\\\"w\\\":1.0},\\\"localPosition\\\":{\\\"x\\\":71.0,\\\"y\\\":0.0,\\\"z\\\":0.0},\\\"localScale\\\":{\\\"x\\\":1.0,\\\"y\\\":1.0,\\\"z\\\":1.0},\\\"anchorMin\\\":{\\\"x\\\":0.0,\\\"y\\\":1.0},\\\"anchorMax\\\":{\\\"x\\\":1.0,\\\"y\\\":1.0},\\\"anchoredPosition\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0},\\\"sizeDelta\\\":{\\\"x\\\":0.0,\\\"y\\\":28.0},\\\"pivot\\\":{\\\"x\\\":0.5,\\\"y\\\":1.0}},{\\\"localRotation\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0,\\\"z\\\":0.0,\\\"w\\\":1.0},\\\"localPosition\\\":{\\\"x\\\":0.0,\\\"y\\\":-14.0,\\\"z\\\":0.0},\\\"localScale\\\":{\\\"x\\\":1.0,\\\"y\\\":1.0,\\\"z\\\":1.0},\\\"anchorMin\\\":{\\\"x\\\":0.0,\\\"y\\\":0.5},\\\"anchorMax\\\":{\\\"x\\\":1.0,\\\"y\\\":0.5},\\\"anchoredPosition\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0},\\\"sizeDelta\\\":{\\\"x\\\":0.0,\\\"y\\\":20.0},\\\"pivot\\\":{\\\"x\\\":0.5,\\\"y\\\":0.5}},{\\\"localRotation\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0,\\\"z\\\":0.0,\\\"w\\\":1.0},\\\"localPosition\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0,\\\"z\\\":0.0},\\\"localScale\\\":{\\\"x\\\":1.0,\\\"y\\\":1.0,\\\"z\\\":1.0},\\\"anchorMin\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0},\\\"anchorMax\\\":{\\\"x\\\":1.0,\\\"y\\\":1.0},\\\"anchoredPosition\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0},\\\"sizeDelta\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0},\\\"pivot\\\":{\\\"x\\\":0.5,\\\"y\\\":0.5}}]},{\\\"CanvasObjectsLayers\\\":[\\\"[TMP]\\\",\\\"Dropdown (LT) [TMP]\\\",\\\"Template\\\",\\\"Viewport\\\",\\\"Content\\\",\\\"Item\\\",\\\"Item Checkmark\\\"],\\\"rectTransforms\\\":[{\\\"localRotation\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0,\\\"z\\\":0.0,\\\"w\\\":1.0},\\\"localPosition\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0,\\\"z\\\":0.0},\\\"localScale\\\":{\\\"x\\\":1.0,\\\"y\\\":1.0,\\\"z\\\":1.0},\\\"anchorMin\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0},\\\"anchorMax\\\":{\\\"x\\\":1.0,\\\"y\\\":1.0},\\\"anchoredPosition\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0},\\\"sizeDelta\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0},\\\"pivot\\\":{\\\"x\\\":0.5,\\\"y\\\":0.5}},{\\\"localRotation\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0,\\\"z\\\":0.0,\\\"w\\\":1.0},\\\"localPosition\\\":{\\\"x\\\":-260.0,\\\"y\\\":30.0,\\\"z\\\":0.0},\\\"localScale\\\":{\\\"x\\\":1.0,\\\"y\\\":1.0,\\\"z\\\":1.0},\\\"anchorMin\\\":{\\\"x\\\":0.5,\\\"y\\\":0.5},\\\"anchorMax\\\":{\\\"x\\\":0.5,\\\"y\\\":0.5},\\\"anchoredPosition\\\":{\\\"x\\\":-260.0,\\\"y\\\":30.0},\\\"sizeDelta\\\":{\\\"x\\\":160.0,\\\"y\\\":30.0},\\\"pivot\\\":{\\\"x\\\":0.5,\\\"y\\\":0.5}},{\\\"localRotation\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0,\\\"z\\\":0.0,\\\"w\\\":1.0},\\\"localPosition\\\":{\\\"x\\\":0.0,\\\"y\\\":-13.0,\\\"z\\\":0.0},\\\"localScale\\\":{\\\"x\\\":1.0,\\\"y\\\":1.0,\\\"z\\\":1.0},\\\"anchorMin\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0},\\\"anchorMax\\\":{\\\"x\\\":1.0,\\\"y\\\":0.0},\\\"anchoredPosition\\\":{\\\"x\\\":0.0,\\\"y\\\":2.0},\\\"sizeDelta\\\":{\\\"x\\\":0.0,\\\"y\\\":150.0},\\\"pivot\\\":{\\\"x\\\":0.5,\\\"y\\\":1.0}},{\\\"localRotation\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0,\\\"z\\\":0.0,\\\"w\\\":1.0},\\\"localPosition\\\":{\\\"x\\\":-80.0,\\\"y\\\":0.0,\\\"z\\\":0.0},\\\"localScale\\\":{\\\"x\\\":1.0,\\\"y\\\":1.0,\\\"z\\\":1.0},\\\"anchorMin\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0},\\\"anchorMax\\\":{\\\"x\\\":1.0,\\\"y\\\":1.0},\\\"anchoredPosition\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0},\\\"sizeDelta\\\":{\\\"x\\\":-18.0,\\\"y\\\":0.0},\\\"pivot\\\":{\\\"x\\\":0.0,\\\"y\\\":1.0}},{\\\"localRotation\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0,\\\"z\\\":0.0,\\\"w\\\":1.0},\\\"localPosition\\\":{\\\"x\\\":71.0,\\\"y\\\":0.0,\\\"z\\\":0.0},\\\"localScale\\\":{\\\"x\\\":1.0,\\\"y\\\":1.0,\\\"z\\\":1.0},\\\"anchorMin\\\":{\\\"x\\\":0.0,\\\"y\\\":1.0},\\\"anchorMax\\\":{\\\"x\\\":1.0,\\\"y\\\":1.0},\\\"anchoredPosition\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0},\\\"sizeDelta\\\":{\\\"x\\\":0.0,\\\"y\\\":28.0},\\\"pivot\\\":{\\\"x\\\":0.5,\\\"y\\\":1.0}},{\\\"localRotation\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0,\\\"z\\\":0.0,\\\"w\\\":1.0},\\\"localPosition\\\":{\\\"x\\\":0.0,\\\"y\\\":-14.0,\\\"z\\\":0.0},\\\"localScale\\\":{\\\"x\\\":1.0,\\\"y\\\":1.0,\\\"z\\\":1.0},\\\"anchorMin\\\":{\\\"x\\\":0.0,\\\"y\\\":0.5},\\\"anchorMax\\\":{\\\"x\\\":1.0,\\\"y\\\":0.5},\\\"anchoredPosition\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0},\\\"sizeDelta\\\":{\\\"x\\\":0.0,\\\"y\\\":20.0},\\\"pivot\\\":{\\\"x\\\":0.5,\\\"y\\\":0.5}},{\\\"localRotation\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0,\\\"z\\\":0.0,\\\"w\\\":1.0},\\\"localPosition\\\":{\\\"x\\\":-61.0,\\\"y\\\":0.0,\\\"z\\\":0.0},\\\"localScale\\\":{\\\"x\\\":1.0,\\\"y\\\":1.0,\\\"z\\\":1.0},\\\"anchorMin\\\":{\\\"x\\\":0.0,\\\"y\\\":0.5},\\\"anchorMax\\\":{\\\"x\\\":0.0,\\\"y\\\":0.5},\\\"anchoredPosition\\\":{\\\"x\\\":10.0,\\\"y\\\":0.0},\\\"sizeDelta\\\":{\\\"x\\\":20.0,\\\"y\\\":20.0},\\\"pivot\\\":{\\\"x\\\":0.5,\\\"y\\\":0.5}}]},{\\\"CanvasObjectsLayers\\\":[\\\"[TMP]\\\",\\\"Dropdown (LT) [TMP]\\\",\\\"Template\\\",\\\"Viewport\\\",\\\"Content\\\",\\\"Item\\\",\\\"Item Label\\\"],\\\"rectTransforms\\\":[{\\\"localRotation\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0,\\\"z\\\":0.0,\\\"w\\\":1.0},\\\"localPosition\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0,\\\"z\\\":0.0},\\\"localScale\\\":{\\\"x\\\":1.0,\\\"y\\\":1.0,\\\"z\\\":1.0},\\\"anchorMin\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0},\\\"anchorMax\\\":{\\\"x\\\":1.0,\\\"y\\\":1.0},\\\"anchoredPosition\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0},\\\"sizeDelta\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0},\\\"pivot\\\":{\\\"x\\\":0.5,\\\"y\\\":0.5}},{\\\"localRotation\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0,\\\"z\\\":0.0,\\\"w\\\":1.0},\\\"localPosition\\\":{\\\"x\\\":-260.0,\\\"y\\\":30.0,\\\"z\\\":0.0},\\\"localScale\\\":{\\\"x\\\":1.0,\\\"y\\\":1.0,\\\"z\\\":1.0},\\\"anchorMin\\\":{\\\"x\\\":0.5,\\\"y\\\":0.5},\\\"anchorMax\\\":{\\\"x\\\":0.5,\\\"y\\\":0.5},\\\"anchoredPosition\\\":{\\\"x\\\":-260.0,\\\"y\\\":30.0},\\\"sizeDelta\\\":{\\\"x\\\":160.0,\\\"y\\\":30.0},\\\"pivot\\\":{\\\"x\\\":0.5,\\\"y\\\":0.5}},{\\\"localRotation\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0,\\\"z\\\":0.0,\\\"w\\\":1.0},\\\"localPosition\\\":{\\\"x\\\":0.0,\\\"y\\\":-13.0,\\\"z\\\":0.0},\\\"localScale\\\":{\\\"x\\\":1.0,\\\"y\\\":1.0,\\\"z\\\":1.0},\\\"anchorMin\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0},\\\"anchorMax\\\":{\\\"x\\\":1.0,\\\"y\\\":0.0},\\\"anchoredPosition\\\":{\\\"x\\\":0.0,\\\"y\\\":2.0},\\\"sizeDelta\\\":{\\\"x\\\":0.0,\\\"y\\\":150.0},\\\"pivot\\\":{\\\"x\\\":0.5,\\\"y\\\":1.0}},{\\\"localRotation\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0,\\\"z\\\":0.0,\\\"w\\\":1.0},\\\"localPosition\\\":{\\\"x\\\":-80.0,\\\"y\\\":0.0,\\\"z\\\":0.0},\\\"localScale\\\":{\\\"x\\\":1.0,\\\"y\\\":1.0,\\\"z\\\":1.0},\\\"anchorMin\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0},\\\"anchorMax\\\":{\\\"x\\\":1.0,\\\"y\\\":1.0},\\\"anchoredPosition\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0},\\\"sizeDelta\\\":{\\\"x\\\":-18.0,\\\"y\\\":0.0},\\\"pivot\\\":{\\\"x\\\":0.0,\\\"y\\\":1.0}},{\\\"localRotation\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0,\\\"z\\\":0.0,\\\"w\\\":1.0},\\\"localPosition\\\":{\\\"x\\\":71.0,\\\"y\\\":0.0,\\\"z\\\":0.0},\\\"localScale\\\":{\\\"x\\\":1.0,\\\"y\\\":1.0,\\\"z\\\":1.0},\\\"anchorMin\\\":{\\\"x\\\":0.0,\\\"y\\\":1.0},\\\"anchorMax\\\":{\\\"x\\\":1.0,\\\"y\\\":1.0},\\\"anchoredPosition\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0},\\\"sizeDelta\\\":{\\\"x\\\":0.0,\\\"y\\\":28.0},\\\"pivot\\\":{\\\"x\\\":0.5,\\\"y\\\":1.0}},{\\\"localRotation\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0,\\\"z\\\":0.0,\\\"w\\\":1.0},\\\"localPosition\\\":{\\\"x\\\":0.0,\\\"y\\\":-14.0,\\\"z\\\":0.0},\\\"localScale\\\":{\\\"x\\\":1.0,\\\"y\\\":1.0,\\\"z\\\":1.0},\\\"anchorMin\\\":{\\\"x\\\":0.0,\\\"y\\\":0.5},\\\"anchorMax\\\":{\\\"x\\\":1.0,\\\"y\\\":0.5},\\\"anchoredPosition\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0},\\\"sizeDelta\\\":{\\\"x\\\":0.0,\\\"y\\\":20.0},\\\"pivot\\\":{\\\"x\\\":0.5,\\\"y\\\":0.5}},{\\\"localRotation\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0,\\\"z\\\":0.0,\\\"w\\\":1.0},\\\"localPosition\\\":{\\\"x\\\":5.0,\\\"y\\\":-0.5,\\\"z\\\":0.0},\\\"localScale\\\":{\\\"x\\\":1.0,\\\"y\\\":1.0,\\\"z\\\":1.0},\\\"anchorMin\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0},\\\"anchorMax\\\":{\\\"x\\\":1.0,\\\"y\\\":1.0},\\\"anchoredPosition\\\":{\\\"x\\\":5.0,\\\"y\\\":-0.5},\\\"sizeDelta\\\":{\\\"x\\\":-30.0,\\\"y\\\":-3.0},\\\"pivot\\\":{\\\"x\\\":0.5,\\\"y\\\":0.5}}]},{\\\"CanvasObjectsLayers\\\":[\\\"[TMP]\\\",\\\"Dropdown (LT) [TMP]\\\",\\\"Template\\\",\\\"Scrollbar\\\",\\\"Sliding Area\\\",\\\"Handle\\\"],\\\"rectTransforms\\\":[{\\\"localRotation\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0,\\\"z\\\":0.0,\\\"w\\\":1.0},\\\"localPosition\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0,\\\"z\\\":0.0},\\\"localScale\\\":{\\\"x\\\":1.0,\\\"y\\\":1.0,\\\"z\\\":1.0},\\\"anchorMin\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0},\\\"anchorMax\\\":{\\\"x\\\":1.0,\\\"y\\\":1.0},\\\"anchoredPosition\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0},\\\"sizeDelta\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0},\\\"pivot\\\":{\\\"x\\\":0.5,\\\"y\\\":0.5}},{\\\"localRotation\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0,\\\"z\\\":0.0,\\\"w\\\":1.0},\\\"localPosition\\\":{\\\"x\\\":-260.0,\\\"y\\\":30.0,\\\"z\\\":0.0},\\\"localScale\\\":{\\\"x\\\":1.0,\\\"y\\\":1.0,\\\"z\\\":1.0},\\\"anchorMin\\\":{\\\"x\\\":0.5,\\\"y\\\":0.5},\\\"anchorMax\\\":{\\\"x\\\":0.5,\\\"y\\\":0.5},\\\"anchoredPosition\\\":{\\\"x\\\":-260.0,\\\"y\\\":30.0},\\\"sizeDelta\\\":{\\\"x\\\":160.0,\\\"y\\\":30.0},\\\"pivot\\\":{\\\"x\\\":0.5,\\\"y\\\":0.5}},{\\\"localRotation\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0,\\\"z\\\":0.0,\\\"w\\\":1.0},\\\"localPosition\\\":{\\\"x\\\":0.0,\\\"y\\\":-13.0,\\\"z\\\":0.0},\\\"localScale\\\":{\\\"x\\\":1.0,\\\"y\\\":1.0,\\\"z\\\":1.0},\\\"anchorMin\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0},\\\"anchorMax\\\":{\\\"x\\\":1.0,\\\"y\\\":0.0},\\\"anchoredPosition\\\":{\\\"x\\\":0.0,\\\"y\\\":2.0},\\\"sizeDelta\\\":{\\\"x\\\":0.0,\\\"y\\\":150.0},\\\"pivot\\\":{\\\"x\\\":0.5,\\\"y\\\":1.0}},{\\\"localRotation\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0,\\\"z\\\":0.0,\\\"w\\\":1.0},\\\"localPosition\\\":{\\\"x\\\":80.0,\\\"y\\\":0.0,\\\"z\\\":0.0},\\\"localScale\\\":{\\\"x\\\":1.0,\\\"y\\\":1.0,\\\"z\\\":1.0},\\\"anchorMin\\\":{\\\"x\\\":1.0,\\\"y\\\":0.0},\\\"anchorMax\\\":{\\\"x\\\":1.0,\\\"y\\\":1.0},\\\"anchoredPosition\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0},\\\"sizeDelta\\\":{\\\"x\\\":20.0,\\\"y\\\":0.0},\\\"pivot\\\":{\\\"x\\\":1.0,\\\"y\\\":1.0}},{\\\"localRotation\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0,\\\"z\\\":0.0,\\\"w\\\":1.0},\\\"localPosition\\\":{\\\"x\\\":-10.0,\\\"y\\\":-75.0,\\\"z\\\":0.0},\\\"localScale\\\":{\\\"x\\\":1.0,\\\"y\\\":1.0,\\\"z\\\":1.0},\\\"anchorMin\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0},\\\"anchorMax\\\":{\\\"x\\\":1.0,\\\"y\\\":1.0},\\\"anchoredPosition\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0},\\\"sizeDelta\\\":{\\\"x\\\":-20.0,\\\"y\\\":-20.0},\\\"pivot\\\":{\\\"x\\\":0.5,\\\"y\\\":0.5}},{\\\"localRotation\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0,\\\"z\\\":0.0,\\\"w\\\":1.0},\\\"localPosition\\\":{\\\"x\\\":0.0,\\\"y\\\":-52.0,\\\"z\\\":0.0},\\\"localScale\\\":{\\\"x\\\":1.0,\\\"y\\\":1.0,\\\"z\\\":1.0},\\\"anchorMin\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0},\\\"anchorMax\\\":{\\\"x\\\":1.0,\\\"y\\\":0.20000000298023225},\\\"anchoredPosition\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0},\\\"sizeDelta\\\":{\\\"x\\\":20.0,\\\"y\\\":20.0},\\\"pivot\\\":{\\\"x\\\":0.5,\\\"y\\\":0.5}}]},{\\\"CanvasObjectsLayers\\\":[\\\"[TMP]\\\",\\\"InputField (LT) [TMP]\\\",\\\"Text Area\\\",\\\"Placeholder\\\"],\\\"rectTransforms\\\":[{\\\"localRotation\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0,\\\"z\\\":0.0,\\\"w\\\":1.0},\\\"localPosition\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0,\\\"z\\\":0.0},\\\"localScale\\\":{\\\"x\\\":1.0,\\\"y\\\":1.0,\\\"z\\\":1.0},\\\"anchorMin\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0},\\\"anchorMax\\\":{\\\"x\\\":1.0,\\\"y\\\":1.0},\\\"anchoredPosition\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0},\\\"sizeDelta\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0},\\\"pivot\\\":{\\\"x\\\":0.5,\\\"y\\\":0.5}},{\\\"localRotation\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0,\\\"z\\\":0.0,\\\"w\\\":1.0},\\\"localPosition\\\":{\\\"x\\\":-260.0,\\\"y\\\":-20.0,\\\"z\\\":0.0},\\\"localScale\\\":{\\\"x\\\":1.0,\\\"y\\\":1.0,\\\"z\\\":1.0},\\\"anchorMin\\\":{\\\"x\\\":0.5,\\\"y\\\":0.5},\\\"anchorMax\\\":{\\\"x\\\":0.5,\\\"y\\\":0.5},\\\"anchoredPosition\\\":{\\\"x\\\":-260.0,\\\"y\\\":-20.0},\\\"sizeDelta\\\":{\\\"x\\\":160.0,\\\"y\\\":30.0},\\\"pivot\\\":{\\\"x\\\":0.5,\\\"y\\\":0.5}},{\\\"localRotation\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0,\\\"z\\\":0.0,\\\"w\\\":1.0},\\\"localPosition\\\":{\\\"x\\\":0.0,\\\"y\\\":-0.5,\\\"z\\\":0.0},\\\"localScale\\\":{\\\"x\\\":1.0,\\\"y\\\":1.0,\\\"z\\\":1.0},\\\"anchorMin\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0},\\\"anchorMax\\\":{\\\"x\\\":1.0,\\\"y\\\":1.0},\\\"anchoredPosition\\\":{\\\"x\\\":0.0,\\\"y\\\":-0.5},\\\"sizeDelta\\\":{\\\"x\\\":-20.0,\\\"y\\\":-13.0},\\\"pivot\\\":{\\\"x\\\":0.5,\\\"y\\\":0.5}},{\\\"localRotation\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0,\\\"z\\\":0.0,\\\"w\\\":1.0},\\\"localPosition\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0,\\\"z\\\":0.0},\\\"localScale\\\":{\\\"x\\\":1.0,\\\"y\\\":1.0,\\\"z\\\":1.0},\\\"anchorMin\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0},\\\"anchorMax\\\":{\\\"x\\\":1.0,\\\"y\\\":1.0},\\\"anchoredPosition\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0},\\\"sizeDelta\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0},\\\"pivot\\\":{\\\"x\\\":0.5,\\\"y\\\":0.5}}]},{\\\"CanvasObjectsLayers\\\":[\\\"[TMP]\\\",\\\"InputField (LT) [TMP]\\\",\\\"Text Area\\\",\\\"Text\\\"],\\\"rectTransforms\\\":[{\\\"localRotation\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0,\\\"z\\\":0.0,\\\"w\\\":1.0},\\\"localPosition\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0,\\\"z\\\":0.0},\\\"localScale\\\":{\\\"x\\\":1.0,\\\"y\\\":1.0,\\\"z\\\":1.0},\\\"anchorMin\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0},\\\"anchorMax\\\":{\\\"x\\\":1.0,\\\"y\\\":1.0},\\\"anchoredPosition\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0},\\\"sizeDelta\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0},\\\"pivot\\\":{\\\"x\\\":0.5,\\\"y\\\":0.5}},{\\\"localRotation\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0,\\\"z\\\":0.0,\\\"w\\\":1.0},\\\"localPosition\\\":{\\\"x\\\":-260.0,\\\"y\\\":-20.0,\\\"z\\\":0.0},\\\"localScale\\\":{\\\"x\\\":1.0,\\\"y\\\":1.0,\\\"z\\\":1.0},\\\"anchorMin\\\":{\\\"x\\\":0.5,\\\"y\\\":0.5},\\\"anchorMax\\\":{\\\"x\\\":0.5,\\\"y\\\":0.5},\\\"anchoredPosition\\\":{\\\"x\\\":-260.0,\\\"y\\\":-20.0},\\\"sizeDelta\\\":{\\\"x\\\":160.0,\\\"y\\\":30.0},\\\"pivot\\\":{\\\"x\\\":0.5,\\\"y\\\":0.5}},{\\\"localRotation\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0,\\\"z\\\":0.0,\\\"w\\\":1.0},\\\"localPosition\\\":{\\\"x\\\":0.0,\\\"y\\\":-0.5,\\\"z\\\":0.0},\\\"localScale\\\":{\\\"x\\\":1.0,\\\"y\\\":1.0,\\\"z\\\":1.0},\\\"anchorMin\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0},\\\"anchorMax\\\":{\\\"x\\\":1.0,\\\"y\\\":1.0},\\\"anchoredPosition\\\":{\\\"x\\\":0.0,\\\"y\\\":-0.5},\\\"sizeDelta\\\":{\\\"x\\\":-20.0,\\\"y\\\":-13.0},\\\"pivot\\\":{\\\"x\\\":0.5,\\\"y\\\":0.5}},{\\\"localRotation\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0,\\\"z\\\":0.0,\\\"w\\\":1.0},\\\"localPosition\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0,\\\"z\\\":0.0},\\\"localScale\\\":{\\\"x\\\":1.0,\\\"y\\\":1.0,\\\"z\\\":1.0},\\\"anchorMin\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0},\\\"anchorMax\\\":{\\\"x\\\":1.0,\\\"y\\\":1.0},\\\"anchoredPosition\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0},\\\"sizeDelta\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0},\\\"pivot\\\":{\\\"x\\\":0.5,\\\"y\\\":0.5}}]},{\\\"CanvasObjectsLayers\\\":[\\\"[TMP]\\\",\\\"Text (LT) [TMP]\\\"],\\\"rectTransforms\\\":[{\\\"localRotation\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0,\\\"z\\\":0.0,\\\"w\\\":1.0},\\\"localPosition\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0,\\\"z\\\":0.0},\\\"localScale\\\":{\\\"x\\\":1.0,\\\"y\\\":1.0,\\\"z\\\":1.0},\\\"anchorMin\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0},\\\"anchorMax\\\":{\\\"x\\\":1.0,\\\"y\\\":1.0},\\\"anchoredPosition\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0},\\\"sizeDelta\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0},\\\"pivot\\\":{\\\"x\\\":0.5,\\\"y\\\":0.5}},{\\\"localRotation\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0,\\\"z\\\":0.0,\\\"w\\\":1.0},\\\"localPosition\\\":{\\\"x\\\":-260.0,\\\"y\\\":-70.0,\\\"z\\\":0.0},\\\"localScale\\\":{\\\"x\\\":1.0,\\\"y\\\":1.0,\\\"z\\\":1.0},\\\"anchorMin\\\":{\\\"x\\\":0.5,\\\"y\\\":0.5},\\\"anchorMax\\\":{\\\"x\\\":0.5,\\\"y\\\":0.5},\\\"anchoredPosition\\\":{\\\"x\\\":-260.0,\\\"y\\\":-70.0},\\\"sizeDelta\\\":{\\\"x\\\":160.0,\\\"y\\\":30.0},\\\"pivot\\\":{\\\"x\\\":0.5,\\\"y\\\":0.5}}]},{\\\"CanvasObjectsLayers\\\":[\\\"[TMP]\\\",\\\"Toggle (LT) [TMP]\\\",\\\"Background\\\",\\\"Checkmark\\\"],\\\"rectTransforms\\\":[{\\\"localRotation\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0,\\\"z\\\":0.0,\\\"w\\\":1.0},\\\"localPosition\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0,\\\"z\\\":0.0},\\\"localScale\\\":{\\\"x\\\":1.0,\\\"y\\\":1.0,\\\"z\\\":1.0},\\\"anchorMin\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0},\\\"anchorMax\\\":{\\\"x\\\":1.0,\\\"y\\\":1.0},\\\"anchoredPosition\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0},\\\"sizeDelta\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0},\\\"pivot\\\":{\\\"x\\\":0.5,\\\"y\\\":0.5}},{\\\"localRotation\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0,\\\"z\\\":0.0,\\\"w\\\":1.0},\\\"localPosition\\\":{\\\"x\\\":-260.0,\\\"y\\\":-110.0,\\\"z\\\":0.0},\\\"localScale\\\":{\\\"x\\\":1.0,\\\"y\\\":1.0,\\\"z\\\":1.0},\\\"anchorMin\\\":{\\\"x\\\":0.5,\\\"y\\\":0.5},\\\"anchorMax\\\":{\\\"x\\\":0.5,\\\"y\\\":0.5},\\\"anchoredPosition\\\":{\\\"x\\\":-260.0,\\\"y\\\":-110.0},\\\"sizeDelta\\\":{\\\"x\\\":160.0,\\\"y\\\":20.0},\\\"pivot\\\":{\\\"x\\\":0.5,\\\"y\\\":0.5}},{\\\"localRotation\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0,\\\"z\\\":0.0,\\\"w\\\":1.0},\\\"localPosition\\\":{\\\"x\\\":-70.0,\\\"y\\\":0.0,\\\"z\\\":0.0},\\\"localScale\\\":{\\\"x\\\":1.0,\\\"y\\\":1.0,\\\"z\\\":1.0},\\\"anchorMin\\\":{\\\"x\\\":0.0,\\\"y\\\":1.0},\\\"anchorMax\\\":{\\\"x\\\":0.0,\\\"y\\\":1.0},\\\"anchoredPosition\\\":{\\\"x\\\":10.0,\\\"y\\\":-10.0},\\\"sizeDelta\\\":{\\\"x\\\":20.0,\\\"y\\\":20.0},\\\"pivot\\\":{\\\"x\\\":0.5,\\\"y\\\":0.5}},{\\\"localRotation\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0,\\\"z\\\":0.0,\\\"w\\\":1.0},\\\"localPosition\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0,\\\"z\\\":0.0},\\\"localScale\\\":{\\\"x\\\":1.0,\\\"y\\\":1.0,\\\"z\\\":1.0},\\\"anchorMin\\\":{\\\"x\\\":0.5,\\\"y\\\":0.5},\\\"anchorMax\\\":{\\\"x\\\":0.5,\\\"y\\\":0.5},\\\"anchoredPosition\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0},\\\"sizeDelta\\\":{\\\"x\\\":20.0,\\\"y\\\":20.0},\\\"pivot\\\":{\\\"x\\\":0.5,\\\"y\\\":0.5}}]},{\\\"CanvasObjectsLayers\\\":[\\\"[TMP]\\\",\\\"Toggle (LT) [TMP]\\\",\\\"Label\\\"],\\\"rectTransforms\\\":[{\\\"localRotation\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0,\\\"z\\\":0.0,\\\"w\\\":1.0},\\\"localPosition\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0,\\\"z\\\":0.0},\\\"localScale\\\":{\\\"x\\\":1.0,\\\"y\\\":1.0,\\\"z\\\":1.0},\\\"anchorMin\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0},\\\"anchorMax\\\":{\\\"x\\\":1.0,\\\"y\\\":1.0},\\\"anchoredPosition\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0},\\\"sizeDelta\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0},\\\"pivot\\\":{\\\"x\\\":0.5,\\\"y\\\":0.5}},{\\\"localRotation\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0,\\\"z\\\":0.0,\\\"w\\\":1.0},\\\"localPosition\\\":{\\\"x\\\":-260.0,\\\"y\\\":-110.0,\\\"z\\\":0.0},\\\"localScale\\\":{\\\"x\\\":1.0,\\\"y\\\":1.0,\\\"z\\\":1.0},\\\"anchorMin\\\":{\\\"x\\\":0.5,\\\"y\\\":0.5},\\\"anchorMax\\\":{\\\"x\\\":0.5,\\\"y\\\":0.5},\\\"anchoredPosition\\\":{\\\"x\\\":-260.0,\\\"y\\\":-110.0},\\\"sizeDelta\\\":{\\\"x\\\":160.0,\\\"y\\\":20.0},\\\"pivot\\\":{\\\"x\\\":0.5,\\\"y\\\":0.5}},{\\\"localRotation\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0,\\\"z\\\":0.0,\\\"w\\\":1.0},\\\"localPosition\\\":{\\\"x\\\":9.0,\\\"y\\\":-0.5,\\\"z\\\":0.0},\\\"localScale\\\":{\\\"x\\\":1.0,\\\"y\\\":1.0,\\\"z\\\":1.0},\\\"anchorMin\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0},\\\"anchorMax\\\":{\\\"x\\\":1.0,\\\"y\\\":1.0},\\\"anchoredPosition\\\":{\\\"x\\\":9.0,\\\"y\\\":-0.5},\\\"sizeDelta\\\":{\\\"x\\\":-28.0,\\\"y\\\":-3.0},\\\"pivot\\\":{\\\"x\\\":0.5,\\\"y\\\":0.5}}]},{\\\"CanvasObjectsLayers\\\":[\\\"Image (LT)\\\"],\\\"rectTransforms\\\":[{\\\"localRotation\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0,\\\"z\\\":0.0,\\\"w\\\":1.0},\\\"localPosition\\\":{\\\"x\\\":0.0,\\\"y\\\":130.0,\\\"z\\\":0.0},\\\"localScale\\\":{\\\"x\\\":1.0,\\\"y\\\":1.0,\\\"z\\\":1.0},\\\"anchorMin\\\":{\\\"x\\\":0.5,\\\"y\\\":0.5},\\\"anchorMax\\\":{\\\"x\\\":0.5,\\\"y\\\":0.5},\\\"anchoredPosition\\\":{\\\"x\\\":0.0,\\\"y\\\":130.0},\\\"sizeDelta\\\":{\\\"x\\\":170.0,\\\"y\\\":100.0},\\\"pivot\\\":{\\\"x\\\":0.5,\\\"y\\\":0.5}}]},{\\\"CanvasObjectsLayers\\\":[\\\"RawImage (LT)\\\"],\\\"rectTransforms\\\":[{\\\"localRotation\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0,\\\"z\\\":0.0,\\\"w\\\":1.0},\\\"localPosition\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0,\\\"z\\\":0.0},\\\"localScale\\\":{\\\"x\\\":1.0,\\\"y\\\":1.0,\\\"z\\\":1.0},\\\"anchorMin\\\":{\\\"x\\\":0.5,\\\"y\\\":0.5},\\\"anchorMax\\\":{\\\"x\\\":0.5,\\\"y\\\":0.5},\\\"anchoredPosition\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0},\\\"sizeDelta\\\":{\\\"x\\\":170.0,\\\"y\\\":100.0},\\\"pivot\\\":{\\\"x\\\":0.5,\\\"y\\\":0.5}}]}],\\\"rectTransform\\\":{\\\"localRotation\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0,\\\"z\\\":0.0,\\\"w\\\":1.0},\\\"localPosition\\\":{\\\"x\\\":960.0,\\\"y\\\":540.0,\\\"z\\\":0.0},\\\"localScale\\\":{\\\"x\\\":2.4000000953674318,\\\"y\\\":2.4000000953674318,\\\"z\\\":2.4000000953674318},\\\"anchorMin\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0},\\\"anchorMax\\\":{\\\"x\\\":0.0,\\\"y\\\":0.0},\\\"anchoredPosition\\\":{\\\"x\\\":960.0,\\\"y\\\":540.0},\\\"sizeDelta\\\":{\\\"x\\\":799.9999389648438,\\\"y\\\":449.9999694824219},\\\"pivot\\\":{\\\"x\\\":0.5,\\\"y\\\":0.5}},\\\"canvas\\\":{\\\"renderMode\\\":0,\\\"planeDistance\\\":100.0,\\\"pixelPerfect\\\":false,\\\"overrideSorting\\\":false,\\\"overridePixelPerfect\\\":false,\\\"sortingBucketNormalizedSize\\\":0.10000000149011612,\\\"vertexColorAlwaysGammaSpace\\\":false,\\\"additionalShaderChannels\\\":25,\\\"updateRectTransformForStandalone\\\":0},\\\"canvasScaler\\\":{\\\"uiScaleMode\\\":1,\\\"referencePixelsPerUnit\\\":100.0,\\\"scaleFactor\\\":1.0,\\\"referenceResolution\\\":{\\\"x\\\":800.0,\\\"y\\\":600.0},\\\"screenMatchMode\\\":0,\\\"matchWidthOrHeight\\\":0.0,\\\"physicalUnit\\\":3,\\\"fallbackScreenDPI\\\":96.0,\\\"defaultSpriteDPI\\\":96.0,\\\"dynamicPixelsPerUnit\\\":1.0,\\\"presetInfoIsWorld\\\":false},\\\"graphicRaycaster\\\":{\\\"ignoreReversedGraphics\\\":true,\\\"blockingObjects\\\":0,\\\"blockingMask\\\":{\\\"serializedVersion\\\":\\\"2\\\",\\\"m_Bits\\\":4294967295}}}\"}")
            };

            // Add the default components and canvas to the corresponding lists.
            componentSave.AddRange(languageDefaults);
            canvasSave.AddRange(canvasDefaults);
        }

        /// <summary>
        /// Draws the appropriate icon for a language component type in the Unity Editor.
        /// </summary>
        /// <param name="componentType">Integer representing the component type.</param>
        public static void DisplayComponentIcon(int componentType)
        {
            // Choose an icon texture based on component type.
            var icon = componentType switch
            {
                1 => FindTextureByName("LanguageText Icon"),
                2 => FindTextureByName("LanguageTextMesh Icon"),
                3 => FindTextureByName("LanguageTextInputField Icon"),
                4 => FindTextureByName("LanguageScript Icon"),
                5 => FindTextureByName("LanguageCreateFile Icon"),
                6 => FindTextureByName("LanguageDropdown Icon"),
                _ => FindTextureByName("Custom Item Icon"),
            };

            // If the icon is found, draw it within a centered layout area.
            if (icon != null)
            {
                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace(); // Center the content horizontally.

                // Calculate position and size for the icon.
                Rect lastRect = GUILayoutUtility.GetLastRect();
                Rect imageRect = new(lastRect.x + lastRect.width - 70f, lastRect.y, 70f, 70f);

                GUI.DrawTexture(imageRect, icon); // Draw the icon texture at calculated position.
                GUILayout.EndHorizontal();
            }
        }

        /// <summary>
        /// Searches for objects in the current scene that contain the specified ID.
        /// If no objects are found, a warning window will be displayed.
        /// If the ID is found, the matching objects will be selected in the Hierarchy.
        /// </summary>
        /// <param name="id">ID to search for in scene objects.</param>
        public static void SearchByID(int id)
        {
            // List to store GameObjects that contain the specified ID.
            List<GameObject> foundObjects = new();

            // Retrieve all GameObjects in the scene, including inactive ones and prefabs.
            var allObjects = Resources.FindObjectsOfTypeAll<GameObject>();

            // Get the currently active scene so we only check objects in this scene.
            Scene activeScene = SceneManager.GetActiveScene();

            foreach (GameObject obj in allObjects)
            {
                // Ignore GameObjects that are not part of the active scene.
                if (obj.scene != activeScene) continue;

                // Check if the object has any components with a direct 'iD' field.
                TryMatchComponentID<LanguageTextMeshTMP>(obj, id, foundObjects);
                TryMatchComponentID<LanguageTextMesh>(obj, id, foundObjects);
                TryMatchComponentID<LanguageTextTMP>(obj, id, foundObjects);
                TryMatchComponentID<LanguageText>(obj, id, foundObjects);
                TryMatchComponentID<LanguageTextInputFieldTMP>(obj, id, foundObjects);
                TryMatchComponentID<LanguageTextInputField>(obj, id, foundObjects);

                // Check if the object has components that contain a list of items, and any of those items has a matching 'iD' field.
                TryMatchListComponentID<LanguageScript, ScriptText>(obj, id, comp => comp.scriptTexts, foundObjects);
                TryMatchListComponentID<LanguageCreateFile, LanguageLines>(obj, id, comp => comp.fileLines, foundObjects);
                TryMatchListComponentID<LanguageDropdownTMP, LanguageOptions>(obj, id, comp => comp.options, foundObjects);
                TryMatchListComponentID<LanguageDropdown, LanguageOptions>(obj, id, comp => comp.options, foundObjects);
            }

            // If any matching objects were found, select them in the Hierarchy.
            if (foundObjects.Count > 0)
            {
                Selection.objects = foundObjects.ToArray();
            }
            else
            {
                // Show a dialog if no matching objects were found.
                EditorUtility.DisplayDialog("Notice", $"No object with ID {id} was found in the scene.", "OK");
            }
        }

        /// <summary>
        /// Checks if the GameObject has a component of type T with the iD field equal to the one informed.
        /// </summary>
        private static void TryMatchComponentID<T>(GameObject obj, int id, List<GameObject> result) where T : MonoBehaviour
        {
            // Try to get the component of type T from the GameObject.
            if (!obj.TryGetComponent<T>(out var comp)) return;

            // Use reflection to find the 'iD' field on the component.
            var idField = typeof(T).GetField("iD");
            if (idField != null && idField.FieldType == typeof(int))
            {
                // Get the value of the 'iD' field and compare it.
                int value = (int)idField.GetValue(comp);
                if (value == id && !result.Contains(obj))
                {
                    result.Add(obj); // Add the GameObject to the result list if it hasn't been added already.
                }
            }
        }

        /// <summary>
        /// Checks if the GameObject has a component of type TComp that contains a list of TItem,
        /// where TItem has an iD field equal to the one informed.
        /// </summary>
        private static void TryMatchListComponentID<TComp, TItem>(GameObject obj, int id, Func<TComp, List<TItem>> getList, List<GameObject> result) where TComp : MonoBehaviour
        {
            // Try to get the component of type TComp from the GameObject.
            if (!obj.TryGetComponent<TComp>(out var comp)) return;

            // Retrieve the list of items from the component using the provided delegate.
            var items = getList(comp);
            if (items == null) return;

            // Loop through each item in the list.
            foreach (TItem item in items)
            {
                // Use reflection to find the 'iD' field on the item type.
                var idField = typeof(TItem).GetField("iD");
                if (idField != null && idField.FieldType == typeof(int))
                {
                    // Get the value of the 'iD' field and compare it.
                    int value = (int)idField.GetValue(item);
                    if (value == id && !result.Contains(obj))
                    {
                        // Add the GameObject to the result list if a match is found.
                        result.Add(obj);
                        break; // Stop checking after the first match in the list.
                    }
                }
            }
        }
    }
}
#endif