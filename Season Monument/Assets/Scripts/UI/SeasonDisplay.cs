using UnityEngine;
using UnityEngine.UI;

public class SeasonDisplay : MonoBehaviour
{
    [SerializeField] private Image seasonIcon;
    [SerializeField] private Button seasonButton;
    [SerializeField] private SeasonStateManager seasonStateManager;

    [Header("Season Sprites")]
    [SerializeField] private Sprite springSprite;
    [SerializeField] private Sprite summerSprite;
    [SerializeField] private Sprite autumnSprite;
    [SerializeField] private Sprite winterSprite;

    private void OnEnable()
    {
        SeasonEvents.OnSeasonChanged += OnSeasonChanged;
        if (seasonButton != null)
            seasonButton.onClick.AddListener(OnIconClicked);
    }

    private void OnDisable()
    {
        SeasonEvents.OnSeasonChanged -= OnSeasonChanged;
        if (seasonButton != null)
            seasonButton.onClick.RemoveListener(OnIconClicked);
    }

    private void OnIconClicked()
    {
        if (seasonStateManager != null)
            seasonStateManager.NextSeason();
    }

    private void OnSeasonChanged(SeasonState season)
    {
        seasonIcon.sprite = season switch
        {
            SeasonState.Spring => springSprite,
            SeasonState.Summer => summerSprite,
            SeasonState.Autumn => autumnSprite,
            SeasonState.Winter => winterSprite,
            _ => seasonIcon.sprite
        };
    }
}
