using UnityEngine;
using UnityEngine.UI;
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

[AddComponentMenu("Language/UI/Legacy/Language Dropdown")]
public class LanguageDropdown : MonoBehaviour
{
    #if UNITY_EDITOR
    [CustomEditor(typeof(LanguageDropdown))]
    public class LanguageDropdownEditor : Editor
    {
        public static List<float> IDs = new(); // List to store IDs.
        private bool savedID; // Flag to track if an ID is saved.

        private void OnEnable()
        {
            LanguageClassToolsEditor.LoadDataFromFile(ref IDs); // Load data from file when editor is enabled.
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update(); // Update the serialized object.
            LanguageDropdown script = (LanguageDropdown)target; // Get the target LanguageDropdown script.

            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.ObjectField("Script", MonoScript.FromMonoBehaviour((MonoBehaviour)target), typeof(MonoBehaviour), false); // Display the script field (read-only).
            EditorGUI.EndDisabledGroup();

            EditorGUILayout.Space(15f); // Add space in the inspector.

            // Button to import settings.
            if (GUILayout.Button("Import Settings"))
            {
                script.SavaDropdown(); // Save dropdown settings.
                LanguageClassToolsEditor.SaveAdjustRect(ref script.adjustRectList); // Save adjustment settings.
                EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene()); // Mark scene as dirty.
                LanguageClassToolsEditor.LoadDataFromFile(ref IDs); // Load data from file.
            }

            // Button to save settings.
            if (GUILayout.Button("Save Settings"))
            {
                bool shouldSave = true;

                if (savedID == true)
                {
                    // Confirmation dialog for saving.
                    shouldSave = EditorUtility.DisplayDialog("Save Confirmation", "An ID with this value already exists. Are you sure you want to save?\nThis action will overwrite the currently saved ID.", "Yes", "No");
                }

                if (shouldSave)
                {
                    EditorApplication.ExecuteMenuItem("Window/Language/Language Save Editor"); // Execute menu item.
                    AddLanguageSaveID(script); // Add language save ID.
                }
            }

