/*
 * ---------------------------------------------------------------------------
 * Description: This script provides the editor window for managing language 
 *              files and components in Unity. It allows users to save, load, and organize 
 *              language files, and provides tools to handle language-related UI components 
 *              and canvas data.
 * Author: Lucas Gomes Cecchini
 * Pseudonym: AGAMENOM
 * ---------------------------------------------------------------------------
*/

#if UNITY_EDITOR
using System.Collections.Generic;
using System.Globalization;
using LanguageTools.Editor;
using LanguageTools;
using UnityEditor;
using UnityEngine;
using System.Linq;
using System.Text;
using System.IO;
using System;

public class LanguageFileManagerWindow : EditorWindow
{
    public string fileName = "ENGLISH"; // The name of the language file.
    public string computerLanguage = "English"; // The selected language on the computer.

    public List<LanguageComponentSave> componentSave = new(); // List of language components to save.
    public List<CanvasSave> canvasSave = new(); // List of canvas components to save.

    public Vector2 scrollPosition = Vector2.zero; // Used for handling scroll position in the GUI.
    public bool firstTime = false; // Flag to determine if it's the first time opening the window.
    public int idIndex; // Stores the current selected ID.

    private const string fileData = "ProjectSettings/LanguageFileData.json"; // Path to the saved JSON data file.
    private string saveFilePath; // Path to the language file folder.

    private SerializedObject serializedObject; // Serialized object to track changes in the GUI.
    private List<int> duplicateID = new(); // List of duplicate component IDs.
    private List<int> canvasID = new(); // List of duplicate canvas IDs.

    private string[] cultureNames; // Array of culture names for language selection.
    private CultureInfo[] cultures; // Array of CultureInfo objects.
    private int selectedCultureIndex; // Stores the currently selected culture index.

    // Adds a menu item to open the Language File Manager window.
    [MenuItem("Window/Language/Language File Manager")]
    public static void ShowEditorWindow()
    {
        var window = GetWindow<LanguageFileManagerWindow>("Language File Manager"); // Opens the window.
        window.titleContent = new GUIContent("Language File Manager", LanguageEditorUtilities.FindTextureByName("LanguageFileManagerWindow Icon"));
    }

    // Called when the window is enabled.
    private void OnEnable()
    {
        Undo.undoRedoPerformed += OnUndoRedo; // Subscribes to undo/redo actions.
        serializedObject = new SerializedObject(this); // Creates a serialized object for this window.

        cultures = CultureInfo.GetCultures(CultureTypes.AllCultures); // Loads all available cultures.
        cultureNames = new string[cultures.Length]; // Initializes the culture name array.

        for (int i = 0; i < cultures.Length; i++) // Fills culture names array with display names.
        {
            cultureNames[i] = cultures[i].DisplayName;
        }

        saveFilePath = LanguageFileManager.GetLanguageFilesFolderPath(); // Gets the save file path for language files.
    }

    // Called when the window is destroyed.
    private void OnDestroy()
    {
        Undo.undoRedoPerformed -= OnUndoRedo; // Unsubscribes from undo/redo actions.
        SaveDataJson(); // Saves data when the window is closed.
    }

    // Repaints the window on undo/redo actions.
    private void OnUndoRedo()
    {
        Repaint();
    }

