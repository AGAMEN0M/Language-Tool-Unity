/*
 * ---------------------------------------------------------------------------
 * Description: A static class that provides methods to create and configure 
 *              various language-related UI and 3D prefabs in Unity. This script includes 
 *              functionality to automatically create a UI Canvas and Event System when 
 *              generating UI prefabs, ensuring proper setup for event handling and UI 
 *              management. It also registers undo actions for easy modification tracking 
 *              within the Unity Editor. Prefabs can be instantiated for audio sources, 
 *              language scripts, buttons, images, text fields, and other UI components, 
 *              streamlining the prefab creation process for language tools.
 * Author: Lucas Gomes Cecchini
 * Pseudonym: AGAMENOM
 * ---------------------------------------------------------------------------
*/

using UnityEngine.EventSystems;
using LanguageTools.Editor;
using UnityEngine.UI;
using UnityEngine;
using UnityEditor;

public static class LanguagePrefabCreator
{
    // Creates a new UI Canvas object with necessary components.
    private static Canvas CreateUICanvas()
    {
        GameObject newCanvasObject = new("Canvas"); // Create a new GameObject for the Canvas.
        var canvasObject = newCanvasObject.AddComponent<Canvas>(); // Add the Canvas component to the GameObject.

        // Add CanvasScaler and GraphicRaycaster components for UI scaling and event handling.
        newCanvasObject.AddComponent<CanvasScaler>();
        newCanvasObject.AddComponent<GraphicRaycaster>();

        canvasObject.renderMode = RenderMode.ScreenSpaceOverlay; // Set the render mode to ScreenSpaceOverlay for standard UI display.
        canvasObject.gameObject.layer = LayerMask.NameToLayer("UI"); // Set the layer to UI for proper rendering.

        // Set sorting order for UI elements.
        canvasObject.sortingOrder = 0;
        canvasObject.targetDisplay = 0;

        // Create an Event System for managing input events in the UI.
        GameObject eventSystemObject = new("EventSystem");
        eventSystemObject.AddComponent<EventSystem>();
        eventSystemObject.AddComponent<StandaloneInputModule>();

        // Register the creation of the Canvas and Event System objects for Undo functionality.
        Undo.RegisterCreatedObjectUndo(newCanvasObject, "Create Canvas");
        Undo.RegisterCreatedObjectUndo(eventSystemObject, "Create EventSystem");

        return canvasObject; // Return the created Canvas.
    }

    // Creates and configures a prefab based on the specified file name and parent GameObject.
    private static void CreateAndConfigurePrefab(string fileName, GameObject selectedGameObject, bool isUI = false)
    {
        Canvas canvasObject = null; // Initialize canvasObject for UI prefabs.
        if (isUI)
        {
            #pragma warning disable UNT0007
            canvasObject = Object.FindAnyObjectByType<Canvas>() ?? CreateUICanvas(); // Find an existing Canvas or create a new one.
            #pragma warning restore UNT0007
        }

        // Find the original prefab by its name.
        GameObject originalPrefab = LanguageEditorUtilities.FindPrefabByName(fileName);
        if (originalPrefab == null)
        {
            Debug.LogError($"Prefab not found: {fileName}.prefab. Please check if the prefab exists in the project and the name is correct.");
            return; // Exit if the prefab is not found.
        }

        // Determine the parent transform for the new GameObject based on the provided arguments.
        Transform parentTransform = selectedGameObject != null ? selectedGameObject.transform : (isUI ? canvasObject.transform : null);
        // Instantiate the prefab as a child of the determined parent transform.
        GameObject newGameObject = PrefabUtility.InstantiatePrefab(originalPrefab, parentTransform) as GameObject;

        FinalizePrefabSetup(fileName, newGameObject); // Finalize the setup for the newly created prefab.
    }

    // Finalizes the setup for the newly created prefab.
    private static void FinalizePrefabSetup(string fileName, GameObject newGameObject)
    {
        if (newGameObject == null) return; // Exit if the new GameObject is null.

        Undo.RegisterCreatedObjectUndo(newGameObject, $"Create {fileName}"); // Register the new GameObject for Undo functionality.

        // Unpack the prefab instance to allow for modifications.
        PrefabUtility.UnpackPrefabInstance(newGameObject, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);

        Selection.activeGameObject = newGameObject; // Select the new GameObject in the hierarchy.

        // Delay the focus event to allow for user interaction after creation.
        EditorApplication.delayCall += () =>
        {
            if (Selection.activeGameObject == newGameObject)
            {
                // Trigger the rename event for the new GameObject.
                EditorWindow.focusedWindow.SendEvent(new Event { keyCode = KeyCode.F2, type = EventType.KeyDown });
            }
        };
    }

    // 3D Object Menu Items.

