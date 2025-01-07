using UnityEngine;
using UnityEditor;
using TSVTools;

public class ExampleTabTableUsage : MonoBehaviour
{
    public VerticalTable[] table;
    public string filePath;
    [Space(10)]
    [TextArea]public string text;
    public int columnVertical;
    public int columnHorizontal;
    [Space(10)]
    public TabTableUtility.LineDirection direction;
    public int AddColumn;

    [ContextMenu("Get File")]
    private void GetFile()
    {
        filePath = EditorUtility.OpenFilePanel("Select TSV File", Application.dataPath, "tsv");
        if (!string.IsNullOrEmpty(filePath))
        {
            TabTableUtility.LoadTableFile(filePath, ref table);
            Debug.Log("File loaded successfully.");
        }
        else
        {
            Debug.LogWarning("No file selected.");
        }
    }

    [ContextMenu("Save File")]
    private void SaveFile()
    {
        if (table == null || table.Length == 0)
        {
            Debug.LogError("Table is empty. Cannot save.");
            return;
        }

        string savePath = EditorUtility.SaveFilePanel("Save TSV File", Application.dataPath, "table", "tsv");
        if (!string.IsNullOrEmpty(savePath))
        {
            TabTableUtility.SaveTableFile(savePath, table);
            Debug.Log($"File saved successfully at: {savePath}");
        }
        else
        {
            Debug.LogWarning("Save operation cancelled.");
        }
    }

    [ContextMenu("Get Text")]
    private void GetText()
    {
        if (table == null || table.Length == 0)
        {
            Debug.LogError("Table is empty. Load a file first.");
            return;
        }

        string cellText = TabTableUtility.GetText(table, columnVertical, columnHorizontal);
        if (cellText != null)
        {
            Debug.Log($"Text at ({columnVertical}, {columnHorizontal}): {cellText}");
        }
    }

    [ContextMenu("Set Text")]
    private void SetText()
    {
        if (table == null || table.Length == 0)
        {
            Debug.LogError("Table is empty. Load a file first.");
            return;
        }

        TabTableUtility.SetText(ref table, columnVertical, columnHorizontal, text);
        Debug.Log($"Text set at ({columnVertical}, {columnHorizontal}): {text}");
    }

    [ContextMenu("Add Column")]
    private void AddColumnInTable()
    {
        if (table == null || table.Length == 0)
        {
            Debug.LogError("Table is empty. Load a file first.");
            return;
        }

        TabTableUtility.AddLine(ref table, AddColumn, direction);
    }

    [ContextMenu("Remove Column")]
    private void RemoveColumnInTable()
    {
        if (table == null || table.Length == 0)
        {
            Debug.LogError("Table is empty. Load a file first.");
            return;
        }

        TabTableUtility.RemoveLine(ref table, AddColumn, direction);
    }
}