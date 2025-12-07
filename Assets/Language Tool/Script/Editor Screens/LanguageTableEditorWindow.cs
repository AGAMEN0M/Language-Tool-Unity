/*
 * ---------------------------------------------------------------------------
 * Description: Custom Unity Editor window for managing language tables.
 *              Provides functionality to load, save, edit, and resize TSV-based
 *              language tables. Supports visibility toggles, row/column selection,
 *              and undo/redo operations.
 *              
 * Author: Lucas Gomes Cecchini
 * Pseudonym: AGAMENOM
 * ---------------------------------------------------------------------------
*/

#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using System.IO;
using TSVTools;
using System;

using static LanguageTools.Editor.LanguageEditorUtilities;
using static LanguageTools.LanguageFileManager;
using static TSVTools.TabTableUtility;

public class LanguageTableEditorWindow : EditorWindow
{
    #region === Enums ===

    /// <summary>
    /// Defines the mode used to load table data.
    /// </summary>
    public enum ChargingMode
    {
        /// <summary>Load from a custom TSV file.</summary>
        CustomFile,

        /// <summary>Load from LanguageData.tsv.</summary>
        LanguageData,

        /// <summary>Load from MetaData.tsv.</summary>
        MetaData,

        /// <summary>Load from CanvasData.tsv.</summary>
        CanvasData
    }

    #endregion

    #region === Core Data ===

    public VerticalTable[] tableData; // All table data for the editor.
    public string currentFilePath = ""; // Currently loaded file path.
    public List<int> selectedRows = new(); // List of selected row indices.
    public List<int> selectedColumns = new(); // List of selected column indices.
    public bool showOnlySelected = false; // If true, only selected rows and columns are shown.
    public float[] rowHeights; // Row heights in the table.
    public float[] columnWidths; // Column widths in the table.
    public ChargingMode currentChargingMode = ChargingMode.CustomFile; // Current mode used to load table data.

    #endregion

    #region === Interaction State ===

    public bool isResizingRow = false; // True if a row is being resized.
    public bool isResizingColumn = false; // True if a column is being resized.
    public Vector2 initialResizePos; // Initial mouse position when resizing.
    public float originalSize; // Original size of row/column before resizing.
    public int resizeIndex; // Index of the row/column being resized.
    public Vector2 mainScroll; // Main scroll position for the editor.
    public Vector2 tableScroll; // Scroll position inside the table.
    public bool isDirty = false; // Indicates unsaved changes.
    private Vector2 lastScroll; // Last scroll position used to detect scroll changes.

    #endregion

    #region === File Paths ===

    private string assetsPath; // Path to assets folder.
    private string languageDataPath; // Path to LanguageData.tsv
    private string metaDataPath; // Path to MetaData.tsv
    private string canvasDataPath; // Path to CanvasData.tsv

    #endregion

    #region === Constants ===

    private const float MIN_COLUMN_WIDTH = 50f; // Minimum column width.
    private const float MIN_ROW_HEIGHT = 30f; // Minimum row height.

    #endregion

    #region === Colors ===

    private static Color COLOR_LINE_DEFAULT = new(0.7f, 0.7f, 0.7f); // Default color for table lines and borders.
    private static Color COLOR_LINE_HIGHLIGHT = Color.red; // Color used to highlight special lines or errors.
    private static Color COLOR_EDITOR_BACKGROUND = Color.black; // Background color of the editor window.
    private static Color COLOR_LABEL_BACKGROUND = Color.blue; // Background color for row and column labels.
    private static Color COLOR_SELECTED = Color.violetRed; // Color used to indicate selected rows or columns.
    private static Color COLOR_BACKGROUND = Color.white; // Default background color for cells.
    private static Color COLOR_INFO = Color.yellow; // Background color for informational cells.
    private static Color COLOR_DATA = Color.gray; // Background color for regular data cells.
    private static Color COLOR_CONTEXT = new(0.31f, 0.53f, 0.89f); // Background color for context cells.
    private static Color COLOR_ID = new(0.984f, 0.843f, 0.471f); // Background color for ID cells.
    private static Color COLOR_TRUE = Color.green; // Background color for boolean cells set to True.
    private static Color COLOR_LIGHT = Color.cyan; // Background color for light-themed cells or highlights.
    private static Color COLOR_NUMBER = Color.orange; // Background color for numeric cells.
    private static Color COLOR_DEFAULT = Color.lightCyan; // Default background color for miscellaneous cells.

    #endregion

    #region === GUI Styles ===

    private GUIStyle styleCellText; // Style used for editable text inside table cells.
    private GUIStyle styleHeader; // Style used for column and row headers.
    private GUIStyle styleNonEditableCenter; // Style used for non-editable cells with centered text.
    private GUIStyle styleNonEditable; // Style used for non-editable cells with left-aligned text.
    private GUIStyle styleBoolPopup; // Style used for boolean dropdown cells.

    #endregion

    #region === Unity Methods ===

    /// <summary>
    /// Opens the Language Table Editor Window.
    /// </summary>
    [MenuItem("Window/Language/Language Table Editor Window", false, 2032)]
    public static void OpenWindow()
    {
        var window = GetWindow<LanguageTableEditorWindow>("Language Table Editor Window");
        window.titleContent = new GUIContent("Language Table Editor Window", FindTextureByName("LanguageTableEditorWindow Icon"));
    }

