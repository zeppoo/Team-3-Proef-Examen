using UnityEngine;
using UnityEngine.UI;

public class WorldStateSwitch : MonoBehaviour
{
    [SerializeField] private Canvas canvas;
    [SerializeField] private Image buttonIcon;

    [Header("Icons")]
    [SerializeField] private Sprite gameplayIcon;
    [SerializeField] private Sprite viewIcon;

    public WorldState CurrentState { get; private set; } = WorldState.Gameplay;
    public enum WorldState
    {
        Gameplay = 0,
        View = 1
    }

    private void Start()
    {
        UpdateDisplay();
    }

    public void SwitchWorldState()
    {
        CurrentState = CurrentState == WorldState.Gameplay
            ? WorldState.View
            : WorldState.Gameplay;

        UpdateDisplay();
    }

    private void UpdateDisplay()
    {
        buttonIcon.sprite = CurrentState == WorldState.Gameplay ? gameplayIcon : viewIcon;
    }
}
