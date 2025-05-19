using System.Collections.Generic;
using LanguageTools;
using UnityEngine;

/// <summary>
/// Converts elements of componentSave and canvasSave lists to JSON.
/// </summary>
public class DataToJsonConverter : MonoBehaviour
{
    [SerializeField] private List<LanguageForEditingSave> componentSave = new();
    [SerializeField] private List<CanvasForEditingSave> canvasSave = new();
    [Space(10)]
    [SerializeField] private List<string> component;
    [SerializeField] private List<string> canvas;

    [ContextMenu("Data to Json")]
    public void JsonConverter()
    {
        // Converts each componentSave item to JSON.
        component = new List<string>();
        foreach (var item in componentSave)
        {
            string json = JsonUtility.ToJson(item);
            component.Add(json);
        }

        // Converts each item from canvasSave to JSON.
        canvas = new List<string>();
        foreach (var item in canvasSave)
        {
            string json = JsonUtility.ToJson(item);
            canvas.Add(json);
        }
    }
}