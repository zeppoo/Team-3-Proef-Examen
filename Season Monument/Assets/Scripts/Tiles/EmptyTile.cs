using UnityEngine;

[ExecuteAlways]
public class EmptyTile : Tile
{
    public override bool isWalkable
    {
        get => _isWalkable;
        set => _isWalkable = value;
    }

    public override void Start()
    {
        base.Start();
        _isWalkable = false;
    }

    void LateUpdate()
    {
        if (Application.isPlaying)
            UpdateWalkable();
    }

    private void OnValidate()
    {
        UpdateWalkableEditor();
    }

    private void UpdateWalkable()
    {
        foreach (Tile neighbour in neighbours)
        {
            if (neighbour != null && neighbour.isWalkable)
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

        float tileSize = col.bounds.size.x;
        Tile[] allTiles = FindObjectsByType<Tile>(FindObjectsSortMode.None);

        foreach (Tile tile in allTiles)
        {
            if (tile == this) continue;

            Vector3 offset = tile.transform.position - transform.position;
            float dx = Mathf.Abs(offset.x);
            float dy = Mathf.Abs(offset.y);
            float dz = Mathf.Abs(offset.z);

            bool axisAligned = (dx < 0.1f) != (dz < 0.1f);
            if (axisAligned && dy <= 2f && Mathf.Max(dx, dz) <= tileSize + 0.1f && tile.isWalkable)
            {
                _isWalkable = true;
                return;
            }
        }

        _isWalkable = false;
    }
}
