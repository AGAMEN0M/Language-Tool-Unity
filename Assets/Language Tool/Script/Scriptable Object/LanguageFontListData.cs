/*
 * ---------------------------------------------------------------------------
 * Description: This ScriptableObject holds a list of legacy Font assets 
 *              for localization purposes in Unity. It provides a custom inspector to 
 *              facilitate adding Font objects via drag and drop.
 * Author: Lucas Gomes Cecchini
 * Pseudonym: AGAMENOM
 * ---------------------------------------------------------------------------
*/

using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

// ScriptableObject that holds a list of Font assets.
[CreateAssetMenu(fileName = "New Language Font List (Legacy)", menuName = "Language/Language Font List Data (Legacy)")]
public class LanguageFontListData : ScriptableObject
{
    public List<Font> fontList; // List of Font assets.
}

#if UNITY_EDITOR
// Custom inspector for the LanguageFontListData ScriptableObject.
[CustomEditor(typeof(LanguageFontListData))]
public class LanguageFontListDataInspector : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update(); // Update the serialized object before drawing the inspector.

        var script = (LanguageFontListData)target; // Cast the target object to LanguageFontListData.

        // Create a drop area in the inspector GUI for dragging and dropping Font assets.
        Rect dropArea = GUILayoutUtility.GetRect(0, 100, GUILayout.ExpandWidth(true));
        EditorGUI.DrawRect(dropArea, new Color(0.15f, 0.15f, 0.15f, 0.5f)); // Draw a background color for the drop area.
        EditorGUI.DropShadowLabel(dropArea, "Drop Font Assets Here"); // Draw a label indicating where to drop Font assets.

        // Handle drag and drop events.
        var evt = Event.current;
        if (evt.type == EventType.DragUpdated || evt.type == EventType.DragPerform)
        {
            // Check if the drag event is within the drop area.
            if (dropArea.Contains(evt.mousePosition))
            {
                // Set visual mode to indicate that dropping is allowed.
                DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                if (evt.type == EventType.DragPerform)
                {
                    DragAndDrop.AcceptDrag(); // Accept the drag and drop operation.
                    Undo.RecordObject(script, "Add Fonts"); // Record the undo operation for adding fonts.

                    // Add each dragged object to the fontList if it is a Font and not already in the list.
                    foreach (Object draggedObject in DragAndDrop.objectReferences)
                    {
                        if (draggedObject is Font font && !script.fontList.Contains(font))
                        {
                            script.fontList.Add(font);
                        }
                    }
                    EditorUtility.SetDirty(script); // Mark the scriptable object as dirty to save changes.
                }
                evt.Use();
            }
        }

        GUILayout.Space(10); // Add some space before drawing the default inspector.

        DrawDefaultInspector(); // Draw the default inspector properties.

        serializedObject.ApplyModifiedProperties(); // Apply any changes made to the serialized object.
    }
}
#endif