using System.Globalization;
using System.IO;
using UnityEngine;

public static class LanguageClassTools
{
    // GetFolderPath method generates a folder path based on conditions.
    // In Unity Editor, it uses FolderNameInUnity; in a build, it uses FolderNameInBuild.
    public static string GetFolderPath(string folderNameInUnity, string folderNameInBuild)
    {
    #if UNITY_EDITOR
        return Application.dataPath + folderNameInUnity; // In Unity Editor, append the specified Unity folder path.
    #else
        return Application.dataPath + folderNameInBuild; // In a build, append the specified build folder path.
    #endif
    }

    // GetJsonPath method generates a JSON file path based on conditions.
    // In Unity Editor, it uses jsonNameInUnity; in a build, it uses jsonNameInBuild.
    public static string GetJsonPath(string jsonNameInUnity, string jsonNameInBuild)
    {
    #if UNITY_EDITOR
        return Application.dataPath + jsonNameInUnity; // In Unity Editor, append the specified Unity folder path.
    #else
        return Application.dataPath + jsonNameInBuild; // In a build, append the specified build folder path.
    #endif
    }

    // FindDefaultLanguageFile method searches for a default language file in a directory.
    // It looks for files with a .txt extension and checks if they contain a line starting with standardFile.
    // If found, it returns the path to the matching file; otherwise, it returns null.
    public static string FindDefaultLanguageFile(string path, string standardFile)
    {
        string[] files = Directory.GetFiles(path, "*.txt"); // Get all files with .txt extension in the specified directory.

        // Iterate through each file.
        foreach (string filePath in files)
        {
            string[] lines = File.ReadAllLines(filePath); // Read all lines in the file.

            // Iterate through each line in the file.
            foreach (string line in lines)
            {
                // Check if the line starts with the provided standardFile.
                if (line.StartsWith(standardFile))
                {
                    return filePath; // If a matching line is found, return the path to the file.
                }
            }
        }

        return null; // If no matching file is found, return null.
    }

    // RemoveContentWithinCurlyBraces method removes content within curly braces from a string.
    public static string RemoveContentWithinCurlyBraces(string input)
    {
        int startIndex = input.IndexOf("{");
        int endIndex = input.IndexOf("}");

        if (startIndex >= 0 && endIndex > startIndex)
        {
            return input.Remove(startIndex, endIndex - startIndex + 1); // Remove the content between the opening and closing curly braces, including the braces themselves.
        }
        return input; // Return the input string as is if there are no curly braces.
    }

    // ExtractTextWithinBraces method extracts text within curly braces from a string.
    public static string ExtractTextWithinBraces(string input)
    {
        int startIndex = input.IndexOf("{");
        int endIndex = input.IndexOf("}");

        if (startIndex >= 0 && endIndex > startIndex)
        {
            // Extract the text inside the braces.
            string textInsideBraces = input.Substring(startIndex + 1, endIndex - startIndex - 1);
            return textInsideBraces;
        }

        //return input; // Return the original input if there are no braces.
        return ""; // Returns an empty string if there are no keys or if they are poorly positioned.
    }

    // ExtractIntValue method extracts an integer value from a string based on an identifier.
    public static int ExtractIntValue(string line, string identifier)
    {
        int startIndex = line.IndexOf(identifier);
        if (startIndex >= 0)
        {
            int semicolonIndex = line.IndexOf(";", startIndex);
            if (semicolonIndex >= 0)
            {
                // Extract the value string between the identifier and the semicolon.
                string valueString = line.Substring(startIndex + identifier.Length, semicolonIndex - startIndex - identifier.Length);
                if (int.TryParse(valueString, out int value))
                {
                    return value;
                }
            }
        }
        return 0; // Default value if the identifier is not found or the value is not a valid integer.
    }

    // ExtractFloatValue method extracts a float value from a string based on an identifier.
    public static float ExtractFloatValue(string line, string identifier)
    {
        int startIndex = line.IndexOf(identifier);
        if (startIndex >= 0)
        {
            int semicolonIndex = line.IndexOf(";", startIndex);
            if (semicolonIndex >= 0)
            {
                string valueString = line.Substring(startIndex + identifier.Length, semicolonIndex - startIndex - identifier.Length); // Extract the value string between the identifier and the semicolon.
                valueString = valueString.Replace(",", "."); // Replace comma with a period before attempting to convert.

                if (float.TryParse(valueString, NumberStyles.Float, CultureInfo.InvariantCulture, out float value))
                {
                    return value;
                }
            }
        }
        return 0.0f; // Default value if the identifier is not found or the value is not a valid floating-point number.
    }

    // ExtractBooleanValue method extracts a boolean value from a string based on an identifier.
    // It interprets 2 as true, 1 as false, and any other value as the default value.
    public static bool ExtractBooleanValue(string line, string identifier, bool defaultValue)
    {
        int startIndex = line.IndexOf(identifier);
        if (startIndex >= 0)
        {
            int semicolonIndex = line.IndexOf(";", startIndex);
            if (semicolonIndex >= 0)
            {
                // Extract the value string between the identifier and the semicolon.
                string valueString = line.Substring(startIndex + identifier.Length, semicolonIndex - startIndex - identifier.Length);
                if (int.TryParse(valueString, out int value))
                {
                    if (value == 2)
                    {
                        return true; // Interpret 2 as true.
                    }
                    else if (value == 1)
                    {
                        return false; // Interpret 1 as false.
                    }
                }
            }
        }
        return defaultValue; // Return the default value if the identifier is not found or the value is not recognized.
    }

    // ExtractLanguageName method retrieves the language name from a language file.
    // It reads the file located at 'filePath' and attempts to extract the language name.
    // The language name is expected within the file's contents, and its location might vary.
    // If the language name is successfully extracted, it is returned; otherwise, 'English (United States)' is used as the default.
    public static string ExtractLanguageName(string filePath)
    {
        string[] lines = File.ReadAllLines(filePath); // Read all lines from the specified file.

        // Check if there are at least two lines in the file to ensure the presence of the language name.
        if (lines.Length > 1)
        {
            // Extract the language name from the second line assuming it contains the language identifier.
            return lines[1]; // Retrieve the language name from the second line.
        }

        // If there's insufficient content in the file, issue a warning and set 'English (United States)' as the default language.
        Debug.LogWarning("Extract Language Name: Insufficient Content in the File");
        return "English (United States)"; // Set 'English (United States)' as the default language.
    }
}