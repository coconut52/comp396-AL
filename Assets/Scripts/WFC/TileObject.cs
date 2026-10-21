using System;
using System.Collections.Generic;
using UnityEngine;

public class TileObject : MonoBehaviour
{
    [SerializeField] private Renderer target;
    [SerializeField] private Material domainMaterial;
    [SerializeField] private TextMesh countLabel;

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