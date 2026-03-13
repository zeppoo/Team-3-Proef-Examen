using UnityEngine;

public class AnimationController : MonoBehaviour
{
    private Animator animator;

    void Start()
    {
        animator = GetComponentInChildren<Animator>();
    }

    public void SetWalking(bool isWalking)
    {
        animator.SetBool("Walk", isWalking);
    }

    public void SetIdle(bool idle)
    {
        animator.SetBool("Walk", idle);
    }

    public void SetVictory(bool victory)
    {
        animator.SetBool("SitDown",victory);
        Debug.Log("Victory animation triggered: ");
    }


}
