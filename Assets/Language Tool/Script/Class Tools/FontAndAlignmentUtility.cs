/*
 * ---------------------------------------------------------------------------
 * Description: A utility class for managing fonts and text alignment in 
 *              Unity's UI system. This class provides methods to retrieve fonts by index, 
 *              find the index of a specified font, and convert between integer alignment 
 *              codes and TextAnchor values. It supports multilingual applications by 
 *              loading language-specific font lists and ensures correct text alignment 
 *              settings.
 * Author: Lucas Gomes Cecchini
 * Pseudonym: AGAMENOM
 * ---------------------------------------------------------------------------
*/

using UnityEngine;

namespace LanguageTools.Legacy
{
    public static class FontAndAlignmentUtility
    {
        // Retrieves a font from the font list based on the given index.
        public static Font GetFontByIndex(int fontListIndex)
        {
            // Loads the language settings to access the font list.
            var fontListObject = LanguageFileManager.LoadLanguageSettings().fontListData;

            // Checks if the font index is within valid range and returns the corresponding font.
            if (fontListIndex > 0 && fontListIndex <= fontListObject.fontList.Count)
            {
                return fontListObject.fontList[fontListIndex - 1];
            }

            return null; // Returns null if the index is out of range.
        }

        // Retrieves the index of a specific font from the font list.
        public static int GetFontIndex(Font font)
        {
            // Loads the language settings to access the font list.
            var fontListObject = LanguageFileManager.LoadLanguageSettings().fontListData;

            // Iterates through the font list to find the matching font and returns its index.
            for (int i = 0; i < fontListObject.fontList.Count; i++)
            {
                if (fontListObject.fontList[i] == font)
                {
                    return i + 1; // Adding 1 to match the indexing style used elsewhere.
                }
            }

            return 0; // Returns 0 if the font is not found in the list.
        }

        // Converts an integer alignment code to the corresponding TextAnchor value.
        public static TextAnchor ConvertToTextAnchor(int alignment)
        {
            var newAlignment = TextAnchor.UpperLeft; // Default value for the text alignment.

            // Maps specific alignment codes to TextAnchor values.
            switch (alignment)
            {
                case 1:
                    newAlignment = TextAnchor.UpperLeft;
                    break;
                case 2:
                    newAlignment = TextAnchor.UpperCenter;
                    break;
                case 3:
                    newAlignment = TextAnchor.UpperRight;
                    break;
                case 7:
                    newAlignment = TextAnchor.MiddleLeft;
                    break;
                case 8:
                    newAlignment = TextAnchor.MiddleCenter;
                    break;
                case 9:
                    newAlignment = TextAnchor.MiddleRight;
                    break;
                case 13:
                    newAlignment = TextAnchor.LowerLeft;
                    break;
                case 14:
                    newAlignment = TextAnchor.LowerCenter;
                    break;
                case 15:
                    newAlignment = TextAnchor.LowerRight;
                    break;                
                default:
                    Debug.LogWarning($"Alignment value '{alignment}' is not recognized. Using default alignment.");
                    break;
            }

            return newAlignment;
        }

        // Converts a TextAnchor value to the corresponding integer alignment code.
        public static int ConvertToAlignmentCode(TextAnchor alignment)
        {
            int alignmentValue = 1; // Default alignment code.

            // Maps TextAnchor values to corresponding integer alignment codes.
            switch (alignment)
            {
                case TextAnchor.UpperLeft:
                    alignmentValue = 1;
                    break;
                case TextAnchor.UpperCenter:
                    alignmentValue = 2;
                    break;
                case TextAnchor.UpperRight:
                    alignmentValue = 3;
                    break;
                case TextAnchor.MiddleLeft:
                    alignmentValue = 7;
                    break;
                case TextAnchor.MiddleCenter:
                    alignmentValue = 8;
                    break;
                case TextAnchor.MiddleRight:
                    alignmentValue = 9;
                    break;
                case TextAnchor.LowerLeft:
                    alignmentValue = 13;
                    break;
                case TextAnchor.LowerCenter:
                    alignmentValue = 14;
                    break;
                case TextAnchor.LowerRight:
                    alignmentValue = 15;
                    break;
                default:
                    Debug.LogWarning($"Alignment option '{alignment}' is not recognized. Returning default alignment value.");
                    break;
            }

            return alignmentValue;
        }
    }
}