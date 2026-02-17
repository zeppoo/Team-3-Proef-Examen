using System;
using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(fileName = "TileData", menuName = "Scriptable Objects/TileData")]
public class TileData : ScriptableObject
{
    [Header("Tile Properties")]
    public bool isWalkable = true;
    public Color tileColor = Color.white;

    [Header("Season Overrides")]
    public SeasonOverride[] seasonOverrides;

    [Header("Events")]
    public UnityEvent OnTileEntered;
    public UnityEvent OnTileExited;
    public UnityEvent<SeasonState> OnTileSeasonChanged;

    public bool TryGetSeasonOverride(SeasonState season, out SeasonOverride result)
    {
        if (seasonOverrides != null)
        {
            for (int i = 0; i < seasonOverrides.Length; i++)
            {
                if (seasonOverrides[i].season == season)
                {
                    result = seasonOverrides[i];
                    return true;
                }
            }
        }

        result = default;
        return false;
    }
}

[Serializable]
public struct SeasonOverride
{
    public SeasonState season;
    public bool overrideWalkable;
    public bool isWalkable;
    public bool overrideColor;
    public Color tileColor;
}
