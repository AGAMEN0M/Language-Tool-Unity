/*
 * ---------------------------------------------------------------------------
 * Description: A Unity editor script for managing TSV (Tab-Separated Values) tables.
 *              Provides functionalities to load, save, modify, and resize rows and columns.
 *              Includes scrollable views, resizing handles, and options for adding/removing rows/columns.
 *              
 * Author: Lucas Gomes Cecchini
 * Pseudonym: AGAMENOM
 * ---------------------------------------------------------------------------
*/

using UnityEngine;
using UnityEditor;
using System.Linq;
using System.IO;
using System;

using static TSVTools.TabTableUtility;

namespace TSVTools
{
    public class TSVEditorWindow : EditorWindow
    {
        #region === Public Variables ===

        public VerticalTable[] tableData; // Array holding the table data for each row.
        public string filePath = ""; // Path of the loaded TSV file.
        public int targetIndex; // Index of the selected row/column for operations.
        public LineDirection removalDirection; // Direction for removing a line: Vertical (row) or Horizontal (column).
        public float[] rowSizes; // Stores the heights of each row.
        public float[] columnSizes; // Stores the widths of each column.

        #endregion

        #region === State Variables ===

        public bool isResizingRow = false; // True if the user is currently resizing a row.
        public bool isResizingColumn = false; // True if the user is currently resizing a column.
        public Vector2 initialResizePosition; // Mouse position at the start of resizing.
        public float originalDimension; // Original width/height before resizing.
        public int resizeTargetIndex; // Index of the row/column being resized.
        public Vector2 mainScrollPosition; // Scroll position for the main window view.
        public Vector2 tableScrollPosition; // Scroll position for the table content.
        public bool isDirty = false; // True if the table has unsaved changes.
        private Vector2 previousScrollPosition; // Tracks previous scroll position.
        private float lastInteractionTime = 0f; // Time of last interaction for double-click detection.
        private bool isValidIndex = true; // True if the targetIndex is within bounds.

        #endregion

        #region === Constants and UI Settings ===

        private const float minColumnWidth = 50f; // Minimum width of a column.
        private const float minRowHeight = 30f; // Minimum height of a row.
        private const float doubleClickThreshold = 0.5f; // Max time interval for detecting double-click in seconds.

        private static Color defaultLineColor = new(0.7f, 0.7f, 0.7f, 1); // Default line color.
        private static Color highlightLineColor = Color.red; // Color used when resizing a line.
        private static Color tableBackgroundColor = Color.gray; // Background color of table cells.
        private static Color editorBackgroundColor = Color.black; // Background color of the editor window.
        private static Color labelBackgroundColor = Color.blue; // Background color of row/column headers.
        private static Color selectedColor = Color.violetRed; // Color for selected row/column.

        private GUIStyle cellTextStyle; // Style for text inside table cells.
        private GUIStyle headerLabelStyle; // Style for row/column headers.
        private Texture2D editorBackgroundTexture; // Texture used for cell backgrounds.

        #endregion

        #region === Menu Item ===

        /// <summary>
        /// Opens the TSV Editor window in Unity's Window menu.
        /// Searches for a custom icon and sets the window title and icon.
        /// </summary>
        [MenuItem("Window/TSV Editor Window", false, 2028)]
        public static void OpenTableEditorWindow()
        {
            var window = GetWindow<TSVEditorWindow>("TSV Editor");
            string fileName = "TSVEditorWindow Icon";
            Texture2D icon = null;

            string[] guids = AssetDatabase.FindAssets($"{fileName} t:Texture2D");
            foreach (string guid in guids)
            {
                string texturePath = AssetDatabase.GUIDToAssetPath(guid);
                string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(texturePath);

                if (fileNameWithoutExtension.Equals(fileName, StringComparison.OrdinalIgnoreCase))
                {
                    icon = AssetDatabase.LoadAssetAtPath<Texture2D>(texturePath);
                }
            }

            window.titleContent = new GUIContent("TSV Editor", icon);
        }

        #endregion

        #region === Unity Callbacks ===

        /// <summary>
        /// Unity callback called when the editor window is enabled.
        /// Subscribes to the Undo/Redo event to repaint the window on changes.
        /// </summary>
        private void OnEnable() => Undo.undoRedoPerformed += OnUndoRedo;

        /// <summary>
        /// Unity callback called when the editor window is destroyed.
        /// Saves temporary table data and unsubscribes from Undo/Redo events.
        /// </summary>
        private void OnDestroy()
        {
            if (tableData != null && tableData.Length > 0) SaveTableFile("UserSettings/TempTSV.tsv", tableData);
            Undo.undoRedoPerformed -= OnUndoRedo;
        }

