using UnityEngine;

public class AnchorPresetsMenu : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private RectTransformViewer transformViewer; // Reference to RectTransformViewer.

    // Methods for different anchor presets.

    // Set anchor to top-left corner.
    public void _1_Top_Left()
    {
        transformViewer.min = new Vector2(0f, 1f);
        transformViewer.max = new Vector2(0f, 1f);
        transformViewer.UpdateUI(); // Update the UI elements.
    }

    // Set anchor to top-center.
    public void _2_Top_Center()
    {
        transformViewer.min = new Vector2(0.5f, 1f);
        transformViewer.max = new Vector2(0.5f, 1f);
        transformViewer.UpdateUI(); // Update the UI elements.
    }

    // Set anchor to top-right corner.
    public void _3_Top_Right()
    {
        transformViewer.min = new Vector2(1f, 1f);
        transformViewer.max = new Vector2(1f, 1f);
        transformViewer.UpdateUI(); // Update the UI elements.
    }

    // Set anchor to middle-left.
    public void _4_Middle_Left()
    {
        transformViewer.min = new Vector2(0f, 0.5f);
        transformViewer.max = new Vector2(0f, 0.5f);
        transformViewer.UpdateUI(); // Update the UI elements.
    }

    // Set anchor to middle-center.
    public void _5_Middle_Center()
    {
        transformViewer.min = new Vector2(0.5f, 0.5f);
        transformViewer.max = new Vector2(0.5f, 0.5f);
        transformViewer.UpdateUI(); // Update the UI elements.
    }

    // Set anchor to middle-right.
    public void _6_Middle_Right()
    {
        transformViewer.min = new Vector2(1f, 0.5f);
        transformViewer.max = new Vector2(1f, 0.5f);
        transformViewer.UpdateUI(); // Update the UI elements.
    }

    // Set anchor to bottom-left corner.
    public void _7_Bottom_Left()
    {
        transformViewer.min = new Vector2(0f, 0f);
        transformViewer.max = new Vector2(0f, 0f);
        transformViewer.UpdateUI(); // Update the UI elements.
    }

    // Set anchor to bottom-center.
    public void _8_Bottom_Center()
    {
        transformViewer.min = new Vector2(0.5f, 0f);
        transformViewer.max = new Vector2(0.5f, 0f);
        transformViewer.UpdateUI(); // Update the UI elements.
    }

    // Set anchor to bottom-right corner.
    public void _9_Bottom_Right()
    {
        transformViewer.min = new Vector2(1f, 0f);
        transformViewer.max = new Vector2(1f, 0f);
        transformViewer.UpdateUI(); // Update the UI elements.
    }

    // Set anchor to top-stretch (stretch horizontally, anchored at top).
    public void _10_Top_Stretch()
    {
        transformViewer.min = new Vector2(0f, 1f);
        transformViewer.max = new Vector2(1f, 1f);
        transformViewer.UpdateUI(); // Update the UI elements.
    }

    // Set anchor to middle-stretch (stretch horizontally, anchored at middle).
    public void _11_Middle_Stretch()
    {
        transformViewer.min = new Vector2(0f, 0.5f);
        transformViewer.max = new Vector2(1f, 0.5f);
        transformViewer.UpdateUI(); // Update the UI elements.
    }

    // Set anchor to bottom-stretch (stretch horizontally, anchored at bottom).
    public void _12_Bottom_Stretch()
    {
        transformViewer.min = new Vector2(0f, 0f);
        transformViewer.max = new Vector2(1f, 0f);
        transformViewer.UpdateUI(); // Update the UI elements.
    }

    // Set anchor to stretch-left (stretch vertically, anchored at left).
    public void _13_Stretch_Left()
    {
        transformViewer.min = new Vector2(0f, 0f);
        transformViewer.max = new Vector2(0f, 1f);
        transformViewer.UpdateUI(); // Update the UI elements.
    }

    // Set anchor to stretch-center (stretch vertically, anchored at center).
    public void _14_Stretch_Center()
    {
        transformViewer.min = new Vector2(0.5f, 0f);
        transformViewer.max = new Vector2(0.5f, 1f);
        transformViewer.UpdateUI(); // Update the UI elements.
    }

    // Set anchor to stretch-right (stretch vertically, anchored at right).
    public void _15_Stretch_Right()
    {
        transformViewer.min = new Vector2(1f, 0f);
        transformViewer.max = new Vector2(1f, 1f);
        transformViewer.UpdateUI(); // Update the UI elements.
    }

    // Set anchor to stretch horizontally and vertically.
    public void _16_Stretch_Stretch()
    {
        transformViewer.min = new Vector2(0f, 0f);
        transformViewer.max = new Vector2(1f, 1f);
        transformViewer.UpdateUI(); // Update the UI elements.
    }
}