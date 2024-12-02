/*
 * ---------------------------------------------------------------------------
 * Description: This ScriptableObject holds a list of TextMeshPro (TMP) 
 *              font assets for localization purposes in Unity. It provides a custom 
 *              inspector to facilitate adding TMP_FontAsset objects via drag and drop.
 * Author: Lucas Gomes Cecchini
 * Pseudonym: AGAMENOM
 * ---------------------------------------------------------------------------
*/

using System.Collections.Generic;
using UnityEngine;
using TMPro;

#if UNITY_EDITOR
using UnityEditor;
#endif

// ScriptableObject to hold a list of TMP_FontAsset.
[CreateAssetMenu(fileName = "New Language Font List (TMP)", menuName = "Language/Language Font List Data (TMP)")]
public class LanguageFontListDataTMP : ScriptableObject
{
    public List<TMP_FontAsset> TMPFontList; // List of TMP_FontAsset assets.
}

#if UNITY_EDITOR
// Custom inspector for the LanguageFontListDataTMP ScriptableObject.
[CustomEditor(typeof(LanguageFontListDataTMP))]
public class LanguageFontListDataTMPInspector : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update(); // Update the serialized object to reflect changes.

        var script = (LanguageFontListDataTMP)target; // Cast the target object to LanguageFontListDataTMP.

        // Create a drop area in the inspector for dragging and dropping TMP_FontAsset assets.
        Rect dropArea = GUILayoutUtility.GetRect(0, 100, GUILayout.ExpandWidth(true));
        EditorGUI.DrawRect(dropArea, new Color(0.15f, 0.15f, 0.15f, 0.5f)); // Draw a background color for the drop area.
        EditorGUI.DropShadowLabel(dropArea, "Drop Font Assets (TMP) Here"); // Draw a label indicating where to drop TMP_FontAsset assets.

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
                    Undo.RecordObject(script, "Add Fonts (TMP)"); // Record the undo operation for adding fonts.

                    // Add each dragged object to the TMPFontList if it is a TMP_FontAsset and not already in the list.
                    foreach (Object draggedObject in DragAndDrop.objectReferences)
                    {
                        if (draggedObject is TMP_FontAsset font && !script.TMPFontList.Contains(font))
                        {
                            script.TMPFontList.Add(font);
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