using System;
using System.Collections;
using UnityEngine;

public class RainStateManager : MonoBehaviour
{
    public static event Action OnRainTick;

    private bool isRaining;
    private Coroutine rainCoroutine;

    private void OnEnable()
    {
        SeasonEvents.OnSeasonChanged += OnSeasonChanged;
    }

    private void OnDisable()
    {
        SeasonEvents.OnSeasonChanged -= OnSeasonChanged;
    }

    private void OnSeasonChanged(SeasonState season)
    {
        if (season == SeasonState.Autumn)
            StartRaining();
        else
            StopRaining();
    }

    private void StartRaining()
    {
        if (isRaining) return;
        isRaining = true;
        rainCoroutine = StartCoroutine(RainTickRoutine());
    }

    private void StopRaining()
    {
        if (!isRaining) return;
        isRaining = false;
        if (rainCoroutine != null)
        {
            StopCoroutine(rainCoroutine);
            rainCoroutine = null;
        }
    }

    private IEnumerator RainTickRoutine()
    {
        while (isRaining)
        {
            OnRainTick?.Invoke();
            yield return new WaitForSeconds(1f);
        }
    }
}
