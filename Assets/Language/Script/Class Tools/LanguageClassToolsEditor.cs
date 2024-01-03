#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public static class LanguageClassToolsEditor
{
    // Method for drawing a colored button in the Unity Editor GUI.
    public static void DrawColoredButton(string label, float textSize, float buttonSize, Color buttonColor, Color textColor, System.Action clickAction)
    {
        // Change the background color of the button and set text color.
        Color previousColor = GUI.backgroundColor;
        GUI.backgroundColor = buttonColor;

        GUIStyle buttonStyle = new(GUI.skin.button)
        {
            fontSize = Mathf.FloorToInt(textSize),
            normal = { textColor = textColor }
        };

        GUILayoutOption[] options = { GUILayout.Width(buttonSize), GUILayout.Height(buttonSize) };

        // Draw the button with custom colors.
        if (GUILayout.Button(label, buttonStyle, options))
        {
            clickAction?.Invoke(); // Invoke the specified action when clicked.
        }

        GUI.backgroundColor = previousColor; // Restore the previous background color.
    }

    // Method to draw a colored box in the Unity Editor GUI.
    public static void DrawBox(System.Action content, Color Color)
    {
        Rect rect = EditorGUILayout.BeginVertical(); // Gets the rectangle that will be used to render the blue background.
        EditorGUI.DrawRect(rect, Color); // Draws a rectangle filled with blue color.
        content.Invoke(); // Calls the custom content function to fill the rectangle with information.
        EditorGUILayout.EndVertical(); // Ends the rectangle rendering area.
    }

    // Method to find and return duplicate IDs in the data lists.
    public static Dictionary<float, List<string>> GetDuplicateIDs(List<LanguageSaveGroups> groupsID, List<LanguageSaveID> textID, List<LanguageSaveComments> commentsID)
    {
        Dictionary<float, List<string>> duplicateIDs = new(); // Dictionary for storing duplicate IDs and their occurrences.

        // Sets to track unique group IDs, texts, and comments.
        HashSet<float> groupIDs = new();
        HashSet<float> textIDs = new();
        HashSet<float> commentIDs = new();

        // Iterates through the group IDs to check for duplicates.
        foreach (var id in groupsID)
        {
            if (groupIDs.Contains(id.textID))
            {
                // If an occurrence of that ID already exists, add it to the duplicates dictionary.
                if (!duplicateIDs.ContainsKey(id.textID))
                {
                    duplicateIDs[id.textID] = new List<string>();
                }

                duplicateIDs[id.textID].Add("Group ID");
            }

            groupIDs.Add(id.textID); // Adds the ID to the set of group IDs.
        }

        // Iterates through the text IDs to check for duplicates.
        foreach (var id in textID)
        {
            if (textIDs.Contains(id.ID))
            {
                // If an occurrence of that ID already exists, add it to the duplicates dictionary.
                if (!duplicateIDs.ContainsKey(id.ID))
                {
                    duplicateIDs[id.ID] = new List<string>();
                }

                duplicateIDs[id.ID].Add("Text ID");
            }

            textIDs.Add(id.ID); // Adds the ID to the set of text IDs.
        }

        // Iterates through comment IDs to check for duplicates.
        foreach (var id in commentsID)
        {
            if (commentIDs.Contains(id.comments))
            {
                // If an occurrence of that ID already exists, add it to the duplicates dictionary.
                if (!duplicateIDs.ContainsKey(id.comments))
                {
                    duplicateIDs[id.comments] = new List<string>();
                }

                duplicateIDs[id.comments].Add("Comment ID");
            }

            commentIDs.Add(id.comments); // Adds the ID to the set of comment IDs.
        }

        return duplicateIDs; // Returns the dictionary of duplicate IDs.
    }

    // Sets default values for language-related data lists.
    public static void DefaultSettings(ref List<LanguageSaveGroups> groupsID, ref List<LanguageSaveID> textID, ref List<LanguageSaveComments> commentsID)
    {
        // Define default LanguageSaveID entries.
        var languageDefaultIDs = new List<LanguageSaveID>
        {
            new() { ID = -17f, componentType = 2f, text = "Hello World", textWrite = true, font = 1f, fontWrite = true, fontSize = 0f, fontSizeWrite = true, reverse = 0f, reverseWrite = true },
            new() { ID = -16f, componentType = 4f, text = "Test Language Script", textWrite = true },
            new() { ID = -15f, componentType = 7f, rotation = new(0, 0, 0, 0), rotationWrite = true, localScale = Vector3.zero, localScaleWrite = true, anchorMin = Vector2.zero, anchorMinWrite = true, anchorMax = Vector2.zero, anchorMaxWrite = true, anchoredPosition = Vector2.zero, anchoredPositionWrite = true, sizeDelta = Vector2.zero, sizeDeltaWrite = true, pivot = Vector2.zero, pivotWrite = true },
            new() { ID = -14f, componentType = 7f, rotation = new(0, 0, 0, 0), rotationWrite = true, localScale = Vector3.zero, localScaleWrite = true, anchorMin = Vector2.zero, anchorMinWrite = true, anchorMax = Vector2.zero, anchorMaxWrite = true, anchoredPosition = Vector2.zero, anchoredPositionWrite = true, sizeDelta = Vector2.zero, sizeDeltaWrite = true, pivot = Vector2.zero, pivotWrite = true },
            new() { ID = -13f, componentType = 1f, rotation = new(0, 0, 0, 0), rotationWrite = true, localScale = Vector3.zero, localScaleWrite = true, anchorMin = Vector2.zero, anchorMinWrite = true, anchorMax = Vector2.zero, anchorMaxWrite = true, anchoredPosition = Vector2.zero, anchoredPositionWrite = true, sizeDelta = Vector2.zero, sizeDeltaWrite = true, pivot = Vector2.zero, pivotWrite = true, text = "Toggle", textWrite = true, font = 1f, fontWrite = true, fontSize = 0f, fontSizeWrite = true, alignment = 0f, alignmentWrite = true, reverse = 0f, reverseWrite = true },
            new() { ID = -12f, componentType = 1f, rotation = new(0, 0, 0, 0), rotationWrite = true, localScale = Vector3.zero, localScaleWrite = true, anchorMin = Vector2.zero, anchorMinWrite = true, anchorMax = Vector2.zero, anchorMaxWrite = true, anchoredPosition = Vector2.zero, anchoredPositionWrite = true, sizeDelta = Vector2.zero, sizeDeltaWrite = true, pivot = Vector2.zero, pivotWrite = true, text = "New Text", textWrite = true, font = 1f, fontWrite = true, fontSize = 0f, fontSizeWrite = true, alignment = 0f, alignmentWrite = true, reverse = 0f, reverseWrite = true },
            new() { ID = -11f, componentType = 7f, rotation = new(0, 0, 0, 0), rotationWrite = true, localScale = Vector3.zero, localScaleWrite = true, anchorMin = Vector2.zero, anchorMinWrite = true, anchorMax = Vector2.zero, anchorMaxWrite = true, anchoredPosition = Vector2.zero, anchoredPositionWrite = true, sizeDelta = Vector2.zero, sizeDeltaWrite = true, pivot = Vector2.zero, pivotWrite = true },
            new() { ID = -10f, componentType = 7f, rotation = new(0, 0, 0, 0), rotationWrite = true, localScale = Vector3.zero, localScaleWrite = true, anchorMin = Vector2.zero, anchorMinWrite = true, anchorMax = Vector2.zero, anchorMaxWrite = true, anchoredPosition = Vector2.zero, anchoredPositionWrite = true, sizeDelta = Vector2.zero, sizeDeltaWrite = true, pivot = Vector2.zero, pivotWrite = true },
            new() { ID = -9f, componentType = 3f, rotation = new(0, 0, 0, 0), rotationWrite = true, localScale = Vector3.zero, localScaleWrite = true, anchorMin = Vector2.zero, anchorMinWrite = true, anchorMax = Vector2.zero, anchorMaxWrite = true, anchoredPosition = Vector2.zero, anchoredPositionWrite = true, sizeDelta = Vector2.zero, sizeDeltaWrite = true, pivot = Vector2.zero, pivotWrite = true, font = 1f, fontWrite = true, fontSize = 1f, fontSizeWrite = true, alignment = 0f, alignmentWrite = true },
            new() { ID = -8f, componentType = 1f, rotation = new(0, 0, 0, 0), rotationWrite = true, localScale = Vector3.zero, localScaleWrite = true, anchorMin = Vector2.zero, anchorMinWrite = true, anchorMax = Vector2.zero, anchorMaxWrite = true, anchoredPosition = Vector2.zero, anchoredPositionWrite = true, sizeDelta = Vector2.zero, sizeDeltaWrite = true, pivot = Vector2.zero, pivotWrite = true, text = "Enter text...", textWrite = true, font = 1f, fontWrite = true, fontSize = 0f, fontSizeWrite = true, alignment = 0f, alignmentWrite = true, reverse = 0f, reverseWrite = true },
            new() { ID = -7f, componentType = 5f, text = "Test Language", textWrite = true },
            new() { ID = -6f, componentType = 6f, rotation = new(0, 0, 0, 0), rotationWrite = true, localScale = Vector3.zero, localScaleWrite = true, anchorMin = Vector2.zero, anchorMinWrite = true, anchorMax = Vector2.zero, anchorMaxWrite = true, anchoredPosition = Vector2.zero, anchoredPositionWrite = true, sizeDelta = Vector2.zero, sizeDeltaWrite = true, pivot = Vector2.zero, pivotWrite = true, font = 1f, fontWrite = true, alignment = 1f, alignmentWrite = true, reverse = 0f, reverseWrite = true },
            new() { ID = -5f, componentType = 6f, text = "Option C", textWrite = true },
            new() { ID = -4f, componentType = 6f, text = "Option B", textWrite = true },
            new() { ID = -3f, componentType = 6f, text = "Option A", textWrite = true },
            new() { ID = -2f, componentType = 7f, rotation = new(0, 0, 0, 0), rotationWrite = true, localScale = Vector3.zero, localScaleWrite = true, anchorMin = Vector2.zero, anchorMinWrite = true, anchorMax = Vector2.zero, anchorMaxWrite = true, anchoredPosition = Vector2.zero, anchoredPositionWrite = true, sizeDelta = Vector2.zero, sizeDeltaWrite = true, pivot = Vector2.zero, pivotWrite = true },
            new() { ID = -1f, componentType = 1f, rotation = new(0, 0, 0, 0), rotationWrite = true, localScale = Vector3.zero, localScaleWrite = true, anchorMin = Vector2.zero, anchorMinWrite = true, anchorMax = Vector2.zero, anchorMaxWrite = true, anchoredPosition = Vector2.zero, anchoredPositionWrite = true, sizeDelta = Vector2.zero, sizeDeltaWrite = true, pivot = Vector2.zero, pivotWrite = true, text = "Button", textWrite = true, font = 1f, fontWrite = true, fontSize = 0f, fontSizeWrite = true, alignment = 0f, alignmentWrite = true, reverse = 0f, reverseWrite = true },
            new() { ID = 0f, componentType = 1f, rotation = new(0, 0, 0, 0), rotationWrite = true, localScale = Vector3.zero, localScaleWrite = true, anchorMin = Vector2.zero, anchorMinWrite = true, anchorMax = Vector2.zero, anchorMaxWrite = true, anchoredPosition = Vector2.zero, anchoredPositionWrite = true, sizeDelta = Vector2.zero, sizeDeltaWrite = true, pivot = Vector2.zero, pivotWrite = true, text = "No language ID", textWrite = true, font = 1f, fontWrite = true, fontSize = 0f, fontSizeWrite = true, alignment = 0f, alignmentWrite = true, reverse = 0f, reverseWrite = true },
        };
        textID.AddRange(languageDefaultIDs);

        // Define default LanguageSaveGroups entries.
        var groupDefaultIDs = new List<LanguageSaveGroups>
        {
            new() { textID = 0f, text = "Unity default items", IDs = "0;-1;-2;-3;-4;-5;-6;-7;-8;-9;-10;-11;-12;-13;-14;-15;-16;-17;", view = true },
        };
        groupsID.AddRange(groupDefaultIDs);

        // Define default LanguageSaveComments entries.
        var commentDefaultIDs = new List<LanguageSaveComments>
        {
            new() { comments = -17f, textComments = "TextMesh Default Value." },
            new() { comments = -16f, textComments = "LanguageScript Default Value." },
            new() { comments = -13f, textComments = "Toggle Default Value." },
            new() { comments = -12f, textComments = "Text Default Value." },
            new() { comments = -11f, textComments = "AdjustRect Default Value." },
            new() { comments = -8f, textComments = "InputField Default Value." },
            new() { comments = -7f, textComments = "LanguageCreateFile Default Value." },
            new() { comments = -3f, textComments = "Dropdown Default Value." },
            new() { comments = -1f, textComments = "Button Default Value." },
            new() { comments = 0f, textComments = "If an id has not been defined, it will show this message." },
        };
        commentsID.AddRange(commentDefaultIDs);
    }

    // Extracts a boolean value from a given string based on a specified keyword.
    public static bool ExtractBooleanValue(string lineWithoutCurlyBraces, string keyword)
    {
        if (lineWithoutCurlyBraces.Contains(keyword))
        {
            int startIndex = lineWithoutCurlyBraces.IndexOf(keyword) + keyword.Length;
            int endIndex = lineWithoutCurlyBraces.IndexOf(";", startIndex);

            if (startIndex >= 0 && endIndex >= 0)
            {
                string valueString = lineWithoutCurlyBraces[startIndex..endIndex]; // Extracts the substring containing the boolean value.

                // Parses the extracted string to obtain the boolean value.
                if (bool.TryParse(valueString, out bool value))
                {
                    return value; // Returns the extracted boolean value.
                }
            }
        }
        return false; // Default return value if extraction fails.
    }

    // Displays an image representing the component type in the Unity Editor.
    public static void ViewComponent(float componentType)
    {
        Texture2D yourImage;
        string pathToImage = "";

        switch (componentType)
        {
            // Associates component types with their respective image paths.
            case 1:
                pathToImage = "Assets/Language/Editor/Icon/Legacy/LanguageText Icon.png";
                break;
            case 2:
                pathToImage = "Assets/Language/Editor/Icon/Legacy/Scene/LanguageTextMesh Icon.png";
                break;
            case 3:
                pathToImage = "Assets/Language/Editor/Icon/Legacy/LanguageTextInputField Icon.png";
                break;
            case 4:
                pathToImage = "Assets/Language/Editor/Icon/LanguageScript Icon.png";
                break;
            case 5:
                pathToImage = "Assets/Language/Editor/Icon/LanguageCreateFile Icon.png";
                break;
            case 6:
                pathToImage = "Assets/Language/Editor/Icon/Legacy/LanguageDropdown Icon.png";
                break;
            case 7:
                pathToImage = "Assets/Language/Editor/Icon/UI/AdjustRect Icon.png";
                break;
            default:
                // Handle default case if needed.
                break;
        }

        yourImage = AssetDatabase.LoadAssetAtPath<Texture2D>(pathToImage); // Loads and displays the associated image if available.

        if (yourImage != null)
        {
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            float imageWidth = 70f;
            float imageHeight = 70f;
            Rect lastRect = GUILayoutUtility.GetLastRect();
            Rect imageRect = new(lastRect.x + lastRect.width - imageWidth, lastRect.y, imageWidth, imageHeight);
            GUI.DrawTexture(imageRect, yourImage); // Draws the loaded image in the GUI.
            GUILayout.EndHorizontal();
        }
    }

    // Resets or clears the properties of an existing LanguageSaveID entry.
    public static void ClearExistingLanguageSaveIDValues(LanguageSaveID existingLanguageSaveID)
    {
        existingLanguageSaveID.componentType = 0; // Set the component to a default value, if applicable.
        existingLanguageSaveID.rotation = Quaternion.identity; // Clear rotation.
        existingLanguageSaveID.rotationWrite = false; // Set the rotation flag to false.
        existingLanguageSaveID.localScale = Vector3.zero; // Clear the scale.
        existingLanguageSaveID.localScaleWrite = false; // Set the scale flag to false.
        existingLanguageSaveID.anchorMin = Vector2.zero; // Clean anchorMin.
        existingLanguageSaveID.anchorMinWrite = false; // Set the anchorMin flag to false.
        existingLanguageSaveID.anchorMax = Vector2.zero; // Clean anchorMax.
        existingLanguageSaveID.anchorMaxWrite = false; // Set the anchorMax flag to false.
        existingLanguageSaveID.anchoredPosition = Vector2.zero; // Clear the anchored position.
        existingLanguageSaveID.anchoredPositionWrite = false; // Set the anchored position flag to false.
        existingLanguageSaveID.sizeDelta = Vector2.zero; // Clear the sizeDelta.
        existingLanguageSaveID.sizeDeltaWrite = false; // Set the sizeDelta flag to false.
        existingLanguageSaveID.pivot = Vector2.zero; // Clean the pivot.
        existingLanguageSaveID.pivotWrite = false; // Set the pivot flag to false.
        existingLanguageSaveID.textWrite = false; // Set the text flag to false.
        existingLanguageSaveID.text = ""; // Clean up the text.
        existingLanguageSaveID.fontWrite = false; // Set the font flag to false.
        existingLanguageSaveID.font = 0; // Clear the font.
        existingLanguageSaveID.fontSizeWrite = false; // Set the font size flag to false.
        existingLanguageSaveID.fontSize = 0; // Clear the font size.
        existingLanguageSaveID.alignmentWrite = false; // Set the alignment flag to false.
        existingLanguageSaveID.alignment = 0; // Clear the alignment.
        existingLanguageSaveID.reverseWrite = false; // Set the reverse flag to false.
        existingLanguageSaveID.reverse = 0; // Clear the reverse value.
    }

    // Checks if a given ID exists within a list of IDs (TextID).
    public static bool IsIDInTextIDList(float ID, ref List<float> IDs)
    {
        return IDs.Contains(ID); // Returns true if the ID exists in the list; otherwise, returns false.
    }

    // Loads data from a JSON file into the provided list of IDs.
    public static void LoadDataFromFile(ref List<float> IDs)
    {
        IDs.Clear(); // Clears the existing list of IDs.

        if (File.Exists("ProjectSettings/LanguageData.json"))
        {
            string jsonData = File.ReadAllText("ProjectSettings/LanguageData.json"); // Reads JSON content from the file.
            LanguageData data = JsonUtility.FromJson<LanguageData>(jsonData); // Deserializes the JSON content into the LanguageData class.

            // Extracts IDs from the TextIDs list in the loaded LanguageData.
            if (data != null && data.TextIDs != null)
            {
                foreach (var languageSaveID in data.TextIDs)
                {
                    IDs.Add(languageSaveID.ID); // Adds IDs to the list.
                }
            }
        }
    }

    // Saves AdjustRectList data by extracting relevant properties to the AdjustRectList entries.
    public static void SaveAdjustRect(ref List<AdjustRectList> adjustRectList)
    {
        if (adjustRectList != null && adjustRectList.Count > 0)
        {
            foreach (AdjustRectList rectList in adjustRectList)
            {
                rectList.adjustRect.SavaRectTransform(); // Saves the RectTransform.
                rectList.ID = rectList.adjustRect.ID; // Assigns the ID.

                // Copies various properties from the RectTransform to the AdjustRectList entry.
                rectList.rotation = rectList.adjustRect.rotation;
                rectList.localScale = rectList.adjustRect.localScale;
                rectList.anchorMin = rectList.adjustRect.anchorMin;
                rectList.anchorMax = rectList.adjustRect.anchorMax;
                rectList.anchoredPosition = rectList.adjustRect.anchoredPosition;
                rectList.sizeDelta = rectList.adjustRect.sizeDelta;
                rectList.pivot = rectList.adjustRect.pivot;
            }
        }
    }

    // Associates IDs with corresponding entries in the AdjustRectList.
    public static void ExportIDsToAdjustRect(List<AdjustRectList> adjustRectList)
    {
        if (adjustRectList != null && adjustRectList.Count > 0)
        {
            foreach (var adjustRectListEntry in adjustRectList)
            {
                adjustRectListEntry.adjustRect.ID = adjustRectListEntry.ID; // Associates IDs with AdjustRectList entries.
            }
        }
    }
}
#endif