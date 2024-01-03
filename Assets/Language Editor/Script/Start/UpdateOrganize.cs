using System.Collections;
using UnityEngine;

public class UpdateOrganize : MonoBehaviour
{
    public void UpdateOrganization()
    {
        Organization(); // start the method.
    }

    // This method is responsible for organizing the objects with the OrganizeIDS component.
    void Organization()
    {
        OrganizeIDS[] objectsToOrganize = FindObjectsOfType<OrganizeIDS>(); // Searches all objects with the OrganizeIDS component.

        StartCoroutine(OrganizeAllObjects()); // The organization routine of all objects begins.

        // For each object with the OrganizeIDS component found, it calls the ID_Organize() method.
        foreach (OrganizeIDS obj in objectsToOrganize)
        {
            obj.ID_Organize();
        }
    }

    // This method is responsible for organizing objects with the OrganizeIDGroups component.
    IEnumerator OrganizeAllObjects()
    {
        OrganizeIDGroups[] GroupsOrganizeScripts = FindObjectsOfType<OrganizeIDGroups>(); // Searches all objects with the OrganizeIDGroups component.

        // For each script found, the organization routine ID_Organize() and the Components method are started.
        foreach (OrganizeIDGroups script in GroupsOrganizeScripts)
        {
            script.Components();
            yield return StartCoroutine(script.ID_Organize());
        }
    }
}