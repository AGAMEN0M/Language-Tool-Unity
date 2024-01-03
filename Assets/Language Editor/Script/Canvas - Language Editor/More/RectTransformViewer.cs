using UnityEngine;
using UnityEngine.UI;

public class RectTransformViewer : MonoBehaviour
{
    [Header("Settings")]
    public OrganizeIDS organizeIDS; // Reference to some OrganizeIDS class.
    [SerializeField] private Text ID; // Text field for displaying an ID.
    [SerializeField] private Button applyButton; // Button for applying changes.
    [Header("Anchor Presets")]
    [SerializeField] private GameObject anchorPresetsMenu; // Menu for anchor presets.
    [SerializeField] private Image anchorPresets; // Image to display anchor presets.
    [SerializeField] private Sprite[] postsets; // Array of sprites for different presets.
    [SerializeField] private Text viewerX, viewerY; // Text for displaying anchor points.
    [Header("Position")]
    public Vector2 anchoredPosition; // Anchored position of the RectTransform.
    [SerializeField] private Text posX, posY; // Text for displaying position.
    [SerializeField] private InputField inputPosX, inputPosY, inputPosZ; // Input fields for position.
    [Header("Size")]
    public Vector2 sizeDelta; // Size of the RectTransform.
    [SerializeField] private Text width, height; // Text for displaying size.
    [SerializeField] private InputField inputWidth, inputHeight; // Input fields for size.
    [Header("Anchor")]
    public Vector2 min; // Minimum anchor position.
    [SerializeField] private InputField inputMinX, inputMinY; // Input fields for minimum anchor.
    public Vector2 max; // Maximum anchor position.
    [SerializeField] private InputField inputMaxX, inputMaxY; // Input fields for maximum anchor.
    [Header("Pivot")]
    public Vector2 pivot; // Pivot point of the RectTransform.
    [SerializeField] private InputField inputPivotX, inputPivotY; // Input fields for pivot.
    [Header("Rotation")]
    public Quaternion rotation; // Rotation of the RectTransform.
    [SerializeField] private InputField inputRotX, inputRotY, inputRotZ; // Input fields for rotation.
    [Header("Scale")]
    public Vector3 localScale; // Local scale of the RectTransform.
    [SerializeField] private InputField inputScaleX, inputScaleY, inputScaleZ; // Input fields for scale.
    [Space(20)]
    [Header("Texts")]
    public string middleText = "Middle";
    public string centerText = "Center";
    public string topText = "Top";
    public string leftText = "Left";
    public string rightText = "Right";
    public string posXText = "Pos X";
    public string posYText = "Pos Y";
    public string widthText = "Width";
    public string heightText = "Height";
    public string stretchText = "Stretch";
    public string bottomText = "Bottom";

    private bool menu; // Flag to track the menu status.
    private bool permission = true; // Flag to control permission for updating values.

    // Initialization.
    private void Start()
    {
        // Add listeners to input fields to trigger updates when their values change.
        inputPosX.onValueChanged.AddListener(delegate { UpdatePosition(); });
        inputPosY.onValueChanged.AddListener(delegate { UpdatePosition(); });
        inputWidth.onValueChanged.AddListener(delegate { UpdatePosition(); });
        inputHeight.onValueChanged.AddListener(delegate { UpdatePosition(); });
        inputMinX.onValueChanged.AddListener(delegate { UpdatePosition(); });
        inputMinY.onValueChanged.AddListener(delegate { UpdatePosition(); });
        inputMaxX.onValueChanged.AddListener(delegate { UpdatePosition(); });
        inputMaxY.onValueChanged.AddListener(delegate { UpdatePosition(); });
        inputPivotX.onValueChanged.AddListener(delegate { UpdatePosition(); });
        inputPivotY.onValueChanged.AddListener(delegate { UpdatePosition(); });
        inputRotX.onValueChanged.AddListener(delegate { UpdatePosition(); });
        inputRotY.onValueChanged.AddListener(delegate { UpdatePosition(); });
        inputRotZ.onValueChanged.AddListener(delegate { UpdatePosition(); });
        inputScaleX.onValueChanged.AddListener(delegate { UpdatePosition(); });
        inputScaleY.onValueChanged.AddListener(delegate { UpdatePosition(); });
        inputScaleZ.onValueChanged.AddListener(delegate { UpdatePosition(); });

        // Restrict input characters to digits and ',' for all input fields.
        inputPosX.onValidateInput += ValidateInput;
        inputPosY.onValidateInput += ValidateInput;
        inputWidth.onValidateInput += ValidateInput;
        inputHeight.onValidateInput += ValidateInput;
        inputMinX.onValidateInput += ValidateInput;
        inputMinY.onValidateInput += ValidateInput;
        inputMaxX.onValidateInput += ValidateInput;
        inputMaxY.onValidateInput += ValidateInput;
        inputPivotX.onValidateInput += ValidateInput;
        inputPivotY.onValidateInput += ValidateInput;
        inputRotX.onValidateInput += ValidateInput;
        inputRotY.onValidateInput += ValidateInput;
        inputRotZ.onValidateInput += ValidateInput;
        inputScaleX.onValidateInput += ValidateInput;
        inputScaleY.onValidateInput += ValidateInput;
        inputScaleZ.onValidateInput += ValidateInput;

        // Set default values for minimum and maximum anchor positions.
        min = new Vector2(0.5f, 0.5f);
        max = new Vector2(0.5f, 0.5f);

        UpdateUI(); // Update the UI to display default RectTransform properties.
    }

