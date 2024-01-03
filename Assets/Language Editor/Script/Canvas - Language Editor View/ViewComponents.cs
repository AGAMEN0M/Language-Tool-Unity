using UnityEngine;

public class ViewComponents : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private GameObject[] gameObjects; // Array to hold GameObject references for different components.

    private int componentType; // Variable to hold the currently selected component type.

    // This function is called at a fixed interval, similar to Update but used for physics calculations.
    private void FixedUpdate()
    {
        componentType = PlayerPrefs.GetInt("componentType"); // Retrieve the component type from PlayerPrefs.
        ViewComponent(componentType); // Call the function to manage GameObjects based on the component type.
    }

    // Function to manage GameObjects based on the component type provided.
    public void ViewComponent(int componentType)
    {
        // Loop through all the gameObjects.
        for (int i = 0; i < gameObjects.Length; i++)
        {
            // If the current index matches the componentType provided.
            if (i == componentType - 1)
            {
                gameObjects[i].SetActive(true); // Activate the corresponding GameObject.
            }
            else
            {
                gameObjects[i].SetActive(false); // Deactivate other GameObjects that don't match the componentType.
            }
        }
    }
}