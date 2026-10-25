using System;
using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;

public class TileObject : MonoBehaviour
{
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
        if (target == null) return;
        if (domainMaterial != null) target.sharedMaterial = domainMaterial;

        float ration = total <= 0 ? 0f : (float)remaining / total;
        Tint(Color.Lerp(almostColor, fullColor, ration));

        if (countLabel != null) countLabel.text = remaining.ToString();
    }

    public void ShowCollapsed(Material material)
    {
        if (target == null) return;

        ClearTint();
        if (material != null) target.sharedMaterial = material;
        if (countLabel != null) countLabel.text = string.Empty;
        if (tile.currentType.ToString().Contains("Water"))
        {
            navModifier = gameObject.GetOrAddComponent<NavMeshModifierVolume>();
            navModifier.area = 1; // Non-Walkable option.
            string[] tokens = tile.currentType.ToString().Split('_');
            if (tokens.Length > 0 && tile.currentType.ToString().Contains("_C_") && tile.currentType.ToString().Contains("_BA"))
            {
                string cornerKey = tokens[1] == "C" ? $"C_{tokens[2]}" : tokens[1];

                Vector2 direction = cornerKey switch
                {
                    "C_NW" => new Vector2(-1f, 1f),
                    "C_NE" => new Vector2(1f, 1f),
                    "C_SE" => new Vector2(1, -1f),
                    "C_SW" => new Vector2(-1, -1f),
                    _ => new Vector2(0, 0),
                };
                navModifier.center = new Vector3(2.5f * direction.x, 0.5f, 2.5f * direction.y);
                navModifier.size = new Vector3(5f, 2f, 5f);
            }
            else if (tokens.Length > 0 && tile.currentType.ToString().Contains("_H_") || tile.currentType.ToString().Contains("_V_"))
            {
                string sideKey = tokens[1] == "H" ? $"H_{tokens[2]}" : tokens[1] == "V" ? $"V_{tokens[2]}" : tokens[1];

                Vector2 side = sideKey switch
                {
                    "H_AB" => Vector2.down,
                    "H_BA" => Vector2.up,
                    "V_AB" => Vector2.right,
                    "V_BA" => Vector2.left,
                    _ => Vector2.zero,
                };

                navModifier.center = new Vector3(2.5f * side.x, 0.5f, 2.5f * side.y);
                navModifier.size = new Vector3(5f + (side.y * 5), 2f, 5f + (side.x * 5));
            }
            else
            {
                navModifier.center = new Vector3(0f, 0.5f, 0f);
                navModifier.size = new Vector3(10f, 2f, 10f);
            }
        }
    }

    public void ShowContradiction(Color color)
    {
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
}