using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

public class CollapseResolver : MonoBehaviour
{
    [System.Serializable]
    public struct MaterialEntry
    {
        public TileType type;
        public Material material;
    }

    [Header("Grid")]
    [SerializeField] private GameObject tilePrefab;
    [SerializeField] private int width = 10;
    [SerializeField] private int height = 10;
    [Tooltip("World-space spacing between cells. The template quad is 1 unit, so use 1.")]
    [SerializeField] private float tileSize = 1f;
    [SerializeField] private Transform gridRoot;

    [Header("Tile set (only entries with a material are used)")]
    [SerializeField] private MaterialEntry[] materials;

    [Header("Domain view")]
    [SerializeField] private Color domainFullColor = new Color(0.75f, 0.75f, 0.75f);
    [SerializeField] private Color domainAlmostColor = new Color(1f, 0.35f, 0.1f);
    [SerializeField] private Color contradictionColor = Color.red;

    private GridCreator gridCreator;
    private readonly Dictionary<TileType, Material> materialLookup = new Dictionary<TileType, Material>();
    [SerializeField] private Shader shader;
    [SerializeField] private Texture2D sandBaseColor;
    [SerializeField] private Texture2D waterBaseColor;
    [SerializeField] private Texture2D grassBaseColor;
    [SerializeField] private Texture2D sandNormal;
    [SerializeField] private Texture2D waterNormal;
    [SerializeField] private Texture2D grassNormal;
    [Header("Masks")]
    [SerializeField] private Texture2D maskFull;
    [SerializeField] private Texture2D maskHorizontal;   // mask_half
    [SerializeField] private Texture2D maskVertical;     // mask_half_v
    [SerializeField] private Texture2D maskCornerNW;     // mask_corner_outer
    [SerializeField] private Texture2D maskCornerNE;
    [SerializeField] private Texture2D maskCornerSE;
    [SerializeField] private Texture2D maskCornerSW;

    private void Start()
    {
        if (tilePrefab == null)
        {
            Debug.LogError("[WFC] No tile prefab assigned.");
            enabled = false;
            return;
        }

        materialLookup.Clear();
        var activeTypes = new List<TileType>();

        foreach (MaterialEntry entry in materials)
        {
            if (entry.type == TileType.Template || entry.material == null) continue;
            if (materialLookup.ContainsKey(entry.type)) continue;

            materialLookup.Add(entry.type, entry.material);
            activeTypes.Add(entry.type);
        }

        if (activeTypes.Count == 0)
        {
            Debug.LogError("[WFC] No materials assigned — nothing to generate.");
            enabled = false;
            return;
        }

        SocketRules.WarnAboutDeadEnds(activeTypes);

        gridCreator = new GridCreator();
        gridCreator.InitializeTileGrid(width, height, tilePrefab, activeTypes.ToArray(), tileSize, gridRoot);
        RefreshViews();
    }

    private void Update()
    {
        if (gridCreator == null || Keyboard.current == null) return;

        if (Keyboard.current.spaceKey.wasPressedThisFrame) StepOnce();

        if (Keyboard.current.rKey.wasPressedThisFrame)
        {
            gridCreator.ResetDomains();
            RefreshViews();
        }
    }

    private void StepOnce()
    {
        StepResult result = gridCreator.Step(out Tile contradictionCell);
        RefreshViews();

        switch (result)
        {
            case StepResult.Complete:
                Debug.Log("[WFC] Grid complete.");
                break;

            case StepResult.Contradiction:
                Debug.LogWarning($"[WFC] Contradiction at ({contradictionCell.X}, {contradictionCell.Z}) — press R to restart.");
                break;
        }
    }

