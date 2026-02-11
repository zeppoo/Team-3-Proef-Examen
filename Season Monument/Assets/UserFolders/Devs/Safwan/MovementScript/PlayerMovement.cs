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

    private List<Tile> currentpath = new List<Tile>();
    private int currentIndex = 0;
    private void Start()
    {
       pathFinder = GetComponent<PathFinder>();
    }
    public void pointAndClick(InputAction.CallbackContext context) 
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (!Physics.Raycast(ray, out RaycastHit hit)) return;

        if (hit.collider.CompareTag("Tile"))
        {
            selectedTile = hit.collider.GetComponent<Tile>();
            float TopY = hit.collider.bounds.max.y + 1;
            Vector3 currentposition = selectedTile.transform.position;
            tile = selectedTile.GetComponent<Tile>();
            pathFinder.startTile = currentTile;
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
        if (currentpath == null)
            return;

        if (currentpath.Count == 0)
            return;

        if (currentIndex >= currentpath.Count)
            return;

        Vector3 position = currentpath[currentIndex].transform.position;

        BoxCollider tileCollider = currentpath[currentIndex].GetComponent<BoxCollider>();
        float topY = tileCollider != null ? tileCollider.bounds.max.y : position.y;
        Vector3 target = new Vector3(position.x, topY, position.z);
        transform.position = Vector3.MoveTowards(transform.position, target, 5f * Time.deltaTime);

        if (Vector3.Distance(transform.position, target) < 0.1f)
        {
            currentIndex++;
            if (currentpath[currentIndex] == null)
            {
                currentpath = null;
                currentIndex = 0;
                return;
            }

        }
    }

    public void setPath(List<Tile> path)
    {
        if(path != null && path.Count > 0)
        {
            currentpath = new List<Tile>(path);
            currentIndex = 0;
        }
        
    }
}
