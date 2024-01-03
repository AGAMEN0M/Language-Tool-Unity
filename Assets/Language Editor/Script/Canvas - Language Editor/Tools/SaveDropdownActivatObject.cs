using UnityEngine;
using UnityEngine.UI;

public class SaveDropdownActivatObject : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private Dropdown dropdown; // Reference to the Dropdown UI element in the Inspector.
    [Space(10)]
    [SerializeField] private string SaveName = "Index"; // PlayerPrefs key to store the selected index.

    private void Start()
    {
        PlayerPrefs.SetInt(SaveName, 0); // Set the initial PlayerPrefs value for the selected index to 0.
        dropdown.onValueChanged.AddListener(OnDropdownValueChanged); // Add a listener to the Dropdown's value change event.
    }

    private void OnDropdownValueChanged(int index)
    {
        PlayerPrefs.SetInt(SaveName, index); // When the Dropdown's value changes, save the new index in PlayerPrefs.
    }
}