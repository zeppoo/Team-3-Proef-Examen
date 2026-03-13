using Unity.VisualScripting;
using UnityEngine;

public class VictoryTile : MonoBehaviour
{

    private void SetPlayerVictoryState(AnimationController animController)
    {
        animController.SetVictory(true);
        animController.SetWalking(false);
        animController.SetIdle(false);
    }
        
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            AnimationController animController = collision.gameObject.GetComponentInParent<AnimationController>();
            SceneManager sceneManager = collision.gameObject.GetComponentInParent<SceneManager>();
            SetPlayerVictoryState(animController);
            StartCoroutine(sceneManager.LoadSceneWithDelay());

        }
    }
}