    // Main GUI method that draws the interface elements.
    private void OnGUI()
    {
        FindDuplicateIds(); // Checks for duplicate IDs.

        if (firstTime == false) // Loads default data if this is the first time opening the window.
        {
            LanguageEditorUtilities.AddDefaultLanguageComponents(ref componentSave, ref canvasSave); // Adds default language components.
            LoadDataJson(); // Loads saved data from JSON.
            firstTime = true;
        }

        serializedObject.Update(); // Updates the serialized object to track changes in the GUI.
        scrollPosition = GUILayout.BeginScrollView(scrollPosition); // Begin scrollable area.

        EditorGUILayout.Space(10);
        DrawActionButtons(); // Draws action buttons (e.g., save/load).
        EditorGUILayout.Space(10);

        EditorGUILayout.LabelField("Language File Editor Settings", LanguageEditorUtilities.CreateLabelStyle(15, true)); // Section title.
        EditorGUILayout.Space(10);

        string previousFileName = fileName; // Cache the previous file name.
        previousFileName = LanguageEditorUtilities.DrawLabeledTextField("File Name:", previousFileName, 130, 150); // Draws a text field for the file name.
        if (fileName != previousFileName) // If the file name changed.
        {
            Undo.RecordObject(this, "Change File Name"); // Records the change for undo.
            fileName = previousFileName; // Updates the file name.
            EditorUtility.SetDirty(this); // Marks the object as dirty (unsaved changes).
        }

        DrawLanguageDropdown();
        EditorGUILayout.Space(20); // Draws the language selection dropdown.

        if (componentSave.Count > 0) // If there are saved components, display them.
        {
            LanguageEditorUtilities.DrawColoredBox(() =>
            {
                EditorGUILayout.Space(10);
                DrawIdDisplayPanel(); // Draws the ID panel.
                EditorGUILayout.Space(10);
            }, new Color(0, 0, 0, 0.15f)); // Adds a slightly transparent box around the ID panel.
        }

        EditorGUILayout.Space(25);
        EditorGUILayout.LabelField("Feature Lists", LanguageEditorUtilities.CreateLabelStyle(15, true)); // Section for component and canvas lists.
        EditorGUILayout.Space(10);

        EditorGUILayout.PropertyField(serializedObject.FindProperty("componentSave"), true, GUILayout.Width(550)); // Displays the component save list.
        EditorGUILayout.PropertyField(serializedObject.FindProperty("canvasSave"), true, GUILayout.Width(550)); // Displays the canvas save list.

        GUILayout.EndScrollView(); // Ends the scrollable area.
        serializedObject.ApplyModifiedProperties(); // Applies any changes made to the serialized object.
    }

    // Draws buttons for actions like saving, loading, and opening the folder.
    private void DrawActionButtons()
    {
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.Space(20);
        DrawButton("Open Folder", () => Application.OpenURL(saveFilePath)); // Opens the save folder.
        EditorGUILayout.Space(10);
        DrawButton("Save File", () => SaveLanguageFile()); // Saves the language file.
        EditorGUILayout.Space(10);
        DrawButton("Load File", () => LoadLanguageFile()); // Loads a language file.
        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space(5);

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.Space(20);
        DrawButton("Create Excel Table", () => LanguageExcelConverterWindow.ShowEditorWindow()); // Opens the Excel converter window.
        EditorGUILayout.Space(10);
        DrawButton("Load Excel Table", () => LoadExcelCsv()); // Loads data from a CSV file.
        EditorGUILayout.Space(10);
        DrawButton("Rebuild Canvas", () => {
            GameObject rebuildCanvas = new("[Rebuild Canvas]"); // Creates a new GameObject to rebuild the canvas.
            Undo.RegisterCreatedObjectUndo(rebuildCanvas, "Create Rebuild Canvas"); // Registers the action for undo.
            Undo.AddComponent<RebuildCanvas>(rebuildCanvas); // Adds the RebuildCanvas component to the GameObject.
        });
        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space(5);

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.Space(20);
        DrawButton("Organize All Items", () => {
            Undo.RecordObject(this, "Organize All Items"); // Records the action for undo.
            CompareID(); // Organizes the component and canvas IDs.
            EditorUtility.SetDirty(this); // Marks the object as dirty (unsaved changes).
        });
        EditorGUILayout.Space(10);
        DrawButton("Remove ID", () => {
            Undo.RecordObject(this, "Remove ID"); // Records the action for undo.
            var itemToRemove = componentSave.FirstOrDefault(cs => cs.iD == idIndex); // Finds the component with the selected ID.

            if (itemToRemove != null) // If a component with the ID exists, ask for confirmation to remove it.
            {
                if (EditorUtility.DisplayDialog("Remove ID?", $"Do you really want to remove the ID '{idIndex}'?", "Yes", "No"))
                {
                    componentSave.Remove(itemToRemove); // Removes the component.
                    EditorUtility.SetDirty(this); // Marks the object as dirty.

                    if (componentSave.Count > 0) // If there are still components left, update the ID index.
                    {
                        idIndex = componentSave.Select(cs => cs.iD).OrderBy(id => Math.Abs(id - idIndex)).FirstOrDefault();
                    }
                    else
                    {
                        idIndex = 0; // Resets the ID index if no components remain.
                    }
                }
            }
            else
            {
                EditorUtility.DisplayDialog("ID not found", $"There is no component with ID '{idIndex}' to remove.", "Ok"); // Displays a warning if the ID is not found.
            }
        });
        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();
    }

