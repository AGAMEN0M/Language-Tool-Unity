/*
 * ---------------------------------------------------------------------------
 * Description: A utility class for managing fonts and text alignment in 
 *              TextMeshPro components within a Unity project. This class provides methods 
 *              to retrieve fonts by index, get the index of a specified font, and convert 
 *              between integer alignment codes and TextAlignmentOptions. It supports 
 *              multilingual setups by loading language-specific font lists and ensuring 
 *              proper alignment settings.
 * Author: Lucas Gomes Cecchini
 * Pseudonym: AGAMENOM
 * ---------------------------------------------------------------------------
*/

using UnityEngine;
using TMPro;

namespace LanguageTools.TMP
{
    public static class FontAndAlignmentUtilityTMP
    {
        // Retrieves a TMP_FontAsset from the TMP font list based on the given index.
        public static TMP_FontAsset GetFontByIndex(int fontListIndex)
        {
            // Loads the language settings to access the TMP font list.
            var fontListObject = LanguageFileManager.LoadLanguageSettings().fontListDataTMP;

            // Checks if the font index is within valid range and returns the corresponding TMP font asset.
            if (fontListIndex > 0 && fontListIndex <= fontListObject.TMPFontList.Count)
            {
                return fontListObject.TMPFontList[fontListIndex - 1];
            }

            return null; // Returns null if the index is out of range.
        }

        // Retrieves the index of a specific TMP_FontAsset from the TMP font list.
        public static int GetFontIndex(TMP_FontAsset TMPFont)
        {
            // Loads the language settings to access the TMP font list.
            var fontListObject = LanguageFileManager.LoadLanguageSettings().fontListDataTMP;

            // Iterates through the TMP font list to find the matching font and returns its index.
            for (int i = 0; i < fontListObject.TMPFontList.Count; i++)
            {
                if (fontListObject.TMPFontList[i] == TMPFont)
                {
                    return i + 1; // Adding 1 to match the indexing style used elsewhere.
                }
            }

            return 0; // Returns 0 if the font is not found in the list.
        }

        // Converts an integer alignment code to the corresponding TextAlignmentOptions value for TMP.
        public static TextAlignmentOptions ConvertToTextAnchor(int alignment)
        {
            var newAlignment = TextAlignmentOptions.TopLeft; // Default value for the text alignment in TMP.

            // Maps specific alignment codes to TextAlignmentOptions values for TMP.
            switch (alignment)
            {
                case 1:
                    newAlignment = TextAlignmentOptions.TopLeft;
                    break;
                case 2:
                    newAlignment = TextAlignmentOptions.Top;
                    break;
                case 3:
                    newAlignment = TextAlignmentOptions.TopRight;
                    break;
                case 4:
                    newAlignment = TextAlignmentOptions.TopJustified;
                    break;
                case 5:
                    newAlignment = TextAlignmentOptions.TopFlush;
                    break;
                case 6:
                    newAlignment = TextAlignmentOptions.TopGeoAligned;
                    break;
                case 7:
                    newAlignment = TextAlignmentOptions.Left;
                    break;
                case 8:
                    newAlignment = TextAlignmentOptions.Center;
                    break;
                case 9:
                    newAlignment = TextAlignmentOptions.Right;
                    break;
                case 10:
                    newAlignment = TextAlignmentOptions.Justified;
                    break;
                case 11:
                    newAlignment = TextAlignmentOptions.Flush;
                    break;
                case 12:
                    newAlignment = TextAlignmentOptions.CenterGeoAligned;
                    break;
                case 13:
                    newAlignment = TextAlignmentOptions.BottomLeft;
                    break;
                case 14:
                    newAlignment = TextAlignmentOptions.Bottom;
                    break;
                case 15:
                    newAlignment = TextAlignmentOptions.BottomRight;
                    break;
                case 16:
                    newAlignment = TextAlignmentOptions.BottomJustified;
                    break;
                case 17:
                    newAlignment = TextAlignmentOptions.BottomFlush;
                    break;
                case 18:
                    newAlignment = TextAlignmentOptions.BottomGeoAligned;
                    break;
                case 19:
                    newAlignment = TextAlignmentOptions.BaselineLeft;
                    break;
                case 20:
                    newAlignment = TextAlignmentOptions.Baseline;
                    break;
                case 21:
                    newAlignment = TextAlignmentOptions.BaselineRight;
                    break;
                case 22:
                    newAlignment = TextAlignmentOptions.BaselineJustified;
                    break;
                case 23:
                    newAlignment = TextAlignmentOptions.BaselineFlush;
                    break;
                case 24:
                    newAlignment = TextAlignmentOptions.BaselineGeoAligned;
                    break;
                case 25:
                    newAlignment = TextAlignmentOptions.MidlineLeft;
                    break;
                case 26:
                    newAlignment = TextAlignmentOptions.Midline;
                    break;
                case 27:
                    newAlignment = TextAlignmentOptions.MidlineRight;
                    break;
                case 28:
                    newAlignment = TextAlignmentOptions.MidlineJustified;
                    break;
                case 29:
                    newAlignment = TextAlignmentOptions.MidlineFlush;
                    break;
                case 30:
                    newAlignment = TextAlignmentOptions.MidlineGeoAligned;
                    break;
                case 31:
                    newAlignment = TextAlignmentOptions.CaplineLeft;
                    break;
                case 32:
                    newAlignment = TextAlignmentOptions.Capline;
                    break;
                case 33:
                    newAlignment = TextAlignmentOptions.CaplineRight;
                    break;
                case 34:
                    newAlignment = TextAlignmentOptions.CaplineJustified;
                    break;
                case 35:
                    newAlignment = TextAlignmentOptions.CaplineFlush;
                    break;
                case 36:
                    newAlignment = TextAlignmentOptions.CaplineGeoAligned;
                    break;
                case 37:
                    newAlignment = TextAlignmentOptions.Converted;
                    break;
                default:
                    Debug.LogWarning($"Alignment value '{alignment}' is not recognized. Using default alignment.");
                    break;
            }

            return newAlignment;
        }

