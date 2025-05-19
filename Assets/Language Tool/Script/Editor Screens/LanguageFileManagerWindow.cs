/*
 * ---------------------------------------------------------------------------
 * Description: Editor window for managing language files and UI components in Unity.
 *              Allows saving, loading, and organizing language files,
 *              with tools to handle language components and canvas data.
 *              
 * Author: Lucas Gomes Cecchini
 * Pseudonym: AGAMENOM
 * ---------------------------------------------------------------------------
*/

#if UNITY_EDITOR
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using LanguageTools;
using UnityEditor;
using UnityEngine;
using TSVTools;
using System.IO;
using System;

using static LanguageTools.Editor.LanguageEditorUtilities;
using static LanguageTools.LanguageFileManager;
using static TSVTools.TabTableUtility;
using static UnityEditor.EditorGUILayout;

public class LanguageFileManagerWindow : EditorWindow
{
    public string languageForEditing = "en"; // The language currently selected for editing in the dropdown.

    public List<LanguageForEditingSave> componentSave = new(); // List of saved language component data used for localization.
    public List<CanvasForEditingSave> canvasSave = new(); // List of saved canvas component data for UI elements.

    public List<LanguageAvailable> availableLanguages = new(); // List of languages that are available and detected from disk.

    public Vector2 scrollPosition = Vector2.zero; // Current scroll position for the window’s scroll view.
    public bool firstTime = false; // Whether the window is being initialized for the first time.
    public int idIndex; // Current index of the ID being edited or searched.
    public bool showTextData = true; // Whether text component data should be displayed in the editor window.
    public bool showCanvasData = true; // Whether canvas component data should be displayed in the editor window.
    public bool fileIsSaved = true; // Tracks whether the current data state has been saved to disk.

    private const string fileData = "ProjectSettings/LanguageFileData.json"; // Path to the local file for storing language editing state.
    private string assetsPath; // Root directory path for language TSV files.
    private string canvasDataFile; // Full path to the canvas data TSV file.
    private string languageDataFile; // Full path to the language data TSV file.
    private string metaDataFile; // Full path to the metadata TSV file.

    private SerializedObject serializedObject; // Serialized representation of this EditorWindow for Unity’s property drawer system.
    private List<int> duplicateID = new(); // List of duplicate IDs found in the language components.
    private List<int> canvasID = new(); // List of duplicate canvas IDs found in the canvas components.

    private string[] cultureNames; // Display names of available cultures.
    private CultureInfo[] cultures; // Array of all CultureInfo supported by .NET.
    private int selectedCultureIndex; // Index of the currently selected culture in the dropdown.

    private Texture2D trashImage; // Cached trash icon used in UI for delete buttons.

    /// <summary>
    /// Adds a menu item to open the Language File Manager editor window.
    /// </summary>
    [MenuItem("Window/Language/Language File Manager", false, 2029)]
    public static void ShowEditorWindow()
    {
        var window = GetWindow<LanguageFileManagerWindow>("Language File Manager");
        window.titleContent = new GUIContent("Language File Manager", FindTextureByName("LanguageFileManagerWindow Icon"));
    }

    /// <summary>
    /// Initializes the editor window when enabled.
    /// </summary>
    private void OnEnable()
    {
        Undo.undoRedoPerformed += OnUndoRedo;

        // Create a SerializedObject to allow Unity to serialize fields properly in the inspector.
        serializedObject = new SerializedObject(this);

        // Get all cultures supported by .NET and their display names for the dropdown list.
        cultures = CultureInfo.GetCultures(CultureTypes.AllCultures);
        cultureNames = cultures.Select(c => c.DisplayName).ToArray();

        // Load trash icon used in UI for removing items.
        trashImage = FindTextureByName("Trash Icon");

        // Initialize file paths for language assets.
        assetsPath = GetLanguageAssetsPath();
        canvasDataFile = $"{assetsPath}/CanvasData.tsv";
        languageDataFile = $"{assetsPath}/LanguageData.tsv";
        metaDataFile = $"{assetsPath}/MetaData.tsv";

        // Ensure all files exist and are valid; create default data if necessary.
        if (IsInvalidTSV(canvasDataFile)) SaveTableFile(canvasDataFile, CreateLanguageDataBase());
        if (IsInvalidTSV(languageDataFile)) SaveTableFile(languageDataFile, CreateLanguageDataBase());
        if (IsInvalidTSV(metaDataFile)) SaveTableFile(metaDataFile, CreateLanguageDataBase());
    }

