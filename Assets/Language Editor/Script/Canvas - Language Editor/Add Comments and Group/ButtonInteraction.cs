using UnityEngine;
using UnityEngine.UI;

public class ButtonInteraction : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private InputField inputField;
    [Space(5)]
    [SerializeField] private Button button;

    private void Start()
    {
        inputField.onValueChanged.AddListener(OnInputFieldValueChanged); // Registers a listener for the text change event in the input field.
        button.interactable = false; // Disables the button initially.
    }

    private void OnInputFieldValueChanged(string value)
    {
        // Checks if the input field is empty.
        if (string.IsNullOrEmpty(value))
        {
            button.interactable = false; // Disables the button.
        }
        else
        {
            button.interactable = true; // Activate the button.
        }
    }
}