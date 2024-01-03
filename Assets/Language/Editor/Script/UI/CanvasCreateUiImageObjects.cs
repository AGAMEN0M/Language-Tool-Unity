using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEditor.SceneManagement;

public class CanvasCreateUiImageObjects : MonoBehaviour
{
    [MenuItem("GameObject/UI/Image (LT)")]
    public static void CreateImageLT()
    {
        // Looks for a Canvas object in the scene.
        Canvas canvasObject = Object.FindObjectOfType<Canvas>();

        if (canvasObject == null)
        {
            // If not found, it creates a new Canvas object in the scene.
            GameObject newCanvasObject = new("Canvas");
            canvasObject = newCanvasObject.AddComponent<Canvas>();
            newCanvasObject.AddComponent<CanvasScaler>();
            newCanvasObject.AddComponent<GraphicRaycaster>();
            canvasObject.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObject.sortingOrder = 0;
            canvasObject.targetDisplay = 0;

            // Defines the layer of the Canvas object.
            canvasObject.gameObject.layer = LayerMask.NameToLayer("UI");

            // Creates an EventSystem object to handle user interaction.
            GameObject eventSystemObject = new("EventSystem");
            eventSystemObject.AddComponent<EventSystem>();
            eventSystemObject.AddComponent<StandaloneInputModule>();
        }

        // Manually added prefab path.
        string prefabPath = "Assets/Language/Prefab/UI/Image (LT).prefab";

        // Loads the prefab from the specified path.
        GameObject originalPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);

        if (originalPrefab == null)
        {
            Debug.LogError("Could not find the original prefab.");
            return;
        }

        // Check if an object is selected inside the Canvas.
        GameObject selectedObject = Selection.activeGameObject;
        if (selectedObject != null && selectedObject.transform.IsChildOf(canvasObject.transform))
        {
            // Creates a new prefab from the existing prefab as a child of the selected object.
            GameObject newPrefab = PrefabUtility.InstantiatePrefab(originalPrefab, selectedObject.transform) as GameObject;
            newPrefab.name = "Image (LT)";

            // Unpacks the created prefab.
            PrefabUtility.UnpackPrefabInstance(newPrefab, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
        }
        else
        {
            // Creates a new prefab from the existing prefab as a child of the currently selected Canvas.
            GameObject newPrefab = PrefabUtility.InstantiatePrefab(originalPrefab, canvasObject.transform) as GameObject;
            newPrefab.name = "Image (LT)";

            // Unpacks the created prefab.
            PrefabUtility.UnpackPrefabInstance(newPrefab, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
        }

        // Indicate that the scene has been modified.
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
    }

    [MenuItem("GameObject/UI/RawImage (LT)")]
    public static void CreateRawImageLT()
    {
        // Looks for a Canvas object in the scene.
        Canvas canvasObject = Object.FindObjectOfType<Canvas>();

        if (canvasObject == null)
        {
            // If not found, it creates a new Canvas object in the scene.
            GameObject newCanvasObject = new("Canvas");
            canvasObject = newCanvasObject.AddComponent<Canvas>();
            newCanvasObject.AddComponent<CanvasScaler>();
            newCanvasObject.AddComponent<GraphicRaycaster>();
            canvasObject.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObject.sortingOrder = 0;
            canvasObject.targetDisplay = 0;

            // Defines the layer of the Canvas object.
            canvasObject.gameObject.layer = LayerMask.NameToLayer("UI");

            // Creates an EventSystem object to handle user interaction.
            GameObject eventSystemObject = new("EventSystem");
            eventSystemObject.AddComponent<EventSystem>();
            eventSystemObject.AddComponent<StandaloneInputModule>();
        }

        // Manually added prefab path.
        string prefabPath = "Assets/Language/Prefab/UI/RawImage (LT).prefab";

        // Loads the prefab from the specified path.
        GameObject originalPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);

        if (originalPrefab == null)
        {
            Debug.LogError("Could not find the original prefab.");
            return;
        }

        // Check if an object is selected inside the Canvas.
        GameObject selectedObject = Selection.activeGameObject;
        if (selectedObject != null && selectedObject.transform.IsChildOf(canvasObject.transform))
        {
            // Creates a new prefab from the existing prefab as a child of the selected object.
            GameObject newPrefab = PrefabUtility.InstantiatePrefab(originalPrefab, selectedObject.transform) as GameObject;
            newPrefab.name = "RawImage (LT)";

            // Unpacks the created prefab.
            PrefabUtility.UnpackPrefabInstance(newPrefab, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
        }
        else
        {
            // Creates a new prefab from the existing prefab as a child of the currently selected Canvas.
            GameObject newPrefab = PrefabUtility.InstantiatePrefab(originalPrefab, canvasObject.transform) as GameObject;
            newPrefab.name = "RawImage (LT)";

            // Unpacks the created prefab.
            PrefabUtility.UnpackPrefabInstance(newPrefab, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
        }

        // Indicate that the scene has been modified.
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
    }
}