/*
 * ---------------------------------------------------------------------------
 * Description: This script manages the loading and saving of language settings 
 *              for the application. It provides methods for retrieving localized strings, 
 *              handling file paths, and extracting content from language files.
 * Author: Lucas Gomes Cecchini
 * Pseudonym: AGAMENOM
 * ---------------------------------------------------------------------------
*/

using UnityEngine;
using System.IO;

namespace LanguageTools
{
    public class LanguageFileManager
    {
        // This method attempts to load the LanguageSettingsData resource from the Resources folder.
        // If the resource is not found, it logs an error and returns null.
        public static LanguageSettingsData LoadLanguageSettings()
        {
            // Attempt to load the LanguageSettingsData from the Resources folder.
            var languageData = Resources.Load<LanguageSettingsData>("Language Data");
            if (languageData == null)
            {
                Debug.LogError("Failed to load LanguageSettingsData from Resources. Ensure the resource exists and is named correctly.");
                return null; // Return null if loading failed.
            }

            return languageData; // Return the loaded LanguageSettingsData.
        }

        // This method retrieves the save file path for the language settings.
        // The path differs depending on whether the game is running in the Unity Editor or in a build.
        public static string GetSaveFilePath()
        {
            // Load language settings to get the save file name.
            var languageData = LoadLanguageSettings();
            if (languageData == null)
            {
                Debug.LogError("Failed to load LanguageSettingsData");
                return ""; // Return an empty string if there is no valid data.
            }

            string saveFile = "";
        #if UNITY_EDITOR
            // In the Unity Editor, the save file is located in the Assets folder.
            saveFile = $"{Application.dataPath}/{languageData.saveNameInUnity}.json";
        #else
            // In a build, the save file is located in the Application's data path.
            saveFile = $"{Application.dataPath}/{languageData.saveNameInUnity}.json";
        #endif

            return saveFile; // Return the constructed file path.
        }

        // This method returns the folder path where the language files are stored.
        // The folder name is specified in the LanguageSettingsData.
        public static string GetLanguageFilesFolderPath()
        {
            // Load language settings to get the folder name.
            var languageData = LoadLanguageSettings();
            if (languageData == null)
            {
                Debug.LogError("Failed to load LanguageSettingsData");
                return ""; // Return an empty string if there is no valid data.
            }

            // Return the constructed folder path for language files.
            return $"{Application.streamingAssetsPath}/{languageData.folderName}";
        }

        // This method finds the default language file path based on the default language setting.
        public static string FindDefaultLanguageFilePath()
        {
            string defaultLanguage = LoadLanguageSettings().defaultLanguage; // Retrieve the default language from the settings.
            string[] files = Directory.GetFiles(GetLanguageFilesFolderPath(), "*.txt"); // Get all language files with the ".txt" extension.

            // Loop through each file to find the one with the matching default language.
            foreach (string filePath in files)
            {
                string[] lines = File.ReadAllLines(filePath);
                if (lines.Length > 1)
                {
                    // If the second line matches the default language, return the file path.
                    if (lines[1] == defaultLanguage) return filePath;
                }
            }

            return ""; // Return an empty string if no matching file is found.
        }

        // This method finds a line by ID in a language file.
        public static string FindLineByID(string filePath, int iD)
        {
            string[] lines = File.ReadAllLines(filePath); // Read all lines from the specified file.

            // Iterate through each line to search for the ID.
            foreach (string line in lines)
            {
                if (line.StartsWith("id:"))
                {
                    // Split the line into parts and extract the ID.
                    string[] parts = line.Split(';');
                    float id = float.Parse(parts[0].Replace("id:", ""));

                    if (iD == id) return line; // If the current line's ID matches the provided ID, return the line.
                }
            }

            return "ID not found"; // Return this if no matching ID is found.
        }

        // This method finds a canvas by its ID in a language file.
        public static string FindCanvasByID(string filePath, int canvasID)
        {
            string[] lines = File.ReadAllLines(filePath); // Read all lines from the specified file.

            // Iterate through the lines to search for the canvas ID.
            foreach (string line in lines)
            {
                if (line.StartsWith("canvasID:"))
                {
                    // Extract the canvas ID and compare it with the provided ID.
                    string[] parts = line.Split(';');
                    float id = float.Parse(parts[0].Replace("canvasID:", ""));

                    // If the current line's canvas ID matches, return the remaining content.
                    if (canvasID == id) return line.Replace(parts[0] + ";", "");
                }
            }

            return "CanvasID not found"; // Return this if no matching canvas ID is found.
        }

