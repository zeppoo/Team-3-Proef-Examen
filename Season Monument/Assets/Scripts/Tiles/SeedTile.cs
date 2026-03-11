using UnityEngine;

public class SeedTile : Tile
{
    public override bool isWalkable => true;
    [SerializeField] private GameObject tree;
    [SerializeField] private GameObject targetLoc;
    [SerializeField] private GameObject connection;
    [SerializeField] private SeasonState connectionActiveSeason;
    private TreeTile treeTile;

    public override void ActivateEffect(SeasonState season)
    {
        Debug.Log(season);
        if (season == SeasonState.Spring)
        {
            GameObject treeObj = Instantiate(tree, new Vector3(transform.position.x, transform.position.y + 1f, transform.position.z), Quaternion.identity);
            treeTile = treeObj.GetComponent<TreeTile>();
            treeTile.SetTargetLoc(targetLoc);
            treeTile.SetConnection(connection);
            treeTile.SetConnectionSeason(connectionActiveSeason);

            StartCoroutine(RebuildNeighborsNextFrame());
            
        }
        base.ActivateEffect(season);
    }

    private System.Collections.IEnumerator RebuildNeighborsNextFrame()
    {
        yield return null; // Wait one frame for the tree's Start() to complete
        GridManager.instance.RebuildAllNeighbours();
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
}
