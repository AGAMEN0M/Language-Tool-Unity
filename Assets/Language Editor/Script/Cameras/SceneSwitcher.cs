using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneSwitcher : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private string sceneName; // The name of the scene to switch to.
    [Space(5)]
    [SerializeField] [TextArea] private string code; // The correct code to switch scenes.
    [Space(5)]
    [Header("Preview")]
    [SerializeField] private string inputString = ""; // The string to store user input for code entry.

    private bool isCheckingCode = false; // Flag to determine whether to check the code.

    private void Start()
    {
        inputString = ""; // Initialize inputString when the script starts.
    }

    private void Update()
    {
        if (Input.anyKeyDown)
        {
            char keyPressed = Input.inputString.Length > 0 ? Input.inputString[0] : '\0';

            if (isCheckingCode)
            {
                inputString += keyPressed; // Continue checking the code.

                if (inputString == code)
                {                    
                    SceneManager.LoadScene(sceneName); // Code entered correctly, switch scene.
                    inputString = ""; // Reset the input string for the next code entry.
                    isCheckingCode = false; // Reset the flag to stop further checking.
                }
                else if (!code.StartsWith(inputString))
                {
                    inputString = keyPressed.ToString(); // If the entered code does not match the expected code, reset the input and stop checking.
                }
            }
            else if (code.StartsWith(keyPressed.ToString()))
            {
                // Start checking the code when the first character of the code is pressed.
                isCheckingCode = true;
                inputString = keyPressed.ToString();
            }
        }
    }
}