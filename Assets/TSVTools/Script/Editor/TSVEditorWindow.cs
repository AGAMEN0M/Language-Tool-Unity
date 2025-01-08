/*
 * ---------------------------------------------------------------------------
 * Description: A Unity editor script for managing TSV (Tab-Separated Values) tables.
 *              It provides functionalities to load, save, modify, and resize rows and columns in a table format.
 *              The script allows users to interact with the table through an editor window with scrollable views,
 *              resizing handles, and options for adding/removing rows/columns.
 * Author: Lucas Gomes Cecchini
 * Pseudonym: AGAMENOM
 * ---------------------------------------------------------------------------
*/

using UnityEngine;
using UnityEditor;
using System.IO;
using System.Linq;
using System;
using static TSVTools.TabTableUtility;

namespace TSVTools
{
    public class TSVEditorWindow : EditorWindow
    {
        // Public variables
        public VerticalTable[] tableData; // Array of vertical tables holding data for each row/column..
        public string filePath = ""; // Path of the loaded file.
        public int targetIndex; // Index of the selected row/column to perform actions on.
        public LineDirection removalDirection; // Direction for removing a row or column (Vertical or Horizontal).
        public float[] rowSizes; // Array to store the heights for each row.
        public float[] columnSizes; // Array to store the widths for each column.

        // State variables
        public bool isResizingRow = false; // Boolean indicating if the user is currently resizing a row.
        public bool isResizingColumn = false; // Boolean indicating if the user is currently resizing a column.
        public Vector2 initialResizePosition; // Initial position of the mouse when starting the resizing action.
        public float originalDimension; // The original size of the row/column before resizing.
        public int resizeTargetIndex; // Index of the row/column being resized.
        public Vector2 mainScrollPosition; // Scroll position of the entire window (for main scroll view).
        public Vector2 tableScrollPosition; // Scroll position of the table inside the window (for table scroll view).
        public bool isDirty = false; // Indicates whether the table has been modified since the last save. When set to true, it indicates unsaved changes.

        // Constants
        private const float MinColumnWidth = 50f; // Minimum width for a column.
        private const float MinRowHeight = 30f; // Minimum height for a row.
        private const float DoubleClickThreshold = 0.5f; // Threshold time in seconds for detecting double-click.

        // UI Colors
        private static Color defaultLineColor = new(0.7f, 0.7f, 0.7f, 1); // Default color for lines (used for resizing handles).
        private static Color highlightLineColor = Color.red; // Highlight color for lines (used when a line is being resized).
        private static Color tableBackgroundColor = Color.gray; // Background color for the table.
        private static Color editorBackgroundColor = Color.black; // Background color for the editor window.
        private static Color labelBackgroundColor = Color.blue; // Background color for labels (headers).

        // Styles and textures
        private GUIStyle cellTextStyle; // Style for the cell text (used in the editor to display cell content).
        private GUIStyle headerLabelStyle; // Style for header labels (used to display column and row headers).
        private Texture2D editorBackgroundTexture; // Background texture for the editor window.

        // Timing
        private float lastInteractionTime = 0f; // The time of the last interaction (used for double-click detection).

