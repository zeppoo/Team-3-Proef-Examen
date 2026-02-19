using UnityEngine;
using UnityEditor;

public static partial class TileEditor
{
    private static Vector3 CalculatePlacementPosition(RaycastResult result, Grid grid)
    {
        if (result.hitTile)
        {
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
        Vector2 mousePos = Event.current.mousePosition;
        Ray ray = HandleUtility.GUIPointToWorldRay(mousePos);

        Vector3 rawPosition = ray.GetPoint(distance);

        return SnapToGridCenter(rawPosition, grid);
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
        if (name.Contains("empty")) return Color.white;
        return Color.white;
    }

    private static void PlaceTile(Vector3 position, GameObject prefab)
    {
        if (prefab == null)
        {
            Debug.LogWarning("TileEditor: No prefab selected.");
            return;
        }

        // Check for overlap
        Tile[] allTiles = Object.FindObjectsByType<Tile>(FindObjectsSortMode.None);
        foreach (Tile tile in allTiles)
        {
            if (Vector3.Distance(tile.transform.position, position) < 0.1f)
            {
                Debug.LogWarning("TileEditor: Position already occupied.");
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
