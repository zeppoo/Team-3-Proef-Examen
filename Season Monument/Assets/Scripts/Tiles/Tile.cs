using System.Collections.Generic;
using UnityEngine;

public abstract class Tile : MonoBehaviour
{
    public Tile parent;
    public Tile connecetdTile;
    public List<GameObject> neighbours = new List<GameObject>();
    public virtual bool isWalkable => false;
    private BoxCollider col2d;
    private Renderer rend;
    private Material materialInstance;

    public virtual void OnEnable()
    {
        rend = GetComponent<Renderer>();

        // Create a unique material instance (IMPORTANT)
        materialInstance = rend.material;

        
        SeasonEvents.OnSeasonChanged += OnSeasonChanged;
    }

    public virtual void OnDisable()
    {
        SeasonEvents.OnSeasonChanged -= OnSeasonChanged;
    }

    public virtual void OnSeasonChanged(SeasonState season)
    {
        // Base implementation does nothing, can be overridden by subclasses
    }
}
