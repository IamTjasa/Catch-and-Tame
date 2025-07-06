using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class BallBounceAnimator : MonoBehaviour
{
    public float bounceForce = 5f;
    public float torqueAmount = 10f;
    public float destroyDelay = 2f;

    public void TriggerBounce()
    {
        Rigidbody rb = GetComponent<Rigidbody>();

        rb.AddForce(Vector3.up * bounceForce, ForceMode.Impulse);

        Vector3 randomTorque = new Vector3(
            Random.Range(-1f, 1f),
            Random.Range(-1f, 1f),
            Random.Range(-1f, 1f)
        ) * torqueAmount;

        rb.AddTorque(randomTorque, ForceMode.Impulse);

        Destroy(gameObject, destroyDelay);
    }
}
