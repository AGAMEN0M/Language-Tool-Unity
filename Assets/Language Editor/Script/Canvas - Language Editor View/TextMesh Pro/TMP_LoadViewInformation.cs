using System;
using UnityEngine;
using TMPro;

public class TMP_LoadViewInformation : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private TMP_Text targetText; // Reference to the Text component.
    [SerializeField] private RectTransform rectTransform; // Reference to the RectTransform component.
    [SerializeField] private TMP_LoadViewInformation loadViewInformation; // Reference to the LoadViewInformation script.
    [Space(10)]
    [Header("Component Settings")]
    [SerializeField] private int componentType; // Type of the UI component.
    [SerializeField] private Vector2 anchoredPosition; // Anchored position of the UI component.
    [SerializeField] private Vector2 sizeDelta; // Size of the UI component.
    [SerializeField] private Vector2 min; // Minimum anchor position.
    [SerializeField] private Vector2 max; // Maximum anchor position.
    [SerializeField] private Vector2 pivot; // Pivot point of the UI component.
    [SerializeField] private Quaternion rotation; // Rotation of the UI component.
    [SerializeField] private Vector3 localScale; // Scale of the UI component.
    [SerializeField] private string text; // Text content of the UI component.
    [SerializeField] private float fontSize; // Font size of the UI component.
    [SerializeField] private float font; // Font type of the UI component.
    [SerializeField] private TMP_LanguageFontListData fontListObject; // Reference to a list of fonts.
    [SerializeField] private int alignmentValue; // Value indicating text alignment.
    [SerializeField] private int reverseValue; // Value indicating text reverse status.
    [SerializeField] private bool reverse = false; // Flag for text reversal.
    [Space(10)]
    [Header("Old Component Settings")]
    [SerializeField] private Vector2 oldAnchoredPosition; // Stored anchored position.
    [SerializeField] private Vector2 oldSizeDelta; // Stored size.
    [SerializeField] private Vector2 oldMin; // Stored minimum anchor position.
    [SerializeField] private Vector2 oldMax; // Stored maximum anchor position.
    [SerializeField] private Vector2 oldPivot; // Stored pivot point.
    [SerializeField] private Quaternion oldRotation; // Stored rotation.
    [SerializeField] private Vector3 oldLocalScale; // Stored scale.
    [SerializeField] private string oldText; // Stored old text content.
    [SerializeField] private float oldFontSize; // Stored old font size.
    [SerializeField] private int oldAlignmentValue; // Stored old alignment value.

    // Method called when the script or GameObject is disabled.
    private void OnDisable()
    {
        // Revert UI element properties to their old values when the object was last enabled.
        rectTransform.anchoredPosition = oldAnchoredPosition; // Set anchored position back to its previous value.
        rectTransform.sizeDelta = oldSizeDelta; // Set size delta (size) back to its previous value.

        // Set anchor's minimum and maximum values back to their previous values.
        rectTransform.anchorMin = oldMin;
        rectTransform.anchorMax = oldMax;

        rectTransform.pivot = oldPivot; // Set the pivot point back to its previous value.
        rectTransform.rotation = oldRotation; // Set rotation back to its previous value.
        rectTransform.localScale = oldLocalScale; // Set local scale back to its previous value.

        // If the targetText variable is not null, restore its text, font size, font, and alignment.
        if (targetText != null)
        {
            // Check if text reversal was previously applied.
            if (reverse == true)
            {
                // Reverse the characters in the oldText and set it as the current text.
                char[] chars = oldText.ToCharArray();
                Array.Reverse(chars);
                targetText.text = new string(chars);
            }
            else { targetText.text = oldText; } // Set the targetText's text to its previous value.

            targetText.fontSize = (int)oldFontSize; // Set the font size to its previous value.
            targetText.font = fontListObject.TMP_fontList[0]; // Set the font to the previously selected font from the font list object.
            OldSetTextAlignment(); // Revert text alignment to its previous state.
            reverse = false; // Reset the text reversal flag to false.
        }
    }

    // Method to collect information about the current state of UI elements.
    [ContextMenu("Collect Information")]
    public void CollectInformation()
    {
        // Store the current state of UI element properties into the 'old' variables.

        // Store the current anchored position.
        oldAnchoredPosition.x = rectTransform.anchoredPosition.x;
        oldAnchoredPosition.y = rectTransform.anchoredPosition.y;

        // Store the current size delta (size).
        oldSizeDelta.x = rectTransform.sizeDelta.x;
        oldSizeDelta.y = rectTransform.sizeDelta.y;

        // Store the current anchor's minimum and maximum values.
        oldMin.x = rectTransform.anchorMin.x;
        oldMin.y = rectTransform.anchorMin.y;
        oldMax.x = rectTransform.anchorMax.x;
        oldMax.y = rectTransform.anchorMax.y;

        // Store the current pivot point.
        oldPivot.x = rectTransform.pivot.x;
        oldPivot.y = rectTransform.pivot.y;

        // Store the current rotation.
        oldRotation.x = rectTransform.rotation.x;
        oldRotation.y = rectTransform.rotation.y;
        oldRotation.z = rectTransform.rotation.z;
        oldRotation.w = rectTransform.rotation.w;

        // Store the current local scale.
        oldLocalScale.x = rectTransform.localScale.x;
        oldLocalScale.y = rectTransform.localScale.y;
        oldLocalScale.z = rectTransform.localScale.z;

        // If the targetText variable is not null, store its current text, font size, and alignment.
        if (targetText != null)
        {
            oldText = targetText.text; // Store the current text content.
            oldFontSize = targetText.fontSize; // Store the current font size.

            // Determine and store the current text alignment value.
            if (targetText.alignment == TextAlignmentOptions.Left || targetText.alignment == TextAlignmentOptions.TopLeft || targetText.alignment == TextAlignmentOptions.BottomLeft)
            {
                oldAlignmentValue = 1;
            }
            else if (targetText.alignment == TextAlignmentOptions.Center || targetText.alignment == TextAlignmentOptions.Top || targetText.alignment == TextAlignmentOptions.Bottom)
            {
                oldAlignmentValue = 2;
            }
            else if (targetText.alignment == TextAlignmentOptions.Right || targetText.alignment == TextAlignmentOptions.TopRight || targetText.alignment == TextAlignmentOptions.BottomRight)
            {
                oldAlignmentValue = 3;
            }
        }
    }

    // Method to load variables from file (PlayerPrefs) to set UI component properties.
    public void LoadVariablesFromFile()
    {
        if (loadViewInformation.enabled == false) { return; } // Check if the LoadViewInformation script is disabled, return if disabled.

        // Load variables from Player Preferences and set UI component properties accordingly.                
        componentType = PlayerPrefs.GetInt("componentType"); // Load component type.

        // Load anchored position.
        anchoredPosition.x = PlayerPrefs.GetFloat("anchoredPositionX");
        anchoredPosition.y = PlayerPrefs.GetFloat("anchoredPositionY");

        // Load size delta (size).
        sizeDelta.x = PlayerPrefs.GetFloat("sizeDeltaX");
        sizeDelta.y = PlayerPrefs.GetFloat("sizeDeltaY");

        // Load anchor's minimum and maximum values.
        min.x = PlayerPrefs.GetFloat("minX");
        min.y = PlayerPrefs.GetFloat("minY");
        max.x = PlayerPrefs.GetFloat("maxX");
        max.y = PlayerPrefs.GetFloat("maxY");

        // Load pivot point.
        pivot.x = PlayerPrefs.GetFloat("pivotX");
        pivot.y = PlayerPrefs.GetFloat("pivotY");

        // Load rotation.
        rotation.x = PlayerPrefs.GetFloat("rotationX");
        rotation.y = PlayerPrefs.GetFloat("rotationY");
        rotation.z = PlayerPrefs.GetFloat("rotationZ");
        rotation.w = PlayerPrefs.GetFloat("rotationW");

        // Load local scale.
        localScale.x = PlayerPrefs.GetFloat("localScaleX");
        localScale.y = PlayerPrefs.GetFloat("localScaleY");
        localScale.z = PlayerPrefs.GetFloat("localScaleZ");

        // If the targetText variable is not null, load text-related variables from PlayerPrefs.
        if (targetText != null)
        {
            text = PlayerPrefs.GetString("text"); // Load text content.
            fontSize = PlayerPrefs.GetFloat("fontSize"); // Load font size.
            font = PlayerPrefs.GetFloat("font"); // Load font type.
            alignmentValue = PlayerPrefs.GetInt("alignmentValue"); // Load alignment value.
            reverseValue = PlayerPrefs.GetInt("reverseValue"); // Load reverse value.
        }

        Size(); // Perform further actions based on loaded variables.
    }

    // Method to perform actions based on componentType and loaded variables.
    private void Size()
    {
        // Check the component type and execute corresponding actions.

        // Actions for componentType 7 or 0.
        if (componentType == 7 || componentType == 0)
        {
            SetRotation();
            SetLocalScale();
            SetAnchoredPosition();
            SetAnchor();
            SetSizeDelta();
            SetPivot();
        }

        // Actions for componentType 1.
        if (componentType == 1)
        {
            SetAnchoredPosition();
            SetSizeDelta();
            SetAnchor();
            SetPivot();
            SetRotation();
            SetLocalScale();

            if (targetText != null)
            {
                SetText();
                SetFontSize();
                SetFont();
                SetAlignment();
                SetReverse();
            }
        }

        // Actions for componentType 3.
        if (componentType == 3)
        {
            SetAnchoredPosition();
            SetSizeDelta();
            SetAnchor();
            SetPivot();
            SetRotation();
            SetLocalScale();

            if (targetText != null)
            {
                SetFontSize();
                SetFont();
                SetAlignment();
            }
        }

        // Actions for componentType 6.
        if (componentType == 6)
        {
            SetAnchoredPosition();
            SetSizeDelta();
            SetAnchor();
            SetPivot();
            SetRotation();
            SetLocalScale();

            if (targetText != null)
            {
                SetText();
                SetFont();
                SetAlignment();
                SetReverse();
            }
        }

        if (targetText != null)
        {
            // Actions for componentType 2
            if (componentType == 2)
            {
                SetText();
                SetFontSize();
                SetFont();
                SetReverse();
            }

            // Actions for componentType 4 or 5.
            if (componentType == 4 || componentType == 5)
            {
                SetText();
            }
        }
    }

    // Method to set the anchored position of the RectTransform.
    private void SetAnchoredPosition()
    {
        // Check if the new anchored position is not (0,0), if so, set it; otherwise, revert to the old position.
        if (anchoredPosition.x != 0 || anchoredPosition.y != 0) { rectTransform.anchoredPosition = anchoredPosition; } // Set the anchored position to the new values.
        else { rectTransform.anchoredPosition = oldAnchoredPosition; } // If new anchored position is (0,0), revert to the old position.
    }

    // Method to set the size delta (size) of the RectTransform.
    private void SetSizeDelta()
    {
        // Check if the new size delta is not (0,0), if so, set it; otherwise, revert to the old size.
        if (sizeDelta.x != 0 || sizeDelta.y != 0) { rectTransform.sizeDelta = sizeDelta; } // Set the size delta to the new values.
        else { rectTransform.sizeDelta = oldSizeDelta; } // If new size delta is (0,0), revert to the old size.
    }

    // Method to set the anchor values of the RectTransform.
    private void SetAnchor()
    {
        // Check if new anchor values are provided (not all values are 0) or if it's a specific component type.
        if (min.x != 0 || min.y != 0 || max.x != 0 || max.y != 0)
        {
            // Set the anchor values to the new provided values.
            rectTransform.anchorMin = min;
            rectTransform.anchorMax = max;
        }
        else if (componentType == 2 || componentType == 4 || componentType == 5)
        {
            // If new anchor values aren't provided but it's a specific component type, revert to the old values.
            rectTransform.anchorMin = oldMin;
            rectTransform.anchorMax = oldMax;
        }
        else
        {
            // If neither new values are provided nor a specific component type, set the anchor values to the old values.
            rectTransform.anchorMin = min;
            rectTransform.anchorMax = max;
        }
    }

    // Method to set the pivot point of the RectTransform.
    private void SetPivot()
    {
        // Check if new pivot values are provided (not both 0 for x and y).
        if (pivot.x != 0 || pivot.y != 0) { rectTransform.pivot = pivot; } // Set the pivot point to the new provided values.
        else { rectTransform.pivot = oldPivot; } // If new pivot values aren't provided, revert to the old pivot point.
    }

    // Method to set the rotation of the RectTransform.
    private void SetRotation()
    {
        // Check if new rotation values are provided (not all values are 0 for x, y, z, w).
        if (rotation.x != 0 || rotation.y != 0 || rotation.z != 0 || rotation.w != 0) { rectTransform.rotation = rotation; } // Set the rotation to the new provided values.
        else { rectTransform.rotation = oldRotation; } // If new rotation values aren't provided, revert to the old rotation.
    }

    // Method to set the local scale of the RectTransform.
    private void SetLocalScale()
    {
        // Check if new local scale values are provided (not all are 0 for x, y, z).
        if (localScale.x != 0 || localScale.y != 0 || localScale.z != 0) { rectTransform.localScale = localScale; } // Set the local scale to the new provided values.
        else { rectTransform.localScale = oldLocalScale; } // If new local scale values aren't provided, revert to the old local scale.
    }

    // Method to set the text content of the targetText.
    private void SetText()
    {
        // Check if the 'text' variable is not an empty string.
        if (text != "")
        {
            // If 'text' is not empty, check if reverse is enabled.
            if (reverse == true)
            {
                // Reverse the characters of 'text' and assign it to targetText.text.
                char[] chars = text.ToCharArray();
                Array.Reverse(chars);
                targetText.text = new string(chars);
            }
            else { targetText.text = text; } // Assign 'text' directly to targetText.text.
        }
        else
        {
            // If 'text' is an empty string, check if reverse is enabled for oldText.
            if (reverse == true)
            {
                // Reverse the characters of 'oldText' and assign it to targetText.text.
                char[] chars = oldText.ToCharArray();
                Array.Reverse(chars);
                targetText.text = new string(chars);
            }
            else { targetText.text = oldText; } // Assign 'oldText' directly to targetText.text.
        }
    }

    // Method to set the font size of the targetText.
    private void SetFontSize()
    {
        // Check if the fontSize variable is not 0.
        if (fontSize != 0) { targetText.fontSize = (int)fontSize; } // Set the font size to the new provided value.
        else { targetText.fontSize = (int)oldFontSize; } // If new font size value is 0, revert to the old font size.
    }

    // Method to set the font of the targetText.
    private void SetFont()
    {
        if (font != 0)
        {
            if (fontListObject != null && font > 0 && font <= fontListObject.TMP_fontList.Count) // Check if fontListObject and valid font index are available.
            { targetText.font = fontListObject.TMP_fontList[(int)font - 1]; } // Set the font to the new provided font from the font list.
        }
        else { targetText.font = fontListObject.TMP_fontList[0]; } // If new font value is 0 or invalid, revert to the default font from the font list.
    }

    // Method to set the text alignment of the targetText.
    private void SetAlignment()
    {
        if (alignmentValue != 0) { SetTextAlignment(); } // If a new alignment value is provided, call the SetTextAlignment method to handle the alignment.
        else { OldSetTextAlignment(); } // If the alignment value is 0 or not provided, revert to the old alignment.
    }

    // Method to set the text alignment based on the alignmentValue.
    private void SetTextAlignment()
    {
        // Check if alignmentValue is within valid range (1 to 3).
        if (alignmentValue >= 1 && alignmentValue <= 3)
        {
            string verticalAlignment = TMP_TextAlignmentConverter.GetVerticalAlignmentFromTextAlignment(targetText.alignment); // Get the vertical alignment based on the current text alignment.
            TextAlignmentOptions newAlignment = TextAlignmentOptions.TopLeft; // Default the new alignment to UpperLeft.

            // Adjust the alignment based on verticalAlignment and alignmentValue.
            if (alignmentValue == 1)
            {
                newAlignment = (verticalAlignment == "Upper") ? TextAlignmentOptions.TopLeft : (verticalAlignment == "Middle") ? TextAlignmentOptions.Left : TextAlignmentOptions.BottomLeft;
            }
            else if (alignmentValue == 2)
            {
                newAlignment = (verticalAlignment == "Upper") ? TextAlignmentOptions.Top : (verticalAlignment == "Middle") ? TextAlignmentOptions.Center : TextAlignmentOptions.Bottom;
            }
            else if (alignmentValue == 3)
            {
                newAlignment = (verticalAlignment == "Upper") ? TextAlignmentOptions.TopRight : (verticalAlignment == "Middle") ? TextAlignmentOptions.Right : TextAlignmentOptions.BottomRight;
            }

            targetText.alignment = newAlignment; // Set the targetText's alignment to the newly calculated alignment.
        }
    }

    // Method to set the text alignment based on the oldAlignmentValue.
    private void OldSetTextAlignment()
    {
        // Check if oldAlignmentValue is within valid range (1 to 3).
        if (oldAlignmentValue >= 1 && oldAlignmentValue <= 3)
        {
            string verticalAlignment = TMP_TextAlignmentConverter.GetVerticalAlignmentFromTextAlignment(targetText.alignment); // Get the vertical alignment based on the current text alignment.
            TextAlignmentOptions newAlignment = TextAlignmentOptions.TopLeft; // Default the new alignment to UpperLeft.

            // Adjust the alignment based on verticalAlignment and oldAlignmentValue.
            if (oldAlignmentValue == 1)
            {
                newAlignment = (verticalAlignment == "Upper") ? TextAlignmentOptions.TopLeft : (verticalAlignment == "Middle") ? TextAlignmentOptions.Left : TextAlignmentOptions.BottomLeft;
            }
            else if (oldAlignmentValue == 2)
            {
                newAlignment = (verticalAlignment == "Upper") ? TextAlignmentOptions.Top : (verticalAlignment == "Middle") ? TextAlignmentOptions.Center : TextAlignmentOptions.Bottom;
            }
            else if (oldAlignmentValue == 3)
            {
                newAlignment = (verticalAlignment == "Upper") ? TextAlignmentOptions.TopRight : (verticalAlignment == "Middle") ? TextAlignmentOptions.Right : TextAlignmentOptions.BottomRight;
            }

            targetText.alignment = newAlignment; // Set the targetText's alignment to the newly calculated alignment.
        }
    }

    // Method to handle the reverse flag for text.
    private void SetReverse()
    {
        if (reverseValue != 0)
        {
            // If a reverseValue is provided, set the reverse flag accordingly.
            if (reverseValue == 1) { reverse = false; } // Set reverse to false.
            if (reverseValue == 2) { reverse = true; } // Set reverse to true.
        }
        else { reverse = false; } // If reverseValue is 0 or not provided, revert to default (false).
    }

    // Method to toggle the value of loadViewInformation.enabled.
    public void ToggleValueChanged(bool change) { loadViewInformation.enabled = change; }
}