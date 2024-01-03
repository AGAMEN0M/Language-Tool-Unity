using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LanguageSettingsWindow : EditorWindow
{
    public Vector2 scrollPosition = Vector2.zero; // Defines the scroll position in the window.

    // Path variable definitions for JSON files and folders.
    public string jsonNameInUnity = "/Language/Editor/LanguageFileSave.json";
    public string jsonSaveNameInUnity = "/Language/Editor/LanguageSave.json";
    public string folderNameInUnity = "/StreamingAssets/Language/";

    public string jsonNameInBuild = "/LanguageFileSave.json";
    public string jsonSaveNameInBuild = "/LanguageSave.json";
    public string folderNameInBuild = "/StreamingAssets/Language/";

    public bool firstTime = false; // Defines whether this is the first time the window is displayed.

    // The lists of fonts used in the project.
    public LanguageFontListData fontListObject;
    /**/
    public TMP_LanguageFontListData TMP_fontListObject;
    /**/

    // Defines the menu that will be displayed in the editor window.
    [MenuItem("Window/Language/Language Settings", false, 2030)]
    public static void ShowWindow()
    {
        // GetWindow<LanguageSettingsWindow>("Language Settings");
        LanguageSettingsWindow window = GetWindow<LanguageSettingsWindow>("Language Settings"); // Gets an instance of the LanguageSettingsWindow window and sets the custom icon.
        Texture2D icon = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Language/Editor/Icon/Editor Screens/LanguageSettingsWindow Icon.png"); // Loads the custom icon located in the specified path.
        window.titleContent = new GUIContent("Language Settings", icon); // Defines the window title and icon.
    }

    private void OnGUI()
    {
        // Set the default settings the first time the method runs and load the saved file if it exits.
        if (firstTime == false)
        {
            LoadDataFromFile();
            firstTime = true;
        }

        scrollPosition = GUILayout.BeginScrollView(scrollPosition); // Creates a scrollable area for the window's GUI.

        GUILayout.Label("Reset all Language settings"); // Label to show the text "Reset all Language settings".

        GUILayout.Space(5); // Vertical spacing.

        // Button.
        if (GUILayout.Button("Reset"))
        {
            // Displays a confirmation dialog with the custom message.
            bool resetConfirmed = EditorUtility.DisplayDialog("Reset Confirmation", "Are you sure you want to reset?\nThis will undo any changes you have made to the Language system.", "Yes", "No");
            if (resetConfirmed)
            {
                // Gets a reference to the "LanguageSaveEditorWindow" window and closes it.
                LanguageSaveEditorWindow editorWindow = EditorWindow.GetWindow<LanguageSaveEditorWindow>("Language Save Editor");
                editorWindow.Close();

                // Resets path variables to their original values.
                jsonNameInUnity = "/Language/Editor/LanguageFileSave.json";
                jsonSaveNameInUnity = "/Language/Editor/LanguageSave.json";
                folderNameInUnity = "/StreamingAssets/Language/";
                jsonNameInBuild = "/LanguageFileSave.json";
                jsonSaveNameInBuild = "/LanguageSave.json";
                folderNameInBuild = "/StreamingAssets/Language/";

                fontListObject = null;
                /**/
                TMP_fontListObject = null;
                /**/

                DeleteDataFile(); // Calls the method to delete the language data file.
            }
        }

        GUILayout.Space(10); // Add some space.

        GUILayout.Label("Archives Location");

        // Input fields for the paths and names of JSON files and folders.
        jsonNameInUnity = EditorGUILayout.TextField("Json Name In Unity:", jsonNameInUnity);
        jsonSaveNameInUnity = EditorGUILayout.TextField("Json Save Name In Unity:", jsonSaveNameInUnity);
        folderNameInUnity = EditorGUILayout.TextField("Folder Name In Unity:", folderNameInUnity);

        GUILayout.Space(5); // Add some space.

        jsonNameInBuild = EditorGUILayout.TextField("Json Name In Build:", jsonNameInBuild);
        jsonSaveNameInBuild = EditorGUILayout.TextField("Json Save Name In Build:", jsonSaveNameInBuild);
        folderNameInBuild = EditorGUILayout.TextField("Folder Name In Build:", folderNameInBuild);

        GUILayout.Space(10); // Add some space.

        GUILayout.Label("Font List Objects");

        // Add ObjectField for fontListObject
        fontListObject = (LanguageFontListData)EditorGUILayout.ObjectField("Font List Object", fontListObject, typeof(LanguageFontListData), false);
        /**/
        TMP_fontListObject = (TMP_LanguageFontListData)EditorGUILayout.ObjectField("TMP - Font List Object", TMP_fontListObject, typeof(TMP_LanguageFontListData), false);
        /**/

        GUILayout.Space(10); // Add some space.

        if (GUILayout.Button("Apply to current Scene"))
        {
            // Displays a confirmation dialog before applying changes to the current scene.
            bool ApplySceneConfirmed = EditorUtility.DisplayDialog("Apply to current Scene Confirmation", "Are you sure you want to do this in the current scene?\nThis will change the settings of all Language objects in the scene.", "Yes", "No");
            if (ApplySceneConfirmed)
            {
                ChangeLabelTextInAllObjects();
                EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
                SaveDataToFile();
            }
        }

        GUILayout.Space(10); // Add some space.

        if (GUILayout.Button("Create Language Data"))
        {
            // Display confirmation dialog before creating language data.
            bool LanguageDataConfirmed = EditorUtility.DisplayDialog("Create Language Data Confirmation", "This will set the current settings as the default for when the game starts.", "Yes", "No");
            if (LanguageDataConfirmed)
            {
                CreateOrReplaceScriptableObject();
                SaveDataToFile();
            }
        }

        GUILayout.EndScrollView(); // Ends the scroll area.
    }

    // Method to delete the language data file.
    private void DeleteDataFile()
    {
        string filePath = "ProjectSettings/LanguageData.json";
        string filePath2 = "ProjectSettings/LanguageSettingsData.txt";

        if (File.Exists(filePath))
        {
            File.Delete(filePath); // Deletes the language data file.
            File.Delete(filePath2); // Deletes the language Settings data file.
            Debug.Log("Data File Deleted Successfully.");
        }
    }

    // Method to change the variables in the scripts of the objects in the scene.
    private void ChangeLabelTextInAllObjects()
    {
        // Get all objects, including inactive ones, in the active scene.
        GameObject[] allObjectsInScene = Resources.FindObjectsOfTypeAll<GameObject>();

        foreach (GameObject gameObject in allObjectsInScene)
        {
            // Check if the object belongs to the active scene.
            if (gameObject.scene == SceneManager.GetActiveScene())
            {
                // For each type of script, find components and apply changes.

                // Checks if the object has the correct component.
                if (gameObject.TryGetComponent<LanguageDropdownOptions>(out var languageDropdownOptions))
                {
                    // Applies changes to variables.
                    languageDropdownOptions.jsonNameInUnity = jsonNameInUnity;
                    languageDropdownOptions.jsonSaveNameInUnity = jsonSaveNameInUnity;
                    languageDropdownOptions.folderNameInUnity = folderNameInUnity;

                    languageDropdownOptions.jsonNameInBuild = jsonNameInBuild;
                    languageDropdownOptions.jsonSaveNameInBuild = jsonSaveNameInBuild;
                    languageDropdownOptions.folderNameInBuild = folderNameInBuild;
                }

                if (gameObject.TryGetComponent<LanguageTextMesh>(out var languageTextMesh))
                {
                    // Applies changes to variables.
                    languageTextMesh.jsonNameInUnity = jsonNameInUnity;
                    languageTextMesh.folderNameInUnity = folderNameInUnity;

                    languageTextMesh.jsonNameInBuild = jsonNameInBuild;
                    languageTextMesh.folderNameInBuild = folderNameInBuild;

                    if (fontListObject != null) { languageTextMesh.fontListObject = fontListObject; }
                }

                if (gameObject.TryGetComponent<LanguageDropdown>(out var languageDropdown))
                {
                    // Applies changes to variables.
                    languageDropdown.jsonNameInUnity = jsonNameInUnity;
                    languageDropdown.folderNameInUnity = folderNameInUnity;

                    languageDropdown.jsonNameInBuild = jsonNameInBuild;
                    languageDropdown.folderNameInBuild = folderNameInBuild;

                    if (fontListObject != null) { languageDropdown.fontListObject = fontListObject; }
                }

                if (gameObject.TryGetComponent<LanguageText>(out var languageText))
                {
                    // Applies changes to variables.
                    languageText.jsonNameInUnity = jsonNameInUnity;
                    languageText.folderNameInUnity = folderNameInUnity;

                    languageText.jsonNameInBuild = jsonNameInBuild;
                    languageText.folderNameInBuild = folderNameInBuild;

                    if (fontListObject != null) { languageText.fontListObject = fontListObject; }
                }

                if (gameObject.TryGetComponent<LanguageTextInputField>(out var languageTextInputField))
                {
                    // Applies changes to variables.
                    languageTextInputField.jsonNameInUnity = jsonNameInUnity;
                    languageTextInputField.folderNameInUnity = folderNameInUnity;

                    languageTextInputField.jsonNameInBuild = jsonNameInBuild;
                    languageTextInputField.folderNameInBuild = folderNameInBuild;

                    if (fontListObject != null) { languageTextInputField.fontListObject = fontListObject; }
                }

                if (gameObject.TryGetComponent<LanguageCreateFile>(out var languageCreateFile))
                {
                    // Applies changes to variables.
                    languageCreateFile.jsonNameInUnity = jsonNameInUnity;
                    languageCreateFile.folderNameInUnity = folderNameInUnity;

                    languageCreateFile.jsonNameInBuild = jsonNameInBuild;
                    languageCreateFile.folderNameInBuild = folderNameInBuild;
                }

                if (gameObject.TryGetComponent<LanguageScript>(out var languageScript))
                {
                    // Applies changes to variables.
                    languageScript.jsonNameInUnity = jsonNameInUnity;
                    languageScript.folderNameInUnity = folderNameInUnity;

                    languageScript.jsonNameInBuild = jsonNameInBuild;
                    languageScript.folderNameInBuild = folderNameInBuild;
                }

                if (gameObject.TryGetComponent<LanguageImage>(out var languageImage))
                {
                    // Applies changes to variables.
                    languageImage.jsonNameInUnity = jsonNameInUnity;
                    languageImage.folderNameInUnity = folderNameInUnity;

                    languageImage.jsonNameInBuild = jsonNameInBuild;
                    languageImage.folderNameInBuild = folderNameInBuild;
                }

                if (gameObject.TryGetComponent<LanguageRawImage>(out var languageRawImage))
                {
                    // Applies changes to variables.
                    languageRawImage.jsonNameInUnity = jsonNameInUnity;
                    languageRawImage.folderNameInUnity = folderNameInUnity;

                    languageRawImage.jsonNameInBuild = jsonNameInBuild;
                    languageRawImage.folderNameInBuild = folderNameInBuild;
                }

                if (gameObject.TryGetComponent<LanguageAudioPlayer>(out var languageAudioPlayer))
                {
                    // Applies changes to variables.
                    languageAudioPlayer.jsonNameInUnity = jsonNameInUnity;
                    languageAudioPlayer.folderNameInUnity = folderNameInUnity;

                    languageAudioPlayer.jsonNameInBuild = jsonNameInBuild;
                    languageAudioPlayer.folderNameInBuild = folderNameInBuild;
                }

                if (gameObject.TryGetComponent<AdjustRect>(out var adjustRect))
                {
                    // Applies changes to variables.
                    adjustRect.jsonNameInUnity = jsonNameInUnity;
                    adjustRect.folderNameInUnity = folderNameInUnity;

                    adjustRect.jsonNameInBuild = jsonNameInBuild;
                    adjustRect.folderNameInBuild = folderNameInBuild;
                }

                /**/
                if (gameObject.TryGetComponent<TMP_LanguageDropdownOptions>(out var TMP_languageDropdownOptions))
                {
                    // Applies changes to variables.
                    TMP_languageDropdownOptions.jsonNameInUnity = jsonNameInUnity;
                    TMP_languageDropdownOptions.jsonSaveNameInUnity = jsonSaveNameInUnity;
                    TMP_languageDropdownOptions.folderNameInUnity = folderNameInUnity;

                    TMP_languageDropdownOptions.jsonNameInBuild = jsonNameInBuild;
                    TMP_languageDropdownOptions.jsonSaveNameInBuild = jsonSaveNameInBuild;
                    TMP_languageDropdownOptions.folderNameInBuild = folderNameInBuild;
                }

                if (gameObject.TryGetComponent<TMP_LanguageTextMesh>(out var TMP_languageTextMesh))
                {
                    // Applies changes to variables.
                    TMP_languageTextMesh.jsonNameInUnity = jsonNameInUnity;
                    TMP_languageTextMesh.folderNameInUnity = folderNameInUnity;

                    TMP_languageTextMesh.jsonNameInBuild = jsonNameInBuild;
                    TMP_languageTextMesh.folderNameInBuild = folderNameInBuild;

                    if (fontListObject != null) { TMP_languageTextMesh.fontListObject = TMP_fontListObject; }
                }

                if (gameObject.TryGetComponent<TMP_LanguageDropdown>(out var TMP_languageDropdown))
                {
                    // Applies changes to variables.
                    TMP_languageDropdown.jsonNameInUnity = jsonNameInUnity;
                    TMP_languageDropdown.folderNameInUnity = folderNameInUnity;

                    TMP_languageDropdown.jsonNameInBuild = jsonNameInBuild;
                    TMP_languageDropdown.folderNameInBuild = folderNameInBuild;

                    if (fontListObject != null) { TMP_languageDropdown.fontListObject = TMP_fontListObject; }
                }

                if (gameObject.TryGetComponent<TMP_LanguageText>(out var TMP_languageText))
                {
                    // Applies changes to variables.
                    TMP_languageText.jsonNameInUnity = jsonNameInUnity;
                    TMP_languageText.folderNameInUnity = folderNameInUnity;

                    TMP_languageText.jsonNameInBuild = jsonNameInBuild;
                    TMP_languageText.folderNameInBuild = folderNameInBuild;

                    if (fontListObject != null) { TMP_languageText.fontListObject = TMP_fontListObject; }
                }

                if (gameObject.TryGetComponent<TMP_LanguageTextInputField>(out var TMP_languageTextInputField))
                {
                    // Applies changes to variables.
                    TMP_languageTextInputField.jsonNameInUnity = jsonNameInUnity;
                    TMP_languageTextInputField.folderNameInUnity = folderNameInUnity;

                    TMP_languageTextInputField.jsonNameInBuild = jsonNameInBuild;
                    TMP_languageTextInputField.folderNameInBuild = folderNameInBuild;

                    if (fontListObject != null) { TMP_languageTextInputField.fontListObject = TMP_fontListObject; }
                }
                /**/

                Debug.Log("Apply to current Scene Successfully.");
            }
        }
    }

    // Method called when window is closed.
    private void OnDestroy()
    {
        SaveDataToFile(); // save the information.
    }

    private void SaveDataToFile()
    {
        // Creates or opens the file for writing.
        using StreamWriter writer = new("ProjectSettings/LanguageSettingsData.txt");
        // Writes the information to the file.
        writer.WriteLine(jsonNameInUnity);
        writer.WriteLine(jsonSaveNameInUnity);
        writer.WriteLine(folderNameInUnity);
        writer.WriteLine(jsonNameInBuild);
        writer.WriteLine(jsonSaveNameInBuild);
        writer.WriteLine(folderNameInBuild);
        writer.WriteLine(firstTime);

        // Include the fontListObject's local identifier
        writer.WriteLine(fontListObject != null ? AssetDatabase.GetAssetPath(fontListObject) : string.Empty);
        /**/
        writer.WriteLine(TMP_fontListObject != null ? AssetDatabase.GetAssetPath(TMP_fontListObject) : string.Empty);
        /**/
    }

    private void LoadDataFromFile()
    {
        // If the file exists.
        if (File.Exists("ProjectSettings/LanguageSettingsData.txt"))
        {
            // Opens the file for reading.
            using StreamReader reader = new("ProjectSettings/LanguageSettingsData.txt");
            // Read information from the file.
            jsonNameInUnity = reader.ReadLine();
            jsonSaveNameInUnity = reader.ReadLine();
            folderNameInUnity = reader.ReadLine();
            jsonNameInBuild = reader.ReadLine();
            jsonSaveNameInBuild = reader.ReadLine();
            folderNameInBuild = reader.ReadLine();
            firstTime = bool.Parse(reader.ReadLine());

            string fontListObjectPath = reader.ReadLine();
            string TMP_fontListObjectPath = reader.ReadLine();

            fontListObject = !string.IsNullOrEmpty(fontListObjectPath) ? AssetDatabase.LoadAssetAtPath<LanguageFontListData>(fontListObjectPath) : null;
            /**/
            TMP_fontListObject = !string.IsNullOrEmpty(TMP_fontListObjectPath) ? AssetDatabase.LoadAssetAtPath<TMP_LanguageFontListData>(TMP_fontListObjectPath) : null;
            /**/
        }
    }

    // Method to create or replace the scriptable object for language settings.
    private void CreateOrReplaceScriptableObject()
    {
        // Ensure the "Resources" folder exists, if not, create it.
        if (!AssetDatabase.IsValidFolder("Assets/Resources"))
        {
            AssetDatabase.CreateFolder("Assets", "Resources");
        }

        string scriptableObjectPath = "Assets/Resources/Language Data.asset"; // Define the path for the scriptable object.
        LanguageSettingsData newScriptableObject = CreateInstance<LanguageSettingsData>(); // Create a new instance of the LanguageSettingsData scriptable object.
        AssetDatabase.CreateAsset(newScriptableObject, scriptableObjectPath); // Create or replace the scriptable object asset at the specified path.

        // Set the properties of the scriptable object based on current settings.
        newScriptableObject.jsonNameInUnity = jsonNameInUnity;
        newScriptableObject.jsonSaveNameInUnity = jsonSaveNameInUnity;
        newScriptableObject.folderNameInUnity = folderNameInUnity;
        newScriptableObject.jsonNameInBuild = jsonNameInBuild;
        newScriptableObject.jsonSaveNameInBuild = jsonSaveNameInBuild;
        newScriptableObject.folderNameInBuild = folderNameInBuild;

        // Set the font lists of the scriptable object.
        if (fontListObject != null) 
        { newScriptableObject.fontListData = fontListObject; }
        else { Debug.LogError("You need to add a list of fonts"); }
        /**/
        // Set the TMP font lists of the scriptable object.
        if (TMP_fontListObject != null) 
        { newScriptableObject.TMP_fontListData = TMP_fontListObject; }
        else { Debug.LogError("You need to add a list of TMP_fonts"); }
        /**/

        EditorUtility.SetDirty(newScriptableObject); // Mark the scriptable object as dirty to ensure changes are saved.

        // Save assets and refresh the Asset Database.
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        
        Debug.Log($"Language Data created successfully at: {scriptableObjectPath}");
    }
}