using System.Security.Cryptography.X509Certificates;
using UnityEngine;

public class WaterTile : Tile
{

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
    }

    public override void OnSeasonChanged(SeasonState season) 
    {
        isWalkable = false;
        materialInstance.color = Color.blue;
    }

    public override void Start()
    {
        base.Start();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
