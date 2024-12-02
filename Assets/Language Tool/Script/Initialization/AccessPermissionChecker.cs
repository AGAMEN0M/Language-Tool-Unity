/*
 * ---------------------------------------------------------------------------
 * Description: This script checks access permissions for essential folders 
 *              in the Unity project and displays a warning if access is restricted.
 * Author: Lucas Gomes Cecchini
 * Pseudonym: AGAMENOM
 * ---------------------------------------------------------------------------
*/

using LanguageTools;
using UnityEngine;
using System.IO;

public class AccessPermissionChecker : MonoBehaviour
{
    // This method is called when the game starts to check access permissions.
    [RuntimeInitializeOnLoadMethod]
    public static void InitializeCheckSettings()
    {
        CheckAccessPermissions(); // Start checking access permissions for necessary folders.
    }

    // This method checks access to essential directories and logs errors if access is restricted.
    private static void CheckAccessPermissions()
    {
        // Check access to the "Assets" and "StreamingAssets" folders.
        bool hasDataPathAccess = CheckFolderAccess(Application.dataPath);
        bool hasStreamingAssetsPathAccess = CheckFolderAccess(Application.streamingAssetsPath);

        // If access to either folder is restricted, display a warning.
        if (!hasDataPathAccess || !hasStreamingAssetsPathAccess)
        {
            ShowWarning(); // Show a warning to the user.
            Debug.LogError("Access to necessary folders is restricted. Warning displayed.");
        }
        else
        {
            Debug.Log("Folder access verified successfully.");
        }
    }

    // This method tests whether the application has access to a specified folder by attempting to write and delete a file.
    private static bool CheckFolderAccess(string path)
    {
        try
        {
            // If the folder does not exist, return false.
            if (!Directory.Exists(path)) return false;

            // Create a temporary file to test write and delete permissions.
            string testFilePath = Path.Combine(path, "TestAccessFile.tmp");
            File.WriteAllText(testFilePath, "Test Access"); // Write a test file.
            File.Delete(testFilePath); // Delete the test file.

            return true; // Return true if file operations succeed.
        }
        catch (IOException ex) // Handle any I/O exceptions, such as file access issues.
        {
            Debug.LogError($"IOException occurred: {ex.Message}");
            return false;
        }
        catch (System.UnauthorizedAccessException ex) // Handle unauthorized access exceptions.
        {
            Debug.LogError($"UnauthorizedAccessException occurred: {ex.Message}");
            return false;
        }
    }

    // This method displays a warning to the user if folder access is restricted.
    private static void ShowWarning()
    {
        // Load the language settings to get the warning UI element.
        var settingsData = LanguageFileManager.LoadLanguageSettings();

        if (settingsData == null) // If settings data is missing, log an error.
        {
            Debug.LogError("The languageSettingsData is not assigned.");
            return;
        }

        // Instantiate the warning GameObject from the language settings.
        GameObject warningInstance = Instantiate(settingsData.errorLanguageTool);
        if (warningInstance != null)
        {
            DontDestroyOnLoad(warningInstance); // Ensure the warning persists across scenes.
            Debug.Log("Warning instance created and set to not destroy on load.");
        }
        else
        {
            Debug.LogError("Failed to instantiate the warning GameObject.");
        }
    }
}