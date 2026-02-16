
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

    [SerializeField] private TileData tileData;
    public List<TileConnection> tileConnections = new List<TileConnection>();

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
        ApplyTileConnections();
        ApplyTileData();
    }

    public void SetTileData(TileData newData)
    {
        tileData = newData;
        ApplyTileData();
    }

    protected void ApplyTileData()
    {
        if (tileData == null) return;

        isWalkable = tileData.isWalkable;
        materialInstance.color = tileData.tileColor;
    }

    public virtual void OnSeasonChanged(SeasonState season)
    {
        if (tileData == null) return;

        // Reset to defaults first
        ApplyTileData();

        // Apply season override if one exists
        if (tileData.TryGetSeasonOverride(season, out SeasonOverride overrideData))
        {
            if (overrideData.overrideWalkable)
                isWalkable = overrideData.isWalkable;
            if (overrideData.overrideColor)
                materialInstance.color = overrideData.tileColor;
        }

        tileData.OnTileSeasonChanged?.Invoke(season);
    }

    public void TileEntered()
    {
        if (tileData != null)
            tileData.OnTileEntered?.Invoke();
    }

    public void TileExited()
    {
        if (tileData != null)
            tileData.OnTileExited?.Invoke();
    }

    private void ApplyTileConnections()
    {
        foreach (TileConnection connection in tileConnections)
        {
            if (connection.connectedTile == null) continue;

            if (!neighbours.Contains(connection.connectedTile.gameObject))
                neighbours.Add(connection.connectedTile.gameObject);

            if (connection.bidirectional && !connection.connectedTile.neighbours.Contains(gameObject))
                connection.connectedTile.neighbours.Add(gameObject);
        }
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