    // Called at fixed intervals.
    private void FixedUpdate()
    {
        SetAnchorLabels(); // Set labels based on anchor positions.

        // Update button interactivity and ID display.
        if (organizeIDS != null)
        {
            bool isAnyInteractable = false;

            // Array of input fields to check for interactability.
            InputField[] inputFields = new[]
            {
                inputPosX, inputPosY, inputWidth, inputHeight, inputMinX, inputMinY, inputMaxX, inputMaxY,
                inputPivotX, inputPivotY, inputRotX, inputRotY, inputRotZ, inputScaleX, inputScaleY, inputScaleZ
            };

            // Check if any input fields are interactable.
            foreach (InputField inputField in inputFields)
            {
                if (inputField.interactable)
                {
                    isAnyInteractable = true;
                    break;
                }
            }

            // Update button interactivity and ID text.
            applyButton.interactable = isAnyInteractable;
            ID.text = $"ID: {organizeIDS.ID}";
        }
        else
        {
            // Disable button and display unknown ID if organizeIDS is null.
            applyButton.interactable = false;
            ID.text = $"ID: ???";

            // Enable all input fields when organizeIDS is null.
            inputPosX.interactable = true;
            inputPosY.interactable = true;
            inputWidth.interactable = true;
            inputHeight.interactable = true;
            inputMinX.interactable = true;
            inputMinY.interactable = true;
            inputMaxX.interactable = true;
            inputMaxY.interactable = true;
            inputPivotX.interactable = true;
            inputPivotY.interactable = true;
            inputRotX.interactable = true;
            inputRotY.interactable = true;
            inputRotZ.interactable = true;
            inputScaleX.interactable = true;
            inputScaleY.interactable = true;
            inputScaleZ.interactable = true;
        }
    }

    // Update UI elements with current values.
    public void UpdateUI()
    {
        permission = false; // Disable permission to prevent value updates triggered by UI changes.

        // Update position fields with RectTransform's anchored position values.
        inputPosX.text = anchoredPosition.x.ToString();
        inputPosY.text = anchoredPosition.y.ToString();

        // Update size fields with RectTransform's size delta values.
        inputWidth.text = sizeDelta.x.ToString();
        inputHeight.text = sizeDelta.y.ToString();

        // Update anchor fields with RectTransform's minimum and maximum anchor positions.
        inputMinX.text = min.x.ToString();
        inputMinY.text = min.y.ToString();
        inputMaxX.text = max.x.ToString();
        inputMaxY.text = max.y.ToString();

        // Update pivot fields with RectTransform's pivot values.
        inputPivotX.text = pivot.x.ToString();
        inputPivotY.text = pivot.y.ToString();

        // Update rotation fields with RectTransform's rotation values.
        inputRotX.text = rotation.x.ToString();
        inputRotY.text = rotation.y.ToString();
        inputRotZ.text = rotation.z.ToString();

        // Update scale fields with RectTransform's local scale values.
        inputScaleX.text = localScale.x.ToString();
        inputScaleY.text = localScale.y.ToString();
        inputScaleZ.text = localScale.z.ToString();

        permission = true; // Enable permission to allow value updates triggered by user input.
    }

