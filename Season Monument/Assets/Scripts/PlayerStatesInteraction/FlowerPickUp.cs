using UnityEngine;

public class FlowerPickUp : MonoBehaviour
{
    [SerializeField] private GameObject flowerModel;
    [SerializeField] private GameObject currentModel;

    [SerializeField] private GameObject flower;
    [SerializeField] internal Tile newCurrentTile;

    private PlayerMovement playerMovement;
    internal bool canPickUp = false;


    private void Start()
    {
        
        currentModel = gameObject;
    }

    public void PickUpFlower()
    {
        Debug.Log("Attempting to pick up flower...");
        if (canPickUp) 
        {
            WaitForSeconds wait = new WaitForSeconds(0.5f);
            Instantiate(flowerModel, transform.position, Quaternion.identity);
            
            PathFinder playerMovement = flowerModel.GetComponent<PathFinder>();
            
            playerMovement.startTile = newCurrentTile;
            flower.SetActive(false);
            currentModel.SetActive(false);
            

        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Flower"))
        {
            PickUpFlower();

            Debug.Log("Can Pick Up Flower");
        }
    }
}



