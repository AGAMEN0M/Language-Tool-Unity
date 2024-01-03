using UnityEngine;
using UnityEngine.UI;

public class ObjectCheck : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private GameObject parentObject; // The object whose child count is checked.
    [Space(5)]
    [SerializeField] private bool hasChildObject = false; // Flag to determine if parentObject has child objects.
    [Space(5)]
    [SerializeField] private Button[] buttons; // Array of Button UI elements.
    [SerializeField] private Dropdown[] dropdown; // Array of Dropdown UI elements.
    [SerializeField] private GameObject[] gameObjects; // Array of GameObjects.

    private void Update()
    {
        hasChildObject = parentObject.transform.childCount > 0; // Check if the parent object has any child objects.

        // Update interactability for buttons based on the presence of child objects.
        for (int i = 0; i < buttons.Length; i++)
        {
            buttons[i].interactable = hasChildObject;
        }

        // Update interactability for dropdowns based on the presence of child objects.
        for (int i = 0; i < dropdown.Length; i++)
        {
            dropdown[i].interactable = hasChildObject;
        }

        // Toggle the active state of game objects based on the absence of child objects.
        for (int i = 0; i < gameObjects.Length; i++)
        {
            gameObjects[i].SetActive(!hasChildObject);
        }
    }
}