    /// <summary>
    /// Initializes editor state when the window is enabled.
    /// Sets paths, subscribes to undo/redo callbacks, and prepares initial data.
    /// </summary>
    private void OnEnable()
    {
        // Subscribe to Unity's undo/redo system so we can repaint when changes occur.
        Undo.undoRedoPerformed += OnUndoRedo;

        // Get the base path for language-related assets.
        assetsPath = GetLanguageAssetsPath();

        // Build paths for predefined TSV files.
        canvasDataPath = $"{assetsPath}/CanvasData.tsv";
        languageDataPath = $"{assetsPath}/LanguageData.tsv";
        metaDataPath = $"{assetsPath}/MetaData.tsv";
    }

    /// <summary>
    /// Cleans up editor state when the window is destroyed.
    /// Saves temporary data if present and unsubscribes from undo/redo events.
    /// </summary>
    private void OnDestroy()
    {
        // If we have loaded table data, save it temporarily to avoid losing work.
        if (tableData != null && tableData.Length > 0) SaveTableFile("UserSettings/TempTranslationTSV.tsv", tableData);
        Undo.undoRedoPerformed -= OnUndoRedo; // Unsubscribe from undo/redo events to avoid memory leaks or dangling references.
    }

    /// <summary>
    /// Repaints the editor window when undo or redo actions are performed.
    /// </summary>
    private void OnUndoRedo() => Repaint();

    /// <summary>
    /// Called every frame to render the GUI of the editor.
    /// Draws scroll view, buttons, visibility toggles, and the language table.
    /// </summary>
    private void OnGUI()
    {
        InitializeStyles(); // Initialize GUI styles if they haven't been set yet.

        // Begin the main scroll view of the editor window.
        mainScroll = GUILayout.BeginScrollView(mainScroll);

        GUILayout.Label("Language Tools Table", EditorStyles.boldLabel);

        // If no table data is loaded, render buttons to open TSV files.
        if (tableData == null || tableData.Length == 0) RenderOpenFileButtons();

        GUILayout.Space(10);

        // If table data is loaded, render save buttons, visibility toggles, and the table.
        if (tableData != null && tableData.Length > 0)
        {
            RenderSaveButtons(); // Handles saving current table.

            GUILayout.Space(10); // Small spacing for clarity.

            RenderVisibilityToggles(); // Handles row/column visibility and selection.
            RenderTable(); // Renders the actual table with rows, columns, and cells.
        }

        // End the main scroll view to finalize GUI layout.
        GUILayout.EndScrollView();
    }

    #endregion

    #region === Styles Initialization ===

    /// <summary>
    /// Initializes all GUI styles used in the editor window.
    /// Sets text color, font style, alignment, and other properties for cells and headers.
    /// Prevents reinitialization if styles are already set.
    /// </summary>
    private void InitializeStyles()
    {
        // If the main cell style is already initialized, skip to prevent redundant operations.
        if (styleCellText != null) return;

        // Editable text cells style setup.
        styleCellText = new GUIStyle(EditorStyles.textField)
        {
            wordWrap = false // Prevent automatic wrapping of text.
        };

        // Set all text colors for different states to the editor background color.
        styleCellText.normal.textColor = COLOR_EDITOR_BACKGROUND;
        styleCellText.hover.textColor = COLOR_EDITOR_BACKGROUND;
        styleCellText.focused.textColor = COLOR_EDITOR_BACKGROUND;
        styleCellText.active.textColor = COLOR_EDITOR_BACKGROUND;

        // Column and row header style.
        styleHeader = new GUIStyle()
        {
            alignment = TextAnchor.MiddleCenter, // Center the text.
            fontStyle = FontStyle.Bold // Bold font for emphasis.
        };

        styleHeader.normal.textColor = COLOR_EDITOR_BACKGROUND;
        styleHeader.hover.textColor = COLOR_EDITOR_BACKGROUND;
        styleHeader.focused.textColor = COLOR_EDITOR_BACKGROUND;
        styleHeader.active.textColor = COLOR_EDITOR_BACKGROUND;

        // Non-editable cell with centered text.
        styleNonEditableCenter = new GUIStyle(EditorStyles.label)
        {
            alignment = TextAnchor.MiddleCenter,
            fontStyle = FontStyle.Bold
        };

        styleNonEditableCenter.normal.textColor = COLOR_EDITOR_BACKGROUND;
        styleNonEditableCenter.hover.textColor = COLOR_EDITOR_BACKGROUND;
        styleNonEditableCenter.focused.textColor = COLOR_EDITOR_BACKGROUND;
        styleNonEditableCenter.active.textColor = COLOR_EDITOR_BACKGROUND;

        // Non-editable cell with left-aligned text.
        styleNonEditable = new GUIStyle(EditorStyles.label)
        {
            fontStyle = FontStyle.Bold
        };

        styleNonEditable.normal.textColor = COLOR_EDITOR_BACKGROUND;
        styleNonEditable.hover.textColor = COLOR_EDITOR_BACKGROUND;
        styleNonEditable.focused.textColor = COLOR_EDITOR_BACKGROUND;
        styleNonEditable.active.textColor = COLOR_EDITOR_BACKGROUND;

        // Boolean popup style setup.
        styleBoolPopup = new GUIStyle(EditorStyles.popup)
        {
            alignment = TextAnchor.MiddleCenter, // Center text in the popup.
            fontStyle = FontStyle.Bold
        };

        styleBoolPopup.normal.textColor = COLOR_EDITOR_BACKGROUND;
        styleBoolPopup.hover.textColor = COLOR_EDITOR_BACKGROUND;
        styleBoolPopup.active.textColor = COLOR_EDITOR_BACKGROUND;
        styleBoolPopup.focused.textColor = COLOR_EDITOR_BACKGROUND;
    }

    #endregion

    #region === File Buttons ===