    // Compares component and canvas IDs to ensure they are in order.
    private void CompareID()
    {
        componentSave.Sort((a, b) => a.iD.CompareTo(b.iD)); // Sorts components by ID.
        canvasSave.Sort((a, b) => a.canvasID.CompareTo(b.canvasID)); // Sorts canvases by ID.
    }

    // Draws a button with a specified label and action.
    private void DrawButton(string label, Action action)
    {
        if (GUILayout.Button(label, LanguageEditorUtilities.CreateCustomButtonStyle(15), GUILayout.Width(160), GUILayout.Height(30)))
        {
            action(); // Executes the provided action when the button is clicked.
        }
    }

    // Draws the dropdown for selecting the computer's language.
    private void DrawLanguageDropdown()
    {
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Computer Language:", GUILayout.Width(130)); // Label for the dropdown.

        string previousLanguage = computerLanguage; // Cache the current language.
        selectedCultureIndex = Array.FindIndex(cultures, c => c.DisplayName == previousLanguage); // Finds the current culture index.

        if (selectedCultureIndex < 0) selectedCultureIndex = 0; // Defaults to the first culture if the language is not found.

        int newSelectedCultureIndex = EditorGUILayout.Popup(selectedCultureIndex, cultureNames, GUILayout.Width(150)); // Draws the dropdown and gets the newly selected index.

        string newLanguage = cultures[newSelectedCultureIndex].DisplayName; // Gets the name of the selected language.
        if (newLanguage != previousLanguage) // If the language has changed.
        {
            Undo.RecordObject(this, "Change Computer Language"); // Records the action for undo.
            computerLanguage = newLanguage; // Updates the computer language.
            EditorUtility.SetDirty(this); // Marks the object as dirty (unsaved changes).
        }

        EditorGUILayout.Space(10);
        GUILayout.Label($"Recommended: {CultureInfo.InstalledUICulture.DisplayName}", LanguageEditorUtilities.CreateLabelStyle(13, true)); // Suggests the system's default language.
        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();
    }

