using UnityEngine;
using TMPro;
using System;
using System.IO;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using System.Linq;
#endif

[AddComponentMenu("Language/UI/TextMesh Pro/Language Text (TMP)")]
public class TMP_LanguageText : MonoBehaviour
{
    #if UNITY_EDITOR
    [CustomEditor(typeof(TMP_LanguageText))]
    public class TMP_LanguageTextEditor : Editor
    {
        public static List<float> IDs = new(); // List to store IDs of language options.
        private bool savedID; // Flag to track if an ID is saved.

        private void OnEnable()
        {
            LanguageClassToolsEditor.LoadDataFromFile(ref IDs); // Load data from file into the IDs list.
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update(); // Update the serialized object.
            TMP_LanguageText script = (TMP_LanguageText)target; // Get the target LanguageText component.

            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.ObjectField("Script", MonoScript.FromMonoBehaviour((MonoBehaviour)target), typeof(MonoBehaviour), false);
            EditorGUI.EndDisabledGroup();

            EditorGUILayout.Space(15f); // Add space in the inspector.

            // Import Settings button.
            if (GUILayout.Button("Import Settings"))
            {
                script.SavaText(); // Save text properties.
                LanguageClassToolsEditor.SaveAdjustRect(ref script.adjustRectList); // Save adjustments.
                EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene()); // Mark the scene as dirty.
                LanguageClassToolsEditor.LoadDataFromFile(ref IDs); // Load data from file into the IDs list.
                script.wasImported = true; // Set the flag to indicate settings were imported.
            }

            // Save Settings button.
            GUI.enabled = script.wasImported; // Enable the button only if settings were previously imported.
            if (GUILayout.Button("Save Settings"))
            {
                bool shouldSave = true;

                if (savedID == true)
                {
                    // Confirmation dialog if ID already exists.
                    shouldSave = EditorUtility.DisplayDialog("Save Confirmation", "An ID with this value already exists. Are you sure you want to save?\nThis action will overwrite the currently saved ID.", "Yes", "No");
                }

                if (shouldSave)
                {
                    EditorApplication.ExecuteMenuItem("Window/Language/Language Save Editor"); // Execute menu item.
                    AddLanguageSaveID(script); // Add language save ID.
                }
            }
            GUI.enabled = true; // Re-enable the GUI after the button.

            EditorGUILayout.PropertyField(serializedObject.FindProperty("TMP_languageText")); // Display languageText property field.

            // Check if ID is saved and display warning if found.
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

            // Display other properties in the inspector.
            EditorGUILayout.PropertyField(serializedObject.FindProperty("translateText"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("fontSize"));
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

            serializedObject.ApplyModifiedProperties(); // Apply modified properties to the serialized object.
        }

        public void AddLanguageSaveID(TMP_LanguageText script)
        {
            var window = EditorWindow.GetWindow<LanguageSaveEditorWindow>(); // Get the LanguageSaveEditorWindow instance.

            var existingLanguageSaveID = window.textID.FirstOrDefault(id => id.ID == script.ID); // Find existing LanguageSaveID with the same ID.

            if (existingLanguageSaveID != null)
            {
                // Update existing LanguageSaveID with new values.
                LanguageClassToolsEditor.ClearExistingLanguageSaveIDValues(existingLanguageSaveID);
                existingLanguageSaveID.componentType = 1;
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
                existingLanguageSaveID.textWrite = true;
                existingLanguageSaveID.text = script.TMP_languageText.text;
                existingLanguageSaveID.fontWrite = true;
                existingLanguageSaveID.font = script.font;
                existingLanguageSaveID.fontSizeWrite = true;
                existingLanguageSaveID.fontSize = script.fontSize;
                existingLanguageSaveID.alignmentWrite = true;
                existingLanguageSaveID.alignment = script.alignment;
                existingLanguageSaveID.reverseWrite = true;
                existingLanguageSaveID.reverse = script.reverseText ? 2 : 1;
            }
            else
            {
                // Create a new LanguageSaveID if ID doesn't exist.
                var languageSaveID = new LanguageSaveID
                {
                    ID = script.ID,
                    componentType = 1,
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
                    textWrite = true,
                    text = script.TMP_languageText.text,
                    fontWrite = true,
                    font = script.font,
                    fontSizeWrite = true,
                    fontSize = script.fontSize,
                    alignmentWrite = true,
                    alignment = script.alignment,
                    reverseWrite = true,
                    reverse = script.reverseText ? 2 : 1
                };
                window.textID.Add(languageSaveID); // Add the new LanguageSaveID to the list.
            }

            // Update AdjustRectList IDs.
            if (script.adjustRectList != null && script.adjustRectList.Count > 0)
            {
                foreach (AdjustRectList id in script.adjustRectList)
                {
                    var ListExistingSaveID = window.textID.FirstOrDefault(existingID => existingID.ID == id.ID);

                    if (ListExistingSaveID != null)
                    {
                        // Update existing LanguageSaveID with adjustRectList values.
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
                        // Create a new LanguageSaveID for adjustRectList if ID doesn't exist.
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
                        window.textID.Add(languageSaveID); // Add the new LanguageSaveID to the list.
                    }
                }
            }

            window.SaveDataJson(); // Save data to file.
            LanguageClassToolsEditor.LoadDataFromFile(ref IDs); // Load data from file into the IDs list.
        }
    }
    #endif

