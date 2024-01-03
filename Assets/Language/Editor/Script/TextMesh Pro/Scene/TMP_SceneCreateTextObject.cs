using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

public static class TMP_SceneCreateTextObject
{
    [MenuItem("GameObject/3D Object/New Text (LT TMP)")]
    public static void TMP_CreateNewTextLT()
    {
        // Manually added prefab path.
        string prefabPath = "Assets/Language/Prefab/TextMesh Pro/Scene/New Text (LT TMP).prefab";
        
        // Load prefab from specified path.
        GameObject originalPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);

        if (originalPrefab == null)
        {
            Debug.LogError("Could not find the original prefab.");
            return;
        }

        // Get the currently selected game object.
        GameObject selectedGameObject = Selection.activeGameObject;

        // Create the new game object as a child of the selected game object, if there is one.
        GameObject newGameObject;
        if (selectedGameObject != null)
        {
            newGameObject = PrefabUtility.InstantiatePrefab(originalPrefab, selectedGameObject.transform) as GameObject;
        }
        else
        {
            newGameObject = PrefabUtility.InstantiatePrefab(originalPrefab) as GameObject;
        }

        // Rename the new game object.
        newGameObject.name = "New Text (LT TMP)";

        // Unpacks the created prefab.
        PrefabUtility.UnpackPrefabInstance(newGameObject, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);

        // Indicate that the scene has been modified.
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
    }
}