    /// <summary>
    /// Cleans up event subscriptions and saves data when the window is destroyed.
    /// </summary>
    private void OnDestroy()
    {
        Undo.undoRedoPerformed -= OnUndoRedo;
        SaveDataJson();
    }

    /// <summary>Repaints the window when an undo or redo action occurs.</summary>
    private void OnUndoRedo() => Repaint();

    /// <summary>
    /// Draws the main GUI elements of the editor window.
    /// </summary>
    private void OnGUI()
    {
        FindDuplicateIds(); // Check for and highlight duplicate IDs in the saved data.

        // Initialize data only once when the window is first opened.
        if (!firstTime)
        {
            // Add any missing default language components.
            AddDefaultLanguageComponents(ref componentSave, ref canvasSave);
            LoadDataJson(); // Load any previously saved component data from JSON file.
            firstTime = true; // Prevent this block from running again.
        }

        serializedObject.Update(); // Begin update for serialized fields.
        scrollPosition = GUILayout.BeginScrollView(scrollPosition); // Begin scrollable view for editor window content.

        // Draw title for editor settings.
        LabelField("Editor Settings", CreateLabelStyle(15, true));
        Space(10);
        DrawActionButtons(); // Draw buttons for common editor actions (save, load, open folder, etc.).
        Space(20);
        DrawLanguageDropdown(); // Draw dropdown to select the editing language.
        Space(20);

        // If there is any component data, show the ID display panel inside a colored box.
        if (componentSave.Count > 0)
        {
            DrawColoredBox(() =>
            {
                Space(10);
                DrawIdDisplayPanel();
                Space(10);
            }, new Color(0, 0, 0, 0.15f)); // Semi-transparent black.
        }

        // Draw title for feature lists.
        Space(25);
        LabelField("Feature Lists", CreateLabelStyle(15, true));
        Space(5);

        // Toggle checkboxes to show/hide Text or Canvas data.
        BeginHorizontal();
        Space(60);
        showTextData = ToggleLeft("Show Text Component Data", showTextData);
        Space(10);
        showCanvasData = ToggleLeft("Show Canvas Component Data", showCanvasData);
        GUILayout.FlexibleSpace();
        EndHorizontal();

        Space(-30); // Adjust spacing before showing property lists.

        // Show serialized property list of text components if enabled.
        if (showTextData) PropertyField(serializedObject.FindProperty("componentSave"), true, GUILayout.Width(550));

        // Show serialized property list of canvas components if enabled.
        if (showCanvasData) PropertyField(serializedObject.FindProperty("canvasSave"), true, GUILayout.Width(550));

        GUILayout.EndScrollView(); // End scroll view for the editor layout.

        serializedObject.ApplyModifiedProperties(); // Apply any modified properties in the serialized object.
    }

    /// <summary>
    /// Draws buttons for saving, loading, opening folder, rebuilding canvas, organizing items, and Search by ID.
    /// </summary>
    private void DrawActionButtons()
    {
        BeginHorizontal();
        Space(20);
        DrawButton("Open Folder", () => Application.OpenURL(assetsPath)); // Button to open the folder containing the TSV files.
        Space(10);
        GUI.backgroundColor = fileIsSaved ? Color.white : Color.yellow; // Highlight the Save button if there are unsaved changes.
        DrawButton("Save File", SaveLanguageFile); // Button to save current language and canvas data to file.
        GUI.backgroundColor = Color.white; // Reset background color after drawing Save button.
        Space(10);
        DrawButton("Load File", LoadLanguageFile); // Button to load language and canvas data from file.
        GUILayout.FlexibleSpace();
        EndHorizontal();

        Space(5);

        BeginHorizontal();
        Space(20);
        // Button to trigger canvas rebuild process by adding the RebuildCanvas component.
        DrawButton("Rebuild Canvas", () =>
        {
            SaveDataJson(); // Save current state before rebuild.

            // Create a new GameObject for rebuild and register with undo system.
            var rebuildCanvas = new GameObject("[Rebuild Canvas]");
            Undo.RegisterCreatedObjectUndo(rebuildCanvas, "Create Rebuild Canvas");

            // Add the component responsible for rebuilding canvas structure.
            Undo.AddComponent<RebuildCanvas>(rebuildCanvas);
        });
        Space(10);
        // Button to organize all language and canvas items (e.g., sort and compare IDs).
        DrawButton("Organize All Items", () =>
        {
            Undo.RecordObject(this, "Organize All Items");

            CompareID(); // Run comparison logic to organize items.

            // Mark the file as needing save and flag this object as dirty.
            fileIsSaved = false;
            EditorUtility.SetDirty(this);
        });
        Space(10);
        DrawButton($"Search by ID: {idIndex}", () => SearchByID(idIndex)); // Button to trigger ID-based search and highlight in the editor.
        GUILayout.FlexibleSpace();
        EndHorizontal();
    }

