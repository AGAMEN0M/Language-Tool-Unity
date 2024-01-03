using UnityEngine;
using UnityEngine.SceneManagement;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class ForceUpdate : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private LoadViewInformation[] loadViewInformationComponents; // Array for LoadViewInformation components.
    [Space(10)]
    [SerializeField] private TMP_LoadViewInformation[] TMP_loadViewInformationComponents; // Array for TMP_LoadViewInformation components.

    #if UNITY_EDITOR
    // Context menu method to collect information about components in the scene.
    [ContextMenu("Collect Information")]
    public void CollectInformation()
    {
        GameObject[] allObjectsInScene = Resources.FindObjectsOfTypeAll<GameObject>(); // Find all GameObjects in the scene.

        // Reset the arrays to empty.
        loadViewInformationComponents = new LoadViewInformation[0];
        TMP_loadViewInformationComponents = new TMP_LoadViewInformation[0];

        // Iterate through all objects in the scene.
        foreach (GameObject gameObject in allObjectsInScene)
        {
            // Check if the object belongs to the active scene.
            if (gameObject.scene == SceneManager.GetActiveScene())
            {
                // Attempt to get LoadViewInformation component and add it to the array.
                if (gameObject.TryGetComponent<LoadViewInformation>(out var loadViewInformation))
                {
                    ArrayUtility.Add(ref loadViewInformationComponents, loadViewInformation);
                }

                // Attempt to get TMP_LoadViewInformation component and add it to the array.
                if (gameObject.TryGetComponent<TMP_LoadViewInformation>(out var tmpLoadViewInformation))
                {
                    ArrayUtility.Add(ref TMP_loadViewInformationComponents, tmpLoadViewInformation);
                }
            }
        }
    }
    #endif

    // Update is called once per frame.
    private void Update()
    {
        // Iterate through LoadViewInformation components and load variables from file.
        foreach (LoadViewInformation componente in loadViewInformationComponents)
        {
            componente.LoadVariablesFromFile();
        }

        // Iterate through TMP_LoadViewInformation components and load variables from file.
        foreach (TMP_LoadViewInformation componente in TMP_loadViewInformationComponents)
        {
            componente.LoadVariablesFromFile();
        }
    }
}