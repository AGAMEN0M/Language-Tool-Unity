using System.IO;
using System.Text;
using System.Windows.Forms;
using System.Collections.Generic;
using UnityEngine;

public class SaveFile : MonoBehaviour
{
    [Header("Save Information")]
    [SerializeField] private string linguagem = "Linguagem - [ENGLISH]"; // Saved language.
    [SerializeField] private string computerLanguage = "English (United States)"; // Computer language.
    [Space(10)]
    [SerializeField] private List<ID> IDs; // List of IDs representing various text properties.
    [SerializeField] private List<Groups> groups; // List of groups of text.
    [SerializeField] private List<Comments> comments; // List of comments associated with text.
    [Space(10)]
    [Header("Archives Location")]
    [SerializeField] private string dateTime = "Ds 00-00-0000 Hs 00-00-00"; // Save date and time.
    [SerializeField] private float timeRemaining = 60f; // Time remaining for auto-save trigger.
    [Space(3)]
    #pragma warning disable CS0414
    [SerializeField] private string fileNameInUnity = "/StreamingAssets/Language/Saves/"; // Save path in Unity.
    [SerializeField] private string fileNameInBuild = "/StreamingAssets/Language/Saves/"; // Save path in build.
    #pragma warning restore CS0414

    // Updates the save date and time.
    private void FixedUpdate()
    {
        dateTime = $"Ds {System.DateTime.Now:dd-MM-yyyy} Hs {System.DateTime.Now:HH-mm-ss}"; // Update the date and time for the save file.
        timeRemaining -= Time.fixedDeltaTime; // Reduces remaining time with each FixedUpdate.

        if (timeRemaining <= 0f)
        {
            // Save the file data.
            OrganizeLinguagem organizeScript = FindObjectOfType<OrganizeLinguagem>();
            if (organizeScript != null)
            {
                // Update language and related data from the scene objects.
                linguagem = string.Format($"Linguagem - [{organizeScript.Linguagem.text}]");
                computerLanguage = organizeScript.LanguageComputer.text;

                // Create data from scene objects.
                CreateIDsFromAllOrganize();
                CreateGroupsFromAllOrganize();
                CreateCommentsFromAllOrganize();

                string Path = GetFilePath();
                string filePath = Path + "AutoSave.txt";
                StartSaveFile(filePath);
                Debug.Log("AutoSave File");
            }

            timeRemaining = 60f; // Reset the timer to 1 minute.
        }
    }

    // Save the file data.
    public void Save()
    {
        OrganizeLinguagem organizeScript = FindObjectOfType<OrganizeLinguagem>(); // Find the script responsible for organizing language data in the scene.

        // Update language and related data from the found script.
        linguagem = string.Format($"Linguagem - [{organizeScript.Linguagem.text}]");
        computerLanguage = organizeScript.LanguageComputer.text;

        // Create data related to IDs, groups, and comments from all objects in the scene.
        CreateIDsFromAllOrganize();
        CreateGroupsFromAllOrganize();
        CreateCommentsFromAllOrganize();

        SaveWindow(); // Open a file dialog window for the user to select a save location and filename.
    }

    // Opens a file dialog for saving the data to a user-selected file location.
    private void SaveWindow()
    {
        // Create a SaveFileDialog for selecting the file save location.
        SaveFileDialog saveFileDialog = new()
        {
            InitialDirectory = Path.GetDirectoryName(GetFilePath()), // Set the initial directory to the path of the last used file (if available).
            Filter = "Text Files (*.txt)|*.txt|All files (*.*)|*.*", // Set the filters for file types in the save dialog.
            FilterIndex = 1, // Set the default filter index.
            RestoreDirectory = true, // Restore the directory to the previously selected one.
            FileName = dateTime // Set the default file name to the current date and time.
        };

        // Show the Save File dialog and wait for user input.
        if (saveFileDialog.ShowDialog() == DialogResult.OK)
        {
            string filePath = saveFileDialog.FileName; // Get the selected file path from the dialog.
            Debug.Log($"File Saved In: {filePath}"); // Log the file path to the Unity console.
            StartSaveFile(filePath); // Start the process of saving the file.
        }
        else
        {
            Debug.Log("No files have been Saved."); // Log a message indicating that no files have been saved.
        }

        saveFileDialog.Dispose(); // Dispose of the SaveFileDialog to release resources.
    }