    /// <summary>
    /// Draws the language selection dropdown for the computer's language.
    /// </summary>
    private void DrawLanguageDropdown()
    {
        BeginHorizontal();

        // Label for dropdown.
        LabelField("Language for Editing:", GUILayout.Width(130));

        string previousLanguage = languageForEditing;

        // Get the index of the currently selected language in the culture list.
        selectedCultureIndex = Array.FindIndex(cultures, c => c.Name == previousLanguage);
        if (selectedCultureIndex < 0) selectedCultureIndex = 0;

        // Show popup with all available cultures.
        int newSelectedCultureIndex = Popup(selectedCultureIndex, cultureNames, GUILayout.Width(150));

        // If a new culture was selected, update internal state and optionally load its file.
        string newLanguage = cultures[newSelectedCultureIndex].Name;
        if (newLanguage != previousLanguage)
        {
            Undo.RecordObject(this, "Change Computer Language");
            languageForEditing = newLanguage;
            fileIsSaved = false;
            EditorUtility.SetDirty(this);

            var match = availableLanguages.FirstOrDefault(lang => lang.culture == newLanguage && lang.isAvailable);
            if (match != null && EditorUtility.DisplayDialog(
                    "Load Language File",
                    $"The language '{match.name}' is available.\nDo you want to load the language?",
                    "Yes",
                    "No"))
            {
                LoadLanguageFile();
            }
        }

        // Display selected culture and system default culture.
        Space(5);
        GUILayout.Label($"({languageForEditing})");
        Space(10);
        GUILayout.Label($"You: '{CultureInfo.InstalledUICulture.DisplayName}'");

        GUILayout.FlexibleSpace();
        EndHorizontal();
    }

