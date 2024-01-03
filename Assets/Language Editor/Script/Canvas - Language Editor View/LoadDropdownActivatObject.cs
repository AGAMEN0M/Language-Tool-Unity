using UnityEngine;

public class LoadDropdownActivatObject : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private string SaveName = "Index"; // PlayerPrefs key to load the selected index.
    [Space(10)]
    [SerializeField] private GameObject[] objectsToActivate; // An array of GameObjects to activate or deactivate.

    private int index; // The loaded index from PlayerPrefs.

    void Update()
    {
        index = PlayerPrefs.GetInt(SaveName); // Load the selected index from PlayerPrefs.
        UpdateObjects(); // Update the state of the GameObjects based on the loaded index.
    }

    void UpdateObjects()
    {
        // Deactivate all GameObjects in the array.
        foreach (GameObject obj in objectsToActivate)
        {
            obj.SetActive(false);
        }

        // Activate the GameObject at the loaded index, if it's within the array bounds.
        if (index >= 0 && index < objectsToActivate.Length)
        {
            objectsToActivate[index].SetActive(true);
        }
    }
}
