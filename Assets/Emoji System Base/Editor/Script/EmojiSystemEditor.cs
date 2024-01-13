using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TextCore.Text;
using UnityEngine.TextCore;
/**/
using TMPro;
/**/

public class EmojiSystemEditor : EditorWindow
{
    private SerializedObject serializedObject; // Serialized object to track modifications in the Inspector.
    public Type emojiType = Type.Legacy; // Emoji type (Legacy or TextMeshPro).

    public SpriteAsset itemEdit; // SpriteAsset object for Legacy emojis.
    /**/
    public TMP_SpriteAsset TMP_itemEdit; // TMP_SpriteAsset object for TextMeshPro emojis.
    /**/

    // Glyph metrics variables.
    public float w; // Custom width for Glyph metrics.
    public float h; // Custom height for Glyph metrics.
    public float bx; // Custom X Bearing for Glyph metrics.
    public float by; // Custom Y bearing for Glyph metrics.
    public float ad; // Custom horizontal feed for Glyph metrics.
    public float scale; // Custom scale for the Glyph.
    public int atlasIndex; // Custom atlas index for Glyph.

    public Vector2 scrollPosition = Vector2.zero; // Scroll position of the ScrollView.
    public enum Type { Legacy, TextMeshPro } // Enum defining the two types of emoji systems: Legacy and TextMeshPro.

    [MenuItem("Window/Emoji System Editor", false, 2030)]
    public static void ShowWindow()
    {
        // Creates an editor window for the Emoji System Editor.
        EmojiSystemEditor window = GetWindow<EmojiSystemEditor>("Emoji System Editor");
        Texture2D icon = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Emoji System Base/Editor/Icon/EmojiSystemEditor Icon.png");
        window.titleContent = new GUIContent("Emoji System Editor", icon);
    }

    private void OnEnable() { serializedObject = new SerializedObject(this); } // Initializes the serialized object.

    private void OnGUI()
    {
        serializedObject.Update(); // Updates the serialized object.
        scrollPosition = GUILayout.BeginScrollView(scrollPosition); // Begins the ScrollView.

        GUILayout.Space(10);
        EditorGUILayout.LabelField("Emoji Type");
        emojiType = (Type)EditorGUILayout.EnumPopup(emojiType, GUILayout.Width(200));

        GUILayout.Space(20);

        switch (emojiType)
        {
            case Type.Legacy:
                EditorGUILayout.LabelField("Legacy Sprite Asset");
                EditorGUILayout.PropertyField(serializedObject.FindProperty("itemEdit")); // Displays the "itemEdit" field in the Inspector and binds it to the corresponding property.
                break;

            case Type.TextMeshPro:
                /**/
                EditorGUILayout.LabelField("TextMeshPro Sprite Asset");
                EditorGUILayout.PropertyField(serializedObject.FindProperty("TMP_itemEdit")); // Displays the "TMP_itemEdit" field in the Inspector and binds it to the corresponding property.
                /**/
                break;
        }

        // GUI for glyph metrics customization.
        GUILayout.Space(20);
        EditorGUILayout.LabelField("Sprite Glyph Table Editor");

        // GUI for glyph metric values.
        GUILayout.BeginHorizontal();
        GUILayout.Label("W:", GUILayout.Width(20));
        GUILayout.Space(5);
        w = EditorGUILayout.FloatField(w, GUILayout.Width(144));

        GUILayout.Label("H:", GUILayout.Width(20));
        GUILayout.Space(5);
        h = EditorGUILayout.FloatField(h, GUILayout.Width(144));
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
        scale = EditorGUILayout.FloatField(scale, GUILayout.Width(40));

        GUILayout.Label("Atlas Index:", GUILayout.Width(70));
        GUILayout.Space(5);
        atlasIndex = EditorGUILayout.IntField(atlasIndex, GUILayout.Width(20));
        GUILayout.EndHorizontal();

        GUILayout.Space(40);

        // Button to save changes.
        if (GUILayout.Button("Save"))
        {
            // Confirmation dialog before saving.
            bool saveConfirmed = EditorUtility.DisplayDialog("Save Confirmation", "Are you sure you want to save?\nThis will not be applied to all Sprite Glyph Tables in SpriteAsset.", "Yes", "No");
            if (saveConfirmed) { Save(); }
        }

        /**/
        // Button to apply changes to TextMeshPro texts in the scene.
        if (emojiType == Type.TextMeshPro)
        {
            GUILayout.Space(20);
            if (GUILayout.Button("Apply to Scene"))
            {
                // Confirmation dialog before applying changes.
                bool applyToScene = EditorUtility.DisplayDialog("Apply Confirmation", "Are you sure you want to do this?\nThis will replace all SpriteAsset of all texts in the scene and it will not be possible to undo this operation.", "Yes", "No");
                if (applyToScene) { ChangeTheSpriteAssetOfTextOnAllObjects(); }
            }
        }
        /**/

        serializedObject.ApplyModifiedProperties(); // Applies the modifications made to the serialized object.
        GUILayout.EndScrollView(); // Ends the ScrollView.
    }

