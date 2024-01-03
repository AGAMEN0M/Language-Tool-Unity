using UnityEngine;

public class DeleteObject : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private KeyCode DeleteButton = KeyCode.Delete; // Defines the key that will be used to delete the object.
    [Space(5)]
    public GameObject ObjectToDelete; // Reference to the object to be deleted.

    void Update()
    {
        // Checks whether the defined key was pressed.
        if (Input.GetKeyDown(DeleteButton))
        {
            Delete(); // Calls the function to delete the object.
        }
    }

    // Function to delete the object.
    public void Delete()
    {
        // Checks if the object exists before deleting it.
        if (ObjectToDelete != null)
        {
            // If the object has the OrganizeIDGroups script, move its children to the object with the tag "Player".
            OrganizeIDGroups organizeIDGroupsScript = ObjectToDelete.GetComponent<OrganizeIDGroups>();

            if (organizeIDGroupsScript != null)
            {
                GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
                if (playerObject != null)
                {
                    // Get the Transform of the content within the OrganizeIDGroups script.
                    Transform contentTransform = organizeIDGroupsScript.Content.transform;

                    // Move each child of the content to the playerObject.
                    for (int i = contentTransform.childCount - 1; i >= 0; i--)
                    {
                        Transform child = contentTransform.GetChild(i);
                        child.SetParent(playerObject.transform);
                    }
                }
            }

            Destroy(ObjectToDelete); // Deletes the object.
        }
        else
        {
            Debug.LogWarning("ObjectToDelete is null. Cannot delete object."); // Displays a warning message on the console if the object does not exist.
        }
    }
}