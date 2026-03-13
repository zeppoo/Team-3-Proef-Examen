using System.Collections.Generic;
using UnityEngine;
using static WorldStateSwitch;

public class PlayerMovement : MonoBehaviour
{
    public Tile currentTile;

    private PathFinder pathFinder;
    private List<Tile> currentPath;
    private int currentIndex;

    private const float travelTime = 0.5f;
    private float travelProgress;
    private Vector3 moveStart;
    private Vector3 moveTarget;
    private bool isMoving;

    private WorldStateSwitch worldStateSwitch;
    private SeasonStateManager seasonStateManager;
    private AnimationController animationController;

    private void Start()
    {
        pathFinder = GetComponent<PathFinder>();
        currentTile = pathFinder.startTile;
        worldStateSwitch = FindFirstObjectByType<WorldStateSwitch>();
        seasonStateManager = FindAnyObjectByType<SeasonStateManager>();
        animationController = GetComponent<AnimationController>();
    }

    private void Update()
    {
        if (Input.touchCount > 0 && Input.GetTouch(0).phase == UnityEngine.TouchPhase.Began)
            HandleTap(Input.GetTouch(0).position);
        else if (Input.GetMouseButtonDown(0))
            HandleTap(Input.mousePosition);

        MovePlayer();

        if(isMoving)
            animationController.SetWalking(true);
        else
            animationController.SetWalking(false);
    }

    private void HandleTap(Vector2 screenPosition)
    {
        // Block input while moving
        if (currentPath != null && currentPath.Count > 0) return;

        Ray ray = Camera.main.ScreenPointToRay(screenPosition);
        if (!Physics.Raycast(ray, out RaycastHit hit)) return;
        if (!hit.collider.CompareTag("Tile")) return;

        Tile tappedTile = hit.collider.GetComponent<Tile>();
        if (tappedTile == null) return;

        if (worldStateSwitch != null && worldStateSwitch.CurrentState == WorldState.Gameplay)
        {
            tappedTile.ActivateEffect(seasonStateManager.currentSeason);
            return;
        }

        if (worldStateSwitch != null && worldStateSwitch.CurrentState == WorldState.View)
        {
            pathFinder.endTile = tappedTile;
            pathFinder.FindPath();
        }
    }

    private void MovePlayer()
    {
        if (currentPath == null || currentPath.Count == 0) return;

        if (!isMoving)
        {
            moveStart = transform.position;
            moveTarget = currentPath[currentIndex].transform.position;
            travelProgress = 0f;
            isMoving = true;
        }

        // Teleport instantly over tile connections, lerp over normal steps
        Tile previousTile = currentIndex > 0 ? currentPath[currentIndex - 1] : currentTile;
        bool isConnectionStep = previousTile != null && previousTile.GetConnectionTo(currentPath[currentIndex]) != null;

        if (isConnectionStep)
        {
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
            currentTile = currentPath[currentIndex];
            currentIndex++;

            if (currentIndex >= currentPath.Count)
            {
                pathFinder.startTile = currentTile;
                currentPath = null;
                currentIndex = 0;
                isMoving = false;
            }
            else
            {
                isMoving = false;
            }
        }
    }

    public void SetPath(List<Tile> path)
    {
        if (path == null || path.Count == 0) return;

        // Strip the start tile if it's the first step
        if (path[0] == pathFinder.startTile && path.Count > 1)
            path.RemoveAt(0);

        currentPath = path;
        currentIndex = 0;
        isMoving = false;
    }
}
