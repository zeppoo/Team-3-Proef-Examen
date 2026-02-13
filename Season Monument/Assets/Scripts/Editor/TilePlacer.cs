using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public static class TilePlacer
{
    private const float TILE_SIZE = 1.0f;
    private const int TILE_LAYER = 6;

    // Cached grid transform for parenting placed tiles
    private static Transform gridParent;

    public static void OnSceneGUI(SceneView sceneView, TileEditorWindow.ToolMode mode, GameObject prefab, Grid grid, float freeDistance = 10f)
    {
        Event e = Event.current;

        // Parent tiles under the Grid object itself
        gridParent = grid.transform;

        DrawGridVisualization(grid);
        DrawPlusIndicators(grid);

        if (mode == TileEditorWindow.ToolMode.Free)
        {
            // Free mode: place at a fixed distance from camera, snapped to grid
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
    }

    private struct RaycastResult
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
        // The grid's "flat" axis (cellSize.z = 0) maps to different world axes:
        // XZY swizzle: grid lies on XZ plane, normal = Y (floor grid)
        // XYZ swizzle: grid lies on XY plane, normal = Z (wall grid)
        // ZXY swizzle: grid lies on ZX plane, normal = Y-ish but rotated
        // Use CellToWorld to derive the plane normal from the grid's two axes.
        Vector3 gridOrigin = grid.CellToWorld(Vector3Int.zero);
        Vector3 axisA = grid.CellToWorld(new Vector3Int(1, 0, 0)) - gridOrigin;
        Vector3 axisB = grid.CellToWorld(new Vector3Int(0, 1, 0)) - gridOrigin;
        Vector3 planeNormal = Vector3.Cross(axisA, axisB).normalized;

        if (planeNormal.sqrMagnitude < 0.01f)
        {
            // Fallback if cross product is degenerate
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

    private static Vector3 SnapToGridCenter(Vector3 worldPoint, Grid grid)
    {
        // Snap to the center of the grid cell on the grid's plane axes,
        // and round to the nearest integer on the perpendicular axis.
        Vector3Int cell = grid.WorldToCell(worldPoint);
        Vector3 corner = grid.CellToWorld(cell);

        // Get the grid's two plane axes in world space
        Vector3 gridOrigin = grid.CellToWorld(Vector3Int.zero);
        Vector3 axisA = grid.CellToWorld(new Vector3Int(1, 0, 0)) - gridOrigin;
        Vector3 axisB = grid.CellToWorld(new Vector3Int(0, 1, 0)) - gridOrigin;

        // Offset by half a cell along each grid axis to reach cell center
        Vector3 snapped = corner + axisA * 0.5f + axisB * 0.5f;

        // The grid plane doesn't control the perpendicular axis,
        // so round each world axis to the nearest integer to keep tiles
        // at whole positions (e.g. Y=0 on a floor grid, not Y=0.5).
        // Only round axes that are perpendicular to the grid plane.
        Vector3 planeNormal = Vector3.Cross(axisA, axisB).normalized;

        // On the perpendicular axis, snap to integer + 0.5 so tiles sit
        // on top of the grid plane (e.g. Y=0.5 on a floor grid).
        if (Mathf.Abs(planeNormal.x) > 0.5f)
            snapped.x = Mathf.Floor(worldPoint.x) + 0.5f;
        if (Mathf.Abs(planeNormal.y) > 0.5f)
            snapped.y = Mathf.Floor(worldPoint.y) + 0.5f;
        if (Mathf.Abs(planeNormal.z) > 0.5f)
            snapped.z = Mathf.Floor(worldPoint.z) + 0.5f;

        return snapped;
    }

    private static Vector3 CalculatePlacementPosition(RaycastResult result, Grid grid)
    {
        if (result.hitTile)
        {
            // When hitting an existing tile, offset by the face normal direction
            Vector3 tileCenter = result.hit.collider.bounds.center;
            Vector3 normal = result.hit.normal;

            Vector3 dir = new Vector3(
                Mathf.Round(normal.x),
                Mathf.Round(normal.y),
                Mathf.Round(normal.z));

            Vector3 rawPosition = tileCenter + dir * TILE_SIZE;
            return SnapToGridCenter(rawPosition, grid);
        }
        else if (result.hitGroundPlane)
        {
            return SnapToGridCenter(result.groundPoint, grid);
        }

        return Vector3.zero;
    }

    private static Vector3 CalculateFreePosition(SceneView sceneView, Grid grid, float distance)
    {
        // Cast a ray from the mouse and place at a fixed distance from the camera
        Vector2 mousePos = Event.current.mousePosition;
        Ray ray = HandleUtility.GUIPointToWorldRay(mousePos);

        Vector3 rawPosition = ray.GetPoint(distance);

        // Snap to nearest integer grid position
        rawPosition.x = Mathf.Round(rawPosition.x);
        rawPosition.y = Mathf.Round(rawPosition.y);
        rawPosition.z = Mathf.Round(rawPosition.z);

        return rawPosition;
    }

    private static void DrawGridVisualization(Grid grid)
    {
        Transform tilemap = grid.transform.Find("Tilemap");
        if (tilemap == null) return;

        // Draw a grid of lines on the plane defined by this grid's orientation
        Vector3 origin = grid.transform.position;
        int halfExtent = 50;

        Handles.color = new Color(1f, 1f, 1f, 0.15f);

        // Determine the two world-space axes based on the grid's swizzle
        // by converting cell offsets to world positions
        Vector3 cellRight = grid.CellToWorld(new Vector3Int(1, 0, 0)) - grid.CellToWorld(Vector3Int.zero);
        Vector3 cellUp = grid.CellToWorld(new Vector3Int(0, 1, 0)) - grid.CellToWorld(Vector3Int.zero);

        Vector3 gridOrigin = grid.CellToWorld(new Vector3Int(-halfExtent, -halfExtent, 0));

        // Draw lines along the cell-right direction
        for (int i = -halfExtent; i <= halfExtent; i++)
        {
            Vector3 start = grid.CellToWorld(new Vector3Int(-halfExtent, i, 0));
            Vector3 end = grid.CellToWorld(new Vector3Int(halfExtent, i, 0));
            Handles.DrawLine(start, end, 2f);
        }

        // Draw lines along the cell-up direction
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

        // Snap each tile's position to its grid center, then build a set of
        // those snapped positions (rounded to avoid float key issues).
        HashSet<Vector3Int> occupied = new HashSet<Vector3Int>();
        List<Vector3> snappedPositions = new List<Vector3>();

        foreach (Tile tile in allTiles)
        {
            Vector3 snapped = SnapToGridCenter(tile.transform.position, grid);
            snappedPositions.Add(snapped);
            occupied.Add(RoundToKey(snapped));
        }

        // Get the grid's world-space axes for planar neighbor offsets
        Vector3 gridOrigin = grid.CellToWorld(Vector3Int.zero);
        Vector3 axisA = grid.CellToWorld(new Vector3Int(1, 0, 0)) - gridOrigin;
        Vector3 axisB = grid.CellToWorld(new Vector3Int(0, 1, 0)) - gridOrigin;

        // 6 neighbor directions: 4 planar (grid axes) + 2 vertical (world up/down)
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
        // Round to nearest 0.5 then multiply by 2 to get a unique integer key.
        // This handles both integer and half-integer tile positions.
        return new Vector3Int(
            Mathf.RoundToInt(v.x * 2),
            Mathf.RoundToInt(v.y * 2),
            Mathf.RoundToInt(v.z * 2));
    }

    private static void DrawPlusSymbol(Vector3 center)
    {
        float halfSize = 0.2f;
        Handles.color = new Color(0f, 1f, 0f, 0.6f);

        // Draw "+" visible from multiple angles
        Handles.DrawLine(center + Vector3.left * halfSize, center + Vector3.right * halfSize, 2f);
        Handles.DrawLine(center + Vector3.forward * halfSize, center + Vector3.back * halfSize, 2f);
        Handles.DrawLine(center + Vector3.up * halfSize, center + Vector3.down * halfSize, 2f);
    }

    private static void DrawGhostPreview(Vector3 position, GameObject prefab)
    {
        Color ghostColor = GetGhostColor(prefab);
        Color fillColor = ghostColor;
        fillColor.a = 0.15f;
        Color outlineColor = ghostColor;
        outlineColor.a = 0.4f;

        // Wire cube outline
        Handles.color = ghostColor;
        Handles.DrawWireCube(position, Vector3.one * TILE_SIZE);

        float h = 0.5f;

        // Top face
        Handles.DrawSolidRectangleWithOutline(new Vector3[]
        {
            position + new Vector3(-h, h, -h),
            position + new Vector3( h, h, -h),
            position + new Vector3( h, h,  h),
            position + new Vector3(-h, h,  h)
        }, fillColor, outlineColor);

        // Bottom face
        Handles.DrawSolidRectangleWithOutline(new Vector3[]
        {
            position + new Vector3(-h, -h, -h),
            position + new Vector3( h, -h, -h),
            position + new Vector3( h, -h,  h),
            position + new Vector3(-h, -h,  h)
        }, fillColor, outlineColor);

        // Front face (+Z)
        Handles.DrawSolidRectangleWithOutline(new Vector3[]
        {
            position + new Vector3(-h, -h, h),
            position + new Vector3( h, -h, h),
            position + new Vector3( h,  h, h),
            position + new Vector3(-h,  h, h)
        }, fillColor, outlineColor);

        // Back face (-Z)
        Handles.DrawSolidRectangleWithOutline(new Vector3[]
        {
            position + new Vector3(-h, -h, -h),
            position + new Vector3( h, -h, -h),
            position + new Vector3( h,  h, -h),
            position + new Vector3(-h,  h, -h)
        }, fillColor, outlineColor);

        // Right face (+X)
        Handles.DrawSolidRectangleWithOutline(new Vector3[]
        {
            position + new Vector3(h, -h, -h),
            position + new Vector3(h, -h,  h),
            position + new Vector3(h,  h,  h),
            position + new Vector3(h,  h, -h)
        }, fillColor, outlineColor);

        // Left face (-X)
        Handles.DrawSolidRectangleWithOutline(new Vector3[]
        {
            position + new Vector3(-h, -h, -h),
            position + new Vector3(-h, -h,  h),
            position + new Vector3(-h,  h,  h),
            position + new Vector3(-h,  h, -h)
        }, fillColor, outlineColor);

        // Label
        GUIStyle labelStyle = new GUIStyle(EditorStyles.boldLabel);
        labelStyle.normal.textColor = ghostColor;
        Handles.Label(position + Vector3.up * 0.7f, prefab != null ? prefab.name : "Unknown", labelStyle);
    }

    private static Color GetGhostColor(GameObject prefab)
    {
        if (prefab == null) return Color.white;
        string name = prefab.name.ToLower();
        if (name.Contains("grass")) return Color.green;
        if (name.Contains("path")) return Color.yellow;
        if (name.Contains("rock")) return Color.gray;
        if (name.Contains("water")) return Color.cyan;
        return Color.white;
    }

    private static void PlaceTile(Vector3 position, GameObject prefab)
    {
        if (prefab == null)
        {
            Debug.LogWarning("TilePlacer: No prefab selected.");
            return;
        }

        // Check for overlap
        Tile[] allTiles = Object.FindObjectsByType<Tile>(FindObjectsSortMode.None);
        foreach (Tile tile in allTiles)
        {
            if (Vector3.Distance(tile.transform.position, position) < 0.1f)
            {
                Debug.LogWarning("TilePlacer: Position already occupied.");
                return;
            }
        }

        GameObject newTile = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
        newTile.transform.position = position;
        newTile.tag = "Tile";
        newTile.layer = TILE_LAYER;

        // Parent to the selected Grid object
        if (gridParent != null)
        {
            newTile.transform.SetParent(gridParent, true);
        }

        Undo.RegisterCreatedObjectUndo(newTile, "Place Tile");
        EditorUtility.SetDirty(newTile);
        Physics.SyncTransforms();
    }

    private static void DeleteTile(RaycastHit hit)
    {
        GameObject tileObj = hit.collider.gameObject;
        if (tileObj.CompareTag("Tile"))
        {
            Undo.DestroyObjectImmediate(tileObj);
        }
    }
}
