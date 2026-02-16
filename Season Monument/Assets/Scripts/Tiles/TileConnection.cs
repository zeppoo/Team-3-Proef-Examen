using System;
using UnityEngine;
using UnityEngine.Events;

[Serializable]
public class TileConnection
{
    public Tile connectedTile;
    public bool bidirectional = true;

    [Header("Events")]
    public UnityEvent OnConnectionUsed;
}
