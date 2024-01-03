using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OrganizeIDGroups : MonoBehaviour
{
    [Header("Settings")]
    public float Text = 0; // Text value.
    public List<float> IDs; // IDs list.
    [Space(10)]
    [SerializeField] private float Chronometer = 0.1f; // Timer to wait before sorting the IDs.
    [Space(10)]
    [SerializeField] private OrganizeIDGroups[] ObjectsToOrganize; // Array of objects to organize.
    [SerializeField] private OrganizeIDS[] allObjects; // Array of all OrganizeIDS objects.
    [SerializeField] private List<OrganizeIDS> matchingObjects; // List of objects that match IDs.
    [SerializeField] private List<float> previousIDs; // List of previous IDs.
    [Space(10)]
    [Header("Components")]
    public InputField textID; // Input field for ID text.
    public InputField textIDs; // Input field for list of IDs.
    public InputField texts; // Input field for other texts.
    public GameObject Content; // Content for the organized objects.
    [Space(10)]
    [Header("Other information")]
    [SerializeField] private GameObject ObjectIDGroups; // ID group object.

    void Start()
    {
        previousIDs = new List<float>(IDs);
        StartCoroutine(ID_Organize()); // Start the coroutine to wait before sorting IDs.
    }

    // Coroutine to wait before sorting IDs.
    public IEnumerator ID_Organize()
    {
        yield return new WaitForSeconds(Chronometer);

        Array.Clear(ObjectsToOrganize, 0, ObjectsToOrganize.Length); // Clears the array of objects to be sorted.
        ObjectsToOrganize = FindObjectsOfType<OrganizeIDGroups>(); // Fills the array with all OrganizeIDGroups objects from the scene.

        List<OrganizeIDGroups> orderedObjects = new List<OrganizeIDGroups>(ObjectsToOrganize); // Creates a list from array of objects.
        orderedObjects.Sort((obj1, obj2) => obj1.Text.CompareTo(obj2.Text)); // Sorts the list based on each object's Text value.

        // Sets the object's index in the parent to match its index in the list.
        foreach (OrganizeIDGroups obj in orderedObjects)
        {
            obj.transform.SetSiblingIndex(orderedObjects.IndexOf(obj));
        }

        OrganizeIDs(); // Calls the function to continue sorting the IDs.
    }

    // Function to organize the IDs.
    public void OrganizeIDs()
    {
        allObjects = FindObjectsOfType<OrganizeIDS>();

        matchingObjects = new List<OrganizeIDS>();

        // Checks that each OrganizeIDS object has a matching ID in the ID list.
        foreach (OrganizeIDS obj in allObjects)
        {
            if (IDs.Contains(obj.ID))
            {
                matchingObjects.Add(obj);
            }
        }

        // Sorts the matchingObjects list based on the ID property value in ascending order.
        matchingObjects.Sort((obj1, obj2) => obj1.ID.CompareTo(obj2.ID));

        // Sets the OrganizeIDS object as a child of the content (Content) if it is a newly added matching object.
        foreach (OrganizeIDS obj in matchingObjects)
        {
            if (!previousIDs.Contains(obj.ID))
            {
                obj.transform.SetParent(Content.transform);
            }
        }

        // Removes the OrganizeIDS object from the current parent if it is not a newly added matching object, then sets it as a child of the object with the "Player" tag (if it exists).
        foreach (OrganizeIDS obj in allObjects)
        {
            if (!matchingObjects.Contains(obj))
            {
                if (previousIDs.Contains(obj.ID))
                {
                    obj.transform.SetParent(null);

                    // Finds the object with the "Player" tag and sets it as the parent of the OrganizeIDS object.
                    GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
                    if (playerObject != null)
                    {
                        obj.transform.SetParent(playerObject.transform);
                    }
                }
            }
        }

        previousIDs.Clear();
        previousIDs.AddRange(IDs);
    }

    // Function to get the components.
    public void Components()
    {
        IDs.Clear(); // Clears the list of IDs.

        if (float.TryParse(textID.text, out float parsedId))
        {
            Text = parsedId; // Updates the text value with the parsed ID.
        }

        if (!string.IsNullOrEmpty(textIDs.text))
        {
            string[] idStrings = textIDs.text.Split(';'); // Splits the string of IDs into substrings separated by the ';' character.
            List<float> newIDs = new List<float>(); // Creates a new list of IDs.

            foreach (string idString in idStrings)
            {
                if (float.TryParse(idString, out float newId))
                {
                    newIDs.Add(newId); // Adds each parsed ID to the new list of IDs.
                }
            }

            IDs = newIDs; // Updates the list of IDs with the new list of parsed IDs.
        }
    }

    // Function to interact.
    public void interacting()
    {
        DeleteObject componenteDeleteObject = FindObjectOfType<DeleteObject>();// Finds the DeleteObject component in the scene.
        componenteDeleteObject.ObjectToDelete = ObjectIDGroups; // Defines the object to be deleted as the ObjectIDGroups.
    }
}