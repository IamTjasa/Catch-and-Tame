using UnityEngine;

public class AnimalReaction : MonoBehaviour
{
    [Header("Animator setup")]
    public int layerIndex = 0;
    public float crossfade = 0.1f;

    [Header("State/Trigger names (match controller)")]
    public string dodgeState = "Dodge";
    public string dodgeTrigger = "PlayReaction";

    public string layState = "Lay";
    public string layTrigger = "Lay";

    public string jumpState = "Jump";
    public string jumpTrigger = "Jump";

    [Header("Timing & priority")]
    public float dodgeDominanceTime = 0.5f; 
    public float jumpDominanceTime = 0.3f; 
    public float layCooldown = 5f;  
    public float jumpCooldown = 1.5f; 

    private Animator animator;
    private AudioSource audioSource;

    private AudioClip dodgeSfx;
    private AudioClip laySfx;
    private AudioClip jumpSfx;

    private float lastLayTime = -999f;
    private float lastDodgeTime = -999f;
    private float lastJumpTime = -999f;

    
    private float layLockedUntil = 0f;

    void Awake()
    {
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();

        
        dodgeSfx = Resources.Load<AudioClip>("Sounds/dodge");
        laySfx = Resources.Load<AudioClip>("Sounds/lay");
        jumpSfx = Resources.Load<AudioClip>("Sounds/jump");
    }

    
    public void PlayReaction()
    {
        if (!animator) return;

        lastDodgeTime = Time.time;

        
        layLockedUntil = Mathf.Max(layLockedUntil, Time.time + dodgeDominanceTime);

        
        ResetTriggerSafe(layTrigger);
        ResetTriggerSafe(jumpTrigger);

        CrossfadeOrTrigger(dodgeState, dodgeTrigger);
        PlaySfx(dodgeSfx);
    }

   
    public void PlayReactionLay()
    {
        if (!animator) return;

        
        if (Time.time < layLockedUntil) return;

        
        if (Time.time - lastLayTime < layCooldown) return;

        
        if (IsPlayingOrTransitioningToAny(animator, layerIndex, dodgeState, jumpState))
            return;

        if (CrossfadeOrTrigger(layState, layTrigger))
        {
            lastLayTime = Time.time;
            PlaySfx(laySfx);
        }
    }

    
    public void PlayReactionJump()
    {
        if (!animator) return;

        
        if (IsPlayingOrTransitioningToAny(animator, layerIndex, dodgeState))
            return;

        
        if (Time.time - lastJumpTime < jumpCooldown) return;

        
        layLockedUntil = Mathf.Max(layLockedUntil, Time.time + jumpDominanceTime);

        
        ResetTriggerSafe(layTrigger);

        if (CrossfadeOrTrigger(jumpState, jumpTrigger))
        {
            lastJumpTime = Time.time;
            PlaySfx(jumpSfx);
        }
    }

    
    private void PlaySfx(AudioClip clip)
    {
        if (clip != null && audioSource != null)
            audioSource.PlayOneShot(clip);
    }

    private void ResetTriggerSafe(string triggerName)
    {
        if (HasParameter(animator, triggerName, AnimatorControllerParameterType.Trigger))
            animator.ResetTrigger(triggerName);
    }

    private bool CrossfadeOrTrigger(string stateName, string triggerName)
    {
        int hash = Animator.StringToHash(stateName);
        bool hasState = animator.HasState(layerIndex, hash);
        bool hasTrigger = HasParameter(animator, triggerName, AnimatorControllerParameterType.Trigger);

        if (hasState)
        {
            animator.CrossFadeInFixedTime(hash, crossfade, layerIndex, 0f);
            return true;
        }
        if (hasTrigger)
        {
            animator.SetTrigger(triggerName);
            return true;
        }
        return false; 
    }

    private static bool HasParameter(Animator anim, string name, AnimatorControllerParameterType type)
    {
        if (!anim || string.IsNullOrEmpty(name)) return false;
        foreach (var p in anim.parameters)
            if (p.type == type && p.name == name)
                return true;
        return false;
    }

    private static bool IsPlayingOrTransitioningToAny(Animator anim, int layer, params string[] stateNames)
    {
        if (!anim || stateNames == null || stateNames.Length == 0) return false;

        var curr = anim.GetCurrentAnimatorStateInfo(layer);
        if (IsAny(curr, stateNames)) return true;

        if (anim.IsInTransition(layer))
        {
            var next = anim.GetNextAnimatorStateInfo(layer);
            if (IsAny(next, stateNames)) return true;
        }
        return false;

        bool IsAny(AnimatorStateInfo info, string[] names)
        {
            foreach (var n in names)
            {
                if (string.IsNullOrEmpty(n)) continue;
                if (info.IsName(n) || info.shortNameHash == Animator.StringToHash(n))
                    return true;
            }
            return false;
        }
    }
}
