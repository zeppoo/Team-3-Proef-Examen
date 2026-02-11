using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectMaterialState : MonoBehaviour
{
    [Header("Season Colors")]
    public Color springColor = new Color(0.4f, 0.8f, 0.4f);
    public Color summerColor = new Color(0.2f, 0.6f, 0.2f);
    public Color autumnColor = new Color(0.8f, 0.5f, 0.2f);
    public Color winterColor = new Color(0.9f, 0.9f, 1.0f);

    private Renderer rend;
    private Material materialInstance;

    void Awake()
    {
        rend = GetComponent<Renderer>();

        // Create a unique material instance (IMPORTANT)
        materialInstance = rend.material;
    }

    void OnEnable()
    {
        SeasonEvents.OnSeasonChanged += OnSeasonChanged;
    }

    void OnDisable()
    {
        SeasonEvents.OnSeasonChanged -= OnSeasonChanged;
    }

    void OnSeasonChanged(SeasonState season)
    {
        materialInstance.color = GetColorForSeason(season);
    }

    Color GetColorForSeason(SeasonState season)
    {
        return season switch
        {
            SeasonState.Spring => springColor,
            SeasonState.Summer => summerColor,
            SeasonState.Autumn => autumnColor,
            SeasonState.Winter => winterColor,
            _ => materialInstance.color
        };
    }
}
