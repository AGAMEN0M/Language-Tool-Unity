/*
 * ---------------------------------------------------------------------------
 * Description: Utility class for serializing, reconstructing and validating 
 *              Unity Canvas hierarchies. Provides functionality to extract 
 *              layout metadata, apply saved structures to existing objects, 
 *              and instantiate canvases programmatically using structured 
 *              data formats. Ensures hierarchy integrity through duplicate 
 *              name detection and preserves component configurations such as 
 *              CanvasScaler and GraphicRaycaster.
 * Author: Lucas Gomes Cecchini
 * Pseudonym: AGAMENOM
 * ---------------------------------------------------------------------------
*/

using System.Collections.Generic;
using UnityEngine.UI;
using System.Linq;
using UnityEngine;

namespace LanguageTools
{
    public class CanvasManager
    {
        // Name prefix used by TextMeshPro to create internal layout elements; excluded from hierarchy processing.
        private const string subMeshUI = "TMP SubMeshUI";

        /// <summary>
        /// Extracts Canvas metadata and hierarchy from a GameObject into a CanvasStructure.
        /// </summary>
        /// <param name="canvasStructure">Structure to populate with extracted data.</param>
        /// <param name="canvasObject">Canvas GameObject source.</param>
        public static void ExtractCanvasData(ref CanvasStructure canvasStructure, GameObject canvasObject)
        {
            // Initialize an empty CanvasStructure instance.
            canvasStructure = new()
            {
                canvasLayers = new CanvasLayers[0],
                rectTransform = new RectTransformData(),
                canvas = new CanvasData(),
                canvasScaler = new CanvasScalerData(),
                graphicRaycaster = new GraphicRaycasterData()
            };

            // Validate uniqueness of sibling names to prevent hierarchy issues.
            if (ContainsDuplicateSiblings(canvasObject.GetComponent<RectTransform>()))
            {
                Debug.LogError("Extraction failed: Duplicate layer names in hierarchy.");
                return;
            }

            // Set canvas name.
            canvasStructure.canvasName = canvasObject.name;

            // Store original active states and temporarily activate all children for hierarchy traversal.
            Dictionary<GameObject, bool> originalStates = new();
            ActivateAllChildren(canvasObject.transform, originalStates);

            // Extract metadata from core canvas components.
            PopulateCanvasMetadata(ref canvasStructure, canvasObject);

            // Extract and build hierarchy layers.
            canvasStructure.canvasLayers = GenerateCanvasLayers(canvasObject);

            // Restore original active states.
            foreach (var kvp in originalStates) kvp.Key.SetActive(kvp.Value);
        }

        /// <summary>
        /// Applies a CanvasStructure's layout and metadata to an existing Canvas GameObject.
        /// </summary>
        /// <param name="canvasStructure">Data structure to apply.</param>
        /// <param name="canvasObject">Target Canvas GameObject.</param>
        public static void ApplyCanvasData(CanvasStructure canvasStructure, GameObject canvasObject)
        {
            // Validate data consistency before applying.
            if (canvasStructure.canvasLayers == null || canvasStructure.canvasLayers.Length == 0 || canvasStructure.canvasLayers.Any(l => l.CanvasObjectsLayers.Length != l.rectTransforms.Length))
            {
                Debug.LogError("Invalid layers: Mismatched CanvasObjectsLayers and rectTransforms.");
                return;
            }

            // Apply canvas component metadata.
            UpdateCanvasMetadata(canvasStructure, canvasObject);

            var root = canvasObject.GetComponent<RectTransform>();

            // Traverse the hierarchy and apply RectTransform data to each matching element.
            foreach (var layer in canvasStructure.canvasLayers)
            {
                var parent = root;

                for (int i = 0; i < layer.CanvasObjectsLayers.Length; i++)
                {
                    string name = layer.CanvasObjectsLayers[i];
                    var existing = FindChildByName(parent, name);

                    if (existing != null && existing.TryGetComponent(out RectTransform rect))
                    {
                        // Apply transform data to matching element.
                        var data = layer.rectTransforms[i];
                        rect.localPosition = data.localPosition;
                        rect.localRotation = data.localRotation;
                        rect.localScale = data.localScale;
                        rect.anchorMin = data.anchorMin;
                        rect.anchorMax = data.anchorMax;
                        rect.anchoredPosition = data.anchoredPosition;
                        rect.sizeDelta = data.sizeDelta;
                        rect.pivot = data.pivot;

                        // Proceed to next child in hierarchy.
                        parent = rect;
                    }
                    else
                    {
                        Debug.LogWarning($"Missing layer: '{name}' not found.");
                    }
                }
            }
        }

