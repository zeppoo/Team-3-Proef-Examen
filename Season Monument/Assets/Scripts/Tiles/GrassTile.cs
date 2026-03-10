using System.Collections.Generic;
using UnityEngine;

public class GrassTile : Tile
{
    public override bool isWalkable => true;
    [SerializeField] private SeasonState connectionActiveSeason = SeasonState.Spring;

    public override void Start()
    {
        base.Start();
        materialInstance.color = Color.green;
    }

    public override void OnSeasonChanged(SeasonState season)
    {
        base.OnSeasonChanged(season);
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
