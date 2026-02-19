using UnityEngine;

public class WaterTile : Tile
{
    private static readonly int IceTransID = Shader.PropertyToID("_iceTrans");
    public override void OnEnable() 
    { 
        base.OnEnable(); 
        SeasonEvents.OnWinterStarted += OnWinterStarted;
    } 
    public override void OnDisable() 
    { 
        base.OnDisable(); 
        SeasonEvents.OnWinterStarted -= OnWinterStarted;
    }

    public void OnWinterStarted() 
    { 
        isWalkable = true;
        materialInstance.color = Color.white;
        FreezeWater();
    }

    public override void OnSeasonChanged(SeasonState season) 
    {
        isWalkable = false;
        materialInstance.color = Color.blue;
        materialInstance.SetFloat(IceTransID, 0f);
    }

    public override void Start()
    {
        base.Start();
        materialInstance = GetComponent<Renderer>().material;
    }

    void Update()
    {
        
    }

    private void FreezeWater()
    {
        Debug.Log("Freezing water tile");
        materialInstance.SetFloat(IceTransID, 1f);
    }
}
