using UnityEngine;
using UnityEngine.UI;

public class OrganizeLinguagem : MonoBehaviour
{
    [Header("Components")]
    public InputField Linguagem; // The InputField for additional information.
    public InputField LanguageComputer; // The InputField for computer information.

    void LateUpdate()
    {
        // Select the current object.
        Transform objetoTransform = transform;

        // Sets the object as the first sibling in the hierarchy.
        objetoTransform.SetAsFirstSibling();
    }
}