        /// <summary>
        /// Instantiates a new Canvas GameObject using data from a CanvasStructure.
        /// </summary>
        /// <param name="canvasStructure">Structure containing Canvas layout and metadata.</param>
        /// <param name="canvasObject">Created Canvas GameObject.</param>
        public static void CreateCanvasFromStructure(CanvasStructure canvasStructure, out GameObject canvasObject)
        {
            canvasObject = null;

            // Ensure valid canvas name.
            if (string.IsNullOrEmpty(canvasStructure.canvasName))
            {
                Debug.LogError("Creation failed: Canvas name is not defined.");
                return;
            }

            // Validate structural consistency before creating the hierarchy.
            if (canvasStructure.canvasLayers == null || canvasStructure.canvasLayers.Length == 0 || canvasStructure.canvasLayers.Any(l => l.CanvasObjectsLayers.Length != l.rectTransforms.Length))
            {
                Debug.LogError("Creation failed: Mismatched CanvasObjectsLayers and rectTransforms.");
                return;
            }

            var colorMap = new Dictionary<int, Color>();

            // Create root canvas GameObject and attach required components.
            canvasObject = new GameObject(canvasStructure.canvasName, typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            UpdateCanvasMetadata(canvasStructure, canvasObject);
            var root = canvasObject.GetComponent<RectTransform>();

            // Build canvas hierarchy layer by layer.
            foreach (var layer in canvasStructure.canvasLayers)
            {
                var parent = root;

                for (int i = 0; i < layer.CanvasObjectsLayers.Length; i++)
                {
                    string name = layer.CanvasObjectsLayers[i];
                    var existing = FindChildByName(parent, name);

                    if (existing != null)
                    {
                        parent = existing;
                        continue;
                    }

                    // Instantiate new GameObject for layer element.
                    var go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Outline));

                    if (go.TryGetComponent(out RectTransform rect))
                    {
                        rect.SetParent(parent != null ? parent : root, false);

                        // Apply RectTransform data if available.
                        if (i < layer.rectTransforms.Length)
                        {
                            var data = layer.rectTransforms[i];
                            rect.localPosition = data.localPosition;
                            rect.localRotation = data.localRotation;
                            rect.localScale = data.localScale;
                            rect.anchorMin = data.anchorMin;
                            rect.anchorMax = data.anchorMax;
                            rect.anchoredPosition = data.anchoredPosition;
                            rect.sizeDelta = data.sizeDelta;
                            rect.pivot = data.pivot;
                        }

                        // Assign a random debug color to the Image component for visual clarity in the Editor.
                        if (go.TryGetComponent(out Image image))
                        {
                            if (!colorMap.ContainsKey(i)) colorMap[i] = new Color(Random.value, Random.value, Random.value);
                            image.color = colorMap[i];
                        }

                        // Add outline effect for visibility.
                        if (go.TryGetComponent(out Outline outline)) outline.effectDistance = new Vector2(3, 3);

                        parent = rect;
                    }
                }
            }
        }