    /// <summary>
    /// Displays the panel for navigating and viewing current selected ID, and shows duplicates if any.
    /// </summary>
    private void DrawIdDisplayPanel()
    {
        // Draw centered header label.
        BeginHorizontal();
        GUILayout.FlexibleSpace();
        LabelField("ID Display", CreateLabelStyle(15, true, true));
        GUILayout.FlexibleSpace();
        EndHorizontal();

        Space(5);

        if (duplicateID.Count == 0)
        {
            // Determine the ID range from the saved components.
            int minID = componentSave.Count > 0 ? componentSave.Min(cs => cs.iD) : 0;
            int maxID = componentSave.Count > 0 ? componentSave.Max(cs => cs.iD) : 0;

            // Show current index range in center.
            BeginHorizontal();
            GUILayout.FlexibleSpace();
            LabelField($"{minID}/{maxID}", CreateLabelStyle(13, true, true));
            GUILayout.FlexibleSpace();
            EndHorizontal();

            // Navigation panel for ID selection.
            BeginHorizontal();
            GUILayout.FlexibleSpace();

            // Backward arrow to decrement ID index.
            DrawArrowButton("<--", ref idIndex, minID, maxID, this, () => idIndex--);

            Space(10);

            // Manual ID index entry field.
            int newIdIndex = IntField(idIndex, new GUIStyle(EditorStyles.textField) { alignment = TextAnchor.MiddleCenter }, GUILayout.Width(50));
            if (newIdIndex != idIndex)
            {
                Undo.RecordObject(this, "Change ID Index");
                idIndex = newIdIndex;
                fileIsSaved = false;
                EditorUtility.SetDirty(this);
            }

            Space(10);

            // Forward arrow to increment ID index.
            DrawArrowButton("-->", ref idIndex, minID, maxID, this, () => idIndex++);
            GUILayout.FlexibleSpace();
            EndHorizontal();
        }
        else
        {
            // Show list of duplicated IDs in red.
            Space(10);
            GUI.color = Color.red;
            GUILayout.Label($"Duplicated ID: {string.Join(", ", duplicateID)}", CreateLabelStyle(15, true, true));
            GUI.color = Color.white;
        }

        Space(10);

        bool itemRemoved = false;
        LanguageForEditingSave itemToRemove = null;

        // Draw main panel to display or remove components.
        DrawColoredBox(() =>
        {
            if (duplicateID.Count > 0)
            {
                // Display all items with duplicate IDs, each with a delete button.
                foreach (var id in duplicateID)
                {
                    var items = componentSave.Where(cs => cs.iD == id).ToList();
                    foreach (var item in items)
                    {
                        RenderComponentDisplay(item);

                        BeginHorizontal();
                        GUILayout.FlexibleSpace();

                        // Draw delete button for this item.
                        GUI.backgroundColor = Color.red;
                        if (GUILayout.Button(new GUIContent(trashImage), GUILayout.Width(30), GUILayout.Height(30)))
                        {
                            itemToRemove = item;
                            itemRemoved = true;
                            GUI.FocusControl(null);
                        }
                        GUI.backgroundColor = Color.white;
                        EndHorizontal();

                        Space(5);

                        // Small visual separator.
                        DrawColoredBox(() => { Space(1); }, new Color(1, 1, 1, 1));
                    }
                }
            }
            else
            {
                // Display the currently selected item if no duplicates.
                var item = componentSave.FirstOrDefault(cs => cs.iD == idIndex);
                if (item != null)
                {
                    RenderComponentDisplay(item);
                }
                else
                {
                    // Indicate empty slot for this ID.
                    LabelField("ID Slot Available", CreateLabelStyle(18, true, true));
                }
            }
        }, new(0, 0, 1, 0.5f)); // Blueish background.

        // If an item was flagged for removal, perform cleanup.
        if (itemRemoved && itemToRemove != null)
        {
            Undo.RecordObject(this, "Remove Duplicate ID");
            componentSave.Remove(itemToRemove);
            fileIsSaved = false;
            EditorUtility.SetDirty(this);
            FindDuplicateIds(); // Re-scan for remaining duplicates.
        }
    }

    /// <summary>
    /// Renders the UI fields for editing a language component's properties.
    /// </summary>
    /// <param name="component">The language component to display and edit.</param>
    private void RenderComponentDisplay(LanguageForEditingSave component)
    {
        Space(10);
        if (component == null) return;

        BeginHorizontal();

        // Display and allow editing of the ID field.
        int previousInt = component.iD;
        previousInt = DrawLabeledIntField("ID:", previousInt, 20, 50);
        if (component.iD != previousInt)
        {
            Undo.RecordObject(this, "Change item.iD");
            component.iD = previousInt;
            fileIsSaved = false;
            EditorUtility.SetDirty(this);
        }

        Space(5);

        // Display and allow editing of the text context field.
        string previousTextContext = component.textContext;
        previousTextContext = DrawLabeledTextField("Text Context:", previousTextContext, 80, 305);
        if (component.textContext != previousTextContext)
        {
            Undo.RecordObject(this, "Change item.textContext");
            component.textContext = previousTextContext;
            fileIsSaved = false;
            EditorUtility.SetDirty(this);
        }

        GUILayout.FlexibleSpace();

        // Delete button (disabled if there are duplicates).
        GUI.enabled = duplicateID.Count == 0;
        GUI.backgroundColor = Color.red;
        if (GUILayout.Button(new GUIContent(trashImage), GUILayout.Width(30f), GUILayout.Height(30f)))
        {
            RemoveID();
        }
        GUI.backgroundColor = Color.white;
        GUI.enabled = true;

        EndHorizontal();

        DisplayComponentIcon(component.componentType); // Show the component type icon.

        Space(10);

        // Check which fields to show based on component type.
        int[] validTypes3 = { 1, 3, 6 };
        int[] validTypes1 = { 4, 5 };
        bool valid3 = Array.Exists(validTypes3, type => type == component.componentType) || component.componentType >= 7;
        bool valid2 = component.componentType == 2;
        bool valid1 = Array.Exists(validTypes1, type => type == component.componentType);

        // If this component type supports it, display editable text field.
        if (valid3 || valid2 || valid1)
        {
            string previousText = component.text;
            previousText = DrawLabeledTextField("Text:", previousText, 40, 430);
            if (component.text != previousText)
            {
                Undo.RecordObject(this, "Change item.text");
                component.text = previousText;
                fileIsSaved = false;
                EditorUtility.SetDirty(this);
            }
        }

        Space(10);
        BeginHorizontal();

        // Alignment field if applicable.
        if (valid3)
        {
            int previousAlignment = component.alignment;
            previousAlignment = DrawLabeledIntField("Alignment:", previousAlignment, 70, 50);
            if (component.alignment != previousAlignment)
            {
                Undo.RecordObject(this, "Change item.alignment");
                component.alignment = previousAlignment;
                fileIsSaved = false;
                EditorUtility.SetDirty(this);
            }
        }

        // Font size field if applicable.
        if (valid3 || valid2)
        {
            int previousIndex = component.fontSize;
            previousIndex = DrawLabeledIntField("Font Size:", previousIndex, 70, 50);
            if (component.fontSize != previousIndex)
            {
                Undo.RecordObject(this, "Change item.fontSize");
                component.fontSize = previousIndex;
                fileIsSaved = false;
                EditorUtility.SetDirty(this);
            }
        }

        // Font index field if applicable.
        if (valid3 || valid2)
        {
            int previousFont = component.fontListIndex;
            previousFont = DrawLabeledIntField("Font Index:", previousFont, 70, 50);
            if (component.fontListIndex != previousFont)
            {
                Undo.RecordObject(this, "Change item.fontListIndex");
                component.fontListIndex = previousFont;
                fileIsSaved = false;
                EditorUtility.SetDirty(this);
            }
        }

        // Visual spacing compensation for alignment.
        if (!(valid3 || valid2)) Space(22);
        if (!(valid3 || valid2 || valid1)) Space(44);

        GUILayout.FlexibleSpace();
        EndHorizontal();

        Space(10);
    }

