/*
 * ---------------------------------------------------------------------------
 * Description: Dynamically adjusts a UI element's width to fit its Dropdown content.
 *              This component uses TextMeshPro elements to detect line breaks and
 *              adapts the UI layout accordingly. It ensures proper alignment inside
 *              the Canvas by changing its fit direction when content overflows.
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
using TMPro;

[RequireComponent(typeof(ScrollRect))]
[AddComponentMenu("Language/UI/TextMesh Pro/Adjust Size To Dropdown (TMP)")]
public class AdjustSizeToDropdownTMP : MonoBehaviour
{
    #region === Nested Classes ===

    /// <summary>
    /// Stores a TMP_Text reference and a flag indicating whether the text wraps to multiple lines.
    /// </summary>
    [Serializable]
    private class BrokenTextsTMP
    {
        [Tooltip("Reference to the UI TMP_Text component to monitor for line breaks.")]
        public TMP_Text itemLabel; // Reference to the TMP_Text component.

        [Tooltip("Indicates whether the text has broken into multiple lines.")]
        public bool brokenText; // Indicates if the text wraps to a second line.
    }

    #endregion

    #region === Enums ===

    /// <summary>
    /// Defines the direction in which the element expands when adjusting its size.
    /// </summary>
    public enum FitDirection { Left, Right } // Enum for fit direction.

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
    private List<BrokenTextsTMP> textList; // TMP_Text components and wrap info.

    [SerializeField, Tooltip("True if any Text component has wrapped to multiple lines.")]
    private bool textIsBroken; // True if any text wraps lines.

    [SerializeField, Tooltip("Final computed adjustment value based on wrapping and manual offsets.")]
    private float sizeAdjustment; // Calculated width adjustment.

    #endregion

    #region === Unity Methods ===

    /// <summary>
    /// Initializes component references and gathers child TMP_Text objects for line check.
    /// </summary>
    private void Start()
    {
        parentRect = GetComponent<RectTransform>(); // Fetch RectTransform of the current UI element.
        scrollRect = GetComponent<ScrollRect>(); // Get the ScrollRect of the current object.

        // Traverse up the hierarchy to find the parent Canvas RectTransform.
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
            Debug.LogError("AdjustSizeToDropdownTMP: No Canvas found in hierarchy.", this);
            return;
        }

        // Populate list with all TMP_Text components and initialize overflow mode.
        foreach (var text in GetComponentsInChildren<TMP_Text>())
        {
            text.overflowMode = TextOverflowModes.Overflow;
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
    /// Monitors and adjusts UI element size based on text content and canvas boundaries.
    /// </summary>
    private void Update()
    {
        if (textList == null || textList.Count == 0)
        {
            Debug.LogWarning("No Text objects found in the children.", this);
            return;
        }

        // Cache canvas and object width each frame.
        canvasWidth = canvasRectTransform.rect.width;
        objectWidth = parentRect.rect.width;

        // Determine if text wraps and update size accordingly.
        CheckLineBreaks();
        textIsBroken = textList.Exists(t => t.brokenText);
        sizeAdjustment += textIsBroken ? sizeMultiplier : 0f;

        float adjustment = sizeAdjustment + manualSizeAdjustment;

        // Apply offset based on the current fit direction.
        parentRect.offsetMin = new Vector2(fitDirection == FitDirection.Left ? -adjustment : 0f, parentRect.offsetMin.y);
        parentRect.offsetMax = new Vector2(fitDirection == FitDirection.Right ? adjustment : 0f, parentRect.offsetMax.y);

        // Convert local UI position to canvas space.
        Vector2 relativePos = canvasRectTransform.InverseTransformPoint(parentRect.position);

        // Reposition based on overflow beyond canvas edges.
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
    /// Checks whether any child text component wraps to a new line.
    /// </summary>
    private void CheckLineBreaks()
    {
        foreach (var item in textList)
        {
            item.brokenText = item.itemLabel.textInfo.lineCount > 1;
            if (item.brokenText) Debug.LogWarning("AdjustSizeToDropdownTMP: Text wrapped to multiple lines.", this);
        }
    }

    #endregion
}