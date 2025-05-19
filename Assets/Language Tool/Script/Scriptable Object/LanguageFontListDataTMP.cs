/*
 * ---------------------------------------------------------------------------
 * Description: ScriptableObject containing a list of TMP_FontAsset objects 
 *              used for localization purposes in Unity. Includes a custom 
 *              inspector that supports drag-and-drop for easier font management.
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

/// <summary>
/// Holds a list of TextMeshPro font assets for localization support.
/// </summary>
[CreateAssetMenu(fileName = "New Language Font List (TMP)", menuName = "Language/Language Font List Data (TMP)", order = 3)]
public class LanguageFontListDataTMP : ScriptableObject
{
    [Tooltip("List of TMP_FontAsset objects used for localization.")]
    public List<TMP_FontAsset> TMPFontList = new(); // Initialized to prevent null errors.
}

#if UNITY_EDITOR
/// <summary>
/// Custom inspector that enables drag-and-drop for adding TMP_FontAssets to the list.
/// </summary>
[CanEditMultipleObjects]
[CustomEditor(typeof(LanguageFontListDataTMP))]
public class LanguageFontListDataTMPInspector : Editor
{
    public override void OnInspectorGUI()
    {
        // Sync serialized data with the target object.
        serializedObject.Update();
        var script = (LanguageFontListDataTMP)target;

        using (new EditorGUI.DisabledScope(targets.Length > 1))
        {
            // Create a large area for drag-and-drop input.
            Rect dropArea = GUILayoutUtility.GetRect(0, 100, GUILayout.ExpandWidth(true));
            EditorGUI.DrawRect(dropArea, new Color(0.15f, 0.15f, 0.15f, 0.5f));
            EditorGUI.DropShadowLabel(dropArea, "Drop TMP Font Assets Here");

            Event evt = Event.current;

            // Handle drag and drop interaction inside the defined area.
            if ((evt.type == EventType.DragUpdated || evt.type == EventType.DragPerform) && dropArea.Contains(evt.mousePosition))
            {
                DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

                if (evt.type == EventType.DragPerform)
                {
                    DragAndDrop.AcceptDrag();
                    Undo.RecordObject(script, "Add TMP Fonts");

                    // Add unique TMP_FontAsset objects to the list.
                    foreach (Object obj in DragAndDrop.objectReferences)
                    {
                        if (obj is TMP_FontAsset font && !script.TMPFontList.Contains(font))
                        {
                            script.TMPFontList.Add(font);
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
#endif