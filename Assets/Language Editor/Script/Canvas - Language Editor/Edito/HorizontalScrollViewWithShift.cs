using UnityEngine;
using UnityEngine.UI;

public class HorizontalScrollViewWithShift : MonoBehaviour
{
    [Header("Component")]
    [SerializeField] private ScrollRect scrollView;
    [Header("Scroll Settings")]
    [Tooltip("Amount of scrolling when using Page Up/Page Down keys")]
    [SerializeField] private float pageScrollAmount = 0.1f;
    [Header("Status")]
    [Tooltip("Tracks whether the Shift key is currently pressed")]
    [SerializeField] private bool isShiftKeyPressed = false;
    [Tooltip("Tracks whether horizontal scrolling is preferred")]
    [SerializeField] private bool isHorizontalPreferred = false;

    private void Update()
    {
        // Checks if the Shift key is being pressed.
        if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
        {
            isShiftKeyPressed = true;
        }
        else
        {
            isShiftKeyPressed = false;
        }

        // Checks and toggles the preference between horizontal and vertical based on the Shift key pressed.
        if (isHorizontalPreferred && !isShiftKeyPressed)
        {
            // If the current preference is horizontal and Shift is not pressed, change the preference to vertical by disabling horizontal scrolling and enabling vertical scrolling.
            scrollView.horizontal = false;
            scrollView.vertical = true;
            isHorizontalPreferred = false;
        }
        else if (!isHorizontalPreferred && isShiftKeyPressed)
        {
            // If the current preference is vertical and Shift is pressed, change the preference to horizontal by disabling vertical scrolling and enabling horizontal scrolling.
            scrollView.horizontal = true;
            scrollView.vertical = false;
            isHorizontalPreferred = true;
        }

        // Scrolls to the bottom when Ctrl + End keys are pressed
        if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
        {
            if (Input.GetKeyDown(KeyCode.End))
            {
                // Scroll to the end in either vertical or horizontal direction based on the current preference
                if (scrollView.vertical)
                {
                    scrollView.normalizedPosition = new Vector2(0, 0);
                }
                else if (scrollView.horizontal)
                {
                    scrollView.normalizedPosition = new Vector2(1, 0);
                }
            }
        }

        // Scrolls up when Page Up key is pressed
        if (Input.GetKeyDown(KeyCode.PageUp))
        {
            if (scrollView.vertical)
            {
                // Scroll up by subtracting pageScrollAmount from normalized vertical position
                scrollView.normalizedPosition = new Vector2(0, scrollView.normalizedPosition.y + pageScrollAmount);
            }
            else if (scrollView.horizontal)
            {
                // Scroll left by subtracting pageScrollAmount from normalized horizontal position
                scrollView.normalizedPosition = new Vector2(scrollView.normalizedPosition.x - pageScrollAmount, 0);
            }
        }

        // Scrolls down when Page Down key is pressed
        if (Input.GetKeyDown(KeyCode.PageDown))
        {
            if (scrollView.vertical)
            {
                // Scroll down by adding pageScrollAmount to normalized vertical position
                scrollView.normalizedPosition = new Vector2(0, scrollView.normalizedPosition.y - pageScrollAmount);
            }
            else if (scrollView.horizontal)
            {
                // Scroll right by adding pageScrollAmount to normalized horizontal position
                scrollView.normalizedPosition = new Vector2(scrollView.normalizedPosition.x + pageScrollAmount, 0);
            }
        }
    }
}