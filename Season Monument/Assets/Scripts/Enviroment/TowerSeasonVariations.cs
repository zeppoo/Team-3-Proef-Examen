using UnityEngine;

[CreateAssetMenu(fileName = "TowerSeasonVariations", menuName = "Scriptable Objects/TowerSeasonVariations")]
public class TowerSeasonVariations : ScriptableObject
{
    [System.Serializable]
    public struct SeasonVariant
    {
        public Mesh model;
        public Material material;
    }

    [Header("Season Variants")]
    public SeasonVariant spring;
    public SeasonVariant summer;
    public SeasonVariant autumn;
    public SeasonVariant winter;

    public SeasonVariant GetVariant(SeasonState season)
    {
        return season switch
        {
            SeasonState.Spring => spring,
            SeasonState.Summer => summer,
            SeasonState.Autumn => autumn,
            SeasonState.Winter => winter,
            _ => spring
        };
    }
}
