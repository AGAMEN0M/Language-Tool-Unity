/*
 * ---------------------------------------------------------------------------
 * Description: Utility class for managing fonts and alignment settings 
 *              in TextMeshPro components used in multilingual Unity projects.
 *              It provides font lookup by index or asset, and bidirectional
 *              conversion between integer alignment codes and TMP's 
 *              TextAlignmentOptions enum values.
 *              
 * Author: Lucas Gomes Cecchini
 * Pseudonym: AGAMENOM
 * ---------------------------------------------------------------------------
*/

using UnityEngine;
using TMPro;

using static LanguageTools.LanguageFileManager;

namespace LanguageTools.TMP
{
    public static class FontAndAlignmentUtilityTMP
    {
        #region === Font Retrieval ===

        /// <summary>
        /// Retrieves a TMP_FontAsset from the language settings by its index.
        /// </summary>
        /// <param name="fontListIndex">1-based index of the font in the list.</param>
        /// <returns>Corresponding TMP_FontAsset, or null if index is invalid.</returns>
        public static TMP_FontAsset GetFontByIndex(int fontListIndex)
        {
            // Load list of TMP fonts from language settings.
            var fonts = LoadLanguageSettings().fontListDataTMP.TMPFontList;

            // Return font at index (1-based); return null if index is out of range.
            return (fontListIndex > 0 && fontListIndex <= fonts.Count) ? fonts[fontListIndex - 1] : null;
        }

        /// <summary>
        /// Retrieves the index of a given TMP_FontAsset from the language settings.
        /// </summary>
        /// <param name="TMPFont">Font asset to find.</param>
        /// <returns>1-based index if found; otherwise, 0.</returns>
        public static int GetFontIndex(TMP_FontAsset TMPFont)
        {
            // Load list of TMP fonts from language settings.
            var fonts = LoadLanguageSettings().fontListDataTMP.TMPFontList;

            // Iterate over the list to find a match.
            for (int i = 0; i < fonts.Count; i++)
            {
                if (fonts[i] == TMPFont) return i + 1; // Return 1-based index.
            }

            return 0; // Return 0 if font not found.
        }

        #endregion

        #region === Alignment Conversion ===

        /// <summary>
        /// Converts an integer alignment code to TextAlignmentOptions.
        /// </summary>
        /// <param name="alignment">Integer alignment code.</param>
        /// <returns>Corresponding TextAlignmentOptions enum value.</returns>
        public static TextAlignmentOptions ConvertToTextAnchor(int alignment)
        {
            // Default to TopLeft if alignment code is invalid.
            var newAlignment = TextAlignmentOptions.TopLeft;

            // Match alignment code to corresponding enum value.
            switch (alignment)
            {
                case 1: newAlignment = TextAlignmentOptions.TopLeft; break;
                case 2: newAlignment = TextAlignmentOptions.Top; break;
                case 3: newAlignment = TextAlignmentOptions.TopRight; break;
                case 4: newAlignment = TextAlignmentOptions.TopJustified; break;
                case 5: newAlignment = TextAlignmentOptions.TopFlush; break;
                case 6: newAlignment = TextAlignmentOptions.TopGeoAligned; break;
                case 7: newAlignment = TextAlignmentOptions.Left; break;
                case 8: newAlignment = TextAlignmentOptions.Center; break;
                case 9: newAlignment = TextAlignmentOptions.Right; break;
                case 10: newAlignment = TextAlignmentOptions.Justified; break;
                case 11: newAlignment = TextAlignmentOptions.Flush; break;
                case 12: newAlignment = TextAlignmentOptions.CenterGeoAligned; break;
                case 13: newAlignment = TextAlignmentOptions.BottomLeft; break;
                case 14: newAlignment = TextAlignmentOptions.Bottom; break;
                case 15: newAlignment = TextAlignmentOptions.BottomRight; break;
                case 16: newAlignment = TextAlignmentOptions.BottomJustified; break;
                case 17: newAlignment = TextAlignmentOptions.BottomFlush; break;
                case 18: newAlignment = TextAlignmentOptions.BottomGeoAligned; break;
                case 19: newAlignment = TextAlignmentOptions.BaselineLeft; break;
                case 20: newAlignment = TextAlignmentOptions.Baseline; break;
                case 21: newAlignment = TextAlignmentOptions.BaselineRight; break;
                case 22: newAlignment = TextAlignmentOptions.BaselineJustified; break;
                case 23: newAlignment = TextAlignmentOptions.BaselineFlush; break;
                case 24: newAlignment = TextAlignmentOptions.BaselineGeoAligned; break;
                case 25: newAlignment = TextAlignmentOptions.MidlineLeft; break;
                case 26: newAlignment = TextAlignmentOptions.Midline; break;
                case 27: newAlignment = TextAlignmentOptions.MidlineRight; break;
                case 28: newAlignment = TextAlignmentOptions.MidlineJustified; break;
                case 29: newAlignment = TextAlignmentOptions.MidlineFlush; break;
                case 30: newAlignment = TextAlignmentOptions.MidlineGeoAligned; break;
                case 31: newAlignment = TextAlignmentOptions.CaplineLeft; break;
                case 32: newAlignment = TextAlignmentOptions.Capline; break;
                case 33: newAlignment = TextAlignmentOptions.CaplineRight; break;
                case 34: newAlignment = TextAlignmentOptions.CaplineJustified; break;
                case 35: newAlignment = TextAlignmentOptions.CaplineFlush; break;
                case 36: newAlignment = TextAlignmentOptions.CaplineGeoAligned; break;
                case 37: newAlignment = TextAlignmentOptions.Converted; break;
                default:
                    Debug.LogWarning($"Alignment value '{alignment}' is not recognized. Using default alignment.");
                    break;
            }

            return newAlignment;
        }

