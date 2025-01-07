using UnityEngine;
using UnityEditor;
using System.IO;
using System.Linq;
using static TSVTools.TabTableUtility;

namespace TSVTools
{
    public class TSVEditorWindow : EditorWindow
    {
        // Public variables
        public VerticalTable[] tableData;
        public string filePath = "";
        public int targetIndex;
        public LineDirection removalDirection;
        public float[] rowSizes;
        public float[] columnSizes;

        // State variables
        public bool isResizingRow = false;
        public bool isResizingColumn = false;
        public Vector2 initialResizePosition;
        public float originalDimension;
        public int resizeTargetIndex;
        public Vector2 mainScrollPosition;
        public Vector2 tableScrollPosition;

        // Constants
        private const float MinColumnWidth = 50f;
        private const float MinRowHeight = 30f;
        private const float DoubleClickThreshold = 0.5f;

        // UI Colors
        private static Color defaultLineColor = new(0.7f, 0.7f, 0.7f, 1);
        private static Color highlightLineColor = Color.red;
        private static Color tableBackgroundColor = Color.gray;
        private static Color editorBackgroundColor = Color.black;
        private static Color labelBackgroundColor = Color.blue;

        // Styles and textures
        private GUIStyle cellTextStyle;
        private GUIStyle headerLabelStyle;
        private Texture2D editorBackgroundTexture;

        // Timing
        private float lastInteractionTime = 0f;

        [MenuItem("Window/TSV Tools/TSV Editor", false, 2028)]
        public static void OpenTableEditorWindow()
        {
            GetWindow<TSVEditorWindow>("TSV Editor");
        }

        private void OnEnable()
        {
            Undo.undoRedoPerformed += OnUndoRedo;
        }

        private void OnDestroy()
        {
            Undo.undoRedoPerformed -= OnUndoRedo;
        }

        private void OnUndoRedo()
        {
            Repaint();
        }

        private void OnGUI()
        {
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

            mainScrollPosition = GUILayout.BeginScrollView(mainScrollPosition);

            GUILayout.Label("TSV Table", EditorStyles.boldLabel);

            if (GUILayout.Button("Open File"))
            {
                string path = EditorUtility.OpenFilePanel("Select TSV File", Application.dataPath, "tsv");
                if (!string.IsNullOrEmpty(path))
                {
                    filePath = path;
                    LoadTableFromFile(filePath);
                }
            }

            GUILayout.BeginHorizontal();
            GUILayout.Label("Uploaded File:", EditorStyles.boldLabel, GUILayout.Width(90));
            GUILayout.Label(filePath);
            GUILayout.EndHorizontal();

            if (tableData != null && tableData.Length > 0)
            {
                if (GUILayout.Button("Save Table"))
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

                var options = new string[] { "Horizontal", "Vertical" };
                int newLineDirectionIndex = EditorGUILayout.Popup("Direction", (int)removalDirection, options);
                if (newLineDirectionIndex != (int)removalDirection)
                {
                    Undo.RecordObject(this, "Change Line Direction");
                    removalDirection = (LineDirection)newLineDirectionIndex;
                }

                if (GUILayout.Button("Remove Row/Column"))
                {
                    if (targetIndex >= 0 && targetIndex < (removalDirection == LineDirection.Vertical ? tableData.Length : tableData[0].horizontalTable.Length))
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
                    }
                    else
                    {
                        EditorUtility.DisplayDialog("Error", "Index out of bounds", "OK");
                    }
                }

                RenderTable();
            }
            else
            {
                GUILayout.Label("No tables loaded.");

                if (GUILayout.Button("Create Table"))
                {
                    Undo.RecordObject(this, "Create Table");

                    tableData = new VerticalTable[2];

                    var row1 = new VerticalTable
                    {
                        horizontalTable = new string[] { "", "" }
                    };

                    var row2 = new VerticalTable
                    {
                        horizontalTable = new string[] { "", "" }
                    };

                    tableData[0] = row1;
                    tableData[1] = row2;

                    rowSizes = new float[tableData.Length];
                    columnSizes = new float[tableData[0].horizontalTable.Length];

                    for (int i = 0; i < tableData.Length; i++)
                    {
                        rowSizes[i] = 30f;
                    }

                    for (int j = 0; j < tableData[0].horizontalTable.Length; j++)
                    {
                        columnSizes[j] = 100f;
                    }
                }
            }

            GUILayout.EndScrollView();
        }

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

                for (int i = 0; i < rows; i++)
                    rowSizes[i] = 30f;

                for (int j = 0; j < cols; j++)
                    columnSizes[j] = 100f;
            }
            else
            {
                EditorUtility.DisplayDialog("Error", "File not found!", "OK");
            }
        }

        private void RenderTable()
        {
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

            tableScrollPosition = GUI.BeginScrollView(blackAreaRect, tableScrollPosition, tableRect);

            Event e = Event.current;

            float xOffset = MinColumnWidth;
            for (int j = 0; j < tableData[0].horizontalTable.Length; j++)
            {
                Rect cellRect = new(xOffset, 0, columnSizes[j], MinRowHeight);
                EditorGUI.DrawRect(cellRect, isResizingColumn && resizeTargetIndex == j ? highlightLineColor : labelBackgroundColor);

                string cellContent = j.ToString();
                GUI.Label(cellRect, cellContent, headerLabelStyle);

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

            HandleResizeInteraction(e);

            GUI.EndScrollView();
        }

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

        private void SaveTableToFile(string path)
        {
            SaveTableFile(path, tableData);
            EditorUtility.DisplayDialog("Success", "Table saved successfully!", "OK");
        }
    }
}