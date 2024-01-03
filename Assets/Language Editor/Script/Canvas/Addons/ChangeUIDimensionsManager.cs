using UnityEngine;

public class ChangeUIDimensionsManager : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private GameObject content; // Reference to the parent GameObject containing ChangeUIDimensions scripts.

    // Method to manage the ChangeUIDimensions scripts.
    public void Manager(bool change)
    {
        ChangeUIDimensions[] scriptsNosFilhos = content.GetComponentsInChildren<ChangeUIDimensions>(); // Get all ChangeUIDimensions scripts in the children of the 'content' GameObject.

        // Loop through each ChangeUIDimensions script found.
        foreach (ChangeUIDimensions script in scriptsNosFilhos)
        {
            // Toggle the 'toggle' boolean of each script based on the 'change' parameter.
            script.toggle = !change; // Invert the value of 'change' and assign it to 'toggle'.
        }

        UpdateOrganize scriptEncontrado = FindObjectOfType<UpdateOrganize>(); // Find an object with the UpdateOrganize script attached.
        scriptEncontrado.UpdateOrganization(); // Call the UpdateOrganization method of the UpdateOrganize script.
    }
}