using System;
using System.Diagnostics;
using UnityEngine;

public class OpenFileStartup : MonoBehaviour
{
    [Header("Location and File Names")]
    [SerializeField] private string[] fileNameInUnity;
    [Space(10)]
    [SerializeField] private string[] fileNameInBuild;

    private void Start()
    {
        OpenFile(); // When the script starts, open the specified files.
    }

    public void OpenFile()
    {
        print("OpenFile"); // Print a message to indicate that the OpenFile method is running.

        Process proc = null;
        try
        {          
            string Dir = Application.dataPath[..(Application.dataPath.LastIndexOf("/") + 1)]; // Get the directory where the Unity project is located.

        #if UNITY_EDITOR
            // If running in Unity Editor, open files from FileNameInUnity array.
            foreach (string file in fileNameInUnity)
            {
                proc = new Process(); // Create a new process for each file.
                proc.StartInfo.Arguments = "start"; // Set the argument (not needed for file opening).
                proc.StartInfo.FileName = Dir + (file); // Set the file name to the directory path concatenated with the file name.
                proc.Start(); // Start the process (attempt to open the file with the default application).
            }
        #else
            // If not running in Unity Editor (i.e., in a build), open files from FileNameInBuild array.
            foreach (string file in fileNameInBuild)
            {
                proc = new Process(); // Create a new process for each file.
                proc.StartInfo.Arguments = "start"; // Set the argument (not needed for file opening).
                proc.StartInfo.FileName = Dir + (file); // Set the file name to the directory path concatenated with the file name.
                proc.Start(); // Start the process (attempt to open the file with the default application).
            }
        #endif
        }        
        catch (Exception ex)
        {
            print(ex.StackTrace.ToString()); // Print the stack trace of any exceptions that occur during the process.
        }
    }
}