using UnityEngine;

public class AnimalReaction : MonoBehaviour
{
    private Animator animator;

    void Awake()
    {
        animator = GetComponent<Animator>();
    }

    public void PlayReaction()
    {
        Debug.Log("Playing reaction trigger!");
        animator.SetTrigger("PlayReaction");
    }
}