        /// <summary>
        /// Callback triggered after an Undo or Redo operation.
        /// Repaints the editor window to reflect the changes.
        /// </summary>
        private void OnUndoRedo() => Repaint();

        #endregion

        #region === GUI Rendering ===

        /// <summary>
        /// Draws the editor GUI every frame.
        /// Handles scroll views, file opening, table creation, saving, and row/column controls.
        /// </summary>
        private void OnGUI()
        {
            InitializeStyles();

            mainScrollPosition = GUILayout.BeginScrollView(mainScrollPosition);

            GUILayout.Label("TSV Table", EditorStyles.boldLabel);

            RenderOpenFileButton();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Uploaded File:", EditorStyles.boldLabel, GUILayout.Width(90));
            GUILayout.Label(filePath);
            GUILayout.EndHorizontal();

            if (tableData != null && tableData.Length > 0)
            {
                RenderSaveButton();
                RenderRemoveRowColumnControls();
                RenderTable();
            }
            else
            {
                RenderCreateTableButton();
            }

            GUILayout.EndScrollView();
        }

        /// <summary>
        /// Initializes GUIStyles for table cells and headers.
        /// Creates textures for cell backgrounds.
        /// </summary>
        private void InitializeStyles()
        {
            if (cellTextStyle != null && editorBackgroundTexture != null) return;

            editorBackgroundTexture = CreateTexture(1, 1, tableBackgroundColor);

            cellTextStyle = new GUIStyle(EditorStyles.textField)
            {
                normal = { textColor = Color.white, background = editorBackgroundTexture },
            };

            headerLabelStyle = new GUIStyle()
            {
                normal = { textColor = Color.white },
                alignment = TextAnchor.MiddleCenter,
                fontStyle = FontStyle.Bold
            };
        }

        /// <summary>
        /// Renders a button to open a TSV file.
        /// Loads the table data if a valid file is selected.
        /// </summary>
        private void RenderOpenFileButton()
        {
            if (!GUILayout.Button("Open File")) return;

            string path = EditorUtility.OpenFilePanel("Select TSV File", Application.dataPath, "tsv");
            if (!string.IsNullOrEmpty(path))
            {
                filePath = path;
                LoadTableFromFile(filePath);
            }
        }

        /// <summary>
        /// Renders the save button for the table.
        /// Shows a warning if the table has unsaved changes, and saves the file when clicked.
        /// </summary>
        private void RenderSaveButton()
        {
            string saveButtonText = isDirty ? "Save Table*" : "Save Table";

            // Highlight button if there are unsaved changes.
            GUI.backgroundColor = isDirty ? Color.yellow : Color.white;
            if (GUILayout.Button(saveButtonText))
            {
                string savePath = EditorUtility.SaveFilePanel("Save TSV File", Application.dataPath, "Table", "tsv");
                if (!string.IsNullOrEmpty(savePath)) SaveTableToFile(savePath);
            }
            GUI.backgroundColor = Color.white;
        }

        /// <summary>
        /// Renders controls for removing rows or columns from the table.
        /// Allows the user to select an index and direction, with validation.
        /// </summary>
        private void RenderRemoveRowColumnControls()
        {
            GUILayout.Label("You can move rows or add new ones by creating them twice.");
            GUILayout.Label("Remove Row/Column", EditorStyles.boldLabel);

            int newIndexToRemove = EditorGUILayout.IntField("Number (index)", targetIndex);
            if (newIndexToRemove < 0) newIndexToRemove = 0;

            if (newIndexToRemove != targetIndex)
            {
                Undo.RecordObject(this, "Change Index to Remove");
                targetIndex = newIndexToRemove;
            }

            var options = new string[] { "Horizontal", "Vertical" };
            int newLineDirectionIndex = EditorGUILayout.Popup("Direction", (int)removalDirection, options);
            if (newLineDirectionIndex != (int)removalDirection)
            {
                Undo.RecordObject(this, "Change Line Direction");
                removalDirection = (LineDirection)newLineDirectionIndex;
            }

            int maxIndex = (removalDirection == LineDirection.Vertical ? tableData.Length : tableData[0].horizontalTable.Length) - 1;
            isValidIndex = targetIndex >= 0 && targetIndex <= maxIndex;

            EditorGUI.BeginDisabledGroup(!isValidIndex);
            if (GUILayout.Button("Remove Row/Column"))
            {
                RemoveSelectedLine();
            }
            EditorGUI.EndDisabledGroup();

            if (!isValidIndex)
            {
                EditorGUILayout.HelpBox($"Invalid index. Valid range: 0 - {maxIndex}", MessageType.Warning);
            }
        }

