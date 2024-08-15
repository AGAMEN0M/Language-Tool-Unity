/*
 * ---------------------------------------------------------------------------
 * Description: The EmojiSystemEditor script is a custom Unity editor window that allows
 *              users to manage and edit emoji sprite assets. It provides functionality for
 *              adjusting glyph metrics and applying these changes to both SpriteAsset and 
 *              TMP_SpriteAsset types.
 *              Users can also apply these changes to all relevant objects in the scene.
 *              Additionally, the script integrates with Python for image processing tasks such as cropping sprites.
 * Author: Lucas Gomes Cecchini
 * Pseudonym: AGAMENOM
 * ---------------------------------------------------------------------------
*/
using UnityEngine.SceneManagement;
using UnityEngine.TextCore.Text;
using UnityEngine.TextCore;
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using TMPro;

public class EmojiSystemEditor : EditorWindow
{
    // SerializedObject for handling serialized properties.
    private SerializedObject serializedObject;

    // Scroll position for the editor window.
    public Vector2 scrollPosition = Vector2.zero;

    // Fields for editing sprite and glyph metrics.
    public SpriteAsset spriteAssetEdit;
    public TMP_SpriteAsset TMP_spriteAssetEdit;
    public Type emojiType = Type.Legacy;
    public float w;
    public float h;
    public float bx;
    public float by;
    public float ad;
    public float scale;
    public int atlasIndex;
    public Texture spriteEdit;
    public int size = 100;
    public string folderPath;
    public string pythonPath;

    // Enumeration for asset types.
    public enum Type
    {
        Legacy,
        TextMeshPro
    }

    // Add menu item to open the editor window.
    [MenuItem("Window/Emoji System Editor", false, 2030)]
    public static void ShowWindow()
    {
        EmojiSystemEditor window = GetWindow<EmojiSystemEditor>("Emoji System Editor");

        // Load and set an icon for the editor window if it exists.
        string[] guids = AssetDatabase.FindAssets("EmojiSystemEditor Icon t:Texture2D");
        if (guids.Length > 0)
        {
            string path = AssetDatabase.GUIDToAssetPath(guids[0]);
            Texture2D icon = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            window.titleContent = new GUIContent("Emoji System Editor", icon);
        }
        else
        {
            window.titleContent = new GUIContent("Emoji System Editor");
        }
    }

    // Initialize the SerializedObject when the window is enabled.
    private void OnEnable()
    {
        serializedObject = new SerializedObject(this);
    }