    [Header("Settings")]
    public TMP_Text TMP_languageText; // Reference to the Text component for displaying the localized text.
    public float ID = -12; // ID to identify the specific language text option.
    public List<AdjustRectList> adjustRectList; // List of adjustments for UI elements.
    [Space(10)]
    [Header("Text Settings")]
    [SerializeField][Tooltip("Disable Text if Necessary.")] private bool translateText = true; // Flag to enable/disable text translation.
    public int fontSize; // The font size of the text.
    public int font; // The index of the font to be used for displaying the text.
    public TMP_LanguageFontListData fontListObject; // Reference to the LanguageFontListData scriptable object containing font information.
    [Space(5)]
    public Quaternion rotation; // Rotation of the text.
    public Vector3 localScale; // Scale of the text.
    [Space(5)]
    public Vector2 anchorMin; // Minimum anchor position.
    public Vector2 anchorMax; // Maximum anchor position.
    public Vector2 anchoredPosition; // Anchored position.
    public Vector2 sizeDelta; // Size delta of the text.
    public Vector2 pivot; // Pivot point of the text.
    [Space(5)]
    public int alignment; // The alignment of the text.
    public bool reverseText = false; // Flag to determine whether to reverse the text.
    [Space(10)]
    [Header("Default language if there is no save")]
    [SerializeField] private string standardFile = "Linguagem - [ENGLISH]"; // The default language file to use if no save data is available.
    [Space(10)]
    [Header("Automatic Information")]
    [SerializeField] private string selectedFile; // The path to the currently selected language file.
    [SerializeField] private string savePath; // The path to the JSON file where the selected language will be saved
    [SerializeField] private string defaultFile; // The path to the default language file.
    [Space(10)]
    [Header("Archives Location")]
    public string jsonNameInUnity = "/Language/Editor/LanguageFileSave.json"; // Path to the JSON file for language selection in Unity Editor.
    public string folderNameInUnity = "/StreamingAssets/Language/"; // Folder containing language files in Unity Editor.
    [Space(10)]
    public string jsonNameInBuild = "/LanguageFileSave.json"; // Path to the JSON file for language selection in the build.
    public string folderNameInBuild = "/StreamingAssets/Language/"; // Folder containing language files in the build.
    [HideInInspector] public bool wasImported; // Flag to track whether settings were imported.

    private bool foundID; // Flag to track if the language text option ID has been found.
    private bool isQuaternionFoldout; // Flag to control quaternion foldout in the inspector.

    private void OnEnable()
    {
        LanguageUpdate(); // Update the language text when the component is enabled.
    }

    public void LanguageUpdate()
    {
        string path = LanguageClassTools.GetFolderPath(folderNameInUnity, folderNameInBuild); // Set the path to locate language files in Unity Editor or in the build.
        savePath = LanguageClassTools.GetJsonPath(jsonNameInUnity, jsonNameInBuild); // Set the save path for the Unity Editor or for the build.
        defaultFile = LanguageClassTools.FindDefaultLanguageFile(path, standardFile); // Find the default language file based on the StandardFile setting.
        foundID = false; // Reset the loop.

        // Check if the save file exists, and load the selected language if available; otherwise, use the default language.
        if (File.Exists(savePath))
        {
            string json = File.ReadAllText(savePath);
            LanguageFileSave fileSaveData = JsonUtility.FromJson<LanguageFileSave>(json);
            selectedFile = fileSaveData.selectedFile;

            ReadFile(selectedFile); // Read and process the language file.
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
    }

    // Process a language option defined in the given line.
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
        string text = LanguageClassTools.ExtractTextWithinBraces(line);
        string lineWithoutCurlyBraces = LanguageClassTools.RemoveContentWithinCurlyBraces(line);
        fontSize = LanguageClassTools.ExtractIntValue(lineWithoutCurlyBraces, "S:");
        font = LanguageClassTools.ExtractIntValue(lineWithoutCurlyBraces, "Font:");
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
        alignment = LanguageClassTools.ExtractIntValue(lineWithoutCurlyBraces, "Ali:");
        reverseText = LanguageClassTools.ExtractBooleanValue(lineWithoutCurlyBraces, "Rev:", reverseText);

        // Set various properties of the language text component based on extracted values.
        if (fontSize != 0)
        { TMP_languageText.fontSize = fontSize; }

        if (fontListObject != null && font > 0 && font <= fontListObject.TMP_fontList.Count)
        { TMP_languageText.font = fontListObject.TMP_fontList[font - 1]; }

        if (TMP_languageText.TryGetComponent<RectTransform>(out var rectTransform))
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

        if (translateText)
        {
            if (reverseText)
            {
                char[] charArray = text.ToCharArray();
                Array.Reverse(charArray);
                text = new string(charArray);
            }

            TMP_languageText.text = text; // Update the text of the language text component.
        }
    }