        /// <summary>
        /// Removes the selected row or column from the table.
        /// Updates the row or column size arrays accordingly.
        /// </summary>
        private void RemoveSelectedLine()
        {
            Undo.RecordObject(this, "Remove Row/Column");
            RemoveLine(ref tableData, targetIndex, removalDirection);

            if (removalDirection == LineDirection.Vertical)
            {
                var newRowHeights = rowSizes.ToList();
                newRowHeights.RemoveAt(targetIndex);
                rowSizes = newRowHeights.ToArray();
            }
            else
            {
                var newColWidths = columnSizes.ToList();
                newColWidths.RemoveAt(targetIndex);
                columnSizes = newColWidths.ToArray();
            }

            isDirty = true;
        }

        /// <summary>
        /// Renders a button to create a new default table if no table is loaded.
        /// Initializes table data and row/column sizes.
        /// </summary>
        private void RenderCreateTableButton()
        {
            GUILayout.Label("No tables loaded.");
            if (!GUILayout.Button("Create Table")) return;

            Undo.RecordObject(this, "Create Table");
            tableData = new VerticalTable[2];

            // Initialize two rows with two columns each.
            tableData[0] = new VerticalTable { horizontalTable = new string[] { "", "" } };
            tableData[1] = new VerticalTable { horizontalTable = new string[] { "", "" } };

            rowSizes = new float[tableData.Length];
            columnSizes = new float[tableData[0].horizontalTable.Length];

            for (int i = 0; i < tableData.Length; i++) rowSizes[i] = 30f;
            for (int j = 0; j < tableData[0].horizontalTable.Length; j++) columnSizes[j] = 100f;

            isDirty = true;
        }

        #endregion

        #region === Table Utility Methods ===

        /// <summary>
        /// Creates a Texture2D of a single color for GUI backgrounds.
        /// </summary>
        /// <param name="width">Texture width.</param>
        /// <param name="height">Texture height.</param>
        /// <param name="color">Color to fill.</param>
        /// <returns>A Texture2D filled with the specified color.</returns>
        private Texture2D CreateTexture(int width, int height, Color color)
        {
            Color32[] pixels = new Color32[width * height];
            Color32 color32 = color;
            for (int i = 0; i < pixels.Length; i++) pixels[i] = color32;

            Texture2D texture = new(width, height);
            texture.SetPixels32(pixels);
            texture.Apply();
            return texture;
        }

        /// <summary>
        /// Loads table data from a TSV file and initializes row and column sizes.
        /// </summary>
        /// <param name="path">Path to the TSV file.</param>
        private void LoadTableFromFile(string path)
        {
            if (!File.Exists(path))
            {
                EditorUtility.DisplayDialog("Error", "File not found!", "OK");
                return;
            }

            Undo.RecordObject(this, "Load Table");
            LoadTableFile(path, ref tableData);

            int rows = tableData.Length;
            int cols = tableData[0].horizontalTable.Length;
            rowSizes = new float[rows];
            columnSizes = new float[cols];

            for (int i = 0; i < rows; i++) rowSizes[i] = 30f;
            for (int j = 0; j < cols; j++) columnSizes[j] = 100f;
        }

        /// <summary>
        /// Saves the current table data to a TSV file.
        /// Marks the table as not dirty and shows a success dialog.
        /// </summary>
        /// <param name="path">Path to save the TSV file.</param>
        private void SaveTableToFile(string path)
        {
            SaveTableFile(path, tableData);
            isDirty = false;
            EditorUtility.DisplayDialog("Success", "Table saved successfully!", "OK");
        }