        // This method retrieves the localized line by ID from a save file or the default file.
        public static string GetLocalizedLineByID(int iD, string saveFile, ref string selectedFile)
        {
            string line;

            // Check if the save file exists.
            if (File.Exists(saveFile))
            {
                // Read the save file and deserialize it to get the selected language file.
                string json = File.ReadAllText(saveFile);
                var saveData = JsonUtility.FromJson<LanguageSaveData>(json);
                selectedFile = saveData.selectedFile;

                line = FindLineByID(selectedFile, iD); // Find the line by ID in the selected file.
            }
            else
            {
                // If no save file exists, use the default language file.
                string defaultFile = FindDefaultLanguageFilePath();
                selectedFile = defaultFile;
                line = FindLineByID(defaultFile, iD);
            }

            return line; // Return the found line.
        }

        // This method retrieves the localized JSON content for a canvas by its ID.
        public static string GetLocalizedJsonByID(int canvasID, string saveFile, ref string selectedFile)
        {
            string jsonLine;

            // Check if the save file exists.
            if (File.Exists(saveFile))
            {
                // Read the save file and deserialize it to get the selected language file.
                string json = File.ReadAllText(saveFile);
                var saveData = JsonUtility.FromJson<LanguageSaveData>(json);
                selectedFile = saveData.selectedFile;

                // Find the canvas content by ID in the selected file.
                jsonLine = FindCanvasByID(selectedFile, canvasID);
            }
            else
            {
                // If no save file exists, use the default language file.
                string defaultFile = FindDefaultLanguageFilePath();
                selectedFile = defaultFile;
                jsonLine = FindCanvasByID(defaultFile, canvasID);
            }

            return jsonLine;
        }

        // This method extracts text enclosed in curly braces from the input string.
        public static string ExtractTextBetweenBraces(string input)
        {
            int startIndex = input.IndexOf("{");
            int endIndex = input.LastIndexOf("}");

            if (startIndex >= 0 && endIndex > startIndex)
            {
                // Extract and return the text between the braces.
                string textInsideBraces = input.Substring(startIndex + 1, endIndex - startIndex - 1);
                return textInsideBraces;
            }

            return ""; // Return an empty string if no valid braces are found.
        }

        // This method removes the text enclosed in curly braces from the input string.
        public static string RemoveTextBetweenBraces(string input)
        {
            int startIndex = input.IndexOf("{");
            int endIndex = input.LastIndexOf("}");

            if (startIndex >= 0 && endIndex > startIndex)
            {
                // Remove the text between the braces (including the braces themselves).
                return input.Remove(startIndex, endIndex - startIndex + 1);
            }

            return input; // Return the input string as-is if no braces are found.
        }

        // This method extracts an integer value from a string based on a given identifier.
        public static int ExtractIntValue(string line, string identifier)
        {
            int startIndex = line.IndexOf(identifier);
            if (startIndex >= 0)
            {
                int semicolonIndex = line.IndexOf(";", startIndex);
                if (semicolonIndex >= 0)
                {
                    // Extract the value string and convert it to an integer.
                    string valueString = line.Substring(startIndex + identifier.Length, semicolonIndex - startIndex - identifier.Length);
                    if (int.TryParse(valueString, out int value)) return value; // Return the extracted value.
                }
            }

            return 0; // Return 0 if the identifier is not found or conversion fails.
        }

        // This method retrieves the language tag from a specified file.
        public static string GetLanguageTagFromFile(string filePath)
        {
            string[] lines = File.ReadAllLines(filePath); // Read all lines from the file.
            if (lines.Length > 1) return lines[1]; // If there are sufficient lines, return the language tag from the second line.

            // Log a warning if the file content is insufficient and return the default language.
            Debug.LogWarning("Extract Language Name: Insufficient Content in the File");
            return LoadLanguageSettings().defaultLanguage;
        }
    }
}