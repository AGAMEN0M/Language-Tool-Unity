#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Globalization;
using System.Linq;
using System;

// Main class that extends EditorWindow to create the edit window.
public class LanguageSaveEditorWindow : EditorWindow
{
    // Variables for storing UI data.
    public string savePath = "/StreamingAssets/Language/";
    public string fileName = "ENGLISH";
    public string languageName = "ENGLISH";
    public string computerLanguage = "English (United States)";

    // Lists for storing groups, text IDs, and comment IDs.
    public List<LanguageSaveGroups> groupsID = new();
    public List<LanguageSaveID> textID = new();
    public List<LanguageSaveComments> commentsID = new();

    // UI Control Variables.
    public Vector2 scrollPosition = Vector2.zero;
    public bool showDrawing = true;
    public bool firstTime = false;

    // Serialized object to work with the GUI.
    private SerializedObject serializedObject;
    private string systemLanguage;
    private string[] availableLanguages;
    private int selectedLanguageIndex = 0;
    private Dictionary<float, List<string>> duplicateIDs;

    // Menu function to show edit window.
    [MenuItem("Window/Language/Language Save Editor", false, 2031)]
    public static void ShowWindow()
    {
        LanguageSaveEditorWindow window = GetWindow<LanguageSaveEditorWindow>("Language Save Editor"); // Gets an instance of the LanguageSaveEditorWindow window and sets the custom icon.
        Texture2D icon = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Language/Editor/Icon/Editor Screens/LanguageSaveEditorWindow Icon.png"); // Loads the custom icon located in the specified path.
        window.titleContent = new GUIContent("Language Save Editor", icon); // Defines the window title and icon.
    }

    // Method called when the window is opened.
    private void OnEnable()
    {
        serializedObject = new SerializedObject(this); // Creates a new serialized object for the current window.
    }

    // Method called when window is closed.
    private void OnDestroy()
    {
        SaveDataJson(); // save the information.

        // Close the Language Save Editor Settings window if it's open.
        LanguageSaveEditorSettingsWindow editorWindow = EditorWindow.GetWindow<LanguageSaveEditorSettingsWindow>("Settings");
        editorWindow.Close();
    }

    // Method called to render the window interface.
    private void OnGUI()
    {
        duplicateIDs = LanguageClassToolsEditor.GetDuplicateIDs(groupsID, textID, commentsID); // Get a dictionary of duplicate IDs.

        // Set the default settings the first time the method runs and load the saved file if it exits.
        if (firstTime == false)
        {
            LanguageClassToolsEditor.DefaultSettings(ref groupsID, ref textID, ref commentsID);
            LoadDataJson();
            firstTime = true;
        }

        scrollPosition = GUILayout.BeginScrollView(scrollPosition); // Sets the scroll position for the window view.

        Tools(); // Render tools and settings section.

        Notice(); // Display a warning if there are duplicate IDs.

        GUILayout.Space(20);
        Button(); // Render action buttons and collect user interactions.

        // Button to show or hide the design section.
        showDrawing = GUILayout.Toggle(showDrawing, "Show Drawing");
        if (showDrawing == true)
        {
            Design(); // Render the design section with data details.
        }

        GUILayout.Space(40);
        ListInspector(); // Render the watch section of the data lists.

        GUILayout.EndScrollView(); // End the scrolling area of the window view.
    }

