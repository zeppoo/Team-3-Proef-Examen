using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class EmptyTile : Tile
{
    public override bool isWalkable
    {
        get => _isWalkable;
        protected set => _isWalkable = value;
    }

    private bool _isWalkable = false;

    public override void Start()
    {
        base.Start();
        _isWalkable = false;
    }

    void LateUpdate()
    {
        if (Application.isPlaying)
        {
            UpdateWalkable();
        }
    }

    private void OnValidate()
    {
        UpdateWalkableEditor();
    }

    private void UpdateWalkable()
    {
        // Collect all tiles that are connected via TileConnection
        HashSet<GameObject> connectedObjects = new HashSet<GameObject>();
        foreach (TileConnection connection in tileConnections)
        {
            if (connection.connectedTile != null)
                connectedObjects.Add(connection.connectedTile.gameObject);
        }

        // Check if any non-connection neighbour is walkable
        foreach (GameObject neighbour in neighbours)
        {
            if (connectedObjects.Contains(neighbour)) continue;

            Tile neighbourTile = neighbour.GetComponent<Tile>();
            if (neighbourTile != null && neighbourTile.isWalkable)
            {
                _isWalkable = true;
                return;
            }
        }

        _isWalkable = false;
    }

    private void UpdateWalkableEditor()
    {
        BoxCollider col = GetComponent<BoxCollider>();
        if (col == null) return;

        // Collect all tiles that are connected via TileConnection
        HashSet<GameObject> connectedObjects = new HashSet<GameObject>();
        foreach (TileConnection connection in tileConnections)
        {
            if (connection.connectedTile != null)
                connectedObjects.Add(connection.connectedTile.gameObject);
        }

        // Find neighbours on the fly since Start() hasn't run in edit mode
        float tileSize = col.bounds.size.x;
        Tile[] allTiles = FindObjectsByType<Tile>(FindObjectsSortMode.None);

        foreach (Tile tile in allTiles)
        {
            if (tile == this) continue;
            if (connectedObjects.Contains(tile.gameObject)) continue;

            Vector3 offset = tile.transform.position - transform.position;

            int axisCount = 0;
            if (Mathf.Abs(offset.x) > 0.01f) axisCount++;
            if (Mathf.Abs(offset.y) > 0.01f) axisCount++;
            if (Mathf.Abs(offset.z) > 0.01f) axisCount++;

            if (axisCount == 1 && offset.magnitude <= tileSize + 0.1f && tile.isWalkable)
            {
                _isWalkable = true;
                return;
            }
        }

        _isWalkable = false;
    }
}