    /// <summary>
    /// Renders buttons to open TSV files for loading.
    /// Includes Custom File, LanguageData, MetaData, and CanvasData options.
    /// </summary>
    private void RenderOpenFileButtons()
    {
        GUILayout.BeginHorizontal();

        // Button to open a custom TSV file from the computer.
        DrawOpenButton(new GUIContent("Open Custom File", "Opens a custom TSV file from your computer."), () =>
        {
            string path = EditorUtility.OpenFilePanel("Select TSV File", assetsPath, "tsv"); // Open file panel.
            if (!string.IsNullOrEmpty(path) && File.Exists(path))
            {
                LoadTableFrom(path, ChargingMode.CustomFile); // Load selected file.
            }
        });

        GUILayout.Space(10);

        // Button to open LanguageData.tsv
        DrawOpenButton(new GUIContent("Open LanguageData.tsv", "Opens the default LanguageData.tsv file."), () =>
        {
            if (File.Exists(languageDataPath)) LoadTableFrom(languageDataPath, ChargingMode.LanguageData);
        });

        GUILayout.Space(10);

        // Button to open MetaData.tsv
        DrawOpenButton(new GUIContent("Open MetaData.tsv", "Opens the MetaData.tsv file."), () =>
        {
            if (File.Exists(metaDataPath)) LoadTableFrom(metaDataPath, ChargingMode.MetaData);
        });

        GUILayout.Space(10);

        // Button to open CanvasData.tsv
        DrawOpenButton(new GUIContent("Open CanvasData.tsv", "Opens the CanvasData.tsv file."), () =>
        {
            if (File.Exists(canvasDataPath)) LoadTableFrom(canvasDataPath, ChargingMode.CanvasData);
        });

        GUILayout.EndHorizontal();
    }

    /// <summary>
    /// Draws an individual button for opening a TSV file.
    /// Executes the provided callback when clicked.
    /// </summary>
    /// <param name="label">GUI content for the button.</param>
    /// <param name="onClick">Action to execute on button click.</param>
    private void DrawOpenButton(GUIContent label, Action onClick)
    {
        // Draw the button with fixed width and height.
        if (GUILayout.Button(label, GUILayout.Width(200), GUILayout.Height(30)))
        {
            GUI.FocusControl(null); // Remove focus from any active control to prevent keyboard input issues.
            onClick(); // Execute the provided action when the button is clicked.
        }
    }

    /// <summary>
    /// Renders buttons for saving the currently loaded table.
    /// Supports both custom file paths and predefined TSV files.
    /// Displays unsaved changes indicator using color and asterisk.
    /// </summary>
    private void RenderSaveButtons()
    {
        GUILayout.BeginHorizontal();

        // Change button background color if there are unsaved changes.
        GUI.backgroundColor = isDirty ? COLOR_INFO : COLOR_BACKGROUND;
        string dirtyMark = isDirty ? "*" : ""; // Add asterisk to indicate unsaved changes.

        // === Custom File Save ===
        if (currentChargingMode == ChargingMode.CustomFile)
        {
            if (GUILayout.Button(new GUIContent($"Save Custom File{dirtyMark}", "Saves the current table to a custom TSV file."), GUILayout.Width(200), GUILayout.Height(30)))
            {
                GUI.FocusControl(null);
                string savePath = EditorUtility.SaveFilePanel("Save TSV File", Application.dataPath, "Table", "tsv"); // Prompt save dialog.

                if (!string.IsNullOrEmpty(savePath))
                {
                    if (AllowOverwrite(savePath)) SaveTableToFile(savePath); // Save only if overwriting is allowed.
                }
            }
        }
        // === LanguageData Save ===
        else if (currentChargingMode == ChargingMode.LanguageData)
        {
            DrawFixedSave(languageDataPath, new GUIContent($"Save LanguageData.tsv{dirtyMark}", "Saves changes to LanguageData.tsv"));
        }
        // === MetaData Save ===
        else if (currentChargingMode == ChargingMode.MetaData)
        {
            DrawFixedSave(metaDataPath, new GUIContent($"Save MetaData.tsv{dirtyMark}", "Saves changes to MetaData.tsv"));
        }
        // === CanvasData Save ===
        else if (currentChargingMode == ChargingMode.CanvasData)
        {
            DrawFixedSave(canvasDataPath, new GUIContent($"Save CanvasData.tsv{dirtyMark}", "Saves changes to CanvasData.tsv"));
        }

        GUI.backgroundColor = COLOR_BACKGROUND; // Reset background color to default.

        GUILayout.Space(10);

        DrawResetButton(); // Draw the reset editor state button.

        GUILayout.EndHorizontal();
    }

    /// <summary>
    /// Checks if a file can be overwritten.
    /// Prompts the user for confirmation if the file already exists.
    /// </summary>
    /// <param name="path">File path to check.</param>
    /// <returns>True if overwriting is allowed, false otherwise.</returns>
    private bool AllowOverwrite(string path)
    {
        // If the file doesn't exist, overwriting is trivially allowed.
        if (!File.Exists(path)) return true;

        // Show a dialog to confirm overwrite if the file exists.
        return EditorUtility.DisplayDialog("Confirm Overwrite", "The selected file already exists.\nDo you want to overwrite it?", "Yes", "No");
    }

    /// <summary>
    /// Draws a save button for predefined TSV file paths.
    /// Handles overwrite confirmation and updates dirty state.
    /// </summary>
    /// <param name="path">Predefined file path to save.</param>
    /// <param name="label">GUI content for the button.</param>
    private void DrawFixedSave(string path, GUIContent label)
    {
        // Draw the button with fixed width and height.
        if (GUILayout.Button(label, GUILayout.Width(200), GUILayout.Height(30)))
        {
            GUI.FocusControl(null); // Remove focus from active controls.

            // Check if user allows overwriting the file.
            if (AllowOverwrite(path))
            {
                SaveTableToFile(path); // Save the table to the predefined path.
                isDirty = false; // Reset the dirty flag since changes are now saved.
            }
        }
    }

