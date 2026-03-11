using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathFinder : MonoBehaviour
{
    [SerializeField] internal Tile startTile;
    internal Tile endTile;

    private PlayerMovement playerMovement;

    private readonly Queue<Tile> tileQueue = new Queue<Tile>();

    private void Start()
    {
        playerMovement = GetComponent<PlayerMovement>();
    }

    private void OnEnable()
    {
        SeasonEvents.OnSeasonChanged += OnSeasonChanged;
    }

    private void OnDisable()
    {
        SeasonEvents.OnSeasonChanged -= OnSeasonChanged;
    }

    private void OnSeasonChanged(SeasonState season)
    {
        if (startTile != null && endTile != null)
        {
            StartCoroutine(FindPathDelayed());
        }
    }

    private IEnumerator FindPathDelayed()
    {
        yield return null; 
        FindPath();
    }

    public void FindPath()
    {
        tileQueue.Clear();
        ClearTiles();
        startTile.visited = true;
        tileQueue.Enqueue(startTile);
        
        bool pathFound = false;
        
        while (tileQueue.Count > 0)
        {
            Tile currentTile = tileQueue.Dequeue();
            if (currentTile.gameObject.GetInstanceID() == endTile.gameObject.GetInstanceID())
            {
                List<Tile> path = RetracePath(startTile, endTile);
                playerMovement.SetPath(path);
                pathFound = true;
                return;
            }
            foreach (GameObject neighbour in currentTile.neighbours)
            {
                Tile neighbourTile = neighbour.GetComponent<Tile>();

                if (neighbourTile.isWalkable && !neighbourTile.visited)
                {
                    neighbourTile.visited = true;
                    neighbourTile.parent = currentTile;
                    tileQueue.Enqueue(neighbourTile);
                }
            }

            foreach (GameObject neighbour in currentTile.connectionNeighbours)
            {
                Tile neighbourTile = neighbour.GetComponent<Tile>();

                if (neighbourTile.isWalkable && !neighbourTile.visited)
                {
                    neighbourTile.visited = true;
                    neighbourTile.parent = currentTile;
                    tileQueue.Enqueue(neighbourTile);
                }
            }
        }
        
        if (!pathFound)
        {
            endTile = null;
        }
    }

    public void ClearTiles()
    {
        Tile[] allTiles = FindObjectsByType<Tile>(FindObjectsSortMode.None);
        foreach (Tile tile in allTiles)
        {
            tile.visited = false;
            tile.parent = null;
        }
    }

    public List<Tile> RetracePath(Tile start, Tile end)
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
