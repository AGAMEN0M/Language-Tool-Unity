using UnityEngine;
using UnityEngine.UI;

public class FileNameWhenStarting : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private Text text; // Reference to the UI Text element.
    public string fileName = "File Name..."; // Default filename.

    private string oldFileName; // Variable to store the old filename.

    private void Start()
    {
        text.text = fileName; // Set the initial text to the default filename.
        oldFileName = fileName; // Store the default filename as the old filename.
    }

    public void TextUpdat()
    {
        // Check if the current text matches the old filename.
        if (text.text == oldFileName)
        {
            text.text = fileName; // Update the text to the current filename.
            oldFileName = fileName; // Set the current filename as the new old filename.
        }
    }
}