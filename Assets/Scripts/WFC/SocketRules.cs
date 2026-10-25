using System.Collections.Generic;
using UnityEngine;

public static class SocketRules
{
    public static TileType[] AllTypes =
    {
        TileType.Water, TileType.Grass, TileType.Sand,

        TileType.SandWater_H_AB, TileType.SandWater_H_BA,
        TileType.SandWater_V_AB, TileType.SandWater_V_BA,

        TileType.SandWater_C_NW_AB, TileType.SandWater_C_NE_AB,
        TileType.SandWater_C_SW_AB, TileType.SandWater_C_SE_AB,
        TileType.SandWater_C_NW_BA, TileType.SandWater_C_NE_BA,
        TileType.SandWater_C_SW_BA, TileType.SandWater_C_SE_BA,

        TileType.SandGrass_H_AB, TileType.SandGrass_H_BA, 
        TileType.SandGrass_V_AB, TileType.SandGrass_V_BA,
        
        TileType.SandGrass_C_NW_AB, TileType.SandGrass_C_NE_AB, 
        TileType.SandGrass_C_SW_AB, TileType.SandGrass_C_SE_AB,
        TileType.SandGrass_C_NW_BA, TileType.SandGrass_C_NE_BA,
        TileType.SandGrass_C_SW_BA, TileType.SandGrass_C_SE_BA,

        TileType.GrassWater_H_AB, TileType.GrassWater_H_BA,
        TileType.GrassWater_V_AB, TileType.GrassWater_V_BA,

        TileType.GrassWater_C_NW_AB, TileType.GrassWater_C_NE_AB, 
        TileType.GrassWater_C_SW_AB, TileType.GrassWater_C_SE_AB,
        TileType.GrassWater_C_NW_BA, TileType.GrassWater_C_NE_BA, 
        TileType.GrassWater_C_SW_BA, TileType.GrassWater_C_SE_BA,
    };

