using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using static WorldStateSwitch;

public class PlayerMovement : MonoBehaviour
{
    public Tile currentTile;
    public Tile selectedTile;
    private PathFinder pathFinder;
    private Tile tile;

    private List<Tile> currentPath = new List<Tile>();
    private int currentIndex = 0;

    private const float travelTime = 0.5f;
    private float travelProgress = 0f;
    private Vector3 moveStart;
    private Vector3 moveTarget;
    private bool isMoving = false;

    private bool isConnectionMove = false;
    private WorldStateSwitch worldStateSwitch;
    private SeasonState season;
    private SeasonStateManager seasonStateManager;

    private void Start()
    {
       pathFinder = GetComponent<PathFinder>();
        currentTile = pathFinder.startTile;
        worldStateSwitch = FindObjectOfType<WorldStateSwitch>();
        seasonStateManager = FindAnyObjectByType<SeasonStateManager>();

    }

    private void Update()
    {
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            if (touch.phase == UnityEngine.TouchPhase.Began)
            {
                HandleTap(touch.position);
            }
        }

       
        if (Input.GetMouseButtonDown(0))
        {
            HandleTap(Input.mousePosition);
        }

        MovePlayer();
    }


    private void HandleTap(Vector2 screenPosition)
    {
        if (currentPath != null && currentPath.Count > 0)
            return;

        Ray ray = Camera.main.ScreenPointToRay(screenPosition);

        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            if (hit.collider.CompareTag("Tile"))
            {
                selectedTile = hit.collider.GetComponent<Tile>();
                if (worldStateSwitch.CurrentState == WorldState.Gameplay)
                {
                    ActivateSeasonEffect(selectedTile);
                    return;
                }
                else if (worldStateSwitch.CurrentState == WorldState.View)
                {
                    float topY = hit.collider.bounds.max.y + 1;
                    Vector3 currentposition = selectedTile.transform.position;

                    tile = selectedTile.GetComponent<Tile>();

                    pathFinder.endTile = selectedTile;
                    pathFinder.FindPath();

                    while (tile.parent != null)
                    {
                        currentposition = tile.parent.transform.position;
                        tile = tile.parent;
                    }


                }
            }
                
        }
    }

    private void ActivateSeasonEffect(Tile tile)
    {
        season = seasonStateManager.currentSeason;
        tile.ActivateEffect(season);
    }
    private void MovePlayer()
    {
      //  Debug.Log("Current Path: " + (currentPath != null ? currentPath.Count.ToString() : "null") + ", Current Index: " + currentIndex);
        if (currentPath == null || currentPath.Count == 0)
            return;

        if (!isMoving)
        {
            moveStart = transform.position;
            Vector3 tilePos = currentPath[currentIndex].transform.position;
            BoxCollider tileCollider = currentPath[currentIndex].GetComponent<BoxCollider>();
            float topY = tileCollider != null ? tileCollider.bounds.max.y : tilePos.y;
            moveTarget = new Vector3(tilePos.x, topY, tilePos.z);
            travelProgress = 0f;
            isMoving = true;

            // Check if this step uses a tile connection
            Tile previousTile = currentIndex > 0 ? currentPath[currentIndex - 1] : currentTile;
            isConnectionMove = previousTile != null && previousTile.GetConnectionTo(currentPath[currentIndex]) != null;
        }

        if (isConnectionMove)
        {
            // Instant teleport over connections
            transform.position = moveTarget;
            travelProgress = 1f;
        }
        else
        {
            travelProgress += Time.deltaTime / travelTime;
            transform.position = Vector3.Lerp(moveStart, moveTarget, travelProgress);
        }

        if (travelProgress >= 1f)
        {
            transform.position = moveTarget;

            // Update the current tile
            currentTile = currentPath[currentIndex];

            isMoving = false;
            currentIndex++;

            if (currentIndex >= currentPath.Count)
            {
                pathFinder.startTile = currentTile;
                currentPath = null;
                currentIndex = 0;
            }
        }
    }

    public void SetPath(List<Tile> path)
    {
        if(path != null && path.Count > 0)
        {
            if (path[0] == pathFinder.startTile && path.Count > 1)
                path.RemoveAt(0);

            currentPath = new List<Tile>(path);
            currentIndex = 0;
            isMoving = false;
        }

    }
}
