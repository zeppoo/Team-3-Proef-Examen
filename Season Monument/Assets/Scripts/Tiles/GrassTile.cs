using UnityEngine;

public class GrassTile : Tile
{
    public override void Start()
    {
        base.Start();
        materialInstance.color = Color.green;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
