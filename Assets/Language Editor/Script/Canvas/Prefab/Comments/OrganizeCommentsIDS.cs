using UnityEngine;
using UnityEngine.UI;

public class OrganizeCommentsIDS : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float Comments; // Value that will be used to organize the objects.
    [Space(10)]
    [Header("Components")]
    public InputField textID; // InputField for the ID value of the objects to organize.
    public InputField Comment; // InputField for the Comments of the objects to arrange.
    [Space(10)]
    [Header("Other information")]
    [SerializeField] private GameObject ObjectComments; // current object.

    private void Update()
    {
        Components(); // Calling the Components method.

        OrganizeIDS[] objectsToOrganize = FindObjectsOfType<OrganizeIDS>(); // Finding all objects in the scene with the OrganizeIDS component.

        // Find the matching object with the closest Comments value.
        OrganizeIDS closestObject = null;
        float closestDistance = Mathf.Infinity;
        
        foreach (OrganizeIDS obj in objectsToOrganize)
        {
            // Checks that the Object ID value is the same as the Comments value and that the object is not the same object that this script is attached to.
            if (obj.ID == Comments && obj != this)
            {
                float distance = Vector3.Distance(transform.position, obj.transform.position);

                if (distance < closestDistance)
                {
                    closestObject = obj;
                    closestDistance = distance;
                }
            }
        }
        
        // Checks if a matching object was found.
        if (closestObject != null)
        {
            // Keeps the same parent as the corresponding object.
            Transform closestObjectParent = closestObject.transform.parent;
            transform.SetParent(closestObjectParent, false);

            // Defines the relative position of the current object in relation to the corresponding object in the Hierarchy.
            int siblingIndex = closestObject.transform.GetSiblingIndex();
            transform.SetSiblingIndex(siblingIndex + 1);
        }
    }

    private void Components()
    {
        // Checks whether the content of the 'textID' text field can be converted to a float number.
        // If it can, it updates the value of the 'Comments' variable to the converted value.
        if (float.TryParse(textID.text, out float parsedId))
        {
            Comments = parsedId;
        }
    }

    public void interacting()
    {
        // Finds the DeleteObject object in the scene and stores its reference in the DeleteObject component variable.
        DeleteObject componenteDeleteObject = FindObjectOfType<DeleteObject>();
        // Defines the object that will be deleted in the ObjectToDelete variable of the DeleteObject component.
        componenteDeleteObject.ObjectToDelete = ObjectComments;
    }
}