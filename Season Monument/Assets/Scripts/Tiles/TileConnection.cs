using System;
using UnityEngine;
using UnityEngine.Events;

[Serializable]
public class TileConnection
{
    public Tile connectedTile;
    public bool bidirectional = true;
    [HideInInspector] public bool valid = true;

    [Header("Waypoint")]
    [Tooltip("Optional waypoint the player passes through when using this connection, to avoid clipping through geometry.")]
    public Transform waypoint;

    [Header("Events")]
    public UnityEvent OnConnectionUsed;
}