    /// <summary>
    /// Draws a reset button to clear the editor state.
    /// Prompts for confirmation if there are unsaved changes.
    /// </summary>
    private void DrawResetButton()
    {
        // Draw the button with fixed width and height.
        if (GUILayout.Button(new GUIContent("Reset Data", "Resets all loaded table data, selections, and editor state to default. Discards unsaved changes if confirmed."), GUILayout.Width(200), GUILayout.Height(30)))
        {
            GUI.FocusControl(null); // Remove focus from any active control.
            bool allow = true; // Default to allow reset.

            // If there are unsaved changes, prompt the user for confirmation.
            if (isDirty) allow = EditorUtility.DisplayDialog("Confirm Reset", "Discard unsaved changes?", "Yes", "No");

            // If allowed, reset the editor state.
            if (allow) ResetEditorState();
        }
    }

    /// <summary>
    /// Resets the editor state to default values.
    /// Clears loaded tables, selections, scrolling, and interaction states.
    /// </summary>
    private void ResetEditorState()
    {
        // Record the undo operation for Unity's Undo system.
        Undo.RecordObject(this, "Reset Data");

        // Reset all core data and interaction states to defaults.
        currentFilePath = "";
        selectedRows.Clear();
        selectedColumns.Clear();
        showOnlySelected = false;
        currentChargingMode = ChargingMode.CustomFile;
        tableData = null;
        rowHeights = null;
        columnWidths = null;
        isResizingRow = false;
        isResizingColumn = false;
        initialResizePos = Vector2.zero;
        originalSize = 0;
        resizeIndex = 0;
        mainScroll = Vector2.zero;
        tableScroll = Vector2.zero;
        isDirty = false;
        lastScroll = Vector2.zero;
    }

    #endregion

    #region === Table Visibility ===

    /// <summary>
    /// Renders visibility controls for rows and columns.
    /// Allows toggling "show only selected" and editing selection indices.
    /// Includes buttons to select all rows or columns.
    /// </summary>
    private void RenderVisibilityToggles()
    {
        GUILayout.Label("Visibility Controls", EditorStyles.boldLabel);

        GUILayout.BeginHorizontal();

        // Toggle to show only selected rows and columns.
        bool showOnly = EditorGUILayout.ToggleLeft(new GUIContent("Show only selected rows/columns", "Toggle to display only the currently selected rows and columns."), showOnlySelected, GUILayout.Width(220));
        if (showOnly != showOnlySelected)
        {
            Undo.RecordObject(this, "Edit show Only");
            showOnlySelected = showOnly;
            EditorUtility.SetDirty(this); // Mark editor as changed.
        }

        GUILayout.Space(10);

        // Input field for selected row indices.
        GUILayout.Label(new GUIContent("Selected Rows (indices)", "Comma-separated list of selected row indices."), GUILayout.Width(140));
        string rowsInput = string.Join(",", selectedRows);
        string newRowsInput = EditorGUILayout.TextField(rowsInput, GUILayout.Width(200));
        if (newRowsInput != rowsInput)
        {
            Undo.RecordObject(this, "Edit Selected Rows");
            selectedRows.Clear();
            foreach (var s in newRowsInput.Split(',', StringSplitOptions.RemoveEmptyEntries))
            {
                if (int.TryParse(s.Trim(), out int row)) selectedRows.Add(row);
            }
            EditorUtility.SetDirty(this);
        }

        GUILayout.Space(5);

        // Button to select all rows.
        if (GUILayout.Button(new GUIContent("=", "Select all rows."), GUILayout.Width(20)))
        {
            GUI.FocusControl(null);
            Undo.RecordObject(this, "Select All Rows");
            selectedRows.Clear();
            for (int i = 0; i < tableData.Length; i++) selectedRows.Add(i);
            EditorUtility.SetDirty(this);
        }

        GUILayout.Space(10);

        // Input field for selected column indices.
        GUILayout.Label(new GUIContent("Selected Columns (indices)", "Comma-separated list of selected column indices."), GUILayout.Width(160));
        string colsInput = string.Join(",", selectedColumns);
        string newColsInput = EditorGUILayout.TextField(colsInput, GUILayout.Width(200));
        if (newColsInput != colsInput)
        {
            Undo.RecordObject(this, "Edit Selected Columns");
            selectedColumns.Clear();
            foreach (var s in newColsInput.Split(',', StringSplitOptions.RemoveEmptyEntries))
            {
                if (int.TryParse(s.Trim(), out int col)) selectedColumns.Add(col);
            }
            EditorUtility.SetDirty(this);
        }

        GUILayout.Space(5);

        // Button to select all columns.
        if (GUILayout.Button(new GUIContent("=", "Select all columns."), GUILayout.Width(20)))
        {
            GUI.FocusControl(null);
            Undo.RecordObject(this, "Select All Columns");
            selectedColumns.Clear();
            for (int j = 0; j < tableData[0].horizontalTable.Length; j++) selectedColumns.Add(j);
            EditorUtility.SetDirty(this);
        }

        GUILayout.EndHorizontal();
    }

    #endregion

    #region === Table Loading & Saving ===

