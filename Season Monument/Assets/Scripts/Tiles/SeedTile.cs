using UnityEngine;

public class SeedTile : Tile
{
    public override bool isWalkable => true;
    [SerializeField] private GameObject tree;
    [SerializeField] private GameObject targetLoc;
    [SerializeField] private GameObject connection;
    [SerializeField] private SeasonState connectionActiveSeason;
    [SerializeField] private GameObject treeDecorationPrefab;
    private TreeTile treeTile;

    public override void ActivateEffect(SeasonState season)
    {
        Debug.Log(season);
        if (season == SeasonState.Spring)
        {
            if (treeDecorationPrefab != null && decoration == null)
            {
                decoration = Instantiate(treeDecorationPrefab, transform.position, Quaternion.identity, transform);
                hasDecoration = true;
            }

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
