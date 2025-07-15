using UnityEngine;

public class AnimalReaction : MonoBehaviour
{
    private Animator animator;
    private AudioSource audioSource;
    private AudioClip dodgeSound;

    void Awake()
    {
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
        dodgeSound = Resources.Load<AudioClip>("Sounds/dodge");
    }

    public void PlayReaction()
    {
        Debug.Log("Playing reaction trigger!");
        animator.SetTrigger("PlayReaction");

        if (dodgeSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(dodgeSound);
        }
    }
}
