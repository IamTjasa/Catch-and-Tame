using UnityEngine;

[RequireComponent(typeof(BallBounceAnimator))]
public class BallCollisionResponder : MonoBehaviour
{
    private BallBounceAnimator bounceAnimator;
    private bool hasBounced = false;

    void Awake()
    {
        bounceAnimator = GetComponent<BallBounceAnimator>();
    }

    void OnCollisionEnter(Collision collision)
    {
        if (!hasBounced)
        {
            hasBounced = true;
            bounceAnimator.TriggerBounce();
        }
    }
}
