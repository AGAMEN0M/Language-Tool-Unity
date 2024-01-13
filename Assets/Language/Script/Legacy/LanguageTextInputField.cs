using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using System.Linq;
#endif

[AddComponentMenu("Language/UI/Legacy/Language Text Input Field")]
public class LanguageTextInputField : MonoBehaviour
{
    #if UNITY_EDITOR
    [CustomEditor(typeof(LanguageTextInputField))]
    public class LanguageTextInputFieldEditor : Editor
    {
        public static List<float> IDs = new(); // List to store unique IDs.
        private bool savedID; // Flag to track if an ID has been saved.

        private void OnEnable()
        {
            LanguageClassToolsEditor.LoadDataFromFile(ref IDs); // Load IDs from a file when the editor is enabled.
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update(); // Update serialized object.
            LanguageTextInputField script = (LanguageTextInputField)target; // Get the target script.

            // Disable editing of the script field.
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.ObjectField("Script", MonoScript.FromMonoBehaviour((MonoBehaviour)target), typeof(MonoBehaviour), false);
            EditorGUI.EndDisabledGroup();

            EditorGUILayout.Space(15f); // Add space in the Inspector.

            // Button to import settings.
            if (GUILayout.Button("Import Settings"))
            {
                // Save input field settings, adjust rect list, and mark scene as dirty.
                script.SavaInputField();
                script.placeholder.SavaText();
                LanguageClassToolsEditor.SaveAdjustRect(ref script.adjustRectList);
                EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
                LanguageClassToolsEditor.LoadDataFromFile(ref IDs); // Reload data after import.
                script.wasImported = true; // Set the flag to indicate settings were imported.
            }

            // Button to save settings.
            GUI.enabled = script.wasImported; // Enable the button only if settings were previously imported.
            if (GUILayout.Button("Save Settings"))
            {
                // Check if the ID has been saved and show a confirmation dialog.
                bool shouldSave = true;

                if (savedID == true)
                {
                    shouldSave = EditorUtility.DisplayDialog("Save Confirmation", "An ID with this value already exists. Are you sure you want to save?\nThis action will overwrite the currently saved ID.", "Yes", "No");
                }

                // Proceed with saving if confirmed.
                if (shouldSave)
                {
                    EditorApplication.ExecuteMenuItem("Window/Language/Language Save Editor");
                    AddLanguageSaveID(script); // Add language save ID based on the script settings.
                }
            }
            GUI.enabled = true; // Re-enable the GUI after the button.

            // Inspector fields for TextComponent and Placeholder.
            EditorGUILayout.PropertyField(serializedObject.FindProperty("textComponent"));

            if (script.placeholder == null) 
            {
                // Show a warning if Placeholder is not assigned.
                GUI.color = Color.red;
                EditorGUILayout.PropertyField(serializedObject.FindProperty("placeholder"));
                EditorGUILayout.HelpBox("You need to assign a Placeholder", MessageType.Error);
            }
            else
            {
                // Check if the Placeholder ID exists in the list.
                float newID;
                if (LanguageClassToolsEditor.IsIDInTextIDList(script.placeholder.ID, ref IDs))
                {
                    savedID = true;
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("placeholder"));

                    GUI.color = Color.yellow;
                    newID = EditorGUILayout.FloatField("Placeholder ID", script.placeholder.ID);                    
                    EditorGUILayout.HelpBox($"ID: {script.placeholder.ID}\nThere is an ID with this number Saved!", MessageType.Warning);
                }
                else
                {
                    savedID = false;
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("placeholder"));
                    newID = EditorGUILayout.FloatField("Placeholder ID", script.placeholder.ID);
                }
                script.placeholder.ID = newID; // Update Placeholder ID with the new value.
            }
            GUI.color = Color.white; // Reset GUI color.

