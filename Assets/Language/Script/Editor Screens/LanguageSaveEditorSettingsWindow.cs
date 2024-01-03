#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public class LanguageSaveEditorSettingsWindow : EditorWindow
{
    // Declare variables for managing editor window state and data.
    public static LanguageSaveEditorWindow instance; // Reference to the editor window instance.
    public Vector2 scrollPosition = Vector2.zero; // Scroll position for the GUI.
    public string text; // Currently selected item's text.

    // Variables for managing group data.
    public float textID; // Group's text ID.
    public string IDs; // Group's IDs.
    public string _text; // Group's text content.

    // Variables for managing comment data.
    public float comments; // Comment's ID.
    public string textComments; // Comment's text content.

    // Flags for controlling which sections are displayed.
    public bool groups;
    public bool _comments;

    // Flags for selecting the type of component (Group, Comment, Text).
    public bool selectGroups;
    public bool selectComments;
    public bool selectText;
        
    public float ID; // ID used for deleting selected items.

    private void OnGUI()
    {
        if (instance == null)
        {
            instance = EditorWindow.GetWindow<LanguageSaveEditorWindow>();
        }

        scrollPosition = GUILayout.BeginScrollView(scrollPosition); // Begin the scroll view for the GUI.

        // Button for clearing all fields.
        if (GUILayout.Button("Clean"))
        {
            // Display a confirmation dialog before clearing fields.
            bool confirmClean = EditorUtility.DisplayDialog("Confirm Clean", "Are you sure you want to clear all fields?", "Yes", "No");
            if (confirmClean)
            {
                groups = false;
                _comments = false;
                selectGroups = false;
                selectComments = false;
                selectText = false;
                text = "";
                ID = 0f;
            }
        }

        GUILayout.Space(10);
        AddObject(); // Section for adding new objects (groups or comments).

        GUILayout.Space(10);
        Design(); // Section for designing and editing group or comment data.

        GUILayout.Space(30);
        DeleteComponent(); // Section for deleting selected components.

        GUILayout.EndScrollView(); // End the scroll view.
    }

    private void AddObject()
    {        
        GUILayout.Label("Add new Object");
        GUILayout.Space(10);

        GUILayout.BeginHorizontal();

        // Buttons for adding new groups and comments.
        LanguageClassToolsEditor.DrawColoredButton("+", 50f, 50f, new Color(0f, 2.5f, 2.5f, 1f), Color.black, AddGroups);
        LanguageClassToolsEditor.DrawColoredButton("+", 50f, 50f, new Color(0f, 2.5f, 0f, 1f), Color.black, AddComments);

        GUILayout.EndHorizontal();
    }

    private void Design()
    {
        GUIStyle ColorLabelStyle = new(EditorStyles.label);
        ColorLabelStyle.normal.textColor = Color.black;

        if (groups)
        {
            // Display group data editing section inside a blue box.
            LanguageClassToolsEditor.DrawBox(() =>
            {
                GUILayout.Space(10);
                GUILayout.BeginHorizontal();
                GUILayout.Label("Text ID:", ColorLabelStyle, GUILayout.Width(45));
                GUILayout.Space(10);
                textID = EditorGUILayout.FloatField(textID, GUILayout.Width(340));
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Label("IDs: ", ColorLabelStyle, GUILayout.Width(45));
                GUILayout.Space(10);
                IDs = EditorGUILayout.TextField(IDs, GUILayout.Width(340));
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Label("Text: ", ColorLabelStyle, GUILayout.Width(45));
                GUILayout.Space(10);
                _text = EditorGUILayout.TextField(_text, GUILayout.Width(340));
                GUILayout.EndHorizontal();
                GUILayout.Space(10);
            }, new(0f, 1f, 1f, 1f));

            if (GUILayout.Button("Apply", GUILayout.Width(100)))
            {
                GroupsApply();
            }
        }

        if (_comments)
        {
            LanguageClassToolsEditor.DrawBox(() =>
            {
                // Display comment data editing section inside a green box.
                GUILayout.Space(10);
                GUILayout.BeginHorizontal();
                GUILayout.Label("Comments", ColorLabelStyle, GUILayout.Width(100));
                GUILayout.Space(10);
                comments = EditorGUILayout.FloatField(comments, GUILayout.Width(285));
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Label("Text Comments: ", ColorLabelStyle, GUILayout.Width(100));
                GUILayout.Space(10);
                textComments = EditorGUILayout.TextField(textComments, GUILayout.Width(285));
                GUILayout.EndHorizontal();
                GUILayout.Space(10);
            }, new(0f, 1f, 0f, 1f));

            if (GUILayout.Button("Apply", GUILayout.Width(100)))
            {
                CommentsApply();
            }
        }
    }

    private void DeleteComponent()
    {
        GUILayout.BeginHorizontal();
        GUILayout.Label("Delete Component", GUILayout.Width(150));

        if (!string.IsNullOrEmpty(text)) 
        {
            GUILayout.Label($"Selected item: {text}");
        }
        else
        {
            GUILayout.Label("Selected item: None");
        }

        GUILayout.EndHorizontal();
        GUILayout.Space(10);

        GUILayout.BeginHorizontal();

        // Button for deleting the selected item.
        GUI.enabled = !string.IsNullOrEmpty(text);
        LanguageClassToolsEditor.DrawColoredButton("X", 50f, 50f, new Color(2.5f, 0f, 0f, 1f), Color.black, Delete);
        GUI.enabled = true;

        GUILayout.Space(105);

        // Buttons for selecting the type of component (Text, Group, Comment).
        LanguageClassToolsEditor.DrawColoredButton("T'ID", 20f, 50f, new Color(2.5f, 2.5f, 2.5f, 1f), Color.black, SelectText);
        LanguageClassToolsEditor.DrawColoredButton("G'ID", 20f, 50f, new Color(0f, 2.5f, 2.5f, 1f), Color.black, SelectGroups);
        LanguageClassToolsEditor.DrawColoredButton("C'ID", 20f, 50f, new Color(0f, 2.5f, 0f, 1f), Color.black, SelectComments);

        GUILayout.EndHorizontal();

        GUILayout.Space(10);

        if (!string.IsNullOrEmpty(text))
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label("ID:", GUILayout.Width(30));
            ID = EditorGUILayout.FloatField(ID, GUILayout.Width(50));
            GUILayout.Label("Enter the ID of the item you want to Delete.");
            GUILayout.EndHorizontal();

            GUILayout.Label($"Selected ID: {ID}");
        }
    }

    private void SelectGroups()
    {
        // Select the Groups section and set the text.
        selectGroups = true;
        selectComments = false;
        selectText = false;

        text = "GroupsID";
    }

    private void AddGroups()
    {
        // Enable the Groups section and disable Comments.
        groups = true;
        _comments = false;

        // Initialize group data fields.
        textID = 0;
        IDs = "";
        _text = "";
    }

    private void GroupsApply()
    {
        // Create a new LanguageSaveGroups object and add it to the list.
        LanguageSaveGroups newGroup = new()
        {
            textID = textID,
            IDs = IDs,
            text = _text,
            view = true
        };

        instance.groupsID.Add(newGroup);

        // Reset group data fields.
        groups = false;
        textID = 0;
        IDs = "";
        _text = "";
    }

    private void SelectComments()
    {
        // Select the Comments section and set the text.
        selectGroups = false;
        selectComments = true;
        selectText = true;

        text = "CommentsID";
    }

    private void AddComments()
    {
        // Enable the Comments section and disable Groups.
        groups = false;
        _comments = true;

        // Initialize comment data fields.
        comments = 0;
        textComments = "";
    }

    private void CommentsApply()
    {
        // Create a new LanguageSaveComments object and add it to the list.
        LanguageSaveComments newComments = new()
        {
            comments = comments,
            textComments = textComments
        };

        instance.commentsID.Add(newComments);

        // Reset comment data fields.
        _comments = false;
        comments = 0;
        textComments = "";
    }

    private void SelectText()
    {
        // Select the Text section.
        selectGroups = false;
        selectComments = false;
        selectText = true;

        text = "TextID";
    }

    private void Delete()
    {
        // Display a confirmation dialog before deleting the selected item.
        bool Confirmed = EditorUtility.DisplayDialog("Confirmation", "He is sure?\nIt will not be possible to undo this action.", "Yes", "No");
        if (Confirmed)
        {
            if (selectGroups)
            {
                // Remove the selected group based on the ID.
                LanguageSaveGroups groupToRemove = instance.groupsID.Find(group => group.textID == ID);
                if (groupToRemove != null)
                {
                    instance.groupsID.Remove(groupToRemove);
                }
            }
            else if (selectComments)
            {
                // Remove the selected comment based on the ID.
                LanguageSaveComments commentsToRemove = instance.commentsID.Find(comments => comments.comments == ID);
                if (commentsToRemove != null)
                {
                    instance.commentsID.Remove(commentsToRemove);
                }
            }
            else if (selectText)
            {
                // Remove the selected text based on the ID.
                LanguageSaveID textToRemove = instance.textID.Find(text => text.ID == ID);
                if (textToRemove != null)
                {
                    instance.textID.Remove(textToRemove);
                }
            }

            text = ""; // Clear the selected item text.
            ID = 0f; // Reset the ID.
        }

        instance.SaveDataJson(); // Update save.
    }
}
#endif