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
           
            materialInstance.color = Color.white;
            isWalkable = true;
            materialInstance.SetFloat(iceTransID, 1f);
        }

        if (season == SeasonState.Summer)
        {

            materialInstance.color = Color.blue;
            isWalkable = false;
            materialInstance.SetFloat(iceTransID, 0f);
        }
    }
    
}