    // Renders the configuration tools.
    private void Tools()
    {
        GUILayout.Label("Tools", EditorStyles.boldLabel); // Display the "Tools" label in bold.

        GUILayout.Space(10);
        GUILayout.Label("Save Folder in dataPath");
        savePath = GUILayout.TextField(savePath); // Text field to define the save folder path.

        GUILayout.Space(5);
        GUILayout.Label("TXT File Name");
        fileName = GUILayout.TextField(fileName); // Text field to define the name of the text file.

        GUILayout.Space(5);
        GUILayout.Label("Language Name");
        languageName = GUILayout.TextField(languageName); // Text field to define the name of the language.

        GUILayout.Space(5);
        GUILayout.Label("Computer Language");
        computerLanguage = GUILayout.TextField(computerLanguage); // Text field to define the computer language.

        // Get all computer languages.
        CultureInfo[] cultures = CultureInfo.GetCultures(CultureTypes.AllCultures);
        availableLanguages = new string[cultures.Length];

        for (int i = 0; i < cultures.Length; i++)
        {
            availableLanguages[i] = cultures[i].DisplayName; // Populate availableLanguages with display names of all cultures.
        }

        Array.Sort(availableLanguages); // Sort the available languages alphabetically.

        GUILayout.Space(10);
        GUILayout.BeginHorizontal();
        selectedLanguageIndex = EditorGUILayout.Popup("Select Language", selectedLanguageIndex, availableLanguages, GUILayout.Width(500)); // Dropdown to select the language.
        GUILayout.Space(20);
        GUILayout.Label($"System Recommended: {systemLanguage}", GUILayout.Width(400)); // Display system recommended language.

        // When the user clicks on "Apply Computer Language".
        if (GUILayout.Button("Apply Computer Language"))
        {
            // Display a confirmation dialog to the user with three buttons.
            int result = EditorUtility.DisplayDialogComplex("Confirmation", "Are you sure?", "Cancel", "System Recommended", "Selected Language");

            if (result == 1)
            {                
                computerLanguage = systemLanguage; // User clicked "Set systemLanguage to computerLanguage".
            }
            else if (result == 2)
            {
                // User clicked "Set selectedLanguageIndex to computerLanguage".
                if (selectedLanguageIndex >= 0 && selectedLanguageIndex < availableLanguages.Length)
                {
                    computerLanguage = availableLanguages[selectedLanguageIndex];
                }
                else
                {
                    Debug.LogError("Invalid language selection.");
                }
            }
        }
        GUILayout.EndHorizontal();

        CultureInfo cultureInfo = CultureInfo.InstalledUICulture; // Gets and displays the language recommended by the system.
        systemLanguage = cultureInfo.DisplayName; // Display system recommended language in UI.
    }

    // Displays a warning if there are duplicate IDs.
    private void Notice()
    {
        // Checks for duplicate IDs.
        if (duplicateIDs.Count > 0)
        {
            GUILayout.Space(10);
            GUILayout.Label("Duplicate IDs:", EditorStyles.boldLabel); // Display label for duplicate IDs.

            // Creates a label style with red color.
            GUIStyle redLabelStyle = new(EditorStyles.label);
            redLabelStyle.normal.textColor = Color.red;

            // Iterates through the duplicate IDs and their occurrences.
            foreach (var kvp in duplicateIDs)
            {
                GUILayout.Label($"ID: {kvp.Key} Duplicated in: {string.Join(", ", kvp.Value)}", redLabelStyle); // Display the duplicate ID and related occurrences in red color.
            }
        }
    }

    // Renders the buttons at the bottom of the window.
    private void Button()
    {
        GUILayout.BeginHorizontal();

        GUI.enabled = duplicateIDs.Count == 0; // Disable the save button if there are duplicate IDs.

        // Renders the "Save" button.
        if (GUILayout.Button("Save"))
        {
            SaveTXTFile(); // Calls the method to save the TXT file.
            SaveDataJson(); // Calls the method to save the data.
        }

        GUI.enabled = true; // Re-enable GUI state to allow interactions with other elements.

        // Renders the "Open Folder" button to open the destination folder.
        if (GUILayout.Button("Open Folder"))
        {
            OpenFolder(); // Calls the method to open the folder.
        }

        // Renders the "Organize All Items" button to sort items by ID.
        if (GUILayout.Button("Organize All Items"))
        {
            SortByID(); // Calls the method to sort the items by ID.
        }

        // Renders a button to open the settings window.
        if (GUILayout.Button("Settings"))
        {
            LanguageSaveEditorSettingsWindow window = GetWindow<LanguageSaveEditorSettingsWindow>("Settings"); // Gets an instance of the LanguageSaveEditorSettingsWindow window and sets the custom icon.
            Texture2D icon = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Language/Editor/Icon/Editor Screens/LanguageSaveEditorSettingsWindow Icon.png"); // Loads the custom icon located in the specified path.
            window.titleContent = new GUIContent("Settings", icon); // Defines the window title and icon.
        }

        GUILayout.EndHorizontal();
    }

