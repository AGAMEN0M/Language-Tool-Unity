/*
 * ---------------------------------------------------------------------------
 * Description: Emoji System Editor for Unity. Allows editing of Legacy and 
 *              TextMeshPro sprite assets, modification of glyph metrics, and cutting of 
 *              textures into square segments. Supports direct scene application of TMP 
 *              sprite assets.
 * 
 * Author: Lucas Gomes Cecchini
 * Pseudonym: AGAMENOM
 * ---------------------------------------------------------------------------
*/

using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using UnityEngine.TextCore.Text;
using UnityEngine.TextCore;
using UnityEditor;
using UnityEngine;
using System.IO;
using TMPro;

/// <summary>
/// Custom Unity Editor window for managing emoji sprite assets. 
/// Supports editing of glyph metrics, sprite asset selection, 
/// and cutting textures into square segments. Works with both Legacy 
/// and TextMeshPro sprite assets.
/// </summary>
public class EmojiSystemEditor : EditorWindow
{
    #region === Serialized Fields ===

    /// <summary>
    /// Enum for selecting the type of emoji asset.
    /// </summary>
    public enum Type
    {
        Legacy,
        TextMeshPro
    }

    private SerializedObject serializedObject; // SerializedObject reference for this EditorWindow, used to manage and apply property changes.
    private GUIStyle textStyle; // GUI style for main section labels and headers, initialized dynamically in OnGUI.
    private GUIStyle foldoutStyle; // GUI style for foldout sections in the editor, initialized dynamically in OnGUI.

    public Vector2 scrollPosition = Vector2.zero; // Scroll position for the scrollable editor window.
    public bool showAssetEditor = true; // Toggle visibility of the Asset Editor section.
    public bool showSpriteEditor = true; // Toggle visibility of the Sprite Editor section.

    public SpriteAsset spriteAssetEdit; // Reference to the selected Legacy SpriteAsset for editing.
    public TMP_SpriteAsset TMP_spriteAssetEdit; // Reference to the selected TMP_SpriteAsset for editing (TextMeshPro).
    public Type emojiType = Type.Legacy; // Type of emoji asset being edited (Legacy or TextMeshPro).
    public float w; // Glyph width for editing metrics.
    public float h; // Glyph height for editing metrics.
    public float bx; // Horizontal bearing X for glyph metrics.
    public float by; // Horizontal bearing Y for glyph metrics.
    public float ad; // Horizontal advance for glyph metrics.
    public float scale; // Scale factor for the glyph.
    public int atlasIndex; // Index of the atlas texture the glyph belongs to.
    public Texture spriteEdit; // Texture to be used in the Sprite Editor for cutting into squares.
    public int size = 100; // Size in pixels of each square segment when cutting the sprite.

    #endregion

    #region === Window Initialization ===

    /// <summary>
    /// Opens the Emoji System Editor window in Unity's Window menu.
    /// </summary>
    [MenuItem("Window/Emoji System Editor", false, 2030)]
    public static void ShowWindow()
    {
        // Get or create an instance of the EmojiSystemEditor window.
        var window = GetWindow<EmojiSystemEditor>("Emoji System Editor");

        // Try to find a custom icon for the window in the project.
        string[] guids = AssetDatabase.FindAssets("EmojiSystemEditor Icon t:Texture2D");
        if (guids.Length > 0)
        {
            // If at least one matching asset is found, get its path from the GUID.
            string path = AssetDatabase.GUIDToAssetPath(guids[0]);

            // Load the texture asset from the path.
            Texture2D icon = AssetDatabase.LoadAssetAtPath<Texture2D>(path);

            // Set the window's title content with text and the icon.
            window.titleContent = new GUIContent("Emoji System Editor", icon);
        }
        else
        {
            // If no icon is found, set the title with text only.
            window.titleContent = new GUIContent("Emoji System Editor");
        }
    }

    /// <summary>
    /// Initializes the serialized object when the editor window is enabled.
    /// </summary>
    private void OnEnable() => serializedObject = new SerializedObject(this);

    #endregion

    #region === GUI Drawing ===

