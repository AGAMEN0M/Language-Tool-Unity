using UnityEngine;
using UnityEngine.UI;

public class AddGroup : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private GameObject prefab; // Prefab to instantiate.
    [Space(5)]
    [SerializeField] private GameObject Object; // Parent object where the prefab will be instantiated.
    [Space(10)]
    [Header("Components")]
    [SerializeField] private InputField TextID; // Input field for ID.
    [SerializeField] private InputField TextIDs; // Input field for IDs.
    [SerializeField] private InputField Text; // Input field for text.

    public void CreatePrefab()
    {
        GameObject objetoFilho = Object; // Get the parent object where the prefab will be instantiated.
        GameObject prefabInstanciado = Instantiate(prefab, objetoFilho.transform.position, objetoFilho.transform.rotation); // Instantiate the prefab with the same position and rotation as the parent object.
        prefabInstanciado.transform.SetParent(objetoFilho.transform, false); // Set the parent of the instantiated prefab to be the parent object.

        OrganizeIDGroups organizeScript = prefabInstanciado.GetComponent<OrganizeIDGroups>(); // Get the OrganizeIDGroups script component from the instantiated prefab.

        // If the OrganizeIDGroups script is found on the instantiated prefab.
        if (organizeScript != null)
        {
            // Set the text values from the input fields to the corresponding variables in the OrganizeIDGroups script.
            organizeScript.textID.text = TextID.text;
            organizeScript.textIDs.text = TextIDs.text;
            organizeScript.texts.text = Text.text;
        }
    }
}