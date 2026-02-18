using UnityEngine;

public class EmptyTile : Tile
{
    public override bool isWalkable => true;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        base.Start();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
