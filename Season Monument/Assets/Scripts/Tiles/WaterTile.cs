using System.Collections.Generic;
using UnityEngine;

public class WaterTile : Tile
{
    private static readonly int iceTransID = Shader.PropertyToID("_iceTrans");
    [SerializeField] private SeasonState connectionActiveSeason = SeasonState.Winter;

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
        base.OnSeasonChanged(season);
        ValidateConnectionsBySeason(season);
    }

    public override void Start()
    {
        base.Start();
        materialInstance = GetComponent<Renderer>().material;
    }

    public override void ActivateEffect(SeasonState season)
    {
        if (season == SeasonState.Winter)
        {
            materialInstance.color = Color.white;
            isWalkable = true;
            materialInstance.SetFloat(iceTransID, 1f);
        }
        else if (season == SeasonState.Summer)
        {
            materialInstance.color = Color.blue;
            isWalkable = false;
            materialInstance.SetFloat(iceTransID, 0f);
        }
        
        ValidateConnectionsBySeason(season);
    }

    private void ValidateConnectionsBySeason(SeasonState currentSeason)
    {
        foreach (TileConnection connection in tileConnections)
        {
            if (connection.connectedTile == null)
            {
                connection.valid = false;
                continue;
            }

            bool seasonMatches = (currentSeason == connectionActiveSeason);
            connection.valid = isWalkable && connection.connectedTile.isWalkable && seasonMatches;
        }
        
        ApplySeasonalConnections();
    }

    private void ApplySeasonalConnections()
    {
        foreach (TileConnection connection in tileConnections)
        {
            if (connection.connectedTile == null) continue;

            if (connection.valid)
            {
                if (!neighbours.Contains(connection.connectedTile.gameObject))
                    neighbours.Add(connection.connectedTile.gameObject);

                if (connection.bidirectional && !connection.connectedTile.neighbours.Contains(gameObject))
                    connection.connectedTile.neighbours.Add(gameObject);
            }
            else
            {
                neighbours.Remove(connection.connectedTile.gameObject);
                
                if (connection.bidirectional)
                    connection.connectedTile.neighbours.Remove(gameObject);
            }
        }
    }
}
