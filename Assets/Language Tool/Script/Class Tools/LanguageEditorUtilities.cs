/*
 * ---------------------------------------------------------------------------
 * Description: Provides utility functions for the language editor, allowing interaction with language files, 
 *              handling fonts, alignment, custom buttons, and database management in the Unity Editor.
 * Author: Lucas Gomes Cecchini
 * Pseudonym: AGAMENOM
 * ---------------------------------------------------------------------------
*/

#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using UnityEditor;
using System.IO;
using System;

namespace LanguageTools.Editor
{
    public class LanguageEditorUtilities
    {
        // Draws a read-only MonoScript field for a given MonoBehaviour target in the editor.
        public static void DrawReadOnlyMonoScriptField(UnityEngine.Object target)
        {
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.ObjectField("Script", MonoScript.FromMonoBehaviour((MonoBehaviour)target), typeof(MonoBehaviour), false);
            EditorGUI.EndDisabledGroup();
        }

        // Creates a customized label style with configurable font size, boldness, and alignment.
        public static GUIStyle CreateLabelStyle(int fontSize, bool bold = false, bool centerAlignment = false)
        {
            return new GUIStyle(GUI.skin.label)
            {
                fontSize = fontSize,
                fontStyle = bold ? FontStyle.Bold : FontStyle.Normal,
                alignment = centerAlignment ? TextAnchor.MiddleCenter : TextAnchor.MiddleLeft
            };
        }

        // Creates a custom button style with a specific font size, color, and alignment.
        public static GUIStyle CreateCustomButtonStyle(int fontSize)
        {
            return new GUIStyle(GUI.skin.button)
            {
                fontSize = fontSize,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = Color.white },
                hover = { textColor = Color.red },
            };
        }

        // Checks if a given ID is present in the language list.
        public static bool IsIDInLanguageList(float iD)
        {
            var IDs = LoadLanguageIDs();
            return IDs.Contains(iD);
        }

        // Checks if a given ID is present in the canvas list from a JSON file.
        public static bool IsIDInCanvasList(float iD)
        {
            List<float> IDs = new();
            string fileData = "ProjectSettings/LanguageFileData.json";

            // Check if the file exists and load data
            if (File.Exists(fileData))
            {
                string jsonData = File.ReadAllText(fileData);
                var data = JsonUtility.FromJson<LanguageFileData>(jsonData);

                if (data != null && data.componentSave != null)
                {
                    // Add canvas IDs to the list
                    foreach (var languageSaveID in data.canvasSave)
                    {
                        IDs.Add(languageSaveID.canvasID);
                    }
                }
            }

            return IDs.Contains(iD);
        }

        // Finds duplicate IDs in the given list of IDs.
        public static List<int> FindDuplicateIDs(int[] iDs)
        {
            var textIDs = new HashSet<int>();
            var duplicateIDs = new List<int>();

            foreach (var id in iDs)
            {
                if (textIDs.Contains(id))
                {
                    if (!duplicateIDs.Contains(id)) duplicateIDs.Add(id); // Add to duplicates list if it's not already there.
                }
                else
                {
                    textIDs.Add(id);
                }
            }

            return duplicateIDs;
        }

        // Loads language IDs from the JSON file.
        public static List<float> LoadLanguageIDs()
        {
            List<float> iDs = new();
            string fileData = "ProjectSettings/LanguageFileData.json";

            // Check if the file exists and load data.
            if (File.Exists(fileData))
            {
                string jsonData = File.ReadAllText(fileData);
                var data = JsonUtility.FromJson<LanguageFileData>(jsonData);

                if (data != null && data.componentSave != null)
                {
                    foreach (var languageSaveID in data.componentSave)
                    {
                        iDs.Add(languageSaveID.iD);
                    }
                }
            }

            return iDs;
        }

        // Finds and returns a prefab by its name.
        public static GameObject FindPrefabByName(string prefabName)
        {
            string[] guids = AssetDatabase.FindAssets($"{prefabName} t:Prefab");
            foreach (string guid in guids)
            {
                string prefabPath = AssetDatabase.GUIDToAssetPath(guid);
                string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(prefabPath);

                if (fileNameWithoutExtension.Equals(prefabName, StringComparison.OrdinalIgnoreCase))
                {
                    GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
                    return prefab;
                }
            }

            Debug.LogError($"Prefab with the name '{prefabName}' not found.");
            return null;
        }

        // Finds and returns a texture by its name.
        public static Texture2D FindTextureByName(string fileName)
        {
            string[] guids = AssetDatabase.FindAssets($"{fileName} t:Texture2D");
            foreach (string guid in guids)
            {
                string texturePath = AssetDatabase.GUIDToAssetPath(guid);
                string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(texturePath);

                if (fileNameWithoutExtension.Equals(fileName, StringComparison.OrdinalIgnoreCase))
                {
                    Texture2D icon = AssetDatabase.LoadAssetAtPath<Texture2D>(texturePath);
                    return icon;
                }
            }

            Debug.LogError($"Texture with the name '{fileName}' not found.");
            return null;
        }

