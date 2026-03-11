using UnityEngine;
using UnityEngine.Rendering;
using System.Collections.Generic;

public class TreeTile : Tile
{
private bool hasBeenActivated = false;
    internal GameObject targetLocation;
    [SerializeField] public GameObject connectingBlock;
    private SeasonState connectionActiveSeason;

    public void SetTargetLoc(GameObject targetLoc)
    {
        targetLocation = targetLoc;
    }

    public void SetConnectionSeason(SeasonState season)
    {
        connectionActiveSeason = season;
    }

    public override void ActivateEffect(SeasonState season)
    {
        if(season == SeasonState.Autumn && hasBeenActivated == false)
        { 
            isWalkable = true;
            Debug.Log("Tree tile is now walkable in Autumn.");
            
            
            Vector3 targetPos = targetLocation.transform.position;
            
         
            float gridSize = 0.5f;
            Vector3 snappedPos = new Vector3(
                Mathf.Round(targetPos.x / gridSize) * gridSize,
                targetPos.y, 
                Mathf.Round(targetPos.z / gridSize) * gridSize
            );
            
            transform.position = snappedPos;
            hasBeenActivated = true;
            
            
            
            if (targetLocation != null)
            {
                targetLocation.SetActive(false);
            }
            
            
            StartCoroutine(RebuildNeighborsAfterMove());
        }
        if(season == SeasonState.Summer)
        {
            Destroy(gameObject);
        }
        base.ActivateEffect(season);
        
    }

    private System.Collections.IEnumerator RebuildNeighborsAfterMove()
    {
        yield return null;
        
    
        
        GridManager.instance.RegisterTile(this);
        GridManager.instance.RebuildAllNeighbours();
        
        yield return null;
        
        ValidateConnections();
        
        foreach (TileConnection connection in tileConnections)
        {
            if (connection.connectedTile == null || !connection.valid) continue;

            if (!neighbours.Contains(connection.connectedTile.gameObject))
                neighbours.Add(connection.connectedTile.gameObject);

            if (connection.bidirectional && !connection.connectedTile.neighbours.Contains(gameObject))
                connection.connectedTile.neighbours.Add(gameObject);
        }
        
        Debug.Log($"Tree now has {neighbours.Count} neighbors after rebuild (including connections)");
        
        foreach (GameObject neighbor in neighbours)
        {
            Tile neighborTile = neighbor.GetComponent<Tile>();
            if (neighborTile != null)
            {
                Debug.Log($"  - Neighbor: {neighbor.name} at {neighbor.transform.position}, Walkable: {neighborTile.isWalkable}");
            }
        }
        
        if (neighbours.Count == 0)
        {
            Debug.LogWarning("Tree has NO neighbors! Checking nearby tiles...");
            Tile[] allTiles = FindObjectsByType<Tile>(FindObjectsSortMode.None);
            foreach (Tile tile in allTiles)
            {
                if (tile == this) continue;
                
                float distance = Vector3.Distance(transform.position, tile.transform.position);
                if (distance < 5f) 
                {
                    Vector3 offset = tile.transform.position - transform.position;
                    Vector3 horizontalOffset = new Vector3(offset.x, 0, offset.z);
                    float horizontalDistance = horizontalOffset.magnitude;
                    
                    int axisCount = 0;
                    if (Mathf.Abs(offset.x) > 0.01f) axisCount++;
                    if (Mathf.Abs(offset.z) > 0.01f) axisCount++;
                    
                    Debug.LogWarning($"  Nearby: {tile.gameObject.name} at {tile.transform.position}, 3DDist: {distance:F2}, HorzDist: {horizontalDistance:F2}, HeightDiff: {Mathf.Abs(offset.y):F2}, AxisCount: {axisCount}, Walkable: {tile.isWalkable}");
                }
            }
        }
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
        UpdateSeasonalConnections(season);
    }

    public override void Start()
    {
        
        base.Start();
    }

    public void SetConnection(GameObject connection)
    {
       Tile tileComponent = connection.GetComponent<Tile>();
       tileConnections.Add(new TileConnection { connectedTile = tileComponent });
    }

    private void UpdateSeasonalConnections(SeasonState currentSeason)
    {
        ValidateConnectionsBySeason(currentSeason);
        
        foreach (TileConnection connection in tileConnections)
        {
            if (connection.connectedTile == null) continue;
            
            UpdateNeighborList(connection, connection.valid);
        }
    }

    private void UpdateNeighborList(TileConnection connection, bool shouldAdd)
    {
        GameObject connectedGameObject = connection.connectedTile.gameObject;
        
        UpdateSingleNeighbor(neighbours, connectedGameObject, shouldAdd);
        
        if (connection.bidirectional)
            UpdateSingleNeighbor(connection.connectedTile.neighbours, gameObject, shouldAdd);
    }

    private void UpdateSingleNeighbor(List<GameObject> neighborList, GameObject neighbor, bool shouldAdd)
    {
        bool contains = neighborList.Contains(neighbor);
        
        if (shouldAdd && !contains)
            neighborList.Add(neighbor);
        else if (!shouldAdd && contains)
            neighborList.Remove(neighbor);
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
    }
}
