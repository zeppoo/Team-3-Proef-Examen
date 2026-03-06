
using System.Collections.Generic;
using UnityEngine;
public abstract class Tile : MonoBehaviour
{
    internal Tile parent;
    internal List<GameObject> neighbours = new List<GameObject>();

    [SerializeField] protected bool _isWalkable = true;
    public virtual bool isWalkable
    {
        get => _isWalkable;
        set => _isWalkable = value;
    }
    protected Material materialInstance;

    [SerializeField] private TileData tileData;
    public List<TileConnection> tileConnections = new List<TileConnection>();

    [Header("Mesh Override")]
    [SerializeField] private Mesh overrideMesh;
    [SerializeField] private Vector3 meshRotation = Vector3.zero;

    [Header("Decoration")]
    [SerializeField] public bool hasDecoration = false;
    [SerializeField] public GameObject decoration = null;

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
        FindNeigbour();
        ApplyTileData();
        ValidateConnections();
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
        if (tileData != null)
        {
            ApplyTileData();

            if (tileData.TryGetSeasonOverride(season, out SeasonOverride overrideData))
            {
                if (overrideData.overrideColor)
                    materialInstance.color = overrideData.tileColor;
            }

            tileData.OnTileSeasonChanged?.Invoke(season);
        }

        ValidateConnections();
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
        CameraController.CameraState perspective = CameraController.ActivePerspective;
        foreach (TileConnection connection in tileConnections)
        {
            if (connection.connectedTile == null)
            {
                connection.valid = false;
                continue;
            }

            bool wasValid = connection.valid;
            connection.valid = isWalkable
                && connection.connectedTile.isWalkable
                && connection.IsActiveForPerspective(perspective);

            if (connection.valid && !wasValid)
            {
                // Connection became valid: add to neighbours
                if (!neighbours.Contains(connection.connectedTile.gameObject))
                    neighbours.Add(connection.connectedTile.gameObject);

                if (connection.bidirectional && !connection.connectedTile.neighbours.Contains(gameObject))
                    connection.connectedTile.neighbours.Add(gameObject);
            }
            else if (!connection.valid && wasValid)
            {
                // Connection became invalid: remove from neighbours
                neighbours.Remove(connection.connectedTile.gameObject);

                if (connection.bidirectional)
                    connection.connectedTile.neighbours.Remove(gameObject);
            }
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
        float tileSize = col.bounds.size.x;

        foreach (Tile tile in allTiles)
        {
            if (tile == this) continue;

            Vector3 offset = tile.transform.position - transform.position;

            // Only allow cardinal directions (exactly one axis differs)
            int axisCount = 0;
            if (Mathf.Abs(offset.x) > 0.01f) axisCount++;
            if (Mathf.Abs(offset.y) > 0.01f) axisCount++;
            if (Mathf.Abs(offset.z) > 0.01f) axisCount++;

            if (axisCount == 1 && offset.magnitude <= tileSize + 0.1f)
            {
                neighbours.Add(tile.gameObject);
            }
        }
    }
}
