/*
 * ---------------------------------------------------------------------------
 * Description: Verifies runtime access permissions to critical Unity project
 *              directories (Assets and StreamingAssets). Displays an in-game
 *              warning if access is denied.
 * Author: Lucas Gomes Cecchini
 * Pseudonym: AGAMENOM
 * ---------------------------------------------------------------------------
*/

using LanguageTools;
using UnityEngine;
using System.IO;

public class AccessPermissionChecker : MonoBehaviour
{
    /// <summary>
    /// Automatically called on game start to verify folder access permissions.
    /// Displays a warning if access to necessary directories is restricted.
    /// </summary>
    [RuntimeInitializeOnLoadMethod]
    public static void InitializeCheckSettings() => CheckAccessPermissions();

    // Checks folder access for Assets and StreamingAssets directories.
    private static void CheckAccessPermissions()
    {
        // Verify read/write permissions for Assets folder.
        bool hasAccessToAssets = CheckFolderAccess(Application.dataPath);

        // Verify read/write permissions for StreamingAssets folder.
        bool hasAccessToStreaming = CheckFolderAccess(Application.streamingAssetsPath);

        // If either folder is inaccessible, show warning.
        if (!hasAccessToAssets || !hasAccessToStreaming)
        {
            Debug.LogError("Access to one or more required folders is restricted.");
            ShowWarning();
        }
        else
        {
            Debug.Log("Folder access check passed.");
        }
    }

    // Attempts to create and delete a temporary file in the given directory.
    // Returns true if both operations succeed.
    private static bool CheckFolderAccess(string path)
    {
        // Return false immediately if the directory doesn't exist.
        if (!Directory.Exists(path)) return false;

        string testFile = Path.Combine(path, "TestAccessFile.tmp");

        try
        {
            File.WriteAllText(testFile, "Test Access"); // Try writing to the directory.
            File.Delete(testFile); // Try deleting the written file.
            return true;
        }
        catch (IOException ex)
        {
            Debug.LogError($"IO error while checking access to {path}: {ex.Message}");
        }
        catch (System.UnauthorizedAccessException ex)
        {
            Debug.LogError($"Access denied to {path}: {ex.Message}");
        }

        return false;
    }

    // Loads the warning UI from language settings and ensures it persists between scenes.
    private static void ShowWarning()
    {
        // Load language settings that include the warning prefab reference.
        var settings = LanguageFileManager.LoadLanguageSettings();

        if (settings == null)
        {
            Debug.LogError("Language settings could not be loaded.");
            return;
        }

        // Instantiate the warning GameObject defined in the settings.
        var warning = Instantiate(settings.errorLanguageTool);

        if (warning != null)
        {
            // Ensure the warning UI persists across scene loads.
            DontDestroyOnLoad(warning);
            Debug.Log("Warning displayed and marked as persistent.");
        }
        else
        {
            Debug.LogError("Failed to instantiate warning GameObject.");
        }
    }
}