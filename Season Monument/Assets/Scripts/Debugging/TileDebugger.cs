using UnityEngine;
using UnityEditor;

public class TileDebugger : EditorWindow
{
    private bool debugDraw = true;
    private float gizmoHeight = 1.0f;
    private float gizmoSize = 0.15f;

    private bool showWalkable = true;
    private bool showNonWalkable = true;
    private bool showNeighbourLines = false;
    private float neighbourLineThickness = 2f;

    private Vector2 scrollPosition;
    private Tile selectedTile;

    [MenuItem("Tools/Tile Debugger")]
    public static void ShowWindow()
    {
        GetWindow<TileDebugger>("Tile Debugger");
    }

    private void OnEnable()
    {
        SceneView.duringSceneGui -= OnSceneGUI;
        SceneView.duringSceneGui += OnSceneGUI;
    }

    private void OnDisable()
    {
        SceneView.duringSceneGui -= OnSceneGUI;
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField("Tile Debugger", EditorStyles.boldLabel);
        EditorGUILayout.Space(5);

        debugDraw = EditorGUILayout.Toggle("Draw Gizmos", debugDraw);

        EditorGUILayout.Space(5);
        EditorGUILayout.LabelField("Gizmo Settings", EditorStyles.boldLabel);
        gizmoHeight = EditorGUILayout.Slider("Height", gizmoHeight, 0f, 5f);
        gizmoSize = EditorGUILayout.Slider("Size", gizmoSize, 0.05f, 1f);

        EditorGUILayout.Space(5);
        EditorGUILayout.LabelField("Filters", EditorStyles.boldLabel);
        showWalkable = EditorGUILayout.Toggle("Show Walkable", showWalkable);
        showNonWalkable = EditorGUILayout.Toggle("Show Non-Walkable", showNonWalkable);
        showNeighbourLines = EditorGUILayout.Toggle("Show Neighbour Lines", showNeighbourLines);
        if (showNeighbourLines)
        {
            neighbourLineThickness = EditorGUILayout.Slider("Line Thickness", neighbourLineThickness, 1f, 10f);
        }

        EditorGUILayout.Space(10);

        Tile[] allTiles = FindObjectsByType<Tile>(FindObjectsSortMode.None);

        int walkableCount = 0;
        int nonWalkableCount = 0;
        foreach (Tile tile in allTiles)
        {
            if (tile.isWalkable) walkableCount++;
            else nonWalkableCount++;
        }

        EditorGUILayout.LabelField("Tile Overview", EditorStyles.boldLabel);
        EditorGUILayout.LabelField($"Total Tiles: {allTiles.Length}");
        EditorGUILayout.LabelField($"Walkable: {walkableCount}  |  Non-Walkable: {nonWalkableCount}");

        EditorGUILayout.Space(5);

        if (GUILayout.Button("Find Neighbours (All Tiles)"))
        {
            FindAllNeighbours(allTiles);
        }

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Tile List", EditorStyles.boldLabel);

        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

        foreach (Tile tile in allTiles)
        {
            if (tile.isWalkable && !showWalkable) continue;
            if (!tile.isWalkable && !showNonWalkable) continue;

            EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);

            string typeName = tile.GetType().Name;
            string walkableLabel = tile.isWalkable ? "Walkable" : "Blocked";
            Color labelColor = tile.isWalkable ? Color.green : Color.red;

            GUIStyle colorStyle = new GUIStyle(EditorStyles.label);
            colorStyle.normal.textColor = labelColor;

            EditorGUILayout.LabelField(tile.gameObject.name, GUILayout.Width(150));
            EditorGUILayout.LabelField(typeName, GUILayout.Width(80));
            EditorGUILayout.LabelField(walkableLabel, colorStyle, GUILayout.Width(70));
            EditorGUILayout.LabelField($"Neighbours: {tile.neighbours.Count}", GUILayout.Width(100));

            if (GUILayout.Button("Select", GUILayout.Width(50)))
            {
                Selection.activeGameObject = tile.gameObject;
                SceneView.lastActiveSceneView.FrameSelected();
                selectedTile = tile;
            }

            EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.EndScrollView();

        if (selectedTile != null)
        {
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Selected Tile Details", EditorStyles.boldLabel);
            EditorGUILayout.LabelField($"Name: {selectedTile.gameObject.name}");
            EditorGUILayout.LabelField($"Type: {selectedTile.GetType().Name}");
            EditorGUILayout.LabelField($"Position: {selectedTile.transform.position}");
            EditorGUILayout.LabelField($"Walkable: {selectedTile.isWalkable}");
            EditorGUILayout.LabelField($"Visited: {selectedTile.visited}");
            EditorGUILayout.LabelField($"Neighbours: {selectedTile.neighbours.Count}");

            if (selectedTile.parent != null)
                EditorGUILayout.LabelField($"Parent Tile: {selectedTile.parent.gameObject.name}");

            if (selectedTile.connecetdTile != null)
                EditorGUILayout.LabelField($"Connected Tile: {selectedTile.connecetdTile.gameObject.name}");
        }

        if (debugDraw)
            SceneView.RepaintAll();
    }

    private const float TILE_SIZE = 1.0f;

    private void FindAllNeighbours(Tile[] allTiles)
    {
        foreach (Tile tile in allTiles)
        {
            tile.neighbours.Clear();
        }

        foreach (Tile tile in allTiles)
        {
            foreach (Tile other in allTiles)
            {
                if (other == tile) continue;

                Vector3 offset = other.transform.position - tile.transform.position;

                // Only match cardinal directions (X, Y, Z axis-aligned), not diagonal
                int axisCount = 0;
                if (Mathf.Abs(offset.x) > 0.01f) axisCount++;
                if (Mathf.Abs(offset.y) > 0.01f) axisCount++;
                if (Mathf.Abs(offset.z) > 0.01f) axisCount++;

                if (axisCount == 1 && offset.magnitude <= TILE_SIZE + 0.1f)
                {
                    tile.neighbours.Add(other.gameObject);
                }
            }
        }

        Debug.Log($"[TileDebugger] Found neighbours for {allTiles.Length} tiles.");
    }

    private void OnSceneGUI(SceneView sceneView)
    {
        if (!debugDraw) return;

        Tile[] allTiles = FindObjectsByType<Tile>(FindObjectsSortMode.None);

        Handles.zTest = UnityEngine.Rendering.CompareFunction.LessEqual;

        foreach (Tile tile in allTiles)
        {
            if (tile.isWalkable && !showWalkable) continue;
            if (!tile.isWalkable && !showNonWalkable) continue;

            Vector3 position = tile.transform.position + Vector3.up * gizmoHeight;

            Color color = tile.isWalkable ? Color.green : Color.red;

            if (selectedTile == tile)
                color = Color.cyan;

            Handles.color = color;
            Handles.SphereHandleCap(0, position, Quaternion.identity, gizmoSize * 2f, EventType.Repaint);

            if (showNeighbourLines)
            {
                foreach (GameObject neighbour in tile.neighbours)
                {
                    if (neighbour == null) continue;

                    Tile neighbourTile = neighbour.GetComponent<Tile>();
                    Handles.color = (neighbourTile != null && !neighbourTile.isWalkable) ? Color.red : Color.green;

                    Vector3 neighbourPos = neighbour.transform.position + Vector3.up * gizmoHeight;
                    Handles.DrawLine(position, neighbourPos, neighbourLineThickness);
                }
            }
        }
    }
}