    // Draws the panel that displays the current selected ID and allows navigation between IDs.
    private void DrawIdDisplayPanel()
    {
        EditorGUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        EditorGUILayout.LabelField("ID Display", LanguageEditorUtilities.CreateLabelStyle(15, true, true)); // Label for the ID display.
        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space(5);

        int minID = componentSave.Count > 0 ? componentSave.Min(cs => cs.iD) : 0; // Gets the minimum ID from the components.
        int maxID = componentSave.Count > 0 ? componentSave.Max(cs => cs.iD) : 0; // Gets the maximum ID from the components.

        EditorGUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        EditorGUILayout.LabelField($"{minID}/{maxID}", LanguageEditorUtilities.CreateLabelStyle(13, true, true)); // Displays the minimum and maximum ID.
        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        LanguageEditorUtilities.DrawArrowButton("<--", ref idIndex, minID, maxID, this, () => idIndex--); // Draws a button to decrease the ID index.

        EditorGUILayout.Space(10);
        int newIdIndex = EditorGUILayout.IntField(idIndex, new GUIStyle(EditorStyles.textField) { alignment = TextAnchor.MiddleCenter }, GUILayout.Width(50)); // Text field to manually enter an ID.
        if (newIdIndex != idIndex) // If the ID has changed.
        {
            Undo.RecordObject(this, "Change ID Index"); // Records the action for undo.
            idIndex = newIdIndex; // Updates the ID index.
            EditorUtility.SetDirty(this); // Marks the object as dirty (unsaved changes).
        }
        EditorGUILayout.Space(10);

        LanguageEditorUtilities.DrawArrowButton("-->", ref idIndex, minID, maxID, this, () => idIndex++); // Draws a button to increase the ID index.
        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();

        if (duplicateID.Count > 0) // Displays duplicate IDs in red if any exist.
        {
            EditorGUILayout.Space(10);
            GUI.color = Color.red;
            GUILayout.Label($"Duplicated ID: {string.Join(", ", duplicateID)}", LanguageEditorUtilities.CreateLabelStyle(15, true, true));
            GUI.color = Color.white;
        }

        EditorGUILayout.Space(10);

        bool itemRemoved = false; // Flag to track if an item is removed.
        LanguageComponentSave itemToRemove = null; // Stores the item to remove if needed.

        LanguageEditorUtilities.DrawColoredBox(() =>
        {
            if (duplicateID.Count > 0) // If there are duplicate IDs, display them.
            {
                foreach (var id in duplicateID)
                {
                    var items = componentSave.Where(cs => cs.iD == id).ToList();
                    foreach (var item in items)
                    {
                        RenderComponentDisplay(item); // Displays the component information.

                        EditorGUILayout.BeginHorizontal();
                        GUILayout.FlexibleSpace();
                        if (GUILayout.Button("-", LanguageEditorUtilities.CreateCustomButtonStyle(15), GUILayout.Width(20), GUILayout.Height(20)))
                        {
                            itemToRemove = item; // Marks the item to be removed.
                            itemRemoved = true; // Sets the flag to true.
                        }
                        EditorGUILayout.EndHorizontal();

                        EditorGUILayout.Space(5);

                        LanguageEditorUtilities.DrawColoredBox(() =>
                        {
                            EditorGUILayout.Space(1);
                        }, new Color(1, 1, 1, 1)); // Adds a small box around the component information.
                    }
                }
            }
            else // If no duplicate IDs, display the selected component.
            {
                var item = componentSave.FirstOrDefault(cs => cs.iD == idIndex);
                if (item != null)
                {
                    RenderComponentDisplay(item);
                }
                else
                {
                    EditorGUILayout.LabelField("ID Slot Available", LanguageEditorUtilities.CreateLabelStyle(18, true, true)); // Displays if the ID slot is available.
                }
            }

        }, new(0, 0, 1, 0.5f));

        if (itemRemoved && itemToRemove != null) // If an item was marked for removal, remove it.
        {
            Undo.RecordObject(this, "Remove Duplicate ID");
            componentSave.Remove(itemToRemove);
            EditorUtility.SetDirty(this);
            FindDuplicateIds(); // Rechecks for duplicate IDs after removing.
        }
    }

    // Renders the display for a language component.
    private void RenderComponentDisplay(LanguageComponentSave component)
    {
        EditorGUILayout.Space(10);
        if (component != null)
        {
            LanguageEditorUtilities.DisplayComponentIcon(component.componentType); // Displays an icon based on the component type.
            int previousInt = component.iD;
            previousInt = LanguageEditorUtilities.DrawLabeledIntField("ID:", previousInt, 20, 50); // Displays the component ID and allows editing.
            if (component.iD != previousInt) // If the ID changed.
            {
                Undo.RecordObject(this, "Change item.iD");
                component.iD = previousInt;
                EditorUtility.SetDirty(this); // Marks the object as dirty.
            }
            EditorGUILayout.Space(10);
            if (component.textWrite) // If the component has text, display the text field.
            {
                string previousText = component.text;
                previousText = LanguageEditorUtilities.DrawLabeledTextField("Text:", previousText, 40, 430); // Displays the text field.
                if (component.text != previousText) // If the text changed.
                {
                    Undo.RecordObject(this, "Change item.text");
                    component.text = previousText;
                    EditorUtility.SetDirty(this); // Marks the object as dirty.
                }
            }
            EditorGUILayout.Space(10);
            EditorGUILayout.BeginHorizontal();
            if (component.alignmentWrite) // If the component has alignment, display the alignment field.
            {
                int previousAlignment = component.alignment;
                previousAlignment = LanguageEditorUtilities.DrawLabeledIntField("Alignment:", previousAlignment, 70, 50);
                if (component.alignment != previousAlignment) // If the alignment changed.
                {
                    Undo.RecordObject(this, "Change item.alignment");
                    component.alignment = previousAlignment;
                    EditorUtility.SetDirty(this); // Marks the object as dirty.
                }
            }
            if (component.fontListIndexWrite) // If the component has a font size, display the font size field.
            {
                int previousIndex = component.fontSize;
                previousIndex = LanguageEditorUtilities.DrawLabeledIntField("Font Size:", previousIndex, 70, 50);
                if (component.fontSize != previousIndex) // If the font size changed.
                {
                    Undo.RecordObject(this, "Change item.fontSize");
                    component.fontSize = previousIndex;
                    EditorUtility.SetDirty(this); // Marks the object as dirty.
                }
            }
            if (component.fontListIndexWrite) // If the component has a font index, display the font index field.
            {
                int previousFont = component.fontListIndex;
                previousFont = LanguageEditorUtilities.DrawLabeledIntField("Font Index:", previousFont, 70, 50);
                if (component.fontListIndex != previousFont) // If the font index changed.
                {
                    Undo.RecordObject(this, "Change item.fontListIndex");
                    component.fontListIndex = previousFont;
                    EditorUtility.SetDirty(this); // Marks the object as dirty.
                }
            }
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
        }
        EditorGUILayout.Space(10);
    }

