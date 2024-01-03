using System;
using System.IO;
using System.Collections;
using System.Windows.Forms;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LoadFile : MonoBehaviour
{
    [Header("File Information")]
    [SerializeField] private string linguagem = "ENGLISH"; // Language of the selected file.
    [SerializeField] private string computerLanguage = "English (United States)"; // Computer language.
    [Space(10)]
    [SerializeField] private List<FileID> fileIDs; // List of file IDs.
    [SerializeField] private List<FileGroups> fileGroups; // List of file text groups.
    [SerializeField] private List<FileComments> fileComments; // List of file comments.
    [Space(10)]
    [Header("Components")]
    [SerializeField] private Text _name; // Text that shows the name of the selected file.
    [SerializeField] private GameObject _object; // GameObject that shows text groups and comments.
    [SerializeField] private GameObject loadingScreen; // Loading Screen Object.
    [SerializeField] private Text loadingPercentage; // Text field to display loading percentage.
    [Space(10)]
    [Header("Prefab Components")]
    [SerializeField] private GameObject prefabLinguagem; // Prefab to create the element that shows the language of the file.
    [SerializeField] private GameObject prefabIDGroups; // Prefab to create the elements that show the text groups.
    [SerializeField] private GameObject prefabIDEditor; // Prefab to create the elements that allow you to edit the text groups.
    [SerializeField] private GameObject prefabComments; // Prefab to create the elements that show the comments.
    [Space(5)]
    [Header("Archives Location")]
    [SerializeField] private List<string> fileNames; // List of available file names.
    #pragma warning disable CS0414
    [SerializeField] private string fileNameInUnity = "/StreamingAssets/Language/"; // File location in Unity Editor.
    [SerializeField] private string fileNameInBuild = "/StreamingAssets/Language/"; // File location in build.
    #pragma warning restore CS0414

    // Method to construct and return the file path based on the platform.
    private string GetFilePath()
    {
        // Check the platform where the application is running.
    #if UNITY_EDITOR
        return UnityEngine.Application.dataPath + fileNameInUnity; // Running in Unity Editor - Construct file path for editor.
    #else
        return UnityEngine.Application.dataPath + fileNameInBuild; // Running in build - Construct file path for build.
    #endif
    }

    // Method to open a file dialog and return the selected file path.
    private string OpenFile()
    {
        // Configure settings for the file dialog window.
        OpenFileDialog openFileDialog = new()
        {
            InitialDirectory = Path.GetDirectoryName(GetFilePath()), // Sets the home directory.
            Filter = "Text Files (*.txt)|*.txt|All files (*.*)|*.*", // Defines the types of files that can be selected.
            FilterIndex = 1, // Sets the default filter (text files).
            RestoreDirectory = true // Restores the previous directory when closing the dialog.
        };

        // Open the file dialog window and wait for file selection.
        if (openFileDialog.ShowDialog() == DialogResult.OK)
        {
            string selectedFilePath = openFileDialog.FileName; // File selected, retrieve its path.
            Debug.Log("Selected File: " + selectedFilePath);
            openFileDialog.Dispose(); // Dispose of resources after file selection.
            return selectedFilePath; // Return the selected file path.
        }
        else
        {
            // No file selected, return null.
            Debug.Log("No file selected.");
            return null;
        }
    }

    // Method to initiate file loading and UI update.
    public void Load()
    {
        string selectedFilePath = OpenFile(); // Get the selected file path using the OpenFile method.

        // Check if a file is selected.
        if (string.IsNullOrEmpty(selectedFilePath))
        {
            // If no file selected, log and exit the method.
            Debug.Log("No file selected.");
            return;
        }
                
        string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(selectedFilePath); // Remove the .txt extension from the file name.
        _name.text = fileNameWithoutExtension; // Update the 'Name' text with the file name.
        loadingPercentage.text = $"0%"; // Reset text field percentage.
        loadingScreen.SetActive(true); // Show the loading screen to indicate file loading.

        // Clear existing UI elements before loading new content.
        foreach (Transform child in _object.transform)
        {
            Destroy(child.gameObject);
        }

        StartCoroutine(LoadFileAsync(selectedFilePath)); // Start loading file content asynchronously.
    }

    // Method to load file content asynchronously and create UI elements.
    private IEnumerator LoadFileAsync(string filePath)
    {
        var loadTask = new StreamReader(filePath).ReadToEndAsync(); // Asynchronously read the entire file content.
        yield return new WaitUntil(() => loadTask.IsCompleted); // Wait until the file is loaded.
        string fileContents = loadTask.Result; // Get the loaded file contents.
        string[] lines = File.ReadAllLines(filePath); // Split the file contents into lines.

        // Extract language and computer language from the file.
        // The language information is stored in the first lines of the file.
        linguagem = lines[0].Substring(lines[0].IndexOf("[") + 1, lines[0].IndexOf("]") - lines[0].IndexOf("[") - 1);
        computerLanguage = lines[1];

        // Create UI element for language display based on extracted information.
        GameObject linguagemObj = Instantiate(prefabLinguagem, _object.transform);
        OrganizeLinguagem organizeLinguagem = linguagemObj.GetComponent<OrganizeLinguagem>();
        organizeLinguagem.Linguagem.text = linguagem;
        organizeLinguagem.LanguageComputer.text = computerLanguage;

        // Clear and prepare lists for file groups, comments, and IDs.
        fileGroups.Clear();
        fileGroups = new List<FileGroups>();
        fileComments.Clear();
        fileComments = new List<FileComments>();
        fileIDs.Clear();
        fileIDs = new List<FileID>();

        // Process each line of the file content to extract and organize information,
        foreach (string line in lines)
        {
            // Extract and organize file groups based on specific pattern in the file.
            if (line.StartsWith("[Text:"))
            {
                // Extract the text ID, text, and IDs from the file and store them in a file group object.
                string test = LanguageClassTools.ExtractTextWithinBraces(line);
                string lineWithoutCurlyBraces = LanguageClassTools.RemoveContentWithinCurlyBraces(line);
                float textID = LanguageClassTools.ExtractFloatValue(lineWithoutCurlyBraces, "[Text:");
                string ids = lineWithoutCurlyBraces[(lineWithoutCurlyBraces.IndexOf("ids:") + 4)..];

                FileGroups fileGroup = new() { textID = textID, test = test, IDs = ids };
                fileGroups.Add(fileGroup);
            }

            // Extract and organize file comments based on specific pattern in the file.
            if (line.StartsWith("[Comments:"))
            {
                string test = LanguageClassTools.ExtractTextWithinBraces(line); // Gets the comment text.
                string lineWithoutCurlyBraces = LanguageClassTools.RemoveContentWithinCurlyBraces(line);
                float ID = LanguageClassTools.ExtractFloatValue(lineWithoutCurlyBraces, "[Comments:"); // Gets the comment ID.

                FileComments fileComment = new() { comments = ID, testComments = test };
                fileComments.Add(fileComment);
            }

            // Extract and organize file IDs based on specific pattern in the file.
            if (line.StartsWith("id:"))
            {
                FileID fileIDs = new(); // Creates a new instance of the FileID class.

                string lineWithoutCurlyBraces = LanguageClassTools.RemoveContentWithinCurlyBraces(line);
                float ID = LanguageClassTools.ExtractFloatValue(lineWithoutCurlyBraces, "id:"); // Gets the file ID.

                // Process various components of the file ID.
                if (lineWithoutCurlyBraces.Contains("CType:"))
                {
                    fileIDs.componentType = LanguageClassTools.ExtractFloatValue(lineWithoutCurlyBraces, "CType:");
                }

                // Process rotation components if present.
                if (lineWithoutCurlyBraces.Contains("XRotation:") || lineWithoutCurlyBraces.Contains("YRotation:") || lineWithoutCurlyBraces.Contains("ZRotation:") || lineWithoutCurlyBraces.Contains("WRotation:"))
                {
                    fileIDs.rotation.x = LanguageClassTools.ExtractFloatValue(lineWithoutCurlyBraces, "XRotation:");
                    fileIDs.rotation.y = LanguageClassTools.ExtractFloatValue(lineWithoutCurlyBraces, "YRotation:");
                    fileIDs.rotation.z = LanguageClassTools.ExtractFloatValue(lineWithoutCurlyBraces, "ZRotation:");
                    fileIDs.rotation.w = LanguageClassTools.ExtractFloatValue(lineWithoutCurlyBraces, "WRotation:");
                    fileIDs.rotationWrite = true;
                }

                // Process local scale components if present.
                if (lineWithoutCurlyBraces.Contains("XScale:") || lineWithoutCurlyBraces.Contains("YScale:") || lineWithoutCurlyBraces.Contains("ZScale:"))
                {
                    fileIDs.localScale.x = LanguageClassTools.ExtractFloatValue(lineWithoutCurlyBraces, "XScale:");
                    fileIDs.localScale.y = LanguageClassTools.ExtractFloatValue(lineWithoutCurlyBraces, "YScale:");
                    fileIDs.localScale.z = LanguageClassTools.ExtractFloatValue(lineWithoutCurlyBraces, "ZScale:");
                    fileIDs.localScaleWrite = true;
                }

                // Process anchor min components if present.
                if (lineWithoutCurlyBraces.Contains("XAnchorMin:") || lineWithoutCurlyBraces.Contains("YAnchorMin:"))
                {
                    fileIDs.anchorMin.x = LanguageClassTools.ExtractFloatValue(lineWithoutCurlyBraces, "XAnchorMin:");
                    fileIDs.anchorMin.y = LanguageClassTools.ExtractFloatValue(lineWithoutCurlyBraces, "YAnchorMin:");
                    fileIDs.anchorMinWrite = true;
                }

                // Process anchor max components if present.
                if (lineWithoutCurlyBraces.Contains("XAnchorMax:") || lineWithoutCurlyBraces.Contains("YAnchorMax:"))
                {
                    fileIDs.anchorMax.x = LanguageClassTools.ExtractFloatValue(lineWithoutCurlyBraces, "XAnchorMax:");
                    fileIDs.anchorMax.y = LanguageClassTools.ExtractFloatValue(lineWithoutCurlyBraces, "YAnchorMax:");
                    fileIDs.anchorMaxWrite = true;
                }

                // Process anchored position components if present.
                if (lineWithoutCurlyBraces.Contains("X:") || lineWithoutCurlyBraces.Contains("Y:"))
                {
                    fileIDs.anchoredPosition.x = LanguageClassTools.ExtractFloatValue(lineWithoutCurlyBraces, "X:");
                    fileIDs.anchoredPosition.y = LanguageClassTools.ExtractFloatValue(lineWithoutCurlyBraces, "Y:");
                    fileIDs.anchoredPositionWrite = true;
                }

                // Process size delta components if present.
                if (lineWithoutCurlyBraces.Contains("Width:") || lineWithoutCurlyBraces.Contains("Height:"))
                {
                    fileIDs.sizeDelta.x = LanguageClassTools.ExtractFloatValue(lineWithoutCurlyBraces, "Width:");
                    fileIDs.sizeDelta.y = LanguageClassTools.ExtractFloatValue(lineWithoutCurlyBraces, "Height:");
                    fileIDs.sizeDeltaWrite = true;
                }

                // Process pivot components if present.
                if (lineWithoutCurlyBraces.Contains("XPivot:") || lineWithoutCurlyBraces.Contains("YPivot:"))
                {
                    fileIDs.pivot.x = LanguageClassTools.ExtractFloatValue(lineWithoutCurlyBraces, "XPivot:");
                    fileIDs.pivot.y = LanguageClassTools.ExtractFloatValue(lineWithoutCurlyBraces, "YPivot:");
                    fileIDs.pivotWrite = true;
                }

                // Process text components if present.
                if (line.Contains("{"))
                {
                    fileIDs.text = LanguageClassTools.ExtractTextWithinBraces(line); // Assigns the text of the file to the FileID object.
                    fileIDs.textWrite = true; // Sets TestWrite to true.
                }

                // Process font components if present.
                if (lineWithoutCurlyBraces.Contains("Font:"))
                {
                    fileIDs.font = LanguageClassTools.ExtractIntValue(lineWithoutCurlyBraces, "Font:");
                    fileIDs.fontWrite = true;
                }

                // Process font size components if present.
                if (lineWithoutCurlyBraces.Contains("S:"))
                {
                    fileIDs.fontSize = LanguageClassTools.ExtractIntValue(lineWithoutCurlyBraces, "S:");
                    fileIDs.fontSizeWrite = true;
                }

                // Process alignment components if present.
                if (lineWithoutCurlyBraces.Contains("Ali:"))
                {
                    fileIDs.alignment = LanguageClassTools.ExtractIntValue(lineWithoutCurlyBraces, "Ali:");
                    fileIDs.alignmentWrite = true;
                }

                // Process reverse components if present.
                if (lineWithoutCurlyBraces.Contains("Rev:"))
                {
                    fileIDs.reverse = LanguageClassTools.ExtractIntValue(lineWithoutCurlyBraces, "Rev:");
                    fileIDs.reverseWrite = true;
                }

                // Extracts the row ID and stores it in the corresponding FileID object.
                fileIDs.id = ID;
                this.fileIDs.Add(fileIDs);
            }
        }

        int itemsPerFrame = 1; // Maximum number of items processed per frame.
        int fileGroupIndex = 0; // Índice atual para processamento de FileGroups.
        int fileCommentsIndex = 0; // Current index for processing FileComments.
        int fileIDsIndex = 0; // Current index for processing FileIDs.
        int totalItems = fileGroups.Count + fileComments.Count + fileIDs.Count; // Total number of items to process.
        int processedItems = 0; // Number of items processed so far.

        // Process data as long as there are items in the lists.
        while (fileGroupIndex < fileGroups.Count || fileCommentsIndex < fileComments.Count || fileIDsIndex < fileIDs.Count)
        {
            // Process FileGroups.
            for (int i = 0; i < itemsPerFrame && fileGroupIndex < fileGroups.Count; i++, fileGroupIndex++)
            {
                FileGroups fileGroup = fileGroups[fileGroupIndex]; // Gets the current FileGroup.

                GameObject idGroupObj = Instantiate(prefabIDGroups, _object.transform);
                OrganizeIDGroups organizeIDGroups = idGroupObj.GetComponent<OrganizeIDGroups>();
                // Set the text of the "textID" field in the OrganizeIDGroups component to the text ID extracted from the file.
                organizeIDGroups.textID.text = fileGroup.textID.ToString();
                organizeIDGroups.textIDs.text = fileGroup.IDs;
                organizeIDGroups.texts.text = fileGroup.test;
            }

            // Process FileComments.
            for (int i = 0; i < itemsPerFrame && fileCommentsIndex < fileComments.Count; i++, fileCommentsIndex++)
            {
                FileComments fileComment = fileComments[fileCommentsIndex]; // Gets the current FileComments.

                GameObject CommentObj = Instantiate(prefabComments, _object.transform); // Creates a new instance of the PrefabComments object.
                OrganizeCommentsIDS organizeCommentsIDS = CommentObj.GetComponent<OrganizeCommentsIDS>(); // Gets the OrganizeCommentsIDS component of the CommentObj object.
                organizeCommentsIDS.textID.text = fileComment.comments.ToString(); // Defines the text of the Text component of the OrganizeCommentIDS instance.
                organizeCommentsIDS.Comment.text = fileComment.testComments; // Defines the text of the Comment component of the OrganizeCommentIDS instance.
            }

            // Process FileIDs.
            for (int i = 0; i < itemsPerFrame && fileIDsIndex < fileIDs.Count; i++, fileIDsIndex++)
            {
                FileID fileID = fileIDs[fileIDsIndex]; // Gets the current FileID.

                GameObject CommentObj = Instantiate(prefabIDEditor, _object.transform);
                OrganizeIDS organizeIDS = CommentObj.GetComponent<OrganizeIDS>();
                organizeIDS.textID = fileID.id;
                organizeIDS.spritesIndex = (int)fileID.componentType;

                // Sets the comment text, if any.
                organizeIDS.interactableText = fileID.textWrite;
                if (fileID.textWrite == true) { organizeIDS.text.text = fileID.text; }

                // Sets the font size of the comment, if any.
                organizeIDS.interactableFontSize = fileID.fontSizeWrite;
                if (fileID.fontSizeWrite == true) { organizeIDS.fontSize.text = fileID.fontSize.ToString(); }

                // Sets the comment font, if any.
                organizeIDS.interactableFont = fileID.fontWrite;
                if (fileID.fontWrite == true) { organizeIDS.font.text = fileID.font.ToString(); }

                // Sets the comment font, if any.
                organizeIDS.interactableRotation = fileID.rotationWrite;
                if (fileID.rotationWrite == true)
                {
                    organizeIDS.rotationX.text = fileID.rotation.x.ToString();
                    organizeIDS.rotationY.text = fileID.rotation.y.ToString();
                    organizeIDS.rotationZ.text = fileID.rotation.z.ToString();
                    organizeIDS.rotationW.text = fileID.rotation.w.ToString();
                }

                // Sets the comment font, if any.
                organizeIDS.interactableLocalScale = fileID.localScaleWrite;
                if (fileID.localScaleWrite == true)
                {
                    organizeIDS.localScaleX.text = fileID.localScale.x.ToString();
                    organizeIDS.localScaleY.text = fileID.localScale.y.ToString();
                    organizeIDS.localScaleZ.text = fileID.localScale.z.ToString();
                }

                // Sets the X position of the comment, if any.
                organizeIDS.interactableAnchoredPosition = fileID.anchoredPositionWrite;
                if (fileID.anchoredPositionWrite == true)
                {
                    organizeIDS.anchoredPositionX.text = fileID.anchoredPosition.x.ToString();
                    organizeIDS.anchoredPositionY.text = fileID.anchoredPosition.y.ToString();
                }

                // Sets the comment font, if any.
                organizeIDS.interactableAnchorMin = fileID.anchorMinWrite;
                if (fileID.anchorMinWrite == true)
                {
                    organizeIDS.anchorMinX.text = fileID.anchorMin.x.ToString();
                    organizeIDS.anchorMinY.text = fileID.anchorMin.y.ToString();
                }

                // Sets the comment font, if any.
                organizeIDS.interactableAnchorMax = fileID.anchorMaxWrite;
                if (fileID.anchorMaxWrite == true)
                {
                    organizeIDS.anchorMaxX.text = fileID.anchorMax.x.ToString();
                    organizeIDS.anchorMaxY.text = fileID.anchorMax.y.ToString();
                }

                // Sets the Y position of the comment, if any.
                organizeIDS.interactableSizeDelta = fileID.sizeDeltaWrite;
                if (fileID.sizeDeltaWrite == true)
                {
                    organizeIDS.sizeDeltaX.text = fileID.sizeDelta.x.ToString();
                    organizeIDS.sizeDeltaY.text = fileID.sizeDelta.y.ToString();
                }

                // Sets the width of the comment, if any.
                organizeIDS.interactablePivit = fileID.pivotWrite;
                if (fileID.pivotWrite == true)
                {
                    organizeIDS.pivitX.text = fileID.pivot.x.ToString();
                    organizeIDS.pivitY.text = fileID.pivot.y.ToString();
                }

                // Sets the interactive alignment capability according to the option selected in the configuration file.
                organizeIDS.interactableAlignment = fileID.alignmentWrite;
                if (fileID.alignmentWrite == true) { organizeIDS.alignmentValue = Convert.ToInt32(fileID.alignment); }

                // Sets the interactive inversion capability according to the option selected in the configuration file.
                organizeIDS.interactableReverse = fileID.reverseWrite;
                if (fileID.reverseWrite == true) { organizeIDS.reverseValue = Convert.ToInt32(fileID.reverse); }
            }

            processedItems = fileGroupIndex + fileCommentsIndex + fileIDsIndex; // Updates the number of items processed.           
            float percentage = (float)processedItems / totalItems * 100f; // Calculates charging percentage.
            loadingPercentage.text = $"{percentage:F1}%"; // Update the text field with the loading percentage.

            yield return null; // Pause until next frame.
        }

        // After creating all objects, wait a second and update the UI.
        loadingScreen.SetActive(false); // Hide the loading screen.
        yield return new WaitForSeconds(1); // Wait for 1 second before updating the UI.

        // Update the UI elements.
        UpdateOrganize scriptEncontrado = FindObjectOfType<UpdateOrganize>();
        scriptEncontrado.UpdateOrganization();
    }
}

