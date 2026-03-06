using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public static partial class TileEditor
{
    private const float TILE_SIZE = 1.0f;
    private const int TILE_LAYER = 6;

    // Cached grid transform for parenting placed tiles
    private static Transform gridParent;

    public static void OnSceneGUI(SceneView sceneView, TileEditorWindow.ToolMode mode, TileEditorWindow.PlacementMode placementMode, GameObject prefab, Grid grid, float freeDistance = 10f)
    {
        Event e = Event.current;

        // Parent tiles under the Grid object itself
        gridParent = grid.transform;

        DrawGridVisualization(grid);
        DrawPlusIndicators(grid);

        if (mode == TileEditorWindow.ToolMode.Select)
        {
            HandleSelectMode(e);
            sceneView.Repaint();
            return;
        }

        if (mode == TileEditorWindow.ToolMode.Place && placementMode == TileEditorWindow.PlacementMode.Free)
        {
            Vector3 placementPos = CalculateFreePosition(sceneView, grid, freeDistance);

            DrawGhostPreview(placementPos, prefab);

            if (e.type == EventType.MouseDown && e.button == 0)
            {
                PlaceTile(placementPos, prefab);
                e.Use();
            }

            sceneView.Repaint();
            return;
        }

        if (mode == TileEditorWindow.ToolMode.Connect)
        {
            HandleConnectMode(e, grid);
            DrawExistingConnections();
            sceneView.Repaint();
            return;
        }

        if (mode == TileEditorWindow.ToolMode.DeleteConnection)
        {
            HandleDeleteConnectionMode(e);
            DrawExistingConnections();
            sceneView.Repaint();
            return;
        }

        RaycastResult result = DoRaycast(grid);

        if (result.didHit)
        {
            Vector3 placementPos = CalculatePlacementPosition(result, grid);

            if (mode == TileEditorWindow.ToolMode.Place)
            {
                DrawGhostPreview(placementPos, prefab);

                if (e.type == EventType.MouseDown && e.button == 0)
                {
                    PlaceTile(placementPos, prefab);
                    e.Use();
                }
            }
            else if (mode == TileEditorWindow.ToolMode.Delete)
            {
                if (result.hitTile)
                {
                    Handles.color = new Color(1f, 0f, 0f, 0.5f);
                    Handles.DrawWireCube(result.hit.collider.bounds.center, Vector3.one * TILE_SIZE);

                    if (e.type == EventType.MouseDown && e.button == 0)
                    {
                        DeleteTile(result.hit);
                        e.Use();
                    }
                }
            }
        }
        sceneView.Repaint();
    }

    public static void Cleanup()
    {
        CancelConnection();
    }

    private static void HandleSelectMode(Event e)
    {
        // Draw connections so they're always visible in select mode
        DrawExistingConnections();
        DrawSelectedConnectionHighlight();

        if (e.type != EventType.MouseDown || e.button != 0) return;

        Vector2 mousePos = e.mousePosition;
        Ray ray = HandleUtility.GUIPointToWorldRay(mousePos);

        // Check for connection midpoint click first
        Tile[] allTiles = Object.FindObjectsByType<Tile>(FindObjectsSortMode.None);
        float closestDist = 0.4f; // click threshold
        Tile bestTile = null;
        TileConnection bestConn = null;

        foreach (Tile tile in allTiles)
        {
            foreach (TileConnection conn in tile.tileConnections)
            {
                if (conn.connectedTile == null) continue;
                Vector3 mid = (tile.transform.position + conn.connectedTile.transform.position) * 0.5f + Vector3.up * 0.5f;
                float dist = Vector3.Cross(ray.direction, mid - ray.origin).magnitude;
                if (dist < closestDist)
                {
                    closestDist = dist;
                    bestTile = tile;
                    bestConn = conn;
                }
            }
        }

        if (bestConn != null)
        {
            SelectedConnectionTile = bestTile;
            SelectedConnection = bestConn;
            e.Use();
            return;
        }

        // Fall back to tile selection
        int layerMask = 1 << TILE_LAYER;
        if (Physics.Raycast(ray, out RaycastHit hit, 1000f, layerMask))
        {
            GameObject hitObj = hit.collider.gameObject;
            if (hitObj.CompareTag("Tile"))
            {
                ClearSelectedConnection();
                Selection.activeGameObject = hitObj;
                e.Use();
            }
        }
    }

    private static void DrawSelectedConnectionHighlight()
    {
        if (SelectedConnection == null || SelectedConnectionTile == null) return;
        if (SelectedConnection.connectedTile == null) return;

        Vector3 from = SelectedConnectionTile.transform.position + Vector3.up * 0.5f;
        Vector3 to = SelectedConnection.connectedTile.transform.position + Vector3.up * 0.5f;
        Vector3 mid = (from + to) * 0.5f;

        Handles.color = Color.yellow;
        Handles.DrawLine(from, to, 5f);
        Handles.SphereHandleCap(0, mid, Quaternion.identity, 0.2f, EventType.Repaint);
    }

    internal struct RaycastResult
    {
        public bool didHit;
        public RaycastHit hit;
        public bool hitTile;
        public bool hitGroundPlane;
        public Vector3 groundPoint;
    }

    private static RaycastResult DoRaycast(Grid grid)
    {
        RaycastResult result = new RaycastResult();

        Vector2 mousePos = Event.current.mousePosition;
        Ray ray = HandleUtility.GUIPointToWorldRay(mousePos);

        int layerMask = 1 << TILE_LAYER;
        if (Physics.Raycast(ray, out RaycastHit hit, 1000f, layerMask))
        {
            result.didHit = true;
            result.hit = hit;
            result.hitTile = hit.collider.CompareTag("Tile");
            return result;
        }

        // Determine the grid's plane based on its swizzle/orientation.
        Vector3 gridOrigin = grid.CellToWorld(Vector3Int.zero);
        Vector3 axisA = grid.CellToWorld(new Vector3Int(1, 0, 0)) - gridOrigin;
        Vector3 axisB = grid.CellToWorld(new Vector3Int(0, 1, 0)) - gridOrigin;
        Vector3 planeNormal = Vector3.Cross(axisA, axisB).normalized;

        if (planeNormal.sqrMagnitude < 0.01f)
        {
            planeNormal = Vector3.up;
        }

        Plane gridPlane = new Plane(planeNormal, gridOrigin);
        if (gridPlane.Raycast(ray, out float enter))
        {
            result.didHit = true;
            result.hitGroundPlane = true;
            result.groundPoint = ray.GetPoint(enter);
            return result;
        }

        return result;
    }

    internal static Vector3 SnapToGridCenter(Vector3 worldPoint, Grid grid)
    {
        Vector3Int cell = grid.WorldToCell(worldPoint);
        Vector3 corner = grid.CellToWorld(cell);

        Vector3 gridOrigin = grid.CellToWorld(Vector3Int.zero);
        Vector3 axisA = grid.CellToWorld(new Vector3Int(1, 0, 0)) - gridOrigin;
        Vector3 axisB = grid.CellToWorld(new Vector3Int(0, 1, 0)) - gridOrigin;

        Vector3 snapped = corner + axisA * 0.5f + axisB * 0.5f;

        Vector3 planeNormal = Vector3.Cross(axisA, axisB).normalized;

        if (Mathf.Abs(planeNormal.x) > 0.5f)
            snapped.x = Mathf.Floor(worldPoint.x) + 0.5f;
        if (Mathf.Abs(planeNormal.y) > 0.5f)
            snapped.y = Mathf.Floor(worldPoint.y) + 0.5f;
        if (Mathf.Abs(planeNormal.z) > 0.5f)
            snapped.z = Mathf.Floor(worldPoint.z) + 0.5f;

        return snapped;
    }

    private static void DrawGridVisualization(Grid grid)
    {
        Transform tilemap = grid.transform.Find("Tilemap");
        if (tilemap == null) return;

        int halfExtent = 50;

        Handles.color = new Color(1f, 1f, 1f, 0.15f);

        for (int i = -halfExtent; i <= halfExtent; i++)
        {
            Vector3 start = grid.CellToWorld(new Vector3Int(-halfExtent, i, 0));
            Vector3 end = grid.CellToWorld(new Vector3Int(halfExtent, i, 0));
            Handles.DrawLine(start, end, 2f);
        }

        for (int i = -halfExtent; i <= halfExtent; i++)
        {
            Vector3 start = grid.CellToWorld(new Vector3Int(i, -halfExtent, 0));
            Vector3 end = grid.CellToWorld(new Vector3Int(i, halfExtent, 0));
            Handles.DrawLine(start, end, 2f);
        }
    }

    private static void DrawPlusIndicators(Grid grid)
    {
        Tile[] allTiles = Object.FindObjectsByType<Tile>(FindObjectsSortMode.None);
        if (allTiles.Length == 0) return;

        HashSet<Vector3Int> occupied = new HashSet<Vector3Int>();
        List<Vector3> snappedPositions = new List<Vector3>();

        foreach (Tile tile in allTiles)
        {
            Vector3 snapped = SnapToGridCenter(tile.transform.position, grid);
            snappedPositions.Add(snapped);
            occupied.Add(RoundToKey(snapped));
        }

        Vector3 gridOrigin = grid.CellToWorld(Vector3Int.zero);
        Vector3 axisA = grid.CellToWorld(new Vector3Int(1, 0, 0)) - gridOrigin;
        Vector3 axisB = grid.CellToWorld(new Vector3Int(0, 1, 0)) - gridOrigin;

        Vector3[] directions = new Vector3[]
        {
            axisA,
            -axisA,
            axisB,
            -axisB,
            Vector3.up * TILE_SIZE,
            Vector3.down * TILE_SIZE,
        };

        HashSet<Vector3Int> drawnIndicators = new HashSet<Vector3Int>();

        for (int i = 0; i < snappedPositions.Count; i++)
        {
            Vector3 tilePos = snappedPositions[i];

            foreach (Vector3 dir in directions)
            {
                Vector3 neighborWorld = SnapToGridCenter(tilePos + dir, grid);
                Vector3Int key = RoundToKey(neighborWorld);

                if (occupied.Contains(key)) continue;
                if (drawnIndicators.Contains(key)) continue;
                if (neighborWorld.y < -0.01f) continue;

                drawnIndicators.Add(key);
                DrawPlusSymbol(neighborWorld);
            }
        }
    }

    private static Vector3Int RoundToKey(Vector3 v)
    {
        return new Vector3Int(
            Mathf.RoundToInt(v.x * 2),
            Mathf.RoundToInt(v.y * 2),
            Mathf.RoundToInt(v.z * 2));
    }

    private static void DrawPlusSymbol(Vector3 center)
    {
        float halfSize = 0.2f;
        Handles.color = new Color(0f, 1f, 0f, 0.6f);

        Handles.DrawLine(center + Vector3.left * halfSize, center + Vector3.right * halfSize, 2f);
        Handles.DrawLine(center + Vector3.forward * halfSize, center + Vector3.back * halfSize, 2f);
        Handles.DrawLine(center + Vector3.up * halfSize, center + Vector3.down * halfSize, 2f);
    }
}
