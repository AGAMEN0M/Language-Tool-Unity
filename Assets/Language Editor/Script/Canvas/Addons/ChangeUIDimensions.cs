using UnityEngine;

public class ChangeUIDimensions : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private RectTransform panel; // Reference to the RectTransform of the UI panel.
    public bool toggle; // Boolean toggle to control the panel size change.
    [Space(10)]
    [SerializeField] private Vector2 active = new(-600, 0); // Dimensions when toggle is true.
    [SerializeField] private Vector2 disabled = new(600, 170); // Dimensions when toggle is false.

    private Transform parent; // Reference to the initial parent of this object.

    private void Start()
    {
        parent = transform.parent; // Store the initial parent of this object.
    }

    private void FixedUpdate()
    {
        // Check if the parent of this object has changed.
        if (parent != transform.parent)
        {
            toggle = false; // Reset toggle if the parent has changed.
            parent = transform.parent; // Update the parent reference.
        }

        // Change the size of the panel based on the toggle value.
        if (toggle)
        {
            panel.sizeDelta = active; // Set panel size to 'active' dimensions.
        }
        else
        {
            panel.sizeDelta = disabled; // Set panel size to 'disabled' dimensions.
        }
    }
}
