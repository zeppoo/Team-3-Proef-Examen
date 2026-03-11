using UnityEngine;

public class SeedTile : Tile
{
[SerializeField] private GameObject tree;
    [SerializeField] private GameObject targetLoc;
    private TreeTile treeTile;

    public override void ActivateEffect(SeasonState season)
    {
        Debug.Log(season);
        if (season == SeasonState.Spring)
        {
            GameObject treeObj = Instantiate(tree, new Vector3(transform.position.x, transform.position.y + 1f, transform.position.z), Quaternion.identity, transform);
            treeTile = treeObj.GetComponent<TreeTile>();
                treeTile.SetTargetLoc(targetLoc);
               

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
        
        base.Start();
    }

    public override void OnSeasonChanged(SeasonState season)
    {   
          
        base.OnSeasonChanged(season);
    }

    private void Update()
    {
        isWalkable = true;
    }
}
