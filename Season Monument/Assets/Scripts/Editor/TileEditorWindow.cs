using UnityEngine;
using UnityEditor;

public class TileEditorWindow : EditorWindow
{
    public enum TileType { Grass, Path, Rock, Water, Empty }
    public enum ToolMode { Select, Place, Delete, Connect, DeleteConnection }
    public enum PlacementMode { Grid, Free }

    private TileType selectedTileType = TileType.Grass;
    private ToolMode currentMode = ToolMode.Select;
    private PlacementMode placementMode = PlacementMode.Grid;
    private bool toolActive = false;

    private GameObject grassPrefab;
    private GameObject pathPrefab;
    private GameObject rockPrefab;
    private GameObject waterPrefab;
    private GameObject emptyPrefab;

    private Grid[] sceneGrids;
    private string[] gridNames;
    private int selectedGridIndex = 0;

    private float freeDistance = 10f;

    // Section foldouts
    private bool showTileSection = true;
    private bool showConnectionSection = true;
    private bool showMeshOverrideSection = true;

    // Mesh override
    private Mesh overrideMesh;
    private Vector3 meshRotation = Vector3.zero;

    [MenuItem("Tools/Tile Editor")]
    public static void ShowWindow()
    {
        GetWindow<TileEditorWindow>("Tile Editor");
    }

    private void OnEnable()
    {
        LoadPrefabs();
        RefreshGrids();
        SceneView.duringSceneGui += OnSceneGUI;
    }

    private void OnDisable()
    {
        SceneView.duringSceneGui -= OnSceneGUI;
        TileEditor.Cleanup();
    }

    private void LoadPrefabs()
    {
        grassPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Tiles/GrassTile.prefab");
        pathPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Tiles/PathTile.prefab");
        rockPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Tiles/RockTile.prefab");
        waterPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Tiles/WaterTile.prefab");
        emptyPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Tiles/EmptyTile.prefab");
    }

