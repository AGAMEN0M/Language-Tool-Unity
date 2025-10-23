/*
 * ---------------------------------------------------------------------------
 * Description: Editor utility window that scans scenes in Build Settings for
 *              components containing an integer field named 'iD'. It helps
 *              identify duplicate IDs and quickly locate objects by ID.
 * 
 * Author: Lucas Gomes Cecchini
 * Pseudonym: AGAMENOM
 * ---------------------------------------------------------------------------
*/

#if UNITY_EDITOR
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using UnityEditor;
using UnityEngine;
using System.IO;
using System;

using static LanguageTools.Editor.LanguageEditorUtilities;

public class LanguageIDScannerWindow : EditorWindow
{
    #region === Fields ===

    private Vector2 scrollPos; // Scroll position for result list.
    private readonly Dictionary<string, List<LanguageIDInfo>> sceneLanguageIDs = new(); // Dictionary that stores scene paths and their corresponding list of found Language IDs.
    private string currentScenePath; // Stores the path of the currently open scene to restore it after scanning.
    private readonly Dictionary<string, bool> sceneFoldoutStates = new(); // Stores foldout (expanded/collapsed) state for each scene individually.
    private int searchID = 0; // Holds the user-input ID for individual searches.

    #endregion

    #region === Menu Item ===

    /// <summary>
    /// Opens the Language ID Scanner window via the Unity Editor menu.
    /// </summary>
    [MenuItem("Window/Language/Language ID Scanner", false, 2031)]
    public static void ShowWindow()
    {
        var window = GetWindow<LanguageIDScannerWindow>("Language ID Scanner");
        window.titleContent = new GUIContent("Language ID Scanner", FindTextureByName("LanguageIDScannerWindow Icon"));
    }

    /// <summary>
    /// Opens the Language ID Scanner window and sets a specific ID for immediate search.
    /// </summary>
    /// <param name="id">ID to prefill for searching.</param>
    public static void ShowWindowID(int id = 0)
    {
        ShowWindow();
        var window = GetWindow<LanguageIDScannerWindow>("Language ID Scanner");
        window.searchID = id;
    }

    #endregion

    #region === GUI ===

