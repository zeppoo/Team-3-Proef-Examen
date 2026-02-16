using UnityEngine;

public class RainCollection : MonoBehaviour
{
    private float rainAmount = 0f;
    [SerializeField] private float rainRate = 1;
    [SerializeField] private float maxRainAmount = 2;

    internal bool isRaining = false;
    private bool isCollecting = false;

    private bool hasCollectedEnoughRain = false;

    void Update()
    {
        if (isRaining)
        {
            isCollecting = rainAmount <= maxRainAmount;

            if (isCollecting)
                CollectRain();
        }

        hasCollectedEnoughRain = rainAmount >= maxRainAmount;

        
    }

    public void CollectRain()
    {
        rainAmount += rainRate * Time.deltaTime;
        Debug.Log("Collecting rain... Current amount: " + rainAmount);
    }

    public void SetRainStatus(bool raining)
    {
        isRaining = raining;
        if (!isRaining)
        {
            isCollecting = false;
        }
    }
}
