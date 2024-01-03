using UnityEngine;

public class CheckParentAndDestroy : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private string Dad1 = "Content"; // Name of the first desired parent object.
    [Space(5)]
    [SerializeField] private string Dad2 = "ID Groups(Clone)"; // Name of the second desired parent object.

    private void Update()
    {
        // Check that the object does not have a parent, or that the parent's name does not equal Dad1, or that the parent's parent is null, or that the parent's parent's name does not equal Dad2.
        if (transform.parent == null || transform.parent.name != Dad1 || transform.parent.parent == null || transform.parent.parent.name != Dad2)
        {
            Destroy(gameObject); // Destroy the object.
        }
        else
        {
            transform.SetAsLastSibling(); // Move the object to the bottom of the hierarchy.
        }
    }
}