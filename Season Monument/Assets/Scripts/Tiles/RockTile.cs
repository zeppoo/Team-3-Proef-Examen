using UnityEngine;

public class RockTile : Tile
{
    public override void Start()
    {
        base.Start();
        materialInstance.color = Color.gray;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
