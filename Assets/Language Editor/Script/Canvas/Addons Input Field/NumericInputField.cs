using UnityEngine;
using UnityEngine.UI;

public class NumericInputField : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private InputField inputField; // Reference to the InputField component.
    [Space(5)]
    [SerializeField] private bool onlyNumbers = false; // Defines whether to accept only numbers.

    private void Awake()
    {
        // Adds the ValidateInput method as a callback to the onValidateInput event of the InputField.
        inputField.onValidateInput += ValidateInput;
    }

    // Method responsible for validating the input of characters in the InputField.
    private char ValidateInput(string text, int charIndex, char addedChar)
    {
        // Checks whether only numbers are allowed or can accept other characters.
        if (!onlyNumbers && (char.IsDigit(addedChar) || addedChar == '-' || addedChar == ' ' || addedChar == ';'))
        {
            return addedChar == ' ' ? ';' : addedChar; // Allows valid character entry, replacing spaces with semicolons.
        }
        else if (onlyNumbers && (char.IsDigit(addedChar) || addedChar == '-'))
        {
            return addedChar; // It only allows numbers.
        }
        else
        {
            return '\0'; // Prevents invalid character entry.
        }
    }
}