        /// <summary>
        /// Converts a TextAlignmentOptions value to its corresponding integer code.
        /// </summary>
        /// <param name="alignment">TextAlignmentOptions enum value.</param>
        /// <returns>Corresponding integer code; returns 1 if not recognized.</returns>
        public static int ConvertToAlignmentCode(TextAlignmentOptions alignment)
        {
            int alignmentValue = 0;

            // Map each enum value to its integer representation.
            switch (alignment)
            {
                case TextAlignmentOptions.TopLeft: alignmentValue = 1; break;
                case TextAlignmentOptions.Top: alignmentValue = 2; break;
                case TextAlignmentOptions.TopRight: alignmentValue = 3; break;
                case TextAlignmentOptions.TopJustified: alignmentValue = 4; break;
                case TextAlignmentOptions.TopFlush: alignmentValue = 5; break;
                case TextAlignmentOptions.TopGeoAligned: alignmentValue = 6; break;
                case TextAlignmentOptions.Left: alignmentValue = 7; break;
                case TextAlignmentOptions.Center: alignmentValue = 8; break;
                case TextAlignmentOptions.Right: alignmentValue = 9; break;
                case TextAlignmentOptions.Justified: alignmentValue = 10; break;
                case TextAlignmentOptions.Flush: alignmentValue = 11; break;
                case TextAlignmentOptions.CenterGeoAligned: alignmentValue = 12; break;
                case TextAlignmentOptions.BottomLeft: alignmentValue = 13; break;
                case TextAlignmentOptions.Bottom: alignmentValue = 14; break;
                case TextAlignmentOptions.BottomRight: alignmentValue = 15; break;
                case TextAlignmentOptions.BottomJustified: alignmentValue = 16; break;
                case TextAlignmentOptions.BottomFlush: alignmentValue = 17; break;
                case TextAlignmentOptions.BottomGeoAligned: alignmentValue = 18; break;
                case TextAlignmentOptions.BaselineLeft: alignmentValue = 19; break;
                case TextAlignmentOptions.Baseline: alignmentValue = 20; break;
                case TextAlignmentOptions.BaselineRight: alignmentValue = 21; break;
                case TextAlignmentOptions.BaselineJustified: alignmentValue = 22; break;
                case TextAlignmentOptions.BaselineFlush: alignmentValue = 23; break;
                case TextAlignmentOptions.BaselineGeoAligned: alignmentValue = 24; break;
                case TextAlignmentOptions.MidlineLeft: alignmentValue = 25; break;
                case TextAlignmentOptions.Midline: alignmentValue = 26; break;
                case TextAlignmentOptions.MidlineRight: alignmentValue = 27; break;
                case TextAlignmentOptions.MidlineJustified: alignmentValue = 28; break;
                case TextAlignmentOptions.MidlineFlush: alignmentValue = 29; break;
                case TextAlignmentOptions.MidlineGeoAligned: alignmentValue = 30; break;
                case TextAlignmentOptions.CaplineLeft: alignmentValue = 31; break;
                case TextAlignmentOptions.Capline: alignmentValue = 32; break;
                case TextAlignmentOptions.CaplineRight: alignmentValue = 33; break;
                case TextAlignmentOptions.CaplineJustified: alignmentValue = 34; break;
                case TextAlignmentOptions.CaplineFlush: alignmentValue = 35; break;
                case TextAlignmentOptions.CaplineGeoAligned: alignmentValue = 36; break;
                case TextAlignmentOptions.Converted: alignmentValue = 37; break;
                default:
                    Debug.LogWarning($"Alignment option '{alignment}' is not recognized. Returning default alignment value.");
                    break;
            }

            return alignmentValue;
        }

        #endregion
    }
}