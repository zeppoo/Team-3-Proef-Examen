
using System.Collections.Generic;
using UnityEngine;
public abstract class Tile : MonoBehaviour
{
    internal Tile parent;
    internal List<GameObject> neighbours = new List<GameObject>();

    public virtual bool isWalkable { get; protected set; }
    protected Material materialInstance;

    [SerializeField] private TileData tileData;
    public List<TileConnection> tileConnections = new List<TileConnection>();

    [Header("Mesh Override")]
    [SerializeField] private Mesh overrideMesh;
    [SerializeField] private Vector3 meshRotation = Vector3.zero;

    internal bool visited;


    public virtual void OnEnable()
    {
        Renderer rend = GetComponent<Renderer>();
        // Use sharedMaterial in edit mode to avoid leaking material instances
        if (Application.isPlaying)
            materialInstance = rend.material;
        else
            materialInstance = rend.sharedMaterial;


        SeasonEvents.OnSeasonChanged += OnSeasonChanged;
    }

    public virtual void OnDisable()
    {
        SeasonEvents.OnSeasonChanged -= OnSeasonChanged;
    }

    public virtual void Start()
    {
        GridManager.instance.RegisterTile(this);
        FindNeigbour();
        ApplyTileData();
        ValidateConnections();
        ApplyTileConnections();
        ApplyMeshOverride();
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

    public void ApplyMeshOverride()
    {
        if (overrideMesh == null) return;

        MeshFilter mf = GetComponent<MeshFilter>();
        if (mf == null) return;

        mf.sharedMesh = overrideMesh;
        transform.localEulerAngles = meshRotation;
    }

    public virtual void OnSeasonChanged(SeasonState season)
    {
        if (tileData == null) return;

        
        ApplyTileData();

      
        if (tileData.TryGetSeasonOverride(season, out SeasonOverride overrideData))
        {
            if (overrideData.overrideWalkable)
                isWalkable = overrideData.isWalkable;
            if (overrideData.overrideColor)
                materialInstance.color = overrideData.tileColor;
        }

        ValidateConnections();
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

    public void ValidateConnections()
    {
        foreach (TileConnection connection in tileConnections)
        {
            if (connection.connectedTile == null)
            {
                connection.valid = false;
                continue;
            }

            connection.valid = isWalkable && connection.connectedTile.isWalkable;
        }
    }

    private void ApplyTileConnections()
    {
        foreach (TileConnection connection in tileConnections)
        {
            if (connection.connectedTile == null || !connection.valid) continue;

            if (!neighbours.Contains(connection.connectedTile.gameObject))
                neighbours.Add(connection.connectedTile.gameObject);

            if (connection.bidirectional && !connection.connectedTile.neighbours.Contains(gameObject))
                connection.connectedTile.neighbours.Add(gameObject);
        }
    }

    public TileConnection GetConnectionTo(Tile other)
    {
        foreach (TileConnection connection in tileConnections)
        {
            if (connection.connectedTile == other)
                return connection;
        }
        return null;
    }

     public void FindNeigbour()
     {
         Tile[] allTiles = FindObjectsByType<Tile>(FindObjectsSortMode.None);
         BoxCollider col = gameObject.GetComponent<BoxCollider>();
         
         if (col == null)
         {
             Debug.LogError($"{gameObject.name} has no BoxCollider! Cannot find neighbors.");
             return;
         }
         
         float tileSize = col.bounds.size.x;
    
         foreach (Tile tile in allTiles)
         {
             if (tile == this) continue;
    
             Vector3 offset = tile.transform.position - transform.position;
             Vector3 horizontalOffset = new Vector3(offset.x, 0, offset.z);
             float horizontalDistance = horizontalOffset.magnitude;
             float heightDifference = Mathf.Abs(offset.y);
    
             if (horizontalDistance <= tileSize * 1.6f && heightDifference <= 2f)
             {
                 if (!neighbours.Contains(tile.gameObject))
                 {
                     neighbours.Add(tile.gameObject);
                 }
             }
         }
     }
    
   public virtual void ActivateEffect(SeasonState season)
    {
       
    }
}
