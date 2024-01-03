using TMPro;

public static class TMP_TextAlignmentConverter
{
    // Method that converts horizontal alignment to vertical alignment.
    public static string GetVerticalAlignmentFromTextAlignment(TextAlignmentOptions alignment)
    {
        return alignment switch
        {
            TextAlignmentOptions.TopLeft or TextAlignmentOptions.Top or TextAlignmentOptions.TopRight => "Upper", // If the horizontal alignment is superior, it returns "Upper".
            TextAlignmentOptions.Left or TextAlignmentOptions.Center or TextAlignmentOptions.Right => "Middle", // If the horizontal alignment is center, it returns "Middle".
            TextAlignmentOptions.BottomLeft or TextAlignmentOptions.Bottom or TextAlignmentOptions.BottomRight => "Lower", // If the horizontal alignment is lower, it returns "Lower".
            _ => "Unknown", // Otherwise, it returns "Unknown" if not an expected value.
        };
    }
}