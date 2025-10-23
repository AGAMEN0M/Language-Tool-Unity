/*
 * ---------------------------------------------------------------------------
 * Description: Dynamically resizes a UI element based on Dropdown content
 *              using Unity's Legacy UI Text components. Monitors line breaks and 
 *              realigns the element to prevent overflow outside the Canvas.
 *              Also adjusts scroll sensitivity based on content length and auto-scrolls
 *              to the selected item within the ScrollRect.
 *              
 * Author: Lucas Gomes Cecchini
 * Pseudonym: AGAMENOM
 * ---------------------------------------------------------------------------
 */

using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using System;

[RequireComponent(typeof(ScrollRect))]
[AddComponentMenu("Language/UI/Legacy/Adjust Size To Dropdown (Legacy)")]
public class AdjustSizeToDropdown : MonoBehaviour
{
    #region === Nested Classes ===

    /// <summary>
    /// Stores a Text reference and a flag indicating whether the text wraps to multiple lines.
    /// </summary>
    [Serializable]
    private class BrokenTexts
    {
        [Tooltip("Reference to the legacy UI Text component to monitor for line breaks.")]
        public Text itemLabel; // Reference to the UI Text component.

        [Tooltip("Indicates whether the text has broken into multiple lines.")]
        public bool brokenText; // True if the text spans multiple lines.
    }

    #endregion

    #region === Enums ===

    /// <summary>
    /// Defines the direction in which the element expands when adjusting its size.
    /// </summary>
    public enum FitDirection { Left, Right } // Direction options for UI expansion.

    #endregion

    #region === Inspector Fields (Settings) ===

    [Header("Settings")]
    [SerializeField, Tooltip("Extra width manually added to the automatic size calculation.")]
    private float manualSizeAdjustment; // Manual value added to automatic sizing.

    [SerializeField, Tooltip("Additional width applied when any text wraps to a new line.")]
    private int sizeMultiplier = 10; // Value added when text is broken.

    [SerializeField, Tooltip("Minimum horizontal margin to maintain from the canvas edge.")]
    private float margin = 1.0f; // Minimum margin from canvas edge.

    [Space(5)]

    [SerializeField, Tooltip("Defines the initial direction for expansion when resizing the element.")]
    private FitDirection fitDirection = FitDirection.Right; // Initial direction to expand.

    #endregion

    #region === Inspector Fields (Automatic Settings) ===

    [Space(10)]

    [Header("Automatic Settings")]
    [SerializeField, Tooltip("RectTransform of the element being resized.")]
    private RectTransform parentRect; // RectTransform being resized.

    [SerializeField, Tooltip("RectTransform of the parent Canvas used for boundary checks.")]
    private RectTransform canvasRectTransform; // Canvas RectTransform.

    [SerializeField, Tooltip("ScrollRect component responsible for controlling vertical scroll behavior.")]
    private ScrollRect scrollRect; // Reference to the ScrollRect component controlling vertical scrolling.

    [SerializeField, Tooltip("Current width of the parent Canvas in pixels.")]
    private float canvasWidth; // Current canvas width.

    [SerializeField, Tooltip("Current width of the object being adjusted.")]
    private float objectWidth; // Current object width.

    [SerializeField, Tooltip("List of Text elements and whether each has wrapped into multiple lines.")]
    private List<BrokenTexts> textList; // Text components and wrap info.

    [SerializeField, Tooltip("True if any Text component has wrapped to multiple lines.")]
    private bool textIsBroken; // True if any text wraps lines.

    [SerializeField, Tooltip("Final computed adjustment value based on wrapping and manual offsets.")]
    private float sizeAdjustment; // Calculated width adjustment.

    #endregion

    #region === Unity Methods ===

