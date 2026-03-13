using UnityEngine;

public class TowerSeasonApplier : MonoBehaviour
{
    [SerializeField] private TowerSeasonVariations variations;

    private MeshFilter[] meshFilters;
    private Renderer[] renderers;

    private void Awake()
    {
        meshFilters = GetComponentsInChildren<MeshFilter>();
        renderers = GetComponentsInChildren<Renderer>();
    }

    private void OnEnable()
    {
        SeasonEvents.OnSeasonChanged += ApplyVariant;
    }

    private void OnDisable()
    {
        SeasonEvents.OnSeasonChanged -= ApplyVariant;
    }

    private void ApplyVariant(SeasonState season)
    {
        if (variations == null) return;

        TowerSeasonVariations.SeasonVariant variant = variations.GetVariant(season);

        if (variant.model != null)
            foreach (MeshFilter mf in meshFilters)
                mf.mesh = variant.model;

        if (variant.material != null)
            foreach (Renderer rend in renderers)
                rend.material = variant.material;
    }
}