    /// <summary>
    /// Draws the Editor GUI including asset editor, glyph editor, 
    /// sprite editor, and scene application options.
    /// </summary>
    private void OnGUI()
    {
        serializedObject.Update(); // Update the serialized object at the start of GUI rendering to sync properties.
        scrollPosition = GUILayout.BeginScrollView(scrollPosition); // Begin a scroll view for the whole editor window.

        // Initialize main text style if not already created.
        textStyle ??= new(GUI.skin.label)
        {
            fontSize = 13, // Larger font for section headers.
            fontStyle = FontStyle.Bold, // Bold font for emphasis.
        };

        // Initialize foldout style if not already created.
        foldoutStyle ??= new(EditorStyles.foldout)
        {
            fontStyle = FontStyle.Bold, // Bold foldout headers.
            fontSize = 12 // Slightly smaller than main text style.
        };

        GUILayout.Label("Emoji System Editor", textStyle);

        GUILayout.Space(10);

        // Draw Asset Editor foldout header with a tooltip.
        showAssetEditor = EditorGUILayout.Foldout(showAssetEditor, new GUIContent("Asset Editor", "Open/Close the Asset Editor section"), true, foldoutStyle);
        if (showAssetEditor)
        {
            // Row for selecting the emoji type (Legacy or TextMeshPro).
            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(new GUIContent("Asset Type:", "Select whether to edit Legacy or TMP sprite assets"), GUILayout.Width(80));
            emojiType = (Type)EditorGUILayout.EnumPopup(emojiType, GUILayout.Width(110));
            GUILayout.EndHorizontal();

            GUILayout.Space(10);

            // Draw the Sprite selection section.
            DrawSection(
                "Sprite to Edit",
                true,
                true,
                () =>
                {
                    if (emojiType == Type.Legacy)
                    {
                        // Row for selecting Legacy sprite asset.
                        GUILayout.BeginHorizontal();
                        GUILayout.Label(new GUIContent("Sprite Asset:", "Select the Legacy SpriteAsset to edit"), GUILayout.Width(115));
                        spriteAssetEdit = (SpriteAsset)EditorGUILayout.ObjectField(spriteAssetEdit, typeof(SpriteAsset), false, GUILayout.Width(230));
                        GUILayout.EndHorizontal();
                    }
                    else if (emojiType == Type.TextMeshPro)
                    {
                        // Row for selecting TMP sprite asset.
                        GUILayout.BeginHorizontal();
                        GUILayout.Label(new GUIContent("Sprite Asset (TMP):", "Select the TMP SpriteAsset to edit"), GUILayout.Width(115));
                        TMP_spriteAssetEdit = (TMP_SpriteAsset)EditorGUILayout.ObjectField(TMP_spriteAssetEdit, typeof(TMP_SpriteAsset), false, GUILayout.Width(230));
                        GUILayout.EndHorizontal();
                    }
                });

            // Draw the Glyph Metrics Editor section.
            DrawSection(
                "Glyph Metrics Editor",
                true,
                HasValidSpriteAsset(), // Editable only if a valid asset is selected.
                () =>
                {
                    // Row for width (W) and height (H) of glyph.
                    GUILayout.BeginHorizontal();
                    GUILayout.Label(new GUIContent("W:", "Glyph width"), GUILayout.Width(25));
                    GUILayout.Space(5);
                    w = EditorGUILayout.FloatField(w, GUILayout.Width(139));

                    GUILayout.Label(new GUIContent("H:", "Glyph height"), GUILayout.Width(25));
                    GUILayout.Space(5);
                    h = EditorGUILayout.FloatField(h, GUILayout.Width(139));
                    GUILayout.EndHorizontal();

                    // Row for glyph bearings (BX, BY) and advance (AD).
                    GUILayout.BeginHorizontal();
                    GUILayout.Label(new GUIContent("BX:", "Horizontal bearing X"), GUILayout.Width(25));
                    GUILayout.Space(5);
                    bx = EditorGUILayout.FloatField(bx, GUILayout.Width(80));

                    GUILayout.Label(new GUIContent("BY:", "Horizontal bearing Y"), GUILayout.Width(25));
                    GUILayout.Space(5);
                    by = EditorGUILayout.FloatField(by, GUILayout.Width(80));

                    GUILayout.Label(new GUIContent("AD:", "Horizontal advance"), GUILayout.Width(25));
                    GUILayout.Space(5);
                    ad = EditorGUILayout.FloatField(ad, GUILayout.Width(80));
                    GUILayout.EndHorizontal();

                    GUILayout.Space(5);

                    // Row for scale factor and atlas index.
                    GUILayout.BeginHorizontal();
                    GUILayout.Label(new GUIContent("Scale:", "Scale factor of glyph"), GUILayout.Width(40));
                    GUILayout.Space(5);
                    scale = EditorGUILayout.FloatField(scale, GUILayout.Width(65));

                    GUILayout.Label(new GUIContent("Atlas Index:", "Index of the atlas texture"), GUILayout.Width(70));
                    GUILayout.Space(5);
                    atlasIndex = EditorGUILayout.IntField(atlasIndex, GUILayout.Width(35));
                    GUILayout.EndHorizontal();

                    GUILayout.Space(10);

                    // Save Asset button.
                    if (GUILayout.Button(new GUIContent("Save Asset", "Save the current glyph changes to the selected asset"), GUILayout.Width(120), GUILayout.Height(30)))
                    {
                        // Confirm before saving as this operation is irreversible.
                        bool saveConfirmed = EditorUtility.DisplayDialog("Save Confirmation", "Are you sure you want to save?\nIt will not be possible to undo this operation.", "Yes", "No");
                        if (saveConfirmed) SaveFile(); // Call SaveFile to write changes.
                    }
                });

            // Scene Edit section for TMP assets only.
            DrawSection(
                "Scene Edit",
                (emojiType == Type.TextMeshPro), // Visible only for TMP assets.                
                TMP_spriteAssetEdit != null, // Editable only if a TMP asset is selected.
                () =>
                {
                    if (GUILayout.Button(new GUIContent("Apply to Scene", "Apply the TMP SpriteAsset to all TMP_Text objects in the scene"), GUILayout.Width(120), GUILayout.Height(30)))
                    {
                        bool applyToScene = EditorUtility.DisplayDialog("Apply Confirmation", "Are you sure you want to do this?\nThis will replace all SpriteAsset of all texts in the scene and it will not be possible to undo this operation.", "Yes", "No");
                        if (applyToScene) ApplyToAllObjects(); // Replace sprite assets in all TMP_Text objects.
                    }
                });
        }

        GUILayout.Space(10);

        // Sprite Editor foldout.
        showSpriteEditor = EditorGUILayout.Foldout(showSpriteEditor, new GUIContent("Sprite Editor", "Open/Close the Sprite Editor section"), true, foldoutStyle);
        if (showSpriteEditor)
        {
            DrawSection(
            "Sprite Editor",
            true,
            true,
            () =>
            {
                GUILayout.Label("Cut sprite into square segments directly in Unity.");
                GUILayout.Space(5);

                // Row for selecting texture and size.
                GUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(new GUIContent("Sprite:", "Select the texture to cut into squares"), GUILayout.Width(40));
                spriteEdit = (Texture)EditorGUILayout.ObjectField(spriteEdit, typeof(Texture), false, GUILayout.Width(170));
                GUILayout.Space(15);
                EditorGUILayout.LabelField(new GUIContent("Size:", "Size of each square segment in pixels"), GUILayout.Width(40));
                size = EditorGUILayout.IntField(size, GUILayout.Width(76));
                GUILayout.EndHorizontal();

                GUILayout.Space(5);

                // Apply button to cut sprite into squares and save to folder.
                GUILayout.BeginHorizontal();
                GUI.enabled = (spriteEdit != null); // Enable button only if texture is selected.
                if (GUILayout.Button(new GUIContent("Apply to Folder", "Cut the texture into squares and save into selected folder"), GUILayout.Width(120), GUILayout.Height(30)))
                {
                    string folderPath = EditorUtility.OpenFolderPanel("Select Folder to Apply Changes", "", "");
                    CutImageIntoSquares(spriteEdit, size, folderPath); // Call cutting method.
                }
                GUI.enabled = true; // Re-enable GUI.
                GUILayout.EndHorizontal();
            });
        }

        GUILayout.EndScrollView();
        serializedObject.ApplyModifiedProperties(); // Apply any changed serialized properties.
    }

