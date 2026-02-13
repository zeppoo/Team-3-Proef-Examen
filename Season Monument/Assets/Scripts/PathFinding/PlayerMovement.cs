using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    public Tile currentTile; 
    public Tile selectedTile;
    private PathFinder pathFinder;
    private Tile tile;

    private List<Tile> currentPath = new List<Tile>();
    private int currentIndex = 0;
    private int targetIndex = 0;
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

    private void Update()
    {
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            pointAndClick();
        }

        bool flowControl = MovePlayer();
        if (!flowControl)
        {
            return;
        }
    }

    private bool MovePlayer()
    {
        if (currentPath == null)
            return false;

        if (currentPath.Count == 0)
            return false;

        

        Vector3 position = currentPath[currentIndex].transform.position;

        BoxCollider tileCollider = currentPath[currentIndex].GetComponent<BoxCollider>();
        float topY = tileCollider != null ? tileCollider.bounds.max.y : position.y;
        Vector3 target = new Vector3(position.x, topY, position.z);
        transform.position = Vector3.MoveTowards(transform.position, target, 5f * Time.deltaTime);

        if (Vector3.Distance(transform.position, target) < 0.1f)
        {
            currentIndex++;
            if (currentIndex >= currentPath.Count)
            {
                pathFinder.startTile = pathFinder.endTile;
                currentPath = null;
                currentIndex = 0;
                return false;
            }

        }

        return true;
    }

    public void setPath(List<Tile> path)
    {
        if(path != null && path.Count > 0)
        {
            currentPath = new List<Tile>(path);
            currentIndex = 0;
        }

    }
}