    // Get settings from OrganizeIDS class.
    public void GetSettings()
    {
        // Retrieve values from the OrganizeIDS class and update RectTransform properties if the values are valid floats.
        if (float.TryParse(organizeIDS.anchoredPositionX.text, out float posXValue)) { anchoredPosition.x = posXValue; }
        if (float.TryParse(organizeIDS.anchoredPositionY.text, out float posYValue)) { anchoredPosition.y = posYValue; }
        if (float.TryParse(organizeIDS.sizeDeltaX.text, out float widthValue)) { sizeDelta.x = widthValue; }
        if (float.TryParse(organizeIDS.sizeDeltaY.text, out float heightValue)) { sizeDelta.y = heightValue; }
        if (float.TryParse(organizeIDS.anchorMinX.text, out float minXValue)) { min.x = minXValue; }
        if (float.TryParse(organizeIDS.anchorMinY.text, out float minYValue)) { min.y = minYValue; }
        if (float.TryParse(organizeIDS.anchorMaxX.text, out float maxXValue)) { max.x = maxXValue; }
        if (float.TryParse(organizeIDS.anchorMaxY.text, out float maxYValue)) { max.y = maxYValue; }
        if (float.TryParse(organizeIDS.pivitX.text, out float pivotXValue)) { pivot.x = pivotXValue; }
        if (float.TryParse(organizeIDS.pivitY.text, out float pivotYValue)) { pivot.y = pivotYValue; }
        if (float.TryParse(organizeIDS.rotationX.text, out float rotXValue)) { rotation.x = rotXValue; }
        if (float.TryParse(organizeIDS.rotationY.text, out float rotYValue)) { rotation.y = rotYValue; }
        if (float.TryParse(organizeIDS.rotationZ.text, out float rotZValue)) { rotation.z = rotZValue; }
        if (float.TryParse(organizeIDS.localScaleX.text, out float scaleXValue)) { localScale.x = scaleXValue; }
        if (float.TryParse(organizeIDS.localScaleY.text, out float scaleYValue)) { localScale.y = scaleYValue; }
        if (float.TryParse(organizeIDS.localScaleZ.text, out float scaleZValue)) { localScale.z = scaleZValue; }

        // Update interactability of input fields based on their respective fields in OrganizeIDS.
        inputPosX.interactable = organizeIDS.anchoredPositionX.interactable;
        inputPosY.interactable = organizeIDS.anchoredPositionY.interactable;
        inputWidth.interactable = organizeIDS.sizeDeltaX.interactable;
        inputHeight.interactable = organizeIDS.sizeDeltaY.interactable;
        inputMinX.interactable = organizeIDS.anchorMinX.interactable;
        inputMinY.interactable = organizeIDS.anchorMinY.interactable;
        inputMaxX.interactable = organizeIDS.anchorMaxX.interactable;
        inputMaxY.interactable = organizeIDS.anchorMaxY.interactable;
        inputPivotX.interactable = organizeIDS.pivitX.interactable;
        inputPivotY.interactable = organizeIDS.pivitY.interactable;
        inputRotX.interactable = organizeIDS.rotationX.interactable;
        inputRotY.interactable = organizeIDS.rotationY.interactable;
        inputRotZ.interactable = organizeIDS.rotationZ.interactable;
        inputScaleX.interactable = organizeIDS.localScaleX.interactable;
        inputScaleY.interactable = organizeIDS.localScaleY.interactable;
        inputScaleZ.interactable = organizeIDS.localScaleZ.interactable;
    }