    /// <summary>
    /// Draws a foldable section box in the editor.
    /// </summary>
    /// <param name="title">Title of the section.</param>
    /// <param name="visible">Whether the section should be visible.</param>
    /// <param name="enabled">Whether the section contents should be editable.</param>
    /// <param name="drawAction">Action that draws the section's GUI contents.</param>
    private void DrawSection(string title, bool visible, bool enabled, System.Action drawAction)
    {
        if (!visible) return; // Skip drawing if section is not visible.

        GUILayout.Space(15); // Add spacing before section.
        GUILayout.Label(title, textStyle); // Draw section title.
        GUILayout.Space(5); // Add spacing after title.

        GUI.enabled = enabled; // Enable/disable GUI elements based on section state.

        EditorGUILayout.BeginVertical("box"); // Draw box around section content.
        drawAction?.Invoke(); // Draw the contents provided by lambda action.
        EditorGUILayout.EndVertical(); // Close box.

        GUI.enabled = true; // Reset GUI to enabled.
    }

    #endregion

    #region === Glyph Editing ===

    /// <summary>
    /// Saves the modified glyph metrics to the selected asset.
    /// </summary>
    private void SaveFile()
    {
        // Check if editing a Legacy sprite asset and the asset reference is valid.
        if (emojiType == Type.Legacy && spriteAssetEdit != null)
        {
            UpdateGlyphMetrics(spriteAssetEdit.spriteGlyphTable); // Update glyph metrics using the values set in the editor.
            SaveAsset(spriteAssetEdit); // Save changes to the Legacy SpriteAsset.
        }
        // Check if editing a TextMeshPro sprite asset and the asset reference is valid.
        else if (emojiType == Type.TextMeshPro && TMP_spriteAssetEdit != null)
        {
            UpdateGlyphMetrics(TMP_spriteAssetEdit.spriteGlyphTable); // Update glyph metrics using the values set in the editor.
            SaveAsset(TMP_spriteAssetEdit); // Save changes to the TMP SpriteAsset.
        }
    }

