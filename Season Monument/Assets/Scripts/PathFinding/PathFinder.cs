using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathFinder : MonoBehaviour
{
    public Tile startTile;
    public Tile endTile;
    private Tile tile;

    public PlayerMovement playerMovement;

    Queue <Tile> tileQueue = new Queue<Tile>();



    private void Start()
    {
        playerMovement = GetComponent<PlayerMovement>();
    }
    private void Update()
    {
    }

    public void FindPath()
    {
        
        tileQueue.Clear();
        ClearTiles();
        startTile.visited = true;
        tileQueue.Enqueue(startTile);
        while (tileQueue.Count > 0)
        {
            
            Tile currentTile = tileQueue.Dequeue();
            if (currentTile.gameObject.GetInstanceID() == endTile.gameObject.GetInstanceID())
            {
           
                List<Tile> path = RetracePath(startTile, endTile);
                playerMovement.setPath(path);
                string s = "";
                foreach (Tile t in path) s += t.name + " -> ";
                
                return;
            }
            foreach (GameObject neighbour in currentTile.neighbours)
            {
                Tile neighbourTile = neighbour.GetComponent<Tile>();
                
                if (neighbourTile.isWalkable && neighbourTile.visited != true)
                {
                    neighbourTile.visited = true;
                    neighbourTile.parent = currentTile;
                    tileQueue.Enqueue(neighbourTile);
                   
                   
                    
                    
                }
            }
        }
    }

    public void ClearTiles()
    {
        Tile[] allTiles = FindObjectsOfType<Tile>();
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