[System.Serializable]
public class FileID
{
    [Header("Custom ID")]
    public float id; // Unique ID for each file.
    public float componentType; // Type of the component.
    [Space(5)]
    [Header("Rect Transform")]
    public Quaternion rotation; // Rotation of the object.
    public bool rotationWrite; // Flag indicating if rotation is to be written.
    public Vector3 localScale; // Scale of the object.
    public bool localScaleWrite; // Flag indicating if local scale is to be written.
    [Space(5)]
    public Vector2 anchorMin; // Minimum anchor position.
    public bool anchorMinWrite; // Flag indicating if anchor min is to be written.
    public Vector2 anchorMax; // Maximum anchor position.
    public bool anchorMaxWrite; // Flag indicating if anchor max is to be written.
    public Vector2 anchoredPosition; // Anchored position.
    public bool anchoredPositionWrite; // Flag indicating if anchored position is to be written.
    public Vector2 sizeDelta; // Size delta.
    public bool sizeDeltaWrite; // Flag indicating if size delta is to be written.
    public Vector2 pivot; // Pivot.
    public bool pivotWrite; // Flag indicating if pivot is to be written.
    [Space(5)]
    [Header("Text")]
    public string text; // Text content.
    public bool textWrite; // Flag indicating if text is to be written.
    [Space(5)]
    [Header("Character")]
    public float font; // Font used.
    public bool fontWrite; // Flag indicating if font is to be written.
    public float fontSize; // Font size.
    public bool fontSizeWrite; // Flag indicating if font size is to be written.
    [Space(5)]
    [Header("Paragraph")]
    public float alignment; // Alignment.
    public bool alignmentWrite; // Flag indicating if alignment is to be written.
    public float reverse; // Reverse value.
    public bool reverseWrite; // Flag indicating if reverse value is to be written.
}

[System.Serializable]
public class FileGroups
{
    [Header("Information")]
    public float textID; // Unique ID for each text group.
    [Space(5)]
    public string IDs; // Sample text for this text group.
    public string test; // Semicolon-separated IDs associated with this text group.
}

[System.Serializable]
public class FileComments
{
    [Header("Information")]
    public float comments; // Comment ID.
    [Space(5)]
    public string testComments; // Comment text.
}