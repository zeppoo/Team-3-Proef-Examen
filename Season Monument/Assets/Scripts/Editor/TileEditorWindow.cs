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
    private bool showDecorationSection = true;

    // Layout
    private Vector2 toolsScrollPos;
    private Vector2 inspectorScrollPos;

    // Decoration picker
    private GameObject[] decorationPrefabs = new GameObject[0];
    private string[] decorationNames = new string[0];
    private int selectedDecorationIndex = 0;

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
        LoadDecorationPrefabs();
        RefreshGrids();
        SceneView.duringSceneGui += OnSceneGUI;
        EditorApplication.update += Repaint;
    }

    private void OnDisable()
    {
        SceneView.duringSceneGui -= OnSceneGUI;
        EditorApplication.update -= Repaint;
        TileEditor.Cleanup();
    }

    private void LoadDecorationPrefabs()
    {
        string[] guids = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets/Prefabs/Decorations" });
        decorationPrefabs = new GameObject[guids.Length];
        decorationNames = new string[guids.Length];
        for (int i = 0; i < guids.Length; i++)
        {
            string path = AssetDatabase.GUIDToAssetPath(guids[i]);
            decorationPrefabs[i] = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            decorationNames[i] = decorationPrefabs[i].name;
        }
        selectedDecorationIndex = 0;
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
        float halfHeight = (position.height - 30f) / 2f;

        // ═══════════════════════════════════════════════
        //  EDITOR TOOLS
        // ═══════════════════════════════════════════════
        DrawSectionHeader("EDITOR TOOLS");

        toolsScrollPos = EditorGUILayout.BeginScrollView(toolsScrollPos, GUILayout.Height(halfHeight - 20f));

        DrawToolActiveAndGrid();
        EditorGUILayout.Space(6);
        DrawTilesSection();
        EditorGUILayout.Space(6);
        DrawConnectionsSection();

        EditorGUILayout.EndScrollView();

        // ── divider ──────────────────────────────────
        Rect divider = EditorGUILayout.GetControlRect(false, 2f);
        EditorGUI.DrawRect(divider, new Color(0.1f, 0.1f, 0.1f, 1f));
        EditorGUILayout.Space(4);

        bool hasSelectedConnection = TileEditor.SelectedConnection != null && TileEditor.SelectedConnectionTile != null;

        // ═══════════════════════════════════════════════
        //  SELECTED TILE / CONNECTION
        // ═══════════════════════════════════════════════
        DrawSectionHeader(hasSelectedConnection ? "SELECTED CONNECTION" : "SELECTED TILE");

        inspectorScrollPos = EditorGUILayout.BeginScrollView(inspectorScrollPos, GUILayout.Height(halfHeight - 20f));

        if (hasSelectedConnection)
        {
            DrawSelectedConnectionPanel();
        }
        else
        {
            Tile selectedTile = GetSingleSelectedTile();
            if (selectedTile == null)
            {
                EditorGUILayout.HelpBox("Select a tile or click a connection line in the scene.", MessageType.Info);
            }
            else
            {
                DrawSelectedTilePanel(selectedTile);
            }
        }

        EditorGUILayout.EndScrollView();
    }

    private void DrawSelectedTilePanel(Tile tile)
    {
        EditorGUILayout.LabelField(tile.gameObject.name, EditorStyles.boldLabel);
        EditorGUILayout.Space(4);

        SerializedObject tileSo = new SerializedObject(tile);
        tileSo.Update();
        SerializedProperty walkableProp = tileSo.FindProperty("_isWalkable");

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Walkable", GUILayout.Width(EditorGUIUtility.labelWidth));
        bool current = walkableProp.boolValue;
        Rect pillRect = EditorGUILayout.GetControlRect(false, 18f);
        pillRect.width = Mathf.Min(pillRect.width, 80f);
        EditorGUI.DrawRect(pillRect, current ? new Color(0.2f, 0.6f, 0.2f) : new Color(0.6f, 0.2f, 0.2f));
        GUI.Label(pillRect, current ? "Walkable" : "Blocked", new GUIStyle(EditorStyles.boldLabel)
        {
            alignment = TextAnchor.MiddleCenter,
            normal = { textColor = Color.white },
            fontSize = 10
        });
        bool toggled = EditorGUILayout.Toggle(current, GUILayout.Width(20f));
        if (toggled != current) walkableProp.boolValue = toggled;
        EditorGUILayout.EndHorizontal();

        if (tileSo.ApplyModifiedProperties())
        {
            tile.ValidateConnections();
            EditorUtility.SetDirty(tile);
        }

        EditorGUILayout.Space(4);
        DrawDecorationSection(tile);
        EditorGUILayout.Space(6);
        DrawMeshOverrideSection();
    }

    private void DrawSelectedConnectionPanel()
    {
        Tile tileA = TileEditor.SelectedConnectionTile;
        TileConnection connA = TileEditor.SelectedConnection;

        if (connA.connectedTile == null)
        {
            EditorGUILayout.HelpBox("Connection data lost — reselect.", MessageType.Warning);
            return;
        }

        Tile tileB = connA.connectedTile;

        // Find reverse connection on tileB (may not exist if unidirectional)
        TileConnection connB = tileB.tileConnections.Find(c => c.connectedTile == tileA);

        int indexA = tileA.tileConnections.IndexOf(connA);
        int indexB = connB != null ? tileB.tileConnections.IndexOf(connB) : -1;

        if (indexA < 0)
        {
            EditorGUILayout.HelpBox("Connection data lost — reselect.", MessageType.Warning);
            return;
        }

        // Header showing both tiles
        EditorGUILayout.LabelField($"{tileA.gameObject.name}  ↔  {tileB.gameObject.name}", EditorStyles.boldLabel);
        EditorGUILayout.Space(4);

        // Perspective toggles — read from connA, write to both
        EditorGUILayout.LabelField("Active Perspectives", EditorStyles.boldLabel);
        PerspectiveFlags currentFlags = connA.activePerspectives;

        EditorGUILayout.BeginHorizontal();
        foreach (CameraController.CameraState cam in System.Enum.GetValues(typeof(CameraController.CameraState)))
        {
            PerspectiveFlags flag = (PerspectiveFlags)(1 << (int)cam);
            bool isOn = (currentFlags & flag) != 0;
            GUI.backgroundColor = isOn ? Color.cyan : Color.white;
            string lbl = cam.ToString().Replace("North", "N").Replace("South", "S")
                                       .Replace("East", "E").Replace("West", "W");
            if (GUILayout.Button(lbl))
            {
                PerspectiveFlags newFlags = isOn
                    ? currentFlags & ~flag
                    : currentFlags | flag;

                // Apply to side A
                Undo.RecordObject(tileA, "Edit Connection Perspective");
                connA.activePerspectives = newFlags;
                tileA.ValidateConnections();
                EditorUtility.SetDirty(tileA);

                // Apply to side B if reverse exists
                if (connB != null)
                {
                    Undo.RecordObject(tileB, "Edit Connection Perspective");
                    connB.activePerspectives = newFlags;
                    tileB.ValidateConnections();
                    EditorUtility.SetDirty(tileB);
                }
            }
        }
        GUI.backgroundColor = Color.white;
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space(4);

        // Active now badge
        bool active = connA.IsActiveForPerspective(CameraController.ActivePerspective);
        bool valid = active && tileA.isWalkable && tileB.isWalkable;
        Rect badgeRect = EditorGUILayout.GetControlRect(false, 18f);
        badgeRect.width = Mathf.Min(badgeRect.width, 130f);
        EditorGUI.DrawRect(badgeRect, valid ? new Color(0.2f, 0.6f, 0.2f) : active ? new Color(0.7f, 0.5f, 0.1f) : new Color(0.5f, 0.5f, 0.5f));
        string badgeText = valid ? "Walkable now" : active ? "Active / blocked" : "Inactive";
        GUI.Label(badgeRect, badgeText, new GUIStyle(EditorStyles.boldLabel)
        {
            alignment = TextAnchor.MiddleCenter,
            normal = { textColor = Color.white },
            fontSize = 10
        });

        EditorGUILayout.Space(6);
        if (GUILayout.Button("Deselect"))
            TileEditor.ClearSelectedConnection();
    }

    private void DrawSectionHeader(string title)
    {
        Rect rect = EditorGUILayout.GetControlRect(false, 20f);
        EditorGUI.DrawRect(rect, new Color(0.18f, 0.18f, 0.18f, 1f));
        EditorGUI.LabelField(rect, title, new GUIStyle(EditorStyles.boldLabel)
        {
            alignment = TextAnchor.MiddleLeft,
            padding = new RectOffset(6, 0, 0, 0),
            normal = { textColor = new Color(0.75f, 0.75f, 0.75f) }
        });
        EditorGUILayout.Space(4);
    }

    private void DrawToolActiveAndGrid()
    {
        toolActive = EditorGUILayout.Toggle("Tool Active", toolActive);
        EditorGUILayout.Space(2);

        if (sceneGrids == null || sceneGrids.Length == 0)
        {
            EditorGUILayout.HelpBox("No Grid components found in scene.", MessageType.Warning);
            if (GUILayout.Button("Refresh Grids")) RefreshGrids();
        }
        else
        {
            EditorGUILayout.BeginHorizontal();
            selectedGridIndex = EditorGUILayout.Popup("Grid", selectedGridIndex, gridNames);
            if (GUILayout.Button("Refresh", GUILayout.Width(60))) RefreshGrids();
            EditorGUILayout.EndHorizontal();
        }
    }

    private void DrawTilesSection()
    {
        showTileSection = EditorGUILayout.BeginFoldoutHeaderGroup(showTileSection, "Tiles");
        if (showTileSection)
        {
            EditorGUI.indentLevel++;

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
                EditorGUILayout.Space(4);
                EditorGUILayout.BeginHorizontal();
                GUI.backgroundColor = placementMode == PlacementMode.Grid ? Color.cyan : Color.white;
                if (GUILayout.Button("Grid")) placementMode = PlacementMode.Grid;
                GUI.backgroundColor = placementMode == PlacementMode.Free ? Color.cyan : Color.white;
                if (GUILayout.Button("Free")) placementMode = PlacementMode.Free;
                GUI.backgroundColor = Color.white;
                EditorGUILayout.EndHorizontal();

                selectedTileType = (TileType)EditorGUILayout.EnumPopup("Tile Type", selectedTileType);

                if (placementMode == PlacementMode.Free)
                    freeDistance = EditorGUILayout.Slider("Distance", freeDistance, 2f, 50f);

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
    }

    private void DrawConnectionsSection()
    {
        showConnectionSection = EditorGUILayout.BeginFoldoutHeaderGroup(showConnectionSection, "Connections");
        if (showConnectionSection)
        {
            EditorGUI.indentLevel++;

            bool isConnectionMode = currentMode == ToolMode.Connect || currentMode == ToolMode.DeleteConnection;

            EditorGUILayout.BeginHorizontal();
            GUI.backgroundColor = currentMode == ToolMode.Connect ? Color.cyan : Color.white;
            if (GUILayout.Button("Create")) currentMode = ToolMode.Connect;
            GUI.backgroundColor = currentMode == ToolMode.DeleteConnection ? Color.red * 0.8f + Color.white * 0.2f : Color.white;
            if (GUILayout.Button("Delete")) currentMode = ToolMode.DeleteConnection;
            GUI.backgroundColor = Color.white;
            if (isConnectionMode && GUILayout.Button("Stop", GUILayout.Width(60)))
            {
                TileEditor.CancelConnection();
                currentMode = ToolMode.Select;
            }
            EditorGUILayout.EndHorizontal();

            if (currentMode == ToolMode.Connect)
            {
                TileEditor.ConnectionBidirectional = EditorGUILayout.Toggle("Bidirectional", TileEditor.ConnectionBidirectional);
                if (TileEditor.ConnectionFirstTile != null)
                {
                    EditorGUILayout.HelpBox($"First tile: {TileEditor.ConnectionFirstTile.gameObject.name}\nClick a second tile to complete.", MessageType.Info);
                    if (GUILayout.Button("Cancel Selection")) TileEditor.CancelConnection();
                }
                else
                {
                    EditorGUILayout.HelpBox("Click a tile to start a connection.\nRight-click or Escape to cancel.", MessageType.Info);
                }
            }
            else if (currentMode == ToolMode.DeleteConnection)
            {
                EditorGUILayout.HelpBox("Click on a connection line to delete it.", MessageType.Info);
            }

            EditorGUI.indentLevel--;
        }
        EditorGUILayout.EndFoldoutHeaderGroup();
    }

    private void DrawDecorationSection(Tile tile)
    {
        showDecorationSection = EditorGUILayout.BeginFoldoutHeaderGroup(showDecorationSection, "Decoration");
        if (showDecorationSection)
        {
            EditorGUI.indentLevel++;

            SerializedObject so = new SerializedObject(tile);
            SerializedProperty hasDecorationProp = so.FindProperty("hasDecoration");
            SerializedProperty decorationProp = so.FindProperty("decoration");

            so.Update();
            EditorGUILayout.PropertyField(hasDecorationProp, new GUIContent("Has Decoration"));

            using (new EditorGUI.DisabledScope(!hasDecorationProp.boolValue))
            {
                if (decorationPrefabs.Length == 0)
                {
                    EditorGUILayout.HelpBox("No prefabs found in Assets/Prefabs/Decorations.", MessageType.Warning);
                    if (GUILayout.Button("Refresh")) LoadDecorationPrefabs();
                }
                else
                {
                    GameObject currentDecoration = decorationProp.objectReferenceValue as GameObject;
                    int currentIndex = System.Array.IndexOf(decorationPrefabs, currentDecoration);
                    if (currentIndex < 0) currentIndex = selectedDecorationIndex;

                    EditorGUILayout.BeginHorizontal();
                    int newIndex = EditorGUILayout.Popup("Decoration", currentIndex, decorationNames);
                    if (GUILayout.Button("Refresh", GUILayout.Width(60))) LoadDecorationPrefabs();
                    EditorGUILayout.EndHorizontal();

                    if (newIndex != currentIndex || decorationProp.objectReferenceValue == null)
                    {
                        selectedDecorationIndex = newIndex;
                        decorationProp.objectReferenceValue = decorationPrefabs[newIndex];
                    }
                }
            }

            if (so.ApplyModifiedProperties())
            {
                ApplyDecorationToTile(tile);
                EditorUtility.SetDirty(tile);
            }

            EditorGUI.indentLevel--;
        }
        EditorGUILayout.EndFoldoutHeaderGroup();
    }

    private void DrawMeshOverrideSection()
    {
        showMeshOverrideSection = EditorGUILayout.BeginFoldoutHeaderGroup(showMeshOverrideSection, "Mesh Override");
        if (showMeshOverrideSection)
        {
            EditorGUI.indentLevel++;

            overrideMesh = (Mesh)EditorGUILayout.ObjectField("Mesh", overrideMesh, typeof(Mesh), false);
            EditorGUILayout.LabelField("Rotation", $"({meshRotation.x}, {meshRotation.y}, {meshRotation.z})");

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
            if (GUILayout.Button("Reset Rotation")) meshRotation = Vector3.zero;

            EditorGUILayout.Space(4);
            if (GUILayout.Button("Apply to Selected Tiles")) ApplyMeshOverrideToSelection();
            if (GUILayout.Button("Clear Mesh Override")) ClearMeshOverrideFromSelection();

            EditorGUI.indentLevel--;
        }
        EditorGUILayout.EndFoldoutHeaderGroup();
    }

    private const string DecorationChildName = "__Decoration__";

    private void ApplyDecorationToTile(Tile tile)
    {
        Transform existing = tile.transform.Find(DecorationChildName);
        if (existing != null)
            Undo.DestroyObjectImmediate(existing.gameObject);

        if (!tile.hasDecoration || tile.decoration == null) return;

        // Default to non-walkable when a decoration is first placed
        Undo.RecordObject(tile, "Spawn Decoration");
        tile.isWalkable = false;

        GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(tile.decoration, tile.transform);
        instance.name = DecorationChildName;
        instance.transform.localPosition = new Vector3(0f, 0.5f, 0f);
        Undo.RegisterCreatedObjectUndo(instance, "Spawn Decoration");
    }

    private Tile GetSingleSelectedTile()
    {
        if (Selection.gameObjects.Length != 1) return null;
        return Selection.gameObjects[0].GetComponent<Tile>();
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
