using UnityEngine;

public class LoadConfirmationButton : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private LoadFile LoadScript; // Reference to a script for loading functionality.
    [SerializeField] private GameObject ConfirmationScreen; // Reference to a UI confirmation screen.
    [Space(5)]
    [SerializeField] private GameObject Content; // Reference to a content container.

    // This method is called when the confirmation button is clicked.
    public void ConfirmationButton()
    {
        // Check if the Content GameObject has child objects.
        if (Content.transform.childCount > 0)
        {
            ConfirmationScreen.SetActive(true); // If there are child objects, activate the ConfirmationScreen to show a confirmation dialog.
        }
        else
        {
            LoadScript.Load(); // If there are no child objects, call the Load() method from the LoadScript to perform loading.
        }
    }
}