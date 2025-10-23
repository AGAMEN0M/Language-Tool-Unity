/*
 * ---------------------------------------------------------------------------
 * Description: This static class provides utility methods for instantiating and configuring 
 *              language-related prefabs in Unity, both in 3D and UI contexts. It includes 
 *              automatic setup of a UI Canvas and Event System when needed, and supports 
 *              instantiating prefabs like language managers, buttons, toggles, input fields, 
 *              and more. Undo registration and prefab unpacking are handled to integrate 
 *              seamlessly into Unity's editor workflow.
 * 
 * Author: Lucas Gomes Cecchini
 * Pseudonym: AGAMENOM
 * ---------------------------------------------------------------------------
*/

using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine;
using UnityEditor;

using static LanguageTools.Editor.LanguageEditorUtilities;

public static class LanguagePrefabCreator
{
    #region === Canvas & Prefab Setup Utilities ===

    /// <summary>
    /// Creates a new Canvas with all required UI components and an EventSystem.
    /// </summary>
    private static Canvas CreateUICanvas()
    {
        // Create a new GameObject for the Canvas.
        GameObject canvasGO = new("Canvas");

        // Add necessary UI components to the Canvas.
        var canvas = canvasGO.AddComponent<Canvas>();
        canvasGO.AddComponent<CanvasScaler>();
        canvasGO.AddComponent<GraphicRaycaster>();

        // Configure canvas rendering settings.
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.gameObject.layer = LayerMask.NameToLayer("UI");
        canvas.sortingOrder = 0;
        canvas.targetDisplay = 0;

        // Create and configure the EventSystem.
        GameObject eventSystemGO = new("EventSystem");
        eventSystemGO.AddComponent<EventSystem>();
        eventSystemGO.AddComponent<StandaloneInputModule>();

        // Register both objects for Undo in the editor.
        Undo.RegisterCreatedObjectUndo(canvasGO, "Create Canvas");
        Undo.RegisterCreatedObjectUndo(eventSystemGO, "Create EventSystem");

        // Return the created Canvas component.
        return canvas;
    }

    /// <summary>
    /// Instantiates and configures a prefab under the given parent.
    /// </summary>
    /// <param name="fileName">Name of the prefab to find and instantiate.</param>
    /// <param name="selectedGameObject">The parent GameObject.</param>
    /// <param name="isUI">Whether the prefab is UI-based and requires a Canvas.</param>
    private static void CreateAndConfigurePrefab(string fileName, GameObject selectedGameObject, bool isUI = false)
    {
        #pragma warning disable
        #pragma warning disable UNT0007
        // Try to find an existing Canvas or create a new one if it's a UI prefab.
        var canvas = isUI ? Object.FindAnyObjectByType<Canvas>() ?? CreateUICanvas() : null;
        #pragma warning restore UNT0007
        #pragma warning restore

        // Load the prefab by name from the asset database.
        var prefab = FindPrefabByName(fileName);
        if (prefab == null)
        {
            Debug.LogError($"Prefab not found: {fileName}.prefab. Ensure it exists in the project.");
            return;
        }

        // Determine the parent transform for the new instance.
        var parent = selectedGameObject != null ? selectedGameObject.transform : (isUI ? canvas.transform : null);

        // Instantiate the prefab as a child of the selected parent.
        var instance = PrefabUtility.InstantiatePrefab(prefab, parent) as GameObject;

        // Finalize the setup for the newly created instance.
        FinalizePrefabSetup(fileName, instance);
    }

    /// <summary>
    /// Finalizes the setup of a newly created prefab: unpack, register undo, and trigger rename.
    /// </summary>
    /// <param name="fileName">The original prefab's name.</param>
    /// <param name="newGameObject">The instantiated prefab GameObject.</param>
    private static void FinalizePrefabSetup(string fileName, GameObject newGameObject)
    {
        if (newGameObject == null) return;

        // Allow undo for the creation of the object.
        Undo.RegisterCreatedObjectUndo(newGameObject, $"Create {fileName}");

        // Fully unpack the prefab so it's editable as a regular GameObject.
        PrefabUtility.UnpackPrefabInstance(newGameObject, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);

        // Select the newly created object in the hierarchy.
        Selection.activeGameObject = newGameObject;

        // Automatically trigger rename mode for the selected object after the frame.
        EditorApplication.delayCall += () =>
        {
            if (Selection.activeGameObject == newGameObject)
            {
                EditorWindow.focusedWindow.SendEvent(new()
                {
                    keyCode = KeyCode.F2,
                    type = EventType.KeyDown
                });
            }
        };
    }

    #endregion

    #region === 3D Prefab Menu ===

    /// <summary>Creates a Language Create File (LT) prefab.</summary>
    [MenuItem("GameObject/Language/3D Object/Language Create File (LT)", false, 1)]
    public static void CreateLanguageFilePrefab() => CreateAndConfigurePrefab("Language Create File (LT)", Selection.activeGameObject);

    /// <summary>Creates a Language Script (LT) prefab.</summary>
    [MenuItem("GameObject/Language/3D Object/Language Script (LT)", false, 2)]
    public static void CreateLanguageScriptPrefab() => CreateAndConfigurePrefab("Language Script (LT)", Selection.activeGameObject);

    /// <summary>Creates an Audio Source (LT) prefab.</summary>
    [MenuItem("GameObject/Language/3D Object/Audio Source (LT)", false, 3)]
    public static void CreateAudioSourcePrefab() => CreateAndConfigurePrefab("Audio Source (LT)", Selection.activeGameObject);

