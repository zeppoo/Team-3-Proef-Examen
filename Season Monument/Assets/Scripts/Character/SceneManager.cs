using System.Collections;
using UnityEngine;

public class SceneManager : MonoBehaviour
{
   public void LoadScene()
   {
        StartCoroutine(LoadSceneWithDelay());
    }

    public IEnumerator LoadSceneWithDelay()
    {
        yield return new WaitForSeconds(2f);
        UnityEngine.SceneManagement.SceneManager.LoadScene(1);
    }
}