            // Display property fields for languageDropdown, itemText, and captionText.
            EditorGUILayout.PropertyField(serializedObject.FindProperty("languageDropdown"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("itemText"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("captionText"));

            // Display property field for options and check for saved IDs.
            EditorGUILayout.PropertyField(serializedObject.FindProperty("options"));
            foreach (var option in script.options)
            {
                if (IDs.Contains(option.ID))
                {
                    savedID = true;
                    GUI.color = Color.yellow;
                    EditorGUILayout.HelpBox($"ID: {option.ID}\nThere is an ID with this number Saved!", MessageType.Warning);
                    GUI.color = Color.white;
                }
                else
                {
                    savedID = false;
                }
            }

            // Check if the script's ID is in the list of saved IDs.
            if (LanguageClassToolsEditor.IsIDInTextIDList(script.ID, ref IDs))
            {
                savedID = true;
                GUI.color = Color.yellow;
                EditorGUILayout.PropertyField(serializedObject.FindProperty("ID"));
                EditorGUILayout.HelpBox("There is an ID with this number Saved!", MessageType.Warning);
                GUI.color = Color.white;
            }
            else
            {
                savedID = false;
                EditorGUILayout.PropertyField(serializedObject.FindProperty("ID"));
            }

            // Display property field for adjustRectList and export IDs to AdjustRect if the list is not empty.
            EditorGUILayout.PropertyField(serializedObject.FindProperty("adjustRectList"));
            if (script.adjustRectList != null && script.adjustRectList.Count > 0)
            {
                if (GUILayout.Button("Export IDs to AdjustRect"))
                {
                    LanguageClassToolsEditor.ExportIDsToAdjustRect(script.adjustRectList); // Button to export IDs to AdjustRect.
                }

                // Display warnings for IDs found in adjustRectList.
                foreach (var adjustRectList in script.adjustRectList)
                {
                    if (IDs.Contains(adjustRectList.ID))
                    {
                        savedID = true;
                        GUI.color = Color.yellow;
                        EditorGUILayout.HelpBox($"ID: {adjustRectList.ID}\nThere is an ID with this number Saved!", MessageType.Warning);
                        GUI.color = Color.white;
                    }
                    else
                    {
                        savedID = false;
                    }
                }
            }

            // Display property fields for various settings related to text, rotation, scale, position, etc.
            EditorGUILayout.PropertyField(serializedObject.FindProperty("font"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("fontListObject"));

            SerializedProperty quaternionProp = serializedObject.FindProperty("rotation");
            script.isQuaternionFoldout = EditorGUILayout.Foldout(quaternionProp.isExpanded, "Rotation");

            quaternionProp.isExpanded = script.isQuaternionFoldout;
            if (script.isQuaternionFoldout)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(quaternionProp.FindPropertyRelative("x"));
                EditorGUILayout.PropertyField(quaternionProp.FindPropertyRelative("y"));
                EditorGUILayout.PropertyField(quaternionProp.FindPropertyRelative("z"));
                EditorGUILayout.PropertyField(quaternionProp.FindPropertyRelative("w"));
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.PropertyField(serializedObject.FindProperty("localScale"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("anchorMin"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("anchorMax"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("anchoredPosition"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("sizeDelta"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("pivot"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("alignment"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("reverseText"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("standardFile"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("selectedFile"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("savePath"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("defaultFile"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("jsonNameInUnity"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("folderNameInUnity"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("jsonNameInBuild"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("folderNameInBuild"));

            serializedObject.ApplyModifiedProperties(); // Apply modified properties.
        }

        // Method to add or update language save IDs.
        public void AddLanguageSaveID(LanguageDropdown script)
        {
            var window = EditorWindow.GetWindow<LanguageSaveEditorWindow>(); // Get the LanguageSaveEditorWindow instance.

            // Check if an existing language save ID matches the script's ID and update its properties.
            var existingLanguageSaveID = window.textID.FirstOrDefault(id => id.ID == script.ID);
            if (existingLanguageSaveID != null)
            {
                // Update properties of the existing language save ID with the script's values.
                LanguageClassToolsEditor.ClearExistingLanguageSaveIDValues(existingLanguageSaveID);
                existingLanguageSaveID.componentType = 6;
                existingLanguageSaveID.rotation = script.rotation;
                existingLanguageSaveID.rotationWrite = true;
                existingLanguageSaveID.localScale = script.localScale;
                existingLanguageSaveID.localScaleWrite = true;
                existingLanguageSaveID.anchorMin = script.anchorMin;
                existingLanguageSaveID.anchorMinWrite = true;
                existingLanguageSaveID.anchorMax = script.anchorMax;
                existingLanguageSaveID.anchorMaxWrite = true;
                existingLanguageSaveID.anchoredPosition = script.anchoredPosition;
                existingLanguageSaveID.anchoredPositionWrite = true;
                existingLanguageSaveID.sizeDelta = script.sizeDelta;
                existingLanguageSaveID.sizeDeltaWrite = true;
                existingLanguageSaveID.pivot = script.pivot;
                existingLanguageSaveID.pivotWrite = true;
                existingLanguageSaveID.fontWrite = true;
                existingLanguageSaveID.font = script.font;
                existingLanguageSaveID.alignmentWrite = true;
                existingLanguageSaveID.alignment = script.alignment;
                existingLanguageSaveID.reverseWrite = true;
                existingLanguageSaveID.reverse = script.reverseText ? 2 : 1;
            }
            else
            {
                // Create a new language save ID and populate its properties with script's values.
                var languageSaveID = new LanguageSaveID
                {
                    ID = script.ID,
                    componentType = 6,
                    rotation = script.rotation,
                    rotationWrite = true,
                    localScale = script.localScale,
                    localScaleWrite = true,
                    anchorMin = script.anchorMin,
                    anchorMinWrite = true,
                    anchorMax = script.anchorMax,
                    anchorMaxWrite = true,
                    anchoredPosition = script.anchoredPosition,
                    anchoredPositionWrite = true,
                    sizeDelta = script.sizeDelta,
                    sizeDeltaWrite = true,
                    pivot = script.pivot,
                    pivotWrite = true,
                    fontWrite = true,
                    font = script.font,
                    alignmentWrite = true,
                    alignment = script.alignment,
                    reverseWrite = true,
                    reverse = script.reverseText ? 2 : 1
                };
                window.textID.Add(languageSaveID); // Add the new language save ID to the window's list.
            }

            // Update or add language options' IDs and text values in the window's textID list.
            foreach (LanguageOptions id in script.options)
            {
                // Check if an existing language save ID for options matches the script's option ID and update its properties.
                var existingLanguageSaveIDForOptions = window.textID.FirstOrDefault(existingID => existingID.ID == id.ID);
                if (existingLanguageSaveIDForOptions != null)
                {
                    // Update properties of the existing language save ID for options with the script's option values.
                    LanguageClassToolsEditor.ClearExistingLanguageSaveIDValues(existingLanguageSaveIDForOptions);
                    existingLanguageSaveIDForOptions.componentType = 6;
                    existingLanguageSaveIDForOptions.text = id.text;
                    existingLanguageSaveIDForOptions.textWrite = true;
                }
                else
                {
                    // Create a new language save ID for options and populate its properties with script's option values.
                    var languageSaveID2 = new LanguageSaveID
                    {
                        ID = id.ID,
                        componentType = 6,
                        text = id.text,
                        textWrite = true
                    };
                    window.textID.Add(languageSaveID2); // Add the new language save ID for options to the window's list.
                }
            }

            // Update adjustRectList IDs and properties in the window's textID list if the list is not empty.
            if (script.adjustRectList != null && script.adjustRectList.Count > 0)
            {
                foreach (AdjustRectList id in script.adjustRectList)
                {
                    // Check if an existing language save ID for adjustRectList matches the script's adjustRectList ID and update its properties.
                    var ListExistingSaveID = window.textID.FirstOrDefault(existingID => existingID.ID == id.ID);
                    if (ListExistingSaveID != null)
                    {
                        // Update properties of the existing language save ID for adjustRectList with the script's adjustRectList values.
                        LanguageClassToolsEditor.ClearExistingLanguageSaveIDValues(ListExistingSaveID);
                        ListExistingSaveID.ID = id.ID;
                        ListExistingSaveID.componentType = 7;
                        ListExistingSaveID.rotation = id.rotation;
                        ListExistingSaveID.localScale = id.localScale;
                        ListExistingSaveID.anchorMin = id.anchorMin;
                        ListExistingSaveID.anchorMax = id.anchorMax;
                        ListExistingSaveID.anchoredPosition = id.anchoredPosition;
                        ListExistingSaveID.sizeDelta = id.sizeDelta;
                        ListExistingSaveID.pivot = id.pivot;
                        ListExistingSaveID.rotationWrite = true;
                        ListExistingSaveID.localScaleWrite = true;
                        ListExistingSaveID.anchorMinWrite = true;
                        ListExistingSaveID.anchorMaxWrite = true;
                        ListExistingSaveID.anchoredPositionWrite = true;
                        ListExistingSaveID.sizeDeltaWrite = true;
                        ListExistingSaveID.pivotWrite = true;
                    }
                    else
                    {
                        // Create a new language save ID for adjustRectList and populate its properties with script's adjustRectList values.
                        var languageSaveID = new LanguageSaveID
                        {
                            ID = id.ID,
                            componentType = 7,
                            rotation = id.rotation,
                            localScale = id.localScale,
                            anchorMin = id.anchorMin,
                            anchorMax = id.anchorMax,
                            anchoredPosition = id.anchoredPosition,
                            sizeDelta = id.sizeDelta,
                            pivot = id.pivot,
                            rotationWrite = true,
                            localScaleWrite = true,
                            anchorMinWrite = true,
                            anchorMaxWrite = true,
                            anchoredPositionWrite = true,
                            sizeDeltaWrite = true,
                            pivotWrite = true
                        };
                        window.textID.Add(languageSaveID); // Add the new language save ID for adjustRectList to the window's list.
                    }
                }
            }

            window.SaveDataJson(); // Save updated data to JSON.
            LanguageClassToolsEditor.LoadDataFromFile(ref IDs); // Reload data from file and update IDs list.
        }
    }
    #endif

    [Header("Settings")]
    public Dropdown languageDropdown; // Reference to the Dropdown UI component to show language options.
    public Text itemText; // Reference to the Text component used for displaying the selected item.
    public Text captionText; // Reference to the Text component used for displaying the selected caption.
    public List<LanguageOptions> options = new(){new LanguageOptions { text = "Option A", sprite = null, ID = -3 }, new LanguageOptions { text = "Option B", sprite = null, ID = -4 }, new LanguageOptions { text = "Option C", sprite = null, ID = -5 }}; // List of LanguageOptions representing language options.
    [SerializeField] private float ID = -6; // ID to identify the language options.
    [SerializeField] private List<AdjustRectList> adjustRectList; // List of adjustments for UI elements.
    [Space(10)]
    [Header("Text Settings")]
    [SerializeField] private int font; // The index of the font to be used for displaying the text.
    public LanguageFontListData fontListObject; // Reference to the LanguageFontListData scriptable object containing font information.
    [Space(5)]
    [SerializeField] private Quaternion rotation; // Rotation of the text.
    [SerializeField] private Vector3 localScale; // Scale of the text.
    [Space(5)]
    [SerializeField] private Vector2 anchorMin; // Minimum anchor position.
    [SerializeField] private Vector2 anchorMax; // Maximum anchor position.
    [SerializeField] private Vector2 anchoredPosition; // Anchored position.
    [SerializeField] private Vector2 sizeDelta; // Size delta of the text.
    [SerializeField] private Vector2 pivot; // Pivot point of the text.
    [Space(5)]
    [SerializeField] private int alignment; // The alignment of the text.
    [SerializeField] private bool reverseText = false; // Flag to determine whether to reverse the text.
    [Space(10)]
    [Header("Default language if there is no save")]
    [SerializeField] private string standardFile = "Linguagem - [ENGLISH]"; // The default language file to use if no save data is available.
    [Space(10)]
    [Header("Automatic Information")]
    [SerializeField] private string selectedFile; // The path to the currently selected language file.
    [SerializeField] private string savePath; // The path to the JSON file where the selected language will be saved.
    [SerializeField] private string defaultFile; // The path to the default language file.
    [Space(10)]
    [Header("Archives Location")]
    public string jsonNameInUnity = "/Language/Editor/LanguageFileSave.json"; // Path to the JSON file for language selection in Unity Editor.
    public string folderNameInUnity = "/StreamingAssets/Language/"; // Folder containing language files in Unity Editor.
    [Space(10)]
    public string jsonNameInBuild = "/LanguageFileSave.json"; // Path to the JSON file for language selection in the build.
    public string folderNameInBuild = "/StreamingAssets/Language/"; // Folder containing language files in the build.

    private int previousSelectedIndex = -1; // The index of the previously selected item in the Dropdown.
    private bool foundID; // Flag to track whether the language text option ID was found.
    private bool isQuaternionFoldout; // Flag to control quaternion foldout in the inspector.

    private void Start()
    {
        itemText = languageDropdown.itemText; // Get the Text component of the Dropdown item.
        captionText = languageDropdown.captionText; // Get the Text component of the Dropdown caption.

        LanguageUpdate(); // Update the language options.
    }

    private void OnEnable()
    {
        LanguageUpdate(); // Update the language options when the component is enabled.
    }

    public void LanguageUpdate()
    {
        string path = LanguageClassTools.GetFolderPath(folderNameInUnity, folderNameInBuild); // Set the path to locate language files in Unity Editor or in the build.
        savePath = LanguageClassTools.GetJsonPath(jsonNameInUnity, jsonNameInBuild); // Set the save path for the Unity Editor or for the build.
        defaultFile = LanguageClassTools.FindDefaultLanguageFile(path, standardFile); // Find the default language file based on the StandardFile setting.

        // Restart all iterations.
        foundID = false;
        foreach (LanguageOptions p in options)
        {
            p.foundID = false;
        }

        if (File.Exists(savePath))
        {
            string json = File.ReadAllText(savePath);
            LanguageFileSave fileSaveData = JsonUtility.FromJson<LanguageFileSave>(json);
            selectedFile = fileSaveData.selectedFile; // Get the selected language file from the save data.

            ReadFile(selectedFile); // Read and process the language file.

            languageDropdown.value = previousSelectedIndex; // Set the value of the Dropdown to the previously selected index.
            languageDropdown.RefreshShownValue(); // Refresh the Dropdown UI to show the selected language.
        }
        else
        {
            selectedFile = defaultFile; // Use the default language file.
            ReadFile(defaultFile); // Read and process the default language file.
        }
    }

    private void ReadFile(string filePath)
    {
        string[] lines = File.ReadAllLines(filePath); // Read and process the language file specified by filePath.

        // Iterate through each line in the file.
        foreach (string line in lines)
        {
            if (line.StartsWith("id:"))
            {
                ProcessLanguageOption(line); // Process the language option.
                if (foundID)
                {
                    break;
                }
            }
        }

        // Update specific language option text based on ID.
        foreach (LanguageOptions p in options)
        {
            if (p.foundID)
            {
                break;
            }

            foreach (string line in lines)
            {
                if (line.StartsWith("id:"))
                {
                    string[] parts = line.Split(';');
                    float id = float.Parse(parts[0].Replace("id:", ""));

                    if (p.ID == id)
                    {
                        p.foundID = true;

                        string text = LanguageClassTools.ExtractTextWithinBraces(line);

                        // Reverse the text if necessary.
                        if (reverseText)
                        {
                            char[] charArray = text.ToCharArray();
                            Array.Reverse(charArray);
                            text = new string(charArray);
                        }
                        p.text = text;
                    }
                }
            }
        }

        ProcessOption(); // Calls the method to process the options.
    }

    // Method to process options after reading the file.
    private void ProcessOption()
    {
        previousSelectedIndex = languageDropdown.value; // Store the previous selected index.
        languageDropdown.ClearOptions(); // Clear the dropdown options.

        // Add the updated language options to the dropdown.
        foreach (LanguageOptions languageOption in options)
        {
            Dropdown.OptionData optionData = new()
            {
                text = languageOption.text,
                image = languageOption.sprite
            };
            languageDropdown.options.Add(optionData);
        }
    }

    // Method for processing a language option defined on the given line.
    private void ProcessLanguageOption(string line)
    {
        // Split the line into parts separated by ';'.
        string[] parts = line.Split(';');
        float id = float.Parse(parts[0].Replace("id:", ""));

        if (ID != id)
        {
            return; // Skip processing if the ID does not match the current language text option ID.
        }

        foundID = true; // Mark that the language option ID has been found.

        // Extract and process various properties of the language option.
        string lineWithoutCurlyBraces = LanguageClassTools.RemoveContentWithinCurlyBraces(line);
        font = LanguageClassTools.ExtractIntValue(lineWithoutCurlyBraces, "Font:");
        alignment = LanguageClassTools.ExtractIntValue(lineWithoutCurlyBraces, "Ali:");
        reverseText = LanguageClassTools.ExtractBooleanValue(lineWithoutCurlyBraces, "Rev:", reverseText);

        string[] prefixes = { "XRotation:", "YRotation:", "ZRotation:", "WRotation:", "XScale:", "YScale:", "ZScale:", "XAnchorMin:", "YAnchorMin:", "XAnchorMax:", "YAnchorMax:", "X:", "Y:", "Width:", "Height:", "XPivot:", "YPivot:" };

        if (prefixes.Any(prefix => line.Contains(prefix)))
        {
            rotation.x = LanguageClassTools.ExtractFloatValue(lineWithoutCurlyBraces, "XRotation:");
            rotation.y = LanguageClassTools.ExtractFloatValue(lineWithoutCurlyBraces, "YRotation:");
            rotation.z = LanguageClassTools.ExtractFloatValue(lineWithoutCurlyBraces, "ZRotation:");
            rotation.w = LanguageClassTools.ExtractFloatValue(lineWithoutCurlyBraces, "WRotation:");
            localScale.x = LanguageClassTools.ExtractFloatValue(lineWithoutCurlyBraces, "XScale:");
            localScale.y = LanguageClassTools.ExtractFloatValue(lineWithoutCurlyBraces, "YScale:");
            localScale.z = LanguageClassTools.ExtractFloatValue(lineWithoutCurlyBraces, "ZScale:");
            anchorMin.x = LanguageClassTools.ExtractFloatValue(lineWithoutCurlyBraces, "XAnchorMin:");
            anchorMin.y = LanguageClassTools.ExtractFloatValue(lineWithoutCurlyBraces, "YAnchorMin:");
            anchorMax.x = LanguageClassTools.ExtractFloatValue(lineWithoutCurlyBraces, "XAnchorMax:");
            anchorMax.y = LanguageClassTools.ExtractFloatValue(lineWithoutCurlyBraces, "YAnchorMax:");
            anchoredPosition.x = LanguageClassTools.ExtractFloatValue(lineWithoutCurlyBraces, "X:");
            anchoredPosition.y = LanguageClassTools.ExtractFloatValue(lineWithoutCurlyBraces, "Y:");
            sizeDelta.x = LanguageClassTools.ExtractFloatValue(lineWithoutCurlyBraces, "Width:");
            sizeDelta.y = LanguageClassTools.ExtractFloatValue(lineWithoutCurlyBraces, "Height:");
            pivot.x = LanguageClassTools.ExtractFloatValue(lineWithoutCurlyBraces, "XPivot:");
            pivot.y = LanguageClassTools.ExtractFloatValue(lineWithoutCurlyBraces, "YPivot:");
        }

        // Sets various properties of the language text component based on the extracted values.
        if (fontListObject != null && font > 0 && font <= fontListObject.fontList.Count)
        {
            itemText.font = fontListObject.fontList[font - 1];
            captionText.font = fontListObject.fontList[font - 1];
        }

        if (languageDropdown.TryGetComponent<RectTransform>(out var rectTransform))
        {
            if (rotation.x != 0 || rotation.y != 0 || rotation.z != 0 || rotation.w != 0)
            { rectTransform.localRotation = new Quaternion(rotation.x, rotation.y, rotation.z, rotation.w); }

            if (localScale.x != 0 || localScale.y != 0 || localScale.z != 0)
            { rectTransform.localScale = new Vector3(localScale.x, localScale.y, localScale.z); }

            if (anchorMin.x != 0 || anchorMin.y != 0)
            { rectTransform.anchorMin = new Vector2(anchorMin.x, anchorMin.y); }

            if (anchorMax.x != 0 || anchorMax.y != 0)
            { rectTransform.anchorMax = new Vector2(anchorMax.x, anchorMax.y); }

            if (anchoredPosition.x != 0 || anchoredPosition.y != 0)
            { rectTransform.anchoredPosition = new Vector2(anchoredPosition.x, anchoredPosition.y); }

            if (sizeDelta.x != 0 || sizeDelta.y != 0)
            { rectTransform.sizeDelta = new Vector2(sizeDelta.x, sizeDelta.y); }

            if (pivot.x != 0 || pivot.y != 0)
            { rectTransform.pivot = new Vector2(pivot.x, pivot.y); }
        }

        SetTextAlignment(); // Set the alignment of the language text component.
    }

    private void SetTextAlignment()
    {
        // Set the alignment of the language text component based on the alignment property.
        if (alignment >= 1 && alignment <= 3)
        {
            // Gets the current vertical alignment of the text component.
            string verticalAlignment = TextAlignmentConverter.GetVerticalAlignmentFromTextAlignment(itemText.alignment);

            // Sets the new alignment based on the value of the 'alignment' property.
            // Vertical alignment is determined by the current value of the 'alignment' property.
            // If 'alignment' is 1, the text will be left aligned at the top (Upper), in the middle (Middle) or at the bottom (Lower).
            // If 'alignment' is 2, the text will be aligned to the center (Center) at the top (Upper), in the middle (Middle) or at the bottom (Lower).
            // If 'alignment' is 3, the text will be aligned to the right (Right), top (Upper), middle (Middle) or bottom (Lower).
            TextAnchor newAlignment = TextAnchor.UpperLeft;
            if (alignment == 1)
            {
                newAlignment = (verticalAlignment == "Upper") ? TextAnchor.UpperLeft : (verticalAlignment == "Middle") ? TextAnchor.MiddleLeft : TextAnchor.LowerLeft;
            }
            else if (alignment == 2)
            {
                newAlignment = (verticalAlignment == "Upper") ? TextAnchor.UpperCenter : (verticalAlignment == "Middle") ? TextAnchor.MiddleCenter : TextAnchor.LowerCenter;
            }
            else if (alignment == 3)
            {
                newAlignment = (verticalAlignment == "Upper") ? TextAnchor.UpperRight : (verticalAlignment == "Middle") ? TextAnchor.MiddleRight : TextAnchor.LowerRight;
            }

            // Sets the new alignment in the text component.
            itemText.alignment = newAlignment;
            captionText.alignment = newAlignment;
        }
    }

    public void SavaDropdown()
    {        
        RectTransform rectTransform = languageDropdown.GetComponent<RectTransform>(); // Get the RectTransform for positioning and sizing.

        // Save the positioning and sizing information.
        rotation = rectTransform.localRotation;
        localScale = rectTransform.localScale;
        anchorMin = rectTransform.anchorMin;
        anchorMax = rectTransform.anchorMax;
        anchoredPosition = rectTransform.anchoredPosition;
        sizeDelta = rectTransform.sizeDelta;
        pivot = rectTransform.pivot;

        Text text = languageDropdown.captionText.GetComponent<Text>();

        // Determine the alignment index for the text.
        if (text.alignment == TextAnchor.UpperLeft || text.alignment == TextAnchor.MiddleLeft || text.alignment == TextAnchor.LowerLeft)
        {
            alignment = 1; // Alignment index for left-aligned text.
        }
        else if (text.alignment == TextAnchor.UpperCenter || text.alignment == TextAnchor.MiddleCenter || text.alignment == TextAnchor.LowerCenter)
        {
            alignment = 2; // Alignment index for center-aligned text.
        }
        else if (text.alignment == TextAnchor.UpperRight || text.alignment == TextAnchor.MiddleRight || text.alignment == TextAnchor.LowerRight)
        {
            alignment = 3; // Alignment index for right-aligned text.
        }

        // Determine the font index for the text.
        Font fontTemp = text.font;
        int index = fontListObject.fontList.IndexOf(fontTemp) + 1;
        if (index > 0) { font = index; }
    }
}

[System.Serializable]
public class LanguageOptions
{
    public string text; // The text of the language option.
    public Sprite sprite; // The sprite (if any) associated with the language option.
    public float ID = 0; // The ID to identify the specific language option.
    [HideInInspector] public bool foundID; // Flag to track if the language text option ID has been found for this option.
}