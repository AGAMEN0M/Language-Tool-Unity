/*
 * ---------------------------------------------------------------------------
 * Description: This script provides a Unity editor interface for testing the 
 *              functionality of the TabTableUtility class. It allows loading, 
 *              saving, modifying, and viewing content in TSV tables. Operations 
 *              such as adding and removing rows or columns, and setting or getting 
 *              text in specific cells are available through the Unity Editor.
 * Author: Lucas Gomes Cecchini
 * Pseudonym: AGAMENOM
 * ---------------------------------------------------------------------------
*/

using UnityEngine;
using TSVTools;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class TestTable : MonoBehaviour
{
#if UNITY_EDITOR
    public VerticalTable[] table; // Array to hold the loaded table data.
    public string filePath; // Path to the TSV file.
    [Space(10)]
    [TextArea] public string text; // Text input for setting a specific cell's content.
    // Indices for selecting a specific cell in the table.
    public int columnVertical;
    public int columnHorizontal;
    [Space(10)]
    public TabTableUtility.LineDirection direction; // Direction for adding/removing rows/columns.
    public int AddColumn; // Position for adding or removing a column or row.

    // Context menu to open a file selection dialog and load a TSV file into the table.
    [ContextMenu("Get File")]
    private void GetFile()
    {
        // Open a file panel for the user to select a TSV file.
        filePath = EditorUtility.OpenFilePanel("Select TSV File", Application.dataPath, "tsv");

        if (!string.IsNullOrEmpty(filePath))
        {
            // Load the table using the selected file path.
            TabTableUtility.LoadTableFile(filePath, ref table);
            Debug.Log("File loaded successfully.");
        }
        else
        {
            Debug.LogWarning("No file selected.");
        }
    }

    // Helper function to check if the table is loaded and valid.
    private bool Checking()
    {
        // If the table is empty or null, log an error and return true.
        if (table == null || table.Length == 0)
        {
            Debug.LogError("Table is empty. Load a file first.");
            return true;
        }

        return false;
    }

    // Context menu to save the current table to a file.
    [ContextMenu("Save File")]
    private void SaveFile()
    {
        if (Checking()) return;

        // Open a save file dialog and get the save path.
        string savePath = EditorUtility.SaveFilePanel("Save TSV File", Application.dataPath, "table", "tsv");

        if (!string.IsNullOrEmpty(savePath))
        {
            // Save the table to the selected path.
            TabTableUtility.SaveTableFile(savePath, table);
            Debug.Log($"File saved successfully at: {savePath}");
        }
        else
        {
            Debug.LogWarning("Save operation cancelled.");
        }
    }

    // Context menu to get the text from a specific cell in the table.
    [ContextMenu("Get Text")]
    private void GetText()
    {
        if (Checking()) return;

        // Retrieve the text from the specified cell.
        string cellText = TabTableUtility.GetText(table, columnVertical, columnHorizontal);

        if (cellText != null)
        {
            // Log the text from the specified cell.
            Debug.Log($"Text at ({columnVertical}, {columnHorizontal}): {cellText}");
        }
    }

    // Context menu to set the text of a specific cell in the table.
    [ContextMenu("Set Text")]
    private void SetText()
    {
        if (Checking()) return;

        // Set the text in the specified cell.
        TabTableUtility.SetText(ref table, columnVertical, columnHorizontal, text);
        Debug.Log($"Text set at ({columnVertical}, {columnHorizontal}): {text}");
    }

    // Context menu to add a row or column in the table at the specified position.
    [ContextMenu("Add Column")]
    private void AddColumnInTable()
    {
        if (Checking()) return;

        // Add a row or column depending on the direction.
        TabTableUtility.AddLine(ref table, AddColumn, direction);
    }

    // Context menu to remove a row or column from the table at the specified position.
    [ContextMenu("Remove Column")]
    private void RemoveColumnInTable()
    {
        if (Checking()) return;

        // Remove a row or column depending on the direction.
        TabTableUtility.RemoveLine(ref table, AddColumn, direction);
    }
#endif
}