using UnityEngine;

public class DecorationController : MonoBehaviour
{
    [SerializeField] private AssetVariations assetVariations;

    void Awake()
    {
        if (assetVariations != null && assetVariations.seasonMaterial != null)
            GetComponentInChildren<Renderer>().material = assetVariations.seasonMaterial;
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
        if (assetVariations == null) return;
        assetVariations.ApplySeason(season);
    }

    void LateUpdate()
    {
        // Make the decoration always face the camera
        transform.forward = Camera.main.transform.forward;
    }
}
