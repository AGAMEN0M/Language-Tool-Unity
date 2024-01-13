using UnityEngine;
using System.IO;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using System.Linq;
using System.Collections.Generic;
#endif

// AddComponentMenu allows this script to be easily added from the Unity editor.
[AddComponentMenu("Language/UI/Adjust Rect")]
public class AdjustRect : MonoBehaviour
{
    // Custom editor for the AdjustRect component.
    #if UNITY_EDITOR
    [CustomEditor(typeof(AdjustRect))]
    public class AdjustRectEditor : Editor
    {
        // Static list to store IDs.
        public static List<float> IDs = new();
        private bool savedID;

        // Called when the editor script is enabled.
        private void OnEnable()
        {
            LanguageClassToolsEditor.LoadDataFromFile(ref IDs); // Load data from file into the IDs list.
        }

        // Override the default inspector GUI.
        public override void OnInspectorGUI()
        {
            serializedObject.Update(); // Update serialized object.
            AdjustRect script = (AdjustRect)target; // Reference to the AdjustRect script.

            // Display the script field as disabled (read-only).
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.ObjectField("Script", MonoScript.FromMonoBehaviour((MonoBehaviour)target), typeof(MonoBehaviour), false);
            EditorGUI.EndDisabledGroup();

            EditorGUILayout.Space(15f); // Add space between elements.

            // Button to import settings.
            if (GUILayout.Button("Import Settings"))
            {
                script.SavaRectTransform(); // Save RectTransform settings.
                EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene()); // Mark scene as dirty.
                LanguageClassToolsEditor.LoadDataFromFile(ref IDs); // Load data from file.
                script.wasImported = true; // Set the flag to indicate settings were imported.
            }

            // Button to save settings.
            GUI.enabled = script.wasImported; // Enable the button only if settings were previously imported.
            if (GUILayout.Button("Save Settings"))
            {
                bool shouldSave = true;

                if (savedID == true)
                {
                    // Display confirmation dialog if ID already exists.
                    shouldSave = EditorUtility.DisplayDialog("Save Confirmation", "An ID with this value already exists. Are you sure you want to save?\nThis action will overwrite the currently saved ID.", "Yes", "No");
                }

                if (shouldSave)
                {
                    EditorApplication.ExecuteMenuItem("Window/Language/Language Save Editor"); // Open Language Save Editor window.
                    AddLanguageSaveID(script); // Add language save ID.
                }
            }
            GUI.enabled = true; // Re-enable the GUI after the button.

            // Display property fields for RectTransform and ID.
            EditorGUILayout.PropertyField(serializedObject.FindProperty("rectTransform"));
            if (LanguageClassToolsEditor.IsIDInTextIDList(script.ID, ref IDs))
            {
                // Display warning if ID already exists.
                savedID = true;
                GUI.color = Color.yellow;
                EditorGUILayout.PropertyField(serializedObject.FindProperty("ID"));
                EditorGUILayout.HelpBox($"There is an ID with the number ({script.ID}) saved!", MessageType.Warning);
                GUI.color = Color.white;
            }
            else
            {
                savedID = false;
                EditorGUILayout.PropertyField(serializedObject.FindProperty("ID"));
            }

            EditorGUILayout.Space(20f); // Space between sections.

            EditorGUILayout.LabelField("Rect Transform Settings", EditorStyles.boldLabel); // Section for RectTransform settings.

            SerializedProperty quaternionProp = serializedObject.FindProperty("rotation");
            script.isQuaternionFoldout = EditorGUILayout.Foldout(quaternionProp.isExpanded, "Rotation");
            quaternionProp.isExpanded = script.isQuaternionFoldout;
            if (script.isQuaternionFoldout)
            {
                EditorGUI.indentLevel++;
                // Display quaternion properties for rotation.
                EditorGUILayout.PropertyField(quaternionProp.FindPropertyRelative("x"));
                EditorGUILayout.PropertyField(quaternionProp.FindPropertyRelative("y"));
                EditorGUILayout.PropertyField(quaternionProp.FindPropertyRelative("z"));
                EditorGUILayout.PropertyField(quaternionProp.FindPropertyRelative("w"));
                EditorGUI.indentLevel--;
            }

