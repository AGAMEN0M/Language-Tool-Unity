using System.IO;
using UnityEngine;

public class FileCopyManager : MonoBehaviour
{
    [Header("Location and File Names In Unity")]
    [SerializeField] private string[] fileNameInUnity; // Array of file names in Unity Editor.
    [SerializeField] private string folderNameInUnity; // Folder name in Unity Editor.
    [Space(20)]
    [Header("Location and File Names In Build")]
    [SerializeField] private string[] fileNameInBuild; // Array of file names in build.
    [SerializeField] private string folderNameInBuild; // Folder name in build.

    private string gameDirectory; // Root directory of the game.
    private string exitPath; // Destination directory for copied files.

    private void Start()
    {
        InitializePaths(); // Initialize file paths.
        CopyFilesToDestination(); // Copy initial files to destination.
    }

    private void FixedUpdate() { CheckAndCopyIfModified(); } // Check for modifications in files and copy if necessary.

    private void InitializePaths()
    {
        gameDirectory = Application.dataPath[..(Application.dataPath.LastIndexOf("/") + 1)]; // Get the root directory of the game.

        // Set the destination path based on whether in Unity Editor or Build.
    #if UNITY_EDITOR
        exitPath = Path.Combine(gameDirectory, folderNameInUnity);
    #else
        exitPath = Path.Combine(gameDirectory, folderNameInBuild);
    #endif

        if (!Directory.Exists(exitPath)) { Directory.CreateDirectory(exitPath); } // Create the destination directory if it doesn't exist.
    }

    private void CopyFilesToDestination()
    {
        print($"Copy File - Ds {System.DateTime.Now:dd-MM-yyyy} Hs {System.DateTime.Now:HH-mm-ss}"); // Display a message indicating file copy with timestamp.

        // Copy files based on whether in Unity Editor or Build.
    #if UNITY_EDITOR
        foreach (string file in fileNameInUnity) { CopyFileAtPath(file); }
    #else
        foreach (string file in fileNameInBuild) { CopyFileAtPath(file); }
    #endif
    }

    private void CheckAndCopyIfModified()
    {
        // Check if files have been modified and copy if necessary.
    #if UNITY_EDITOR
        foreach (string file in fileNameInUnity)
        {
            string sourcePath = Path.Combine(gameDirectory, file);
            string destinationPath = Path.Combine(exitPath, Path.GetFileName(file));

            if (File.Exists(sourcePath))
            {
                FileInfo sourceFileInfo = new(sourcePath);
                FileInfo destinationFileInfo = new(destinationPath);
                if (sourceFileInfo.LastWriteTime > destinationFileInfo.LastWriteTime) { CopyFileAtPath(file); }
            }
        }
    #else
        foreach (string file in fileNameInBuild)
        {
            string sourcePath = Path.Combine(gameDirectory, file);
            string destinationPath = Path.Combine(exitPath, Path.GetFileName(file));

            if (File.Exists(sourcePath))
            {
                FileInfo sourceFileInfo = new(sourcePath);
                FileInfo destinationFileInfo = new(destinationPath);
                if (sourceFileInfo.LastWriteTime > destinationFileInfo.LastWriteTime) { CopyFileAtPath(file); }
            }
        }
    #endif
    }

    private void CopyFileAtPath(string filePath)
    {
        // Copy a file from source to destination.
        string sourcePath = Path.Combine(gameDirectory, filePath);
        string destinationPath = Path.Combine(exitPath, Path.GetFileName(filePath));
        print($"Copying file: {sourcePath}");
        if (File.Exists(sourcePath)) { File.Copy(sourcePath, destinationPath, true); }
    }
}