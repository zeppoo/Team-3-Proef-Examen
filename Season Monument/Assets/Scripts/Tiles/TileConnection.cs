using System;
using UnityEngine;
using UnityEngine.Events;

[Serializable]
public class TileConnection
{
    public Tile connectedTile;
    public bool bidirectional = true;
    [HideInInspector] public bool valid = false;

    [Header("Perspective")]
    [Tooltip("The camera perspective from which this connection is visible and walkable. Set to None to allow all perspectives.")]
    public PerspectiveFlags activePerspectives = PerspectiveFlags.All;

    [Header("Waypoint")]
    [Tooltip("Optional waypoint the player passes through when using this connection, to avoid clipping through geometry.")]
    public Transform waypoint;

    [Header("Events")]
    public UnityEvent OnConnectionUsed;

    public bool IsActiveForPerspective(CameraController.CameraState perspective)
    {
        return (activePerspectives & (PerspectiveFlags)(1 << (int)perspective)) != 0;
    }
}

[Flags]
public enum PerspectiveFlags
{
    None      = 0,
    NorthEast = 1 << 0,
    SouthEast = 1 << 1,
    SouthWest = 1 << 2,
    NorthWest = 1 << 3,
    All       = NorthEast | SouthEast | SouthWest | NorthWest
}