            // Display other RectTransform properties.
            EditorGUILayout.PropertyField(serializedObject.FindProperty("localScale"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("anchorMin"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("anchorMax"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("anchoredPosition"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("sizeDelta"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("pivot"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("standardFile"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("selectedFile"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("savePath"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("defaultFile"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("jsonNameInUnity"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("folderNameInUnity"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("jsonNameInBuild"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("folderNameInBuild"));

            serializedObject.ApplyModifiedProperties(); // Apply changes to the serialized object.
        }

        // Method to add a language save ID for an AdjustRect component.
        public void AddLanguageSaveID(AdjustRect script)
        {
            var window = EditorWindow.GetWindow<LanguageSaveEditorWindow>(); // Get the LanguageSaveEditorWindow.

            var existingLanguageSaveID = window.textID.FirstOrDefault(id => id.ID == script.ID);

            if (existingLanguageSaveID != null)
            {
                // Update existing language save ID.
                LanguageClassToolsEditor.ClearExistingLanguageSaveIDValues(existingLanguageSaveID);
                existingLanguageSaveID.componentType = 7;
                existingLanguageSaveID.rotation = script.rotation;
                existingLanguageSaveID.localScale = script.localScale;
                existingLanguageSaveID.anchorMin = script.anchorMin;
                existingLanguageSaveID.anchorMax = script.anchorMax;
                existingLanguageSaveID.anchoredPosition = script.anchoredPosition;
                existingLanguageSaveID.sizeDelta = script.sizeDelta;
                existingLanguageSaveID.pivot = script.pivot;
                existingLanguageSaveID.rotationWrite = true;
                existingLanguageSaveID.localScaleWrite = true;
                existingLanguageSaveID.anchorMinWrite = true;
                existingLanguageSaveID.anchorMaxWrite = true;
                existingLanguageSaveID.anchoredPositionWrite = true;
                existingLanguageSaveID.sizeDeltaWrite = true;
                existingLanguageSaveID.pivotWrite = true;
            }
            else
            {
                // Create a new language save ID if it doesn't exist
                var languageSaveID = new LanguageSaveID
                {
                    ID = script.ID,
                    componentType = 7,
                    rotation = script.rotation,
                    localScale = script.localScale,
                    anchorMin = script.anchorMin,
                    anchorMax = script.anchorMax,
                    anchoredPosition = script.anchoredPosition,
                    sizeDelta = script.sizeDelta,
                    pivot = script.pivot,
                    rotationWrite = true,
                    localScaleWrite = true,
                    anchorMinWrite = true,
                    anchorMaxWrite = true,
                    anchoredPositionWrite = true,
                    sizeDeltaWrite = true,
                    pivotWrite = true
                };
                window.textID.Add(languageSaveID); // Add the new language save ID to the window's textID list.
            }

            window.SaveDataJson(); // Save data to file.
            LanguageClassToolsEditor.LoadDataFromFile(ref IDs); // Load data from file into IDs list.
        }
    }
    #endif

    [Header("Settings")]
    public RectTransform rectTransform; // Reference to the RectTransform.
    public float ID = -11; // ID to identify the specific language text option.
    [Space(10)]
    [Header("Rect Transform Settings")]
    public Quaternion rotation; // Rotation of the RectTransform
    public Vector3 localScale; // Scale of the RectTransform.
    [Space(5)]
    public Vector2 anchorMin; // Minimum anchor position.
    public Vector2 anchorMax; // Maximum anchor position.
    public Vector2 anchoredPosition; // Anchored position.
    public Vector2 sizeDelta; // Size delta of the RectTransform.
    public Vector2 pivot; // Pivot point of the RectTransform.
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
    private bool isQuaternionFoldout; // Flag to control the foldout state for Quaternion settings.

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
                    break; // Break the loop if the language option ID is found.
                }
            }
        }
    }

    // Process a language option defined in the given line.
    private void ProcessLanguageOption(string line)
    {
        // Split the line into parts separated by ';'.
        string[] parts = line.Split(';');
        float id = float.Parse(parts[0].Replace("id:", "")); // Extract ID from the line.

        if (ID != id)
        {
            return; // Skip processing if the ID does not match the current language text option ID.
        }

        foundID = true; // Mark that the language option ID has been found.

        // Extract and process various properties of the language option.
        string lineWithoutCurlyBraces = LanguageClassTools.RemoveContentWithinCurlyBraces(line);
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

        // Set various properties of the language text component based on extracted values.
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

    // Method to save RectTransform settings.
    public void SavaRectTransform()
    {
        // Save RectTransform settings to the AdjustRect instance.
        rotation = rectTransform.localRotation;
        localScale = rectTransform.localScale;
        anchorMin = rectTransform.anchorMin;
        anchorMax = rectTransform.anchorMax;
        anchoredPosition = rectTransform.anchoredPosition;
        sizeDelta = rectTransform.sizeDelta;
        pivot = rectTransform.pivot;
    }
}

// Serializable class to hold a list of AdjustRect objects with associated settings.
[System.Serializable]
public class AdjustRectList
{
    [Header("Settings")]
    public AdjustRect adjustRect; // Reference to the AdjustRect component.
    public float ID = 0; // ID associated with the AdjustRect.
    [Space(10)]
    [Header("Rect Transform Settings")]
    public Quaternion rotation; // Rotation setting.
    public Vector3 localScale; // Scale setting.
    [Space(5)]
    public Vector2 anchorMin; // Anchor min setting.
    public Vector2 anchorMax; // Anchor max setting.
    public Vector2 anchoredPosition; // Anchored position setting.
    public Vector2 sizeDelta; // Size delta setting.
    public Vector2 pivot; // Pivot setting.
}