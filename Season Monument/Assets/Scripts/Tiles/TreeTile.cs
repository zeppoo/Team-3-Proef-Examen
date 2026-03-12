using UnityEngine;
using System.Collections;

public class TreeTile : Tile
{
    private bool hasBeenActivated = false;
    internal GameObject targetLocation;
    [SerializeField] public GameObject connectingBlock;

    public void SetTargetLoc(GameObject targetLoc)
    {
        targetLocation = targetLoc;
    }

    public void SetConnection(GameObject connection)
    {
        Tile tileComponent = connection.GetComponent<Tile>();
        tileConnections.Add(new TileConnection { connectedTile = tileComponent });
    }

    public void SetConnectionSeason(SeasonState season) { }

    public override void Start()
    {
        base.Start();
    }

    public override void OnEnable()
    {
        base.OnEnable();
    }

    public override void OnDisable()
    {
        base.OnDisable();
    }

    public override void OnSeasonChanged(SeasonState season)
    {
        base.OnSeasonChanged(season);
    }

    public override void ActivateEffect(SeasonState season)
    {
        if (season == SeasonState.Autumn && !hasBeenActivated)
        {
            isWalkable = true;

            Vector3 targetPos = targetLocation.transform.position;
            float gridSize = 0.5f;
            transform.position = new Vector3(
                Mathf.Round(targetPos.x / gridSize) * gridSize,
                targetPos.y,
                Mathf.Round(targetPos.z / gridSize) * gridSize
            );

            hasBeenActivated = true;

            if (targetLocation != null)
                targetLocation.SetActive(false);

            StartCoroutine(RebuildNeighborsAfterMove());
        }

        if (season == SeasonState.Summer)
            Destroy(gameObject);

        base.ActivateEffect(season);
    }

    private IEnumerator RebuildNeighborsAfterMove()
    {
        yield return null;
        GridManager.instance.RegisterTile(this);
        GridManager.instance.RebuildAllNeighbours();
        yield return null;
        ValidateConnections();
        Debug.Log($"Tree rebuilt with {neighbours.Count} neighbours.");
    }
}
