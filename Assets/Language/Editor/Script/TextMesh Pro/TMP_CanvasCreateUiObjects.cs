using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEditor.SceneManagement;

public class TMP_CanvasCreateUiObjects : MonoBehaviour
{
    [MenuItem("GameObject/UI/Language Dropdown (LT TMP)")]
    public static void TMP_CreateLanguageDropdownLT()
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
        string prefabPath = "Assets/Language/Prefab/TextMesh Pro/Language Dropdown (LT TMP).prefab";

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
            newPrefab.name = "Language Dropdown (LT TMP)";

            // Unpacks the created prefab.
            PrefabUtility.UnpackPrefabInstance(newPrefab, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
        }
        else
        {
            // Creates a new prefab from the existing prefab as a child of the currently selected Canvas.
            GameObject newPrefab = PrefabUtility.InstantiatePrefab(originalPrefab, canvasObject.transform) as GameObject;
            newPrefab.name = "Language Dropdown (LT TMP)";

            // Unpacks the created prefab.
            PrefabUtility.UnpackPrefabInstance(newPrefab, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
        }

        // Indicate that the scene has been modified.
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
    }

    [MenuItem("GameObject/UI/Text (LT TMP)")]
    public static void TMP_Create2TextLT()
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
        string prefabPath = "Assets/Language/Prefab/TextMesh Pro/Text (LT TMP).prefab";

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
            newPrefab.name = "Text (LT TMP)";

            // Unpacks the created prefab.
            PrefabUtility.UnpackPrefabInstance(newPrefab, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
        }
        else
        {
            // Creates a new prefab from the existing prefab as a child of the currently selected Canvas.
            GameObject newPrefab = PrefabUtility.InstantiatePrefab(originalPrefab, canvasObject.transform) as GameObject;
            newPrefab.name = "Text (LT TMP)";

            // Unpacks the created prefab.
            PrefabUtility.UnpackPrefabInstance(newPrefab, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
        }

        // Indicate that the scene has been modified.
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
    }

    [MenuItem("GameObject/UI/Toggle (LT TMP)")]
    public static void TMP_CreateToggleLT()
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
        string prefabPath = "Assets/Language/Prefab/TextMesh Pro/Toggle (LT TMP).prefab";

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
            newPrefab.name = "Toggle (LT TMP)";

            // Unpacks the created prefab.
            PrefabUtility.UnpackPrefabInstance(newPrefab, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
        }
        else
        {
            // Creates a new prefab from the existing prefab as a child of the currently selected Canvas.
            GameObject newPrefab = PrefabUtility.InstantiatePrefab(originalPrefab, canvasObject.transform) as GameObject;
            newPrefab.name = "Toggle (LT TMP)";

            // Unpacks the created prefab.
            PrefabUtility.UnpackPrefabInstance(newPrefab, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
        }

        // Indicate that the scene has been modified.
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
    }

    [MenuItem("GameObject/UI/Button (LT TMP)")]
    public static void TMP_CreateButtonLT()
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
        string prefabPath = "Assets/Language/Prefab/TextMesh Pro/Button (LT TMP).prefab";

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
            newPrefab.name = "Button (LT TMP)";

            // Unpacks the created prefab.
            PrefabUtility.UnpackPrefabInstance(newPrefab, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
        }
        else
        {
            // Creates a new prefab from the existing prefab as a child of the currently selected Canvas.
            GameObject newPrefab = PrefabUtility.InstantiatePrefab(originalPrefab, canvasObject.transform) as GameObject;
            newPrefab.name = "Button (LT TMP)";

            // Unpacks the created prefab.
            PrefabUtility.UnpackPrefabInstance(newPrefab, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
        }

        // Indicate that the scene has been modified.
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
    }

    [MenuItem("GameObject/UI/Dropdown (LT TMP)")]
    public static void TMP_CreateDropdownLT()
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
        string prefabPath = "Assets/Language/Prefab/TextMesh Pro/Dropdown (LT TMP).prefab";

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
            newPrefab.name = "Dropdown (LT TMP)";

            // Unpacks the created prefab.
            PrefabUtility.UnpackPrefabInstance(newPrefab, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
        }
        else
        {
            // Creates a new prefab from the existing prefab as a child of the currently selected Canvas.
            GameObject newPrefab = PrefabUtility.InstantiatePrefab(originalPrefab, canvasObject.transform) as GameObject;
            newPrefab.name = "Dropdown (LT TMP)";

            // Unpacks the created prefab.
            PrefabUtility.UnpackPrefabInstance(newPrefab, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
        }

        // Indicate that the scene has been modified.
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
    }

    [MenuItem("GameObject/UI/InputField (LT TMP)")]
    public static void TMP_CreateInputFieldLT()
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
        string prefabPath = "Assets/Language/Prefab/TextMesh Pro/InputField (LT TMP).prefab";

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
            newPrefab.name = "InputField (LT TMP)";

            // Unpacks the created prefab.
            PrefabUtility.UnpackPrefabInstance(newPrefab, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
        }
        else
        {
            // Creates a new prefab from the existing prefab as a child of the currently selected Canvas.
            GameObject newPrefab = PrefabUtility.InstantiatePrefab(originalPrefab, canvasObject.transform) as GameObject;
            newPrefab.name = "InputField (LT TMP)";

            // Unpacks the created prefab.
            PrefabUtility.UnpackPrefabInstance(newPrefab, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
        }

        // Indicate that the scene has been modified.
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
    }
}