using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Collider))]
public class SceneSwitch : MonoBehaviour
{
    [Header("Target")]
    public string targetScene = "Playground";

    [Header("Hands (assign from your Camera Rig)")]
    public OVRHand leftHand;
    public OVRHand rightHand;

    [Header("Touch detection")]
    [Tooltip("Kolikšna bližina konice prsta/roke do colliderja šteje kot dotik (v metrih).")]
    public float touchRadius = 0.05f;   // 5 cm
    [Tooltip("Koliko èasa mora dotik trajati, da sproži preklop.")]
    public float holdTime = 0.20f;      // sekunde
    public float startDelay = 1.0f;     // ignoriraj prve X sekund po loadu
    public float cooldown = 1.0f;       // zaklep po preklopu
    public bool requireHighConfidence = true;

    [Header("Debug")]
    public bool debugLog = false;

    float sceneStartTime;
    float lockUntil;
    float leftTimer, rightTimer;
    Collider col;

    void Awake()
    {
        col = GetComponent<Collider>();
        sceneStartTime = Time.time;
        lockUntil = 0f;
        leftTimer = rightTimer = 0f;
    }

    void Update()
    {
        if (string.IsNullOrEmpty(targetScene)) return;
        if (SceneManager.GetActiveScene().name == targetScene) return;         // ne preklopi v isto sceno
        if (Time.time - sceneStartTime < startDelay) { ResetTimers(); return; }
        if (Time.time < lockUntil) { ResetTimers(); return; }

        bool lTouch = IsHandTouching(leftHand);
        bool rTouch = IsHandTouching(rightHand);

        if (lTouch) leftTimer += Time.deltaTime; else leftTimer = 0f;
        if (rTouch) rightTimer += Time.deltaTime; else rightTimer = 0f;

        if (leftTimer >= holdTime || rightTimer >= holdTime)
        {
            if (debugLog) Debug.Log("[TouchToLoadScene] Touch confirmed  loading " + targetScene);
            lockUntil = Time.time + cooldown;
            SceneManager.LoadScene(targetScene, LoadSceneMode.Single);
        }
    }

    void ResetTimers() { leftTimer = rightTimer = 0f; }

    bool IsHandTouching(OVRHand hand)
    {
        if (!hand) return false;
        if (!hand.IsTracked) return false;
        if (requireHighConfidence &&
            hand.HandConfidence != OVRHand.TrackingConfidence.High) return false;

        Transform tip = GetIndexTipTransform(hand);
        if (!tip)
        {
            // fallback: pointer pose ali kar root roke
            tip = hand.PointerPose ? hand.PointerPose : hand.transform;
        }

        Vector3 p = tip.position;

        // uporabi najbližjo toèko na colliderju za robustnost
        Vector3 closest = col ? col.ClosestPoint(p) : transform.position;
        float d = Vector3.Distance(p, closest);

        if (debugLog) Debug.Log($"[TouchToLoadScene] {hand.name} d={d:F3}");
        return d <= touchRadius;
    }

    Transform GetIndexTipTransform(OVRHand hand)
    {
        var skel = hand.GetComponent<OVRSkeleton>();
        if (skel != null && skel.Bones != null && skel.Bones.Count > 0)
        {
            // Najprej poskusi toèen "IndexTip", èe tvoja verzija SDK spremeni enum, gremo po imenu.
            var byName = skel.Bones.FirstOrDefault(b =>
                b != null && b.Transform &&
                b.Transform.name.ToLower().Contains("index") &&
                b.Transform.name.ToLower().Contains("tip"));
            if (byName != null) return byName.Transform;

            // (Èe tvoja verzija podpira enum BoneId.Hand_IndexTip, ga lahko uporabiš namesto po imenu.)
            // var exact = skel.Bones.FirstOrDefault(b => b.Id == OVRSkeleton.BoneId.Hand_IndexTip);
            // if (exact != null) return exact.Transform;
        }
        return null;
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        // Informativno: sfera okoli središèa objekta (dejanski test je najbližja toèka colliderja).
        Gizmos.DrawWireSphere(transform.position, touchRadius);
    }
#endif
}
