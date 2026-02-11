using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{

    private GameObject selectedTile;
    private Vector3 targetPosition;
    private Tile tile;
    private void Start()
    {
        
    }

    private void Update()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (!Physics.Raycast(ray, out RaycastHit hit)) return;

        if(hit.collider.CompareTag("Tile"))
        {
            selectedTile = hit.collider.gameObject;
            float TopY = hit.collider.bounds.max.y + 1;
            Vector3 currentposition = selectedTile.transform.position;
            tile.parent = selectedTile.GetComponent<Tile>();
            targetPosition = new Vector3(currentposition.x, TopY, currentposition.z);
        }
    }

   public void pointAndClick(InputAction.CallbackContext context) 
    {
       
    }
}