    /// <summary>
    /// Draws a button with the specified label and executes the provided action on click.
    /// </summary>
    /// <param name="label">The button's label text.</param>
    /// <param name="action">The action to perform when clicked.</param>
    private void DrawButton(string label, Action action)
    {
        // Draw button with custom style.
        if (GUILayout.Button(label, CreateCustomButtonStyle(15), GUILayout.Width(160), GUILayout.Height(30)))
        {
            action(); // Execute provided action on click.
            GUI.FocusControl(null); // Remove focus to avoid accidental repeats.
        }
    }

    /// <summary>
    /// Sorts componentSave and canvasSave lists by their IDs in ascending order.
    /// </summary>
    private void CompareID()
    {
        componentSave.Sort((a, b) => a.iD.CompareTo(b.iD)); // Sort language components by their ID.
        canvasSave.Sort((a, b) => a.canvasID.CompareTo(b.canvasID)); // Sort canvas components by their canvas ID.
    }

    /// <summary>
    /// Identifies and stores duplicate IDs in componentSave and canvasSave.
    /// </summary>
    private void FindDuplicateIds()
    {
        duplicateID = FindDuplicateIDs(componentSave.Select(cs => cs.iD).ToArray()); // Get duplicate IDs from the language components.
        canvasID = FindDuplicateIDs(canvasSave.Select(cs => cs.canvasID).ToArray()); // Get duplicate canvas IDs.
    }

    /// <summary>
    /// Removes the component with the current idIndex, optionally confirming with the user.
    /// </summary>
    private void RemoveID()
    {
        Undo.RecordObject(this, "Remove ID");

        // Find the component to remove based on current idIndex.
        var itemToRemove = componentSave.FirstOrDefault(cs => cs.iD == idIndex);
        if (itemToRemove == null)
        {
            // Show message if no matching ID is found.
            EditorUtility.DisplayDialog("ID not found", $"There is no component with ID '{idIndex}' to remove.", "Ok");
            return;
        }

        // Check if confirmation dialog should be skipped.
        bool skipConfirmation = EditorPrefs.GetBool("RemoveID_SkipConfirmation", false);
        bool shouldRemove = true;

        if (!skipConfirmation)
        {
            // Prompt the user for confirmation before deletion.
            int result = EditorUtility.DisplayDialogComplex(
                "Remove ID?",
                $"Do you really want to remove the ID '{idIndex}'?",
                "Yes",
                "No",
                "Yes, and don't ask again"
            );

            if (result == 1) shouldRemove = false;
            else if (result == 2) EditorPrefs.SetBool("RemoveID_SkipConfirmation", true);
        }

        if (!shouldRemove) return;

        // Perform removal and mark the file as dirty for saving.
        componentSave.Remove(itemToRemove);
        fileIsSaved = false;
        EditorUtility.SetDirty(this);

        // Update idIndex to nearest available ID, or reset if none left.
        if (componentSave.Count > 0)
        {
            idIndex = componentSave.Select(cs => cs.iD).OrderBy(id => Math.Abs(id - idIndex)).First();
        }
        else
        {
            idIndex = 0;
        }
    }

