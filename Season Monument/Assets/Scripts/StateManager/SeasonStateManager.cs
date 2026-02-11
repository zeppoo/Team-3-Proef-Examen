using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum SeasonState
{
    Spring,
    Summer,
    Autumn,
    Winter
}

public class SeasonStateManager : MonoBehaviour
{
    [SerializeField] private SeasonState currentSeason = SeasonState.Spring;

    public void SetSeason(SeasonState newSeason)
    {
        currentSeason = newSeason;
        Debug.Log("Season changed to: " + currentSeason);
        SeasonEvents.RaiseSeasonChanged(currentSeason);
        
        switch (currentSeason)
        {
            case SeasonState.Spring:
                SeasonEvents.RaiseSpringStarted();
                break;
            case SeasonState.Summer:
                SeasonEvents.RaiseSummerStarted();
                break;
            case SeasonState.Autumn:
                SeasonEvents.RaiseAutumnStarted();
                break;
            case SeasonState.Winter:
                SeasonEvents.RaiseWinterStarted();
                break;
        }
    }

    public void NextSeason()
    {
        currentSeason = (SeasonState)(((int)currentSeason + 1) % 4);
        SetSeason(currentSeason);
    }

    public void PreviousSeason()
    {
        currentSeason = (SeasonState)(((int)currentSeason + 3) % 4);
        SetSeason(currentSeason);
    }

    void Start()
    {
        // Raise initial season event
        SetSeason(currentSeason);
    }
}

public static class SeasonEvents
{
    public static event Action<SeasonState> OnSeasonChanged;
    
    public static event Action OnSpringStarted;
    public static event Action OnSummerStarted;
    public static event Action OnAutumnStarted;
    public static event Action OnWinterStarted;

    internal static void RaiseSeasonChanged(SeasonState newSeason)
    {
        OnSeasonChanged?.Invoke(newSeason);
    }

    internal static void RaiseSpringStarted()
    {
        OnSpringStarted?.Invoke();
    }

    internal static void RaiseSummerStarted()
    {
        OnSummerStarted?.Invoke();
    }

    internal static void RaiseAutumnStarted()
    {
        OnAutumnStarted?.Invoke();
    }

    internal static void RaiseWinterStarted()
    {
        OnWinterStarted?.Invoke();
    }
}