    /// <summary>
    /// Initializes necessary components and collects child Text elements.
    /// </summary>
    private void Start()
    {
        parentRect = GetComponent<RectTransform>(); // Get the RectTransform of the current object.
        scrollRect = GetComponent<ScrollRect>(); // Get the ScrollRect of the current object.

        // Traverse hierarchy to locate the Canvas component.
        var current = transform.parent;
        while (current != null)
        {
            if (current.TryGetComponent(out Canvas canvasComponent))
            {
                canvasRectTransform = canvasComponent.GetComponent<RectTransform>();
                break;
            }
            current = current.parent;
        }

        if (canvasRectTransform == null)
        {
            Debug.LogError("AdjustSizeToDropdown: No Canvas found in hierarchy.", this);
            return;
        }

        // Gather all child Text components and allow overflow.
        foreach (var text in GetComponentsInChildren<Text>())
        {
            text.verticalOverflow = VerticalWrapMode.Overflow;
            textList.Add(new() { itemLabel = text, brokenText = false });
        }

        // Dynamically adjust scroll sensitivity based on number of items, ensuring usability for long lists.
        scrollRect.scrollSensitivity = Mathf.Clamp(10f + textList.Count / 5f, 10f, 100f);
        ScrollToSelected(); // Automatically scroll to the currently selected item in the list.
    }

    /// <summary>
    /// Automatically scrolls the dropdown so the selected option is visible in the viewport.
    /// </summary>
    private void ScrollToSelected()
    {
        // Ensure scrollRect and textList are initialized and contain enough items to scroll.
        if (scrollRect == null || textList == null || textList.Count < 2) return;

        // Find the index of the currently selected item (Toggle isOn).
        int selectedIndex = textList.FindIndex(t => t.itemLabel.GetComponentInParent<Toggle>().isOn == true);
        if (selectedIndex < 0) return;

        // Calculate normalized scroll position to bring selected item into view.
        float normalizedPosition = 1f - (float)selectedIndex / (textList.Count - 1);
        scrollRect.verticalNormalizedPosition = Mathf.Clamp01(normalizedPosition); // Apply clamped position.
    }

    /// <summary>
    /// Adjusts the UI size and repositioning logic each frame based on content overflow.
    /// </summary>
    private void Update()
    {
        if (textList == null || textList.Count == 0)
        {
            Debug.LogWarning("No Text objects found in the children.", this);
            return;
        }

        canvasWidth = canvasRectTransform.rect.width;
        objectWidth = parentRect.rect.width;

        // Calculate dynamic adjustment based on text line breaks.
        CheckLineBreaks();
        textIsBroken = textList.Exists(t => t.brokenText);
        sizeAdjustment += textIsBroken ? sizeMultiplier : 0f;

        float adjustment = sizeAdjustment + manualSizeAdjustment;

        // Offset the element depending on fit direction.
        parentRect.offsetMin = new Vector2(fitDirection == FitDirection.Left ? -adjustment : 0f, parentRect.offsetMin.y);
        parentRect.offsetMax = new Vector2(fitDirection == FitDirection.Right ? adjustment : 0f, parentRect.offsetMax.y);

        // Convert position to canvas-relative coordinates.
        Vector2 relativePos = canvasRectTransform.InverseTransformPoint(parentRect.position);

        // Adjust direction if element overflows canvas boundary.
        if (relativePos.x + objectWidth / 2 + margin > canvasWidth / 2)
        {
            Debug.Log("The object is coming out to the right of the Canvas!", this);
            fitDirection = FitDirection.Left;
        }

        if (relativePos.x - objectWidth / 2 - margin < -canvasWidth / 2)
        {
            Debug.Log("The object is coming out to the left of the Canvas!", this);
            fitDirection = FitDirection.Right;
        }
    }

    #endregion

    #region === Utility Methods ===

    /// <summary>
    /// Checks if any legacy Text component exceeds one line.
    /// </summary>
    private void CheckLineBreaks()
    {
        foreach (var item in textList)
        {
            item.brokenText = item.itemLabel.cachedTextGenerator.lineCount > 1;
            if (item.brokenText) Debug.LogWarning("AdjustSizeToDropdown: Text wrapped to multiple lines.", this);
        }
    }

    #endregion
}