        /// <summary>
        /// Renders the entire TSV table inside the editor window, including:
        /// - Column headers and row labels.
        /// - Table cells with editable content.
        /// - Scrollable view for large tables.
        /// - Row and column resizing handles.
        /// - Double-click functionality to add new rows/columns.
        /// </summary>
        private void RenderTable()
        {
            #region === Calculate Table Dimensions ===

            // Calculate total width and height of the table based on column widths and row heights.
            float totalWidth = 0f;
            float totalHeight = 0f;

            foreach (float width in columnSizes) totalWidth += width;
            foreach (float height in rowSizes) totalHeight += height;

            // Margins for scrollable area: leave space for headers and extra padding.
            Vector2 margin = new(30, isValidIndex ? 190 : 230);
            totalWidth += minColumnWidth; // Include space for row labels.
            totalHeight += minRowHeight;  // Include space for column headers.

            #endregion

            #region === Setup Scroll View ===

            // Define the table's scrollable area and the visible window area.
            Rect tableRect = new(0, 0, totalWidth, totalHeight);
            Rect blackAreaRect = new(margin.x, margin.y, position.width - margin.x - 10, position.height - margin.y - 10);
            EditorGUI.DrawRect(blackAreaRect, editorBackgroundColor); // Draw editor background.

            // Begin scrollable area for table content.
            tableScrollPosition = GUI.BeginScrollView(blackAreaRect, tableScrollPosition, tableRect);
            if (tableScrollPosition != previousScrollPosition) GUI.FocusControl(null); // Lose focus if scrolling.
            previousScrollPosition = tableScrollPosition;

            var e = Event.current; // Current Unity event for input handling.
            Rect visibleRect = new(tableScrollPosition.x, tableScrollPosition.y, blackAreaRect.width, blackAreaRect.height); // Currently visible area.
            
            #endregion

            #region === Render Column Headers ===

            float xOffset = minColumnWidth; // Start after the row labels.
            for (int j = 0; j < tableData[0].horizontalTable.Length; j++)
            {
                Rect cellRect = new(xOffset, 0, columnSizes[j], minRowHeight);
                bool isColumnVisible = cellRect.xMax >= visibleRect.x && cellRect.x <= visibleRect.xMax;
                if (isColumnVisible)
                {
                    // Highlight selected column if in removal mode.
                    Color cellColor = labelBackgroundColor;
                    if (removalDirection == LineDirection.Horizontal && j == targetIndex) cellColor = selectedColor;

                    EditorGUI.DrawRect(cellRect, isResizingColumn && resizeTargetIndex == j ? highlightLineColor : cellColor);
                    GUI.Label(cellRect, j.ToString(), headerLabelStyle);

                    // Draw column resize handle.
                    Rect colResizeHandle = new(cellRect.xMax - 5, cellRect.y, 5, cellRect.height);
                    EditorGUI.DrawRect(colResizeHandle, isResizingColumn && resizeTargetIndex == j ? highlightLineColor : defaultLineColor);

                    // Handle mouse interaction for resizing or adding columns.
                    if (e.type == EventType.MouseDown && colResizeHandle.Contains(e.mousePosition))
                    {
                        if (Time.realtimeSinceStartup - lastInteractionTime < doubleClickThreshold)
                        {
                            // Double-click adds a new column to the right.
                            AddLine(ref tableData, j + 1, LineDirection.Horizontal);
                            var newColWidths = columnSizes.ToList();
                            newColWidths.Insert(j + 1, minColumnWidth);
                            columnSizes = newColWidths.ToArray();
                            isDirty = true;
                        }

                        lastInteractionTime = Time.realtimeSinceStartup;
                        isResizingColumn = true;
                        initialResizePosition = e.mousePosition;
                        originalDimension = columnSizes[j];
                        resizeTargetIndex = j;
                        e.Use();
                    }
                }

                xOffset += columnSizes[j]; // Move to the next column.
            }

            #endregion

            #region === Render Rows and Cells ===

            float yOffset = minRowHeight; // Start after column headers.
            for (int i = 0; i < tableData.Length; i++)
            {
                xOffset = 0f;

                // Draw row label.
                Rect rowLabelRect = new(xOffset, yOffset, minColumnWidth, rowSizes[i]);
                bool isRowVisible = rowLabelRect.yMax >= visibleRect.y && rowLabelRect.y <= visibleRect.yMax;

                if (isRowVisible)
                {
                    Color rowColor = labelBackgroundColor;
                    if (removalDirection == LineDirection.Vertical && i == targetIndex) rowColor = selectedColor;
                    EditorGUI.DrawRect(rowLabelRect, isResizingRow && resizeTargetIndex == i ? highlightLineColor : rowColor);
                    GUI.Label(rowLabelRect, i.ToString(), headerLabelStyle);
                }

                // Draw each cell in the row.
                for (int j = 0; j < tableData[i].horizontalTable.Length; j++)
                {
                    Rect cellRect = new(xOffset + minColumnWidth, yOffset, columnSizes[j], rowSizes[i]);
                    bool isCellVisible = cellRect.yMax >= visibleRect.y && cellRect.y <= visibleRect.yMax && cellRect.xMax >= visibleRect.x && cellRect.x <= visibleRect.xMax;

                    if (isCellVisible)
                    {
                        // Draw cell background and text field.
                        EditorGUI.DrawRect(cellRect, tableBackgroundColor);
                        string cellContent = tableData[i].horizontalTable[j];
                        Rect textFieldRect = new(cellRect.x + 5, cellRect.y + 5, cellRect.width - 10, cellRect.height - 10);

                        string newCellContent = EditorGUI.TextField(textFieldRect, cellContent, cellTextStyle);
                        if (newCellContent != tableData[i].horizontalTable[j])
                        {
                            Undo.RecordObject(this, "Edit Cell Content");
                            tableData[i].horizontalTable[j] = newCellContent;
                            isDirty = true;
                        }
                    }

                    // Draw column resize handles inside the row.
                    Rect colResizeHandle = new(cellRect.xMax - 5, cellRect.y, 5, cellRect.height);
                    bool isColHandleVisible = colResizeHandle.yMax >= visibleRect.y && colResizeHandle.y <= visibleRect.yMax &&
                                              colResizeHandle.xMax >= visibleRect.x && colResizeHandle.x <= visibleRect.xMax;
                    if (isColHandleVisible)
                    {
                        EditorGUI.DrawRect(colResizeHandle, isResizingColumn && resizeTargetIndex == j ? highlightLineColor : defaultLineColor);

                        if (e.type == EventType.MouseDown && colResizeHandle.Contains(e.mousePosition))
                        {
                            if (Time.realtimeSinceStartup - lastInteractionTime < doubleClickThreshold)
                            {
                                // Double-click adds a new column.
                                Undo.RecordObject(this, "Add Line");
                                AddLine(ref tableData, j + 1, LineDirection.Horizontal);
                                var newColWidths = columnSizes.ToList();
                                newColWidths.Insert(j + 1, minColumnWidth);
                                columnSizes = newColWidths.ToArray();
                                isDirty = true;
                            }

                            lastInteractionTime = Time.realtimeSinceStartup;
                            isResizingColumn = true;
                            initialResizePosition = e.mousePosition;
                            originalDimension = columnSizes[j];
                            resizeTargetIndex = j;
                            e.Use();
                            GUI.FocusControl(null);
                        }
                    }

                    xOffset += columnSizes[j];
                }

                // Draw row resize handle.
                Rect rowResizeHandle = new(0, yOffset + rowSizes[i] - 5, tableRect.width, 5);
                bool isRowHandleVisible = rowResizeHandle.yMax >= visibleRect.y && rowResizeHandle.y <= visibleRect.yMax;

                if (isRowHandleVisible)
                {
                    EditorGUI.DrawRect(rowResizeHandle, isResizingRow && resizeTargetIndex == i ? highlightLineColor : defaultLineColor);

                    if (e.type == EventType.MouseDown && rowResizeHandle.Contains(e.mousePosition))
                    {
                        if (Time.realtimeSinceStartup - lastInteractionTime < doubleClickThreshold)
                        {
                            // Double-click adds a new row below.
                            Undo.RecordObject(this, "Add Line");
                            AddLine(ref tableData, i + 1, LineDirection.Vertical);
                            var newRowHeights = rowSizes.ToList();
                            newRowHeights.Insert(i + 1, minRowHeight);
                            rowSizes = newRowHeights.ToArray();
                            isDirty = true;
                        }

                        lastInteractionTime = Time.realtimeSinceStartup;
                        isResizingRow = true;
                        initialResizePosition = e.mousePosition;
                        originalDimension = rowSizes[i];
                        resizeTargetIndex = i;
                        e.Use();
                        GUI.FocusControl(null);
                    }
                }

                yOffset += rowSizes[i]; // Move to next row.
            }

            #endregion

            #region === Handle Resizing ===

            // Process any ongoing row/column resizing.
            HandleResizeInteraction(e);
            GUI.EndScrollView();

            #endregion
        }

        /// <summary>
        /// Handles resizing of columns and rows based on mouse drag events.
        /// Ensures sizes do not go below minimum width or height.
        /// </summary>
        /// <param name="e">Current Unity event.</param>
        private void HandleResizeInteraction(Event e)
        {
            if (e.type == EventType.MouseDrag)
            {
                if (isResizingColumn)
                {
                    Vector2 mouseDelta = e.mousePosition - initialResizePosition;
                    Undo.RecordObject(this, "Resize Column");
                    columnSizes[resizeTargetIndex] = Mathf.Max(originalDimension + mouseDelta.x, minColumnWidth);
                    e.Use();
                }

                if (isResizingRow)
                {
                    Vector2 mouseDelta = e.mousePosition - initialResizePosition;
                    Undo.RecordObject(this, "Resize Row");
                    rowSizes[resizeTargetIndex] = Mathf.Max(originalDimension + mouseDelta.y, minRowHeight);
                    e.Use();
                }
            }

            if (e.type == EventType.MouseUp)
            {
                isResizingColumn = false;
                isResizingRow = false;
            }
        }

        #endregion
    }
}