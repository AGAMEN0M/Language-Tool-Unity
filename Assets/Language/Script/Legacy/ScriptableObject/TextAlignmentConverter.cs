using UnityEngine;

public static class TextAlignmentConverter
{
    // Method that converts horizontal alignment to vertical alignment.
    public static string GetVerticalAlignmentFromTextAlignment(TextAnchor alignment)
    {
        return alignment switch
        {
            TextAnchor.UpperLeft or TextAnchor.UpperCenter or TextAnchor.UpperRight => "Upper", // If the horizontal alignment is superior, it returns "Upper".
            TextAnchor.MiddleLeft or TextAnchor.MiddleCenter or TextAnchor.MiddleRight => "Middle", // If the horizontal alignment is center, it returns "Middle".
            TextAnchor.LowerLeft or TextAnchor.LowerCenter or TextAnchor.LowerRight => "Lower", // If the horizontal alignment is lower, it returns "Lower".
            _ => "Unknown", // Otherwise, it returns "Unknown" if not an expected value.
        };
    }
}