using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WorldStateSwitch : MonoBehaviour
{
    [SerializeField] private Canvas canvas;
    [SerializeField] private Button switchButton;
    [SerializeField] private Image displayImage;

    public WorldState CurrentState { get; private set; } = WorldState.Gameplay;
    public enum WorldState
    {
        Gameplay = 0,
        View = 1
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
        if (CurrentState == WorldState.Gameplay)
        {
            displayImage.color = Color.green;
            switchButton.GetComponentInChildren<TMP_Text>().text = "Switch to View";
        }
        else
        {
            displayImage.color = Color.blue;
            switchButton.GetComponentInChildren<TMP_Text>().text = "Switch to Gameplay";
        }
    }
}
