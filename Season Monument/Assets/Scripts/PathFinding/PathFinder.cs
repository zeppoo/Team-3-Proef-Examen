using System.Collections.Generic;
using UnityEngine;

public class PathFinder : MonoBehaviour
{
    [SerializeField] internal Tile startTile;
    internal Tile endTile;

    private PlayerMovement playerMovement;

    private void Start()
    {
        playerMovement = GetComponent<PlayerMovement>();
    }

    public void FindPath()
    {
        if (startTile == null || endTile == null) return;

        // Fresh validate so connections reflect current perspective
        foreach (Tile t in FindObjectsByType<Tile>(FindObjectsSortMode.None))
        {
            t.visited = false;
            t.parent = null;
            t.ValidateConnections();
        }

        Queue<Tile> queue = new Queue<Tile>();
        startTile.visited = true;
        queue.Enqueue(startTile);

        while (queue.Count > 0)
        {
            Tile current = queue.Dequeue();

            if (current == endTile)
            {
                playerMovement.SetPath(RetracePath(startTile, endTile));
                return;
            }

            // Regular neighbours
            foreach (Tile neighbour in current.neighbours)
            {
                if (!neighbour.visited && neighbour.isWalkable)
                {
                    neighbour.visited = true;
                    neighbour.parent = current;
                    queue.Enqueue(neighbour);
                }
            }

            // Connection neighbours (perspective-filtered by ValidateConnections)
            foreach (Tile neighbour in current.GetValidConnectionNeighbours())
            {
                if (!neighbour.visited && neighbour.isWalkable)
                {
                    neighbour.visited = true;
                    neighbour.parent = current;
                    queue.Enqueue(neighbour);
                }
            }
        }

        // No path found
        endTile = null;
    }

    private List<Tile> RetracePath(Tile start, Tile end)
    {
        List<Tile> path = new List<Tile>();
        Tile current = end;
        while (current != null && current != start)
        {
            path.Add(current);
            current = current.parent;
        }
        path.Add(start);
        path.Reverse();
        return path;
    }
}