        /// <summary>
        /// Generates CanvasLayers from a Canvas GameObject hierarchy.
        /// </summary>
        private static CanvasLayers[] GenerateCanvasLayers(GameObject canvasObject)
        {
            var paths = new List<List<RectTransform>>();

            // Recursively collect all RectTransform paths from root to each leaf node.
            void Collect(RectTransform parent, List<RectTransform> path)
            {
                // Ignore objects whose name starts with subMeshUI.
                if (parent.name.StartsWith(subMeshUI)) return;

                path.Add(parent);

                bool hasValidChild = false;

                foreach (RectTransform child in parent)
                {
                    if (!child.name.StartsWith(subMeshUI))
                    {
                        hasValidChild = true;
                        Collect(child, path);
                    }
                }

                if (!hasValidChild) paths.Add(new(path));

                path.RemoveAt(path.Count - 1);
            }

            foreach (RectTransform child in canvasObject.transform)
            {
                if (!child.name.StartsWith(subMeshUI))
                {
                    Collect(child, new());
                }
            }

            // Convert collected paths into CanvasLayers.
            return paths.Select(p => new CanvasLayers
            {
                CanvasObjectsLayers = p.Select(t => t.name).ToArray(),
                rectTransforms = p.Select(ConvertToRectTransformData).ToArray()
            }).ToArray();
        }

        /// <summary>
        /// Returns true if any sibling in the hierarchy has a duplicate name,
        /// ignoring objects that contain subMeshUI in their name.
        /// </summary>
        private static bool ContainsDuplicateSiblings(RectTransform parent)
        {
            HashSet<string> names = new();

            foreach (RectTransform child in parent)
            {
                // Skip children with names that contain subMeshUI.
                if (child.name.Contains(subMeshUI)) continue;

                // Check for duplicate names among siblings at this level.
                if (!names.Add(child.name))
                {
                #if UNITY_EDITOR
                    UnityEditor.Selection.activeObject = child;
                    Debug.LogWarning($"Duplicate: {child.name}", child.gameObject);
                #endif
                    return true;
                }

                // Recursively check child levels.
                if (ContainsDuplicateSiblings(child)) return true;
            }

            return false;
        }

        /// <summary>
        /// Converts a RectTransform into RectTransformData.
        /// </summary>
        private static RectTransformData ConvertToRectTransformData(RectTransform t) => new()
        {
            localPosition = t.localPosition,
            localRotation = t.localRotation,
            localScale = t.localScale,
            anchorMin = t.anchorMin,
            anchorMax = t.anchorMax,
            anchoredPosition = t.anchoredPosition,
            sizeDelta = t.sizeDelta,
            pivot = t.pivot
        };

        /// <summary>
        /// Recursively activates all children and stores their original active states.
        /// </summary>
        /// <param name="parent">Parent transform to start from.</param>
        /// <param name="states">Dictionary to store original active states.</param>
        private static void ActivateAllChildren(Transform parent, Dictionary<GameObject, bool> states)
        {
            foreach (Transform child in parent)
            {
                if (!child.name.StartsWith(subMeshUI))
                {
                    GameObject go = child.gameObject;
                    if (!states.ContainsKey(go))
                    {
                        states[go] = go.activeSelf;
                        if (!go.activeSelf) go.SetActive(true);
                    }

                    ActivateAllChildren(child, states);
                }
            }
        }

        /// <summary>
        /// Finds a child RectTransform by name.
        /// </summary>
        private static RectTransform FindChildByName(RectTransform parent, string name)
        {
            foreach (RectTransform child in parent)
                if (child.name == name) return child;
            return null;
        }

