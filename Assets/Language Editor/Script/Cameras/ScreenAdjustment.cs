using UnityEngine;

public class ScreenAdjustment : MonoBehaviour
{
    [Header("Size of my pc")]
    [SerializeField] private float WidthOfMyPc = 1920; // The width of my PC screen.
    [SerializeField] private float HeightOfMyPc = 1080f; // The height of my PC screen.
    [Space(10)]
    [Header("Desired Size")]
    [SerializeField] private float WidthDesired = 1600; // The desired width of the game screen.
    [SerializeField] private float HeightDesired = 900; // The desired height of the game screen.
    [Space(10)]
    [Header("Current PC size")]
    [SerializeField] private float WidthCurrent; // The current width of the game screen.
    [SerializeField] private float HeightCurrent; // The current height of the game screen.
    [Space(10)]
    [Header("Difference")]
    [SerializeField] private float WidthDifference; // The difference between the desired width and the current width.
    [SerializeField] private float HeightDifference; // The difference between the desired height and the current height.
    [Space(10)]
    [Header("Correct Size to Place")]
    [SerializeField] private float WidthPlace; // The correct width to place the game screen.
    [SerializeField] private float HeightPlace; // The correct height to place the game screen.
    [Space(10)]
    [Header("If the Size is Bigger or Smaller than my pc")]
    [SerializeField] private bool IsBigger; // A flag to check whether the game screen is bigger or smaller than my PC screen.

    void Start()
    {
        // Get the current width of the PC screen.
        WidthCurrent = Screen.currentResolution.width;

        // Get the current height of the PC screen.
        HeightCurrent = Screen.currentResolution.height;

        // Calculate the difference between the desired width and the current width.
        WidthDifference = WidthOfMyPc - WidthCurrent;

        // Calculate the difference between the desired height and the current height.
        HeightDifference = HeightOfMyPc - HeightCurrent;

        if (WidthCurrent > WidthOfMyPc || HeightCurrent > HeightOfMyPc)
        {
            // If the game screen is bigger than my PC screen, set the flag to true.
            IsBigger = true;
        }
        else
        {
            // If the game screen is smaller than my PC screen, set the flag to false.
            IsBigger = false;
        }

        if (IsBigger)
        {
            // Calculate the correct width to place the game screen if it's bigger than my PC screen.
            WidthPlace = WidthDesired - WidthDifference;

            // Calculate the correct height to place the game screen if it's bigger than my PC screen.
            HeightPlace = HeightDesired - HeightDifference;
        }
        else
        {
            // Calculate the correct width to place the game screen if it's smaller than my PC screen.
            WidthPlace = WidthDesired - WidthDifference;
            // Calculate the correct height to place the game screen if it's smaller than my PC screen.
            HeightPlace = HeightDesired - HeightDifference;
        }

        if (IsBigger)
        {
            // Set the game screen to the correct size if the current size is larger than the desired size.
            Screen.SetResolution((int)WidthPlace, (int)HeightPlace, Screen.fullScreen);
        }
        else
        {
            // Set the game screen to the correct size if the current size is smaller than the desired size.
            Screen.SetResolution((int)WidthPlace, (int)HeightPlace, Screen.fullScreen);
        }
    }
}