    // Method to sort data lists by ID.
    private void SortByID()
    {
        textID.Sort((a, b) => a.ID.CompareTo(b.ID)); // Sorts the list of texts by ID value using a comparison between IDs.
        groupsID.Sort((a, b) => a.textID.CompareTo(b.textID)); // Sorts the list of groups by the TextID value using a comparison between TextIDs.
        commentsID.Sort((a, b) => a.comments.CompareTo(b.comments)); // Sorts the list of comments by the value of comments using a comparison between comments.
    }

    // Renders the watch section of the data lists.
    private void ListInspector()
    {
        serializedObject.Update(); // Updates the serialized object to reflect property changes.

        GUILayout.Label("List Data", EditorStyles.boldLabel);
        GUILayout.Label("Language Groups IDs", EditorStyles.boldLabel);

        SerializedProperty groupsIDProperty = serializedObject.FindProperty("groupsID"); // Finds the "GroupsID" property on the serialized object.
        EditorGUILayout.PropertyField(groupsIDProperty, true); // Renders the "GroupsID" property with all of its child elements.

        GUILayout.Space(10);
        GUILayout.Label("Language Text IDs", EditorStyles.boldLabel);

        SerializedProperty textIDProperty = serializedObject.FindProperty("textID"); // Finds the "TextID" property on the serialized object.
        EditorGUILayout.PropertyField(textIDProperty, true); // Renders the "TextID" property with all its child elements.
        
        GUILayout.Space(10);
        GUILayout.Label("Language Comments IDs", EditorStyles.boldLabel);

        SerializedProperty commentsIDProperty = serializedObject.FindProperty("commentsID");  // Finds the "CommentsID" property on the serialized object.
        EditorGUILayout.PropertyField(commentsIDProperty, true); // Renders the "CommentsID" property with all of its child elements.
        
        serializedObject.ApplyModifiedProperties(); // Apply modified properties to serialized object.
    }

    // Renders the design section with colored boxes to display data.
    private void Design()
    {
        GUIStyle ColorLabelStyle = new(EditorStyles.label); // Creates a GUI style for colored labels.
        ColorLabelStyle.normal.textColor = Color.black; // Sets the text color to black.

        GUILayout.Space(20);

        // Renders a purple rectangle with Language Name and Computer Language information.
        LanguageClassToolsEditor.DrawBox(() =>
        {
            GUILayout.Label($"Language Name: {languageName}", ColorLabelStyle);
            GUILayout.Label($"Computer Language: {computerLanguage}", ColorLabelStyle);
        }, new(1f, 0f, 0.9f, 1f)); // Sets the box color to purple.

        // Iterates through the groups and renders information for each group.
        foreach (var group in groupsID)
        {
            // Renders a blue rectangle for each group.
            LanguageClassToolsEditor.DrawBox(() =>
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label($"Text ID: {group.textID}", ColorLabelStyle, GUILayout.Width(990)); // Displays the Text ID.
                GUILayout.Space(10);
                group.view = EditorGUILayout.Toggle(group.view, GUILayout.Width(10)); // Toggles the view of elements.
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Label("IDs: ", ColorLabelStyle, GUILayout.Width(30)); // Displays 'IDs'.
                GUILayout.Space(10);
                group.IDs = EditorGUILayout.TextField(group.IDs, GUILayout.Width(975)); // Displays and modifies IDs.
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Label("Text: ", ColorLabelStyle, GUILayout.Width(30)); // Displays 'Text'.
                GUILayout.Space(10);
                group.text = EditorGUILayout.TextField(group.text, GUILayout.Width(975)); // Displays and modifies text.
                GUILayout.EndHorizontal();
                                
                var idsArray = group.IDs.Split(';'); // Splits the list of IDs into the group.
                var numericIds = idsArray.Where(idStr => int.TryParse(idStr, out int _)).OrderBy(idStr => int.Parse(idStr)).ToList(); // Order IDs numerically.
                foreach (var idStr in numericIds)
                {
                    if (int.TryParse(idStr, out int id))
                    {
                        // Finds the text corresponding to the current ID.
                        var correspondingText = textID.Find(text => text.ID == id);
                        if (correspondingText != null)
                        {
                            // Whether to show the other elements yes or no.
                            if (group.view == true)
                            {                                
                                RenderTextBox(correspondingText, new Color(1f, 1f, 1f, 1f), ColorLabelStyle); // Renders a white rectangle for the corresponding text.
                            }
                        }
                    }
                }
            }, new(0f, 1f, 1f, 1f)); // Sets the box color to blue.

            GUILayout.Space(10);
        }

