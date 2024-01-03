using System;
using UnityEngine;
using UnityEngine.UI;
using System.Diagnostics;

[RequireComponent(typeof(Button))]
public class ButtonsOpenFile : MonoBehaviour
{
    [Header("Location and File Names")] // Serializable fields for file names in the Unity Editor and in a build.
    [SerializeField] private string[] FileNameInUnity;
    [SerializeField] private string[] FileNameInBuild;

    public void OpenFile()
    {
        // Print a message in the console.
        print("OpenFile");
        
        // Create a process variable.
        Process proc = null;
        try
        {
            // Get the directory of the data path and remove the last "/" and everything after it.
            string Dir = Application.dataPath.Substring(0, Application.dataPath.LastIndexOf("/") + 1);

        #if UNITY_EDITOR
            // Loop through all file names in the "FileNameInUnity" array.
            foreach (string file in FileNameInUnity)
            {
                // Create a new process.
                proc = new Process();
                proc.StartInfo.Arguments = "start";
                
                // Set the file name and start the process.
                proc.StartInfo.FileName = Dir + (file);
                proc.Start();                
            }
        #else
            // Loop through all file names in the "FileNameInBuild" array.
            foreach (string file in FileNameInBuild)
            {
                // Create a new process.
                proc = new Process();
                proc.StartInfo.Arguments = "start";
                
                // Set the file name and start the process.
                proc.StartInfo.FileName = Dir + (file);
                proc.Start();                        
            }
        #endif
        }
        
        // Catch any exceptions that might occur during the process.
        catch (Exception ex)
        {
            // Print the stack trace of the exception in the console.
            print(ex.StackTrace.ToString());
        }
    }
}