            if (LanguageClassToolsEditor.IsIDInTextIDList(script.ID, ref IDs))
            {
                // Check if the main ID exists in the list.
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

            // Inspector fields for fontSize, font, fontListObject, rotation, localScale, anchorMin, anchorMax, anchoredPosition, sizeDelta, pivot, alignment, and other properties.
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

        public void AddLanguageSaveID(LanguageTextInputField script)
        {
            var window = EditorWindow.GetWindow<LanguageSaveEditorWindow>();

            // Find if the ID exists in the window's textID list and update its properties if found.
            var existingLanguageSaveID = window.textID.FirstOrDefault(id => id.ID == script.ID);
            if (existingLanguageSaveID != null)
            {
                // Update existing language save ID properties.
                LanguageClassToolsEditor.ClearExistingLanguageSaveIDValues(existingLanguageSaveID);
                existingLanguageSaveID.componentType = 3;
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
                existingLanguageSaveID.fontSizeWrite = true;
                existingLanguageSaveID.fontSize = script.fontSize;
                existingLanguageSaveID.alignmentWrite = true;
                existingLanguageSaveID.alignment = script.alignment;
            }
            else
            {
                // Create a new LanguageSaveID if the ID does not exist in the window's textID list.
                var languageSaveID2 = new LanguageSaveID
                {
                    ID = script.ID,
                    componentType = 3,
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
                    fontSizeWrite = true,
                    fontSize = script.fontSize,
                    alignmentWrite = true,
                    alignment = script.alignment,
                };
                window.textID.Add(languageSaveID2);
            }

            // Check and update placeholder properties in the LanguageSaveEditorWindow if placeholder exists.
            if (script.placeholder != null)
            {
                var placeholderSaveID = window.textID.FirstOrDefault(id => id.ID == script.placeholder.ID);

                if (placeholderSaveID != null)
                {
                    // Update existing or create new LanguageSaveID for the placeholder.
                    LanguageClassToolsEditor.ClearExistingLanguageSaveIDValues(placeholderSaveID);
                    placeholderSaveID.componentType = 1;
                    placeholderSaveID.rotation = script.placeholder.rotation;
                    placeholderSaveID.rotationWrite = true;
                    placeholderSaveID.localScale = script.placeholder.localScale;
                    placeholderSaveID.localScaleWrite = true;
                    placeholderSaveID.anchorMin = script.placeholder.anchorMin;
                    placeholderSaveID.anchorMinWrite = true;
                    placeholderSaveID.anchorMax = script.placeholder.anchorMax;
                    placeholderSaveID.anchorMaxWrite = true;
                    placeholderSaveID.anchoredPosition = script.placeholder.anchoredPosition;
                    placeholderSaveID.anchoredPositionWrite = true;
                    placeholderSaveID.sizeDelta = script.placeholder.sizeDelta;
                    placeholderSaveID.sizeDeltaWrite = true;
                    placeholderSaveID.pivot = script.placeholder.pivot;
                    placeholderSaveID.pivotWrite = true;
                    placeholderSaveID.textWrite = true;
                    placeholderSaveID.text = script.placeholder.languageText.text;
                    placeholderSaveID.fontWrite = true;
                    placeholderSaveID.font = script.placeholder.font;
                    placeholderSaveID.fontSizeWrite = true;
                    placeholderSaveID.fontSize = script.placeholder.fontSize;
                    placeholderSaveID.alignmentWrite = true;
                    placeholderSaveID.alignment = script.placeholder.alignment;
                    placeholderSaveID.reverseWrite = true;
                    placeholderSaveID.reverse = script.placeholder.reverseText ? 2 : 1;
                }
                else
                {
                    // Create a new LanguageSaveID for the placeholder if it doesn't exist.
                    var languageSaveID = new LanguageSaveID
                    {
                        ID = script.placeholder.ID,
                        componentType = 1,
                        rotation = script.placeholder.rotation,
                        rotationWrite = true,
                        localScale = script.placeholder.localScale,
                        localScaleWrite = true,
                        anchorMin = script.placeholder.anchorMin,
                        anchorMinWrite = true,
                        anchorMax = script.placeholder.anchorMax,
                        anchorMaxWrite = true,
                        anchoredPosition = script.placeholder.anchoredPosition,
                        anchoredPositionWrite = true,
                        sizeDelta = script.placeholder.sizeDelta,
                        sizeDeltaWrite = true,
                        pivot = script.placeholder.pivot,
                        pivotWrite = true,
                        textWrite = true,
                        text = script.placeholder.languageText.text,
                        fontWrite = true,
                        font = script.placeholder.font,
                        fontSizeWrite = true,
                        fontSize = script.placeholder.fontSize,
                        alignmentWrite = true,
                        alignment = script.placeholder.alignment,
                        reverseWrite = true,
                        reverse = script.placeholder.reverseText ? 2 : 1
                    };
                    window.textID.Add(languageSaveID);
                }
            }

            // Check and update adjustRectList properties in the LanguageSaveEditorWindow if exists.
            if (script.adjustRectList != null && script.adjustRectList.Count > 0)
            {
                foreach (AdjustRectList id in script.adjustRectList)
                {
                    var ListExistingSaveID = window.textID.FirstOrDefault(existingID => existingID.ID == id.ID);

                    if (ListExistingSaveID != null)
                    {
                        // Update existing adjustRectList properties in the LanguageSaveEditorWindow.
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
                        // Create a new adjustRectList LanguageSaveID if it doesn't exist.
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
                        window.textID.Add(languageSaveID);
                    }
                }
            }

            window.SaveDataJson(); // Save updated data to JSON.
            LanguageClassToolsEditor.LoadDataFromFile(ref IDs); // Reload data from file and update IDs list.
        }
    }
    #endif

