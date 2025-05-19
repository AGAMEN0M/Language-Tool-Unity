/*
 * ---------------------------------------------------------------------------
 * Description: Defines a custom PropertyAttribute and its corresponding PropertyDrawer
 *              to validate unique integer IDs in Unity's Inspector. It reads a JSON 
 *              configuration file in ProjectSettings to check if the assigned ID 
 *              (for a component or canvas) is already in use, helping prevent conflicts 
 *              in localized UI systems.
 *
 * Usage:   [IDExists] public int componentID = 0;
 *          [IDExists(true)] public int canvasID = 0;
 *
 * Author: Lucas Gomes Cecchini
 * Pseudonym: AGAMENOM
 * ---------------------------------------------------------------------------
*/

using UnityEngine;

#if UNITY_EDITOR
using LanguageTools;
using UnityEditor;
using System.Linq;
using System.IO;
#endif

/// <summary>
/// Attribute to mark integer fields that require validation against duplicate IDs.
/// </summary>
public class IDExistsAttribute : PropertyAttribute
{
    /// <summary>
    /// Whether the check should be done against canvas IDs instead of component IDs.
    /// </summary>
    public bool searchCanvas;

    /// <summary>
    /// Initializes a new instance of the <see cref="IDExistsAttribute"/> class.
    /// </summary>
    /// <param name="searchCanvas">Set to true to check canvas IDs; false for component IDs.</param>
    public IDExistsAttribute(bool searchCanvas = false)
    {
        this.searchCanvas = searchCanvas;
    }
}

#if UNITY_EDITOR
/// <summary>
/// Custom drawer that checks for duplicate IDs in LanguageFileData.json
/// and displays a warning in the Unity Inspector.
/// </summary>
[CustomPropertyDrawer(typeof(IDExistsAttribute))]
public class IDExistsDrawer : PropertyDrawer
{
    private const string FilePath = "ProjectSettings/LanguageFileData.json";
    private static LanguageFileManagerWindowData cachedData = new();
    private static System.DateTime lastFileWriteTime;

    /// <summary>
    /// Renders the integer field and warning box if a duplicate ID is detected.
    /// </summary>
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        var existsAttribute = (IDExistsAttribute)attribute;

        // Save the original GUI color to restore later.
        Color previousColor = GUI.color;

        // Check if the integer value already exists in the corresponding ID list.
        bool idIsUsed = property.propertyType == SerializedPropertyType.Integer && CheckIDExists(property.intValue, existsAttribute.searchCanvas);

        // If the ID is used, set GUI color to yellow for highlighting.
        GUI.color = idIsUsed ? Color.yellow : previousColor;

        // Draw the property field for the integer.
        Rect fieldRect = new(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
        EditorGUI.PropertyField(fieldRect, property, label);

        // If the ID is duplicated, show a warning help box below the field.
        if (idIsUsed)
        {
            string idType = existsAttribute.searchCanvas ? "canvasID" : "component ID";
            Rect helpBoxRect = new(position.x, position.y + EditorGUIUtility.singleLineHeight + 2, position.width, EditorGUIUtility.singleLineHeight * 2f);
            EditorGUI.HelpBox(helpBoxRect, $"The {idType} '{property.intValue}' is already in use in the Language File Manager Window!", MessageType.Warning);
        }

        // Restore the original GUI color.
        GUI.color = previousColor;
    }

    /// <summary>
    /// Adjusts the property height to accommodate the warning message if necessary.
    /// </summary>
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        var existsAttribute = (IDExistsAttribute)attribute;

        // If not an integer field, use default height.
        if (property.propertyType != SerializedPropertyType.Integer) return EditorGUIUtility.singleLineHeight;

        // Add extra height if the ID already exists and a warning box will be displayed.
        return CheckIDExists(property.intValue, existsAttribute.searchCanvas) ? EditorGUIUtility.singleLineHeight * 3f + 4 : EditorGUIUtility.singleLineHeight;
    }

    /// <summary>
    /// Loads LanguageFileData.json (only if changed) and checks for duplicate IDs.
    /// </summary>
    private bool CheckIDExists(int id, bool checkCanvas)
    {
        // Return false if the file does not exist.
        if (!File.Exists(FilePath)) return false;

        // Get the last modified timestamp of the JSON file.
        var currentWriteTime = File.GetLastWriteTime(FilePath);

        // Reload and parse the file only if it was changed since last read.
        if (currentWriteTime != lastFileWriteTime)
        {
            cachedData = JsonUtility.FromJson<LanguageFileManagerWindowData>(File.ReadAllText(FilePath));
            lastFileWriteTime = currentWriteTime;
        }

        // Check for the presence of the ID in either the canvas or component list.
        return cachedData != null && (checkCanvas? cachedData.canvasSave?.Any(c => c.canvasID == id) == true : cachedData.componentSave?.Any(c => c.iD == id) == true);
    }
}
#endif