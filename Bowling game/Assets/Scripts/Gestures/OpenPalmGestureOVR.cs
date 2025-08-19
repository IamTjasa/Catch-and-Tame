using System.Collections;
using UnityEngine;

public class OpenPalmSingleOVR : MonoBehaviour
{
    [Header("Meta hand")]
    public OVRHand hand;                 // OVRHand (Left ali Right)

    [Header("Primary target (optional)")]
    public AnimalReaction target;        // pusti prazno -> auto-find

    [Header("Fallback animator (�e target ni prisoten)")]
    public Animator animator;            // dodeli Animator od �ivali (�e target ni)
    public string playStateName = "Lay"; // ime stanja v Animatorju
    public float crossfade = 0.1f;

    [Header("Auto-find nastavitve")]
    [SerializeField] bool autoFindTarget = true;
    [SerializeField] float refetchEvery = 0.5f;

    [Header("Gesture tuning")]
    [Range(0f, 1f)] public float pinchMax = 0.15f;  // vsi prsti < threshold => odprta dlan
    public float minInterval = 0.25f;               // min �as med spro�itvami

    bool wasOpen;
    float lastFire, nextRefetch;

    IEnumerator Start()
    {
        if (!hand) hand = GetComponentInChildren<OVRHand>();
        yield return null; // po�akaj init

        // Najprej poskusi najti AnimalReaction
        if (!target && autoFindTarget) target = FindObjectOfType<AnimalReaction>();

        // �e ga ni, poskusi najti Animator na �ivali
        if (!target && !animator && autoFindTarget)
        {
            var liveAnimal = FindObjectOfType<Animator>();
            if (liveAnimal != null) animator = liveAnimal;
        }
    }

    void Update()
    {
        if (!hand) return;

        // ob�asno ponovno poi��emo tar�o
        if (autoFindTarget && Time.time >= nextRefetch)
        {
            nextRefetch = Time.time + refetchEvery;

            if (target == null || !target.isActiveAndEnabled)
                target = FindObjectOfType<AnimalReaction>();

            if (!target && (animator == null || !animator.isActiveAndEnabled))
                animator = FindObjectOfType<Animator>();
        }

        // zahtevaj zanesljiv tracking
        if (hand.HandConfidence != OVRHand.TrackingConfidence.High) return;

        bool open =
            hand.GetFingerPinchStrength(OVRHand.HandFinger.Thumb) < pinchMax &&
            hand.GetFingerPinchStrength(OVRHand.HandFinger.Index) < pinchMax &&
            hand.GetFingerPinchStrength(OVRHand.HandFinger.Middle) < pinchMax &&
            hand.GetFingerPinchStrength(OVRHand.HandFinger.Ring) < pinchMax &&
            hand.GetFingerPinchStrength(OVRHand.HandFinger.Pinky) < pinchMax;

        // pro�i samo na prehod "zaprt -> odprt"
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
            // obstoje�e vedenje � ne spreminjamo AnimalReaction
            target.PlayReactionLay();
            return;
        }

        if (animator)
        {
            // zaigraj stanje po imenu (potreben state "Lay" v Animatorju)
            animator.CrossFadeInFixedTime(playStateName, crossfade, 0, 0f);
            return;
        }

        Debug.LogWarning("[OpenPalmSingleOVR] Ne najdem ne AnimalReaction ne Animatorja.");
    }
}