    /// <summary>
    /// Updates the glyph metrics for each glyph in the sprite glyph table.
    /// </summary>
    /// <typeparam name="T">Type of glyph (Legacy or TMP).</typeparam>
    /// <param name="spriteGlyphTable">List of glyphs to update.</param>
    private void UpdateGlyphMetrics<T>(List<T> spriteGlyphTable) where T : Glyph
    {
        // Iterate through each glyph in the table.
        foreach (var glyph in spriteGlyphTable)
        {
            // Only update metrics if they exist for the glyph.
            if (glyph.metrics != null)
            {
                // Create a new GlyphMetrics object with values from editor fields.
                GlyphMetrics customMetrics = new()
                {
                    width = w,               // Set glyph width.
                    height = h,              // Set glyph height.
                    horizontalBearingX = bx, // Set horizontal bearing X.
                    horizontalBearingY = by, // Set horizontal bearing Y.
                    horizontalAdvance = ad   // Set horizontal advance.
                };

                // Assign the new metrics to the glyph.
                glyph.metrics = customMetrics;
            }

            // Update additional glyph properties.
            glyph.scale = scale; // Set scale factor of the glyph.
            glyph.atlasIndex = atlasIndex; // Set atlas index.
        }
    }

    /// <summary>
    /// Marks the asset as dirty and saves all changes to the AssetDatabase.
    /// </summary>
    /// <param name="asset">Asset to save.</param>
    private void SaveAsset(Object asset)
    {
        EditorUtility.SetDirty(asset); // Mark the asset as modified so Unity knows it needs saving.
        AssetDatabase.SaveAssets(); // Save all pending asset changes to the AssetDatabase.
        AssetDatabase.Refresh(); // Refresh the AssetDatabase to reflect changes in the editor.
    }

    /// <summary>
    /// Applies the selected TMP sprite asset to all TMP_Text objects in the current scene.
    /// </summary>
    private void ApplyToAllObjects()
    {
        // Find all GameObjects currently loaded, including inactive and prefab instances.
        var allObjectsInScene = Resources.FindObjectsOfTypeAll<GameObject>();

        // Get the active scene to ensure we only modify objects in the current scene.
        var activeScene = SceneManager.GetActiveScene();

        int modifiedCount = 0; // Counter for how many objects were updated.

        // Iterate through each object in the scene.
        foreach (var gameObject in allObjectsInScene)
        {
            // Only consider objects that belong to the active scene.
            if (gameObject.scene == activeScene)
            {
                // Try to get a TMP_Text component from the object.
                if (gameObject.TryGetComponent<TMP_Text>(out var text))
                {
                    // Apply the selected TMP sprite asset.
                    text.spriteAsset = TMP_spriteAssetEdit;
                    modifiedCount++; // Increment counter.
                }
            }
        }

        // If at least one object was modified, mark the scene dirty.
        if (modifiedCount > 0)
        {
            EditorSceneManager.MarkSceneDirty(activeScene);
            Debug.Log($"Applied TMP_SpriteAsset to {modifiedCount} objects and marked scene as dirty.");
        }
        else
        {
            Debug.Log("No TMP_Text objects found in the current scene to update.");
        }
    }