        // Iterates through the texts and renders information for each text not associated with a group.
        foreach (var text in textID)
        {
            if (!groupsID.Any(group => group.IDs.Split(';').Any(groupIdStr => int.TryParse(groupIdStr, out int groupId) && groupId == text.ID)))
            {                
                RenderTextBox(text, new Color(1f, 1f, 1f, 1f), ColorLabelStyle); // Renders a white rectangle for each text not associated with a group.
                GUILayout.Space(5);
            }
        }
    }

    // Renders a box with information associated with a specific text.
    private void RenderTextBox(LanguageSaveID textData, Color boxColor, GUIStyle ColorLabelStyle)
    {
        // Renders a white rectangle for the corresponding text.
        LanguageClassToolsEditor.DrawBox(() =>
        {
            LanguageClassToolsEditor.ViewComponent(textData.componentType); // Calls a method to view the component type.

            GUILayout.Label($"ID = {textData.ID}", ColorLabelStyle); // Displays the ID of the text.
            GUILayout.BeginHorizontal();
            GUI.enabled = textData.textWrite; // Enables the text modification if allowed.
            GUILayout.Label("Text: ", ColorLabelStyle, GUILayout.Width(30)); // Displays 'Text'.
            GUILayout.Space(10);
            textData.text = EditorGUILayout.TextField(textData.text, GUILayout.Width(975)); // Displays and modifies the text.
            GUI.enabled = true;
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUI.enabled = textData.fontSizeWrite;
            GUILayout.Label("Font Size: ", ColorLabelStyle, GUILayout.Width(60));
            GUILayout.Space(10);
            textData.fontSize = EditorGUILayout.FloatField(textData.fontSize, GUILayout.Width(60));
            GUI.enabled = true;
            
            GUILayout.Space(20);
            
            GUI.enabled = textData.fontWrite;
            GUILayout.Label("Font: ", ColorLabelStyle, GUILayout.Width(30));
            GUILayout.Space(10);
            textData.font = EditorGUILayout.FloatField(textData.font, GUILayout.Width(60));
            GUI.enabled = true;
            
            GUILayout.Space(20);
            
            GUI.enabled = textData.rotationWrite;
            GUILayout.Label("Rotation: ", ColorLabelStyle, GUILayout.Width(55));
            GUILayout.Space(10);
            Vector4 quaternionValues = new(textData.rotation.x, textData.rotation.y, textData.rotation.z, textData.rotation.w);
            quaternionValues = EditorGUILayout.Vector4Field("", quaternionValues, GUILayout.Width(300));
            textData.rotation = new Quaternion(quaternionValues.x, quaternionValues.y, quaternionValues.z, quaternionValues.w);
            GUI.enabled = true;
            
            GUILayout.Space(27);
            
            GUI.enabled = textData.localScaleWrite;
            GUILayout.Label("Local Scale: ", ColorLabelStyle, GUILayout.Width(75));
            GUILayout.Space(10);
            textData.localScale = EditorGUILayout.Vector3Field("", textData.localScale, GUILayout.Width(250));
            GUI.enabled = true;            
            
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            
            GUI.enabled = textData.anchoredPositionWrite;
            GUILayout.Label("Anchored Position: ", ColorLabelStyle, GUILayout.Width(110));
            GUILayout.Space(10);
            textData.anchoredPosition = EditorGUILayout.Vector2Field("", textData.anchoredPosition, GUILayout.Width(150));
            GUI.enabled = true;
            
            GUILayout.Space(130);
            
            GUI.enabled = textData.anchorMinWrite;
            GUILayout.Label("Anchor Min: ", ColorLabelStyle, GUILayout.Width(75));
            GUILayout.Space(10);
            textData.anchorMin = EditorGUILayout.Vector2Field("", textData.anchorMin, GUILayout.Width(150));
            GUI.enabled = true;
            
            GUILayout.Space(133);
            
            GUI.enabled = textData.anchorMaxWrite;
            GUILayout.Label("Anchor Max: ", ColorLabelStyle, GUILayout.Width(75));
            GUILayout.Space(10);
            textData.anchorMax = EditorGUILayout.Vector2Field("", textData.anchorMax, GUILayout.Width(150));
            GUI.enabled = true;
            
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            
            GUI.enabled = textData.sizeDeltaWrite;
            GUILayout.Label("Size Delta: ", ColorLabelStyle, GUILayout.Width(70));
            GUILayout.Space(10);
            textData.sizeDelta = EditorGUILayout.Vector2Field("", textData.sizeDelta, GUILayout.Width(150));
            GUI.enabled = true;
            
            GUILayout.Space(100);
            
            GUI.enabled = textData.pivotWrite;
            GUILayout.Label("Pivot: ", ColorLabelStyle, GUILayout.Width(35));
            GUILayout.Space(10);
            textData.pivot = EditorGUILayout.Vector2Field("", textData.pivot, GUILayout.Width(150));
            GUI.enabled = true;
            
            GUILayout.Space(100);
            
            GUI.enabled = textData.alignmentWrite;
            GUILayout.Label("Alignment: ", ColorLabelStyle, GUILayout.Width(65));
            GUILayout.Space(10);
            textData.alignment = EditorGUILayout.FloatField(textData.alignment, GUILayout.Width(60));
            GUI.enabled = true;
            
            GUILayout.Space(108);
            
            GUI.enabled = textData.reverseWrite;
            GUILayout.Label("Reverse: ", ColorLabelStyle, GUILayout.Width(60));
            GUILayout.Space(10);
            textData.reverse = EditorGUILayout.FloatField(textData.reverse, GUILayout.Width(60));
            GUI.enabled = true;
            GUILayout.EndHorizontal();

            // Finds the comment corresponding to the text ID.
            var correspondingComment = commentsID.FirstOrDefault(comment => comment.comments == textData.ID);
            if (correspondingComment != null)
            {
                GUILayout.Space(10);

                // Renders a green rectangle for the corresponding comment.
                LanguageClassToolsEditor.DrawBox(() =>
                {
                    GUILayout.Label($"Comments: {correspondingComment.comments}", ColorLabelStyle); // Displays the comments ID.

                    GUILayout.BeginHorizontal();
                    GUILayout.Label("Text Comments: ", ColorLabelStyle, GUILayout.Width(100)); // Displays 'Text Comments'.
                    GUILayout.Space(10);
                    correspondingComment.textComments = EditorGUILayout.TextField(correspondingComment.textComments, GUILayout.Width(905)); // Displays and modifies text comments.
                    GUILayout.EndHorizontal();
                }, new(0f, 1f, 0f, 1f)); // Sets the box color to green for comments.
            }
        }, boxColor); // Sets the background color of the box.
    }

    // Opens the destination folder on the file system.
    private void OpenFolder()
    {
        string Path = Application.dataPath + savePath; // Full path to the save folder.
        Application.OpenURL(Path); // Opens File Explorer to the specified folder.
    }

    // Save data to a text file.
    private void SaveTXTFile()
    {
        duplicateIDs = LanguageClassToolsEditor.GetDuplicateIDs(groupsID, textID, commentsID); // Gets a dictionary of duplicate IDs.

        // Checks for duplicate IDs.
        if (duplicateIDs.Count == 0)
        {
            SortByID(); // First, it sorts the elements by ID before saving.

            string Path = Application.dataPath + savePath; // Creates the full path to the save directory.
            string filePath = Path + fileName + ".txt"; // Creates the full path to the text file to be created.

            StringBuilder sb = new(); // Creates a StringBuilder to build the contents of the text file.

            // Adds the text file header.
            sb.AppendLine($"Linguagem - [{languageName}]");
            sb.AppendLine(computerLanguage);

            // Iterates over the elements of the TextID list to add their information to the file.
            foreach (LanguageSaveID id in textID)
            {
                // Adds the ID and optional information to the StringBuilder.
                sb.Append($"id:{id.ID}; ");
                if (id.textWrite) { sb.Append("{" + id.text + "} "); }
                if (id.fontSizeWrite) { sb.Append($"S:{id.fontSize:F0}; "); }
                if (id.fontWrite) { sb.Append($"Font:{id.font}; "); }
                if (id.rotationWrite) { sb.Append($"XRotation:{id.rotation.x}; YRotation:{id.rotation.y}; ZRotation:{id.rotation.z}; WRotation:{id.rotation.w}; "); }
                if (id.localScaleWrite) { sb.Append($"XScale:{id.localScale.x}; YScale:{id.localScale.y}; ZScale:{id.localScale.z}; "); }
                if (id.anchorMinWrite) { sb.Append($"XAnchorMin:{id.anchorMin.x}; YAnchorMin:{id.anchorMin.y}; "); }
                if (id.anchorMaxWrite) { sb.Append($"XAnchorMax:{id.anchorMax.x}; YAnchorMax:{id.anchorMax.y}; "); }
                if (id.anchoredPositionWrite) { sb.Append($"X:{id.anchoredPosition.x}; Y:{id.anchoredPosition.y}; "); }
                if (id.sizeDeltaWrite) { sb.Append($"Width:{id.sizeDelta.x}; Height:{id.sizeDelta.y}; "); }
                if (id.pivotWrite) { sb.Append($"XPivot:{id.pivot.x}; YPivot:{id.pivot.y}; "); }
                if (id.alignmentWrite) { sb.Append($"Ali:{id.alignment}; "); }
                if (id.reverseWrite) { sb.Append($"Rev:{id.reverse}; "); }
                sb.Append($"CType:{id.componentType:F0};");

                sb.AppendLine(); // Jumps to a new line after adding this ID information.
            }

            // Iterates over the elements of the GroupsID list to add their information to the file.
            foreach (LanguageSaveGroups groups in groupsID)
            {
                sb.AppendLine($"[Text:{groups.textID}; " + "{" + groups.text + "}" + $" ids:{groups.IDs}]");
            }

            // Iterates over elements in the CommentsID list to add their information to the file.
            foreach (LanguageSaveComments comment in commentsID)
            {
                sb.AppendLine($"[Comments:{comment.comments}; " + "{" + comment.textComments + "}]");
            }

            File.WriteAllText(filePath, sb.ToString()); // Writes the contents of the StringBuilder to the text file.
            Debug.Log("Saved File: " + filePath); // Displays a log message that the file was saved.
        }
        else
        {
            Debug.LogError("Language Save Editor: Unable to save due to items with duplicate IDs");
        }
    }

    // Method for saving window settings to JSON file.
    public void SaveDataJson()
    {
        duplicateIDs = LanguageClassToolsEditor.GetDuplicateIDs(groupsID, textID, commentsID); // Gets a dictionary of duplicate IDs.

        // Checks if there are duplicate IDs.
        if (duplicateIDs.Count == 0)
        {
            SortByID(); // Organize all items by ID before saving.

            // Create an object to contain the data.
            LanguageData data = new()
            {
                SavePath = savePath,
                FileName = fileName,
                LanguageName = languageName,
                ComputerLanguage = computerLanguage,
                FirstTime = firstTime,
                TextIDs = textID,
                GroupsIDs = groupsID,
                CommentsIDs = commentsID
            };

            string jsonData = JsonUtility.ToJson(data); // Convert the data object to JSON format.

            try
            {
                File.WriteAllText("ProjectSettings/LanguageData.json", jsonData); // Write the JSON data to a file.
            }
            catch (Exception e)
            {
                Debug.LogError($"Error while saving data: {e.Message}");
            }
        }
        else
        {
            Debug.LogError("Language Save Editor - Unable to save due to items with duplicate IDs");
        }
    }

    // Method for loading window settings from a JSON file.
    private void LoadDataJson()
    {
        // Check if the file exists.
        if (File.Exists("ProjectSettings/LanguageData.json"))
        {
            string jsonData = File.ReadAllText("ProjectSettings/LanguageData.json"); // Read the JSON content from the file.
            var data = JsonUtility.FromJson<LanguageData>(jsonData); // Convert the JSON content back into objects.

            // Assign the retrieved values back to the variables.
            savePath = data.SavePath;
            fileName = data.FileName;
            languageName = data.LanguageName;
            computerLanguage = data.ComputerLanguage;
            firstTime = data.FirstTime;

            // Clear existing data structures before assigning new values.
            textID.Clear();
            groupsID.Clear();
            commentsID.Clear();

            // Assign the retrieved lists from the data object.
            textID = data.TextIDs;
            groupsID = data.GroupsIDs;
            commentsID = data.CommentsIDs;
        }
    }
}

