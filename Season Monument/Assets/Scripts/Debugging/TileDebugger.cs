using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class TileDebugger : MonoBehaviour
{
    [Header("Debug Settings")]
    public bool debugDraw = false;
    public float gizmoHeight = 1.0f;
    public float gizmoSize = 0.15f;

    private void OnDrawGizmos()
    {
        if (!debugDraw)
            return;

        Tile[] allTiles = FindObjectsByType<Tile>(FindObjectsSortMode.None);

        foreach (Tile tile in allTiles)
        {
            Gizmos.color = tile.isWalkable ? Color.white : Color.red;

            Vector3 position = tile.transform.position + Vector3.up * gizmoHeight;
            Gizmos.DrawSphere(position, gizmoSize);
        }
    }
}
