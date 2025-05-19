/*
 * ---------------------------------------------------------------------------
 * Description: Utility class for managing legacy Unity UI fonts and text 
 *              alignment in multilingual applications. Provides font access 
 *              by index or asset, and conversion between integer alignment 
 *              codes and UnityEngine.TextAnchor values.
 * Author: Lucas Gomes Cecchini
 * Pseudonym: AGAMENOM
 * ---------------------------------------------------------------------------
*/

using UnityEngine;

using static LanguageTools.LanguageFileManager;

namespace LanguageTools.Legacy
{
    public static class FontAndAlignmentUtility
    {
        /// <summary>
        /// Retrieves a Font from the language settings by its index.
        /// </summary>
        /// <param name="fontListIndex">1-based index of the font in the list.</param>
        /// <returns>Corresponding Font, or null if index is invalid.</returns>
        public static Font GetFontByIndex(int fontListIndex)
        {
            // Load list of legacy fonts from language settings.
            var fonts = LoadLanguageSettings().fontListData.fontList;

            // Return font at index (1-based); return null if index is out of bounds.
            return (fontListIndex > 0 && fontListIndex <= fonts.Count) ? fonts[fontListIndex - 1] : null;
        }

        /// <summary>
        /// Retrieves the index of a given Font from the language settings.
        /// </summary>
        /// <param name="font">Font asset to find.</param>
        /// <returns>1-based index if found; otherwise, 0.</returns>
        public static int GetFontIndex(Font font)
        {
            // Load list of fonts from language settings.
            var fonts = LoadLanguageSettings().fontListData.fontList;

            // Search for the matching font and return its index (1-based).
            for (int i = 0; i < fonts.Count; i++)
            {
                if (fonts[i] == font) return i + 1;
            }

            return 0; // Return 0 if the font is not found.
        }

        /// <summary>
        /// Converts an integer alignment code to TextAnchor.
        /// </summary>
        /// <param name="alignment">Integer alignment code.</param>
        /// <returns>Corresponding TextAnchor enum value.</returns>
        public static TextAnchor ConvertToTextAnchor(int alignment)
        {
            // Default alignment value.
            var newAlignment = TextAnchor.UpperLeft;

            // Match integer code to corresponding TextAnchor.
            switch (alignment)
            {
                case 1: newAlignment = TextAnchor.UpperLeft; break;
                case 2: newAlignment = TextAnchor.UpperCenter; break;
                case 3: newAlignment = TextAnchor.UpperRight; break;
                case 7: newAlignment = TextAnchor.MiddleLeft; break;
                case 8: newAlignment = TextAnchor.MiddleCenter; break;
                case 9: newAlignment = TextAnchor.MiddleRight; break;
                case 13: newAlignment = TextAnchor.LowerLeft; break;
                case 14: newAlignment = TextAnchor.LowerCenter; break;
                case 15: newAlignment = TextAnchor.LowerRight; break;                
                default:
                    Debug.LogWarning($"Alignment value '{alignment}' is not recognized. Using default alignment.");
                    break;
            }

            return newAlignment;
        }

        /// <summary>
        /// Converts a TextAnchor value to its corresponding integer code.
        /// </summary>
        /// <param name="alignment">TextAnchor enum value.</param>
        /// <returns>Corresponding integer code; returns 1 if not recognized.</returns>
        public static int ConvertToAlignmentCode(TextAnchor alignment)
        {
            int alignmentValue = 0;

            // Match TextAnchor value to integer code.
            switch (alignment)
            {
                case TextAnchor.UpperLeft: alignmentValue = 1; break;
                case TextAnchor.UpperCenter: alignmentValue = 2; break;
                case TextAnchor.UpperRight: alignmentValue = 3; break;
                case TextAnchor.MiddleLeft: alignmentValue = 7; break;
                case TextAnchor.MiddleCenter: alignmentValue = 8; break;
                case TextAnchor.MiddleRight: alignmentValue = 9; break;
                case TextAnchor.LowerLeft: alignmentValue = 13; break;
                case TextAnchor.LowerCenter: alignmentValue = 14; break;
                case TextAnchor.LowerRight: alignmentValue = 15; break;
                default:
                    Debug.LogWarning($"Alignment option '{alignment}' is not recognized. Returning default alignment value.");
                    break;
            }

            return alignmentValue;
        }
    }
}