    /// <summary>
    /// Saves the language files (.tsv) by syncing IDs and data, confirming overwrites if necessary.
    /// </summary>
    private void SaveLanguageFile()
    {
        Undo.RecordObject(this, "Save File");
        CompareID(); // Sort components before saving.
        EditorUtility.SetDirty(this);
        FindDuplicateIds(); // Ensure no duplicates exist before proceeding.
        SaveDataJson();

        // Abort save if any duplicate IDs are detected.
        if (duplicateID.Count > 0)
        {
            Debug.LogError($"Language Save Editor: Unable to save due to duplicate IDs: {string.Join(", ", duplicateID)}");
            return;
        }
        if (canvasID.Count > 0)
        {
            Debug.LogError($"Language Save Editor: Unable to save due to duplicate canvasIDs: {string.Join(", ", canvasID)}");
            return;
        }

        // Ask the user to confirm overwriting existing files.
        if (!IsInvalidTSV(canvasDataFile) || !IsInvalidTSV(languageDataFile) || !IsInvalidTSV(metaDataFile))
        {
            if (!EditorUtility.DisplayDialog("Replace files?", "The '.tsv' files already exist. Do you want to replace them?", "Yes", "No"))
            {
                return;
            }
        }

        // Load current TSV files into memory.
        VerticalTable[] canvasTable = null, languageDataTable = null, metaTable = null;
        LoadTableFile(canvasDataFile, ref canvasTable);
        LoadTableFile(languageDataFile, ref languageDataTable);
        LoadTableFile(metaDataFile, ref metaTable);

        // Collect supported languages and locate the selected language's index.
        GetAllavailableLanguages(languageDataTable);
        int columnIndex = availableLanguages.Find(lang => lang.culture == languageForEditing)?.columnIndex ?? -1;

        // Build consistent headers based on the selected language and languages available.
        var generatedHeader = BuildTableFromAvailableLanguages(availableLanguages, columnIndex);

        // Replace top header rows in the loaded tables.
        var canvasRebuild = ReplaceTopRows(canvasTable, generatedHeader);
        var languageDataRebuild = ReplaceTopRows(languageDataTable, generatedHeader);
        var metaRebuild = ReplaceTopRows(metaTable, generatedHeader);

        // Create data rows from internal data structures.
        var canvasIds = canvasSave.Select(c => new ManagerLanguageIdData { iD = c.canvasID, text = c.json, textContext = c.textContext }).ToList();
        var languageIds = componentSave.Select(c => new ManagerLanguageIdData { iD = c.iD, text = c.text, textContext = c.textContext }).ToList();

        var metaIds = componentSave.Select(c => new ManagerLanguageIdData
        {
            iD = c.iD,
            text = JsonUtility.ToJson(new IdMetaData
            {
                iD = c.iD,
                alignment = c.alignment,
                fontSize = c.fontSize,
                fontListIndex = c.fontListIndex,
                componentType = c.componentType
            }),
            textContext = c.textContext
        }).ToList();

        // Sync data rows into their respective tables.
        SyncTableWithIds(ref canvasRebuild, canvasIds);
        SyncTableWithIds(ref languageDataRebuild, languageIds);
        SyncTableWithIds(ref metaRebuild, metaIds);

        // Insert additional data columns into tables based on the current language.
        InsertIDsEditor(ref canvasRebuild, canvasIds, availableLanguages, languageForEditing);
        InsertIDsEditor(ref languageDataRebuild, languageIds, availableLanguages, languageForEditing);
        InsertIDsEditor(ref metaRebuild, metaIds, availableLanguages, languageForEditing);

        // Save the final versions to their respective files.
        SaveTableFile(canvasDataFile, canvasRebuild);
        SaveTableFile(languageDataFile, languageDataRebuild);
        SaveTableFile(metaDataFile, metaRebuild);

        Debug.Log($"Saved Files in: {assetsPath}");
        fileIsSaved = true;
    }