    /// <summary>
    /// Loads a TSV table from the given file path and charging mode.
    /// Initializes row heights and column widths after loading.
    /// </summary>
    /// <param name="path">Path to the TSV file.</param>
    /// <param name="mode">Mode indicating which type of file is being loaded.</param>
    private void LoadTableFrom(string path, ChargingMode mode)
    {
        // Verify if file exists.
        if (!File.Exists(path))
        {
            EditorUtility.DisplayDialog("Error", "File not found!", "OK");
            return;
        }

        // Record the operation for undo functionality.
        Undo.RecordObject(this, "Load Table");

        currentChargingMode = mode;
        currentFilePath = path;

        // Load table data from the TSV file.
        LoadTableFile(path, ref tableData);

        // Initialize default row heights and column widths.
        int rows = tableData.Length;
        int cols = tableData[0].horizontalTable.Length;

        rowHeights = Enumerable.Repeat(30f, rows).ToArray();
        columnWidths = Enumerable.Repeat(100f, cols).ToArray();
    }

    /// <summary>
    /// Saves the currently loaded table to the specified path.
    /// Updates dirty state and shows a confirmation dialog.
    /// </summary>
    /// <param name="path">Path to save the TSV file.</param>
    private void SaveTableToFile(string path)
    {
        SaveTableFile(path, tableData); // Save the current table data to the specified TSV file.
        isDirty = false; // Reset the dirty flag after saving.

        // Notify the user of successful save operation.
        EditorUtility.DisplayDialog("Success", "Table saved successfully!", "OK");
    }

    #endregion

    #region === Table Rendering ===

    /// <summary>
    /// Renders the entire table inside a scroll view.
    /// Handles row/column headers, cells, and resizing.
    /// </summary>
    private void RenderTable()
    {
        if (tableData == null || tableData.Length == 0) return;

        // Define margins for the scrollable table area.
        Vector2 margin = new(30, 140);
        float totalWidth = columnWidths.Sum() + MIN_COLUMN_WIDTH;
        float totalHeight = rowHeights.Sum() + MIN_ROW_HEIGHT;

        Rect tableRect = new(0, 0, totalWidth, totalHeight);
        Rect viewRect = new(margin.x, margin.y, position.width - margin.x - 10, position.height - margin.y - 10);

        EditorGUI.DrawRect(viewRect, COLOR_EDITOR_BACKGROUND); // Draw background for the table view.
        tableScroll = GUI.BeginScrollView(viewRect, tableScroll, tableRect); // Begin scroll view for table.

        // Clear focus when scrolling to prevent editing conflicts.
        if (tableScroll != lastScroll) GUI.FocusControl(null);
        lastScroll = tableScroll;

        var e = Event.current;
        Rect visible = new(tableScroll.x, tableScroll.y, viewRect.width, viewRect.height);

        // Render headers and table content.
        RenderColumnHeaders(visible, e);
        RenderRowsAndCells(visible, e, tableRect);

        HandleResize(e); // Handle resizing of rows and columns.
        GUI.EndScrollView();
    }

    /// <summary>
    /// Renders column headers inside the table.
    /// Handles visibility, selection highlighting, and column resizing.
    /// </summary>
    /// <param name="visible">The visible portion of the table scroll view.</param>
    /// <param name="e">Current Event (mouse, keyboard) for handling interactions.</param>
    private void RenderColumnHeaders(Rect visible, Event e)
    {
        float xOffset = MIN_COLUMN_WIDTH;

        // Render only selected columns if toggle is active.
        if (showOnlySelected)
        {
            List<int> visibleColumns = selectedColumns.OrderBy(c => c).ToList();

            foreach (int j in visibleColumns)
            {
                Rect cellRect = new(xOffset, 0, columnWidths[j], MIN_ROW_HEIGHT);
                if (cellRect.xMax < visible.x || cellRect.x > visible.xMax) { xOffset += columnWidths[j]; continue; }

                Color cellColor = COLOR_LABEL_BACKGROUND;

                // Highlight if currently resizing.
                EditorGUI.DrawRect(cellRect, isResizingColumn && resizeIndex == j ? COLOR_SELECTED : cellColor);
                
                // Display column index as label.
                GUI.Label(cellRect, j.ToString(), styleHeader);

                // Add resize handle for this column.
                HandleColumnResize(cellRect, j, e);
                xOffset += columnWidths[j];
            }
        }
        else
        {
            // Render all columns when not filtering.
            for (int j = 0; j < tableData[0].horizontalTable.Length; j++)
            {
                Rect cellRect = new(xOffset, 0, columnWidths[j], MIN_ROW_HEIGHT);
                if (cellRect.xMax < visible.x || cellRect.x > visible.xMax) { xOffset += columnWidths[j]; continue; }

                Color cellColor = COLOR_LABEL_BACKGROUND;
                EditorGUI.DrawRect(cellRect, isResizingColumn && resizeIndex == j ? COLOR_SELECTED : cellColor);
                GUI.Label(cellRect, j.ToString(), styleHeader);

                HandleColumnResize(cellRect, j, e);
                xOffset += columnWidths[j];
            }
        }
    }