    private void RefreshGrids()
    {
        sceneGrids = FindObjectsByType<Grid>(FindObjectsSortMode.None);
        gridNames = new string[sceneGrids.Length];
        for (int i = 0; i < sceneGrids.Length; i++)
        {
            gridNames[i] = sceneGrids[i].gameObject.name;
        }
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField("Tile Editor", EditorStyles.boldLabel);
        EditorGUILayout.Space(5);

        toolActive = EditorGUILayout.Toggle("Tool Active", toolActive);

        EditorGUILayout.Space(5);

        // Grid selector
        if (sceneGrids == null || sceneGrids.Length == 0)
        {
            EditorGUILayout.HelpBox("No Grid components found in scene.", MessageType.Warning);
            if (GUILayout.Button("Refresh Grids"))
                RefreshGrids();
        }
        else
        {
            EditorGUILayout.BeginHorizontal();
            selectedGridIndex = EditorGUILayout.Popup("Grid", selectedGridIndex, gridNames);
            if (GUILayout.Button("Refresh", GUILayout.Width(60)))
                RefreshGrids();
            EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.Space(10);

        // --- Tiles Section ---
        showTileSection = EditorGUILayout.BeginFoldoutHeaderGroup(showTileSection, "Tiles");
        if (showTileSection)
        {
            EditorGUI.indentLevel++;

            // Main tool mode: Select, Place, Delete
            EditorGUILayout.BeginHorizontal();
            GUI.backgroundColor = currentMode == ToolMode.Select ? Color.cyan : Color.white;
            if (GUILayout.Button("Select")) currentMode = ToolMode.Select;
            GUI.backgroundColor = currentMode == ToolMode.Place ? Color.cyan : Color.white;
            if (GUILayout.Button("Place")) currentMode = ToolMode.Place;
            GUI.backgroundColor = currentMode == ToolMode.Delete ? Color.red * 0.8f + Color.white * 0.2f : Color.white;
            if (GUILayout.Button("Delete")) currentMode = ToolMode.Delete;
            GUI.backgroundColor = Color.white;
            EditorGUILayout.EndHorizontal();

            if (currentMode == ToolMode.Select)
            {
                EditorGUILayout.HelpBox("Left-click on a tile to select it.", MessageType.Info);
            }
            else if (currentMode == ToolMode.Place)
            {
                EditorGUILayout.Space(5);

                // Placement mode: Grid, Free
                EditorGUILayout.BeginHorizontal();
                GUI.backgroundColor = placementMode == PlacementMode.Grid ? Color.cyan : Color.white;
                if (GUILayout.Button("Grid")) placementMode = PlacementMode.Grid;
                GUI.backgroundColor = placementMode == PlacementMode.Free ? Color.cyan : Color.white;
                if (GUILayout.Button("Free")) placementMode = PlacementMode.Free;
                GUI.backgroundColor = Color.white;
                EditorGUILayout.EndHorizontal();

                selectedTileType = (TileType)EditorGUILayout.EnumPopup("Tile Type", selectedTileType);

                if (placementMode == PlacementMode.Free)
                {
                    freeDistance = EditorGUILayout.Slider("Distance", freeDistance, 2f, 50f);
                }

                EditorGUILayout.Space(5);
                string helpText = placementMode switch
                {
                    PlacementMode.Grid => "Left-click to place tiles on the grid.\nClick tile faces to place adjacent.",
                    PlacementMode.Free => "Left-click to place at cursor position.\nSnaps to nearest grid cell.",
                    _ => ""
                };
                EditorGUILayout.HelpBox(helpText, MessageType.Info);
            }
            else if (currentMode == ToolMode.Delete)
            {
                EditorGUILayout.HelpBox("Left-click on a tile to delete it.", MessageType.Info);
            }

            EditorGUI.indentLevel--;
        }
        EditorGUILayout.EndFoldoutHeaderGroup();

        EditorGUILayout.Space(10);

        // --- Connections Section ---
        showConnectionSection = EditorGUILayout.BeginFoldoutHeaderGroup(showConnectionSection, "Connections");
        if (showConnectionSection)
        {
            EditorGUI.indentLevel++;

            bool isConnectionMode = currentMode == ToolMode.Connect || currentMode == ToolMode.DeleteConnection;

            EditorGUILayout.BeginHorizontal();
            GUI.backgroundColor = currentMode == ToolMode.Connect ? Color.cyan : Color.white;
            if (GUILayout.Button("Create"))
            {
                currentMode = ToolMode.Connect;
            }
            GUI.backgroundColor = currentMode == ToolMode.DeleteConnection ? Color.red * 0.8f + Color.white * 0.2f : Color.white;
            if (GUILayout.Button("Delete"))
            {
                currentMode = ToolMode.DeleteConnection;
            }
            GUI.backgroundColor = Color.white;

            if (isConnectionMode)
            {
                if (GUILayout.Button("Stop", GUILayout.Width(60)))
                {
                    TileEditor.CancelConnection();
                    currentMode = ToolMode.Select;
                }
            }
            EditorGUILayout.EndHorizontal();

            if (currentMode == ToolMode.Connect)
            {
                TileEditor.ConnectionBidirectional = EditorGUILayout.Toggle("Bidirectional", TileEditor.ConnectionBidirectional);

                if (TileEditor.ConnectionFirstTile != null)
                {
                    EditorGUILayout.HelpBox($"First tile: {TileEditor.ConnectionFirstTile.gameObject.name}\nClick a second tile to complete the connection.", MessageType.Info);

                    if (GUILayout.Button("Cancel Selection"))
                    {
                        TileEditor.CancelConnection();
                    }
                }
                else
                {
                    EditorGUILayout.HelpBox("Click on a tile to start a connection.\nRight-click or Escape to cancel.", MessageType.Info);
                }
            }
            else if (currentMode == ToolMode.DeleteConnection)
            {
                EditorGUILayout.HelpBox("Click on a connection line to delete it.", MessageType.Info);
            }

            EditorGUI.indentLevel--;
        }
        EditorGUILayout.EndFoldoutHeaderGroup();

        EditorGUILayout.Space(10);

        // --- Mesh Override Section ---
        showMeshOverrideSection = EditorGUILayout.BeginFoldoutHeaderGroup(showMeshOverrideSection, "Mesh Override");
        if (showMeshOverrideSection)
        {
            EditorGUI.indentLevel++;

            overrideMesh = (Mesh)EditorGUILayout.ObjectField("Mesh", overrideMesh, typeof(Mesh), false);

            // Rotation display
            EditorGUILayout.LabelField("Rotation", $"({meshRotation.x}, {meshRotation.y}, {meshRotation.z})");

            // 90-degree rotation buttons
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("X +90")) meshRotation.x = (meshRotation.x + 90f) % 360f;
            if (GUILayout.Button("X -90")) meshRotation.x = (meshRotation.x - 90f + 360f) % 360f;
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Y +90")) meshRotation.y = (meshRotation.y + 90f) % 360f;
            if (GUILayout.Button("Y -90")) meshRotation.y = (meshRotation.y - 90f + 360f) % 360f;
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Z +90")) meshRotation.z = (meshRotation.z + 90f) % 360f;
            if (GUILayout.Button("Z -90")) meshRotation.z = (meshRotation.z - 90f + 360f) % 360f;
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Reset Rotation")) meshRotation = Vector3.zero;
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(5);

            if (GUILayout.Button("Apply to Selected Tiles"))
            {
                ApplyMeshOverrideToSelection();
            }

            if (GUILayout.Button("Clear Mesh Override"))
            {
                ClearMeshOverrideFromSelection();
            }

            EditorGUILayout.Space(5);

            int selectedCount = GetSelectedTileCount();
            if (selectedCount == 0)
            {
                EditorGUILayout.HelpBox("Select one or more tiles in the scene to apply the mesh override.", MessageType.Info);
            }
            else
            {
                EditorGUILayout.HelpBox($"{selectedCount} tile(s) selected.", MessageType.None);
            }

            EditorGUI.indentLevel--;
        }
        EditorGUILayout.EndFoldoutHeaderGroup();
    }

