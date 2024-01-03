using UnityEngine;
using UnityEngine.UI;

public class InputFieldNumeric : MonoBehaviour
{    
    [Header("Settings")]
    [SerializeField] private InputField inputField; // Reference to the InputField component.

    private void Awake()
    {        
        inputField.onValidateInput += ValidateInput; // Adds the onValidateInput event.
    }

    private char ValidateInput(string text, int charIndex, char addedChar)
    {
        // Checks whether the added character is a digit, minus sign, or comma.
        if (char.IsDigit(addedChar) || addedChar == '-' || addedChar == ',')
        {
            return addedChar; // If it's a valid character, it returns it.
        }

        // If it is not a valid character, it returns an empty character ('\0') to indicate that the character should not be entered.
        return '\0';
    }
}