using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    private Tile currentTile;
    private Tile selectedTile;
    private PathFinder pathFinder;

    private List<Tile> currentPath = new List<Tile>();
    private int currentIndex = 0;

    private const float TRAVEL_TIME = 0.5f;
    private float travelProgress = 0f;
    private Vector3 moveStart;
    private Vector3 moveTarget;
    private bool isMoving = false;

    private bool isConnectionMove = false;

    private void Start()
    {
       pathFinder = GetComponent<PathFinder>();
        currentTile = pathFinder.startTile;
    }
    public void pointAndClick(InputAction.CallbackContext context)
    {
        if (!context.performed) return;
        pointAndClick();
    }

    public void pointAndClick()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (!Physics.Raycast(ray, out RaycastHit hit)) return;
        if (hit.collider.CompareTag("Tile"))
        {
            selectedTile = hit.collider.GetComponent<Tile>();
            pathFinder.endTile = selectedTile;
            pathFinder.FindPath();
        }
    }

    private void Update()
    {
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            pointAndClick();
        }

        MovePlayer();
    }

    private void MovePlayer()
    {
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
            travelProgress += Time.deltaTime / TRAVEL_TIME;
            transform.position = Vector3.Lerp(moveStart, moveTarget, travelProgress);
        }

        if (travelProgress >= 1f)
        {
            transform.position = moveTarget;
            isMoving = false;
            currentIndex++;

            if (currentIndex >= currentPath.Count)
            {
                pathFinder.startTile = pathFinder.endTile;
                currentPath = null;
                currentIndex = 0;
            }
        }
    }

    public void setPath(List<Tile> path)
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