    /// <summary>
    /// Renders all cells for a given row.
    /// Handles visibility, selection filtering, cell rendering, and column resize handles.
    /// </summary>
    /// <param name="rowIndex">Index of the row to render.</param>
    /// <param name="yOffset">Vertical offset in pixels for the row's top position.</param>
    /// <param name="visible">The visible portion of the table scroll view.</param>
    /// <param name="e">Current Event (mouse, keyboard) for handling interactions.</param>
    private void RenderRowCells(int rowIndex, float yOffset, Rect visible, Event e)
    {
        float xOffset = MIN_COLUMN_WIDTH;

        if (showOnlySelected)
        {
            // Render only selected columns when toggle is active.
            List<int> visibleColumns = selectedColumns.OrderBy(c => c).ToList();

            foreach (int j in visibleColumns)
            {
                Rect cellRect = new(xOffset, yOffset, columnWidths[j], rowHeights[rowIndex]);
                if (!IsRectVisible(cellRect, visible)) continue;

                // Render the actual cell content.
                RenderCell(cellRect, rowIndex, j);

                // Draw resize handle for the column if necessary.
                RenderColumnResizeHandle(cellRect, j, e, visible);

                xOffset += columnWidths[j];
            }
        }
        else
        {
            // Render all columns when not filtering.
            for (int j = 0; j < tableData[rowIndex].horizontalTable.Length; j++)
            {
                Rect cellRect = new(xOffset, yOffset, columnWidths[j], rowHeights[rowIndex]);
                if (!IsRectVisible(cellRect, visible)) { xOffset += columnWidths[j]; continue; }

                RenderCell(cellRect, rowIndex, j);
                RenderColumnResizeHandle(cellRect, j, e, visible);

                xOffset += columnWidths[j];
            }
        }
    }

    /// <summary>
    /// Renders the row label in the first column.
    /// Handles selection highlighting and visibility.
    /// </summary>
    /// <param name="rowIndex">Index of the row to render.</param>
    /// <param name="yOffset">Vertical offset for the row.</param>
    /// <param name="visible">The visible portion of the table scroll view.</param>
    private void RenderRowLabel(int rowIndex, float yOffset, Rect visible)
    {
        Rect rowLabelRect = new(0, yOffset, MIN_COLUMN_WIDTH, rowHeights[rowIndex]);
        if (rowLabelRect.yMax < visible.y || rowLabelRect.y > visible.yMax) return;

        Color rowColor = COLOR_LABEL_BACKGROUND;

        // Highlight if currently resizing this row.
        EditorGUI.DrawRect(rowLabelRect, isResizingRow && resizeIndex == rowIndex ? COLOR_SELECTED : rowColor);
        GUI.Label(rowLabelRect, rowIndex.ToString(), styleHeader); // Display the row index as label.
    }

    /// <summary>
    /// Renders all rows and their cells.
    /// Handles selection filtering, row labels, row resize handles, and cell rendering.
    /// </summary>
    /// <param name="visible">The visible portion of the table scroll view.</param>
    /// <param name="e">Current Event (mouse, keyboard) for interactions.</param>
    /// <param name="tableRect">Rectangle representing the full table area.</param>
    private void RenderRowsAndCells(Rect visible, Event e, Rect tableRect)
    {
        float yOffset = MIN_ROW_HEIGHT;

        if (showOnlySelected)
        {
            // Render only selected rows when toggle is active.
            List<int> visibleRows = selectedRows.OrderBy(r => r).ToList();

            foreach (int i in visibleRows)
            {
                RenderRowLabel(i, yOffset, visible);
                RenderRowCells(i, yOffset, visible, e);
                RenderRowResizeHandle(i, yOffset, tableRect, e);

                yOffset += rowHeights[i];
            }
        }
        else
        {
            // Render all rows when not filtering.
            for (int i = 0; i < tableData.Length; i++)
            {
                if (showOnlySelected && !selectedRows.Contains(i))
                {
                    yOffset += rowHeights[i];
                    continue;
                }

                RenderRowLabel(i, yOffset, visible);
                RenderRowCells(i, yOffset, visible, e);
                RenderRowResizeHandle(i, yOffset, tableRect, e);
                yOffset += rowHeights[i];
            }
        }
    }

    /// <summary>
    /// Checks if a rectangle is visible inside the scroll view.
    /// </summary>
    /// <param name="rect">Rectangle to check.</param>
    /// <param name="visible">Visible scroll area rectangle.</param>
    /// <returns>True if any portion of rect is inside visible, false otherwise.</returns>
    private bool IsRectVisible(Rect rect, Rect visible) => rect.xMax >= visible.x && rect.x <= visible.xMax && rect.yMax >= visible.y && rect.y <= visible.yMax;

    /// <summary>
    /// Renders a single table cell.
    /// Determines whether it is editable, boolean, or standard text, and applies coloring.
    /// </summary>
    /// <param name="cellRect">Rectangle for the cell position and size.</param>
    /// <param name="rowIndex">Row index of the cell.</param>
    /// <param name="colIndex">Column index of the cell.</param>
    private void RenderCell(Rect cellRect, int rowIndex, int colIndex)
    {
        string value = tableData[rowIndex].horizontalTable[colIndex];
        bool editable = true;

        // Determine the cell's background color and if it is editable.
        Color bg = GetCellColor(rowIndex, colIndex, value, ref editable);

        EditorGUI.DrawRect(cellRect, bg);
        GUIStyle dynamicStyle = new(styleCellText);

        Rect innerRect = new(cellRect.x + 5, cellRect.y + 5, cellRect.width - 10, cellRect.height - 10);

        if (rowIndex == 3 && colIndex >= 2)
        {
            // Render boolean cells for specific row and column ranges.
            RenderBoolCell(innerRect, rowIndex, colIndex, value, bg);
        }
        else if (ShouldCenterText(bg))
        {
            // Render centered non-editable labels for header/info cells.
            GUI.Label(innerRect, value, (rowIndex == 0 && colIndex == 0) ? styleNonEditable : styleNonEditableCenter);
        }
        else if (editable)
        {
            // Render editable text cells with alignment and background support.
            RenderEditableTextCell(innerRect, rowIndex, colIndex, value, dynamicStyle, rowIndex >= 4 && colIndex == 0 ? TextAnchor.MiddleRight : TextAnchor.MiddleLeft, bg);
        }
    }