    // Method to save changes made to emoji assets.
    private void Save()
    {
        switch (emojiType)
        {
            case Type.Legacy:
                if (itemEdit != null)
                {
                    foreach (SpriteGlyph glyph in itemEdit.spriteGlyphTable)
                    {
                        if (glyph.GetType().GetProperty("metrics") != null)
                        {
                            // Creates a custom set of metrics for Glyph.
                            GlyphMetrics customMetrics = new()
                            {
                                width = w,
                                height = h,
                                horizontalBearingX = bx,
                                horizontalBearingY = by,
                                horizontalAdvance = ad
                            };

                            glyph.GetType().GetProperty("metrics").SetValue(glyph, customMetrics); // Defines custom metrics for the Glyph.
                        }

                        // Applies the atlas scale and index to the Glyph.
                        glyph.scale = scale;
                        glyph.atlasIndex = atlasIndex;
                    }

                    EditorUtility.SetDirty(itemEdit); // Mark the scriptable object as dirty to ensure changes are saved.                    

                    // Save assets and refresh the Asset Database.
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();
                }
                break;

            case Type.TextMeshPro:
                /**/
                if (TMP_itemEdit != null)
                {
                    foreach (TMP_SpriteGlyph glyph in TMP_itemEdit.spriteGlyphTable)
                    {
                        if (glyph.metrics != null)
                        {
                            // Creates a custom set of metrics for the TextMeshPro Glyph.
                            GlyphMetrics customMetrics = new()
                            {
                                width = w,
                                height = h,
                                horizontalBearingX = bx,
                                horizontalBearingY = by,
                                horizontalAdvance = ad
                            };
                            
                            glyph.metrics = customMetrics; // Defines custom metrics for the TextMeshPro Glyph.
                        }

                        // Applies the atlas scale and index to the TextMeshPro Glyph.
                        glyph.scale = scale;
                        glyph.atlasIndex = atlasIndex;
                    }

                    EditorUtility.SetDirty(TMP_itemEdit); // Mark the scriptable object as dirty to ensure changes are saved.                    
                    
                    // Save assets and refresh the Asset Database.
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();
                }
                /**/
                break;
        }

        // Confirmation dialog after saving.
        EditorUtility.DisplayDialog("Save Completed", "Settings have been saved.\nPlease interact with SpriteAsset to keep the values saved.", "OK");
    }

    /**/
    // Method to apply changes to TextMeshPro texts in the scene.
    private void ChangeTheSpriteAssetOfTextOnAllObjects()
    {
        GameObject[] allObjectsInScene = Resources.FindObjectsOfTypeAll<GameObject>(); // Find all objects in the scene.

        // Iterate through all objects and update TextMeshPro texts.
        foreach (GameObject gameObject in allObjectsInScene)
        {
            if (gameObject.scene == SceneManager.GetActiveScene())
            {
                if (gameObject.TryGetComponent<TMP_Text>(out var text)) { text.spriteAsset = TMP_itemEdit; }
                Debug.Log("Apply to current Scene Successfully.");
            }
        }
    }
    /**/
}