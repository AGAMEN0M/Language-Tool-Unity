/*
 * ---------------------------------------------------------------------------
 * Description: This script offers utility functions for working with tab-separated value (TSV) tables in Unity. 
 *              It allows loading, saving, modifying, and accessing cell data in a table format, with methods for 
 *              adding and removing rows or columns. The tables are represented as arrays of VerticalTable objects, 
 *              where each object contains a row of tab-separated values. The script also provides text conversion 
 *              functions to handle special tokens for formatting.
 *              
 * Author: Lucas Gomes Cecchini
 * Pseudonym: AGAMENOM
 * ---------------------------------------------------------------------------
*/

using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.IO;

namespace TSVTools
{
    /// <summary>
    /// Utility class for handling tab-separated value (TSV) tables.
    /// Provides methods for converting text, loading and saving tables, and accessing cell data.
    /// </summary>
    public class TabTableUtility
    {
        /// <summary>
        /// Converts text between raw format and readable format by replacing or restoring special tokens.
        /// </summary>
        /// <param name="text">The text to convert.</param>
        /// <returns>The converted text.</returns>
        public static string ConvertText(string text)
        {
            if (string.IsNullOrEmpty(text)) return string.Empty; // Handle null or empty text.

            // Check if text contains special tokens, replace them with actual characters.
            if (text.Contains("%tab%") || text.Contains("%newline%"))
            {
                text = text.Replace("%tab%", "\t"); // Replace %tab% with a tab character.
                text = text.Replace("%newline%", "\n"); // Replace %newline% with a newline character.
            }
            else
            {
                text = text.Replace("\t", "%tab%"); // Replace tab character with %tab% token.
                text = text.Replace("\n", "%newline%"); // Replace newline character with %newline% token.
            }

            return text; // Return the modified text.
        }

        /// <summary>
        /// Loads a TSV file into a table of VerticalTable objects.
        /// </summary>
        /// <param name="filePath">The path to the file to load.</param>
        /// <param name="table">The table to populate with the loaded data.</param>
        public static void LoadTableFile(string filePath, ref VerticalTable[] table)
        {
            // Check if the file exists. If not, log an error and return..
            if (!File.Exists(filePath))
            {
                Debug.LogError($"File not found: {filePath}");
                return;
            }

            // Initialize a list to store the rows dynamically (avoiding array resizing).
            var tableList = new List<VerticalTable>();

            // Open the file using StreamReader for efficient reading.
            using (StreamReader reader = new(filePath))
            {
                string line;

                // Read each line one by one.
                while ((line = reader.ReadLine()) != null)
                {
                    // Convert the line into a VerticalTable object and add it to the list.
                    var verticalTable = new VerticalTable
                    {
                        // Split each line into cells based on tab characters and convert the text.
                        horizontalTable = line.Split('\t').Select(cell => ConvertText(cell)).ToArray()
                    };

                    tableList.Add(verticalTable);
                }
            }

            // After processing all lines, convert the list to an array.
            table = tableList.ToArray();
        }

        /// <summary>
        /// Saves the given table to a TSV file, ensuring all rows have the same number of columns.
        /// </summary>
        /// <param name="filePath">The path to save the file to.</param>
        /// <param name="table">The table data to save.</param>
        public static void SaveTableFile(string filePath, VerticalTable[] table)
        {
            // Check if the table is null or empty. If so, log an error and return.
            if (table == null || table.Length == 0)
            {
                Debug.LogError("Table is empty or null.");
                return;
            }

            // Find the maximum number of columns in any row.
            int maxColumns = table.Max(row => row.horizontalTable?.Length ?? 0);

            // Normalize all rows to have the same number of columns.
            foreach (var row in table)
            {
                if (row.horizontalTable == null)
                {
                    row.horizontalTable = new string[maxColumns];
                }
                else if (row.horizontalTable.Length < maxColumns)
                {
                    var extendedRow = new string[maxColumns];
                    row.horizontalTable.CopyTo(extendedRow, 0); // Copy existing data.
                    for (int i = row.horizontalTable.Length; i < maxColumns; i++)
                    {
                        extendedRow[i] = string.Empty; // Fill missing cells with empty string.
                    }
                    row.horizontalTable = extendedRow;
                }
            }

            // Convert each row of the table into a string with tab-separated values.
            string[] lines = table.Select(row => string.Join("\t", row.horizontalTable.Select(cell => ConvertText(cell)))).ToArray();

            // Write the lines into the specified file.
            File.WriteAllLines(filePath, lines);
        }

        /// <summary>
        /// Retrieves the text from a specific cell in the table based on its vertical and horizontal indices.
        /// </summary>
        /// <param name="table">The table to retrieve data from.</param>
        /// <param name="columnVertical">The vertical index (row).</param>
        /// <param name="columnHorizontal">The horizontal index (column).</param>
        /// <returns>The text in the specified cell, or null if the indices are invalid.</returns>
        public static string GetText(VerticalTable[] table, int columnVertical, int columnHorizontal)
        {
            // Check if the table is null or if the vertical index is out of bounds. If so, log an error and return null.
            if (table == null || columnVertical < 0 || columnVertical >= table.Length)
            {
                Debug.LogError("Invalid vertical index.");
                return null;
            }

            // Check if the horizontal index is out of bounds. If so, log an error and return null.
            if (columnHorizontal < 0 || columnHorizontal >= table[columnVertical].horizontalTable.Length)
            {
                Debug.LogError("Invalid horizontal index.");
                return null;
            }

            // Return the requested text from the specified cell.
            return table[columnVertical].horizontalTable[columnHorizontal];
        }

