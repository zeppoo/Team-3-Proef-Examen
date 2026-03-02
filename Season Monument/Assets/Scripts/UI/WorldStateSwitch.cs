using UnityEngine;
using UnityEngine.UI;

public class WorldStateSwitch : MonoBehaviour
{
    [SerializeField] private Canvas canvas;
    [SerializeField] private Button switchButton;
    [SerializeField] private Image displayImage;

    private int currentState = 0;

    public void SwitchWorldState()
    {
        currentState = (currentState + 1) % 2; 
        UpdateDisplay();
    }

    private void UpdateDisplay()
    {
        if (currentState == 0)
        {
            displayImage.color = Color.white; 
        }
        else
        {
            displayImage.color = Color.gray;
        }
    }
}
