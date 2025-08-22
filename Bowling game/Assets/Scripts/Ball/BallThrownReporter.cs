using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class BallThrownReporter : MonoBehaviour
{
    public float speedThreshold = 1.5f;   
    public float armGraceTime = 0.15f;    
    private Rigidbody rb;
    private bool reported;
    private float spawnTime;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        spawnTime = Time.time;
    }

    void Update()
    {
        if (reported) return;
        if (Time.time - spawnTime < armGraceTime) return;

        if (rb.velocity.magnitude >= speedThreshold)
        {
            QuestEvents.BallThrown();
            reported = true;
        }
    }
}
