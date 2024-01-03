using System.Collections.Generic;
using UnityEngine;

public class ExpandUIHeight : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private RectTransform Contemner; // The UI object that will be expanded.
    [Space(10)]
    [SerializeField] private float MinimumHeight = 100f; // The minimum height of the object.
    [SerializeField] private List<RectTransform> ChildObjects; // List of child objects of the container.
    [Space(10)]
    [Header("Height of Objects")]
    [SerializeField] private float LastContemnerHeight; // Previously recorded height of the Contemner object.

    private void Update()
    {
        float totalHeight = 0f;

        // Calculates the total height of child objects.
        foreach (RectTransform childElement in ChildObjects)
        {
            totalHeight += childElement.sizeDelta.y;
        }

        // Checks whether the total height of child objects has changed.
        if (totalHeight != LastContemnerHeight)
        {
            LastContemnerHeight = totalHeight;

            // Calculates the new height based on the total height and minimum height.
            float newHeight = Mathf.Max(totalHeight, MinimumHeight);

            // Updates the size of the Contemner object's RectTransform with the new height.
            Vector2 newSize = Contemner.sizeDelta;
            newSize.y = newHeight;
            Contemner.sizeDelta = newSize;
        }
    }
}