        /// <summary>
        /// Populates CanvasStructure with metadata from a Canvas GameObject.
        /// </summary>
        private static void PopulateCanvasMetadata(ref CanvasStructure s, GameObject obj)
        {
            // Populate structure with RectTransform data.
            if (obj.TryGetComponent(out RectTransform rt)) s.rectTransform = ConvertToRectTransformData(rt);

            // Extract Canvas settings.
            if (obj.TryGetComponent(out Canvas canvas))
            {
                var c = s.canvas;
                c.renderMode = canvas.renderMode;
                c.planeDistance = canvas.planeDistance;
                c.pixelPerfect = canvas.pixelPerfect;
                c.overrideSorting = canvas.overrideSorting;
                c.overridePixelPerfect = canvas.overridePixelPerfect;
                c.sortingBucketNormalizedSize = canvas.normalizedSortingGridSize;
                c.vertexColorAlwaysGammaSpace = canvas.vertexColorAlwaysGammaSpace;
                c.additionalShaderChannels = canvas.additionalShaderChannels;
                c.updateRectTransformForStandalone = canvas.updateRectTransformForStandalone;
            }

            // Extract CanvasScaler settings.
            if (obj.TryGetComponent(out CanvasScaler scaler))
            {
                var sData = s.canvasScaler;
                sData.uiScaleMode = scaler.uiScaleMode;
                sData.referencePixelsPerUnit = scaler.referencePixelsPerUnit;
                sData.scaleFactor = scaler.scaleFactor;
                sData.referenceResolution = scaler.referenceResolution;
                sData.screenMatchMode = scaler.screenMatchMode;
                sData.matchWidthOrHeight = scaler.matchWidthOrHeight;
                sData.physicalUnit = scaler.physicalUnit;
                sData.fallbackScreenDPI = scaler.fallbackScreenDPI;
                sData.defaultSpriteDPI = scaler.defaultSpriteDPI;
                sData.dynamicPixelsPerUnit = scaler.dynamicPixelsPerUnit;
            }

            // Extract GraphicRaycaster settings.
            if (obj.TryGetComponent(out GraphicRaycaster raycaster))
            {
                var g = s.graphicRaycaster;
                g.ignoreReversedGraphics = raycaster.ignoreReversedGraphics;
                g.blockingObjects = raycaster.blockingObjects;
                g.blockingMask = raycaster.blockingMask;
            }
        }

        /// <summary>
        /// Applies metadata from a CanvasStructure to a Canvas GameObject.
        /// </summary>
        private static void UpdateCanvasMetadata(CanvasStructure s, GameObject obj)
        {
            // Apply RectTransform data.
            if (obj.TryGetComponent(out RectTransform rt))
            {
                var data = s.rectTransform;
                rt.localPosition = data.localPosition;
                rt.localRotation = data.localRotation;
                rt.localScale = data.localScale;
                rt.anchorMin = data.anchorMin;
                rt.anchorMax = data.anchorMax;
                rt.anchoredPosition = data.anchoredPosition;
                rt.sizeDelta = data.sizeDelta;
                rt.pivot = data.pivot;
            }

            // Apply Canvas settings.
            if (obj.TryGetComponent(out Canvas canvas))
            {
                var c = s.canvas;
                canvas.renderMode = c.renderMode;
                canvas.planeDistance = c.planeDistance;
                canvas.pixelPerfect = c.pixelPerfect;
                canvas.overrideSorting = c.overrideSorting;
                canvas.overridePixelPerfect = c.overridePixelPerfect;
                canvas.normalizedSortingGridSize = c.sortingBucketNormalizedSize;
                canvas.vertexColorAlwaysGammaSpace = c.vertexColorAlwaysGammaSpace;
                canvas.additionalShaderChannels = c.additionalShaderChannels;
                canvas.updateRectTransformForStandalone = c.updateRectTransformForStandalone;
            }

            // Apply CanvasScaler settings.
            if (obj.TryGetComponent(out CanvasScaler scaler))
            {
                var sData = s.canvasScaler;
                scaler.uiScaleMode = sData.uiScaleMode;
                scaler.referencePixelsPerUnit = sData.referencePixelsPerUnit;
                scaler.scaleFactor = sData.scaleFactor;
                scaler.referenceResolution = sData.referenceResolution;
                scaler.screenMatchMode = sData.screenMatchMode;
                scaler.matchWidthOrHeight = sData.matchWidthOrHeight;
                scaler.physicalUnit = sData.physicalUnit;
                scaler.fallbackScreenDPI = sData.fallbackScreenDPI;
                scaler.defaultSpriteDPI = sData.defaultSpriteDPI;
                scaler.dynamicPixelsPerUnit = sData.dynamicPixelsPerUnit;
            }

            // Apply GraphicRaycaster settings.
            if (obj.TryGetComponent(out GraphicRaycaster raycaster))
            {
                var g = s.graphicRaycaster;
                raycaster.ignoreReversedGraphics = g.ignoreReversedGraphics;
                raycaster.blockingObjects = g.blockingObjects;
                raycaster.blockingMask = g.blockingMask;
            }
        }
    }
}