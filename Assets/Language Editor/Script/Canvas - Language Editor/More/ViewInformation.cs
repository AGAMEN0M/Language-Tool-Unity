using UnityEngine;

public class ViewInformation : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private RectTransformViewer rectTransformViewer; // Reference to RectTransformViewer.

    private void Start()
    {
        // Setting default PlayerPrefs values.
        PlayerPrefs.SetInt("componentType", 0);
        PlayerPrefs.SetFloat("anchoredPositionX", 0f);
        PlayerPrefs.SetFloat("anchoredPositionY", 0f);
        PlayerPrefs.SetFloat("sizeDeltaX", 0f);
        PlayerPrefs.SetFloat("sizeDeltaY", 0f);
        PlayerPrefs.SetFloat("minX", 0.5f);
        PlayerPrefs.SetFloat("minY", 0.5f);
        PlayerPrefs.SetFloat("maxX", 0.5f);
        PlayerPrefs.SetFloat("maxY", 0.5f);
        PlayerPrefs.SetFloat("pivotX", 0f);
        PlayerPrefs.SetFloat("pivotY", 0f);
        PlayerPrefs.SetFloat("rotationX", 0f);
        PlayerPrefs.SetFloat("rotationY", 0f);
        PlayerPrefs.SetFloat("rotationZ", 0f);
        PlayerPrefs.SetFloat("rotationW", 0f);
        PlayerPrefs.SetFloat("localScaleX", 0f);
        PlayerPrefs.SetFloat("localScaleY", 0f);
        PlayerPrefs.SetFloat("localScaleZ", 0f);
        PlayerPrefs.SetString("text", "");
        PlayerPrefs.SetFloat("fontSize", 0f);
        PlayerPrefs.SetFloat("font", 0f);
        PlayerPrefs.SetInt("alignmentValue", 0);
        PlayerPrefs.SetInt("reverseValue", 0);
    }

    private void Update()
    {
        // Checking if organizeIDS is not null in the RectTransformViewer.
        if (rectTransformViewer.organizeIDS != null)
        {
            SaveOrganizeIDS(); // If organizeIDS is not null, save its settings.
        }
        else
        {
            SaveRectTransformViewer(); // If organizeIDS is null, save default RectTransformViewer settings.
        }
    }

    // Method to save settings from organizeIDS.
    private void SaveOrganizeIDS()
    {
        // Saving PlayerPrefs based on interactable properties of organizeIDS.
        // (retrieving values from rectTransformViewer.organizeIDS and saving to PlayerPrefs)
        PlayerPrefs.SetInt("componentType", rectTransformViewer.organizeIDS.spritesIndex);

        if (rectTransformViewer.organizeIDS.interactableAnchoredPosition)
        {
            PlayerPrefs.SetFloat("anchoredPositionX", rectTransformViewer.anchoredPosition.x);
            PlayerPrefs.SetFloat("anchoredPositionY", rectTransformViewer.anchoredPosition.y);
        }
        else
        {
            PlayerPrefs.SetFloat("anchoredPositionX", 0f);
            PlayerPrefs.SetFloat("anchoredPositionY", 0f);
        }

        if (rectTransformViewer.organizeIDS.interactableSizeDelta)
        {
            PlayerPrefs.SetFloat("sizeDeltaX", rectTransformViewer.sizeDelta.x);
            PlayerPrefs.SetFloat("sizeDeltaY", rectTransformViewer.sizeDelta.y);
        }
        else
        {
            PlayerPrefs.SetFloat("sizeDeltaX", 0f);
            PlayerPrefs.SetFloat("sizeDeltaY", 0f);
        }

        if (rectTransformViewer.organizeIDS.interactableAnchorMin)
        {
            PlayerPrefs.SetFloat("minX", rectTransformViewer.min.x);
            PlayerPrefs.SetFloat("minY", rectTransformViewer.min.y);
        }
        else
        {
            PlayerPrefs.SetFloat("minX", 0.5f);
            PlayerPrefs.SetFloat("minY", 0.5f);
        }

        if (rectTransformViewer.organizeIDS.interactableAnchorMax)
        {
            PlayerPrefs.SetFloat("maxX", rectTransformViewer.max.x);
            PlayerPrefs.SetFloat("maxY", rectTransformViewer.max.y);
        }
        else
        {
            PlayerPrefs.SetFloat("maxX", 0.5f);
            PlayerPrefs.SetFloat("maxY", 0.5f);
        }

        if (rectTransformViewer.organizeIDS.interactablePivit)
        {
            PlayerPrefs.SetFloat("pivotX", rectTransformViewer.pivot.x);
            PlayerPrefs.SetFloat("pivotY", rectTransformViewer.pivot.y);
        }
        else
        {
            PlayerPrefs.SetFloat("pivotX", 0f);
            PlayerPrefs.SetFloat("pivotY", 0f);
        }

        if (rectTransformViewer.organizeIDS.interactableRotation)
        {
            PlayerPrefs.SetFloat("rotationX", rectTransformViewer.rotation.x);
            PlayerPrefs.SetFloat("rotationY", rectTransformViewer.rotation.y);
            PlayerPrefs.SetFloat("rotationZ", rectTransformViewer.rotation.z);
            PlayerPrefs.SetFloat("rotationW", rectTransformViewer.rotation.w);
        }
        else
        {
            PlayerPrefs.SetFloat("rotationX", 0f);
            PlayerPrefs.SetFloat("rotationY", 0f);
            PlayerPrefs.SetFloat("rotationZ", 0f);
            PlayerPrefs.SetFloat("rotationW", 0f);
        }

        if (rectTransformViewer.organizeIDS.interactableLocalScale)
        {
            PlayerPrefs.SetFloat("localScaleX", rectTransformViewer.localScale.x);
            PlayerPrefs.SetFloat("localScaleY", rectTransformViewer.localScale.y);
            PlayerPrefs.SetFloat("localScaleZ", rectTransformViewer.localScale.z);
        }
        else
        {
            PlayerPrefs.SetFloat("localScaleX", 0f);
            PlayerPrefs.SetFloat("localScaleY", 0f);
            PlayerPrefs.SetFloat("localScaleZ", 0f);
        }

        if (rectTransformViewer.organizeIDS.interactableText)
        { PlayerPrefs.SetString("text", rectTransformViewer.organizeIDS.text.text); }
        else { PlayerPrefs.SetString("text", ""); }

        if (rectTransformViewer.organizeIDS.interactableFontSize)
        { PlayerPrefs.SetFloat("fontSize", float.TryParse(rectTransformViewer.organizeIDS.fontSize.text, out float fs) ? fs : 0f); }
        else { PlayerPrefs.SetFloat("fontSize", 0f); }

        if (rectTransformViewer.organizeIDS.interactableFont)
        { PlayerPrefs.SetFloat("font", float.TryParse(rectTransformViewer.organizeIDS.font.text, out float f) ? f : 0f); }
        else { PlayerPrefs.SetFloat("font", 0f); }

        if (rectTransformViewer.organizeIDS.interactableAlignment)
        { PlayerPrefs.SetInt("alignmentValue", rectTransformViewer.organizeIDS.alignmentValue); }
        else { PlayerPrefs.SetInt("alignmentValue", 0); }

        if (rectTransformViewer.organizeIDS.interactableReverse)
        { PlayerPrefs.SetInt("reverseValue", rectTransformViewer.organizeIDS.reverseValue); }
        else { PlayerPrefs.SetInt("reverseValue", 0); }
    }

    // Method to save default RectTransformViewer settings.
    private void SaveRectTransformViewer()
    {
        // Saving default PlayerPrefs values directly from the RectTransformViewer.
        PlayerPrefs.SetInt("componentType", 0);
        PlayerPrefs.SetFloat("anchoredPositionX", rectTransformViewer.anchoredPosition.x);
        PlayerPrefs.SetFloat("anchoredPositionY", rectTransformViewer.anchoredPosition.y);
        PlayerPrefs.SetFloat("sizeDeltaX", rectTransformViewer.sizeDelta.x);
        PlayerPrefs.SetFloat("sizeDeltaY", rectTransformViewer.sizeDelta.y);
        PlayerPrefs.SetFloat("minX", rectTransformViewer.min.x);
        PlayerPrefs.SetFloat("minY", rectTransformViewer.min.y);
        PlayerPrefs.SetFloat("maxX", rectTransformViewer.max.x);
        PlayerPrefs.SetFloat("maxY", rectTransformViewer.max.y);
        PlayerPrefs.SetFloat("pivotX", rectTransformViewer.pivot.x);
        PlayerPrefs.SetFloat("pivotY", rectTransformViewer.pivot.y);
        PlayerPrefs.SetFloat("rotationX", rectTransformViewer.rotation.x);
        PlayerPrefs.SetFloat("rotationY", rectTransformViewer.rotation.y);
        PlayerPrefs.SetFloat("rotationZ", rectTransformViewer.rotation.z);
        PlayerPrefs.SetFloat("rotationW", rectTransformViewer.rotation.w);
        PlayerPrefs.SetFloat("localScaleX", rectTransformViewer.localScale.x);
        PlayerPrefs.SetFloat("localScaleY", rectTransformViewer.localScale.y);
        PlayerPrefs.SetFloat("localScaleZ", rectTransformViewer.localScale.z);
        PlayerPrefs.SetString("text", "");
        PlayerPrefs.SetFloat("fontSize", 0f);
        PlayerPrefs.SetFloat("font", 0f);
        PlayerPrefs.SetInt("alignmentValue", 0);
        PlayerPrefs.SetInt("reverseValue", 0);
    }
}