    /// <summary>
    /// Checks if the currently selected sprite asset is valid.
    /// </summary>
    /// <returns>True if a valid sprite asset is selected, false otherwise.</returns>
    private bool HasValidSpriteAsset()
    {
        // Use a switch expression to return true if the corresponding asset exists.
        return emojiType switch
        {
            Type.Legacy => spriteAssetEdit != null, // Legacy asset check.
            Type.TextMeshPro => TMP_spriteAssetEdit != null, // TMP asset check.
            _ => false
        };
    }

    #endregion

    #region === Sprite Cutting ===

    /// <summary>
    /// Cuts the selected texture into square segments and saves them to the destination folder.
    /// </summary>
    /// <param name="texture">Texture to cut.</param>
    /// <param name="squareSize">Size of each square segment in pixels.</param>
    /// <param name="destinationFolder">Folder to save the cropped squares.</param>
    private void CutImageIntoSquares(Texture texture, int squareSize, string destinationFolder)
    {
        // Ensure a texture is selected.
        if (texture == null)
        {
            Debug.LogError("No texture selected for cropping.");
            return;
        }

        var sourceTexture = LoadTextureFromAssetFile(texture); // Attempt to load a readable version of the texture from its asset file.

        // If loading from disk fails, create a readable copy from GPU memory.
        if (sourceTexture == null)
        {
            Debug.LogWarning("Failed to load texture from file. Falling back to readable GPU copy.");
            sourceTexture = GetReadableTexture((Texture2D)texture);
        }

        // Abort if a readable texture could not be obtained.
        if (sourceTexture == null)
        {
            Debug.LogError("Unable to obtain a readable source texture.");
            return;
        }

        // Get texture dimensions.
        int width = sourceTexture.width;
        int height = sourceTexture.height;

        // Validate square size.
        if (squareSize <= 0)
        {
            Debug.LogError("Square size must be greater than zero.");
            return;
        }

        // Calculate how many squares can fit horizontally and vertically.
        int numberSquaresHorizontal = width / squareSize;
        int numberSquaresVertical = height / squareSize;

        // Ensure at least one square can fit in both dimensions.
        if (numberSquaresHorizontal <= 0 || numberSquaresVertical <= 0)
        {
            Debug.LogError($"Square size {squareSize} is larger than image dimensions {width}x{height}.");
            return;
        }

        // Create destination folder if it does not exist.
        if (!Directory.Exists(destinationFolder)) Directory.CreateDirectory(destinationFolder);

        // Get all pixels of the source texture at once (optimized).
        var allPixels = sourceTexture.GetPixels32();

        // Loop through each row of squares.
        for (int row = 0; row < numberSquaresVertical; row++)
        {
            int startY_topOrigin = row * squareSize; // Calculate Y starting position from top.
            int startY = height - startY_topOrigin - squareSize; // Convert to bottom-left origin for Unity's texture coordinates.

            // Loop through each column of squares.
            for (int col = 0; col < numberSquaresHorizontal; col++)
            {
                int startX = col * squareSize; // X starting position for this square.

                // Skip squares that are out-of-bounds due to rounding.
                if (startX < 0 || startY < 0 || startX + squareSize > width || startY + squareSize > height)
                {
                    Debug.LogWarning($"Skipping out-of-bounds square at row {row}, col {col} (startX={startX}, startY={startY}).");
                    continue;
                }

                // Create a temporary array to store pixels for this square.
                var croppedPixels = new Color32[squareSize * squareSize];

                // Copy pixels from the source texture to the cropped array.
                for (int y = 0; y < squareSize; y++)
                {
                    int sourceY = startY + y; // Current source row in the texture.
                    int sourceIndex = sourceY * width + startX; // Start index in source pixel array.
                    int targetIndex = y * squareSize; // Start index in cropped pixel array for this row.

                    // Copy a row of pixels from source to target.
                    System.Array.Copy(allPixels, sourceIndex, croppedPixels, targetIndex, squareSize);
                }

                // Create a new Texture2D for the cropped square.
                Texture2D cropped = new(squareSize, squareSize, TextureFormat.RGBA32, false);
                cropped.SetPixels32(croppedPixels); // Assign pixel data.
                cropped.Apply(); // Apply changes to the texture.

                // Encode the cropped texture to PNG format.
                byte[] pngData = cropped.EncodeToPNG();
                string fileName = $"square_{row}_{col}.png";
                string filePath = Path.Combine(destinationFolder, fileName);

                try
                {
                    File.WriteAllBytes(filePath, pngData); // Write PNG file to disk.
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"Failed to write file '{filePath}': {e.Message}");
                }

                DestroyImmediate(cropped); // Destroy the temporary cropped texture to free memory.
            }
        }