    // Finds and stores duplicate component and canvas IDs.
    private void FindDuplicateIds()
    {
        var idsArray = componentSave.Select(cs => cs.iD).ToArray();
        duplicateID = LanguageEditorUtilities.FindDuplicateIDs(idsArray); // Finds duplicate component IDs.

        var canvasArray = canvasSave.Select(cs => cs.canvasID).ToArray();
        canvasID = LanguageEditorUtilities.FindDuplicateIDs(canvasArray); // Finds duplicate canvas IDs.
    }

    // Saves the language file to disk as a .txt file.
    private void SaveLanguageFile()
    {
        Undo.RecordObject(this, "Save File"); // Records the action for undo.
        CompareID(); // Organizes the component and canvas IDs.
        EditorUtility.SetDirty(this); // Marks the object as dirty (unsaved changes).
        FindDuplicateIds(); // Rechecks for duplicate IDs.
        SaveDataJson(); // Saves the data to JSON.

        if (duplicateID.Count == 0 && canvasID.Count == 0) // If no duplicates exist, proceed to save.
        {
            string filePath = $"{saveFilePath}/{fileName}.txt"; // Determines the file path.

            if (File.Exists(filePath)) // If the file already exists, ask for confirmation to replace it.
            {
                if (!EditorUtility.DisplayDialog("Replace file?", $"The file '{fileName}.txt' already exists. Do you want to replace it?", "Yes", "No"))
                {
                    return;
                }
            }

            StringBuilder sb = new(); // StringBuilder to build the file content.

            sb.AppendLine($"Linguagem - [{fileName}]"); // Adds the file header.
            sb.AppendLine(computerLanguage); // Adds the computer language.

            foreach (LanguageComponentSave id in componentSave) // Adds each component to the file.
            {
                sb.Append($"id:{id.iD}; ");
                if (id.textWrite) sb.Append($"{{{id.text}}} ");
                if (id.alignmentWrite) sb.Append($"Ali:{id.alignment}; ");
                if (id.fontSizeWrite) sb.Append($"S:{id.fontSize:F0}; ");
                if (id.fontListIndexWrite) sb.Append($"Font:{id.fontListIndex}; ");
                sb.Append($"Type:{id.componentType:F0};");
                sb.AppendLine();
            }

            sb.AppendLine("");

            foreach (CanvasSave canvasID in canvasSave) // Adds each canvas to the file.
            {
                sb.Append($"canvasID:{canvasID.canvasID};");
                sb.Append(canvasID.json);
                sb.AppendLine();
            }

            File.WriteAllText(filePath, sb.ToString()); // Writes the content to the file.
            Debug.Log($"Saved File: {filePath}"); // Logs the file save location.
        }
        else // If duplicates exist, logs an error.
        {
            if (duplicateID.Count != 0)
            {
                Debug.LogError($"Language Save Editor: Unable to save due to duplicate IDs: {string.Join(", ", duplicateID)}");
            }
            else
            {
                Debug.LogError($"Language Save Editor: Unable to save due to duplicate canvasIDs: {string.Join(", ", canvasID)}");
            }
        }
    }