    // Save the file.
    private void StartSaveFile(string filePath)
    {
        SortByID(); // Organize all items.        
        if (string.IsNullOrEmpty(filePath)) { Debug.Log("No file selected."); return; } // Check if no file path is provided.
        StringBuilder sb = new(); // Create a StringBuilder to build the string that will be written to the file.

        // Write the language information to the file.
        sb.AppendLine(linguagem);
        sb.AppendLine(computerLanguage);

        // Write the data for each ID to the file.
        foreach (ID id in IDs)
        {            
            // Adds the ID and optional information to the StringBuilder.
            sb.Append($"id:{id.id}; ");
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

            sb.AppendLine(); // Move to a new line after adding this ID information.
        }

        // Write the data for each group to the file.
        foreach (Groups groups in groups)
        {
            sb.AppendLine($"[Text:{groups.textID}; " + "{" + groups.text + "}" + $" ids:{groups.IDs}]"); // Append group information to the StringBuilder.
        }

        // Write the data for each comment to the file.
        foreach (Comments comment in comments)
        {
            sb.AppendLine($"[Comments:{comment.comments}; " + "{" + comment.textComments + "}]"); // Append comment information to the StringBuilder.
        }

        File.WriteAllText(filePath, sb.ToString()); // Write the final constructed string to the file.
        Debug.Log($"Saved File: {filePath}"); // Log the path and name of the saved file to the console.
    }

    // Collect and create ID data from all OrganizeIDS scripts in the scene.
    private void CreateIDsFromAllOrganize()
    {
        IDs.Clear(); // Clear the list of IDs to avoid duplicates.
        OrganizeIDS[] organizeScripts = FindObjectsOfType<OrganizeIDS>(); // Get all OrganizeIDS components present in the scene.

        // Iterate over each OrganizeIDS found in the scene.
        foreach (OrganizeIDS organize in organizeScripts)
        {
            ID id = MapOrganizeToID(organize); // Map the OrganizeIDS component to an ID object.
            IDs.Add(id); // Add the ID object to the list of IDs.
        }
    }

    // Map data from OrganizeIDS to ID object.
    private ID MapOrganizeToID(OrganizeIDS organize)
    {
        // Create an ID object and assign values based on OrganizeIDS properties.
        ID id = new()
        {
            id = organize.textID,
            componentType = organize.spritesIndex,
            rotation = new Quaternion(float.TryParse(organize.rotationX.text, out float rX) ? rX : 0f, float.TryParse(organize.rotationY.text, out float rY) ? rY : 0f, float.TryParse(organize.rotationZ.text, out float rZ) ? rZ : 0f, float.TryParse(organize.rotationW.text, out float rW) ? rW : 0f),
            rotationWrite = organize.interactableRotation,
            localScale = new Vector3(float.TryParse(organize.localScaleX.text, out float lsX) ? lsX : 0f, float.TryParse(organize.localScaleY.text, out float lsY) ? lsY : 0f, float.TryParse(organize.localScaleZ.text, out float lsZ) ? lsZ : 0f),
            localScaleWrite = organize.interactableLocalScale,
            anchorMin = new Vector2(float.TryParse(organize.anchorMinX.text, out float minX) ? minX : 0f, float.TryParse(organize.anchorMinY.text, out float minY) ? minY : 0f),
            anchorMinWrite = organize.interactableAnchorMin,
            anchorMax = new Vector2(float.TryParse(organize.anchorMaxX.text, out float maxX) ? maxX : 0f, float.TryParse(organize.anchorMaxY.text, out float maxY) ? maxY : 0f),
            anchorMaxWrite = organize.interactableAnchorMax,
            anchoredPosition = new Vector2(float.TryParse(organize.anchoredPositionX.text, out float apX) ? apX : 0f, float.TryParse(organize.anchoredPositionY.text, out float apY) ? apY : 0f),
            anchoredPositionWrite = organize.interactableAnchoredPosition,
            sizeDelta = new Vector2(float.TryParse(organize.sizeDeltaX.text, out float sdX) ? sdX : 0f, float.TryParse(organize.sizeDeltaY.text, out float sdY) ? sdY : 0f),
            sizeDeltaWrite = organize.interactableSizeDelta,
            pivot = new Vector2(float.TryParse(organize.pivitX.text, out float pX) ? pX : 0f, float.TryParse(organize.pivitY.text, out float pY) ? pY : 0f),
            pivotWrite = organize.interactablePivit,
            text = organize.text.text,
            textWrite = organize.interactableText,
            font = float.TryParse(organize.font.text, out float f) ? f : 0f,
            fontWrite = organize.interactableFont,
            fontSize = float.TryParse(organize.fontSize.text, out float fs) ? fs : 0f,
            fontSizeWrite = organize.interactableFontSize,
            alignmentWrite = organize.interactableAlignment,
            alignment = organize.alignmentValue,
            reverseWrite = organize.interactableReverse,
            reverse = organize.reverseValue
        };        

        return id; // Return the mapped ID object.
    }

    // Collect and create group data from all OrganizeIDGroups scripts in the scene.
    private void CreateGroupsFromAllOrganize()
    {
        groups.Clear(); // Clear the list of groups.
        OrganizeIDGroups[] organizeScripts = FindObjectsOfType<OrganizeIDGroups>(); // Find all OrganizeIDGroups scripts in the scene.

        // Iterate over each OrganizeIDGroups found in the scene.
        foreach (OrganizeIDGroups organize in organizeScripts)
        {
            Groups group = MapOrganizeToGroups(organize); // Map the OrganizeIDGroups properties to a Groups object.
            groups.Add(group); // Add the mapped Groups object to the list of groups.
        }
    }

