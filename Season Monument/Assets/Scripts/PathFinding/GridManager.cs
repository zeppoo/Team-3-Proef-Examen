using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(-100)]
public class GridManager : MonoBehaviour
{
    public static GridManager instance;

    private Dictionary<Vector2Int, Tile> grid = new Dictionary<Vector2Int, Tile>();
    Dictionary<Tile, Vector2Int> tilePositions = new Dictionary<Tile, Vector2Int>();

    [SerializeField] private float tileSize = 1f;

    private void Awake()
    {
        instance = this;
    }

    public Vector2Int WorldToGrid(Vector3 worldPos)
    {
        int x = Mathf.RoundToInt(worldPos.x / tileSize);
        int z = Mathf.RoundToInt(worldPos.z / tileSize);
        return new Vector2Int(x, z);
    }

    public void RegisterTile(Tile tile)
    {
        Vector2Int coord = WorldToGrid(tile.transform.position);

        // If tile already exists, remove old position
        if (tilePositions.TryGetValue(tile, out Vector2Int oldCoord))
        {
            if (grid.ContainsKey(oldCoord))
                grid.Remove(oldCoord);
        }

        grid[coord] = tile;
        tilePositions[tile] = coord;

        RebuildNeighbours(tile, coord);
    }
    public void RemoveTile(Tile tile)
    {
        Vector2Int coord = WorldToGrid(tile.transform.position);

        if (grid.ContainsKey(coord))
            grid.Remove(coord);
    }

    private void RebuildNeighbours(Tile tile, Vector2Int coord)
    {
        tile.neighbours.Clear();

        Vector2Int[] directions =
        {
            Vector2Int.up,
            Vector2Int.down,
            Vector2Int.left,
            Vector2Int.right
        };

        foreach (Vector2Int dir in directions)
        {
            Vector2Int neighbourCoord = coord + dir;

            if (grid.TryGetValue(neighbourCoord, out Tile neighbour))
            {
                if (!tile.neighbours.Contains(neighbour))
                    tile.neighbours.Add(neighbour);

                if (!neighbour.neighbours.Contains(tile))
                    neighbour.neighbours.Add(tile);
            }
        }
    }

    public Tile GetTile(Vector2Int coord)
    {
        grid.TryGetValue(coord, out Tile tile);
        return tile;
    }

    public void RebuildAllNeighbours()
    {
        Debug.Log($"RebuildAllNeighbours called from: {new System.Diagnostics.StackTrace().GetFrame(1).GetMethod().Name}");
        
        Tile[] tiles = FindObjectsByType<Tile>(FindObjectsSortMode.None);

        foreach (Tile t in tiles)
        {
            t.neighbours.Clear();
        }

        foreach (Tile t in tiles)
        {
            t.FindNeigbour();
        }

        foreach (Tile t in tiles)
        {
            t.ValidateConnections();
        }
    }
}