        // Converts a TextAlignmentOptions value for TMP to the corresponding integer alignment code.
        public static int ConvertToAlignmentCode(TextAlignmentOptions alignment)
        {
            int alignmentValue = 1; // Default alignment code.

            // Maps TextAlignmentOptions values for TMP to corresponding integer alignment codes.
            switch (alignment)
            {
                case TextAlignmentOptions.TopLeft:
                    alignmentValue = 1;
                    break;
                case TextAlignmentOptions.Top:
                    alignmentValue = 2;
                    break;
                case TextAlignmentOptions.TopRight:
                    alignmentValue = 3;
                    break;
                case TextAlignmentOptions.TopJustified:
                    alignmentValue = 4;
                    break;
                case TextAlignmentOptions.TopFlush:
                    alignmentValue = 5;
                    break;
                case TextAlignmentOptions.TopGeoAligned:
                    alignmentValue = 6;
                    break;
                case TextAlignmentOptions.Left:
                    alignmentValue = 7;
                    break;
                case TextAlignmentOptions.Center:
                    alignmentValue = 8;
                    break;
                case TextAlignmentOptions.Right:
                    alignmentValue = 9;
                    break;
                case TextAlignmentOptions.Justified:
                    alignmentValue = 10;
                    break;
                case TextAlignmentOptions.Flush:
                    alignmentValue = 11;
                    break;
                case TextAlignmentOptions.CenterGeoAligned:
                    alignmentValue = 12;
                    break;
                case TextAlignmentOptions.BottomLeft:
                    alignmentValue = 13;
                    break;
                case TextAlignmentOptions.Bottom:
                    alignmentValue = 14;
                    break;
                case TextAlignmentOptions.BottomRight:
                    alignmentValue = 15;
                    break;
                case TextAlignmentOptions.BottomJustified:
                    alignmentValue = 16;
                    break;
                case TextAlignmentOptions.BottomFlush:
                    alignmentValue = 17;
                    break;
                case TextAlignmentOptions.BottomGeoAligned:
                    alignmentValue = 18;
                    break;
                case TextAlignmentOptions.BaselineLeft:
                    alignmentValue = 19;
                    break;
                case TextAlignmentOptions.Baseline:
                    alignmentValue = 20;
                    break;
                case TextAlignmentOptions.BaselineRight:
                    alignmentValue = 21;
                    break;
                case TextAlignmentOptions.BaselineJustified:
                    alignmentValue = 22;
                    break;
                case TextAlignmentOptions.BaselineFlush:
                    alignmentValue = 23;
                    break;
                case TextAlignmentOptions.BaselineGeoAligned:
                    alignmentValue = 24;
                    break;
                case TextAlignmentOptions.MidlineLeft:
                    alignmentValue = 25;
                    break;
                case TextAlignmentOptions.Midline:
                    alignmentValue = 26;
                    break;
                case TextAlignmentOptions.MidlineRight:
                    alignmentValue = 27;
                    break;
                case TextAlignmentOptions.MidlineJustified:
                    alignmentValue = 28;
                    break;
                case TextAlignmentOptions.MidlineFlush:
                    alignmentValue = 29;
                    break;
                case TextAlignmentOptions.MidlineGeoAligned:
                    alignmentValue = 30;
                    break;
                case TextAlignmentOptions.CaplineLeft:
                    alignmentValue = 31;
                    break;
                case TextAlignmentOptions.Capline:
                    alignmentValue = 32;
                    break;
                case TextAlignmentOptions.CaplineRight:
                    alignmentValue = 33;
                    break;
                case TextAlignmentOptions.CaplineJustified:
                    alignmentValue = 34;
                    break;
                case TextAlignmentOptions.CaplineFlush:
                    alignmentValue = 35;
                    break;
                case TextAlignmentOptions.CaplineGeoAligned:
                    alignmentValue = 36;
                    break;
                case TextAlignmentOptions.Converted:
                    alignmentValue = 37;
                    break;
                default:
                    Debug.LogWarning($"Alignment option '{alignment}' is not recognized. Returning default alignment value.");
                    break;
            }

            return alignmentValue;
        }
    }
}