[System.Serializable]
public class LanguageSaveGroups
{
    public float textID; // Language group unique ID.
    public string IDs; // IDs of elements belonging to this group (separated by semicolons).
    public string text; // Text associated with this group.
    [HideInInspector] public bool view; // If you are going to show the yes or no elements.
}

[System.Serializable]
public class LanguageSaveID
{
    [Header("Custom ID")]
    public float ID; // Unique ID of the element.
    [HideInInspector] public float componentType; // Component type associated with the element.
    [Space(5)]
    [Header("Rect Transform")]
    public Quaternion rotation; // Rotation of the element.
    [HideInInspector] public bool rotationWrite; // Flag indicating if rotation is editable.
    public Vector3 localScale; // Local scale of the element.
    [HideInInspector] public bool localScaleWrite; // Flag indicating if local scale is editable.
    [Space(5)]
    public Vector2 anchorMin; // Minimum anchor position of the element.
    [HideInInspector] public bool anchorMinWrite; // Flag indicating if anchor minimum is editable.
    public Vector2 anchorMax; // Maximum anchor position of the element.
    [HideInInspector] public bool anchorMaxWrite; // Flag indicating if anchor maximum is editable.
    public Vector2 anchoredPosition; // Anchored position of the element.
    [HideInInspector] public bool anchoredPositionWrite; // Flag indicating if anchored position is editable.
    public Vector2 sizeDelta; // Size delta of the element.
    [HideInInspector] public bool sizeDeltaWrite; // Flag indicating if size delta is editable.
    public Vector2 pivot; // Pivot of the element.
    [HideInInspector] public bool pivotWrite; // Flag indicating if pivot is editable.
    [Space(5)]
    [Header("Text")]
    [Multiline] public string text; // Text associated with the element.
    [HideInInspector] public bool textWrite; // Flag indicating if text is editable.
    [Space(5)]
    [Header("Character")]
    public float font; // Font ID used by the element.
    [HideInInspector] public bool fontWrite; // Flag indicating if font ID is editable.
    public float fontSize; // Element font size.
    [HideInInspector] public bool fontSizeWrite; // Flag indicating if font size is editable.
    [Space(5)]
    [Header("Paragraph")]
    public float alignment; // Text alignment.
    [HideInInspector] public bool alignmentWrite; // Flag indicating if alignment is editable.
    public float reverse; // Text reversal indicator.
    [HideInInspector] public bool reverseWrite; // Flag indicating if text reversal is editable.
}

[System.Serializable]
public class LanguageSaveComments
{
    public float comments; // ID associated with the comment.
    public string textComments; // Text associated with the comment.
}

[System.Serializable]
public class LanguageData
{
    public string SavePath; // Path where the data will be saved.
    public string FileName; // Name of the file.
    public string LanguageName; // Name of the language.
    public string ComputerLanguage; // Language used by the computer.
    public bool FirstTime; // Flag to indicate if it's the first time the method runs.
    public List<LanguageSaveID> TextIDs; // List of LanguageSaveID objects containing text information.
    public List<LanguageSaveGroups> GroupsIDs; // List of LanguageSaveGroups objects containing group information.
    public List<LanguageSaveComments> CommentsIDs; // List of LanguageSaveComments objects containing comment information.
}
#endif