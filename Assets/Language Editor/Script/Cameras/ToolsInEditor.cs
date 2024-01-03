using UnityEngine;

public class ToolsInEditor : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private GameObject[] gameObjects; // An array of GameObjects to work with.

    // This code will only be included when you are in the Unity Editor.
    #if UNITY_EDITOR
    private void Start()
    {
        ActivateAllGameObjects(); // Automatically activate all GameObjects when the game starts in the Unity Editor.
    }
    #endif
        
    // Activates all GameObjects in the array.
    [ContextMenu("Activate All GameObjects")]
    private void ActivateAllGameObjects()
    {
        foreach (GameObject obj in gameObjects)
        {
            obj.SetActive(true);
        }
    }

    // Disables all GameObjects in the array.
    [ContextMenu("Disable All GameObjects")]
    private void DisableAllGameObjects()
    {
        foreach (GameObject obj in gameObjects)
        {
            obj.SetActive(false);
        }
    }
}