    /// <summary>
    /// Loads language files (.tsv) and updates internal data structures accordingly.
    /// </summary>
    private void LoadLanguageFile()
    {
        // Validate directory existence.
        if (!Directory.Exists(assetsPath))
        {
            Debug.LogError($"Folder path does not exist: {assetsPath}");
            return;
        }

        // Validate each TSV file and log individual errors.
        if (IsInvalidTSV(canvasDataFile)) Debug.LogError($"Invalid canvasDataFile: {canvasDataFile}");
        if (IsInvalidTSV(languageDataFile)) Debug.LogError($"Invalid languageDataFile: {languageDataFile}");
        if (IsInvalidTSV(metaDataFile)) Debug.LogError($"Invalid metaDataFile: {metaDataFile}");

        // Abort if any file is invalid.
        if (IsInvalidTSV(canvasDataFile) || IsInvalidTSV(languageDataFile) || IsInvalidTSV(metaDataFile)) return;

        try
        {
            // Load data from TSV files into tables.
            VerticalTable[] canvasTable = null, languageDataTable = null, metaTable = null;
            LoadTableFile(canvasDataFile, ref canvasTable);
            LoadTableFile(languageDataFile, ref languageDataTable);
            LoadTableFile(metaDataFile, ref metaTable);

            Undo.RecordObject(this, "Load File");

            // Extract available languages from the table.
            GetAllavailableLanguages(languageDataTable);

            componentSave.Clear();
            canvasSave.Clear();

            // Parse and store metadata entries.
            var metaData = new List<IdMetaData>();
            var idData = ExtractIDsEditor(languageDataTable, availableLanguages, languageForEditing);
            var idMetaData = ExtractIDs(metaTable, availableLanguages, languageForEditing);
            var idCanvasData = ExtractIDsEditor(canvasTable, availableLanguages, languageForEditing);

            foreach (var id in idMetaData)
            {
                try
                {
                    // Try parsing metadata JSON for each ID.
                    var dataJson = JsonUtility.FromJson<IdMetaData>(id.text);
                    metaData.Add(new()
                    {
                        iD = id.iD,
                        alignment = dataJson.alignment,
                        fontSize = dataJson.fontSize,
                        fontListIndex = dataJson.fontListIndex,
                        componentType = dataJson.componentType
                    });
                }
                catch
                {
                    // Log warning and add default metadata if parsing fails.
                    Debug.LogWarning($"Failed to parse JSON for ID {id.iD}");
                    metaData.Add(new() { iD = id.iD, alignment = 0, fontSize = 0, fontListIndex = 0, componentType = 7 });
                }
            }

            // Merge parsed metadata with corresponding component data.
            foreach (var data in idData.Where(d => d != null))
            {
                var meta = metaData.FirstOrDefault(m => m.iD == data.iD);
                componentSave.Add(new()
                {
                    iD = data.iD,
                    text = data.text,
                    textContext = data.textContext,
                    alignment = meta?.alignment ?? 0,
                    fontSize = meta?.fontSize ?? 0,
                    fontListIndex = meta?.fontListIndex ?? 0,
                    componentType = meta?.componentType ?? 7
                });
            }

            // Populate canvas data from extracted TSV records.
            foreach (var canvasData in idCanvasData.Where(d => d != null))
            {
                canvasSave.Add(new() { canvasID = canvasData.iD, textContext = canvasData.textContext, json = canvasData.text });
            }

            CompareID();
            fileIsSaved = true;
            EditorUtility.SetDirty(this);
        }
        catch (Exception e)
        {
            Debug.LogError($"Error while loading file: {e.Message}");
        }
    }

    /// <summary>
    /// Populates availableLanguages from the provided language data table.
    /// </summary>
    /// <param name="languageDataTable">The table containing language data.</param>
    private void GetAllavailableLanguages(VerticalTable[] languageDataTable)
    {
        // Ensure the table is valid.
        if (languageDataTable == null || languageDataTable.Length == 0)
        {
            Debug.LogError("Language data table is null or empty.");
            return;
        }

        availableLanguages.Clear();

        // Count rows and columns in the table.
        RowAndColumnCounter(languageDataTable, out int rowCount, out int columnCount);
        if (rowCount < 3 || columnCount < 3)
        {
            Debug.LogWarning("The table does not have enough rows or columns to extract language data.");
            return;
        }

        // Loop through each language column and parse culture, name, and availability.
        for (int col = 2; col < columnCount; col++)
        {
            var culture = GetText(languageDataTable, 0, col);
            var name = GetText(languageDataTable, 1, col);
            var isAvailableText = GetText(languageDataTable, 2, col);

            bool isAvailable = bool.TryParse(isAvailableText, out bool result) && result;

            availableLanguages.Add(new() { culture = culture, name = name, isAvailable = isAvailable, columnIndex = col });
        }
    }

