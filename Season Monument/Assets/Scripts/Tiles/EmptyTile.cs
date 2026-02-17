using UnityEngine;

public class EmptyTile : Tile
{
    public override bool isWalkable => false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        base.Start();
        materialInstance.color = Color.green;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
