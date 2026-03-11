using UnityEngine;

public class TreeTile : Tile
{
private bool hasBeenActivated = false;
    internal GameObject targetLocation;


    public void SetTargetLoc(GameObject targetLoc)
    {
        targetLocation = targetLoc;
    }
    public override void ActivateEffect(SeasonState season)
    {
        if(season == SeasonState.Autumn && hasBeenActivated == false)
        { 
            isWalkable = true;
            Debug.Log("Tree tile is now walkable in Autumn.");
            transform.position = targetLocation.transform.position;
            hasBeenActivated = true;
        }
        if(season == SeasonState.Summer)
        {
            Destroy(gameObject);
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
