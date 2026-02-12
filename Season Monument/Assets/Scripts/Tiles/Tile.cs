
using System.Collections.Generic;
using UnityEngine;
public abstract class Tile : MonoBehaviour
{
    public Tile parent;
    public Tile connecetdTile;
    public List<GameObject> neighbours = new List<GameObject>();

    public virtual bool isWalkable { get; protected set; }
    private BoxCollider col;
    private Renderer rend;
    protected Material materialInstance;

    internal bool visited;


    public virtual void OnEnable()
    {
        rend = GetComponent<Renderer>();
        col = gameObject.GetComponent<BoxCollider>();
        // Create a unique material instance (IMPORTANT)
        materialInstance = rend.material;

        
        SeasonEvents.OnSeasonChanged += OnSeasonChanged;
    }

    public virtual void OnDisable()
    {
        SeasonEvents.OnSeasonChanged -= OnSeasonChanged;
    }

    public virtual void Start()
    { 
        FindNeigbour();
    }

    public virtual void OnSeasonChanged(SeasonState season)
    {
        // Base implementation does nothing, can be overridden by subclasses
    }

    public void FindNeigbour()
    {
        Tile[] allTiles = FindObjectsByType<Tile>(FindObjectsSortMode.None);

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