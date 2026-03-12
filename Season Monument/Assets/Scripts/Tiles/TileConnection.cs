using System;
using UnityEngine;
using UnityEngine.Events;

[Serializable]
public class TileConnection
{
    public Tile connectedTile;

    [Tooltip("Which camera perspectives allow this connection. Use All to always allow it.")]
    public PerspectiveFlags activePerspectives = PerspectiveFlags.All;

    [HideInInspector] public bool valid = false;

    public UnityEvent OnConnectionUsed;

    public bool IsActiveForPerspective(CameraController.CameraState perspective)
    {
        PerspectiveFlags flag = (PerspectiveFlags)(1 << (int)perspective);
        return (activePerspectives & flag) != 0;
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
