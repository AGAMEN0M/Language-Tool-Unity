/*
 * ---------------------------------------------------------------------------
 * Description: ScriptableObject containing a list of legacy Unity Font objects 
 *              used for localization purposes in Unity. Includes a custom 
 *              inspector that supports drag-and-drop for easier font management.
 *              
 * Author: Lucas Gomes Cecchini
 * Pseudonym: AGAMENOM
 * ---------------------------------------------------------------------------
*/

using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
using static LanguageTools.Editor.LanguageEditorUtilities;
#endif

/// <summary>
/// Holds a list of legacy Font assets for localization support.
/// </summary>
[CreateAssetMenu(fileName = "New Language Font List (Legacy)", menuName = "Language/Language Font List Data (Legacy)", order = 2)]
public class LanguageFontListData : ScriptableObject
{
    #region === Fields ===

    [Tooltip("List of Unity legacy Font objects used for localization.")]
    public List<Font> fontList = new(); // Initialized to prevent null errors.

    #endregion
}

#if UNITY_EDITOR

#region === Custom Editor ===

/// <summary>
/// Custom inspector that enables drag-and-drop for adding Font assets to the list.
/// </summary>
[CanEditMultipleObjects]
[CustomEditor(typeof(LanguageFontListData))]
public class LanguageFontListDataInspector : Editor
{
    public override void OnInspectorGUI()
    {
        // Sync serialized data with the target object.
        serializedObject.Update();
        var script = (LanguageFontListData)target;

        using (new EditorGUI.DisabledScope(targets.Length > 1))
        {
            // Create a large area for drag-and-drop input.
            Rect dropArea = GUILayoutUtility.GetRect(0, 100, GUILayout.ExpandWidth(true));
            EditorGUI.DrawRect(dropArea, new Color(0.15f, 0.15f, 0.15f, 0.5f));
            EditorGUI.DropShadowLabel(dropArea, "Drop Font Assets Here", CreateLabelStyle(13, true, true));

            var evt = Event.current;

            // Handle drag and drop interaction inside the defined area.
            if ((evt.type == EventType.DragUpdated || evt.type == EventType.DragPerform) && dropArea.Contains(evt.mousePosition))
            {
                DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

                if (evt.type == EventType.DragPerform)
                {
                    DragAndDrop.AcceptDrag();
                    Undo.RecordObject(script, "Add Fonts");

                    // Add unique Font objects to the list.
                    foreach (var obj in DragAndDrop.objectReferences)
                    {
                        if (obj is Font font && !script.fontList.Contains(font))
                        {
                            script.fontList.Add(font);
                        }
                    }

                    EditorUtility.SetDirty(script); // Mark the asset as dirty to save changes.
                }

                evt.Use(); // Use the current event to avoid default processing.
            }
        }

        // Draw any other serialized properties.
        GUILayout.Space(10);
        DrawDefaultInspector();

        // Apply property modifications to serialized object.
        serializedObject.ApplyModifiedProperties();
    }
}

#endregion

#endif