using UnityEngine;

public class GrassTile : Tile
{
    public override bool isWalkable => true;

    public override void Start()
    {
        base.Start();
    }

    public override void OnSeasonChanged(SeasonState season)
    {
        base.OnSeasonChanged(season);
    }
}
