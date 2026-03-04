using UnityEngine;

public class WaterTile : Tile
{
    private static readonly int iceTransID = Shader.PropertyToID("_iceTrans");
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
 
    }

    public override void OnSeasonChanged(SeasonState season) 
    {
        isWalkable = false;
        materialInstance.color = Color.blue;
        materialInstance.SetFloat(iceTransID, 0f);
    }

    public override void Start()
    {
        base.Start();
        materialInstance = GetComponent<Renderer>().material;
    }

    void Update()
    {
        
    }

    public override void ActivateEffect(SeasonState season)
    {
       
        if (season == SeasonState.Winter)
        {
            Debug.Log("Water tile effect activated for Winter");
            materialInstance.color = Color.white;
            isWalkable = true;
            materialInstance.SetFloat(iceTransID, 1f);
        }
    }
    
}
