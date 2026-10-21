using System.Collections.Generic;
using UnityEngine;

public enum TileType
{
    /// <summary>Sentinel: the cell has not collapsed yet. Never part of a domain.</summary>
    Template = 0,

    // Full terrain
    Water, Grass, Sand,

    // Sand / Water (A = Sand, B = Water)
    SandWater_H_AB, SandWater_H_BA, SandWater_V_AB, SandWater_V_BA,
    SandWater_C_NW_AB, SandWater_C_NE_AB, SandWater_C_SW_AB, SandWater_C_SE_AB,
    SandWater_C_NW_BA, SandWater_C_NE_BA, SandWater_C_SW_BA, SandWater_C_SE_BA,

    // Sand / Grass (A = Sand, B = Grass)
    SandGrass_H_AB, SandGrass_H_BA, SandGrass_V_AB, SandGrass_V_BA,
    SandGrass_C_NW_AB, SandGrass_C_NE_AB, SandGrass_C_SW_AB, SandGrass_C_SE_AB,
    SandGrass_C_NW_BA, SandGrass_C_NE_BA, SandGrass_C_SW_BA, SandGrass_C_SE_BA,

    // Grass / Water (A = Grass, B = Water)
    GrassWater_H_AB, GrassWater_H_BA, GrassWater_V_AB, GrassWater_V_BA,
    GrassWater_C_NW_AB, GrassWater_C_NE_AB, GrassWater_C_SW_AB, GrassWater_C_SE_AB,
    GrassWater_C_NW_BA, GrassWater_C_NE_BA, GrassWater_C_SW_BA, GrassWater_C_SE_BA,
}

public enum TileDirection
{
    Top = 0,
    Right = 1,
    Bottom = 2,
    Left = 3
}

public enum Socket
{
    Water, Sand, Grass,

    SandWater_H_AB, SandWater_H_BA, SandWater_V_AB, SandWater_V_BA,
    SandGrass_H_AB, SandGrass_H_BA, SandGrass_V_AB, SandGrass_V_BA,
    GrassWater_H_AB, GrassWater_H_BA, GrassWater_V_AB, GrassWater_V_BA,
}


[System.Serializable]
public class Tile
{
    public TileType currentType;
    public bool isCollapsed;
    public GameObject self;

    public int X { get; }
    public int Z { get; }

    public readonly List<TileType> Domain = new List<TileType>();

    public int Entropy => Domain.Count;

    public Tile(GameObject self, int x, int z, IReadOnlyList<TileType> availableTypes)
    {
        this.self = self;
        X = x;
        Z = z;
        ResetDomain(availableTypes);
    }

    public void ResetDomain(IReadOnlyList<TileType> availableTypes)
    {
        Domain.Clear();
        for (int i = 0; i < availableTypes.Count; i++)
        {
            Domain.Add(availableTypes[i]);
        }
        currentType = TileType.Template;
        isCollapsed = false;
    }

    public void Collapse(TileType type)
    {
        currentType = type;
        isCollapsed = true;
        Domain.Clear();
        Domain.Add(type);
    }

    public bool Supports(TileDirection side, TileType neighbourType)
    {
        for (int i = 0; i < Domain.Count; i++)
        {
            if (SocketRules.CanTouch(Domain[i], side, neighbourType))
            {
                return true;
            }
        }
        return false;
    }
}