    /// <summary>Creates a Legacy Text (LT) prefab.</summary>
    [MenuItem("GameObject/Language/3D Object/New Text (LT) [Legacy]", false, 4)]
    public static void CreateLegacyTextPrefab() => CreateAndConfigurePrefab("New Text (LT) [Legacy]", Selection.activeGameObject);

    /// <summary>Creates a TMP Text (LT) prefab.</summary>
    [MenuItem("GameObject/Language/3D Object/New Text (LT) [TMP]", false, 5)]
    public static void CreateTMPTextPrefab() => CreateAndConfigurePrefab("New Text (LT) [TMP]", Selection.activeGameObject);

    #endregion

    #region === UI Prefab Menu ===

    /// <summary>Creates a RawImage (LT) UI prefab.</summary>
    [MenuItem("GameObject/Language/UI/RawImage (LT)", false, 1)]
    public static void CreateRawImagePrefab() => CreateAndConfigurePrefab("RawImage (LT)", Selection.activeGameObject, true);

    /// <summary>Creates an Image (LT) UI prefab.</summary>
    [MenuItem("GameObject/Language/UI/Image (LT)", false, 2)]
    public static void CreateImagePrefab() => CreateAndConfigurePrefab("Image (LT)", Selection.activeGameObject, true);

    /// <summary>Creates a TMP Language Manager UI prefab.</summary>
    [MenuItem("GameObject/Language/UI/Language Manager (LT) [TMP]", false, 3)]
    public static void CreateTMPLanguageManagerPrefab() => CreateAndConfigurePrefab("Language Manager (LT) [TMP]", Selection.activeGameObject, true);

    /// <summary>Creates a TMP Dropdown UI prefab.</summary>
    [MenuItem("GameObject/Language/UI/Dropdown (LT) [TMP]", false, 4)]
    public static void CreateTMPDropdownPrefab() => CreateAndConfigurePrefab("Dropdown (LT) [TMP]", Selection.activeGameObject, true);

    /// <summary>Creates a TMP InputField UI prefab.</summary>
    [MenuItem("GameObject/Language/UI/InputField (LT) [TMP]", false, 5)]
    public static void CreateTMPInputFieldPrefab() => CreateAndConfigurePrefab("InputField (LT) [TMP]", Selection.activeGameObject, true);

    /// <summary>Creates a TMP Toggle UI prefab.</summary>
    [MenuItem("GameObject/Language/UI/Toggle (LT) [TMP]", false, 6)]
    public static void CreateTMPTogglePrefab() => CreateAndConfigurePrefab("Toggle (LT) [TMP]", Selection.activeGameObject, true);

    /// <summary>Creates a TMP Button UI prefab.</summary>
    [MenuItem("GameObject/Language/UI/Button (LT) [TMP]", false, 7)]
    public static void CreateTMPButtonPrefab() => CreateAndConfigurePrefab("Button (LT) [TMP]", Selection.activeGameObject, true);

    /// <summary>Creates a TMP Text UI prefab.</summary>
    [MenuItem("GameObject/Language/UI/Text (LT) [TMP]", false, 8)]
    public static void CreateTMPTextUIPrefab() => CreateAndConfigurePrefab("Text (LT) [TMP]", Selection.activeGameObject, true);

    /// <summary>Creates a Force Import of Fonts prefab.</summary>
    [MenuItem("GameObject/Language/UI/Force Import of Fonts [TMP] (Complement)", false, 9)]
    public static void CreateForceImportofFontsPrefab() => CreateAndConfigurePrefab("{Force Import of Fonts}", null);

    #endregion

    #region === UI Legacy Prefab Menu ===

    /// <summary>Creates a Legacy Language Manager UI prefab.</summary>
    [MenuItem("GameObject/Language/UI/Legacy/Language Manager (LT) [Legacy]", false, 1)]
    public static void CreateLanguageManagerPrefab() => CreateAndConfigurePrefab("Language Manager (LT) [Legacy]", Selection.activeGameObject, true);

    /// <summary>Creates a Legacy Dropdown UI prefab.</summary>
    [MenuItem("GameObject/Language/UI/Legacy/Dropdown (LT) [Legacy]", false, 2)]
    public static void CreateDropdownPrefab() => CreateAndConfigurePrefab("Dropdown (LT) [Legacy]", Selection.activeGameObject, true);

    /// <summary>Creates a Legacy InputField UI prefab.</summary>
    [MenuItem("GameObject/Language/UI/Legacy/InputField (LT) [Legacy]", false, 3)]
    public static void CreateInputFieldPrefab() => CreateAndConfigurePrefab("InputField (LT) [Legacy]", Selection.activeGameObject, true);

    /// <summary>Creates a Legacy Toggle UI prefab.</summary>
    [MenuItem("GameObject/Language/UI/Legacy/Toggle (LT) [Legacy]", false, 4)]
    public static void CreateTogglePrefab() => CreateAndConfigurePrefab("Toggle (LT) [Legacy]", Selection.activeGameObject, true);

    /// <summary>Creates a Legacy Button UI prefab.</summary>
    [MenuItem("GameObject/Language/UI/Legacy/Button (LT) [Legacy]", false, 5)]
    public static void CreateButtonPrefab() => CreateAndConfigurePrefab("Button (LT) [Legacy]", Selection.activeGameObject, true);

    /// <summary>Creates a Legacy Text UI prefab.</summary>
    [MenuItem("GameObject/Language/UI/Legacy/Text (LT) [Legacy]", false, 6)]
    public static void CreateTextUIPrefab() => CreateAndConfigurePrefab("Text (LT) [Legacy]", Selection.activeGameObject, true);

    #endregion
}