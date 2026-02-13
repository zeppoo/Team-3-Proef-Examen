using UnityEngine;
using System;
using System.Collections;


public class PathTile : Tile
{
    public override bool isWalkable => true;

    public override void Start()
    {
        base.Start();
        materialInstance.color = Color.yellow;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