    // Draw the GUI for the editor window.
    private void OnGUI()
    {
        serializedObject.Update();
        scrollPosition = GUILayout.BeginScrollView(scrollPosition);

        // Define text style for labels.
        GUIStyle textStyle = new(GUI.skin.label)
        {
            fontSize = 13,
            fontStyle = FontStyle.Bold,
        };

        // Display window title and type selection.
        GUILayout.Label("Emoji System Editor", textStyle);
        GUILayout.Space(10);
        GUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Asset Type:", GUILayout.Width(80));
        emojiType = (Type)EditorGUILayout.EnumPopup(emojiType, GUILayout.Width(110));
        GUILayout.EndHorizontal();

        GUILayout.Space(20);

        // Display fields for selecting sprite assets based on type.
        EditorGUILayout.LabelField("Sprite to Edit", textStyle);
        if (emojiType == Type.Legacy)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label("Sprite Asset:", GUILayout.Width(115));
            spriteAssetEdit = (SpriteAsset)EditorGUILayout.ObjectField(spriteAssetEdit, typeof(SpriteAsset), false, GUILayout.Width(230));
            GUILayout.EndHorizontal();
        }
        else if (emojiType == Type.TextMeshPro)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label("Sprite Asset (TMP):", GUILayout.Width(115));
            TMP_spriteAssetEdit = (TMP_SpriteAsset)EditorGUILayout.ObjectField(TMP_spriteAssetEdit, typeof(TMP_SpriteAsset), false, GUILayout.Width(230));
            GUILayout.EndHorizontal();
        }

        GUILayout.Space(20);
        EditorGUILayout.LabelField("Glyph Metrics Editor", textStyle);

        // Display fields for editing glyph metrics.
        GUILayout.BeginHorizontal();
        GUILayout.Label("W:", GUILayout.Width(25));
        GUILayout.Space(5);
        w = EditorGUILayout.FloatField(w, GUILayout.Width(139));

        GUILayout.Label("H:", GUILayout.Width(25));
        GUILayout.Space(5);
        h = EditorGUILayout.FloatField(h, GUILayout.Width(139));
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.Label("BX:", GUILayout.Width(25));
        GUILayout.Space(5);
        bx = EditorGUILayout.FloatField(bx, GUILayout.Width(80));

        GUILayout.Label("BY:", GUILayout.Width(25));
        GUILayout.Space(5);
        by = EditorGUILayout.FloatField(by, GUILayout.Width(80));

        GUILayout.Label("AD:", GUILayout.Width(25));
        GUILayout.Space(5);
        ad = EditorGUILayout.FloatField(ad, GUILayout.Width(80));
        GUILayout.EndHorizontal();

        GUILayout.Space(5);

        GUILayout.BeginHorizontal();
        GUILayout.Label("Scale:", GUILayout.Width(40));
        GUILayout.Space(5);
        scale = EditorGUILayout.FloatField(scale, GUILayout.Width(65));

        GUILayout.Label("Atlas Index:", GUILayout.Width(70));
        GUILayout.Space(5);
        atlasIndex = EditorGUILayout.IntField(atlasIndex, GUILayout.Width(35));
        GUILayout.EndHorizontal();

        GUILayout.Space(10);

        // Button to save the edited asset.
        GUI.enabled = (TMP_spriteAssetEdit != null || spriteAssetEdit != null);
        if (GUILayout.Button("Save Asset", GUILayout.Width(120), GUILayout.Height(30)))
        {
            bool saveConfirmed = EditorUtility.DisplayDialog("Save Confirmation", "Are you sure you want to save?\nIt will not be possible to undo this operation.", "Yes", "No");
            if (saveConfirmed)
            {
                SaveFile();
            }
        }
        GUI.enabled = true;

        // If using TextMeshPro, show the button to apply changes to all objects in the scene.
        if (emojiType == Type.TextMeshPro)
        {
            GUILayout.Space(20);
            EditorGUILayout.LabelField("Scene Edit", textStyle);

            GUI.enabled = TMP_spriteAssetEdit != null;
            if (GUILayout.Button("Apply to Scene", GUILayout.Width(120), GUILayout.Height(30)))
            {
                bool applyToScene = EditorUtility.DisplayDialog("Apply Confirmation", "Are you sure you want to do this?\nThis will replace all SpriteAsset of all texts in the scene and it will not be possible to undo this operation.", "Yes", "No");
                if (applyToScene)
                {
                    ApplyToAllObjects();
                }
            }
            GUI.enabled = true;
        }

        GUILayout.Space(20);
        GUILayout.Label("Sprite Editor", textStyle);
        GUILayout.Label("You need to install the Python Pillow library.");
        GUILayout.Space(5);

        // Display fields for editing sprite and selecting size.
        GUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Sprite:", GUILayout.Width(40));
        spriteEdit = (Texture)EditorGUILayout.ObjectField(spriteEdit, typeof(Texture), false, GUILayout.Width(170));
        GUILayout.Space(15);
        EditorGUILayout.LabelField("Size:", GUILayout.Width(40));
        size = EditorGUILayout.IntField(size, GUILayout.Width(76));
        GUILayout.EndHorizontal();

        GUILayout.Space(5);

        // Button to apply changes to a folder.
        GUILayout.BeginHorizontal();
        GUI.enabled = (spriteEdit != null && pythonPath != "");
        if (GUILayout.Button("Apply to Folder", GUILayout.Width(120), GUILayout.Height(30)))
        {
            folderPath = EditorUtility.OpenFolderPanel("Select Folder to Apply Changes", "", "");
            RunPythonScript(folderPath);
        }
        GUI.enabled = true;

        GUILayout.Space(20);

        // Button to select the Python executable.
        if (GUILayout.Button("python.exe", GUILayout.Width(120), GUILayout.Height(30)))
        {
            GetPython();
        }
        GUILayout.EndHorizontal();

        GUILayout.EndScrollView();
        serializedObject.ApplyModifiedProperties();
    }

    // Save the sprite asset or TMP sprite asset with updated metrics.
    private void SaveFile()
    {
        if (emojiType == Type.Legacy && spriteAssetEdit != null)
        {
            UpdateGlyphMetrics(spriteAssetEdit.spriteGlyphTable);
            SaveAsset(spriteAssetEdit);
        }
        else if (emojiType == Type.TextMeshPro && TMP_spriteAssetEdit != null)
        {
            UpdateGlyphMetrics(TMP_spriteAssetEdit.spriteGlyphTable);
            SaveAsset(TMP_spriteAssetEdit);
        }
    }

    // Update the metrics of glyphs in the sprite glyph table.
    private void UpdateGlyphMetrics<T>(List<T> spriteGlyphTable) where T : Glyph
    {
        foreach (var glyph in spriteGlyphTable)
        {
            if (glyph.metrics != null)
            {
                GlyphMetrics customMetrics = new()
                {
                    width = w,
                    height = h,
                    horizontalBearingX = bx,
                    horizontalBearingY = by,
                    horizontalAdvance = ad
                };

                glyph.metrics = customMetrics;
            }

            glyph.scale = scale;
            glyph.atlasIndex = atlasIndex;
        }
    }

    // Save the modified asset and refresh the AssetDatabase.
    private void SaveAsset(Object asset)
    {
        EditorUtility.SetDirty(asset);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    // Apply the sprite asset to all relevant objects in the current scene.
    private void ApplyToAllObjects()
    {
        GameObject[] allObjectsInScene = Resources.FindObjectsOfTypeAll<GameObject>();

        foreach (GameObject gameObject in allObjectsInScene)
        {
            if (gameObject.scene == SceneManager.GetActiveScene())
            {
                if (gameObject.TryGetComponent<TMP_Text>(out var text))
                {
                    text.spriteAsset = TMP_spriteAssetEdit;
                }
                UnityEngine.Debug.Log("Applied changes to all relevant objects in the current scene.");
            }
        }
    }

    // Run the Python script for processing the sprite.
    private void RunPythonScript(string folder)
    {
        if (string.IsNullOrEmpty(pythonPath))
        {
            UnityEngine.Debug.LogError("Python path not set.");
            return;
        }

        string scriptPath = "";
        string[] files = Directory.GetFiles(Application.dataPath, "crop_image.py", SearchOption.AllDirectories);
        if (files.Length > 0)
        {
            scriptPath = files[0];
        }

        if (string.IsNullOrEmpty(scriptPath))
        {
            UnityEngine.Debug.LogError("Python script 'crop_image.py' not found.");
            return;
        }

        string imagePath = AssetDatabase.GetAssetPath(spriteEdit);
        string destinationFolder = folder;
        int squareSize = size;

        // Prepare arguments for the Python script.
        string arguments = $"\"{scriptPath}\" \"{imagePath}\" {squareSize} \"{destinationFolder}\"";

        // Set up the process start info for running the Python script.
        ProcessStartInfo startInfo = new()
        {
            FileName = pythonPath,
            Arguments = arguments,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using Process process = Process.Start(startInfo);
        string output = process.StandardOutput.ReadToEnd();
        string error = process.StandardError.ReadToEnd();

        process.WaitForExit();

        if (output != "")
        {
            UnityEngine.Debug.Log(output);
        }

        if (error != "")
        {
            UnityEngine.Debug.LogError(error);
        }
    }

    // Open a file panel to select the Python executable.
    private void GetPython()
    {
        string path = EditorUtility.OpenFilePanel("Select Python Executable", "", "exe");

        if (!string.IsNullOrEmpty(path))
        {
            pythonPath = path;
            UnityEngine.Debug.Log($"Python path defined for: {pythonPath}");
        }
        else
        {
            UnityEngine.Debug.LogError("Python path not selected.");
        }
    }
}