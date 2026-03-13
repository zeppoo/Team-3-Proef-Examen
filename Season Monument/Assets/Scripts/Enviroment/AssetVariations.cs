using UnityEngine;

[CreateAssetMenu(fileName = "AssetVariations", menuName = "Scriptable Objects/AssetVariations")]
public class AssetVariations : ScriptableObject
{
    public Material seasonMaterial;

    [Header("Season Albedo Textures")]
    public Texture2D springAlbedo;
    public Texture2D summerAlbedo;
    public Texture2D autumnAlbedo;
    public Texture2D winterAlbedo;

    public void ApplySeason(SeasonState season)
    {
        if (seasonMaterial == null) return;

        Texture2D texture = season switch
        {
            SeasonState.Spring => springAlbedo,
            SeasonState.Summer => summerAlbedo,
            SeasonState.Autumn => autumnAlbedo,
            SeasonState.Winter => winterAlbedo,
            _ => null
        };

        if (texture != null)
            seasonMaterial.mainTexture = texture;
    }
}