    [Header("Settings")]
    public Text textComponent; // Reference to the Text component for displaying the localized text.
    public LanguageText placeholder;
    [SerializeField] private float ID = -9; // ID to identify the specific language text option.
    public List<AdjustRectList> adjustRectList; // List of adjustments for UI elements.
    [Space(10)]
    [Header("Text Settings")]
    [SerializeField] private int fontSize; // The font size of the text.
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

        // Set various properties of the language text component based on extracted values.
        if (fontSize != 0)
        { textComponent.fontSize = fontSize; }

        if (fontListObject != null && font > 0 && font <= fontListObject.fontList.Count)
        { textComponent.font = fontListObject.fontList[font - 1]; }

        if (textComponent.TryGetComponent<RectTransform>(out var rectTransform))
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
            string verticalAlignment = TextAlignmentConverter.GetVerticalAlignmentFromTextAlignment(textComponent.alignment);

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

            textComponent.alignment = newAlignment; // Sets the new alignment in the text component.
        }
    }

    public void SavaInputField()
    {
        RectTransform rectTransform = textComponent.GetComponent<RectTransform>(); // Get the RectTransform for positioning and sizing.

        // Save the positioning and sizing information.
        rotation = rectTransform.localRotation;
        localScale = rectTransform.localScale;
        anchorMin = rectTransform.anchorMin;
        anchorMax = rectTransform.anchorMax;
        anchoredPosition = rectTransform.anchoredPosition;
        sizeDelta = rectTransform.sizeDelta;
        pivot = rectTransform.pivot;

        // Save the font size and alignment.
        fontSize = textComponent.fontSize;

        // Determine the alignment index for the text.
        if (textComponent.alignment == TextAnchor.UpperLeft || textComponent.alignment == TextAnchor.MiddleLeft || textComponent.alignment == TextAnchor.LowerLeft)
        {
            alignment = 1;
        }
        else if (textComponent.alignment == TextAnchor.UpperCenter || textComponent.alignment == TextAnchor.MiddleCenter || textComponent.alignment == TextAnchor.LowerCenter)
        {
            alignment = 2;
        }
        else if (textComponent.alignment == TextAnchor.UpperRight || textComponent.alignment == TextAnchor.MiddleRight || textComponent.alignment == TextAnchor.LowerRight)
        {
            alignment = 3;
        }

        // Determine the font index for the text.
        Font fontTemp = textComponent.font;
        int index = fontListObject.fontList.IndexOf(fontTemp) + 1;
        if (index > 0) { font = index; }
    }
}