    /// <summary>
    /// Draws the user interface for the Language ID Scanner window.
    /// </summary>
    private void OnGUI()
    {
        GUILayout.Space(5);

        // Button to scan all scenes in the Build Settings.
        if (GUILayout.Button(new GUIContent("Scan All Scenes in Build Settings", "Scans every scene listed in Build Settings for Language IDs."), GUILayout.Height(30)))
        {
            // Ask user to save modified scenes before scanning.
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

            // Save the current scene path to reopen later.
            currentScenePath = EditorSceneManager.GetActiveScene().path;

            // Execute the scan.
            ScanAllScenesInBuild();
            EditorUtility.DisplayDialog("Language ID Scanner", "Full scan completed successfully.", "OK");

            // Reopen the previously active scene.
            if (!string.IsNullOrEmpty(currentScenePath)) EditorSceneManager.OpenScene(currentScenePath, OpenSceneMode.Single);
        }

        GUILayout.Space(10);

        // Determine if entered ID exists in the language list.
        bool validId = IsIDInLanguageList(searchID);

        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        GUILayout.Label("Search Specific ID:", EditorStyles.boldLabel);

        // Highlight field in red if ID is invalid.
        GUI.color = validId ? Color.white : Color.red;
        searchID = EditorGUILayout.IntField(new GUIContent("Target ID", "Enter the language ID to search for across all scenes."), searchID);
        GUI.color = Color.white;

        EditorGUILayout.BeginHorizontal();

        // Enable "Search" button only if ID exists in language list.
        GUI.enabled = validId;
        if (GUILayout.Button(new GUIContent("Search ID", "Search all scenes for the specified ID."), GUILayout.Height(25)))
        {
            if (validId)
            {
                if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
                currentScenePath = EditorSceneManager.GetActiveScene().path;

                ScanSpecificID(searchID); // Perform the search for a specific ID.

                // Restore the scene after the scan completes.
                if (!string.IsNullOrEmpty(currentScenePath)) EditorSceneManager.OpenScene(currentScenePath, OpenSceneMode.Single);
            }
            else
            {
                EditorUtility.DisplayDialog("Notice", $"The ID {searchID} was not found in the language list.\n\nMake sure this ID exists in your LanguageSettings file before scanning scenes.", "OK");
            }
        }
        GUI.enabled = true;

        // Button to clear previous scan results.
        if (GUILayout.Button(new GUIContent("Clear Results", "Clears all previously found results."), GUILayout.Height(25)))
        {
            sceneLanguageIDs.Clear();
        }

        EditorGUILayout.EndHorizontal();
        EditorGUILayout.EndVertical();

        GUILayout.Space(10);

        // If there are no results, display a help box.
        if (sceneLanguageIDs.Count == 0)
        {
            EditorGUILayout.HelpBox("No results found. Run a scan or search for a specific ID.", MessageType.Info);
            return;
        }

        // Begin scrollable area for scan results.
        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

        // Display scene entries.
        foreach (var kvp in sceneLanguageIDs)
        {
            string sceneName = Path.GetFileNameWithoutExtension(kvp.Key);
            var idList = kvp.Value;

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            // Retrieve or initialize foldout state for this specific scene.
            if (!sceneFoldoutStates.ContainsKey(sceneName)) sceneFoldoutStates[sceneName] = true;

            // Draw the foldout with the stored state.
            sceneFoldoutStates[sceneName] = EditorGUILayout.Foldout(
                sceneFoldoutStates[sceneName],
                $"{sceneName} ({idList.Count} IDs found)",
                true
            );

            // If this scene's foldout is expanded, list its items.
            if (sceneFoldoutStates[sceneName])
            {
                foreach (var info in idList)
                {
                    GUI.color = info.isDuplicate ? Color.red : Color.white;
                    GUILayout.BeginHorizontal();

                    // Button to select the GameObject associated with the found ID.
                    if (GUILayout.Button(new GUIContent("Select", "Selects and highlights the GameObject in the Hierarchy."), GUILayout.Width(60)))
                    {
                        // Load scene if it's not currently open.
                        if (EditorSceneManager.GetActiveScene().path != kvp.Key)
                        {
                            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
                            EditorSceneManager.OpenScene(kvp.Key, OpenSceneMode.Single);
                        }

                        // Find GameObject in scene and highlight it.
                        var target = GameObject.Find(info.hierarchyPath);
                        if (target != null)
                        {
                            Selection.activeGameObject = target;
                            EditorGUIUtility.PingObject(target);
                        }
                        else
                        {
                            Debug.LogWarning($"Could not find object: {info.hierarchyPath} in scene {sceneName}.");
                        }
                    }

                    GUI.color = Color.white;
                    GUILayout.Label($"ID: {info.id} | Component: {info.componentType} | Object: {info.objectName}");
                    GUILayout.EndHorizontal();
                }
            }

            EditorGUILayout.EndVertical();
        }

        EditorGUILayout.EndScrollView();
    }

    #endregion

    #region === Core Logic ===