        // Draws a labeled text field in the Unity Editor with a specified label and size.
        public static string DrawLabeledTextField(string label, string currentString, int labelSize, int textSize)
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label(label, GUILayout.Width(labelSize));
            string newText = GUILayout.TextField(currentString, GUILayout.Width(textSize));
            EditorGUILayout.EndHorizontal();
            return newText;
        }

        // Draws a labeled integer field in the Unity Editor with a specified label and size.
        public static int DrawLabeledIntField(string label, int currentString, int labelSize, int textSize)
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label(label, GUILayout.Width(labelSize));
            int newInt = EditorGUILayout.IntField(currentString, GUILayout.Width(textSize));
            EditorGUILayout.EndHorizontal();
            return newInt;
        }

        // Parses a line of text to extract language component data.
        public static LanguageComponentSave ParseLanguageComponent(string line)
        {
            var component = new LanguageComponentSave();

            string[] parts = line.Split(';');
            if (parts.Length < 1) return null;

            component.iD = int.Parse(parts[0].Replace("id:", ""));

            // Check for text and handle it if present.
            if (line.Contains("{"))
            {
                component.text = LanguageFileManager.ExtractTextBetweenBraces(line);
                component.textWrite = true;
            }

            string keylessLine = LanguageFileManager.RemoveTextBetweenBraces(line);
            component.componentType = LanguageFileManager.ExtractIntValue(keylessLine, "Type:");

            // Extract other component properties.
            component.alignment = ExtractIntValueOrDefault(keylessLine, "Ali:", ref component.alignmentWrite);
            component.fontSize = ExtractIntValueOrDefault(keylessLine, "S:", ref component.fontSizeWrite);
            component.fontListIndex = ExtractIntValueOrDefault(keylessLine, "Font:", ref component.fontListIndexWrite);

            return component;
        }

        // Parses a line of text to extract canvas save data.
        public static CanvasSave ParseCanvasSave(string line)
        {
            var component = new CanvasSave();

            string[] parts = line.Split(';');
            if (parts.Length < 1) return null;

            component.canvasID = int.Parse(parts[0].Replace("canvasID:", ""));
            component.json = line.Replace(parts[0] + ";", "");

            return component;
        }

        // Extracts an integer value from a line with a given prefix or returns a default value.
        public static int ExtractIntValueOrDefault(string line, string prefix, ref bool writeFlag)
        {
            if (line.Contains(prefix))
            {
                writeFlag = true;
                return LanguageFileManager.ExtractIntValue(line, prefix);
            }
            return 0;
        }

        // Draws a colored box around a content block in the Unity Editor.
        public static void DrawColoredBox(Action content, Color Color)
        {
            Rect rect = EditorGUILayout.BeginVertical();
            EditorGUI.DrawRect(rect, Color);
            content.Invoke();
            EditorGUILayout.EndVertical();
        }

        // Draws a button with an arrow icon and handles changes to an ID index.
        public static void DrawArrowButton(string label, ref int idIndex, int minID, int maxID, UnityEngine.Object undoContext, Action action)
        {
            bool isEnabled = (label == "<--") ? idIndex > minID : idIndex < maxID;
            GUI.enabled = isEnabled;

            if (GUILayout.Button(label, CreateCustomButtonStyle(15), GUILayout.Width(50)))
            {
                Undo.RecordObject(undoContext, "Change ID Index");
                action();
                EditorUtility.SetDirty(undoContext);
            }

            GUI.enabled = true;
        }

        // Adds default language components and canvas data to the given lists.
        public static void AddDefaultLanguageComponents(ref List<LanguageComponentSave> componentSave, ref List<CanvasSave> canvasSave)
        {
            // Default list of language components with predefined data.
            var languageDefaultIDs = new List<LanguageComponentSave>
            {
                new() { iD = -10, componentType = 2, text = "Hello World", fontSize = 0, fontListIndex = 1, textWrite = true, fontSizeWrite = true, fontListIndexWrite = true },
                new() { iD = -9, componentType = 4, text = "Test Language Script", textWrite = true },
                new() { iD = -8, componentType = 1, text = "Toggle", alignment = 0, fontSize = 0, fontListIndex = 1, textWrite = true, alignmentWrite = true, fontSizeWrite = true, fontListIndexWrite = true },
                new() { iD = -7, componentType = 1, text = "New Text", alignment = 0, fontSize = 0, fontListIndex = 1, textWrite = true, alignmentWrite = true, fontSizeWrite = true, fontListIndexWrite = true },
                new() { iD = -6, componentType = 3, text = "Enter text...", alignment = 0, fontSize = 0, fontListIndex = 1, textWrite = true, alignmentWrite = true, fontSizeWrite = true, fontListIndexWrite = true },
                new() { iD = -5, componentType = 5, text = "Test Language", textWrite = true },
                new() { iD = -4, componentType = 6, text = "Option C", textWrite = true },
                new() { iD = -3, componentType = 6, text = "Option B", textWrite = true },
                new() { iD = -2, componentType = 6, text = "Option A", alignment = 0, fontSize = 0, fontListIndex = 1, textWrite = true, alignmentWrite = true, fontSizeWrite = true, fontListIndexWrite = true },
                new() { iD = -1, componentType = 1, text = "Button", alignment = 0, fontSize = 0, fontListIndex = 1, textWrite = true, alignmentWrite = true, fontSizeWrite = true, fontListIndexWrite = true },
                new() { iD = 0, componentType = 1, text = "No language ID", alignment = 0, fontSize = 0, fontListIndex = 1, textWrite = true, alignmentWrite = true, fontSizeWrite = true, fontListIndexWrite = true },
            };

            // Default canvas save data.
            var canvasDefaultIDs = new List<CanvasSave>
            {
                new() { canvasID = 0, json = "{\"canvasData\":{\"canvasName\":\"Canvas\",\"canvasHierarchy\":[\"\",\"[Legacy]*[19020]*\",\"[Legacy]*[19020]*'/'Language Manager (LT) [Legacy]*[19566]*\",\"[Legacy]*[19020]*'/'Language Manager (LT) [Legacy]*[19566]*'/'Label*[19098]*\",\"[Legacy]*[19020]*'/'Language Manager (LT) [Legacy]*[19566]*'/'Arrow*[19612]*\",\"[Legacy]*[19020]*'/'Language Manager (LT) [Legacy]*[19566]*'/'Template*[19678]*\",\"[Legacy]*[19020]*'/'Language Manager (LT) [Legacy]*[19566]*'/'Template*[19678]*'/'Viewport*[19654]*\",\"[Legacy]*[19020]*'/'Language Manager (LT) [Legacy]*[19566]*'/'Template*[19678]*'/'Viewport*[19654]*'/'Content*[19496]*\",\"[Legacy]*[19020]*'/'Language Manager (LT) [Legacy]*[19566]*'/'Template*[19678]*'/'Viewport*[19654]*'/'Content*[19496]*'/'Item*[19684]*\",\"[Legacy]*[19020]*'/'Language Manager (LT) [Legacy]*[19566]*'/'Template*[19678]*'/'Viewport*[19654]*'/'Content*[19496]*'/'Item*[19684]*'/'Item Background*[19642]*\",\"[Legacy]*[19020]*'/'Language Manager (LT) [Legacy]*[19566]*'/'Template*[19678]*'/'Viewport*[19654]*'/'Content*[19496]*'/'Item*[19684]*'/'Item Checkmark*[19294]*\",\"[Legacy]*[19020]*'/'Language Manager (LT) [Legacy]*[19566]*'/'Template*[19678]*'/'Viewport*[19654]*'/'Content*[19496]*'/'Item*[19684]*'/'Item Label*[19338]*\",\"[Legacy]*[19020]*'/'Language Manager (LT) [Legacy]*[19566]*'/'Template*[19678]*'/'Scrollbar*[19226]*\",\"[Legacy]*[19020]*'/'Language Manager (LT) [Legacy]*[19566]*'/'Template*[19678]*'/'Scrollbar*[19226]*'/'Sliding Area*[19620]*\",\"[Legacy]*[19020]*'/'Language Manager (LT) [Legacy]*[19566]*'/'Template*[19678]*'/'Scrollbar*[19226]*'/'Sliding Area*[19620]*'/'Handle*[19356]*\",\"[Legacy]*[19020]*'/'Button (LT) [Legacy]*[19504]*\",\"[Legacy]*[19020]*'/'Button (LT) [Legacy]*[19504]*'/'Text (LT) [Legacy]*[19448]*\",\"[Legacy]*[19020]*'/'Dropdown (LT) [Legacy]*[19264]*\",\"[Legacy]*[19020]*'/'Dropdown (LT) [Legacy]*[19264]*'/'Label*[19616]*\",\"[Legacy]*[19020]*'/'Dropdown (LT) [Legacy]*[19264]*'/'Arrow*[19048]*\",\"[Legacy]*[19020]*'/'Dropdown (LT) [Legacy]*[19264]*'/'Template*[19466]*\",\"[Legacy]*[19020]*'/'Dropdown (LT) [Legacy]*[19264]*'/'Template*[19466]*'/'Viewport*[19334]*\",\"[Legacy]*[19020]*'/'Dropdown (LT) [Legacy]*[19264]*'/'Template*[19466]*'/'Viewport*[19334]*'/'Content*[19296]*\",\"[Legacy]*[19020]*'/'Dropdown (LT) [Legacy]*[19264]*'/'Template*[19466]*'/'Viewport*[19334]*'/'Content*[19296]*'/'Item*[19202]*\",\"[Legacy]*[19020]*'/'Dropdown (LT) [Legacy]*[19264]*'/'Template*[19466]*'/'Viewport*[19334]*'/'Content*[19296]*'/'Item*[19202]*'/'Item Background*[19200]*\",\"[Legacy]*[19020]*'/'Dropdown (LT) [Legacy]*[19264]*'/'Template*[19466]*'/'Viewport*[19334]*'/'Content*[19296]*'/'Item*[19202]*'/'Item Checkmark*[19136]*\",\"[Legacy]*[19020]*'/'Dropdown (LT) [Legacy]*[19264]*'/'Template*[19466]*'/'Viewport*[19334]*'/'Content*[19296]*'/'Item*[19202]*'/'Item Label*[19626]*\",\"[Legacy]*[19020]*'/'Dropdown (LT) [Legacy]*[19264]*'/'Template*[19466]*'/'Scrollbar*[19130]*\",\"[Legacy]*[19020]*'/'Dropdown (LT) [Legacy]*[19264]*'/'Template*[19466]*'/'Scrollbar*[19130]*'/'Sliding Area*[19422]*\",\"[Legacy]*[19020]*'/'Dropdown (LT) [Legacy]*[19264]*'/'Template*[19466]*'/'Scrollbar*[19130]*'/'Sliding Area*[19422]*'/'Handle*[19312]*\",\"[Legacy]*[19020]*'/'InputField (LT) [Legacy]*[19478]*\",\"[Legacy]*[19020]*'/'InputField (LT) [Legacy]*[19478]*'/'Placeholder*[19056]*\",\"[Legacy]*[19020]*'/'InputField (LT) [Legacy]*[19478]*'/'Text (Legacy)*[19106]*\",\"[Legacy]*[19020]*'/'Text (LT) [Legacy]*[19454]*\",\"[Legacy]*[19020]*'/'Toggle (LT) [Legacy]*[19450]*\",\"[Legacy]*[19020]*'/'Toggle (LT) [Legacy]*[19450]*'/'Background*[19470]*\",\"[Legacy]*[19020]*'/'Toggle (LT) [Legacy]*[19450]*'/'Background*[19470]*'/'Checkmark*[19164]*\",\"[Legacy]*[19020]*'/'Toggle (LT) [Legacy]*[19450]*'/'Label*[19432]*\",\"[TMP]*[18996]*\",\"[TMP]*[18996]*'/'Language Manager (LT) [TMP]*[19446]*\",\"[TMP]*[18996]*'/'Language Manager (LT) [TMP]*[19446]*'/'Label*[19644]*\",\"[TMP]*[18996]*'/'Language Manager (LT) [TMP]*[19446]*'/'Arrow*[19228]*\",\"[TMP]*[18996]*'/'Language Manager (LT) [TMP]*[19446]*'/'Template*[19552]*\",\"[TMP]*[18996]*'/'Language Manager (LT) [TMP]*[19446]*'/'Template*[19552]*'/'Viewport*[19522]*\",\"[TMP]*[18996]*'/'Language Manager (LT) [TMP]*[19446]*'/'Template*[19552]*'/'Viewport*[19522]*'/'Content*[19166]*\",\"[TMP]*[18996]*'/'Language Manager (LT) [TMP]*[19446]*'/'Template*[19552]*'/'Viewport*[19522]*'/'Content*[19166]*'/'Item*[19646]*\",\"[TMP]*[18996]*'/'Language Manager (LT) [TMP]*[19446]*'/'Template*[19552]*'/'Viewport*[19522]*'/'Content*[19166]*'/'Item*[19646]*'/'Item Background*[19172]*\",\"[TMP]*[18996]*'/'Language Manager (LT) [TMP]*[19446]*'/'Template*[19552]*'/'Viewport*[19522]*'/'Content*[19166]*'/'Item*[19646]*'/'Item Checkmark*[19494]*\",\"[TMP]*[18996]*'/'Language Manager (LT) [TMP]*[19446]*'/'Template*[19552]*'/'Viewport*[19522]*'/'Content*[19166]*'/'Item*[19646]*'/'Item Label*[19146]*\",\"[TMP]*[18996]*'/'Language Manager (LT) [TMP]*[19446]*'/'Template*[19552]*'/'Scrollbar*[19564]*\",\"[TMP]*[18996]*'/'Language Manager (LT) [TMP]*[19446]*'/'Template*[19552]*'/'Scrollbar*[19564]*'/'Sliding Area*[19262]*\",\"[TMP]*[18996]*'/'Language Manager (LT) [TMP]*[19446]*'/'Template*[19552]*'/'Scrollbar*[19564]*'/'Sliding Area*[19262]*'/'Handle*[19462]*\",\"[TMP]*[18996]*'/'Button (LT) [TMP]*[19638]*\",\"[TMP]*[18996]*'/'Button (LT) [TMP]*[19638]*'/'Text (LT) [TMP]*[19460]*\",\"[TMP]*[18996]*'/'Dropdown (LT) [TMP]*[19434]*\",\"[TMP]*[18996]*'/'Dropdown (LT) [TMP]*[19434]*'/'Label*[19314]*\",\"[TMP]*[18996]*'/'Dropdown (LT) [TMP]*[19434]*'/'Arrow*[19574]*\",\"[TMP]*[18996]*'/'Dropdown (LT) [TMP]*[19434]*'/'Template*[19418]*\",\"[TMP]*[18996]*'/'Dropdown (LT) [TMP]*[19434]*'/'Template*[19418]*'/'Viewport*[19608]*\",\"[TMP]*[18996]*'/'Dropdown (LT) [TMP]*[19434]*'/'Template*[19418]*'/'Viewport*[19608]*'/'Content*[19704]*\",\"[TMP]*[18996]*'/'Dropdown (LT) [TMP]*[19434]*'/'Template*[19418]*'/'Viewport*[19608]*'/'Content*[19704]*'/'Item*[19286]*\",\"[TMP]*[18996]*'/'Dropdown (LT) [TMP]*[19434]*'/'Template*[19418]*'/'Viewport*[19608]*'/'Content*[19704]*'/'Item*[19286]*'/'Item Background*[19358]*\",\"[TMP]*[18996]*'/'Dropdown (LT) [TMP]*[19434]*'/'Template*[19418]*'/'Viewport*[19608]*'/'Content*[19704]*'/'Item*[19286]*'/'Item Checkmark*[19640]*\",\"[TMP]*[18996]*'/'Dropdown (LT) [TMP]*[19434]*'/'Template*[19418]*'/'Viewport*[19608]*'/'Content*[19704]*'/'Item*[19286]*'/'Item Label*[19702]*\",\"[TMP]*[18996]*'/'Dropdown (LT) [TMP]*[19434]*'/'Template*[19418]*'/'Scrollbar*[19082]*\",\"[TMP]*[18996]*'/'Dropdown (LT) [TMP]*[19434]*'/'Template*[19418]*'/'Scrollbar*[19082]*'/'Sliding Area*[19104]*\",\"[TMP]*[18996]*'/'Dropdown (LT) [TMP]*[19434]*'/'Template*[19418]*'/'Scrollbar*[19082]*'/'Sliding Area*[19104]*'/'Handle*[19336]*\",\"[TMP]*[18996]*'/'InputField (LT) [TMP]*[19430]*\",\"[TMP]*[18996]*'/'InputField (LT) [TMP]*[19430]*'/'Text Area*[19592]*\",\"[TMP]*[18996]*'/'InputField (LT) [TMP]*[19430]*'/'Text Area*[19592]*'/'Placeholder*[19404]*\",\"[TMP]*[18996]*'/'InputField (LT) [TMP]*[19430]*'/'Text Area*[19592]*'/'Text*[19438]*\",\"[TMP]*[18996]*'/'Text (LT) [TMP]*[19568]*\",\"[TMP]*[18996]*'/'Toggle (LT) [TMP]*[19190]*\",\"[TMP]*[18996]*'/'Toggle (LT) [TMP]*[19190]*'/'Background*[19382]*\",\"[TMP]*[18996]*'/'Toggle (LT) [TMP]*[19190]*'/'Background*[19382]*'/'Checkmark*[19528]*\",\"[TMP]*[18996]*'/'Toggle (LT) [TMP]*[19190]*'/'Label*[19218]*\",\"Image (LT)*[19518]*\",\"RawImage (LT)*[19244]*\"],\"localRotation\":{\"x\":0.0,\"y\":0.0,\"z\":0.0,\"w\":1.0},\"localPosition\":{\"x\":960.0,\"y\":540.0,\"z\":0.0},\"localScale\":{\"x\":2.4000000953674318,\"y\":2.4000000953674318,\"z\":2.4000000953674318},\"anchorMin\":{\"x\":0.0,\"y\":0.0},\"anchorMax\":{\"x\":0.0,\"y\":0.0},\"anchoredPosition\":{\"x\":960.0,\"y\":540.0},\"sizeDelta\":{\"x\":799.9999389648438,\"y\":449.9999694824219},\"pivot\":{\"x\":0.5,\"y\":0.5},\"uiScaleMode\":1,\"referencePixelsPerUnit\":100.0,\"scaleFactor\":1.0,\"referenceResolution\":{\"x\":800.0,\"y\":600.0},\"screenMatchMode\":0,\"matchWidthOrHeight\":0.0,\"physicalUnit\":3,\"fallbackScreenDPI\":96.0,\"defaultSpriteDPI\":96.0,\"dynamicPixelsPerUnit\":1.0,\"presetInfoIsWorld\":false},\"savedCanvasData\":[{\"gameObjectName\":\"[Legacy]\",\"gameObjectEnable\":true,\"rectTransform\":{\"instanceID\":19020},\"instanceID\":19020,\"localRotation\":{\"x\":0.0,\"y\":0.0,\"z\":0.0,\"w\":1.0},\"localPosition\":{\"x\":0.0,\"y\":0.0,\"z\":0.0},\"localScale\":{\"x\":1.0,\"y\":1.0,\"z\":1.0},\"anchorMin\":{\"x\":0.0,\"y\":0.0},\"anchorMax\":{\"x\":1.0,\"y\":1.0},\"anchoredPosition\":{\"x\":0.0,\"y\":0.0},\"sizeDelta\":{\"x\":0.0,\"y\":0.0},\"pivot\":{\"x\":0.5,\"y\":0.5}},{\"gameObjectName\":\"Language Manager (LT) [Legacy]\",\"gameObjectEnable\":true,\"rectTransform\":{\"instanceID\":19566},\"instanceID\":19566,\"localRotation\":{\"x\":0.0,\"y\":0.0,\"z\":0.0,\"w\":1.0},\"localPosition\":{\"x\":230.0,\"y\":135.0,\"z\":0.0},\"localScale\":{\"x\":1.0,\"y\":1.0,\"z\":1.0},\"anchorMin\":{\"x\":0.5,\"y\":0.5},\"anchorMax\":{\"x\":0.5,\"y\":0.5},\"anchoredPosition\":{\"x\":230.0,\"y\":135.0},\"sizeDelta\":{\"x\":220.0,\"y\":30.0},\"pivot\":{\"x\":0.5,\"y\":0.5}},{\"gameObjectName\":\"Label\",\"gameObjectEnable\":true,\"rectTransform\":{\"instanceID\":19098},\"instanceID\":19098,\"localRotation\":{\"x\":0.0,\"y\":0.0,\"z\":0.0,\"w\":1.0},\"localPosition\":{\"x\":-7.5,\"y\":-0.5,\"z\":0.0},\"localScale\":{\"x\":1.0,\"y\":1.0,\"z\":1.0},\"anchorMin\":{\"x\":0.0,\"y\":0.0},\"anchorMax\":{\"x\":1.0,\"y\":1.0},\"anchoredPosition\":{\"x\":-7.5,\"y\":-0.5},\"sizeDelta\":{\"x\":-35.0,\"y\":-13.0},\"pivot\":{\"x\":0.5,\"y\":0.5}},{\"gameObjectName\":\"Arrow\",\"gameObjectEnable\":true,\"rectTransform\":{\"instanceID\":19612},\"instanceID\":19612,\"localRotation\":{\"x\":0.0,\"y\":0.0,\"z\":0.0,\"w\":1.0},\"localPosition\":{\"x\":95.0,\"y\":0.0,\"z\":0.0},\"localScale\":{\"x\":1.0,\"y\":1.0,\"z\":1.0},\"anchorMin\":{\"x\":1.0,\"y\":0.5},\"anchorMax\":{\"x\":1.0,\"y\":0.5},\"anchoredPosition\":{\"x\":-15.0,\"y\":0.0},\"sizeDelta\":{\"x\":20.0,\"y\":20.0},\"pivot\":{\"x\":0.5,\"y\":0.5}},{\"gameObjectName\":\"Template\",\"gameObjectEnable\":false,\"rectTransform\":{\"instanceID\":19678},\"instanceID\":19678,\"localRotation\":{\"x\":0.0,\"y\":0.0,\"z\":0.0,\"w\":1.0},\"localPosition\":{\"x\":0.0,\"y\":-13.0,\"z\":0.0},\"localScale\":{\"x\":1.0,\"y\":1.0,\"z\":1.0},\"anchorMin\":{\"x\":0.0,\"y\":0.0},\"anchorMax\":{\"x\":1.0,\"y\":0.0},\"anchoredPosition\":{\"x\":0.0,\"y\":2.0},\"sizeDelta\":{\"x\":0.0,\"y\":150.0},\"pivot\":{\"x\":0.5,\"y\":1.0}},{\"gameObjectName\":\"Viewport\",\"gameObjectEnable\":true,\"rectTransform\":{\"instanceID\":19654},\"instanceID\":19654,\"localRotation\":{\"x\":0.0,\"y\":0.0,\"z\":0.0,\"w\":1.0},\"localPosition\":{\"x\":-110.0,\"y\":0.0,\"z\":0.0},\"localScale\":{\"x\":1.0,\"y\":1.0,\"z\":1.0},\"anchorMin\":{\"x\":0.0,\"y\":0.0},\"anchorMax\":{\"x\":1.0,\"y\":1.0},\"anchoredPosition\":{\"x\":0.0,\"y\":0.0},\"sizeDelta\":{\"x\":-18.0,\"y\":0.0},\"pivot\":{\"x\":0.0,\"y\":1.0}},{\"gameObjectName\":\"Content\",\"gameObjectEnable\":true,\"rectTransform\":{\"instanceID\":19496},\"instanceID\":19496,\"localRotation\":{\"x\":0.0,\"y\":0.0,\"z\":0.0,\"w\":1.0},\"localPosition\":{\"x\":101.0,\"y\":0.0,\"z\":0.0},\"localScale\":{\"x\":1.0,\"y\":1.0,\"z\":1.0},\"anchorMin\":{\"x\":0.0,\"y\":1.0},\"anchorMax\":{\"x\":1.0,\"y\":1.0},\"anchoredPosition\":{\"x\":0.0,\"y\":0.0},\"sizeDelta\":{\"x\":0.0,\"y\":28.0},\"pivot\":{\"x\":0.5,\"y\":1.0}},{\"gameObjectName\":\"Item\",\"gameObjectEnable\":true,\"rectTransform\":{\"instanceID\":19684},\"instanceID\":19684,\"localRotation\":{\"x\":0.0,\"y\":0.0,\"z\":0.0,\"w\":1.0},\"localPosition\":{\"x\":0.0,\"y\":-14.0,\"z\":0.0},\"localScale\":{\"x\":1.0,\"y\":1.0,\"z\":1.0},\"anchorMin\":{\"x\":0.0,\"y\":0.5},\"anchorMax\":{\"x\":1.0,\"y\":0.5},\"anchoredPosition\":{\"x\":0.0,\"y\":0.0},\"sizeDelta\":{\"x\":0.0,\"y\":20.0},\"pivot\":{\"x\":0.5,\"y\":0.5}},{\"gameObjectName\":\"Item Background\",\"gameObjectEnable\":true,\"rectTransform\":{\"instanceID\":19642},\"instanceID\":19642,\"localRotation\":{\"x\":0.0,\"y\":0.0,\"z\":0.0,\"w\":1.0},\"localPosition\":{\"x\":0.0,\"y\":0.0,\"z\":0.0},\"localScale\":{\"x\":1.0,\"y\":1.0,\"z\":1.0},\"anchorMin\":{\"x\":0.0,\"y\":0.0},\"anchorMax\":{\"x\":1.0,\"y\":1.0},\"anchoredPosition\":{\"x\":0.0,\"y\":0.0},\"sizeDelta\":{\"x\":0.0,\"y\":0.0},\"pivot\":{\"x\":0.5,\"y\":0.5}},{\"gameObjectName\":\"Item Checkmark\",\"gameObjectEnable\":true,\"rectTransform\":{\"instanceID\":19294},\"instanceID\":19294,\"localRotation\":{\"x\":0.0,\"y\":0.0,\"z\":0.0,\"w\":1.0},\"localPosition\":{\"x\":-91.0,\"y\":0.0,\"z\":0.0},\"localScale\":{\"x\":1.0,\"y\":1.0,\"z\":1.0},\"anchorMin\":{\"x\":0.0,\"y\":0.5},\"anchorMax\":{\"x\":0.0,\"y\":0.5},\"anchoredPosition\":{\"x\":10.0,\"y\":0.0},\"sizeDelta\":{\"x\":20.0,\"y\":20.0},\"pivot\":{\"x\":0.5,\"y\":0.5}},{\"gameObjectName\":\"Item Label\",\"gameObjectEnable\":true,\"rectTransform\":{\"instanceID\":19338},\"instanceID\":19338,\"localRotation\":{\"x\":0.0,\"y\":0.0,\"z\":0.0,\"w\":1.0},\"localPosition\":{\"x\":5.0,\"y\":-0.5,\"z\":0.0},\"localScale\":{\"x\":1.0,\"y\":1.0,\"z\":1.0},\"anchorMin\":{\"x\":0.0,\"y\":0.0},\"anchorMax\":{\"x\":1.0,\"y\":1.0},\"anchoredPosition\":{\"x\":5.0,\"y\":-0.5},\"sizeDelta\":{\"x\":-30.0,\"y\":-3.0},\"pivot\":{\"x\":0.5,\"y\":0.5}},{\"gameObjectName\":\"Scrollbar\",\"gameObjectEnable\":true,\"rectTransform\":{\"instanceID\":19226},\"instanceID\":19226,\"localRotation\":{\"x\":0.0,\"y\":0.0,\"z\":0.0,\"w\":1.0},\"localPosition\":{\"x\":110.0,\"y\":0.0,\"z\":0.0},\"localScale\":{\"x\":1.0,\"y\":1.0,\"z\":1.0},\"anchorMin\":{\"x\":1.0,\"y\":0.0},\"anchorMax\":{\"x\":1.0,\"y\":1.0},\"anchoredPosition\":{\"x\":0.0,\"y\":0.0},\"sizeDelta\":{\"x\":20.0,\"y\":0.0},\"pivot\":{\"x\":1.0,\"y\":1.0}},{\"gameObjectName\":\"Sliding Area\",\"gameObjectEnable\":true,\"rectTransform\":{\"instanceID\":19620},\"instanceID\":19620,\"localRotation\":{\"x\":0.0,\"y\":0.0,\"z\":0.0,\"w\":1.0},\"localPosition\":{\"x\":-10.0,\"y\":-75.0,\"z\":0.0},\"localScale\":{\"x\":1.0,\"y\":1.0,\"z\":1.0},\"anchorMin\":{\"x\":0.0,\"y\":0.0},\"anchorMax\":{\"x\":1.0,\"y\":1.0},\"anchoredPosition\":{\"x\":0.0,\"y\":0.0},\"sizeDelta\":{\"x\":-20.0,\"y\":-20.0},\"pivot\":{\"x\":0.5,\"y\":0.5}},{\"gameObjectName\":\"Handle\",\"gameObjectEnable\":true,\"rectTransform\":{\"instanceID\":19356},\"instanceID\":19356,\"localRotation\":{\"x\":0.0,\"y\":0.0,\"z\":0.0,\"w\":1.0},\"localPosition\":{\"x\":0.0,\"y\":-52.0,\"z\":0.0},\"localScale\":{\"x\":1.0,\"y\":1.0,\"z\":1.0},\"anchorMin\":{\"x\":0.0,\"y\":0.0},\"anchorMax\":{\"x\":1.0,\"y\":0.20000000298023225},\"anchoredPosition\":{\"x\":0.0,\"y\":0.0},\"sizeDelta\":{\"x\":20.0,\"y\":20.0},\"pivot\":{\"x\":0.5,\"y\":0.5}},{\"gameObjectName\":\"Button (LT) [Legacy]\",\"gameObjectEnable\":true,\"rectTransform\":{\"instanceID\":19504},\"instanceID\":19504,\"localRotation\":{\"x\":0.0,\"y\":0.0,\"z\":0.0,\"w\":1.0},\"localPosition\":{\"x\":260.0,\"y\":85.0,\"z\":0.0},\"localScale\":{\"x\":1.0,\"y\":1.0,\"z\":1.0},\"anchorMin\":{\"x\":0.5,\"y\":0.5},\"anchorMax\":{\"x\":0.5,\"y\":0.5},\"anchoredPosition\":{\"x\":260.0,\"y\":85.0},\"sizeDelta\":{\"x\":160.0,\"y\":30.0},\"pivot\":{\"x\":0.5,\"y\":0.5}},{\"gameObjectName\":\"Text (LT) [Legacy]\",\"gameObjectEnable\":true,\"rectTransform\":{\"instanceID\":19448},\"instanceID\":19448,\"localRotation\":{\"x\":0.0,\"y\":0.0,\"z\":0.0,\"w\":1.0},\"localPosition\":{\"x\":0.0,\"y\":0.0,\"z\":0.0},\"localScale\":{\"x\":1.0,\"y\":1.0,\"z\":1.0},\"anchorMin\":{\"x\":0.0,\"y\":0.0},\"anchorMax\":{\"x\":1.0,\"y\":1.0},\"anchoredPosition\":{\"x\":0.0,\"y\":0.0},\"sizeDelta\":{\"x\":0.0,\"y\":0.0},\"pivot\":{\"x\":0.5,\"y\":0.5}},{\"gameObjectName\":\"Dropdown (LT) [Legacy]\",\"gameObjectEnable\":true,\"rectTransform\":{\"instanceID\":19264},\"instanceID\":19264,\"localRotation\":{\"x\":0.0,\"y\":0.0,\"z\":0.0,\"w\":1.0},\"localPosition\":{\"x\":260.0,\"y\":30.0,\"z\":0.0},\"localScale\":{\"x\":1.0,\"y\":1.0,\"z\":1.0},\"anchorMin\":{\"x\":0.5,\"y\":0.5},\"anchorMax\":{\"x\":0.5,\"y\":0.5},\"anchoredPosition\":{\"x\":260.0,\"y\":30.0},\"sizeDelta\":{\"x\":160.0,\"y\":30.0},\"pivot\":{\"x\":0.5,\"y\":0.5}},{\"gameObjectName\":\"Label\",\"gameObjectEnable\":true,\"rectTransform\":{\"instanceID\":19616},\"instanceID\":19616,\"localRotation\":{\"x\":0.0,\"y\":0.0,\"z\":0.0,\"w\":1.0},\"localPosition\":{\"x\":-7.5,\"y\":-0.5,\"z\":0.0},\"localScale\":{\"x\":1.0,\"y\":1.0,\"z\":1.0},\"anchorMin\":{\"x\":0.0,\"y\":0.0},\"anchorMax\":{\"x\":1.0,\"y\":1.0},\"anchoredPosition\":{\"x\":-7.5,\"y\":-0.5},\"sizeDelta\":{\"x\":-35.0,\"y\":-13.0},\"pivot\":{\"x\":0.5,\"y\":0.5}},{\"gameObjectName\":\"Arrow\",\"gameObjectEnable\":true,\"rectTransform\":{\"instanceID\":19048},\"instanceID\":19048,\"localRotation\":{\"x\":0.0,\"y\":0.0,\"z\":0.0,\"w\":1.0},\"localPosition\":{\"x\":65.0,\"y\":0.0,\"z\":0.0},\"localScale\":{\"x\":1.0,\"y\":1.0,\"z\":1.0},\"anchorMin\":{\"x\":1.0,\"y\":0.5},\"anchorMax\":{\"x\":1.0,\"y\":0.5},\"anchoredPosition\":{\"x\":-15.0,\"y\":0.0},\"sizeDelta\":{\"x\":20.0,\"y\":20.0},\"pivot\":{\"x\":0.5,\"y\":0.5}},{\"gameObjectName\":\"Template\",\"gameObjectEnable\":false,\"rectTransform\":{\"instanceID\":19466},\"instanceID\":19466,\"localRotation\":{\"x\":0.0,\"y\":0.0,\"z\":0.0,\"w\":1.0},\"localPosition\":{\"x\":0.0,\"y\":-13.0,\"z\":0.0},\"localScale\":{\"x\":1.0,\"y\":1.0,\"z\":1.0},\"anchorMin\":{\"x\":0.0,\"y\":0.0},\"anchorMax\":{\"x\":1.0,\"y\":0.0},\"anchoredPosition\":{\"x\":0.0,\"y\":2.0},\"sizeDelta\":{\"x\":0.0,\"y\":150.0},\"pivot\":{\"x\":0.5,\"y\":1.0}},{\"gameObjectName\":\"Viewport\",\"gameObjectEnable\":true,\"rectTransform\":{\"instanceID\":19334},\"instanceID\":19334,\"localRotation\":{\"x\":0.0,\"y\":0.0,\"z\":0.0,\"w\":1.0},\"localPosition\":{\"x\":-80.0,\"y\":0.0,\"z\":0.0},\"localScale\":{\"x\":1.0,\"y\":1.0,\"z\":1.0},\"anchorMin\":{\"x\":0.0,\"y\":0.0},\"anchorMax\":{\"x\":1.0,\"y\":1.0},\"anchoredPosition\":{\"x\":0.0,\"y\":0.0},\"sizeDelta\":{\"x\":-18.0,\"y\":0.0},\"pivot\":{\"x\":0.0,\"y\":1.0}},{\"gameObjectName\":\"Content\",\"gameObjectEnable\":true,\"rectTransform\":{\"instanceID\":19296},\"instanceID\":19296,\"localRotation\":{\"x\":0.0,\"y\":0.0,\"z\":0.0,\"w\":1.0},\"localPosition\":{\"x\":71.0,\"y\":0.0,\"z\":0.0},\"localScale\":{\"x\":1.0,\"y\":1.0,\"z\":1.0},\"anchorMin\":{\"x\":0.0,\"y\":1.0},\"anchorMax\":{\"x\":1.0,\"y\":1.0},\"anchoredPosition\":{\"x\":0.0,\"y\":0.0},\"sizeDelta\":{\"x\":0.0,\"y\":28.0},\"pivot\":{\"x\":0.5,\"y\":1.0}},{\"gameObjectName\":\"Item\",\"gameObjectEnable\":true,\"rectTransform\":{\"instanceID\":19202},\"instanceID\":19202,\"localRotation\":{\"x\":0.0,\"y\":0.0,\"z\":0.0,\"w\":1.0},\"localPosition\":{\"x\":0.0,\"y\":-14.0,\"z\":0.0},\"localScale\":{\"x\":1.0,\"y\":1.0,\"z\":1.0},\"anchorMin\":{\"x\":0.0,\"y\":0.5},\"anchorMax\":{\"x\":1.0,\"y\":0.5},\"anchoredPosition\":{\"x\":0.0,\"y\":0.0},\"sizeDelta\":{\"x\":0.0,\"y\":20.0},\"pivot\":{\"x\":0.5,\"y\":0.5}},{\"gameObjectName\":\"Item Background\",\"gameObjectEnable\":true,\"rectTransform\":{\"instanceID\":19200},\"instanceID\":19200,\"localRotation\":{\"x\":0.0,\"y\":0.0,\"z\":0.0,\"w\":1.0},\"localPosition\":{\"x\":0.0,\"y\":0.0,\"z\":0.0},\"localScale\":{\"x\":1.0,\"y\":1.0,\"z\":1.0},\"anchorMin\":{\"x\":0.0,\"y\":0.0},\"anchorMax\":{\"x\":1.0,\"y\":1.0},\"anchoredPosition\":{\"x\":0.0,\"y\":0.0},\"sizeDelta\":{\"x\":0.0,\"y\":0.0},\"pivot\":{\"x\":0.5,\"y\":0.5}},{\"gameObjectName\":\"Item Checkmark\",\"gameObjectEnable\":true,\"rectTransform\":{\"instanceID\":19136},\"instanceID\":19136,\"localRotation\":{\"x\":0.0,\"y\":0.0,\"z\":0.0,\"w\":1.0},\"localPosition\":{\"x\":-61.0,\"y\":0.0,\"z\":0.0},\"localScale\":{\"x\":1.0,\"y\":1.0,\"z\":1.0},\"anchorMin\":{\"x\":0.0,\"y\":0.5},\"anchorMax\":{\"x\":0.0,\"y\":0.5},\"anchoredPosition\":{\"x\":10.0,\"y\":0.0},\"sizeDelta\":{\"x\":20.0,\"y\":20.0},\"pivot\":{\"x\":0.5,\"y\":0.5}},{\"gameObjectName\":\"Item Label\",\"gameObjectEnable\":true,\"rectTransform\":{\"instanceID\":19626},\"instanceID\":19626,\"localRotation\":{\"x\":0.0,\"y\":0.0,\"z\":0.0,\"w\":1.0},\"localPosition\":{\"x\":5.0,\"y\":-0.5,\"z\":0.0},\"localScale\":{\"x\":1.0,\"y\":1.0,\"z\":1.0},\"anchorMin\":{\"x\":0.0,\"y\":0.0},\"anchorMax\":{\"x\":1.0,\"y\":1.0},\"anchoredPosition\":{\"x\":5.0,\"y\":-0.5},\"sizeDelta\":{\"x\":-30.0,\"y\":-3.0},\"pivot\":{\"x\":0.5,\"y\":0.5}},{\"gameObjectName\":\"Scrollbar\",\"gameObjectEnable\":true,\"rectTransform\":{\"instanceID\":19130},\"instanceID\":19130,\"localRotation\":{\"x\":0.0,\"y\":0.0,\"z\":0.0,\"w\":1.0},\"localPosition\":{\"x\":80.0,\"y\":0.0,\"z\":0.0},\"localScale\":{\"x\":1.0,\"y\":1.0,\"z\":1.0},\"anchorMin\":{\"x\":1.0,\"y\":0.0},\"anchorMax\":{\"x\":1.0,\"y\":1.0},\"anchoredPosition\":{\"x\":0.0,\"y\":0.0},\"sizeDelta\":{\"x\":20.0,\"y\":0.0},\"pivot\":{\"x\":1.0,\"y\":1.0}},{\"gameObjectName\":\"Sliding Area\",\"gameObjectEnable\":true,\"rectTransform\":{\"instanceID\":19422},\"instanceID\":19422,\"localRotation\":{\"x\":0.0,\"y\":0.0,\"z\":0.0,\"w\":1.0},\"localPosition\":{\"x\":-10.0,\"y\":-75.0,\"z\":0.0},\"localScale\":{\"x\":1.0,\"y\":1.0,\"z\":1.0},\"anchorMin\":{\"x\":0.0,\"y\":0.0},\"anchorMax\":{\"x\":1.0,\"y\":1.0},\"anchoredPosition\":{\"x\":0.0,\"y\":0.0},\"sizeDelta\":{\"x\":-20.0,\"y\":-20.0},\"pivot\":{\"x\":0.5,\"y\":0.5}},{\"gameObjectName\":\"Handle\",\"gameObjectEnable\":true,\"rectTransform\":{\"instanceID\":19312},\"instanceID\":19312,\"localRotation\":{\"x\":0.0,\"y\":0.0,\"z\":0.0,\"w\":1.0},\"localPosition\":{\"x\":0.0,\"y\":-52.0,\"z\":0.0},\"localScale\":{\"x\":1.0,\"y\":1.0,\"z\":1.0},\"anchorMin\":{\"x\":0.0,\"y\":0.0},\"anchorMax\":{\"x\":1.0,\"y\":0.20000000298023225},\"anchoredPosition\":{\"x\":0.0,\"y\":0.0},\"sizeDelta\":{\"x\":20.0,\"y\":20.0},\"pivot\":{\"x\":0.5,\"y\":0.5}},{\"gameObjectName\":\"InputField (LT) [Legacy]\",\"gameObjectEnable\":true,\"rectTransform\":{\"instanceID\":19478},\"instanceID\":19478,\"localRotation\":{\"x\":0.0,\"y\":0.0,\"z\":0.0,\"w\":1.0},\"localPosition\":{\"x\":260.0,\"y\":-20.0,\"z\":0.0},\"localScale\":{\"x\":1.0,\"y\":1.0,\"z\":1.0},\"anchorMin\":{\"x\":0.5,\"y\":0.5},\"anchorMax\":{\"x\":0.5,\"y\":0.5},\"anchoredPosition\":{\"x\":260.0,\"y\":-20.0},\"sizeDelta\":{\"x\":160.0,\"y\":30.0},\"pivot\":{\"x\":0.5,\"y\":0.5}},{\"gameObjectName\":\"Placeholder\",\"gameObjectEnable\":true,\"rectTransform\":{\"instanceID\":19056},\"instanceID\":19056,\"localRotation\":{\"x\":0.0,\"y\":0.0,\"z\":0.0,\"w\":1.0},\"localPosition\":{\"x\":0.0,\"y\":-0.5,\"z\":0.0},\"localScale\":{\"x\":1.0,\"y\":1.0,\"z\":1.0},\"anchorMin\":{\"x\":0.0,\"y\":0.0},\"anchorMax\":{\"x\":1.0,\"y\":1.0},\"anchoredPosition\":{\"x\":0.0,\"y\":-0.5},\"sizeDelta\":{\"x\":-20.0,\"y\":-13.0},\"pivot\":{\"x\":0.5,\"y\":0.5}},{\"gameObjectName\":\"Text (Legacy)\",\"gameObjectEnable\":true,\"rectTransform\":{\"instanceID\":19106},\"instanceID\":19106,\"localRotation\":{\"x\":0.0,\"y\":0.0,\"z\":0.0,\"w\":1.0},\"localPosition\":{\"x\":0.0,\"y\":-0.5,\"z\":0.0},\"localScale\":{\"x\":1.0,\"y\":1.0,\"z\":1.0},\"anchorMin\":{\"x\":0.0,\"y\":0.0},\"anchorMax\":{\"x\":1.0,\"y\":1.0},\"anchoredPosition\":{\"x\":0.0,\"y\":-0.5},\"sizeDelta\":{\"x\":-20.0,\"y\":-13.0},\"pivot\":{\"x\":0.5,\"y\":0.5}},{\"gameObjectName\":\"Text (LT) [Legacy]\",\"gameObjectEnable\":true,\"rectTransform\":{\"instanceID\":19454},\"instanceID\":19454,\"localRotation\":{\"x\":0.0,\"y\":0.0,\"z\":0.0,\"w\":1.0},\"localPosition\":{\"x\":260.0,\"y\":-70.0,\"z\":0.0},\"localScale\":{\"x\":1.0,\"y\":1.0,\"z\":1.0},\"anchorMin\":{\"x\":0.5,\"y\":0.5},\"anchorMax\":{\"x\":0.5,\"y\":0.5},\"anchoredPosition\":{\"x\":260.0,\"y\":-70.0},\"sizeDelta\":{\"x\":160.0,\"y\":30.0},\"pivot\":{\"x\":0.5,\"y\":0.5}},{\"gameObjectName\":\"Toggle (LT) [Legacy]\",\"gameObjectEnable\":true,\"rectTransform\":{\"instanceID\":19450},\"instanceID\":19450,\"localRotation\":{\"x\":0.0,\"y\":0.0,\"z\":0.0,\"w\":1.0},\"localPosition\":{\"x\":260.0,\"y\":-110.0,\"z\":0.0},\"localScale\":{\"x\":1.0,\"y\":1.0,\"z\":1.0},\"anchorMin\":{\"x\":0.5,\"y\":0.5},\"anchorMax\":{\"x\":0.5,\"y\":0.5},\"anchoredPosition\":{\"x\":260.0,\"y\":-110.0},\"sizeDelta\":{\"x\":160.0,\"y\":20.0},\"pivot\":{\"x\":0.5,\"y\":0.5}},{\"gameObjectName\":\"Background\",\"gameObjectEnable\":true,\"rectTransform\":{\"instanceID\":19470},\"instanceID\":19470,\"localRotation\":{\"x\":0.0,\"y\":0.0,\"z\":0.0,\"w\":1.0},\"localPosition\":{\"x\":-70.0,\"y\":0.0,\"z\":0.0},\"localScale\":{\"x\":1.0,\"y\":1.0,\"z\":1.0},\"anchorMin\":{\"x\":0.0,\"y\":1.0},\"anchorMax\":{\"x\":0.0,\"y\":1.0},\"anchoredPosition\":{\"x\":10.0,\"y\":-10.0},\"sizeDelta\":{\"x\":20.0,\"y\":20.0},\"pivot\":{\"x\":0.5,\"y\":0.5}},{\"gameObjectName\":\"Checkmark\",\"gameObjectEnable\":true,\"rectTransform\":{\"instanceID\":19164},\"instanceID\":19164,\"localRotation\":{\"x\":0.0,\"y\":0.0,\"z\":0.0,\"w\":1.0},\"localPosition\":{\"x\":0.0,\"y\":0.0,\"z\":0.0},\"localScale\":{\"x\":1.0,\"y\":1.0,\"z\":1.0},\"anchorMin\":{\"x\":0.5,\"y\":0.5},\"anchorMax\":{\"x\":0.5,\"y\":0.5},\"anchoredPosition\":{\"x\":0.0,\"y\":0.0},\"sizeDelta\":{\"x\":20.0,\"y\":20.0},\"pivot\":{\"x\":0.5,\"y\":0.5}},{\"gameObjectName\":\"Label\",\"gameObjectEnable\":true,\"rectTransform\":{\"instanceID\":19432},\"instanceID\":19432,\"localRotation\":{\"x\":0.0,\"y\":0.0,\"z\":0.0,\"w\":1.0},\"localPosition\":{\"x\":9.0,\"y\":-0.5,\"z\":0.0},\"localScale\":{\"x\":1.0,\"y\":1.0,\"z\":1.0},\"anchorMin\":{\"x\":0.0,\"y\":0.0},\"anchorMax\":{\"x\":1.0,\"y\":1.0},\"anchoredPosition\":{\"x\":9.0,\"y\":-0.5},\"sizeDelta\":{\"x\":-28.0,\"y\":-3.0},\"pivot\":{\"x\":0.5,\"y\":0.5}},{\"gameObjectName\":\"[TMP]\",\"gameObjectEnable\":true,\"rectTransform\":{\"instanceID\":18996},\"instanceID\":18996,\"localRotation\":{\"x\":0.0,\"y\":0.0,\"z\":0.0,\"w\":1.0},\"localPosition\":{\"x\":0.0,\"y\":0.0,\"z\":0.0},\"localScale\":{\"x\":1.0,\"y\":1.0,\"z\":1.0},\"anchorMin\":{\"x\":0.0,\"y\":0.0},\"anchorMax\":{\"x\":1.0,\"y\":1.0},\"anchoredPosition\":{\"x\":0.0,\"y\":0.0},\"sizeDelta\":{\"x\":0.0,\"y\":0.0},\"pivot\":{\"x\":0.5,\"y\":0.5}},{\"gameObjectName\":\"Language Manager (LT) [TMP]\",\"gameObjectEnable\":true,\"rectTransform\":{\"instanceID\":19446},\"instanceID\":19446,\"localRotation\":{\"x\":0.0,\"y\":0.0,\"z\":0.0,\"w\":1.0},\"localPosition\":{\"x\":-230.0,\"y\":135.0,\"z\":0.0},\"localScale\":{\"x\":1.0,\"y\":1.0,\"z\":1.0},\"anchorMin\":{\"x\":0.5,\"y\":0.5},\"anchorMax\":{\"x\":0.5,\"y\":0.5},\"anchoredPosition\":{\"x\":-230.0,\"y\":135.0},\"sizeDelta\":{\"x\":220.0,\"y\":30.0},\"pivot\":{\"x\":0.5,\"y\":0.5}},{\"gameObjectName\":\"Label\",\"gameObjectEnable\":true,\"rectTransform\":{\"instanceID\":19644},\"instanceID\":19644,\"localRotation\":{\"x\":0.0,\"y\":0.0,\"z\":0.0,\"w\":1.0},\"localPosition\":{\"x\":-7.5,\"y\":-0.5,\"z\":0.0},\"localScale\":{\"x\":1.0,\"y\":1.0,\"z\":1.0},\"anchorMin\":{\"x\":0.0,\"y\":0.0},\"anchorMax\":{\"x\":1.0,\"y\":1.0},\"anchoredPosition\":{\"x\":-7.5,\"y\":-0.5},\"sizeDelta\":{\"x\":-35.0,\"y\":-13.0},\"pivot\":{\"x\":0.5,\"y\":0.5}},{\"gameObjectName\":\"Arrow\",\"gameObjectEnable\":true,\"rectTransform\":{\"instanceID\":19228},\"instanceID\":19228,\"localRotation\":{\"x\":0.0,\"y\":0.0,\"z\":0.0,\"w\":1.0},\"localPosition\":{\"x\":95.0,\"y\":0.0,\"z\":0.0},\"localScale\":{\"x\":1.0,\"y\":1.0,\"z\":1.0},\"anchorMin\":{\"x\":1.0,\"y\":0.5},\"anchorMax\":{\"x\":1.0,\"y\":0.5},\"anchoredPosition\":{\"x\":-15.0,\"y\":0.0},\"sizeDelta\":{\"x\":20.0,\"y\":20.0},\"pivot\":{\"x\":0.5,\"y\":0.5}},{\"gameObjectName\":\"Template\",\"gameObjectEnable\":false,\"rectTransform\":{\"instanceID\":19552},\"instanceID\":19552,\"localRotation\":{\"x\":0.0,\"y\":0.0,\"z\":0.0,\"w\":1.0},\"localPosition\":{\"x\":0.0,\"y\":-13.0,\"z\":0.0},\"localScale\":{\"x\":1.0,\"y\":1.0,\"z\":1.0},\"anchorMin\":{\"x\":0.0,\"y\":0.0},\"anchorMax\":{\"x\":1.0,\"y\":0.0},\"anchoredPosition\":{\"x\":0.0,\"y\":2.0},\"sizeDelta\":{\"x\":0.0,\"y\":150.0},\"pivot\":{\"x\":0.5,\"y\":1.0}},{\"gameObjectName\":\"Viewport\",\"gameObjectEnable\":true,\"rectTransform\":{\"instanceID\":19522},\"instanceID\":19522,\"localRotation\":{\"x\":0.0,\"y\":0.0,\"z\":0.0,\"w\":1.0},\"localPosition\":{\"x\":-110.0,\"y\":0.0,\"z\":0.0},\"localScale\":{\"x\":1.0,\"y\":1.0,\"z\":1.0},\"anchorMin\":{\"x\":0.0,\"y\":0.0},\"anchorMax\":{\"x\":1.0,\"y\":1.0},\"anchoredPosition\":{\"x\":0.0,\"y\":0.0},\"sizeDelta\":{\"x\":-18.0,\"y\":0.0},\"pivot\":{\"x\":0.0,\"y\":1.0}},{\"gameObjectName\":\"Content\",\"gameObjectEnable\":true,\"rectTransform\":{\"instanceID\":19166},\"instanceID\":19166,\"localRotation\":{\"x\":0.0,\"y\":0.0,\"z\":0.0,\"w\":1.0},\"localPosition\":{\"x\":101.0,\"y\":0.0,\"z\":0.0},\"localScale\":{\"x\":1.0,\"y\":1.0,\"z\":1.0},\"anchorMin\":{\"x\":0.0,\"y\":1.0},\"anchorMax\":{\"x\":1.0,\"y\":1.0},\"anchoredPosition\":{\"x\":0.0,\"y\":0.0},\"sizeDelta\":{\"x\":0.0,\"y\":28.0},\"pivot\":{\"x\":0.5,\"y\":1.0}},{\"gameObjectName\":\"Item\",\"gameObjectEnable\":true,\"rectTransform\":{\"instanceID\":19646},\"instanceID\":19646,\"localRotation\":{\"x\":0.0,\"y\":0.0,\"z\":0.0,\"w\":1.0},\"localPosition\":{\"x\":0.0,\"y\":-14.0,\"z\":0.0},\"localScale\":{\"x\":1.0,\"y\":1.0,\"z\":1.0},\"anchorMin\":{\"x\":0.0,\"y\":0.5},\"anchorMax\":{\"x\":1.0,\"y\":0.5},\"anchoredPosition\":{\"x\":0.0,\"y\":0.0},\"sizeDelta\":{\"x\":0.0,\"y\":20.0},\"pivot\":{\"x\":0.5,\"y\":0.5}},{\"gameObjectName\":\"Item Background\",\"gameObjectEnable\":true,\"rectTransform\":{\"instanceID\":19172},\"instanceID\":19172,\"localRotation\":{\"x\":0.0,\"y\":0.0,\"z\":0.0,\"w\":1.0},\"localPosition\":{\"x\":0.0,\"y\":0.0,\"z\":0.0},\"localScale\":{\"x\":1.0,\"y\":1.0,\"z\":1.0},\"anchorMin\":{\"x\":0.0,\"y\":0.0},\"anchorMax\":{\"x\":1.0,\"y\":1.0},\"anchoredPosition\":{\"x\":0.0,\"y\":0.0},\"sizeDelta\":{\"x\":0.0,\"y\":0.0},\"pivot\":{\"x\":0.5,\"y\":0.5}},{\"gameObjectName\":\"Item Checkmark\",\"gameObjectEnable\":true,\"rectTransform\":{\"instanceID\":19494},\"instanceID\":19494,\"localRotation\":{\"x\":0.0,\"y\":0.0,\"z\":0.0,\"w\":1.0},\"localPosition\":{\"x\":-91.0,\"y\":0.0,\"z\":0.0},\"localScale\":{\"x\":1.0,\"y\":1.0,\"z\":1.0},\"anchorMin\":{\"x\":0.0,\"y\":0.5},\"anchorMax\":{\"x\":0.0,\"y\":0.5},\"anchoredPosition\":{\"x\":10.0,\"y\":0.0},\"sizeDelta\":{\"x\":20.0,\"y\":20.0},\"pivot\":{\"x\":0.5,\"y\":0.5}},{\"gameObjectName\":\"Item Label\",\"gameObjectEnable\":true,\"rectTransform\":{\"instanceID\":19146},\"instanceID\":19146,\"localRotation\":{\"x\":0.0,\"y\":0.0,\"z\":0.0,\"w\":1.0},\"localPosition\":{\"x\":5.0,\"y\":-0.5,\"z\":0.0},\"localScale\":{\"x\":1.0,\"y\":1.0,\"z\":1.0},\"anchorMin\":{\"x\":0.0,\"y\":0.0},\"anchorMax\":{\"x\":1.0,\"y\":1.0},\"anchoredPosition\":{\"x\":5.0,\"y\":-0.5},\"sizeDelta\":{\"x\":-30.0,\"y\":-3.0},\"pivot\":{\"x\":0.5,\"y\":0.5}},{\"gameObjectName\":\"Scrollbar\",\"gameObjectEnable\":true,\"rectTransform\":{\"instanceID\":19564},\"instanceID\":19564,\"localRotation\":{\"x\":0.0,\"y\":0.0,\"z\":0.0,\"w\":1.0},\"localPosition\":{\"x\":110.0,\"y\":0.0,\"z\":0.0},\"localScale\":{\"x\":1.0,\"y\":1.0,\"z\":1.0},\"anchorMin\":{\"x\":1.0,\"y\":0.0},\"anchorMax\":{\"x\":1.0,\"y\":1.0},\"anchoredPosition\":{\"x\":0.0,\"y\":0.0},\"sizeDelta\":{\"x\":20.0,\"y\":0.0},\"pivot\":{\"x\":1.0,\"y\":1.0}},{\"gameObjectName\":\"Sliding Area\",\"gameObjectEnable\":true,\"rectTransform\":{\"instanceID\":19262},\"instanceID\":19262,\"localRotation\":{\"x\":0.0,\"y\":0.0,\"z\":0.0,\"w\":1.0},\"localPosition\":{\"x\":-10.0,\"y\":-75.0,\"z\":0.0},\"localScale\":{\"x\":1.0,\"y\":1.0,\"z\":1.0},\"anchorMin\":{\"x\":0.0,\"y\":0.0},\"anchorMax\":{\"x\":1.0,\"y\":1.0},\"anchoredPosition\":{\"x\":0.0,\"y\":0.0},\"sizeDelta\":{\"x\":-20.0,\"y\":-20.0},\"pivot\":{\"x\":0.5,\"y\":0.5}},{\"gameObjectName\":\"Handle\",\"gameObjectEnable\":true,\"rectTransform\":{\"instanceID\":19462},\"instanceID\":19462,\"localRotation\":{\"x\":0.0,\"y\":0.0,\"z\":0.0,\"w\":1.0},\"localPosition\":{\"x\":0.0,\"y\":-52.0,\"z\":0.0},\"localScale\":{\"x\":1.0,\"y\":1.0,\"z\":1.0},\"anchorMin\":{\"x\":0.0,\"y\":0.0},\"anchorMax\":{\"x\":1.0,\"y\":0.20000000298023225},\"anchoredPosition\":{\"x\":0.0,\"y\":0.0},\"sizeDelta\":{\"x\":20.0,\"y\":20.0},\"pivot\":{\"x\":0.5,\"y\":0.5}},{\"gameObjectName\":\"Button (LT) [TMP]\",\"gameObjectEnable\":true,\"rectTransform\":{\"instanceID\":19638},\"instanceID\":19638,\"localRotation\":{\"x\":0.0,\"y\":0.0,\"z\":0.0,\"w\":1.0},\"localPosition\":{\"x\":-260.0,\"y\":85.0,\"z\":0.0},\"localScale\":{\"x\":1.0,\"y\":1.0,\"z\":1.0},\"anchorMin\":{\"x\":0.5,\"y\":0.5},\"anchorMax\":{\"x\":0.5,\"y\":0.5},\"anchoredPosition\":{\"x\":-260.0,\"y\":85.0},\"sizeDelta\":{\"x\":160.0,\"y\":30.0},\"pivot\":{\"x\":0.5,\"y\":0.5}},{\"gameObjectName\":\"Text (LT) [TMP]\",\"gameObjectEnable\":true,\"rectTransform\":{\"instanceID\":19460},\"instanceID\":19460,\"localRotation\":{\"x\":0.0,\"y\":0.0,\"z\":0.0,\"w\":1.0},\"localPosition\":{\"x\":0.0,\"y\":0.0,\"z\":0.0},\"localScale\":{\"x\":1.0,\"y\":1.0,\"z\":1.0},\"anchorMin\":{\"x\":0.0,\"y\":0.0},\"anchorMax\":{\"x\":1.0,\"y\":1.0},\"anchoredPosition\":{\"x\":0.0,\"y\":0.0},\"sizeDelta\":{\"x\":0.0,\"y\":0.0},\"pivot\":{\"x\":0.5,\"y\":0.5}},{\"gameObjectName\":\"Dropdown (LT) [TMP]\",\"gameObjectEnable\":true,\"rectTransform\":{\"instanceID\":19434},\"instanceID\":19434,\"localRotation\":{\"x\":0.0,\"y\":0.0,\"z\":0.0,\"w\":1.0},\"localPosition\":{\"x\":-260.0,\"y\":30.0,\"z\":0.0},\"localScale\":{\"x\":1.0,\"y\":1.0,\"z\":1.0},\"anchorMin\":{\"x\":0.5,\"y\":0.5},\"anchorMax\":{\"x\":0.5,\"y\":0.5},\"anchoredPosition\":{\"x\":-260.0,\"y\":30.0},\"sizeDelta\":{\"x\":160.0,\"y\":30.0},\"pivot\":{\"x\":0.5,\"y\":0.5}},{\"gameObjectName\":\"Label\",\"gameObjectEnable\":true,\"rectTransform\":{\"instanceID\":19314},\"instanceID\":19314,\"localRotation\":{\"x\":0.0,\"y\":0.0,\"z\":0.0,\"w\":1.0},\"localPosition\":{\"x\":-7.5,\"y\":-0.5,\"z\":0.0},\"localScale\":{\"x\":1.0,\"y\":1.0,\"z\":1.0},\"anchorMin\":{\"x\":0.0,\"y\":0.0},\"anchorMax\":{\"x\":1.0,\"y\":1.0},\"anchoredPosition\":{\"x\":-7.5,\"y\":-0.5},\"sizeDelta\":{\"x\":-35.0,\"y\":-13.0},\"pivot\":{\"x\":0.5,\"y\":0.5}},{\"gameObjectName\":\"Arrow\",\"gameObjectEnable\":true,\"rectTransform\":{\"instanceID\":19574},\"instanceID\":19574,\"localRotation\":{\"x\":0.0,\"y\":0.0,\"z\":0.0,\"w\":1.0},\"localPosition\":{\"x\":65.0,\"y\":0.0,\"z\":0.0},\"localScale\":{\"x\":1.0,\"y\":1.0,\"z\":1.0},\"anchorMin\":{\"x\":1.0,\"y\":0.5},\"anchorMax\":{\"x\":1.0,\"y\":0.5},\"anchoredPosition\":{\"x\":-15.0,\"y\":0.0},\"sizeDelta\":{\"x\":20.0,\"y\":20.0},\"pivot\":{\"x\":0.5,\"y\":0.5}},{\"gameObjectName\":\"Template\",\"gameObjectEnable\":false,\"rectTransform\":{\"instanceID\":19418},\"instanceID\":19418,\"localRotation\":{\"x\":0.0,\"y\":0.0,\"z\":0.0,\"w\":1.0},\"localPosition\":{\"x\":0.0,\"y\":-13.0,\"z\":0.0},\"localScale\":{\"x\":1.0,\"y\":1.0,\"z\":1.0},\"anchorMin\":{\"x\":0.0,\"y\":0.0},\"anchorMax\":{\"x\":1.0,\"y\":0.0},\"anchoredPosition\":{\"x\":0.0,\"y\":2.0},\"sizeDelta\":{\"x\":0.0,\"y\":150.0},\"pivot\":{\"x\":0.5,\"y\":1.0}},{\"gameObjectName\":\"Viewport\",\"gameObjectEnable\":true,\"rectTransform\":{\"instanceID\":19608},\"instanceID\":19608,\"localRotation\":{\"x\":0.0,\"y\":0.0,\"z\":0.0,\"w\":1.0},\"localPosition\":{\"x\":-80.0,\"y\":0.0,\"z\":0.0},\"localScale\":{\"x\":1.0,\"y\":1.0,\"z\":1.0},\"anchorMin\":{\"x\":0.0,\"y\":0.0},\"anchorMax\":{\"x\":1.0,\"y\":1.0},\"anchoredPosition\":{\"x\":0.0,\"y\":0.0},\"sizeDelta\":{\"x\":-18.0,\"y\":0.0},\"pivot\":{\"x\":0.0,\"y\":1.0}},{\"gameObjectName\":\"Content\",\"gameObjectEnable\":true,\"rectTransform\":{\"instanceID\":19704},\"instanceID\":19704,\"localRotation\":{\"x\":0.0,\"y\":0.0,\"z\":0.0,\"w\":1.0},\"localPosition\":{\"x\":71.0,\"y\":0.0,\"z\":0.0},\"localScale\":{\"x\":1.0,\"y\":1.0,\"z\":1.0},\"anchorMin\":{\"x\":0.0,\"y\":1.0},\"anchorMax\":{\"x\":1.0,\"y\":1.0},\"anchoredPosition\":{\"x\":0.0,\"y\":0.0},\"sizeDelta\":{\"x\":0.0,\"y\":28.0},\"pivot\":{\"x\":0.5,\"y\":1.0}},{\"gameObjectName\":\"Item\",\"gameObjectEnable\":true,\"rectTransform\":{\"instanceID\":19286},\"instanceID\":19286,\"localRotation\":{\"x\":0.0,\"y\":0.0,\"z\":0.0,\"w\":1.0},\"localPosition\":{\"x\":0.0,\"y\":-14.0,\"z\":0.0},\"localScale\":{\"x\":1.0,\"y\":1.0,\"z\":1.0},\"anchorMin\":{\"x\":0.0,\"y\":0.5},\"anchorMax\":{\"x\":1.0,\"y\":0.5},\"anchoredPosition\":{\"x\":0.0,\"y\":0.0},\"sizeDelta\":{\"x\":0.0,\"y\":20.0},\"pivot\":{\"x\":0.5,\"y\":0.5}},{\"gameObjectName\":\"Item Background\",\"gameObjectEnable\":true,\"rectTransform\":{\"instanceID\":19358},\"instanceID\":19358,\"localRotation\":{\"x\":0.0,\"y\":0.0,\"z\":0.0,\"w\":1.0},\"localPosition\":{\"x\":0.0,\"y\":0.0,\"z\":0.0},\"localScale\":{\"x\":1.0,\"y\":1.0,\"z\":1.0},\"anchorMin\":{\"x\":0.0,\"y\":0.0},\"anchorMax\":{\"x\":1.0,\"y\":1.0},\"anchoredPosition\":{\"x\":0.0,\"y\":0.0},\"sizeDelta\":{\"x\":0.0,\"y\":0.0},\"pivot\":{\"x\":0.5,\"y\":0.5}},{\"gameObjectName\":\"Item Checkmark\",\"gameObjectEnable\":true,\"rectTransform\":{\"instanceID\":19640},\"instanceID\":19640,\"localRotation\":{\"x\":0.0,\"y\":0.0,\"z\":0.0,\"w\":1.0},\"localPosition\":{\"x\":-61.0,\"y\":0.0,\"z\":0.0},\"localScale\":{\"x\":1.0,\"y\":1.0,\"z\":1.0},\"anchorMin\":{\"x\":0.0,\"y\":0.5},\"anchorMax\":{\"x\":0.0,\"y\":0.5},\"anchoredPosition\":{\"x\":10.0,\"y\":0.0},\"sizeDelta\":{\"x\":20.0,\"y\":20.0},\"pivot\":{\"x\":0.5,\"y\":0.5}},{\"gameObjectName\":\"Item Label\",\"gameObjectEnable\":true,\"rectTransform\":{\"instanceID\":19702},\"instanceID\":19702,\"localRotation\":{\"x\":0.0,\"y\":0.0,\"z\":0.0,\"w\":1.0},\"localPosition\":{\"x\":5.0,\"y\":-0.5,\"z\":0.0},\"localScale\":{\"x\":1.0,\"y\":1.0,\"z\":1.0},\"anchorMin\":{\"x\":0.0,\"y\":0.0},\"anchorMax\":{\"x\":1.0,\"y\":1.0},\"anchoredPosition\":{\"x\":5.0,\"y\":-0.5},\"sizeDelta\":{\"x\":-30.0,\"y\":-3.0},\"pivot\":{\"x\":0.5,\"y\":0.5}},{\"gameObjectName\":\"Scrollbar\",\"gameObjectEnable\":true,\"rectTransform\":{\"instanceID\":19082},\"instanceID\":19082,\"localRotation\":{\"x\":0.0,\"y\":0.0,\"z\":0.0,\"w\":1.0},\"localPosition\":{\"x\":80.0,\"y\":0.0,\"z\":0.0},\"localScale\":{\"x\":1.0,\"y\":1.0,\"z\":1.0},\"anchorMin\":{\"x\":1.0,\"y\":0.0},\"anchorMax\":{\"x\":1.0,\"y\":1.0},\"anchoredPosition\":{\"x\":0.0,\"y\":0.0},\"sizeDelta\":{\"x\":20.0,\"y\":0.0},\"pivot\":{\"x\":1.0,\"y\":1.0}},{\"gameObjectName\":\"Sliding Area\",\"gameObjectEnable\":true,\"rectTransform\":{\"instanceID\":19104},\"instanceID\":19104,\"localRotation\":{\"x\":0.0,\"y\":0.0,\"z\":0.0,\"w\":1.0},\"localPosition\":{\"x\":-10.0,\"y\":-75.0,\"z\":0.0},\"localScale\":{\"x\":1.0,\"y\":1.0,\"z\":1.0},\"anchorMin\":{\"x\":0.0,\"y\":0.0},\"anchorMax\":{\"x\":1.0,\"y\":1.0},\"anchoredPosition\":{\"x\":0.0,\"y\":0.0},\"sizeDelta\":{\"x\":-20.0,\"y\":-20.0},\"pivot\":{\"x\":0.5,\"y\":0.5}},{\"gameObjectName\":\"Handle\",\"gameObjectEnable\":true,\"rectTransform\":{\"instanceID\":19336},\"instanceID\":19336,\"localRotation\":{\"x\":0.0,\"y\":0.0,\"z\":0.0,\"w\":1.0},\"localPosition\":{\"x\":0.0,\"y\":-52.0,\"z\":0.0},\"localScale\":{\"x\":1.0,\"y\":1.0,\"z\":1.0},\"anchorMin\":{\"x\":0.0,\"y\":0.0},\"anchorMax\":{\"x\":1.0,\"y\":0.20000000298023225},\"anchoredPosition\":{\"x\":0.0,\"y\":0.0},\"sizeDelta\":{\"x\":20.0,\"y\":20.0},\"pivot\":{\"x\":0.5,\"y\":0.5}},{\"gameObjectName\":\"InputField (LT) [TMP]\",\"gameObjectEnable\":true,\"rectTransform\":{\"instanceID\":19430},\"instanceID\":19430,\"localRotation\":{\"x\":0.0,\"y\":0.0,\"z\":0.0,\"w\":1.0},\"localPosition\":{\"x\":-260.0,\"y\":-20.0,\"z\":0.0},\"localScale\":{\"x\":1.0,\"y\":1.0,\"z\":1.0},\"anchorMin\":{\"x\":0.5,\"y\":0.5},\"anchorMax\":{\"x\":0.5,\"y\":0.5},\"anchoredPosition\":{\"x\":-260.0,\"y\":-20.0},\"sizeDelta\":{\"x\":160.0,\"y\":30.0},\"pivot\":{\"x\":0.5,\"y\":0.5}},{\"gameObjectName\":\"Text Area\",\"gameObjectEnable\":true,\"rectTransform\":{\"instanceID\":19592},\"instanceID\":19592,\"localRotation\":{\"x\":0.0,\"y\":0.0,\"z\":0.0,\"w\":1.0},\"localPosition\":{\"x\":0.0,\"y\":-0.5,\"z\":0.0},\"localScale\":{\"x\":1.0,\"y\":1.0,\"z\":1.0},\"anchorMin\":{\"x\":0.0,\"y\":0.0},\"anchorMax\":{\"x\":1.0,\"y\":1.0},\"anchoredPosition\":{\"x\":0.0,\"y\":-0.5},\"sizeDelta\":{\"x\":-20.0,\"y\":-13.0},\"pivot\":{\"x\":0.5,\"y\":0.5}},{\"gameObjectName\":\"Placeholder\",\"gameObjectEnable\":true,\"rectTransform\":{\"instanceID\":19404},\"instanceID\":19404,\"localRotation\":{\"x\":0.0,\"y\":0.0,\"z\":0.0,\"w\":1.0},\"localPosition\":{\"x\":0.0,\"y\":0.0,\"z\":0.0},\"localScale\":{\"x\":1.0,\"y\":1.0,\"z\":1.0},\"anchorMin\":{\"x\":0.0,\"y\":0.0},\"anchorMax\":{\"x\":1.0,\"y\":1.0},\"anchoredPosition\":{\"x\":0.0,\"y\":0.0},\"sizeDelta\":{\"x\":0.0,\"y\":0.0},\"pivot\":{\"x\":0.5,\"y\":0.5}},{\"gameObjectName\":\"Text\",\"gameObjectEnable\":true,\"rectTransform\":{\"instanceID\":19438},\"instanceID\":19438,\"localRotation\":{\"x\":0.0,\"y\":0.0,\"z\":0.0,\"w\":1.0},\"localPosition\":{\"x\":0.0,\"y\":0.0,\"z\":0.0},\"localScale\":{\"x\":1.0,\"y\":1.0,\"z\":1.0},\"anchorMin\":{\"x\":0.0,\"y\":0.0},\"anchorMax\":{\"x\":1.0,\"y\":1.0},\"anchoredPosition\":{\"x\":0.0,\"y\":0.0},\"sizeDelta\":{\"x\":0.0,\"y\":0.0},\"pivot\":{\"x\":0.5,\"y\":0.5}},{\"gameObjectName\":\"Text (LT) [TMP]\",\"gameObjectEnable\":true,\"rectTransform\":{\"instanceID\":19568},\"instanceID\":19568,\"localRotation\":{\"x\":0.0,\"y\":0.0,\"z\":0.0,\"w\":1.0},\"localPosition\":{\"x\":-260.0,\"y\":-70.0,\"z\":0.0},\"localScale\":{\"x\":1.0,\"y\":1.0,\"z\":1.0},\"anchorMin\":{\"x\":0.5,\"y\":0.5},\"anchorMax\":{\"x\":0.5,\"y\":0.5},\"anchoredPosition\":{\"x\":-260.0,\"y\":-70.0},\"sizeDelta\":{\"x\":160.0,\"y\":30.0},\"pivot\":{\"x\":0.5,\"y\":0.5}},{\"gameObjectName\":\"Toggle (LT) [TMP]\",\"gameObjectEnable\":true,\"rectTransform\":{\"instanceID\":19190},\"instanceID\":19190,\"localRotation\":{\"x\":0.0,\"y\":0.0,\"z\":0.0,\"w\":1.0},\"localPosition\":{\"x\":-260.0,\"y\":-110.0,\"z\":0.0},\"localScale\":{\"x\":1.0,\"y\":1.0,\"z\":1.0},\"anchorMin\":{\"x\":0.5,\"y\":0.5},\"anchorMax\":{\"x\":0.5,\"y\":0.5},\"anchoredPosition\":{\"x\":-260.0,\"y\":-110.0},\"sizeDelta\":{\"x\":160.0,\"y\":20.0},\"pivot\":{\"x\":0.5,\"y\":0.5}},{\"gameObjectName\":\"Background\",\"gameObjectEnable\":true,\"rectTransform\":{\"instanceID\":19382},\"instanceID\":19382,\"localRotation\":{\"x\":0.0,\"y\":0.0,\"z\":0.0,\"w\":1.0},\"localPosition\":{\"x\":-70.0,\"y\":0.0,\"z\":0.0},\"localScale\":{\"x\":1.0,\"y\":1.0,\"z\":1.0},\"anchorMin\":{\"x\":0.0,\"y\":1.0},\"anchorMax\":{\"x\":0.0,\"y\":1.0},\"anchoredPosition\":{\"x\":10.0,\"y\":-10.0},\"sizeDelta\":{\"x\":20.0,\"y\":20.0},\"pivot\":{\"x\":0.5,\"y\":0.5}},{\"gameObjectName\":\"Checkmark\",\"gameObjectEnable\":true,\"rectTransform\":{\"instanceID\":19528},\"instanceID\":19528,\"localRotation\":{\"x\":0.0,\"y\":0.0,\"z\":0.0,\"w\":1.0},\"localPosition\":{\"x\":0.0,\"y\":0.0,\"z\":0.0},\"localScale\":{\"x\":1.0,\"y\":1.0,\"z\":1.0},\"anchorMin\":{\"x\":0.5,\"y\":0.5},\"anchorMax\":{\"x\":0.5,\"y\":0.5},\"anchoredPosition\":{\"x\":0.0,\"y\":0.0},\"sizeDelta\":{\"x\":20.0,\"y\":20.0},\"pivot\":{\"x\":0.5,\"y\":0.5}},{\"gameObjectName\":\"Label\",\"gameObjectEnable\":true,\"rectTransform\":{\"instanceID\":19218},\"instanceID\":19218,\"localRotation\":{\"x\":0.0,\"y\":0.0,\"z\":0.0,\"w\":1.0},\"localPosition\":{\"x\":9.0,\"y\":-0.5,\"z\":0.0},\"localScale\":{\"x\":1.0,\"y\":1.0,\"z\":1.0},\"anchorMin\":{\"x\":0.0,\"y\":0.0},\"anchorMax\":{\"x\":1.0,\"y\":1.0},\"anchoredPosition\":{\"x\":9.0,\"y\":-0.5},\"sizeDelta\":{\"x\":-28.0,\"y\":-3.0},\"pivot\":{\"x\":0.5,\"y\":0.5}},{\"gameObjectName\":\"Image (LT)\",\"gameObjectEnable\":true,\"rectTransform\":{\"instanceID\":19518},\"instanceID\":19518,\"localRotation\":{\"x\":0.0,\"y\":0.0,\"z\":0.0,\"w\":1.0},\"localPosition\":{\"x\":0.0,\"y\":130.0,\"z\":0.0},\"localScale\":{\"x\":1.0,\"y\":1.0,\"z\":1.0},\"anchorMin\":{\"x\":0.5,\"y\":0.5},\"anchorMax\":{\"x\":0.5,\"y\":0.5},\"anchoredPosition\":{\"x\":0.0,\"y\":130.0},\"sizeDelta\":{\"x\":170.0,\"y\":100.0},\"pivot\":{\"x\":0.5,\"y\":0.5}},{\"gameObjectName\":\"RawImage (LT)\",\"gameObjectEnable\":true,\"rectTransform\":{\"instanceID\":19244},\"instanceID\":19244,\"localRotation\":{\"x\":0.0,\"y\":0.0,\"z\":0.0,\"w\":1.0},\"localPosition\":{\"x\":0.0,\"y\":0.0,\"z\":0.0},\"localScale\":{\"x\":1.0,\"y\":1.0,\"z\":1.0},\"anchorMin\":{\"x\":0.5,\"y\":0.5},\"anchorMax\":{\"x\":0.5,\"y\":0.5},\"anchoredPosition\":{\"x\":0.0,\"y\":0.0},\"sizeDelta\":{\"x\":170.0,\"y\":100.0},\"pivot\":{\"x\":0.5,\"y\":0.5}}]}" }
            };

            // Adds the default language and canvas data to the provided lists.
            componentSave.AddRange(languageDefaultIDs);
            canvasSave.AddRange(canvasDefaultIDs);
        }

        // Displays a component icon based on the component type.
        public static void DisplayComponentIcon(int componentType)
        {
            Texture2D yourImage = null;

            // Determine which icon to load based on component type.
            switch (componentType)
            {
                case 1:
                    yourImage = FindTextureByName("LanguageText Icon");
                    break;
                case 2:
                    yourImage = FindTextureByName("LanguageTextMesh Icon");
                    break;
                case 3:
                    yourImage = FindTextureByName("LanguageTextInputField Icon");
                    break;
                case 4:
                    yourImage = FindTextureByName("LanguageScript Icon");
                    break;
                case 5:
                    yourImage = FindTextureByName("LanguageCreateFile Icon");
                    break;
                case 6:
                    yourImage = FindTextureByName("LanguageDropdown Icon");
                    break;
                default:
                    break;
            }

            // If an icon is found, draw it in the editor.
            if (yourImage != null)
            {
                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                float imageWidth = 70f;
                float imageHeight = 70f;
                Rect lastRect = GUILayoutUtility.GetLastRect();
                Rect imageRect = new(lastRect.x + lastRect.width - imageWidth, lastRect.y, imageWidth, imageHeight);
                GUI.DrawTexture(imageRect, yourImage);
                GUILayout.EndHorizontal();
            }
        }

        // Opens an editor window with a specific language component.
        public static void OpenEditorWindowWithComponent(int iD, int type, string text, int alignment, int size, int fontIndex, bool textWrite, bool alignmentWrite, bool sizeWrite, bool fontIndexWrite)
        {
            var data = new LanguageComponentSave()
            {
                iD = iD,
                componentType = type,
                text = text,
                alignment = alignment,
                fontSize = size,
                fontListIndex = fontIndex,
                textWrite = textWrite,
                alignmentWrite = alignmentWrite,
                fontSizeWrite = sizeWrite,
                fontListIndexWrite = fontIndexWrite
            };

            // Show and configure the language file manager window.
            LanguageFileManagerWindow.ShowEditorWindow();
            var window = EditorWindow.GetWindow<LanguageFileManagerWindow>();
            window.AddComponent(data);
        }

        // Opens an editor window with a specific canvas.
        public static void OpenEditorWindowWithCanvas(int iD, string json)
        {
            var data = new CanvasSave()
            {
                canvasID = iD,
                json = json
            };

            // Show and configure the language file manager window.
            LanguageFileManagerWindow.ShowEditorWindow();
            var window = EditorWindow.GetWindow<LanguageFileManagerWindow>();
            window.AddCanvas(data);
        }

        // Gets the full hierarchy path of a GameObject as a string.
        public static string GetGameObjectPath(Transform transform)
        {
            List<string> pathList = new();
            Transform currentTransform = transform;

            // Traverse the hierarchy upwards to build the full path.
            while (currentTransform != null)
            {
                if (currentTransform.GetComponent<Canvas>() && currentTransform.GetComponent<CanvasScaler>() && currentTransform.GetComponent<GraphicRaycaster>())
                {
                    break;
                }

                pathList.Insert(0, $"{currentTransform.name}*[{currentTransform.GetInstanceID()}]*");
                currentTransform = currentTransform.parent;
            }

            return string.Join("'/'", pathList); // Return the constructed path as a string.
        }

        // Gets the depth of the hierarchy from the path array.
        public static int GetHierarchyDepth(string[] pathParts)
        {
            return pathParts.Length - 1; // Subtracting 1 to ignore root element.
        }

        // Extracts text between asterisks from a string input.
        public static string ExtractTextBetweenAsterisks(string input)
        {
            int startIndex = input.IndexOf("*[");
            int endIndex = input.LastIndexOf("]*");

            // Check if the asterisks exist in the input.
            if (startIndex >= 0 && endIndex > startIndex)
            {
                string textInsideAsterisks = input.Substring(startIndex + 2, endIndex - startIndex - 2);
                return textInsideAsterisks;
            }

            return "0"; // Default return value if no valid text is found.
        }
    }
}
#endif