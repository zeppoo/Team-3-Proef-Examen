using UnityEngine;

public class SeedTile : Tile
{
    public override bool isWalkable { get => base.isWalkable; protected set => base.isWalkable = value; }
    [SerializeField] private GameObject tree;

    public override void ActivateEffect(SeasonState season)
    {
        Debug.Log(season);
        if (season == SeasonState.Spring)
        {
            Instantiate(tree, new Vector3(transform.position.x, transform.position.y + 1f, transform.position.z), Quaternion.identity, transform);
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

    public override void Start()
    {
        isWalkable = false;
        base.Start();
    }

    public override void OnSeasonChanged(SeasonState season)
    {   
          
        base.OnSeasonChanged(season);
    }
}
