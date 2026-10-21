using System.Collections.Generic;
using UnityEngine;

public enum StepResult
{
    Collapsed, Complete, Contradiction
}

public class GridCreator
{
    private static readonly TileDirection[] Directions =
    {
        TileDirection.Top, TileDirection.Right, TileDirection.Bottom, TileDirection.Left,
    };

    private Tile[,] TileGrid;
    private TileType[] availableTypes;

    public int Width => TileGrid?.GetLength(0) ?? 0;
    public int Height => TileGrid?.GetLength(1) ?? 0;
    public TileType[] AvailableTypes => availableTypes;

    public bool HasContradiction { get; private set; }
    public Tile ContradictionCell { get; private set; }

    public int RemainingCells
    {
        get
        {
            int count = 0;
            foreach (Tile tile in AllTiles)
            {
                if (!tile.isCollapsed) count++;
            }
            return count;
        }
    }

    public IEnumerable<Tile> AllTiles
    {
        get
        {
            if (TileGrid == null) yield break;
            for (int x = 0; x < Width ; x++)
            for (int z = 0; z < Height; z++)
            {
                yield return TileGrid[x, z];
            }
        }
    }

    public void InitializeTileGrid(int width, int height, GameObject template, TileType[] types, float tileSize = 1f, Transform parent = null)
    {
        availableTypes = types;
        TileGrid = new Tile[width, height];
        HasContradiction = false;
        ContradictionCell = null;


        for (int x = 0; x < width; x++)
        for (int z = 0; z < height; z++)
        {
            Vector3 position = new Vector3(x * tileSize, 0f, z * tileSize);
            GameObject tileObject = Object.Instantiate(template, position, Quaternion.identity, parent);
            tileObject.name = $"Tile_{x}_{z}";

            Tile tile = new Tile(tileObject, x, z, availableTypes);
            TileGrid[x, z] = tile;
            
            TileObject view = tileObject.GetOrAddComponent<TileObject>();
            if (view != null)
            {
                view.Bind(tile);
            }
        }
    }

    public Tile GetCell(int x, int z) => TileGrid[x, z];
    public void ResetDomains()
    {
        HasContradiction = false;
        ContradictionCell = null;
        foreach (Tile tile in AllTiles)
        {
            tile.ResetDomain(availableTypes);
        }
    }

    public StepResult Step(out Tile contradictionCell)
    {
        contradictionCell= null;
        if (TileGrid == null) return StepResult.Complete;

        if (HasContradiction)
        {
            contradictionCell = ContradictionCell;
            return StepResult.Contradiction;
        }

        Tile cell = SelectLowestEntropyCell();
        if (cell == null) return StepResult.Complete;

        TileType chosen = cell.Domain[Random.Range(0, cell.Domain.Count)];
        cell.Collapse(chosen);

        var queue = new Queue<Tile>();
        var inQueue = new HashSet<Tile> { cell };
        queue.Enqueue(cell);

        while (queue.Count > 0)
        {
            Tile current = queue.Dequeue();
            inQueue.Remove(current);

            foreach (TileDirection direction in Directions)
            {
                Tile neighbour = GetNeighbour(current, direction);
                if (neighbour == null) continue;

                int removed = neighbour.Domain.RemoveAll(type => !current.Supports(direction, type));
                if (removed == 0) continue;

                if (neighbour.Domain.Count == 0)
                {
                    HasContradiction = true;
                    ContradictionCell = neighbour;
                    contradictionCell = neighbour;
                    return StepResult.Contradiction;
                }

                if (inQueue.Add(neighbour)) queue.Enqueue(neighbour);
            }
        }
        return StepResult.Collapsed;
    }

    private Tile SelectLowestEntropyCell()
    {
        int baseEntropy = int.MaxValue;
        var candidates = new List<Tile>();

        foreach (Tile tile in AllTiles)
        { 
            if (tile.isCollapsed) continue;

            if (tile.Entropy < baseEntropy)
            {
                baseEntropy = tile.Entropy;
                candidates.Clear();
                candidates.Add(tile);
            }
            else if (tile.Entropy ==  baseEntropy)
            {
                candidates.Add(tile);
            }
        }
        return candidates.Count == 0 ? null : candidates[Random.Range(0, candidates.Count)];
    }

    private Tile GetNeighbour(Tile current, TileDirection direction)
    {
        Vector2Int offset = Offset(direction);
        int x = current.X + offset.x;
        int z = current.Z + offset.y;
        if (x < 0 || z < 0 || x >= Width || z >= Height) return null;
        return TileGrid[x, z];
    }

    private static Vector2Int Offset(TileDirection direction)
    {
        return direction switch
        {
            TileDirection.Top => Vector2Int.up,
            TileDirection.Right => Vector2Int.right,
            TileDirection.Bottom => Vector2Int.down,
            _ => Vector2Int.left,
        };
    }
}