    private int GetSelectedTileCount()
    {
        int count = 0;
        foreach (GameObject obj in Selection.gameObjects)
        {
            if (obj.GetComponent<Tile>() != null)
                count++;
        }
        return count;
    }

    private void ApplyMeshOverrideToSelection()
    {
        if (overrideMesh == null)
        {
            Debug.LogWarning("TileEditor: No mesh selected for override.");
            return;
        }

        int applied = 0;
        foreach (GameObject obj in Selection.gameObjects)
        {
            Tile tile = obj.GetComponent<Tile>();
            if (tile == null) continue;

            Undo.RecordObject(tile, "Apply Mesh Override");

            SerializedObject so = new SerializedObject(tile);
            so.FindProperty("overrideMesh").objectReferenceValue = overrideMesh;
            so.FindProperty("meshRotation").vector3Value = meshRotation;
            so.ApplyModifiedProperties();

            tile.ApplyMeshOverride();
            EditorUtility.SetDirty(tile);
            applied++;
        }

        if (applied == 0)
            Debug.LogWarning("TileEditor: No tiles found in selection.");
        else
            Debug.Log($"TileEditor: Applied mesh override to {applied} tile(s).");
    }

    private void ClearMeshOverrideFromSelection()
    {
        int cleared = 0;
        foreach (GameObject obj in Selection.gameObjects)
        {
            Tile tile = obj.GetComponent<Tile>();
            if (tile == null) continue;

            Undo.RecordObject(tile, "Clear Mesh Override");

            SerializedObject so = new SerializedObject(tile);
            so.FindProperty("overrideMesh").objectReferenceValue = null;
            so.FindProperty("meshRotation").vector3Value = Vector3.zero;
            so.ApplyModifiedProperties();

            // Reset mesh to default cube
            MeshFilter mf = obj.GetComponent<MeshFilter>();
            if (mf != null)
            {
                Undo.RecordObject(mf, "Clear Mesh Override");
                mf.sharedMesh = Resources.GetBuiltinResource<Mesh>("Cube.fbx");
            }

            // Reset rotation
            Undo.RecordObject(obj.transform, "Clear Mesh Override");
            obj.transform.localEulerAngles = Vector3.zero;

            EditorUtility.SetDirty(tile);
            cleared++;
        }

        if (cleared == 0)
            Debug.LogWarning("TileEditor: No tiles found in selection.");
        else
            Debug.Log($"TileEditor: Cleared mesh override from {cleared} tile(s).");
    }

    private void OnSceneGUI(SceneView sceneView)
    {
        if (!toolActive) return;
        if (sceneGrids == null || sceneGrids.Length == 0) return;
        if (selectedGridIndex >= sceneGrids.Length) return;

        Grid selectedGrid = sceneGrids[selectedGridIndex];
        if (selectedGrid == null)
        {
            RefreshGrids();
            return;
        }

        HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));

        TileEditor.OnSceneGUI(sceneView, currentMode, placementMode, GetSelectedPrefab(), selectedGrid, freeDistance);
    }

    private GameObject GetSelectedPrefab()
    {
        return selectedTileType switch
        {
            TileType.Grass => grassPrefab,
            TileType.Path => pathPrefab,
            TileType.Rock => rockPrefab,
            TileType.Water => waterPrefab,
            TileType.Empty => emptyPrefab,
            _ => grassPrefab
        };
    }
}
