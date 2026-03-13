using System.Collections.Generic;
using UnityEngine;

public abstract class Tile : MonoBehaviour
{
    internal Tile parent;
    internal List<Tile> neighbours = new List<Tile>();

    [SerializeField] protected bool _isWalkable = true;
    public virtual bool isWalkable
    {
        get => _isWalkable;
        set => _isWalkable = value;
    }

    protected Material materialInstance;

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
        materialInstance = Application.isPlaying ? rend.material : rend.sharedMaterial;

        SeasonEvents.OnSeasonChanged += OnSeasonChanged;
        CameraEvents.OnPerspectiveChanged += OnPerspectiveChanged;
    }

    public virtual void OnDisable()
    {
        SeasonEvents.OnSeasonChanged -= OnSeasonChanged;
        CameraEvents.OnPerspectiveChanged -= OnPerspectiveChanged;
    }

    public virtual void Start()
    {
        FindNeigbour();
        ValidateConnections();
        ApplyMeshOverride();
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
        ValidateConnections();
    }

    private void OnPerspectiveChanged(CameraController.CameraState _)
    {
        ValidateConnections();
    }

    // A connection is valid if:
    // - both tiles are walkable
    // - this connection allows the current perspective
    // - the reverse connection (if it exists) also allows the current perspective
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

            bool thisAllows = connection.IsActiveForPerspective(perspective);

            TileConnection reverse = connection.connectedTile.GetConnectionTo(this);
            bool reverseAllows = reverse == null || reverse.IsActiveForPerspective(perspective);

            connection.valid = isWalkable
                && connection.connectedTile.isWalkable
                && thisAllows
                && reverseAllows;
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

    // Returns all currently valid connection neighbours
    public List<Tile> GetValidConnectionNeighbours()
    {
        List<Tile> result = new List<Tile>();
        foreach (TileConnection connection in tileConnections)
        {
            if (connection.valid)
                result.Add(connection.connectedTile);
        }
        return result;
    }

    public void FindNeigbour()
    {
        neighbours.Clear();
        Tile[] allTiles = FindObjectsByType<Tile>(FindObjectsSortMode.None);
        BoxCollider col = GetComponent<BoxCollider>();

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
            float dx = Mathf.Abs(offset.x);
            float dy = Mathf.Abs(offset.y);
            float dz = Mathf.Abs(offset.z);

            // Axis-aligned only: exactly one of dx/dz is non-zero
            bool axisAligned = (dx < 0.1f) != (dz < 0.1f);
            if (axisAligned && dy <= 2f && Mathf.Max(dx, dz) <= tileSize + 0.1f)
                neighbours.Add(tile);
        }
    }

    public virtual void ActivateEffect(SeasonState season) { }
}
