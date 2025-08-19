/*using UnityEngine;

public class BallHoldRelay : MonoBehaviour
{
    [Tooltip("Del imena parenta, ko je žoga držana (npr. 'Hand', 'Grab', 'Anchor'). Pusti prazno za auto-detekcijo.")]
    public string heldParentNameContains = "Hand";

    [Tooltip("Koliko sekund po spustu naj ostane blokirano.")]
    public float tailAfterRelease = 1.5f;

    bool isHeld;

    void Start()
    {
        CheckHeldImmediate();
    }

    void OnTransformParentChanged()
    {
        CheckHeldImmediate();
    }

    void OnDestroy()
    {
        if (isHeld) GestureInhibit.EndHold(tailAfterRelease);
    }

    void CheckHeldImmediate()
    {
        bool nowHeld = IsHandLike(transform.parent);

        if (nowHeld && !isHeld)
        {
            GestureInhibit.BeginHold();
        }
        else if (!nowHeld && isHeld)
        {
            GestureInhibit.EndHold(tailAfterRelease);
        }

        isHeld = nowHeld;
    }

    bool IsHandLike(Transform t)
    {
        if (!t) return false;
        if (!string.IsNullOrEmpty(heldParentNameContains) && t.name.Contains(heldParentNameContains))
            return true;

        for (int i = 0; i < 3 && t != null; i++, t = t.parent)
            if (t.name.Contains("Hand") || t.name.Contains("Grab") || t.name.Contains("Anchor"))
                return true;

        return false;
    }
}
*/