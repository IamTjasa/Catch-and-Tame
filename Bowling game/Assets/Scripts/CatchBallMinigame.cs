using System.Collections;
using System.Linq;
using UnityEngine;

public class CatchBallMinigame : MonoBehaviour
{
    [Header("Scene refs")]
    public OVRCameraRig rig;
    public Transform centerEye;
    public Transform playerSpawn;

    [Header("Anchors")]
    public Transform playerAnchor; 
    public Transform animalAnchor; 
    public Transform animalHold;   

    [Header("Animal")]
    public Animator animalAnimator;
    public AnimalReaction animalReaction;            
    public string animTriggerOnHit = "PlayReaction"; 

    [Header("Ball")]
    public GameObject ballPrefab;  

    [Header("Arc tuning")]
    public float timeToPlayer = 1.0f;  
    public float timeToAnimal = 1.0f;  
    public float arcHeight = 0.6f;  

    [Header("Hands (for tap)")]
    public OVRHand leftHand;          
    public OVRHand rightHand;
    public bool requireHighConfidence = true;  
    public float touchRadius = 0.10f;          

    [Header("Fail & respawn")]
    public float groundY = 0.0f;       
    public float dropSpeed = 2.0f;     
    public float respawnDelay = 0.8f;  

    [Header("Flow")]
    public float startDelay = 5.0f;    
    public bool debugLog = false;

    
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

       
        leftTip = FindIndexTip(leftHand);
        rightTip = FindIndexTip(rightHand);

       
        ball = Instantiate(ballPrefab, animalHold.position, animalHold.rotation);

        StartCoroutine(GameLoop());
    }

    
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
            
            SnapBallTo(animalHold);
            PlayAnimalBounce();

            
            yield return OutAndBackSamePathWithTap(
                from: animalAnchor.position,
                to: playerAnchor.position,
                duration: timeToPlayer,
                height: arcHeight
            );

            
            SnapBallTo(animalHold);
            PlayAnimalBounce();

            yield return new WaitForSeconds(0.25f);
        }
    }

    void PlayAnimalBounce()
    {
        if (animalReaction) animalReaction.PlayReaction();          
        else if (animalAnimator) animalAnimator.SetTrigger(animTriggerOnHit);
    }

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

    
    Vector3 EvaluateParabola(Vector3 from, Vector3 to, float height, float u)
    {
        Vector3 linear = Vector3.Lerp(from, to, u);
        float yOffset = 4f * height * u * (1f - u); 
        return new Vector3(linear.x, linear.y + yOffset, linear.z);
    }

    void SnapBallTo(Transform t)
    {
        ball.transform.position = t.position;
        ball.transform.rotation = t.rotation;
    }

   
    bool IsPlayerTouching(Vector3 ballPos)
    {
        return HandTouch(leftHand, ref leftTip, ballPos) ||
               HandTouch(rightHand, ref rightTip, ballPos);
    }

    bool HandTouch(OVRHand hand, ref Transform tip, Vector3 ballPos)
    {
        if (!hand) return false;
        if (!hand.IsTracked) return false;

        
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

    
    Transform FindIndexTip(OVRHand hand)
    {
        if (!hand) return null;

        
        var skel = hand.GetComponent<OVRSkeleton>();
        if (skel && skel.Bones != null && skel.Bones.Count > 0)
        {
            var tip = skel.Bones.FirstOrDefault(b =>
                b != null && b.Transform &&
                b.Transform.name.ToLower().Contains("index") &&
                b.Transform.name.ToLower().Contains("tip"));
            if (tip != null) return tip.Transform;
        }

        
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
