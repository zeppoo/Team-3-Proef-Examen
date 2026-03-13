using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class SeasonTextureApplier : MonoBehaviour
{
    [SerializeField] private AssetVariations assetVariations;

    private Renderer rend;

    private void OnEnable()
    {
        rend = GetComponent<Renderer>();
        SeasonEvents.OnSeasonChanged += OnSeasonChanged;
    }

    private void OnDisable()
    {
        SeasonEvents.OnSeasonChanged -= OnSeasonChanged;
    }

    private void OnSeasonChanged(SeasonState season)
    {
        if (assetVariations == null) return;

        Texture2D texture = season switch
        {
            SeasonState.Spring => assetVariations.springAlbedo,
            SeasonState.Summer => assetVariations.summerAlbedo,
            SeasonState.Autumn => assetVariations.autumnAlbedo,
            SeasonState.Winter => assetVariations.winterAlbedo,
            _ => null
        };

        if (texture != null)
            rend.material.mainTexture = texture;
    }
}