        // MenuItem to open the TSV editor window.
        [MenuItem("Window/TSV Editor Window", false, 2028)]
        public static void OpenTableEditorWindow()
        {
            var window = GetWindow<TSVEditorWindow>("TSV Editor"); // Opens the window.

            string fileName = "TSVEditorWindow Icon";
            Texture2D icon = null;

            // Finds and returns a texture by its name.
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

        // OnEnable callback to subscribe to undo/redo actions.
        private void OnEnable()
        {
            Undo.undoRedoPerformed += OnUndoRedo;
        }

        // OnDestroy callback to unsubscribe from undo/redo actions.
        private void OnDestroy()
        {
            // Check if there is data in the table.
            if (tableData != null && tableData.Length > 0)
            {
                SaveTableFile("UserSettings/TempTSV.tsv", tableData);
            }

            Undo.undoRedoPerformed -= OnUndoRedo;
        }

        // Callback when undo or redo actions are performed.
        private void OnUndoRedo()
        {
            Repaint();
        }

        // OnGUI method to draw and manage the editor window.
        private void OnGUI()
        {
            // Initialize styles and textures if they are not set.
            if (cellTextStyle == null || editorBackgroundTexture == null)
            {
                editorBackgroundTexture = CreateTexture(1, 1, tableBackgroundColor);

                cellTextStyle = new GUIStyle(EditorStyles.textField)
                {
                    normal = { textColor = editorBackgroundColor, background = editorBackgroundTexture },
                    focused = { textColor = editorBackgroundColor, background = editorBackgroundTexture },
                    active = { textColor = editorBackgroundColor, background = editorBackgroundTexture }
                };

                headerLabelStyle = new GUIStyle()
                {
                    normal = { textColor = Color.white },
                    alignment = TextAnchor.MiddleCenter,
                    fontStyle = FontStyle.Bold
                };
            }

            // Create scrollable region for the window content.
            mainScrollPosition = GUILayout.BeginScrollView(mainScrollPosition);

            GUILayout.Label("TSV Table", EditorStyles.boldLabel);

            // Button to open a TSV file.
            if (GUILayout.Button("Open File"))
            {
                string path = EditorUtility.OpenFilePanel("Select TSV File", Application.dataPath, "tsv");
                if (!string.IsNullOrEmpty(path))
                {
                    filePath = path;
                    LoadTableFromFile(filePath);
                }
            }

            // Display uploaded file path.
            GUILayout.BeginHorizontal();
            GUILayout.Label("Uploaded File:", EditorStyles.boldLabel, GUILayout.Width(90));
            GUILayout.Label(filePath);
            GUILayout.EndHorizontal();

            // If a table is loaded, allow saving and table manipulation.
            if (tableData != null && tableData.Length > 0)
            {
                string saveButtonText = isDirty ? "Save Table*" : "Save Table";

                // Save button.
                if (GUILayout.Button(saveButtonText))
                {
                    string savePath = EditorUtility.SaveFilePanel("Save TSV File", Application.dataPath, "Table", "tsv");
                    if (!string.IsNullOrEmpty(savePath))
                    {
                        SaveTableToFile(savePath);
                    }
                }

                GUILayout.Label("You can move rows or add new ones by creating them twice.");

                GUILayout.Label("Remove Row/Column", EditorStyles.boldLabel);
                int newIndexToRemove = EditorGUILayout.IntField("Number (index)", targetIndex);
                if (newIndexToRemove != targetIndex)
                {
                    Undo.RecordObject(this, "Change Index to Remove");
                    targetIndex = newIndexToRemove;
                }

                // Dropdown for selecting removal direction (row/column).
                var options = new string[] { "Horizontal", "Vertical" };
                int newLineDirectionIndex = EditorGUILayout.Popup("Direction", (int)removalDirection, options);
                if (newLineDirectionIndex != (int)removalDirection)
                {
                    Undo.RecordObject(this, "Change Line Direction");
                    removalDirection = (LineDirection)newLineDirectionIndex;
                }

                // Remove row/column button.
                if (GUILayout.Button("Remove Row/Column"))
                {
                    if (targetIndex >= 0 && targetIndex < (removalDirection == LineDirection.Vertical ? tableData.Length : tableData[0].horizontalTable.Length))
                    {
                        Undo.RecordObject(this, "Remove Row/Column");

                        // Remove the row or column based on the selected direction.
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
                    else
                    {
                        EditorUtility.DisplayDialog("Error", "Index out of bounds", "OK");
                    }
                }

                RenderTable(); // Render the table content.
            }
            else
            {
                GUILayout.Label("No tables loaded.");

                // Create a new table if none is loaded.
                if (GUILayout.Button("Create Table"))
                {
                    Undo.RecordObject(this, "Create Table");

                    // Create a 2x2 table as an initial example.
                    tableData = new VerticalTable[2];

                    var row1 = new VerticalTable
                    {
                        horizontalTable = new string[] { "", "" },
                    };

                    var row2 = new VerticalTable
                    {
                        horizontalTable = new string[] { "", "" }
                    };

                    tableData[0] = row1;
                    tableData[1] = row2;

                    rowSizes = new float[tableData.Length];
                    columnSizes = new float[tableData[0].horizontalTable.Length];

                    // Set default row heights and column widths.
                    for (int i = 0; i < tableData.Length; i++)
                    {
                        rowSizes[i] = 30f;
                    }

                    for (int j = 0; j < tableData[0].horizontalTable.Length; j++)
                    {
                        columnSizes[j] = 100f;
                    }

                    isDirty = true;
                }
            }

            GUILayout.EndScrollView();
        }

        // Create a single-colored texture for styling.
        private Texture2D CreateTexture(int width, int height, Color color)
        {
            Color32[] pixels = new Color32[width * height];
            Color32 color32 = color;
            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i] = color32;
            }
            Texture2D texture = new(width, height);
            texture.SetPixels32(pixels);
            texture.Apply();
            return texture;
        }

