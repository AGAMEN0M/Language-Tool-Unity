/*
 * ---------------------------------------------------------------------------
 * Description: This component dynamically adjusts the size of a UI element 
 *              in Unity based on the dimensions of a Dropdown menu. It ensures that the 
 *              UI element fits well within the Canvas boundaries, accommodating text line 
 *              breaks and defined margins. The adjustment direction can be set to 
 *              either Left or Right, providing flexibility in UI layout.
 * Author: Lucas Gomes Cecchini
 * Pseudonym: AGAMENOM
 * ---------------------------------------------------------------------------
*/

using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

[AddComponentMenu("Language/UI/Legacy/Adjust Size To Dropdown (Legacy)")]
public class AdjustSizeToDropdown : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float manualSizeAdjustment; // Manual size adjustment value for the UI element.
    [SerializeField] private int sizeMultiplier = 10; // Multiplier used for adjusting the size dynamically.
    [SerializeField] private float margin = 1.0f; // Margin to add extra spacing between the UI element and canvas boundaries.
    [Space(5)]
    [SerializeField] private FitDirection fitDirection = FitDirection.Right; // Fit direction (Left or Right) for the UI element.
    [Space(10)]
    [Header("Automatic Settings")]
    [SerializeField] private RectTransform parentRect; // Reference to the RectTransform of the UI element.
    [SerializeField] private RectTransform canvasRectTransform; // Reference to the RectTransform of the Canvas.
    [SerializeField] private float canvasWidth; // Width of the canvas.
    [SerializeField] private float objectWidth; // Width of the UI element.
    [SerializeField] private List<BrokenTexts> textList; // List of Text components and their line break status.
    [SerializeField] private bool textIsBroken; // Flag indicating if text breaks into multiple lines.
    [SerializeField] private float sizeAdjustment; // Adjustment value for UI element size.

    // Enum to represent the fit direction (Left or Right) for the UI element.
    public enum FitDirection
    {
        Left,
        Right
    }

    private void Start()
    {
        parentRect = GetComponent<RectTransform>(); // Get the RectTransform of the UI element.

        Canvas highestCanvas = null;
        Transform currentTransform = transform.parent;

        // Traverse up the parent hierarchy to find the highest-level Canvas component that contains the UI element.
        while (currentTransform != null)
        {
            if (currentTransform.TryGetComponent<Canvas>(out var canvasComponent))
            {
                highestCanvas = canvasComponent;
            }

            currentTransform = currentTransform.parent;
        }

        // Set the RectTransform of the Canvas that contains the UI element.
        if (highestCanvas != null)
        {
            canvasRectTransform = highestCanvas.GetComponent<RectTransform>();
        }
        else
        {
            Debug.LogError("The object is not contained in a Canvas or does not have a Canvas in the hierarchy."); // If no Canvas is found, log an error.
        }

        var textComponents = GetComponentsInChildren<Text>(); // Get all Texts in the hierarchy.

        // Initialize the list of Text components within the UI element.
        foreach (Text text in textComponents)
        {
            text.verticalOverflow = VerticalWrapMode.Overflow;
            BrokenTexts newText = new() { itemLabel = text, brokenText = false };
            textList.Add(newText);
        }
    }

    private void Update()
    {
        // Get the width of the canvas and the current width of the UI element.
        canvasWidth = canvasRectTransform.rect.width;
        objectWidth = parentRect.rect.width;

        // If no Text components are found, log a warning and return.
        if (textList == null && textList.Count == 0)
        {
            Debug.LogWarning("No Text objects found in the children.");
            return;
        }

        // Calculate the preferred width of the largest Text component inside the UI element.
        CheckTheLineBreak();
        textIsBroken = false;

        foreach (BrokenTexts item in textList)
        {
            if (item.brokenText)
            {
                textIsBroken = true;
                break;
            }
        }

        // Increase the size adjustment if text breaks into multiple lines.
        if (textIsBroken)
        {
            sizeAdjustment += sizeMultiplier;
        }

        // Variables to store the left and right width, and left and right adjustments.
        float leftAdjustment = 0f;
        float rightAdjustment = 0f;

        // Based on the chosen fitDirection, calculate the left or right adjustments for the UI element's width.
        if (fitDirection == FitDirection.Left)
        {
            leftAdjustment = sizeAdjustment + manualSizeAdjustment;
        }
        else if (fitDirection == FitDirection.Right)
        {
            rightAdjustment = sizeAdjustment + manualSizeAdjustment;
        }

        // Apply the calculated adjustments to the UI element's offsetMin and offsetMax to resize it.
        parentRect.offsetMin = new Vector2(-leftAdjustment, parentRect.offsetMin.y);
        parentRect.offsetMax = new Vector2(rightAdjustment, parentRect.offsetMax.y);

        Vector2 objectPosition = canvasRectTransform.InverseTransformPoint(parentRect.position); // Calculate the position of the UI element relative to the canvas.

        // Check if the UI element goes beyond the canvas boundaries and update the fitDirection accordingly.
        if (objectPosition.x + objectWidth / 2 + margin > canvasWidth / 2)
        {
            Debug.Log("The object is coming out to the right of the Canvas!");
            fitDirection = FitDirection.Left;
        }

        if (objectPosition.x - objectWidth / 2 - margin < -canvasWidth / 2)
        {
            Debug.Log("The object is coming out to the left of the Canvas!");
            fitDirection = FitDirection.Right;
        }
    }

    // Check if any Text component breaks into multiple lines.
    private void CheckTheLineBreak()
    {
        foreach (BrokenTexts item in textList)
        {
            // Check if the current Text component breaks into multiple lines.
            if (item.itemLabel.cachedTextGenerator.lineCount > 1)
            {
                item.brokenText = true; // Set the brokenText flag to true if the text breaks into multiple lines.
                Debug.LogWarning("Text broken into multiple lines!");
            }
            else
            {
                item.brokenText = false; // Set the brokenText flag to false if the text does not break into multiple lines.
            }
        }
    }

    [System.Serializable]
    private class BrokenTexts
    {
        public Text itemLabel; // Reference to the Text component.
        public bool brokenText; // Flag indicating if the text breaks into multiple lines.
    }
}