    /// <summary>
    /// Renders a boolean cell as a popup.
    /// Allows toggling between True/False values with visual coloring.
    /// Updates the table data if the value changes.
    /// </summary>
    /// <param name="innerRect">Rectangle for the inner cell rendering.</param>
    /// <param name="rowIndex">Row index of the cell.</param>
    /// <param name="colIndex">Column index of the cell.</param>
    /// <param name="value">Current string value ("True" or "False").</param>
    /// <param name="bg">Background color of the cell.</param>
    private void RenderBoolCell(Rect innerRect, int rowIndex, int colIndex, string value, Color bg)
    {
        int boolIndex = value.Equals("True", StringComparison.OrdinalIgnoreCase) ? 1 : 0;
        GUI.color = bg * 6; // Apply background color multiplier for visual effect.

        // Render popup for boolean selection.
        int newIndex = EditorGUI.Popup(innerRect, boolIndex, new[] { "False", "True" }, styleBoolPopup);

        GUI.color = COLOR_BACKGROUND;
        string newValue = newIndex == 1 ? "True" : "False";

        if (newValue != value)
        {
            Undo.RecordObject(this, "Toggle Bool");
            EditorUtility.SetDirty(this);

            // Update table data and mark as dirty for saving.
            tableData[rowIndex].horizontalTable[colIndex] = newValue;
            isDirty = true;
        }
    }

    /// <summary>
    /// Renders an editable text cell.
    /// Allows in-place editing of the text with alignment and background color support.
    /// Updates the table data if the value changes.
    /// </summary>
    /// <param name="innerRect">Rectangle for the inner cell rendering.</param>
    /// <param name="rowIndex">Row index of the cell.</param>
    /// <param name="colIndex">Column index of the cell.</param>
    /// <param name="value">Current string value of the cell.</param>
    /// <param name="style">GUIStyle to use for rendering the text.</param>
    /// <param name="alignment">Text alignment inside the cell.</param>
    /// <param name="bg">Background color of the cell.</param>
    private void RenderEditableTextCell(Rect innerRect, int rowIndex, int colIndex, string value, GUIStyle style, TextAnchor alignment, Color bg)
    {
        style.alignment = alignment;

        // Apply the background color for the text field.
        GUI.color = bg * 6;
        string newValue = EditorGUI.TextField(innerRect, value, style);
        GUI.color = COLOR_BACKGROUND;

        // Update the table data if the value has changed.
        if (newValue != value)
        {
            Undo.RecordObject(this, "Edit Cell");
            EditorUtility.SetDirty(this);
            tableData[rowIndex].horizontalTable[colIndex] = newValue;
            isDirty = true;
        }
    }

    /// <summary>
    /// Gets the cell background color and sets whether it is editable.
    /// Determines color based on row and column indexes and cell value.
    /// </summary>
    /// <param name="rowIndex">Row index of the cell.</param>
    /// <param name="colIndex">Column index of the cell.</param>
    /// <param name="value">Current string value of the cell.</param>
    /// <param name="editable">Output parameter indicating if the cell is editable.</param>
    /// <returns>Color to render as the cell background.</returns>
    private Color GetCellColor(int rowIndex, int colIndex, string value, ref bool editable)
    {
        editable = true; // By default, assume the cell is editable.

        // Handle special rows.
        switch (rowIndex)
        {
            // Row 0 is typically the header row.
            case 0:
                if (colIndex == 0) return COLOR_INFO; // Top-left cell, often a label, info-colored.
                if (colIndex == 1)
                {
                    editable = false; // This header cell is not editable.
                    return COLOR_EDITOR_BACKGROUND; // Dark background for non-editable header.
                }
                return COLOR_DATA; // Other header cells are standard data cells.

            // Rows 1 and 2 might be meta rows with fixed columns.
            case 1:
            case 2:
                if (colIndex <= 1)
                {
                    editable = false; // First two columns are read-only.
                    return COLOR_EDITOR_BACKGROUND; // Non-editable background.
                }
                return COLOR_DATA;// Remaining columns are standard data cells.

            // Row 3 may contain context, IDs, and boolean flags.
            case 3:
                switch (colIndex)
                {
                    case 0: return COLOR_CONTEXT; // First column is context info.
                    case 1: return COLOR_ID; // Second column is ID info.
                    default:
                        // Boolean cells: color depends on value.
                        if (string.Equals(value, "True", StringComparison.OrdinalIgnoreCase)) return COLOR_TRUE; // Green for True.
                        if (string.Equals(value, "False", StringComparison.OrdinalIgnoreCase)) return COLOR_LINE_HIGHLIGHT; // Red for False.
                        return COLOR_DEFAULT; // Default for other values.
                }

            // Remaining rows.
            default:
                return colIndex switch
                {
                    0 => COLOR_LIGHT, // First column is light-themed.
                    1 => COLOR_NUMBER, // Second column is numeric-themed.
                    _ => COLOR_DEFAULT, // Other cells use default color.
                };
        }
    }

    /// <summary>
    /// Determines if text should be centered based on background color.
    /// Used to adjust alignment for headers, labels, and special cells.
    /// </summary>
    /// <param name="bg">Background color of the cell.</param>
    /// <returns>True if text should be centered; false if left-aligned.</returns>
    private bool ShouldCenterText(Color bg) => bg == COLOR_INFO || bg == COLOR_DATA || bg == COLOR_CONTEXT || bg == COLOR_ID || bg == COLOR_NUMBER;

