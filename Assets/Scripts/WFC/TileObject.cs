using System;
using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;

public class TileObject : MonoBehaviour
{
    private const float TileSize = 10f;
    private const float HalfTileSize = TileSize / 2f;
    private const float QuarterTileOffset = TileSize / 4f;
    private const float ModifierHeight = 2f;
    private const float ModifierCenterY = 0.5f;

    [SerializeField] private Renderer target;
    [SerializeField] private Material domainMaterial;
    [SerializeField] private TextMesh countLabel;
    [SerializeField] private NavMeshModifierVolume navModifier;

    private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
    private static readonly int ColorId = Shader.PropertyToID("_Color");

    private Tile tile;
    private MaterialPropertyBlock block;

    private void Awake()
    {
        if (target == null) target = GetComponentInChildren<Renderer>();
        if (countLabel == null) countLabel = GetComponentInChildren<TextMesh>();

        block = new MaterialPropertyBlock();
    }

    public void Bind(Tile boundTile) => tile = boundTile;

    public void ShowDomain(int remaining, int total, Color fullColor, Color almostColor)
    {
        DisableNavigationBlockers();

        if (target == null) return;
        if (domainMaterial != null) target.sharedMaterial = domainMaterial;

        float ration = total <= 0 ? 0f : (float)remaining / total;
        Tint(Color.Lerp(almostColor, fullColor, ration));

        if (countLabel != null) countLabel.text = remaining.ToString();
    }

    public void ShowCollapsed(Material material)
    {
        if (target != null)
        {
            ClearTint();
            if (material != null) target.sharedMaterial = material;
        }

        if (countLabel != null) countLabel.text = string.Empty;
        ConfigureNavigationBlockers();
    }

    public void ShowContradiction(Color color)
    {
        DisableNavigationBlockers();

        if (target == null) return;

        if (domainMaterial != null) target.sharedMaterial = domainMaterial;
        Tint(color);

        if (countLabel != null) countLabel.text = "X";
    }

    private void Tint(Color color)
    {
        if (target == null) return;

        target.GetPropertyBlock(block);
        block.SetColor(BaseColorId, color); // URP Lit / Unlit
        block.SetColor(ColorId, color);     // Built-in
        target.SetPropertyBlock(block);
    }

    private void ClearTint()
    {
        if (target == null) return;

        block.Clear();
        target.SetPropertyBlock(block);
    }

    private void ConfigureNavigationBlockers()
    {
        DisableNavigationBlockers();

        if (tile == null) return;

        string tileName = tile.currentType.ToString();
        if (!tileName.Contains("Water")) return;

        string[] tokens = tileName.Split('_');

        // A full Water tile needs one blocker covering the complete 10 x 10 tile.
        if (tokens.Length == 1)
        {
            Vector3 center = new Vector3(0f, ModifierCenterY, 0f);
            SetNavigationBlocker(0, center, new Vector3(TileSize, ModifierHeight, TileSize));
            return;
        }

        if (tokens.Length >= 3 && (tokens[1] == "H" || tokens[1] == "V"))
        {
            ConfigureHalfTile(tokens[1], tokens[2]);
            return;
        }

        if (tokens.Length >= 4 && tokens[1] == "C")
        {
            ConfigureCornerTile(tokens[2], tokens[3]);
        }
    }

    private void ConfigureHalfTile(string split, string order)
    {
        Vector2 waterSide = $"{split}_{order}" switch
        {
            "H_AB" => Vector2.down,
            "H_BA" => Vector2.up,
            "V_AB" => Vector2.right,
            "V_BA" => Vector2.left,
            _ => Vector2.zero,
        };

        if (waterSide == Vector2.zero) return;

        Vector3 center = new Vector3(
            QuarterTileOffset * waterSide.x,
            ModifierCenterY,
            QuarterTileOffset * waterSide.y);

        Vector3 size = split == "H"
            ? new Vector3(TileSize, ModifierHeight, HalfTileSize)
            : new Vector3(HalfTileSize, ModifierHeight, TileSize);

        SetNavigationBlocker(0, center, size);
    }

    private void ConfigureCornerTile(string corner, string order)
    {
        Vector2 cornerDirection = corner switch
        {
            "NW" => new Vector2(-1f, 1f),
            "NE" => new Vector2(1f, 1f),
            "SE" => new Vector2(1f, -1f),
            "SW" => new Vector2(-1f, -1f),
            _ => Vector2.zero,
        };

        if (cornerDirection == Vector2.zero) return;

        if (order == "BA")
        {
            // BA corner tiles contain water in the named quarter.
            Vector3 center = new Vector3(
                QuarterTileOffset * cornerDirection.x,
                ModifierCenterY,
                QuarterTileOffset * cornerDirection.y);

            SetNavigationBlocker(0, center, new Vector3(HalfTileSize, ModifierHeight, HalfTileSize));
            return;
        }

        if (order != "AB") return;

        // AB corner tiles contain water everywhere except the named quarter.
        // Two non-overlapping boxes approximate that three-quarter shape.
        Vector3 oppositeHalfCenter = new Vector3(
            -QuarterTileOffset * cornerDirection.x,
            ModifierCenterY,
            0f);

        Vector3 remainingQuarterCenter = new Vector3(
            QuarterTileOffset * cornerDirection.x,
            ModifierCenterY,
            -QuarterTileOffset * cornerDirection.y);

        SetNavigationBlocker(0, oppositeHalfCenter, new Vector3(HalfTileSize, ModifierHeight, TileSize));
        SetNavigationBlocker(1, remainingQuarterCenter, new Vector3(HalfTileSize, ModifierHeight, HalfTileSize));
    }

    private void SetNavigationBlocker(int index, Vector3 center, Vector3 size)
    {
        NavMeshModifierVolume[] modifiers = GetComponents<NavMeshModifierVolume>();
        NavMeshModifierVolume modifier = index < modifiers.Length
            ? modifiers[index]
            : gameObject.AddComponent<NavMeshModifierVolume>();

        modifier.enabled = true;
        modifier.area = NavMesh.GetAreaFromName("Not Walkable");
        modifier.center = center;
        modifier.size = size;

        if (index == 0) navModifier = modifier;
    }

    private void DisableNavigationBlockers()
    {
        foreach (NavMeshModifierVolume modifier in GetComponents<NavMeshModifierVolume>())
        {
            modifier.enabled = false;
        }
    }
}