    // Loads a language file from disk.
    private void LoadLanguageFile()
    {
        if (!Directory.Exists(saveFilePath)) // Checks if the folder path exists.
        {
            Debug.LogError($"Folder path does not exist: {saveFilePath}");
            return;
        }

        string filePath = EditorUtility.OpenFilePanel("Select Language File", saveFilePath, "txt"); // Opens a file selection dialog.

        if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath) || Path.GetExtension(filePath) != ".txt") // Checks if the selected file is valid.
        {
            Debug.LogError("Invalid file selected. Please select a valid .txt file.");
            return;
        }

        try
        {
            Undo.RecordObject(this, "Load File"); // Records the action for undo.

            string[] lines = File.ReadAllLines(filePath); // Reads all lines from the file.
            componentSave.Clear(); // Clears the current components.
            canvasSave.Clear(); // Clears the current canvases.

            fileName = Path.GetFileNameWithoutExtension(filePath); // Gets the file name.
            computerLanguage = lines.Length > 1 ? lines[1] : "Invariant Language (Invariant Country)"; // Gets the computer language.

            foreach (string line in lines) // Processes each line to add components.
            {
                if (!line.StartsWith("id:")) continue;
                var component = LanguageEditorUtilities.ParseLanguageComponent(line);
                if (component != null) componentSave.Add(component);
            }

            foreach (string line in lines) // Processes each line to add canvases.
            {
                if (!line.StartsWith("canvasID:")) continue;
                var canvas = LanguageEditorUtilities.ParseCanvasSave(line);
                if (canvas != null) canvasSave.Add(canvas);
            }

            CompareID(); // Organizes the IDs.
            EditorUtility.SetDirty(this); // Marks the object as dirty.
        }
        catch (Exception e) // Catches and logs any errors during loading.
        {
            Debug.LogError($"Error while loading file: {e.Message}");
        }
    }

    // Loads data from a CSV file.
    private void LoadExcelCsv()
    {
        Undo.RecordObject(this, "Load Excel Table"); // Records the action for undo.
        string filePath = EditorUtility.OpenFilePanel("Select CSV File", saveFilePath, "csv"); // Opens a file selection dialog.

        if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath) || Path.GetExtension(filePath) != ".csv") // Checks if the selected file is valid.
        {
            Debug.LogWarning("Invalid CSV file selected.");
            return;
        }

        try
        {
            var lines = File.ReadAllLines(filePath, Encoding.UTF8); // Reads all lines from the file.

            if (lines.Length == 0) // If the file is empty, logs a warning.
            {
                Debug.LogWarning("The CSV file is empty.");
                return;
            }

            string[] headerFields = lines[0].Split(';'); // Splits the header into fields.
            string[] languages = headerFields.Skip(1).ToArray(); // Gets the language columns.

            List<ExcelData> excelDataList = new(); // List to store the parsed data.
            foreach (string language in languages) // Initializes the list for each language.
            {
                excelDataList.Add(new ExcelData
                {
                    language = language,
                    lines = new List<KeyValuePair<int, string>>()
                });
            }

            for (int i = 1; i < lines.Length; i++) // Processes each row in the CSV.
            {
                string[] fields = lines[i].Split(';'); // Splits the row into fields.

                if (fields.Length != headerFields.Length) // Checks if the row has the correct number of fields.
                {
                    Debug.LogWarning($"Row {i + 1} has a different number of columns than the header and was ignored.");
                    continue;
                }

                if (!int.TryParse(fields[0], out int id)) // Parses the ID from the first column.
                {
                    Debug.LogWarning($"The ID at line {i + 1} is not a valid number and was ignored.");
                    continue;
                }

                for (int j = 1; j < fields.Length; j++) // Processes each language field.
                {
                    string text = fields[j];
                    excelDataList[j - 1].lines.Add(new KeyValuePair<int, string>(id, text)); // Adds the ID and text to the list.
                }
            }

            foreach (var excelData in excelDataList) // Updates the components with the parsed data.
            {
                if (excelData.language == fileName) // Checks if the language matches the current file.
                {
                    foreach (var line in excelData.lines) // Updates the matching components.
                    {
                        var existingComponent = componentSave.FirstOrDefault(component => component.iD == line.Key);

                        if (existingComponent != null)
                        {
                            int index = componentSave.IndexOf(existingComponent);
                            componentSave[index].text = line.Value; // Updates the text of the component.
                        }
                    }
                }
            }
        }
        catch (Exception ex) // Catches and logs any errors during loading.
        {
            Debug.LogError($"Error loading CSV file: {ex.Message}");
        }

        EditorUtility.SetDirty(this); // Marks the object as dirty (unsaved changes).
    }

    // Saves the current data to a JSON file.
    public void SaveDataJson()
    {
        LanguageFileData data = new()
        {
            fileName = fileName,
            computerLanguage = computerLanguage,
            firstTime = firstTime,
            idIndex = idIndex,
            componentSave = componentSave,
            canvasSave = canvasSave
        };
        string jsonData = JsonUtility.ToJson(data); // Converts the data to JSON format.

        try
        {
            File.WriteAllText(fileData, jsonData); // Writes the JSON data to the file.
        }
        catch (Exception e) // Catches and logs any errors during saving.
        {
            Debug.LogError($"Error while saving data: {e.Message}");
        }
    }

    // Loads data from a JSON file.
    private void LoadDataJson()
    {
        if (File.Exists(fileData)) // Checks if the data file exists.
        {
            string jsonData = File.ReadAllText(fileData); // Reads the JSON data from the file.
            var data = JsonUtility.FromJson<LanguageFileData>(jsonData); // Parses the JSON data.

            fileName = data.fileName; // Updates the file name.
            computerLanguage = data.computerLanguage; // Updates the computer language.
            firstTime = data.firstTime; // Updates the first-time flag.
            idIndex = data.idIndex; // Updates the selected ID index.

            componentSave.Clear(); // Clears the current component list.
            componentSave = data.componentSave; // Updates the component list.

            canvasSave.Clear(); // Clears the current canvas list.
            canvasSave = data.canvasSave; // Updates the canvas list.
        }
    }

    // Adds or updates a component in the save list.
    public void AddComponent(LanguageComponentSave data)
    {
        Undo.RecordObject(this, "Add Component"); // Records the action for undo.
        var existingComponent = componentSave.FirstOrDefault(component => component.iD == data.iD); // Checks if the component already exists.

        if (existingComponent != null) // If the component exists, update it.
        {
            int index = componentSave.IndexOf(existingComponent);
            componentSave[index] = data;
        }
        else // If the component doesn't exist, add it.
        {
            componentSave.Add(data);
        }

        componentSave.Sort((a, b) => a.iD.CompareTo(b.iD)); // Sorts the components by ID.
        idIndex = data.iD; // Updates the selected ID index.
        EditorUtility.SetDirty(this); // Marks the object as dirty.
    }

    // Adds or updates a canvas in the save list.
    public void AddCanvas(CanvasSave data)
    {
        Undo.RecordObject(this, "Add Canvas"); // Records the action for undo.
        var existingComponent = canvasSave.FirstOrDefault(component => component.canvasID == data.canvasID); // Checks if the canvas already exists.

        if (existingComponent != null) // If the canvas exists, update it.
        {
            int index = canvasSave.IndexOf(existingComponent);
            canvasSave[index] = data;
        }
        else // If the canvas doesn't exist, add it.
        {
            canvasSave.Add(data);
        }

        canvasSave.Sort((a, b) => a.canvasID.CompareTo(b.canvasID)); // Sorts the canvases by ID.
        idIndex = data.canvasID; // Updates the selected canvas ID index.
        EditorUtility.SetDirty(this); // Marks the object as dirty.
    }
}
#endif