    // Set anchor labels based on anchor positions.
    private void SetAnchorLabels()
    {
        // Check different combinations of anchor positions.
        switch ((min.x, min.y, max.x, max.y))
        {
            case (0f, 1f, 0f, 1f):
                //top - left
                viewerY.text = topText;
                viewerX.text = leftText;
                DefaultLabels();
                anchorPresets.sprite = postsets[0];
                break;

            case (0.5f, 1f, 0.5f, 1f):
                //top - center
                viewerY.text = topText;
                viewerX.text = centerText;
                DefaultLabels();
                anchorPresets.sprite = postsets[1];
                break;

            case (1f, 1f, 1f, 1f):
                //top - right
                viewerY.text = topText;
                viewerX.text = rightText;
                DefaultLabels();
                anchorPresets.sprite = postsets[2];
                break;

            case (0f, 0.5f, 0f, 0.5f):
                //middle - left
                viewerY.text = middleText;
                viewerX.text = leftText;
                DefaultLabels();
                anchorPresets.sprite = postsets[3];
                break;

            case (0.5f, 0.5f, 0.5f, 0.5f):
                //middle - center
                viewerY.text = middleText;
                viewerX.text = centerText;
                DefaultLabels();
                anchorPresets.sprite = postsets[4];
                break;

            case (1f, 0.5f, 1f, 0.5f):
                //middle - right
                viewerY.text = middleText;
                viewerX.text = rightText;
                DefaultLabels();
                anchorPresets.sprite = postsets[5];
                break;

            case (0f, 0f, 0f, 0f):
                //bottom - left
                viewerY.text = bottomText;
                viewerX.text = leftText;
                DefaultLabels();
                anchorPresets.sprite = postsets[6];
                break;

            case (0.5f, 0f, 0.5f, 0f):
                //bottom - center
                viewerY.text = bottomText;
                viewerX.text = centerText;
                DefaultLabels();
                anchorPresets.sprite = postsets[7];
                break;

            case (1f, 0f, 1f, 0f):
                //bottom - right
                viewerY.text = bottomText;
                viewerX.text = rightText;
                DefaultLabels();
                anchorPresets.sprite = postsets[8];
                break;

            case (0f, 1f, 1f, 1f):
                //top - stretch
                viewerY.text = topText;
                viewerX.text = stretchText;
                posX.text = leftText;
                posY.text = posYText;
                width.text = rightText;
                height.text = bottomText;
                anchorPresets.sprite = postsets[9];
                break;

            case (0f, 0.5f, 1f, 0.5f):
                //middle - stretch
                viewerY.text = middleText;
                viewerX.text = stretchText;
                posX.text = leftText;
                posY.text = posYText;
                width.text = rightText;
                height.text = bottomText;
                anchorPresets.sprite = postsets[10];
                break;

            case (0f, 0f, 1f, 0f):
                //bottom - stretch
                viewerY.text = bottomText;
                viewerX.text = stretchText;
                posX.text = leftText;
                posY.text = posYText;
                width.text = rightText;
                height.text = bottomText;
                anchorPresets.sprite = postsets[11];
                break;

            case (0f, 0f, 0f, 1f):
                //stretch - left
                viewerY.text = stretchText;
                viewerX.text = leftText;
                posX.text = posXText;
                posY.text = topText;
                width.text = rightText;
                height.text = bottomText;
                anchorPresets.sprite = postsets[12];
                break;

            case (0.5f, 0f, 0.5f, 1f):
                //stretch - center
                viewerY.text = stretchText;
                viewerX.text = centerText;
                posX.text = posXText;
                posY.text = topText;
                width.text = rightText;
                height.text = bottomText;
                anchorPresets.sprite = postsets[13];
                break;

            case (1f, 0f, 1f, 1f):
                //stretch - right
                viewerY.text = stretchText;
                viewerX.text = rightText;
                posX.text = posXText;
                posY.text = topText;
                width.text = rightText;
                height.text = bottomText;
                anchorPresets.sprite = postsets[14];
                break;

            case (0f, 0f, 1f, 1f):
                //stretch - stretch
                viewerY.text = stretchText;
                viewerX.text = stretchText;
                posX.text = leftText;
                posY.text = topText;
                width.text = rightText;
                height.text = bottomText;
                anchorPresets.sprite = postsets[15];
                break;
            // Default case if anchor presets don't match any predefined cases.
            default:
                //Default Labels
                viewerY.text = "???";
                viewerX.text = "???";
                DefaultLabels();
                anchorPresets.sprite = postsets[16];
                break;
        }

        Debug.Log($"Anchor Presets: {viewerY.text} / {viewerX.text}");
    }

    // Set default labels for UI elements.
    private void DefaultLabels()
    {
        posX.text = posXText;
        posY.text = posYText;
        width.text = widthText;
        height.text = heightText;
    }

    // Toggle anchor presets menu.
    public void Switch()
    {
        menu = !menu;
        anchorPresetsMenu.SetActive(menu);
    }

    // Update RectTransform properties based on user input.
    private void UpdatePosition()
    {
        // Check if permission is granted to update values.
        if (permission)
        {
            // Update anchored position if input is a valid float.
            if (float.TryParse(inputPosX.text, out float posXValue)) { anchoredPosition.x = posXValue; }
            if (float.TryParse(inputPosY.text, out float posYValue)) { anchoredPosition.y = posYValue; }

            // Update size if input is a valid float.
            if (float.TryParse(inputWidth.text, out float widthValue)) { sizeDelta.x = widthValue; }
            if (float.TryParse(inputHeight.text, out float heightValue)) { sizeDelta.y = heightValue; }

            // Update anchor points if input is a valid float.
            if (float.TryParse(inputMinX.text, out float minXValue)) { min.x = minXValue; }
            if (float.TryParse(inputMinY.text, out float minYValue)) { min.y = minYValue; }
            if (float.TryParse(inputMaxX.text, out float maxXValue)) { max.x = maxXValue; }
            if (float.TryParse(inputMaxY.text, out float maxYValue)) { max.y = maxYValue; }

            // Update pivot if input is a valid float.
            if (float.TryParse(inputPivotX.text, out float pivotXValue)) { pivot.x = pivotXValue; }
            if (float.TryParse(inputPivotY.text, out float pivotYValue)) { pivot.y = pivotYValue; }

            // Update rotation if input is a valid float.
            if (float.TryParse(inputRotX.text, out float rotXValue)) { rotation.x = rotXValue; }
            if (float.TryParse(inputRotY.text, out float rotYValue)) { rotation.y = rotYValue; }
            if (float.TryParse(inputRotZ.text, out float rotZValue)) { rotation.z = rotZValue; }

            // Update scale if input is a valid float.
            if (float.TryParse(inputScaleX.text, out float scaleXValue)) { localScale.x = scaleXValue; }
            if (float.TryParse(inputScaleY.text, out float scaleYValue)) { localScale.y = scaleYValue; }
            if (float.TryParse(inputScaleZ.text, out float scaleZValue)) { localScale.z = scaleZValue; }
        }
    }

