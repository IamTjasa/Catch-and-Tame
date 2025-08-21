using System.Collections;
using System.Linq;
using UnityEngine;

/// Minigame: žival (maèka) in igralec si podajata ENO žogo po vedno isti paraboli.
/// - Ob vstopu: postavi rig na PlayerSpawn in ga obrni proti maèki.
/// - Maèka NIKOLI ne zgreši: ob prihodu žoge jo "ujame" (snap na AnimalHold) in sproži odboj (AnimalReaction.PlayReaction()).
/// - Igralec se žoge SAMO DOTAKNE (leva ali desna roka). Dotik med letom ANIMALPLAYER žogo takoj obrne NAZAJ
///   po ISTI poti (obratni u), ne glede na mesto dotika.
/// - Èe dotika ni, žoga pride do PlayerAnchor, "pade" na groundY in se respawna pri maèki.
/// - Prvi met po startDelay sekundah.
public class CatchBallMinigame : MonoBehaviour
{
    [Header("Scene refs")]
    public OVRCameraRig rig;
    public Transform centerEye;
    public Transform playerSpawn;

    [Header("Anchors")]
    public Transform playerAnchor; // fiksna toèka pri igralcu
    public Transform animalAnchor; // toèka pri maèki, kamor prileti žoga
    public Transform animalHold;   // toèka držanja žoge pri maèki

    [Header("Animal")]
    public Animator animalAnimator;
    public AnimalReaction animalReaction;            // èe je nastavljeno, klièemo PlayReaction()
    public string animTriggerOnHit = "PlayReaction"; // fallback trigger, èe ni AnimalReaction

    [Header("Ball")]
    public GameObject ballPrefab;  // prefab žoge (Rigidbody NI potreben)

    [Header("Arc tuning")]
    public float timeToPlayer = 1.0f;  // flight time ANIMALPLAYER
    public float timeToAnimal = 1.0f;  // (neuporabljeno pri "same path", pusti za rezervo)
    public float arcHeight = 0.6f;   // višina parabole

    [Header("Hands (for tap)")]
    public OVRHand leftHand;           // ***povleci OVRHand komponenti z Left/RightHandAnchor!***
    public OVRHand rightHand;
    public bool requireHighConfidence = true;  // èe je Low, ignoriramo dotik
    public float touchRadius = 0.10f;          // 10 cm okno dotika

    [Header("Fail & respawn")]
    public float groundY = 0.0f;       // višina tal
    public float dropSpeed = 2.0f;     // m/s
    public float respawnDelay = 0.8f;  // po failu

    [Header("Flow")]
    public float startDelay = 5.0f;    // prvi met po 5 s
    public bool debugLog = false;

    // --- internal ---
    GameObject ball;
    Transform leftTip, rightTip;

    void Awake()
    {
        if (!rig) rig = FindObjectOfType<OVRCameraRig>();
        if (rig && !centerEye) centerEye = rig.centerEyeAnchor;
        if (!animalReaction && animalAnimator)
            animalReaction = animalAnimator.GetComponent<AnimalReaction>();
    }

    void Start()
    {
        if (!ballPrefab || !playerAnchor || !animalAnchor || !animalHold)
        {
            Debug.LogError("[CatchBallMinigame] Manjkajo reference (ballPrefab/playerAnchor/animalAnchor/animalHold).");
            enabled = false;
            return;
        }

        PlaceRigAtSpawnFacingAnimal();

        // poišèi index tip transformi (podpira OVRSkeleton in OVRCustomSkeleton)
        leftTip = FindIndexTip(leftHand);
        rightTip = FindIndexTip(rightHand);

        // ustvari edino žogo in jo postavi k maèki
        ball = Instantiate(ballPrefab, animalHold.position, animalHold.rotation);

        StartCoroutine(GameLoop());
    }

    // postavi rig na PlayerSpawn in ga obrni proti maèki
    void PlaceRigAtSpawnFacingAnimal()
    {
        if (!rig || !centerEye || !playerSpawn) return;

        Vector3 headOffset = centerEye.position - rig.transform.position;

        Vector3 newPos = playerSpawn.position - new Vector3(headOffset.x, 0f, headOffset.z);
        newPos.y = playerSpawn.position.y - headOffset.y;

        float yaw = rig.transform.eulerAngles.y;
        Transform look = animalAnchor ? animalAnchor : animalHold;
        if (look)
        {
            Vector3 headAfter = newPos + headOffset;
            Vector3 dir = look.position - headAfter; dir.y = 0f;
            if (dir.sqrMagnitude > 0.0001f)
                yaw = Quaternion.LookRotation(dir, Vector3.up).eulerAngles.y;
        }

        rig.transform.SetPositionAndRotation(newPos, Quaternion.Euler(0f, yaw, 0f));
    }

    IEnumerator GameLoop()
    {
        yield return new WaitForSeconds(startDelay);

        while (true)
        {
            // maèka "odboji" in poda
            SnapBallTo(animalHold);
            PlayAnimalBounce();

            // ANIMAL  PLAYER; dotik obrne NAZAJ po ISTI poti
            yield return OutAndBackSamePathWithTap(
                from: animalAnchor.position,
                to: playerAnchor.position,
                duration: timeToPlayer,
                height: arcHeight
            );

            // po vrnitvi nazaj maèka ujame in lahko sledi kratka pavza
            SnapBallTo(animalHold);
            PlayAnimalBounce();

            yield return new WaitForSeconds(0.25f);
        }
    }

