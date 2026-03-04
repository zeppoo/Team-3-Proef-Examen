using UnityEngine;

public class TreeTile : Tile
{
    public override bool isWalkable { get => base.isWalkable; protected set => base.isWalkable = value; }
    private bool hasBeenActivated = false;

    public override void ActivateEffect(SeasonState season)
    {
        if(season == SeasonState.Autumn && hasBeenActivated == false)
        { 
            isWalkable = true;
            Debug.Log("Tree tile is now walkable in Autumn.");
            transform.position = new Vector3(transform.position.x + 1f, transform.position.y - 1f, transform.position.z);
            hasBeenActivated = true;
        }
        base.ActivateEffect(season);
    }

    public override void OnDisable()
    {
        base.OnDisable();
    }

    public override void OnEnable()
    {
        base.OnEnable();
    }

    public override void OnSeasonChanged(SeasonState season)
    {
        base.OnSeasonChanged(season);
    }

    public override void Start()
    {
        base.Start();
    }
}
