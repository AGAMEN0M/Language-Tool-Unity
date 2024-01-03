using UnityEngine;
using UnityEngine.UI;

public class AddComments : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private GameObject prefab; // Prefab to instantiate.
    [Space(5)]
    [SerializeField] private GameObject Object; // Parent object for the instantiated prefab.
    [Space(10)]
    [Header("Components")]
    [SerializeField] private InputField TextID; // Input field for ID value.
    [SerializeField] private InputField TextComment; // Input field for comments.

    public void CreatePrefab()
    {
        GameObject objetoFilho = Object; // Reference to the parent object.

        // Instantiate the prefab and set its parent.
        GameObject prefabInstanciado = Instantiate(prefab, objetoFilho.transform.position, objetoFilho.transform.rotation);
        prefabInstanciado.transform.SetParent(objetoFilho.transform, false);

        OrganizeCommentsIDS organizeScript = prefabInstanciado.GetComponent<OrganizeCommentsIDS>(); // Get the OrganizeCommentsIDS script attached to the instantiated prefab.

        // Check if the script exists.
        if (organizeScript != null)
        {
            organizeScript.textID.text = TextID.text; // Copy the text from TextID to textID in the OrganizeCommentsIDS script.
            organizeScript.Comment.text = TextComment.text; // Copy the text from TextComment to Comment in the OrganizeCommentsIDS script.
        }
    }
}