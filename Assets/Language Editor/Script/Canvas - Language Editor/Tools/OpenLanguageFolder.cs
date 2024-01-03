using UnityEngine;

public class OpenLanguageFolder : MonoBehaviour
{
    [Header("Archives Location")]
    #pragma warning disable CS0414
    [SerializeField] private string folderNameInUnity = "/StreamingAssets/Language/"; // Folder name in the Unity Editor (inside the 'Assets' folder).
    [SerializeField] private string folderNameInBuild = "/StreamingAssets/Language/"; // Folder name in the build (inside the persistent data directory).
    #pragma warning restore CS0414
    [Space(10)]
    [SerializeField] private bool inData = true; // Flag to determine whether to use the path in 'Assets' or in the build directory.

    // Method to open the folder in file explorer.
    public void OpenFolder()
    {
        string path = "";

        // Checks whether to use the path in 'Assets' or in the build directory.
        if (inData)
        {
        #if UNITY_EDITOR
            path = Application.dataPath + folderNameInUnity; // Path in the Unity editor (inside the 'Assets' folder).
        #else
            path = Application.dataPath + folderNameInBuild; // Build path (inside the persistent data directory).
        #endif
        }
        else
        {
            string dir = Application.dataPath[..Application.dataPath.LastIndexOf("/")]; // Get the parent directory of 'Assets' in the application path.

        #if UNITY_EDITOR
            path = dir + folderNameInUnity; // Path in the Unity editor (outside the 'Assets' folder).
        #else
            path = dir + folderNameInBuild; // Build path (outside the persistent data directory).
        #endif
        }

        Application.OpenURL(path); // Open the URL (or folder) in file explorer.
        Debug.Log($"Open Folder: {path}"); // Log the message for debugging.
    }
}