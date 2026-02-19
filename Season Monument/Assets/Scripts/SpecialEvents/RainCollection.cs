using UnityEngine;

public class RainCollection : MonoBehaviour
{
    private float rainAmount = 0f;
    [SerializeField] private float rainRate = 1f;
    [SerializeField] private float maxRainAmount = 2f;

    private void OnEnable()
    {
        RainStateManager.OnRainTick += OnRainTick;
    }

    private void OnDisable()
    {
        RainStateManager.OnRainTick -= OnRainTick;
    }

    private void OnRainTick()
    {
        if (rainAmount >= maxRainAmount) return;

        rainAmount += rainRate;
        Debug.Log("Collecting rain... Current amount: " + rainAmount);
    }
}
