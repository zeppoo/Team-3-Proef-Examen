using System.Collections.Generic;
using System.Drawing;
using UnityEngine;

public class Tile : MonoBehaviour
{
    public Tile parent;
    public Tile connecetdTile;
    public List<GameObject> neighbours = new List<GameObject>();
    public bool isWalkable = true;
    private BoxCollider col;
    internal bool visited;

    private void Start()
    {
        col = gameObject.GetComponent<BoxCollider>();
        FindNeigbour();
        isWalkable = true;
    }

    public void FindNeigbour()
    {
        Tile[] allTiles = FindObjectsOfType<Tile>();

        foreach (Tile tile in allTiles)
        {
            if (tile.gameObject.GetInstanceID() != gameObject.GetInstanceID())
            {
           
                if (col.bounds.Intersects(tile.gameObject.GetComponent<BoxCollider>().bounds))
                {
                    neighbours.Add(tile.gameObject);
                }
            }
        }
    }
}
