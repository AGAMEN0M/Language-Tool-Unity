using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OrganizeIDS : MonoBehaviour
{
    [Header("Settings")]
    public float ID = 0; // The order number of this object.
    [Space(10)]
    [SerializeField] private OrganizeIDS[] objectsToOrganize; // Private variable that stores all objects to be sorted.
    [SerializeField] private Image view; // Reference to the Image component for displaying visuals.
    [SerializeField] private Sprite[] sprites; // Array containing sprites for the view.
    public int spritesIndex; // Index to select a sprite from the sprites array.
    [Space(10)]
    [Header("Components")]
    public Text numberID; // Reference to the text component to display the order number.
    [Space(10)]
    public InputField text; // Reference to the text field component to edit the text.
    [Space(10)]
    public InputField fontSize; // Reference to InputField component to edit font size.
    [Space(10)]
    public InputField font; // Reference to the text field component to edit the font.
    [Space(10)]
    // References to InputField components for rotation values.
    public InputField rotationX;
    public InputField rotationY;
    public InputField rotationZ;
    public InputField rotationW;
    [Space(10)]
    // References to InputField components for scale values.
    public InputField localScaleX;
    public InputField localScaleY;
    public InputField localScaleZ;
    [Space(10)]
    // References to InputField components for position values.
    public InputField anchoredPositionX;
    public InputField anchoredPositionY;
    [Space(10)]
    // References to InputField components for anchor min values.
    public InputField anchorMinX;
    public InputField anchorMinY;
    [Space(10)]
    // References to InputField components for anchor max values.
    public InputField anchorMaxX;
    public InputField anchorMaxY;
    [Space(10)]
    // References to InputField components for size delta values.
    public InputField sizeDeltaX;
    public InputField sizeDeltaY;
    [Space(10)]
    // References to InputField components for pivot values.
    public InputField pivitX;
    public InputField pivitY;
    [Space(10)]
    public Dropdown alignment; // Reference to dropdown component for editing alignment.
    [Space(10)]
    public Dropdown reverse; // Reference to dropdown menu component to reverse sort order.
    [Space(10)]
    [Header("Information")]
    public float textID = 0f; // Order number of this object in float form.
    [Space(10)]
    public bool interactableText = true; // Whether the text component is interactive.
    public bool interactableFontSize = true; // Whether the font size component is interactive.
    public bool interactableFont = true; // Whether the font component is interactive.
    public bool interactableRotation = true; // Whether rotation components are interactive.
    public bool interactableLocalScale = true; // Whether scale components are interactive.
    public bool interactableAnchoredPosition = true; // Whether position components are interactive.
    public bool interactableAnchorMin = true; // Whether anchor min components are interactive.
    public bool interactableAnchorMax = true; // Whether anchor max components are interactive.
    public bool interactableSizeDelta = true; // Whether size delta components are interactive.
    public bool interactablePivit = true; // Whether pivot components are interactive.
    [Space(10)]
    public bool interactableAlignment = true; // Whether alignment component is interactive.
    public int alignmentValue = 0; // Value selected in alignment dropdown component.
    [Space(10)]
    public bool interactableReverse = true; // Whether order reversal component is interactive.
    public int reverseValue = 0; // Value selected in reverse order dropdown component.
    [Space(10)]
    [SerializeField] private OrganizeIDS organizeIDS; // Reference to another OrganizeIDS component.

    // Start is called before the first frame update.
    private void Start()
    {
        ID = textID; // Assigns the textID value to ID for this object.
        Information(); // Displays information and sets up UI elements based on the current values.
        ID_Organize(); // Organizes objects based on their ID values.
        ViewComponent(spritesIndex); // Sets the view component based on the spritesIndex.
    }

    // Function for sorting objects based on the ID variable.
    public void ID_Organize()
    {
        objectsToOrganize = new OrganizeIDS[0]; // Clears the array before populating it again to avoid duplicates.
        objectsToOrganize = FindObjectsOfType<OrganizeIDS>(); // Finds all objects of type OrganizeIDS in the scene and populates the objectsToOrganize array.
        List<OrganizeIDS> orderedObjects = new(objectsToOrganize); // Creates a list from the array of found objects to perform sorting.
        orderedObjects.Sort((obj1, obj2) => obj1.ID.CompareTo(obj2.ID)); // Sorts the objects based on their ID values.

        // Loops through all sorted objects and updates their hierarchy based on their order.
        foreach (OrganizeIDS obj in orderedObjects)
        {
            obj.transform.SetSiblingIndex(orderedObjects.IndexOf(obj)); // Sets the object's position in the hierarchy based on its order in the sorted list.
        }
    }

    // Method for updating info panel fields with information for the current instance.
    private void Information()
    {
        numberID.text = textID.ToString() + ";"; // Sets the text of the NumberID Variable Text component to the current ID.

        // Defines whether each UI component should be interactive or not based on their respective boolean values.
        text.interactable = interactableText;
        fontSize.interactable = interactableFontSize;
        font.interactable = interactableFont;
        rotationX.interactable = interactableRotation;
        rotationY.interactable = interactableRotation;
        rotationZ.interactable = interactableRotation;
        rotationW.interactable = interactableRotation;
        localScaleX.interactable = interactableLocalScale;
        localScaleY.interactable = interactableLocalScale;
        localScaleZ.interactable = interactableLocalScale;
        anchoredPositionX.interactable = interactableAnchoredPosition;
        anchoredPositionY.interactable = interactableAnchoredPosition;
        anchorMinX.interactable = interactableAnchorMin;
        anchorMinY.interactable = interactableAnchorMin;
        anchorMaxX.interactable = interactableAnchorMax;
        anchorMaxY.interactable = interactableAnchorMax;
        sizeDeltaX.interactable = interactableSizeDelta;
        sizeDeltaY.interactable = interactableSizeDelta;
        pivitX.interactable = interactablePivit;
        pivitY.interactable = interactablePivit;
        alignment.interactable = interactableAlignment;
        alignment.value = alignmentValue; // Sets the selected value of the Dropdown component of the Alignment variable.
        reverse.interactable = interactableReverse;
        reverse.value = reverseValue; // Sets the selected value of the Dropdown component of the Reverse variable.
    }

    // Method called when the Dropdown Alignment value is changed by the user.
    public void OnDropdownValueChangedAlignment()
    {
        alignmentValue = alignment.value; // Stores the current Dropdown Alignment value in the AlignmentValue variable.
    }

    // Method called when the Dropdown Reverse value changes.
    public void OnDropdownValueChangedReverse()
    {
        reverseValue = reverse.value; // Updates the ReverseValue variable with the value selected by the user in the Reverse Dropdown.
    }

    // Method to interact with other components.
    public void Interacting()
    {
        RectTransformViewer[] componentsRectTransformViewer = FindObjectsOfType<RectTransformViewer>(); // Finds all RectTransformViewer components in the scene.

        // For each RectTransformViewer component found:
        foreach (RectTransformViewer component in componentsRectTransformViewer)
        {
            component.organizeIDS = organizeIDS; // Assigns the organizeIDS reference to the component's organizeIDS variable.
            component.GetSettings(); // Gets settings from the component.
            component.UpdateUI(); // Updates the UI of the component.
        }
    }

    // Method to set view component sprite based on componentType.
    public void ViewComponent(int componentType)
    {
        // Sets the sprite of the view component based on the provided componentType.
        // The switch statement checks the value of componentType and assigns a sprite accordingly.
        // If componentType matches a case, it sets the view sprite to the corresponding sprite from the sprites array.
        // If no match is found, it sets the view sprite to sprites[7].
        view.sprite = componentType switch
        {
            1 => sprites[0], // ComponentType 1 corresponds to sprites[0].
            2 => sprites[1], // ComponentType 2 corresponds to sprites[1].
            3 => sprites[2], // ComponentType 3 corresponds to sprites[2].
            4 => sprites[3], // ComponentType 4 corresponds to sprites[3].
            5 => sprites[4], // ComponentType 5 corresponds to sprites[4].
            6 => sprites[5], // ComponentType 6 corresponds to sprites[5].
            7 => sprites[6], // ComponentType 7 corresponds to sprites[6].
            _ => sprites[7]  // Default case: assigns sprites[7] if no match is found.
        };
    }
}