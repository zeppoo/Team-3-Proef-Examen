using UnityEngine;


public class PathTile : Tile
{
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
