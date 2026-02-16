using UnityEngine;

public class StartRain : MonoBehaviour
{
    [SerializeField] internal RainCollection rainCollection;


    private void Start()
    {
        rainCollection = FindObjectOfType<RainCollection>();
    }
    public void StartRaining()
    {
      rainCollection.SetRainStatus(true);
    }
    
    public void StopRaining()
    {
        rainCollection.SetRainStatus(false);
    }
}