    // Validate input characters in input fields.
    private char ValidateInput(string text, int charIndex, char addedChar)
    {
        // Checks whether the added character is a digit, minus sign, or comma.
        if (char.IsDigit(addedChar) || addedChar == '-' || addedChar == ',')
        {
            return addedChar; // Allow the character to be added to the input field.
        }

        return '\0'; // Return null character to prevent the addition of invalid characters.
    }

    // Apply changes to OrganizeIDS and reset RectTransform properties.
    public void Apply()
    {
        // Parse input field values to update the respective fields in the OrganizeIDS class.
        if (float.TryParse(inputPosX.text, out float posXValue)) { organizeIDS.anchoredPositionX.text = posXValue.ToString(); }
        if (float.TryParse(inputPosY.text, out float posYValue)) { organizeIDS.anchoredPositionY.text = posYValue.ToString(); }
        if (float.TryParse(inputWidth.text, out float widthValue)) { organizeIDS.sizeDeltaX.text = widthValue.ToString(); }
        if (float.TryParse(inputHeight.text, out float heightValue)) { organizeIDS.sizeDeltaY.text = heightValue.ToString(); }
        if (float.TryParse(inputMinX.text, out float minXValue)) { organizeIDS.anchorMinX.text = minXValue.ToString(); }
        if (float.TryParse(inputMinY.text, out float minYValue)) { organizeIDS.anchorMinY.text = minYValue.ToString(); }
        if (float.TryParse(inputMaxX.text, out float maxXValue)) { organizeIDS.anchorMaxX.text = maxXValue.ToString(); }
        if (float.TryParse(inputMaxY.text, out float maxYValue)) { organizeIDS.anchorMaxY.text = maxYValue.ToString(); }
        if (float.TryParse(inputPivotX.text, out float pivotXValue)) { organizeIDS.pivitX.text = pivotXValue.ToString(); }
        if (float.TryParse(inputPivotY.text, out float pivotYValue)) { organizeIDS.pivitY.text = pivotYValue.ToString(); }
        if (float.TryParse(inputRotX.text, out float rotXValue)) { organizeIDS.rotationX.text = rotXValue.ToString(); }
        if (float.TryParse(inputRotY.text, out float rotYValue)) { organizeIDS.rotationY.text = rotYValue.ToString(); }
        if (float.TryParse(inputRotZ.text, out float rotZValue)) { organizeIDS.rotationZ.text = rotZValue.ToString(); }
        if (float.TryParse(inputScaleX.text, out float scaleXValue)) { organizeIDS.localScaleX.text = scaleXValue.ToString(); }
        if (float.TryParse(inputScaleY.text, out float scaleYValue)) { organizeIDS.localScaleY.text = scaleYValue.ToString(); }
        if (float.TryParse(inputScaleZ.text, out float scaleZValue)) { organizeIDS.localScaleZ.text = scaleZValue.ToString(); }

        // Reset all RectTransform properties and UI elements to their initial states.
        organizeIDS = null; // Clear the reference to the OrganizeIDS class.
        anchoredPosition = Vector2.zero; // Reset anchored position.
        sizeDelta = Vector2.zero; // Reset size delta.
        min = new Vector2(0.5f, 0.5f); // Reset minimum anchor.
        max = new Vector2(0.5f, 0.5f); // Reset maximum anchor.
        pivot = Vector2.zero; // Reset pivot.
        rotation = new Quaternion(0, 0, 0, 0); // Reset rotation.
        localScale = Vector3.zero; // Reset local scale.
        UpdateUI(); // Update the UI to reflect the reset values.
    }
}