using System.Collections.Generic;
using System.Drawing;
using UnityEngine;

public class Tile : MonoBehaviour
{
    public Tile parent;
    public Tile connecetdTile;
    public List<GameObject> neighbours = new List<GameObject>();
    public bool isWalkable;
    private BoxCollider col2d;
    bool debug;

    private void Start()
    {
        FindNeigbour();
        col2d = gameObject.GetComponent<BoxCollider>();
    }

    public void FindNeigbour()
    {
        Tile[] allTiles = FindObjectsOfType<Tile>();

        foreach (Tile tile in allTiles)
        {
            if (tile.gameObject.GetInstanceID() != gameObject.GetInstanceID())
            {
                if (col2d.bounds.Intersects(tile.gameObject.GetComponent<Collider2D>().bounds))
                {
                    Debug.Log("[" + gameObject.name + "] found a neighbour: " + tile.gameObject.name);
                    neighbours.Add(tile.gameObject);
                }
            }
        }
    }
}