    /// <summary>
    /// Scans all scenes included in Build Settings for Language IDs.
    /// </summary>
    private void ScanAllScenesInBuild()
    {
        sceneLanguageIDs.Clear();

        // Retrieve enabled scenes from Build Settings.
        var scenes = EditorBuildSettings.scenes.Where(s => s.enabled).ToArray();

        if (scenes.Length == 0)
        {
            EditorUtility.DisplayDialog("Language ID Scanner", "No scenes found in Build Settings.", "OK");
            return;
        }

        Dictionary<int, int> idOccurrences = new(); // Track ID occurrence to detect duplicates.

        foreach (var sceneSetting in scenes)
        {
            string scenePath = sceneSetting.path;

            try
            {
                // Open the scene for scanning.
                Scene scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);

                // Scan the scene for IDs.
                var ids = ScanSceneForLanguageIDs(scene);
                sceneLanguageIDs[scenePath] = ids;

                // Count ID occurrences.
                foreach (var info in ids)
                {
                    if (idOccurrences.ContainsKey(info.id))
                    {
                        idOccurrences[info.id]++;
                    }
                    else
                    {
                        idOccurrences[info.id] = 1;
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning($"Could not open scene {scenePath}: {e.Message}");
            }
        }

        // Mark duplicate IDs.
        foreach (var list in sceneLanguageIDs.Values.SelectMany(v => v))
        {
            if (idOccurrences.TryGetValue(list.id, out int count) && count > 1) list.isDuplicate = true;
        }
    }

    /// <summary>
    /// Scans only for a specific ID across all Build Settings scenes.
    /// </summary>
    /// <param name="id">The target ID to search for.</param>
    private void ScanSpecificID(int id)
    {
        sceneLanguageIDs.Clear();

        var scenes = EditorBuildSettings.scenes.Where(s => s.enabled).ToArray();
        if (scenes.Length == 0)
        {
            EditorUtility.DisplayDialog("Language ID Scanner", "No scenes found in Build Settings.", "OK");
            return;
        }

        // Iterate through all scenes and search for matches.
        foreach (var sceneSetting in scenes)
        {
            try
            {
                Scene scene = EditorSceneManager.OpenScene(sceneSetting.path, OpenSceneMode.Single);
                var matches = ScanSceneForLanguageIDs(scene).Where(x => x.id == id).ToList();
                if (matches.Count > 0) sceneLanguageIDs[sceneSetting.path] = matches;
            }
            catch (Exception e)
            {
                Debug.LogWarning($"Could not open scene {sceneSetting.path}: {e.Message}");
            }
        }

        // Show search results.
        if (sceneLanguageIDs.Count == 0)
        {
            EditorUtility.DisplayDialog("Search Result", $"No components found with ID {id}.", "OK");
        }
        else
        {
            EditorUtility.DisplayDialog("Search Result", $"Found {sceneLanguageIDs.Values.Sum(v => v.Count)} matches for ID {id}.", "OK");
        }
    }

    /// <summary>
    /// Scans a specific scene for components that have an integer field named 'iD'.
    /// </summary>
    /// <param name="scene">Scene to scan.</param>
    /// <returns>List of LanguageIDInfo entries found in the scene.</returns>
    private List<LanguageIDInfo> ScanSceneForLanguageIDs(Scene scene)
    {
        List<LanguageIDInfo> result = new();

        // Iterate through all root objects and their children.
        foreach (var root in scene.GetRootGameObjects())
        {
            foreach (var comp in root.GetComponentsInChildren<Component>(true))
            {
                if (comp == null) continue;
                var type = comp.GetType();

                // Try to find a field named 'iD' of type int.
                var idField = type.GetField("iD", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                if (idField != null && idField.FieldType == typeof(int))
                {
                    int idValue = (int)idField.GetValue(comp);
                    result.Add(new LanguageIDInfo
                    {
                        id = idValue,
                        componentType = type.Name,
                        objectName = comp.gameObject.name,
                        hierarchyPath = GetHierarchyPath(comp.gameObject),
                        isDuplicate = false
                    });
                }

                // Check if there are lists containing elements with an 'iD' field.
                foreach (var field in type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
                {
                    if (field.FieldType.IsGenericType && field.FieldType.GetGenericTypeDefinition() == typeof(List<>))
                    {
                        if (field.GetValue(comp) is not System.Collections.IEnumerable list) continue;

                        foreach (var item in list)
                        {
                            if (item == null) continue;
                            var itemType = item.GetType();
                            var subIDField = itemType.GetField("iD", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                            if (subIDField != null && subIDField.FieldType == typeof(int))
                            {
                                int idValue = (int)subIDField.GetValue(item);
                                result.Add(new LanguageIDInfo
                                {
                                    id = idValue,
                                    componentType = $"{type.Name}.{itemType.Name}",
                                    objectName = comp.gameObject.name,
                                    hierarchyPath = GetHierarchyPath(comp.gameObject),
                                    isDuplicate = false
                                });
                            }
                        }
                    }
                }
            }
        }

        return result;
    }

    /// <summary>
    /// Builds the full hierarchy path for a given GameObject.
    /// </summary>
    /// <param name="obj">GameObject to trace back through the hierarchy.</param>
    /// <returns>String representing the object's hierarchy path.</returns>
    private string GetHierarchyPath(GameObject obj)
    {
        string path = obj.name;
        var current = obj.transform;

        // Traverse upwards through the hierarchy until reaching the root.
        while (current.parent != null)
        {
            current = current.parent;
            path = $"{current.name}/{path}";
        }

        return path;
    }

    #endregion

    #region === Helper Class ===

    /// <summary>
    /// Represents metadata about a detected Language ID entry within a scene.
    /// </summary>
    private class LanguageIDInfo
    {
        public int id;
        public string componentType;
        public string objectName;
        public string hierarchyPath;
        public bool isDuplicate;
    }

    #endregion
}
#endif