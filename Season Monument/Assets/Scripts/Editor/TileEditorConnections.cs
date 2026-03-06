using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public static partial class TileEditor
{
    // Connection mode state
    public static Tile ConnectionFirstTile { get; private set; }
    internal static bool ConnectionBidirectional = true;

    // Selected connection
    public static Tile SelectedConnectionTile { get; private set; }
    public static TileConnection SelectedConnection { get; private set; }

    public static void ClearSelectedConnection()
    {
        SelectedConnectionTile = null;
        SelectedConnection = null;
    }

    public static void CancelConnection()
    {
        ConnectionFirstTile = null;
    }

    private static void HandleConnectMode(Event e, Grid grid)
    {
        // Cancel on right-click or Escape
        if ((e.type == EventType.MouseDown && e.button == 1) ||
            (e.type == EventType.KeyDown && e.keyCode == KeyCode.Escape))
        {
            CancelConnection();
            e.Use();
            return;
        }

        // Raycast to find tile under cursor
        Vector2 mousePos = e.mousePosition;
        Ray ray = HandleUtility.GUIPointToWorldRay(mousePos);

        int layerMask = 1 << TILE_LAYER;
        if (Physics.Raycast(ray, out RaycastHit hit, 1000f, layerMask))
        {
            Tile hoveredTile = hit.collider.GetComponent<Tile>();
            if (hoveredTile != null)
            {
                // Highlight hovered tile
                Color highlightColor = ConnectionFirstTile == null ? Color.cyan : Color.magenta;
                Handles.color = highlightColor;
                Handles.DrawWireCube(hit.collider.bounds.center, Vector3.one * TILE_SIZE * 1.05f);

                // Draw line from first tile to cursor if first tile is selected
                if (ConnectionFirstTile != null)
                {
                    Handles.color = Color.cyan;
                    Handles.DrawDottedLine(
                        ConnectionFirstTile.transform.position + Vector3.up * 0.5f,
                        hoveredTile.transform.position + Vector3.up * 0.5f,
                        4f);
                }

                if (e.type == EventType.MouseDown && e.button == 0)
                {
                    if (ConnectionFirstTile == null)
                    {
                        // Select first tile
                        ConnectionFirstTile = hoveredTile;
                    }
                    else if (hoveredTile != ConnectionFirstTile)
                    {
                        // Create connection between first and second tile
                        CreateConnection(ConnectionFirstTile, hoveredTile, ConnectionBidirectional);
                        ConnectionFirstTile = null;
                    }
                    e.Use();
                }
            }
        }
        else if (ConnectionFirstTile != null)
        {
            // Draw line from first tile to mouse world position
            Vector3 worldPoint = ray.GetPoint(10f);
            Handles.color = new Color(0f, 1f, 1f, 0.3f);
            Handles.DrawDottedLine(
                ConnectionFirstTile.transform.position + Vector3.up * 0.5f,
                worldPoint,
                4f);
        }

        // Highlight first selected tile
        if (ConnectionFirstTile != null)
        {
            Handles.color = Color.cyan;
            Handles.DrawWireCube(ConnectionFirstTile.transform.position, Vector3.one * TILE_SIZE * 1.1f);

            GUIStyle labelStyle = new GUIStyle(EditorStyles.boldLabel);
            labelStyle.normal.textColor = Color.cyan;
            Handles.Label(ConnectionFirstTile.transform.position + Vector3.up * 1.2f, "First Tile", labelStyle);
        }
    }

    private static void CreateConnection(Tile from, Tile to, bool bidirectional)
    {
        // Check if connection already exists
        foreach (TileConnection existing in from.tileConnections)
        {
            if (existing.connectedTile == to)
            {
                Debug.LogWarning($"Connection already exists from {from.gameObject.name} to {to.gameObject.name}");
                return;
            }
        }

        Undo.RecordObject(from, "Create Tile Connection");
        TileConnection connection = new TileConnection
        {
            connectedTile = to,
            bidirectional = bidirectional
        };
        from.tileConnections.Add(connection);
        EditorUtility.SetDirty(from);

        // If bidirectional, also add the reverse connection on the other tile
        if (bidirectional)
        {
            bool reverseExists = false;
            foreach (TileConnection existing in to.tileConnections)
            {
                if (existing.connectedTile == from)
                {
                    reverseExists = true;
                    break;
                }
            }

            if (!reverseExists)
            {
                Undo.RecordObject(to, "Create Tile Connection");
                TileConnection reverse = new TileConnection
                {
                    connectedTile = from,
                    bidirectional = bidirectional
                };
                to.tileConnections.Add(reverse);
                EditorUtility.SetDirty(to);
            }
        }

        Debug.Log($"Connected {from.gameObject.name} <-> {to.gameObject.name} (bidirectional: {bidirectional})");
    }

    private static void DrawExistingConnections()
    {
        Tile[] allTiles = Object.FindObjectsByType<Tile>(FindObjectsSortMode.None);

        HashSet<(int, int)> drawn = new HashSet<(int, int)>();

        foreach (Tile tile in allTiles)
        {
            foreach (TileConnection connection in tile.tileConnections)
            {
                if (connection.connectedTile == null) continue;

                int idA = tile.gameObject.GetInstanceID();
                int idB = connection.connectedTile.gameObject.GetInstanceID();

                // Avoid drawing the same line twice for bidirectional connections
                var key = idA < idB ? (idA, idB) : (idB, idA);
                if (drawn.Contains(key)) continue;
                drawn.Add(key);

                bool isValid = tile.isWalkable && connection.connectedTile.isWalkable;
                Handles.color = isValid ? Color.blue : Color.red;

                Vector3 from = tile.transform.position + Vector3.up * 0.5f;
                Vector3 to = connection.connectedTile.transform.position + Vector3.up * 0.5f;

                Handles.DrawLine(from, to, 3f);

                // Draw arrow at midpoint
                Vector3 mid = (from + to) * 0.5f;
                Handles.SphereHandleCap(0, mid, Quaternion.identity, 0.15f, EventType.Repaint);
            }
        }
    }

    private static void HandleDeleteConnectionMode(Event e)
    {
        Tile[] allTiles = Object.FindObjectsByType<Tile>(FindObjectsSortMode.None);

        Vector2 mousePos = e.mousePosition;
        Ray ray = HandleUtility.GUIPointToWorldRay(mousePos);

        // Find the closest connection midpoint to the mouse ray
        float closestDist = float.MaxValue;
        Tile closestTileA = null;
        Tile closestTileB = null;
        Vector3 closestMid = Vector3.zero;
        HashSet<(int, int)> checked_ = new HashSet<(int, int)>();

        foreach (Tile tile in allTiles)
        {
            foreach (TileConnection connection in tile.tileConnections)
            {
                if (connection.connectedTile == null) continue;

                int idA = tile.gameObject.GetInstanceID();
                int idB = connection.connectedTile.gameObject.GetInstanceID();
                var key = idA < idB ? (idA, idB) : (idB, idA);
                if (checked_.Contains(key)) continue;
                checked_.Add(key);

                Vector3 from = tile.transform.position + Vector3.up * 0.5f;
                Vector3 to = connection.connectedTile.transform.position + Vector3.up * 0.5f;
                Vector3 mid = (from + to) * 0.5f;

                // Distance from ray to midpoint
                float dist = Vector3.Cross(ray.direction, mid - ray.origin).magnitude;
                if (dist < closestDist)
                {
                    closestDist = dist;
                    closestTileA = tile;
                    closestTileB = connection.connectedTile;
                    closestMid = mid;
                }
            }
        }

        // Highlight closest connection if within threshold
        if (closestTileA != null && closestDist < 0.5f)
        {
            Vector3 fromPos = closestTileA.transform.position + Vector3.up * 0.5f;
            Vector3 toPos = closestTileB.transform.position + Vector3.up * 0.5f;

            Handles.color = Color.red;
            Handles.DrawLine(fromPos, toPos, 4f);
            Handles.SphereHandleCap(0, closestMid, Quaternion.identity, 0.25f, EventType.Repaint);

            GUIStyle labelStyle = new GUIStyle(EditorStyles.boldLabel);
            labelStyle.normal.textColor = Color.red;
            Handles.Label(closestMid + Vector3.up * 0.4f, "Click to delete", labelStyle);

            if (e.type == EventType.MouseDown && e.button == 0)
            {
                RemoveConnection(closestTileA, closestTileB);
                e.Use();
            }
        }
    }

    private static void RemoveConnection(Tile tileA, Tile tileB)
    {
        Undo.RecordObject(tileA, "Delete Tile Connection");
        tileA.tileConnections.RemoveAll(c => c.connectedTile == tileB);
        EditorUtility.SetDirty(tileA);

        Undo.RecordObject(tileB, "Delete Tile Connection");
        tileB.tileConnections.RemoveAll(c => c.connectedTile == tileA);
        EditorUtility.SetDirty(tileB);

        Debug.Log($"Removed connection between {tileA.gameObject.name} and {tileB.gameObject.name}");
    }
}