        // Destroy the source texture if it was a temporary readable copy.
        if (!ReferenceEquals(sourceTexture, texture)) DestroyImmediate(sourceTexture);

        // Refresh AssetDatabase to show new files in the Unity Editor.
        AssetDatabase.Refresh();
        Debug.Log("Images successfully applied to folder (optimized with SetPixels32).");
    }

    /// <summary>
    /// Loads a texture from disk using its AssetDatabase path.
    /// </summary>
    /// <param name="texture">Texture asset to load.</param>
    /// <returns>Readable Texture2D, or null if loading fails.</returns>
    private Texture2D LoadTextureFromAssetFile(Texture texture)
    {
        try
        {
            string assetPath = AssetDatabase.GetAssetPath(texture); // Get asset path.
            if (string.IsNullOrEmpty(assetPath)) return null;

            // Get project root path.
            string projectRoot = Directory.GetParent(Application.dataPath).FullName;

            // Attempt to construct full path.
            string fullPath = Path.Combine(projectRoot, assetPath);

            // Fallback if file does not exist.
            if (!File.Exists(fullPath))
            {
                fullPath = Path.Combine(projectRoot, assetPath.TrimStart(Path.DirectorySeparatorChar, '/'));
                if (!File.Exists(fullPath))
                {
                    Debug.LogWarning($"Original file not found on disk: {fullPath}");
                    return null;
                }
            }

            // Read raw file data into byte array.
            byte[] fileData = File.ReadAllBytes(fullPath);

            // Create temporary Texture2D and load data.
            Texture2D tex = new(2, 2, TextureFormat.RGBA32, false);
            if (!tex.LoadImage(fileData))
            {
                Debug.LogWarning($"Texture.LoadImage failed for file: {fullPath}");
                Object.DestroyImmediate(tex);
                return null;
            }

            return tex; // Return loaded texture.
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"LoadTextureFromAssetFile failed: {e.Message}");
            return null;
        }
    }

    /// <summary>
    /// Creates a readable copy of a GPU texture for manipulation.
    /// </summary>
    /// <param name="texture">Source texture.</param>
    /// <returns>Readable Texture2D copy.</returns>
    private Texture2D GetReadableTexture(Texture2D texture)
    {
        if (texture == null) return null;

        // Create a temporary RenderTexture with same dimensions.
        var tmp = RenderTexture.GetTemporary(texture.width, texture.height, 0, RenderTextureFormat.Default, RenderTextureReadWrite.Linear);

        Graphics.Blit(texture, tmp); // Copy GPU texture to the RenderTexture.

        // Save current active RenderTexture.
        var previous = RenderTexture.active;
        RenderTexture.active = tmp;

        // Create a new readable Texture2D.
        Texture2D readable = new(texture.width, texture.height, TextureFormat.RGBA32, false);
        readable.ReadPixels(new Rect(0, 0, tmp.width, tmp.height), 0, 0); // Copy pixels from RenderTexture.
        readable.Apply(); // Apply changes.

        // Restore previous active RenderTexture and release temporary one.
        RenderTexture.active = previous;
        RenderTexture.ReleaseTemporary(tmp);

        return readable; // Return readable texture copy.
    }

    #endregion
}