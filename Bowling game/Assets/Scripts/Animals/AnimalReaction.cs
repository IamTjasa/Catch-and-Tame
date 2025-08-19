using UnityEngine;

public class AnimalReaction : MonoBehaviour
{
    [Header("Animator setup")]
    public int layerIndex = 0;
    public float crossfade = 0.1f;

    [Header("Names (match your controller)")]
    public string dodgeState = "Dodge";
    public string dodgeTrigger = "PlayReaction";
    public string layState = "Lay";
    public string layTrigger = "Lay";

    [Header("Timing & priority")]
    public float dodgeDominanceTime = 0.5f;   // Lay ignoriramo kratek èas po Dodge
    public float layCooldown = 5f;            //  5 s cooldown za Lay

    private Animator animator;
    private AudioSource audioSource;
    private AudioClip dodgeSound;

    private float lastLayTime = -999f;
    private float lastDodgeTime = -999f;
    private float layLockedUntil = 0f;

    void Awake()
    {
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
        dodgeSound = Resources.Load<AudioClip>("Sounds/dodge");
    }

    // ====== DODGE (ima prednost) ======
    public void PlayReaction()
    {
        if (!animator) return;

        lastDodgeTime = Time.time;

        // zakleni Lay za kratek èas po Dodge
        layLockedUntil = Mathf.Max(layLockedUntil, Time.time + dodgeDominanceTime);

        // èe je bil Lay trigger pending, ga resetiramo
        if (HasParameter(animator, layTrigger, AnimatorControllerParameterType.Trigger))
            animator.ResetTrigger(layTrigger);

        int dodgeHash = Animator.StringToHash(dodgeState);
        bool hasDodgeState = animator.HasState(layerIndex, dodgeHash);
        bool hasDodgeTrigger = HasParameter(animator, dodgeTrigger, AnimatorControllerParameterType.Trigger);

        if (hasDodgeState)
            animator.CrossFadeInFixedTime(dodgeHash, crossfade, layerIndex, 0f);
        else if (hasDodgeTrigger)
            animator.SetTrigger(dodgeTrigger);

        PlaySfx();
    }

    // ====== LAY (nižja prioriteta + 5 s cooldown) ======
    public void PlayReactionLay()
    {
        if (!animator) return;

        //  lock po Dodge
        if (Time.time < layLockedUntil) return;

        //  5 s cooldown
        if (Time.time - lastLayTime < layCooldown) return;

        // ne proži Lay, èe smo v/ proti Dodge
        if (IsPlayingOrTransitioningTo(animator, layerIndex, layState: dodgeState))
            return;

        bool hasLayTrigger = HasParameter(animator, layTrigger, AnimatorControllerParameterType.Trigger);
        int layHash = Animator.StringToHash(layState);
        bool hasLayState = animator.HasState(layerIndex, layHash);

        if (hasLayTrigger)
        {
            animator.SetTrigger(layTrigger);
            lastLayTime = Time.time;   // cooldown zaène teèi samo, èe smo res sprožili
            PlaySfx();
            return;
        }

        if (hasLayState)
        {
            animator.CrossFadeInFixedTime(layHash, crossfade, layerIndex, 0f);
            lastLayTime = Time.time;   // cooldown
            PlaySfx();
            return;
        }

        // Ni niè poimenovanega "Lay" — tiho preskoèi, brez cooldowna
    }

    private void PlaySfx()
    {
        if (dodgeSound != null && audioSource != null)
            audioSource.PlayOneShot(dodgeSound);
    }

    private static bool HasParameter(Animator anim, string name, AnimatorControllerParameterType type)
    {
        if (!anim || string.IsNullOrEmpty(name)) return false;
        foreach (var p in anim.parameters)
            if (p.type == type && p.name == name)
                return true;
        return false;
    }

    private static bool IsPlayingOrTransitioningTo(Animator anim, int layer, string layState)
    {
        if (!anim || string.IsNullOrEmpty(layState)) return false;

        var curr = anim.GetCurrentAnimatorStateInfo(layer);
        if (curr.IsName(layState) || curr.shortNameHash == Animator.StringToHash(layState))
            return true;

        if (anim.IsInTransition(layer))
        {
            var next = anim.GetNextAnimatorStateInfo(layer);
            if (next.IsName(layState) || next.shortNameHash == Animator.StringToHash(layState))
                return true;
        }
        return false;
    }
}