    private void SetTextAlignment()
    {
        // Set the alignment of the language text component based on the alignment property.
        if (alignment >= 1 && alignment <= 3)
        {
            // Gets the current vertical alignment of the text component.
            string verticalAlignment = TMP_TextAlignmentConverter.GetVerticalAlignmentFromTextAlignment(TMP_languageText.alignment);

            // Sets the new alignment based on the value of the 'alignment' property.
            // Vertical alignment is determined by the current value of the 'alignment' property.
            // If 'alignment' is 1, the text will be left aligned at the top (Upper), in the middle (Middle) or at the bottom (Lower).
            // If 'alignment' is 2, the text will be aligned to the center (Center) at the top (Upper), in the middle (Middle) or at the bottom (Lower).
            // If 'alignment' is 3, the text will be aligned to the right (Right), top (Upper), middle (Middle) or bottom (Lower).
            TextAlignmentOptions newAlignment = TextAlignmentOptions.TopLeft;
            if (alignment == 1)
            {
                newAlignment = (verticalAlignment == "Upper") ? TextAlignmentOptions.TopLeft : (verticalAlignment == "Middle") ? TextAlignmentOptions.Left : TextAlignmentOptions.BottomLeft;
            }
            else if (alignment == 2)
            {
                newAlignment = (verticalAlignment == "Upper") ? TextAlignmentOptions.Top : (verticalAlignment == "Middle") ? TextAlignmentOptions.Center : TextAlignmentOptions.Bottom;
            }
            else if (alignment == 3)
            {
                newAlignment = (verticalAlignment == "Upper") ? TextAlignmentOptions.TopRight : (verticalAlignment == "Middle") ? TextAlignmentOptions.Right : TextAlignmentOptions.BottomRight;
            }

            TMP_languageText.alignment = newAlignment; // Sets the new alignment in the text component.
        }
    }

    // Function to save text properties.
    public void SavaText()
    {
        RectTransform rectTransform = TMP_languageText.GetComponent<RectTransform>(); // Get the RectTransform for positioning and sizing.

        // Save rotation, scale, anchor positions, and sizes.
        rotation = rectTransform.localRotation;
        localScale = rectTransform.localScale;
        anchorMin = rectTransform.anchorMin;
        anchorMax = rectTransform.anchorMax;
        anchoredPosition = rectTransform.anchoredPosition;
        sizeDelta = rectTransform.sizeDelta;
        pivot = rectTransform.pivot;

        fontSize = (int)TMP_languageText.fontSize; // Save the font size.

        // Determine the alignment based on the TextAnchor enumeration.
        if (TMP_languageText.alignment == TextAlignmentOptions.Left || TMP_languageText.alignment == TextAlignmentOptions.TopLeft || TMP_languageText.alignment == TextAlignmentOptions.BottomLeft)
        {
            alignment = 1; // Left alignment.
        }
        else if (TMP_languageText.alignment == TextAlignmentOptions.Center || TMP_languageText.alignment == TextAlignmentOptions.Top || TMP_languageText.alignment == TextAlignmentOptions.Bottom)
        {
            alignment = 2; // Center alignment.
        }
        else if (TMP_languageText.alignment == TextAlignmentOptions.Right || TMP_languageText.alignment == TextAlignmentOptions.TopRight || TMP_languageText.alignment == TextAlignmentOptions.BottomRight)
        {
            alignment = 3; // Right alignment.
        }

        // Determine the index of the font in the fontListObject.
        TMP_FontAsset fontTemp = TMP_languageText.font;
        int index = fontListObject.TMP_fontList.IndexOf(fontTemp) + 1;
        if (index > 0) { font = index; }
    }
}