    void PlayAnimalBounce()
    {
        if (animalReaction) animalReaction.PlayReaction();           // tvoj "Dodge" odboj
        else if (animalAnimator) animalAnimator.SetTrigger(animTriggerOnHit);
    }

    // Let ANIMALPLAYER; ob DOTIKU: invertira param u in se po isti paraboli vrne do ANIMAL.
    // Èe dotika ni: pade na tla in respawn pri maèki.
    IEnumerator OutAndBackSamePathWithTap(Vector3 from, Vector3 to, float duration, float height)
    {
        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            float u = Mathf.Clamp01(t / duration);
            Vector3 pos = EvaluateParabola(from, to, height, u);
            ball.transform.position = pos;

            if (IsPlayerTouching(pos))
            {
                // vrni po ISTI krivulji (u -> 0) s podobno hitrostjo
                float uStart = u;
                float backTime = uStart * duration;
                float tb = 0f;
                while (tb < backTime)
                {
                    tb += Time.deltaTime;
                    float ur = Mathf.Lerp(uStart, 0f, tb / backTime);
                    ball.transform.position = EvaluateParabola(from, to, height, ur);
                    yield return null;
                }
                ball.transform.position = from;
                yield break;
            }
            yield return null;
        }

        // ni bilo dotika  padec in respawn
        ball.transform.position = to;
        yield return DropToGround();
        yield return new WaitForSeconds(respawnDelay);
        SnapBallTo(animalHold);
    }

    IEnumerator DropToGround()
    {
        if (debugLog) Debug.Log("[CatchBallMinigame] Miss  drop");
        Vector3 p = ball.transform.position;
        if (p.y <= groundY + 0.001f) yield break;

        while (p.y > groundY)
        {
            p.y = Mathf.Max(groundY, p.y - dropSpeed * Time.deltaTime);
            ball.transform.position = p;
            yield return null;
        }
    }

    // -- parabola: linearno Lerp + navpièni offset 4*h*u*(1-u)
    Vector3 EvaluateParabola(Vector3 from, Vector3 to, float height, float u)
    {
        Vector3 linear = Vector3.Lerp(from, to, u);
        float yOffset = 4f * height * u * (1f - u); // maxheight pri u=0.5
        return new Vector3(linear.x, linear.y + yOffset, linear.z);
    }

    void SnapBallTo(Transform t)
    {
        ball.transform.position = t.position;
        ball.transform.rotation = t.rotation;
    }

    // -------- DOTIK (index tip -> pointer pose -> root roke) --------
    bool IsPlayerTouching(Vector3 ballPos)
    {
        return HandTouch(leftHand, ref leftTip, ballPos) ||
               HandTouch(rightHand, ref rightTip, ballPos);
    }

    bool HandTouch(OVRHand hand, ref Transform tip, Vector3 ballPos)
    {
        if (!hand) return false;
        if (!hand.IsTracked) return false;

        // mehkejši gate: èe je Low in je requireHighConfidence=true, prezri dotik
        if (requireHighConfidence &&
            hand.HandConfidence == OVRHand.TrackingConfidence.Low)
            return false;

        if (!tip || !tip.gameObject.activeInHierarchy)
            tip = FindIndexTip(hand);

        Transform probe = tip ? tip : (hand.PointerPose ? hand.PointerPose : hand.transform);

        float d = Vector3.Distance(probe.position, ballPos);
        if (debugLog) Debug.Log($"[CatchBallMinigame] {hand.name}ball d={d:F3}");
        return d <= touchRadius;
    }

    // podpira OVRSkeleton in OVRCustomSkeleton; išèe po imenu "index" + "tip"
    Transform FindIndexTip(OVRHand hand)
    {
        if (!hand) return null;

        // 1) OVRSkeleton
        var skel = hand.GetComponent<OVRSkeleton>();
        if (skel && skel.Bones != null && skel.Bones.Count > 0)
        {
            var tip = skel.Bones.FirstOrDefault(b =>
                b != null && b.Transform &&
                b.Transform.name.ToLower().Contains("index") &&
                b.Transform.name.ToLower().Contains("tip"));
            if (tip != null) return tip.Transform;
        }

        // 2) OVRCustomSkeleton (pogosto v Building Blocks)
        var custom = hand.GetComponent<OVRCustomSkeleton>();
        if (custom && custom.Bones != null && custom.Bones.Count > 0)
        {
            var tip = custom.Bones.FirstOrDefault(b =>
                b != null && b.Transform &&
                b.Transform.name.ToLower().Contains("index") &&
                b.Transform.name.ToLower().Contains("tip"));
            if (tip != null) return tip.Transform;
        }

        return null;
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        if (playerAnchor && animalAnchor)
        {
            Vector3 prev = animalAnchor.position;
            for (int i = 1; i <= 24; i++)
            {
                float u = i / 24f;
                Vector3 p = EvaluateParabola(animalAnchor.position, playerAnchor.position, arcHeight, u);
                Gizmos.DrawLine(prev, p);
                prev = p;
            }
        }
    }
#endif
}