    // Map data from OrganizeIDGroups to Groups object.
    private Groups MapOrganizeToGroups(OrganizeIDGroups organize)
    {
        // Create a Groups object and assign values based on OrganizeIDGroups properties.
        Groups group = new()
        {
            // Assign text ID, associated IDs, and group text from OrganizeIDGroups.
            textID = float.TryParse(organize.textID.text, out float tID) ? tID : 0f,
            IDs = organize.textIDs.text,
            text = organize.texts.text
        };

        return group; // Return the mapped Groups object.
    }

    // Collect and create comment data from all OrganizeCommentsIDS scripts in the scene.
    private void CreateCommentsFromAllOrganize()
    {
        comments.Clear(); // Clear the comments list.
        OrganizeCommentsIDS[] organizeScripts = FindObjectsOfType<OrganizeCommentsIDS>(); // Find all OrganizeCommentsIDS scripts in the scene.

        // Iterate over each OrganizeCommentsIDS found in the scene.
        foreach (OrganizeCommentsIDS organize in organizeScripts)
        {
            Comments Comment = MapOrganizeToComments(organize); // Map the properties of OrganizeCommentsIDS to a Comments object.
            comments.Add(Comment); // Add the mapped Comments object to the list of comments.
        }
    }

    // Map data from OrganizeCommentsIDS to Comments object.
    private Comments MapOrganizeToComments(OrganizeCommentsIDS organize)
    {
        // Create a Comments object and assign values based on OrganizeCommentsIDS properties.
        Comments Comment = new()
        {
            // Assign comment value and associated text from OrganizeCommentsIDS.
            comments = float.TryParse(organize.textID.text, out float c) ? c : 0f,
            textComments = organize.Comment.text
        };

        return Comment; // Return the mapped Comments object.
    }

    // Method that sorts the list items based on their IDs.
    private void SortByID()
    {
        IDs.Sort((a, b) => a.id.CompareTo(b.id)); // Sort the list of IDs by their ID values.
        groups.Sort((a, b) => a.textID.CompareTo(b.textID)); // Sort the list of groups by their text ID values.
        comments.Sort((a, b) => a.comments.CompareTo(b.comments)); // Sort the list of comments by their comment values.
    }

    // Get the file path based on the platform - for Unity Editor and build.
    private string GetFilePath()
    {
        // Define the path to the file folder according to the platform.
    #if UNITY_EDITOR
        return UnityEngine.Application.dataPath + fileNameInUnity; // File path for Unity Editor.
    #else
        return UnityEngine.Application.dataPath + fileNameInBuild; // File path for build.
    #endif
    }
}

[System.Serializable]
public class ID
{
    [Header("Custom ID")]
    public float id; // The unique identifier for the text.
    public float componentType; // Type of component associated with the ID.
    [Space(5)]
    [Header("Rect Transform")]
    public Quaternion rotation; // Rotation information for RectTransform.
    public bool rotationWrite; // Indicates if rotation should be written to the save file.
    public Vector3 localScale; // Local scale information for RectTransform.
    public bool localScaleWrite; // Indicates if local scale should be written to the save file.
    [Space(5)]
    public Vector2 anchorMin; // Anchor Min information for RectTransform.
    public bool anchorMinWrite; // Indicates if anchor min should be written to the save file.
    public Vector2 anchorMax; // Anchor Max information for RectTransform.
    public bool anchorMaxWrite; // Indicates if anchor max should be written to the save file.
    public Vector2 anchoredPosition; // Anchored Position information for RectTransform.
    public bool anchoredPositionWrite; // Indicates if anchored position should be written to the save file.
    public Vector2 sizeDelta; // Size Delta information for RectTransform.
    public bool sizeDeltaWrite; // Indicates if size delta should be written to the save file.
    public Vector2 pivot; // Pivot information for RectTransform.
    public bool pivotWrite; // Indicates if pivot should be written to the save file.
    [Space(5)]
    [Header("Text")]
    public string text; // The text content.
    public bool textWrite; // Indicates whether text can be written to the save file.
    [Space(5)]
    [Header("Character")]
    public float font; // Font of the text.
    public bool fontWrite; // Indicates if font should be written to the save file.
    public float fontSize; // The text size.
    public bool fontSizeWrite; // Indicates if font size should be written to the save file.
    [Space(5)]
    [Header("Paragraph")]
    public float alignment; // Text alignment value.
    public bool alignmentWrite; // Indicates if alignment should be written to the save file.
    public float reverse; // Indicates whether the text is inverted.
    public bool reverseWrite; // Indicates if inverted text should be written to the save file.
}

[System.Serializable]
public class Groups
{
    public float textID; // ID of the text that makes up the group.
    public string IDs; // IDs of the texts that are part of the group (separated by semicolons).
    public string text; // Group text.
}

[System.Serializable]
public class Comments
{
    public float comments; // Stores the Value of the comment.
    public string textComments; // Stores the comment text.
}