    private void RefreshViews()
    {
        int total = gridCreator.AvailableTypes.Length;

        foreach (Tile tile in gridCreator.AllTiles)
        {
            TileObject view = tile.self.GetComponent<TileObject>();
            if (view == null) continue;

            if (gridCreator.HasContradiction && tile == gridCreator.ContradictionCell)
            {
                view.ShowContradiction(contradictionColor);
            }
            else if (tile.isCollapsed)
            {
                Material material = materialLookup.TryGetValue(tile.currentType, out Material found) ? found : null;
                view.ShowCollapsed(material);
            }
            else
            {
                view.ShowDomain(tile.Entropy, total, domainFullColor, domainAlmostColor);
            }
        }
    }

#if UNITY_EDITOR    
    [ContextMenu("Build Materials for CollapseResolver")]
    public void CreateMaterialsAndAssignOnResolver()
    {
        if (shader == null)
        {
            Debug.LogError("Shader not found");
            return;
        }
        var terrainTextures = new Dictionary<string, (Texture2D baseColor, Texture2D normal)>
        {
            { "Sand",  (sandBaseColor, sandNormal) },
            { "Water", (waterBaseColor, waterNormal) },
            { "Grass", (grassBaseColor, grassNormal) },
        };

        const string folder = "Assets/Shaders/Materials";
        if (!AssetDatabase.IsValidFolder(folder))
            AssetDatabase.CreateFolder("Assets/Shaders", "Materials");

        TileType[] types = (TileType[])Enum.GetValues(typeof(TileType));
        materials = new MaterialEntry[types.Length];
        int built = 0;


        for (int i = 0; i < types.Length; i++)
        {
            TileType type = types[i];
            string typeName = type.ToString();

            if (type == TileType.Template)
            {
                materials[i] = new MaterialEntry { type = type, material = null };
                continue;
            }

            string[] tokens = typeName.Split('_');                          // SandWater, H, BA
            bool isTransition = tokens.Length > 1;
            string order = isTransition ? tokens[tokens.Length - 1] : null; // "AB" / "BA"
            string[] pair = Regex.Split(tokens[0], @"(?<!^)(?=[A-Z])");     // Sand, Water

            string terrainA = pair[0];
            string terrainB = pair.Length > 1 ? pair[1] : pair[0];

            // _BA = pair drawn the other way round: B belongs in the A slot.
            if (order == "AB")
                (terrainA, terrainB) = (terrainB, terrainA);

            string path = $"{folder}/M_{typeName}.mat";
            Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                material = new Material(shader);
                AssetDatabase.CreateAsset(material, path);
            }
            else
            {
                material.shader = shader;
            }

            SetTerrain(material, "A", terrainA, terrainTextures);
            if (isTransition)
            {
                SetTerrain(material, "B", terrainB, terrainTextures);
            }
            else
            {
                material.SetTexture("_TerrainB", null);
                material.SetTexture("_NormalB", null);
            }
            string maskKey = !isTransition
                ? "full"
                : tokens[1] == "C" ? $"C_{tokens[2]}" : tokens[1];

            Texture2D mask = maskKey switch
            {
                "full" => maskFull,
                "H" => maskHorizontal,
                "V" => maskVertical,
                "C_NW" => maskCornerNW,
                "C_NE" => maskCornerNE,
                "C_SE" => maskCornerSE,
                "C_SW" => maskCornerSW,
                _ => null
            };

            if (mask != null)
                material.SetTexture("_Mask", mask);
            else
                Debug.LogWarning($"[WFC] No mask assigned for {typeName}.");

            UnityEditor.EditorUtility.SetDirty(material);

            materials[i] = new MaterialEntry { type = type, material = material };
            built++;
        }
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($"[WFC] Built {built} materials under {folder}.");

        static void SetTerrain(Material mat, string slot, string terrain,
            IReadOnlyDictionary<string, (Texture2D baseColor, Texture2D normal)> textures)
        {
            if (!textures.TryGetValue(terrain, out var tex))
            {
                Debug.LogWarning($"[WFC] Unknown terrain '{terrain}' in {mat.name}.");
                return;
            }

            mat.SetTexture($"_Terrain{slot}", tex.baseColor);
            mat.SetTexture($"_Normal{slot}", tex.normal);
        }
    }

    [ContextMenu("Generate Rotated Masks")]
    public void GenerateRotatedMasks()
    {
        const string folder = "Assets/Textures/Masks";

        Texture2D half = AssetDatabase.LoadAssetAtPath<Texture2D>($"{folder}/mask_half.png");
        Texture2D corner = AssetDatabase.LoadAssetAtPath<Texture2D>($"{folder}/mask_corner_outer.png");

        if (half == null || corner == null)
        {
            Debug.LogError($"[WFC] Missing mask_half.png / mask_corner_outer.png in {folder}.");
            return;
        }

        // white = _TerrainA
        SaveRotated(half, 270, $"{folder}/mask_half_v.png");    // white moves to the left half
        SaveRotated(corner, 90, $"{folder}/mask_corner_ne.png"); // NW wedge -> NE
        SaveRotated(corner, 180, $"{folder}/mask_corner_se.png");
        SaveRotated(corner, 270, $"{folder}/mask_corner_sw.png");

        AssetDatabase.Refresh();

        void SaveRotated(Texture2D source, int degreesCW, string path)
        {
            string sourcePath = AssetDatabase.GetAssetPath(source);
            var sourceImporter = (TextureImporter)AssetImporter.GetAtPath(sourcePath);
            bool wasReadable = sourceImporter.isReadable;

            if (!wasReadable)
            {
                sourceImporter.isReadable = true;
                sourceImporter.SaveAndReimport();
                source = AssetDatabase.LoadAssetAtPath<Texture2D>(sourcePath);
            }

            int w = source.width;
            int h = source.height;
            Color32[] src = source.GetPixels32();   // index 0 = bottom-left
            Color32[] dst = new Color32[src.Length];

            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    int sx, sy;
                    switch (degreesCW)
                    {
                        case 90: sx = w - 1 - y; sy = x; break;
                        case 180: sx = w - 1 - x; sy = h - 1 - y; break;
                        case 270: sx = y; sy = w - 1 - x; break;
                        default: sx = x; sy = y; break;
                    }
                    dst[x + y * w] = src[sx + sy * w];
                }
            }

            var rotated = new Texture2D(w, h, TextureFormat.RGBA32, false);
            rotated.SetPixels32(dst);
            rotated.Apply();
            System.IO.File.WriteAllBytes(path, rotated.EncodeToPNG());
            DestroyImmediate(rotated);

            if (!wasReadable)
            {
                var restore = (TextureImporter)AssetImporter.GetAtPath(sourcePath);
                restore.isReadable = false;
                restore.SaveAndReimport();
            }

            AssetDatabase.ImportAsset(path);
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType = TextureImporterType.Default;
            importer.sRGBTexture = false;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.mipmapEnabled = false;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.SaveAndReimport();

            Debug.Log($"[WFC] Wrote {path}");
        }
    }
#endif
}