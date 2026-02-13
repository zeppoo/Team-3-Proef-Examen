using UnityEngine;
using UnityEditor;

public class TileEditorWindow : EditorWindow
{
    public enum TileType { Grass, Path, Rock, Water }
    public enum ToolMode { Place, Free, Delete }

    private TileType selectedTileType = TileType.Grass;
    private ToolMode currentMode = ToolMode.Place;
    private bool toolActive = false;

    private GameObject grassPrefab;
    private GameObject pathPrefab;
    private GameObject rockPrefab;
    private GameObject waterPrefab;

    private Grid[] sceneGrids;
    private string[] gridNames;
    private int selectedGridIndex = 0;

    private float freeDistance = 10f;

    [MenuItem("Tools/Tile Placer")]
    public static void ShowWindow()
    {
        GetWindow<TileEditorWindow>("Tile Placer");
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
        TilePlacer.Cleanup();
    }

    private void LoadPrefabs()
    {
        grassPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Tiles/GrassTile.prefab");
        pathPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Tiles/PathTile.prefab");
        rockPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Tiles/RockTile.prefab");
        waterPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Tiles/WaterTile.prefab");
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
        EditorGUILayout.LabelField("Tile Placement Tool", EditorStyles.boldLabel);
        EditorGUILayout.Space(5);

        toolActive = EditorGUILayout.Toggle("Tool Active", toolActive);

        EditorGUILayout.Space(5);

        // Grid selector
        if (sceneGrids == null || sceneGrids.Length == 0)
        {
            EditorGUILayout.HelpBox("No Grid components found in scene. Make sure GridManager is in the scene.", MessageType.Warning);
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

        EditorGUILayout.Space(5);

        currentMode = (ToolMode)EditorGUILayout.EnumPopup("Mode", currentMode);

        if (currentMode == ToolMode.Place || currentMode == ToolMode.Free)
        {
            selectedTileType = (TileType)EditorGUILayout.EnumPopup("Tile Type", selectedTileType);
        }

        if (currentMode == ToolMode.Free)
        {
            freeDistance = EditorGUILayout.Slider("Distance", freeDistance, 2f, 50f);
        }

        EditorGUILayout.Space(10);

        string helpText = currentMode switch
        {
            ToolMode.Place => "Left-click in Scene View to place tiles.\nClick tile faces to place adjacent or stack on top.",
            ToolMode.Free => "Left-click to place at the cursor position.\nThe ghost snaps to the nearest grid cell at the set distance from the camera.",
            ToolMode.Delete => "Left-click on a tile to delete it.",
            _ => ""
        };
        EditorGUILayout.HelpBox(helpText, MessageType.Info);
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

        TilePlacer.OnSceneGUI(sceneView, currentMode, GetSelectedPrefab(), selectedGrid, freeDistance);
    }

    private GameObject GetSelectedPrefab()
    {
        return selectedTileType switch
        {
            TileType.Grass => grassPrefab,
            TileType.Path => pathPrefab,
            TileType.Rock => rockPrefab,
            TileType.Water => waterPrefab,
            _ => grassPrefab
        };
    }
}
