using System.Collections;
using UnityEngine;

public class OpenPalmSingleOVR : MonoBehaviour
{
    [Header("Meta hand")]
    public OVRHand hand;                

    [Header("Primary target (optional)")]
    public AnimalReaction target;       

    [Header("Fallback animator (èe target ni prisoten)")]
    public Animator animator;          
    public string playStateName = "Lay"; 
    public float crossfade = 0.1f;

    [Header("Auto-find nastavitve")]
    [SerializeField] bool autoFindTarget = true;
    [SerializeField] float refetchEvery = 0.5f;

    [Header("Gesture tuning")]
    [Range(0f, 1f)] public float pinchMax = 0.15f;  
    public float minInterval = 0.25f;               

    bool wasOpen;
    float lastFire, nextRefetch;

    IEnumerator Start()
    {
        if (!hand) hand = GetComponentInChildren<OVRHand>();
        yield return null;

        
        if (!target && autoFindTarget) target = FindObjectOfType<AnimalReaction>();

        
        if (!target && !animator && autoFindTarget)
        {
            var liveAnimal = FindObjectOfType<Animator>();
            if (liveAnimal != null) animator = liveAnimal;
        }
    }

    void Update()
    {
        if (!hand) return;

        
        if (autoFindTarget && Time.time >= nextRefetch)
        {
            nextRefetch = Time.time + refetchEvery;

            if (target == null || !target.isActiveAndEnabled)
                target = FindObjectOfType<AnimalReaction>();

            if (!target && (animator == null || !animator.isActiveAndEnabled))
                animator = FindObjectOfType<Animator>();
        }

        
        if (hand.HandConfidence != OVRHand.TrackingConfidence.High) return;

        bool open =
            hand.GetFingerPinchStrength(OVRHand.HandFinger.Thumb) < pinchMax &&
            hand.GetFingerPinchStrength(OVRHand.HandFinger.Index) < pinchMax &&
            hand.GetFingerPinchStrength(OVRHand.HandFinger.Middle) < pinchMax &&
            hand.GetFingerPinchStrength(OVRHand.HandFinger.Ring) < pinchMax &&
            hand.GetFingerPinchStrength(OVRHand.HandFinger.Pinky) < pinchMax;

        
        if (open && !wasOpen && Time.time - lastFire >= minInterval)
        {
            lastFire = Time.time;
            FireReaction();
        }

        wasOpen = open;
    }

    void FireReaction()
    {
        if (target)
        {
            
            target.PlayReactionLay();
            return;
        }

        if (animator)
        {
           
            animator.CrossFadeInFixedTime(playStateName, crossfade, 0, 0f);
            return;
        }

        Debug.LogWarning("[OpenPalmSingleOVR] Ne najdem ne AnimalReaction ne Animatorja.");
    }
}