        /// <summary>
        /// Sets the text in a specific cell in the table based on its vertical and horizontal indices.
        /// </summary>
        /// <param name="table">The table to update.</param>
        /// <param name="columnVertical">The vertical index (row).</param>
        /// <param name="columnHorizontal">The horizontal index (column).</param>
        /// <param name="text">The new text to set.</param>
        public static void SetText(ref VerticalTable[] table, int columnVertical, int columnHorizontal, string text)
        {
            // Check if the table is null or if the vertical index is out of bounds. If so, log an error and return.
            if (table == null || columnVertical < 0 || columnVertical >= table.Length)
            {
                Debug.LogError("Invalid vertical index.");
                return;
            }

            // Check if the horizontal index is out of bounds. If so, log an error and return.
            if (columnHorizontal < 0 || columnHorizontal >= table[columnVertical].horizontalTable.Length)
            {
                Debug.LogError("Invalid horizontal index.");
                return;
            }

            // Set the new text in the specified cell.
            table[columnVertical].horizontalTable[columnHorizontal] = text;
        }

        public enum LineDirection { Vertical, Horizontal }

        /// <summary>
        /// Adds a new line to the table, either vertically (row) or horizontally (column).
        /// </summary>
        /// <param name="table">The table to modify.</param>
        /// <param name="column">The position where the new line should be added.</param>
        /// <param name="lineDirection">The direction of the line to add (Vertical or Horizontal).</param>
        public static void AddLine(ref VerticalTable[] table, int column, LineDirection lineDirection)
        {
            if (table == null || table.Length == 0)
            {
                Debug.LogError("Table is empty or null.");
                return;
            }

            switch (lineDirection)
            {
                case LineDirection.Vertical:
                    // Adds a new row (Vertical Table) at the specific position.
                    var newVerticalRow = new VerticalTable
                    {
                        // Initializes the row with the same number of columns as the others, all empty.
                        horizontalTable = new string[table[0].horizontalTable.Length]
                    };

                    // Creates a list for manipulation and inserts the new line at the desired position.
                    var verticalList = table.ToList();
                    column = Mathf.Clamp(column, 0, verticalList.Count); // Ensures the index is within the limit.
                    verticalList.Insert(column, newVerticalRow);

                    // Updates the table with the new list.
                    table = verticalList.ToArray();
                    break;

                case LineDirection.Horizontal:
                    // Adds a new column (empty string) to all existing rows.
                    foreach (var row in table)
                    {
                        if (row.horizontalTable == null)
                        {
                            Debug.LogError("Linha inválida na tabela.");
                            return;
                        }

                        // Creates a list for manipulation and inserts the new cell in the desired position.
                        var horizontalList = row.horizontalTable.ToList();
                        column = Mathf.Clamp(column, 0, horizontalList.Count); // Ensures the index is within the limit.
                        horizontalList.Insert(column, string.Empty);

                        // Updates the row with the new list.
                        row.horizontalTable = horizontalList.ToArray();
                    }
                    break;

                default:
                    Debug.LogError("Invalid line direction.");
                    break;
            }
        }

        /// <summary>
        /// Removes a line from the table, either vertically (row) or horizontally (column).
        /// </summary>
        /// <param name="table">The table to modify.</param>
        /// <param name="column">The position of the line to remove.</param>
        /// <param name="lineDirection">The direction of the line to remove (Vertical or Horizontal).</param>
        public static void RemoveLine(ref VerticalTable[] table, int column, LineDirection lineDirection)
        {
            if (table == null || table.Length == 0)
            {
                Debug.LogError("Table is empty or null.");
                return;
            }

            switch (lineDirection)
            {
                case LineDirection.Vertical:
                    // Checks if the index is within limits.
                    if (column < 0 || column >= table.Length)
                    {
                        Debug.LogError("Vertical line index out of bounds.");
                        return;
                    }

                    // Removes the specified vertical line.
                    var verticalList = table.ToList();
                    verticalList.RemoveAt(column);

                    // Updates the table with the modified list.
                    table = verticalList.ToArray();
                    break;

                case LineDirection.Horizontal:
                    // Checks if the index is within bounds for all rows.
                    if (column < 0 || column >= table[0].horizontalTable.Length)
                    {
                        Debug.LogError("Horizontal column index out of bounds.");
                        return;
                    }

                    // Removes the specified column from all rows.
                    foreach (var row in table)
                    {
                        if (row.horizontalTable == null || row.horizontalTable.Length <= column)
                        {
                            Debug.LogError("Invalid row in table or index out of bounds.");
                            return;
                        }

                        // Creates a list for manipulation and removes the cell at the desired position.
                        var horizontalList = row.horizontalTable.ToList();
                        horizontalList.RemoveAt(column);

                        // Updates the row with the new list.
                        row.horizontalTable = horizontalList.ToArray();
                    }
                    break;

                default:
                    Debug.LogError("Invalid line direction.");
                    break;
            }
        }
    }

    /// <summary>
    /// Represents a vertical table, where each row contains a horizontal table (array of strings).
    /// </summary>
    [System.Serializable]
    public class VerticalTable
    {
        /// <summary>
        /// Array of strings representing the cells in a row of the table.
        /// </summary>
        public string[] horizontalTable;
    }
}