    /// <summary>
    /// Saves the current editor data to a JSON file.
    /// </summary>
    public void SaveDataJson()
    {
        // Prepare data for serialization.
        LanguageFileManagerWindowData data = new()
        {
            languageForEditing = languageForEditing,
            componentSave = componentSave,
            canvasSave = canvasSave,
            availableLanguages = availableLanguages,
            firstTime = firstTime,
            idIndex = idIndex,
            showTextData = showTextData,
            showCanvasData = showCanvasData,
            fileIsSaved = fileIsSaved
        };

        try
        {
            File.WriteAllText(fileData, JsonUtility.ToJson(data)); // Write serialized data to disk.
        }
        catch (Exception e)
        {
            Debug.LogError($"Error while saving data: {e.Message}");
        }
    }

    /// <summary>
    /// Loads editor data from a JSON file.
    /// </summary>
    private void LoadDataJson()
    {
        // Skip if file doesn't exist.
        if (!File.Exists(fileData)) return;

        var data = JsonUtility.FromJson<LanguageFileManagerWindowData>(File.ReadAllText(fileData));

        // Restore internal state from deserialized data.
        languageForEditing = data.languageForEditing;

        componentSave.Clear();
        componentSave = data.componentSave;

        canvasSave.Clear();
        canvasSave = data.canvasSave;

        availableLanguages.Clear();
        availableLanguages = data.availableLanguages;

        firstTime = data.firstTime;
        idIndex = data.idIndex;
        showTextData = data.showTextData;
        showCanvasData = data.showCanvasData;
        fileIsSaved = data.fileIsSaved;
    }

    /// <summary>
    /// Adds or updates a component in the save list and updates the saved data.
    /// </summary>
    /// <param name="data">The component data to add or update.</param>
    public void AddComponent(LanguageForEditingSave data)
    {
        Undo.RecordObject(this, "Add Component");

        // Preserve current state before modification.
        SaveDataJson();
        LoadDataJson();

        // Check if component already exists.
        var existingComponent = componentSave.FirstOrDefault(c => c.iD == data.iD);

        if (existingComponent != null)
        {
            // Update existing component while preserving its textContext.
            int index = componentSave.IndexOf(existingComponent);
            data.textContext = componentSave[index].textContext;
            componentSave[index] = data;
        }
        else
        {
            componentSave.Add(data); // Add new component to list.
        }

        // Sort components and mark as unsaved.
        componentSave.Sort((a, b) => a.iD.CompareTo(b.iD));
        idIndex = data.iD;
        fileIsSaved = false;

        // Save and mark the object dirty for Unity's serialization.
        SaveDataJson();
        EditorUtility.SetDirty(this);
    }

    /// <summary>
    /// Adds or updates a canvas in the save list and updates the saved data.
    /// </summary>
    /// <param name="data">The canvas data to add or update.</param>
    public void AddCanvas(CanvasForEditingSave data)
    {
        Undo.RecordObject(this, "Add Canvas");

        // Preserve current state before modification.
        SaveDataJson();
        LoadDataJson();

        // Check if canvas already exists.
        var existingComponent = canvasSave.FirstOrDefault(c => c.canvasID == data.canvasID);

        if (existingComponent != null)
        {
            // Update existing canvas while preserving its textContext.
            int index = canvasSave.IndexOf(existingComponent);
            data.textContext = canvasSave[index].textContext;
            canvasSave[index] = data;
        }
        else
        {
            canvasSave.Add(data); // Add new canvas to list.
        }

        // Sort canvases and mark data as modified.
        canvasSave.Sort((a, b) => a.canvasID.CompareTo(b.canvasID));
        fileIsSaved = false;

        // Save and mark the object dirty for Unity.
        SaveDataJson();
        EditorUtility.SetDirty(this);
    }
}
#endif