    private static readonly Dictionary<TileType, Socket[]> Sides = new Dictionary<TileType, Socket[]>
    {
        // ------------------------------------------------------------------ full tiles
        { TileType.Water, Row(Socket.Water, Socket.Water, Socket.Water, Socket.Water) },
        { TileType.Grass, Row(Socket.Grass, Socket.Grass, Socket.Grass, Socket.Grass) },
        { TileType.Sand,  Row(Socket.Sand,  Socket.Sand,  Socket.Sand,  Socket.Sand)  },

        // ------------------------------------------------------- Sand / Water (A = Sand)
        { TileType.SandWater_H_AB,
            Row(Socket.Sand, Socket.SandWater_V_AB, Socket.Water, Socket.SandWater_V_AB) },
        { TileType.SandWater_H_BA,
            Row(Socket.Water, Socket.SandWater_V_BA, Socket.Sand, Socket.SandWater_V_BA) },
        { TileType.SandWater_V_AB,
            Row(Socket.SandWater_H_AB, Socket.Water, Socket.SandWater_H_AB, Socket.Sand) },
        { TileType.SandWater_V_BA,
            Row(Socket.SandWater_H_BA, Socket.Sand, Socket.SandWater_H_BA, Socket.Water) },

        { TileType.SandWater_C_NW_AB,
            Row(Socket.SandWater_H_AB, Socket.Water, Socket.Water, Socket.SandWater_V_AB) },
        { TileType.SandWater_C_NE_AB,
            Row(Socket.SandWater_H_BA, Socket.SandWater_V_AB, Socket.Water, Socket.Water) },
        { TileType.SandWater_C_SW_AB,
            Row(Socket.Water, Socket.Water, Socket.SandWater_H_AB, Socket.SandWater_V_BA) },
        { TileType.SandWater_C_SE_AB,
            Row(Socket.Water, Socket.SandWater_V_BA, Socket.SandWater_H_BA, Socket.Water) },

        { TileType.SandWater_C_NW_BA,
            Row(Socket.SandWater_H_BA, Socket.Sand, Socket.Sand, Socket.SandWater_V_BA) },
        { TileType.SandWater_C_NE_BA,
            Row(Socket.SandWater_H_AB, Socket.SandWater_V_BA, Socket.Sand, Socket.Sand) },
        { TileType.SandWater_C_SW_BA,
            Row(Socket.Sand, Socket.Sand, Socket.SandWater_H_BA, Socket.SandWater_V_AB) },
        { TileType.SandWater_C_SE_BA,
            Row(Socket.Sand, Socket.SandWater_V_AB, Socket.SandWater_H_AB, Socket.Sand) },

        // ------------------------------------------------------ Sand / Grass (A = Sand)
        { TileType.SandGrass_H_AB,
            Row(Socket.Sand, Socket.SandGrass_V_AB, Socket.Grass, Socket.SandGrass_V_AB) },
        { TileType.SandGrass_H_BA,
            Row(Socket.Grass, Socket.SandGrass_V_BA, Socket.Sand, Socket.SandGrass_V_BA) },
        { TileType.SandGrass_V_AB,
            Row(Socket.SandGrass_H_AB, Socket.Grass, Socket.SandGrass_H_AB, Socket.Sand) },
        { TileType.SandGrass_V_BA,
            Row(Socket.SandGrass_H_BA, Socket.Sand, Socket.SandGrass_H_BA, Socket.Grass) },

        { TileType.SandGrass_C_NW_AB,
            Row(Socket.SandGrass_H_AB, Socket.Grass, Socket.Grass, Socket.SandGrass_V_AB) },
        { TileType.SandGrass_C_NE_AB,
            Row(Socket.SandGrass_H_BA, Socket.SandGrass_V_AB, Socket.Grass, Socket.Grass) },
        { TileType.SandGrass_C_SW_AB,
            Row(Socket.Grass, Socket.Grass, Socket.SandGrass_H_AB, Socket.SandGrass_V_BA) },
        { TileType.SandGrass_C_SE_AB,
            Row(Socket.Grass, Socket.SandGrass_V_BA, Socket.SandGrass_H_BA, Socket.Grass) },

        { TileType.SandGrass_C_NW_BA,
            Row(Socket.SandGrass_H_BA, Socket.Sand, Socket.Sand, Socket.SandGrass_V_BA) },
        { TileType.SandGrass_C_NE_BA,
            Row(Socket.SandGrass_H_AB, Socket.SandGrass_V_BA, Socket.Sand, Socket.Sand) },
        { TileType.SandGrass_C_SW_BA,
            Row(Socket.Sand, Socket.Sand, Socket.SandGrass_H_BA, Socket.SandGrass_V_AB) },
        { TileType.SandGrass_C_SE_BA,
            Row(Socket.Sand, Socket.SandGrass_V_AB, Socket.SandGrass_H_AB, Socket.Sand) },

        // ------------------------------------------------------ Grass / Water (A = Grass)
        { TileType.GrassWater_H_AB,
            Row(Socket.Grass, Socket.GrassWater_V_AB, Socket.Water, Socket.GrassWater_V_AB) },
        { TileType.GrassWater_H_BA,
            Row(Socket.Water, Socket.GrassWater_V_BA, Socket.Grass, Socket.GrassWater_V_BA) },
        { TileType.GrassWater_V_AB,
            Row(Socket.GrassWater_H_AB, Socket.Water, Socket.GrassWater_H_AB, Socket.Grass) },
        { TileType.GrassWater_V_BA,
            Row(Socket.GrassWater_H_BA, Socket.Grass, Socket.GrassWater_H_BA, Socket.Water) },

        { TileType.GrassWater_C_NW_AB,
            Row(Socket.GrassWater_H_AB, Socket.Water, Socket.Water, Socket.GrassWater_V_AB) },
        { TileType.GrassWater_C_NE_AB,
            Row(Socket.GrassWater_H_BA, Socket.GrassWater_V_AB, Socket.Water, Socket.Water) },
        { TileType.GrassWater_C_SW_AB,
            Row(Socket.Water, Socket.Water, Socket.GrassWater_H_AB, Socket.GrassWater_V_BA) },
        { TileType.GrassWater_C_SE_AB,
            Row(Socket.Water, Socket.GrassWater_V_BA, Socket.GrassWater_H_BA, Socket.Water) },

        { TileType.GrassWater_C_NW_BA,
            Row(Socket.GrassWater_H_BA, Socket.Grass, Socket.Grass, Socket.GrassWater_V_BA) },
        { TileType.GrassWater_C_NE_BA,
            Row(Socket.GrassWater_H_AB, Socket.GrassWater_V_BA, Socket.Grass, Socket.Grass) },
        { TileType.GrassWater_C_SW_BA,
            Row(Socket.Grass, Socket.Grass, Socket.GrassWater_H_BA, Socket.GrassWater_V_AB) },
        { TileType.GrassWater_C_SE_BA,
            Row(Socket.Grass, Socket.GrassWater_V_AB, Socket.GrassWater_H_AB, Socket.Grass) },
    };

    private static Socket[] Row(Socket top, Socket right, Socket bottom, Socket left)
    {
        return new[] {top, right, bottom, left}; 
    }
    
    public static Socket SocketOf(TileType type, TileDirection side)
    {
        return Sides[type][(int)side];
    }
    public static TileDirection Opposite(TileDirection side)
    {
        return (TileDirection)(((int)side + 2) & 3);
    }
    public static bool CanTouch(TileType a, TileDirection sideA, TileType b)
    {
        return Sides[a][(int)sideA] == Sides[b][(int)Opposite(sideA)];
    }

    public static void WarnAboutDeadEnds(IReadOnlyList<TileType> types)
    {
        var top = new HashSet<Socket>();
        var right = new HashSet<Socket>();
        var bottom = new HashSet<Socket>();
        var left = new HashSet<Socket>();

        for (int i = 0; i < types.Count; i++)
        {
            var sides = Sides[types[i]];
            top.Add(sides[(int)TileDirection.Top]);
            right.Add(sides[(int)TileDirection.Right]);
            bottom.Add(sides[(int)TileDirection.Bottom]);
            left.Add(sides[(int)TileDirection.Left]);
        }

        ReportDeadEnds(top, bottom, "Top", "Bottom");
        ReportDeadEnds(right, left, "Right", "Left");
        ReportDeadEnds(bottom, top, "Bottom", "Top");
        ReportDeadEnds(left, right, "Left", "Right");
    }
    private static void ReportDeadEnds(HashSet<Socket> mine, HashSet<Socket> partners, string mineName, string partnerName)
    {
        foreach (Socket socket in mine)
        {
            if (partners.Contains(socket)) continue;

            Debug.LogWarning($"[WFC] Socket '{socket}' is used on {mineName} sides but never on " +
                $"{partnerName} sides — those tiles are dead ends with the current material set.");
        }
    }
}