        // Load the table data from a TSV file.
        private void LoadTableFromFile(string path)
        {
            if (File.Exists(path))
            {
                Undo.RecordObject(this, "Load Table");

                LoadTableFile(path, ref tableData);

                int rows = tableData.Length;
                int cols = tableData[0].horizontalTable.Length;

                rowSizes = new float[rows];
                columnSizes = new float[cols];

                // Set default row heights and column widths for the loaded table.
                for (int i = 0; i < rows; i++)
                {
                    rowSizes[i] = 30f;
                }

                for (int j = 0; j < cols; j++)
                {
                    columnSizes[j] = 100f;
                }
            }
            else
            {
                EditorUtility.DisplayDialog("Error", "File not found!", "OK");
            }
        }

        // Render the table with dynamic resizing and interaction.
        private void RenderTable()
        {
            // Calculate total width and height of the table based on current row and column sizes.
            float totalWidth = 0f;
            float totalHeight = 0f;

            foreach (float width in columnSizes) totalWidth += width;
            foreach (float height in rowSizes) totalHeight += height;

            Vector2 margin = new(30, 190);

            totalWidth += MinColumnWidth;
            totalHeight += MinRowHeight;

            Rect tableRect = new(0, 0, totalWidth, totalHeight);

            Rect blackAreaRect = new(margin.x, margin.y, position.width - margin.x - 10, position.height - margin.y - 10);
            EditorGUI.DrawRect(blackAreaRect, editorBackgroundColor);

            // Start scroll view for the table.
            tableScrollPosition = GUI.BeginScrollView(blackAreaRect, tableScrollPosition, tableRect);

            Event e = Event.current;

            // Render columns (headers and resizing handles).
            float xOffset = MinColumnWidth;
            for (int j = 0; j < tableData[0].horizontalTable.Length; j++)
            {
                Rect cellRect = new(xOffset, 0, columnSizes[j], MinRowHeight);
                EditorGUI.DrawRect(cellRect, isResizingColumn && resizeTargetIndex == j ? highlightLineColor : labelBackgroundColor);

                string cellContent = j.ToString();
                GUI.Label(cellRect, cellContent, headerLabelStyle);

                // Handle column resizing interactions.
                Rect colResizeHandle = new(cellRect.xMax - 5, cellRect.y, 5, cellRect.height);
                EditorGUI.DrawRect(colResizeHandle, isResizingColumn && resizeTargetIndex == j ? highlightLineColor : defaultLineColor);

                if (e.type == EventType.MouseDown && colResizeHandle.Contains(e.mousePosition))
                {
                    if (Time.realtimeSinceStartup - lastInteractionTime < DoubleClickThreshold)
                    {
                        AddLine(ref tableData, j + 1, LineDirection.Horizontal);

                        var newColWidths = columnSizes.ToList();
                        newColWidths.Insert(j + 1, MinColumnWidth);
                        columnSizes = newColWidths.ToArray();
                    }

                    lastInteractionTime = Time.realtimeSinceStartup;

                    isResizingColumn = true;
                    initialResizePosition = e.mousePosition;
                    originalDimension = columnSizes[j];
                    resizeTargetIndex = j;
                    e.Use();
                }

                xOffset += columnSizes[j];
            }

            // Render rows with their corresponding cells.
            float yOffset = MinRowHeight;
            for (int i = 0; i < tableData.Length; i++)
            {
                xOffset = 0f;
                Rect rowLabelRect = new(xOffset, yOffset, MinColumnWidth, rowSizes[i]);
                EditorGUI.DrawRect(rowLabelRect, isResizingRow && resizeTargetIndex == i ? highlightLineColor : labelBackgroundColor);

                string rowLabel = i.ToString();
                GUI.Label(rowLabelRect, rowLabel, headerLabelStyle);

                for (int j = 0; j < tableData[i].horizontalTable.Length; j++)
                {
                    Rect cellRect = new(xOffset + MinColumnWidth, yOffset, columnSizes[j], rowSizes[i]);
                    EditorGUI.DrawRect(cellRect, tableBackgroundColor);

                    string cellContent = tableData[i].horizontalTable[j];
                    Rect textFieldRect = new(cellRect.x + 5, cellRect.y + 5, cellRect.width - 10, cellRect.height - 10);

                    string newCellContent = EditorGUI.TextField(textFieldRect, cellContent, cellTextStyle);
                    if (newCellContent != tableData[i].horizontalTable[j])
                    {
                        Undo.RecordObject(this, "Edit Cell Content");
                        cellContent = newCellContent;
                        isDirty = true;
                    }

                    if (cellContent != tableData[i].horizontalTable[j])
                    {
                        tableData[i].horizontalTable[j] = cellContent;
                    }

                    Rect colResizeHandle = new(cellRect.xMax - 5, cellRect.y, 5, cellRect.height);
                    EditorGUI.DrawRect(colResizeHandle, isResizingColumn && resizeTargetIndex == j ? highlightLineColor : defaultLineColor);

                    if (e.type == EventType.MouseDown && colResizeHandle.Contains(e.mousePosition))
                    {
                        if (Time.realtimeSinceStartup - lastInteractionTime < DoubleClickThreshold)
                        {
                            Undo.RecordObject(this, "Add Line");

                            AddLine(ref tableData, j + 1, LineDirection.Horizontal);

                            var newColWidths = columnSizes.ToList();
                            newColWidths.Insert(j + 1, MinColumnWidth);
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

                    xOffset += columnSizes[j];
                }

                // Handle row resizing.
                Rect rowResizeHandle = new(0, yOffset + rowSizes[i] - 5, tableRect.width, 5);
                EditorGUI.DrawRect(rowResizeHandle, isResizingRow && resizeTargetIndex == i ? highlightLineColor : defaultLineColor);

                if (e.type == EventType.MouseDown && rowResizeHandle.Contains(e.mousePosition))
                {
                    if (Time.realtimeSinceStartup - lastInteractionTime < DoubleClickThreshold)
                    {
                        Undo.RecordObject(this, "Add Line");

                        AddLine(ref tableData, i + 1, LineDirection.Vertical);

                        var newRowHeights = rowSizes.ToList();
                        newRowHeights.Insert(i + 1, MinRowHeight);
                        rowSizes = newRowHeights.ToArray();
                        isDirty = true;
                    }

                    lastInteractionTime = Time.realtimeSinceStartup;

                    isResizingRow = true;
                    initialResizePosition = e.mousePosition;
                    originalDimension = rowSizes[i];
                    resizeTargetIndex = i;
                    e.Use();
                }

                yOffset += rowSizes[i];
            }

            // Handle resizing interactions (dragging columns or rows).
            HandleResizeInteraction(e);

            GUI.EndScrollView();
        }

        // Handle column and row resizing interactions.
        private void HandleResizeInteraction(Event e)
        {
            if (e.type == EventType.MouseDrag)
            {
                if (isResizingColumn)
                {
                    Vector2 mouseDelta = e.mousePosition - initialResizePosition;

                    Undo.RecordObject(this, "Resize Column");

                    columnSizes[resizeTargetIndex] = Mathf.Max(originalDimension + mouseDelta.x, MinColumnWidth);
                    e.Use();
                }

                if (isResizingRow)
                {
                    Vector2 mouseDelta = e.mousePosition - initialResizePosition;

                    Undo.RecordObject(this, "Resize Row");

                    rowSizes[resizeTargetIndex] = Mathf.Max(originalDimension + mouseDelta.y, MinRowHeight);
                    e.Use();
                }
            }

            if (e.type == EventType.MouseUp)
            {
                isResizingColumn = false;
                isResizingRow = false;
            }
        }

        // Save the table data to a TSV file.
        private void SaveTableToFile(string path)
        {
            SaveTableFile(path, tableData);
            isDirty = false;
            EditorUtility.DisplayDialog("Success", "Table saved successfully!", "OK");
        }
    }
}