    /// <summary>
    /// Handles column resize logic on mouse events.
    /// Draws a resize handle and starts resizing when clicked.
    /// </summary>
    /// <param name="cellRect">Rectangle of the column cell.</param>
    /// <param name="colIndex">Index of the column being rendered.</param>
    /// <param name="e">Current Event for mouse interaction.</param>
    private void HandleColumnResize(Rect cellRect, int colIndex, Event e)
    {
        // Create a thin rectangle at the right edge of the column to act as a resize handle.
        Rect colResizeHandle = new(cellRect.xMax - 5, cellRect.y, 5, cellRect.height);
        EditorGUI.DrawRect(colResizeHandle, isResizingColumn && resizeIndex == colIndex ? COLOR_SELECTED : COLOR_LINE_DEFAULT);

        // Begin resizing if mouse is pressed on the handle.
        if (e.type == EventType.MouseDown && colResizeHandle.Contains(e.mousePosition))
        {
            isResizingColumn = true;
            initialResizePos = e.mousePosition;
            originalSize = columnWidths[colIndex];
            resizeIndex = colIndex;
            e.Use(); // Consume the event so it doesn't propagate further.
        }
    }

    /// <summary>
    /// Handles row resize handle rendering and mouse interaction.
    /// Allows vertical resizing of rows with a draggable handle.
    /// </summary>
    /// <param name="rowIndex">Row index for which the handle is rendered.</param>
    /// <param name="yOffset">Vertical offset of the row.</param>
    /// <param name="tableRect">Rectangle of the full table area.</param>
    /// <param name="e">Current Event for mouse interaction.</param>
    private void RenderRowResizeHandle(int rowIndex, float yOffset, Rect tableRect, Event e)
    {
        // Thin rectangle at the bottom of the row to act as resize handle.
        Rect rowResizeHandle = new(0, yOffset + rowHeights[rowIndex] - 5, tableRect.width, 5);

        // Skip rendering if not visible in scroll view.
        if (!IsRectVisible(rowResizeHandle, new Rect(tableScroll.x, tableScroll.y, position.width, position.height))) return;

        EditorGUI.DrawRect(rowResizeHandle, isResizingRow && resizeIndex == rowIndex ? COLOR_SELECTED : COLOR_LINE_DEFAULT);

        // Begin resizing if mouse is pressed on the handle.
        if (e.type == EventType.MouseDown && rowResizeHandle.Contains(e.mousePosition))
        {
            isResizingRow = true;
            initialResizePos = e.mousePosition;
            originalSize = rowHeights[rowIndex];
            resizeIndex = rowIndex;
            e.Use();
        }
    }

    /// <summary>
    /// Handles column resize handle rendering and mouse interaction.
    /// Draws the handle only if visible in the scroll view and allows resizing on drag.
    /// </summary>
    /// <param name="cellRect">Rectangle of the column cell.</param>
    /// <param name="colIndex">Index of the column being rendered.</param>
    /// <param name="e">Current Event for mouse interaction.</param>
    /// <param name="visible">Currently visible portion of the table scroll view.</param>
    private void RenderColumnResizeHandle(Rect cellRect, int colIndex, Event e, Rect visible)
    {
        // Thin rectangle at the right edge of the column for resizing.
        Rect colResizeHandle = new(cellRect.xMax - 5, cellRect.y, 5, cellRect.height);

        // Skip if the handle is outside the visible scroll area.
        bool isColVisible = colResizeHandle.yMax >= visible.y && colResizeHandle.y <= visible.yMax && colResizeHandle.xMax >= visible.x && colResizeHandle.x <= visible.xMax;
        if (!isColVisible) return;

        EditorGUI.DrawRect(colResizeHandle, isResizingColumn && resizeIndex == colIndex ? COLOR_SELECTED : COLOR_LINE_DEFAULT);

        // Begin resizing if mouse is pressed on the handle.
        if (e.type == EventType.MouseDown && colResizeHandle.Contains(e.mousePosition))
        {
            isResizingColumn = true;
            initialResizePos = e.mousePosition;
            originalSize = columnWidths[colIndex];
            resizeIndex = colIndex;
            e.Use();
        }
    }

    /// <summary>
    /// Handles all resizing logic for both rows and columns during mouse drag events.
    /// Updates the sizes in real-time and finalizes on mouse release.
    /// </summary>
    /// <param name="e">Current Event for mouse drag or release interaction.</param>
    private void HandleResize(Event e)
    {
        // Check if the mouse is being dragged.
        if (e.type == EventType.MouseDrag)
        {
            // Handle column resizing.
            if (isResizingColumn)
            {
                // Calculate how much the mouse has moved horizontally since the drag started.
                Vector2 mouseDelta = e.mousePosition - initialResizePos;

                // Record undo for this change to support Unity's undo system.
                Undo.RecordObject(this, "Resize Column");

                // Update the column width, ensuring it doesn't go below the minimum.
                columnWidths[resizeIndex] = Mathf.Max(originalSize + mouseDelta.x, MIN_COLUMN_WIDTH);

                // Consume the event so it doesn't propagate further.
                e.Use();
            }

            // Handle row resizing.
            if (isResizingRow)
            {
                // Calculate how much the mouse has moved vertically since the drag started.
                Vector2 mouseDelta = e.mousePosition - initialResizePos;

                // Record undo for this change to support Unity's undo system.
                Undo.RecordObject(this, "Resize Row");

                // Update the row height, ensuring it doesn't go below the minimum.
                rowHeights[resizeIndex] = Mathf.Max(originalSize + mouseDelta.y, MIN_ROW_HEIGHT);

                // Consume the event to prevent further propagation.
                e.Use();
            }
        }

        // Reset resizing state when the mouse is released.
        if (e.type == EventType.MouseUp)
        {
            isResizingColumn = false;
            isResizingRow = false;
        }
    }

    #endregion
}
#endif