    [MenuItem("GameObject/Language/3D Object/Audio Source (LT)")]
    public static void CreateAudioSourcePrefab()
    {
        CreateAndConfigurePrefab("Audio Source (LT)", Selection.activeGameObject);
    }

    [MenuItem("GameObject/Language/3D Object/Language Create File (LT)")]
    public static void CreateLanguageFilePrefab()
    {
        CreateAndConfigurePrefab("Language Create File (LT)", Selection.activeGameObject);
    }

    [MenuItem("GameObject/Language/3D Object/Language Script (LT)")]
    public static void CreateLanguageScriptPrefab()
    {
        CreateAndConfigurePrefab("Language Script (LT)", Selection.activeGameObject);
    }

    [MenuItem("GameObject/Language/3D Object/New Text (LT) [Legacy]")]
    public static void CreateLegacyTextPrefab()
    {
        CreateAndConfigurePrefab("New Text (LT) [Legacy]", Selection.activeGameObject);
    }

    [MenuItem("GameObject/Language/3D Object/New Text (LT) [TMP]")]
    public static void CreateTMPTextPrefab()
    {
        CreateAndConfigurePrefab("New Text (LT) [TMP]", Selection.activeGameObject);
    }

    // UI Menu Items.

    [MenuItem("GameObject/Language/UI/Image (LT)")]
    public static void CreateImagePrefab()
    {
        CreateAndConfigurePrefab("Image (LT)", Selection.activeGameObject, true);
    }

    [MenuItem("GameObject/Language/UI/RawImage (LT)")]
    public static void CreateRawImagePrefab()
    {
        CreateAndConfigurePrefab("RawImage (LT)", Selection.activeGameObject, true);
    }

    [MenuItem("GameObject/Language/UI/Legacy/Language Manager (LT) [Legacy]")]
    public static void CreateLanguageManagerPrefab()
    {
        CreateAndConfigurePrefab("Language Manager (LT) [Legacy]", Selection.activeGameObject, true);
    }

    [MenuItem("GameObject/Language/UI/Language Manager (LT) [TMP]")]
    public static void CreateTMPLanguageManagerPrefab()
    {
        CreateAndConfigurePrefab("Language Manager (LT) [TMP]", Selection.activeGameObject, true);
    }

    [MenuItem("GameObject/Language/UI/Legacy/Button (LT) [Legacy]")]
    public static void CreateButtonPrefab()
    {
        CreateAndConfigurePrefab("Button (LT) [Legacy]", Selection.activeGameObject, true);
    }

    [MenuItem("GameObject/Language/UI/Button (LT) [TMP]")]
    public static void CreateTMPButtonPrefab()
    {
        CreateAndConfigurePrefab("Button (LT) [TMP]", Selection.activeGameObject, true);
    }

    [MenuItem("GameObject/Language/UI/Legacy/Dropdown (LT) [Legacy]")]
    public static void CreateDropdownPrefab()
    {
        CreateAndConfigurePrefab("Dropdown (LT) [Legacy]", Selection.activeGameObject, true);
    }

    [MenuItem("GameObject/Language/UI/Dropdown (LT) [TMP]")]
    public static void CreateTMPDropdownPrefab()
    {
        CreateAndConfigurePrefab("Dropdown (LT) [TMP]", Selection.activeGameObject, true);
    }

    [MenuItem("GameObject/Language/UI/Legacy/InputField (LT) [Legacy]")]
    public static void CreateInputFieldPrefab()
    {
        CreateAndConfigurePrefab("InputField (LT) [Legacy]", Selection.activeGameObject, true);
    }

    [MenuItem("GameObject/Language/UI/InputField (LT) [TMP]")]
    public static void CreateTMPInputFieldPrefab()
    {
        CreateAndConfigurePrefab("InputField (LT) [TMP]", Selection.activeGameObject, true);
    }

    [MenuItem("GameObject/Language/UI/Legacy/Text (LT) [Legacy]")]
    public static void CreateTextUIPrefab()
    {
        CreateAndConfigurePrefab("Text (LT) [Legacy]", Selection.activeGameObject, true);
    }

    [MenuItem("GameObject/Language/UI/Text (LT) [TMP]")]
    public static void CreateTMPTextUIPrefab()
    {
        CreateAndConfigurePrefab("Text (LT) [TMP]", Selection.activeGameObject, true);
    }

    [MenuItem("GameObject/Language/UI/Legacy/Toggle (LT) [Legacy]")]
    public static void CreateTogglePrefab()
    {
        CreateAndConfigurePrefab("Toggle (LT) [Legacy]", Selection.activeGameObject, true);
    }

    [MenuItem("GameObject/Language/UI/Toggle (LT) [TMP]")]
    public static void CreateTMPTogglePrefab()
    {
        CreateAndConfigurePrefab("Toggle (LT) [TMP]", Selection.activeGameObject, true);
    }
}